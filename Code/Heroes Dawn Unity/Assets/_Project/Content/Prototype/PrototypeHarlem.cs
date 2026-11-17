using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Content.Prototype
{
    /// <summary>
    /// Chapitre Harlem (New York, chapitre 3, « coût humain et choix marquant ») : quatre zones 3.1 à 3.4, déblocage linéaire, boss : Dante Reyes dit « Braise ».
    /// Graphes : 3.1 Boucle (D), 3.2 Pont (B sans boss), 3.3 Brownsville (A), 3.4 Trois voies (C avec boss). Le choix de 3.3 (clinique ou marché) n'a pas de mécanique de plateau :
    /// il est exprimé dans les textes, l'épilogue variera plus tard.
    /// </summary>
    public static class PrototypeHarlem
    {
        public const string Borough = "harlem";

        public static readonly ZoneId Zone31 = new ZoneId("zone_ny_3_1");
        public static readonly ZoneId Zone32 = new ZoneId("zone_ny_3_2");
        public static readonly ZoneId Zone33 = new ZoneId("zone_ny_3_3");
        public static readonly ZoneId Zone34 = new ZoneId("zone_ny_3_4");
        static readonly ZoneId PreviousChapterLast = new ZoneId("zone_ny_2_4");

        public static IEnumerable<ZoneDef> Zones()
        {
            yield return BuildZone31();
            yield return BuildZone32();
            yield return BuildZone33();
            yield return BuildZone34();
        }

        /// <summary>Déblocage linéaire : 3.1 exige la dernière zone de Queens (2.4).</summary>
        public static IReadOnlyList<ZoneId> Requires(ZoneId zone)
        {
            if (zone == Zone31) return new[] { PreviousChapterLast };
            if (zone == Zone32) return new[] { Zone31 };
            if (zone == Zone33) return new[] { Zone32 };
            if (zone == Zone34) return new[] { Zone33 };
            return System.Array.Empty<ZoneId>();
        }

        /// <summary>Réplique du script New York : clé <c>narration.&lt;zone&gt;.&lt;slot&gt;.&lt;i&gt;</c> (ex. <c>ny_3_1</c>, <c>elite.before</c>).</summary>
        static NarrationLineDef L(string speaker, string zone, string slot, int i) => new NarrationLineDef(speaker, "narration." + zone + "." + slot + "." + i);

        /// <summary>Variante de parcours (<c>.d</c> / <c>.m</c>) : remplace la réplique de base <paramref name="replaces"/> (null si elle est réservée à ce parcours).</summary>
        static NarrationLineDef V(string speaker, string zone, string slot, int i, string suffix, string route, bool replacesBase)
            => new NarrationLineDef(speaker, "narration." + zone + "." + slot + "." + i + "." + suffix, route, replacesBase ? "narration." + zone + "." + slot + "." + i : null);

        /// <summary>Réplique réservée à un parcours (clé sans suffixe, ex. rappel personnel d'Agent D).</summary>
        static NarrationLineDef R(string speaker, string zone, string slot, int i, string route)
            => new NarrationLineDef(speaker, "narration." + zone + "." + slot + "." + i, route);

        static ObjectiveDef Obj(string id, ObjectiveType t, string node, string zoneKey, string key)
            => new ObjectiveDef(id, t, new CaseId(node), "objectives." + zoneKey + "." + key);

        static EncounterDef Enc(CombatRank r, params string[] enemies)
        {
            var ids = new EnemyId[enemies.Length];
            for (int i = 0; i < ids.Length; i++) ids[i] = new EnemyId(enemies[i]);
            return new EncounterDef(r, ids);
        }

        /// <summary>Bloc incendié : graphe D. Évacuer (n11 et n20), affronter le lieutenant incendiaire (n13), sortir avant l'effondrement (n21).</summary>
        static ZoneDef BuildZone31()
        {
            var intro = new NarrationDef(new[]
            {
                L("narrateur", "ny_3_1", "intro", 0), L("giulia_rossi", "ny_3_1", "intro", 1), L("habitant", "ny_3_1", "intro", 2),
                L("agent_d", "ny_3_1", "intro", 3), V("agent_m", "ny_3_1", "intro", 3, "m", "agent_m", true), L("lucas_mendes", "ny_3_1", "intro", 4),
            }, null);
            var arsonist = new NarrationDef(
                new[] { L("enforcer", "ny_3_1", "elite.before", 0), L("ilona_varga", "ny_3_1", "elite.before", 1) },
                new[] { L("giulia_rossi", "ny_3_1", "elite.after", 0) });
            var exit = new NarrationDef(null, new[] { L("habitant", "ny_3_1", "exit", 0), L("ilona_varga", "ny_3_1", "exit", 1), R("agent_d", "ny_3_1", "exit", 2, "agent_d") });
            var objectives = new List<ObjectiveDef>
            {
                Obj("obj_fire", ObjectiveType.ReachCase, "n03", "ny31", "fire"),
                Obj("obj_evacuate", ObjectiveType.CompleteMiniGame, "n11", "ny31", "evacuate"),
                Obj("obj_arsonist", ObjectiveType.DefeatCombat, "n13", "ny31", "arsonist"),
                Obj("obj_roofs", ObjectiveType.CompleteMiniGame, "n20", "ny31", "roofs"),
            };
            var encounters = new List<EncounterDef>
            {
                Enc(CombatRank.Easy, "hrl_gangster", "hrl_roofer"),
                Enc(CombatRank.Easy, "hrl_gangster", "thug"),
                Enc(CombatRank.Normal, "hrl_gangster", "hrl_torch", "hrl_roofer"),
                Enc(CombatRank.Elite, "hrl_lieutenant", "hrl_torch", "hrl_gangster"),
            };
            return new ZoneDef(Zone31, "zones.ny_3_1.name", new CaseId("n00"), ChapterGraphs.LoopNodes(arsonist, exit, "rescue_tool", "rooftop_rescue", false, intro), ChapterGraphs.LoopLinks(),
                               objectives, encounters, "3.1", 3, Borough, 18, Requires(Zone31), new ZoneEnvironment(Biome.City, Weather.Night));
        }

        /// <summary>Clinique de fortune : graphe B sans boss. Entrer (n04), rassembler les soins (n11), tenir face à l'assaut (n14), tenir jusqu'au bout (n18).</summary>
        static ZoneDef BuildZone32()
        {
            var intro = new NarrationDef(new[]
            {
                L("narrateur", "ny_3_2", "intro", 0), L("giulia_rossi", "ny_3_2", "intro", 1), L("dr_colby", "ny_3_2", "intro", 2),
                L("ilona_varga", "ny_3_2", "intro", 3), L("agent_d", "ny_3_2", "intro", 4), V("agent_m", "ny_3_2", "intro", 4, "m", "agent_m", true),
            }, null);
            var assault = new NarrationDef(
                new[] { L("enforcer", "ny_3_2", "elite.before", 0), L("dr_colby", "ny_3_2", "elite.before", 1) },
                new[] { L("lucas_mendes", "ny_3_2", "elite.after", 0) });
            var exit = new NarrationDef(null, new[] { L("dr_colby", "ny_3_2", "exit", 0), L("dr_colby", "ny_3_2", "exit", 1) });
            var objectives = new List<ObjectiveDef>
            {
                Obj("obj_clinic", ObjectiveType.ReachCase, "n04", "ny32", "clinic"),
                Obj("obj_supplies", ObjectiveType.CompleteMiniGame, "n11", "ny32", "supplies"),
                Obj("obj_assault", ObjectiveType.DefeatCombat, "n14", "ny32", "assault"),
                Obj("obj_hold", ObjectiveType.ReachCase, "n18", "ny32", "hold"),
            };
            var encounters = new List<EncounterDef>
            {
                Enc(CombatRank.Easy, "hrl_gangster", "hrl_gangster"),
                Enc(CombatRank.Normal, "hrl_gangster", "hrl_roofer", "hrl_torch"),
                Enc(CombatRank.Normal, "brute", "hrl_roofer", "hrl_roofer"),
                Enc(CombatRank.Elite, "hrl_lieutenant", "hrl_gangster", "hrl_roofer"),
            };
            return new ZoneDef(Zone32, "zones.ny_3_2.name", new CaseId("n00"), PrototypeZones.BridgeNodes(null, assault, exit, "dna_lab", false, true, intro), PrototypeZones.BridgeLinks(),
                               objectives, encounters, "3.2", 3, Borough, 19, Requires(Zone32), new ZoneEnvironment(Biome.City, Weather.Rain));
        }

        /// <summary>Marché de rue : graphe A. Le dilemme (clinique ou marché) est exprimé dans la description et les répliques ; aucune mécanique de choix.</summary>
        static ZoneDef BuildZone33()
        {
            // Le script ne prévoit aucune scène d'élite en 3.3 (slots intro, choice, branch_*, exit seulement).
            var intro = new NarrationDef(new[]
            {
                L("narrateur", "ny_3_3", "intro", 0), L("commercant", "ny_3_3", "intro", 1),
                L("lucas_mendes", "ny_3_3", "intro", 2), L("giulia_rossi", "ny_3_3", "intro", 3),
            }, null);
            var exit = new NarrationDef(null, new[] { L("giulia_rossi", "ny_3_3", "exit", 0), L("ilona_varga", "ny_3_3", "exit", 1) });
            var objectives = new List<ObjectiveDef>
            {
                Obj("obj_market", ObjectiveType.ReachCase, "n03", "ny33", "market"),
                Obj("obj_stalls", ObjectiveType.CompleteMiniGame, "n13", "ny33", "stalls"),
                Obj("obj_raiders", ObjectiveType.DefeatCombat, "n14", "ny33", "raiders"),
                Obj("obj_priority", ObjectiveType.ReachCase, "n19", "ny33", "priority"),
            };
            var encounters = new List<EncounterDef>
            {
                Enc(CombatRank.Easy, "hrl_gangster", "hrl_roofer"),
                Enc(CombatRank.Easy, "thug", "hrl_roofer"),
                Enc(CombatRank.Normal, "hrl_gangster", "hrl_torch", "thug"),
                Enc(CombatRank.Normal, "brute", "hrl_roofer", "hrl_torch"),
                Enc(CombatRank.Elite, "hrl_lieutenant", "hrl_roofer", "hrl_gangster"),
            };
            return new ZoneDef(Zone33, "zones.ny_3_3.name", new CaseId("n00"), PrototypeZones.BrownsvilleNodes(null, exit, "crate_pick_depot", true, intro), PrototypeZones.BrownsvilleLinks(),
                               objectives, encounters, "3.3", 3, Borough, 21, Requires(Zone33), new ZoneEnvironment(Biome.City, Weather.Day));
        }

        /// <summary>
        /// Rue de l'atelier : graphe C avec boss, Braise (n22). Dialogues alignés sur le script (13_Script_New_York.md).
        /// </summary>
        static ZoneDef BuildZone34()
        {
            var intro = new NarrationDef(new[]
            {
                L("narrateur", "ny_3_4", "intro", 0), L("lucas_mendes", "ny_3_4", "intro", 1), L("giulia_rossi", "ny_3_4", "intro", 2),
            }, null);
            var sidewalk = new NarrationDef(
                new[] { L("enforcer", "ny_3_4", "elite.before", 0) },
                new[] { L("ilona_varga", "ny_3_4", "elite.after", 0) });
            // Case de fin (n22) : boss.before ; boss.after puis chapter_end. Phases de boss et épilogues non branchés.
            var braise = new NarrationDef(
                new[] { L("braise", "ny_3_4", "boss.before", 0), L("braise", "ny_3_4", "boss.before", 1), L("braise", "ny_3_4", "boss.before", 2), L("ilona_varga", "ny_3_4", "boss.before", 3),
                       V("agent_d", "ny_3_4", "boss.before", 4, "d", "agent_d", false), V("habitant", "ny_3_4", "boss.before", 4, "m", "agent_m", false) },
                new[] { L("braise", "ny_3_4", "boss.after", 0), L("ilona_varga", "ny_3_4", "boss.after", 1),
                        V("agent_d", "ny_3_4", "boss.after", 2, "d", "agent_d", false), V("agent_m", "ny_3_4", "boss.after", 2, "m", "agent_m", false),
                        L("narrateur", "ny_3_4", "chapter_end", 0) });
            var objectives = new List<ObjectiveDef>
            {
                Obj("obj_chase", ObjectiveType.CompleteMiniGame, "n13", "ny34", "chase"),
                Obj("obj_sidewalk", ObjectiveType.DefeatCombat, "n14", "ny34", "sidewalk"),
                Obj("obj_workshop", ObjectiveType.CompleteMiniGame, "n21", "ny34", "workshop"),
                Obj("obj_braise", ObjectiveType.DefeatCombat, "n22", "ny34", "braise"),
            };
            var encounters = new List<EncounterDef>
            {
                Enc(CombatRank.Easy, "hrl_gangster", "hrl_roofer"),
                Enc(CombatRank.Normal, "hrl_gangster", "hrl_torch", "hrl_roofer"),
                Enc(CombatRank.Normal, "brute", "hrl_torch", "hrl_roofer"),
                Enc(CombatRank.Elite, "hrl_lieutenant", "hrl_torch", "hrl_gangster"),
                Enc(CombatRank.Boss, "hrl_braise", "hrl_roofer", "hrl_gangster"),
            };
            return new ZoneDef(Zone34, "zones.ny_3_4.name", new CaseId("n00"), ChapterGraphs.ThreeLanesNodes(sidewalk, braise, "rhythm_rooftop", "gears_workshop", true, intro), ChapterGraphs.ThreeLanesLinks(),
                               objectives, encounters, "3.4", 3, Borough, 23, Requires(Zone34), new ZoneEnvironment(Biome.Industrial, Weather.Fog));
        }

        static EffectDef Dmg(int pct) => PrototypeZones.Dmg(pct);
        static EffectDef St(StatusType s, int turns, int chance = 100) => PrototypeZones.St(s, turns, chance);
        static EffectDef Shield(int pct, int turns = 2) => PrototypeZones.Shield(pct, turns);
        static SkillDef S(string id, SkillSlot slot, int cd, TargetRule t, params EffectDef[] fx) => PrototypeZones.S(id, slot, cd, t, fx);

        /// <summary>Ennemis de Harlem : homme de main (front), incendiaire (attaquant, brûlure modérée), tireur de toit (tir), lieutenant (élite), Braise (boss).</summary>
        public static IEnumerable<EnemyDef> Enemies()
        {
            yield return PrototypeZones.E("hrl_gangster", Position.FrontLine, Family.PhysicalBio, new StatBlock(1600, 118, 95, 98), false,
                S("hrl_gangster_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(100)),
                S("hrl_gangster_a2", SkillSlot.A2, 4, TargetRule.Self, Shield(15), St(StatusType.Taunt, 1)));
            yield return PrototypeZones.E("hrl_torch", Position.Attacker, Family.Elemental, new StatBlock(1000, 135, 65, 105), false,
                S("hrl_torch_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(95), St(StatusType.Burn, 2, 30)),
                S("hrl_torch_a2", SkillSlot.A2, 4, TargetRule.AllEnemies, Dmg(65), St(StatusType.Burn, 1, 25)));
            yield return PrototypeZones.E("hrl_roofer", Position.Ranged, Family.Perception, new StatBlock(900, 135, 60, 110), false,
                S("hrl_roofer_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(105)),
                S("hrl_roofer_a2", SkillSlot.A2, 4, TargetRule.SingleEnemy, Dmg(170), St(StatusType.Mark, 2)));
            yield return PrototypeZones.E("hrl_lieutenant", Position.Attacker, Family.Elemental, new StatBlock(3000, 600, 110, 100), false,
                S("hrl_lieutenant_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(110), St(StatusType.Burn, 2, 25)),
                S("hrl_lieutenant_a2", SkillSlot.A2, 3, TargetRule.AllEnemies, Dmg(75), St(StatusType.Burn, 1, 30)),
                S("hrl_lieutenant_a3", SkillSlot.A3, 5, TargetRule.Self, Shield(20)));
            // Boss : Dante Reyes, « Braise ». Phase 1 « Étincelle » ; sous 66 % « Brasier » (A2 remplacée par un coup unique brûlant) ; sous 33 % « Cendres ».
            yield return PrototypeZones.Styled(PrototypeZones.Phased(PrototypeZones.E("hrl_braise", Position.Attacker, Family.Elemental, new StatBlock(5800, 800, 130, 100), true,
                S("hrl_braise_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(115), St(StatusType.Burn, 2, 35)),
                S("hrl_braise_a2", SkillSlot.A2, 3, TargetRule.Self, Shield(20), St(StatusType.Taunt, 1)),
                S("hrl_braise_a3", SkillSlot.A3, 5, TargetRule.AllEnemies, Dmg(85), St(StatusType.Burn, 2, 30)),
                S("hrl_braise_a4", SkillSlot.A4, 4, TargetRule.SingleEnemy, Dmg(80), St(StatusType.Mark, 2))),
                PrototypeZones.Phase(66, "enemies.hrl_braise.phase2", 10, 0, 5,
                    new[]
                    {
                        S("hrl_braise_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(115), St(StatusType.Burn, 2, 35)),
                        S("hrl_braise_p2_a2", SkillSlot.A2, 3, TargetRule.SingleEnemy, Dmg(150), St(StatusType.Burn, 2, 50)),
                        S("hrl_braise_a3", SkillSlot.A3, 5, TargetRule.AllEnemies, Dmg(85), St(StatusType.Burn, 2, 30)),
                        S("hrl_braise_a4", SkillSlot.A4, 4, TargetRule.SingleEnemy, Dmg(80), St(StatusType.Mark, 2)),
                    }, AiStyle.Aggressive, new[] { L("braise", "ny_3_4", "boss.phase1", 0) }),
                PrototypeZones.Phase(33, "enemies.hrl_braise.phase3", 25, -10, 10, lines: new[] { L("braise", "ny_3_4", "boss.phase2", 0) })), AiStyle.Aggressive);
        }
    }
}
