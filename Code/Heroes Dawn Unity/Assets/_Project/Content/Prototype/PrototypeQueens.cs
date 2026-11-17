using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Content.Prototype
{
    /// <summary>
    /// Chapitre Queens (New York, chapitre 2, « preuve de la collusion ») : quatre zones 2.1 à 2.4, déblocage linéaire (2.1 exige la fin de Brooklyn),
    /// boss : le Régisseur (Elias Voss, intendant du Syndicat). Graphes : 2.1 = C, 2.2 = A, 2.3 = D, 2.4 = B avec boss.
    /// L'AMIF n'apparaît que par ses sigles, ses listes et ses registres : aucun ennemi amif_* dans les rencontres.
    /// </summary>
    public static class PrototypeQueens
    {
        public const string Borough = "queens";

        public static readonly ZoneId Zone21 = new ZoneId("zone_ny_2_1");
        public static readonly ZoneId Zone22 = new ZoneId("zone_ny_2_2");
        public static readonly ZoneId Zone23 = new ZoneId("zone_ny_2_3");
        public static readonly ZoneId Zone24 = new ZoneId("zone_ny_2_4");

        public static IEnumerable<ZoneDef> Zones()
        {
            yield return BuildZone21();
            yield return BuildZone22();
            yield return BuildZone23();
            yield return BuildZone24();
        }

        /// <summary>Zones à valider avant d'ouvrir celle-ci : linéaire, 2.1 exige la dernière zone de Brooklyn (1.4).</summary>
        public static IReadOnlyList<ZoneId> Requires(ZoneId zone)
        {
            if (zone == Zone21) return new[] { PrototypeZones.Zone14 };
            if (zone == Zone22) return new[] { Zone21 };
            if (zone == Zone23) return new[] { Zone22 };
            if (zone == Zone24) return new[] { Zone23 };
            return System.Array.Empty<ZoneId>();
        }

        static NarrationLineDef L(string speaker, string key) => new NarrationLineDef(speaker, key);

        /// <summary>Variante réservée à un parcours : remplace la réplique <paramref name="replaces"/> pour ce parcours.</summary>
        static NarrationLineDef V(string speaker, string key, string route, string replaces) => new NarrationLineDef(speaker, key, route, replaces);

        /// <summary>Réplique ajoutée pour un seul parcours (reconnaissance).</summary>
        static NarrationLineDef R(string speaker, string key, string route) => new NarrationLineDef(speaker, key, route, null);

        static ObjectiveDef Obj(string id, ObjectiveType t, string node, string zoneKey, string key)
            => new ObjectiveDef(id, t, new CaseId(node), "objectives." + zoneKey + "." + key);

        static EncounterDef Enc(CombatRank r, params string[] enemies)
        {
            var ids = new EnemyId[enemies.Length];
            for (int i = 0; i < ids.Length; i++) ids[i] = new EnemyId(enemies[i]);
            return new EncounterDef(r, ids);
        }

        static EffectDef Dmg(int pct) => PrototypeZones.Dmg(pct);
        static EffectDef St(StatusType s, int turns, int chance = 100) => PrototypeZones.St(s, turns, chance);
        static EffectDef Shield(int pct, int turns = 2) => PrototypeZones.Shield(pct, turns);
        static SkillDef S(string id, SkillSlot slot, int cd, TargetRule t, params EffectDef[] fx) => PrototypeZones.S(id, slot, cd, t, fx);

        /// <summary>Flushing, marché sous caméras : graphe C. Couper les caméras (n13), vaincre le collecteur (n14), relever le sigle (n22).</summary>
        static ZoneDef BuildZone21()
        {
            var collector = new NarrationDef(
                new[] { L("enforcer", "narration.ny_2_1.elite.before.0"), L("ilona_varga", "narration.ny_2_1.elite.before.1") },
                new[] { L("lucas_mendes", "narration.ny_2_1.elite.after.0"), L("giulia_rossi", "narration.ny_2_1.elite.after.1") });
            var exit = new NarrationDef(null, new[] { L("ilona_varga", "narration.ny_2_1.exit.0"), L("agent_d", "narration.ny_2_1.exit.1"), R("agent_m", "narration.ny_2_1.exit.2", "agent_m") });
            var intro = new NarrationDef(new[] { L("narrateur", "narration.ny_2_1.intro.0"), L("giulia_rossi", "narration.ny_2_1.intro.1"), L("lucas_mendes", "narration.ny_2_1.intro.2"), L("commercant", "narration.ny_2_1.intro.3"), L("agent_d", "narration.ny_2_1.intro.4"), V("agent_m", "narration.ny_2_1.intro.4.m", "agent_m", "narration.ny_2_1.intro.4"), L("commercant", "narration.ny_2_1.intro.5") }, null);
            var objectives = new List<ObjectiveDef>
            {
                Obj("obj_market", ObjectiveType.ReachCase, "n04", "ny21", "market"),
                Obj("obj_cameras", ObjectiveType.CompleteMiniGame, "n13", "ny21", "cameras"),
                Obj("obj_collector", ObjectiveType.DefeatCombat, "n14", "ny21", "collector"),
                Obj("obj_logo", ObjectiveType.ReachCase, "n22", "ny21", "logo"),
            };
            var encounters = new List<EncounterDef>
            {
                Enc(CombatRank.Easy, "syn_racketeer", "syn_hacker"),
                Enc(CombatRank.Easy, "thug", "syn_hacker"),
                Enc(CombatRank.Normal, "syn_racketeer", "syn_gunman", "syn_hacker"),
                Enc(CombatRank.Elite, "syn_collector", "syn_racketeer", "syn_hacker"),
            };
            return new ZoneDef(Zone21, "zones.ny_2_1.name", new CaseId("n00"), ChapterGraphs.ThreeLanesNodes(collector, exit, "infiltration_camp", "gears_workshop", false, intro), ChapterGraphs.ThreeLanesLinks(),
                               objectives, encounters, "2.1", 2, Borough, 12, Requires(Zone21), new ZoneEnvironment(Biome.City, Weather.Day));
        }

        /// <summary>Astoria, les indics : graphe A. Rendez-vous (n03), poursuite de l'indic (n13), son escorte (n14), mise à l'abri (n19).</summary>
        static ZoneDef BuildZone22()
        {
            var escort = new NarrationDef(
                new[] { L("enforcer", "narration.ny_2_2.elite.before.0") },
                new[] { L("giulia_rossi", "narration.ny_2_2.elite.after.0") });
            var exit = new NarrationDef(null, new[] { L("indic", "narration.ny_2_2.exit.0"), L("giulia_rossi", "narration.ny_2_2.exit.1"), L("indic", "narration.ny_2_2.exit.2"), L("ilona_varga", "narration.ny_2_2.exit.3"), R("agent_m", "narration.ny_2_2.exit.4", "agent_m") });
            var intro = new NarrationDef(new[] { L("narrateur", "narration.ny_2_2.intro.0"), L("lucas_mendes", "narration.ny_2_2.intro.1"), L("indic", "narration.ny_2_2.intro.2"), L("agent_d", "narration.ny_2_2.intro.3"), V("agent_m", "narration.ny_2_2.intro.3.m", "agent_m", "narration.ny_2_2.intro.3"), L("ilona_varga", "narration.ny_2_2.intro.4") }, null);
            var objectives = new List<ObjectiveDef>
            {
                Obj("obj_meeting", ObjectiveType.ReachCase, "n03", "ny22", "meeting"),
                Obj("obj_chase", ObjectiveType.CompleteMiniGame, "n13", "ny22", "chase"),
                Obj("obj_escort", ObjectiveType.DefeatCombat, "n14", "ny22", "escort"),
                Obj("obj_shelter", ObjectiveType.ReachCase, "n19", "ny22", "shelter"),
            };
            var encounters = new List<EncounterDef>
            {
                Enc(CombatRank.Easy, "thug", "syn_hacker"),
                Enc(CombatRank.Easy, "syn_racketeer", "syn_gunman"),
                Enc(CombatRank.Normal, "syn_gunman", "syn_racketeer", "syn_hacker"),
                Enc(CombatRank.Elite, "syn_collector", "syn_gunman", "sniper"),
            };
            return new ZoneDef(Zone22, "zones.ny_2_2.name", new CaseId("n00"), PrototypeZones.BrownsvilleNodes(escort, exit, "rooftop_rescue", true, intro), PrototypeZones.BrownsvilleLinks(),
                               objectives, encounters, "2.2", 2, Borough, 13, Requires(Zone22), new ZoneEnvironment(Biome.Harbor, Weather.Night));
        }

        /// <summary>Triage de Jamaica, convoi de captifs : graphe D. Wagons (n11), escorte (n13), captifs identifiés (n20), convoi libéré (n21).</summary>
        static ZoneDef BuildZone23()
        {
            var escort = new NarrationDef(
                new[] { L("enforcer", "narration.ny_2_3.elite.before.0"), L("ilona_varga", "narration.ny_2_3.elite.before.1") },
                new[] { L("captif", "narration.ny_2_3.elite.after.0"), L("giulia_rossi", "narration.ny_2_3.elite.after.1"), L("captif", "narration.ny_2_3.elite.after.2") });
            var exit = new NarrationDef(null, new[] { L("ilona_varga", "narration.ny_2_3.exit.0"), L("agent_d", "narration.ny_2_3.exit.1"), R("agent_m", "narration.ny_2_3.exit.2", "agent_m") });
            var intro = new NarrationDef(new[] { L("narrateur", "narration.ny_2_3.intro.0"), L("lucas_mendes", "narration.ny_2_3.intro.1"), L("giulia_rossi", "narration.ny_2_3.intro.2"), L("agent_d", "narration.ny_2_3.intro.3"), V("agent_m", "narration.ny_2_3.intro.3.m", "agent_m", "narration.ny_2_3.intro.3") }, null);
            var objectives = new List<ObjectiveDef>
            {
                Obj("obj_wagons", ObjectiveType.CompleteMiniGame, "n11", "ny23", "wagons"),
                Obj("obj_escort", ObjectiveType.DefeatCombat, "n13", "ny23", "escort"),
                Obj("obj_captives", ObjectiveType.CompleteMiniGame, "n20", "ny23", "captives"),
                Obj("obj_free", ObjectiveType.ReachCase, "n21", "ny23", "free"),
            };
            var encounters = new List<EncounterDef>
            {
                Enc(CombatRank.Easy, "syn_racketeer", "syn_racketeer"),
                Enc(CombatRank.Easy, "syn_gunman", "syn_hacker"),
                Enc(CombatRank.Normal, "brute", "syn_gunman", "syn_hacker"),
                Enc(CombatRank.Elite, "syn_collector", "syn_racketeer", "street_pyro"),
            };
            return new ZoneDef(Zone23, "zones.ny_2_3.name", new CaseId("n00"), ChapterGraphs.LoopNodes(escort, exit, "crate_pick_depot", "dna_lab", false, intro), ChapterGraphs.LoopLinks(),
                               objectives, encounters, "2.3", 2, Borough, 15, Requires(Zone23), new ZoneEnvironment(Biome.Industrial, Weather.Rain));
        }

        /// <summary>Terminal de fret : graphe B avec boss fixe au bout, le Régisseur. Le registre de transport est la preuve n°1.</summary>
        static ZoneDef BuildZone24()
        {
            var guard = new NarrationDef(
                new[] { L("enforcer", "narration.ny_2_4.elite.before.0") },
                new[] { L("ilona_varga", "narration.ny_2_4.elite.after.0") });
            var regisseur = new NarrationDef(
                new[] { L("regisseur", "narration.ny_2_4.boss.before.0"), L("regisseur", "narration.ny_2_4.boss.before.1"), L("agent_d", "narration.ny_2_4.boss.before.2"), V("agent_m", "narration.ny_2_4.boss.before.2.m", "agent_m", "narration.ny_2_4.boss.before.2"), L("regisseur", "narration.ny_2_4.boss.before.3") },
                new[] { L("giulia_rossi", "narration.ny_2_4.boss.after.0"), L("lucas_mendes", "narration.ny_2_4.boss.after.1"), L("ilona_varga", "narration.ny_2_4.boss.after.2"), L("regisseur", "narration.ny_2_4.boss.after.3"), R("agent_m", "narration.ny_2_4.boss.after.4", "agent_m"), L("narrateur", "narration.ny_2_4.chapter_end.0"), L("ilona_varga", "narration.ny_2_4.chapter_end.1") });
            var intro = new NarrationDef(new[] { L("narrateur", "narration.ny_2_4.intro.0"), L("lucas_mendes", "narration.ny_2_4.intro.1"), L("giulia_rossi", "narration.ny_2_4.intro.2"), L("agent_d", "narration.ny_2_4.intro.3") }, null);
            var objectives = new List<ObjectiveDef>
            {
                Obj("obj_terminal", ObjectiveType.ReachCase, "n04", "ny24", "terminal"),
                Obj("obj_cages", ObjectiveType.CompleteMiniGame, "n11", "ny24", "cages"),
                Obj("obj_guard", ObjectiveType.DefeatCombat, "n14", "ny24", "guard"),
                Obj("obj_regisseur", ObjectiveType.DefeatCombat, "n18", "ny24", "regisseur"),
            };
            var encounters = new List<EncounterDef>
            {
                Enc(CombatRank.Easy, "syn_racketeer", "syn_hacker"),
                Enc(CombatRank.Normal, "syn_racketeer", "syn_gunman", "syn_gunman"),
                Enc(CombatRank.Elite, "syn_collector", "syn_gunman", "syn_hacker"),
                Enc(CombatRank.Boss, "syn_regisseur", "syn_gunman", "syn_hacker"),
            };
            return new ZoneDef(Zone24, "zones.ny_2_4.name", new CaseId("n00"), PrototypeZones.BridgeNodes(regisseur, guard, null, "rescue_tool", true, true, intro), PrototypeZones.BridgeLinks(),
                               objectives, encounters, "2.4", 2, Borough, 17, Requires(Zone24), new ZoneEnvironment(Biome.Industrial, Weather.Fog));
        }

        public static IEnumerable<EnemyDef> Enemies()
        {
            // Base : racketteur (ligne de front, bouclier + provocation), pirate des caméras (tireur, aveuglement), exécuteur (attaquant, vulnérabilité).
            yield return PrototypeZones.E("syn_racketeer", Position.FrontLine, Family.Matter, new StatBlock(1600, 120, 95, 100), false,
                S("syn_racketeer_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(100)),
                S("syn_racketeer_a2", SkillSlot.A2, 4, TargetRule.Self, Shield(15), St(StatusType.Taunt, 1)));
            yield return PrototypeZones.E("syn_hacker", Position.Ranged, Family.Perception, new StatBlock(950, 125, 60, 112), false,
                S("syn_hacker_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(100)),
                S("syn_hacker_a2", SkillSlot.A2, 4, TargetRule.SingleEnemy, Dmg(80), St(StatusType.Blind, 2, 60)));
            yield return PrototypeZones.E("syn_gunman", Position.Attacker, Family.PhysicalBio, new StatBlock(1100, 148, 70, 108), false,
                S("syn_gunman_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(105)),
                S("syn_gunman_a2", SkillSlot.A2, 4, TargetRule.SingleEnemy, Dmg(150), St(StatusType.Vulnerable, 2)));
            // Élite : le collecteur du Syndicat.
            yield return PrototypeZones.E("syn_collector", Position.Attacker, Family.Matter, new StatBlock(3200, 600, 115, 100), false,
                S("syn_collector_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(110)),
                S("syn_collector_a2", SkillSlot.A2, 3, TargetRule.AllEnemies, Dmg(75), St(StatusType.Mark, 2, 50)),
                S("syn_collector_a3", SkillSlot.A3, 5, TargetRule.Self, Shield(20)));
            // Boss : le Régisseur. Phase 1 « Exécution du contrat » ; sous 66 % « Pénalités de retard » (A2 remplacée par un coup unique) ; sous 33 % « Rupture de contrat ».
            yield return PrototypeZones.Styled(PrototypeZones.Phased(PrototypeZones.E("syn_regisseur", Position.Attacker, Family.Perception, new StatBlock(5800, 830, 135, 100), true,
                S("syn_regisseur_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(120), St(StatusType.Mark, 2)),
                S("syn_regisseur_a2", SkillSlot.A2, 3, TargetRule.Self, Shield(25), St(StatusType.Taunt, 1)),
                S("syn_regisseur_a3", SkillSlot.A3, 5, TargetRule.AllEnemies, Dmg(90), St(StatusType.Blind, 2, 40)),
                S("syn_regisseur_a4", SkillSlot.A4, 4, TargetRule.SingleEnemy, Dmg(75), St(StatusType.Bind, 1, 50))),
                PrototypeZones.Phase(66, "enemies.syn_regisseur.phase2", 10, 0, 5,
                    new[]
                    {
                        S("syn_regisseur_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(120), St(StatusType.Mark, 2)),
                        S("syn_regisseur_p2_a2", SkillSlot.A2, 3, TargetRule.SingleEnemy, Dmg(150), St(StatusType.Vulnerable, 2)),
                        S("syn_regisseur_a3", SkillSlot.A3, 5, TargetRule.AllEnemies, Dmg(90), St(StatusType.Blind, 2, 40)),
                        S("syn_regisseur_a4", SkillSlot.A4, 4, TargetRule.SingleEnemy, Dmg(75), St(StatusType.Bind, 1, 50)),
                    }, AiStyle.Aggressive, new[] { L("regisseur", "narration.ny_2_4.boss.phase1.0") }),
                PrototypeZones.Phase(33, "enemies.syn_regisseur.phase3", 25, -10, 10, null, null, new[] { L("regisseur", "narration.ny_2_4.boss.phase2.0") })), AiStyle.Control);
        }
    }
}
