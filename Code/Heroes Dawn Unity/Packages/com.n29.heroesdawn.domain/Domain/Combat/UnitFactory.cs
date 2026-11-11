using System.Collections.Generic;
using N29.HeroesDawn.Core;
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

        public static CombatUnit Ally(int id, MemberDef d, AllyEntry e, BalanceConfig b, CollectionDef? collection = null, SynergyBonus synergy = default, PairBonusTotal pair = default)
            => Agent(Side.Ally, id, d, e, b, collection, synergy, pair);

        /// <summary>Agent d'un autre joueur dans le camp adverse (duel de tournoi) : même calcul qu'un allié, camp <see cref="Side.Enemy"/>, style d'IA selon la position.</summary>
        public static CombatUnit Opponent(int id, AllyEntry entry, MemberDef def, CollectionDef? collection, BalanceConfig b, SynergyBonus synergy = default, PairBonusTotal pair = default)
        {
            var unit = Agent(Side.Enemy, id, def, entry, b, collection, synergy, pair);
            unit.Style = AiStyles.Default(def.Position);
            return unit;
        }

        static CombatUnit Agent(Side side, int id, MemberDef d, AllyEntry e, BalanceConfig b, CollectionDef? collection, SynergyBonus synergy, PairBonusTotal pair)
        {
            int f = FatigueMultiplierPermille(e.FatiguePercent, b);
            var tiers = LevelTiers.BonusAt(e.Level); // paliers de niveau : bonus passifs de l agent
            int hpP = synergy.Hp + tiers.Hp + pair.Hp, atkP = synergy.Atk + tiers.Atk + pair.Atk, defP = synergy.Def + tiers.Def + pair.Def, spdP = synergy.Spd + tiers.Spd + pair.Spd,
                crit = pair.Crit, startGauge = pair.StartGauge, shield = pair.OpeningShield, skillPower = 0; // la synergie d équipe est permanente ; les bonus de paire aussi
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
                        case ItemPerk.SkillPower: skillPower += it.PerkValue; break;
                    }
                }
            int S(int v, int itemPercent) => ScaleStat(v, e.Level, b) * f / 1000 * (100 + e.StatPercent) / 100 * (100 + itemPercent) / 100;
            var unit = new CombatUnit(id, side, d.Id.Value, d.Family, d.Position, false,
                                      S(d.BaseStats.Hp, hpP), S(d.BaseStats.Atk, atkP), S(d.BaseStats.Def, defP), d.BaseStats.Spd * (100 + spdP) / 100,
                                      b.TimelinePoints, EmpoweredSkills(SkillsWithCards(d, e, collection), skillPower));
            unit.MaxEnergy = b.EnergyMax;
            unit.Energy = System.Math.Min(b.EnergyMax, b.EnergyStart + pair.StartEnergy);
            unit.EnergyRegenBonus = pair.EnergyPerStep;
            unit.CritBonusPercent = crit;
            unit.StartPointBonus = startGauge * b.TimelinePoints / 100;
            if (shield > 0) unit.Statuses.Add(new StatusInstance(StatusType.Shield, 2, unit.MaxHp * shield / 100));
            return unit;
        }

        /// <summary>
        /// Compétences de l agent : ses deux compétences de base, puis ses cartes équipées dans l ordre des emplacements (jusqu à cinq compétences).
        /// Équipement libre : aucune condition de famille. Une carte absente du jeu ou déjà portée dans un emplacement précédent est ignorée.
        /// Ancien kit de trois compétences : la première carte remplace la 3e.
        /// Chaque carte reçoit son bonus de position (+15 %) et de famille (+5 %) après son palier, avant la puissance de compétence des objets.
        /// Chaque carte reçoit son bonus de position (+15 %) et de famille (+5 %) après son palier, avant la puissance de compétence des objets.
        /// </summary>
        static IReadOnlyList<SkillDef> SkillsWithCards(MemberDef d, AllyEntry e, CollectionDef? collection)
        {
            if (collection == null || e.Cards.Count == 0) return d.Skills;
            var list = new List<SkillDef>(d.Skills);
            var seen = new HashSet<CardId>();
            bool first = true;
            for (int i = 0; i < e.Cards.Count; i++)
            {
                if (!e.Cards[i].HasValue) continue;
                var id = e.Cards[i]!.Value;
                if (!collection.HasCard(id) || !seen.Add(id)) continue;
                int lvl = i < e.CardLevels.Count ? e.CardLevels[i] : 1;
                var card = collection.Card(id);
                var skill = CardBonuses.Apply(CardLevels.SkillAt(card, lvl), CardBonuses.Percent(card, d));
                if (first && list.Count >= 3) list[2] = skill;   // ancien kit de trois compétences : la carte remplace la 3e
                else list.Add(skill);
                first = false;
            }
            return list;
        }

        /// <summary>Applique la puissance de compétence des objets (dégâts, soins, boucliers) à toutes les compétences.</summary>
        static IReadOnlyList<SkillDef> EmpoweredSkills(IReadOnlyList<SkillDef> skills, int skillPower)
        {
            if (skillPower <= 0) return skills;
            var list = new List<SkillDef>(skills.Count);
            foreach (var s in skills) list.Add(CardLevels.ScalePercent(s, 100 + skillPower));
            return list;
        }

        public static CombatUnit Enemy(int id, EnemyDef d, EnemyEntry e, CombatRank rank, BalanceConfig b)
        {
            int pct = rank == CombatRank.Easy ? b.RankEasyStatPercent
                    : (rank == CombatRank.Elite || rank == CombatRank.Boss) ? b.RankEliteStatPercent : 100;
            int S(int v) => ScaleStat(v, e.Level, b) * pct / 100;
            var unit = new CombatUnit(id, Side.Enemy, d.Id.Value, d.Family, d.Position, d.IsBoss,
                                  S(d.BaseStats.Hp), S(d.BaseStats.Atk), S(d.BaseStats.Def), d.BaseStats.Spd,
                                  b.TimelinePoints, d.Skills);
            unit.MaxEnergy = b.EnergyMax;
            unit.Energy = System.Math.Min(b.EnergyMax, b.EnergyStart);
            unit.Phases = d.Phases;
            unit.Style = d.Style;
            return unit;
        }
    }
}
