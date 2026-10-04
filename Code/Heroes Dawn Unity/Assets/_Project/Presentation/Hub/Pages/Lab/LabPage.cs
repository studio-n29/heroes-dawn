#nullable enable
using System.Linq;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Hub
{
    public interface ILabPageView : IListPageView { }

    public sealed class LabPagePresenter : ListPagePresenter<ILabPageView>
    {
        readonly ResearchLab _lab;
        readonly ISessionReadModel _session;

        public LabPagePresenter(ILabPageView view, ResearchLab lab, ISessionReadModel session, ILocalization loc, IToastService toasts,
                                ISubscriber<MetaChanged> meta, ISubscriber<WalletChanged> wallet, ISubscriber<RosterChanged> roster, IClock clock)
            : base(view, loc, toasts, meta, wallet, roster, clock)
        {
            _lab = lab; _session = session;
        }

        public override string Title => Loc.T("page.lab");
        protected override bool HasTimers => true;

        protected override void OnPageShown() => Finish();

        void Finish()
        {
            var done = _lab.Refresh();
            if (done != null) Toasts.Show(Loc.F("lab.done", Loc.T("lab." + done.Id)), ToastKind.Reward);
        }

        protected override void Render()
        {
            Finish();
            var views = _lab.Views();
            var active = views.FirstOrDefault(v => v.Active);
            View.ShowHeader(new ListHeaderData
            {
                Title = Loc.T("lab.title"),
                Line = active.Tech != null ? Loc.F("lab.running", Loc.T("lab." + active.Tech.Id), MetaFormat.Duration(active.Left)) : Loc.T("lab.idle"),
                Progress = -1f,
                A = active.Tech != null
                    ? RowButtonData.Of(Loc.F("lab.speed", ResearchLab.SpeedUpCost(active.Left)), _session.Wallet.Gems >= ResearchLab.SpeedUpCost(active.Left), Loc.T("errors.not_enough_gems"), 1)
                    : default,
            });
            var rows = new InfoRowData[views.Count];
            for (int i = 0; i < rows.Length; i++)
            {
                var v = views[i];
                var r = InfoRowData.Basic(Loc.F("lab.level", Loc.T("lab." + v.Tech.Id), v.Level, v.Tech.MaxLevel), Loc.T("lab." + v.Tech.Id + ".desc"));
                r.Tag = v.Level.ToString();
                r.TagColor = v.Maxed ? Theme.Success : Theme.Info;
                r.Highlight = v.Active;
                if (v.Active)
                {
                    int minutes = v.Tech.MinutesFor(v.Level);
                    r.Progress = Mathf.Clamp01(1f - (float)(v.Left.TotalMinutes / minutes));
                    r.ProgressLabel = MetaFormat.Duration(v.Left);
                }
                if (v.Maxed) r.A = RowButtonData.Of(Loc.T("lab.max"), false, "", 3);
                else
                {
                    int cost = v.Tech.CostFor(v.Level);
                    bool busy = active.Tech != null;
                    bool afford = _session.Wallet.Credits >= cost;
                    r.A = RowButtonData.Of(Loc.F("lab.start", cost), !busy && afford, busy ? Loc.T("lab.busy") : Loc.F("hub.train.missing", cost - _session.Wallet.Credits), 0);
                    r.Subtitle += " · " + Loc.F("lab.time", MetaFormat.Duration(System.TimeSpan.FromMinutes(v.Tech.MinutesFor(v.Level))));
                }
                rows[i] = r;
            }
            View.ShowRows(rows);
        }

        protected override void OnHeader(int index) => Report(_lab.SpeedUp(), Loc.T("lab.sped"));

        protected override void OnRowButton(int row, int button)
        {
            if (row < 0 || row >= ResearchLab.Techs.Count) return;
            Report(_lab.Start(ResearchLab.Techs[row].Id), Loc.T("lab.started"));
        }
    }
}
