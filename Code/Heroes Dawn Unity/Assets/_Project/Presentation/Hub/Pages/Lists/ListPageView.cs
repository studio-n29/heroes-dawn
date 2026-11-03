#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Feedback;
using N29.HeroesDawn.Presentation.Mvp;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>
    /// Base des pages « liste » : en-tête (titre, ligne, barre, 2 boutons), liste de lignes défilante, colonne de droite optionnelle.
    /// Les lignes sont recyclées ; chaque page concrète est un prefab distinct (sous-classe vide) pour être retrouvée par type.
    /// </summary>
    public abstract class ListPageView : ViewBase, IListPageView, IFilterBarView
    {
        const float FilterRowH = 56f, FilterGap = 8f, FilterButtonW = 190f;
        [SerializeField] TMP_Text headerTitle = null!;
        [SerializeField] TMP_Text headerLine = null!;
        [SerializeField] UiBar headerBar = null!;
        [SerializeField, OptionalRef] Image? headerPanel;
        [SerializeField] UiButton headerA = null!;
        [SerializeField] UiButton headerB = null!;
        [SerializeField] RectTransform rows = null!;
        [SerializeField] InfoRowView rowPrefab = null!;
        [SerializeField, OptionalRef] GameObject? sideRoot;
        [SerializeField, OptionalRef] TMP_Text? sideTitle;
        [SerializeField, OptionalRef] RectTransform? sideRows;
        [SerializeField, OptionalRef] TMP_Text? sideText;
        /// <summary>Barre de filtres sous l'en-tête (masquée sans groupe) et zone de la liste qui descend sous elle.</summary>
        [SerializeField, OptionalRef] RectTransform? filterBar;
        [SerializeField, OptionalRef] RectTransform? listArea;

        readonly List<List<UiButton>> _filterButtons = new List<List<UiButton>>();
        string _filterShape = "";
        float _listTop;

        public event Action<int, int>? FilterClicked;

        readonly List<InfoRowView> _main = new List<InfoRowView>();
        readonly List<InfoRowView> _side = new List<InfoRowView>();
        bool _cascaded;

        public event Action<int>? HeaderClicked;
        public event Action<int, int>? RowButtonClicked;
        public event Action<int>? RowSelected;
        public event Action<int, int>? SideButtonClicked;
        public event Action<int>? SideRowSelected;

        public UiButton HeaderButtonA => headerA;
        public UiButton HeaderButtonB => headerB;
        public IReadOnlyList<InfoRowView> MainRows => _main;
        public IReadOnlyList<InfoRowView> SideRows => _side;

        protected virtual void Awake()
        {
            headerA.Button.onClick.AddListener(() => HeaderClicked?.Invoke(0));
            headerB.Button.onClick.AddListener(() => HeaderClicked?.Invoke(1));
        }

        public void ShowHeader(ListHeaderData d)
        {
            headerTitle.text = d.Title;
            headerLine.text = d.Line;
            bool bar = d.Progress >= 0f;
            headerBar.gameObject.SetActive(bar);
            if (bar) { headerBar.Set(d.Progress); headerBar.SetLabel(d.ProgressLabel); }
            if (headerPanel != null && d.HeaderTint.HasValue) headerPanel.color = Theme.WithAlpha(d.HeaderTint.Value, 0.9f);
            Apply(headerA, d.A);
            Apply(headerB, d.B);
        }

        static void Apply(UiButton b, RowButtonData d)
        {
            if (b.gameObject.activeSelf != d.Visible) b.gameObject.SetActive(d.Visible);
            if (!d.Visible) return;
            b.SetLabel(d.Label);
            var bg = d.Style == 1 ? Theme.Pr : d.Style == 2 ? Theme.Danger : d.Style == 3 ? Theme.Surface3 : Theme.Primary;
            b.SetColors(bg, d.Style == 2 || d.Style == 3 ? Theme.Text : Theme.TextDark);
            b.Interactable = d.Enabled;
            b.DisabledReason = d.Reason;
        }

        /// <summary>Un rang de boutons par groupe (l'option choisie en jaune) ; la liste descend sous la barre, masquée sans groupe.</summary>
        public void ShowFilters(FilterGroupData[] groups)
        {
            if (filterBar == null || listArea == null) return;
            if (_listTop == 0f) _listTop = listArea.offsetMax.y;
            bool any = groups.Length > 0;
            filterBar.gameObject.SetActive(any);
            float h = any ? groups.Length * (FilterRowH + FilterGap) : 0f;
            filterBar.sizeDelta = new Vector2(filterBar.sizeDelta.x, h);
            listArea.offsetMax = new Vector2(listArea.offsetMax.x, _listTop - h);
            string shape = string.Join("|", groups.Select(g => string.Join(",", g.Labels)));
            if (shape != _filterShape) BuildFilters(groups);
            _filterShape = shape;
            for (int g = 0; g < groups.Length && g < _filterButtons.Count; g++)
                for (int o = 0; o < _filterButtons[g].Count; o++)
                    _filterButtons[g][o].SetColors(o == groups[g].Selected ? Theme.Primary : Theme.Surface3, o == groups[g].Selected ? Theme.TextDark : Theme.Text);
        }

        void BuildFilters(FilterGroupData[] groups)
        {
            for (int i = filterBar!.childCount - 1; i >= 0; i--) Destroy(filterBar.GetChild(i).gameObject);
            _filterButtons.Clear();
            for (int g = 0; g < groups.Length; g++)
            {
                var row = new GameObject("Filters" + g, typeof(RectTransform)).GetComponent<RectTransform>();
                row.SetParent(filterBar, false);
                row.anchorMin = new Vector2(0, 1); row.anchorMax = new Vector2(1, 1); row.pivot = new Vector2(0.5f, 1);
                row.sizeDelta = new Vector2(0, FilterRowH);
                row.anchoredPosition = new Vector2(0, -g * (FilterRowH + FilterGap));
                var buttons = new List<UiButton>();
                for (int o = 0; o < groups[g].Labels.Length; o++)
                {
                    var b = Instantiate(headerA, row);
                    b.gameObject.name = "Filter" + o;
                    b.gameObject.SetActive(true);
                    var rt = b.Rect;
                    rt.anchorMin = rt.anchorMax = new Vector2(0, 0.5f);
                    rt.pivot = new Vector2(0, 0.5f);
                    rt.sizeDelta = new Vector2(FilterButtonW, FilterRowH);
                    rt.anchoredPosition = new Vector2(4 + o * (FilterButtonW + 8), 0);
                    b.SetLabel(groups[g].Labels[o]);
                    b.Interactable = true;
                    b.DisabledReason = "";
                    b.Button.onClick.RemoveAllListeners();
                    int group = g, option = o;
                    b.Button.onClick.AddListener(() => FilterClicked?.Invoke(group, option));
                    buttons.Add(b);
                }
                _filterButtons.Add(buttons);
            }
        }

        /// <summary>Sélection d'une rangée, comme un toucher (utilisé par les pages qui la simulent en test).</summary>
        protected void RaiseRowSelected(int row) => RowSelected?.Invoke(row);

        public void ShowRows(InfoRowData[] data)
        {
            Fill(_main, rows, data, (r, b) => RowButtonClicked?.Invoke(r, b), r => RowSelected?.Invoke(r), !_cascaded);
            _cascaded = true;
        }

        public void ShowSide(ListSideData d)
        {
            if (sideRoot == null) return;
            if (sideTitle != null) sideTitle.text = d.Title;
            if (sideText != null) sideText.text = d.Text ?? "";
            if (sideRows != null) Fill(_side, sideRows, d.Rows ?? Array.Empty<InfoRowData>(), (r, b) => SideButtonClicked?.Invoke(r, b), r => SideRowSelected?.Invoke(r), false);
        }

        void Fill(List<InfoRowView> pool, RectTransform parent, InfoRowData[] data, Action<int, int> onButton, Action<int> onSelect, bool animate)
        {
            while (pool.Count < data.Length)
            {
                var row = Instantiate(rowPrefab, parent);
                int index = pool.Count;
                row.ButtonClicked += b => onButton(index, b);
                row.Selected += () => onSelect(index);
                pool.Add(row);
                if (animate && UnityEngine.Application.isPlaying) Fx.PopInAfter(row.transform, Stagger.DelayFor(index));
            }
            for (int i = 0; i < pool.Count; i++)
            {
                bool used = i < data.Length;
                if (pool[i].gameObject.activeSelf != used) pool[i].gameObject.SetActive(used);
                if (used) pool[i].Show(data[i]);
            }
        }
    }
}
