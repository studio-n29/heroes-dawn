#nullable enable
using System.Collections.Generic;
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
    /// Page Tournoi (JcJ simulé, 0.15.0) : à gauche le classement (neuf joueurs factices et le joueur), à droite les trois manches du jour avec leur adversaire
    /// et leur issue. Le bouton du bandeau joue la manche suivante, puis réclame la récompense quand le tournoi est fini ; un tournoi par jour.
    /// </summary>
    public sealed class TournamentPagePresenter : ListPagePresenter<ITournamentPageView>
    {
        readonly Tournament _tournament;

        public TournamentPagePresenter(ITournamentPageView view, Tournament tournament, ILocalization loc, IToastService toasts, ISubscriber<MetaChanged> meta,
                                       ISubscriber<WalletChanged> wallet, ISubscriber<RosterChanged> roster, IClock clock)
            : base(view, loc, toasts, meta, wallet, roster, clock)
        {
            _tournament = tournament;
        }

        public override string Title => Loc.T("page.tournament");

        static readonly string[] RoundKeys = { "tournament.round.quarter", "tournament.round.semi", "tournament.round.final" };

        string NameOf(LeaderboardEntry e) => e.IsPlayer ? Loc.T("tournament.you") : e.Name;

        protected override void Render()
        {
            int round = _tournament.Round;
            bool finished = _tournament.Finished;
            RowButtonData button;
            if (!finished) button = RowButtonData.Of(Loc.F("tournament.play", Loc.T(RoundKeys[round])), _tournament.CanPlay, Loc.T("errors.meta_no_members"), 0);
            else if (!_tournament.Claimed) button = RowButtonData.Of(Loc.T("tournament.claim"), true, "", 0);
            else button = RowButtonData.Of(Loc.T("tournament.tomorrow"), false, Loc.T("tournament.tomorrow_reason"), 0);
            string state = !finished ? Loc.F("tournament.state.running", round, Tournament.Rounds)
                         : _tournament.Champion ? Loc.T("tournament.state.champion") : Loc.F("tournament.state.out", round);
            View.ShowHeader(new ListHeaderData
            {
                Title = Loc.T("tournament.title"),
                Line = Loc.F("tournament.line", _tournament.PlayerRank, _tournament.Rating, state),
                Progress = round / (float)Tournament.Rounds,
                ProgressLabel = round + " / " + Tournament.Rounds,
                A = button,
            });

            var board = _tournament.Leaderboard();
            var rows = new InfoRowData[board.Count];
            for (int i = 0; i < rows.Length; i++)
            {
                var e = board[i];
                var r = InfoRowData.Basic(NameOf(e), Loc.F("tournament.points", e.Rating));
                r.Tag = e.Rank.ToString();
                r.TagColor = e.IsPlayer ? Theme.Primary : e.Rank <= 3 ? Theme.Accent : Theme.TextMuted;
                r.Highlight = e.IsPlayer;
                rows[i] = r;
            }
            View.ShowRows(rows);
            ShowRounds(round, finished);
        }

        void ShowRounds(int round, bool finished)
        {
            var opponents = _tournament.Opponents();
            var side = new List<InfoRowData>();
            for (int i = 0; i < Tournament.Rounds; i++)
            {
                bool won = i < round;
                bool lost = finished && _tournament.Eliminated && i == round;
                var r = InfoRowData.Basic(Loc.F("tournament.round_line", Loc.T(RoundKeys[i]), opponents[i].Name),
                                          won ? Loc.T("tournament.won") : lost ? Loc.T("tournament.lost") : Loc.F("tournament.power", opponents[i].Power));
                r.Tag = (i + 1).ToString();
                r.TagColor = won ? Theme.Success : lost ? Theme.Danger : i == round && !finished ? Theme.Primary : Theme.TextMuted;
                r.Highlight = i == round && !finished;
                r.Dim = !won && !lost && (i > round || finished);
                side.Add(r);
            }
            var mine = InfoRowData.Basic(Loc.T("tournament.your_power"), Loc.F("tournament.power", _tournament.PlayerPower()));
            mine.Tag = "A";
            mine.TagColor = Theme.Info;
            side.Add(mine);
            View.ShowSide(new ListSideData { Title = Loc.T("tournament.rounds"), Rows = side.ToArray(), Text = "" });
        }

        protected override void OnHeader(int index)
        {
            if (index != 0) return;
            if (_tournament.Finished)
            {
                if (!_tournament.Claimed) Report(_tournament.Claim(), "tournament.claimed");
                return;
            }
            var played = _tournament.PlayNext();
            if (played.IsFailure) { Toasts.Show(Loc.Error(played.Error!), ToastKind.Error); return; }
            var m = played.Value;
            Toasts.Show(m.Won ? Loc.F("tournament.toast.win", m.Opponent.Name) : Loc.F("tournament.toast.loss", m.Opponent.Name), m.Won ? ToastKind.Reward : ToastKind.Warning);
        }
    }
}
