#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Domain.Activities;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Fonds des mini-jeux 2D (Resources/MiniGames/bg_&lt;type&gt;.png, rasterisés depuis Art/MiniGames/*.svg) et leur teinte par environnement.</summary>
    public static class MiniGameArt
    {
        /// <summary>Les types d'activité qui ont un fond (tous les mini-jeux 2D du lot H ; le dialogue garde son propre habillage).</summary>
        public static readonly string[] Kinds =
        {
            "dna_analysis", "infiltration", "gears", "snow_survival", "rhythm", "free_throw", "rescue_engineering", "crate_pick",
        };

        static readonly Dictionary<string, Sprite?> Cache = new Dictionary<string, Sprite?>();

        /// <summary>Le fond du type d'activité, ou null s'il n'en a pas (l'absence est mémorisée).</summary>
        public static Sprite? Background(string kind)
        {
            if (!Cache.TryGetValue(kind, out var sprite))
                Cache[kind] = sprite = Resources.Load<Sprite>("MiniGames/bg_" + kind);
            return sprite;
        }

        /// <summary>Teinte du fond : la couleur du sol de l'environnement, très éclaircie (65 % de blanc) pour garder le dessin lisible.</summary>
        public static Color Tint(EnvironmentPalette palette)
        {
            var ground = new Color(((palette.Ground >> 16) & 255) / 255f, ((palette.Ground >> 8) & 255) / 255f, (palette.Ground & 255) / 255f, 1f);
            return Color.Lerp(ground, Color.white, 0.65f);
        }
    }
}
