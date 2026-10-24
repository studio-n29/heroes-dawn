#nullable enable
using System.Linq;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Hub
{
    public interface IArchivesPageView : IListPageView { }

    public sealed class ArchivesPagePresenter : ListPagePresenter<IArchivesPageView>
    {
        readonly ArchiveBook _archives;
        readonly N29.HeroesDawn.Application.Session.ISessionReadModel? _session;
        string _selected = "";

        public ArchivesPagePresenter(IArchivesPageView view, ArchiveBook archives, ILocalization loc, IToastService toasts,
                                     ISubscriber<MetaChanged> meta, ISubscriber<WalletChanged> wallet, ISubscriber<RosterChanged> roster, IClock clock,
                                     N29.HeroesDawn.Application.Session.ISessionReadModel? session = null)
            : base(view, loc, toasts, meta, wallet, roster, clock)
        {
            _archives = archives; _session = session;
        }

        public override string Title => Loc.T("page.archives");

        protected override void Render()
        {
            var entries = _archives.Entries();
            int unlocked = entries.Count(e => e.Unlocked);
            if (_selected.Length == 0 || entries.All(e => e.Id != _selected))
                _selected = (entries.FirstOrDefault(e => e.Unread).Id ?? entries.First(e => e.Unlocked).Id);
            View.ShowHeader(new ListHeaderData
            {
                Title = Loc.T("archives.title"),
                Line = _session == null ? Loc.F("archives.count", unlocked, entries.Count)
                     : Loc.F("profile.line", _session.Route.HeroName, _session.CompletedZones.Count, _session.Roster.Count, unlocked, entries.Count),
                Progress = unlocked / (float)entries.Count,
                ProgressLabel = unlocked + " / " + entries.Count,
            });
            var rows = new InfoRowData[entries.Count];
            for (int i = 0; i < rows.Length; i++)
            {
                var e = entries[i];
                var r = InfoRowData.Basic(e.Unlocked ? Loc.T(e.TitleKey) : Loc.T("archives.locked"), Loc.T("archives.cat." + e.Category));
                r.Tag = e.Category switch { ArchiveCategory.Hero => "H", ArchiveCategory.Enemy => "E", ArchiveCategory.Zone => "Z", _ => "L" };
                r.TagColor = e.Unlocked ? CategoryColor(e.Category) : Theme.Surface3;
                r.Badge = e.Unread ? Loc.T("archives.new") : "";
                r.Highlight = e.Id == _selected;
                r.Dim = !e.Unlocked;
                rows[i] = r;
            }
            View.ShowRows(rows);
            var sel = entries.First(e => e.Id == _selected);
            View.ShowSide(new ListSideData
            {
                Title = sel.Unlocked ? Loc.T(sel.TitleKey) : Loc.T("archives.locked"),
                Rows = System.Array.Empty<InfoRowData>(),
                Text = sel.Unlocked ? Loc.T(sel.BodyKey) : Loc.T("archives.locked_hint"),
            });
        }

        static Color CategoryColor(ArchiveCategory c) => c == ArchiveCategory.Hero ? Theme.Primary : c == ArchiveCategory.Enemy ? Theme.Danger
                                                      : c == ArchiveCategory.Zone ? Theme.Info : Theme.Perception;

        protected override void OnRowSelected(int row)
        {
            var entries = _archives.Entries();
            if (row < 0 || row >= entries.Count) return;
            var e = entries[row];
            _selected = e.Id;
            if (e.Unlocked) _archives.MarkRead(e.Id);
            Refresh();
        }
    }
}
