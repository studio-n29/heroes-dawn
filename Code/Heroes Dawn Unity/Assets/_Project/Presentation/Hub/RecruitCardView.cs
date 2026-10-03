#nullable enable
using System;
using N29.HeroesDawn.Presentation.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Carte d'une offre de la boutique (widget prefab) : coût en PR, bouton Recruter, état Recruté, bande de rareté.</summary>
    public sealed class RecruitCardView : MemberCardView
    {
        [SerializeField] UiButton recruit = null!;
        [SerializeField] CanvasGroup group = null!;
        [SerializeField] TMP_Text stamp = null!;
        [SerializeField] Image rarityBand = null!;

        public event Action? RecruitClicked;

        /// <summary>Bouton Recruter (tests PlayMode : clic réel).</summary>
        public Button RecruitButton => recruit.Button;

        void Awake() => recruit.Button.onClick.AddListener(() => RecruitClicked?.Invoke());

        public void Show(RecruitCardData d)
        {
            ShowCard(d.Card);
            rarityBand.color = Theme.RarityColor(d.Card.Rarity);
            recruit.SetLabel(d.RecruitLabel);
            recruit.Interactable = d.RecruitEnabled && !d.Recruited;
            recruit.gameObject.SetActive(!d.Recruited);
            stamp.text = d.StampText;
            stamp.gameObject.SetActive(d.Recruited);
            group.alpha = d.Recruited ? 0.45f : 1f;
        }
    }
}
