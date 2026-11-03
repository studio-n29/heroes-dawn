#nullable enable
using System.Collections.Generic;

namespace N29.HeroesDawn.Application.Features.Meta
{
    /// <summary>Fonds proposés sur la carte de profil ; le joueur les fait défiler (identifiants stockés dans <see cref="N29.HeroesDawn.Application.Ports.MetaSave.profileBackground"/>).</summary>
    public static class ProfileBackgrounds
    {
        public const string Night = "night", Dawn = "dawn", Steel = "steel";

        public static readonly IReadOnlyList<string> All = new[] { Night, Dawn, Steel };

        /// <summary>Le fond enregistré, ou le premier si rien n'a été choisi ou si l'identifiant n'existe plus.</summary>
        public static string Resolve(string? chosen)
        {
            foreach (var id in All) if (id == chosen) return id;
            return Night;
        }

        /// <summary>Fond suivant dans l'ordre de la liste (revient au premier après le dernier).</summary>
        public static string Next(string? current)
        {
            string now = Resolve(current);
            int i = 0;
            for (int k = 0; k < All.Count; k++) if (All[k] == now) i = k;
            return All[(i + 1) % All.Count];
        }
    }

    /// <summary>Rang du joueur, d'après son niveau de pass : quatre paliers, de « Recrue » à « Chef d'unité ».</summary>
    public static class ProfileRank
    {
        /// <summary>Clé de localisation du rang atteint au niveau de pass donné.</summary>
        public static string KeyFor(int passLevel)
        {
            if (passLevel < 5) return "profile.rank.recruit";
            if (passLevel < 10) return "profile.rank.agent";
            if (passLevel < 20) return "profile.rank.veteran";
            return "profile.rank.chief";
        }
    }
}
