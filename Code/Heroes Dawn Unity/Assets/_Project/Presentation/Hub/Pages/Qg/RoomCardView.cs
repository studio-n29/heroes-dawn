#nullable enable
using System;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Wip;
using TMPro;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Case de pièce du QG (widget prefab) : nom, info réelle ou bandeau BIENTÔT.</summary>
    public sealed class RoomCardView : MonoBehaviour
    {
        [SerializeField] UiButton button = null!;
        [SerializeField] TMP_Text title = null!;
        [SerializeField] TMP_Text info = null!;
        [SerializeField] WipMark wip = null!;
        [SerializeField] GameObject badge = null!;
        [SerializeField] TMP_Text badgeText = null!;

        public event Action? Clicked;

        public UiButton Button => button;

        void Awake() => button.Button.onClick.AddListener(() => Clicked?.Invoke());

        public void Show(RoomCardData d)
        {
            title.text = d.Title;
            info.text = d.Info;
            bool soon = d.WipId != null;
            if (wip.gameObject.activeSelf != soon) wip.gameObject.SetActive(soon);
            if (soon) wip.FeatureId = d.WipId!;
            if (badge.activeSelf != d.Badge > 0) badge.SetActive(d.Badge > 0);
            badgeText.text = d.Badge.ToString();
        }
    }
}
