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
    /// <summary>
    /// Page Équipe (UI Toolkit) : filtres de famille et tri, grille de vignettes (portrait cadré sur le visage, famille, niveau),
    /// escouade et sa fatigue, synergies, compositions, « Préparer une mission ».
    /// </summary>
    public sealed class TeamPageView : HdView, ITeamPageView
    {
        static readonly MemberSort[] SortOrder = { MemberSort.Level, MemberSort.Rarity, MemberSort.Fatigue };

        /// <summary>Places affichées dans le panneau d'escouade (les places libres sont dessinées vides).</summary>
        const int SquadSlots = 4;

        /// <summary>Ce que les tests lisent d'une carte de la grille.</summary>
        public sealed class TileInfo
        {
            public HdButtonRef TapButton = null!;
            public HdButtonRef SquadButton = null!;
        }

        readonly List<TileInfo> _infos = new List<TileInfo>();
        readonly List<UnityEngine.UIElements.Button> _filters = new List<UnityEngine.UIElements.Button>();
        readonly List<UnityEngine.UIElements.Button> _sorts = new List<UnityEngine.UIElements.Button>();
        readonly List<UnityEngine.UIElements.Button> _presets = new List<UnityEngine.UIElements.Button>();
        ScrollView _grid = null!;
        VisualElement _squadRow = null!, _presetBox = null!;
        VisualElement? _squadFatigue, _squadFatigueFill;
        Label _squadTitle = null!, _synergies = null!, _synergiesTitle = null!, _hint = null!;
        Label? _squadFatigueLabel;
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
            _squadFatigue = TryQ<VisualElement>("squadFatigue"); _squadFatigueFill = TryQ<VisualElement>("squadFatigueFill"); _squadFatigueLabel = TryQ<Label>("squadFatigueLabel");
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
            {
                // Puce de famille : son icône du kit devant le nom.
                var chip = AddChip(filterBox, _filters, Loc.T("family." + f), index++, i => FilterClicked?.Invoke(i));
                // Icône seule (le nom de la famille reste en infobulle) : le nom écrasait l'icône dans une puce trop étroite.
                chip.text = "";
                chip.tooltip = Loc.T("family." + f);
                chip.AddToClassList("hd-team__chip--icon");
                var icon = HdTeamUi.FamilyIcon(f);
                icon.AddToClassList("hd-team__chipicon");
                chip.Add(icon);
            }
            var sortBox = Q<VisualElement>("sorts");
            sortBox.Clear();
            var sortIcon = HdTeamUi.Icon("sort");
            sortIcon.AddToClassList("hd-team__sortico");
            sortBox.Add(sortIcon);
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
            _presetSave.AddToClassList("hd-btn"); _presetSave.AddToClassList("hd-btn--small"); _presetSave.AddToClassList("hd-btn--plain"); _presetSave.AddToClassList("hd-team__chip");
            _presetSave.clicked += () => PresetSaveClicked?.Invoke();
            _presetBox.Add(_presetSave);
            if (_squadFatigue != null) _squadFatigue.style.display = DisplayStyle.None;
            _hint.style.display = DisplayStyle.None;
            HdTutorialAnchor.Attach(this, Q<VisualElement>("side"), TutorialTarget.TeamSquad);
            HdTutorialAnchor.Attach(this, _mission, TutorialTarget.TeamMission);
            _tileAnchor = null;
            if (_last.HasValue) Show(_last.Value);
        }

        static UnityEngine.UIElements.Button AddChip(VisualElement host, List<UnityEngine.UIElements.Button> list, string label, int index, Action<int> click)
        {
            var b = new UnityEngine.UIElements.Button { text = label };
            b.AddToClassList("hd-btn"); b.AddToClassList("hd-btn--small"); b.AddToClassList("hd-btn--plain"); b.AddToClassList("hd-team__chip");
            b.clicked += () => click(index);
            host.Add(b); list.Add(b);
            return b;
        }

        static void Highlight(UnityEngine.UIElements.Button b, bool on)
        {
            b.EnableInClassList("hd-btn--primary", on);
            b.EnableInClassList("hd-btn--secondary", !on);
        }

        static VisualElement Passive(string className)
        {
            var e = new VisualElement { pickingMode = PickingMode.Ignore };
            e.AddToClassList(className);
            return e;
        }

        static Label PassiveLabel(string text, string className)
        {
            var l = new Label(text) { pickingMode = PickingMode.Ignore };
            l.AddToClassList(className);
            return l;
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
            // Fatigue de l'escouade : celle de l'agent le plus fatigué (c'est lui qui décide du soin).
            int worst = 0, count = 0;
            foreach (var s in d.Squad)
            {
                _squadRow.Add(NewSquadSlot(s));
                worst = Mathf.Max(worst, s.Fatigue);
                count++;
            }
            for (int i = count; i < SquadSlots; i++) _squadRow.Add(NewEmptySlot());
            if (_squadFatigue != null)
            {
                _squadFatigue.style.display = count > 0 ? DisplayStyle.Flex : DisplayStyle.None;
                if (_squadFatigueFill != null)
                {
                    _squadFatigueFill.style.width = Length.Percent(Mathf.Clamp(worst, 0, 100));
                    _squadFatigueFill.style.unityBackgroundImageTintColor = Theme.FatigueColor(worst);
                }
                if (_squadFatigueLabel != null) _squadFatigueLabel.text = Loc.F("ui.fatigue", worst);
            }
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
            _hint.style.display = string.IsNullOrEmpty(d.MissionHint) ? DisplayStyle.None : DisplayStyle.Flex;
        }

        VisualElement NewTile(MemberTileData m)
        {
            var root = new UnityEngine.UIElements.Button();
            root.RemoveFromClassList("unity-button");
            root.AddToClassList("hd-member");
            HdTeamUi.SetFamily(root, m.Card.Family, "hd-member--");
            root.EnableInClassList("hd-member--squad", m.InSquad);
            root.EnableInClassList("hd-member--exhausted", m.Exhausted);

            // Vignette carrée : portrait cadré sur le visage, cadre de famille, icône de famille, bandeau de niveau.
            var portrait = Passive("hd-member__portrait");
            var art = Passive("hd-member__art");
            HdTeamUi.Head(art, m.Id.Value, m.Card.Portrait.Art);
            var family = HdTeamUi.FamilyIcon(m.Card.Family);
            family.AddToClassList("hd-member__family");
            portrait.Add(art);
            portrait.Add(Passive("hd-member__frame"));
            portrait.Add(family);
            // Position (sous la famille, teinte de la famille) et insigne de rang AMIF (coin haut droit).
            var role = HdTeamUi.PositionIcon(m.Role, m.Card.Family);
            role.AddToClassList("hd-member__role");
            portrait.Add(role);
            var rank = HdTeamUi.RankBadge(m.Card.PowerTier);
            rank.AddToClassList("hd-member__rank");
            portrait.Add(rank);
            portrait.Add(PassiveLabel(m.Card.LevelText, "hd-member__level"));

            // Pied : nom, état écrit (ou fatigue chiffrée), jauge de fatigue.
            var text = Passive("hd-member__foot");
            text.Add(PassiveLabel(m.Card.Name, "hd-member__name"));
            bool hasStatus = !string.IsNullOrEmpty(m.Status);
            var status = PassiveLabel(hasStatus ? m.Status : m.FatigueText, "hd-member__status");
            status.EnableInClassList("hd-member__status--alert", m.Exhausted);
            status.EnableInClassList("hd-member__status--plain", !hasStatus);
            text.Add(status);
            var bar = Passive("hd-member__bar");
            var fill = Passive("hd-member__barfill");
            fill.style.width = Length.Percent(Mathf.Clamp01(m.Fatigue01) * 100f);
            fill.style.backgroundColor = Theme.FatigueColor(m.Fatigue);
            bar.Add(fill);
            text.Add(bar);

            var squad = new UnityEngine.UIElements.Button();
            squad.RemoveFromClassList("unity-button");
            squad.AddToClassList("hd-member__toggle");
            squad.EnableInClassList("hd-member__toggle--add", !m.InSquad);
            squad.tooltip = m.SquadLabel;
            squad.Add(HdTeamUi.Icon(m.InSquad ? "minus" : "plus"));
            // Bouton d'escouade (plus ou moins) dans le coin bas droit du portrait, à côté du bandeau de niveau.
            portrait.Add(squad);

            var id = m.Id;
            squad.clicked += () => SquadToggled?.Invoke(id);
            void Tap() => MemberTapped?.Invoke(id);
            // Un clic sur la vignette ouvre la fiche ; un clic sur le bouton d'escouade (autre bouton, imbriqué) ne l'ouvre pas.
            root.RegisterCallback<ClickEvent>(e => { if (e.target == root || !(e.target is UnityEngine.UIElements.Button)) Tap(); });
            root.RegisterCallback<NavigationSubmitEvent>(e => { if (e.target == root) Tap(); });
            root.Add(portrait); root.Add(text);
            _infos.Add(new TileInfo { TapButton = new HdButtonRef(root, this), SquadButton = new HdButtonRef(squad, this) });
            return root;
        }

        static VisualElement NewSquadSlot(SquadSlotData s)
        {
            var slot = Passive("hd-squadslot");
            HdTeamUi.SetFamily(slot, s.Portrait.Family, "hd-squadslot--");
            slot.EnableInClassList("hd-squadslot--exhausted", s.Exhausted);
            var portrait = Passive("hd-squadslot__portrait");
            var art = Passive("hd-squadslot__face");
            HdTeamUi.Head(art, s.Portrait.Art);
            portrait.Add(art);
            if (s.Portrait.Art == null) portrait.Add(PassiveLabel(s.Portrait.Initial, "hd-squadslot__initial"));
            portrait.Add(Passive("hd-squadslot__frame"));
            var family = HdTeamUi.FamilyIcon(s.Portrait.Family);
            family.AddToClassList("hd-squadslot__family");
            portrait.Add(family);
            var bar = Passive("hd-squadslot__gauge");
            var fill = Passive("hd-squadslot__gaugefill");
            fill.style.width = Length.Percent(Mathf.Clamp01(s.Fatigue01) * 100f);
            fill.style.backgroundColor = Theme.FatigueColor(s.Fatigue);
            bar.Add(fill);
            slot.Add(portrait); slot.Add(PassiveLabel(s.Name, "hd-squadslot__name")); slot.Add(bar);
            return slot;
        }

        /// <summary>Place libre de l'escouade : cadre neutre et signe « plus ».</summary>
        static VisualElement NewEmptySlot()
        {
            var slot = Passive("hd-squadslot");
            slot.AddToClassList("hd-squadslot--empty");
            var portrait = Passive("hd-squadslot__portrait");
            portrait.Add(Passive("hd-squadslot__frame"));
            var plus = HdTeamUi.Icon("plus");
            plus.AddToClassList("hd-squadslot__plus");
            portrait.Add(plus);
            slot.Add(portrait);
            return slot;
        }
    }
}
