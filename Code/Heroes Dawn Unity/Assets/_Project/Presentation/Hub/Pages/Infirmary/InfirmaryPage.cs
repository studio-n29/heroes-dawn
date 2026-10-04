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
    /// <summary>Palier de fatigue : en forme (moins de 60), fatigué (60 à 99), épuisé (100).</summary>
    public enum FatigueBand { Fit, Tired, Exhausted }

    public struct InfirmaryRowData
    {
        public MemberId Id;
        public MemberCardData Card;
        public int Fatigue;
        public float Fatigue01;
        public string FatigueText;
        public FatigueBand Band;
        public string BandText;
    }

    public struct InfirmaryPageData
    {
        public string Banner;
        public InfirmaryRowData[] Rows;
        public string HealLabel;
        public bool HealEnabled;
        public string HealReason;
    }

    public interface IInfirmaryPageView : IView
    {
        event Action HealClicked;
        void Show(InfirmaryPageData data);
    }

    /// <summary>Infirmerie, version simple (spec §4) : un soin pour toute l'équipe (-30 % de fatigue, 150 CR), du plus fatigué au moins fatigué.</summary>
    public sealed class InfirmaryPagePresenter : PresenterBase<IInfirmaryPageView>, IPageTitle
    {
        public const int TiredFrom = 60;

        readonly ISessionReadModel _session;
        readonly IHealRosterUseCase _heal;
        readonly IToastService _toasts;
        readonly ILocalization _loc;
        readonly ISubscriber<WalletChanged> _wallet;
        readonly ISubscriber<RosterChanged> _roster;
        readonly PageAction _action;
        IDisposable? _subs;

        public InfirmaryPagePresenter(IInfirmaryPageView view, ISessionReadModel session, IHealRosterUseCase heal, IToastService toasts,
                                      ILocalization loc, ISubscriber<WalletChanged> wallet, ISubscriber<RosterChanged> roster)
            : base(view, null)
        {
            _session = session; _heal = heal; _toasts = toasts; _loc = loc; _wallet = wallet; _roster = roster;
            _action = new PageAction(toasts, loc);
        }

        public string Title => _loc.T("page.infirmary");

        protected override void OnStart() => View.HealClicked += Heal;

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
            View.HealClicked -= Heal;
        }

        void Heal() => _action.Run(ct => _heal.ExecuteAsync(ct), Lifetime, () =>
        {
            _toasts.Show(_loc.T("hub.healed"), ToastKind.Reward);
            Refresh();
        });

        void Refresh()
        {
            if (Disposed) return;
            bool any = _session.Roster.Any(m => m.Fatigue > 0);
            int missing = MetaRules.InfirmaryCost - _session.Wallet.Credits;
            View.Show(new InfirmaryPageData
            {
                Banner = _loc.T("infirmary.simple"),
                Rows = _session.Roster.OrderByDescending(m => m.Fatigue).Select(Row).ToArray(),
                HealLabel = _loc.F("hub.heal", MetaRules.InfirmaryCost + " CR"),
                HealEnabled = any && missing <= 0,
                HealReason = !any ? _loc.T("qg.infirmary.none") : missing > 0 ? _loc.F("hub.train.missing", missing) : "",
            });
        }

        InfirmaryRowData Row(ITeamMemberReadModel m)
        {
            var band = m.Fatigue >= 100 ? FatigueBand.Exhausted : m.Fatigue >= TiredFrom ? FatigueBand.Tired : FatigueBand.Fit;
            return new InfirmaryRowData
            {
                Id = m.Id,
                Card = MemberCards.Card(m.Def, m.Level, _session.Content.Balance, _loc),
                Fatigue = m.Fatigue,
                Fatigue01 = m.Fatigue / 100f,
                FatigueText = _loc.F("ui.fatigue", m.Fatigue),
                Band = band,
                BandText = _loc.T(band == FatigueBand.Exhausted ? "infirmary.band.exhausted" : band == FatigueBand.Tired ? "infirmary.band.tired" : "infirmary.band.fit"),
            };
        }
    }
}
