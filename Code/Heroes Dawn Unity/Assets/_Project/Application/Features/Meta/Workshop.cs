#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;

namespace N29.HeroesDawn.Application.Features.Meta
{
    /// <summary>Une recette connue, avec ce que le joueur en tire aujourd'hui.</summary>
    public readonly struct RecipeView
    {
        public readonly RecipeDef Recipe;
        readonly GameSession _session;

        /// <summary>Chances (%) du tirage −1 / 0 / +1 sur l'étoile de départ.</summary>
        public static readonly IReadOnlyList<int> StarOdds = new[] { 25, 50, 25 };

        public RecipeView(RecipeDef recipe, GameSession session) { Recipe = recipe; _session = session; }

        public int Cost => Recipe.ComponentCost;
        public ComponentGrade MinGrade => Recipe.MinGrade;

        /// <summary>Étoile de départ avec ces Composants (avant le tirage −1 / 0 / +1).</summary>
        public int BaseStars(ComponentGrade grade) => CraftStars.Base(grade);

        /// <summary>Qualité acceptée et assez de Composants de ce type.</summary>
        public bool CanCraft(ComponentGrade grade)
            => grade >= Recipe.MinGrade && CraftingOps.Components(_session, ComponentKinds.For(Recipe.Type), grade) >= Recipe.ComponentCost;
    }

    /// <summary>Résultat d'une fabrication : l'objet, ses étoiles et ses statistiques.</summary>
    public readonly struct CraftOutcome
    {
        public readonly ItemDef Item;
        public readonly int Stars;
        public readonly CraftedStats Stats;
        public CraftOutcome(ItemDef item, int stars, CraftedStats stats) { Item = item; Stars = stars; Stats = stats; }
    }

    /// <summary>Atelier de fabrication : fabrique des objets avec des Composants (une seule qualité) et un catalyseur optionnel, selon les recettes connues.</summary>
    public sealed class Workshop
    {
        readonly MetaContext _c;

        readonly PassProgress _pass;

        public Workshop(MetaContext context, PassProgress pass) { _c = context; _pass = pass; }

        GameSession Session => _c.Session;

        public int Components(ComponentKind kind, ComponentGrade grade) => CraftingOps.Components(Session, kind, grade);
        public int Catalysts(CatalystKind kind) => CraftingOps.Catalysts(Session, kind);

        /// <summary>Recettes connues, dans l'ordre du catalogue.</summary>
        public IReadOnlyList<RecipeView> Recipes()
            => Definitions.Recipes.All.Where(r => CraftingOps.Knows(Session, r.Id)).Select(r => new RecipeView(r, Session)).ToList();

        /// <summary>Fabrique l'objet : rien n'est dépensé si la recette est inconnue, la qualité trop basse, les Composants ou le catalyseur manquent.</summary>
        public Result<CraftOutcome> Craft(string recipeId, ComponentGrade grade, CatalystKind? catalyst)
        {
            var recipe = Definitions.Recipes.Find(recipeId);
            if (recipe == null || !CraftingOps.Knows(Session, recipeId) || ReferenceEquals(Session.Content.Collection, CollectionDef.Empty)) return Result<CraftOutcome>.Fail(MetaErrors.Unknown);
            if (grade < recipe.MinGrade) return Result<CraftOutcome>.Fail(MetaErrors.GradeTooLow);
            var kind = ComponentKinds.For(recipe.Type);
            if (CraftingOps.Components(Session, kind, grade) < recipe.ComponentCost) return Result<CraftOutcome>.Fail(MetaErrors.NotEnoughComponents);
            if (catalyst.HasValue && CraftingOps.Catalysts(Session, catalyst.Value) < 1) return Result<CraftOutcome>.Fail(MetaErrors.NoCatalyst);

            CraftingOps.TrySpendComponents(Session, kind, grade, recipe.ComponentCost);
            if (catalyst.HasValue) CraftingOps.AddCatalyst(Session, catalyst.Value, -1);

            var rng = new Pcg32(CollectionRules.Seed(Session.Account?.PlayerId ?? "local", "craft", Session.Meta.craftedNext));
            int stars = CraftingRules.RollStars(rng, grade);
            var stats = CraftingRules.RollStats(rng, recipe, stars, catalyst);
            var item = CraftingOps.AddCrafted(Session, recipe, stars, stats);

            _pass.AddXp(PassXp.Craft);
            _c.Touch(MetaArea.Lab);
            _c.Touch(MetaArea.Collection);
            return Result<CraftOutcome>.Ok(new CraftOutcome(item, stars, stats));
        }
    }
}
