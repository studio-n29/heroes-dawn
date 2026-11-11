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
    /// <summary>Composants, catalyseurs, recettes connues et objets fabriqués dans le profil (comme <see cref="CollectionOps"/> pour les objets).</summary>
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

        // ---- Composants (id « Weapon.Common ») et catalyseurs
        static string Key(ComponentKind kind, ComponentGrade grade) => kind + "." + grade;

        public static int Components(GameSession s, ComponentKind kind, ComponentGrade grade) => Get(s.Meta.components, Key(kind, grade));

        public static void AddComponents(GameSession s, ComponentKind kind, ComponentGrade grade, int count)
            => Set(s.Meta.components, Key(kind, grade), Math.Max(0, Components(s, kind, grade) + count));

        public static bool TrySpendComponents(GameSession s, ComponentKind kind, ComponentGrade grade, int count)
        {
            if (Components(s, kind, grade) < count) return false;
            AddComponents(s, kind, grade, -count);
            return true;
        }

        public static int Catalysts(GameSession s, CatalystKind kind) => Get(s.Meta.catalysts, kind.ToString());

        public static void AddCatalyst(GameSession s, CatalystKind kind, int count) => Set(s.Meta.catalysts, kind.ToString(), Math.Max(0, Catalysts(s, kind) + count));

        // ---- Recettes : les quatre Communes sont connues d'emblée, sans écriture dans la sauvegarde
        public static bool Knows(GameSession s, string recipeId) => Recipes.Starter.Contains(recipeId) || s.Meta.recipes.Contains(recipeId);

        public static void Learn(GameSession s, string recipeId)
        {
            if (!Knows(s, recipeId)) s.Meta.recipes.Add(recipeId);
        }

        /// <summary>Tire et verse les Composants et le catalyseur d'une victoire, et apprend la recette obtenue (déterministe selon le joueur et le nombre de tirages).</summary>
        public static CraftDrops GrantDrops(GameSession s, CombatRank rank, bool boss)
        {
            var unknown = Recipes.All.Where(r => !Knows(s, r.Id)).Select(r => r.Id).ToList();
            var rng = new Core.Pcg32(CollectionRules.Seed(s.Account?.PlayerId ?? "local", "components", s.Meta.scrapDrops++));
            var drops = CraftingRules.RollDrops(rng, rank, boss, unknown);
            for (int k = 0; k < drops.Components.Count; k++)
                if (drops.Components[k] > 0) AddComponents(s, (ComponentKind)(k / ComponentKinds.GradeCount), (ComponentGrade)(k % ComponentKinds.GradeCount), drops.Components[k]);
            if (drops.Catalyst.HasValue) AddCatalyst(s, drops.Catalyst.Value, 1);
            if (drops.NewRecipe != null) Learn(s, drops.NewRecipe);
            return drops;
        }

        // ---- Objets fabriqués
        /// <summary>Crée l'exemplaire <c>crafted_N</c> : l'enregistre dans le catalogue de la session, dans la sauvegarde et dans l'inventaire.</summary>
        public static ItemDef AddCrafted(GameSession s, RecipeDef recipe, int stars, CraftedStats stats)
        {
            var m = s.Meta;
            string id = CraftedItems.IdPrefix + (++m.craftedNext);
            m.crafted.Add(new CraftedItemSave { id = id, recipeId = recipe.Id, stars = stars, hp = stats.Hp, atk = stats.Atk, def = stats.Def, spd = stats.Spd });
            var def = CraftedItems.ToDef(id, recipe, stars, stats);
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
                int stars = Math.Max(CraftStars.Min, Math.Min(CraftStars.Max, c.stars > 0 ? c.stars : c.quality + 1));
                collection.Register(CraftedItems.ToDef(c.id, recipe, stars, new CraftedStats(c.hp, c.atk, c.def, c.spd)));
            }
        }
    }
}
