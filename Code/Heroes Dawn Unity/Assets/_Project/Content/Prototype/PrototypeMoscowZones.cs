using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Content.Prototype
{
    /// <summary>
    /// Chapitre Sergueiv (Moscou, parcours d'Agent M) : quatre zones M1.1 à M1.4, déblocage linéaire, boss : le Passeur.
    /// Les plateaux reprennent les graphes éprouvés de Brooklyn (graphe A = Brownsville, graphe B = pont) ; spec 2026-11-13 §4.
    /// </summary>
    public static class PrototypeMoscowZones
    {
        public const string Borough = "sergueiv";

        public static readonly ZoneId Zone11 = new ZoneId("zone_mos_1_1");
        public static readonly ZoneId Zone12 = new ZoneId("zone_mos_1_2");
        public static readonly ZoneId Zone13 = new ZoneId("zone_mos_1_3");
        public static readonly ZoneId Zone14 = new ZoneId("zone_mos_1_4");

        public static IEnumerable<ZoneDef> Zones()
        {
            yield return BuildZone11();
            yield return BuildZone12();
            yield return BuildZone13();
            yield return BuildZone14();
        }

        /// <summary>Zones à valider avant d'ouvrir celle-ci : linéaire, M1.1 sans prérequis.</summary>
        public static IReadOnlyList<ZoneId> Requires(ZoneId zone)
        {
            if (zone == Zone12) return new[] { Zone11 };
            if (zone == Zone13) return new[] { Zone12 };
            if (zone == Zone14) return new[] { Zone13 };
            return System.Array.Empty<ZoneId>();
        }

        static NarrationLineDef L(string zone, string node, string phase, int i, string speaker) => PrototypeZones.L(zone, node, phase, i, speaker);

        static ObjectiveDef Obj(string id, ObjectiveType t, string node, string zoneKey, string key)
            => new ObjectiveDef(id, t, new CaseId(node), "objectives." + zoneKey + "." + key);

        static EncounterDef Enc(CombatRank r, params string[] enemies)
        {
            var ids = new EnemyId[enemies.Length];
            for (int i = 0; i < ids.Length; i++) ids[i] = new EnemyId(enemies[i]);
            return new EncounterDef(r, ids);
        }

        /// <summary>Gare du maglev : graphe A. L'homme de main garde le cargo scellé (n14) ; sortie par n19.</summary>
        static ZoneDef BuildZone11()
        {
            var enforcer = new NarrationDef(
                new[] { L("mos_1_1", "n14", "before", 0, "enforcer"), L("mos_1_1", "n14", "before", 1, "ilona_varga") },
                new[] { L("mos_1_1", "n14", "after", 0, "agent_m") });
            var exit = new NarrationDef(null, new[] { L("mos_1_1", "n19", "after", 0, "ilona_varga") });
            var objectives = new List<ObjectiveDef>
            {
                Obj("obj_welcome", ObjectiveType.ReachCase, "n03", "mos11", "welcome"),
                Obj("obj_platforms", ObjectiveType.CompleteMiniGame, "n13", "mos11", "platforms"),
                Obj("obj_escort", ObjectiveType.DefeatCombat, "n14", "mos11", "escort"),
                Obj("obj_leave", ObjectiveType.ReachCase, "n19", "mos11", "leave"),
            };
            var encounters = new List<EncounterDef>
            {
                Enc(CombatRank.Easy, "mos_guard", "mos_guard"),
                Enc(CombatRank.Easy, "mos_smuggler", "thug"),
                Enc(CombatRank.Normal, "brute", "mos_smuggler", "mos_guard"),
                Enc(CombatRank.Elite, "mos_enforcer", "mos_guard", "mos_smuggler"),
            };
            return new ZoneDef(Zone11, "zones.mos_1_1.name", new CaseId("n00"), PrototypeZones.BrownsvilleNodes(enforcer, exit, "infiltration_camp", true), PrototypeZones.BrownsvilleLinks(),
                               objectives, encounters, "M1.1", 1, Borough, 5, Requires(Zone11), new ZoneEnvironment(Biome.City, Weather.Day));
        }

        /// <summary>Monastère-archives : graphe B sans boss. Gardien des archives (n14), registre à emporter (n18).</summary>
        static ZoneDef BuildZone12()
        {
            var guardian = new NarrationDef(
                new[] { L("mos_1_2", "n14", "before", 0, "enforcer"), L("mos_1_2", "n14", "before", 1, "agent_m") },
                new[] { L("mos_1_2", "n14", "after", 0, "agent_m") });
            var exit = new NarrationDef(null, new[] { L("mos_1_2", "n18", "after", 0, "ilona_varga") });
            var objectives = new List<ObjectiveDef>
            {
                Obj("obj_archives", ObjectiveType.ReachCase, "n04", "mos12", "archives"),
                Obj("obj_fresco", ObjectiveType.CompleteMiniGame, "n11", "mos12", "fresco"),
                Obj("obj_guardian", ObjectiveType.DefeatCombat, "n14", "mos12", "guardian"),
                Obj("obj_register", ObjectiveType.ReachCase, "n18", "mos12", "register"),
            };
            var encounters = new List<EncounterDef>
            {
                Enc(CombatRank.Easy, "mos_guard", "mos_guard"),
                Enc(CombatRank.Normal, "mos_guard", "mos_smuggler", "mos_smuggler"),
                Enc(CombatRank.Elite, "mos_enforcer", "mos_smuggler", "mos_smuggler"), // spec : + thug ; deux tireurs pour tenir 60-85 % (tâche 8)
            };
            return new ZoneDef(Zone12, "zones.mos_1_2.name", new CaseId("n00"), PrototypeZones.BridgeNodes(null, guardian, exit, "gears_workshop", false, true), PrototypeZones.BridgeLinks(),
                               objectives, encounters, "M1.2", 1, Borough, 6, Requires(Zone12), new ZoneEnvironment(Biome.City, Weather.Fog));
        }

        /// <summary>Voie du maglev : graphe B sans boss. Rattraper le convoi (n11), briser l'escorte (n14), ouvrir les caisses (n18).</summary>
        static ZoneDef BuildZone13()
        {
            var escort = new NarrationDef(
                new[] { L("mos_1_3", "n14", "before", 0, "enforcer") },
                new[] { L("mos_1_3", "n14", "after", 0, "agent_m") });
            var exit = new NarrationDef(null, new[] { L("mos_1_3", "n18", "after", 0, "ilona_varga") });
            var objectives = new List<ObjectiveDef>
            {
                Obj("obj_convoy", ObjectiveType.CompleteMiniGame, "n11", "mos13", "convoy"),
                Obj("obj_escort", ObjectiveType.DefeatCombat, "n14", "mos13", "escort"),
                Obj("obj_crates", ObjectiveType.ReachCase, "n18", "mos13", "crates"),
            };
            var encounters = new List<EncounterDef>
            {
                Enc(CombatRank.Easy, "thug", "mos_smuggler"),
                Enc(CombatRank.Normal, "brute", "mos_smuggler", "street_pyro"),
                Enc(CombatRank.Elite, "mos_enforcer", "brute", "mos_smuggler"),
            };
            return new ZoneDef(Zone13, "zones.mos_1_3.name", new CaseId("n00"), PrototypeZones.BridgeNodes(null, escort, exit, "rooftop_rescue", false), PrototypeZones.BridgeLinks(),
                               objectives, encounters, "M1.3", 1, Borough, 8, Requires(Zone13), new ZoneEnvironment(Biome.City, Weather.Night));
        }

        /// <summary>Dépôt du maglev : graphe B avec boss fixe au bout, le Passeur (avant, après, fin de chapitre).</summary>
        static ZoneDef BuildZone14()
        {
            var passeur = new NarrationDef(
                new[] { L("mos_1_4", "n18", "before", 0, "passeur"), L("mos_1_4", "n18", "before", 1, "passeur"), L("mos_1_4", "n18", "before", 2, "ilona_varga") },
                new[] { L("mos_1_4", "n18", "after", 0, "passeur"), L("mos_1_4", "n18", "after", 1, "agent_m"), L("mos_1_4", "n18", "after", 2, "ilona_varga") });
            var objectives = new List<ObjectiveDef>
            {
                Obj("obj_depot", ObjectiveType.CompleteMiniGame, "n11", "mos14", "depot"),
                Obj("obj_guard", ObjectiveType.DefeatCombat, "n14", "mos14", "guard"),
                Obj("obj_passeur", ObjectiveType.DefeatCombat, "n18", "mos14", "passeur"),
            };
            var encounters = new List<EncounterDef>
            {
                Enc(CombatRank.Easy, "mos_guard", "thug"),
                Enc(CombatRank.Normal, "mos_guard", "mos_smuggler", "brute"),
                Enc(CombatRank.Elite, "mos_enforcer", "mos_guard", "mos_guard"),
                Enc(CombatRank.Boss, "mos_passeur", "mos_smuggler", "mos_guard"),
            };
            return new ZoneDef(Zone14, "zones.mos_1_4.name", new CaseId("n00"), PrototypeZones.BridgeNodes(passeur, null, null, "crate_pick_depot", true), PrototypeZones.BridgeLinks(),
                               objectives, encounters, "M1.4", 1, Borough, 10, Requires(Zone14), new ZoneEnvironment(Biome.City, Weather.Snow));
        }
    }
}
