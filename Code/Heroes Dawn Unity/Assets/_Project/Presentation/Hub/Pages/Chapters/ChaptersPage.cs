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
    /// « Relire » rouvre la fenêtre d'ouverture sans toucher aux chapitres déjà vus. La colonne de droite liste les fragments de souvenirs du chapitre choisi.
    /// </summary>
    public sealed class ChaptersPagePresenter : ListPagePresenter<IChaptersPageView>
    {
        readonly ISessionReadModel _session;
        readonly ChapterBook _book;
        readonly ChapterIntroFlow _intro;
        readonly List<ChapterDef> _rows = new List<ChapterDef>();
        readonly MemoryFragmentBook _fragments;
        readonly IFragmentAudio _audio;
        readonly IModalService _modals;
        readonly HubNavigator _nav;
        string? _selected;
        bool _reading;

        public ChaptersPagePresenter(IChaptersPageView view, ISessionReadModel session, ChapterBook book, ChapterIntroFlow intro, MemoryFragmentBook fragments, IFragmentAudio audio, IModalService modals, HubNavigator nav, ILocalization loc,
                                     IToastService toasts, ISubscriber<MetaChanged> meta, ISubscriber<WalletChanged> wallet,
                                     ISubscriber<RosterChanged> roster, IClock clock)
            : base(view, loc, toasts, meta, wallet, roster, clock)
        {
            _session = session; _book = book; _intro = intro; _fragments = fragments; _audio = audio; _modals = modals; _nav = nav;
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
                A = RowButtonData.Of(Loc.T("chapters.lair"), true, "", 0),
            });

            _rows.Clear();
            var rows = new List<InfoRowData>();
            foreach (var chapter in ChapterProgress.InDisplayOrder(_session))
            {
                var status = ChapterProgress.StatusOf(_session, chapter);
                int fresh = _fragments.NewCount(chapter.Id);
                string state = Loc.T("chapter.status." + status) + (fresh > 0 ? " · " + Loc.F("fragments.new", fresh) : "");
                var r = InfoRowData.Basic(Loc.T(chapter.TitleKey), Loc.F("chapters.row_sub", state, Excerpt(Loc.T(chapter.IntroKey))));
                r.Tag = chapter.Order.ToString();
                r.TagColor = status == ChapterStatus.Done ? Theme.Success : status == ChapterStatus.Current ? Theme.Primary : Theme.TextMuted;
                r.Highlight = chapter.Id == _selected;
                r.Dim = status == ChapterStatus.Locked || status == ChapterStatus.Soon;
                r.A = RowButtonData.Of(Loc.T("chapter.relire"), status != ChapterStatus.Soon, Loc.T("chapter.soon_reason"), 0);
                rows.Add(r);
                _rows.Add(chapter);
            }
            if (_selected == null && _rows.Count > 0) { _selected = _rows[0].Id; rows[0] = Highlighted(rows[0]); }
            View.ShowRows(rows.ToArray());
            ShowFragments();
        }

        static InfoRowData Highlighted(InfoRowData r) { r.Highlight = true; return r; }

        static string Excerpt(string text) => text.Length <= 120 ? text : text.Substring(0, 120).TrimEnd() + "…";

        ChapterDef? SelectedChapter => _rows.FirstOrDefault(c => c.Id == _selected);

        /// <summary>Colonne de droite : les souvenirs du chapitre choisi (verrouillés tant que leur zone n'est pas validée) et le texte du souvenir ouvert.</summary>
        void ShowFragments()
        {
            var chapter = SelectedChapter;
            if (chapter == null) return;
            var fragments = _fragments.OfChapter(chapter.Id);
            var rows = new InfoRowData[fragments.Count];
            string text = fragments.Count == 0 ? Loc.T("fragments.none") : ""; // le récit s'ouvre dans une fenêtre ; pas de ligne d'aide qui gênerait les rangées
            for (int i = 0; i < rows.Length; i++)
            {
                var f = fragments[i];
                bool open = _fragments.Unlocked(f);
                var r = InfoRowData.Basic(open ? Loc.T(f.TitleKey) : Loc.T("fragments.locked"),
                                          open ? (_audio.Has(f.AudioKey) ? Loc.T("fragments.audio") : Loc.T("fragments.no_audio")) : Loc.F("fragments.unlock", ZoneName(f.UnlockZone)));
                r.Tag = (i + 1).ToString();
                r.TagColor = !open ? Theme.TextMuted : _fragments.Read(f.Id) ? Theme.Success : Theme.Accent;
                r.Dim = !open;
                r.A = RowButtonData.Of(Loc.T("fragments.read"), open, Loc.F("fragments.unlock", ZoneName(f.UnlockZone)), 0);
                rows[i] = r;
            }
            View.ShowSide(new ListSideData
            {
                Title = Loc.F("fragments.title", Loc.T(chapter.TitleKey), _fragments.UnlockedCount(chapter.Id), fragments.Count),
                Rows = rows,
                Text = text,
            });
        }

        string ZoneName(string zoneId)
        {
            var zone = _session.Content.Zones.FirstOrDefault(z => z.Id.Value == zoneId);
            return zone == null ? zoneId : Loc.T(zone.NameKey.Replace(".name", ".short"));
        }

        protected override void OnHeader(int index) { if (index == 0) _nav.Open(HubPage.Lair); }

        protected override void OnRowSelected(int row)
        {
            if (row < 0 || row >= _rows.Count || _rows[row].Id == _selected) return;
            _selected = _rows[row].Id;
            Refresh();
        }

        protected override void OnRowButton(int row, int button)
        {
            if (row < 0 || row >= _rows.Count) return;
            _selected = _rows[row].Id;
            ReadAsync(_rows[row]).Forget();
            Refresh();
        }

        /// <summary>Un souvenir ouvert se lit dans la colonne de droite et se marque lu ; un souvenir verrouillé ne s'ouvre pas.</summary>
        protected override void OnSideButton(int row, int button) => OpenFragment(row);

        protected override void OnSideRowSelected(int row) => OpenFragment(row);

        void OpenFragment(int row)
        {
            var chapter = SelectedChapter;
            if (chapter == null) return;
            var fragments = _fragments.OfChapter(chapter.Id);
            if (row < 0 || row >= fragments.Count || !_fragments.Unlocked(fragments[row])) return;
            if (_reading) return;
            var fragment = fragments[row];
            _fragments.MarkRead(fragment.Id); // publie MetaChanged : la page se rafraîchit
            ReadFragmentAsync(fragment).Forget();
        }

        /// <summary>Le récit du souvenir dans une fenêtre simple : titre, texte, et « audio à venir » tant que la voix n'existe pas.</summary>
        async UniTaskVoid ReadFragmentAsync(MemoryFragmentDef fragment)
        {
            _reading = true;
            try
            {
                string body = Loc.T(fragment.TextKey) + (_audio.Has(fragment.AudioKey) ? "" : "\n\n" + Loc.T("fragments.no_audio"));
                await _modals.ConfirmAsync(new ConfirmData(Loc.T(fragment.TitleKey), body, Loc.T("fragments.close"), "", Tone.Primary), Lifetime);
            }
            catch (OperationCanceledException) { }
            finally { _reading = false; }
        }

        async UniTaskVoid ReadAsync(ChapterDef chapter)
        {
            try { await _intro.ShowAsync(chapter, Lifetime); }
            catch (OperationCanceledException) { }
        }
    }
}
