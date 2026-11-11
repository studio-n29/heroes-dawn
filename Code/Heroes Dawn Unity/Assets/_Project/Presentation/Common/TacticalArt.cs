#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Illustrations Aube tactique sans texte, séparées des contrôles et des données du jeu.</summary>
    public static class TacticalArt
    {
        public const string Folder = "UI/AubeTactique/";
        static readonly Dictionary<string, Sprite?> Cache = new Dictionary<string, Sprite?>();

        /// <summary>Sprite du dossier d'extension (Resources/UI/AubeTactique/Ext/<groupe>/<nom>) : agents détourés, compétences, composants, icônes…</summary>
        public static Sprite? Ext(string group, string name)
        {
            string key = "Ext/" + group + "/" + name;
            if (!Cache.TryGetValue(key, out var sprite) || sprite == null)
                Cache[key] = sprite = Resources.Load<Sprite>(Folder + key);
            return sprite;
        }

        public static Sprite? Load(string key)
        {
            if (!Cache.TryGetValue(key, out var sprite) || sprite == null)
                Cache[key] = sprite = Resources.Load<Sprite>(Folder + key);
            return sprite;
        }
    }
}
