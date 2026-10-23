using System;
using N29.HeroesDawn.Definitions;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Palette « sombre néon » (spec 2026-10-23 : verre sombre, liseré cyan, accent corail). Les noms historiques sont conservés.</summary>
    public static class Theme
    {
        // ---- Jetons du thème sombre ------------------------------------------------------------------------
        public static readonly Color Night = Hex(0x0B1020);
        public static readonly Color NightBottom = Hex(0x1A1033);
        public static readonly Color Glass = Hex(0x141B38);
        public static readonly Color GlassHi = new Color(1f, 1f, 1f, 0.18f);
        public static readonly Color Neon = Hex(0x25E6FF);
        public static readonly Color NeonEdge = WithAlpha(Neon, 0.55f);
        public static readonly Color NeonGlow = WithAlpha(Neon, 0.25f);
        /// <summary>Noir des ombres ; l'appelant module l'alpha.</summary>
        public static readonly Color Shadow = Color.black;
        public static readonly Color Accent = Hex(0xFF6B5E);
        public static readonly Color AccentSoft = Hex(0xFF8F84);
        public static readonly Color AccentDark = Hex(0xF0566A);
        public static readonly Color FrostEdge = NeonEdge;
        /// <summary>Jaune vif des cases accessibles sur le plateau (contour animé).</summary>
        public static readonly Color Reach = Hex(0xFFD23F);

        // ---- Alias du thème pastel, supprimés au lot 4 (balayage des couleurs en dur) -----------------------
        [Obsolete("Lot 4 : remplacer par Night/Glass/Neon")] public static readonly Color Navy = Night;
        [Obsolete("Lot 4 : remplacer par Night/Glass/Neon")] public static readonly Color Frost = Glass;
        [Obsolete("Lot 4 : remplacer par Night/Glass/Neon")] public static readonly Color SkyTop = Night;
        [Obsolete("Lot 4 : remplacer par Night/Glass/Neon")] public static readonly Color SkyMid = Night;
        [Obsolete("Lot 4 : remplacer par Night/Glass/Neon")] public static readonly Color SkyBottom = NightBottom;
        [Obsolete("Lot 4 : remplacer par Night/Glass/Neon")] public static readonly Color Glow = NeonGlow;

        // ---- Noms historiques (valeurs sombres) -------------------------------------------------------------
        public static readonly Color Primary = AccentDark;
        public static readonly Color Danger = Accent;
        public static readonly Color Success = Hex(0x3DDC84);
        public static readonly Color Info = Hex(0x4DB8FF);
        public static readonly Color Background = Night;
        public static readonly Color SurfaceDark = Night;
        public static readonly Color Surface = Glass;
        public static readonly Color Surface2 = Hex(0x1B2447);
        public static readonly Color Surface3 = Hex(0x243059);
        public static readonly Color Text = Hex(0xEAF1FF);
        /// <summary>Texte posé sur un fond clair ou corail clair (Accent).</summary>
        public static readonly Color TextDark = Night;
        public static readonly Color TextMuted = Hex(0x9AA8D4);
        /// <summary>Texte posé sur un fond sombre ou sur le corail soutenu.</summary>
        public static readonly Color TextOnDark = Color.white;
        public static readonly Color Ink = Night;
        public static readonly Color Overlay = new Color(Night.r, Night.g, Night.b, 0.7f);

        public static readonly Color Matter = Hex(0xFFB547);
        public static readonly Color Elemental = Hex(0x3CD7FF);
        public static readonly Color PhysicalBio = Hex(0xFF5C7A);
        public static readonly Color Perception = Hex(0xB07CFF);

        public static readonly Color Credits = Hex(0xFFC53D);
        public static readonly Color Pr = Hex(0x2EE6C0);
        public static readonly Color Hp = Hex(0x3DDC84);
        public static readonly Color HpEnemy = Accent;
        public static readonly Color Gauge = Accent;
        public static readonly Color Fatigue = Hex(0xFF9F43);
        public static readonly Color Shield = Hex(0x7CC4FF);

        public static Color FamilyColor(Family f)
        {
            switch (f)
            {
                case Family.Matter: return Matter;
                case Family.Elemental: return Elemental;
                case Family.PhysicalBio: return PhysicalBio;
                default: return Perception;
            }
        }

        public static Color RarityColor(Rarity r)
        {
            switch (r)
            {
                case Rarity.Legend: return Hex(0xFFD24A);
                case Rarity.Elite: return Hex(0xC3CCE6);
                default: return Hex(0xD9A066);
            }
        }

        public static Color CaseColor(CaseType t, CombatRank rank)
        {
            switch (t)
            {
                case CaseType.Entry: return Hex(0x6B7690);
                case CaseType.Combat:
                    return rank == CombatRank.Easy ? Hex(0xFF8F45) : rank == CombatRank.Normal ? Hex(0xFF4D55) : Hex(0xC0262F);
                case CaseType.MiniGame: return Hex(0x3BC7F2);
                case CaseType.Narrative: return Hex(0xA878FF);
                case CaseType.Resource: return Hex(0xFFD23B);
                case CaseType.Rest: return Hex(0x4CE06B);
                default: return Hex(0x4A5370);
            }
        }

        public static Color FatigueColor(int fatigue)
            => fatigue >= 100 ? Danger : fatigue > 60 ? Fatigue : Hex(0x7BE06B);

        public static Color WithAlpha(Color c, float a) => new Color(c.r, c.g, c.b, a);

        public static Color Hex(uint rgb)
            => new Color(((rgb >> 16) & 0xFF) / 255f, ((rgb >> 8) & 0xFF) / 255f, (rgb & 0xFF) / 255f, 1f);
    }
}
