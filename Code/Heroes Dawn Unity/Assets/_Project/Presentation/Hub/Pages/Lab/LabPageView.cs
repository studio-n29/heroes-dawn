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
    /// Atelier de fabrication (planche 3.3) : colonne de filtres à cases, liste des recettes en vignettes, grand panneau de détail
    /// (illustration, statistiques, Composants requis avec quantités, bouton Fabriquer). Vue dédiée, modèle Screens/Lab/Lab.uxml.
    /// Elle respecte le contrat <see cref="IListPageView"/> (le présentateur n'a pas changé) : l'en-tête, les lignes et la colonne de droite
    /// génériques sont ignorés au profit de <see cref="ILabDetailView.ShowLab"/>.
    /// </summary>
    public sealed class LabPageView : HdView, ILabPageView, ILabDetailView
    {
        sealed class RecipeRow
        {
            public UnityEngine.UIElements.Button Root = null!;
            public VisualElement Thumb = null!;
            public Label Name = null!, Sub = null!;
        }

        sealed class ComponentRow
        {
            public VisualElement Root = null!, Thumb = null!;
            public Label Name = null!, Count = null!, Sub = null!;
        }

        sealed class StatChip
        {
            public VisualElement Root = null!, Icon = null!;
            public Label Text = null!;
            public string IconClass = "";
        }

        readonly List<RecipeRow> _recipeRows = new List<RecipeRow>();
        readonly List<ComponentRow> _componentRows = new List<ComponentRow>();
        readonly List<StatChip> _statChips = new List<StatChip>();
        readonly List<List<UnityEngine.UIElements.Button>> _filterButtons = new List<List<UnityEngine.UIElements.Button>>();
        string _filterShape = "";
        LabData? _lab;

        ScrollView _filterScroll = null!, _recipeScroll = null!, _detailScroll = null!;
        VisualElement _art = null!, _stats = null!, _components = null!;
        Label _filtersTitle = null!, _recipesTitle = null!, _stockTitle = null!, _stockText = null!, _name = null!, _sub = null!,
              _desc = null!, _info = null!, _requiredTitle = null!, _state = null!;
        UnityEngine.UIElements.Button _craft = null!;

        public event Action<int>? HeaderClicked { add { } remove { } }
        public event Action<int, int>? RowButtonClicked { add { } remove { } }
        public event Action<int>? RowSelected;
        public event Action<int, int>? SideButtonClicked;
        public event Action<int>? SideRowSelected { add { } remove { } }
        public event Action<int, int>? FilterClicked;

        /// <summary>Recettes affichées (tests PlayMode).</summary>
        public int MainRowCount => _lab?.Recipes.Length ?? 0;

        /// <summary>Le seul bouton du panneau de droite est Fabriquer : une « ligne » de panneau, un bouton.</summary>
        public int SideRowCount => 1;

        public HdButtonRef? MainRowButton(int row, int button) => button == 0 && row >= 0 && row < _recipeRows.Count ? new HdButtonRef(_recipeRows[row].Root, this) : null;
        public HdButtonRef? SideRowButton(int row, int button) => row == 0 && button == 0 ? new HdButtonRef(_craft, this) : null;

        /// <summary>Tests PlayMode : sélectionne une recette comme le ferait un toucher.</summary>
        public void SelectRowForTest(int row) => RowSelected?.Invoke(row);

        protected override void Bind()
        {
            var page = Q<VisualElement>("page");
            // Prefab généré avant Lab.uxml (modèle ListPage) : on pose le même squelette par code.
            if (page.Q<VisualElement>("labDetail") == null) BuildSkeleton(page);
            _filterScroll = Q<ScrollView>("labFilterScroll");
            _recipeScroll = Q<ScrollView>("labRecipes");
            _detailScroll = Q<ScrollView>("labDetailScroll");
            _filtersTitle = Q<Label>("labFiltersTitle");
            _recipesTitle = Q<Label>("labRecipesTitle");
            _stockTitle = Q<Label>("labStockTitle");
            _stockText = Q<Label>("labStockText");
            _name = Q<Label>("labName");
            _sub = Q<Label>("labSub");
            _desc = Q<Label>("labDesc");
            _info = Q<Label>("labInfo");
            _requiredTitle = Q<Label>("labRequiredTitle");
            _state = Q<Label>("labState");
            _art = Q<VisualElement>("labArt");
            _stats = Q<VisualElement>("labStats");
            _components = Q<VisualElement>("labComponents");
            _craft = Q<UnityEngine.UIElements.Button>("labCraft");
            _craft.clicked += () => SideButtonClicked?.Invoke(0, 0);
            _recipeRows.Clear(); _componentRows.Clear(); _statChips.Clear(); _filterButtons.Clear(); _filterShape = "";
            if (_lab != null) ShowLab(_lab);
        }

        // Contrat de liste : le présentateur envoie aussi l'en-tête, les lignes et la colonne de droite génériques, sans objet ici
        // (le titre est dans la barre de la coque ; tout le contenu passe par ShowLab).
        public void ShowHeader(ListHeaderData data) { }
        public void ShowRows(InfoRowData[] rows) { }
        public void ShowSide(ListSideData data) { }
        public void ShowFilters(FilterGroupData[] groups) { }

        public void ShowLab(LabData data)
        {
            _lab = data;
            if (_craft == null) return;
            _filtersTitle.text = data.FiltersTitle;
            _recipesTitle.text = data.RecipesTitle;
            _stockTitle.text = data.StockTitle;
            _stockText.text = data.StockText;
            ShowFilters(data);
            ShowRecipes(data);
            ShowDetail(data);
        }

        // ---------------------------------------------------------------- Filtres

        void ShowFilters(LabData data)
        {
            string shape = string.Join("|", data.Filters.Select(g => string.Join(",", g.Labels))) + "#" + data.TypeTitle + "," + data.GradeTitle;
            if (shape != _filterShape)
            {
                _filterScroll.Clear(); _filterButtons.Clear();
                for (int g = 0; g < data.Filters.Length; g++)
                {
                    var title = new Label(g == 0 ? data.TypeTitle : data.GradeTitle) { pickingMode = PickingMode.Ignore };
                    title.AddToClassList("hd-lab__group");
                    _filterScroll.Add(title);
                    var buttons = new List<UnityEngine.UIElements.Button>();
                    for (int o = 0; o < data.Filters[g].Labels.Length; o++)
                    {
                        int group = g, option = o;
                        var b = new UnityEngine.UIElements.Button();
                        b.AddToClassList("hd-lab__filter");
                        var check = new VisualElement { pickingMode = PickingMode.Ignore };
                        check.AddToClassList("hd-check"); check.AddToClassList("hd-lab__check");
                        var label = new Label(data.Filters[g].Labels[o]) { pickingMode = PickingMode.Ignore };
                        label.AddToClassList("hd-lab__filterlabel");
                        b.Add(check); b.Add(label);
                        b.clicked += () => FilterClicked?.Invoke(group, option);
                        _filterScroll.Add(b); buttons.Add(b);
                    }
                    _filterButtons.Add(buttons);
                }
                _filterShape = shape;
            }
            for (int g = 0; g < data.Filters.Length && g < _filterButtons.Count; g++)
                for (int o = 0; o < _filterButtons[g].Count; o++)
                {
                    bool on = o == data.Filters[g].Selected;
                    var b = _filterButtons[g][o];
                    b.EnableInClassList("hd-lab__filter--on", on);
                    b[0].EnableInClassList("hd-check--on", on);
                }
        }

        // ---------------------------------------------------------------- Recettes

        void ShowRecipes(LabData data)
        {
            while (_recipeRows.Count < data.Recipes.Length)
            {
                int index = _recipeRows.Count;
                var row = NewRecipeRow(index);
                _recipeRows.Add(row);
                _recipeScroll.Add(row.Root);
            }
            for (int i = 0; i < _recipeRows.Count; i++)
            {
                bool used = i < data.Recipes.Length;
                var row = _recipeRows[i];
                row.Root.style.display = used ? DisplayStyle.Flex : DisplayStyle.None;
                if (!used) continue;
                var r = data.Recipes[i];
                row.Name.text = r.Name;
                row.Sub.text = r.Sub;
                SetArt(row.Thumb, r.Art);
                row.Root.EnableInClassList("hd-lab__row--sel", r.Selected);
            }
        }

        RecipeRow NewRecipeRow(int index)
        {
            var row = new RecipeRow { Root = new UnityEngine.UIElements.Button() };
            row.Root.AddToClassList("hd-lab__row");
            row.Thumb = new VisualElement { pickingMode = PickingMode.Ignore };
            row.Thumb.AddToClassList("hd-lab__thumb");
            var text = new VisualElement { pickingMode = PickingMode.Ignore };
            text.AddToClassList("hd-lab__rowtext");
            row.Name = new Label { pickingMode = PickingMode.Ignore }; row.Name.AddToClassList("hd-lab__rowname");
            row.Sub = new Label { pickingMode = PickingMode.Ignore }; row.Sub.AddToClassList("hd-lab__rowsub");
            text.Add(row.Name); text.Add(row.Sub);
            var chevron = new VisualElement { pickingMode = PickingMode.Ignore };
            chevron.AddToClassList("hd-lab__chevron");
            row.Root.Add(row.Thumb); row.Root.Add(text); row.Root.Add(chevron);
            row.Root.clicked += () => RowSelected?.Invoke(index);
            return row;
        }

        // ---------------------------------------------------------------- Détail

        void ShowDetail(LabData data)
        {
            _name.text = data.Name;
            _sub.text = data.Sub;
            _sub.style.display = string.IsNullOrEmpty(data.Sub) ? DisplayStyle.None : DisplayStyle.Flex;
            _detailScroll.style.display = data.HasSelection ? DisplayStyle.Flex : DisplayStyle.None;
            _craft.style.display = data.HasSelection ? DisplayStyle.Flex : DisplayStyle.None;
            if (!data.HasSelection) return;
            SetArt(_art, data.Art);
            _desc.text = data.Description;
            _info.text = data.Info;
            _requiredTitle.text = data.RequiredTitle;
            _state.text = data.State;
            _state.EnableInClassList("hd-lab__state--ok", data.CanCraft);
            _craft.text = data.CraftLabel;
            _craft.SetEnabled(data.CanCraft);
            _craft.tooltip = data.CanCraft ? "" : data.CraftReason;
            ShowStats(data.Stats);
            ShowComponents(data.Components);
        }

        void ShowStats(LabStatData[] stats)
        {
            while (_statChips.Count < stats.Length)
            {
                var chip = new StatChip { Root = new VisualElement { pickingMode = PickingMode.Ignore } };
                chip.Root.AddToClassList("hd-lab__stat");
                chip.Icon = new VisualElement { pickingMode = PickingMode.Ignore };
                chip.Icon.AddToClassList("hd-icon"); chip.Icon.AddToClassList("hd-icon--md"); chip.Icon.AddToClassList("hd-icon--tint-brass");
                chip.Text = new Label { pickingMode = PickingMode.Ignore }; chip.Text.AddToClassList("hd-lab__stattext");
                chip.Root.Add(chip.Icon); chip.Root.Add(chip.Text);
                _statChips.Add(chip);
                _stats.Add(chip.Root);
            }
            for (int i = 0; i < _statChips.Count; i++)
            {
                bool used = i < stats.Length;
                var chip = _statChips[i];
                chip.Root.style.display = used ? DisplayStyle.Flex : DisplayStyle.None;
                if (!used) continue;
                chip.Text.text = stats[i].Text;
                string cls = "hd-icon--" + stats[i].Icon;
                if (cls != chip.IconClass)
                {
                    if (chip.IconClass.Length > 0) chip.Icon.RemoveFromClassList(chip.IconClass);
                    chip.Icon.AddToClassList(cls);
                    chip.IconClass = cls;
                }
            }
        }

        void ShowComponents(LabComponentData[] components)
        {
            while (_componentRows.Count < components.Length)
            {
                var row = new ComponentRow { Root = new VisualElement { pickingMode = PickingMode.Ignore } };
                row.Root.AddToClassList("hd-lab__comp");
                row.Thumb = new VisualElement { pickingMode = PickingMode.Ignore };
                row.Thumb.AddToClassList("hd-lab__compthumb");
                var text = new VisualElement { pickingMode = PickingMode.Ignore };
                text.AddToClassList("hd-lab__comptext");
                row.Name = new Label { pickingMode = PickingMode.Ignore }; row.Name.AddToClassList("hd-lab__compname");
                row.Sub = new Label { pickingMode = PickingMode.Ignore }; row.Sub.AddToClassList("hd-lab__compsub");
                text.Add(row.Name); text.Add(row.Sub);
                row.Count = new Label { pickingMode = PickingMode.Ignore }; row.Count.AddToClassList("hd-lab__compcount");
                row.Root.Add(row.Thumb); row.Root.Add(text); row.Root.Add(row.Count);
                _componentRows.Add(row);
                _components.Add(row.Root);
            }
            for (int i = 0; i < _componentRows.Count; i++)
            {
                bool used = i < components.Length;
                var row = _componentRows[i];
                row.Root.style.display = used ? DisplayStyle.Flex : DisplayStyle.None;
                if (!used) continue;
                var c = components[i];
                row.Name.text = c.Name;
                row.Sub.text = c.Sub;
                // Quantité possédée sur quantité requise ; manque : rouge atténué, sans cadre d'alerte.
                row.Count.text = c.Have + "/" + c.Need;
                row.Root.EnableInClassList("hd-lab__comp--missing", !c.Enough);
                SetArt(row.Thumb, c.Art);
            }
        }

        static void SetArt(VisualElement target, Sprite? art)
            => target.style.backgroundImage = art != null ? new StyleBackground(art) : new StyleBackground(StyleKeyword.Null);

        // ---------------------------------------------------------------- Squelette (identique à Screens/Lab/Lab.uxml)

        static VisualElement NewBox(string name, params string[] classes)
        {
            var e = new VisualElement();
            if (name.Length > 0) e.name = name;
            foreach (var c in classes) e.AddToClassList(c);
            return e;
        }

        static Label NewLabel(string name, params string[] classes)
        {
            var l = new Label { pickingMode = PickingMode.Ignore };
            if (name.Length > 0) l.name = name;
            foreach (var c in classes) l.AddToClassList(c);
            return l;
        }

        static ScrollView NewScroll(string name, params string[] classes)
        {
            var s = new ScrollView { name = name };
            foreach (var c in classes) s.AddToClassList(c);
            return s;
        }

        /// <summary>Même arbre que Lab.uxml : posé quand le prefab de la page charge encore l'ancien modèle ListPage.</summary>
        static void BuildSkeleton(VisualElement page)
        {
            page.Clear();
            page.RemoveFromClassList("hd-list"); page.RemoveFromClassList("hd-listpage");
            page.AddToClassList("hd-lab");

            var filters = NewBox("labFilters", "hd-panel", "hd-lab__filters");
            var filtersHead = NewBox("", "hd-lab__head");
            filtersHead.Add(NewBox("", "hd-icon", "hd-icon--md", "hd-icon--filter", "hd-icon--tint-brass"));
            filtersHead.Add(NewLabel("labFiltersTitle", "hd-lab__headtitle"));
            filters.Add(filtersHead);
            filters.Add(NewScroll("labFilterScroll", "hd-lab__scroll"));
            page.Add(filters);

            var main = NewBox("", "hd-lab__main");
            var recipes = NewBox("labRecipesPanel", "hd-panel", "hd-lab__recipes");
            var recipesHead = NewBox("", "hd-lab__head");
            recipesHead.Add(NewLabel("labRecipesTitle", "hd-lab__headtitle", "hd-lab__headtitle--first"));
            recipes.Add(recipesHead);
            recipes.Add(NewScroll("labRecipes", "hd-lab__scroll"));
            main.Add(recipes);
            var stock = NewBox("labStock", "hd-panel", "hd-lab__stock");
            stock.Add(NewLabel("labStockTitle", "hd-lab__stocktitle"));
            stock.Add(NewLabel("labStockText", "hd-lab__stocktext"));
            main.Add(stock);
            page.Add(main);

            var detail = NewBox("labDetail", "hd-panel", "hd-lab__detail");
            var dhead = NewBox("", "hd-lab__dhead");
            dhead.Add(NewBox("", "hd-lab__star"));
            var titles = NewBox("", "hd-lab__dtitles");
            titles.Add(NewLabel("labName", "hd-lab__name"));
            titles.Add(NewLabel("labSub", "hd-lab__sub"));
            dhead.Add(titles);
            detail.Add(dhead);
            var scroll = NewScroll("labDetailScroll", "hd-lab__dscroll");
            var body = NewBox("labBody", "hd-lab__body");
            body.Add(NewBox("labArt", "hd-lab__art"));
            var info = NewBox("", "hd-lab__info");
            info.Add(NewBox("labStats", "hd-lab__stats"));
            info.Add(NewLabel("labDesc", "hd-lab__desc"));
            info.Add(NewLabel("labInfo", "hd-lab__infoline"));
            body.Add(info);
            scroll.Add(body);
            scroll.Add(NewLabel("labRequiredTitle", "hd-lab__section"));
            scroll.Add(NewBox("labComponents", "hd-lab__components"));
            scroll.Add(NewLabel("labState", "hd-lab__state"));
            detail.Add(scroll);
            var foot = NewBox("", "hd-lab__foot");
            var craft = new UnityEngine.UIElements.Button { name = "labCraft" };
            craft.AddToClassList("hd-btn"); craft.AddToClassList("hd-btn--primary"); craft.AddToClassList("hd-lab__craft");
            foot.Add(craft);
            detail.Add(foot);
            page.Add(detail);
        }
    }
}
