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

        /// <summary>Affiche le portrait ; détouré (dialogues), l'anneau et le disque de fond disparaissent : seul le personnage reste sur la scène.</summary>
        public void Show(PortraitData d, bool cutout = false)
        {
            // Les illustrations actuelles sont des images pleines : elles gardent leur disque (sinon elles s'afficheraient en rectangle).
            // Seule la silhouette provisoire se détache sans fond ; des portraits détourés demanderont leur propre indicateur.
            cutout &= d.Art == null;
            var ring = GetComponent<Image>();
            if (ring != null) ring.enabled = !cutout;
            disc.enabled = !cutout;
            var family = Theme.FamilyColor(d.Family);
            disc.color = family;
            bust.color = Color.Lerp(family, Theme.Outline, 0.5f);
            bool hasArt = d.Art != null;
            art.gameObject.SetActive(hasArt);
            bust.gameObject.SetActive(!hasArt);
            if (hasArt)
            {
                art.sprite = d.Art;
                var fitter = art.GetComponent<AspectRatioFitter>();
                if (fitter != null) fitter.aspectRatio = d.Art!.rect.width / d.Art.rect.height;
                if (fitter != null) fitter.enabled = false;
                CropFace();
            }
            initial.text = d.Initial;
            initial.gameObject.SetActive(!hasArt);
            boss.SetActive(d.Boss);
        }

        void OnRectTransformDimensionsChange()
        {
            if (art != null && art.sprite != null && art.gameObject.activeSelf) CropFace();
        }

        void CropFace() => FrameFace(art);

        public static void FrameFace(Image art, float zoomMultiplier = 1f)
        {
            if (art.sprite == null) return;
            var viewport = (RectTransform)art.transform.parent;
            var focus = new Vector2(0.5f, 0.78f);
            float zoom = 4.2f;
            // Coordonnées dans les illustrations source existantes, sans réinventer les visages.
            switch (art.sprite.name)
            {
                case "giulia_rossi": focus = new Vector2(0.43f, 0.79f); zoom = 5f; break;
                case "ilona_varga": focus = new Vector2(0.36f, 0.79f); zoom = 5.5f; break;
                case "lucas_mendes": focus = new Vector2(0.60f, 0.77f); zoom = 5f; break;
                case "mei_lin_chen": focus = new Vector2(0.53f, 0.83f); zoom = 4.6f; break;
                case "agent_d": focus = new Vector2(0.45f, 0.83f); zoom = 4.6f; break;
            }
            float width = Mathf.Max(1, viewport.rect.width) * zoom * zoomMultiplier;
            float height = width * art.sprite.rect.height / art.sprite.rect.width;
            var rect = art.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(width, height);
            rect.anchoredPosition = new Vector2((0.5f-focus.x)*width, (0.5f-focus.y)*height);
        }
    }
}
