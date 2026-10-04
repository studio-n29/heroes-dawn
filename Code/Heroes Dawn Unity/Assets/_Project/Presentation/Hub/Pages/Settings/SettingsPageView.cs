#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Page Paramètres (prefab Pages/Settings) : catégories à gauche, réglages de la catégorie à droite.</summary>
    public sealed class SettingsPageView : ViewBase, ISettingsPageView
    {
        [SerializeField] UiButton[] categories = Array.Empty<UiButton>();
        [SerializeField] TMP_Text title = null!;
        [SerializeField] UiButton resetCategory = null!;
        [SerializeField] RectTransform rows = null!;
        [SerializeField] SettingRowView rowPrefab = null!;

        readonly List<SettingRowView> _rows = new List<SettingRowView>();

        public event Action<int>? CategorySelected;
        public event Action<int, int>? RowAction;
        public event Action? ResetCategoryClicked;

        public IReadOnlyList<SettingRowView> Rows => _rows;
        public Button CategoryButton(int i) => categories[i].Button;

        /// <summary>Premier bouton d'action portant ce libellé (tests PlayMode : clics réels).</summary>
        public Button? ActionButton(string label)
        {
            foreach (var r in _rows)
                if (r.gameObject.activeSelf && r.Action.gameObject.activeSelf && r.Action.Label.text == label) return r.Action.Button;
            return null;
        }

        void Awake()
        {
            for (int i = 0; i < categories.Length; i++)
            {
                int index = i;
                categories[i].Button.onClick.AddListener(() => CategorySelected?.Invoke(index));
            }
            resetCategory.Button.onClick.AddListener(() => ResetCategoryClicked?.Invoke());
        }

        public void Show(SettingsPageData d)
        {
            for (int i = 0; i < categories.Length; i++)
            {
                bool used = i < d.Categories.Length;
                if (categories[i].gameObject.activeSelf != used) categories[i].gameObject.SetActive(used);
                if (!used) continue;
                categories[i].SetLabel(d.Categories[i]);
                bool active = i == d.Active;
                categories[i].SetColors(active ? Theme.Primary : Theme.Surface3, active ? Theme.TextDark : Theme.Text);
            }
            title.text = d.CategoryTitle;
            resetCategory.gameObject.SetActive(d.CanReset);
            resetCategory.SetLabel(d.ResetLabel);

            while (_rows.Count < d.Rows.Length)
            {
                var row = Instantiate(rowPrefab, rows);
                int index = _rows.Count;
                row.Acted += a => RowAction?.Invoke(index, a);
                _rows.Add(row);
            }
            for (int i = 0; i < _rows.Count; i++)
            {
                bool used = i < d.Rows.Length;
                if (_rows[i].gameObject.activeSelf != used) _rows[i].gameObject.SetActive(used);
                if (used) _rows[i].Show(d.Rows[i]);
            }
        }
    }
}
