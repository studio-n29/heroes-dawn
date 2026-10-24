using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Content.Prototype
{
    /// <summary>
    /// Contenu du prototype M1 : 8 recrues, 4 ennemis (dont Brick), synergies de famille, zone 1.1 (Red Hook).
    /// Stats de base = onglet « Stats » du tableur (Agent niv. 1) × multiplicateur de rareté.
    /// </summary>
    public static class PrototypeContent
    {
        public static readonly ZoneId Zone11 = new ZoneId("zone_ny_1_1");
        /// <summary>Niveau tutoriel (hors carte et hors progression).</summary>
        public static readonly ZoneId ZoneTuto = new ZoneId("zone_tuto");

        static EffectDef Dmg(int pct) => new EffectDef(EffectType.Damage, pct);
        static EffectDef St(StatusType s, int turns, int chance = 100) => new EffectDef(EffectType.ApplyStatus, 0, s, turns, chance);
        static EffectDef Heal(int pct) => new EffectDef(EffectType.Heal, pct);
        static EffectDef Shield(int pct, int turns = 2) => new EffectDef(EffectType.Shield, pct, StatusType.Shield, turns);
        static EffectDef Gauge(int points) => new EffectDef(EffectType.GaugeDelta, points);
        static EffectDef Push(int pct) => new EffectDef(EffectType.PushBack, pct);
        static EffectDef Adv(int pct) => new EffectDef(EffectType.Advance, pct);

        static SkillDef S(string id, SkillSlot slot, int cd, TargetRule t, params EffectDef[] fx)
            => new SkillDef(new SkillId(id), slot, cd, t, fx);

        /// <summary>Statistiques de départ d un agent selon son rôle et sa rareté (valeurs proposées par l outil des agents).</summary>
        public static StatBlock BaseStats(Role role, Rarity rarity) => Stats(role, rarity);

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

        static SynergyDef C(string id, Family a, Family b, SynergyStat stat, int percent)
            => new SynergyDef(id, "synergies." + id + ".name", a, b, stat, percent);


        public static ContentCatalog Build() => Build(new BalanceConfig());

        /// <summary>Contenu avec l'équilibrage et les réglages d'activités (Resources/Balance/activities.json) donnés.</summary>
        public static ContentCatalog Build(BalanceConfig balance, IReadOnlyDictionary<string, int> activityOverrides)
            => WithActivities(Build(balance), ActivityParamsJson.Apply(PrototypeActivities.Build(balance), activityOverrides));

        /// <summary>Même chose avec des agents et une équipe de départ donnés (jeux de données de test).</summary>
        public static ContentCatalog Build(BalanceConfig balance, IReadOnlyDictionary<string, int> activityOverrides, IReadOnlyList<MemberDef> agents, IReadOnlyList<string> starterTeam)
            => WithActivities(Build(balance, agents, starterTeam), ActivityParamsJson.Apply(PrototypeActivities.Build(balance), activityOverrides));

        static ContentCatalog WithActivities(ContentCatalog c, IReadOnlyList<ActivityDef> activities)
            => new ContentCatalog(c.Balance, c.Members, c.Enemies, c.Synergies, c.Zones, activities, c.Collection, c.StarterTeam);

        public static ContentCatalog Build(BalanceConfig balance) => Build(balance, PrototypeAgents.Members(Stats).ToList(), PrototypeAgents.StarterTeam);

        /// <summary>Catalogue avec ces agents et cette équipe de départ (jeux de données de test).</summary>
        public static ContentCatalog Build(BalanceConfig balance, IReadOnlyList<MemberDef> agents, IReadOnlyList<string> starterTeam)
        {
            var members = agents;
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

            var synergies = new List<SynergyDef>
            {
                C("charged_projectiles", Family.Matter, Family.Elemental, SynergyStat.Atk, 6),
                C("forged_armor", Family.Matter, Family.PhysicalBio, SynergyStat.Def, 8),
                C("trap", Family.Matter, Family.Perception, SynergyStat.Spd, 5),
                C("elemental_body", Family.Elemental, Family.PhysicalBio, SynergyStat.Hp, 6),
                C("weak_point", Family.Elemental, Family.Perception, SynergyStat.Atk, 8),
                C("triage", Family.PhysicalBio, Family.Perception, SynergyStat.Hp, 8),
                C("resonance_matter", Family.Matter, Family.Matter, SynergyStat.Def, 4),
                C("resonance_elemental", Family.Elemental, Family.Elemental, SynergyStat.Atk, 4),
                C("resonance_physical", Family.PhysicalBio, Family.PhysicalBio, SynergyStat.Hp, 4),
                C("resonance_perception", Family.Perception, Family.Perception, SynergyStat.Spd, 4),
            };

            enemies.AddRange(PrototypeZones.Enemies());
            var zones = new List<ZoneDef> { BuildZone11(), BuildTutorialZone() };
            zones.AddRange(PrototypeZones.Zones());
            return new ContentCatalog(balance, members, enemies, synergies, zones, PrototypeActivities.Build(balance), PrototypeCollection.Build(), starterTeam);
        }

        /// <summary>Niveau tutoriel : trois cases et un seul combat très facile. Il n'est pas une vraie zone (ni carte, ni récompenses, ni progression).</summary>
        static ZoneDef BuildTutorialZone()
        {
            var nodes = new List<NodeDef>
            {
                new NodeDef(new CaseId("n00"), CaseType.Entry, true, CombatRank.Normal, 0, 2, null, null, null),
                new NodeDef(new CaseId("n01"), CaseType.Empty, false, CombatRank.Normal, 1, 2, null, null, null),
                new NodeDef(new CaseId("n02"), CaseType.Combat, true, CombatRank.Easy, 2, 2, null, null, null),
            };
            var links = new List<LinkDef> { new LinkDef(new CaseId("n00"), new CaseId("n01")), new LinkDef(new CaseId("n01"), new CaseId("n02")) };
            var objectives = new List<ObjectiveDef> { new ObjectiveDef("obj_tuto", ObjectiveType.DefeatCombat, new CaseId("n02"), "objectives.tuto.fight") };
            var encounters = new List<EncounterDef> { new EncounterDef(CombatRank.Easy, new[] { new EnemyId("thug") }) };
            return new ZoneDef(ZoneTuto, "zones.tuto.name", new CaseId("n00"), nodes, links, objectives, encounters, "0", 1, ZoneDef.TutorialBorough, 1, null, new ZoneEnvironment(Biome.Harbor, Weather.Night));
        }

        /// <summary>Red Hook — les quais : 25 cases, 3 embranchements, 6 combats (1 Élite fixe : Brick).</summary>
        static ZoneDef BuildZone11()
        {
            NodeDef N(string id, CaseType t, int x, int y, bool fixedNode = false, CombatRank r = CombatRank.Normal, string activity = null, NarrationDef narration = null)
                => new NodeDef(new CaseId(id), t, fixedNode, r, x, y, activity == null ? (ActivityId?)null : new ActivityId(activity), null, narration);

            NarrationLineDef L(string speaker, string key) => new NarrationLineDef(speaker, "narration.ny_1_1.n23." + key);
            var brickTalk = new NarrationDef(
                new[] { L("brick", "before.0"), L("darius_brooks", "before.1") },
                new[] { L("darius_brooks", "after.0"), L("darius_brooks", "after.1") });

            var nodes = new List<NodeDef>
            {
                N("n00", CaseType.Entry, 0, 2, true),
                N("n01", CaseType.Resource, 1, 2), N("n02", CaseType.Empty, 2, 2),
                N("n03", CaseType.Combat, 3, 3, r: CombatRank.Easy), N("n04", CaseType.Resource, 4, 3), N("n05", CaseType.Narrative, 5, 3),
                N("n06", CaseType.Resource, 3, 1), N("n07", CaseType.Combat, 4, 1, r: CombatRank.Easy), N("n08", CaseType.MiniGame, 5, 1, true),
                N("n09", CaseType.Empty, 6, 2), N("n10", CaseType.Rest, 7, 2),
                N("n11", CaseType.Combat, 8, 3), N("n12", CaseType.Resource, 9, 3), N("n13", CaseType.Narrative, 10, 3),
                N("n14", CaseType.Resource, 8, 1), N("n15", CaseType.MiniGame, 9, 1, activity: "deflect_docks"), N("n16", CaseType.Combat, 10, 1),
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
                new ObjectiveDef("obj_infiltrate", ObjectiveType.ReachCase, new CaseId("n10"), "objectives.ny11.infiltrate"),
                new ObjectiveDef("obj_generator", ObjectiveType.CompleteMiniGame, new CaseId("n08"), "objectives.ny11.generator"),
                new ObjectiveDef("obj_documents", ObjectiveType.ReachCase, new CaseId("n18"), "objectives.ny11.documents"),
                new ObjectiveDef("obj_exchange", ObjectiveType.DefeatCombat, new CaseId("n23"), "objectives.ny11.exchange"),
                new ObjectiveDef("obj_escape", ObjectiveType.ReachCase, new CaseId("n24"), "objectives.ny11.escape"),
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
