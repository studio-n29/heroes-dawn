using N29.HeroesDawn.Definitions;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Palette « HUD pastel » (spec 2026-10-12 : verre givré, bleu marine, accent corail). Les noms historiques sont conservés.</summary>
    public static class Theme
    {
        // ---- Nouveaux jetons -------------------------------------------------------------------------------
        public static readonly Color Navy = Hex(0x1B2550);
        public static readonly Color Frost = Hex(0xF2F6FF);
        public static readonly Color FrostEdge = Color.white;
        public static readonly Color Accent = Hex(0xFF6B5E);
        public static readonly Color AccentSoft = Hex(0xFF8F84);
        public static readonly Color AccentDark = Hex(0xF0566A);
        public static readonly Color SkyTop = Hex(0xA9CDFA);
        public static readonly Color SkyMid = Hex(0xB7C4F5);
        public static readonly Color SkyBottom = Hex(0xF6C4D2);
        public static readonly Color Glow = Hex(0xFFD9A8);
        /// <summary>Jaune vif des cases accessibles sur le plateau (contour animé).</summary>
        public static readonly Color Reach = Hex(0xFFD23F);

        // ---- Noms historiques (valeurs pastel) -------------------------------------------------------------
        public static readonly Color Primary = AccentDark;
        public static readonly Color Danger = Accent;
        public static readonly Color Success = Hex(0x4CCB8A);
        public static readonly Color Info = Hex(0x3AA6F5);
        public static readonly Color Background = SkyMid;
        public static readonly Color SurfaceDark = Navy;
        public static readonly Color Surface = Frost;
        public static readonly Color Surface2 = Hex(0xE1EAFB);
        public static readonly Color Surface3 = Hex(0xCCD8F4);
        public static readonly Color Text = Navy;
        public static readonly Color TextDark = Navy;
        public static readonly Color TextMuted = Hex(0x3B4A8A);
        /// <summary>Texte posé sur un fond foncé (Navy, corail soutenu).</summary>
        public static readonly Color TextOnDark = Color.white;
        public static readonly Color Ink = Navy;
        public static readonly Color Overlay = new Color(Navy.r, Navy.g, Navy.b, 0.55f);

        public static readonly Color Matter = Hex(0xF7C06A);
        public static readonly Color Elemental = Hex(0x7FD8F7);
        public static readonly Color PhysicalBio = Hex(0xF08A8E);
        public static readonly Color Perception = Hex(0xB79BF7);

        public static readonly Color Credits = Hex(0xF5B83D);
        public static readonly Color Pr = Hex(0x5CCBB0);
        public static readonly Color Hp = Hex(0x5ED48A);
        public static readonly Color HpEnemy = Accent;
        public static readonly Color Gauge = Accent;
        public static readonly Color Fatigue = Hex(0xF5A05A);
        public static readonly Color Shield = Hex(0x9FD8FF);

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
                case Rarity.Legend: return Hex(0xF5C542);
                case Rarity.Elite: return Hex(0xA9B4CC);
                default: return Hex(0xC9976A);
            }
        }

        public static Color CaseColor(CaseType t, CombatRank rank)
        {
            switch (t)
            {
                case CaseType.Entry: return Soft(0x5B6478);
                case CaseType.Combat:
                    return rank == CombatRank.Easy ? Soft(0xE88A4D) : rank == CombatRank.Normal ? Soft(0xE5484D) : Soft(0xA8232B);
                case CaseType.MiniGame: return Soft(0x3BC7F2);
                case CaseType.Narrative: return Soft(0x9B6BF2);
                case CaseType.Resource: return Soft(0xF2C23B);
                case CaseType.Rest: return Soft(0x4CC96B);
                default: return Soft(0x3A4152);
            }
        }

        public static Color FatigueColor(int fatigue)
            => fatigue >= 100 ? Danger : fatigue > 60 ? Fatigue : Hex(0x8FD16B);

        public static Color WithAlpha(Color c, float a) => new Color(c.r, c.g, c.b, a);

        /// <summary>Même teinte, éclaircie de 25 % vers le blanc (adoucissement pastel).</summary>
        static Color Soft(uint rgb) => Color.Lerp(Hex(rgb), Color.white, 0.25f);

        public static Color Hex(uint rgb)
            => new Color(((rgb >> 16) & 0xFF) / 255f, ((rgb >> 8) & 0xFF) / 255f, (rgb & 0xFF) / 255f, 1f);
    }
}
