#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Features.World;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Hub
{
    public interface IChaptersPageView : IListPageView { }

    /// <summary>
    /// Page Chapitres : les 8 chapitres (celui de l'acte en cours d'abord), leur statut et le début de leur texte d'ouverture ;
    /// « Relire » rouvre la fenêtre d'ouverture sans toucher aux chapitres déjà vus.
    /// </summary>
    public sealed class ChaptersPagePresenter : ListPagePresenter<IChaptersPageView>
    {
        readonly ISessionReadModel _session;
        readonly ChapterBook _book;
        readonly ChapterIntroFlow _intro;
        readonly List<ChapterDef> _rows = new List<ChapterDef>();

        public ChaptersPagePresenter(IChaptersPageView view, ISessionReadModel session, ChapterBook book, ChapterIntroFlow intro, ILocalization loc,
                                     IToastService toasts, ISubscriber<MetaChanged> meta, ISubscriber<WalletChanged> wallet,
                                     ISubscriber<RosterChanged> roster, IClock clock)
            : base(view, loc, toasts, meta, wallet, roster, clock)
        {
            _session = session; _book = book; _intro = intro;
        }

        public override string Title => Loc.T("page.chapters");

        protected override void Render()
        {
            int done = _book.DoneCount();
            View.ShowHeader(new ListHeaderData
            {
                Title = Loc.T("chapters.header"),
                Line = Loc.F("chapters.line", done, Chapters.All.Count),
                Progress = done / (float)Chapters.All.Count,
                ProgressLabel = done + " / " + Chapters.All.Count,
            });

            _rows.Clear();
            var rows = new List<InfoRowData>();
            foreach (var chapter in ChapterProgress.InDisplayOrder(_session))
            {
                var status = ChapterProgress.StatusOf(_session, chapter);
                var r = InfoRowData.Basic(Loc.T(chapter.TitleKey), Loc.F("chapters.row_sub", Loc.T("chapter.status." + status), Excerpt(Loc.T(chapter.IntroKey))));
                r.Tag = chapter.Order.ToString();
                r.TagColor = status == ChapterStatus.Done ? Theme.Success : status == ChapterStatus.Current ? Theme.Primary : Theme.TextMuted;
                r.Highlight = status == ChapterStatus.Current;
                r.Dim = status == ChapterStatus.Locked || status == ChapterStatus.Soon;
                r.A = RowButtonData.Of(Loc.T("chapter.relire"), status != ChapterStatus.Soon, Loc.T("chapter.soon_reason"), 0);
                rows.Add(r);
                _rows.Add(chapter);
            }
            View.ShowRows(rows.ToArray());
        }

        static string Excerpt(string text) => text.Length <= 120 ? text : text.Substring(0, 120).TrimEnd() + "…";

        protected override void OnRowButton(int row, int button)
        {
            if (row < 0 || row >= _rows.Count) return;
            ReadAsync(_rows[row]).Forget();
        }

        async UniTaskVoid ReadAsync(ChapterDef chapter)
        {
            try { await _intro.ShowAsync(chapter, Lifetime); }
            catch (OperationCanceledException) { }
        }
    }
}
