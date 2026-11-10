#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.UiToolkit;
using UnityEngine;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Page Recruter (UI Toolkit) : onglets de la boutique, règle, compte à rebours, trois cartes, lots de scraps, Renouveler, fiche du héros choisi.</summary>
    public sealed class RecruitPageView : HdView, IRecruitPageView
    {
        /// <summary>Ce que les tests lisent d'une carte de héros affichée.</summary>
        public sealed class CardInfo
        {
            public HdButtonRef RecruitButton = null!;
            public HdButtonRef TapButton = null!;
            public bool Visible;
            public RecruitCardState State;
        }

        sealed class HeroCard
        {
            public VisualElement Root = null!, Art = null!, Body = null!;
            public Label Name = null!, Rarity = null!, Role = null!, Stats = null!, InTeam = null!;
            public UnityEngine.UIElements.Button Recruit = null!;
            public CardInfo Info = new CardInfo();
        }

        readonly List<HeroCard> _cards = new List<HeroCard>();
        readonly List<UnityEngine.UIElements.Button> _scraps = new List<UnityEngine.UIElements.Button>();
        readonly int[] _slots = new int[8];
        readonly int[] _scrapSlots = new int[8];
        VisualElement _cardsBox = null!, _scrapsBox = null!;
        Label _rule = null!, _countdown = null!, _empty = null!, _progress = null!;
        Label _dName = null!, _dRarity = null!, _dRole = null!, _dStats = null!, _dSkills = null!;
        UnityEngine.UIElements.Button _reroll = null!, _registry = null!, _dRecruit = null!;
        HdShopTabs? _tabs;
        RecruitPageData? _last;
        int _selected, _activeTab = -1;

        public event Action<int>? CardTapped;
        public event Action<int>? RecruitClicked;
        public event Action? RerollClicked;
        public event Action<int>? ScrapClicked;
        public event Action? RegistryClicked;
        public event Action<int>? ShopTabClicked;

        public HdButtonRef RerollButton => new HdButtonRef(_reroll, this);
        public HdButtonRef RegistryButton => new HdButtonRef(_registry, this);
        public CardInfo Card(int index) => _cards[index].Info;
        public int VisibleCardCount { get { int n = 0; foreach (var c in _cards) if (c.Info.Visible) n++; return n; } }
        /// <summary>Bandeau d'onglets de la boutique (tests PlayMode).</summary>
        public HdShopTabs ShopTabs => _tabs!;

        protected override void Bind()
        {
            _cardsBox = Q<VisualElement>("cards"); _scrapsBox = Q<VisualElement>("scraps");
            _rule = Q<Label>("rule"); _countdown = Q<Label>("countdown"); _empty = Q<Label>("empty"); _progress = Q<Label>("progress");
            _dName = Q<Label>("detailName"); _dRarity = Q<Label>("detailRarity"); _dRole = Q<Label>("detailRole");
            _dStats = Q<Label>("detailStats"); _dSkills = Q<Label>("detailSkills");
            _reroll = Q<UnityEngine.UIElements.Button>("reroll"); _registry = Q<UnityEngine.UIElements.Button>("registry");
            _dRecruit = Q<UnityEngine.UIElements.Button>("detailRecruit");
            _reroll.clicked += () => RerollClicked?.Invoke();
            _registry.text = Loc.T("agents.open");
            _registry.clicked += () => RegistryClicked?.Invoke();
            _dRecruit.clicked += () => RecruitClicked?.Invoke(_slots[Mathf.Clamp(_selected, 0, _slots.Length - 1)]);
            _tabs = new HdShopTabs(Q<VisualElement>("shopTabs"), this);
            _tabs.TabClicked += i => ShopTabClicked?.Invoke(i);
            if (_activeTab >= 0) _tabs.Show(_activeTab);
            _cards.Clear(); _scraps.Clear();
            if (_last.HasValue) Show(_last.Value);
        }

        public void ShowShopTab(int active)
        {
            _activeTab = active;
            _tabs?.Show(active);
        }

        public void Show(RecruitPageData d)
        {
            _last = d;
            if (_cardsBox == null) return;
            _rule.text = d.Rule;
            _countdown.text = d.Countdown;
            while (_cards.Count < d.Cards.Length) { var c = NewCard(_cards.Count); _cards.Add(c); _cardsBox.Add(c.Root); }
            for (int i = 0; i < _cards.Count; i++)
            {
                bool used = i < d.Cards.Length;
                _cards[i].Root.style.display = used ? DisplayStyle.Flex : DisplayStyle.None;
                _cards[i].Info.Visible = used;
                if (!used) continue;
                _slots[i] = d.Cards[i].Slot;
                ShowCard(_cards[i], d.Cards[i], i == _selected);
            }
            while (_scraps.Count < d.Scraps.Length)
            {
                int index = _scraps.Count;
                var b = new UnityEngine.UIElements.Button();
                b.AddToClassList("hd-btn"); b.AddToClassList("hd-btn--secondary"); b.AddToClassList("hd-btn--small"); b.AddToClassList("hd-recruit__scrap");
                b.clicked += () => ScrapClicked?.Invoke(_scrapSlots[index]);
                _scraps.Add(b); _scrapsBox.Add(b);
            }
            for (int i = 0; i < _scraps.Count; i++)
            {
                bool has = i < d.Scraps.Length;
                _scraps[i].style.display = has ? DisplayStyle.Flex : DisplayStyle.None;
                if (!has) continue;
                _scrapSlots[i] = d.Scraps[i].Slot;
                _scraps[i].text = d.Scraps[i].Text;
                _scraps[i].SetEnabled(d.Scraps[i].Enabled);
            }
            _empty.text = d.EmptyText;
            _empty.style.display = d.Cards.Length == 0 ? DisplayStyle.Flex : DisplayStyle.None;
            _progress.text = d.Progress;
            _reroll.text = d.RerollLabel;
            _reroll.SetEnabled(d.RerollEnabled);
            if (_selected >= d.Cards.Length) _selected = 0;
            ShowDetail();
        }

        HeroCard NewCard(int index)
        {
            var c = new HeroCard { Root = new UnityEngine.UIElements.Button() };
            c.Root.RemoveFromClassList("unity-button");
            c.Root.AddToClassList("hd-hero");
            c.Art = new VisualElement { pickingMode = PickingMode.Ignore }; c.Art.AddToClassList("hd-hero__art");
            c.Body = new VisualElement { pickingMode = PickingMode.Ignore }; c.Body.AddToClassList("hd-hero__body");
            c.Name = new Label { pickingMode = PickingMode.Ignore }; c.Name.AddToClassList("hd-hero__name");
            c.Rarity = new Label { pickingMode = PickingMode.Ignore }; c.Rarity.AddToClassList("hd-hero__rarity");
            c.Role = new Label { pickingMode = PickingMode.Ignore }; c.Role.AddToClassList("hd-hero__role");
            c.Stats = new Label { pickingMode = PickingMode.Ignore }; c.Stats.AddToClassList("hd-hero__stats");
            c.InTeam = new Label { pickingMode = PickingMode.Ignore }; c.InTeam.AddToClassList("hd-hero__inteam");
            c.Recruit = new UnityEngine.UIElements.Button();
            c.Recruit.AddToClassList("hd-btn"); c.Recruit.AddToClassList("hd-btn--small");
            c.Recruit.clicked += () => RecruitClicked?.Invoke(_slots[index]);
            c.Body.Add(c.Name); c.Body.Add(c.Rarity); c.Body.Add(c.Role); c.Body.Add(c.Stats);
            c.Root.Add(c.Art); c.Root.Add(c.Body); c.Root.Add(c.InTeam); c.Root.Add(c.Recruit);
            void Tap()
            {
                if (_selected == index) CardTapped?.Invoke(_slots[index]);
                else { _selected = index; RefreshSelection(); ShowDetail(); }
            }
            c.Root.RegisterCallback<ClickEvent>(e => { if (!(e.target is UnityEngine.UIElements.Button)) Tap(); });
            c.Root.RegisterCallback<NavigationSubmitEvent>(e => { if (e.target == c.Root) Tap(); });
            c.Info.RecruitButton = new HdButtonRef(c.Recruit, this);
            c.Info.TapButton = new HdButtonRef((UnityEngine.UIElements.Button)c.Root, this);
            return c;
        }

        void RefreshSelection()
        {
            for (int i = 0; i < _cards.Count; i++) _cards[i].Root.EnableInClassList("hd-hero--selected", i == _selected);
        }

        static void ShowCard(HeroCard c, RecruitCardState d, bool selected)
        {
            var color = Theme.RarityColor(d.Rarity);
            c.Info.State = d;
            c.Root.style.borderTopColor = c.Root.style.borderBottomColor = c.Root.style.borderLeftColor = c.Root.style.borderRightColor = color;
            c.Root.EnableInClassList("hd-hero--legend", d.Halo);
            c.Root.EnableInClassList("hd-hero--selected", selected);
            c.Root.EnableInClassList("hd-hero--taken", d.InTeam);
            var art = d.Card.Portrait.Art;
            c.Art.style.backgroundImage = art != null ? new StyleBackground(art) : new StyleBackground(StyleKeyword.Null);
            c.Art.style.borderBottomColor = Theme.FamilyColor(d.Card.Family);
            c.Name.text = d.Card.Name;
            c.Rarity.text = d.Card.RarityText; c.Rarity.style.color = color;
            c.Role.text = d.Card.RoleLine;
            c.Stats.text = d.Card.Stats;
            c.Recruit.text = d.PriceLabel;
            c.Recruit.style.display = d.InTeam ? DisplayStyle.None : DisplayStyle.Flex;
            c.Recruit.EnableInClassList("hd-btn--primary", d.Affordable);
            c.Recruit.EnableInClassList("hd-btn--secondary", !d.Affordable);
            c.InTeam.text = d.InTeamText;
            c.InTeam.style.display = d.InTeam ? DisplayStyle.Flex : DisplayStyle.None;
        }

        void ShowDetail()
        {
            RefreshSelection();
            if (_cards.Count == 0) return;
            var s = _cards[Mathf.Clamp(_selected, 0, _cards.Count - 1)].Info.State;
            if (s.Card.Name == null) return;
            _dName.text = s.Card.Name;
            _dRarity.text = s.Card.RarityText; _dRarity.style.color = Theme.RarityColor(s.Rarity);
            _dRole.text = s.Card.RoleLine;
            _dStats.text = s.Card.Stats;
            _dSkills.text = s.Card.Skills;
            _dRecruit.text = s.PriceLabel;
            _dRecruit.style.display = s.InTeam ? DisplayStyle.None : DisplayStyle.Flex;
            _dRecruit.EnableInClassList("hd-btn--primary", s.Affordable);
            _dRecruit.EnableInClassList("hd-btn--secondary", !s.Affordable);
        }
    }
}
