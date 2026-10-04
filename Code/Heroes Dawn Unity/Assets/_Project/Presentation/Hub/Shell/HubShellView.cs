#nullable enable
using System;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Presentation.Wip;
using TMPro;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Coque du hub (prefab Screens/HubShell) : titre, Retour, ressources, réglages, zone de page, 5 onglets.</summary>
    public sealed class HubShellView : ViewBase, IHubShellView, IPageHost
    {
        [SerializeField] TMP_Text title = null!;
        [SerializeField] UiButton back = null!;
        [SerializeField] UiButton settings = null!;
        [SerializeField] HubWalletView wallet = null!;
        [SerializeField] RectTransform pageArea = null!;
        [SerializeField] UiTapRelay relay = null!;
        [SerializeField] UiButton[] tabs = Array.Empty<UiButton>();
        [SerializeField] TMP_Text[] badges = Array.Empty<TMP_Text>();

        public event Action<HubPage>? TabClicked;
        public event Action? BackClicked;
        public event Action? SettingsClicked;

        public IWalletView Wallet => wallet;
        public IPageHost Pages => this;
        public IUiTapRelay Relay => relay;

        /// <summary>Bouton d'un onglet (tests PlayMode : clics réels).</summary>
        public UiButton TabButton(HubPage tab) => tabs[Array.IndexOf(HubNavigator.Tabs, tab)];
        public UiButton BackButton => back;
        public UiButton SettingsButton => settings;

        void Awake()
        {
            for (int i = 0; i < tabs.Length && i < HubNavigator.Tabs.Length; i++)
            {
                var page = HubNavigator.Tabs[i];
                tabs[i].Button.onClick.AddListener(() => TabClicked?.Invoke(page));
            }
            back.Button.onClick.AddListener(() => BackClicked?.Invoke());
            settings.Button.onClick.AddListener(() => SettingsClicked?.Invoke());
        }

        public void ShowChrome(HubShellData d)
        {
            title.text = d.Title;
            if (back.gameObject.activeSelf != d.ShowBack) back.gameObject.SetActive(d.ShowBack);
            for (int i = 0; i < tabs.Length && i < d.Tabs.Length; i++)
            {
                var t = d.Tabs[i];
                tabs[i].SetLabel(t.Label);
                tabs[i].SetColors(t.Active ? Theme.Primary : Theme.WithAlpha(Theme.Surface, 0f), t.Active ? Theme.TextDark : Theme.TextMuted);
                bool show = !string.IsNullOrEmpty(t.Badge);
                var badge = badges[i].transform.parent.gameObject;
                if (badge.activeSelf != show) badge.SetActive(show);
                badges[i].text = t.Badge;
            }
        }

        public GameObject Spawn(GameObject pagePrefab)
        {
            var go = Instantiate(pagePrefab, pageArea, false);
            ((RectTransform)go.transform).Stretch();
            go.transform.SetAsLastSibling();
            return go;
        }

        public void Despawn(GameObject page)
        {
            if (page != null) Destroy(page);
        }
    }
}
