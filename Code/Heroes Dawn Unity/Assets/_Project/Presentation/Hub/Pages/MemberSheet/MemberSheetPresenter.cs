#nullable enable
using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Hub;
using N29.HeroesDawn.Application.Features.Shop;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>
    /// Fiche d'un membre (M21). Membre possédé : stats, compétences, synergies de sa famille, flèches pour parcourir l'effectif
    /// (sans empiler de page), raccourcis Salon / Infirmerie, Entraîner · N CR.
    /// Héros de la boutique (<see cref="HubLocation.FromShop"/>) : même fiche, bouton Recruter · N PR par la confirmation M31 ;
    /// si l'offre disparaît (boutique renouvelée), retour à Recruter avec un toast.
    /// </summary>
    public sealed class MemberSheetPresenter : PresenterBase<IMemberSheetView>, IPageTitle
    {
        readonly ISessionReadModel _session;
        readonly ITrainMemberUseCase _train;
        readonly RecruitFlow _recruit;
        readonly HubNavigator _nav;
        readonly IToastService _toasts;
        readonly ILocalization _loc;
        readonly ISubscriber<WalletChanged> _wallet;
        readonly ISubscriber<RosterChanged> _roster;
        readonly ISubscriber<ShopChanged> _shop;
        readonly PageAction _action;
        readonly bool _fromShop;
        IDisposable? _subs;
        MemberId _member;
        bool _left;

        public MemberSheetPresenter(IMemberSheetView view, HubLocation location, ISessionReadModel session, ITrainMemberUseCase train,
                                    RecruitFlow recruit, HubNavigator nav, IToastService toasts, ILocalization loc,
                                    ISubscriber<WalletChanged> wallet, ISubscriber<RosterChanged> roster, ISubscriber<ShopChanged> shop)
            : base(view, null)
        {
            _session = session; _train = train; _recruit = recruit; _nav = nav; _toasts = toasts; _loc = loc;
            _wallet = wallet; _roster = roster; _shop = shop;
            _member = location.Member ?? throw new ArgumentException("Fiche sans membre", nameof(location));
            _fromShop = location.FromShop;
            _action = new PageAction(toasts, loc);
        }

        public string Title => _loc.T("page.member_sheet");

        protected override void OnStart()
        {
            View.PreviousClicked += Previous;
            View.NextClicked += Next;
            View.PrimaryClicked += Primary;
            View.RoomClicked += OpenRoom;
            View.LoadoutClicked += OpenLoadout;
        }

        protected override void OnShown()
        {
            _subs?.Dispose();
            var bag = DisposableBag.CreateBuilder();
            _wallet.Subscribe(_ => Refresh()).AddTo(bag);
            _roster.Subscribe(_ => Refresh()).AddTo(bag);
            _shop.Subscribe(_ => Refresh()).AddTo(bag);
            _subs = bag.Build();
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
            View.PreviousClicked -= Previous;
            View.NextClicked -= Next;
            View.PrimaryClicked -= Primary;
            View.RoomClicked -= OpenRoom;
            View.LoadoutClicked -= OpenLoadout;
        }

        void Previous() => Step(-1);
        void Next() => Step(1);

        void Step(int delta)
        {
            if (_fromShop) return;
            var roster = _session.Roster;
            if (roster.Count == 0) return;
            int i = 0;
            for (int k = 0; k < roster.Count; k++) if (roster[k].Id == _member) { i = k; break; }
            _member = roster[((i + delta) % roster.Count + roster.Count) % roster.Count].Id;
            _nav.ReplaceCurrent(new HubLocation(HubPage.MemberSheet, _member));
            Refresh();
        }

        void OpenRoom(HubPage page) => _nav.Open(page);

        void OpenLoadout() => _nav.Open(HubPage.Loadout, _member);

        void Primary()
        {
            if (_fromShop) Recruit();
            else Train();
        }

        void Train()
        {
            var id = _member;
            _action.Run(ct => _train.ExecuteAsync(id, ct), Lifetime, () =>
            {
                var m = _session.Roster.FirstOrDefault(x => x.Id == id);
                if (m != null) _toasts.Show(_loc.F("hub.trained", _loc.MemberName(m.Def), m.Level), ToastKind.Reward);
                Refresh();
            });
        }

        void Recruit()
        {
            int slot = Slot();
            if (slot < 0) return;
            RecruitAsync(slot).Forget();
        }

        async UniTaskVoid RecruitAsync(int slot)
        {
            var outcome = await _recruit.RunAsync(slot, Lifetime);
            if (Disposed) return;
            if (outcome == RecruitOutcome.Expired) Leave(null);
            else Refresh();
        }

        int Slot()
        {
            var offers = _session.Shop.Offers;
            for (int i = 0; i < offers.Count; i++) if (offers[i].Id == _member) return i;
            return -1;
        }

        /// <summary>
        /// Le héros de la fiche n'existe plus (offre renouvelée, membre effacé) : retour à la page précédente, une seule fois,
        /// avec le toast <paramref name="error"/> si personne ne l'a déjà affiché.
        /// </summary>
        void Leave(DomainError? error)
        {
            if (_left) return;
            _left = true;
            if (error != null) _toasts.Show(_loc.Error(error), ToastKind.Error);
            _nav.Back();
        }

        void Refresh()
        {
            if (Disposed || _left) return;
            if (_fromShop) RefreshShop();
            else RefreshOwned();
        }

        void RefreshOwned()
        {
            var m = _session.Roster.FirstOrDefault(x => x.Id == _member);
            if (m == null) { Leave(HubErrors.UnknownMember); return; }
            bool max = m.Level >= MetaRules.MaxLevel;
            int cost = _session.TrainCostFor(m.Level);
            int missing = cost - _session.Wallet.Credits;
            var d = Sheet(m.Def, m.Level, true);
            d.FatigueText = _loc.F("ui.fatigue", m.Fatigue);
            d.Fatigue01 = m.Fatigue / 100f;
            d.Fatigue = m.Fatigue;
            d.PrimaryLabel = max ? _loc.T("hub.train.max") : _loc.F("hub.train", cost + " CR");
            d.PrimaryEnabled = !max && missing <= 0;
            d.PrimaryReason = !max && missing > 0 ? _loc.F("hub.train.missing", missing) : "";
            d.ShowArrows = _session.Roster.Count > 1;
            d.ShowRooms = true;
            d.ShowLoadout = true;
            d.LoadoutLabel = _loc.F("sheet.loadout_counts", m.Items.Count, 2, m.Cards.Count(c => c.HasValue), 2);
            View.Show(d);
        }

        void RefreshShop()
        {
            int slot = Slot();
            if (slot < 0) { Leave(ShopErrors.Expired); return; }
            var offer = _session.Shop.Offers[slot];
            var def = _session.Content.Member(offer.Id);
            bool inTeam = offer.Recruited || _session.Roster.Any(x => x.Id == offer.Id);
            var d = Sheet(def, MetaRules.RecruitLevel, false);
            d.FatigueText = "";
            d.PrimaryLabel = inTeam ? _loc.T("recruit.in_team") : _loc.F("hub.recruit", MetaRules.RecruitCost(def.Rarity));
            d.PrimaryEnabled = !inTeam;
            d.PrimaryReason = "";
            View.Show(d);
        }

        MemberSheetData Sheet(MemberDef def, int level, bool owned) => new MemberSheetData
        {
            Card = MemberCards.Card(def, level, _session.Content.Balance, _loc),
            Owned = owned,
            Skills = def.Skills.Select(s => new SkillLineData { Name = _loc.SkillName(s), Description = _loc.DescribeSkill(s) }).ToArray(),
            Synergies = SquadSynergies.For(def.Family, _session.Content.Synergies)
                                .Select(c => _loc.F("sheet.synergy_line", _loc.SynergyName(c), _loc.SynergyEffect(c), _loc.FamilyName(c.A == def.Family ? c.B : c.A)))
                                .ToArray(),
        };
    }
}
