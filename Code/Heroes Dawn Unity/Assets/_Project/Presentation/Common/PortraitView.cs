#nullable enable
using N29.HeroesDawn.Definitions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Ce qu'affiche un portrait provisoire : initiale, couleur de famille, couronne de boss.</summary>
    public readonly struct PortraitData
    {
        public readonly string Initial;
        public readonly Family Family;
        public readonly bool Boss;

        public PortraitData(string name, Family family, bool boss = false)
        {
            Initial = string.IsNullOrEmpty(name) ? "?" : name.Substring(0, 1).ToUpperInvariant();
            Family = family;
            Boss = boss;
        }
    }

    /// <summary>Portrait rond (widget prefab) : disque de la couleur de famille, initiale, couronne de boss.</summary>
    public sealed class PortraitView : MonoBehaviour
    {
        [SerializeField] Image disc = null!;
        [SerializeField] TMP_Text initial = null!;
        [SerializeField] GameObject boss = null!;

        public Image Disc => disc;

        public void Show(PortraitData d)
        {
            disc.color = Theme.FamilyColor(d.Family);
            initial.text = d.Initial;
            boss.SetActive(d.Boss);
        }
    }
}
