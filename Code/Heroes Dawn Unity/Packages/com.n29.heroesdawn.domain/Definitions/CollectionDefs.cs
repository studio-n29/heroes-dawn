#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Definitions
{
    /// <summary>Rareté d'un objet ou d'une carte.</summary>
    public enum ItemRarity { Common, Rare, Epic, Legendary }

    /// <summary>Effet spécial d'un objet, en plus de ses bonus de statistiques.</summary>
    public enum ItemPerk
    {
        None,
        /// <summary>Départ de combat avancé de N % sur la frise.</summary>
        StartGauge,
        /// <summary>Bouclier de N % des PV max en début de combat.</summary>
        OpeningShield,
        /// <summary>N % de chances de critique en plus.</summary>
        CritChance,
        /// <summary>Toutes les compétences de l'agent infligent, soignent et protègent N % de plus.</summary>
        SkillPower,
    }

    /// <summary>Objet équipable (jusqu'à 2 par agent) : bonus de statistiques en % et effet spécial éventuel.</summary>
    public sealed class ItemDef
    {
        public ItemId Id { get; }
        public string NameKey { get; }
        public ItemRarity Rarity { get; }
        public int HpPercent { get; }
        public int AtkPercent { get; }
        public int DefPercent { get; }
        public int SpdPercent { get; }
        public ItemPerk Perk { get; }
        public int PerkValue { get; }
        /// <summary>Prix en Crédits en boutique.</summary>
        public int Price { get; }

        public ItemDef(ItemId id, string nameKey, ItemRarity rarity, int hp = 0, int atk = 0, int def = 0, int spd = 0,
                       ItemPerk perk = ItemPerk.None, int perkValue = 0, int price = 0)
        {
            Id = id; NameKey = nameKey; Rarity = rarity; HpPercent = hp; AtkPercent = atk; DefPercent = def; SpdPercent = spd;
            Perk = perk; PerkValue = perkValue; Price = price;
        }
    }

    /// <summary>Nature d'une carte : ce qu'elle fait dans le combat (axe croisé avec la famille, le type de pouvoir).</summary>
    public enum CardNature { Offensive, Defensive, Support }

    /// <summary>Nature déduite de l'effet de base et de la cible ; pré-remplit la nature saisie de chaque carte.</summary>
    public static class CardNatures
    {
        public static CardNature Of(SkillDef skill)
        {
            var fx = skill.BaseEffect;
            if (fx == null) return CardNature.Offensive;
            if (fx.Type == EffectType.Shield) return CardNature.Defensive;
            if (fx.Type == EffectType.Heal) return CardNature.Support;
            return skill.Target == TargetRule.SingleEnemy || skill.Target == TargetRule.AllEnemies ? CardNature.Offensive : CardNature.Support;
        }
    }

    /// <summary>Carte du grimoire : une compétence qu'un agent de la catégorie (famille) peut équiper en emplacement 3 ou 4.</summary>
    public sealed class SkillCardDef
    {
        public CardId Id { get; }
        public string NameKey { get; }
        /// <summary>Catégorie d'agent qui peut l'utiliser (le type de pouvoir de la carte).</summary>
        public Family Family { get; }
        public ItemRarity Rarity { get; }
        public SkillDef Skill { get; }
        /// <summary>Offensive, défensive ou de soutien : saisie, sinon déduite de l'effet.</summary>
        public CardNature Nature { get; }

        public SkillCardDef(CardId id, string nameKey, Family family, ItemRarity rarity, SkillDef skill, CardNature? nature = null)
        {
            Id = id; NameKey = nameKey; Family = family; Rarity = rarity; Skill = skill;
            Nature = nature ?? CardNatures.Of(skill);
        }
    }

    /// <summary>Booster de cartes : nombre de cartes, poids de rareté (Commun, Rare, Épique, Légendaire), carte au moins Rare garantie tous les N boosters.</summary>
    public sealed class BoosterDef
    {
        /// <summary>Booster ordinaire : gagne en jeu, vendu en boutique.</summary>
        public const string StandardId = "booster_standard";
        public string Id { get; }
        public string NameKey { get; }
        public int Cards { get; }
        public IReadOnlyList<int> Weights { get; }
        /// <summary>Pitié : au bout de ce nombre de boosters sans carte Rare ou mieux, le suivant en contient une.</summary>
        public int PityEvery { get; }
        /// <summary>Prix en PR en boutique ; 0 : non vendu.</summary>
        public int PricePr { get; }

        public BoosterDef(string id, string nameKey, int cards, IReadOnlyList<int> weights, int pityEvery, int pricePr)
        {
            Id = id; NameKey = nameKey; Cards = cards; Weights = weights; PityEvery = pityEvery; PricePr = pricePr;
        }
    }

    /// <summary>Objets, cartes et boosters du jeu.</summary>
    public sealed class CollectionDef
    {
        readonly Dictionary<ItemId, ItemDef> _items = new Dictionary<ItemId, ItemDef>();
        readonly Dictionary<CardId, SkillCardDef> _cards = new Dictionary<CardId, SkillCardDef>();
        readonly Dictionary<string, BoosterDef> _boosters = new Dictionary<string, BoosterDef>();

        public static readonly CollectionDef Empty = new CollectionDef(Array.Empty<ItemDef>(), Array.Empty<SkillCardDef>(), Array.Empty<BoosterDef>());

        public IReadOnlyList<ItemDef> Items { get; }
        public IReadOnlyList<SkillCardDef> Cards { get; }
        public IReadOnlyList<BoosterDef> Boosters { get; }

        public CollectionDef(IReadOnlyList<ItemDef> items, IReadOnlyList<SkillCardDef> cards, IReadOnlyList<BoosterDef> boosters)
        {
            Items = items; Cards = cards; Boosters = boosters;
            foreach (var i in items) _items.Add(i.Id, i);
            foreach (var c in cards) _cards.Add(c.Id, c);
            foreach (var b in boosters) _boosters.Add(b.Id, b);
        }

        public bool HasItem(ItemId id) => _items.ContainsKey(id);
        public bool HasCard(CardId id) => _cards.ContainsKey(id);
        public bool HasBooster(string id) => _boosters.ContainsKey(id);
        public ItemDef Item(ItemId id) => _items[id];
        public SkillCardDef Card(CardId id) => _cards[id];
        public BoosterDef Booster(string id) => _boosters[id];
    }
}
