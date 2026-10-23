#nullable enable
using N29.HeroesDawn.Presentation.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Combat
{
    /// <summary>Bonus actif de l'équipe, affiché en pastille sous les alliés : synergie de familles ou effet de composition.</summary>
    public struct BonusChipData
    {
        public string Name;
        public string Effect;
        public Color Color;
    }

    /// <summary>Pastille de bonus (prefab du combat) : cadre à la couleur du bonus, nom puis effet.</summary>
    public sealed class BonusChipView : MonoBehaviour
    {
        [SerializeField] Image frame = null!;
        [SerializeField] TMP_Text title = null!;
        [SerializeField] TMP_Text effect = null!;

        public void Show(BonusChipData d)
        {
            frame.color = d.Color;
            title.text = d.Name;
            effect.text = d.Effect;
        }
    }
}
