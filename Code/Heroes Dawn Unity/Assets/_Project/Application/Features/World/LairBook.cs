#nullable enable
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;

namespace N29.HeroesDawn.Application.Features.World
{
    /// <summary>Ce qu un boss de repaire vaincu a laissé : Composants (type, qualité, nombre), catalyseur et recette ; <see cref="Reduced"/> si ce n était pas la première victoire du jour.</summary>
    public readonly struct LairGrant
    {
        public readonly ComponentKind Kind;
        public readonly ComponentGrade Grade;
        public readonly int Count;
        public readonly ComponentGrade BonusGrade;
        public readonly int BonusCount;
        public readonly CatalystKind? Catalyst;
        public readonly string? Recipe;
        public readonly bool Reduced;
        /// <summary>Total de Composants versés (toutes qualités).</summary>
        public int Components => Count + BonusCount;
        public LairGrant(ComponentKind kind, ComponentGrade grade, int count, ComponentGrade bonusGrade, int bonusCount, CatalystKind? catalyst, string? recipe, bool reduced)
        {
            Kind = kind; Grade = grade; Count = count; BonusGrade = bonusGrade; BonusCount = bonusCount; Catalyst = catalyst; Recipe = recipe; Reduced = reduced;
        }
    }

    /// <summary>Un boss d'un repaire et s'il est déjà tombé.</summary>
    public readonly struct LairBossView
    {
        public readonly LairBossDef Boss;
        public readonly bool Defeated;
        public LairBossView(LairBossDef boss, bool defeated) { Boss = boss; Defeated = defeated; }
    }

    /// <summary>
    /// Progression des repaires, gardée dans le profil : boss vaincus (retenus d'une partie à l'autre) et nombre de repaires terminés.
    /// Les enregistrements sont statiques sur <see cref="MetaSave"/> : ils sont appelés depuis la fin de combat et la validation de zone.
    /// </summary>
    public sealed class LairBook
    {
        readonly MetaContext _c;

        public LairBook(MetaContext context) { _c = context; }

        static string Key(string lairId, string caseId) => lairId + ":" + caseId;
        static string ClearsKey(string lairId) => "lair_clears:" + lairId;

        public bool Unlocked(LairDef lair) => _c.Session.CompletedZones.Contains(lair.RequiresZone);

        public IReadOnlyList<LairBossView> Bosses(LairDef lair)
            => lair.Bosses.Select(b => new LairBossView(b, _c.Meta.lairBosses.Contains(Key(lair.Id, b.Case)))).ToList();

        public int DefeatedCount(LairDef lair) => Bosses(lair).Count(b => b.Defeated);

        public int Clears(LairDef lair) => _c.Meta.GetCounter(_c.Meta.stats, ClearsKey(lair.Id));

        /// <summary>Un combat de la zone vient d'être gagné : si c'est un boss de repaire, il est retenu.</summary>
        public static void RecordBossDefeat(MetaSave meta, string zoneId, string caseId)
        {
            var lair = Lairs.OfZone(zoneId);
            if (lair == null || lair.Bosses.All(b => b.Case != caseId)) return;
            string key = Key(lair.Id, caseId);
            if (!meta.lairBosses.Contains(key)) meta.lairBosses.Add(key);
        }

        /// <summary>Jour « aaaa-mm-jj » en entier AAAAMMJJ (0 si le texte est vide).</summary>
        static int DayNumber(string today)
        {
            int n = 0;
            foreach (char ch in today) if (ch >= '0' && ch <= '9') n = n * 10 + (ch - '0');
            return n;
        }

        /// <summary>
        /// Verse la récompense d'un boss de repaire vaincu : Composants, chance de catalyseur et recette (bloc C de la 1.0.0).
        /// Complète la première fois du jour pour ce boss, réduite de moitié ensuite (Composants arrondis à l'entier inférieur, au moins 1 ;
        /// chance de catalyseur divisée par deux ; pas de recette). La recette n'est donnée que si elle est encore inconnue.
        /// Tirage déterministe selon le joueur, le boss, le jour et le nombre de récompenses déjà versées pour ce boss.
        /// </summary>
        public static LairGrant GrantBossReward(MetaSave meta, GameSession s, LairDef lair, LairBossDef boss, string today)
        {
            var reward = boss.Reward;
            if (reward == null) return default;
            string key = Key(lair.Id, boss.Case);
            int day = DayNumber(today);
            bool reduced = day != 0 && meta.GetCounter(meta.lairRewardDay, key) == day;
            if (!reduced) meta.SetCounter(meta.lairRewardDay, key, day);
            string drawKey = "lair_rewards:" + key;
            int draw = meta.GetCounter(meta.stats, drawKey);
            meta.SetCounter(meta.stats, drawKey, draw + 1);

            var rng = new Core.Pcg32(CollectionRules.Seed(s.Account?.PlayerId ?? "local", "lair:" + key + ":" + day, draw));
            var kind = reward.Kind ?? (ComponentKind)rng.Range(0, ComponentKinds.Count);
            int count = reduced ? System.Math.Max(1, reward.Count / 2) : reward.Count;
            int bonus = reduced ? reward.BonusCount / 2 : reward.BonusCount;
            if (count > 0) CraftingOps.AddComponents(s, kind, reward.Grade, count);
            if (bonus > 0) CraftingOps.AddComponents(s, kind, reward.BonusGrade, bonus);

            CatalystKind? catalyst = null;
            int chance = reduced ? reward.CatalystPercent / 2 : reward.CatalystPercent;
            bool lucky = chance > 0 && rng.Chance(chance);
            var kindOfCatalyst = (CatalystKind)rng.Range(0, ComponentKinds.CatalystCount);
            if (lucky) { catalyst = kindOfCatalyst; CraftingOps.AddCatalyst(s, kindOfCatalyst, 1); }

            string? recipe = null;
            if (!reduced && reward.RecipeId != null && !CraftingOps.Knows(s, reward.RecipeId))
            {
                CraftingOps.Learn(s, reward.RecipeId);
                recipe = reward.RecipeId;
            }
            return new LairGrant(kind, reward.Grade, count, reward.BonusGrade, bonus, catalyst, recipe, reduced);
        }

        /// <summary>Même chose à partir de la zone et de la case du combat gagné ; nul si ce n'est pas un boss de repaire.</summary>
        public static LairGrant? GrantForCase(GameSession s, string zoneId, string caseId, string today)
        {
            var lair = Lairs.OfZone(zoneId);
            var boss = lair?.Bosses.FirstOrDefault(b => b.Case == caseId);
            if (lair == null || boss == null || boss.Reward == null) return null;
            return GrantBossReward(s.Meta, s, lair, boss, today);
        }

        /// <summary>La zone d'un repaire vient d'être validée : un repaire terminé de plus.</summary>
        public static void RecordClear(MetaSave meta, string zoneId)
        {
            var lair = Lairs.OfZone(zoneId);
            if (lair == null) return;
            meta.SetCounter(meta.stats, ClearsKey(lair.Id), meta.GetCounter(meta.stats, ClearsKey(lair.Id)) + 1);
        }
    }
}
