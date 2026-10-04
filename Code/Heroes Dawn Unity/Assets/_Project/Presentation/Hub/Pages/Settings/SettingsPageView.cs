#nullable enable
using System;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Presentation.Wip;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Page Paramètres (prefab Pages/Settings) : catégories à gauche (BIENTÔT sauf Compte), compte à droite.</summary>
    public sealed class SettingsPageView : ViewBase, ISettingsPageView
    {
        [SerializeField] UiButton[] categories = Array.Empty<UiButton>();
        [SerializeField] WipMark[] categoryWips = Array.Empty<WipMark>();
        [SerializeField] TMP_Text playerLine = null!;
        [SerializeField] UiButton logout = null!;
        [SerializeField] UiButton reset = null!;

        public event Action? LogoutClicked;
        public event Action? ResetClicked;

        public Button LogoutButton => logout.Button;
        public Button ResetButton => reset.Button;

        void Awake()
        {
            logout.Button.onClick.AddListener(() => LogoutClicked?.Invoke());
            reset.Button.onClick.AddListener(() => ResetClicked?.Invoke());
        }

        public void Show(SettingsPageData d)
        {
            for (int i = 0; i < categories.Length; i++)
            {
                bool used = i < d.Categories.Length;
                if (categories[i].gameObject.activeSelf != used) categories[i].gameObject.SetActive(used);
                if (!used) continue;
                var c = d.Categories[i];
                categories[i].SetLabel(c.Title);
                categories[i].SetColors(c.Active ? Theme.Primary : Theme.Surface3, c.Active ? Theme.TextDark : Theme.Text);
                bool soon = c.WipId != null;
                var wip = categoryWips[i];
                if (wip.gameObject.activeSelf != soon) wip.gameObject.SetActive(soon);
                if (soon) wip.FeatureId = c.WipId!;
            }
            playerLine.text = d.PlayerLine;
        }
    }
}
