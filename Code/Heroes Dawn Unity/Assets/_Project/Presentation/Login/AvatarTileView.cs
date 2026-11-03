#nullable enable
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using N29.HeroesDawn.Presentation.Common;

namespace N29.HeroesDawn.Presentation.Login
{
    /// <summary>Vignette d'un avatar de la galerie (widget prefab 150 × 150) : son numéro et un cadre qui s'allume quand il est choisi.</summary>
    public sealed class AvatarTileView : MonoBehaviour
    {
        [SerializeField] Image frame = null!;
        [SerializeField] TMP_Text label = null!;
        [SerializeField] Button button = null!;

        public string Id { get; private set; } = "";
        public bool Selected { get; private set; }
        public event Action<string>? Clicked;

        void Awake() => button.onClick.AddListener(() => Clicked?.Invoke(Id));

        public void Setup(string id, int number)
        {
            Id = id;
            gameObject.name = "Avatar_" + id;
            label.text = number.ToString();
            SetSelected(false);
        }

        public void SetSelected(bool on)
        {
            Selected = on;
            frame.color = on ? Theme.Glow : Theme.FrostEdge;
            transform.localScale = on ? Vector3.one * 1.08f : Vector3.one;
        }

        /// <summary>Bouton de la vignette (tests PlayMode : clic réel).</summary>
        public Button Button => button;
    }
}
