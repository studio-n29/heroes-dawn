#nullable enable
using System;
using N29.HeroesDawn.Presentation.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Combat
{
    /// <summary>
    /// Icône d'une compétence sous la carte d'un agent (éventail de 4) : numéro de l'emplacement, initiale de la compétence, recharge
    /// en nombre et en secteur, anneau quand elle est lançable ou choisie. Le 4e emplacement attend une carte du grimoire.
    /// </summary>
    public sealed class SkillIconView : MonoBehaviour
    {
        [SerializeField] Button button = null!;
        [SerializeField] Image disc = null!;
        [SerializeField] Image cooldownFill = null!;
        [SerializeField] Image ring = null!;
        [SerializeField] TMP_Text glyph = null!;
        [SerializeField] TMP_Text slot = null!;
        [SerializeField] TMP_Text cooldown = null!;
        [SerializeField] CanvasGroup group = null!;

        public Button Button => button;
        public event Action? Clicked;

        /// <summary>Dernière donnée affichée (tests).</summary>
        public SkillSlotData Data { get; private set; }

        void Awake() => button.onClick.AddListener(() => Clicked?.Invoke());

        public void Show(SkillSlotData d, int index)
        {
            Data = d;
            gameObject.SetActive(d.Visible);
            if (!d.Visible) return;
            slot.text = (index + 1).ToString();
            glyph.text = d.Locked ? "+" : d.Glyph;
            var baseColor = d.Locked ? Theme.Surface3 : index == 2 ? Theme.Danger : index == 1 ? Theme.Info : Theme.Primary;
            disc.color = d.Pending ? Color.white : baseColor;
            glyph.color = d.Pending || index != 2 || d.Locked ? Theme.TextDark : Color.white;
            if (d.Locked) glyph.color = Theme.TextMuted;
            bool cooling = d.Cooldown > 0;
            cooldown.gameObject.SetActive(cooling);
            cooldown.text = cooling ? d.Cooldown.ToString() : "";
            cooldownFill.gameObject.SetActive(cooling);
            cooldownFill.fillAmount = cooling ? Mathf.Clamp01(d.Cooldown / 4f) : 0f;
            ring.gameObject.SetActive(d.Enabled || d.Pending);
            ring.color = d.Pending ? Theme.Primary : Color.white;
            button.interactable = d.Enabled;
            group.alpha = d.Locked ? 0.45f : d.Enabled || d.Pending ? 1f : 0.62f;
        }
    }
}
