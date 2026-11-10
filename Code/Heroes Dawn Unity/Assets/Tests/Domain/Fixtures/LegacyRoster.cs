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

        static MemberDef M(string id, Family f, Position r, int ra, params SkillDef[] skills)
            => new MemberDef(new MemberId(id), "members." + id + ".name", f, r, ra, PrototypeContent.BaseStats(r, ra), skills);

        static MemberDef Locked(MemberDef d, ZoneId zone)
            => new MemberDef(d.Id, d.NameKey, d.Family, d.Position, d.Rank, d.BaseStats, d.Skills, zone);

        /// <summary>Les huit premiers agents (tests des règles de boutique, indépendants du reste de la roster).</summary>
        public static IReadOnlyList<MemberDef> Core() => new List<MemberDef>
        {
            M("ancre", Family.PhysicalBio, Position.FrontLine, 4,
                S("ancre_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(110), St(StatusType.Taunt, 1, 30), Push(15)),
                S("ancre_a2", SkillSlot.A2, 3, TargetRule.Self, St(StatusType.Taunt, 2), Shield(20)),
                S("ancre_a3", SkillSlot.A3, 5, TargetRule.AllAllies, Shield(15), St(StatusType.Anchor, 2))),
            Locked(M("verre", Family.Matter, Position.Controller, 4,
                S("verre_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(100), St(StatusType.Vulnerable, 2, 25)),
                S("verre_a2", SkillSlot.A2, 4, TargetRule.SingleEnemy, St(StatusType.Bind, 2), Push(30)),
                S("verre_a3", SkillSlot.A3, 5, TargetRule.AllAllies, Shield(15), St(StatusType.Anchor, 2))), PrototypeContent.Zone11),
            M("givre", Family.Elemental, Position.Attacker, 4,
                S("givre_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(100), St(StatusType.Freeze, 2, 40)),
                S("givre_a2", SkillSlot.A2, 3, TargetRule.AllEnemies, Dmg(70), Gauge(-150)),
                S("givre_a3", SkillSlot.A3, 5, TargetRule.SingleEnemy, Dmg(220))),
            M("echo", Family.Perception, Position.Ranged, 4,
                S("echo_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(100), St(StatusType.Mark, 99)),
                S("echo_a2", SkillSlot.A2, 3, TargetRule.AllEnemies, St(StatusType.Vulnerable, 2), Gauge(-100)),
                S("echo_a3", SkillSlot.A3, 5, TargetRule.AllEnemies, Dmg(70))),
            M("arc", Family.Elemental, Position.Attacker, 3,
                S("arc_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(100), St(StatusType.Shock, 1, 20)),
                S("arc_a2", SkillSlot.A2, 3, TargetRule.SingleAlly, St(StatusType.Haste, 2), Adv(35)),
                S("arc_a3", SkillSlot.A3, 5, TargetRule.AllEnemies, Dmg(90))),
            M("rempart", Family.PhysicalBio, Position.FrontLine, 3,
                S("rempart_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(110), Push(15)),
                S("rempart_a2", SkillSlot.A2, 3, TargetRule.SingleAlly, Shield(25)),
                S("rempart_a3", SkillSlot.A3, 5, TargetRule.Self, St(StatusType.Taunt, 2), Shield(30))),
            M("seve", Family.PhysicalBio, Position.Support, 3,
                S("seve_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(80)),
                S("seve_a2", SkillSlot.A2, 3, TargetRule.SingleAlly, Heal(30)),
                S("seve_a3", SkillSlot.A3, 5, TargetRule.AllAllies, St(StatusType.Regen, 2))),
            M("etincelle", Family.Elemental, Position.Controller, 1,
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
            new PrototypeAgents.Entry("bastion", "Bastion", Family.Matter, Position.FrontLine, 1),
            new PrototypeAgents.Entry("prism", "Prism", Family.Matter, Position.Attacker, 3),
            new PrototypeAgents.Entry("warden", "Warden", Family.Matter, Position.Controller, 1),
            new PrototypeAgents.Entry("cinder", "Cinder", Family.Elemental, Position.Attacker, 1),
            new PrototypeAgents.Entry("volt", "Volt", Family.Elemental, Position.Controller, 3),
            new PrototypeAgents.Entry("tide", "Tide", Family.Elemental, Position.Support, 1),
            new PrototypeAgents.Entry("sinew", "Sinew", Family.PhysicalBio, Position.Attacker, 1),
            new PrototypeAgents.Entry("marrow", "Marrow", Family.PhysicalBio, Position.Support, 3),
            new PrototypeAgents.Entry("radar", "Radar", Family.Perception, Position.Ranged, 1),
            new PrototypeAgents.Entry("lens", "Lens", Family.Perception, Position.Controller, 3),
            new PrototypeAgents.Entry("vanguard", "Vanguard", Family.PhysicalBio, Position.FrontLine, 3, UnlockDef.Arc(Z11)),
            new PrototypeAgents.Entry("oracle", "Oracle", Family.Perception, Position.Support, 4, UnlockDef.Arc(Z12)),
            new PrototypeAgents.Entry("meteor", "Meteor", Family.Elemental, Position.Attacker, 4, UnlockDef.Arc(Z12)),
            new PrototypeAgents.Entry("monolith", "Monolith", Family.Matter, Position.FrontLine, 4, UnlockDef.Arc(Z13)),
            new PrototypeAgents.Entry("forge", "Forge", Family.Matter, Position.Attacker, 3, UnlockDef.WeekendEvent(Z11)),
            new PrototypeAgents.Entry("tempest", "Tempest", Family.Elemental, Position.Controller, 3, UnlockDef.WeekendEvent(Z12)),
            new PrototypeAgents.Entry("mend", "Mend", Family.PhysicalBio, Position.Support, 3, UnlockDef.WeekendEvent(Z12)),
            new PrototypeAgents.Entry("mirage", "Mirage", Family.Perception, Position.Ranged, 3, UnlockDef.WeekendEvent(Z13)),
            new PrototypeAgents.Entry("lone_wolf", "Lone Wolf", Family.PhysicalBio, Position.Attacker, 4, UnlockDef.FeatOf(Z11, FeatKind.SoloClear)),
            new PrototypeAgents.Entry("hermit", "Hermit", Family.Perception, Position.Ranged, 4, UnlockDef.FeatOf(Z13, FeatKind.SoloClear)),
            new PrototypeAgents.Entry("phoenix", "Phoenix", Family.Elemental, Position.Support, 4, UnlockDef.FeatOf(Z12, FeatKind.NoDefeat)),
            new PrototypeAgents.Entry("aegis", "Aegis", Family.Matter, Position.FrontLine, 4, UnlockDef.FeatOf(Z13, FeatKind.NoDefeat)),
        };

        static EffectDef Signature(Family f)
        {
            switch (f)
            {
                case Family.Matter: return St(StatusType.Vulnerable, 2, 25);
                case Family.Elemental: return St(StatusType.Burn, 2, 40);
                case Family.PhysicalBio: return St(StatusType.Taunt, 1, 30);
                default: return St(StatusType.Mark, 99);
            }
        }

        /// <summary>Ancien kit de trois compétences construit sur le rôle (A1 légère, A2 intermédiaire, A3 lourde ou de soutien).</summary>
        static IReadOnlyList<SkillDef> LegacyKit(PrototypeAgents.Entry e)
        {
            string a1 = e.Id + "_a1", a2 = e.Id + "_a2", a3 = e.Id + "_a3";
            switch (e.Position)
            {
                case Position.FrontLine:
                    return new[]
                    {
                        S(a1, SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(110), Push(10)),
                        S(a2, SkillSlot.A2, 3, TargetRule.Self, St(StatusType.Taunt, 2), Shield(20)),
                        S(a3, SkillSlot.A3, 5, TargetRule.AllAllies, Shield(15), St(StatusType.Anchor, 2)),
                    };
                case Position.Attacker:
                    return new[]
                    {
                        S(a1, SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(100), Signature(e.Family)),
                        S(a2, SkillSlot.A2, 3, TargetRule.AllEnemies, Dmg(70)),
                        S(a3, SkillSlot.A3, 5, TargetRule.SingleEnemy, Dmg(220)),
                    };
                case Position.Controller:
                    return new[]
                    {
                        S(a1, SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(100), St(StatusType.Shock, 1, 15)),
                        S(a2, SkillSlot.A2, 4, TargetRule.SingleEnemy, St(StatusType.Bind, 2), Push(25)),
                        S(a3, SkillSlot.A3, 5, TargetRule.AllEnemies, Dmg(60), St(StatusType.Vulnerable, 2)),
                    };
                case Position.Support:
                    return new[]
                    {
                        S(a1, SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(80)),
                        S(a2, SkillSlot.A2, 3, TargetRule.SingleAlly, Heal(30), Adv(20)),
                        S(a3, SkillSlot.A3, 5, TargetRule.AllAllies, St(StatusType.Regen, 2), St(StatusType.Haste, 2)),
                    };
                default: // Précision
                    return new[]
                    {
                        S(a1, SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(100), St(StatusType.Mark, 99)),
                        S(a2, SkillSlot.A2, 3, TargetRule.AllEnemies, St(StatusType.Vulnerable, 2), Gauge(-100)),
                        S(a3, SkillSlot.A3, 5, TargetRule.AllEnemies, Dmg(70)),
                    };
            }
        }

        /// <summary>Les 30 anciens agents.</summary>
        public static IReadOnlyList<MemberDef> All()
        {
            var list = Core().ToList();
            list.AddRange(Extra.Select(e => new MemberDef(new MemberId(e.Id), "members." + e.Id + ".name", e.Family, e.Position, e.Rank,
                PrototypeContent.BaseStats(e.Position, e.Rank), LegacyKit(e), null, e.Unlock)));
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
