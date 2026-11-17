#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Tutorial;
using N29.HeroesDawn.Presentation.UiToolkit;
using UnityEngine;
using UnityEngine.UIElements;
using DefPosition = N29.HeroesDawn.Definitions.Position;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>
    /// Page Recruter (UI Toolkit, planche 10.1) : onglets de la boutique à gauche, règle et compte à rebours, trois cartes d'agent en portrait
    /// (cadre de famille, nom, prix en pastille), lots de scraps, Renouveler, et à droite la fiche du héros choisi.
    /// </summary>
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
            public VisualElement Root = null!, Art = null!, Bust = null!, Frame = null!, FamilyIcon = null!, Body = null!, Role = null!, Rank = null!;
            public Label Name = null!, Rarity = null!, InTeam = null!;
            public UnityEngine.UIElements.Button Recruit = null!;
            public string BustClass = "";
            public CardInfo Info = new CardInfo();
        }

        sealed class StatCell
        {
            public VisualElement Root = null!;
            public Label Name = null!, Value = null!;
        }

        const string PartSeparator = " · ";
        static readonly string[] FamilyKeys = { "matter", "elemental", "bio", "perception" };
        static readonly string[] StatIcons = { "hd-ico--health", "hd-ico--attack", "hd-ico--defense", "hd-ico--speed" };

        readonly List<HeroCard> _cards = new List<HeroCard>();
        readonly List<UnityEngine.UIElements.Button> _scraps = new List<UnityEngine.UIElements.Button>();
        readonly List<StatCell> _stats = new List<StatCell>();
        readonly List<Label> _skillChips = new List<Label>();
        readonly int[] _slots = new int[8];
        readonly int[] _scrapSlots = new int[8];
        VisualElement _cardsBox = null!, _scrapsBox = null!, _dFamily = null!, _dStatGrid = null!, _dSkillList = null!;
        Label _rule = null!, _countdown = null!, _empty = null!, _progress = null!;
        Label _dName = null!, _dRarity = null!, _dRole = null!, _dStats = null!, _dSkills = null!, _dLevel = null!;
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
        /// <summary>Colonne d'onglets de la boutique (tests PlayMode).</summary>
        public HdShopTabs ShopTabs => _tabs!;

        protected override void Bind()
        {
            // Zone centrale à défilement vertical tactile : glisser pour défiler, aucune barre visible qui capterait le pointeur.
            var scroll = Q<ScrollView>("recruitScroll");
            scroll.mode = ScrollViewMode.Vertical;
            scroll.verticalScrollerVisibility = ScrollerVisibility.Hidden;
            scroll.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            scroll.touchScrollBehavior = ScrollView.TouchScrollBehavior.Elastic;
            // Un bouton pris au focus (manette, clavier) est ramené dans la zone visible.
            scroll.RegisterCallback<FocusInEvent>(e => { if (e.target is VisualElement v && v != scroll && v.focusable) scroll.ScrollTo(v); });
            _cardsBox = Q<VisualElement>("cards"); _scrapsBox = Q<VisualElement>("scraps");
            _rule = Q<Label>("rule"); _countdown = Q<Label>("countdown"); _empty = Q<Label>("empty"); _progress = Q<Label>("progress");
            _dName = Q<Label>("detailName"); _dRarity = Q<Label>("detailRarity"); _dRole = Q<Label>("detailRole");
            _dStats = Q<Label>("detailStats"); _dSkills = Q<Label>("detailSkills"); _dLevel = Q<Label>("detailLevel");
            _dFamily = Q<VisualElement>("detailFamily"); _dStatGrid = Q<VisualElement>("detailStatGrid"); _dSkillList = Q<VisualElement>("detailSkillList");
            Q<Label>("detailSkillsTitle").text = Loc.T("sheet.skills");
            _reroll = Q<UnityEngine.UIElements.Button>("reroll"); _registry = Q<UnityEngine.UIElements.Button>("registry");
            _dRecruit = Q<UnityEngine.UIElements.Button>("detailRecruit");
            _reroll.clicked += () => RerollClicked?.Invoke();
            HdButtonContent.Fill(_registry, Loc.T("agents.open"), "hd-ico--register");
            _registry.clicked += () => RegistryClicked?.Invoke();
            _dRecruit.clicked += () => RecruitClicked?.Invoke(_slots[Mathf.Clamp(_selected, 0, _slots.Length - 1)]);
            _tabs = new HdShopTabs(Q<VisualElement>("shopTabs"), this);
            _tabs.TabClicked += i => ShopTabClicked?.Invoke(i);
            if (_activeTab >= 0) _tabs.Show(_activeTab);
            _cards.Clear(); _scraps.Clear(); _stats.Clear(); _skillChips.Clear();
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
                HdButtonContent.FillPrice(_scraps[i], d.Scraps[i].Text);
                _scraps[i].SetEnabled(d.Scraps[i].Enabled);
            }
            _empty.text = d.EmptyText;
            _empty.style.display = d.Cards.Length == 0 ? DisplayStyle.Flex : DisplayStyle.None;
            _progress.text = d.Progress;
            HdButtonContent.FillPrice(_reroll, d.RerollLabel, "hd-ico--refresh");
            _reroll.SetEnabled(d.RerollEnabled);
            if (_selected >= d.Cards.Length) _selected = 0;
            ShowDetail();
        }

        static Label Text(string cls)
        {
            var l = new Label { pickingMode = PickingMode.Ignore };
            l.AddToClassList(cls);
            return l;
        }

        static VisualElement Box(string cls)
        {
            var e = new VisualElement { pickingMode = PickingMode.Ignore };
            e.AddToClassList(cls);
            return e;
        }

        HeroCard NewCard(int index)
        {
            var c = new HeroCard { Root = new UnityEngine.UIElements.Button() };
            c.Root.RemoveFromClassList("unity-button");
            c.Root.AddToClassList("hd-hero");
            c.Art = Box("hd-hero__art");
            // Portrait cadré sur le visage (kit) posé sur l'illustration : sans portrait au kit, l'illustration reste visible.
            c.Bust = Box("hd-hero__bust");
            c.Frame = Box("hd-hero__frame");
            c.FamilyIcon = Box("hd-famicon"); c.FamilyIcon.AddToClassList("hd-hero__famicon");
            c.Body = Box("hd-hero__body");
            c.Name = Text("hd-hero__name");
            c.Rarity = Text("hd-hero__rarity");
            c.InTeam = Text("hd-hero__inteam");
            c.Recruit = new UnityEngine.UIElements.Button();
            c.Recruit.AddToClassList("hd-btn"); c.Recruit.AddToClassList("hd-btn--small"); c.Recruit.AddToClassList("hd-hero__price");
            c.Recruit.clicked += () => RecruitClicked?.Invoke(_slots[index]);
            // Position (teinte de famille, sous l'icône de famille) et insigne de rang AMIF (coin haut droit).
            c.Role = HdTeamUi.PositionIcon(DefPosition.FrontLine, Family.Matter); c.Role.AddToClassList("hd-hero__role");
            c.Rank = HdTeamUi.RankBadge(1); c.Rank.AddToClassList("hd-hero__rank");
            c.Art.Add(c.Bust); c.Art.Add(Box("hd-hero__shade")); c.Art.Add(c.FamilyIcon); c.Art.Add(c.Role); c.Art.Add(c.Rank);
            c.Body.Add(c.Name); c.Body.Add(c.Rarity);
            c.Root.Add(c.Art); c.Root.Add(c.Body); c.Root.Add(c.InTeam); c.Root.Add(c.Recruit); c.Root.Add(c.Frame);
            void Tap()
            {
                if (_selected == index) CardTapped?.Invoke(_slots[index]);
                else { _selected = index; RefreshSelection(); ShowDetail(); }
            }
            // Le clic sur la pastille de prix est celui du bouton Recruter, pas celui de la carte.
            c.Root.RegisterCallback<ClickEvent>(e => { if (e.target == c.Root) Tap(); });
            c.Root.RegisterCallback<NavigationSubmitEvent>(e => { if (e.target == c.Root) Tap(); });
            c.Info.RecruitButton = new HdButtonRef(c.Recruit, this);
            if (index == 0) HdTutorialAnchor.Attach(this, c.Recruit, TutorialTarget.RecruitCard);
            c.Info.TapButton = new HdButtonRef((UnityEngine.UIElements.Button)c.Root, this);
            return c;
        }

        void RefreshSelection()
        {
            for (int i = 0; i < _cards.Count; i++) _cards[i].Root.EnableInClassList("hd-hero--selected", i == _selected);
        }

        static string FamilyKey(Family f)
            => f == Family.Matter ? FamilyKeys[0] : f == Family.Elemental ? FamilyKeys[1] : f == Family.PhysicalBio ? FamilyKeys[2] : FamilyKeys[3];

        /// <summary>Pose la classe de famille (cadre, icône) et retire celles des autres familles.</summary>
        static void SetFamily(VisualElement e, string prefix, Family family)
        {
            string key = FamilyKey(family);
            foreach (var k in FamilyKeys) e.EnableInClassList(prefix + k, k == key);
        }

        static void ShowCard(HeroCard c, RecruitCardState d, bool selected)
        {
            c.Info.State = d;
            c.Root.EnableInClassList("hd-hero--legend", d.Halo);
            c.Root.EnableInClassList("hd-hero--selected", selected);
            c.Root.EnableInClassList("hd-hero--taken", d.InTeam);
            SetFamily(c.Frame, "hd-hero__frame--", d.Card.Family);
            SetFamily(c.FamilyIcon, "hd-famicon--", d.Card.Family);
            HdTeamUi.SetPosition(c.Role, d.Role, d.Card.Family);
            HdTeamUi.SetRank(c.Rank, d.Card.PowerTier);
            var art = d.Card.Portrait.Art;
            c.Art.style.backgroundImage = art != null ? new StyleBackground(art) : new StyleBackground(StyleKeyword.Null);
            string bust = "hd-bust--" + d.Card.Id.Value;
            if (bust != c.BustClass)
            {
                if (c.BustClass.Length > 0) c.Bust.RemoveFromClassList(c.BustClass);
                c.Bust.AddToClassList(bust);
                c.BustClass = bust;
            }
            c.Name.text = d.Card.Name;
            c.Rarity.text = d.Card.RarityText; c.Rarity.style.color = Theme.RarityColor(d.Rarity);
            HdButtonContent.FillPrice(c.Recruit, d.PriceLabel, null, true);
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
            _dLevel.text = s.Card.LevelText ?? "";
            _dLevel.style.display = string.IsNullOrEmpty(s.Card.LevelText) ? DisplayStyle.None : DisplayStyle.Flex;
            SetFamily(_dFamily, "hd-famicon--", s.Card.Family);
            ShowStats(s.Card.Stats ?? "");
            ShowSkills(s.Card.Skills ?? "");
            HdButtonContent.FillPrice(_dRecruit, s.PriceLabel, "hd-ico--star");
            _dRecruit.style.display = s.InTeam ? DisplayStyle.None : DisplayStyle.Flex;
            _dRecruit.EnableInClassList("hd-btn--primary", s.Affordable);
            _dRecruit.EnableInClassList("hd-btn--secondary", !s.Affordable);
        }

        /// <summary>
        /// Quatre statistiques à icône (PV, ATQ, DÉF, VIT) tirées de la ligne « PV 1671 · ATQ 150 · … » ;
        /// une ligne d'une autre forme est affichée telle quelle.
        /// </summary>
        void ShowStats(string line)
        {
            var parts = line.Split(new[] { PartSeparator }, StringSplitOptions.RemoveEmptyEntries);
            bool grid = parts.Length == StatIcons.Length;
            for (int i = 0; grid && i < parts.Length; i++) grid = parts[i].Trim().LastIndexOf(' ') > 0;
            _dStatGrid.style.display = grid ? DisplayStyle.Flex : DisplayStyle.None;
            _dStats.style.display = grid ? DisplayStyle.None : DisplayStyle.Flex;
            _dStats.text = line;
            if (!grid) return;
            while (_stats.Count < parts.Length)
            {
                var cell = new StatCell { Root = Box("hd-stat"), Name = Text("hd-stat__label"), Value = Text("hd-stat__value") };
                var icon = Box("hd-stat__icon"); icon.AddToClassList(StatIcons[_stats.Count]);
                cell.Root.Add(icon); cell.Root.Add(cell.Name); cell.Root.Add(cell.Value);
                _stats.Add(cell); _dStatGrid.Add(cell.Root);
            }
            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i].Trim();
                int cut = part.LastIndexOf(' ');
                _stats[i].Name.text = part.Substring(0, cut);
                _stats[i].Value.text = part.Substring(cut + 1);
            }
        }

        /// <summary>Une vignette par compétence (la ligne « A · B » du héros).</summary>
        void ShowSkills(string line)
        {
            var parts = line.Split(new[] { PartSeparator }, StringSplitOptions.RemoveEmptyEntries);
            // Le libellé d'origine garde son texte mais les vignettes le remplacent à l'écran.
            _dSkills.text = line;
            _dSkills.style.display = DisplayStyle.None;
            while (_skillChips.Count < parts.Length) { var chip = Text("hd-recruit__skill"); _skillChips.Add(chip); _dSkillList.Add(chip); }
            for (int i = 0; i < _skillChips.Count; i++)
            {
                bool used = i < parts.Length;
                _skillChips[i].style.display = used ? DisplayStyle.Flex : DisplayStyle.None;
                if (used) _skillChips[i].text = parts[i].Trim();
            }
        }
    }
}
