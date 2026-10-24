#nullable enable
using System.Collections.Generic;
using System.Linq;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Hub;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Hub
{
    public interface ILoungePageView : IListPageView { }


    /// <summary>
    /// Salon des agents : l'ancien Salon d'entraînement et l'ancienne Infirmerie sur un seul écran. Entraîner un agent coûte des Crédits,
    /// soigner l'équipe entière aussi. Les agents les plus fatigués viennent d'abord.
    /// </summary>
    public sealed class LoungePagePresenter : ListPagePresenter<ILoungePageView>
    {
        const int TiredFrom = 50;

        readonly ISessionReadModel _session;
        readonly ITrainMemberUseCase _train;
        readonly IHealRosterUseCase _heal;
        readonly PageAction _action;
        readonly List<MemberId> _rows = new List<MemberId>();

        public LoungePagePresenter(ILoungePageView view, ISessionReadModel session, ITrainMemberUseCase train, IHealRosterUseCase heal,
                                   ILocalization loc, IToastService toasts, ISubscriber<MetaChanged> meta, ISubscriber<WalletChanged> wallet,
                                   ISubscriber<RosterChanged> roster, IClock clock)
            : base(view, loc, toasts, meta, wallet, roster, clock)
        {
            _session = session; _train = train; _heal = heal;
            _action = new PageAction(toasts, loc);
        }

        public override string Title => Loc.T("page.salon");

        protected override void Render()
        {
            var roster = _session.Roster;
            int toTrain = roster.Count(m => m.Level < MetaRules.MaxLevel);
            int tired = roster.Count(m => m.Fatigue > 0);
            int healMissing = MetaRules.InfirmaryCost - _session.Wallet.Credits;
            View.ShowHeader(new ListHeaderData
            {
                Title = Loc.T("lounge.header"),
                Line = Loc.F("lounge.line", roster.Count, tired),
                Progress = -1f,
                A = RowButtonData.Of(Loc.F("hub.heal", MetaRules.InfirmaryCost + " CR"), tired > 0 && healMissing <= 0,
                                     tired == 0 ? Loc.T("qg.infirmary.none") : Loc.F("hub.train.missing", healMissing), 1),
            });

            _rows.Clear();
            var rows = new List<InfoRowData>();
            foreach (var m in roster.OrderByDescending(x => x.Fatigue).ThenBy(x => x.Level))
            {
                bool max = m.Level >= MetaRules.MaxLevel;
                int cost = _session.TrainCostFor(m.Level);
                int missing = cost - _session.Wallet.Credits;
                var r = InfoRowData.Basic(Loc.MemberName(m.Def), Loc.F("lounge.row", m.Level, Loc.F("ui.fatigue", m.Fatigue)));
                r.Tag = CollectionText.Tag(Loc.MemberName(m.Def));
                r.TagColor = Theme.FamilyColor(m.Def.Family);
                r.Progress = m.Fatigue / 100f;
                r.ProgressLabel = Loc.F("ui.fatigue", m.Fatigue);
                r.Badge = m.Fatigue >= 100 ? Loc.T("infirmary.band.exhausted") : m.Fatigue >= TiredFrom ? Loc.T("infirmary.band.tired") : "";
                r.Art = AgentArt.For(m.Def.Id.Value);
                r.A = max ? RowButtonData.Of(Loc.T("hub.train.max"), false, "", 3)
                          : RowButtonData.Of(Loc.F("hub.train", cost + " CR"), missing <= 0, missing > 0 ? Loc.F("hub.train.missing", missing) : "", 0);
                rows.Add(r);
                _rows.Add(m.Id);
            }
            View.ShowRows(rows.ToArray());
        }

        protected override void OnHeader(int index)
        {
            if (index != 0) return;
            _action.Run(ct => _heal.ExecuteAsync(ct), Lifetime, () => { Toasts.Show(Loc.T("hub.healed"), ToastKind.Reward); Refresh(); });
        }

        protected override void OnRowButton(int row, int button)
        {
            if (button != 0 || row < 0 || row >= _rows.Count) return;
            var id = _rows[row];
            _action.Run(ct => _train.ExecuteAsync(id, ct), Lifetime, () =>
            {
                var m = _session.Roster.FirstOrDefault(x => x.Id == id);
                if (m != null) Toasts.Show(Loc.F("hub.trained", Loc.MemberName(m.Def), m.Level), ToastKind.Reward);
                Refresh();
            });
        }
    }
}
