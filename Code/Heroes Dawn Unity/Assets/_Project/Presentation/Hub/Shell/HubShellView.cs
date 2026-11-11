#nullable enable
using System;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Feedback;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Presentation.Tutorial;
using N29.HeroesDawn.Presentation.UiToolkit;
using N29.HeroesDawn.Presentation.Wip;
using UnityEngine;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>
    /// Coque du hub (UI Toolkit) : fond, barre haute (Retour, titre, chef d'unité, ressources, réglages) et barre de navigation basse.
    /// Les pages s'insèrent dans la zone <c>pageArea</c> (RectTransform entre les deux barres) : elles restent en uGUI tant qu'elles
    /// n'ont pas été migrées, puis deviendront des documents UI Toolkit posés entre les barres.
    /// </summary>
    public sealed class HubShellView : HdView, IHubShellView, IPageHost
    {
        const float PageSlide = 28f, SlideDuration = 0.22f;

        [SerializeField] RectTransform pageArea = null!;
        [SerializeField] UiTapRelay relay = null!;

        VisualElement _backdrop = null!;
        Label _title = null!, _avatarName = null!, _avatarRole = null!;
        VisualElement _avatar = null!, _avatarPortrait = null!, _tabsRow = null!;
        UnityEngine.UIElements.Button _back = null!, _settings = null!;
        readonly UnityEngine.UIElements.Button[] _tabs = new UnityEngine.UIElements.Button[HubNavigator.Tabs.Length];
        readonly Label[] _badges = new Label[HubNavigator.Tabs.Length];
        readonly VisualElement[] _badgeBoxes = new VisualElement[HubNavigator.Tabs.Length];
        readonly HdButtonRef[] _tabRefs = new HdButtonRef[HubNavigator.Tabs.Length];
        readonly WalletBar _wallet = new WalletBar();
        readonly System.Collections.Generic.List<HdTutorialAnchor> _anchors = new System.Collections.Generic.List<HdTutorialAnchor>();

        public event Action<HubPage>? TabClicked;
        public event Action? BackClicked;
        public event Action? SettingsClicked;

        public IWalletView Wallet => _wallet;
        /// <summary>Barre de ressources (nombre d affichages lu par les tests).</summary>
        public WalletBar Bar => _wallet;
        public IPageHost Pages => this;
        public IUiTapRelay Relay => relay;

        public HdButtonRef BackButton { get; private set; } = null!;
        public HdButtonRef SettingsButton { get; private set; } = null!;

        /// <summary>Bouton d'un onglet (tests PlayMode : clics réels).</summary>
        public HdButtonRef TabButton(HubPage tab) => _tabRefs[Array.IndexOf(HubNavigator.Tabs, tab)];

        protected override void Bind()
        {
            _backdrop = Q<VisualElement>("backdrop");
            _title = Q<Label>("title");
            _avatar = Q<VisualElement>("avatar"); _avatarPortrait = Q<VisualElement>("avatarPortrait");
            _avatarName = Q<Label>("avatarName"); _avatarRole = Q<Label>("avatarRole");
            _back = Q<UnityEngine.UIElements.Button>("back"); _settings = Q<UnityEngine.UIElements.Button>("settings");
            _tabsRow = Q<VisualElement>("tabs");
            _wallet.Attach(Q<Label>("credits"), Q<Label>("pr"), Q<Label>("gems"), Q<Label>("saving"));
            foreach (var old in _anchors) if (old != null) Destroy(old.gameObject);
            _anchors.Clear();
            BackButton = new HdButtonRef(_back, this); SettingsButton = new HdButtonRef(_settings, this);
            _back.clicked += () => BackClicked?.Invoke();
            _settings.clicked += () => SettingsClicked?.Invoke();
            _back.style.display = DisplayStyle.None;

            _tabsRow.Clear();
            for (int i = 0; i < HubNavigator.Tabs.Length; i++)
            {
                var page = HubNavigator.Tabs[i];
                var tab = new UnityEngine.UIElements.Button();
                tab.AddToClassList("hd-navtab");
                var icon = new VisualElement { pickingMode = PickingMode.Ignore };
                icon.AddToClassList("hd-navtab__icon");
                icon.AddToClassList(IconClass(page));
                tab.Add(icon);
                var badge = new VisualElement { pickingMode = PickingMode.Ignore };
                badge.AddToClassList("hd-navtab__badge");
                var badgeText = new Label { pickingMode = PickingMode.Ignore };
                badge.Add(badgeText);
                badge.style.display = DisplayStyle.None;
                tab.Add(badge);
                tab.clicked += () => TabClicked?.Invoke(page);
                _tabsRow.Add(tab);
                _tabs[i] = tab; _badges[i] = badgeText; _badgeBoxes[i] = badge;
                _tabRefs[i] = new HdButtonRef(tab, this);
                if (page == HubPage.Team) _anchors.Add(HdTutorialAnchor.Attach(this, tab, TutorialTarget.TabTeam));
                if (page == HubPage.Shop) _anchors.Add(HdTutorialAnchor.Attach(this, tab, TutorialTarget.TabRecruit));
            }
        }

        static string IconClass(HubPage page) => page switch
        {
            HubPage.Team => "hd-icon--team",
            HubPage.Pass => "hd-icon--pass",
            HubPage.Shop or HubPage.Recruit => "hd-icon--shop",
            HubPage.Profile => "hd-icon--profile",
            _ => "hd-icon--qg",
        };

        public void ShowChrome(HubShellData d)
        {
            _title.text = d.Title;
            bool home = d.Tabs != null && d.Tabs.Length > 0 && d.Tabs[0].Active && !d.ShowBack;
            _backdrop.EnableInClassList("hd-shell__backdrop--home", home);
            if (!string.IsNullOrEmpty(d.BackgroundKey))
            {
                var art = TacticalArt.Load(d.BackgroundKey);
                if (art != null) _backdrop.style.backgroundImage = new StyleBackground(art);
            }
            bool hasAvatar = !string.IsNullOrEmpty(d.AvatarName);
            _avatar.style.display = hasAvatar ? DisplayStyle.Flex : DisplayStyle.None;
            if (hasAvatar)
            {
                _avatarName.text = d.AvatarName;
                _avatarRole.text = d.AvatarRole;
                _avatarPortrait.style.backgroundImage = d.Avatar.Art != null ? new StyleBackground(d.Avatar.Art) : new StyleBackground(StyleKeyword.Null);
            }
            _back.style.display = d.ShowBack ? DisplayStyle.Flex : DisplayStyle.None;
            _back.text = Loc.T("nav.back");
            _settings.text = Loc.T("nav.settings");
            for (int i = 0; i < _tabs.Length && i < d.Tabs.Length; i++)
            {
                var t = d.Tabs[i];
                _tabs[i].text = t.Label;
                _tabs[i].EnableInClassList("hd-navtab--active", t.Active);
                _tabs[i].style.display = t.Hidden ? DisplayStyle.None : DisplayStyle.Flex;
                bool show = !string.IsNullOrEmpty(t.Badge);
                _badgeBoxes[i].style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
                _badges[i].text = t.Badge;
            }
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

        /// <summary>Ressources de la barre haute : Crédits, PR, Gemmes et indicateur de sauvegarde (le compteur roule vers la nouvelle valeur).</summary>
        public sealed class WalletBar : IWalletView
        {
            Label _credits = null!, _pr = null!, _gems = null!, _saving = null!;
            string _savingText = "";
            int _lastCredits = -1, _lastPr = -1, _lastGems;

            /// <summary>Le document est recréé à chaque affichage : les étiquettes changent, les compteurs restent.</summary>
            public void Attach(Label credits, Label pr, Label gems, Label saving)
            {
                _credits = credits; _pr = pr; _gems = gems; _saving = saving;
                if (_lastCredits >= 0) { _credits.text = _lastCredits.ToString("N0"); _pr.text = _lastPr.ToString("N0"); _gems.text = _lastGems.ToString("N0"); }
                _saving.text = _savingText;
            }

            public int Shows { get; private set; }

            public void Show(WalletData d)
            {
                Shows++;
                _lastGems = d.Gems;
                _gems.text = d.Gems.ToString("N0");
                Roll(_credits, _lastCredits, d.Credits);
                Roll(_pr, _lastPr, d.Pr);
                _lastCredits = d.Credits; _lastPr = d.Pr;
            }

            static void Roll(Label label, int from, int to)
            {
                if (from < 0 || from == to || !UnityEngine.Application.isPlaying) { label.text = to.ToString("N0"); return; }
                float duration = CounterRoll.DurationFor(from, to);
                long start = (long)(Time.realtimeSinceStartup * 1000f);
                IVisualElementScheduledItem? item = null;
                item = label.schedule.Execute(() =>
                {
                    float k = Mathf.Clamp01((Time.realtimeSinceStartup * 1000f - start) / (duration * 1000f));
                    label.text = CounterRoll.At(from, to, k).ToString("N0");
                    if (k >= 1f) item?.Pause();
                }).Every(16);
            }

            public void SetSaving(bool on) { _savingText = on ? Loc.T("ui.saving") : ""; _saving.text = _savingText; }
        }
    }
}
