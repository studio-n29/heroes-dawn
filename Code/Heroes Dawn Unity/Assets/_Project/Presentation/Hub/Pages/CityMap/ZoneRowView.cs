#nullable enable
using System;
using N29.HeroesDawn.Application.Features.World;
using N29.HeroesDawn.Presentation.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Ligne de la liste des zones de l'acte (widget prefab) : code et nom, état, cadre jaune quand elle est choisie.</summary>
    public sealed class ZoneRowView : MonoBehaviour
    {
        [SerializeField] Button button = null!;
        [SerializeField] Image rim = null!;
        [SerializeField] TMP_Text title = null!;
        [SerializeField] TMP_Text status = null!;

        public event Action? Clicked;
        public Button Button => button;

        void Awake() => button.onClick.AddListener(() => Clicked?.Invoke());

        public void Show(ZoneRowData d)
        {
            title.text = d.Title;
            status.text = d.StatusText;
            title.color = d.Status == ZoneStatus.Locked ? Theme.TextMuted : Theme.Text;
            status.color = d.Status == ZoneStatus.Done ? Theme.Success : d.Status == ZoneStatus.Locked ? Theme.TextMuted : Theme.Primary;
            rim.color = d.Selected ? Theme.Primary : Theme.Ink;
        }
    }
}
