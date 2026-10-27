#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace N29.HeroesDawn.Definitions
{
    /// <summary>Ce qu'un bonus de paire apporte : une statistique en %, ou l'une des briques de combat existantes.</summary>
    public enum PairBonusKind
    {
        HpPercent, AtkPercent, DefPercent, SpdPercent,
        /// <summary>Points de chances de critique en plus.</summary>
        CritPercent,
        /// <summary>Bouclier d'ouverture, en % des PV max.</summary>
        OpeningShieldPercent,
        /// <summary>Jauge de départ avancée, en % de la frise.</summary>
        StartGaugePercent,
        /// <summary>Points d'énergie au départ du combat.</summary>
        StartEnergy,
        /// <summary>Points d'énergie gagnés en plus à chaque pas de la frise.</summary>
        EnergyPerStep,
    }

    public readonly struct PairBonus
    {
        public PairBonusKind Kind { get; }
        public int Value { get; }
        public PairBonus(PairBonusKind kind, int value) { Kind = kind; Value = value; }
    }

    /// <summary>Somme des bonus de paire d'un agent, déjà plafonnée par type.</summary>
    public readonly struct PairBonusTotal
    {
        public int Hp { get; }
        public int Atk { get; }
        public int Def { get; }
        public int Spd { get; }
        public int Crit { get; }
        public int OpeningShield { get; }
        public int StartGauge { get; }
        public int StartEnergy { get; }
        public int EnergyPerStep { get; }

        public PairBonusTotal(int hp, int atk, int def, int spd, int crit, int openingShield, int startGauge, int startEnergy, int energyPerStep)
        {
            Hp = hp; Atk = atk; Def = def; Spd = spd; Crit = crit; OpeningShield = openingShield;
            StartGauge = startGauge; StartEnergy = startEnergy; EnergyPerStep = energyPerStep;
        }

        public bool IsNone => Hp == 0 && Atk == 0 && Def == 0 && Spd == 0 && Crit == 0 && OpeningShield == 0 && StartGauge == 0 && StartEnergy == 0 && EnergyPerStep == 0;
    }

    /// <summary>
    /// Bonus propres à chaque paire d'agents : le couple de rôles des deux agents fixe un thème et un bonus par niveau de lien (2 à 5).
    /// Les bonus s'appliquent aux deux agents de la paire, seulement quand ils sont dans la même escouade, et se cumulent (plafonnés).
    /// </summary>
    public static class PairBonusRules
    {
        public const int MinLevel = 2;
        public const int MaxLevel = 5;

        public const int HpCap = 12, AtkCap = 12, DefCap = 12, SpdCap = 12, CritCap = 15, OpeningShieldCap = 20, StartGaugeCap = 20, StartEnergyCap = 20, EnergyPerStepCap = 1;

        static PairBonus H(int v) => new PairBonus(PairBonusKind.HpPercent, v);
        static PairBonus A(int v) => new PairBonus(PairBonusKind.AtkPercent, v);
        static PairBonus D(int v) => new PairBonus(PairBonusKind.DefPercent, v);
        static PairBonus V(int v) => new PairBonus(PairBonusKind.SpdPercent, v);
        static PairBonus C(int v) => new PairBonus(PairBonusKind.CritPercent, v);
        static PairBonus S(int v) => new PairBonus(PairBonusKind.OpeningShieldPercent, v);
        static PairBonus G(int v) => new PairBonus(PairBonusKind.StartGaugePercent, v);
        static PairBonus E(int v) => new PairBonus(PairBonusKind.StartEnergy, v);
        static PairBonus R(int v) => new PairBonus(PairBonusKind.EnergyPerStep, v);

        /// <summary>Les 15 couples de rôles (premier rôle ≤ second dans l'ordre de l'enum) : le bonus de chaque niveau, 2 à 5.</summary>
        static readonly Dictionary<(Role, Role), PairBonus[]> Table = new Dictionary<(Role, Role), PairBonus[]>
        {
            [(Role.Tank, Role.Tank)] = new[] { D(4), H(4), S(8), S(7) },
            [(Role.Tank, Role.Damage)] = new[] { H(3), A(3), E(10), C(5) },
            [(Role.Tank, Role.Control)] = new[] { D(3), V(3), G(10), E(10) },
            [(Role.Tank, Role.Support)] = new[] { H(4), D(3), S(8), R(1) },
            [(Role.Tank, Role.Precision)] = new[] { D(3), C(4), G(10), C(4) },
            [(Role.Damage, Role.Damage)] = new[] { A(4), A(4), C(5), E(10) },
            [(Role.Damage, Role.Control)] = new[] { A(3), V(3), E(10), G(10) },
            [(Role.Damage, Role.Support)] = new[] { A(3), H(3), E(10), R(1) },
            [(Role.Damage, Role.Precision)] = new[] { A(3), C(4), C(4), A(4) },
            [(Role.Control, Role.Control)] = new[] { V(4), V(4), G(10), E(10) },
            [(Role.Control, Role.Support)] = new[] { H(3), V(3), E(10), R(1) },
            [(Role.Control, Role.Precision)] = new[] { V(3), C(4), G(10), C(4) },
            [(Role.Support, Role.Support)] = new[] { H(4), H(4), S(8), R(1) },
            [(Role.Support, Role.Precision)] = new[] { H(3), C(4), R(1), C(4) },
            [(Role.Precision, Role.Precision)] = new[] { C(4), V(3), C(4), G(10) },
        };

        static (Role, Role) Key(Role a, Role b) => (int)a <= (int)b ? (a, b) : (b, a);

        /// <summary>Le bonus qu'ajoute ce niveau précis (2 à 5) ; l'ordre des rôles n'a pas d'importance.</summary>
        public static PairBonus At(Role a, Role b, int level)
        {
            int l = Math.Max(MinLevel, Math.Min(MaxLevel, level));
            return Table[Key(a, b)][l - MinLevel];
        }

        /// <summary>Bonus actifs à ce niveau de lien : ceux des niveaux 2 jusqu'à <paramref name="level"/> (aucun avant le niveau 2, borné à 5).</summary>
        public static IReadOnlyList<PairBonus> Of(Role a, Role b, int level)
        {
            int top = Math.Min(MaxLevel, level);
            var list = new List<PairBonus>();
            for (int l = MinLevel; l <= top; l++) list.Add(At(a, b, l));
            return list;
        }

        /// <summary>Clé de texte du thème : <c>link.theme.tank_damage</c> (rôles en minuscules, dans l'ordre de l'enum).</summary>
        public static string ThemeKey(Role a, Role b)
        {
            var (x, y) = Key(a, b);
            return "link.theme." + x.ToString().ToLowerInvariant() + "_" + y.ToString().ToLowerInvariant();
        }

        /// <summary>Additionne les bonus de même type (ordre de première apparition), pour les afficher sans doublon.</summary>
        public static IEnumerable<PairBonus> Merge(IEnumerable<PairBonus> bonuses) =>
            bonuses.GroupBy(b => b.Kind).Select(g => new PairBonus(g.Key, g.Sum(b => b.Value)));

        /// <summary>Somme des bonus d'un agent, puis plafonds par type.</summary>
        public static PairBonusTotal Total(IEnumerable<PairBonus> bonuses)
        {
            int Sum(PairBonusKind kind, int cap) => Math.Min(cap, bonuses.Where(b => b.Kind == kind).Sum(b => b.Value));
            return new PairBonusTotal(
                Sum(PairBonusKind.HpPercent, HpCap), Sum(PairBonusKind.AtkPercent, AtkCap), Sum(PairBonusKind.DefPercent, DefCap), Sum(PairBonusKind.SpdPercent, SpdCap),
                Sum(PairBonusKind.CritPercent, CritCap), Sum(PairBonusKind.OpeningShieldPercent, OpeningShieldCap), Sum(PairBonusKind.StartGaugePercent, StartGaugeCap),
                Sum(PairBonusKind.StartEnergy, StartEnergyCap), Sum(PairBonusKind.EnergyPerStep, EnergyPerStepCap));
        }
    }
}
