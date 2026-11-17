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

        FilterGroupData[] FilterGroups()
        {
            string all = Loc.T("filters.all");
            return new[]
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
            };
        }

        void ShowFilters() => View.ShowFilters(FilterGroups());

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
            // Écran détaillé (colonnes filtres, recettes, détail) : seulement si la vue le propose ; le contrat de liste ci-dessus reste complet.
            if (View is ILabDetailView detail) detail.ShowLab(BuildLab());
        }

        /// <summary>Données de l'écran de l'Atelier : recettes filtrées, détail de la recette choisie (statistiques, Composants requis avec quantités), stock.</summary>
        LabData BuildLab()
        {
            string stock = StockLine();
            var data = new LabData
            {
                FiltersTitle = Loc.T("lab.filters"),
                TypeTitle = Loc.T("lab.group.type"),
                GradeTitle = Loc.T("lab.group.grade"),
                RecipesTitle = Loc.F("lab.recipes", _recipes.Count),
                RequiredTitle = Loc.T("lab.required"),
                CraftLabel = Loc.T("lab.craft"),
                StockTitle = Loc.T("lab.workshop"),
                StockText = stock.Length > 0 ? stock : "—",
                Filters = FilterGroups(),
                Recipes = _recipes.Select((v, i) => new LabRecipeData
                {
                    Name = Loc.T(v.Recipe.NameKey),
                    Sub = RecipeKind(v.Recipe),
                    Art = LabArt.Recipe(v.Recipe.Id),
                    Selected = i == _selected,
                }).ToArray(),
            };
            if (_selected < 0 || _selected >= _recipes.Count)
            {
                data.Name = Loc.T("lab.side.none");
                return data;
            }
            var recipe = _recipes[_selected];
            var grade = GradeFor(recipe);
            var kind = ComponentKinds.For(recipe.Recipe.Type);
            int have = _workshop.Components(kind, grade);
            bool can = recipe.CanCraft(grade);
            data.HasSelection = true;
            data.Name = Loc.T(recipe.Recipe.NameKey);
            data.Sub = RecipeKind(recipe.Recipe);
            data.Art = LabArt.Recipe(recipe.Recipe.Id);
            data.Stats = StatsOf(recipe.Recipe);
            data.Description = Loc.T("lab.provenance");
            data.Info = RecipeLine(recipe, grade);
            data.State = Loc.T(can ? "lab.state.ok" : "lab.state.missing");
            data.CraftReason = Loc.T("errors.not_enough_components");
            data.CanCraft = can;
            data.Components = new[]
            {
                new LabComponentData
                {
                    Art = LabArt.Component(kind),
                    Name = Loc.T("component." + kind + "." + grade),
                    Have = have,
                    Need = recipe.Cost,
                    Sub = have >= recipe.Cost ? Loc.F("lab.have", have) : Loc.F("lab.missing", recipe.Cost - have),
                },
            };
            return data;
        }

        /// <summary>« Arme · Commun » : type d'objet et rareté de la recette.</summary>
        string RecipeKind(RecipeDef r) => CollectionText.ItemTypeName(Loc, r.Type) + " · " + CollectionText.RarityName(Loc, r.Rarity);

        /// <summary>Statistiques tirées par la recette (fourchettes en %), puis son bonus particulier.</summary>
        LabStatData[] StatsOf(RecipeDef r)
        {
            var list = new List<LabStatData>();
            foreach (var s in r.Stats)
            {
                string range = s.Min == s.Max ? s.Min.ToString() : s.Min + "–" + s.Max;
                string icon, key;
                switch (s.Stat)
                {
                    case RecipeStatKind.Hp: icon = "health"; key = "item.fx.hp"; break;
                    case RecipeStatKind.Atk: icon = "attack"; key = "item.fx.atk"; break;
                    case RecipeStatKind.Def: icon = "defense"; key = "item.fx.def"; break;
                    default: icon = "speed"; key = "item.fx.spd"; break;
                }
                list.Add(new LabStatData { Icon = icon, Text = Loc.F(key, range) });
            }
            string? perkKey = null;
            switch (r.Perk)
            {
                case ItemPerk.StartGauge: perkKey = "item.fx.start_gauge"; break;
                case ItemPerk.OpeningShield: perkKey = "item.fx.opening_shield"; break;
                case ItemPerk.CritChance: perkKey = "item.fx.crit"; break;
                case ItemPerk.SkillPower: perkKey = "item.fx.skill_power"; break;
            }
            if (perkKey != null) list.Add(new LabStatData { Icon = "star", Text = Loc.F(perkKey, r.PerkValue) });
            return list.ToArray();
        }

        /// <summary>Composants possédés, séparés par « · » (un type et une qualité par segment) ; vide sans stock.</summary>
        string StockLine()
        {
            var stock = new List<string>();
            for (int k = 0; k < ComponentKinds.Count; k++)
                for (int g = 0; g < ComponentKinds.GradeCount; g++)
                {
                    int n = _workshop.Components((ComponentKind)k, (ComponentGrade)g);
                    if (n > 0) stock.Add(CollectionText.Component(Loc, (ComponentKind)k, (ComponentGrade)g, n));
                }
            return string.Join(" · ", stock);
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
            var title = InfoRowData.Basic(Loc.T("lab.workshop"), StockLine());
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
