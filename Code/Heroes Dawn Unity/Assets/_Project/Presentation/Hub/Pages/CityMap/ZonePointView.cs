#nullable enable
using System;
using N29.HeroesDawn.Application.Features.World;
using N29.HeroesDawn.Presentation.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>
    /// Point précis de mission sur la carte (widget prefab) : disque numéroté, anneau pulsé quand il est choisi,
    /// coche quand la zone est validée, grisé quand elle est verrouillée.
    /// </summary>
    public sealed class ZonePointView : MonoBehaviour
    {
        [SerializeField] Button button = null!;
        [SerializeField] Image disc = null!;
        [SerializeField] Image ring = null!;
        [SerializeField] TMP_Text label = null!;
        [SerializeField] TMP_Text sub = null!;
        [SerializeField] TMP_Text check = null!;

        public event Action? Clicked;
        public Button Button => button;

        void Awake() => button.onClick.AddListener(() => Clicked?.Invoke());

        public void Show(ZonePointData d)
        {
            label.text = d.Label;
            sub.text = d.Sub;
            bool locked = d.Status == ZoneStatus.Locked;
            bool done = d.Status == ZoneStatus.Done;
            disc.color = locked ? Theme.Surface3 : done ? Theme.Success : d.Selected ? Theme.Primary : Theme.Info;
            label.color = locked ? Theme.TextMuted : Theme.TextDark;
            if (ring.gameObject.activeSelf != d.Selected) ring.gameObject.SetActive(d.Selected);
            if (check.gameObject.activeSelf != done) check.gameObject.SetActive(done);
            // Un point verrouillé reste visible mais discret.
            var g = GetComponent<CanvasGroup>();
            if (g != null) g.alpha = locked ? 0.7f : 1f;
        }
    }
}
