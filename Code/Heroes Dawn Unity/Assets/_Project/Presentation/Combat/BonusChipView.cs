#nullable enable
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Combat
{
    /// <summary>Bonus actif de l'équipe, affiché en pastille sous les alliés (HUD UI Toolkit) : synergie de familles ou effet de composition.</summary>
    public struct BonusChipData
    {
        public string Name;
        public string Effect;
        public Color Color;
    }
}
