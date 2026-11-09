using N29.HeroesDawn.Definitions;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>
    /// Aube tactique : marine encre, surfaces mates, ivoire d'action et filets de laiton.
    /// Les couleurs de famille restent indépendantes de la couleur des pouvoirs.
    /// </summary>
    public static class Theme
    {
        // ---- Jetons du style ---------------------------------------------------------------------------------
        /// <summary>Contour épais de tous les panneaux, boutons et textes (violet presque noir).</summary>
        public static readonly Color Outline = Hex(0x0A111A);
        public static readonly Color Navy = Outline;
        public static readonly Color Frost = Hex(0x101C29);
        public static readonly Color FrostEdge = Hex(0x71808A);
        public static readonly Color Accent = Hex(0xD6AF5B);
        public static readonly Color AccentSoft = Hex(0xE7CF98);
        public static readonly Color AccentDark = Hex(0x95743C);
        public static readonly Color SkyTop = Hex(0x243D50);
        public static readonly Color SkyMid = Hex(0x172A3A);
        public static readonly Color SkyBottom = Hex(0x101C29);
        public static readonly Color Glow = Hex(0xE7CF98);
        /// <summary>Jaune vif des cases accessibles sur le plateau (contour animé).</summary>
        public static readonly Color Reach = Accent;

        // ---- Noms historiques ---------------------------------------------------------------------------------
        public static readonly Color Primary = Hex(0xECE7DC);
        public static readonly Color Danger = Hex(0xB63840);
        public static readonly Color Success = Hex(0x60BA9B);
        public static readonly Color Info = Hex(0x87B8CE);
        public static readonly Color Background = Hex(0x101C29);
        public static readonly Color SurfaceDark = Outline;
        public static readonly Color Surface = Hex(0x263B4C);
        public static readonly Color Surface2 = Hex(0x304959);
        public static readonly Color Surface3 = Hex(0x1B2B3B);
        /// <summary>Bleu vif des boutons secondaires (les boutons de fond Surface2/Surface3 sont repeints avec).</summary>
        public static readonly Color Secondary = Surface3;
        public static readonly Color Text = Primary;
        /// <summary>Texte sombre posé sur un aplat clair (jaune, vert, orange).</summary>
        public static readonly Color TextDark = Outline;
        public static readonly Color TextMuted = Hex(0xC1CDD2);
        public static readonly Color TextOnDark = Primary;
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

        
        static Color Soft(uint rgb) => Hex(rgb);

        public static Color Hex(uint rgb)
            => new Color(((rgb >> 16) & 0xFF) / 255f, ((rgb >> 8) & 0xFF) / 255f, (rgb & 0xFF) / 255f, 1f);
    }
}
