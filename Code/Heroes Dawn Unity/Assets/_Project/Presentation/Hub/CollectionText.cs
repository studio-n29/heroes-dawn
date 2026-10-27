#nullable enable
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Domain.Combat;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Textes et couleurs des objets et des cartes : nom, rareté, effets en une ligne.</summary>
    public static class CollectionText
    {
        public static string ItemName(ILocalization loc, ItemDef item) => loc.T(item.NameKey);

        public static string RarityName(ILocalization loc, ItemRarity r) => loc.T("item.rarity." + r);

        /// <summary>Couleur de rareté (jamais seule porteuse du sens : le nom de la rareté est toujours écrit).</summary>
        public static Color RarityColor(ItemRarity r)
        {
            switch (r)
            {
                case ItemRarity.Legendary: return Theme.Primary;
                case ItemRarity.Epic: return Theme.Perception;
                case ItemRarity.Rare: return Theme.Info;
                default: return Theme.TextMuted;
            }
        }

        /// <summary>« PV +10 % · Critique +8 % ».</summary>
        public static string ItemEffects(ILocalization loc, ItemDef item)
        {
            var parts = new List<string>();
            if (item.HpPercent != 0) parts.Add(loc.F("item.fx.hp", item.HpPercent));
            if (item.AtkPercent != 0) parts.Add(loc.F("item.fx.atk", item.AtkPercent));
            if (item.DefPercent != 0) parts.Add(loc.F("item.fx.def", item.DefPercent));
            if (item.SpdPercent != 0) parts.Add(loc.F("item.fx.spd", item.SpdPercent));
            switch (item.Perk)
            {
                case ItemPerk.StartGauge: parts.Add(loc.F("item.fx.start_gauge", item.PerkValue)); break;
                case ItemPerk.OpeningShield: parts.Add(loc.F("item.fx.opening_shield", item.PerkValue)); break;
                case ItemPerk.CritChance: parts.Add(loc.F("item.fx.crit", item.PerkValue)); break;
                case ItemPerk.SkillPower: parts.Add(loc.F("item.fx.skill_power", item.PerkValue)); break;
            }
            return string.Join(" · ", parts);
        }

        /// <summary>Montants bruts d'un agent à un niveau donné (de quoi afficher « Dégâts 120 » plutôt que « 80 % »).</summary>
        public static SkillOwnerStats StatsOf(MemberDef def, int level, BalanceConfig balance)
            => new SkillOwnerStats(UnitFactory.ScaleStat(def.BaseStats.Atk, level, balance), UnitFactory.ScaleStat(def.BaseStats.Hp, level, balance));

        /// <summary>Statistiques de référence quand la carte n'est pas portée : le meilleur agent de l'équipe, ou le premier niveau à défaut.</summary>
        public static SkillOwnerStats ReferenceStats(ISessionReadModel session)
        {
            var best = session.Roster.OrderByDescending(m => m.Level).FirstOrDefault();
            return best != null ? StatsOf(best.Def, best.Level, session.Content.Balance) : new SkillOwnerStats(100, 1000);
        }

        public static string CardName(ILocalization loc, SkillCardDef card) => loc.SkillName(card.Skill);

        /// <summary>« Matière · Rare · Dégâts 80 % … » (catégorie, rareté, effets).</summary>
        public static string CardLine(ILocalization loc, SkillCardDef card, SkillOwnerStats? owner = null, int level = 1)
            => loc.F("grimoire.card_line", loc.FamilyName(card.Family), RarityName(loc, card.Rarity), owner.HasValue ? loc.DescribeSkill(CardLevels.SkillAt(card, level), owner.Value.Atk, owner.Value.MaxHp) : loc.DescribeSkill(CardLevels.SkillAt(card, level)));

        /// <summary>Initiale colorée pour la pastille d'une ligne.</summary>
        public static string Tag(string name) => string.IsNullOrEmpty(name) ? "?" : name.Substring(0, 1).ToUpperInvariant();

        /// <summary>Effets d'un palier en une ligne (« Rebond 50 % », « Brûlure 2 t + Purge (1) »).</summary>
        public static string TierEffects(CardTier tier) => string.Join(" + ", SkillText.Effects(tier.Effects));

        /// <summary>Paliers d'une carte, un par ligne : atteint en vert précédé de « ● », à venir grisé.</summary>
        public static string TierLines(ILocalization loc, SkillCardDef card, int level)
            => string.Join("\n", card.Tiers.Select(t =>
            {
                bool reached = t.Level <= level;
                string line = loc.F("grimoire.tier_line", t.Level, TierEffects(t));
                return "<color=#" + ColorUtility.ToHtmlStringRGB(reached ? Theme.Success : Theme.TextMuted) + ">" + (reached ? "● " : "") + line + "</color>";
            }));

        public static string NatureName(ILocalization loc, CardNature n) => loc.T("card.nature." + n.ToString().ToLowerInvariant());
        public static string ItemTypeName(ILocalization loc, ItemType t) => loc.T("item.type." + t.ToString().ToLowerInvariant());

        /// <summary>Pastille colorée de la nature d'une carte (« ● Défensive »), en texte enrichi.</summary>
        public static string NatureChip(ILocalization loc, CardNature n)
            => Chip(NatureName(loc, n), n == CardNature.Offensive ? Theme.Danger : n == CardNature.Defensive ? Theme.Info : Theme.Success);

        /// <summary>Pastille colorée du type d'un objet (« ● Armure »), en texte enrichi.</summary>
        public static string ItemTypeChip(ILocalization loc, ItemType t)
            => Chip(ItemTypeName(loc, t), t == ItemType.Weapon ? Theme.Danger : t == ItemType.Armor ? Theme.Info : t == ItemType.Tool ? Theme.Primary : Theme.Pr);

        static string Chip(string label, Color color) => "<color=#" + ColorUtility.ToHtmlStringRGB(color) + ">● " + label + "</color>";

        public static IEnumerable<SkillCardDef> CardsFor(CollectionDef defs, MemberDef member)
            => defs.Cards.Where(c => c.Family == member.Family);
    }
}
