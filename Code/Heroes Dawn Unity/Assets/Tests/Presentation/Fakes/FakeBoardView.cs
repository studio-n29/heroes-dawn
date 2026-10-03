#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Flow;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Presentation.Board;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;

namespace N29.HeroesDawn.Tests.Presentation.Fakes
{
    public sealed class FakeBoardMap : IBoardMapView
    {
        public event Action<CaseId>? NodeTapped;
        public int Builds;
        public int Refreshes;
        public IReadOnlyList<BoardNodeState> LastNodes = Array.Empty<BoardNodeState>();
        public readonly List<IReadOnlyList<CaseId>> Moves = new List<IReadOnlyList<CaseId>>();

        public void Tap(string id) => NodeTapped?.Invoke(new CaseId(id));
        public void Build(BoardMapData d) => Builds++;
        public void Refresh(IReadOnlyList<BoardNodeState> nodes, IReadOnlyList<BoardLinkState> links, CaseId pawnAt, bool movePawn) { Refreshes++; LastNodes = nodes; }
        public UniTask MovePawnAsync(IReadOnlyList<CaseId> path, CancellationToken ct) { Moves.Add(path); return UniTask.CompletedTask; }
    }

    public sealed class FakeBoardView : FakeView, IBoardView
    {
        public event Action? RollClicked;
        public event Action? BonusClicked;
        public event Action? EndMoveClicked;
        public event Action? BackClicked;

        public readonly FakeWalletView WalletView = new FakeWalletView();
        public readonly FakeBoardMap MapView = new FakeBoardMap();
        public IWalletView Wallet => WalletView;
        public IBoardMapView Map => MapView;

        public string Title = "";
        public readonly List<BoardHudData> Huds = new List<BoardHudData>();
        public IReadOnlyList<ObjectiveLineData> Objectives = Array.Empty<ObjectiveLineData>();
        public readonly List<int> DieRolls = new List<int>();

        public void ClickRoll() => RollClicked?.Invoke();
        public void ClickBonus() => BonusClicked?.Invoke();
        public void ClickEnd() => EndMoveClicked?.Invoke();
        public void ClickBack() => BackClicked?.Invoke();

        public void SetZoneTitle(string title) => Title = title;
        public void ShowObjectives(IReadOnlyList<ObjectiveLineData> o) => Objectives = o;
        public void ShowSquad(IReadOnlyList<SquadSlotData> squad) { }
        public void ShowHud(BoardHudData hud) => Huds.Add(hud);
        public UniTask RollDieAsync(int value, int faces, CancellationToken ct) { DieRolls.Add(value); return UniTask.CompletedTask; }
        public void ResetDie() { }
        public UniTask PauseAsync(float seconds, CancellationToken ct) => UniTask.CompletedTask;
    }

    /// <summary>Machine d'états de test : état courant réglable, aucune transition réelle.</summary>
    public sealed class FakeGameFlow : IGameFlow
    {
        public GameState Current { get; set; } = GameState.Board;
        public bool IsTransitioning => false;
        public UniTask<bool> RequestAsync(GameState target, CancellationToken ct = default) => UniTask.FromResult(true);
    }
}
