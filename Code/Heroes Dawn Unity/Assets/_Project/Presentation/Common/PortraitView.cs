#nullable enable
using N29.HeroesDawn.Definitions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>
    /// Ce qu'affiche un portrait : silhouette provisoire (buste à la couleur de la famille) ou, plus tard, l'illustration de l'agent
    /// (<see cref="Art"/>) ; initiale et couronne de boss.
    /// </summary>
    public readonly struct PortraitData
    {
        public readonly string Initial;
        public readonly Family Family;
        public readonly bool Boss;
        /// <summary>Illustration de l'agent ; null : la silhouette provisoire est affichée.</summary>
        public readonly Sprite? Art;

        public PortraitData(string name, Family family, bool boss = false, Sprite? art = null)
        {
            Initial = string.IsNullOrEmpty(name) ? "?" : name.Substring(0, 1).ToUpperInvariant();
            Family = family;
            Boss = boss;
            Art = art;
        }
    }

    /// <summary>Portrait rond (widget prefab) : disque de la couleur de famille, silhouette (ou illustration), reflet, initiale, couronne de boss.</summary>
    public sealed class PortraitView : MonoBehaviour
    {
        [SerializeField] Image disc = null!;
        [SerializeField] Image bust = null!;
        [SerializeField] Image art = null!;
        [SerializeField] TMP_Text initial = null!;
        [SerializeField] GameObject boss = null!;

        public Image Disc => disc;
        public Image Bust => bust;
        public Image Art => art;

        public void Show(PortraitData d)
        {
            var family = Theme.FamilyColor(d.Family);
            disc.color = Color.Lerp(family, Color.white, 0.25f);
            bust.color = Color.Lerp(family, Theme.Navy, 0.5f);
            bool hasArt = d.Art != null;
            art.gameObject.SetActive(hasArt);
            bust.gameObject.SetActive(!hasArt);
            if (hasArt) art.sprite = d.Art;
            initial.text = d.Initial;
            boss.SetActive(d.Boss);
        }
    }
}
