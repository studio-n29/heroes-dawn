#nullable enable
using System;
using N29.HeroesDawn.Presentation.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Ligne d'une liste (widget prefab) : pastille, titre, sous-titre, barre, badge et jusqu'à trois boutons.</summary>
    public sealed class InfoRowView : MonoBehaviour
    {
        [SerializeField] Image rim = null!;
        [SerializeField] Image fill = null!;
        [SerializeField] Button select = null!;
        [SerializeField] Image tag = null!;
        [SerializeField] TMP_Text tagText = null!;
        [SerializeField] TMP_Text title = null!;
        [SerializeField] TMP_Text subtitle = null!;
        RectTransform? _subtitleRect;
        [SerializeField] UiBar bar = null!;
        [SerializeField] TMP_Text badge = null!;
        [SerializeField] UiButton[] buttons = Array.Empty<UiButton>();

        public event Action? Selected;
        public event Action<int>? ButtonClicked;

        public UiButton[] Buttons => buttons;

        void Awake()
        {
            select.onClick.AddListener(() => Selected?.Invoke());
            for (int i = 0; i < buttons.Length; i++)
            {
                int index = i;
                buttons[i].Button.onClick.AddListener(() => ButtonClicked?.Invoke(index));
            }
        }

        public void Show(InfoRowData d)
        {
            bool hasTag = !string.IsNullOrEmpty(d.Tag);
            tag.gameObject.SetActive(hasTag);
            if (hasTag) { tag.color = d.TagColor; tagText.text = d.Tag; }
            title.text = d.Title;
            subtitle.text = d.Subtitle;
            bool hasBar = d.Progress >= 0f;
            _subtitleRect ??= (RectTransform)subtitle.transform;
            _subtitleRect.offsetMin = new Vector2(_subtitleRect.offsetMin.x, hasBar ? 38f : 8f);
            subtitle.overflowMode = hasBar ? TextOverflowModes.Ellipsis : TextOverflowModes.Overflow;
            bar.gameObject.SetActive(hasBar);
            if (hasBar) { bar.Set(d.Progress, true); bar.SetLabel(d.ProgressLabel); }
            badge.gameObject.SetActive(!string.IsNullOrEmpty(d.Badge));
            badge.text = d.Badge;
            rim.color = d.Highlight ? Theme.Primary : Theme.Ink;
            fill.color = d.Dim ? Theme.WithAlpha(Theme.Surface, 0.7f) : Theme.Surface2;
            title.color = d.Dim ? Theme.TextMuted : Theme.Text;
            SetButton(0, d.A);
            SetButton(1, d.B);
            SetButton(2, d.C);
        }

        void SetButton(int i, RowButtonData b)
        {
            if (i >= buttons.Length) return;
            var ub = buttons[i];
            if (ub.gameObject.activeSelf != b.Visible) ub.gameObject.SetActive(b.Visible);
            if (!b.Visible) return;
            ub.SetLabel(b.Label);
            ub.SetColors(StyleBg(b.Style), b.Style == 2 || b.Style == 3 ? Theme.Text : Theme.TextDark);
            ub.Interactable = b.Enabled;
            ub.DisabledReason = b.Reason;
        }

        static Color StyleBg(int style) => style == 1 ? Theme.Pr : style == 2 ? Theme.Danger : style == 3 ? Theme.Surface3 : Theme.Primary;
    }
}
