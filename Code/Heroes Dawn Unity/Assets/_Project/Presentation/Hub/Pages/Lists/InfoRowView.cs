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
            Layout(d.A, d.B, d.C);
            SetButton(0, d.A);
            SetButton(1, d.B);
            SetButton(2, d.C);
        }

        /// <summary>Les boutons visibles se rangent à droite, le texte prend la place restante.</summary>
        void Layout(RowButtonData a, RowButtonData b, RowButtonData c)
        {
            var vis = new System.Collections.Generic.List<int>(3);
            if (a.Visible) vis.Add(0);
            if (b.Visible) vis.Add(1);
            if (c.Visible) vis.Add(2);
            for (int k = 0; k < vis.Count; k++)
            {
                var rt = buttons[vis[k]].Rect;
                rt.anchoredPosition = new Vector2(-12f - (vis.Count - 1 - k) * 204f, rt.anchoredPosition.y);
            }
            float right = -(24f + vis.Count * 204f);
            foreach (var t in new[] { (RectTransform)title.transform, (RectTransform)subtitle.transform, (RectTransform)bar.transform })
                t.offsetMax = new Vector2(right, t.offsetMax.y);
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
