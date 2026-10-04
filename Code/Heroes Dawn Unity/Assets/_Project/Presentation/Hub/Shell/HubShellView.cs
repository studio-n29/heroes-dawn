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
    /// <summary>
    /// Coque du hub (prefab Screens/Hub) : titre, Retour, ressources, réglages, zone de page, 5 onglets.
    /// Feedbacks : page qui entre en glissant (le fondu vient de la vue), bloc d'onglet actif qui glisse, badges qui pulsent (prefab).
    /// </summary>
    public sealed class HubShellView : ViewBase, IHubShellView, IPageHost
    {
        const float PageSlide = 28f, SlideDuration = 0.22f, IndicatorDuration = 0.2f;

        [SerializeField] TMP_Text title = null!;
        [SerializeField] UiButton back = null!;
        [SerializeField] UiButton settings = null!;
        [SerializeField] HubWalletView wallet = null!;
        [SerializeField] RectTransform pageArea = null!;
        [SerializeField] UiTapRelay relay = null!;
        [SerializeField] UiButton[] tabs = Array.Empty<UiButton>();
        [SerializeField] TMP_Text[] badges = Array.Empty<TMP_Text>();
        [SerializeField, OptionalRef] RectTransform? tabIndicator;

        int _activeTab = -1;

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
            int active = -1;
            for (int i = 0; i < tabs.Length && i < d.Tabs.Length; i++)
            {
                var t = d.Tabs[i];
                if (t.Active) active = i;
                tabs[i].SetLabel(t.Label);
                tabs[i].SetColors(t.Active ? Theme.Primary : Theme.WithAlpha(Theme.Surface, 0f), t.Active ? Theme.TextDark : Theme.TextMuted);
                bool show = !string.IsNullOrEmpty(t.Badge);
                var badge = badges[i].transform.parent.gameObject;
                if (badge.activeSelf != show) badge.SetActive(show);
                badges[i].text = t.Badge;
            }
            MoveIndicator(active);
        }

        /// <summary>
        /// Le bloc sous l'onglet actif glisse jusqu'au nouvel onglet (placé d'un coup la première fois). Il devient enfant de
        /// l'onglet actif : il reste aligné quelle que soit la taille de l'écran.
        /// </summary>
        void MoveIndicator(int active)
        {
            if (tabIndicator == null || active < 0 || active == _activeTab) return;
            bool first = _activeTab < 0;
            _activeTab = active;
            var from = tabIndicator.position;
            tabIndicator.SetParent(tabs[active].transform, false);
            tabIndicator.anchorMin = tabIndicator.anchorMax = new Vector2(0.5f, 0f);
            tabIndicator.pivot = new Vector2(0.5f, 1f);
            tabIndicator.position = from;
            var to = new Vector2(0, -2);
            if (first || !UnityEngine.Application.isPlaying) tabIndicator.anchoredPosition = to;
            else Tween.Move(tabIndicator, to, IndicatorDuration);
        }

        public GameObject Spawn(GameObject pagePrefab)
        {
            var go = Instantiate(pagePrefab, pageArea, false);
            var rt = (RectTransform)go.transform;
            rt.Stretch();
            go.transform.SetAsLastSibling();
            if (UnityEngine.Application.isPlaying)
            {
                rt.anchoredPosition = new Vector2(0, -PageSlide);
                Tween.Move(rt, Vector2.zero, SlideDuration);
            }
            return go;
        }

        public void Despawn(GameObject page)
        {
            if (page != null) Destroy(page);
        }
    }
}
