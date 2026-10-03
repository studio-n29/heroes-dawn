#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Combat;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Ports;

namespace N29.HeroesDawn.Application.Flow
{
    /// <summary>
    /// Machine d'états du jeu (spec du sous-projet 2, §6). Chaque transition autorisée est une suite d'effets
    /// sur les scènes ; en cas d'échec, les effets déjà faits sont annulés dans l'ordre inverse.
    /// </summary>
    public sealed class GameFlowMachine : IGameFlow, IDisposable
    {
        enum Op { Load, Unload, Show, Hide, LoginOn, LoginOff }

        readonly struct Step
        {
            public readonly Op Op;
            public readonly string Scene;
            public Step(Op op, string scene = "") { Op = op; Scene = scene; }
            public Step Inverse() => new Step(Op switch
            {
                Op.Load => Op.Unload, Op.Unload => Op.Load, Op.Show => Op.Hide, Op.Hide => Op.Show,
                Op.LoginOn => Op.LoginOff, _ => Op.LoginOn,
            }, Scene);
        }

        static readonly Dictionary<(GameState, GameState), Step[]> Transitions = new Dictionary<(GameState, GameState), Step[]>
        {
            [(GameState.Boot, GameState.Login)] = new[] { new Step(Op.LoginOn) },
            [(GameState.Login, GameState.Hub)] = new[] { new Step(Op.LoginOff), new Step(Op.Load, SceneNames.Hub) },
            [(GameState.Hub, GameState.Login)] = new[] { new Step(Op.Unload, SceneNames.Hub), new Step(Op.LoginOn) },
            [(GameState.Hub, GameState.Board)] = new[] { new Step(Op.Unload, SceneNames.Hub), new Step(Op.Load, SceneNames.Board) },
            [(GameState.Board, GameState.Hub)] = new[] { new Step(Op.Unload, SceneNames.Board), new Step(Op.Load, SceneNames.Hub) },
            [(GameState.Board, GameState.Combat)] = new[] { new Step(Op.Load, SceneNames.Combat), new Step(Op.Hide, SceneNames.Board) },
            [(GameState.Combat, GameState.Board)] = new[] { new Step(Op.Unload, SceneNames.Combat), new Step(Op.Show, SceneNames.Board) },
            [(GameState.Combat, GameState.Hub)] = new[]
            {
                new Step(Op.Unload, SceneNames.Combat), new Step(Op.Unload, SceneNames.Board), new Step(Op.Load, SceneNames.Hub),
            },
        };

        readonly ISceneLoader _scenes;
        readonly IScreenVisibility _screens;
        readonly IPublisher<GameStateChanged> _changed;
        readonly IPublisher<ReturnedToBoard> _returned;
        readonly IPublisher<ToastRequested> _toasts;
        readonly ILogger _log;
        readonly IDisposable _subscriptions;
        GameState _inFlight;
        (GameState Target, CombatReport? Report)? _queued;

        public GameState Current { get; private set; } = GameState.Boot;
        public bool IsTransitioning { get; private set; }

        public GameFlowMachine(ISceneLoader scenes, IScreenVisibility screens, ISubscriber<NavigateRequested> navigate,
                               ISubscriber<CombatResultsClosed> combatClosed, IPublisher<GameStateChanged> changed,
                               IPublisher<ReturnedToBoard> returned, IPublisher<ToastRequested> toasts, ILogger log)
        {
            _scenes = scenes; _screens = screens; _changed = changed; _returned = returned; _toasts = toasts; _log = log;
            var bag = DisposableBag.CreateBuilder();
            navigate.Subscribe(m => RequestAsync(m.Target).Forget()).AddTo(bag);
            combatClosed.Subscribe(OnCombatClosed).AddTo(bag);
            _subscriptions = bag.Build();
        }

        /// <summary>Boot → Login : affiche l'écran de connexion. À appeler une fois l'interface construite.</summary>
        public void Start() => RequestAsync(GameState.Login).Forget();

        void OnCombatClosed(CombatResultsClosed m)
        {
            // Le rapport voyage avec sa demande : il n'est jamais rejoué par une navigation ultérieure.
            if (m.Report.ReturnedToHq) Request(GameState.Hub, null).Forget();
            else Request(GameState.Board, m.Report).Forget();
        }

        public UniTask<bool> RequestAsync(GameState target, CancellationToken ct = default) => Request(target, null, ct);

        /// <summary>
        /// Exécute une transition. Pendant une transition, une demande vers un autre état est mise en attente
        /// (la dernière l'emporte) et exécutée juste après ; une demande identique à celle en cours est ignorée.
        /// </summary>
        async UniTask<bool> Request(GameState target, CombatReport? report, CancellationToken ct = default)
        {
            if (IsTransitioning)
            {
                if (target != _inFlight) _queued = (target, report);
                return false;
            }
            if (target == Current) return false;
            var from = Current;
            if (!Transitions.TryGetValue((from, target), out var steps))
            {
                _log.Warn($"Navigation refusée : {from} → {target}");
                return false;
            }

            IsTransitioning = true;
            _inFlight = target;
            bool succeeded = false;
            var done = new List<Step>();
            try
            {
                try
                {
                    foreach (var step in steps)
                    {
                        await Apply(step, ct);
                        done.Add(step);
                    }
                }
                catch (OperationCanceledException)
                {
                    await Rollback(done);
                    throw;
                }
                catch (Exception e)
                {
                    _log.Error($"Navigation {from} → {target} impossible : {e.Message}");
                    await Rollback(done);
                    _toasts.Publish(new ToastRequested("errors.navigation", ToastKind.Error));
                    return false;
                }
                Current = target;
                succeeded = true;
            }
            finally
            {
                IsTransitioning = false; // toujours, même si un abonné plante
                if (!succeeded) RunQueued();
            }

            _changed.Publish(new GameStateChanged(from, target));
            if (from == GameState.Combat && target == GameState.Board && report != null)
                _returned.Publish(new ReturnedToBoard(report));
            RunQueued();
            return true;
        }

        void RunQueued()
        {
            if (_queued == null) return;
            var (target, report) = _queued.Value;
            _queued = null;
            if (target != Current) Request(target, report).Forget();
        }

        async UniTask Apply(Step step, CancellationToken ct)
        {
            switch (step.Op)
            {
                case Op.Load: await _scenes.LoadAsync(step.Scene, ct); break;
                case Op.Unload: await _scenes.UnloadAsync(step.Scene, ct); break;
                case Op.Show: _scenes.SetVisible(step.Scene, true); break;
                case Op.Hide: _scenes.SetVisible(step.Scene, false); break;
                case Op.LoginOn: _screens.SetLoginVisible(true); break;
                case Op.LoginOff: _screens.SetLoginVisible(false); break;
            }
        }

        /// <summary>Annule les effets déjà faits, dans l'ordre inverse (au mieux : une erreur d'annulation est journalisée).</summary>
        async UniTask Rollback(List<Step> done)
        {
            for (int i = done.Count - 1; i >= 0; i--)
            {
                try { await Apply(done[i].Inverse(), CancellationToken.None); }
                catch (Exception e) { _log.Error("Annulation de navigation incomplète : " + e.Message); }
            }
        }

        public void Dispose() => _subscriptions.Dispose();
    }
}
