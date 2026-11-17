using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Content.Prototype
{
    /// <summary>
    /// Chapitre Kapotnya (Moscou, chapitre 2 : « preuve du marché noir ») : quatre zones M2.1 à M2.4, déblocage linéaire, boss : le Contremaître.
    /// Graphes : B (raffinerie), C (route du convoi), D (laboratoire), C avec boss (quai de chargement). L'AMIF n'apparaît jamais à l'écran (canon).
    /// </summary>
    public static class PrototypeKapotnya
    {
        public const string Borough = "kapotnya";

        public static readonly ZoneId Zone21 = new ZoneId("zone_mos_2_1");
        public static readonly ZoneId Zone22 = new ZoneId("zone_mos_2_2");
        public static readonly ZoneId Zone23 = new ZoneId("zone_mos_2_3");
        public static readonly ZoneId Zone24 = new ZoneId("zone_mos_2_4");

        public static IEnumerable<ZoneDef> Zones()
        {
            yield return BuildZone21();
            yield return BuildZone22();
            yield return BuildZone23();
            yield return BuildZone24();
        }

        /// <summary>Zones à valider avant d'ouvrir celle-ci : M2.1 exige la dernière zone de Sergueiv, puis déblocage linéaire.</summary>
        public static IReadOnlyList<ZoneId> Requires(ZoneId zone)
        {
            if (zone == Zone21) return new[] { PrototypeMoscowZones.Zone14 };
            if (zone == Zone22) return new[] { Zone21 };
            if (zone == Zone23) return new[] { Zone22 };
            if (zone == Zone24) return new[] { Zone23 };
            return System.Array.Empty<ZoneId>();
        }

        static NarrationLineDef L(string speaker, string key) => new NarrationLineDef(speaker, key);

        static ObjectiveDef Obj(string id, ObjectiveType t, string node, string zoneKey, string key)
            => new ObjectiveDef(id, t, new CaseId(node), "objectives." + zoneKey + "." + key);

        static EncounterDef Enc(CombatRank r, params string[] enemies)
        {
            var ids = new EnemyId[enemies.Length];
            for (int i = 0; i < ids.Length; i++) ids[i] = new EnemyId(enemies[i]);
            return new EncounterDef(r, ids);
        }

        /// <summary>Raffinerie : graphe B sans boss. Colmater la fuite (n11), briser la garde (n14), sortir avec les ouvriers (n18).</summary>
        static ZoneDef BuildZone21()
        {
            var intro = new NarrationDef(new[]
            {
                L("narrateur", "narration.mos_2_1.intro.0"), L("lucas_mendes", "narration.mos_2_1.intro.1"),
                L("agent_m", "narration.mos_2_1.intro.2"),
                new NarrationLineDef("agent_d", "narration.mos_2_1.intro.2.d", "agent_d", "narration.mos_2_1.intro.2"),
                L("giulia_rossi", "narration.mos_2_1.intro.3"),
                new NarrationLineDef("agent_m", "narration.mos_2_1.intro.4", "agent_m"),
            }, null);
            var guard = new NarrationDef(
                new[] { L("enforcer", "narration.mos_2_1.elite.before.0"), L("ilona_varga", "narration.mos_2_1.elite.before.1") },
                new[] { L("ouvrier", "narration.mos_2_1.elite.after.0"), L("agent_m", "narration.mos_2_1.elite.after.1") });
            var exit = new NarrationDef(null,
                new[] { L("ilona_varga", "narration.mos_2_1.exit.0"), L("agent_m", "narration.mos_2_1.exit.1") });
            var objectives = new List<ObjectiveDef>
            {
                Obj("obj_gate", ObjectiveType.ReachCase, "n04", "mos21", "gate"),
                Obj("obj_leak", ObjectiveType.CompleteMiniGame, "n11", "mos21", "leak"),
                Obj("obj_guard", ObjectiveType.DefeatCombat, "n14", "mos21", "guard"),
                Obj("obj_workers", ObjectiveType.ReachCase, "n18", "mos21", "workers"),
            };
            var encounters = new List<EncounterDef>
            {
                Enc(CombatRank.Easy, "kap_garde", "kap_garde"),
                Enc(CombatRank.Easy, "kap_convoyeur", "thug"),
                Enc(CombatRank.Normal, "kap_garde", "kap_convoyeur", "mos_guard"),
                Enc(CombatRank.Elite, "kap_surveillant", "kap_garde", "kap_convoyeur"),
            };
            return new ZoneDef(Zone21, "zones.mos_2_1.name", new CaseId("n00"), PrototypeZones.BridgeNodes(null, guard, exit, "gears_workshop", false, true, intro), PrototypeZones.BridgeLinks(),
                               objectives, encounters, "M2.1", 2, Borough, 12, Requires(Zone21), new ZoneEnvironment(Biome.Industrial, Weather.Fog));
        }

        /// <summary>Route du convoi : graphe C sans boss. Rattraper les camions (n13), briser l'escorte (n14), fouiller les résidus (n21), sortie (n22).</summary>
        static ZoneDef BuildZone22()
        {
            var intro = new NarrationDef(new[]
            {
                L("narrateur", "narration.mos_2_2.intro.0"), L("giulia_rossi", "narration.mos_2_2.intro.1"), L("agent_m", "narration.mos_2_2.intro.2"),
                new NarrationLineDef("agent_d", "narration.mos_2_2.intro.2.d", "agent_d", "narration.mos_2_2.intro.2"),
            }, null);
            var escort = new NarrationDef(
                new[] { L("enforcer", "narration.mos_2_2.elite.before.0"), L("lucas_mendes", "narration.mos_2_2.elite.before.1") },
                new[] { L("giulia_rossi", "narration.mos_2_2.elite.after.0"), L("ilona_varga", "narration.mos_2_2.elite.after.1") });
            var exit = new NarrationDef(null,
                new[] { L("agent_m", "narration.mos_2_2.exit.0"), L("ilona_varga", "narration.mos_2_2.exit.1") });
            var objectives = new List<ObjectiveDef>
            {
                Obj("obj_chase", ObjectiveType.CompleteMiniGame, "n13", "mos22", "chase"),
                Obj("obj_escort", ObjectiveType.DefeatCombat, "n14", "mos22", "escort"),
                Obj("obj_residue", ObjectiveType.CompleteMiniGame, "n21", "mos22", "residue"),
                Obj("obj_roadend", ObjectiveType.ReachCase, "n22", "mos22", "roadend"),
            };
            var encounters = new List<EncounterDef>
            {
                Enc(CombatRank.Easy, "kap_convoyeur", "mos_smuggler"),
                Enc(CombatRank.Normal, "kap_garde", "kap_convoyeur", "street_pyro"),
                Enc(CombatRank.Normal, "brute", "kap_convoyeur", "kap_convoyeur"),
                Enc(CombatRank.Elite, "kap_surveillant", "kap_convoyeur", "kap_garde"),
            };
            return new ZoneDef(Zone22, "zones.mos_2_2.name", new CaseId("n00"), ChapterGraphs.ThreeLanesNodes(escort, exit, "rooftop_rescue", "crate_pick_depot", false, intro), ChapterGraphs.ThreeLanesLinks(),
                               objectives, encounters, "M2.2", 2, Borough, 13, Requires(Zone22), new ZoneEnvironment(Biome.Industrial, Weather.Night));
        }

        /// <summary>Laboratoire clandestin : graphe D sans boss. Les dossiers (n11), les techniciens (n13), les Apex à libérer (n20), sortie (n21).</summary>
        static ZoneDef BuildZone23()
        {
            var intro = new NarrationDef(new[]
            {
                L("narrateur", "narration.mos_2_3.intro.0"), L("lucas_mendes", "narration.mos_2_3.intro.1"),
                L("apex_libere", "narration.mos_2_3.intro.2"), L("ilona_varga", "narration.mos_2_3.intro.3"),
                new NarrationLineDef("agent_m", "narration.mos_2_3.intro.4", "agent_m"),
            }, null);
            var techs = new NarrationDef(
                new[]
                {
                    L("technicien", "narration.mos_2_3.elite.before.0"), L("agent_m", "narration.mos_2_3.elite.before.1"),
                    new NarrationLineDef("agent_d", "narration.mos_2_3.elite.before.1.d", "agent_d", "narration.mos_2_3.elite.before.1"),
                },
                new[] { L("giulia_rossi", "narration.mos_2_3.elite.after.0"), L("lucas_mendes", "narration.mos_2_3.elite.after.1"), L("ilona_varga", "narration.mos_2_3.elite.after.2") });
            var exit = new NarrationDef(null,
                new[] { L("apex_libere", "narration.mos_2_3.exit.0"), L("agent_m", "narration.mos_2_3.exit.1"), L("ilona_varga", "narration.mos_2_3.exit.2") });
            var objectives = new List<ObjectiveDef>
            {
                Obj("obj_files", ObjectiveType.CompleteMiniGame, "n11", "mos23", "files"),
                Obj("obj_techs", ObjectiveType.DefeatCombat, "n13", "mos23", "techs"),
                Obj("obj_free", ObjectiveType.CompleteMiniGame, "n20", "mos23", "free"),
                Obj("obj_leave", ObjectiveType.ReachCase, "n21", "mos23", "leave"),
            };
            var encounters = new List<EncounterDef>
            {
                Enc(CombatRank.Easy, "kap_technicien", "kap_garde"),
                Enc(CombatRank.Normal, "kap_technicien", "kap_technicien", "kap_convoyeur"),
                Enc(CombatRank.Normal, "kap_garde", "kap_technicien", "street_pyro"),
                Enc(CombatRank.Elite, "kap_surveillant", "kap_technicien", "kap_technicien"),
            };
            return new ZoneDef(Zone23, "zones.mos_2_3.name", new CaseId("n00"), ChapterGraphs.LoopNodes(techs, exit, "dna_lab", "rescue_tool", false, intro), ChapterGraphs.LoopLinks(),
                               objectives, encounters, "M2.3", 2, Borough, 15, Requires(Zone23), new ZoneEnvironment(Biome.City, Weather.Rain));
        }

        /// <summary>Quai de chargement : graphe C avec boss fixe au bout, le Contremaître (avant, après, fin de chapitre).</summary>
        static ZoneDef BuildZone24()
        {
            var intro = new NarrationDef(new[]
            {
                L("narrateur", "narration.mos_2_4.intro.0"), L("giulia_rossi", "narration.mos_2_4.intro.1"),
            }, null);
            var overseer = new NarrationDef(
                new[] { L("enforcer", "narration.mos_2_4.elite.before.0") },
                new[] { L("ilona_varga", "narration.mos_2_4.elite.after.0") });
            var boss = new NarrationDef(
                new[]
                {
                    L("contremaitre", "narration.mos_2_4.boss.before.0"), L("contremaitre", "narration.mos_2_4.boss.before.1"),
                    L("agent_m", "narration.mos_2_4.boss.before.2"),
                    new NarrationLineDef("agent_d", "narration.mos_2_4.boss.before.2.d", "agent_d", "narration.mos_2_4.boss.before.2"),
                    L("contremaitre", "narration.mos_2_4.boss.before.3"),
                },
                new[]
                {
                    L("contremaitre", "narration.mos_2_4.boss.after.0"), L("contremaitre", "narration.mos_2_4.boss.after.1"),
                    L("agent_m", "narration.mos_2_4.boss.after.2"),
                    new NarrationLineDef("contremaitre", "narration.mos_2_4.boss.after.3", "agent_m"),
                    L("narrateur", "narration.mos_2_4.chapter_end.0"),
                });
            var objectives = new List<ObjectiveDef>
            {
                Obj("obj_cranes", ObjectiveType.CompleteMiniGame, "n13", "mos24", "cranes"),
                Obj("obj_overseer", ObjectiveType.DefeatCombat, "n14", "mos24", "overseer"),
                Obj("obj_dockers", ObjectiveType.CompleteMiniGame, "n21", "mos24", "dockers"),
                Obj("obj_foreman", ObjectiveType.DefeatCombat, "n22", "mos24", "foreman"),
            };
            var encounters = new List<EncounterDef>
            {
                Enc(CombatRank.Easy, "kap_garde", "thug"),
                Enc(CombatRank.Normal, "kap_convoyeur", "brute", "mos_smuggler"),
                Enc(CombatRank.Normal, "kap_garde", "kap_technicien", "kap_convoyeur"),
                Enc(CombatRank.Elite, "kap_surveillant", "mos_enforcer", "kap_garde"),
                Enc(CombatRank.Boss, "kap_contremaitre", "kap_convoyeur", "kap_garde"),
            };
            return new ZoneDef(Zone24, "zones.mos_2_4.name", new CaseId("n00"), ChapterGraphs.ThreeLanesNodes(overseer, boss, "deflect_docks", "dialogue_dockers", true, intro), ChapterGraphs.ThreeLanesLinks(),
                               objectives, encounters, "M2.4", 2, Borough, 17, Requires(Zone24), new ZoneEnvironment(Biome.Harbor, Weather.Snow));
        }

        static EffectDef Dmg(int pct) => PrototypeZones.Dmg(pct);
        static EffectDef St(StatusType s, int turns, int chance = 100) => PrototypeZones.St(s, turns, chance);
        static EffectDef Shield(int pct, int turns = 2) => PrototypeZones.Shield(pct, turns);
        static SkillDef S(string id, SkillSlot slot, int cd, TargetRule t, params EffectDef[] fx) => PrototypeZones.S(id, slot, cd, t, fx);

        public static IEnumerable<EnemyDef> Enemies()
        {
            // Base : garde de raffinerie (ligne de front, bouclier + provocation), convoyeur (tireur, marque), technicien du laboratoire (attaquant, corrosion).
            yield return PrototypeZones.E("kap_garde", Position.FrontLine, Family.Matter, new StatBlock(1700, 120, 105, 98), false,
                S("kap_garde_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(100)),
                S("kap_garde_a2", SkillSlot.A2, 4, TargetRule.Self, Shield(18), St(StatusType.Taunt, 1)));
            yield return PrototypeZones.E("kap_convoyeur", Position.Ranged, Family.Perception, new StatBlock(950, 140, 62, 112), false,
                S("kap_convoyeur_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(105)),
                S("kap_convoyeur_a2", SkillSlot.A2, 4, TargetRule.SingleEnemy, Dmg(155), St(StatusType.Mark, 2)));
            yield return PrototypeZones.E("kap_technicien", Position.Attacker, Family.Elemental, new StatBlock(1200, 145, 70, 105), false,
                S("kap_technicien_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(100)),
                S("kap_technicien_a2", SkillSlot.A2, 4, TargetRule.SingleEnemy, Dmg(140), St(StatusType.Corrosion, 2, 60)));
            // Élite : surveillant de site.
            yield return PrototypeZones.E("kap_surveillant", Position.Attacker, Family.PhysicalBio, new StatBlock(3200, 600, 115, 100), false,
                S("kap_surveillant_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(110)),
                S("kap_surveillant_a2", SkillSlot.A2, 3, TargetRule.AllEnemies, Dmg(75), St(StatusType.Burn, 2, 30)),
                S("kap_surveillant_a3", SkillSlot.A3, 5, TargetRule.Self, Shield(20)));
            // Boss : le Contremaître. Phase 1 « Prise de poste » ; sous 66 % « Heures supplémentaires » (A2 remplacée par un coup unique) ; sous 33 % « Fin de service ».
            yield return PrototypeZones.Styled(PrototypeZones.Phased(PrototypeZones.E("kap_contremaitre", Position.Attacker, Family.Matter, new StatBlock(5800, 840, 135, 100), true,
                S("kap_contremaitre_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(120), St(StatusType.Mark, 2)),
                S("kap_contremaitre_a2", SkillSlot.A2, 3, TargetRule.Self, Shield(25), St(StatusType.Taunt, 1)),
                S("kap_contremaitre_a3", SkillSlot.A3, 5, TargetRule.AllEnemies, Dmg(85), St(StatusType.Burn, 2, 40)),
                S("kap_contremaitre_a4", SkillSlot.A4, 4, TargetRule.SingleEnemy, Dmg(75), St(StatusType.Bind, 1, 50))),
                PrototypeZones.Phase(66, "enemies.kap_contremaitre.phase2", 10, 0, 5,
                    new[]
                    {
                        S("kap_contremaitre_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(120), St(StatusType.Mark, 2)),
                        S("kap_contremaitre_p2_a2", SkillSlot.A2, 3, TargetRule.SingleEnemy, Dmg(150), St(StatusType.Corrosion, 2)),
                        S("kap_contremaitre_a3", SkillSlot.A3, 5, TargetRule.AllEnemies, Dmg(85), St(StatusType.Burn, 2, 40)),
                        S("kap_contremaitre_a4", SkillSlot.A4, 4, TargetRule.SingleEnemy, Dmg(75), St(StatusType.Bind, 1, 50)),
                    }, AiStyle.Aggressive, lines: new[] { L("contremaitre", "narration.mos_2_4.boss.phase1.0") }),
                PrototypeZones.Phase(33, "enemies.kap_contremaitre.phase3", 25, -10, 10,
                    lines: new[] { L("contremaitre", "narration.mos_2_4.boss.phase2.0") })), AiStyle.Control);
        }
    }
}
