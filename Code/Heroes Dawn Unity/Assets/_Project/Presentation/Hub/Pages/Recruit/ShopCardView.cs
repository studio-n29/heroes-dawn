#nullable enable
using System;
using N29.HeroesDawn.Presentation.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>
    /// Carte d'un héros de la boutique (widget prefab) : cadre à la couleur de rareté (cuivre, argent, or), Légende surélevée
    /// avec halo, prix sur le bouton, « Dans l'équipe » une fois recruté. Toucher la carte ouvre la fiche.
    /// </summary>
    public sealed class ShopCardView : MonoBehaviour
    {
        const float LegendRaise = 18f;

        [SerializeField] RectTransform card = null!;
        [SerializeField] Button tap = null!;
        [SerializeField] Image rim = null!;
        [SerializeField] Image halo = null!;
        [SerializeField] Image strip = null!;
        [SerializeField] PortraitView portrait = null!;
        [SerializeField] TMP_Text memberName = null!;
        [SerializeField] TMP_Text rarity = null!;
        [SerializeField] TMP_Text role = null!;
        [SerializeField] TMP_Text stats = null!;
        [SerializeField] TMP_Text skills = null!;
        [SerializeField] UiButton recruit = null!;
        [SerializeField] TMP_Text inTeam = null!;
        [SerializeField] CanvasGroup body = null!;
        [SerializeField, N29.HeroesDawn.Presentation.Mvp.OptionalRef] Image? bigArt;
        [SerializeField, N29.HeroesDawn.Presentation.Mvp.OptionalRef] Image? familyBadge;
        [SerializeField, N29.HeroesDawn.Presentation.Mvp.OptionalRef] Image? selection;

        bool _shown;
        bool _wasInTeam;

        public event Action? Tapped;
        public event Action? RecruitClicked;

        public Button RecruitButton => recruit.Button;
        public Button TapButton => tap;
        /// <summary>Dernier état affiché (alimente la fiche de droite de la page).</summary>
        public RecruitCardState State { get; private set; }

        public void SetSelected(bool on) { if (selection != null && selection.gameObject.activeSelf != on) selection.gameObject.SetActive(on); }

        void Awake()
        {
            tap.onClick.AddListener(() => Tapped?.Invoke());
            recruit.Button.onClick.AddListener(() => RecruitClicked?.Invoke());
        }

        public void Show(RecruitCardState d)
        {
            var color = Theme.RarityColor(d.Rarity);
            rim.color = color;
            halo.color = Theme.WithAlpha(color, 0.3f);
            if (halo.gameObject.activeSelf != d.Halo) halo.gameObject.SetActive(d.Halo);
            card.anchoredPosition = new Vector2(0, d.Halo ? LegendRaise : 0);
            ((RectTransform)halo.transform).anchoredPosition = card.anchoredPosition; // le halo suit la carte surélevée
            strip.color = Theme.FamilyColor(d.Card.Family);
            portrait.Show(d.Card.Portrait);
            State = d;
            if (familyBadge != null) familyBadge.color = Theme.FamilyColor(d.Card.Family);
            if (bigArt != null)
            {
                var art = d.Card.Portrait.Art;
                bigArt.gameObject.SetActive(art != null);
                portrait.gameObject.SetActive(art == null);
                if (art != null) { bigArt.sprite = art; Cover(bigArt, art); }
            }
            memberName.text = d.Card.Name;
            rarity.text = d.Card.RarityText;
            rarity.color = color;
            role.text = d.Card.RoleLine;
            stats.text = d.Card.Stats;
            skills.text = d.Card.Skills;
            recruit.SetLabel(d.PriceLabel);
            recruit.SetColors(d.Affordable ? Theme.Primary : Theme.Surface3, d.Affordable ? Theme.TextDark : Theme.Text);
            recruit.Interactable = !d.InTeam;
            if (recruit.gameObject.activeSelf == d.InTeam) recruit.gameObject.SetActive(!d.InTeam);
            inTeam.text = d.InTeamText;
            if (inTeam.gameObject.activeSelf != d.InTeam) inTeam.gameObject.SetActive(d.InTeam);
            body.alpha = d.InTeam ? 0.55f : 1f;
            // Recrutement : le tampon « Dans l'équipe » s'écrase et la carte jette un éclat.
            if (d.InTeam && _shown && !_wasInTeam)
            {
                Tween.PopIn(inTeam.transform, 0.26f);
                Feedback.Fx.Burst(card);
                Feedback.Fx.Sparks(card, color, 14, 170f, 20f);
            }
            _wasInTeam = d.InTeam;
            _shown = true;
        }

        /// <summary>Illustration pleine carte : couvre toute la zone, ancrée vers le haut où se trouve le visage.</summary>
        static void Cover(Image image, Sprite art)
        {
            var area = ((RectTransform)image.transform.parent).rect.size;
            if (area.x < 1f || area.y < 1f) area = new Vector2(288f, 508f);
            float scale = Mathf.Max(area.x / art.rect.width, area.y / art.rect.height);
            var size = new Vector2(art.rect.width * scale, art.rect.height * scale);
            var rect = image.rectTransform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = new Vector2(0f, -(size.y - area.y) * 0.4f);
        }

        /// <summary>Nouvelle offre dans cet emplacement (renouvellement) : l'état « recruté » repart de zéro.</summary>
        public void ResetState() => _shown = false;
    }
}
