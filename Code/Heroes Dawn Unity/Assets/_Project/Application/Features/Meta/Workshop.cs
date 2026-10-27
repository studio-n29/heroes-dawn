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
        public readonly bool CanAfford;
        public readonly int Skill;
        /// <summary>Probabilités (%) des cinq qualités au niveau actuel, dans l'ordre de <see cref="CraftQuality"/>.</summary>
        public readonly IReadOnlyList<int> Odds;
        public RecipeView(RecipeDef recipe, bool canAfford, int skill, IReadOnlyList<int> odds) { Recipe = recipe; CanAfford = canAfford; Skill = skill; Odds = odds; }
    }

    /// <summary>Niveau de la compétence de création d'un type d'objet et avancement vers le suivant.</summary>
    public readonly struct SkillView
    {
        public readonly ItemType Type;
        public readonly int Level;
        public readonly int XpInLevel;
        public readonly int XpToNext;
        public bool Maxed => Level >= CraftingRules.MaxSkill;
        public SkillView(ItemType type, int level, int xpInLevel, int xpToNext) { Type = type; Level = level; XpInLevel = xpInLevel; XpToNext = xpToNext; }
    }

    /// <summary>Résultat d'une fabrication : l'objet, sa qualité, ses statistiques et la compétence.</summary>
    public readonly struct CraftOutcome
    {
        public readonly ItemDef Item;
        public readonly CraftQuality Quality;
        public readonly CraftedStats Stats;
        public readonly int NewSkill;
        public readonly bool LeveledUp;
        public CraftOutcome(ItemDef item, CraftQuality quality, CraftedStats stats, int newSkill, bool leveledUp)
        { Item = item; Quality = quality; Stats = stats; NewSkill = newSkill; LeveledUp = leveledUp; }
    }

    /// <summary>Atelier du labo : fabrique des objets avec des scraps, selon les recettes connues.</summary>
    public sealed class Workshop
    {
        static readonly ItemType[] Types = { ItemType.Weapon, ItemType.Armor, ItemType.Tool, ItemType.Implant };

        readonly MetaContext _c;

        public Workshop(MetaContext context) { _c = context; }

        GameSession Session => _c.Session;

        public int Scraps(ScrapKind kind) => CraftingOps.Scraps(Session, kind);

        static bool CanAfford(GameSession s, RecipeDef r)
            => CraftingOps.Scraps(s, ScrapKinds.For(r.Type)) >= r.ScrapCost && CraftingOps.Scraps(s, ScrapKind.Core) >= r.CoreCost;

        /// <summary>Recettes connues, dans l'ordre du catalogue.</summary>
        public IReadOnlyList<RecipeView> Recipes()
            => Definitions.Recipes.All.Where(r => CraftingOps.Knows(Session, r.Id))
                          .Select(r =>
                          {
                              int skill = CraftingOps.Skill(Session, r.Type);
                              return new RecipeView(r, CanAfford(Session, r), skill, CraftingRules.QualityOdds(skill, r.Difficulty));
                          }).ToList();

        public IReadOnlyList<SkillView> Skills()
            => Types.Select(t =>
            {
                int xp = CraftingOps.Xp(Session, t), level = CraftingRules.SkillLevel(xp);
                if (level >= CraftingRules.MaxSkill) return new SkillView(t, level, 0, 0);
                int from = CraftingRules.XpForLevel(level);
                return new SkillView(t, level, xp - from, CraftingRules.XpForLevel(level + 1) - from);
            }).ToList();

        /// <summary>Fabrique l'objet : rien n'est dépensé si la recette est inconnue ou si les scraps manquent.</summary>
        public Result<CraftOutcome> Craft(string recipeId)
        {
            var recipe = Definitions.Recipes.Find(recipeId);
            if (recipe == null || !CraftingOps.Knows(Session, recipeId)) return Result<CraftOutcome>.Fail(MetaErrors.Unknown);
            if (!CanAfford(Session, recipe)) return Result<CraftOutcome>.Fail(MetaErrors.NotEnoughScraps);

            CraftingOps.TrySpendScraps(Session, ScrapKinds.For(recipe.Type), recipe.ScrapCost);
            if (recipe.CoreCost > 0) CraftingOps.TrySpendScraps(Session, ScrapKind.Core, recipe.CoreCost);

            int skillBefore = CraftingOps.Skill(Session, recipe.Type);
            var rng = new Pcg32(CollectionRules.Seed(Session.Account?.PlayerId ?? "local", "craft", Session.Meta.craftedNext));
            var quality = CraftingRules.RollQuality(rng, skillBefore, recipe.Difficulty);
            var stats = CraftingRules.RollStats(rng, recipe, quality);
            CraftingOps.AddXp(Session, recipe.Type, CraftingRules.XpGain(skillBefore, recipe.Difficulty));
            var item = CraftingOps.AddCrafted(Session, recipe, quality, stats);
            int skillAfter = CraftingOps.Skill(Session, recipe.Type);

            _c.Touch(MetaArea.Lab);
            _c.Touch(MetaArea.Collection);
            return Result<CraftOutcome>.Ok(new CraftOutcome(item, quality, stats, skillAfter, skillAfter > skillBefore));
        }
    }
}
