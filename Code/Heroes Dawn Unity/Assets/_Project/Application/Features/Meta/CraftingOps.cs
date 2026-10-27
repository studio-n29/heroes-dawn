#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Application.Features.Collection;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;

namespace N29.HeroesDawn.Application.Features.Meta
{
    /// <summary>Scraps, recettes connues, compétences de création et objets fabriqués dans le profil (comme <see cref="CollectionOps"/> pour les objets).</summary>
    public static class CraftingOps
    {
        static int Get(List<CounterSave> list, string id)
        {
            foreach (var c in list) if (c.id == id) return c.value;
            return 0;
        }

        static void Set(List<CounterSave> list, string id, int value)
        {
            foreach (var c in list) if (c.id == id) { c.value = value; return; }
            list.Add(new CounterSave { id = id, value = value });
        }

        // ---- Scraps
        public static int Scraps(GameSession s, ScrapKind kind) => Get(s.Meta.scraps, kind.ToString());

        public static void AddScraps(GameSession s, ScrapKind kind, int count) => Set(s.Meta.scraps, kind.ToString(), Math.Max(0, Scraps(s, kind) + count));

        public static bool TrySpendScraps(GameSession s, ScrapKind kind, int count)
        {
            if (Scraps(s, kind) < count) return false;
            AddScraps(s, kind, -count);
            return true;
        }

        // ---- Recettes : les quatre Communes sont connues d'emblée, sans écriture dans la sauvegarde
        public static bool Knows(GameSession s, string recipeId) => Recipes.Starter.Contains(recipeId) || s.Meta.recipes.Contains(recipeId);

        public static void Learn(GameSession s, string recipeId)
        {
            if (!Knows(s, recipeId)) s.Meta.recipes.Add(recipeId);
        }

        // ---- Compétence de création, un niveau par type d'objet
        public static int Xp(GameSession s, ItemType type) => Get(s.Meta.craftXp, type.ToString());

        public static void AddXp(GameSession s, ItemType type, int xp) => Set(s.Meta.craftXp, type.ToString(), Math.Min(CraftingRules.XpForLevel(CraftingRules.MaxSkill), Xp(s, type) + xp));

        public static int Skill(GameSession s, ItemType type) => CraftingRules.SkillLevel(Xp(s, type));

        /// <summary>Tire et verse les scraps d'une victoire, et apprend la recette obtenue (déterministe selon le joueur et le nombre de tirages).</summary>
        public static CraftDrops GrantDrops(GameSession s, CombatRank rank, bool boss)
        {
            var unknown = Recipes.All.Where(r => !Knows(s, r.Id)).Select(r => r.Id).ToList();
            var rng = new Core.Pcg32(CollectionRules.Seed(s.Account?.PlayerId ?? "local", "scrap", s.Meta.scrapDrops++));
            var drops = CraftingRules.RollDrops(rng, rank, boss, unknown);
            for (int k = 0; k < ScrapKinds.Count; k++)
                if (drops.Scraps[k] > 0) AddScraps(s, (ScrapKind)k, drops.Scraps[k]);
            if (drops.NewRecipe != null) Learn(s, drops.NewRecipe);
            return drops;
        }

        // ---- Objets fabriqués
        /// <summary>Crée l'exemplaire <c>crafted_N</c> : l'enregistre dans le catalogue de la session, dans la sauvegarde et dans l'inventaire.</summary>
        public static ItemDef AddCrafted(GameSession s, RecipeDef recipe, CraftQuality quality, CraftedStats stats)
        {
            var m = s.Meta;
            string id = CraftedItems.IdPrefix + (++m.craftedNext);
            m.crafted.Add(new CraftedItemSave { id = id, recipeId = recipe.Id, quality = (int)quality, hp = stats.Hp, atk = stats.Atk, def = stats.Def, spd = stats.Spd });
            var def = CraftedItems.ToDef(id, recipe, quality, stats);
            s.Content.Collection.Register(def);
            CollectionOps.AddItem(s, def.Id);
            return def;
        }

        /// <summary>Après le chargement d'un profil : le catalogue ne garde que les objets fabriqués de CE profil (un identifiant resté d'un profil précédent disparaît).</summary>
        public static void RegisterSaved(GameSession s)
        {
            foreach (var c in s.Meta.crafted)
                if (c.id.StartsWith(CraftedItems.IdPrefix) && int.TryParse(c.id.Substring(CraftedItems.IdPrefix.Length), out int n))
                    s.Meta.craftedNext = Math.Max(s.Meta.craftedNext, n);
            var collection = s.Content.Collection;
            if (ReferenceEquals(collection, CollectionDef.Empty)) return;
            collection.ClearCrafted();
            foreach (var c in s.Meta.crafted)
            {
                var recipe = Recipes.Find(c.recipeId);
                if (recipe == null || string.IsNullOrEmpty(c.id)) continue;
                var quality = (CraftQuality)Math.Max(0, Math.Min((int)CraftQuality.Masterwork, c.quality));
                collection.Register(CraftedItems.ToDef(c.id, recipe, quality, new CraftedStats(c.hp, c.atk, c.def, c.spd)));
            }
        }
    }
}
