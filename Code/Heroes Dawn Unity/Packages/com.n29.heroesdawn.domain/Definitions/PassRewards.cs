#nullable enable
using System.Collections.Generic;

namespace N29.HeroesDawn.Definitions
{
    /// <summary>
    /// Récompense d'un palier du pass : Crédits, PR, Gemmes, Composants, catalyseurs, minutes d'accélérateur et cosmétiques (identifiants du catalogue).
    /// Jamais de recette, de duel ni de point de tournoi : un test le garantit.
    /// </summary>
    public sealed class PassRewardDef
    {
        public int Credits { get; }
        public int Pr { get; }
        public int Gems { get; }
        public int SpeedUpMinutes { get; }
        public IReadOnlyList<ComponentGrant> Components { get; }
        public IReadOnlyList<CatalystGrant> Catalysts { get; }
        public IReadOnlyList<string> Cosmetics { get; }

        public PassRewardDef(int credits, int pr, int gems, int speedUpMinutes, IReadOnlyList<ComponentGrant> components, IReadOnlyList<CatalystGrant> catalysts, IReadOnlyList<string> cosmetics)
        {
            Credits = credits; Pr = pr; Gems = gems; SpeedUpMinutes = speedUpMinutes; Components = components; Catalysts = catalysts; Cosmetics = cosmetics;
        }

        public static readonly PassRewardDef Empty = new PassRewardDef(0, 0, 0, 0, new ComponentGrant[0], new CatalystGrant[0], new string[0]);

        public bool IsEmpty => Credits == 0 && Pr == 0 && Gems == 0 && SpeedUpMinutes == 0 && Components.Count == 0 && Catalysts.Count == 0 && Cosmetics.Count == 0;
    }

    /// <summary>Les récompenses des deux pistes du pass, un palier par entrée (le palier 1 est à l'index 0).</summary>
    public sealed class PassRewardTable
    {
        public IReadOnlyList<PassRewardDef> Free { get; }
        public IReadOnlyList<PassRewardDef> Premium { get; }

        public PassRewardTable(IReadOnlyList<PassRewardDef> free, IReadOnlyList<PassRewardDef> premium) { Free = free; Premium = premium; }

        public static readonly PassRewardTable Empty = new PassRewardTable(new PassRewardDef[0], new PassRewardDef[0]);

        public PassRewardDef FreeAt(int level) => level >= 1 && level <= Free.Count ? Free[level - 1] : PassRewardDef.Empty;
        public PassRewardDef PremiumAt(int level) => level >= 1 && level <= Premium.Count ? Premium[level - 1] : PassRewardDef.Empty;
    }
}
