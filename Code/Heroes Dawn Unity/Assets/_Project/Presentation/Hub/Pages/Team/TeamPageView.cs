#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Page Équipe (prefab Pages/Team) : filtres et tri, grille de cartes, escouade, combos, « Préparer une mission ».</summary>
    public sealed class TeamPageView : ViewBase, ITeamPageView
    {
        static readonly MemberSort[] SortOrder = { MemberSort.Level, MemberSort.Rarity, MemberSort.Fatigue };

        [SerializeField] UiButton[] filters = Array.Empty<UiButton>();
        [SerializeField] UiButton[] sorts = Array.Empty<UiButton>();
        [SerializeField] RectTransform grid = null!;
        [SerializeField] TMP_Text squadTitle = null!;
        [SerializeField] RectTransform squadRow = null!;
        [SerializeField] TMP_Text combos = null!;
        [SerializeField] GameObject combosNone = null!;
        [SerializeField] UiButton mission = null!;
        [SerializeField] TMP_Text missionHint = null!;
        [SerializeField] MemberTileView tilePrefab = null!;
        [SerializeField] SquadSlotView squadSlotPrefab = null!;

        readonly List<MemberTileView> _tiles = new List<MemberTileView>();

        public event Action<int>? FilterClicked;
        public event Action<MemberSort>? SortClicked;
        public event Action<MemberId>? MemberTapped;
        public event Action<MemberId>? SquadToggled;
        public event Action? MissionClicked;

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
        }

        public void Show(TeamPageData d)
        {
            for (int i = 0; i < filters.Length; i++) Highlight(filters[i], i == d.FilterIndex);
            for (int i = 0; i < sorts.Length && i < SortOrder.Length; i++) Highlight(sorts[i], SortOrder[i] == d.Sort);

            grid.Clear();
            _tiles.Clear();
            foreach (var m in d.Members)
            {
                var tile = Instantiate(tilePrefab, grid);
                var id = m.Id;
                tile.Tapped += () => MemberTapped?.Invoke(id);
                tile.SquadClicked += () => SquadToggled?.Invoke(id);
                tile.Show(m);
                _tiles.Add(tile);
            }

            squadTitle.text = d.SquadTitle;
            squadRow.Clear();
            foreach (var s in d.Squad) Instantiate(squadSlotPrefab, squadRow).Show(s);
            combos.text = string.Join("\n", d.Combos);
            bool none = d.Combos.Length == 0;
            if (combosNone.activeSelf != none) combosNone.SetActive(none);
            mission.Interactable = d.MissionEnabled;
            missionHint.text = d.MissionHint;
        }

        static void Highlight(UiButton b, bool on)
            => b.SetColors(on ? Theme.Primary : Theme.Surface3, on ? Theme.TextDark : Theme.Text);
    }
}
