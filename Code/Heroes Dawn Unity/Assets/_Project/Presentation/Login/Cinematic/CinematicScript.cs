#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Login
{
    /// <summary>
    /// Les 46 plans de la cinématique de lancement (02_Lore_Narration/05_Cinematique_Lancement.md), avec leurs mouvements de caméra.
    /// Les illustrations sont celles du Drive, recadrées par la caméra ; les plans sans illustration (ADN, moniteur, feux d'artifice,
    /// supernova, Terre, étoile finale) sont dessinés par code. Les durées sont les minimums du document : la voix off peut les allonger.
    /// </summary>
    public static class CinematicScript
    {
        public static readonly IReadOnlyList<CinematicShot> Shots = Build();

        static CamPose P(float scale, float fx, float fy) => new CamPose(scale, fx, fy);

        static readonly Color Blue = new Color(0.45f, 0.75f, 1f, 0.9f);
        static readonly Color Green = new Color(0.35f, 1f, 0.45f, 0.9f);
        static readonly Color Gold = new Color(1f, 0.8f, 0.3f, 0.95f);
        static readonly Color Red = new Color(1f, 0.25f, 0.18f, 0.9f);
        static readonly Color Candle = new Color(1f, 0.65f, 0.25f, 0.9f);

        static IReadOnlyList<CinematicShot> Build() => new[]
        {
            // ---------------------------------------------------------------- Séquence 0 : ouverture
            new CinematicShot { Id = "S01", Seconds = 6, Proc = CinematicProc.Ember, Fx = CinematicFx.Embers, In = CinematicIn.FromBlack, FadeOut = true,
                Music = CinematicMusic.Silence, Sfx = "sfx_note", SfxAt = 0.3f, SfxVolume = 0.7f },

            // ---------------------------------------------------------------- Séquence 1 : 1973, Frank Khost
            new CinematicShot { Id = "S02", Seconds = 6, Art = "lab", From = P(1.0f, 0.22f, 0.30f), To = P(1.35f, 0.17f, 0.28f), Grade = CinematicGrade.Archive,
                Fx = CinematicFx.Dust | CinematicFx.Flicker, In = CinematicIn.FromBlack, Title = "1973", Music = CinematicMusic.Cold },
            new CinematicShot { Id = "S03", Seconds = 5, Proc = CinematicProc.Helix, Grade = CinematicGrade.Archive, Fx = CinematicFx.Dust, Sfx = "sfx_beep", SfxAt = 0.4f, SfxVolume = 0.25f },
            new CinematicShot { Id = "S04", Seconds = 5, Art = "lab", From = P(2.0f, 0.18f, 0.22f), To = P(2.5f, 0.17f, 0.2f), Grade = CinematicGrade.Archive, Voice = false, FadeOut = true,
                Fx = CinematicFx.Dust, Glows = new[] { new CinematicGlow(0.17f, 0.2f, 360, Blue, 1.6f) } },

            // ---------------------------------------------------------------- Séquence 2 : 1981, l'Apex Unit
            new CinematicShot { Id = "S05", Seconds = 5, Art = "flame", From = P(1.9f, 0.82f, 0.28f), To = P(1.35f, 0.78f, 0.34f), Grade = CinematicGrade.Archive, In = CinematicIn.FromBlack,
                Fx = CinematicFx.Smoke | CinematicFx.Dust, Title = "1981", Music = CinematicMusic.War, SmokeColor = new Color(0.7f, 0.65f, 0.55f),
                Glows = new[] { new CinematicGlow(0.30f, 0.55f, 120, Red, 2.4f), new CinematicGlow(0.62f, 0.62f, 100, Green, 1.7f) } },
            new CinematicShot { Id = "S06", Seconds = 5, Art = "west", From = P(1.1f, 0.55f, 0.55f), To = P(1.4f, 0.4f, 0.4f), Grade = CinematicGrade.Archive, Fx = CinematicFx.Dust, Sfx = "sfx_impact", SfxAt = 3.2f, SfxVolume = 0.3f },
            new CinematicShot { Id = "S07", Seconds = 5, Art = "rayner", From = P(1.1f, 0.55f, 0.35f), To = P(1.45f, 0.5f, 0.3f), Grade = CinematicGrade.Archive, Fx = CinematicFx.BlueSparks | CinematicFx.Flicker,
                Glows = new[] { new CinematicGlow(0.47f, 0.28f, 220, Blue, 3f), new CinematicGlow(0.62f, 0.84f, 260, Blue, 2.2f) } },
            new CinematicShot { Id = "S08", Seconds = 5, Art = "guerin", From = P(1.15f, 0.6f, 0.4f), To = P(1.5f, 0.45f, 0.38f), Grade = CinematicGrade.Archive, Fx = CinematicFx.Dust | CinematicFx.GoldSparks },
            new CinematicShot { Id = "S09", Seconds = 5, Art = "bronson", From = P(1.5f, 0.25f, 0.3f), To = P(2.6f, 0.13f, 0.3f), Grade = CinematicGrade.Archive, Fx = CinematicFx.Snow,
                Glows = new[] { new CinematicGlow(0.14f, 0.3f, 130, Blue, 1.4f) }, Sfx = "sfx_heart", SfxAt = 0.4f, SfxVolume = 0.6f },
            new CinematicShot { Id = "S10", Seconds = 6, Layout = CinematicLayout.Quad, Parts = new[] { "west", "rayner", "guerin", "bronson" }, PartFocus = new[] { 0.35f, 0.55f, 0.55f, 0.2f },
                From = P(1.0f, 0.5f, 0.5f), To = P(1.18f, 0.5f, 0.42f), Grade = CinematicGrade.Archive, Fx = CinematicFx.Searchlights | CinematicFx.Dust, In = CinematicIn.GoldBurn, FadeOut = true,
                Sfx = "sfx_impact", SfxAt = 0.1f, SfxVolume = 0.9f },

            // ---------------------------------------------------------------- Séquence 3 : Volkov et 1991
            new CinematicShot { Id = "S11", Seconds = 6, Art = "greyred", From = P(1.05f, 0.5f, 0.55f), To = P(1.3f, 0.5f, 0.4f), Grade = CinematicGrade.Archive, Fx = CinematicFx.Snow | CinematicFx.RedPulse,
                In = CinematicIn.FromBlack, Title = "URSS", Music = CinematicMusic.Cold },
            new CinematicShot { Id = "S12", Seconds = 5, Proc = CinematicProc.Fireworks, Grade = CinematicGrade.Archive, Title = "1991" },
            new CinematicShot { Id = "S13", Seconds = 4, Proc = CinematicProc.CoatSnow, Grade = CinematicGrade.Archive, Fx = CinematicFx.Snow, Sfx = "sfx_wind", SfxAt = 0f, SfxVolume = 0.5f },
            new CinematicShot { Id = "S14", Seconds = 5, Art = "nyc", From = P(1.18f, 0.28f, 0.6f), To = P(1.18f, 0.7f, 0.6f), Grade = CinematicGrade.Archive, Fx = CinematicFx.Dust | CinematicFx.Embers, FadeOut = true },

            // ---------------------------------------------------------------- Séquence 4 : 2026, Phoenix Birth
            new CinematicShot { Id = "S15", Seconds = 6, Art = "fete", From = P(1.3f, 0.45f, 0.5f), To = P(1.0f, 0.45f, 0.5f), Grade = CinematicGrade.Cold, Fx = CinematicFx.Rain, In = CinematicIn.FromBlack,
                Title = "2026", Music = CinematicMusic.Piano },
            new CinematicShot { Id = "S16", Seconds = 5, Proc = CinematicProc.Monitor, Grade = CinematicGrade.Cold, Fx = CinematicFx.Frost, Sfx = "sfx_beep", SfxAt = 2.6f, SfxVolume = 0.5f },
            new CinematicShot { Id = "S17", Seconds = 5, Art = "eda_coat", From = P(1.0f, 0.5f, 0.2f), To = P(1.2f, 0.5f, 0.75f), Grade = CinematicGrade.Cold, Fx = CinematicFx.Frost | CinematicFx.Rays | CinematicFx.Dust, Sfx = "sfx_wind", SfxAt = 0.2f, SfxVolume = 0.4f },
            new CinematicShot { Id = "S18", Seconds = 5, Art = "eda_hell", From = P(2.2f, 0.3f, 0.2f), To = P(3.6f, 0.3f, 0.16f), Grade = CinematicGrade.Cold, Fx = CinematicFx.GreenFire | CinematicFx.Frost,
                Glows = new[] { new CinematicGlow(0.30f, 0.16f, 220, Green, 2.4f) }, Sfx = "sfx_surge", SfxAt = 1.6f },
            new CinematicShot { Id = "S19", Seconds = 4, Art = "hood", From = P(1.5f, 0.45f, 0.55f), To = P(1.2f, 0.45f, 0.5f), Grade = CinematicGrade.Cold, Fx = CinematicFx.Rain | CinematicFx.GreenFire, FadeOut = true,
                Glows = new[] { new CinematicGlow(0.5f, 0.58f, 160, Green, 4f) } },

            // ---------------------------------------------------------------- Séquence 5 : la X Society
            new CinematicShot { Id = "S20", Seconds = 5, Art = "greyred", From = P(1.5f, 0.52f, 0.2f), To = P(1.25f, 0.5f, 0.34f), Grade = CinematicGrade.Cold, Fx = CinematicFx.Dust | CinematicFx.RedPulse, In = CinematicIn.FromBlack,
                Music = CinematicMusic.War, Glows = new[] { new CinematicGlow(0.30f, 0.78f, 140, Red, 1.8f) } },
            new CinematicShot { Id = "S21", Seconds = 6, Art = "eda_sit", From = P(1.5f, 0.35f, 0.5f), To = P(1.0f, 0.5f, 0.5f), Grade = CinematicGrade.Cold, Fx = CinematicFx.Fire | CinematicFx.Embers | CinematicFx.Smoke,
                SmokeColor = new Color(0.25f, 0.2f, 0.2f), Music = CinematicMusic.Silence, Sfx = "sfx_fire", SfxAt = 0f },
            new CinematicShot { Id = "S22", Seconds = 5, Art = "eda_hell", From = P(1.8f, 0.3f, 0.35f), To = P(1.3f, 0.45f, 0.4f), Grade = CinematicGrade.Cold, Fx = CinematicFx.GreenFire | CinematicFx.RedSmoke,
                Glows = new[] { new CinematicGlow(0.30f, 0.17f, 200, Green, 2.2f), new CinematicGlow(0.6f, 0.5f, 360, Red, 1.5f) } },
            new CinematicShot { Id = "S23", Seconds = 5, Art = "blue", Proc = CinematicProc.Rings, From = P(1.0f, 0.5f, 0.45f), To = P(1.25f, 0.5f, 0.5f), Grade = CinematicGrade.Hell, Fx = CinematicFx.RedPulse | CinematicFx.Embers, Music = CinematicMusic.War },
            new CinematicShot { Id = "S24", Seconds = 6, Art = "mephisto", From = P(1.0f, 0.5f, 0.55f), To = P(1.3f, 0.45f, 0.3f), Grade = CinematicGrade.Hell, Fx = CinematicFx.RedSmoke | CinematicFx.Embers | CinematicFx.Lightning,
                In = CinematicIn.WhiteFlash, FadeOut = true, Sfx = "sfx_impact", SfxAt = 0.1f, SmokeColor = new Color(0.8f, 0.1f, 0.1f) },

            // ---------------------------------------------------------------- Séquence 6 : les rebelles
            new CinematicShot { Id = "S25", Seconds = 5, Art = "hood", From = P(1.1f, 0.3f, 0.5f), To = P(1.3f, 0.65f, 0.5f), Grade = CinematicGrade.Cold, Fx = CinematicFx.Searchlights | CinematicFx.Rain, In = CinematicIn.FromBlack, Music = CinematicMusic.War },
            new CinematicShot { Id = "S26", Seconds = 5, Layout = CinematicLayout.Pair, Parts = new[] { "eda_coat", "rayner" }, PartFocus = new[] { 0.42f, 0.5f }, From = P(1.05f, 0.5f, 0.5f), To = P(1.25f, 0.5f, 0.5f),
                Grade = CinematicGrade.Cold, Fx = CinematicFx.Dust, Music = CinematicMusic.Piano, Glows = new[] { new CinematicGlow(0.5f, 0.55f, 520, Candle, 5f) } },
            new CinematicShot { Id = "S27", Seconds = 5, Art = "kraken", From = P(1.0f, 0.5f, 0.5f), To = P(1.3f, 0.5f, 0.55f), Grade = CinematicGrade.Cold, Fx = CinematicFx.RedPulse | CinematicFx.Smoke, Music = CinematicMusic.War,
                SmokeColor = new Color(0.55f, 0.6f, 0.65f) },
            new CinematicShot { Id = "S28", Seconds = 5, Art = "dome", From = P(1.4f, 0.8f, 0.3f), To = P(1.0f, 0.6f, 0.45f), Grade = CinematicGrade.Cold, Fx = CinematicFx.Smoke | CinematicFx.Lightning | CinematicFx.Embers,
                SmokeColor = new Color(0.3f, 0.3f, 0.35f), Sfx = "sfx_impact", SfxAt = 1.2f, SfxVolume = 0.5f },
            new CinematicShot { Id = "S29", Seconds = 5, Art = "rayner", From = P(1.5f, 0.55f, 0.32f), To = P(1.7f, 0.57f, 0.3f), Grade = CinematicGrade.Cold, Fx = CinematicFx.BlueSparks | CinematicFx.Dust, Music = CinematicMusic.Silence,
                Sfx = "sfx_high", SfxAt = 0.6f, SfxVolume = 0.12f },
            new CinematicShot { Id = "S30", Seconds = 5, Art = "eda_hell", From = P(1.1f, 0.4f, 0.5f), To = P(1.45f, 0.35f, 0.4f), Grade = CinematicGrade.Cold, Fx = CinematicFx.GreenFire | CinematicFx.Fire | CinematicFx.Embers | CinematicFx.Lightning,
                In = CinematicIn.WhiteFlash, Music = CinematicMusic.War, Sfx = "sfx_impact", SfxAt = 0.1f, Glows = new[] { new CinematicGlow(0.3f, 0.2f, 300, Green, 3f) } },
            new CinematicShot { Id = "S31", Seconds = 5, Art = "mephisto", From = P(1.5f, 0.5f, 0.28f), To = P(1.2f, 0.5f, 0.4f), Grade = CinematicGrade.Hell, Fx = CinematicFx.Embers | CinematicFx.RedSmoke, SmokeColor = new Color(0.7f, 0.1f, 0.1f),
                Glows = new[] { new CinematicGlow(0.5f, 0.2f, 260, Red, 1.6f) } },
            new CinematicShot { Id = "S32", Seconds = 5, Art = "eda_sit", From = P(1.0f, 0.5f, 0.5f), To = P(1.25f, 0.4f, 0.5f), Grade = CinematicGrade.Cold, Fx = CinematicFx.Dust | CinematicFx.Rays, FadeOut = true, Music = CinematicMusic.Piano },

            // ---------------------------------------------------------------- Séquence 7 : Paris
            new CinematicShot { Id = "S33", Seconds = 6, Art = "paris", From = P(1.0f, 0.5f, 0.5f), To = P(1.3f, 0.6f, 0.32f), Grade = CinematicGrade.Hell, Fx = CinematicFx.Lightning | CinematicFx.Embers | CinematicFx.Smoke,
                In = CinematicIn.FromBlack, Music = CinematicMusic.War, Sfx = "sfx_surge", SfxAt = 0.2f, SmokeColor = new Color(0.55f, 0.1f, 0.25f) },
            new CinematicShot { Id = "S34", Seconds = 5, Art = "cover", From = P(1.0f, 0.6f, 0.5f), To = P(1.2f, 0.45f, 0.45f), Grade = CinematicGrade.Hell, Fx = CinematicFx.Embers | CinematicFx.GoldSparks },
            new CinematicShot { Id = "S35", Seconds = 5, Art = "rayner", From = P(1.3f, 0.6f, 0.75f), To = P(1.1f, 0.6f, 0.55f), Grade = CinematicGrade.Hell, Fx = CinematicFx.BlueSparks | CinematicFx.Embers, Music = CinematicMusic.Silence,
                Glows = new[] { new CinematicGlow(0.52f, 0.9f, 240, Blue, 3.4f) } },
            new CinematicShot { Id = "S36", Seconds = 5, Art = "eda_hell", From = P(3.0f, 0.3f, 0.17f), To = P(4.2f, 0.3f, 0.17f), Grade = CinematicGrade.None, Fx = CinematicFx.GoldSparks, Sfx = "sfx_heart", SfxAt = 0.3f,
                Glows = new[] { new CinematicGlow(0.30f, 0.17f, 260, Green, 2f, Gold) } },
            new CinematicShot { Id = "S37", Seconds = 6, Art = "eda_phoenix", Proc = CinematicProc.Wings, From = P(1.0f, 0.5f, 0.6f), To = P(1.25f, 0.5f, 0.42f), Grade = CinematicGrade.Hell,
                Fx = CinematicFx.GoldSparks | CinematicFx.Rays | CinematicFx.Embers, In = CinematicIn.GoldBurn, Music = CinematicMusic.War, Sfx = "sfx_impact", SfxAt = 0.1f, SfxVolume = 1f },
            new CinematicShot { Id = "S38", Seconds = 5, Art = "eda_hell", From = P(1.2f, 0.35f, 0.45f), To = P(1.6f, 0.5f, 0.35f), Grade = CinematicGrade.Hell, Fx = CinematicFx.RedSmoke | CinematicFx.GoldSparks | CinematicFx.Rays,
                SmokeColor = new Color(0.8f, 0.1f, 0.1f), Glows = new[] { new CinematicGlow(0.3f, 0.35f, 420, Gold, 2.2f) } },
            new CinematicShot { Id = "S39", Seconds = 5, Art = "paris", From = P(1.0f, 0.55f, 0.45f), To = P(2.4f, 0.72f, 0.28f), Grade = CinematicGrade.Hell, Fx = CinematicFx.GoldSparks | CinematicFx.Lightning,
                Glows = new[] { new CinematicGlow(0.7f, 0.3f, 90, Gold, 3f) } },
            new CinematicShot { Id = "S40", Seconds = 5, Proc = CinematicProc.Supernova, Voice = false, In = CinematicIn.WhiteFlash, Music = CinematicMusic.Silence, Sfx = "sfx_high", SfxAt = 1.4f },
            new CinematicShot { Id = "S41", Seconds = 6, Proc = CinematicProc.Globe, Sfx = "sfx_wind", SfxAt = 0.2f, SfxVolume = 0.6f, FadeOut = true },

            // ---------------------------------------------------------------- Séquence 8 : l'Aube
            new CinematicShot { Id = "S42", Seconds = 5, Art = "ruins", Proc = CinematicProc.Crystals, From = P(1.1f, 0.4f, 0.55f), To = P(1.35f, 0.5f, 0.6f), Grade = CinematicGrade.Dawn, Fx = CinematicFx.Rays | CinematicFx.Dust | CinematicFx.GoldSparks,
                In = CinematicIn.FromBlack, Music = CinematicMusic.MainTheme },
            new CinematicShot { Id = "S43", Seconds = 5, Art = "woman", From = P(1.15f, 0.5f, 0.5f), To = P(1.55f, 0.45f, 0.72f), Grade = CinematicGrade.Dawn, Fx = CinematicFx.Dust | CinematicFx.GoldSparks,
                Glows = new[] { new CinematicGlow(0.45f, 0.86f, 220, Gold, 2.4f) }, Sfx = "sfx_heart", SfxAt = 0.3f, SfxVolume = 0.35f },
            new CinematicShot { Id = "S44", Seconds = 6, Art = "nyc", From = P(1.5f, 0.25f, 0.65f), To = P(1.0f, 0.5f, 0.5f), Grade = CinematicGrade.Dawn, Fx = CinematicFx.Rays | CinematicFx.Embers | CinematicFx.GoldSparks, In = CinematicIn.GoldBurn,
                Glows = new[] { new CinematicGlow(0.69f, 0.47f, 520, Gold, 1.2f) } },
            new CinematicShot { Id = "S45", Seconds = 5, Art = "nyc", From = P(1.0f, 0.5f, 0.55f), To = P(1.4f, 0.22f, 0.58f), Grade = CinematicGrade.Dawn, Fx = CinematicFx.Dust | CinematicFx.Rays },
            new CinematicShot { Id = "S46", Seconds = 6, Proc = CinematicProc.Star, Voice = false, In = CinematicIn.GoldBurn, Fx = CinematicFx.Embers | CinematicFx.GoldSparks, Sfx = "sfx_resonance", SfxAt = 0.2f, SfxVolume = 0.9f, Logo = true },
        };
    }
}
