#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Content.Prototype
{
    /// <summary>Objets, cartes de compétences et booster du prototype.</summary>
    public static class PrototypeCollection
    {
        static ItemDef I(string id, ItemType type, ItemRarity r, int price, int hp = 0, int atk = 0, int def = 0, int spd = 0, ItemPerk perk = ItemPerk.None, int perkValue = 0)
            => new ItemDef(new ItemId(id), "items." + id + ".name", r, hp, atk, def, spd, perk, perkValue, price, type);

        static EffectDef Dmg(int pct) => new EffectDef(EffectType.Damage, pct);
        static EffectDef Heal(int pct) => new EffectDef(EffectType.Heal, pct);
        static EffectDef Shield(int pct) => new EffectDef(EffectType.Shield, pct, StatusType.Shield, 2);
        static EffectDef St(StatusType s, int turns, int chance = 100) => new EffectDef(EffectType.ApplyStatus, 0, s, turns, chance);
        static EffectDef Push(int pct) => new EffectDef(EffectType.PushBack, pct);
        static EffectDef Adv(int pct) => new EffectDef(EffectType.Advance, pct);
        static EffectDef Steal(int pct) => new EffectDef(EffectType.Lifesteal, pct);
        static EffectDef Purge(int count) => new EffectDef(EffectType.Purge, count);
        static EffectDef Bounce(int pct) => new EffectDef(EffectType.Bounce, pct);
        static EffectDef Exec(int pct) => new EffectDef(EffectType.Execute, pct);

        /// <summary>Les trois paliers d'une carte : niveaux 3, 6 et 10.</summary>
        static CardTier[] Tiers(EffectDef l3, EffectDef l6, EffectDef l10) => new[] { new CardTier(3, l3), new CardTier(6, l6), new CardTier(10, l10) };

        static SkillCardDef C(string id, Family f, CardNature n, ItemRarity r, int cooldown, TargetRule t, CardTier[] tiers, params EffectDef[] fx)
            => new SkillCardDef(new CardId(id), "cards." + id + ".name", f, r, new SkillDef(new SkillId(id), SkillSlot.A3, cooldown, t, fx), n, tiers);

        public static CollectionDef Build() => new CollectionDef(
            new List<ItemDef>
            {
                I("kevlar_vest", ItemType.Armor, ItemRarity.Common, 300, hp: 10),
                I("steel_knuckles", ItemType.Weapon, ItemRarity.Common, 300, atk: 8),
                I("running_shoes", ItemType.Tool, ItemRarity.Common, 300, spd: 6),
                I("riot_shield", ItemType.Armor, ItemRarity.Common, 300, def: 10),
                I("adrenaline_patch", ItemType.Tool, ItemRarity.Rare, 800, spd: 8, perk: ItemPerk.StartGauge, perkValue: 15),
                I("titan_gauntlets", ItemType.Weapon, ItemRarity.Rare, 900, atk: 12, perk: ItemPerk.CritChance, perkValue: 8),
                I("reactive_plating", ItemType.Armor, ItemRarity.Rare, 900, hp: 8, def: 10, perk: ItemPerk.OpeningShield, perkValue: 12),
                I("phoenix_core", ItemType.Armor, ItemRarity.Epic, 2200, hp: 15, atk: 10, perk: ItemPerk.OpeningShield, perkValue: 20),
                I("time_shard", ItemType.Tool, ItemRarity.Epic, 2400, spd: 10, perk: ItemPerk.StartGauge, perkValue: 30),
                I("power_gauntlet", ItemType.Implant, ItemRarity.Rare, 1000, atk: 4, perk: ItemPerk.SkillPower, perkValue: 15),
                I("arcane_focus", ItemType.Implant, ItemRarity.Epic, 2600, hp: 6, perk: ItemPerk.SkillPower, perkValue: 25),
                I("apex_module", ItemType.Weapon, ItemRarity.Legendary, 0, atk: 18, def: 12, spd: 8, perk: ItemPerk.CritChance, perkValue: 15),
            },
            new List<SkillCardDef>
            {
                C("card_glass_wall", Family.Matter, CardNature.Defensive, ItemRarity.Common, 5, TargetRule.AllAllies, Tiers(Purge(1), St(StatusType.Anchor, 1), St(StatusType.Regen, 2)), Shield(15)),
                C("card_shackle", Family.Matter, CardNature.Offensive, ItemRarity.Rare, 4, TargetRule.SingleEnemy, Tiers(St(StatusType.Vulnerable, 2), Purge(1), St(StatusType.Shock, 1)), St(StatusType.Bind, 2), Push(25)),
                C("card_prism_burst", Family.Matter, CardNature.Offensive, ItemRarity.Epic, 5, TargetRule.AllEnemies, Tiers(St(StatusType.Corrosion, 2, 30), Exec(30), Purge(1)), Dmg(80), St(StatusType.Vulnerable, 2)),
                C("card_ember", Family.Elemental, CardNature.Offensive, ItemRarity.Common, 5, TargetRule.SingleEnemy, Tiers(St(StatusType.Burn, 2), Bounce(50), Exec(50)), Dmg(140)),
                C("card_storm", Family.Elemental, CardNature.Offensive, ItemRarity.Rare, 5, TargetRule.AllEnemies, Tiers(St(StatusType.Blind, 1, 30), Steal(15), St(StatusType.Vulnerable, 2)), Dmg(70), St(StatusType.Shock, 1, 30)),
                C("card_inferno", Family.Elemental, CardNature.Offensive, ItemRarity.Epic, 6, TargetRule.AllEnemies, Tiers(Steal(10), St(StatusType.Corrosion, 2), Exec(40)), Dmg(110), St(StatusType.Burn, 3)),
                C("card_second_wind", Family.PhysicalBio, CardNature.Support, ItemRarity.Common, 5, TargetRule.SingleAlly, Tiers(Purge(0), Shield(10), St(StatusType.Regen, 2)), Heal(35)),
                C("card_bulwark_charge", Family.PhysicalBio, CardNature.Offensive, ItemRarity.Rare, 5, TargetRule.SingleEnemy, Tiers(Steal(20), Bounce(50), St(StatusType.Vulnerable, 2)), Dmg(120), Push(25)),
                C("card_regen_field", Family.PhysicalBio, CardNature.Support, ItemRarity.Epic, 6, TargetRule.AllAllies, Tiers(Purge(1), Shield(8), St(StatusType.Haste, 1)), Heal(20), St(StatusType.Regen, 3)),
                C("card_mark", Family.Perception, CardNature.Offensive, ItemRarity.Common, 4, TargetRule.SingleEnemy, Tiers(Exec(30), Bounce(40), St(StatusType.Blind, 1)), Dmg(100), St(StatusType.Mark, 99)),
                C("card_precog", Family.Perception, CardNature.Support, ItemRarity.Rare, 5, TargetRule.SingleAlly, Tiers(St(StatusType.Evade, 1), Purge(0), Shield(15)), Adv(50), St(StatusType.Haste, 2)),
                C("card_time_rift", Family.Perception, CardNature.Support, ItemRarity.Legendary, 7, TargetRule.AllAllies, Tiers(St(StatusType.Regen, 2), Purge(1), St(StatusType.Evade, 1)), Adv(40), St(StatusType.Haste, 3)),
            },
            new List<BoosterDef>
            {
                new BoosterDef(BoosterDef.StandardId, "boosters.booster_standard.name", 3, new[] { 70, 22, 7, 1 }, 5, 120),
            });
    }
}
