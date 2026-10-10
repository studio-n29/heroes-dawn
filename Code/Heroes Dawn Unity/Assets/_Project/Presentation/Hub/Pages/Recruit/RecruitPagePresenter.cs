#nullable enable
using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Shop;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Shop;
using N29.HeroesDawn.Presentation.Mvp;
using VContainer.Unity;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>
    /// Recruter (M30 adapté à la boutique éphémère) : 3 cartes, compte à rebours, renouvellement (gratuit puis confirmé),
    /// achat par la confirmation M31. Ouvrir la page marque la boutique du jour comme vue (badge « N »).
    /// Une fois par minute : rouvre la boutique (nouveau tirage à 9h00) et met à jour le compte à rebours.
    /// </summary>
    public sealed class RecruitPagePresenter : PresenterBase<IRecruitPageView>, IPageTitle, ITickable
    {
        readonly ISessionReadModel _session;
        readonly IOpenShopUseCase _open;
        readonly IRerollShopUseCase _reroll;
        readonly IMarkShopSeenUseCase _markSeen;
        readonly RecruitFlow _flow;
        readonly HubNavigator _nav;
        readonly IClock _clock;
        readonly IModalService _modals;
        readonly IToastService _toasts;
        readonly ILocalization _loc;
        readonly ISubscriber<ShopChanged> _shop;
        readonly ISubscriber<WalletChanged> _wallet;
        readonly ISubscriber<RosterChanged> _roster;
        readonly PageAction _action;
        IDisposable? _subs;
        int _lastMinute = -1;

        public RecruitPagePresenter(IRecruitPageView view, ISessionReadModel session, IOpenShopUseCase open, IRerollShopUseCase reroll,
                                    IMarkShopSeenUseCase markSeen, RecruitFlow flow, HubNavigator nav, IClock clock, IModalService modals,
                                    IToastService toasts, ILocalization loc,
                                    ISubscriber<ShopChanged> shop, ISubscriber<WalletChanged> wallet, ISubscriber<RosterChanged> roster)
            : base(view, null)
        {
            _session = session; _open = open; _reroll = reroll; _markSeen = markSeen; _flow = flow; _nav = nav; _clock = clock;
            _modals = modals; _toasts = toasts; _loc = loc; _shop = shop; _wallet = wallet; _roster = roster;
            _action = new PageAction(toasts, loc);
        }

        public string Title => _loc.T("page.recruit");

        protected override void OnStart()
        {
            View.CardTapped += OpenSheet;
            View.RecruitClicked += Recruit;
            View.RerollClicked += Reroll;
            View.RegistryClicked += OpenRegistry;
        }

        void OpenRegistry() => _nav.Open(HubPage.Agents);

        protected override void OnShown()
        {
            _subs?.Dispose();
            var bag = DisposableBag.CreateBuilder();
            _shop.Subscribe(_ => Refresh()).AddTo(bag);
            _wallet.Subscribe(_ => Refresh()).AddTo(bag);
            _roster.Subscribe(_ => Refresh()).AddTo(bag);
            _subs = bag.Build();
            OpenAndMarkSeenAsync().Forget();
            Refresh();
        }

        protected override void OnHidden()
        {
            _subs?.Dispose();
            _subs = null;
        }

        protected override void OnDispose()
        {
            _subs?.Dispose();
            View.CardTapped -= OpenSheet;
            View.RecruitClicked -= Recruit;
            View.RerollClicked -= Reroll;
            View.RegistryClicked -= OpenRegistry;
        }

        public void Tick()
        {
            if (Disposed || !View.IsVisible) return;
            var now = _clock.LocalNow;
            if (now.Hour * 60 + now.Minute == _lastMinute) return;
            OpenAndMarkSeenAsync().Forget();
            Refresh();
        }

        /// <summary>Tire la boutique du jour si besoin, puis la marque vue (le joueur la regarde).</summary>
        async UniTaskVoid OpenAndMarkSeenAsync()
        {
            try
            {
                await _open.ExecuteAsync(Lifetime);
                if (Disposed) return;
                await _markSeen.ExecuteAsync(Lifetime);
            }
            catch (OperationCanceledException) { }
        }

        void OpenSheet(int slot)
        {
            var offers = _session.Shop.Offers;
            if (slot >= 0 && slot < offers.Count) _nav.Open(HubPage.MemberSheet, offers[slot].Id, fromShop: true);
        }

        void Recruit(int slot) => _flow.RunAsync(slot, Lifetime).Forget();

        /// <summary>Renouveler : direct si gratuit, sinon après confirmation de la dépense.</summary>
        void Reroll()
        {
            int cost = ShopRules.RerollCost(_session.Shop.Draw, _session.Content.Balance);
            if (cost == 0) { _action.Run(ct => _reroll.ExecuteAsync(ct), Lifetime); return; }
            if (_action.IsBusy) return;
            RerollPaidAsync(cost, _session.Shop.Day, _session.Shop.Draw).Forget();
        }

        async UniTaskVoid RerollPaidAsync(int cost, string day, int draw)
        {
            bool ok;
            try
            {
                ok = await _modals.ConfirmAsync(new ConfirmData(_loc.T("shop.reroll.confirm.title"), _loc.F("shop.reroll.confirm.body", cost),
                                                                _loc.T("ui.confirm"), _loc.T("ui.cancel"), Tone.Primary), Lifetime);
            }
            catch (OperationCanceledException) { return; }
            if (!ok || Disposed) return;
            // La boutique a changé pendant la confirmation (9h00 passée) : ce qui a été accepté n'existe plus.
            if (_session.Shop.Day != day || _session.Shop.Draw != draw)
            {
                _toasts.Show(_loc.Error(ShopErrors.Expired), ToastKind.Error);
                Refresh();
                return;
            }
            _action.Run(ct => _reroll.ExecuteAsync(ct), Lifetime);
        }

        void Refresh()
        {
            if (Disposed) return;
            var s = _session;
            var now = _clock.LocalNow;
            _lastMinute = now.Hour * 60 + now.Minute;
            var offers = s.Shop.Offers;
            int rerollCost = ShopRules.RerollCost(s.Shop.Draw, s.Content.Balance);
            View.Show(new RecruitPageData
            {
                Rule = _loc.T("recruit.rule"),
                Countdown = ShopCountdown.Text(now, s.Content.Balance.ShopResetHour, _loc),
                Cards = offers.Select((o, i) => Card(o, i)).ToArray(),
                EmptyText = offers.Count == 0 ? _loc.T("shop.empty") : "",
                RerollLabel = rerollCost == 0 ? _loc.T("shop.reroll.free") : _loc.F("shop.reroll.cost", rerollCost),
                RerollEnabled = offers.Count > 0 && s.Wallet.Credits >= rerollCost,
                Progress = _loc.F("recruit.progress", s.Roster.Count, s.Content.Members.Count),
            });
        }

        RecruitCardState Card(ShopOffer offer, int slot)
        {
            var def = _session.Content.Member(offer.Id);
            int cost = MetaRules.RecruitCost(def.Rarity);
            bool inTeam = offer.Recruited || _session.Roster.Any(m => m.Id == offer.Id);
            return new RecruitCardState
            {
                Slot = slot,
                Card = MemberCards.Card(def, MetaRules.RecruitLevel, _session.Content.Balance, _loc),
                Rarity = def.Rarity,
                PriceLabel = _loc.F("hub.recruit", cost),
                InTeam = inTeam,
                InTeamText = _loc.T("recruit.in_team"),
                Affordable = !inTeam && _session.Wallet.Pr >= cost,
                Halo = def.Rarity == Rarity.Legend,
            };
        }
    }
}
