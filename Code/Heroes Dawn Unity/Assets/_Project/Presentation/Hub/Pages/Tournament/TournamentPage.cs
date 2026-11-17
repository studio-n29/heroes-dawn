#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>
    /// Écran Tournoi (planches 13.1 et 13.2) : classement (top 100) à gauche ; à droite les cinq paliers de points (à la place des « manches » de la planche),
    /// l'état (points, prochain palier, duels restants) et les boutons Réclamer et Duel. Entre deux tournois, l'écran annonce le suivant et propose Retour au QG.
    /// Le lancement d'un duel n'est pas encore branché dans l'interface : le bouton de duel reste grisé (« Bientôt »).
    /// </summary>
    public sealed class TournamentPagePresenter : PresenterBase<ITournamentPageView>, IPageTitle
    {
        readonly TournamentArena _tournament;
        readonly ILocalization _loc;
        readonly IToastService _toasts;
        readonly ISubscriber<MetaChanged> _meta;
        readonly ISubscriber<WalletChanged> _wallet;
        readonly ISubscriber<RosterChanged> _roster;
        readonly HubNavigator _nav;
        readonly CancellationTokenSource _cts = new CancellationTokenSource();
        IReadOnlyList<ScoreRow> _board = new List<ScoreRow>();
        IDisposable? _subs;

        public TournamentPagePresenter(ITournamentPageView view, TournamentArena tournament, ILocalization loc, IToastService toasts, ISubscriber<MetaChanged> meta,
                                       ISubscriber<WalletChanged> wallet, ISubscriber<RosterChanged> roster, IClock clock, HubNavigator nav)
            : base(view, null)
        {
            _tournament = tournament; _loc = loc; _toasts = toasts; _meta = meta; _wallet = wallet; _roster = roster; _nav = nav;
        }

        public string Title => _loc.T("page.tournament");

        protected override void OnStart()
        {
            View.ClaimClicked += OnClaim;
            View.BackToQgClicked += OnBackToQg;
        }

        protected override void OnShown()
        {
            _subs?.Dispose();
            var bag = DisposableBag.CreateBuilder();
            _meta.Subscribe(_ => Refresh()).AddTo(bag);
            _wallet.Subscribe(_ => Refresh()).AddTo(bag);
            _roster.Subscribe(_ => Refresh()).AddTo(bag);
            _subs = bag.Build();
            LoadBoardAsync().Forget();
            Refresh();
        }

        protected override void OnHidden() { _subs?.Dispose(); _subs = null; }

        protected override void OnDispose()
        {
            _cts.Cancel();
            _subs?.Dispose();
            View.ClaimClicked -= OnClaim;
            View.BackToQgClicked -= OnBackToQg;
        }

        async UniTaskVoid LoadBoardAsync()
        {
            if (!_tournament.SignedIn) return;
            var r = await _tournament.LeaderboardAsync(_cts.Token);
            if (Disposed || r.IsFailure) return;
            _board = r.Value;
            Refresh();
        }

        void Refresh()
        {
            if (Disposed) return;
            View.Show(Build());
        }

        TournamentScreenData Build()
        {
            var v = _tournament.Current();
            string name = _loc.T(v.Def.NameKey);
            var tiers = new TournamentTierData[ArenaRules.TierPoints.Count];
            for (int i = 0; i < tiers.Length; i++)
                tiers[i] = new TournamentTierData(_loc.T("tournament.tier." + (i + 1)), !v.IsActive ? TournamentTierState.Locked
                    : v.Tier >= i + 1 ? TournamentTierState.Done : v.Tier == i ? TournamentTierState.Current : TournamentTierState.Locked);

            string sub;
            if (!v.SignedIn) sub = _loc.T("tournament.sign_in");
            else if (!v.IsActive) sub = _loc.F("tournament.next", name, v.Start.ToString("dd/MM"));
            else if (v.Tier >= ArenaRules.TierPoints.Count) sub = _loc.F("tournament.status_max", v.Points, _loc.F("tournament.duels_left", v.DuelsLeft));
            else sub = _loc.F("tournament.status_sub", v.Points, ArenaRules.TierPoints[v.Tier], _loc.F("tournament.duels_left", v.DuelsLeft));

            bool claimable = v.IsActive && v.ClaimableTiers.Count > 0;
            return new TournamentScreenData
            {
                Title = name,
                StateLabel = _loc.T(v.IsActive ? "tournament.state.active" : "tournament.state.upcoming"),
                Finished = !v.IsActive,
                RankingTitle = _loc.T(v.IsActive ? "tournament.ranking" : "tournament.ranking_final"),
                RankingEmpty = _loc.T(v.SignedIn ? "tournament.ranking_empty" : "tournament.sign_in"),
                Ranking = v.SignedIn ? _board.Select(e => new TournamentRankData(e.Rank, e.IsPlayer ? _loc.T("tournament.you") : e.Name, _loc.F("tournament.points", e.Points), e.IsPlayer)).ToArray()
                                     : Array.Empty<TournamentRankData>(),
                TiersTitle = _loc.T("tournament.tiers"),
                Tiers = tiers,
                StatusTitle = _loc.T("tournament.status"),
                StatusMain = _loc.F("tournament.tier_status", v.IsActive ? v.Tier : 0, ArenaRules.TierPoints.Count),
                StatusSub = sub,
                Progress = v.IsActive ? v.Tier / (float)ArenaRules.TierPoints.Count : 0f,
                ClaimLabel = _loc.T("tournament.claim"),
                CanClaim = claimable,
                ClaimReason = claimable ? "" : _loc.T("tournament.nothing_to_claim"),
                DuelLabel = _loc.T("tournament.duel"),
                CanDuel = false,
                DuelReason = _loc.T("tournament.duel_soon"),
                BackLabel = _loc.T("tournament.back_to_qg"),
            };
        }

        void OnClaim()
        {
            var v = _tournament.Current();
            if (!v.IsActive || v.ClaimableTiers.Count == 0) return;
            ClaimAsync(v.ClaimableTiers[0]).Forget();
        }

        async UniTaskVoid ClaimAsync(int tier)
        {
            var r = await _tournament.ClaimTierAsync(tier, _cts.Token);
            if (Disposed) return;
            if (r.IsFailure) _toasts.Show(_loc.Error(r.Error!), ToastKind.Error);
            else { _toasts.Show(_loc.T("tournament.claimed"), ToastKind.Reward); Refresh(); }
        }

        void OnBackToQg() => _nav.SelectTab(HubPage.Qg);
    }
}
