#nullable enable
using System.Linq;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Hub
{
    public interface IOperationsPageView : IListPageView { }

    public sealed class OperationsPagePresenter : ListPagePresenter<IOperationsPageView>
    {
        readonly Expeditions _ops;
        readonly ISessionReadModel _session;

        public OperationsPagePresenter(IOperationsPageView view, Expeditions ops, ISessionReadModel session, ILocalization loc, IToastService toasts,
                                       ISubscriber<MetaChanged> meta, ISubscriber<WalletChanged> wallet, ISubscriber<RosterChanged> roster, IClock clock)
            : base(view, loc, toasts, meta, wallet, roster, clock)
        {
            _ops = ops; _session = session;
        }

        public override string Title => Loc.T("page.operations");
        protected override bool HasTimers => true;

        protected override void Render()
        {
            int free = _ops.Available().Count;
            View.ShowHeader(new ListHeaderData
            {
                Title = Loc.T("ops.title"),
                Line = free > 0 ? Loc.F("ops.free", free) : Loc.T("ops.none_free"),
                Progress = -1f,
            });
            var slots = _ops.SlotViews();
            var rows = new InfoRowData[slots.Count];
            for (int i = 0; i < rows.Length; i++) rows[i] = RowOf(slots[i], free);
            View.ShowRows(rows);
        }

        InfoRowData RowOf(ExpeditionSlotView s, int free)
        {
            var r = InfoRowData.Basic("", "");
            r.Tag = (s.Slot + 1).ToString();
            if (s.Empty)
            {
                r.TagColor = Theme.Surface3;
                r.Title = Loc.F("ops.slot_free", s.Slot + 1);
                r.Subtitle = Loc.T("ops.templates");
                var t = Expeditions.Templates;
                r.A = Button(t[0], free);
                r.B = Button(t[1], free);
                r.C = Button(t[2], free);
                return r;
            }
            var tpl = s.Template!;
            r.Title = Loc.T("ops." + tpl.Id);
            r.Subtitle = Loc.F("ops.running", string.Join(", ", s.Members.Select(id => Loc.T("members." + id + ".name"))), MetaFormat.RewardText(tpl.Reward, Loc));
            if (s.Ready)
            {
                r.TagColor = Theme.Success;
                r.Highlight = true;
                r.Progress = 1f;
                r.ProgressLabel = Loc.T("ops.back");
                r.A = RowButtonData.Of(Loc.T("ops.claim"), true, "", 0);
            }
            else
            {
                r.TagColor = Theme.Info;
                r.Progress = Mathf.Clamp01(1f - (float)(s.Left.TotalMinutes / tpl.Minutes));
                r.ProgressLabel = MetaFormat.Duration(s.Left);
                bool gems = _session.Wallet.Gems >= Expeditions.SpeedUpCost(s.Left);
                r.A = RowButtonData.Of(Loc.F("ops.speed", Expeditions.SpeedUpCost(s.Left)), gems, Loc.T("errors.not_enough_gems"), 1);
            }
            return r;
        }

        RowButtonData Button(ExpeditionTemplate t, int free)
            => RowButtonData.Of(Loc.F("ops.go", Duration(t.Minutes), t.Members), free >= t.Members, Loc.F("ops.need", t.Members), 0);

        static string Duration(int minutes) => minutes >= 60 ? (minutes / 60) + " h" : minutes + " min";

        protected override void OnRowButton(int row, int button)
        {
            var slots = _ops.SlotViews();
            if (row < 0 || row >= slots.Count) return;
            var s = slots[row];
            if (s.Empty)
            {
                var t = Expeditions.Templates[Mathf.Clamp(button, 0, 2)];
                Report(_ops.Start(s.Slot, t.Id), Loc.T("ops.started"));
            }
            else if (s.Ready) Report(_ops.Claim(s.Slot), "ops.claimed");
            else Report(_ops.SpeedUp(s.Slot));
        }
    }
}
