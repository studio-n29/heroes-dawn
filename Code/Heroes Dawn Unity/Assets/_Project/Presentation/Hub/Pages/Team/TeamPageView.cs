#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Tutorial;
using N29.HeroesDawn.Presentation.UiToolkit;
using UnityEngine;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Page Équipe (UI Toolkit) : filtres et tri, grille de cartes, escouade, synergies, compositions, « Préparer une mission ».</summary>
    public sealed class TeamPageView : HdView, ITeamPageView
    {
        static readonly MemberSort[] SortOrder = { MemberSort.Level, MemberSort.Rarity, MemberSort.Fatigue };

        /// <summary>Ce que les tests lisent d'une carte de la grille.</summary>
        public sealed class TileInfo
        {
            public HdButtonRef TapButton = null!;
            public HdButtonRef SquadButton = null!;
        }

        sealed class Tile
        {
            public VisualElement Root = null!, Art = null!, FatigueFill = null!;
            public Label Name = null!, Level = null!, Rarity = null!, Status = null!, FatigueLabel = null!;
            public UnityEngine.UIElements.Button Squad = null!;
        }

        readonly List<TileInfo> _infos = new List<TileInfo>();
        readonly List<UnityEngine.UIElements.Button> _filters = new List<UnityEngine.UIElements.Button>();
        readonly List<UnityEngine.UIElements.Button> _sorts = new List<UnityEngine.UIElements.Button>();
        readonly List<UnityEngine.UIElements.Button> _presets = new List<UnityEngine.UIElements.Button>();
        ScrollView _grid = null!;
        VisualElement _squadRow = null!, _presetBox = null!;
        Label _squadTitle = null!, _synergies = null!, _synergiesTitle = null!, _hint = null!;
        UnityEngine.UIElements.Button _healAll = null!, _mission = null!, _presetSave = null!;
        TeamPageData? _last;
        HdTutorialAnchor? _tileAnchor;

        public event Action<int>? FilterClicked;
        public event Action<MemberSort>? SortClicked;
        public event Action<MemberId>? MemberTapped;
        public event Action<MemberId>? SquadToggled;
        public event Action? MissionClicked;
        public event Action? HealAllClicked;
        public event Action<int>? PresetClicked;
        public event Action? PresetSaveClicked;

        public HdButtonRef MissionButton => new HdButtonRef(_mission, this);
        public HdButtonRef HealAllButton => new HdButtonRef(_healAll, this);
        public IReadOnlyList<TileInfo> Tiles => _infos;

        protected override void Bind()
        {
            _grid = Q<ScrollView>("grid"); _squadRow = Q<VisualElement>("squadRow"); _presetBox = Q<VisualElement>("presets");
            _squadTitle = Q<Label>("squadTitle"); _synergies = Q<Label>("synergies"); _synergiesTitle = Q<Label>("synergiesTitle"); _hint = Q<Label>("missionHint");
            _healAll = Q<UnityEngine.UIElements.Button>("healAll"); _mission = Q<UnityEngine.UIElements.Button>("mission");
            _grid.contentContainer.style.flexDirection = FlexDirection.Row;
            _grid.contentContainer.style.flexWrap = Wrap.Wrap;
            _synergiesTitle.text = Loc.T("team.synergies");
            _mission.text = Loc.T("team.mission");
            _mission.clicked += () => MissionClicked?.Invoke();
            _healAll.clicked += () => HealAllClicked?.Invoke();
            _filters.Clear(); _sorts.Clear(); _presets.Clear(); _infos.Clear();
            var filterBox = Q<VisualElement>("filters");
            filterBox.Clear();
            AddChip(filterBox, _filters, Loc.T("team.filter.all"), 0, i => FilterClicked?.Invoke(i));
            int index = 1;
            foreach (Family f in Enum.GetValues(typeof(Family)))
                AddChip(filterBox, _filters, Loc.T("family." + f), index++, i => FilterClicked?.Invoke(i));
            var sortBox = Q<VisualElement>("sorts");
            sortBox.Clear();
            var sortLabel = new Label(Loc.T("team.sort"));
            sortLabel.AddToClassList("hd-team__sortlabel");
            sortBox.Add(sortLabel);
            string[] keys = { "team.sort.level", "team.sort.rarity", "team.sort.fatigue" };
            for (int i = 0; i < SortOrder.Length; i++)
            {
                var sort = SortOrder[i];
                AddChip(sortBox, _sorts, Loc.T(keys[i]), i, _ => SortClicked?.Invoke(sort));
            }
            _presetBox.Clear();
            for (int i = 0; i < 3; i++) AddChip(_presetBox, _presets, "", i, k => PresetClicked?.Invoke(k));
            _presetSave = new UnityEngine.UIElements.Button();
            _presetSave.AddToClassList("hd-btn"); _presetSave.AddToClassList("hd-btn--small"); _presetSave.AddToClassList("hd-team__chip");
            _presetSave.clicked += () => PresetSaveClicked?.Invoke();
            _presetBox.Add(_presetSave);
            HdTutorialAnchor.Attach(this, Q<VisualElement>("side"), TutorialTarget.TeamSquad);
            HdTutorialAnchor.Attach(this, _mission, TutorialTarget.TeamMission);
            _tileAnchor = null;
            if (_last.HasValue) Show(_last.Value);
        }

        static void AddChip(VisualElement host, List<UnityEngine.UIElements.Button> list, string label, int index, Action<int> click)
        {
            var b = new UnityEngine.UIElements.Button { text = label };
            b.AddToClassList("hd-btn"); b.AddToClassList("hd-btn--small"); b.AddToClassList("hd-team__chip");
            b.clicked += () => click(index);
            host.Add(b); list.Add(b);
        }

        static void Highlight(UnityEngine.UIElements.Button b, bool on)
        {
            b.EnableInClassList("hd-btn--primary", on);
            b.EnableInClassList("hd-btn--secondary", !on);
        }

        public void Show(TeamPageData d)
        {
            _last = d;
            if (_grid == null) return;
            for (int i = 0; i < _filters.Count; i++) Highlight(_filters[i], i == d.FilterIndex);
            for (int i = 0; i < _sorts.Count; i++) Highlight(_sorts[i], SortOrder[i] == d.Sort);

            _grid.Clear();
            _infos.Clear();
            foreach (var m in d.Members) _grid.Add(NewTile(m));
            if (_tileAnchor != null) Destroy(_tileAnchor.gameObject);
            _tileAnchor = _grid.childCount > 0 ? HdTutorialAnchor.Attach(this, _grid[0], TutorialTarget.TeamTile) : null;

            _squadTitle.text = d.SquadTitle;
            _squadRow.Clear();
            foreach (var s in d.Squad) _squadRow.Add(NewSquadSlot(s));
            _synergies.text = d.Synergies.Length == 0 ? Loc.T("team.synergies.none") : string.Join("\n", d.Synergies);
            for (int i = 0; i < _presets.Count; i++)
            {
                bool has = d.PresetLabels != null && i < d.PresetLabels.Length;
                _presets[i].style.display = has ? DisplayStyle.Flex : DisplayStyle.None;
                if (!has) continue;
                _presets[i].text = d.PresetLabels![i];
                bool saved = d.PresetSaved != null && i < d.PresetSaved.Length && d.PresetSaved[i];
                _presets[i].EnableInClassList("hd-btn--primary", d.SaveMode || saved);
                _presets[i].EnableInClassList("hd-btn--secondary", !(d.SaveMode || saved));
            }
            _presetSave.text = d.SaveLabel ?? "";
            Highlight(_presetSave, d.SaveMode);
            _healAll.text = d.HealAllLabel ?? "";
            _healAll.SetEnabled(d.HealAllEnabled);
            _healAll.tooltip = d.HealAllReason;
            _mission.SetEnabled(d.MissionEnabled);
            _mission.tooltip = d.MissionHint;
            _hint.text = d.MissionHint;
        }

        VisualElement NewTile(MemberTileData m)
        {
            var t = new Tile { Root = new UnityEngine.UIElements.Button() };
            t.Root.RemoveFromClassList("unity-button");
            t.Root.AddToClassList("hd-member");
            t.Art = new VisualElement { pickingMode = PickingMode.Ignore }; t.Art.AddToClassList("hd-member__art");
            t.Name = new Label(m.Card.Name) { pickingMode = PickingMode.Ignore }; t.Name.AddToClassList("hd-member__name");
            t.Level = new Label(m.Card.LevelText) { pickingMode = PickingMode.Ignore }; t.Level.AddToClassList("hd-member__sub");
            t.Rarity = new Label(m.Card.RarityText) { pickingMode = PickingMode.Ignore }; t.Rarity.AddToClassList("hd-member__sub");
            t.Rarity.style.color = Theme.RarityColor(m.Card.Rarity);
            t.Status = new Label(m.Status) { pickingMode = PickingMode.Ignore }; t.Status.AddToClassList("hd-member__status");
            t.Status.style.color = m.Exhausted ? Theme.Danger : Theme.Primary;
            var bar = new VisualElement { pickingMode = PickingMode.Ignore }; bar.AddToClassList("hd-bar"); bar.AddToClassList("hd-member__bar");
            t.FatigueFill = new VisualElement { pickingMode = PickingMode.Ignore }; t.FatigueFill.AddToClassList("hd-bar__fill");
            t.FatigueFill.style.width = Length.Percent(Mathf.Clamp01(m.Fatigue01) * 100f);
            t.FatigueFill.style.unityBackgroundImageTintColor = Theme.FatigueColor(m.Fatigue);
            t.FatigueLabel = new Label(m.FatigueText) { pickingMode = PickingMode.Ignore }; t.FatigueLabel.AddToClassList("hd-list__barlabel");
            bar.Add(t.FatigueFill); bar.Add(t.FatigueLabel);
            t.Squad = new UnityEngine.UIElements.Button { text = m.SquadLabel };
            t.Squad.AddToClassList("hd-btn"); t.Squad.AddToClassList("hd-btn--small");
            t.Squad.AddToClassList(m.InSquad ? "hd-btn--secondary" : "hd-btn--primary");
            var art = m.Card.Portrait.Art;
            if (art != null) t.Art.style.backgroundImage = new StyleBackground(art);
            t.Art.style.borderBottomColor = Theme.FamilyColor(m.Card.Family);
            t.Root.EnableInClassList("hd-member--squad", m.InSquad);
            t.Root.EnableInClassList("hd-member--exhausted", m.Exhausted);
            var id = m.Id;
            t.Squad.clicked += () => SquadToggled?.Invoke(id);
            void Tap() => MemberTapped?.Invoke(id);
            t.Root.RegisterCallback<ClickEvent>(e => { if (!(e.target is UnityEngine.UIElements.Button)) Tap(); });
            t.Root.RegisterCallback<NavigationSubmitEvent>(e => { if (e.target == t.Root) Tap(); });
            t.Root.Add(t.Art); t.Root.Add(t.Name); t.Root.Add(t.Level); t.Root.Add(t.Rarity); t.Root.Add(t.Status); t.Root.Add(bar); t.Root.Add(t.Squad);
            _infos.Add(new TileInfo { TapButton = new HdButtonRef((UnityEngine.UIElements.Button)t.Root, this), SquadButton = new HdButtonRef(t.Squad, this) });
            return t.Root;
        }

        static VisualElement NewSquadSlot(SquadSlotData s)
        {
            var slot = new VisualElement { pickingMode = PickingMode.Ignore };
            slot.AddToClassList("hd-squadslot");
            var art = new VisualElement { pickingMode = PickingMode.Ignore }; art.AddToClassList("hd-squadslot__art");
            if (s.Portrait.Art != null) art.style.backgroundImage = new StyleBackground(s.Portrait.Art);
            if (s.Exhausted) art.style.borderTopColor = art.style.borderBottomColor = art.style.borderLeftColor = art.style.borderRightColor = Theme.Danger;
            var name = new Label(s.Name) { pickingMode = PickingMode.Ignore }; name.AddToClassList("hd-squadslot__name");
            var bar = new VisualElement { pickingMode = PickingMode.Ignore }; bar.AddToClassList("hd-bar"); bar.AddToClassList("hd-squadslot__bar");
            var fill = new VisualElement { pickingMode = PickingMode.Ignore }; fill.AddToClassList("hd-bar__fill");
            fill.style.width = Length.Percent(Mathf.Clamp01(s.Fatigue01) * 100f);
            fill.style.unityBackgroundImageTintColor = Theme.FatigueColor(s.Fatigue);
            bar.Add(fill);
            slot.Add(art); slot.Add(name); slot.Add(bar);
            return slot;
        }
    }
}
