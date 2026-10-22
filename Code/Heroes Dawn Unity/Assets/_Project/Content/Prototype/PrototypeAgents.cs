#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Content.Prototype
{
    /// <summary>
    /// Les 30 agents numérotés de The Dawn (02_Lore_Narration/04_Agents_The_Dawn.md). Quatre forment l'équipe de départ, quatorze entrent dans la
    /// boutique de recrutement (dont un après la zone 1.1), quatre rejoignent à la fin d'un arc narratif, quatre pendant un événement temporaire
    /// (week-end) et quatre sont des compagnons gagnés par une prouesse. Chaque agent a trois compétences construites sur son rôle.
    /// L'identifiant est le nom civil sans accent (« ilona_varga ») ; le nom affiché et le pouvoir sont dans <c>Loc</c>.
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
            /// <summary>Zone à valider avant l'entrée dans la boutique (nul : disponible tout de suite).</summary>
            public readonly ZoneId? ShopZone;
            public Entry(string id, string name, Family family, Role role, Rarity rarity, UnlockDef? unlock = null, ZoneId? shopZone = null)
            {
                Id = id; Name = name; Family = family; Role = role; Rarity = rarity; Unlock = unlock; ShopZone = shopZone;
            }
        }

        static readonly ZoneId Z11 = PrototypeContent.Zone11;
        static readonly ZoneId Z12 = PrototypeZones.Zone12;
        static readonly ZoneId Z13 = PrototypeZones.Zone13;

        /// <summary>Équipe d'un nouveau profil : une recrue par famille.</summary>
        public static readonly string[] StarterTeam = { "darius_brooks", "mei_lin_chen", "freya_lindqvist", "kwame_abara" };

        public static readonly IReadOnlyList<Entry> Roster = new[]
        {
            // Départ (New York)
            new Entry("darius_brooks", "Darius Brooks", Family.Matter, Role.Tank, Rarity.Agent),
            new Entry("mei_lin_chen", "Mei-Lin Chen", Family.Elemental, Role.Damage, Rarity.Agent),
            new Entry("freya_lindqvist", "Freya Lindqvist", Family.Perception, Role.Support, Rarity.Agent),
            new Entry("kwame_abara", "Kwame Abara", Family.PhysicalBio, Role.Damage, Rarity.Agent),
            // Boutique de recrutement
            new Entry("giulia_rossi", "Giulia Rossi", Family.Matter, Role.Damage, Rarity.Agent),
            new Entry("lucas_mendes", "Lucas Mendes", Family.PhysicalBio, Role.Tank, Rarity.Agent),
            new Entry("rin_takeda", "Rin Takeda", Family.Elemental, Role.Control, Rarity.Agent),
            new Entry("amara_hughes", "Amara Hughes", Family.Perception, Role.Support, Rarity.Agent, null, Z11),
            new Entry("ilona_varga", "Ilona Varga", Family.Perception, Role.Control, Rarity.Agent),
            new Entry("fatou_diallo", "Fatou Diallo", Family.PhysicalBio, Role.Tank, Rarity.Agent),
            new Entry("clement_lefevre", "Clément Lefèvre", Family.Perception, Role.Precision, Rarity.Elite),
            new Entry("youssef_haddad", "Youssef Haddad", Family.PhysicalBio, Role.Precision, Rarity.Elite),
            new Entry("severine_morel", "Séverine Morel", Family.PhysicalBio, Role.Support, Rarity.Agent),
            new Entry("sam_okafor", "Sam Okafor", Family.PhysicalBio, Role.Tank, Rarity.Agent),
            new Entry("astrid_hallstrom", "Astrid Hallström", Family.Elemental, Role.Support, Rarity.Elite),
            new Entry("celeste_moreau", "Céleste Moreau", Family.Elemental, Role.Control, Rarity.Agent),
            new Entry("haruto_nakamura", "Haruto Nakamura", Family.Perception, Role.Control, Rarity.Agent),
            new Entry("yelena_ivanova", "Yelena Ivanova", Family.Elemental, Role.Control, Rarity.Elite),
            // Arcs narratifs : l'agent rejoint à la validation de la zone
            new Entry("noah_kaplan", "Noah Kaplan", Family.Matter, Role.Control, Rarity.Agent, UnlockDef.Arc(Z11)),
            new Entry("tunde_oyelaran", "Tunde Oyelaran", Family.Matter, Role.Support, Rarity.Agent, UnlockDef.Arc(Z12)),
            new Entry("victor_sorel", "Victor Sorel", Family.Perception, Role.Control, Rarity.Legend, UnlockDef.Arc(Z12)),
            new Entry("odile_ferrand", "Odile Ferrand", Family.Perception, Role.Support, Rarity.Legend, UnlockDef.Arc(Z13)),
            // Événements temporaires (week-end) dans des zones précises
            new Entry("louis_dubois", "Louis Dubois", Family.Matter, Role.Control, Rarity.Elite, UnlockDef.WeekendEvent(Z11)),
            new Entry("marco_bellini", "Marco Bellini", Family.Matter, Role.Damage, Rarity.Elite, UnlockDef.WeekendEvent(Z12)),
            new Entry("zofia_kowalczyk", "Zofia Kowalczyk", Family.Elemental, Role.Damage, Rarity.Elite, UnlockDef.WeekendEvent(Z12)),
            new Entry("mateo_vasquez", "Mateo Vasquez", Family.PhysicalBio, Role.Support, Rarity.Elite, UnlockDef.WeekendEvent(Z13)),
            // Compagnons : une prouesse
            new Entry("ada_okonkwo", "Ada Okonkwo", Family.PhysicalBio, Role.Support, Rarity.Legend, UnlockDef.FeatOf(Z11, FeatKind.SoloClear)),
            new Entry("eero_nieminen", "Eero Nieminen", Family.Matter, Role.Damage, Rarity.Legend, UnlockDef.FeatOf(Z12, FeatKind.NoDefeat)),
            new Entry("ji_hoon_park", "Ji-Hoon Park", Family.Perception, Role.Precision, Rarity.Agent, UnlockDef.FeatOf(Z13, FeatKind.SoloClear)),
            new Entry("anna_petrov", "Anna Petrov", Family.Perception, Role.Precision, Rarity.Agent, UnlockDef.FeatOf(Z13, FeatKind.NoDefeat)),
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
                yield return new MemberDef(new MemberId(e.Id), "members." + e.Id + ".name", e.Family, e.Role, e.Rarity, stats(e.Role, e.Rarity), Kit(e), e.ShopZone, e.Unlock);
        }
    }
}
