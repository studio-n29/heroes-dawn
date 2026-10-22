using System.Collections.Generic;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Combat
{
    public static class UnitFactory
    {
        /// <summary>Base × (1 + croissance)^(niveau−1), en entiers.</summary>
        public static int ScaleStat(int baseValue, int level, BalanceConfig b)
        {
            long v = baseValue;
            for (int i = 1; i < level; i++) v = v * (1000 + b.StatGrowthPermillePerLevel) / 1000;
            return (int)v;
        }

        /// <summary>03_Metahumains — malus linéaire au-delà du seuil, jusqu'à −30 % à 100 % de fatigue.</summary>
        public static int FatigueMultiplierPermille(int fatigue, BalanceConfig b)
        {
            if (fatigue <= b.FatigueMalusThreshold) return 1000;
            int over = fatigue - b.FatigueMalusThreshold;
            int span = 100 - b.FatigueMalusThreshold;
            return 1000 - b.FatigueMalusMaxPercent * 10 * over / span;
        }

        public static CombatUnit Ally(int id, MemberDef d, AllyEntry e, BalanceConfig b, CollectionDef? collection = null, SynergyBonus synergy = default)
        {
            int f = FatigueMultiplierPermille(e.FatiguePercent, b);
            int hpP = synergy.Hp, atkP = synergy.Atk, defP = synergy.Def, spdP = synergy.Spd, crit = 0, startGauge = 0, shield = 0; // la synergie d équipe est permanente
            if (collection != null)
                foreach (var itemId in e.Items)
                {
                    if (!collection.HasItem(itemId)) continue;
                    var it = collection.Item(itemId);
                    hpP += it.HpPercent; atkP += it.AtkPercent; defP += it.DefPercent; spdP += it.SpdPercent;
                    switch (it.Perk)
                    {
                        case ItemPerk.StartGauge: startGauge += it.PerkValue; break;
                        case ItemPerk.OpeningShield: shield += it.PerkValue; break;
                        case ItemPerk.CritChance: crit += it.PerkValue; break;
                    }
                }
            int S(int v, int itemPercent) => ScaleStat(v, e.Level, b) * f / 1000 * (100 + e.StatPercent) / 100 * (100 + itemPercent) / 100;
            var unit = new CombatUnit(id, Side.Ally, d.Id.Value, d.Family, d.Role, false,
                                      S(d.BaseStats.Hp, hpP), S(d.BaseStats.Atk, atkP), S(d.BaseStats.Def, defP), d.BaseStats.Spd * (100 + spdP) / 100,
                                      b.TimelinePoints, SkillsWithCards(d, e, collection));
            unit.MaxEnergy = b.EnergyMax;
            unit.Energy = System.Math.Min(b.EnergyMax, b.EnergyStart);
            unit.CritBonusPercent = crit;
            unit.StartPointBonus = startGauge * b.TimelinePoints / 100;
            if (shield > 0) unit.Statuses.Add(new StatusInstance(StatusType.Shield, 2, unit.MaxHp * shield / 100));
            return unit;
        }

        /// <summary>
        /// Compétences de l agent : ses deux compétences de base ; une carte équipée prend la 3e place (carte 1) puis la 4e (carte 2).
        /// Une carte d'une autre catégorie, absente du jeu ou une carte 2 sans carte 1 est ignorée.
        /// </summary>
        static IReadOnlyList<SkillDef> SkillsWithCards(MemberDef d, AllyEntry e, CollectionDef? collection)
        {
            if (collection == null || e.Cards.Count == 0) return d.Skills;
            SkillDef? Pick(int i)
            {
                if (i >= e.Cards.Count || !e.Cards[i].HasValue || !collection.HasCard(e.Cards[i]!.Value)) return null;
                var card = collection.Card(e.Cards[i]!.Value);
                return card.Family == d.Family ? card.Skill : null;
            }
            var list = new List<SkillDef>(d.Skills);
            var third = Pick(0);
            if (third != null && list.Count == 2) list.Add(third);          // deux compétences de base : la carte 1 prend la 3e place
            else if (third != null && list.Count >= 3) list[2] = third;     // ancien kit de trois compétences : la carte remplace la 3e
            var fourth = Pick(1);
            if (fourth != null && list.Count >= 3) list.Add(fourth);
            return list;
        }

        public static CombatUnit Enemy(int id, EnemyDef d, EnemyEntry e, CombatRank rank, BalanceConfig b)
        {
            int pct = rank == CombatRank.Easy ? b.RankEasyStatPercent
                    : (rank == CombatRank.Elite || rank == CombatRank.Boss) ? b.RankEliteStatPercent : 100;
            int S(int v) => ScaleStat(v, e.Level, b) * pct / 100;
            var unit = new CombatUnit(id, Side.Enemy, d.Id.Value, d.Family, d.Role, d.IsBoss,
                                  S(d.BaseStats.Hp), S(d.BaseStats.Atk), S(d.BaseStats.Def), d.BaseStats.Spd,
                                  b.TimelinePoints, d.Skills);
            unit.MaxEnergy = b.EnergyMax;
            unit.Energy = System.Math.Min(b.EnergyMax, b.EnergyStart);
            return unit;
        }
    }
}
