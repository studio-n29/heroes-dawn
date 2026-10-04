#nullable enable
using System;
using System.Linq;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Hub;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>
    /// Fiche d'un membre possédé (M21) : stats, compétences décrites, combos de sa famille, flèches pour parcourir l'effectif
    /// (sans empiler de page), raccourcis Salon / Infirmerie, Entraîner · N CR.
    /// </summary>
    public sealed class MemberSheetPresenter : PresenterBase<IMemberSheetView>, IPageTitle
    {
        readonly ISessionReadModel _session;
        readonly ITrainMemberUseCase _train;
        readonly HubNavigator _nav;
        readonly IToastService _toasts;
        readonly ILocalization _loc;
        readonly ISubscriber<WalletChanged> _wallet;
        readonly ISubscriber<RosterChanged> _roster;
        readonly PageAction _action;
        IDisposable? _subs;
        MemberId _member;

        public MemberSheetPresenter(IMemberSheetView view, HubLocation location, ISessionReadModel session, ITrainMemberUseCase train,
                                    HubNavigator nav, IToastService toasts, ILocalization loc,
                                    ISubscriber<WalletChanged> wallet, ISubscriber<RosterChanged> roster)
            : base(view, null)
        {
            _session = session; _train = train; _nav = nav; _toasts = toasts; _loc = loc; _wallet = wallet; _roster = roster;
            _member = location.Member ?? throw new ArgumentException("Fiche sans membre", nameof(location));
            _action = new PageAction(toasts, loc);
        }

        public string Title => _loc.T("page.member_sheet");

        protected override void OnStart()
        {
            View.PreviousClicked += Previous;
            View.NextClicked += Next;
            View.PrimaryClicked += Train;
            View.RoomClicked += OpenRoom;
        }

        protected override void OnShown()
        {
            _subs?.Dispose();
            var bag = DisposableBag.CreateBuilder();
            _wallet.Subscribe(_ => Refresh()).AddTo(bag);
            _roster.Subscribe(_ => Refresh()).AddTo(bag);
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
            View.PrimaryClicked -= Train;
            View.RoomClicked -= OpenRoom;
        }

        ITeamMemberReadModel? Current => _session.Roster.FirstOrDefault(m => m.Id == _member);

        void Previous() => Step(-1);
        void Next() => Step(1);

        void Step(int delta)
        {
            var roster = _session.Roster;
            if (roster.Count == 0) return;
            int i = 0;
            for (int k = 0; k < roster.Count; k++) if (roster[k].Id == _member) { i = k; break; }
            _member = roster[((i + delta) % roster.Count + roster.Count) % roster.Count].Id;
            Refresh();
        }

        void OpenRoom(HubPage page) => _nav.Open(page);

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

        void Refresh()
        {
            if (Disposed) return;
            var m = Current;
            if (m == null) return;
            var def = m.Def;
            bool max = m.Level >= MetaRules.MaxLevel;
            int cost = MetaRules.TrainCost(m.Level);
            int missing = cost - _session.Wallet.Credits;
            View.Show(new MemberSheetData
            {
                Card = MemberCards.Card(def, m.Level, _session.Content.Balance, _loc),
                Owned = true,
                FatigueText = _loc.F("ui.fatigue", m.Fatigue),
                Fatigue01 = m.Fatigue / 100f,
                Fatigue = m.Fatigue,
                Skills = def.Skills.Select(s => new SkillLineData { Name = _loc.SkillName(s), Description = _loc.DescribeSkill(s) }).ToArray(),
                Combos = SquadCombos.For(def.Family, _session.Content.Combos)
                                    .Select(c => _loc.F("sheet.combo_line", _loc.ComboName(c), _loc.FamilyName(c.A == def.Family ? c.B : c.A)))
                                    .ToArray(),
                PrimaryLabel = max ? _loc.T("hub.train.max") : _loc.F("hub.train", cost + " CR"),
                PrimaryEnabled = !max && missing <= 0,
                PrimaryReason = !max && missing > 0 ? _loc.F("hub.train.missing", missing) : "",
                ShowArrows = _session.Roster.Count > 1,
                ShowRooms = true,
            });
        }
    }
}
