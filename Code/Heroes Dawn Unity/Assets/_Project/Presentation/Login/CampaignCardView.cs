#nullable enable
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using N29.HeroesDawn.Presentation.Common;

namespace N29.HeroesDawn.Presentation.Login
{
    /// <summary>Carte d'une campagne (widget prefab 300 × 420) : nom, description, badge ; seule une carte disponible se choisit.</summary>
    public sealed class CampaignCardView : MonoBehaviour
    {
        [SerializeField] Image frame = null!;
        [SerializeField] Image fill = null!;
        [SerializeField] TMP_Text cardName = null!;
        [SerializeField] TMP_Text description = null!;
        [SerializeField] TMP_Text badge = null!;
        [SerializeField] Button button = null!;

        public string Id { get; private set; } = "";
        public bool Available { get; private set; }
        public bool Selected { get; private set; }
        public event Action<string>? Clicked;

        void Awake() => button.onClick.AddListener(() => Clicked?.Invoke(Id));

        public void Setup(CampaignCardData d)
        {
            Id = d.Id;
            Available = d.Available;
            gameObject.name = "Card_" + d.Id;
            cardName.text = d.Name;
            description.text = d.Description;
            badge.text = d.Badge;
            fill.color = d.Available ? Theme.Frost : Theme.Surface3;
            cardName.color = d.Available ? Theme.Navy : Theme.TextMuted;
            badge.color = d.Available ? Theme.Success : Theme.TextMuted;
            SetSelected(false);
        }

        public void SetSelected(bool on)
        {
            Selected = on;
            frame.color = on ? Theme.Glow : Theme.FrostEdge;
            transform.localScale = on ? Vector3.one * 1.05f : Vector3.one;
        }

        /// <summary>Bouton de la carte (tests PlayMode : clic réel).</summary>
        public Button Button => button;
    }
}
