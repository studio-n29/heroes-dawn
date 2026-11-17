using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Content.Prototype
{
    /// <summary>
    /// Chapitre Manhattan (New York, chapitre 4, « face-à-face ») : quatre zones 4.1 à 4.4, déblocage linéaire (4.1 exige Harlem 3.4), boss : le Général Stroke.
    /// L'AMIF est ici l'adversaire à visage découvert. Graphes : 4.1 = A, 4.2 = D (boucle), 4.3 = C (trois voies), 4.4 = B (pont) avec boss.
    /// </summary>
    public static class PrototypeManhattan
    {
        public const string Borough = "manhattan";

        public static readonly ZoneId Zone41 = new ZoneId("zone_ny_4_1");
        public static readonly ZoneId Zone42 = new ZoneId("zone_ny_4_2");
        public static readonly ZoneId Zone43 = new ZoneId("zone_ny_4_3");
        public static readonly ZoneId Zone44 = new ZoneId("zone_ny_4_4");

        static readonly ZoneId HarlemLast = new ZoneId("zone_ny_3_4");

        public static IEnumerable<ZoneDef> Zones()
        {
            yield return BuildZone41();
            yield return BuildZone42();
            yield return BuildZone43();
            yield return BuildZone44();
        }

        /// <summary>Zones à valider avant d'ouvrir celle-ci : 4.1 exige la dernière zone de Harlem, puis déblocage linéaire.</summary>
        public static IReadOnlyList<ZoneId> Requires(ZoneId zone)
        {
            if (zone == Zone41) return new[] { HarlemLast };
            if (zone == Zone42) return new[] { Zone41 };
            if (zone == Zone43) return new[] { Zone42 };
            if (zone == Zone44) return new[] { Zone43 };
            return System.Array.Empty<ZoneId>();
        }

        /// <summary>Réplique du script : clé <c>narration.&lt;zone&gt;.&lt;slot&gt;.&lt;i&gt;</c>.</summary>
        static NarrationLineDef L(string speaker, string key) => new NarrationLineDef(speaker, "narration." + key);

        static NarrationLineDef[] Lines(string prefix, params string[] speakers)
        {
            var r = new NarrationLineDef[speakers.Length];
            for (int i = 0; i < r.Length; i++) r[i] = L(speakers[i], prefix + "." + i);
            return r;
        }

        /// <summary>Variante ou réplique réservée à un parcours (<c>agent_d</c> / <c>agent_m</c>) ; <c>replaces</c> = clé de la base remplacée.</summary>
        static NarrationLineDef R(string speaker, string key, string route, string replaces = null)
            => new NarrationLineDef(speaker, "narration." + key, route, replaces == null ? null : "narration." + replaces);

        static NarrationLineDef[] Cat(NarrationLineDef[] a, NarrationLineDef[] b)
        {
            var r = new NarrationLineDef[a.Length + b.Length];
            a.CopyTo(r, 0); b.CopyTo(r, a.Length);
            return r;
        }

        static ObjectiveDef Obj(string id, ObjectiveType t, string node, string zoneKey, string key)
            => new ObjectiveDef(id, t, new CaseId(node), "objectives." + zoneKey + "." + key);

        static EncounterDef Enc(CombatRank r, params string[] enemies)
        {
            var ids = new EnemyId[enemies.Length];
            for (int i = 0; i < ids.Length; i++) ids[i] = new EnemyId(enemies[i]);
            return new EncounterDef(r, ids);
        }

        /// <summary>Times Square : graphe A. Reprendre les écrans à la propagande ; le chef de la sécurité garde la régie (n14).</summary>
        static ZoneDef BuildZone41()
        {
            var intro = new NarrationDef(Cat(Lines("ny_4_1.intro", "narrateur", "voix", "giulia_rossi", "lucas_mendes", "agent_d"),
                new[] { R("agent_m", "ny_4_1.intro.4.m", "agent_m", "ny_4_1.intro.4") }), null);
            var chief = new NarrationDef(Lines("ny_4_1.elite.before", "agent_amif", "ilona_varga"), Lines("ny_4_1.elite.after", "lucas_mendes"));
            var exit = new NarrationDef(null, Cat(Lines("ny_4_1.exit", "narrateur", "ilona_varga", "giulia_rossi"),
                new[] { R("agent_m", "ny_4_1.exit.3", "agent_m") }));
            var objectives = new List<ObjectiveDef>
            {
                Obj("obj_square", ObjectiveType.ReachCase, "n03", "ny41", "square"),
                Obj("obj_screens", ObjectiveType.CompleteMiniGame, "n13", "ny41", "screens"),
                Obj("obj_control", ObjectiveType.DefeatCombat, "n14", "ny41", "control"),
                Obj("obj_broadcast", ObjectiveType.ReachCase, "n19", "ny41", "broadcast"),
            };
            var encounters = new List<EncounterDef>
            {
                Enc(CombatRank.Easy, "mnh_drone", "mnh_guard"),
                Enc(CombatRank.Normal, "mnh_guard", "mnh_agent", "mnh_drone"),
                Enc(CombatRank.Elite, "mnh_chief", "mnh_drone", "mnh_agent"),
            };
            return new ZoneDef(Zone41, "zones.ny_4_1.name", new CaseId("n00"), PrototypeZones.BrownsvilleNodes(chief, exit, "dna_lab", true, intro), PrototypeZones.BrownsvilleLinks(),
                               objectives, encounters, "4.1", 4, Borough, 24, Requires(Zone41), new ZoneEnvironment(Biome.City, Weather.Night));
        }

        /// <summary>Tours de Midtown : graphe D. Infiltrer les cercles de pouvoir, recouper les financeurs ; sortie par n21.</summary>
        static ZoneDef BuildZone42()
        {
            var intro = new NarrationDef(Cat(Lines("ny_4_2.intro", "narrateur", "giulia_rossi", "lucas_mendes", "agent_d"),
                new[] { R("agent_m", "ny_4_2.intro.3.m", "agent_m", "ny_4_2.intro.3") }), null);
            var chief = new NarrationDef(Lines("ny_4_2.elite.before", "enforcer", "ilona_varga"), Lines("ny_4_2.elite.after", "giulia_rossi", "lucas_mendes"));
            var exit = new NarrationDef(null, Cat(Lines("ny_4_2.exit", "giulia_rossi", "ilona_varga", "agent_d"),
                new[] { R("agent_m", "ny_4_2.exit.3", "agent_m") }));
            var objectives = new List<ObjectiveDef>
            {
                Obj("obj_badges", ObjectiveType.CompleteMiniGame, "n11", "ny42", "badges"),
                Obj("obj_security", ObjectiveType.DefeatCombat, "n13", "ny42", "security"),
                Obj("obj_roofs", ObjectiveType.CompleteMiniGame, "n20", "ny42", "roofs"),
                Obj("obj_ledger", ObjectiveType.ReachCase, "n21", "ny42", "ledger"),
            };
            var encounters = new List<EncounterDef>
            {
                Enc(CombatRank.Easy, "mnh_guard", "mnh_agent"),
                Enc(CombatRank.Normal, "mnh_guard", "mnh_agent", "mnh_agent"),
                Enc(CombatRank.Normal, "mnh_drone", "mnh_guard", "mnh_agent"),
                Enc(CombatRank.Elite, "mnh_chief", "mnh_guard", "mnh_agent"),
            };
            return new ZoneDef(Zone42, "zones.ny_4_2.name", new CaseId("n00"), ChapterGraphs.LoopNodes(chief, exit, "infiltration_camp", "rhythm_rooftop", false, intro), ChapterGraphs.LoopLinks(),
                               objectives, encounters, "4.2", 4, Borough, 25, Requires(Zone42), new ZoneEnvironment(Biome.City, Weather.Rain));
        }

        /// <summary>Siège de l'ONU : graphe C. Protéger les délégués et l'annonce ; Rayner et Stroke s'affrontent (élite n14, sortie n22).</summary>
        static ZoneDef BuildZone43()
        {
            var intro = new NarrationDef(Cat(Lines("ny_4_3.intro", "narrateur", "giulia_rossi", "lucas_mendes", "agent_d"),
                new[] { R("agent_m", "ny_4_3.intro.3.m", "agent_m", "ny_4_3.intro.3") }), null);
            var captain = new NarrationDef(Lines("ny_4_3.elite.before", "agent_amif", "ilona_varga"), Lines("ny_4_3.elite.after", "giulia_rossi"));
            var exit = new NarrationDef(null, Lines("ny_4_3.exit", "narrateur", "ilona_varga"));
            var stage = new NarrationDef(null, Cat(Lines("ny_4_3.stage", "rayner", "stroke", "rayner", "stroke", "rayner"), new[]
            {
                new NarrationLineDef("agent_d", "narration.ny_4_3.stage.5.d", "agent_d", null),
                new NarrationLineDef("agent_m", "narration.ny_4_3.stage.5.m", "agent_m", null),
            }));
            var objectives = new List<ObjectiveDef>
            {
                Obj("obj_delegates", ObjectiveType.CompleteMiniGame, "n13", "ny43", "delegates"),
                Obj("obj_captain", ObjectiveType.DefeatCombat, "n14", "ny43", "captain"),
                Obj("obj_hall", ObjectiveType.CompleteMiniGame, "n21", "ny43", "hall"),
                Obj("obj_accords", ObjectiveType.ReachCase, "n22", "ny43", "accords"),
            };
            var encounters = new List<EncounterDef>
            {
                Enc(CombatRank.Easy, "amif_trooper", "mnh_drone"),
                Enc(CombatRank.Normal, "amif_trooper", "amif_marksman", "mnh_guard"),
                Enc(CombatRank.Elite, "amif_captain", "amif_trooper", "amif_marksman"),
            };
            return new ZoneDef(Zone43, "zones.ny_4_3.name", new CaseId("n00"), ChapterGraphs.ThreeLanesNodes(captain, exit, "deflect_docks", "arena_training", false, intro, stage), ChapterGraphs.ThreeLanesLinks(),
                               objectives, encounters, "4.3", 4, Borough, 27, Requires(Zone43), new ZoneEnvironment(Biome.City, Weather.Day));
        }

        /// <summary>Esplanade de l'ONU : graphe B avec boss fixe au bout, le Général Stroke (avant, après, fin de chapitre).</summary>
        static ZoneDef BuildZone44()
        {
            var intro = new NarrationDef(Lines("ny_4_4.intro", "narrateur", "lucas_mendes"), null);
            var guard = new NarrationDef(Lines("ny_4_4.elite.before", "agent_amif", "ilona_varga"), Lines("ny_4_4.elite.after", "giulia_rossi"));
            var stroke = new NarrationDef(
                new[]
                {
                    L("stroke", "ny_4_4.boss.before.0"),
                    L("stroke", "ny_4_4.boss.before.1"),
                    L("agent_d", "ny_4_4.boss.before.2"),
                    R("agent_m", "ny_4_4.boss.before.2.m", "agent_m", "ny_4_4.boss.before.2"),
                    L("stroke", "ny_4_4.boss.before.3"),
                },
                Cat(Cat(Lines("ny_4_4.boss.after", "stroke", "ilona_varga", "stroke"),
                        new[] { R("agent_d", "ny_4_4.boss.after.3.d", "agent_d"), R("agent_m", "ny_4_4.boss.after.3.m", "agent_m") }),
                    Lines("ny_4_4.chapter_end", "narrateur", "rayner")));
            var objectives = new List<ObjectiveDef>
            {
                Obj("obj_esplanade", ObjectiveType.ReachCase, "n04", "ny44", "esplanade"),
                Obj("obj_evacuate", ObjectiveType.CompleteMiniGame, "n11", "ny44", "evacuate"),
                Obj("obj_line", ObjectiveType.DefeatCombat, "n14", "ny44", "line"),
                Obj("obj_stroke", ObjectiveType.DefeatCombat, "n18", "ny44", "stroke"),
            };
            var encounters = new List<EncounterDef>
            {
                Enc(CombatRank.Easy, "amif_trooper", "amif_marksman"),
                Enc(CombatRank.Normal, "amif_trooper", "amif_marksman", "mnh_agent"),
                Enc(CombatRank.Elite, "amif_captain", "mnh_guard", "amif_marksman"),
                Enc(CombatRank.Boss, "mnh_stroke", "amif_marksman", "amif_trooper"),
            };
            return new ZoneDef(Zone44, "zones.ny_4_4.name", new CaseId("n00"), PrototypeZones.BridgeNodes(stroke, guard, null, "rooftop_rescue", true, true, intro), PrototypeZones.BridgeLinks(),
                               objectives, encounters, "4.4", 4, Borough, 30, Requires(Zone44), new ZoneEnvironment(Biome.City, Weather.Fog));
        }

        static EffectDef Dmg(int pct) => PrototypeZones.Dmg(pct);
        static EffectDef St(StatusType s, int turns, int chance = 100) => PrototypeZones.St(s, turns, chance);
        static EffectDef Shield(int pct, int turns = 2) => PrototypeZones.Shield(pct, turns);
        static SkillDef S(string id, SkillSlot slot, int cd, TargetRule t, params EffectDef[] fx) => PrototypeZones.S(id, slot, cd, t, fx);

        public static IEnumerable<EnemyDef> Enemies()
        {
            // Base : garde de tour (ligne de front, bouclier + provocation), agent de sécurité (tireur, marque), drone de propagande (attaquant, aveuglement).
            yield return PrototypeZones.E("mnh_guard", Position.FrontLine, Family.Matter, new StatBlock(1700, 120, 100, 100), false,
                S("mnh_guard_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(100)),
                S("mnh_guard_a2", SkillSlot.A2, 4, TargetRule.Self, Shield(15), St(StatusType.Taunt, 1)));
            yield return PrototypeZones.E("mnh_agent", Position.Ranged, Family.Perception, new StatBlock(1000, 140, 65, 110), false,
                S("mnh_agent_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(105)),
                S("mnh_agent_a2", SkillSlot.A2, 4, TargetRule.SingleEnemy, Dmg(160), St(StatusType.Mark, 2)));
            yield return PrototypeZones.E("mnh_drone", Position.Attacker, Family.Elemental, new StatBlock(1100, 135, 70, 115), false,
                S("mnh_drone_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(100)),
                S("mnh_drone_a2", SkillSlot.A2, 4, TargetRule.AllEnemies, Dmg(60), St(StatusType.Blind, 1, 35)));
            // Élite : chef de la sécurité privée.
            yield return PrototypeZones.E("mnh_chief", Position.Attacker, Family.PhysicalBio, new StatBlock(3200, 600, 115, 100), false,
                S("mnh_chief_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(110)),
                S("mnh_chief_a2", SkillSlot.A2, 3, TargetRule.AllEnemies, Dmg(80), St(StatusType.Shock, 1, 30)),
                S("mnh_chief_a3", SkillSlot.A3, 5, TargetRule.Self, Shield(20)));
            // Boss : le Général Stroke, humain équipé. Les phases sont des changements de dispositif :
            // assaut (fusil, couverture, barrage), puis dispositif lourd sous 66 %, puis repli sous 33 %.
            yield return PrototypeZones.Styled(PrototypeZones.Phased(PrototypeZones.E("mnh_stroke", Position.Attacker, Family.PhysicalBio, new StatBlock(6000, 850, 135, 100), true,
                S("mnh_stroke_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(120), St(StatusType.Mark, 2)),
                S("mnh_stroke_a2", SkillSlot.A2, 3, TargetRule.Self, Shield(25), St(StatusType.Taunt, 1)),
                S("mnh_stroke_a3", SkillSlot.A3, 5, TargetRule.AllEnemies, Dmg(90), St(StatusType.Blind, 1, 40)),
                S("mnh_stroke_a4", SkillSlot.A4, 4, TargetRule.SingleEnemy, Dmg(75), St(StatusType.Bind, 1, 50))),
                PrototypeZones.Phase(66, "enemies.mnh_stroke.phase2", 10, 0, 5,
                    new[]
                    {
                        S("mnh_stroke_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(120), St(StatusType.Mark, 2)),
                        S("mnh_stroke_p2_a2", SkillSlot.A2, 3, TargetRule.SingleEnemy, Dmg(150), St(StatusType.Vulnerable, 2)),
                        S("mnh_stroke_a3", SkillSlot.A3, 5, TargetRule.AllEnemies, Dmg(90), St(StatusType.Blind, 1, 40)),
                        S("mnh_stroke_a4", SkillSlot.A4, 4, TargetRule.SingleEnemy, Dmg(75), St(StatusType.Bind, 1, 50)),
                    }, AiStyle.Aggressive, new[] { L("stroke", "ny_4_4.boss.phase1.0") }),
                PrototypeZones.Phase(33, "enemies.mnh_stroke.phase3", 25, -10, 10,
                    new[]
                    {
                        S("mnh_stroke_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(120), St(StatusType.Mark, 2)),
                        S("mnh_stroke_p2_a2", SkillSlot.A2, 3, TargetRule.SingleEnemy, Dmg(150), St(StatusType.Vulnerable, 2)),
                        S("mnh_stroke_p3_a3", SkillSlot.A3, 5, TargetRule.AllEnemies, Dmg(100), St(StatusType.Burn, 2, 40)),
                        S("mnh_stroke_a4", SkillSlot.A4, 4, TargetRule.SingleEnemy, Dmg(75), St(StatusType.Bind, 1, 50)),
                    }, AiStyle.Aggressive, new[] { L("stroke", "ny_4_4.boss.phase2.0") })), AiStyle.Balanced);
        }
    }
}
