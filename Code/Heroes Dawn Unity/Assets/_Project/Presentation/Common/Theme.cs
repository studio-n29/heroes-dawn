using N29.HeroesDawn.Definitions;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Palette « comics moderne » (03_Art_UI_UX/02_Direction_Artistique_Audio).</summary>
    public static class Theme
    {
        public static readonly Color Primary = Hex(0xFFD23F);
        public static readonly Color Danger = Hex(0xE5484D);
        public static readonly Color Success = Hex(0x4CC96B);
        public static readonly Color Info = Hex(0x3BC7F2);
        public static readonly Color Background = Hex(0x14171F);
        public static readonly Color SurfaceDark = Hex(0x101218);
        public static readonly Color Surface = Hex(0x1E222B);
        public static readonly Color Surface2 = Hex(0x2B3140);
        public static readonly Color Surface3 = Hex(0x3A4152);
        public static readonly Color Text = Color.white;
        public static readonly Color TextDark = Hex(0x101218);
        public static readonly Color TextMuted = Hex(0xB7BCC9);
        public static readonly Color Ink = Color.black;
        public static readonly Color Overlay = new Color(0f, 0f, 0f, 0.72f);

        public static readonly Color Matter = Hex(0xF2A93B);
        public static readonly Color Elemental = Hex(0x3BC7F2);
        public static readonly Color PhysicalBio = Hex(0xE5484D);
        public static readonly Color Perception = Hex(0x9B6BF2);

        public static readonly Color Credits = Hex(0xFFD23F);
        public static readonly Color Pr = Hex(0x7FE0C8);
        public static readonly Color Hp = Hex(0x4CC96B);
        public static readonly Color HpEnemy = Hex(0xE5484D);
        public static readonly Color Gauge = Hex(0xFFD23F);
        public static readonly Color Fatigue = Hex(0xF2873B);
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
                case Rarity.Legend: return Hex(0xFFD23F);
                case Rarity.Elite: return Hex(0xC9D1E0);
                default: return Hex(0xB07A4A);
            }
        }

        public static Color CaseColor(CaseType t, CombatRank rank)
        {
            switch (t)
            {
                case CaseType.Entry: return Hex(0x5B6478);
                case CaseType.Combat:
                    return rank == CombatRank.Easy ? Hex(0xE88A4D) : rank == CombatRank.Normal ? Hex(0xE5484D) : Hex(0xA8232B);
                case CaseType.MiniGame: return Hex(0x3BC7F2);
                case CaseType.Narrative: return Hex(0x9B6BF2);
                case CaseType.Resource: return Hex(0xF2C23B);
                case CaseType.Rest: return Hex(0x4CC96B);
                default: return Hex(0x3A4152);
            }
        }

        public static Color FatigueColor(int fatigue)
            => fatigue >= 100 ? Danger : fatigue > 60 ? Fatigue : Hex(0x8FD16B);

        public static Color WithAlpha(Color c, float a) => new Color(c.r, c.g, c.b, a);

        public static Color Hex(uint rgb)
            => new Color(((rgb >> 16) & 0xFF) / 255f, ((rgb >> 8) & 0xFF) / 255f, (rgb & 0xFF) / 255f, 1f);
    }
}
