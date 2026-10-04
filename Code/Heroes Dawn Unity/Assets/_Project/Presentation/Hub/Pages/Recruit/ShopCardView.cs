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
        const float LegendRaise = 24f;

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

        public event Action? Tapped;
        public event Action? RecruitClicked;

        public Button RecruitButton => recruit.Button;
        public Button TapButton => tap;

        void Awake()
        {
            tap.onClick.AddListener(() => Tapped?.Invoke());
            recruit.Button.onClick.AddListener(() => RecruitClicked?.Invoke());
        }

        public void Show(RecruitCardState d)
        {
            var color = Theme.RarityColor(d.Rarity);
            rim.color = color;
            halo.color = Theme.WithAlpha(color, 0.45f);
            if (halo.gameObject.activeSelf != d.Halo) halo.gameObject.SetActive(d.Halo);
            card.anchoredPosition = new Vector2(0, d.Halo ? LegendRaise : 0);
            strip.color = Theme.FamilyColor(d.Card.Family);
            portrait.Show(d.Card.Portrait);
            memberName.text = d.Card.Name;
            rarity.text = d.Card.RarityText;
            rarity.color = color;
            role.text = d.Card.RoleLine;
            stats.text = d.Card.Stats;
            skills.text = d.Card.Skills;
            recruit.SetLabel(d.PriceLabel);
            recruit.SetColors(d.Affordable ? Theme.Pr : Theme.Surface3, d.Affordable ? Theme.TextDark : Theme.Text);
            recruit.Interactable = !d.InTeam;
            if (recruit.gameObject.activeSelf == d.InTeam) recruit.gameObject.SetActive(!d.InTeam);
            inTeam.text = d.InTeamText;
            if (inTeam.gameObject.activeSelf != d.InTeam) inTeam.gameObject.SetActive(d.InTeam);
            body.alpha = d.InTeam ? 0.55f : 1f;
        }
    }
}
