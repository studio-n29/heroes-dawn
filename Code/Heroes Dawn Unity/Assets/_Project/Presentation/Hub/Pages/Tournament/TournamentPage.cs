#nullable enable
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Hub
{
    public interface ITournamentPageView : IListPageView { }

    /// <summary>
    /// Page Tournoi, version minimale du bloc D (1.0.0) : tournoi actif ou prochain, points, palier et classement (top 100) lus sur le serveur ;
    /// le bandeau réclame le prochain palier atteint. Le lancement d'un duel, la file d'attente, le duel en ligne, le résumé et la réclamation du classement
    /// sont à faire dans le lot UI (voir la note de passation du bloc D).
    /// </summary>
    public sealed class TournamentPagePresenter : ListPagePresenter<ITournamentPageView>
    {
        readonly TournamentArena _tournament;
        readonly CancellationTokenSource _cts = new CancellationTokenSource();
        IReadOnlyList<ScoreRow> _board = new List<ScoreRow>();

        public TournamentPagePresenter(ITournamentPageView view, TournamentArena tournament, ILocalization loc, IToastService toasts, ISubscriber<MetaChanged> meta,
                                       ISubscriber<WalletChanged> wallet, ISubscriber<RosterChanged> roster, IClock clock)
            : base(view, loc, toasts, meta, wallet, roster, clock)
        {
            _tournament = tournament;
        }

        public override string Title => Loc.T("page.tournament");

        protected override void OnPageShown() => LoadBoardAsync().Forget();

        async UniTaskVoid LoadBoardAsync()
        {
            if (!_tournament.SignedIn) return;
            var r = await _tournament.LeaderboardAsync(_cts.Token);
            if (Disposed || r.IsFailure) return;
            _board = r.Value;
            Refresh();
        }

        protected override void OnDispose()
        {
            _cts.Cancel();
            base.OnDispose();
        }

        protected override void Render()
        {
            var v = _tournament.Current();
            string name = Loc.T(v.Def.NameKey);
            string line = !v.SignedIn ? Loc.T("tournament.sign_in")
                        : v.IsActive ? Loc.F("tournament.line", v.Tier, v.Points, Loc.F("tournament.duels_left", v.DuelsLeft))
                        : Loc.F("tournament.next", name, v.Start.ToString("dd/MM"));
            RowButtonData button = v.ClaimableTiers.Count > 0
                ? RowButtonData.Of(Loc.T("tournament.claim"), true, "", 0)
                : RowButtonData.Of(Loc.T("tournament.claim"), false, Loc.T("tournament.nothing_to_claim"), 0);
            View.ShowHeader(new ListHeaderData
            {
                Title = v.IsActive ? name : Loc.T("tournament.title"),
                Line = line,
                Progress = v.Tier / 5f,
                ProgressLabel = v.Tier + " / 5",
                A = button,
            });

            var rows = new InfoRowData[_board.Count];
            for (int i = 0; i < rows.Length; i++)
            {
                var e = _board[i];
                var r = InfoRowData.Basic(e.IsPlayer ? Loc.T("tournament.you") : e.Name, Loc.F("tournament.points", e.Points));
                r.Tag = e.Rank.ToString();
                r.TagColor = e.IsPlayer ? Theme.Primary : e.Rank <= 3 ? Theme.Accent : Theme.TextMuted;
                r.Highlight = e.IsPlayer;
                rows[i] = r;
            }
            View.ShowRows(rows);
            View.ShowSide(new ListSideData { Title = Loc.T("tournament.rewards"), Rows = new InfoRowData[0], Text = "" });
        }

        protected override void OnHeader(int index)
        {
            if (index != 0) return;
            var v = _tournament.Current();
            if (v.ClaimableTiers.Count == 0) return;
            ClaimAsync(v.ClaimableTiers[0]).Forget();
        }

        async UniTaskVoid ClaimAsync(int tier)
        {
            var r = await _tournament.ClaimTierAsync(tier, _cts.Token);
            if (Disposed) return;
            if (Report(r, Loc.T("tournament.claimed"))) Refresh();
        }
    }
}
