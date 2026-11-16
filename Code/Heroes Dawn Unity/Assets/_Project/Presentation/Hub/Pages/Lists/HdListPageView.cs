#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Presentation.UiToolkit;
using UnityEngine;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>
    /// Base UI Toolkit des pages « liste » : en-tête (titre, ligne, barre, 2 boutons), barre de filtres, lignes défilantes,
    /// colonne de droite optionnelle (titre, texte, lignes). Même contrat que <see cref="ListPageView"/> : les présentateurs ne changent pas.
    /// Les lignes sont réutilisées d'un affichage à l'autre (le focus et le défilement survivent aux mises à jour par seconde).
    /// </summary>
    public abstract class HdListPageView : HdView, IListPageView, IFilterBarView
    {
        sealed class Row
        {
            public VisualElement Root = null!, Tag = null!, TagIcon = null!, Text = null!, BarFill = null!, Bar = null!;
            public string IconClass = "";
            public Label TagText = null!, Title = null!, Sub = null!, BarLabel = null!, Badge = null!;
            public UnityEngine.UIElements.Button[] Buttons = new UnityEngine.UIElements.Button[3];
        }

        readonly List<Row> _main = new List<Row>(), _sideList = new List<Row>();
        readonly List<List<UnityEngine.UIElements.Button>> _filterButtons = new List<List<UnityEngine.UIElements.Button>>();
        string _filterShape = "";

        UnityEngine.UIElements.Button _headerA = null!, _headerB = null!;
        Label _title = null!, _line = null!, _barLabel = null!, _sideTitle = null!, _sideText = null!;
        VisualElement _bar = null!, _barFill = null!, _side = null!, _filters = null!;
        ScrollView _rows = null!, _sideRows = null!;

        ListHeaderData? _lastHeader;
        InfoRowData[]? _lastRows;
        ListSideData? _lastSide;
        FilterGroupData[]? _lastFilters;

        public event Action<int>? HeaderClicked;
        public event Action<int, int>? RowButtonClicked;
        public event Action<int>? RowSelected;
        public event Action<int, int>? SideButtonClicked;
        public event Action<int>? SideRowSelected;
        public event Action<int, int>? FilterClicked;

        /// <summary>La colonne de droite est large (page de texte : Archives).</summary>
        protected virtual bool WideSide => false;
        protected virtual bool HasSide => true;

        protected void RaiseRowSelected(int row) => RowSelected?.Invoke(row);

        public HdButtonRef HeaderButtonA { get; private set; } = null!;
        public HdButtonRef HeaderButtonB { get; private set; } = null!;

        public int MainRowCount => _main.Count(r => r.Root.style.display != DisplayStyle.None);
        public int SideRowCount => _sideList.Count(r => r.Root.style.display != DisplayStyle.None);

        /// <summary>Bouton d'une ligne principale (tests PlayMode : clic réel).</summary>
        public HdButtonRef? MainRowButton(int row, int button) => row < _main.Count ? new HdButtonRef(_main[row].Buttons[button], this) : null;
        public HdButtonRef? SideRowButton(int row, int button) => row < _sideList.Count ? new HdButtonRef(_sideList[row].Buttons[button], this) : null;

        protected override void Bind()
        {
            _title = Q<Label>("headerTitle"); _line = Q<Label>("headerLine"); _barLabel = Q<Label>("headerBarLabel");
            _bar = Q<VisualElement>("headerBar"); _barFill = Q<VisualElement>("headerFill");
            _headerA = Q<UnityEngine.UIElements.Button>("headerA"); _headerB = Q<UnityEngine.UIElements.Button>("headerB");
            _side = Q<VisualElement>("side"); _sideTitle = Q<Label>("sideTitle"); _sideText = Q<Label>("sideText");
            _filters = Q<VisualElement>("filters");
            _rows = Q<ScrollView>("rows"); _sideRows = Q<ScrollView>("sideRows");
            HeaderButtonA = new HdButtonRef(_headerA, this); HeaderButtonB = new HdButtonRef(_headerB, this);
            _headerA.clicked += () => HeaderClicked?.Invoke(0);
            _headerB.clicked += () => HeaderClicked?.Invoke(1);
            _side.EnableInClassList("hd-list__side--wide", WideSide);
            _side.style.display = HasSide ? DisplayStyle.Flex : DisplayStyle.None;
            _main.Clear(); _sideList.Clear(); _filterButtons.Clear(); _filterShape = "";
            if (_lastHeader.HasValue) ShowHeader(_lastHeader.Value);
            if (_lastFilters != null) ShowFilters(_lastFilters);
            if (_lastRows != null) ShowRows(_lastRows);
            if (_lastSide.HasValue) ShowSide(_lastSide.Value);
        }

        public void ShowHeader(ListHeaderData d)
        {
            _lastHeader = d;
            if (_title == null) return;
            _title.text = d.Title;
            _line.text = d.Line ?? "";
            _line.style.display = string.IsNullOrEmpty(d.Line) ? DisplayStyle.None : DisplayStyle.Flex;
            Q<VisualElement>("header").style.backgroundColor = d.HeaderTint.HasValue ? new Color(d.HeaderTint.Value.r, d.HeaderTint.Value.g, d.HeaderTint.Value.b, 0.9f) : new StyleColor(StyleKeyword.Null);
            bool bar = d.Progress >= 0f;
            _bar.style.display = bar ? DisplayStyle.Flex : DisplayStyle.None;
            if (bar) { _barFill.style.width = Length.Percent(Mathf.Clamp01(d.Progress) * 100f); _barLabel.text = d.ProgressLabel; }
            ApplyButton(_headerA, d.A);
            ApplyButton(_headerB, d.B, true);
        }

        static void ApplyButton(UnityEngine.UIElements.Button b, RowButtonData d, bool forceSecondary = false)
        {
            b.style.display = d.Visible ? DisplayStyle.Flex : DisplayStyle.None;
            if (!d.Visible) return;
            // Chevrons écrits en texte (« < », « > ») : remplacés par les icônes du kit, le bouton garde sa taille.
            bool back = d.Label == "<", forward = d.Label == ">";
            b.text = back || forward ? "" : d.Label;
            b.EnableInClassList("hd-btn--icon-back", back);
            b.EnableInClassList("hd-btn--icon-forward", forward);
            var glyph = b.Q<VisualElement>(className: "hd-btn__glyph");
            if (glyph == null && (back || forward))
            {
                glyph = new VisualElement { pickingMode = PickingMode.Ignore };
                glyph.AddToClassList("hd-btn__glyph");
                b.Add(glyph);
            }
            if (glyph != null) glyph.style.display = back || forward ? DisplayStyle.Flex : DisplayStyle.None;
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
                        b.AddToClassList("hd-btn"); b.AddToClassList("hd-btn--small"); b.AddToClassList("hd-list__filter");
                        b.clicked += () => FilterClicked?.Invoke(group, option);
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

        public void ShowRows(InfoRowData[] data)
        {
            _lastRows = data;
            if (_rows == null) return;
            Fill(_main, _rows, data, (r, b) => RowButtonClicked?.Invoke(r, b), r => RowSelected?.Invoke(r));
        }

        public void ShowSide(ListSideData d)
        {
            _lastSide = d;
            if (_side == null) return;
            _sideTitle.text = d.Title;
            _sideText.text = d.Text ?? "";
            _sideText.style.display = string.IsNullOrEmpty(d.Text) ? DisplayStyle.None : DisplayStyle.Flex;
            Fill(_sideList, _sideRows, d.Rows ?? Array.Empty<InfoRowData>(), (r, b) => SideButtonClicked?.Invoke(r, b), r => SideRowSelected?.Invoke(r));
        }

        static void Fill(List<Row> pool, ScrollView parent, InfoRowData[] data, Action<int, int> onButton, Action<int> onSelect)
        {
            while (pool.Count < data.Length)
            {
                var row = NewRow(pool.Count, onButton, onSelect);
                pool.Add(row);
                parent.Add(row.Root);
            }
            for (int i = 0; i < pool.Count; i++)
            {
                bool used = i < data.Length;
                pool[i].Root.style.display = used ? DisplayStyle.Flex : DisplayStyle.None;
                if (used) Show(pool[i], data[i]);
            }
        }

        static Row NewRow(int index, Action<int, int> onButton, Action<int> onSelect)
        {
            var r = new Row();
            r.Root = new VisualElement { focusable = true };
            r.Root.AddToClassList("hd-row");
            r.Root.RegisterCallback<ClickEvent>(e => { if (!(e.target is UnityEngine.UIElements.Button)) onSelect(index); });
            r.Root.RegisterCallback<NavigationSubmitEvent>(e => { if (e.target == r.Root) onSelect(index); });
            r.Tag = new VisualElement { pickingMode = PickingMode.Ignore };
            r.Tag.AddToClassList("hd-row__tag");
            r.TagText = new Label { pickingMode = PickingMode.Ignore };
            r.Tag.Add(r.TagText);
            r.TagIcon = new VisualElement { pickingMode = PickingMode.Ignore };
            r.TagIcon.AddToClassList("hd-row__tagicon");
            r.Tag.Add(r.TagIcon);
            r.Root.Add(r.Tag);
            r.Text = new VisualElement { pickingMode = PickingMode.Ignore };
            r.Text.AddToClassList("hd-row__text");
            r.Title = new Label { pickingMode = PickingMode.Ignore }; r.Title.AddToClassList("hd-row__title");
            r.Sub = new Label { pickingMode = PickingMode.Ignore }; r.Sub.AddToClassList("hd-row__sub");
            r.Bar = new VisualElement { pickingMode = PickingMode.Ignore }; r.Bar.AddToClassList("hd-bar"); r.Bar.AddToClassList("hd-row__bar");
            r.BarFill = new VisualElement { pickingMode = PickingMode.Ignore }; r.BarFill.AddToClassList("hd-bar__fill");
            r.BarLabel = new Label { pickingMode = PickingMode.Ignore }; r.BarLabel.AddToClassList("hd-list__barlabel");
            r.Bar.Add(r.BarFill); r.Bar.Add(r.BarLabel);
            r.Text.Add(r.Title); r.Text.Add(r.Sub); r.Text.Add(r.Bar);
            r.Root.Add(r.Text);
            var buttons = new VisualElement();
            buttons.AddToClassList("hd-row__buttons");
            for (int i = 0; i < 3; i++)
            {
                int b = i;
                var button = new UnityEngine.UIElements.Button();
                button.AddToClassList("hd-btn"); button.AddToClassList("hd-btn--small"); button.AddToClassList("hd-row__btn");
                button.clicked += () => onButton(index, b);
                r.Buttons[i] = button;
                buttons.Add(button);
            }
            r.Root.Add(buttons);
            r.Badge = new Label { pickingMode = PickingMode.Ignore }; r.Badge.AddToClassList("hd-row__badge");
            r.Root.Add(r.Badge);
            return r;
        }

        /// <summary>Classe USS de l'icône du kit qui remplace un sigle de pastille ; vide : la pastille garde son texte (rang, niveau, initiale).</summary>
        static string TagIconClass(string tag)
        {
            switch (tag)
            {
                case "OK": return "hd-row__tagicon--check";
                case "KO": return "hd-row__tagicon--close";
                case "--": return "hd-row__tagicon--lock";
                case "+": return "hd-row__tagicon--plus";
                case "!": return "hd-row__tagicon--info";
                case "GO": return "hd-row__tagicon--play";
                default: return "";
            }
        }

        static void Show(Row r, InfoRowData d)
        {
            bool hasArt = d.Art != null;
            bool hasTag = hasArt || !string.IsNullOrEmpty(d.Tag);
            r.Tag.style.display = hasTag ? DisplayStyle.Flex : DisplayStyle.None;
            // Vignette : l'illustration quand la ligne en fournit une ; sinon la pastille, avec une icône du kit à la place des sigles (OK, KO, +, !…).
            string iconClass = hasArt || !hasTag ? "" : TagIconClass(d.Tag);
            if (iconClass != r.IconClass)
            {
                if (r.IconClass.Length > 0) r.TagIcon.RemoveFromClassList(r.IconClass);
                if (iconClass.Length > 0) r.TagIcon.AddToClassList(iconClass);
                r.IconClass = iconClass;
            }
            r.TagIcon.style.display = iconClass.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            r.TagText.style.display = hasArt || iconClass.Length > 0 ? DisplayStyle.None : DisplayStyle.Flex;
            r.Tag.EnableInClassList("hd-row__tag--art", hasArt);
            // Pastille sombre (verrouillé, occupé) : texte et icône clairs, sinon ils disparaissent dans le fond.
            r.Tag.EnableInClassList("hd-row__tag--dark", !hasArt && hasTag && d.TagColor.r * 0.299f + d.TagColor.g * 0.587f + d.TagColor.b * 0.114f < 0.4f);
            if (hasArt)
            {
                r.Tag.style.backgroundImage = new StyleBackground(d.Art);
                r.Tag.style.backgroundColor = new StyleColor(StyleKeyword.Null);
            }
            else if (hasTag)
            {
                r.Tag.style.backgroundImage = new StyleBackground(StyleKeyword.Null);
                r.Tag.style.backgroundColor = d.TagColor;
                r.TagText.text = d.Tag;
            }
            r.Title.text = d.Title;
            r.Sub.text = d.Subtitle;
            r.Sub.style.display = string.IsNullOrEmpty(d.Subtitle) ? DisplayStyle.None : DisplayStyle.Flex;
            bool hasBar = d.Progress >= 0f;
            r.Bar.style.display = hasBar ? DisplayStyle.Flex : DisplayStyle.None;
            if (hasBar) { r.BarFill.style.width = Length.Percent(Mathf.Clamp01(d.Progress) * 100f); r.BarLabel.text = d.ProgressLabel; }
            r.Badge.text = d.Badge ?? "";
            r.Badge.style.display = string.IsNullOrEmpty(d.Badge) ? DisplayStyle.None : DisplayStyle.Flex;
            r.Root.EnableInClassList("hd-row--hl", d.Highlight);
            r.Root.EnableInClassList("hd-row--dim", d.Dim);
            ApplyButton(r.Buttons[0], d.A);
            ApplyButton(r.Buttons[1], d.B);
            ApplyButton(r.Buttons[2], d.C);
        }
    }
}
