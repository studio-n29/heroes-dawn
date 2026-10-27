#nullable enable
using System;

namespace N29.HeroesDawn.Application.Features.Meta
{
    /// <summary>Tag personnel du joueur (« Pseudo#1234 ») : le pseudo nettoyé et quatre chiffres tirés de l'identifiant du joueur.</summary>
    public static class PlayerTag
    {
        public const int MaxNameLength = 20;
        public const string GuestName = "Invité";

        /// <summary>Tag d'un joueur : le même identifiant donne toujours les mêmes quatre chiffres (FNV-1a 32 bits modulo 10 000).</summary>
        public static string Of(string playerId, string pseudo)
        {
            uint hash = 2166136261;
            foreach (char c in playerId)
            {
                hash ^= c;
                hash *= 16777619;
            }
            return Format(Clean(pseudo), (int)(hash % 10000));
        }

        static string Clean(string pseudo)
        {
            string name = pseudo.Replace("#", "").Trim();
            if (name.Length > MaxNameLength) name = name.Substring(0, MaxNameLength).Trim();
            return name.Length == 0 ? GuestName : name;
        }

        /// <summary>Lit « nom#0042 » : nom de 1 à 20 caractères sans #, puis exactement quatre chiffres.</summary>
        public static bool TryParse(string tag, out string name, out int code)
        {
            name = ""; code = 0;
            string t = tag.Trim();
            int hash = t.IndexOf('#');
            if (hash <= 0 || hash != t.LastIndexOf('#')) return false;
            string n = t.Substring(0, hash).Trim();
            string digits = t.Substring(hash + 1);
            if (n.Length == 0 || n.Length > MaxNameLength || digits.Length != 4) return false;
            foreach (char c in digits) if (c < '0' || c > '9') return false;
            name = n;
            code = int.Parse(digits);
            return true;
        }

        public static string Format(string name, int code) => name + "#" + code.ToString("D4");
    }
}
