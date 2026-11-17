using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Content.Prototype
{
    /// <summary>
    /// Chapitre Place Rouge (Moscou, chapitre 4, « face-à-face et choix de confiance ») : quatre zones M4.1 à M4.4, déblocage linéaire
    /// après Sadovoye Koltso (zone_mos_3_4), boss : le Conseiller (Arkadi Zorin). Premier chapitre où l'AMIF se montre à Moscou (zones 4.3 et 4.4).
    /// Le choix final (l'arrêter ou l'écouter) ne change que l'épilogue (StoryChoices) ; la scène « après le boss » porte la révélation de la cathédrale (Agent M) ou le constat politique (Agent D).
    /// </summary>
    public static class PrototypePlaceRouge
    {
        public const string Borough = "place_rouge";

        public static readonly ZoneId Zone41 = new ZoneId("zone_mos_4_1");
        public static readonly ZoneId Zone42 = new ZoneId("zone_mos_4_2");
        public static readonly ZoneId Zone43 = new ZoneId("zone_mos_4_3");
        public static readonly ZoneId Zone44 = new ZoneId("zone_mos_4_4");

        /// <summary>Dernière zone du chapitre précédent (Sadovoye Koltso).</summary>
        static readonly ZoneId PreviousChapterLast = new ZoneId("zone_mos_3_4");

        public static IEnumerable<ZoneDef> Zones()
        {
            yield return BuildZone41();
            yield return BuildZone42();
            yield return BuildZone43();
            yield return BuildZone44();
        }

        /// <summary>Zones à valider avant d'ouvrir celle-ci : linéaire, M4.1 exige M3.4.</summary>
        public static IReadOnlyList<ZoneId> Requires(ZoneId zone)
        {
            if (zone == Zone41) return new[] { PreviousChapterLast };
            if (zone == Zone42) return new[] { Zone41 };
            if (zone == Zone43) return new[] { Zone42 };
            if (zone == Zone44) return new[] { Zone43 };
            return System.Array.Empty<ZoneId>();
        }

        /// <summary>Réplique du script narratif : clé « narration.&lt;zone&gt;.&lt;slot&gt;.&lt;i&gt; ».</summary>
        static NarrationLineDef Ln(string speaker, string slotKey) => new NarrationLineDef(speaker, "narration." + slotKey);

        /// <summary>Réplique propre à un parcours (« agent_d » / « agent_m ») ; <paramref name="replaces"/> : clé (sans « narration. ») de la réplique de base qu'elle remplace pour ce parcours, sinon réplique ajoutée.</summary>
        static NarrationLineDef Lr(string speaker, string slotKey, string route, string replaces = null)
            => new NarrationLineDef(speaker, "narration." + slotKey, route, replaces == null ? null : "narration." + replaces);

        static ObjectiveDef Obj(string id, ObjectiveType t, string node, string zoneKey, string key)
            => new ObjectiveDef(id, t, new CaseId(node), "objectives." + zoneKey + "." + key);

        static EncounterDef Enc(CombatRank r, params string[] enemies)
        {
            var ids = new EnemyId[enemies.Length];
            for (int i = 0; i < ids.Length; i++) ids[i] = new EnemyId(enemies[i]);
            return new EncounterDef(r, ids);
        }

        /// <summary>Rues sous couvre-feu : graphe D. Protéger les civils bloqués, briser le cordon des forces spéciales (n13), gagner la grille (n21).</summary>
        static ZoneDef BuildZone41()
        {
            var intro = new NarrationDef(new[]
            {
                Ln("narrateur", "mos_4_1.intro.0"), Ln("giulia_rossi", "mos_4_1.intro.1"), Ln("civil_moscou", "mos_4_1.intro.2"), Ln("agent_m", "mos_4_1.intro.3"),
                Lr("agent_d", "mos_4_1.intro.3.d", "agent_d", "mos_4_1.intro.3"),
            }, null);
            var cordon = new NarrationDef(
                new[] { Ln("enforcer", "mos_4_1.elite.before.0"), Ln("ilona_varga", "mos_4_1.elite.before.1") },
                new[] { Ln("lucas_mendes", "mos_4_1.elite.after.0") });
            var exit = new NarrationDef(null,
                new[] { Ln("giulia_rossi", "mos_4_1.exit.0"), Ln("ilona_varga", "mos_4_1.exit.1"), Ln("agent_m", "mos_4_1.exit.2"),
                    Lr("agent_d", "mos_4_1.exit.3", "agent_d") });
            var objectives = new List<ObjectiveDef>
            {
                Obj("obj_civilians", ObjectiveType.CompleteMiniGame, "n11", "mos41", "civilians"),
                Obj("obj_cordon", ObjectiveType.DefeatCombat, "n13", "mos41", "cordon"),
                Obj("obj_barrage", ObjectiveType.CompleteMiniGame, "n20", "mos41", "barrage"),
                Obj("obj_grille", ObjectiveType.ReachCase, "n21", "mos41", "grille"),
            };
            var encounters = new List<EncounterDef>
            {
                Enc(CombatRank.Easy, "pr_operator", "pr_operator"),
                Enc(CombatRank.Easy, "pr_operator", "mos_guard"),
                Enc(CombatRank.Normal, "pr_kremlin_guard", "pr_operator", "pr_operator"),
                Enc(CombatRank.Normal, "pr_kremlin_guard", "pr_technician", "pr_operator"),
                Enc(CombatRank.Elite, "pr_commandant", "pr_operator", "pr_kremlin_guard"),
            };
            return new ZoneDef(Zone41, "zones.mos_4_1.name", new CaseId("n00"), ChapterGraphs.LoopNodes(cordon, exit, "rescue_tool", "deflect_docks", false, intro), ChapterGraphs.LoopLinks(),
                               objectives, encounters, "M4.1", 4, Borough, 24, Requires(Zone41), new ZoneEnvironment(Biome.City, Weather.Snow));
        }

        /// <summary>Tunnels sous le Kremlin : graphe B sans boss. Descendre (n04), traverser les salles de recherche (n11), briser le gardien (n14), atteindre le conseil (n18).</summary>
        static ZoneDef BuildZone42()
        {
            var intro = new NarrationDef(new[]
            {
                Ln("narrateur", "mos_4_2.intro.0"), Ln("lucas_mendes", "mos_4_2.intro.1"), Ln("giulia_rossi", "mos_4_2.intro.2"), Ln("agent_m", "mos_4_2.intro.3"),
                Lr("agent_d", "mos_4_2.intro.3.d", "agent_d", "mos_4_2.intro.3"), Lr("agent_m", "mos_4_2.intro.4", "agent_m"),
            }, null);
            var warden = new NarrationDef(
                new[] { Ln("enforcer", "mos_4_2.elite.before.0") },
                new[] { Ln("ilona_varga", "mos_4_2.elite.after.0") });
            var exit = new NarrationDef(null,
                new[] { Ln("giulia_rossi", "mos_4_2.exit.0"), Ln("ilona_varga", "mos_4_2.exit.1"), Ln("agent_m", "mos_4_2.exit.2") });
            var objectives = new List<ObjectiveDef>
            {
                Obj("obj_descent", ObjectiveType.ReachCase, "n04", "mos42", "descent"),
                Obj("obj_halls", ObjectiveType.CompleteMiniGame, "n11", "mos42", "halls"),
                Obj("obj_warden", ObjectiveType.DefeatCombat, "n14", "mos42", "warden"),
                Obj("obj_council", ObjectiveType.ReachCase, "n18", "mos42", "council"),
            };
            var encounters = new List<EncounterDef>
            {
                Enc(CombatRank.Easy, "pr_kremlin_guard", "pr_technician"),
                Enc(CombatRank.Easy, "pr_operator", "pr_technician"),
                Enc(CombatRank.Normal, "pr_kremlin_guard", "pr_technician", "pr_operator"),
                Enc(CombatRank.Normal, "mos_guard", "pr_technician", "pr_technician"),
                Enc(CombatRank.Elite, "pr_commandant", "pr_technician", "pr_kremlin_guard"),
            };
            return new ZoneDef(Zone42, "zones.mos_4_2.name", new CaseId("n00"), PrototypeZones.BridgeNodes(null, warden, exit, "survival_snow", false, true, intro), PrototypeZones.BridgeLinks(),
                               objectives, encounters, "M4.2", 4, Borough, 25, Requires(Zone42), new ZoneEnvironment(Biome.Industrial, Weather.Fog));
        }

        /// <summary>Salle du conseil, l'offre : graphe C. Première apparition de l'AMIF à Moscou (équipe d'interception, élite en n14), puis l'offre du Conseiller (n22).</summary>
        static ZoneDef BuildZone43()
        {
            var intro = new NarrationDef(new[]
            {
                Ln("narrateur", "mos_4_3.intro.0"), Ln("conseiller", "mos_4_3.intro.1"), Lr("conseiller", "mos_4_3.intro.1.m", "agent_m", "mos_4_3.intro.1"), Ln("conseiller", "mos_4_3.intro.2"),
                Ln("conseiller", "mos_4_3.intro.3"), Ln("ilona_varga", "mos_4_3.intro.4"), Lr("agent_m", "mos_4_3.intro.5", "agent_m"),
            }, null);
            var amif = new NarrationDef(
                new[] { Ln("agent_amif", "mos_4_3.elite.before.0"), Ln("conseiller", "mos_4_3.elite.before.1") },
                new[] { Ln("giulia_rossi", "mos_4_3.elite.after.0") });
            var offer = new NarrationDef(null,
                new[] { Ln("conseiller", "mos_4_3.exit.0"), Ln("agent_m", "mos_4_3.exit.1") });
            var objectives = new List<ObjectiveDef>
            {
                Obj("obj_antechamber", ObjectiveType.ReachCase, "n04", "mos43", "antechamber"),
                Obj("obj_hearing", ObjectiveType.CompleteMiniGame, "n13", "mos43", "hearing"),
                Obj("obj_amif", ObjectiveType.DefeatCombat, "n14", "mos43", "amif"),
                Obj("obj_offer", ObjectiveType.ReachCase, "n22", "mos43", "offer"),
            };
            var encounters = new List<EncounterDef>
            {
                Enc(CombatRank.Easy, "pr_kremlin_guard", "pr_operator"),
                Enc(CombatRank.Easy, "pr_operator", "pr_technician"),
                Enc(CombatRank.Normal, "pr_kremlin_guard", "pr_technician", "pr_operator"),
                Enc(CombatRank.Normal, "amif_trooper", "amif_marksman", "amif_trooper"),
                Enc(CombatRank.Elite, "amif_captain", "amif_trooper", "amif_marksman"),
            };
            return new ZoneDef(Zone43, "zones.mos_4_3.name", new CaseId("n00"), ChapterGraphs.ThreeLanesNodes(amif, offer, "dialogue_dockers", "arena_training", false, intro), ChapterGraphs.ThreeLanesLinks(),
                               objectives, encounters, "M4.3", 4, Borough, 27, Requires(Zone43), new ZoneEnvironment(Biome.City, Weather.Day));
        }

        /// <summary>Salle du conseil, le Conseiller : graphe B avec boss fixe au bout (avant, après, fin de chapitre « à suivre »).</summary>
        static ZoneDef BuildZone44()
        {
            var intro = new NarrationDef(new[] { Ln("narrateur", "mos_4_4.intro.0") }, null);
            var guard = new NarrationDef(
                new[] { Ln("enforcer", "mos_4_4.elite.before.0") },
                new[] { Ln("ilona_varga", "mos_4_4.elite.after.0") });
            // Le choix (StoryChoices, AfterKey = boss.after.5, dernière réplique de la scène) s'insère entre boss.after et chapter_end ; épilogue : StoryChoices.
            // boss.after.1 n'a pas de base dans le script : seulement .m (Agent M, révélation) et .d (Agent D, constat politique).
            var conseiller = new NarrationDef(
                new[]
                {
                    Ln("conseiller", "mos_4_4.boss.before.0"), Ln("conseiller", "mos_4_4.boss.before.1"),
                    Ln("agent_m", "mos_4_4.boss.before.2"), Ln("conseiller", "mos_4_4.boss.before.3"),
                },
                new[]
                {
                    Ln("conseiller", "mos_4_4.boss.after.0"),
                    Lr("conseiller", "mos_4_4.boss.after.1.m", "agent_m", "mos_4_4.boss.after.1"), Lr("conseiller", "mos_4_4.boss.after.1.d", "agent_d", "mos_4_4.boss.after.1"),
                    Lr("agent_m", "mos_4_4.boss.after.2", "agent_m"), Lr("agent_m", "mos_4_4.boss.after.3", "agent_m"),
                    Lr("ilia", "mos_4_4.boss.after.4", "agent_m"), Lr("conseiller", "mos_4_4.boss.after.5", "agent_m"),
                    Ln("narrateur", "mos_4_4.chapter_end.0"),
                });
            var objectives = new List<ObjectiveDef>
            {
                Obj("obj_file", ObjectiveType.CompleteMiniGame, "n11", "mos44", "file"),
                Obj("obj_guard", ObjectiveType.DefeatCombat, "n14", "mos44", "guard"),
                Obj("obj_conseiller", ObjectiveType.DefeatCombat, "n18", "mos44", "conseiller"),
            };
            var encounters = new List<EncounterDef>
            {
                Enc(CombatRank.Easy, "pr_operator", "mos_guard"),
                Enc(CombatRank.Normal, "pr_kremlin_guard", "pr_technician", "pr_operator"),
                Enc(CombatRank.Normal, "amif_marksman", "amif_trooper", "pr_kremlin_guard"),
                Enc(CombatRank.Elite, "pr_commandant", "pr_kremlin_guard", "pr_operator"),
                Enc(CombatRank.Boss, "pr_conseiller", "pr_kremlin_guard", "pr_technician"),
            };
            return new ZoneDef(Zone44, "zones.mos_4_4.name", new CaseId("n00"), PrototypeZones.BridgeNodes(conseiller, guard, null, "dna_lab", true, false, intro), PrototypeZones.BridgeLinks(),
                               objectives, encounters, "M4.4", 4, Borough, 30, Requires(Zone44), new ZoneEnvironment(Biome.City, Weather.Night));
        }

        static EffectDef Dmg(int pct) => PrototypeZones.Dmg(pct);
        static EffectDef St(StatusType s, int turns, int chance = 100) => PrototypeZones.St(s, turns, chance);
        static EffectDef Shield(int pct, int turns = 2) => PrototypeZones.Shield(pct, turns);
        static SkillDef S(string id, SkillSlot slot, int cd, TargetRule t, params EffectDef[] fx) => PrototypeZones.S(id, slot, cd, t, fx);

        public static IEnumerable<EnemyDef> Enemies()
        {
            // Base : garde du Kremlin (ligne de front), technicien des salles de recherche (tireur), opérateur des forces spéciales (attaquant).
            yield return PrototypeZones.E("pr_kremlin_guard", Position.FrontLine, Family.Matter, new StatBlock(1900, 118, 115, 96), false,
                S("pr_kremlin_guard_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(100)),
                S("pr_kremlin_guard_a2", SkillSlot.A2, 4, TargetRule.Self, Shield(18), St(StatusType.Taunt, 1)));
            yield return PrototypeZones.E("pr_technician", Position.Ranged, Family.Perception, new StatBlock(950, 130, 60, 112), false,
                S("pr_technician_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(105)),
                S("pr_technician_a2", SkillSlot.A2, 4, TargetRule.SingleEnemy, Dmg(140), St(StatusType.Vulnerable, 2)));
            yield return PrototypeZones.E("pr_operator", Position.Attacker, Family.PhysicalBio, new StatBlock(1300, 142, 75, 108), false,
                S("pr_operator_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(100)),
                S("pr_operator_a2", SkillSlot.A2, 4, TargetRule.SingleEnemy, Dmg(150), St(StatusType.Bind, 1, 40)));
            // Élite : commandant de la garde.
            yield return PrototypeZones.E("pr_commandant", Position.Attacker, Family.PhysicalBio, new StatBlock(3200, 600, 115, 100), false,
                S("pr_commandant_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(110)),
                S("pr_commandant_a2", SkillSlot.A2, 3, TargetRule.AllEnemies, Dmg(75), St(StatusType.Blind, 1, 30)),
                S("pr_commandant_a3", SkillSlot.A3, 5, TargetRule.Self, Shield(20)));
            // Boss : le Conseiller. Phase 1 « Courtoisie » ; sous 66 % « Neutralité » (A2 remplacée par un coup unique) ; sous 33 % « Mains propres ».
            yield return PrototypeZones.Styled(PrototypeZones.Phased(PrototypeZones.E("pr_conseiller", Position.Attacker, Family.Perception, new StatBlock(5800, 830, 135, 100), true,
                S("pr_conseiller_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(115), St(StatusType.Mark, 2)),
                S("pr_conseiller_a2", SkillSlot.A2, 3, TargetRule.Self, Shield(22), St(StatusType.Taunt, 1)),
                S("pr_conseiller_a3", SkillSlot.A3, 5, TargetRule.AllEnemies, Dmg(85), St(StatusType.Blind, 1, 35)),
                S("pr_conseiller_a4", SkillSlot.A4, 4, TargetRule.SingleEnemy, Dmg(80), St(StatusType.Vulnerable, 2, 60))),
                PrototypeZones.Phase(66, "enemies.pr_conseiller.phase2", 10, 0, 5,
                    new[]
                    {
                        S("pr_conseiller_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(115), St(StatusType.Mark, 2)),
                        S("pr_conseiller_p2_a2", SkillSlot.A2, 3, TargetRule.SingleEnemy, Dmg(150), St(StatusType.Vulnerable, 2)),
                        S("pr_conseiller_a3", SkillSlot.A3, 5, TargetRule.AllEnemies, Dmg(85), St(StatusType.Blind, 1, 35)),
                        S("pr_conseiller_a4", SkillSlot.A4, 4, TargetRule.SingleEnemy, Dmg(80), St(StatusType.Vulnerable, 2, 60)),
                    }, AiStyle.Aggressive, lines: new[] { Ln("conseiller", "mos_4_4.boss.phase1.0") }),
                PrototypeZones.Phase(33, "enemies.pr_conseiller.phase3", 25, -10, 10, lines: new[] { Ln("conseiller", "mos_4_4.boss.phase2.0") })), AiStyle.Control);
        }
    }
}
