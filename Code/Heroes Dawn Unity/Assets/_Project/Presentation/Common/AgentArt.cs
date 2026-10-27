#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>
    /// Illustrations des agents (Resources/Illustrations/Characters) et décors (Environment). L'attribution agent → illustration est une
    /// table à éditer ci-dessous : les agents sans entrée reçoivent une illustration stable (déduite de leur identifiant) en attendant leur art.
    /// </summary>
    public static class AgentArt
    {
        public const int CharacterCount = 19;
        public const int EnvironmentCount = 3;

        static readonly Dictionary<string, int> Table = new Dictionary<string, int>
        {
            ["agent_d"] = 7, ["agent_m"] = 14, ["darius_brooks"] = 16, ["mei_lin_chen"] = 8, ["freya_lindqvist"] = 3,
            ["kwame_abara"] = 4, ["giulia_rossi"] = 2, ["lucas_mendes"] = 10, ["rin_takeda"] = 11, ["amara_hughes"] = 15,
            ["ilona_varga"] = 12, ["fatou_diallo"] = 9, ["clement_lefevre"] = 13, ["youssef_haddad"] = 6, ["sam_okafor"] = 1,
            // Personnages non jouables.
            ["rayner"] = 3,
        };

        static readonly Dictionary<int, Sprite?> Cache = new Dictionary<int, Sprite?>();

        /// <summary>Illustration d'un agent (ou d'un personnage nommé) ; null si la ressource manque.</summary>
        public static Sprite? For(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
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
