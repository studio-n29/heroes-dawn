#nullable enable
using System;
using System.Collections.Generic;
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
    public abstract class ListPageView : ViewBase, IListPageView
    {
        [SerializeField] TMP_Text headerTitle = null!;
        [SerializeField] TMP_Text headerLine = null!;
        [SerializeField] UiBar headerBar = null!;
        [SerializeField] UiButton headerA = null!;
        [SerializeField] UiButton headerB = null!;
        [SerializeField] RectTransform rows = null!;
        [SerializeField] InfoRowView rowPrefab = null!;
        [SerializeField, OptionalRef] GameObject? sideRoot;
        [SerializeField, OptionalRef] TMP_Text? sideTitle;
        [SerializeField, OptionalRef] RectTransform? sideRows;
        [SerializeField, OptionalRef] TMP_Text? sideText;

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
