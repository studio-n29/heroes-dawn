#nullable enable
using System.Collections.Generic;
using System.Linq;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Hub
{
    public interface ILabPageView : IListPageView, IFilterBarView { }

    /// <summary>
    /// Atelier de fabrication, version minimale (l'écran définitif est un lot UI) : recettes connues, fabrication avec la plus basse qualité de Composants
    /// possible et sans catalyseur.
    /// </summary>
    public sealed class LabPagePresenter : ListPagePresenter<ILabPageView>
    {
        readonly Workshop _workshop;
        IReadOnlyList<RecipeView> _recipes = new List<RecipeView>();

        public LabPagePresenter(ILabPageView view, ISessionReadModel session, ILocalization loc, IToastService toasts,
                                ISubscriber<MetaChanged> meta, ISubscriber<WalletChanged> wallet, ISubscriber<RosterChanged> roster, IClock clock,
                                Workshop workshop)
            : base(view, loc, toasts, meta, wallet, roster, clock)
        {
            _workshop = workshop;
        }

        public override string Title => Loc.T("page.lab");

        /// <summary>Filtres des recettes : catégorie (0 = toutes, sinon 1 + le type d'objet) et qualité minimale (0 = toutes, sinon 1 + la qualité).</summary>
        int _category, _grade;

        protected override void OnStart()
        {
            base.OnStart();
            View.FilterClicked += OnFilter;
        }

        protected override void OnDispose()
        {
            View.FilterClicked -= OnFilter;
            base.OnDispose();
        }

        void OnFilter(int group, int option)
        {
            if (group == 0) _category = option; else _grade = option;
            _selected = 0;
            Refresh();
        }

        /// <summary>Recettes connues, filtrées par catégorie et par qualité minimale.</summary>
        IReadOnlyList<RecipeView> FilteredRecipes() => _workshop.Recipes()
            .Where(v => _category == 0 || (int)v.Recipe.Type == _category - 1)
            .Where(v => _grade == 0 || (int)v.MinGrade == _grade - 1)
            .ToList();

        void ShowFilters()
        {
            string all = Loc.T("filters.all");
            View.ShowFilters(new[]
            {
                new FilterGroupData
                {
                    Labels = new[] { all }.Concat(System.Enum.GetValues(typeof(ItemType)).Cast<ItemType>().Select(t => CollectionText.ItemTypeName(Loc, t))).ToArray(),
                    Selected = _category,
                },
                new FilterGroupData
                {
                    Labels = new[] { all }.Concat(System.Enum.GetValues(typeof(ComponentGrade)).Cast<ComponentGrade>().Select(g => Loc.T("grade." + g))).ToArray(),
                    Selected = _grade,
                },
            });
        }

        /// <summary>Qualité utilisée : la plus basse acceptée par la recette dont le joueur a assez de Composants, sinon la qualité minimale.</summary>
        static ComponentGrade GradeFor(RecipeView v)
        {
            for (var g = v.MinGrade; g <= ComponentGrade.Exceptional; g++)
                if (v.CanCraft(g)) return g;
            return v.MinGrade;
        }

        protected override void Render()
        {
            _recipes = FilteredRecipes();
            if (_selected >= _recipes.Count) _selected = 0;
            ShowFilters();
            View.ShowHeader(new ListHeaderData
            {
                // Centre de l'écran : l'objet choisi (les Composants sont dans le panneau de droite).
                Title = _selected >= 0 && _selected < _recipes.Count ? Loc.T(_recipes[_selected].Recipe.NameKey) : Loc.T("page.lab"),
                Line = "",
                Progress = -1f,
            });
            var rows = new List<InfoRowData>();
            AddWorkshopRows(rows);
            View.ShowRows(rows.ToArray());
            ShowSide();
        }

        /// <summary>Panneau de droite : la recette choisie, ses Composants (possédés et manquants), l'état et le bouton Fabriquer.</summary>
        void ShowSide()
        {
            if (_selected < 0 || _selected >= _recipes.Count)
            {
                View.ShowSide(new ListSideData { Title = Loc.T("lab.side.none"), Rows = new InfoRowData[0], Text = "" });
                return;
            }
            var v = _recipes[_selected];
            var grade = GradeFor(v);
            var rows = new List<InfoRowData> { MaterialRow(ComponentKinds.For(v.Recipe.Type), grade, v.Cost) };
            var craft = InfoRowData.Basic(Loc.T("lab.craft"), "");
            craft.A = RowButtonData.Of(Loc.T("lab.craft"), v.CanCraft(grade), Loc.T("errors.not_enough_components"), 0);
            rows.Add(craft);
            View.ShowSide(new ListSideData
            {
                Title = Loc.T(v.Recipe.NameKey),
                Rows = rows.ToArray(),
                Text = Loc.T(v.CanCraft(grade) ? "lab.state.ok" : "lab.state.missing") + "\n" + Loc.T("lab.provenance"),
            });
        }

        /// <summary>Une matière : ce qu'on possède, ou ce qui manque pour fabriquer.</summary>
        InfoRowData MaterialRow(ComponentKind kind, ComponentGrade grade, int need)
        {
            int have = _workshop.Components(kind, grade);
            bool enough = have >= need;
            var r = InfoRowData.Basic(CollectionText.Component(Loc, kind, grade, need), enough ? Loc.F("lab.have", have) : Loc.F("lab.missing", need - have));
            r.Tag = enough ? "OK" : "KO";
            r.TagColor = enough ? Theme.Success : Theme.Danger;
            return r;
        }

        protected override void OnRowSelected(int row)
        {
            if (row < 0 || row >= _recipes.Count) return;
            _selected = row;
            Refresh(); // recalcule les Composants possédés avant d'afficher le panneau
        }

        protected override void OnSideButton(int row, int button)
        {
            if (_selected < 0 || _selected >= _recipes.Count) return;
            Craft(_recipes[_selected]);
        }

        /// <summary>Atelier : une rangée par recette connue, puis un titre qui récapitule les Composants possédés.</summary>
        void AddWorkshopRows(List<InfoRowData> rows)
        {
            int index = 0;
            foreach (var v in _recipes)
            {
                string name = Loc.T(v.Recipe.NameKey);
                var grade = GradeFor(v);
                var r = InfoRowData.Basic(name, RecipeLine(v, grade));
                r.Tag = CollectionText.Tag(name);
                r.TagColor = CollectionText.RarityColor(v.Recipe.Rarity);
                r.A = RowButtonData.Of(Loc.T("lab.craft"), v.CanCraft(grade), Loc.T("errors.not_enough_components"), 0);
                r.Highlight = index++ == _selected;
                rows.Add(r);
            }
            var stock = new List<string>();
            for (int k = 0; k < ComponentKinds.Count; k++)
                for (int g = 0; g < ComponentKinds.GradeCount; g++)
                {
                    int n = _workshop.Components((ComponentKind)k, (ComponentGrade)g);
                    if (n > 0) stock.Add(CollectionText.Component(Loc, (ComponentKind)k, (ComponentGrade)g, n));
                }
            var title = InfoRowData.Basic(Loc.T("lab.workshop"), string.Join(" · ", stock));
            title.Dim = true;
            rows.Add(title);
        }

        string RecipeLine(RecipeView v, ComponentGrade grade)
            => Loc.F("lab.recipe_sub", CollectionText.Component(Loc, ComponentKinds.For(v.Recipe.Type), grade, v.Cost), Loc.T("grade." + v.MinGrade), CollectionText.StarsText(Loc, v.BaseStars(grade)));

        protected override void OnRowButton(int row, int button)
        {
            if (row >= 0 && row < _recipes.Count) Craft(_recipes[row]);
        }

        /// <summary>Recette affichée dans le panneau de droite (index dans les recettes filtrées).</summary>
        int _selected;

        void Craft(RecipeView v)
        {
            var r = _workshop.Craft(v.Recipe.Id, GradeFor(v), null);
            if (r.IsFailure) { Report(r); return; }
            var o = r.Value;
            Toasts.Show(Loc.F("lab.crafted", CollectionText.ItemName(Loc, o.Item), CollectionText.ItemEffects(Loc, o.Item)), ToastKind.Reward);
        }
    }
}
