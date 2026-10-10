#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Content.Prototype
{
    /// <summary>
    /// Les 22 agents ajoutés aux 8 premiers : 30 agents pour la première ville. Dix entrent dans la boutique de recrutement, quatre rejoignent
    /// à la fin d'un arc narratif, quatre pendant un événement temporaire (week-end) et quatre sont des compagnons gagnés par une prouesse.
    /// Chaque agent a trois compétences construites sur son rôle et sa famille.
    /// </summary>
    public static class PrototypeAgents
    {
        public readonly struct Entry
        {
            public readonly string Id;
            public readonly string Name;
            public readonly Family Family;
            public readonly Role Role;
            public readonly Rarity Rarity;
            public readonly UnlockDef? Unlock;
            public Entry(string id, string name, Family family, Role role, Rarity rarity, UnlockDef? unlock = null)
            {
                Id = id; Name = name; Family = family; Role = role; Rarity = rarity; Unlock = unlock;
            }
        }

        static readonly ZoneId Z11 = PrototypeContent.Zone11;
        static readonly ZoneId Z12 = PrototypeZones.Zone12;
        static readonly ZoneId Z13 = PrototypeZones.Zone13;

        public static readonly IReadOnlyList<Entry> Roster = new[]
        {
            // Boutique de recrutement
            new Entry("bastion", "Bastion", Family.Matter, Role.Tank, Rarity.Agent),
            new Entry("prism", "Prism", Family.Matter, Role.Damage, Rarity.Elite),
            new Entry("warden", "Warden", Family.Matter, Role.Control, Rarity.Agent),
            new Entry("cinder", "Cinder", Family.Elemental, Role.Damage, Rarity.Agent),
            new Entry("volt", "Volt", Family.Elemental, Role.Control, Rarity.Elite),
            new Entry("tide", "Tide", Family.Elemental, Role.Support, Rarity.Agent),
            new Entry("sinew", "Sinew", Family.PhysicalBio, Role.Damage, Rarity.Agent),
            new Entry("marrow", "Marrow", Family.PhysicalBio, Role.Support, Rarity.Elite),
            new Entry("radar", "Radar", Family.Perception, Role.Precision, Rarity.Agent),
            new Entry("lens", "Lens", Family.Perception, Role.Control, Rarity.Elite),
            // Arcs narratifs : l'agent rejoint à la validation de la zone
            new Entry("vanguard", "Vanguard", Family.PhysicalBio, Role.Tank, Rarity.Elite, UnlockDef.Arc(Z11)),
            new Entry("oracle", "Oracle", Family.Perception, Role.Support, Rarity.Legend, UnlockDef.Arc(Z12)),
            new Entry("meteor", "Meteor", Family.Elemental, Role.Damage, Rarity.Legend, UnlockDef.Arc(Z12)),
            new Entry("monolith", "Monolith", Family.Matter, Role.Tank, Rarity.Legend, UnlockDef.Arc(Z13)),
            // Événements temporaires (week-end) dans des zones précises
            new Entry("forge", "Forge", Family.Matter, Role.Damage, Rarity.Elite, UnlockDef.WeekendEvent(Z11)),
            new Entry("tempest", "Tempest", Family.Elemental, Role.Control, Rarity.Elite, UnlockDef.WeekendEvent(Z12)),
            new Entry("mend", "Mend", Family.PhysicalBio, Role.Support, Rarity.Elite, UnlockDef.WeekendEvent(Z12)),
            new Entry("mirage", "Mirage", Family.Perception, Role.Precision, Rarity.Elite, UnlockDef.WeekendEvent(Z13)),
            // Compagnons : une prouesse
            new Entry("lone_wolf", "Lone Wolf", Family.PhysicalBio, Role.Damage, Rarity.Legend, UnlockDef.FeatOf(Z11, FeatKind.SoloClear)),
            new Entry("hermit", "Hermit", Family.Perception, Role.Precision, Rarity.Legend, UnlockDef.FeatOf(Z13, FeatKind.SoloClear)),
            new Entry("phoenix", "Phoenix", Family.Elemental, Role.Support, Rarity.Legend, UnlockDef.FeatOf(Z12, FeatKind.NoDefeat)),
            new Entry("aegis", "Aegis", Family.Matter, Role.Tank, Rarity.Legend, UnlockDef.FeatOf(Z13, FeatKind.NoDefeat)),
        };

        static EffectDef Dmg(int pct) => new EffectDef(EffectType.Damage, pct);
        static EffectDef Heal(int pct) => new EffectDef(EffectType.Heal, pct);
        static EffectDef Shield(int pct) => new EffectDef(EffectType.Shield, pct, StatusType.Shield, 2);
        static EffectDef St(StatusType s, int turns, int chance = 100) => new EffectDef(EffectType.ApplyStatus, 0, s, turns, chance);

        static SkillDef S(string id, SkillSlot slot, int cd, TargetRule t, params EffectDef[] fx)
            => new SkillDef(new SkillId(id), slot, cd, t, fx);

        /// <summary>Statut signature de la famille, ajouté à la première compétence offensive.</summary>
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

        /// <summary>Les trois compétences d'un agent : A1 légère, A2 intermédiaire, A3 lourde ou de soutien selon le rôle.</summary>
        public static IReadOnlyList<SkillDef> Kit(Entry e)
        {
            string a1 = e.Id + "_a1", a2 = e.Id + "_a2", a3 = e.Id + "_a3";
            switch (e.Role)
            {
                case Role.Tank:
                    return new[]
                    {
                        S(a1, SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(110), new EffectDef(EffectType.PushBack, 10)),
                        S(a2, SkillSlot.A2, 3, TargetRule.Self, St(StatusType.Taunt, 2), Shield(20)),
                        S(a3, SkillSlot.A3, 5, TargetRule.AllAllies, Shield(15), St(StatusType.Anchor, 2)),
                    };
                case Role.Damage:
                    return new[]
                    {
                        S(a1, SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(100), Signature(e.Family)),
                        S(a2, SkillSlot.A2, 3, TargetRule.AllEnemies, Dmg(70)),
                        S(a3, SkillSlot.A3, 5, TargetRule.SingleEnemy, Dmg(220)),
                    };
                case Role.Control:
                    return new[]
                    {
                        S(a1, SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(100), St(StatusType.Shock, 1, 15)),
                        S(a2, SkillSlot.A2, 4, TargetRule.SingleEnemy, St(StatusType.Bind, 2), new EffectDef(EffectType.PushBack, 25)),
                        S(a3, SkillSlot.A3, 5, TargetRule.AllEnemies, Dmg(60), St(StatusType.Vulnerable, 2)),
                    };
                case Role.Support:
                    return new[]
                    {
                        S(a1, SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(80)),
                        S(a2, SkillSlot.A2, 3, TargetRule.SingleAlly, Heal(30), new EffectDef(EffectType.Advance, 20)),
                        S(a3, SkillSlot.A3, 5, TargetRule.AllAllies, St(StatusType.Regen, 2), St(StatusType.Haste, 2)),
                    };
                default: // Précision
                    return new[]
                    {
                        S(a1, SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(100), St(StatusType.Mark, 99)),
                        S(a2, SkillSlot.A2, 3, TargetRule.AllEnemies, St(StatusType.Vulnerable, 2), new EffectDef(EffectType.GaugeDelta, -100)),
                        S(a3, SkillSlot.A3, 5, TargetRule.AllEnemies, Dmg(70)),
                    };
            }
        }

        public static IEnumerable<MemberDef> Members(System.Func<Role, Rarity, StatBlock> stats)
        {
            foreach (var e in Roster)
                yield return new MemberDef(new MemberId(e.Id), "members." + e.Id + ".name", e.Family, e.Role, e.Rarity, stats(e.Role, e.Rarity), Kit(e), null, e.Unlock);
        }
    }
}
