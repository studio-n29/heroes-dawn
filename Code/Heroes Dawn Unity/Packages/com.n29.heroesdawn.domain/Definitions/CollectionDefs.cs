#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
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

    /// <summary>Type d'un objet : un agent ne porte pas deux objets du même type (l'ordre est celui des entiers de items.json).</summary>
    public enum ItemType { Weapon, Armor, Tool, Implant }

    /// <summary>Type déduit d'un objet : son effet spécial, sinon sa stat la plus forte (égalité : ATQ, puis PV/DEF, puis VIT).</summary>
    public static class ItemTypes
    {
        public static ItemType Of(ItemPerk perk, int hp, int atk, int def, int spd)
        {
            switch (perk)
            {
                case ItemPerk.SkillPower: return ItemType.Implant;
                case ItemPerk.CritChance: return ItemType.Weapon;
                case ItemPerk.OpeningShield: return ItemType.Armor;
                case ItemPerk.StartGauge: return ItemType.Tool;
            }
            int guard = Math.Max(hp, def);
            if (atk >= guard && atk >= spd) return ItemType.Weapon;
            return guard >= spd ? ItemType.Armor : ItemType.Tool;
        }
    }

    /// <summary>Objet équipable (jusqu'à 2 par agent, de types différents) : bonus de statistiques en % et effet spécial éventuel.</summary>
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
        /// <summary>Arme, armure, outil ou implant : saisi, sinon déduit.</summary>
        public ItemType Type { get; }

        public ItemDef(ItemId id, string nameKey, ItemRarity rarity, int hp = 0, int atk = 0, int def = 0, int spd = 0,
                       ItemPerk perk = ItemPerk.None, int perkValue = 0, int price = 0, ItemType? type = null)
        {
            Id = id; NameKey = nameKey; Rarity = rarity; HpPercent = hp; AtkPercent = atk; DefPercent = def; SpdPercent = spd;
            Perk = perk; PerkValue = perkValue; Price = price;
            Type = type ?? ItemTypes.Of(perk, hp, atk, def, spd);
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
        /// <summary>Paliers d'effets secondaires (niveaux 3, 6 et 10), dans l'ordre des niveaux ; vide : aucun.</summary>
        public IReadOnlyList<CardTier> Tiers { get; }

        public SkillCardDef(CardId id, string nameKey, Family family, ItemRarity rarity, SkillDef skill, CardNature? nature = null, IReadOnlyList<CardTier>? tiers = null)
        {
            Id = id; NameKey = nameKey; Family = family; Rarity = rarity; Skill = skill;
            Nature = nature ?? CardNatures.Of(skill);
            Tiers = tiers ?? Array.Empty<CardTier>();
        }
    }

    /// <summary>Palier d'une carte : à partir de ce niveau, ses effets s'ajoutent à la compétence (sans être augmentés par le niveau).</summary>
    public sealed class CardTier
    {
        public int Level { get; }
        public IReadOnlyList<EffectDef> Effects { get; }

        public CardTier(int level, params EffectDef[] effects) { Level = level; Effects = effects; }
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

        readonly Dictionary<ItemId, ItemDef> _craftedById = new Dictionary<ItemId, ItemDef>();
        readonly List<ItemDef> _crafted = new List<ItemDef>();

        /// <summary>Objets fabriqués au labo (exemplaires uniques), à part du catalogue : ni la boutique ni les drops ne les voient.</summary>
        public IReadOnlyList<ItemDef> Crafted => _crafted;

        /// <summary>Le catalogue puis les objets fabriqués : tout ce qui peut être possédé ou équipé.</summary>
        public IReadOnlyList<ItemDef> AllItems => _crafted.Count == 0 ? Items : Items.Concat(_crafted).ToList();

        /// <summary>Enregistre un objet fabriqué (même identifiant : remplacé). Refusé sur <see cref="Empty"/>, partagée par tout le jeu.</summary>
        public void Register(ItemDef item)
        {
            if (ReferenceEquals(this, Empty)) throw new InvalidOperationException("CollectionDef.Empty est partagée : elle n'accepte aucun objet fabriqué.");
            if (_craftedById.TryGetValue(item.Id, out var old)) _crafted.Remove(old);
            _craftedById[item.Id] = item;
            _crafted.Add(item);
        }

        public void ClearCrafted()
        {
            _craftedById.Clear();
            _crafted.Clear();
        }

        public bool HasItem(ItemId id) => _items.ContainsKey(id) || _craftedById.ContainsKey(id);
        public bool HasCard(CardId id) => _cards.ContainsKey(id);
        public bool HasBooster(string id) => _boosters.ContainsKey(id);
        public ItemDef Item(ItemId id) => _items.TryGetValue(id, out var item) ? item : _craftedById[id];
        public SkillCardDef Card(CardId id) => _cards[id];
        public BoosterDef Booster(string id) => _boosters[id];
    }
}
