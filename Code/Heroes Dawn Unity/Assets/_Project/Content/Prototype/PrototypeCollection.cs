#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Authoring;
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
        static EffectDef Gauge(int points) => new EffectDef(EffectType.GaugeDelta, points);

        /// <summary>Les trois paliers d'une carte : niveaux 3, 6 et 10.</summary>
        static CardTier[] Tiers(EffectDef l3, EffectDef l6, EffectDef l10) => new[] { new CardTier(3, l3), new CardTier(6, l6), new CardTier(10, l10) };

        static SkillCardDef C(string id, Family f, CardNature n, int stars, int cooldown, TargetRule t, CardTier[] tiers, params EffectDef[] fx)
            => new SkillCardDef(new CardId(id), "cards." + id + ".name", f, stars, new SkillDef(new SkillId(id), SkillSlot.A3, cooldown, t, fx), n, tiers);

        /// <summary>Étiquette une carte avec son archétype et son rôle (d'après docs/catalogue/cartes-existantes.md), sans toucher à aucune valeur de jeu.</summary>
        static SkillCardDef T(SkillCardDef c, Archetype archetype, CardRole role)
            => new SkillCardDef(c.Id, c.NameKey, c.Family, c.Stars, c.Skill, c.Nature, c.Tiers, new[] { archetype }, role);

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
                // Objets venus des planches Aube tactique (kit d'extension) : tissu balistique, gel de récupération, cellule d'énergie, puce neuronale.
                I("ballistic_fabric", ItemType.Armor, ItemRarity.Common, 300, def: 8),
                I("recovery_gel", ItemType.Tool, ItemRarity.Common, 300, hp: 12),
                I("energy_cell", ItemType.Tool, ItemRarity.Rare, 800, hp: 6, perk: ItemPerk.StartGauge, perkValue: 12),
                I("neural_chip", ItemType.Implant, ItemRarity.Epic, 2400, spd: 4, perk: ItemPerk.SkillPower, perkValue: 20),
            },
            CardDataLibrary.Load(LegacyCards),
            new List<BoosterDef>
            {
                new BoosterDef(BoosterDef.StandardId, "boosters.booster_standard.name", 3, BoosterDef.StandardWeights, 5, 120),
                new BoosterDef(BoosterDef.OperationId, "boosters.booster_operation.name", 5, new[] { 40, 30, 19, 9, 2 }, 4, 300),
                new BoosterDef(BoosterDef.MissionId, "boosters.booster_mission.name", 4, new[] { 20, 35, 28, 14, 3 }, 3, 250),
            });

        /// <summary>Les 48 cartes du prototype écrites en code : repli de <c>cards.json</c> et référence de l'export (menu Heroes Dawn ▸ Contenu).</summary>
        public static IReadOnlyList<SkillCardDef> LegacyCards() => new List<SkillCardDef>
            {
                T(C("card_glass_wall", Family.Matter, CardNature.Defensive, 1, 5, TargetRule.AllAllies, Tiers(Purge(1), St(StatusType.Anchor, 1), St(StatusType.Regen, 2)), Shield(15)), Archetype.Bulwark, CardRole.Basic),
                T(C("card_shackle", Family.Matter, CardNature.Control, 3, 4, TargetRule.SingleEnemy, Tiers(St(StatusType.Vulnerable, 2), Purge(1), St(StatusType.Shock, 1)), St(StatusType.Bind, 2), Push(25)), Archetype.Restrain, CardRole.Primer),
                T(C("card_prism_burst", Family.Matter, CardNature.Control, 4, 5, TargetRule.AllEnemies, Tiers(St(StatusType.Corrosion, 2, 30), Exec(30), Purge(1)), Dmg(80), St(StatusType.Vulnerable, 2)), Archetype.Restrain, CardRole.Basic),
                T(C("card_ember", Family.Elemental, CardNature.Offensive, 2, 5, TargetRule.SingleEnemy, Tiers(St(StatusType.Burn, 2), Bounce(50), Exec(50)), Dmg(140)), Archetype.Burn, CardRole.Primer),
                T(C("card_storm", Family.Elemental, CardNature.Offensive, 3, 5, TargetRule.AllEnemies, Tiers(St(StatusType.Blind, 1, 30), Steal(15), St(StatusType.Vulnerable, 2)), Dmg(70), St(StatusType.Shock, 1, 30)), Archetype.Restrain, CardRole.Primer),
                T(C("card_inferno", Family.Elemental, CardNature.Offensive, 4, 6, TargetRule.AllEnemies, Tiers(Steal(10), St(StatusType.Corrosion, 2), Exec(40)), Dmg(110), St(StatusType.Burn, 3)), Archetype.Burn, CardRole.Primer),
                T(C("card_second_wind", Family.PhysicalBio, CardNature.Support, 2, 5, TargetRule.SingleAlly, Tiers(Purge(0), Shield(10), St(StatusType.Regen, 2)), Heal(35)), Archetype.Renewal, CardRole.Basic),
                T(C("card_bulwark_charge", Family.PhysicalBio, CardNature.Offensive, 3, 5, TargetRule.SingleEnemy, Tiers(Steal(20), Bounce(50), St(StatusType.Vulnerable, 2)), Dmg(120), Push(25)), Archetype.Bulwark, CardRole.Basic),
                T(C("card_regen_field", Family.PhysicalBio, CardNature.Support, 4, 6, TargetRule.AllAllies, Tiers(Purge(1), Shield(8), St(StatusType.Haste, 1)), Heal(20), St(StatusType.Regen, 3)), Archetype.Renewal, CardRole.Primer),
                T(C("card_mark", Family.Perception, CardNature.Control, 2, 4, TargetRule.SingleEnemy, Tiers(Exec(30), Bounce(40), St(StatusType.Blind, 1)), Dmg(100), St(StatusType.Mark, 99)), Archetype.Hunt, CardRole.Primer),
                T(C("card_precog", Family.Perception, CardNature.Support, 3, 5, TargetRule.SingleAlly, Tiers(St(StatusType.Evade, 1), Purge(0), Shield(15)), Adv(50), St(StatusType.Haste, 2)), Archetype.Tempo, CardRole.Primer),
                T(C("card_time_rift", Family.Perception, CardNature.Support, 5, 7, TargetRule.AllAllies, Tiers(St(StatusType.Regen, 2), Purge(1), St(StatusType.Evade, 1)), Adv(40), St(StatusType.Haste, 3)), Archetype.Tempo, CardRole.Primer),

                // ---- Lot C2 : Matière
                T(C("card_glass_shards", Family.Matter, CardNature.Offensive, 2, 4, TargetRule.SingleEnemy, Tiers(Bounce(40), St(StatusType.Corrosion, 2), Exec(30)), Dmg(120), St(StatusType.Vulnerable, 1, 30)), Archetype.Hunt, CardRole.Primer),
                T(C("card_concrete_rampart", Family.Matter, CardNature.Defensive, 1, 5, TargetRule.SingleAlly, Tiers(St(StatusType.Anchor, 1), Purge(1), St(StatusType.Regen, 2)), Shield(20)), Archetype.Bulwark, CardRole.Basic),
                T(C("card_rivet_armor", Family.Matter, CardNature.Defensive, 1, 4, TargetRule.Self, Tiers(St(StatusType.Taunt, 1), St(StatusType.Anchor, 1), St(StatusType.Regen, 2)), Shield(25)), Archetype.Bulwark, CardRole.Basic),
                T(C("card_healing_molt", Family.Matter, CardNature.Support, 1, 5, TargetRule.SingleAlly, Tiers(Purge(1), St(StatusType.Regen, 2), St(StatusType.Haste, 1)), Heal(30)), Archetype.Renewal, CardRole.Basic),
                T(C("card_reflecting_prism", Family.Matter, CardNature.Defensive, 3, 5, TargetRule.AllAllies, Tiers(St(StatusType.Evade, 1, 30), Purge(1), St(StatusType.Anchor, 1)), Shield(12)), Archetype.Bulwark, CardRole.Basic),
                T(C("card_support_cables", Family.Matter, CardNature.Support, 3, 5, TargetRule.SingleAlly, Tiers(Shield(8), Purge(0), St(StatusType.Haste, 1)), Adv(40), St(StatusType.Anchor, 1)), Archetype.Tempo, CardRole.Primer),
                T(C("card_borrowed_mask", Family.Matter, CardNature.Support, 2, 5, TargetRule.Self, Tiers(St(StatusType.Haste, 1), Purge(0), Shield(15)), St(StatusType.Evade, 2)), Archetype.Bulwark, CardRole.Primer),
                T(C("card_living_forge", Family.Matter, CardNature.Support, 4, 6, TargetRule.AllAllies, Tiers(St(StatusType.Regen, 2), Purge(1), Shield(10)), Heal(15)), Archetype.Renewal, CardRole.Basic),
                T(C("card_monolith", Family.Matter, CardNature.Offensive, 5, 7, TargetRule.AllEnemies, Tiers(St(StatusType.Vulnerable, 2), Steal(15), Exec(40)), Dmg(120), St(StatusType.Bind, 1)), Archetype.Restrain, CardRole.Primer),

                // ---- Lot C2 : Élémentaire
                T(C("card_frost_wall", Family.Elemental, CardNature.Defensive, 1, 5, TargetRule.SingleAlly, Tiers(Purge(1), St(StatusType.Anchor, 1), St(StatusType.Regen, 2)), Shield(20)), Archetype.Bulwark, CardRole.Basic),
                T(C("card_steam_shield", Family.Elemental, CardNature.Defensive, 1, 4, TargetRule.Self, Tiers(St(StatusType.Evade, 1, 30), Purge(1), St(StatusType.Regen, 2)), Shield(25)), Archetype.Bulwark, CardRole.Basic),
                T(C("card_tonic_current", Family.Elemental, CardNature.Support, 2, 5, TargetRule.SingleAlly, Tiers(St(StatusType.Haste, 1), Purge(1), St(StatusType.Regen, 2)), Heal(30)), Archetype.Renewal, CardRole.Basic),
                T(C("card_soothing_mist", Family.Elemental, CardNature.Support, 1, 5, TargetRule.AllAllies, Tiers(Purge(1), St(StatusType.Regen, 2), Shield(8)), Heal(15)), Archetype.Renewal, CardRole.Basic),
                T(C("card_neon_halo", Family.Elemental, CardNature.Defensive, 2, 5, TargetRule.AllAllies, Tiers(Purge(1), St(StatusType.Evade, 1, 30), St(StatusType.Regen, 2)), Shield(12)), Archetype.Bulwark, CardRole.Basic),
                T(C("card_spark_rush", Family.Elemental, CardNature.Support, 3, 5, TargetRule.SingleAlly, Tiers(St(StatusType.Haste, 1), Shield(10), Purge(0)), Adv(50)), Archetype.Tempo, CardRole.Primer),
                T(C("card_power_flux", Family.Elemental, CardNature.Support, 3, 5, TargetRule.AllAllies, Tiers(St(StatusType.Haste, 1), Purge(1), Shield(8)), Gauge(100)), Archetype.Tempo, CardRole.Primer),
                T(C("card_ice_carapace", Family.Elemental, CardNature.Defensive, 4, 6, TargetRule.AllAllies, Tiers(St(StatusType.Anchor, 1), Purge(1), St(StatusType.Regen, 2)), Shield(18)), Archetype.Bulwark, CardRole.Basic),
                T(C("card_chain_lightning", Family.Elemental, CardNature.Offensive, 5, 7, TargetRule.SingleEnemy, Tiers(Bounce(60), St(StatusType.Vulnerable, 2), Exec(50)), Dmg(160), St(StatusType.Shock, 1, 40)), Archetype.Restrain, CardRole.Primer),

                // ---- Lot C2 : Physique / Bio
                T(C("card_ram_strike", Family.PhysicalBio, CardNature.Control, 2, 4, TargetRule.SingleEnemy, Tiers(St(StatusType.Vulnerable, 1), Steal(15), Bounce(40)), Dmg(110), Push(20)), Archetype.Restrain, CardRole.Primer),
                T(C("card_steel_skin", Family.PhysicalBio, CardNature.Defensive, 1, 4, TargetRule.Self, Tiers(St(StatusType.Taunt, 1), St(StatusType.Regen, 2), St(StatusType.Anchor, 1)), Shield(25)), Archetype.Bulwark, CardRole.Basic),
                T(C("card_deep_anchor", Family.PhysicalBio, CardNature.Defensive, 1, 5, TargetRule.SingleAlly, Tiers(Purge(1), St(StatusType.Regen, 2), St(StatusType.Haste, 1)), Shield(15), St(StatusType.Anchor, 1)), Archetype.Bulwark, CardRole.Primer),
                T(C("card_fresh_blood", Family.PhysicalBio, CardNature.Support, 1, 5, TargetRule.SingleAlly, Tiers(St(StatusType.Regen, 2), Purge(0), Shield(10)), Heal(30)), Archetype.Renewal, CardRole.Basic),
                T(C("card_bloodletting", Family.PhysicalBio, CardNature.Offensive, 3, 5, TargetRule.SingleEnemy, Tiers(St(StatusType.Corrosion, 2), Exec(30), Bounce(50)), Dmg(120), Steal(20)), Archetype.Hunt, CardRole.Basic),
                T(C("card_challenge", Family.PhysicalBio, CardNature.Defensive, 3, 5, TargetRule.Self, Tiers(St(StatusType.Anchor, 1), St(StatusType.Regen, 2), Purge(0)), Shield(20), St(StatusType.Taunt, 2)), Archetype.Bulwark, CardRole.Primer),
                T(C("card_shared_pulse", Family.PhysicalBio, CardNature.Support, 2, 5, TargetRule.AllAllies, Tiers(Purge(1), St(StatusType.Regen, 2), St(StatusType.Haste, 1)), Heal(18)), Archetype.Renewal, CardRole.Basic),
                T(C("card_living_bastion", Family.PhysicalBio, CardNature.Defensive, 4, 6, TargetRule.AllAllies, Tiers(St(StatusType.Regen, 2), St(StatusType.Anchor, 1), Purge(1)), Shield(18)), Archetype.Bulwark, CardRole.Basic),
                T(C("card_telluric_fist", Family.PhysicalBio, CardNature.Offensive, 5, 7, TargetRule.AllEnemies, Tiers(St(StatusType.Vulnerable, 2), Steal(20), Exec(40)), Dmg(120), Push(25)), Archetype.Restrain, CardRole.Primer),

                // ---- Lot C2 : Perception
                T(C("card_precise_shot", Family.Perception, CardNature.Offensive, 2, 4, TargetRule.SingleEnemy, Tiers(Exec(30), St(StatusType.Vulnerable, 1), Bounce(40)), Dmg(130)), Archetype.Hunt, CardRole.Basic),
                T(C("card_side_step", Family.Perception, CardNature.Defensive, 1, 4, TargetRule.Self, Tiers(St(StatusType.Haste, 1), Purge(1), St(StatusType.Regen, 2)), Shield(15), St(StatusType.Evade, 1)), Archetype.Tempo, CardRole.Basic),
                T(C("card_camouflage", Family.Perception, CardNature.Defensive, 1, 5, TargetRule.SingleAlly, Tiers(St(StatusType.Evade, 1, 30), Purge(1), St(StatusType.Haste, 1)), Shield(18)), Archetype.Bulwark, CardRole.Basic),
                T(C("card_guiding_echo", Family.Perception, CardNature.Support, 2, 5, TargetRule.SingleAlly, Tiers(St(StatusType.Haste, 1), Shield(10), Purge(0)), Adv(40)), Archetype.Tempo, CardRole.Primer),
                T(C("card_guard_break", Family.Perception, CardNature.Control, 3, 5, TargetRule.SingleEnemy, Tiers(St(StatusType.Mark, 1), Exec(40), Bounce(50)), Dmg(100), Purge(1), St(StatusType.Vulnerable, 2)), Archetype.Hunt, CardRole.Primer),
                T(C("card_premonition", Family.Perception, CardNature.Defensive, 2, 5, TargetRule.AllAllies, Tiers(St(StatusType.Evade, 1, 30), Purge(1), St(StatusType.Haste, 1)), Shield(12)), Archetype.Bulwark, CardRole.Basic),
                T(C("card_shared_watch", Family.Perception, CardNature.Support, 3, 5, TargetRule.AllAllies, Tiers(Purge(1), St(StatusType.Haste, 1), Shield(8)), Gauge(100)), Archetype.Tempo, CardRole.Primer),
                T(C("card_silent_takedown", Family.Perception, CardNature.Offensive, 4, 6, TargetRule.SingleEnemy, Tiers(St(StatusType.Blind, 1), Steal(15), Bounce(60)), Dmg(150), Exec(50)), Archetype.Hunt, CardRole.Payoff),
                T(C("card_time_veil", Family.Perception, CardNature.Defensive, 4, 6, TargetRule.AllAllies, Tiers(Purge(1), St(StatusType.Evade, 1, 30), St(StatusType.Regen, 2)), Shield(15), Adv(20)), Archetype.Tempo, CardRole.Basic),
            };
    }
}
