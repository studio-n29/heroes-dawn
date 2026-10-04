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
    /// <summary>Ligne du Salon : membre, prix de l'entraînement, raison si grisé.</summary>
    public struct SalonRowData
    {
        public MemberId Id;
        public MemberCardData Card;
        public string Label;
        public bool Enabled;
        public string Reason;
    }

    public struct SalonPageData
    {
        public string Banner;
        public SalonRowData[] Rows;
    }

    public interface ISalonPageView : IView
    {
        event Action<MemberId> TrainClicked;
        void Show(SalonPageData data);
    }

    /// <summary>Salon d'entraînement, version simple (spec §4) : entraînement instantané, du plus bas niveau au plus haut.</summary>
    public sealed class SalonPagePresenter : PresenterBase<ISalonPageView>, IPageTitle
    {
        readonly ISessionReadModel _session;
        readonly ITrainMemberUseCase _train;
        readonly IToastService _toasts;
        readonly ILocalization _loc;
        readonly ISubscriber<WalletChanged> _wallet;
        readonly ISubscriber<RosterChanged> _roster;
        readonly PageAction _action;
        IDisposable? _subs;
        System.Collections.Generic.List<MemberId>? _order;

        public SalonPagePresenter(ISalonPageView view, ISessionReadModel session, ITrainMemberUseCase train, IToastService toasts,
                                  ILocalization loc, ISubscriber<WalletChanged> wallet, ISubscriber<RosterChanged> roster)
            : base(view, null)
        {
            _session = session; _train = train; _toasts = toasts; _loc = loc; _wallet = wallet; _roster = roster;
            _action = new PageAction(toasts, loc);
        }

        public string Title => _loc.T("page.salon");

        protected override void OnStart() => View.TrainClicked += Train;

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
            View.TrainClicked -= Train;
        }

        void Train(MemberId id) => _action.Run(ct => _train.ExecuteAsync(id, ct), Lifetime, () =>
        {
            var m = _session.Roster.FirstOrDefault(x => x.Id == id);
            if (m != null) _toasts.Show(_loc.F("hub.trained", _loc.MemberName(m.Def), m.Level), ToastKind.Reward);
            Refresh();
        });

        void Refresh()
        {
            if (Disposed) return;
            View.Show(new SalonPageData
            {
                Banner = _loc.T("salon.simple"),
                Rows = Ordered().Select(Row).ToArray(),
            });
        }

        /// <summary>
        /// Du plus bas niveau au plus haut, ordre figé à l'ouverture : une ligne ne glisse jamais sous le doigt après un entraînement
        /// (un second toucher paierait pour un autre membre). Nouveaux membres à la fin, membres disparus retirés.
        /// </summary>
        System.Collections.Generic.IEnumerable<ITeamMemberReadModel> Ordered()
        {
            var roster = _session.Roster;
            _order ??= roster.OrderBy(m => m.Level).Select(m => m.Id).ToList();
            foreach (var m in roster) if (!_order.Contains(m.Id)) _order.Add(m.Id);
            _order.RemoveAll(id => roster.All(m => m.Id != id));
            return _order.Select(id => roster.First(m => m.Id == id));
        }

        SalonRowData Row(ITeamMemberReadModel m)
        {
            bool max = m.Level >= MetaRules.MaxLevel;
            int cost = _session.TrainCostFor(m.Level);
            int missing = cost - _session.Wallet.Credits;
            return new SalonRowData
            {
                Id = m.Id,
                Card = MemberCards.Card(m.Def, m.Level, _session.Content.Balance, _loc),
                Label = max ? _loc.T("hub.train.max") : _loc.F("hub.train", cost + " CR"),
                Enabled = !max && missing <= 0,
                Reason = !max && missing > 0 ? _loc.F("hub.train.missing", missing) : "",
            };
        }
    }
}
