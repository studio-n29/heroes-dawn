#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>
    /// Illustrations des agents et décors. Chaque agent jouable a son illustration dans <c>Resources/Illustrations/Agents/&lt;id&gt;</c> (1280 × 720) ;
    /// les personnages sans illustration propre (Rayner, figurants) gardent une des illustrations génériques de <c>Resources/Illustrations/Characters</c>
    /// (table ci-dessous, ou choix stable déduit de l'identifiant).
    /// </summary>
    public static class AgentArt
    {
        public const int CharacterCount = 19;
        public const int EnvironmentCount = 3;

        /// <summary>Illustrations génériques des personnages non jouables (les agents jouables ont la leur, voir <see cref="For"/>).</summary>
        static readonly Dictionary<string, int> Table = new Dictionary<string, int>
        {
            ["rayner"] = 3,
        };

        static readonly Dictionary<string, Sprite?> OwnCache = new Dictionary<string, Sprite?>();

        static readonly Dictionary<int, Sprite?> Cache = new Dictionary<int, Sprite?>();

        /// <summary>Illustration d'un agent (la sienne) ou d'un personnage nommé (générique) ; null si l'identifiant est vide.</summary>
        public static Sprite? For(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (!OwnCache.TryGetValue(id, out var own))
                OwnCache[id] = own = Resources.Load<Sprite>("Illustrations/Agents/" + id);
            if (own != null) return own;
            if (!Table.TryGetValue(id, out int n)) n = 1 + Mathf.Abs(StableHash(id)) % CharacterCount;
            return Character(n);
        }

        public static Sprite? Character(int n)
        {
            int key = n;
            if (!Cache.TryGetValue(key, out var s) || s == null)
                Cache[key] = s = Resources.Load<Sprite>("Illustrations/Characters/char_" + n.ToString("D2"));
            return s;
        }

        /// <summary>Décor illustré (1 à 3), tourné par numéro d'écran pour varier les fonds.</summary>
        public static Sprite? Environment(int n)
        {
            int k = 1 + Mathf.Abs(n) % EnvironmentCount;
            int key = 1000 + k;
            if (!Cache.TryGetValue(key, out var s) || s == null)
                Cache[key] = s = Resources.Load<Sprite>("Illustrations/Environment/env_" + k.ToString("D2"));
            return s;
        }

        /// <summary>Visuel de compétence (2 illustrations pour l'instant), attribué de façon stable à partir d'une clé.</summary>
        public static Sprite? Ability(string key)
        {
            int k = 1 + Mathf.Abs(StableHash(key)) % 2;
            int cacheKey = 2000 + k;
            if (!Cache.TryGetValue(cacheKey, out var s) || s == null)
                Cache[cacheKey] = s = Resources.Load<Sprite>("Illustrations/Ability/ability_" + k.ToString("D2"));
            return s;
        }

        /// <summary>Illustration temporaire d une carte (recadrage de l art du Drive, teinté par catégorie) ; à défaut un visuel de capacité.</summary>
        public static Sprite? Card(string id) => Load("Illustrations/Cards/" + id, 3000, id) ?? Ability(id);

        /// <summary>Chemin de ressource de l'icône d'une offre de boutique (<c>Illustrations/Shop/&lt;id&gt;</c>).</summary>
        public static string ShopPath(string id) => "Illustrations/Shop/" + id;

        static readonly Dictionary<string, Sprite?> ChapterCache = new Dictionary<string, Sprite?>();

        /// <summary>Illustration d'ouverture d'un chapitre (<c>Illustrations/Chapters/&lt;id&gt;</c>) ; null si le fichier manque.</summary>
        public static Sprite? Chapter(string id)
        {
            if (!ChapterCache.TryGetValue(id, out var s)) ChapterCache[id] = s = Resources.Load<Sprite>("Illustrations/Chapters/" + id);
            return s;
        }

        static readonly Dictionary<string, Sprite?> ShopCache = new Dictionary<string, Sprite?>();

        /// <summary>Icône SVG d'une offre de boutique (booster, paquets de gemmes, échanges, cadeau) ; null si le fichier manque.</summary>
        public static Sprite? Shop(string id)
        {
            if (!ShopCache.TryGetValue(id, out var s)) ShopCache[id] = s = Resources.Load<Sprite>(ShopPath(id));
            return s;
        }

        /// <summary>Illustration temporaire d un objet (recadrage de l art du Drive, teinté par statistique) ; null si elle manque.</summary>
        public static Sprite? Item(string id) => Load("Illustrations/Items/" + id, 4000, id);

        static Sprite? Load(string path, int cacheBase, string id)
        {
            int key = cacheBase + Mathf.Abs(StableHash(id)) % 997;
            if (!Cache.TryGetValue(key, out var s) || s == null) Cache[key] = s = Resources.Load<Sprite>(path);
            return s;
        }

        static int StableHash(string id)
        {
            int h = 17;
            foreach (char c in id) h = h * 31 + c;
            return h;
        }
    }
}
