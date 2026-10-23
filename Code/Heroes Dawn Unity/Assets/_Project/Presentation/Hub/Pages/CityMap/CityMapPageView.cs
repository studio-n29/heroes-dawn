#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Application.Features.World;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Feedback;
using N29.HeroesDawn.Presentation.Mvp;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>
    /// Page Carte de la ville (prefab Pages/CityMap) : à gauche les quartiers (polygones), leurs points de mission ;
    /// à droite l'acte, la liste des zones, la fiche de la zone choisie et DÉPLOYER.
    /// </summary>
    public sealed class CityMapPageView : ViewBase, ICityMapPageView
    {
        [SerializeField] RectTransform mapArea = null!;
        [SerializeField] TMP_Text tagTemplate = null!;
        [SerializeField] ZonePointView pointPrefab = null!;
        [SerializeField, OptionalRef] Material? lockedMaterial;
        [SerializeField] TMP_Text actTitle = null!;
        [SerializeField] ZoneRowView[] rows = Array.Empty<ZoneRowView>();
        [SerializeField] TMP_Text detailTitle = null!;
        [SerializeField] TMP_Text description = null!;
        [SerializeField] TMP_Text chips = null!;
        [SerializeField] RectTransform objectives = null!;
        [SerializeField] TextLineView objectiveLinePrefab = null!;
        [SerializeField] TMP_Text rewards = null!;
        [SerializeField] UiButton deploy = null!;

        readonly Dictionary<string, UiPolygon> _polygons = new Dictionary<string, UiPolygon>();
        readonly Dictionary<string, TMP_Text> _tags = new Dictionary<string, TMP_Text>();
        readonly List<ZonePointView> _points = new List<ZonePointView>();
        readonly string[] _rowIds = new string[8];
        readonly List<string> _pointIds = new List<string>();
        bool _built;
        bool _cascaded;

        public event Action<string>? ZoneSelected;
        public event Action? DeployClicked;
        public event Action<string>? LockedTapped;

        /// <summary>Contrôles pour les tests PlayMode (clics réels).</summary>
        public Button DeployButton => deploy.Button;
        public Button? RowButton(int i) => i >= 0 && i < rows.Length ? rows[i].Button : null;
        public Button? PointButton(string zoneId)
        {
            int i = _pointIds.IndexOf(zoneId);
            return i >= 0 ? _points[i].Button : null;
        }

        void Awake()
        {
            deploy.Button.onClick.AddListener(() => DeployClicked?.Invoke());
            for (int i = 0; i < rows.Length; i++)
            {
                int index = i;
                rows[i].Clicked += () => { if (!string.IsNullOrEmpty(_rowIds[index])) ZoneSelected?.Invoke(_rowIds[index]); };
            }
        }

        public void Show(CityMapPageData d)
        {
            if (!_built) Build(d);
            actTitle.text = d.ActTitle;
            foreach (var b in d.Boroughs) Paint(b);
            for (int i = 0; i < _points.Count; i++)
            {
                _points[i].Show(d.Points[i]);
                _pointIds[i] = d.Points[i].ZoneId ?? "";
                if (!_cascaded) Fx.PopInAfter(_points[i].transform, 0.15f + Stagger.DelayFor(i));
            }
            for (int i = 0; i < rows.Length; i++)
            {
                bool used = i < d.Rows.Length;
                if (rows[i].gameObject.activeSelf != used) rows[i].gameObject.SetActive(used);
                if (!used) { _rowIds[i] = ""; continue; }
                _rowIds[i] = d.Rows[i].ZoneId;
                rows[i].Show(d.Rows[i]);
            }
            ShowDetail(d.Detail);
            _cascaded = true;
        }

        void ShowDetail(ZoneDetailData z)
        {
            detailTitle.text = z.Title;
            description.text = z.Description;
            chips.text = z.Chips;
            objectives.Clear();
            for (int i = 0; i < z.Objectives.Length; i++)
            {
                bool done = z.ObjectivesDone[i];
                Instantiate(objectiveLinePrefab, objectives).Show((done ? "[X] " : "[  ] ") + z.Objectives[i], done ? Theme.Success : Theme.Text);
            }
            rewards.text = z.Rewards;
            deploy.SetLabel(z.ButtonLabel);
            deploy.Interactable = z.ButtonEnabled;
            deploy.DisabledReason = z.Reason;
        }

        void Build(CityMapPageData d)
        {
            // Quartiers d'abord (dessous), puis les points (dessus).
            foreach (var b in d.Boroughs)
            {
                var go = new GameObject("Borough_" + b.Id, typeof(RectTransform), typeof(CanvasRenderer), typeof(UiPolygon)) { layer = 5 };
                var rt = (RectTransform)go.transform;
                rt.SetParent(mapArea, false);
                rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
                var poly = go.GetComponent<UiPolygon>();
                poly.raycastTarget = true;
                poly.SetPoints(b.Polygon);
                var id = b.Id;
                var click = go.AddComponent<Button>();
                click.targetGraphic = poly;
                click.transition = Selectable.Transition.None;
                click.onClick.AddListener(() => { if (_polygons.TryGetValue(id, out var p) && p.color.a > 0 && IsLocked(id)) LockedTapped?.Invoke(TagOf(id)); });
                _polygons[b.Id] = poly;

                var tag = Instantiate(tagTemplate, mapArea);
                tag.gameObject.SetActive(true);
                tag.name = "Tag_" + b.Id;
                var trt = tag.rectTransform;
                trt.anchorMin = trt.anchorMax = new Vector2(b.LabelAt.x, 1f - b.LabelAt.y);
                trt.pivot = new Vector2(0.5f, 0.5f);
                trt.anchoredPosition = Vector2.zero;
                trt.sizeDelta = new Vector2(420, 44);
                _tags[b.Id] = tag;
            }
            foreach (var p in d.Points)
            {
                var pv = Instantiate(pointPrefab, mapArea);
                var rt = (RectTransform)pv.transform;
                rt.anchorMin = rt.anchorMax = new Vector2(p.Position.x, 1f - p.Position.y);
                rt.anchoredPosition = Vector2.zero;
                var zoneId = p.ZoneId;
                var label = p.Label;
                pv.Clicked += () =>
                {
                    if (zoneId != null) ZoneSelected?.Invoke(zoneId);
                    else LockedTapped?.Invoke(label);
                };
                _points.Add(pv);
                _pointIds.Add("");
            }
            _built = true;
        }

        readonly Dictionary<string, bool> _lockedBoroughs = new Dictionary<string, bool>();
        readonly Dictionary<string, string> _tagText = new Dictionary<string, string>();
        bool IsLocked(string id) => _lockedBoroughs.TryGetValue(id, out var l) && l;
        string TagOf(string id) => _tagText.TryGetValue(id, out var t) ? t : id;

        void Paint(BoroughData b)
        {
            var poly = _polygons[b.Id];
            _lockedBoroughs[b.Id] = b.Locked;
            _tagText[b.Id] = b.Tag;
            poly.color = b.Locked ? Theme.WithAlpha(Theme.Surface3, 0.85f) : b.Highlight ? Theme.WithAlpha(Theme.Glow, 0.9f) : Theme.WithAlpha(Color.Lerp(Theme.SkyTop, Theme.Surface2, 0.5f), 0.95f);
            poly.OutlineColor = b.Locked ? Theme.WithAlpha(Theme.Navy, 0.5f) : Theme.Primary;
            poly.OutlineWidth = b.Locked ? 4f : 7f;
            poly.material = b.Locked && lockedMaterial != null ? lockedMaterial : null;
            var tag = _tags[b.Id];
            tag.text = b.Name + " · " + b.Tag;
            tag.color = b.Locked ? Theme.TextMuted : Theme.Text;
        }
    }
}
