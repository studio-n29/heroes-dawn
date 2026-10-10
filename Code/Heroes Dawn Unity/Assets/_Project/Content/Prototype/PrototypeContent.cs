using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Content.Prototype
{
    /// <summary>
    /// Contenu du prototype M1 : 8 recrues, 4 ennemis (dont Brick), combos de famille, zone 1.1 (Red Hook).
    /// Stats de base = onglet « Stats » du tableur (Agent niv. 1) × multiplicateur de rareté.
    /// </summary>
    public static class PrototypeContent
    {
        public static readonly ZoneId Zone11 = new ZoneId("zone_ny_1_1");

        static EffectDef Dmg(int pct) => new EffectDef(EffectType.Damage, pct);
        static EffectDef St(StatusType s, int turns, int chance = 100) => new EffectDef(EffectType.ApplyStatus, 0, s, turns, chance);
        static EffectDef Heal(int pct) => new EffectDef(EffectType.Heal, pct);
        static EffectDef Shield(int pct, int turns = 2) => new EffectDef(EffectType.Shield, pct, StatusType.Shield, turns);
        static EffectDef Gauge(int points) => new EffectDef(EffectType.GaugeDelta, points);
        static EffectDef Push(int pct) => new EffectDef(EffectType.PushBack, pct);
        static EffectDef Adv(int pct) => new EffectDef(EffectType.Advance, pct);

        static SkillDef S(string id, SkillSlot slot, int cd, TargetRule t, params EffectDef[] fx)
            => new SkillDef(new SkillId(id), slot, cd, t, fx);

        static StatBlock Stats(Role role, Rarity rarity)
        {
            StatBlock b;
            switch (role)
            {
                case Role.Tank: b = new StatBlock(1500, 80, 120, 95); break;
                case Role.Damage: b = new StatBlock(1000, 130, 70, 105); break;
                case Role.Control: b = new StatBlock(1100, 100, 90, 110); break;
                case Role.Support: b = new StatBlock(1100, 85, 85, 100); break;
                default: b = new StatBlock(950, 120, 70, 115); break;
            }
            int m = rarity == Rarity.Elite ? 120 : rarity == Rarity.Legend ? 145 : 100;
            int spd = b.Spd + (rarity == Rarity.Elite ? 5 : rarity == Rarity.Legend ? 10 : 0);
            return new StatBlock(b.Hp * m / 100, b.Atk * m / 100, b.Def * m / 100, spd);
        }

        static MemberDef M(string id, Family f, Role r, Rarity ra, params SkillDef[] skills)
            => new MemberDef(new MemberId(id), "members." + id + ".name", f, r, ra, Stats(r, ra), skills);

        /// <summary>Recrue qui n'entre dans la boutique qu'une fois <paramref name="zone"/> validée.</summary>
        static MemberDef Locked(MemberDef d, ZoneId zone)
            => new MemberDef(d.Id, d.NameKey, d.Family, d.Role, d.Rarity, d.BaseStats, d.Skills, zone);

        static EnemyDef E(string id, Role r, Family f, StatBlock st, bool boss, params SkillDef[] skills)
            => new EnemyDef(new EnemyId(id), "enemies." + id + ".name", r, f, st, skills, boss);

        static ComboDef C(string id, Family a, Family b, TargetRule t, params EffectDef[] fx)
            => new ComboDef(id, "combos." + id + ".name", a, b, S("combo_" + id, SkillSlot.A3, 0, t, fx));

        static AvatarSkillDef A(string id, int cooldownTicks, TargetRule t, params EffectDef[] fx)
            => new AvatarSkillDef(new SkillId(id), cooldownTicks, t, fx);

        /// <summary>Compétences de l avatar du joueur (une par catégorie) ; recharge en ticks de temps actif (20 ticks = 1 s).</summary>
        static List<AvatarSkillDef> AvatarSkills() => new List<AvatarSkillDef>
        {
            A("avatar_strike", 300, TargetRule.SingleEnemy, Dmg(150), St(StatusType.Vulnerable, 2)),
            A("avatar_mend", 400, TargetRule.AllAllies, Heal(25)),
            A("avatar_barrier", 500, TargetRule.AllAllies, Shield(20), St(StatusType.Anchor, 2)),
            A("avatar_rally", 400, TargetRule.AllAllies, St(StatusType.Haste, 3), St(StatusType.Regen, 2)),
            A("avatar_rewind", 600, TargetRule.AllEnemies, Gauge(-300), St(StatusType.Vulnerable, 2)),
        };

        public static ContentCatalog Build() => Build(new BalanceConfig());

        /// <summary>Contenu avec l'équilibrage et les réglages d'activités (Resources/Balance/activities.json) donnés.</summary>
        public static ContentCatalog Build(BalanceConfig balance, IReadOnlyDictionary<string, int> activityOverrides)
            => WithActivities(Build(balance), ActivityParamsJson.Apply(PrototypeActivities.Build(balance), activityOverrides));

        static ContentCatalog WithActivities(ContentCatalog c, IReadOnlyList<ActivityDef> activities)
            => new ContentCatalog(c.Balance, c.Members, c.Enemies, c.Combos, c.Zones, c.AvatarSkills, activities);

        public static ContentCatalog Build(BalanceConfig balance)
        {
            var members = new List<MemberDef>
            {
                M("ancre", Family.PhysicalBio, Role.Tank, Rarity.Legend,
                    S("ancre_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(110), St(StatusType.Taunt, 1, 30), Push(15)),
                    S("ancre_a2", SkillSlot.A2, 3, TargetRule.Self, St(StatusType.Taunt, 2), Shield(20)),
                    S("ancre_a3", SkillSlot.A3, 5, TargetRule.AllAllies, Shield(15), St(StatusType.Anchor, 2))),
                Locked(M("verre", Family.Matter, Role.Control, Rarity.Legend,
                    S("verre_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(100), St(StatusType.Vulnerable, 2, 25)),
                    S("verre_a2", SkillSlot.A2, 4, TargetRule.SingleEnemy, St(StatusType.Bind, 2), Push(30)),
                    S("verre_a3", SkillSlot.A3, 5, TargetRule.AllAllies, Shield(15), St(StatusType.Anchor, 2))), Zone11),
                M("givre", Family.Elemental, Role.Damage, Rarity.Legend,
                    S("givre_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(100), St(StatusType.Freeze, 2, 40)),
                    S("givre_a2", SkillSlot.A2, 3, TargetRule.AllEnemies, Dmg(70), Gauge(-150)),
                    S("givre_a3", SkillSlot.A3, 5, TargetRule.SingleEnemy, Dmg(220))),
                M("echo", Family.Perception, Role.Precision, Rarity.Legend,
                    S("echo_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(100), St(StatusType.Mark, 99)),
                    S("echo_a2", SkillSlot.A2, 3, TargetRule.AllEnemies, St(StatusType.Vulnerable, 2), Gauge(-100)),
                    S("echo_a3", SkillSlot.A3, 5, TargetRule.AllEnemies, Dmg(70))),
                M("arc", Family.Elemental, Role.Damage, Rarity.Elite,
                    S("arc_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(100), St(StatusType.Shock, 1, 20)),
                    S("arc_a2", SkillSlot.A2, 3, TargetRule.SingleAlly, St(StatusType.Haste, 2), Adv(35)),
                    S("arc_a3", SkillSlot.A3, 5, TargetRule.AllEnemies, Dmg(90))),
                M("rempart", Family.PhysicalBio, Role.Tank, Rarity.Elite,
                    S("rempart_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(110), Push(15)),
                    S("rempart_a2", SkillSlot.A2, 3, TargetRule.SingleAlly, Shield(25)),
                    S("rempart_a3", SkillSlot.A3, 5, TargetRule.Self, St(StatusType.Taunt, 2), Shield(30))),
                M("seve", Family.PhysicalBio, Role.Support, Rarity.Elite,
                    S("seve_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(80)),
                    S("seve_a2", SkillSlot.A2, 3, TargetRule.SingleAlly, Heal(30)),
                    S("seve_a3", SkillSlot.A3, 5, TargetRule.AllAllies, St(StatusType.Regen, 2))),
                M("etincelle", Family.Elemental, Role.Control, Rarity.Agent,
                    S("etincelle_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(100), St(StatusType.Shock, 1, 15)),
                    S("etincelle_a2", SkillSlot.A2, 3, TargetRule.SingleAlly, Adv(60)),
                    S("etincelle_a3", SkillSlot.A3, 5, TargetRule.SingleEnemy, Dmg(150))),
            };

            var enemies = new List<EnemyDef>
            {
                E("thug", Role.Damage, Family.PhysicalBio, new StatBlock(1000, 120, 70, 100), false,
                    S("thug_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(100)),
                    S("thug_a2", SkillSlot.A2, 4, TargetRule.SingleEnemy, Dmg(160))),
                E("brute", Role.Tank, Family.PhysicalBio, new StatBlock(1500, 80, 120, 90), false,
                    S("brute_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(90)),
                    S("brute_a2", SkillSlot.A2, 4, TargetRule.Self, St(StatusType.Taunt, 2))),
                E("street_pyro", Role.Damage, Family.Elemental, new StatBlock(950, 125, 65, 105), false,
                    S("pyro_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(90), St(StatusType.Burn, 2, 30)),
                    S("pyro_a2", SkillSlot.A2, 4, TargetRule.AllEnemies, Dmg(70))),
                E("brick", Role.Tank, Family.PhysicalBio, new StatBlock(4500, 110, 130, 95), true,
                    S("brick_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(120)),
                    S("brick_a2", SkillSlot.A2, 3, TargetRule.Self, St(StatusType.Taunt, 2), Shield(10)),
                    S("brick_a3", SkillSlot.A3, 5, TargetRule.AllEnemies, Dmg(90), St(StatusType.Shock, 1, 30))),
            };

            var combos = new List<ComboDef>
            {
                C("charged_projectiles", Family.Matter, Family.Elemental, TargetRule.AllEnemies, Dmg(150), St(StatusType.Burn, 2)),
                C("forged_armor", Family.Matter, Family.PhysicalBio, TargetRule.AllAllies, Shield(30)),
                C("trap", Family.Matter, Family.Perception, TargetRule.SingleEnemy, St(StatusType.Bind, 2), Gauge(-500)),
                C("elemental_body", Family.Elemental, Family.PhysicalBio, TargetRule.AllEnemies, Dmg(120)),
                C("weak_point", Family.Elemental, Family.Perception, TargetRule.SingleEnemy, Dmg(350)),
                C("triage", Family.PhysicalBio, Family.Perception, TargetRule.AllAllies, Heal(25)),
                C("resonance_matter", Family.Matter, Family.Matter, TargetRule.AllAllies, Shield(15)),
                C("resonance_elemental", Family.Elemental, Family.Elemental, TargetRule.AllEnemies, Dmg(120)),
                C("resonance_physical", Family.PhysicalBio, Family.PhysicalBio, TargetRule.AllAllies, St(StatusType.Regen, 2)),
                C("resonance_perception", Family.Perception, Family.Perception, TargetRule.AllAllies, St(StatusType.Haste, 2)),
            };

            enemies.AddRange(PrototypeZones.Enemies());
            var zones = new List<ZoneDef> { BuildZone11() };
            zones.AddRange(PrototypeZones.Zones());
            return new ContentCatalog(balance, members, enemies, combos, zones, AvatarSkills(), PrototypeActivities.Build(balance));
        }

        /// <summary>Red Hook — les quais : 25 cases, 3 embranchements, 6 combats (1 Élite fixe : Brick).</summary>
        static ZoneDef BuildZone11()
        {
            NodeDef N(string id, CaseType t, int x, int y, bool fixedNode = false, CombatRank r = CombatRank.Normal, string activity = null, NarrationDef narration = null)
                => new NodeDef(new CaseId(id), t, fixedNode, r, x, y, activity == null ? (ActivityId?)null : new ActivityId(activity), null, narration);

            NarrationLineDef L(string speaker, string key) => new NarrationLineDef(speaker, "narration.ny_1_1.n23." + key);
            var brickTalk = new NarrationDef(
                new[] { L("brick", "before.0"), L("ancre", "before.1") },
                new[] { L("ancre", "after.0"), L("ancre", "after.1") });

            var nodes = new List<NodeDef>
            {
                N("n00", CaseType.Entry, 0, 2, true),
                N("n01", CaseType.Resource, 1, 2), N("n02", CaseType.Empty, 2, 2),
                N("n03", CaseType.Combat, 3, 3, r: CombatRank.Easy), N("n04", CaseType.Resource, 4, 3), N("n05", CaseType.Narrative, 5, 3),
                N("n06", CaseType.Resource, 3, 1), N("n07", CaseType.Combat, 4, 1, r: CombatRank.Easy), N("n08", CaseType.MiniGame, 5, 1, true),
                N("n09", CaseType.Empty, 6, 2), N("n10", CaseType.Rest, 7, 2),
                N("n11", CaseType.Combat, 8, 3), N("n12", CaseType.Resource, 9, 3), N("n13", CaseType.Narrative, 10, 3),
                N("n14", CaseType.Resource, 8, 1), N("n15", CaseType.MiniGame, 9, 1, activity: "fishing_docks"), N("n16", CaseType.Combat, 10, 1),
                N("n17", CaseType.Empty, 11, 2), N("n18", CaseType.Resource, 12, 2),
                N("n19", CaseType.Combat, 13, 3), N("n20", CaseType.Narrative, 14, 3), N("n21", CaseType.MiniGame, 13, 1, activity: "dialogue_dockers"),
                N("n22", CaseType.Rest, 14, 1), N("n23", CaseType.Combat, 15, 2, true, CombatRank.Elite, narration: brickTalk),
                N("n24", CaseType.Empty, 16, 2, true),
            };

            (string, string)[] pairs =
            {
                ("n00", "n01"), ("n01", "n02"), ("n02", "n03"), ("n02", "n06"), ("n03", "n04"), ("n04", "n05"), ("n06", "n07"), ("n07", "n08"),
                ("n05", "n09"), ("n08", "n09"), ("n09", "n10"), ("n10", "n11"), ("n10", "n14"), ("n11", "n12"), ("n12", "n13"), ("n14", "n15"),
                ("n15", "n16"), ("n13", "n17"), ("n16", "n17"), ("n17", "n18"), ("n18", "n19"), ("n18", "n21"), ("n19", "n20"), ("n21", "n22"),
                ("n20", "n23"), ("n22", "n23"), ("n23", "n24"),
            };
            var links = new List<LinkDef>();
            foreach (var (a, b) in pairs) links.Add(new LinkDef(new CaseId(a), new CaseId(b)));

            var objectives = new List<ObjectiveDef>
            {
                new ObjectiveDef("obj_warehouse", ObjectiveType.DefeatCombat, new CaseId("n23"), "objectives.ny11.warehouse"),
                new ObjectiveDef("obj_generator", ObjectiveType.CompleteMiniGame, new CaseId("n08"), "objectives.ny11.generator"),
                new ObjectiveDef("obj_crane", ObjectiveType.ReachCase, new CaseId("n24"), "objectives.ny11.crane"),
            };

            var encounters = new List<EncounterDef>
            {
                new EncounterDef(CombatRank.Easy, new[] { new EnemyId("thug"), new EnemyId("thug") }),
                new EncounterDef(CombatRank.Easy, new[] { new EnemyId("thug"), new EnemyId("street_pyro") }),
                new EncounterDef(CombatRank.Normal, new[] { new EnemyId("brute"), new EnemyId("thug"), new EnemyId("street_pyro") }),
                new EncounterDef(CombatRank.Elite, new[] { new EnemyId("brick"), new EnemyId("thug") }),
            };

            return new ZoneDef(Zone11, "zones.ny_1_1.name", new CaseId("n00"), nodes, links, objectives, encounters, "1.1", 1, "brooklyn", 5, null, new ZoneEnvironment(Biome.Harbor, Weather.Night));
        }
    }
}
