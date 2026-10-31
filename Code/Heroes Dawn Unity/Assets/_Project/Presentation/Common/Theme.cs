using System;
using N29.HeroesDawn.Definitions;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>
    /// Palette « Mo.Co » (style jeu mobile épais et lisible) : panneaux violets pleins, contour sombre épais, aplats vifs par rôle
    /// (jaune action, vert validation, orange boutique, rouge danger), texte blanc à contour. Les noms historiques sont conservés.
    /// </summary>
    public static class Theme
    {
        // ---- Jetons du style ---------------------------------------------------------------------------------
        /// <summary>Contour épais de tous les panneaux, boutons et textes (violet presque noir).</summary>
        public static readonly Color Outline = Hex(0x17123A);
        public static readonly Color Navy = Outline;
        public static readonly Color Frost = Hex(0x4B3FA0);
        public static readonly Color FrostEdge = Outline;
        public static readonly Color Accent = Hex(0xFFCF33);
        public static readonly Color AccentSoft = Hex(0xFFE27A);
        public static readonly Color AccentDark = Hex(0xE8A317);
        public static readonly Color SkyTop = Hex(0x5B49C4);
        public static readonly Color SkyMid = Hex(0x3A2F7A);
        public static readonly Color SkyBottom = Hex(0x241C57);
        public static readonly Color Glow = Hex(0xFFE27A);
        /// <summary>Jaune vif des cases accessibles sur le plateau (contour animé).</summary>
        public static readonly Color Reach = Hex(0xFFD23F);

        // ---- Noms historiques ---------------------------------------------------------------------------------
        public static readonly Color Primary = Accent;
        public static readonly Color Danger = Hex(0xE8453C);
        public static readonly Color Success = Hex(0x3AD6A0);
        public static readonly Color Info = Hex(0x4DA3FF);
        public static readonly Color Background = Hex(0x2D2566);
        public static readonly Color SurfaceDark = Outline;
        public static readonly Color Surface = Hex(0x4B3FA0);
        public static readonly Color Surface2 = Hex(0x5D58B8);
        public static readonly Color Surface3 = Hex(0x3A2F7A);
        /// <summary>Bleu vif des boutons secondaires (les boutons de fond Surface2/Surface3 sont repeints avec).</summary>
        public static readonly Color Secondary = Hex(0x5B8CFF);
        public static readonly Color Text = Color.white;
        /// <summary>Texte sombre posé sur un aplat clair (jaune, vert, orange).</summary>
        public static readonly Color TextDark = Outline;
        public static readonly Color TextMuted = Hex(0xCBC5F5);
        public static readonly Color TextOnDark = Color.white;
        public static readonly Color Ink = Outline;
        public static readonly Color Overlay = new Color(Outline.r, Outline.g, Outline.b, 0.7f);

        public static readonly Color Matter = Hex(0xFFB547);
        public static readonly Color Elemental = Hex(0x3CD7FF);
        public static readonly Color PhysicalBio = Hex(0xFF5C7A);
        public static readonly Color Perception = Hex(0xB07CFF);

        public static readonly Color Credits = Hex(0xFFC53D);
        public static readonly Color Pr = Hex(0x3AD6A0);
        public static readonly Color Hp = Hex(0x4CE06B);
        public static readonly Color HpEnemy = Danger;
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

        
        static Color Soft(uint rgb) => Hex(rgb);

        public static Color Hex(uint rgb)
            => new Color(((rgb >> 16) & 0xFF) / 255f, ((rgb >> 8) & 0xFF) / 255f, (rgb & 0xFF) / 255f, 1f);
    }
}
