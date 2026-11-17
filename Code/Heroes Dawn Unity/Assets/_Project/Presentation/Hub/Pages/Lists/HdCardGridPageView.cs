#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.UiToolkit;
using UnityEngine;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Vue de liste qui peut afficher un insigne de rang AMIF par ligne (la page l'interroge par <c>as</c> : le contrat <see cref="IListPageView"/> reste inchangé).</summary>
    public interface IRowRanks
    {
        void SetRowRanks(int[]? ranks);
    }

    /// <summary>
    /// Base UI Toolkit des pages « grille de cartes » (Grimoire, Équipement, Registre, Boutique) : en-tête, onglets et recherche optionnels,
    /// filtres, grille de tuiles défilante, fiche détaillée de la tuile choisie (illustration, texte, jusqu'à trois boutons).
    /// Même contrat que <see cref="CardGridPageView"/> : une tuile par ligne (les lignes de section ne sont pas dessinées),
    /// les boutons de la fiche sont ceux de la ligne choisie.
    /// </summary>
    public abstract class HdCardGridPageView : HdView, IListPageView, IFilterBarView, IRowRanks
    {
        sealed class Tile
        {
            public VisualElement Root = null!, Art = null!, Rank = null!;
            public Label Title = null!, Sub = null!;
        }

        readonly List<Tile> _tiles = new List<Tile>();
        readonly List<int> _rowOf = new List<int>();
        readonly List<UnityEngine.UIElements.Button> _detailButtons = new List<UnityEngine.UIElements.Button>();
        readonly List<List<UnityEngine.UIElements.Button>> _filterButtons = new List<List<UnityEngine.UIElements.Button>>();
        string _filterShape = "";
        int[]? _rowRanks;

        UnityEngine.UIElements.Button _headerA = null!, _headerB = null!;
        Label _title = null!, _line = null!, _barLabel = null!, _detailTitle = null!, _detailBadge = null!, _detailText = null!;
        VisualElement _bar = null!, _barFill = null!, _filters = null!, _detailArt = null!, _detail = null!;
        ScrollView _grid = null!;

        ListHeaderData? _lastHeader;
        InfoRowData[] _rows = Array.Empty<InfoRowData>();
        FilterGroupData[]? _lastFilters;
        int _selectedRow = -1;

        public event Action<int>? HeaderClicked;
        public event Action<int, int>? RowButtonClicked;
        public event Action<int>? RowSelected;
        public event Action<int, int>? SideButtonClicked { add { } remove { } }
        public event Action<int>? SideRowSelected { add { } remove { } }
        public event Action<int, int>? FilterClicked;

        /// <summary>Les portraits d'agents sont cadrés sur le visage (Registre).</summary>
        protected virtual bool PortraitFraming => false;
        protected virtual bool HasSearch => false;
        protected virtual bool HasShopTabs => false;

        protected TextField? Search { get; private set; }
        protected VisualElement? TabsHost { get; private set; }

        public HdButtonRef HeaderButtonA { get; private set; } = null!;
        public HdButtonRef HeaderButtonB { get; private set; } = null!;
        public int SelectedRow => _selectedRow;
        public int VisibleTileCount => _rowOf.Count;

        /// <summary>Bouton de la fiche détaillée (tests PlayMode : clic réel).</summary>
        public HdButtonRef DetailButton(int i) => new HdButtonRef(_detailButtons[i], this);

        /// <summary>Premier bouton visible de la fiche portant ce libellé (tests PlayMode).</summary>
        public HdButtonRef? DetailButtonLabelled(string label)
        {
            foreach (var b in _detailButtons) if (b.style.display != DisplayStyle.None && b.text == label) return new HdButtonRef(b, this);
            return null;
        }

        /// <summary>Touche la tuile n comme le ferait un joueur (tests PlayMode).</summary>
        public void TapTile(int tile) => Select(tile);

        protected override void Bind()
        {
            _title = Q<Label>("headerTitle"); _line = Q<Label>("headerLine"); _barLabel = Q<Label>("headerBarLabel");
            _bar = Q<VisualElement>("headerBar"); _barFill = Q<VisualElement>("headerFill");
            _headerA = Q<UnityEngine.UIElements.Button>("headerA"); _headerB = Q<UnityEngine.UIElements.Button>("headerB");
            _filters = Q<VisualElement>("filters"); _grid = Q<ScrollView>("grid");
            _detail = Q<VisualElement>("detail"); _detailArt = Q<VisualElement>("detailArt");
            _detailTitle = Q<Label>("detailTitle"); _detailBadge = Q<Label>("detailBadge"); _detailText = Q<Label>("detailText");
            Search = TryQ<TextField>("search");
            TabsHost = TryQ<VisualElement>("shopTabs");
            if (Search != null) Search.style.display = HasSearch ? DisplayStyle.Flex : DisplayStyle.None;
            if (TabsHost != null) TabsHost.style.display = HasShopTabs ? DisplayStyle.Flex : DisplayStyle.None;
            _grid.contentContainer.style.flexDirection = FlexDirection.Row;
            _grid.contentContainer.style.flexWrap = Wrap.Wrap;
            // Classes de variante pour la feuille de style : registre (portraits), collections (grimoire, inventaire, équipement).
            var page = TryQ<VisualElement>("page");
            if (page != null)
            {
                page.EnableInClassList("hd-grid--portrait", PortraitFraming);
                page.EnableInClassList("hd-grid--collection", !HasShopTabs && !PortraitFraming);
            }
            HeaderButtonA = new HdButtonRef(_headerA, this); HeaderButtonB = new HdButtonRef(_headerB, this);
            _headerA.clicked += () => HeaderClicked?.Invoke(0);
            _headerB.clicked += () => HeaderClicked?.Invoke(1);
            _detailButtons.Clear();
            for (int i = 0; i < 3; i++)
            {
                int k = i;
                var b = Q<UnityEngine.UIElements.Button>("detailButton" + i);
                b.clicked += () => { if (_selectedRow >= 0) RowButtonClicked?.Invoke(_selectedRow, k); };
                _detailButtons.Add(b);
            }
            _tiles.Clear(); _filterButtons.Clear(); _filterShape = "";
            if (_lastHeader.HasValue) ShowHeader(_lastHeader.Value);
            if (_lastFilters != null) ShowFilters(_lastFilters);
            if (_rows.Length > 0) Redraw();
        }

        public void ShowHeader(ListHeaderData d)
        {
            _lastHeader = d;
            if (_title == null) return;
            _title.text = d.Title;
            _line.text = d.Line;
            bool bar = d.Progress >= 0f;
            _bar.style.display = bar ? DisplayStyle.Flex : DisplayStyle.None;
            if (bar) { _barFill.style.width = Length.Percent(Mathf.Clamp01(d.Progress) * 100f); _barLabel.text = d.ProgressLabel; }
            Apply(_headerA, d.A);
            Apply(_headerB, d.B, true);
        }

        static void Apply(UnityEngine.UIElements.Button b, RowButtonData d, bool forceSecondary = false)
        {
            b.style.display = d.Visible ? DisplayStyle.Flex : DisplayStyle.None;
            if (!d.Visible) return;
            b.text = d.Label;
            b.EnableInClassList("hd-btn--primary", d.Style == 0 && !forceSecondary);
            b.EnableInClassList("hd-btn--pr", d.Style == 1);
            b.EnableInClassList("hd-btn--danger", d.Style == 2);
            b.EnableInClassList("hd-btn--secondary", d.Style == 1 || d.Style == 3 || (forceSecondary && d.Style == 0));
            b.SetEnabled(d.Enabled);
            b.tooltip = d.Reason;
        }

        public void ShowFilters(FilterGroupData[] groups)
        {
            _lastFilters = groups;
            if (_filters == null) return;
            _filters.style.display = groups.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            string shape = string.Join("|", groups.Select(g => string.Join(",", g.Labels)));
            if (shape != _filterShape)
            {
                _filters.Clear(); _filterButtons.Clear();
                for (int g = 0; g < groups.Length; g++)
                {
                    var row = new VisualElement();
                    row.AddToClassList("hd-list__filterrow");
                    var buttons = new List<UnityEngine.UIElements.Button>();
                    for (int o = 0; o < groups[g].Labels.Length; o++)
                    {
                        int group = g, option = o;
                        var b = new UnityEngine.UIElements.Button { text = groups[g].Labels[o] };
                        b.AddToClassList("hd-btn"); b.AddToClassList("hd-btn--small"); b.AddToClassList("hd-btn--plain"); b.AddToClassList("hd-list__filter");
                        b.clicked += () => FilterClicked?.Invoke(group, option);
                        AddFamilyIcon(b);
                        row.Add(b); buttons.Add(b);
                    }
                    _filters.Add(row); _filterButtons.Add(buttons);
                }
                _filterShape = shape;
            }
            for (int g = 0; g < groups.Length && g < _filterButtons.Count; g++)
                for (int o = 0; o < _filterButtons[g].Count; o++)
                {
                    bool on = o == groups[g].Selected;
                    _filterButtons[g][o].EnableInClassList("hd-btn--primary", on);
                    _filterButtons[g][o].EnableInClassList("hd-btn--secondary", !on);
                }
        }

        /// <summary>Un filtre qui porte le nom d'une famille reçoit l'icône de cette famille devant son libellé.</summary>
        static void AddFamilyIcon(UnityEngine.UIElements.Button filter)
        {
            foreach (Family f in Enum.GetValues(typeof(Family)))
            {
                if (!string.Equals(filter.text, Loc.FamilyName(f), StringComparison.OrdinalIgnoreCase)) continue;
                filter.AddToClassList("hd-tm-ibtn");
                var icon = HdTeamUi.FamilyIcon(f);
                icon.AddToClassList("hd-tm-ibtn__ico");
                filter.Add(icon);
                return;
            }
        }

        /// <summary>Rang AMIF de chaque ligne (indexé comme les lignes de <see cref="ShowRows"/>), à poser avant ShowRows ; null ou 0 : pas d'insigne.</summary>
        public void SetRowRanks(int[]? ranks) => _rowRanks = ranks;

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

        static bool IsSection(InfoRowData r) => r.Dim && string.IsNullOrEmpty(r.Tag) && !r.A.Visible && !r.B.Visible && !r.C.Visible;

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
            if (_grid == null) return;
            while (_tiles.Count < _rowOf.Count)
            {
                var tile = NewTile(_tiles.Count);
                _tiles.Add(tile);
                _grid.Add(tile.Root);
            }
            for (int i = 0; i < _tiles.Count; i++)
            {
                bool used = i < _rowOf.Count;
                _tiles[i].Root.style.display = used ? DisplayStyle.Flex : DisplayStyle.None;
                if (used) ShowTile(_tiles[i], _rows[_rowOf[i]], _rowOf[i] == _selectedRow, _rowOf[i]);
            }
            // Grille vide (aucun objet, aucun résultat de filtre) : la fiche reprend le message de la liste au lieu de rester vide.
            if (_rowOf.Count == 0 && _rows.Length > 0) ShowDetail(_rows[0]);
            else ShowDetail(_selectedRow >= 0 && _selectedRow < _rows.Length ? _rows[_selectedRow] : (InfoRowData?)null);
        }

        Tile NewTile(int index)
        {
            var t = new Tile { Root = new VisualElement { focusable = true } };
            t.Root.AddToClassList("hd-tile");
            t.Root.RegisterCallback<ClickEvent>(_ => Select(index));
            t.Root.RegisterCallback<NavigationSubmitEvent>(_ => Select(index));
            t.Art = new VisualElement { pickingMode = PickingMode.Ignore };
            t.Art.AddToClassList("hd-tile__art");
            t.Art.EnableInClassList("hd-tile__art--portrait", PortraitFraming);
            t.Title = new Label { pickingMode = PickingMode.Ignore }; t.Title.AddToClassList("hd-tile__title");
            t.Sub = new Label { pickingMode = PickingMode.Ignore }; t.Sub.AddToClassList("hd-tile__sub");
            t.Rank = HdTeamUi.RankBadge(1); t.Rank.AddToClassList("hd-tile__rank"); t.Rank.style.display = DisplayStyle.None;
            t.Art.Add(t.Rank);
            t.Root.Add(t.Art); t.Root.Add(t.Title); t.Root.Add(t.Sub);
            return t;
        }

        void ShowTile(Tile t, InfoRowData r, bool selected, int row)
        {
            t.Title.text = r.Title;
            t.Sub.text = r.Badge ?? "";
            t.Sub.style.display = string.IsNullOrEmpty(r.Badge) ? DisplayStyle.None : DisplayStyle.Flex;
            // Registre : portrait cadré sur le visage ; ailleurs l'illustration de la carte ou de l'objet (sans illustration : case « plus »).
            if (PortraitFraming) HdTeamUi.Head(t.Art, r.Art);
            else t.Art.style.backgroundImage = r.Art != null ? new StyleBackground(r.Art) : new StyleBackground(StyleKeyword.Null);
            t.Art.EnableInClassList("hd-tile__art--none", r.Art == null);
            // Insigne de rang AMIF (Registre) : posé par la page par ligne, masqué sans rang.
            int rank = _rowRanks != null && row >= 0 && row < _rowRanks.Length ? _rowRanks[row] : 0;
            t.Rank.style.display = rank > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            if (rank > 0) HdTeamUi.SetRank(t.Rank, rank);
            Color frame = selected ? Theme.Primary : r.TagColor.a > 0.01f ? r.TagColor : Theme.Outline;
            t.Root.style.borderTopColor = t.Root.style.borderBottomColor = t.Root.style.borderLeftColor = t.Root.style.borderRightColor = frame;
            t.Root.EnableInClassList("hd-tile--selected", selected);
            t.Root.EnableInClassList("hd-tile--locked", r.Dim && !selected);
        }

        void ShowDetail(InfoRowData? row)
        {
            bool has = row.HasValue;
            _detailTitle.text = has ? row!.Value.Title : "";
            _detailBadge.text = has ? row!.Value.Badge ?? "" : "";
            _detailText.text = has ? row!.Value.Subtitle ?? "" : "";
            bool art = has && row!.Value.Art != null;
            _detailArt.style.display = art ? DisplayStyle.Flex : DisplayStyle.None;
            if (PortraitFraming) HdTeamUi.Head(_detailArt, art ? row!.Value.Art : null);
            else _detailArt.style.backgroundImage = art ? new StyleBackground(row!.Value.Art) : new StyleBackground(StyleKeyword.Null);
            _detailArt.EnableInClassList("hd-grid__detailart--portrait", PortraitFraming);
            Apply(_detailButtons[0], has ? row!.Value.A : default);
            Apply(_detailButtons[1], has ? row!.Value.B : default);
            Apply(_detailButtons[2], has ? row!.Value.C : default);
        }
    }
}
