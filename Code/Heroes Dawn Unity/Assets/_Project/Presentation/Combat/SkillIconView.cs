#nullable enable
using System;
using N29.HeroesDawn.Presentation.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Combat
{
    /// <summary>
    /// Carte de compétence de l'éventail du bas d'écran : numéro de l'emplacement, initiale, nom, recharge en nombre et en secteur,
    /// anneau quand elle est lançable ou choisie. Le 4e emplacement attend une carte du grimoire.
    /// </summary>
    public sealed class SkillIconView : MonoBehaviour
    {
        [SerializeField] Button button = null!;
        [SerializeField] Image disc = null!;
        [SerializeField] Image costBadge = null!;
        [SerializeField] Image ring = null!;
        [SerializeField] TMP_Text glyph = null!;
        [SerializeField] TMP_Text slot = null!;
        [SerializeField] TMP_Text cost = null!;
        [SerializeField, Mvp.OptionalRef] TMP_Text? label;
        [SerializeField, Mvp.OptionalRef] SkillHoldTrigger? hold;
        [SerializeField, Mvp.OptionalRef] SkillCardDrag? drag;
        [SerializeField] CanvasGroup group = null!;

        public Button Button => button;
        public event Action? Clicked;
        /// <summary>Appui maintenu (true) ou relâché (false) sur la carte : affiche sa description d'effet.</summary>
        public event Action<SkillIconView, bool>? Holding;
        /// <summary>Carte prise au glisser (true : la compétence se pose sur une cible).</summary>
        public event Action<SkillIconView>? DragBegan;
        /// <summary>Carte lâchée à cette position de l'écran.</summary>
        public event Action<SkillIconView, Vector2>? DragEnded;

        /// <summary>Dernière donnée affichée (tests).</summary>
        public SkillSlotData Data { get; private set; }

        void Awake()
        {
            button.onClick.AddListener(() => Clicked?.Invoke());
            if (hold != null) hold.Held += down => Holding?.Invoke(this, down);
            if (drag != null)
            {
                drag.Began += () => DragBegan?.Invoke(this);
                drag.Ended += at => DragEnded?.Invoke(this, at);
            }
        }

        public void Show(SkillSlotData d, int index)
        {
            Data = d;
            gameObject.SetActive(d.Visible);
            if (!d.Visible) return;
            slot.text = (index + 1).ToString();
            glyph.text = d.Locked ? "+" : d.Glyph;
            if (label != null) label.text = d.Locked ? "" : NameOf(d.Label);
            var baseColor = d.Locked ? Theme.Surface3 : Color.Lerp(index == 3 ? Theme.Perception : index == 2 ? Theme.Danger : index == 1 ? Theme.Info : Theme.Primary, Color.white, 0.3f);
            disc.color = d.Pending ? Color.white : baseColor;
            glyph.color = d.Locked ? Theme.TextMuted : Theme.Navy;
            bool paid = !d.Locked && d.Cost > 0;
            costBadge.gameObject.SetActive(paid);
            cost.text = paid ? d.Cost.ToString() : "";
            cost.color = d.Affordable ? Theme.Glow : Theme.Danger;
            ring.gameObject.SetActive(d.Enabled || d.Pending);
            ring.color = d.Pending ? Theme.Primary : Theme.Glow;
            button.interactable = d.Enabled;
            if (drag != null) drag.CanDrag = d.Enabled;
            group.alpha = d.Locked ? 0.45f : d.Enabled || d.Pending ? 1f : 0.62f;
        }

        /// <summary>« [2] Barrière de fer » → « Barrière de fer » (le numéro est déjà affiché dans le coin).</summary>
        static string NameOf(string labelWithKey)
        {
            int close = labelWithKey.IndexOf(']');
            return close >= 0 ? labelWithKey.Substring(close + 1).Trim() : labelWithKey;
        }
    }
}
