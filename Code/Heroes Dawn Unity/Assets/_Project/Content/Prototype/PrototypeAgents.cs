#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Content.Prototype
{
    /// <summary>
    /// Les 30 agents numérotés de The Dawn (02_Lore_Narration/04_Agents_The_Dawn.md) et les deux personnages joueurs. Un seul forme l équipe de départ (le personnage joueur est un avatar hors effectif), quatorze entrent dans la
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

        /// <summary>Équipe d un nouveau profil qui n a pas choisi de personnage : celle d Agent D.</summary>
        public static readonly System.Collections.Generic.IReadOnlyList<string> StarterTeam = StoryRoutes.D.StarterTeam;

        public static readonly IReadOnlyList<Entry> Roster = new[]
        {
            // Personnages joueurs : le choix de la première connexion décide lequel rejoint l équipe
            new Entry(StoryRoutes.AgentD, "Agent D", Family.Matter, Role.Control, Rarity.Legend, UnlockDef.Hero()),
            new Entry(StoryRoutes.AgentM, "Agent M", Family.Perception, Role.Precision, Rarity.Legend, UnlockDef.Hero()),
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

        /// <summary>
        /// Statut propre à chaque agent, ajouté à ses deux compétences de base (premier : compétence 1, second : compétence 2). Les noms sont dans
        /// <c>LocDawnAgents</c>. Compétence 1 et, selon le rôle, compétence 2 visent l'ennemi (Brûlure, Gel, Choc, Corrosion, Vulnérable, Entrave,
        /// Marque) ou l'allié (Régénération, Hâte, Bouclier, Esquive, Ancrage).
        /// </summary>
        static readonly Dictionary<string, (StatusType First, StatusType Second)> Flavors = new Dictionary<string, (StatusType, StatusType)>
        {
            ["agent_d"] = (StatusType.Bind, StatusType.Vulnerable), ["agent_m"] = (StatusType.Shock, StatusType.Vulnerable),
            ["darius_brooks"] = (StatusType.Vulnerable, StatusType.Anchor), ["mei_lin_chen"] = (StatusType.Burn, StatusType.Burn),
            ["freya_lindqvist"] = (StatusType.Mark, StatusType.Haste), ["kwame_abara"] = (StatusType.Bind, StatusType.Vulnerable),
            ["giulia_rossi"] = (StatusType.Shock, StatusType.Vulnerable), ["lucas_mendes"] = (StatusType.Mark, StatusType.Evade),
            ["rin_takeda"] = (StatusType.Shock, StatusType.Shock), ["amara_hughes"] = (StatusType.Bind, StatusType.Regen),
            ["ilona_varga"] = (StatusType.Shock, StatusType.Mark), ["fatou_diallo"] = (StatusType.Shock, StatusType.Haste),
            ["clement_lefevre"] = (StatusType.Shock, StatusType.Bind), ["youssef_haddad"] = (StatusType.Vulnerable, StatusType.Mark),
            ["severine_morel"] = (StatusType.Corrosion, StatusType.Regen), ["sam_okafor"] = (StatusType.Shock, StatusType.Anchor),
            ["astrid_hallstrom"] = (StatusType.Freeze, StatusType.Regen), ["celeste_moreau"] = (StatusType.Corrosion, StatusType.Corrosion),
            ["haruto_nakamura"] = (StatusType.Bind, StatusType.Vulnerable), ["yelena_ivanova"] = (StatusType.Freeze, StatusType.Freeze),
            ["noah_kaplan"] = (StatusType.Shock, StatusType.Vulnerable), ["tunde_oyelaran"] = (StatusType.Bind, StatusType.Shield),
            ["victor_sorel"] = (StatusType.Mark, StatusType.Vulnerable), ["odile_ferrand"] = (StatusType.Freeze, StatusType.Haste),
            ["louis_dubois"] = (StatusType.Shock, StatusType.Shock), ["marco_bellini"] = (StatusType.Corrosion, StatusType.Corrosion),
            ["zofia_kowalczyk"] = (StatusType.Shock, StatusType.Shock), ["mateo_vasquez"] = (StatusType.Corrosion, StatusType.Regen),
            ["ada_okonkwo"] = (StatusType.Corrosion, StatusType.Regen), ["eero_nieminen"] = (StatusType.Bind, StatusType.Vulnerable),
            ["ji_hoon_park"] = (StatusType.Shock, StatusType.Vulnerable), ["anna_petrov"] = (StatusType.Shock, StatusType.Bind),
        };

        /// <summary>Statut de la famille sur la compétence 1 des agents sans statut propre.</summary>
        static StatusType Fallback(Family f)
        {
            switch (f)
            {
                case Family.Matter: return StatusType.Vulnerable;
                case Family.Elemental: return StatusType.Burn;
                case Family.PhysicalBio: return StatusType.Taunt;
                default: return StatusType.Mark;
            }
        }

        static void AddStatus(List<EffectDef> fx, StatusType s, int chance)
        {
            if (s != StatusType.None) fx.Add(St(s, 2, chance));
        }

        /// <summary>
        /// Les deux compétences de base d'un agent (deux seulement : la 3e place de l'éventail est une carte du grimoire) :
        /// 1 légère et gratuite, 2 spéciale (coût 30), construites sur son rôle avec un statut propre à l'agent.
        /// </summary>
        public static IReadOnlyList<SkillDef> Kit(Entry e)
        {
            var (first, second) = Flavors.TryGetValue(e.Id, out var f) ? f : (Fallback(e.Family), StatusType.None);
            string a1 = e.Id + "_a1", a2 = e.Id + "_a2";
            var fx1 = new List<EffectDef>();
            var fx2 = new List<EffectDef>();
            TargetRule target2;
            int cooldown2 = 3;
            switch (e.Role)
            {
                case Role.Tank:
                    fx1.Add(Dmg(110)); fx1.Add(new EffectDef(EffectType.PushBack, 10)); AddStatus(fx1, first, 30);
                    target2 = TargetRule.Self; fx2.Add(St(StatusType.Taunt, 2)); fx2.Add(Shield(25)); AddStatus(fx2, second, 100);
                    break;
                case Role.Damage:
                    fx1.Add(Dmg(100)); AddStatus(fx1, first, 40);
                    target2 = TargetRule.AllEnemies; fx2.Add(Dmg(90)); AddStatus(fx2, second, 30);
                    break;
                case Role.Control:
                    fx1.Add(Dmg(90)); AddStatus(fx1, first, 30);
                    target2 = TargetRule.SingleEnemy; fx2.Add(St(StatusType.Bind, 2)); fx2.Add(new EffectDef(EffectType.PushBack, 25)); AddStatus(fx2, second, 100);
                    cooldown2 = 4;
                    break;
                case Role.Support:
                    fx1.Add(Dmg(80)); AddStatus(fx1, first, 25);
                    target2 = TargetRule.SingleAlly; fx2.Add(Heal(35)); fx2.Add(new EffectDef(EffectType.Advance, 20)); AddStatus(fx2, second, 100);
                    break;
                default: // Précision
                    fx1.Add(Dmg(100)); fx1.Add(St(StatusType.Mark, 99)); AddStatus(fx1, first, 30);
                    target2 = TargetRule.AllEnemies; fx2.Add(St(StatusType.Vulnerable, 2)); fx2.Add(new EffectDef(EffectType.GaugeDelta, -100)); AddStatus(fx2, second, 30);
                    break;
            }
            return new[]
            {
                S(a1, SkillSlot.A1, 0, TargetRule.SingleEnemy, fx1.ToArray()),
                S(a2, SkillSlot.A2, cooldown2, target2, fx2.ToArray()),
            };
        }

        public static IEnumerable<MemberDef> Members(System.Func<Role, Rarity, StatBlock> stats)
        {
            foreach (var e in Roster)
                yield return new MemberDef(new MemberId(e.Id), "members." + e.Id + ".name", e.Family, e.Role, e.Rarity, stats(e.Role, e.Rarity), Kit(e), e.ShopZone, e.Unlock);
        }
    }
}
