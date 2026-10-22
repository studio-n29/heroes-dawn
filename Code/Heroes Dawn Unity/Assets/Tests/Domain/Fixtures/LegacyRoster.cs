#nullable enable
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>
    /// Anciens agents du prototype (Ancre, Verre, Givre, Écho… et les 22 ajoutés), conservés comme jeu de données de test
    /// pour les tests de mécaniques (combat, boutique, méta) qui s'appuient sur leurs statistiques et leurs compétences.
    /// Le jeu livré n'expose que les agents de The Dawn (<see cref="PrototypeAgents"/>).
    /// </summary>
    public static class LegacyRoster
    {
        public static readonly string[] StarterTeam = { "ancre", "givre", "echo", "seve" };

        static EffectDef Dmg(int pct) => new EffectDef(EffectType.Damage, pct);
        static EffectDef St(StatusType s, int turns, int chance = 100) => new EffectDef(EffectType.ApplyStatus, 0, s, turns, chance);
        static EffectDef Heal(int pct) => new EffectDef(EffectType.Heal, pct);
        static EffectDef Shield(int pct, int turns = 2) => new EffectDef(EffectType.Shield, pct, StatusType.Shield, turns);
        static EffectDef Gauge(int points) => new EffectDef(EffectType.GaugeDelta, points);
        static EffectDef Push(int pct) => new EffectDef(EffectType.PushBack, pct);
        static EffectDef Adv(int pct) => new EffectDef(EffectType.Advance, pct);
        static SkillDef S(string id, SkillSlot slot, int cd, TargetRule t, params EffectDef[] fx) => new SkillDef(new SkillId(id), slot, cd, t, fx);

        static MemberDef M(string id, Family f, Role r, Rarity ra, params SkillDef[] skills)
            => new MemberDef(new MemberId(id), "members." + id + ".name", f, r, ra, PrototypeContent.BaseStats(r, ra), skills);

        static MemberDef Locked(MemberDef d, ZoneId zone)
            => new MemberDef(d.Id, d.NameKey, d.Family, d.Role, d.Rarity, d.BaseStats, d.Skills, zone);

        /// <summary>Les huit premiers agents (tests des règles de boutique, indépendants du reste de la roster).</summary>
        public static IReadOnlyList<MemberDef> Core() => new List<MemberDef>
        {
            M("ancre", Family.PhysicalBio, Role.Tank, Rarity.Legend,
                S("ancre_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(110), St(StatusType.Taunt, 1, 30), Push(15)),
                S("ancre_a2", SkillSlot.A2, 3, TargetRule.Self, St(StatusType.Taunt, 2), Shield(20)),
                S("ancre_a3", SkillSlot.A3, 5, TargetRule.AllAllies, Shield(15), St(StatusType.Anchor, 2))),
            Locked(M("verre", Family.Matter, Role.Control, Rarity.Legend,
                S("verre_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(100), St(StatusType.Vulnerable, 2, 25)),
                S("verre_a2", SkillSlot.A2, 4, TargetRule.SingleEnemy, St(StatusType.Bind, 2), Push(30)),
                S("verre_a3", SkillSlot.A3, 5, TargetRule.AllAllies, Shield(15), St(StatusType.Anchor, 2))), PrototypeContent.Zone11),
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

        static readonly ZoneId Z11 = PrototypeContent.Zone11;
        static readonly ZoneId Z12 = PrototypeZones.Zone12;
        static readonly ZoneId Z13 = PrototypeZones.Zone13;

        /// <summary>Les 22 agents ajoutés : boutique, arcs, événements de week-end et prouesses (kits générés par rôle).</summary>
        public static readonly IReadOnlyList<PrototypeAgents.Entry> Extra = new[]
        {
            new PrototypeAgents.Entry("bastion", "Bastion", Family.Matter, Role.Tank, Rarity.Agent),
            new PrototypeAgents.Entry("prism", "Prism", Family.Matter, Role.Damage, Rarity.Elite),
            new PrototypeAgents.Entry("warden", "Warden", Family.Matter, Role.Control, Rarity.Agent),
            new PrototypeAgents.Entry("cinder", "Cinder", Family.Elemental, Role.Damage, Rarity.Agent),
            new PrototypeAgents.Entry("volt", "Volt", Family.Elemental, Role.Control, Rarity.Elite),
            new PrototypeAgents.Entry("tide", "Tide", Family.Elemental, Role.Support, Rarity.Agent),
            new PrototypeAgents.Entry("sinew", "Sinew", Family.PhysicalBio, Role.Damage, Rarity.Agent),
            new PrototypeAgents.Entry("marrow", "Marrow", Family.PhysicalBio, Role.Support, Rarity.Elite),
            new PrototypeAgents.Entry("radar", "Radar", Family.Perception, Role.Precision, Rarity.Agent),
            new PrototypeAgents.Entry("lens", "Lens", Family.Perception, Role.Control, Rarity.Elite),
            new PrototypeAgents.Entry("vanguard", "Vanguard", Family.PhysicalBio, Role.Tank, Rarity.Elite, UnlockDef.Arc(Z11)),
            new PrototypeAgents.Entry("oracle", "Oracle", Family.Perception, Role.Support, Rarity.Legend, UnlockDef.Arc(Z12)),
            new PrototypeAgents.Entry("meteor", "Meteor", Family.Elemental, Role.Damage, Rarity.Legend, UnlockDef.Arc(Z12)),
            new PrototypeAgents.Entry("monolith", "Monolith", Family.Matter, Role.Tank, Rarity.Legend, UnlockDef.Arc(Z13)),
            new PrototypeAgents.Entry("forge", "Forge", Family.Matter, Role.Damage, Rarity.Elite, UnlockDef.WeekendEvent(Z11)),
            new PrototypeAgents.Entry("tempest", "Tempest", Family.Elemental, Role.Control, Rarity.Elite, UnlockDef.WeekendEvent(Z12)),
            new PrototypeAgents.Entry("mend", "Mend", Family.PhysicalBio, Role.Support, Rarity.Elite, UnlockDef.WeekendEvent(Z12)),
            new PrototypeAgents.Entry("mirage", "Mirage", Family.Perception, Role.Precision, Rarity.Elite, UnlockDef.WeekendEvent(Z13)),
            new PrototypeAgents.Entry("lone_wolf", "Lone Wolf", Family.PhysicalBio, Role.Damage, Rarity.Legend, UnlockDef.FeatOf(Z11, FeatKind.SoloClear)),
            new PrototypeAgents.Entry("hermit", "Hermit", Family.Perception, Role.Precision, Rarity.Legend, UnlockDef.FeatOf(Z13, FeatKind.SoloClear)),
            new PrototypeAgents.Entry("phoenix", "Phoenix", Family.Elemental, Role.Support, Rarity.Legend, UnlockDef.FeatOf(Z12, FeatKind.NoDefeat)),
            new PrototypeAgents.Entry("aegis", "Aegis", Family.Matter, Role.Tank, Rarity.Legend, UnlockDef.FeatOf(Z13, FeatKind.NoDefeat)),
        };

        /// <summary>Les 30 anciens agents.</summary>
        public static IReadOnlyList<MemberDef> All()
        {
            var list = Core().ToList();
            list.AddRange(Extra.Select(e => new MemberDef(new MemberId(e.Id), "members." + e.Id + ".name", e.Family, e.Role, e.Rarity,
                PrototypeContent.BaseStats(e.Role, e.Rarity), PrototypeAgents.Kit(e), null, e.Unlock)));
            return list;
        }
    }

    /// <summary>Catalogues de test construits avec les anciens agents (voir <see cref="LegacyRoster"/>).</summary>
    public static class TestContent
    {
        public static ContentCatalog Build() => Build(new BalanceConfig());
        public static ContentCatalog Build(BalanceConfig balance) => PrototypeContent.Build(balance, LegacyRoster.All(), LegacyRoster.StarterTeam);
        public static ContentCatalog Build(BalanceConfig balance, System.Collections.Generic.IReadOnlyDictionary<string, int> activityOverrides)
            => PrototypeContent.Build(balance, activityOverrides, LegacyRoster.All(), LegacyRoster.StarterTeam);
        public static ContentCatalog BuildCore() => PrototypeContent.Build(new BalanceConfig(), LegacyRoster.Core(), LegacyRoster.StarterTeam);
    }
}
