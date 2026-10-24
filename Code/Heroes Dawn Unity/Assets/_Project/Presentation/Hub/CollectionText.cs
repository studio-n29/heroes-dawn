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
            }
            return string.Join(" · ", parts);
        }

        /// <summary>Montants bruts d'un agent à un niveau donné (de quoi afficher « Dégâts 120 » plutôt que « 80 % »).</summary>
        public static SkillOwnerStats StatsOf(MemberDef def, int level, BalanceConfig balance)
            => new SkillOwnerStats(UnitFactory.ScaleStat(def.BaseStats.Atk, level, balance), UnitFactory.ScaleStat(def.BaseStats.Hp, level, balance));

        /// <summary>Statistiques de référence quand la carte n'est pas portée : le meilleur agent de l'équipe, ou le premier niveau à défaut.</summary>
        public static SkillOwnerStats ReferenceStats(GameSession session)
        {
            var best = session.Roster.OrderByDescending(m => m.Level).FirstOrDefault();
            return best != null ? StatsOf(best.Def, best.Level, session.Content.Balance) : new SkillOwnerStats(100, 1000);
        }

        public static string CardName(ILocalization loc, SkillCardDef card) => loc.SkillName(card.Skill);

        /// <summary>« Matière · Rare · Dégâts 80 % … » (catégorie, rareté, effets).</summary>
        public static string CardLine(ILocalization loc, SkillCardDef card, SkillOwnerStats? owner = null)
            => loc.F("grimoire.card_line", loc.FamilyName(card.Family), RarityName(loc, card.Rarity), owner.HasValue ? loc.DescribeSkill(card.Skill, owner.Value.Atk, owner.Value.MaxHp) : loc.DescribeSkill(card.Skill));

        /// <summary>Initiale colorée pour la pastille d'une ligne.</summary>
        public static string Tag(string name) => string.IsNullOrEmpty(name) ? "?" : name.Substring(0, 1).ToUpperInvariant();

        public static IEnumerable<SkillCardDef> CardsFor(CollectionDef defs, MemberDef member)
            => defs.Cards.Where(c => c.Family == member.Family);
    }
}
