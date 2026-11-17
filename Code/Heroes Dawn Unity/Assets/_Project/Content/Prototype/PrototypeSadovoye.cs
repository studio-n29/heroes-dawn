using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Content.Prototype
{
    /// <summary>
    /// Chapitre Sadovoye Koltso (Moscou, chapitre 3, « le pouvoir se met en scène ») : quatre zones M3.1 à M3.4, déblocage linéaire,
    /// la première exige Kapotnya (zone_mos_2_4). Boss : Dmitri Orlan. Graphes : C (3.1), A (3.2), B sans boss (3.3), D avec boss (3.4).
    /// </summary>
    public static class PrototypeSadovoye
    {
        public const string Borough = "sadovoye";

        public static readonly ZoneId Zone31 = new ZoneId("zone_mos_3_1");
        public static readonly ZoneId Zone32 = new ZoneId("zone_mos_3_2");
        public static readonly ZoneId Zone33 = new ZoneId("zone_mos_3_3");
        public static readonly ZoneId Zone34 = new ZoneId("zone_mos_3_4");

        static readonly ZoneId PreviousChapterLast = new ZoneId("zone_mos_2_4");

        public static IEnumerable<ZoneDef> Zones()
        {
            yield return BuildZone31();
            yield return BuildZone32();
            yield return BuildZone33();
            yield return BuildZone34();
        }

        /// <summary>Zones à valider avant d'ouvrir celle-ci : M3.1 exige Kapotnya (M2.4), puis déblocage linéaire.</summary>
        public static IReadOnlyList<ZoneId> Requires(ZoneId zone)
        {
            if (zone == Zone31) return new[] { PreviousChapterLast };
            if (zone == Zone32) return new[] { Zone31 };
            if (zone == Zone33) return new[] { Zone32 };
            if (zone == Zone34) return new[] { Zone33 };
            return System.Array.Empty<ZoneId>();
        }

        /// <summary>Lignes d'un slot du script : locuteurs dans l'ordre, clés <c>narration.&lt;zone&gt;.&lt;slot&gt;.0..n</c> (ex. « mos_3_1.elite.before »).</summary>
        static NarrationLineDef[] Lines(string zoneSlot, params string[] speakers)
        {
            var r = new NarrationLineDef[speakers.Length];
            for (int i = 0; i < r.Length; i++) r[i] = new NarrationLineDef(speakers[i], "narration." + zoneSlot + "." + i);
            return r;
        }

        static NarrationLineDef L(string speaker, string key, string route = null, string replaces = null) => new NarrationLineDef(speaker, key, route, replaces);

        static ObjectiveDef Obj(string id, ObjectiveType t, string node, string zoneKey, string key)
            => new ObjectiveDef(id, t, new CaseId(node), "objectives." + zoneKey + "." + key);

        static EncounterDef Enc(CombatRank r, params string[] enemies)
        {
            var ids = new EnemyId[enemies.Length];
            for (int i = 0; i < ids.Length; i++) ids[i] = new EnemyId(enemies[i]);
            return new EncounterDef(r, ids);
        }

        /// <summary>Tours de verre : graphe C. Le maître de salon garde le salon des trophées (n14) ; on y accède par les étages privés (n04) ; sortie en n22.</summary>
        static ZoneDef BuildZone31()
        {
            var intro = new NarrationDef(new[]
            {
                L("narrateur", "narration.mos_3_1.intro.0"), L("giulia_rossi", "narration.mos_3_1.intro.1"), L("lucas_mendes", "narration.mos_3_1.intro.2"),
                L("agent_m", "narration.mos_3_1.intro.3"),
                L("agent_d", "narration.mos_3_1.intro.3.d", "agent_d", "narration.mos_3_1.intro.3"),
            }, null);
            var maitre = new NarrationDef(
                Lines("mos_3_1.elite.before", "enforcer", "ilona_varga"),
                Lines("mos_3_1.elite.after", "giulia_rossi"));
            var exit = new NarrationDef(null, new[]
            {
                L("agent_m", "narration.mos_3_1.exit.0"), L("ilona_varga", "narration.mos_3_1.exit.1"),
                L("agent_m", "narration.mos_3_1.exit.2", "agent_m"),
            });
            var objectives = new List<ObjectiveDef>
            {
                Obj("obj_floors", ObjectiveType.ReachCase, "n04", "mos31", "floors"),
                Obj("obj_badge", ObjectiveType.CompleteMiniGame, "n13", "mos31", "badge"),
                Obj("obj_salon", ObjectiveType.DefeatCombat, "n14", "mos31", "salon"),
                Obj("obj_trophies", ObjectiveType.CompleteMiniGame, "n21", "mos31", "trophies"),
            };
            var encounters = new List<EncounterDef>
            {
                Enc(CombatRank.Easy, "sad_guard", "sad_guard"),
                Enc(CombatRank.Easy, "sad_agent", "mos_guard"),
                Enc(CombatRank.Normal, "sad_guard", "sad_agent", "sad_operator"),
                Enc(CombatRank.Elite, "sad_majordome", "sad_guard", "sad_agent"),
            };
            return new ZoneDef(Zone31, "zones.mos_3_1.name", new CaseId("n00"), ChapterGraphs.ThreeLanesNodes(maitre, exit, "dialogue_dockers", "dna_lab", false, intro), ChapterGraphs.ThreeLanesLinks(),
                               objectives, encounters, "M3.1", 3, Borough, 18, Requires(Zone31), new ZoneEnvironment(Biome.City, Weather.Day));
        }

        /// <summary>Enchères privées : graphe A. La salle de vente (n03), perturber les enchères (n13), garde du lot (n14), le lot Apex (n19).</summary>
        static ZoneDef BuildZone32()
        {
            var intro = new NarrationDef(new[]
            {
                L("narrateur", "narration.mos_3_2.intro.0"), L("commissaire", "narration.mos_3_2.intro.1"), L("giulia_rossi", "narration.mos_3_2.intro.2"),
                L("agent_m", "narration.mos_3_2.intro.3"),
                L("agent_d", "narration.mos_3_2.intro.3.d", "agent_d", "narration.mos_3_2.intro.3"),
            }, null);
            var lot = new NarrationDef(
                Lines("mos_3_2.elite.before", "enforcer", "ilona_varga"),
                Lines("mos_3_2.elite.after", "lucas_mendes"));
            var exit = new NarrationDef(null, Lines("mos_3_2.exit", "commissaire", "ilona_varga"));
            var objectives = new List<ObjectiveDef>
            {
                Obj("obj_hall", ObjectiveType.ReachCase, "n03", "mos32", "hall"),
                Obj("obj_bids", ObjectiveType.CompleteMiniGame, "n13", "mos32", "bids"),
                Obj("obj_lot", ObjectiveType.DefeatCombat, "n14", "mos32", "lot"),
                Obj("obj_apex", ObjectiveType.ReachCase, "n19", "mos32", "apex"),
            };
            var encounters = new List<EncounterDef>
            {
                Enc(CombatRank.Easy, "sad_agent", "sad_guard"),
                Enc(CombatRank.Easy, "sad_operator", "mos_guard"),
                Enc(CombatRank.Normal, "sad_guard", "sad_agent", "mos_smuggler"),
                Enc(CombatRank.Elite, "mos_enforcer", "sad_guard", "sad_agent"),
            };
            return new ZoneDef(Zone32, "zones.mos_3_2.name", new CaseId("n00"), PrototypeZones.BrownsvilleNodes(lot, exit, "free_throw_court", true, intro), PrototypeZones.BrownsvilleLinks(),
                               objectives, encounters, "M3.2", 3, Borough, 19, Requires(Zone32), new ZoneEnvironment(Biome.City, Weather.Night));
        }

        /// <summary>Studio d'hologrammes : graphe B sans boss. La régie (n04), la synchro des projecteurs (n11), le technicien (n14), la vidéo à démonter (n18).</summary>
        static ZoneDef BuildZone33()
        {
            var intro = new NarrationDef(new[]
            {
                L("narrateur", "narration.mos_3_3.intro.0"), L("technicien_holo", "narration.mos_3_3.intro.1"), L("lucas_mendes", "narration.mos_3_3.intro.2"),
                L("agent_m", "narration.mos_3_3.intro.3"),
                L("agent_d", "narration.mos_3_3.intro.3.d", "agent_d", "narration.mos_3_3.intro.3"),
            }, null);
            var technicien = new NarrationDef(
                Lines("mos_3_3.elite.before", "enforcer"),
                Lines("mos_3_3.elite.after", "giulia_rossi"));
            var exit = new NarrationDef(null, Lines("mos_3_3.exit", "ilona_varga", "agent_m"));
            var objectives = new List<ObjectiveDef>
            {
                Obj("obj_control", ObjectiveType.ReachCase, "n04", "mos33", "control"),
                Obj("obj_sync", ObjectiveType.CompleteMiniGame, "n11", "mos33", "sync"),
                Obj("obj_operator", ObjectiveType.DefeatCombat, "n14", "mos33", "operator"),
                Obj("obj_fake", ObjectiveType.ReachCase, "n18", "mos33", "fake"),
            };
            var encounters = new List<EncounterDef>
            {
                Enc(CombatRank.Easy, "sad_operator", "sad_guard"),
                Enc(CombatRank.Normal, "sad_operator", "sad_agent", "sad_guard"),
                Enc(CombatRank.Normal, "sad_operator", "sad_operator", "mos_guard"),
                Enc(CombatRank.Elite, "mos_enforcer", "sad_operator", "sad_operator"),
            };
            return new ZoneDef(Zone33, "zones.mos_3_3.name", new CaseId("n00"), PrototypeZones.BridgeNodes(null, technicien, exit, "rhythm_rooftop", false, true, intro), PrototypeZones.BridgeLinks(),
                               objectives, encounters, "M3.3", 3, Borough, 21, Requires(Zone33), new ZoneEnvironment(Biome.Industrial, Weather.Fog));
        }

        /// <summary>Tour Orlan : graphe D avec boss fixe au bout, Dmitri Orlan (avant, après, fin de chapitre).</summary>
        static ZoneDef BuildZone34()
        {
            var intro = new NarrationDef(Lines("mos_3_4.intro", "narrateur", "giulia_rossi"), null);
            var garde = new NarrationDef(
                Lines("mos_3_4.elite.before", "enforcer"),
                Lines("mos_3_4.elite.after", "ilona_varga"));
            // Case du boss (n21) : boss.after puis chapter_end dans la partie After.
            var afterLines = new List<NarrationLineDef>(Lines("mos_3_4.boss.after", "orlan", "agent_m", "orlan"));
            afterLines.Add(L("orlan", "narration.mos_3_4.boss.after.3", "agent_m"));
            afterLines.AddRange(Lines("mos_3_4.chapter_end", "narrateur"));
            var orlan = new NarrationDef(
                new[]
                {
                    L("orlan", "narration.mos_3_4.boss.before.0"),
                    L("orlan", "narration.mos_3_4.boss.before.1"),
                    L("orlan", "narration.mos_3_4.boss.before.1.d", "agent_d", "narration.mos_3_4.boss.before.1"),
                    L("agent_m", "narration.mos_3_4.boss.before.2"),
                    L("agent_d", "narration.mos_3_4.boss.before.2.d", "agent_d", "narration.mos_3_4.boss.before.2"),
                    L("orlan", "narration.mos_3_4.boss.before.3"),
                },
                afterLines.ToArray());
            var objectives = new List<ObjectiveDef>
            {
                Obj("obj_vault", ObjectiveType.CompleteMiniGame, "n11", "mos34", "vault"),
                Obj("obj_guard", ObjectiveType.DefeatCombat, "n13", "mos34", "guard"),
                Obj("obj_office", ObjectiveType.CompleteMiniGame, "n20", "mos34", "office"),
                Obj("obj_orlan", ObjectiveType.DefeatCombat, "n21", "mos34", "orlan"),
            };
            var encounters = new List<EncounterDef>
            {
                Enc(CombatRank.Easy, "sad_guard", "sad_operator"),
                Enc(CombatRank.Normal, "sad_guard", "sad_agent", "sad_operator"),
                Enc(CombatRank.Elite, "sad_majordome", "sad_guard", "sad_operator"),
                Enc(CombatRank.Boss, "sad_orlan", "sad_agent", "sad_guard"),
            };
            return new ZoneDef(Zone34, "zones.mos_3_4.name", new CaseId("n00"), ChapterGraphs.LoopNodes(garde, orlan, "rescue_tool", "arena_training", true, intro), ChapterGraphs.LoopLinks(),
                               objectives, encounters, "M3.4", 3, Borough, 23, Requires(Zone34), new ZoneEnvironment(Biome.City, Weather.Rain));
        }

        static EffectDef Dmg(int pct) => PrototypeZones.Dmg(pct);
        static EffectDef St(StatusType s, int turns, int chance = 100) => PrototypeZones.St(s, turns, chance);
        static EffectDef Shield(int pct, int turns = 2) => PrototypeZones.Shield(pct, turns);
        static SkillDef S(string id, SkillSlot slot, int cd, TargetRule t, params EffectDef[] fx) => PrototypeZones.S(id, slot, cd, t, fx);

        /// <summary>Garde privé (front), agent d'enchères (tireur), opérateur d'hologrammes (attaquant), majordome (élite), Dmitri Orlan (boss).</summary>
        public static IEnumerable<EnemyDef> Enemies()
        {
            yield return PrototypeZones.E("sad_guard", Position.FrontLine, Family.Matter, new StatBlock(1800, 120, 115, 100), false,
                S("sad_guard_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(100)),
                S("sad_guard_a2", SkillSlot.A2, 4, TargetRule.Self, Shield(15), St(StatusType.Taunt, 1)));
            yield return PrototypeZones.E("sad_agent", Position.Ranged, Family.Perception, new StatBlock(1000, 140, 65, 110), false,
                S("sad_agent_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(105)),
                S("sad_agent_a2", SkillSlot.A2, 4, TargetRule.SingleEnemy, Dmg(155), St(StatusType.Mark, 2)));
            yield return PrototypeZones.E("sad_operator", Position.Attacker, Family.Elemental, new StatBlock(1300, 135, 80, 106), false,
                S("sad_operator_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(110)),
                S("sad_operator_a2", SkillSlot.A2, 4, TargetRule.AllEnemies, Dmg(70), St(StatusType.Blind, 1, 40)));
            yield return PrototypeZones.E("sad_majordome", Position.Attacker, Family.PhysicalBio, new StatBlock(3300, 600, 120, 100), false,
                S("sad_majordome_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(110)),
                S("sad_majordome_a2", SkillSlot.A2, 3, TargetRule.AllEnemies, Dmg(80), St(StatusType.Vulnerable, 2, 35)),
                S("sad_majordome_a3", SkillSlot.A3, 5, TargetRule.Self, Shield(20)));
            // Boss : Dmitri Orlan. Phase 1 « Hospitalité » ; sous 66 % « Mise aux enchères » (A2 remplacée) ; sous 33 % « Dernière offre ».
            yield return PrototypeZones.Styled(PrototypeZones.Phased(PrototypeZones.E("sad_orlan", Position.Attacker, Family.Matter, new StatBlock(5800, 840, 135, 100), true,
                S("sad_orlan_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(120), St(StatusType.Mark, 2)),
                S("sad_orlan_a2", SkillSlot.A2, 3, TargetRule.Self, Shield(25), St(StatusType.Taunt, 1)),
                S("sad_orlan_a3", SkillSlot.A3, 5, TargetRule.AllEnemies, Dmg(85), St(StatusType.Blind, 1, 40)),
                S("sad_orlan_a4", SkillSlot.A4, 4, TargetRule.SingleEnemy, Dmg(75), St(StatusType.Bind, 1, 50))),
                PrototypeZones.Phase(66, "enemies.sad_orlan.phase2", 10, 0, 5,
                    new[]
                    {
                        S("sad_orlan_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(120), St(StatusType.Mark, 2)),
                        S("sad_orlan_p2_a2", SkillSlot.A2, 3, TargetRule.SingleEnemy, Dmg(150), St(StatusType.Vulnerable, 2)),
                        S("sad_orlan_a3", SkillSlot.A3, 5, TargetRule.AllEnemies, Dmg(85), St(StatusType.Blind, 1, 40)),
                        S("sad_orlan_a4", SkillSlot.A4, 4, TargetRule.SingleEnemy, Dmg(75), St(StatusType.Bind, 1, 50)),
                    }, AiStyle.Aggressive,
                    new[] { L("orlan", "narration.mos_3_4.boss.phase1.0") }),
                PrototypeZones.Phase(33, "enemies.sad_orlan.phase3", 25, -10, 10, null, null,
                    new[] { L("orlan", "narration.mos_3_4.boss.phase2.0") })), AiStyle.Control);
        }
    }
}
