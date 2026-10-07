using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Content.Prototype
{
    /// <summary>
    /// Zones 1.2 (Gowanus, le canal) et 1.3 (Dumbo, le pont) de l'Acte 1 et leurs ennemis. L'ordre est libre dans l'acte
    /// (1.1 et 1.2 sont disponibles d'emblée) ; 1.3 s'ouvre quand les deux autres sont validées (<see cref="Requires"/>).
    /// </summary>
    public static class PrototypeZones
    {
        public static readonly ZoneId Zone12 = new ZoneId("zone_ny_1_2");
        public static readonly ZoneId Zone13 = new ZoneId("zone_ny_1_3");

        static EffectDef Dmg(int pct) => new EffectDef(EffectType.Damage, pct);
        static EffectDef St(StatusType s, int turns, int chance = 100) => new EffectDef(EffectType.ApplyStatus, 0, s, turns, chance);
        static EffectDef Shield(int pct, int turns = 2) => new EffectDef(EffectType.Shield, pct, StatusType.Shield, turns);

        static SkillDef S(string id, SkillSlot slot, int cd, TargetRule t, params EffectDef[] fx)
            => new SkillDef(new SkillId(id), slot, cd, t, fx);

        static EnemyDef E(string id, Role r, Family f, StatBlock st, bool boss, params SkillDef[] skills)
            => new EnemyDef(new EnemyId(id), "enemies." + id + ".name", r, f, st, skills, boss);

        /// <summary>Ennemis ajoutés aux zones 1.2 et 1.3.</summary>
        public static IEnumerable<EnemyDef> Enemies()
        {
            yield return E("sniper", Role.Precision, Family.Perception, new StatBlock(900, 135, 60, 110), false,
                S("sniper_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(110)),
                S("sniper_a2", SkillSlot.A2, 4, TargetRule.SingleEnemy, Dmg(180), St(StatusType.Mark, 2)));
            yield return E("canal_chief", Role.Damage, Family.Matter, new StatBlock(2600, 125, 100, 100), false,
                S("canal_chief_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(110)),
                S("canal_chief_a2", SkillSlot.A2, 3, TargetRule.AllEnemies, Dmg(80), St(StatusType.Corrosion, 2, 40)),
                S("canal_chief_a3", SkillSlot.A3, 5, TargetRule.Self, Shield(20)));
            yield return E("bridge_titan", Role.Tank, Family.PhysicalBio, new StatBlock(5600, 120, 140, 95), true,
                S("bridge_titan_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(125)),
                S("bridge_titan_a2", SkillSlot.A2, 3, TargetRule.Self, St(StatusType.Taunt, 2), Shield(12)),
                S("bridge_titan_a3", SkillSlot.A3, 5, TargetRule.AllEnemies, Dmg(95), St(StatusType.Shock, 1, 35)));
        }

        public static IEnumerable<ZoneDef> Zones()
        {
            yield return BuildZone12();
            yield return BuildZone13();
        }

        /// <summary>Zones à valider avant d'ouvrir celle-ci (vide : disponible d'emblée).</summary>
        public static IReadOnlyList<ZoneId> Requires(ZoneId zone)
        {
            if (zone == Zone13) return new[] { PrototypeContent.Zone11, Zone12 };
            return System.Array.Empty<ZoneId>();
        }

        static NodeDef N(string id, CaseType t, int x, int y, bool fixedNode = false, CombatRank r = CombatRank.Normal, string activity = null)
            => new NodeDef(new CaseId(id), t, fixedNode, r, x, y, activity == null ? (ActivityId?)null : new ActivityId(activity));

        static List<LinkDef> Links(params (string, string)[] pairs)
        {
            var links = new List<LinkDef>();
            foreach (var (a, b) in pairs) links.Add(new LinkDef(new CaseId(a), new CaseId(b)));
            return links;
        }

        /// <summary>Gowanus — le canal : 20 cases, 2 embranchements, Élite fixe : le chef du gang.</summary>
        static ZoneDef BuildZone12()
        {
            var nodes = new List<NodeDef>
            {
                N("n00", CaseType.Entry, 0, 2, true),
                N("n01", CaseType.Resource, 1, 2), N("n02", CaseType.Combat, 2, 2, r: CombatRank.Easy),
                N("n03", CaseType.Narrative, 3, 3), N("n04", CaseType.Resource, 4, 3), N("n05", CaseType.Combat, 5, 3),
                N("n06", CaseType.Resource, 3, 1), N("n07", CaseType.Empty, 4, 1), N("n08", CaseType.Combat, 5, 1, r: CombatRank.Easy),
                N("n09", CaseType.Rest, 6, 2), N("n10", CaseType.Combat, 7, 2),
                N("n11", CaseType.Resource, 8, 3), N("n12", CaseType.Narrative, 9, 3),
                N("n13", CaseType.MiniGame, 8, 1, true), N("n14", CaseType.Combat, 9, 1, true, CombatRank.Elite),
                N("n15", CaseType.Empty, 10, 2), N("n16", CaseType.Resource, 11, 2), N("n17", CaseType.Combat, 12, 2),
                N("n18", CaseType.Rest, 13, 2), N("n19", CaseType.Empty, 14, 2, true),
            };
            var links = Links(("n00", "n01"), ("n01", "n02"), ("n02", "n03"), ("n02", "n06"), ("n03", "n04"), ("n04", "n05"),
                              ("n06", "n07"), ("n07", "n08"), ("n05", "n09"), ("n08", "n09"), ("n09", "n10"), ("n10", "n11"),
                              ("n10", "n13"), ("n11", "n12"), ("n13", "n14"), ("n12", "n15"), ("n14", "n15"), ("n15", "n16"),
                              ("n16", "n17"), ("n17", "n18"), ("n18", "n19"));
            var objectives = new List<ObjectiveDef>
            {
                new ObjectiveDef("obj_chief", ObjectiveType.DefeatCombat, new CaseId("n14"), "objectives.ny12.chief"),
                new ObjectiveDef("obj_pump", ObjectiveType.CompleteMiniGame, new CaseId("n13"), "objectives.ny12.pump"),
                new ObjectiveDef("obj_bridge", ObjectiveType.ReachCase, new CaseId("n19"), "objectives.ny12.bridge"),
            };
            var encounters = new List<EncounterDef>
            {
                new EncounterDef(CombatRank.Easy, new[] { new EnemyId("thug"), new EnemyId("thug") }),
                new EncounterDef(CombatRank.Easy, new[] { new EnemyId("sniper"), new EnemyId("thug") }),
                new EncounterDef(CombatRank.Normal, new[] { new EnemyId("brute"), new EnemyId("sniper"), new EnemyId("thug") }),
                new EncounterDef(CombatRank.Normal, new[] { new EnemyId("street_pyro"), new EnemyId("sniper"), new EnemyId("brute") }),
                new EncounterDef(CombatRank.Elite, new[] { new EnemyId("canal_chief"), new EnemyId("thug"), new EnemyId("sniper") }),
            };
            return new ZoneDef(Zone12, "zones.ny_1_2.name", new CaseId("n00"), nodes, links, objectives, encounters, "1.2", 1, "brooklyn", 6, null, new ZoneEnvironment(Biome.Industrial, Weather.Rain));
        }

        /// <summary>Dumbo — le pont : 19 cases, boss fixe au bout : le Titan du pont.</summary>
        static ZoneDef BuildZone13()
        {
            var nodes = new List<NodeDef>
            {
                N("n00", CaseType.Entry, 0, 2, true),
                N("n01", CaseType.Resource, 1, 2), N("n02", CaseType.Combat, 2, 2),
                N("n03", CaseType.Resource, 3, 3), N("n04", CaseType.Narrative, 4, 3), N("n05", CaseType.Combat, 5, 3),
                N("n06", CaseType.MiniGame, 3, 1, activity: "potion_field"), N("n07", CaseType.Combat, 4, 1, r: CombatRank.Easy), N("n08", CaseType.Resource, 5, 1),
                N("n09", CaseType.Rest, 6, 2), N("n10", CaseType.Combat, 7, 2),
                N("n11", CaseType.MiniGame, 8, 3, true), N("n12", CaseType.Resource, 9, 3),
                N("n13", CaseType.Narrative, 8, 1), N("n14", CaseType.Combat, 9, 1, true, CombatRank.Elite),
                N("n15", CaseType.Empty, 10, 2, true), N("n16", CaseType.Resource, 11, 2), N("n17", CaseType.Rest, 12, 2),
                N("n18", CaseType.Combat, 13, 2, true, CombatRank.Boss),
            };
            var links = Links(("n00", "n01"), ("n01", "n02"), ("n02", "n03"), ("n02", "n06"), ("n03", "n04"), ("n04", "n05"),
                              ("n06", "n07"), ("n07", "n08"), ("n05", "n09"), ("n08", "n09"), ("n09", "n10"), ("n10", "n11"),
                              ("n10", "n13"), ("n11", "n12"), ("n13", "n14"), ("n12", "n15"), ("n14", "n15"), ("n15", "n16"),
                              ("n16", "n17"), ("n17", "n18"));
            var objectives = new List<ObjectiveDef>
            {
                new ObjectiveDef("obj_titan", ObjectiveType.DefeatCombat, new CaseId("n18"), "objectives.ny13.titan"),
                new ObjectiveDef("obj_cables", ObjectiveType.CompleteMiniGame, new CaseId("n11"), "objectives.ny13.cables"),
                new ObjectiveDef("obj_lieutenant", ObjectiveType.DefeatCombat, new CaseId("n14"), "objectives.ny13.lieutenant"),
            };
            var encounters = new List<EncounterDef>
            {
                new EncounterDef(CombatRank.Easy, new[] { new EnemyId("thug"), new EnemyId("sniper") }),
                new EncounterDef(CombatRank.Normal, new[] { new EnemyId("brute"), new EnemyId("sniper"), new EnemyId("street_pyro") }),
                new EncounterDef(CombatRank.Normal, new[] { new EnemyId("brute"), new EnemyId("thug"), new EnemyId("sniper") }),
                new EncounterDef(CombatRank.Elite, new[] { new EnemyId("canal_chief"), new EnemyId("brute"), new EnemyId("sniper") }),
                new EncounterDef(CombatRank.Boss, new[] { new EnemyId("bridge_titan"), new EnemyId("sniper"), new EnemyId("thug") }),
            };
            return new ZoneDef(Zone13, "zones.ny_1_3.name", new CaseId("n00"), nodes, links, objectives, encounters, "1.3", 1, "brooklyn", 8, Requires(Zone13), new ZoneEnvironment(Biome.City, Weather.Fog));
        }
    }
}
