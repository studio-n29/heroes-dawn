#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>
    /// Page « grille de cartes » (Grimoire, Équipement) : en-tête (titre, ligne, barre, 2 boutons), grille de tuiles défilante à gauche,
    /// fiche détaillée de la tuile choisie à droite (illustration, texte, jusqu'à trois boutons).
    /// Elle reprend l'interface des pages liste : une tuile par ligne (les lignes de section, sans pastille ni bouton, ne sont pas dessinées),
    /// et les boutons de la fiche sont ceux de la ligne choisie.
    /// </summary>
    public abstract class CardGridPageView : ViewBase, IListPageView, IFilterBarView
    {
        [SerializeField] TMP_Text headerTitle = null!;
        [SerializeField] TMP_Text headerLine = null!;
        [SerializeField] UiBar headerBar = null!;
        [SerializeField] UiButton headerA = null!;
        [SerializeField] UiButton headerB = null!;
        [SerializeField] RectTransform grid = null!;
        [SerializeField] SheetTileView tilePrefab = null!;
        [SerializeField] Image detailArt = null!;
        [SerializeField] TMP_Text detailTitle = null!;
        [SerializeField] TMP_Text detailBadge = null!;
        [SerializeField] TMP_Text detailText = null!;
        [SerializeField] UiButton[] detailButtons = Array.Empty<UiButton>();
        [Tooltip("Barre de filtres (au-dessus de la grille) ; nulle : page sans filtres.")]
        [SerializeField] RectTransform? filterBar;
        [Tooltip("Zone de la grille, descendue sous la barre de filtres quand elle est visible.")]
        [SerializeField] RectTransform? gridArea;

        const float FilterRowH = 56f, FilterGap = 8f, FilterButtonW = 190f;
        readonly List<List<UiButton>> _filterButtons = new List<List<UiButton>>();
        string _filterShape = "";
        float _gridTop;

        readonly List<SheetTileView> _tiles = new List<SheetTileView>();
        readonly List<int> _rowOf = new List<int>();
        InfoRowData[] _rows = Array.Empty<InfoRowData>();
        int _selectedRow = -1;

        public event Action<int>? HeaderClicked;
        public event Action<int, int>? RowButtonClicked;
        public event Action<int>? RowSelected;
        public event Action<int, int>? SideButtonClicked;
        public event Action<int>? SideRowSelected;
        public event Action<int, int>? FilterClicked;

        public UiButton HeaderButtonA => headerA;
        public UiButton HeaderButtonB => headerB;
        public IReadOnlyList<SheetTileView> Tiles => _tiles;
        public int SelectedRow => _selectedRow;

        protected virtual void Awake()
        {
            if (gridArea != null) _gridTop = gridArea.offsetMax.y;
            if (filterBar != null) filterBar.gameObject.SetActive(false);
            headerA.Button.onClick.AddListener(() => HeaderClicked?.Invoke(0));
            headerB.Button.onClick.AddListener(() => HeaderClicked?.Invoke(1));
            for (int i = 0; i < detailButtons.Length; i++)
            {
                int k = i;
                detailButtons[i].Button.onClick.AddListener(() => { if (_selectedRow >= 0) RowButtonClicked?.Invoke(_selectedRow, k); });
            }
        }

        public void ShowHeader(ListHeaderData d)
        {
            headerTitle.text = d.Title;
            headerLine.text = d.Line;
            bool bar = d.Progress >= 0f;
            headerBar.gameObject.SetActive(bar);
            if (bar) { headerBar.Set(d.Progress); headerBar.SetLabel(d.ProgressLabel); }
            Apply(headerA, d.A);
            Apply(headerB, d.B);
        }

        /// <summary>Un rang de boutons par groupe (l'option choisie en jaune) ; la grille descend sous la barre, qui est masquée sans groupe.</summary>
        public void ShowFilters(FilterGroupData[] groups)
        {
            if (filterBar == null || gridArea == null) return;
            bool any = groups.Length > 0;
            filterBar.gameObject.SetActive(any);
            float h = any ? groups.Length * (FilterRowH + FilterGap) : 0f;
            filterBar.sizeDelta = new Vector2(filterBar.sizeDelta.x, h);
            gridArea.offsetMax = new Vector2(gridArea.offsetMax.x, _gridTop - h);
            string shape = string.Join("|", groups.Select(g => string.Join(",", g.Labels)));
            if (shape != _filterShape) BuildFilters(groups);
            _filterShape = shape;
            for (int g = 0; g < groups.Length && g < _filterButtons.Count; g++)
                for (int o = 0; o < _filterButtons[g].Count; o++)
                    _filterButtons[g][o].SetColors(o == groups[g].Selected ? Theme.Primary : Theme.Surface3, o == groups[g].Selected ? Theme.TextDark : Theme.Text);
        }

        void BuildFilters(FilterGroupData[] groups)
        {
            for (int i = filterBar!.childCount - 1; i >= 0; i--) Destroy(filterBar.GetChild(i).gameObject);
            _filterButtons.Clear();
            for (int g = 0; g < groups.Length; g++)
            {
                var row = new GameObject("Filters" + g, typeof(RectTransform)).GetComponent<RectTransform>();
                row.SetParent(filterBar, false);
                row.anchorMin = new Vector2(0, 1); row.anchorMax = new Vector2(1, 1); row.pivot = new Vector2(0.5f, 1);
                row.sizeDelta = new Vector2(0, FilterRowH);
                row.anchoredPosition = new Vector2(0, -g * (FilterRowH + FilterGap));
                var buttons = new List<UiButton>();
                for (int o = 0; o < groups[g].Labels.Length; o++)
                {
                    var b = Instantiate(headerA, row);
                    b.gameObject.name = "Filter" + o;
                    b.gameObject.SetActive(true);
                    var rt = b.Rect;
                    rt.anchorMin = rt.anchorMax = new Vector2(0, 0.5f);
                    rt.pivot = new Vector2(0, 0.5f);
                    rt.sizeDelta = new Vector2(FilterButtonW, FilterRowH);
                    rt.anchoredPosition = new Vector2(4 + o * (FilterButtonW + 8), 0);
                    b.SetLabel(groups[g].Labels[o]);
                    b.Interactable = true;
                    b.DisabledReason = "";
                    b.Button.onClick.RemoveAllListeners();
                    int group = g, option = o;
                    b.Button.onClick.AddListener(() => FilterClicked?.Invoke(group, option));
                    buttons.Add(b);
                }
                _filterButtons.Add(buttons);
            }
        }

        /// <summary>Pas de colonne de droite sur cette page : la fiche détaillée la remplace.</summary>
        public void ShowSide(ListSideData data) { }

        public void ShowRows(InfoRowData[] data)
        {
            _rows = data;
            _rowOf.Clear();
            for (int i = 0; i < data.Length; i++) if (!IsSection(data[i])) _rowOf.Add(i);
            int focus = Array.FindIndex(data, r => r.Highlight && !IsSection(r));
            if (focus >= 0) _selectedRow = focus;
            else if (_selectedRow < 0 || _selectedRow >= data.Length || IsSection(data[_selectedRow])) _selectedRow = FirstActionable();
            Redraw();
        }

        /// <summary>Ligne de section : titre seul, grisée, sans pastille, sans bouton.</summary>
        static bool IsSection(InfoRowData r) => r.Dim && string.IsNullOrEmpty(r.Tag) && !r.A.Visible && !r.B.Visible && !r.C.Visible;

        /// <summary>Première tuile qui propose une action ; à défaut la première tuile.</summary>
        int FirstActionable()
        {
            foreach (int row in _rowOf) if (_rows[row].A.Visible || _rows[row].B.Visible || _rows[row].C.Visible) return row;
            return _rowOf.Count > 0 ? _rowOf[0] : -1;
        }

        void Select(int tile)
        {
            if (tile < 0 || tile >= _rowOf.Count) return;
            _selectedRow = _rowOf[tile];
            Redraw();
            RowSelected?.Invoke(_selectedRow);
        }

        void Redraw()
        {
            while (_tiles.Count < _rowOf.Count)
            {
                var tile = Instantiate(tilePrefab, grid);
                int index = _tiles.Count;
                tile.Button.onClick.AddListener(() => Select(index));
                _tiles.Add(tile);
            }
            for (int i = 0; i < _tiles.Count; i++)
            {
                bool used = i < _rowOf.Count;
                if (_tiles[i].gameObject.activeSelf != used) _tiles[i].gameObject.SetActive(used);
                if (used) _tiles[i].Show(ToTile(_rows[_rowOf[i]], _rowOf[i] == _selectedRow));
            }
            ShowDetail(_selectedRow >= 0 && _selectedRow < _rows.Length ? _rows[_selectedRow] : (InfoRowData?)null);
        }

        static SheetTile ToTile(InfoRowData r, bool selected) => new SheetTile
        {
            Title = r.Title,
            Sub = r.Badge ?? "",
            Art = r.Art,
            Frame = selected ? Theme.Primary : r.TagColor.a > 0.01f ? r.TagColor : Theme.Outline,
            Kind = r.Dim && !selected ? SheetTileKind.Locked : SheetTileKind.Card,
        };

        void ShowDetail(InfoRowData? row)
        {
            bool has = row.HasValue;
            detailTitle.text = has ? row!.Value.Title : "";
            detailBadge.text = has ? row!.Value.Badge ?? "" : "";
            detailText.text = has ? row!.Value.Subtitle ?? "" : "";
            detailArt.enabled = has && row!.Value.Art != null;
            if (has && row!.Value.Art != null)
            {
                detailArt.sprite = row.Value.Art;
                var fitter = detailArt.GetComponent<AspectRatioFitter>();
                if (fitter != null) fitter.aspectRatio = row.Value.Art.rect.width / row.Value.Art.rect.height;
            }
            Apply(0, has ? row!.Value.A : default);
            Apply(1, has ? row!.Value.B : default);
            Apply(2, has ? row!.Value.C : default);
        }

        void Apply(int i, RowButtonData d)
        {
            if (i < detailButtons.Length) Apply(detailButtons[i], d);
        }

        static void Apply(UiButton b, RowButtonData d)
        {
            if (b.gameObject.activeSelf != d.Visible) b.gameObject.SetActive(d.Visible);
            if (!d.Visible) return;
            b.SetLabel(d.Label);
            var bg = d.Style == 1 ? Theme.Pr : d.Style == 2 ? Theme.Danger : d.Style == 3 ? Theme.Surface3 : Theme.Primary;
            b.SetColors(bg, d.Style == 2 || d.Style == 3 ? Theme.Text : Theme.TextDark);
            b.Interactable = d.Enabled;
            b.DisabledReason = d.Reason;
        }
    }
}
