#nullable enable
using System;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Login
{
    /// <summary>Étalonnage d'une époque (palettes du document de cinématique, §2).</summary>
    public enum CinematicGrade { None, Archive, Cold, Hell, Dawn }

    /// <summary>Effets d'ambiance posés au-dessus de l'image (particules et lumières).</summary>
    [Flags]
    public enum CinematicFx
    {
        None = 0,
        Embers = 1 << 0,
        Snow = 1 << 1,
        Rain = 1 << 2,
        Dust = 1 << 3,
        BlueSparks = 1 << 4,
        GoldSparks = 1 << 5,
        Smoke = 1 << 6,
        Fire = 1 << 7,
        Searchlights = 1 << 8,
        RedPulse = 1 << 9,
        Flicker = 1 << 10,
        Lightning = 1 << 11,
        Frost = 1 << 12,
        Rays = 1 << 13,
        GreenFire = 1 << 14,
        RedSmoke = 1 << 15,
    }

    /// <summary>Scène dessinée par code (plans sans illustration) ou motif ajouté par-dessus une illustration.</summary>
    public enum CinematicProc { None, Ember, Helix, Monitor, Fireworks, CoatSnow, Rings, Wings, Supernova, Globe, Star, Crystals }

    /// <summary>Façon d'entrer dans le plan.</summary>
    public enum CinematicIn { Dissolve, Cut, FromBlack, GoldBurn, WhiteFlash }

    /// <summary>Musique de fond : « Keep » conserve la nappe en cours.</summary>
    public enum CinematicMusic { Keep, Silence, Cold, War, Piano, MainTheme }

    /// <summary>Composition de l'image : une illustration, quatre bandes (groupe) ou deux moitiés.</summary>
    public enum CinematicLayout { Single, Quad, Pair }

    /// <summary>Pose de la caméra : échelle (1 = l'image couvre l'écran) et point regardé, en coordonnées normalisées de l'image (y vers le bas).</summary>
    public readonly struct CamPose
    {
        public readonly float Scale;
        public readonly Vector2 Focus;
        public CamPose(float scale, float fx, float fy) { Scale = scale; Focus = new Vector2(fx, fy); }
    }

    /// <summary>Lueur posée sur l'image (yeux, flamme, bougie, arcs électriques) ; elle suit la caméra.</summary>
    public readonly struct CinematicGlow
    {
        public readonly Vector2 Pos;
        public readonly float Size;
        public readonly Color Color;
        public readonly Color? ColorTo;
        public readonly float Pulse;

        public CinematicGlow(float x, float y, float size, Color color, float pulse = 1.2f, Color? to = null)
        { Pos = new Vector2(x, y); Size = size; Color = color; Pulse = pulse; ColorTo = to; }
    }

    /// <summary>
    /// Un plan de la cinématique : illustration, mouvement de caméra, étalonnage, ambiance, transition, voix off et sons.
    /// La durée est un minimum : le plan dure au moins le temps de la voix (« caler les images sur la voix »).
    /// </summary>
    public sealed class CinematicShot
    {
        public string Id = "";
        public float Seconds = 5f;
        /// <summary>Illustration (Resources/Cinematic/Art/&lt;nom&gt;) ; nul : plan entièrement dessiné par code.</summary>
        public string? Art;
        /// <summary>Illustrations des bandes ou moitiés (<see cref="CinematicLayout.Quad"/>, <see cref="CinematicLayout.Pair"/>).</summary>
        public string[]? Parts;
        public float[]? PartFocus;
        public CinematicLayout Layout = CinematicLayout.Single;
        public CamPose From = new CamPose(1f, 0.5f, 0.5f);
        public CamPose To = new CamPose(1.12f, 0.5f, 0.5f);
        public CinematicGrade Grade = CinematicGrade.None;
        public CinematicFx Fx = CinematicFx.None;
        public CinematicProc Proc = CinematicProc.None;
        public CinematicIn In = CinematicIn.Dissolve;
        /// <summary>Fondu au noir à la fin du plan (changement de séquence).</summary>
        public bool FadeOut;
        /// <summary>Titre incrusté au montage (date ou lieu).</summary>
        public string? Title;
        public CinematicMusic Music = CinematicMusic.Keep;
        /// <summary>Effet sonore (Resources/Cinematic/Audio/&lt;nom&gt;) joué à <see cref="SfxAt"/> secondes.</summary>
        public string? Sfx;
        public float SfxAt;
        public float SfxVolume = 0.8f;
        /// <summary>Une voix off existe (Resources/Cinematic/Voice/vo_&lt;Id&gt;) et son texte (cinematic.vo.&lt;Id&gt;).</summary>
        public bool Voice = true;
        public CinematicGlow[]? Glows;
        public Color? SmokeColor;
        /// <summary>Le logo du jeu s'affiche à la fin du plan (montage final).</summary>
        public bool Logo;
    }
}
