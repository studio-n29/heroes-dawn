#nullable enable
using System;
using N29.HeroesDawn.Presentation.Common;
using TMPro;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Combat
{
    /// <summary>Bouton de compétence (widget prefab) : nom, recharge, effet court ; couleur par emplacement, blanc en ciblage.</summary>
    public sealed class SkillButtonView : MonoBehaviour
    {
        [SerializeField] UiButton button = null!;
        [SerializeField] TMP_Text description = null!;

        public event Action? Clicked;

        void Awake() => button.Button.onClick.AddListener(() => Clicked?.Invoke());

        public void Show(SkillSlotData d, int slot)
        {
            gameObject.SetActive(d.Visible);
            if (!d.Visible) return;
            button.SetLabel(d.Label);
            description.text = d.Description;
            var bg = d.Pending ? Color.white : slot == 2 ? Theme.Danger : slot == 1 ? Theme.Info : Theme.Primary;
            var fg = slot == 2 && !d.Pending ? Color.white : Theme.TextDark;
            button.SetColors(bg, fg);
            description.color = fg;
            button.Interactable = d.Enabled;
        }
    }
}
