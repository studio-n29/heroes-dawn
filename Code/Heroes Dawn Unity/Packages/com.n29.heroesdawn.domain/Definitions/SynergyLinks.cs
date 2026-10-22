#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Definitions
{
    /// <summary>Niveau de lien de deux agents précis dans une escouade (1 par défaut).</summary>
    public sealed class LinkEntry
    {
        public MemberId A { get; }
        public MemberId B { get; }
        public int Level { get; }
        public LinkEntry(MemberId a, MemberId b, int level) { A = a; B = b; Level = level; }
        public bool Is(MemberId x, MemberId y) => (A == x && B == y) || (A == y && B == x);
    }

    /// <summary>
    /// Lien entre deux agents : chaque victoire remportée ensemble le fait progresser. Son niveau (1 à 5) amplifie le bonus passif
    /// de la synergie que forment leurs deux familles : +25 % de l'effet par niveau au-delà du premier.
    /// </summary>
    public static class LinkRules
    {
        public const int MaxLevel = 5;

        /// <summary>Victoires communes nécessaires pour atteindre le niveau 2, 3, 4 et 5.</summary>
        public static readonly int[] WinsForLevel = { 3, 8, 15, 25 };

        public static int Level(int wins)
        {
            int level = 1;
            foreach (var need in WinsForLevel) if (wins >= need) level++;
            return level;
        }

        /// <summary>Multiplicateur de l'effet passif en % : 100, 125, 150, 175, 200.</summary>
        public static int MultiplierPercent(int level)
        {
            int l = Math.Max(1, Math.Min(MaxLevel, level));
            return 100 + 25 * (l - 1);
        }

        /// <summary>Victoires qui manquent pour le niveau suivant (0 au niveau maximal).</summary>
        public static int WinsToNext(int wins)
        {
            int level = Level(wins);
            return level >= MaxLevel ? 0 : WinsForLevel[level - 1] - wins;
        }
    }

    /// <summary>Effets uniques que déclenche la composition de l'équipe (en plus des bonus passifs de familles).</summary>
    public enum TeamEffectKind
    {
        /// <summary>Quatre agents de quatre familles différentes : +10 % de chances de critique pour toute l'équipe.</summary>
        Quartet,
        /// <summary>Un tank, un agent de dégâts et un soutien : +1 énergie par pas de la frise.</summary>
        Trinity,
        /// <summary>Deux tanks ou plus : bouclier d'ouverture de 10 % des PV pour toute l'équipe.</summary>
        DoubleFront,
        /// <summary>Deux agents de contrôle ou plus : +20 énergie au départ du combat.</summary>
        Spark,
    }

    public static class TeamEffects
    {
        public const int QuartetCritPercent = 10;
        public const int TrinityEnergyRegen = 1;
        public const int DoubleFrontShieldPercent = 10;
        public const int SparkStartEnergy = 20;

        public static IReadOnlyList<TeamEffectKind> Active(IReadOnlyList<Family> families, IReadOnlyList<Role> roles)
        {
            var list = new List<TeamEffectKind>();
            var distinct = new HashSet<Family>(families);
            if (families.Count >= 4 && distinct.Count >= 4) list.Add(TeamEffectKind.Quartet);
            int tanks = 0, damage = 0, support = 0, control = 0;
            foreach (var r in roles)
                switch (r)
                {
                    case Role.Tank: tanks++; break;
                    case Role.Damage: damage++; break;
                    case Role.Support: support++; break;
                    case Role.Control: control++; break;
                }
            if (tanks >= 1 && damage >= 1 && support >= 1) list.Add(TeamEffectKind.Trinity);
            if (tanks >= 2) list.Add(TeamEffectKind.DoubleFront);
            if (control >= 2) list.Add(TeamEffectKind.Spark);
            return list;
        }
    }
}
