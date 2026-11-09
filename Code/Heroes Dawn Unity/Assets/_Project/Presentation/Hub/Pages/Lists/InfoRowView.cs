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
        InfoRowData _data;
        float _width = -1;
        bool _shown;
        void LateUpdate()
        {
            float width = ((RectTransform)transform).rect.width;
            if (_shown && Mathf.Abs(width - _width) > .5f) Layout(_data.A, _data.B, _data.C);
        }

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
            _data = d;
            _shown = true;
            bool hasTag = !string.IsNullOrEmpty(d.Tag);
            tag.gameObject.SetActive(hasTag);
            if (hasTag) { tag.color = d.TagColor; tagText.text = d.Tag; }
            title.text = d.Title;
            subtitle.text = d.Subtitle;
            bool hasBar = d.Progress >= 0f;
            _subtitleRect ??= (RectTransform)subtitle.transform;
            _subtitleRect.offsetMin = new Vector2(_subtitleRect.offsetMin.x, hasBar ? 38f : 8f);
            subtitle.overflowMode = TextOverflowModes.Ellipsis;
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
            _width = ((RectTransform)transform).rect.width;
            bool stacked = vis.Count > 0 && _width < 1120;
            float buttonWidth = stacked ? Mathf.Min(260, (_width - 40 - (vis.Count - 1) * 12) / vis.Count) : 196;
            var element = GetComponent<LayoutElement>();
            if (element != null) element.preferredHeight = stacked ? 244 : 144;
            for (int k = 0; k < vis.Count; k++)
            {
                var rt = buttons[vis[k]].Rect;
                rt.anchorMin = rt.anchorMax = new Vector2(1, stacked ? 0 : .5f);
                rt.pivot = new Vector2(1, stacked ? 0 : .5f);
                rt.sizeDelta = new Vector2(buttonWidth, 96);
                rt.anchoredPosition = new Vector2(-16f - (vis.Count - 1 - k) * (buttonWidth + 12), stacked ? 12 : 0);
            }
            float right = stacked ? -20 : -(24f + vis.Count * (buttonWidth + 12));
            foreach (var t in new[] { (RectTransform)title.transform, (RectTransform)subtitle.transform, (RectTransform)bar.transform })
                t.offsetMax = new Vector2(right, t.offsetMax.y);
            title.rectTransform.offsetMin = new Vector2(96, -58);
            title.rectTransform.offsetMax = new Vector2(right, -8);
            subtitle.rectTransform.offsetMax = new Vector2(right, -62);
            subtitle.rectTransform.offsetMin = new Vector2(96, stacked ? (bar.gameObject.activeSelf ? 146 : 116) : (bar.gameObject.activeSelf ? 38 : 10));
            var br = (RectTransform)bar.transform;
            br.offsetMin = new Vector2(96, stacked ? 116 : 10);
            br.offsetMax = new Vector2(right, stacked ? 138 : 32);
            tag.rectTransform.anchorMin = tag.rectTransform.anchorMax = Vector2.up;
            tag.rectTransform.pivot = Vector2.up;
            tag.rectTransform.anchoredPosition = new Vector2(14, -14);
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
