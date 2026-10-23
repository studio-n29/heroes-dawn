#nullable enable
using System;
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Tutorial;
using N29.HeroesDawn.Application.Flow;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Presentation.Mvp;
using UnityEngine;
using VContainer.Unity;

namespace N29.HeroesDawn.Presentation.Tutorial
{
    /// <summary>Ce que chaque étape désigne et comment elle s'affiche.</summary>
    public readonly struct TutorialStepInfo
    {
        public readonly TutorialTarget Target;
        public readonly bool Spotlight;
        public readonly bool BubbleAtTop;
        /// <summary>Voile sombre bloquant (faux : seulement l anneau et la bulle, le reste de l écran reste utilisable).</summary>
        public readonly bool Dim;

        public TutorialStepInfo(TutorialTarget target, bool spotlight, bool bubbleAtTop, bool dim = true)
        { Target = target; Spotlight = spotlight; BubbleAtTop = bubbleAtTop; Dim = dim; }

        public static TutorialStepInfo Of(TutorialStep step) => step switch
        {
            TutorialStep.Welcome => new TutorialStepInfo(TutorialTarget.QgPage, false, false),
            TutorialStep.OpenTeam => new TutorialStepInfo(TutorialTarget.TabTeam, true, true),
            TutorialStep.Squad => new TutorialStepInfo(TutorialTarget.TeamSquad, true, true),
            TutorialStep.OpenSheet => new TutorialStepInfo(TutorialTarget.TeamTile, true, true),
            TutorialStep.Train => new TutorialStepInfo(TutorialTarget.SheetTrain, true, true),
            TutorialStep.EquipSlot => new TutorialStepInfo(TutorialTarget.SheetSlot, true, true),
            TutorialStep.EquipPick => new TutorialStepInfo(TutorialTarget.SheetPick, true, true),
            TutorialStep.OpenRecruit => new TutorialStepInfo(TutorialTarget.TabRecruit, true, true),
            TutorialStep.Recruit => new TutorialStepInfo(TutorialTarget.RecruitCard, true, true),
            TutorialStep.BackToTeam => new TutorialStepInfo(TutorialTarget.TabTeam, true, true),
            TutorialStep.Duo => new TutorialStepInfo(TutorialTarget.TeamSquad, true, true),
            TutorialStep.StartMission => new TutorialStepInfo(TutorialTarget.TeamMission, true, true),
            TutorialStep.Deploy => new TutorialStepInfo(TutorialTarget.MapDeploy, true, true),
            TutorialStep.Objectives => new TutorialStepInfo(TutorialTarget.BoardObjectives, true, false),
            TutorialStep.Roll => new TutorialStepInfo(TutorialTarget.BoardRoll, true, true),
            TutorialStep.Move => new TutorialStepInfo(TutorialTarget.BoardDestination, true, true, dim: false),
            TutorialStep.CombatAtb => new TutorialStepInfo(TutorialTarget.CombatEnemies, true, false),
            TutorialStep.CombatSkill => new TutorialStepInfo(TutorialTarget.CombatSkills, true, true, dim: false),
            TutorialStep.Results => new TutorialStepInfo(TutorialTarget.ResultsContinue, true, true),
            _ => new TutorialStepInfo(TutorialTarget.None, false, false),
        };
    }

    /// <summary>
    /// Pilote la couche du tutoriel : à chaque image, si une étape est en cours et que son élément est visible, l'éclaire ;
    /// relaie « Compris » et « Passer » au service, affiche les astuces contextuelles, et traduit les messages du jeu
    /// (plateau ouvert, résultats fermés) en signaux. Les gestes du joueur (onglet, MISSION, dé, déplacement, compétence)
    /// sont signalés par les présentateurs concernés.
    /// </summary>
    public sealed class TutorialDirector : IStartable, ITickable, IDisposable
    {
        readonly TutorialGuide _tutorial;
        readonly UiRoot _ui;
        readonly ILocalization _loc;
        readonly IModalService _modals;
        readonly ISubscriber<CombatResultsClosed> _results;
        readonly ISubscriber<GameStateChanged> _state;
        readonly Vector3[] _corners = new Vector3[4];
        IDisposable? _subs;
        TutorialOverlayView? _view;
        bool _skipping;

        public TutorialDirector(TutorialGuide tutorial, UiRoot ui, ILocalization loc, IModalService modals,
                                ISubscriber<CombatResultsClosed> results, ISubscriber<GameStateChanged> state)
        {
            _tutorial = tutorial; _ui = ui; _loc = loc; _modals = modals; _results = results; _state = state;
        }

        public void Start()
        {
            _view = _ui.Tutorial;
            if (_view == null) return;
            _view.AcknowledgeClicked += () => _tutorial.Acknowledge();
            _view.SkipClicked += Skip;
            _tutorial.HintRaised += OnHint;
            var bag = DisposableBag.CreateBuilder();
            _results.Subscribe(_ => _tutorial.Signal(TutorialSignal.ResultsClosed)).AddTo(bag);
            _state.Subscribe(s =>
            {
                if (s.To == GameState.Board) _tutorial.Signal(TutorialSignal.BoardOpened);
                if (s.To == GameState.Hub && s.From == GameState.Login) _tutorial.BeginIfNeeded();
            }).AddTo(bag);
            _subs = bag.Build();
        }

        public void Tick()
        {
            var view = _view;
            if (view == null) return;
            var current = TutorialStepInfo.Of(_tutorial.Current);
            // Une modale ouverte (confirmation, achat…) prend la main, sauf l étape qui désigne le bouton d une popup (Résultats).
            bool blockedByModal = _modals.AnyOpen && current.Target != TutorialTarget.ResultsContinue;
            if (!_tutorial.Active || blockedByModal)
            {
                if (view.StepVisible) view.Hide();
                _tutorial.Presenting = false;
                return;
            }
            var step = _tutorial.Current;
            var info = TutorialStepInfo.Of(step);
            var marker = TutorialTargetMarker.Find(info.Target);
            if (marker == null)
            {
                if (view.StepVisible) view.Hide();
                _tutorial.Presenting = false;
                return;
            }
            Rect? hole = null;
            if (info.Spotlight)
            {
                marker.Rect.GetWorldCorners(_corners);
                var overlay = (RectTransform)view.transform;
                var a = overlay.InverseTransformPoint(_corners[0]);
                var b = overlay.InverseTransformPoint(_corners[2]);
                hole = Rect.MinMaxRect(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y), Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y));
            }
            bool ack = TutorialGuide.NeedsAcknowledge(step);
            string key = "tutorial.step." + step;
            view.ShowStep(hole, new TutorialStepView
            {
                Title = _loc.T(key + ".title"),
                Body = _loc.T(key + ".body"),
                Progress = _loc.F("tutorial.progress", _tutorial.Position, TutorialGuide.StepCount),
                OkLabel = _loc.T("tutorial.ok"),
                SkipLabel = _loc.T("tutorial.skip"),
                ShowOk = ack,
            }, info.BubbleAtTop, blockHole: ack, dim: info.Dim);
            _tutorial.Presenting = true;
        }

        void Skip()
        {
            if (_skipping) return;
            SkipAsync().Forget();
        }

        async UniTaskVoid SkipAsync()
        {
            _skipping = true;
            try
            {
                bool ok = await _modals.ConfirmAsync(new ConfirmData(_loc.T("tutorial.skip.title"), _loc.T("tutorial.skip.body"),
                                                                     _loc.T("ui.confirm"), _loc.T("ui.cancel"), Tone.Danger), default);
                if (ok) _tutorial.Skip();
            }
            catch (OperationCanceledException) { }
            finally { _skipping = false; }
        }

        void OnHint(string id)
        {
            _view?.ShowHint(_loc.T("tutorial.hint.title"), _loc.T("tutorial.hint." + id), _loc.T("tutorial.hint.ok"));
        }

        public void Dispose()
        {
            _tutorial.HintRaised -= OnHint;
            _subs?.Dispose();
        }
    }
}
