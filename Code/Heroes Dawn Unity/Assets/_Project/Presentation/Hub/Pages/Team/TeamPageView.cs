#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Feedback;
using N29.HeroesDawn.Presentation.Mvp;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Page Équipe (prefab Pages/Team) : filtres et tri, grille de cartes, escouade, synergies, « Préparer une mission ».</summary>
    public sealed class TeamPageView : ViewBase, ITeamPageView
    {
        static readonly MemberSort[] SortOrder = { MemberSort.Level, MemberSort.Rarity, MemberSort.Fatigue };

        [SerializeField] UiButton[] filters = Array.Empty<UiButton>();
        [SerializeField] UiButton[] sorts = Array.Empty<UiButton>();
        [SerializeField] RectTransform grid = null!;
        [SerializeField] TMP_Text squadTitle = null!;
        [SerializeField] RectTransform squadRow = null!;
        [SerializeField] TMP_Text synergies = null!;
        [SerializeField] GameObject synergiesNone = null!;
        [SerializeField] UiButton mission = null!;
        [SerializeField] UiButton healAll = null!;
        [SerializeField] UiButton[] presets = Array.Empty<UiButton>();
        [SerializeField] UiButton presetSave = null!;
        [SerializeField] TMP_Text missionHint = null!;
        [SerializeField] MemberTileView tilePrefab = null!;
        [SerializeField] SquadSlotView squadSlotPrefab = null!;

        readonly List<MemberTileView> _tiles = new List<MemberTileView>();
        readonly Dictionary<MemberId, bool> _inSquad = new Dictionary<MemberId, bool>();
        bool _cascaded;

        public event Action<int>? FilterClicked;
        public event Action<MemberSort>? SortClicked;
        public event Action<MemberId>? MemberTapped;
        public event Action<MemberId>? SquadToggled;
        public event Action? MissionClicked;
        public event Action? HealAllClicked;
        public event Action<int>? PresetClicked;
        public event Action? PresetSaveClicked;

        public Button MissionButton => mission.Button;
        public IReadOnlyList<MemberTileView> Tiles => _tiles;

        void Awake()
        {
            for (int i = 0; i < filters.Length; i++)
            {
                int index = i;
                filters[i].Button.onClick.AddListener(() => FilterClicked?.Invoke(index));
            }
            for (int i = 0; i < sorts.Length && i < SortOrder.Length; i++)
            {
                var sort = SortOrder[i];
                sorts[i].Button.onClick.AddListener(() => SortClicked?.Invoke(sort));
            }
            mission.Button.onClick.AddListener(() => MissionClicked?.Invoke());
            healAll.Button.onClick.AddListener(() => HealAllClicked?.Invoke());
            for (int i = 0; i < presets.Length; i++) { int k = i; presets[i].Button.onClick.AddListener(() => PresetClicked?.Invoke(k)); }
            presetSave.Button.onClick.AddListener(() => PresetSaveClicked?.Invoke());
        }

        public void Show(TeamPageData d)
        {
            for (int i = 0; i < filters.Length; i++) Highlight(filters[i], i == d.FilterIndex);
            for (int i = 0; i < sorts.Length && i < SortOrder.Length; i++) Highlight(sorts[i], SortOrder[i] == d.Sort);

            grid.Clear();
            _tiles.Clear();
            int index = 0;
            foreach (var m in d.Members)
            {
                var tile = Instantiate(tilePrefab, grid);
                var id = m.Id;
                tile.Tapped += () => MemberTapped?.Invoke(id);
                tile.SquadClicked += () => SquadToggled?.Invoke(id);
                tile.Show(m);
                _tiles.Add(tile);
                // Feedbacks : cascade à la première ouverture, rebond d'une carte qui entre ou sort de l'escouade.
                if (!_cascaded) Fx.PopInAfter(tile.transform, Stagger.DelayFor(index));
                else if (_inSquad.TryGetValue(id, out var was) && was != m.InSquad) Fx.Burst(tile.transform);
                _inSquad[id] = m.InSquad;
                index++;
            }
            _cascaded = true;

            squadTitle.text = d.SquadTitle;
            squadRow.Clear();
            foreach (var s in d.Squad) Instantiate(squadSlotPrefab, squadRow).Show(s);
            synergies.text = string.Join(System.Environment.NewLine, d.Synergies);
            bool none = d.Synergies.Length == 0;
            if (synergiesNone.activeSelf != none) synergiesNone.SetActive(none);
            for (int i = 0; i < presets.Length; i++)
            {
                bool has = d.PresetLabels != null && i < d.PresetLabels.Length;
                presets[i].gameObject.SetActive(has);
                if (!has) continue;
                presets[i].SetLabel(d.PresetLabels![i]);
                bool saved = d.PresetSaved != null && i < d.PresetSaved.Length && d.PresetSaved[i];
                presets[i].SetColors(d.SaveMode ? Theme.Accent : saved ? Theme.Info : Theme.Surface3, d.SaveMode ? Theme.TextOnDark : saved ? Theme.TextDark : Theme.Text);
            }
            presetSave.SetLabel(d.SaveLabel ?? "");
            presetSave.SetColors(d.SaveMode ? Theme.Accent : Theme.Surface3, d.SaveMode ? Theme.TextOnDark : Theme.Text);
            healAll.SetLabel(d.HealAllLabel ?? "");
            healAll.Interactable = d.HealAllEnabled;
            healAll.DisabledReason = d.HealAllReason;
            mission.Interactable = d.MissionEnabled;
            mission.DisabledReason = d.MissionHint;
            missionHint.text = d.MissionHint;
        }

        static void Highlight(UiButton b, bool on)
            => b.SetColors(on ? Theme.Primary : Theme.Surface3, on ? Theme.TextDark : Theme.Text);
    }
}
