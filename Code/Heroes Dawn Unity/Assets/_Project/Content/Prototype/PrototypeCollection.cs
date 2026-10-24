#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Content.Prototype
{
    /// <summary>Objets, cartes de compétences et booster du prototype.</summary>
    public static class PrototypeCollection
    {
        static ItemDef I(string id, ItemRarity r, int price, int hp = 0, int atk = 0, int def = 0, int spd = 0, ItemPerk perk = ItemPerk.None, int perkValue = 0)
            => new ItemDef(new ItemId(id), "items." + id + ".name", r, hp, atk, def, spd, perk, perkValue, price);

        static EffectDef Dmg(int pct) => new EffectDef(EffectType.Damage, pct);
        static EffectDef Heal(int pct) => new EffectDef(EffectType.Heal, pct);
        static EffectDef Shield(int pct) => new EffectDef(EffectType.Shield, pct, StatusType.Shield, 2);
        static EffectDef St(StatusType s, int turns, int chance = 100) => new EffectDef(EffectType.ApplyStatus, 0, s, turns, chance);
        static EffectDef Push(int pct) => new EffectDef(EffectType.PushBack, pct);
        static EffectDef Adv(int pct) => new EffectDef(EffectType.Advance, pct);

        static SkillCardDef C(string id, Family f, ItemRarity r, int cooldown, TargetRule t, params EffectDef[] fx)
            => new SkillCardDef(new CardId(id), "cards." + id + ".name", f, r, new SkillDef(new SkillId(id), SkillSlot.A3, cooldown, t, fx));

        public static CollectionDef Build() => new CollectionDef(
            new List<ItemDef>
            {
                I("kevlar_vest", ItemRarity.Common, 300, hp: 10),
                I("steel_knuckles", ItemRarity.Common, 300, atk: 8),
                I("running_shoes", ItemRarity.Common, 300, spd: 6),
                I("riot_shield", ItemRarity.Common, 300, def: 10),
                I("adrenaline_patch", ItemRarity.Rare, 800, spd: 8, perk: ItemPerk.StartGauge, perkValue: 15),
                I("titan_gauntlets", ItemRarity.Rare, 900, atk: 12, perk: ItemPerk.CritChance, perkValue: 8),
                I("reactive_plating", ItemRarity.Rare, 900, hp: 8, def: 10, perk: ItemPerk.OpeningShield, perkValue: 12),
                I("phoenix_core", ItemRarity.Epic, 2200, hp: 15, atk: 10, perk: ItemPerk.OpeningShield, perkValue: 20),
                I("time_shard", ItemRarity.Epic, 2400, spd: 10, perk: ItemPerk.StartGauge, perkValue: 30),
                I("power_gauntlet", ItemRarity.Rare, 1000, atk: 4, perk: ItemPerk.SkillPower, perkValue: 15),
                I("arcane_focus", ItemRarity.Epic, 2600, hp: 6, perk: ItemPerk.SkillPower, perkValue: 25),
                I("apex_module", ItemRarity.Legendary, 0, atk: 18, def: 12, spd: 8, perk: ItemPerk.CritChance, perkValue: 15),
            },
            new List<SkillCardDef>
            {
                C("card_glass_wall", Family.Matter, ItemRarity.Common, 5, TargetRule.AllAllies, Shield(15)),
                C("card_shackle", Family.Matter, ItemRarity.Rare, 4, TargetRule.SingleEnemy, St(StatusType.Bind, 2), Push(25)),
                C("card_prism_burst", Family.Matter, ItemRarity.Epic, 5, TargetRule.AllEnemies, Dmg(80), St(StatusType.Vulnerable, 2)),
                C("card_ember", Family.Elemental, ItemRarity.Common, 5, TargetRule.SingleEnemy, Dmg(140)),
                C("card_storm", Family.Elemental, ItemRarity.Rare, 5, TargetRule.AllEnemies, Dmg(70), St(StatusType.Shock, 1, 30)),
                C("card_inferno", Family.Elemental, ItemRarity.Epic, 6, TargetRule.AllEnemies, Dmg(110), St(StatusType.Burn, 3)),
                C("card_second_wind", Family.PhysicalBio, ItemRarity.Common, 5, TargetRule.SingleAlly, Heal(35)),
                C("card_bulwark_charge", Family.PhysicalBio, ItemRarity.Rare, 5, TargetRule.SingleEnemy, Dmg(120), Push(25)),
                C("card_regen_field", Family.PhysicalBio, ItemRarity.Epic, 6, TargetRule.AllAllies, Heal(20), St(StatusType.Regen, 3)),
                C("card_mark", Family.Perception, ItemRarity.Common, 4, TargetRule.SingleEnemy, Dmg(100), St(StatusType.Mark, 99)),
                C("card_precog", Family.Perception, ItemRarity.Rare, 5, TargetRule.SingleAlly, Adv(50), St(StatusType.Haste, 2)),
                C("card_time_rift", Family.Perception, ItemRarity.Legendary, 7, TargetRule.AllAllies, Adv(40), St(StatusType.Haste, 3)),
            },
            new List<BoosterDef>
            {
                new BoosterDef(BoosterDef.StandardId, "boosters.booster_standard.name", 3, new[] { 70, 22, 7, 1 }, 5, 120),
            });
    }
}
