#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Feedback;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Presentation.UiToolkit;
using UnityEngine;
using UnityEngine.InputSystem;

namespace N29.HeroesDawn.Presentation.Board
{
    /// <summary>
    /// Plateau (prefab Screens/Board) : fond uGUI et HUD UI Toolkit (<see cref="BoardHudView"/>) qui porte la carte, les objectifs,
    /// l'escouade, le dé et les boutons. Les appels faits avant que le HUD soit monté sont rejoués dès qu'il l'est.
    /// </summary>
    public sealed class BoardView : ViewBase, IBoardView
    {
        [SerializeField] BoardHudView hud = null!;

        readonly MapProxy _map = new MapProxy();

        public event Action? RollClicked;
        public event Action? BonusClicked;
        public event Action? EndMoveClicked;
        public event Action? BackClicked;
        public event Action? MoveConfirmClicked;
        public event Action? MoveCancelClicked;

        public IWalletView Wallet => hud.Wallet;
        public IBoardMapView Map => _map;

        /// <summary>Carte et boutons (tests PlayMode : clics réels).</summary>
        public BoardMapView MapView => hud.Map!;
        public HdButtonRef NodeButton(CaseId id) => hud.Map!.NodeButton(id);
        public HdButtonRef BackButton => hud.BackButton;
        public HdButtonRef RollButton => hud.RollButton;
        public HdButtonRef EndMoveButton => hud.EndMoveButton;
        public HdButtonRef MoveConfirmButton => hud.MoveConfirmButton;
        public HdButtonRef PreviewCancelButton => hud.PreviewCancelButton;
        public bool PreviewVisible => hud.PreviewVisible;
        public ComicBurst? Burst => hud.Burst;
        public BoardHudView Hud => hud;

        void Awake()
        {
            hud.RollClicked += () => RollClicked?.Invoke();
            hud.BonusClicked += () => BonusClicked?.Invoke();
            hud.EndMoveClicked += () => EndMoveClicked?.Invoke();
            hud.BackClicked += () => BackClicked?.Invoke();
            hud.MoveConfirmClicked += () => MoveConfirmClicked?.Invoke();
            hud.MoveCancelClicked += () => MoveCancelClicked?.Invoke();
            hud.MapRebound += _map.Bind;
            if (hud.Map != null) _map.Bind(hud.Map);
        }

        void Update()
        {
            if (Keys.Pressed(Key.Space)) RollClicked?.Invoke();
            else if (Keys.Pressed(Key.Enter) || Keys.Pressed(Key.NumpadEnter))
            {
                // Entrée : « S'y rendre » si la carte d'information est ouverte, sinon valide la case sélectionnée au clavier.
                if (PreviewVisible) MoveConfirmClicked?.Invoke();
                else hud.Map?.ConfirmFocus();
            }
            else if (Keys.Pressed(Key.T)) EndMoveClicked?.Invoke();
            else if (Keys.Pressed(Key.B)) BonusClicked?.Invoke();
        }

        public void ShowMovePreview(MovePreviewData? data) => hud.ShowMovePreview(data);
        public void SetZoneTitle(string title) => hud.SetZoneTitle(title);
        public void ShowObjectives(IReadOnlyList<ObjectiveLineData> objectives) => hud.ShowObjectives(objectives);
        public void ShowSquad(IReadOnlyList<SquadSlotData> squad) => hud.ShowSquad(squad);
        public void ShowHud(BoardHudData d) => hud.ShowHud(d);
        public UniTask RollDieAsync(int value, int faces, CancellationToken ct) => hud.RollDieAsync(value, faces, ct);
        public void ResetDie() => hud.ResetDie();

        public async UniTask PlayCueAsync(ComicCueData cue, CancellationToken ct)
        {
            await UniTask.WaitUntil(() => hud.Burst != null, cancellationToken: ct);
            await hud.Burst!.PlayAsync(cue, ct);
        }

        public UniTask PauseAsync(float seconds, CancellationToken ct)
            => UniTask.Delay(TimeSpan.FromSeconds(seconds), ignoreTimeScale: true, cancellationToken: ct);

        /// <summary>Carte vue du présentateur : garde le dernier état pour le rejouer quand le document du HUD est (re)monté.</summary>
        sealed class MapProxy : IBoardMapView
        {
            BoardMapView? _target;
            BoardMapData? _build;
            (IReadOnlyList<BoardNodeState> Nodes, IReadOnlyList<BoardLinkState> Links, CaseId PawnAt, bool Move)? _refresh;
            (CaseId Id, bool Animate)? _center;

            public event Action<CaseId>? NodeTapped;
            public CaseId? Focused => _target?.Focused;

            public void Bind(BoardMapView map)
            {
                _target = map;
                map.NodeTapped += id => NodeTapped?.Invoke(id);
                if (_build.HasValue) map.Build(_build.Value);
                if (_refresh.HasValue) map.Refresh(_refresh.Value.Nodes, _refresh.Value.Links, _refresh.Value.PawnAt, _refresh.Value.Move);
                if (_center.HasValue) map.CenterOn(_center.Value.Id, false);
            }

            public void Build(BoardMapData data) { _build = data; _target?.Build(data); }

            public void Refresh(IReadOnlyList<BoardNodeState> nodes, IReadOnlyList<BoardLinkState> links, CaseId pawnAt, bool movePawn)
            {
                _refresh = (nodes, links, pawnAt, movePawn);
                _target?.Refresh(nodes, links, pawnAt, movePawn);
            }

            public void CenterOn(CaseId id, bool animate) { _center = (id, animate); _target?.CenterOn(id, animate); }
            public void FitTo(IReadOnlyList<CaseId> ids, bool animate) => _target?.FitTo(ids, animate);

            public async UniTask MovePawnAsync(IReadOnlyList<CaseId> path, CancellationToken ct)
            {
                await UniTask.WaitUntil(() => _target != null, cancellationToken: ct);
                await _target!.MovePawnAsync(path, ct);
            }
        }
    }
}
