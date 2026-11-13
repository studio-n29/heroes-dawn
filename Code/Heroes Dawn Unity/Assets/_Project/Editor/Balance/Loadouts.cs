#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Combat;

namespace N29.HeroesDawn.EditorTools.Balance
{
    /// <summary>Niveau d'équipement supposé d'une équipe simulée (les cartes et objets de la collection du jeu).</summary>
    public enum EquipmentProfile
    {
        /// <summary>Aucun objet, aucune carte : le plancher.</summary>
        Bare,
        /// <summary>Début de campagne : un objet commun et une carte 1 étoile par agent.</summary>
        Light,
        /// <summary>Milieu de Brooklyn : deux objets (communs ou rares) et trois cartes de 1 à 2 étoiles par agent.</summary>
        Average,
        /// <summary>Joueur bien équipé : deux objets rares ou épiques et trois cartes de 1 à 3 étoiles par agent.</summary>
        Strong,
        /// <summary>Diagnostic : les deux objets du profil moyen, aucune carte.</summary>
        ItemsOnly,
        /// <summary>Diagnostic : les trois cartes du profil moyen, aucun objet.</summary>
        CardsOnly,
        /// <summary>Joueur bien équipé, objets seuls : deux objets rares ou épiques, aucune carte (les cartes se jouent à la main, pas en simulation automatique).</summary>
        ItemsStrong,
    }

    /// <summary>
    /// Équipement supposé des agents simulés : on choisit, dans la collection réelle du jeu, les objets les plus forts de la rareté permise
    /// (sans doublon dans l'équipe) et les cartes de la famille de l'agent d'abord (bonus de famille), niveau 1.
    /// </summary>
    public static class Loadouts
    {
        public static AllyEntry Entry(ContentCatalog content, MemberId member, int level, int fatiguePercent, EquipmentProfile profile, ISet<string> usedItems)
        {
            if (profile == EquipmentProfile.Bare) return new AllyEntry(member, level, fatiguePercent);
            var def = content.Member(member);
            int itemCount = profile == EquipmentProfile.Light ? 1 : profile == EquipmentProfile.CardsOnly ? 0 : 2;
            int cardCount = profile == EquipmentProfile.Light ? 1 : (profile == EquipmentProfile.ItemsOnly || profile == EquipmentProfile.ItemsStrong) ? 0 : 3;
            int maxStars = profile == EquipmentProfile.Light ? 1 : profile == EquipmentProfile.Strong ? 3 : 2;
            ItemRarity maxRarity = profile == EquipmentProfile.Light ? ItemRarity.Common : (profile == EquipmentProfile.Strong || profile == EquipmentProfile.ItemsStrong) ? ItemRarity.Epic : ItemRarity.Rare;
            ItemRarity minRarity = (profile == EquipmentProfile.Strong || profile == EquipmentProfile.ItemsStrong) ? ItemRarity.Rare : ItemRarity.Common;

            var items = content.Collection.Items
                .Where(i => i.Rarity <= maxRarity && i.Rarity >= minRarity && !usedItems.Contains(i.Id.Value))
                .OrderByDescending(i => i.HpPercent + i.AtkPercent + i.DefPercent + i.SpdPercent + i.PerkValue)
                .ThenBy(i => i.Id.Value, StringComparer.Ordinal)
                .Take(itemCount).ToList();
            foreach (var i in items) usedItems.Add(i.Id.Value);

            var cards = content.Collection.Cards
                .Where(c => c.Stars <= maxStars)
                .OrderByDescending(c => c.Family == def.Family ? 1 : 0).ThenByDescending(c => c.Stars).ThenBy(c => c.Id.Value, StringComparer.Ordinal)
                .Take(cardCount).Select(c => (CardId?)c.Id).ToList();
            return new AllyEntry(member, level, fatiguePercent, 0, items.Select(i => i.Id).ToList(), cards, cards.Select(_ => 1).ToList());
        }
    }
}
