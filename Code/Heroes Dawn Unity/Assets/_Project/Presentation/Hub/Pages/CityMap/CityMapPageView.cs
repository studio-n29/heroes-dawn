#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Application.Features.World;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Tutorial;
using N29.HeroesDawn.Presentation.UiToolkit;
using UnityEngine;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>
    /// Page Carte de la ville (UI Toolkit) : à gauche les quartiers (polygones), leurs points de mission ;
    /// à droite l'acte, la liste des zones, la fiche de la zone choisie et DÉPLOYER.
    /// </summary>
    public sealed class CityMapPageView : HdView, ICityMapPageView
    {
        sealed class Point
        {
            public UnityEngine.UIElements.Button Root = null!;
            public Label Label = null!, Sub = null!;
            public string ZoneId = "";
        }

        readonly Dictionary<string, HdPolygon> _polygons = new Dictionary<string, HdPolygon>();
        readonly Dictionary<string, Label> _tags = new Dictionary<string, Label>();
        readonly Dictionary<string, bool> _lockedBoroughs = new Dictionary<string, bool>();
        readonly Dictionary<string, string> _tagText = new Dictionary<string, string>();
        readonly List<Point> _points = new List<Point>();
        readonly List<UnityEngine.UIElements.Button> _rows = new List<UnityEngine.UIElements.Button>();
        readonly List<string> _rowIds = new List<string>();
        VisualElement _mapArea = null!, _rowBox = null!, _objectives = null!;
        Label _actTitle = null!, _detailTitle = null!, _description = null!, _chips = null!, _rewards = null!;
        UnityEngine.UIElements.Button _deploy = null!, _chapters = null!;
        CityMapPageData? _last;
        bool _built;

        public event Action<string>? ZoneSelected;
        public event Action? DeployClicked;
        public event Action<string>? LockedTapped;
        public event Action? ChaptersClicked;

        /// <summary>Contrôles pour les tests PlayMode (clics réels).</summary>
        public HdButtonRef DeployButton => new HdButtonRef(_deploy, this);
        public HdButtonRef ChaptersButton => new HdButtonRef(_chapters, this);
        public HdButtonRef? RowButton(int i) => i >= 0 && i < _rows.Count ? new HdButtonRef(_rows[i], this) : null;
        public HdButtonRef? PointButton(string zoneId)
        {
            foreach (var p in _points) if (p.ZoneId == zoneId) return new HdButtonRef(p.Root, this);
            return null;
        }

        protected override void Bind()
        {
            _mapArea = Q<VisualElement>("mapArea"); _rowBox = Q<VisualElement>("rows"); _objectives = Q<VisualElement>("objectives");
            _actTitle = Q<Label>("actTitle"); _detailTitle = Q<Label>("detailTitle"); _description = Q<Label>("description");
            _chips = Q<Label>("chips"); _rewards = Q<Label>("rewards");
            _deploy = Q<UnityEngine.UIElements.Button>("deploy"); _chapters = Q<UnityEngine.UIElements.Button>("chapters");
            _chapters.text = Loc.T("chapters.button");
            _deploy.clicked += () => DeployClicked?.Invoke();
            HdTutorialAnchor.Attach(this, _deploy, TutorialTarget.MapDeploy);
            _chapters.clicked += () => ChaptersClicked?.Invoke();
            _polygons.Clear(); _tags.Clear(); _points.Clear(); _rows.Clear(); _rowIds.Clear();
            _built = false;
            if (_last.HasValue) Show(_last.Value);
        }

        public void Show(CityMapPageData d)
        {
            _last = d;
            if (_mapArea == null) return;
            if (!_built) Build(d);
            _actTitle.text = d.ActTitle;
            foreach (var b in d.Boroughs) Paint(b);
            for (int i = 0; i < _points.Count && i < d.Points.Length; i++) ShowPoint(_points[i], d.Points[i]);
            while (_rows.Count < d.Rows.Length)
            {
                int index = _rows.Count;
                var b = new UnityEngine.UIElements.Button();
                b.AddToClassList("hd-btn"); b.AddToClassList("hd-btn--small"); b.AddToClassList("hd-map__row");
                b.clicked += () => { if (index < _rowIds.Count && !string.IsNullOrEmpty(_rowIds[index])) ZoneSelected?.Invoke(_rowIds[index]); };
                _rows.Add(b); _rowIds.Add(""); _rowBox.Add(b);
            }
            for (int i = 0; i < _rows.Count; i++)
            {
                bool used = i < d.Rows.Length;
                _rows[i].style.display = used ? DisplayStyle.Flex : DisplayStyle.None;
                if (!used) { _rowIds[i] = ""; continue; }
                var r = d.Rows[i];
                _rowIds[i] = r.ZoneId;
                _rows[i].text = r.Title + "  ·  " + r.StatusText;
                _rows[i].EnableInClassList("hd-btn--primary", r.Selected);
                _rows[i].EnableInClassList("hd-btn--secondary", !r.Selected);
                _rows[i].SetEnabled(r.Status != ZoneStatus.Locked || r.Selected);
            }
            ShowDetail(d.Detail);
        }

        void ShowDetail(ZoneDetailData z)
        {
            _detailTitle.text = z.Title; _description.text = z.Description; _chips.text = z.Chips;
            _objectives.Clear();
            for (int i = 0; i < z.Objectives.Length; i++)
            {
                bool done = z.ObjectivesDone[i];
                var line = new Label((done ? "[X] " : "[  ] ") + z.Objectives[i]);
                line.AddToClassList("hd-pop__text");
                line.style.color = done ? Theme.Success : Theme.Text;
                _objectives.Add(line);
            }
            _rewards.text = z.Rewards;
            _deploy.text = z.ButtonLabel;
            _deploy.SetEnabled(z.ButtonEnabled);
            _deploy.tooltip = z.Reason;
        }

        void Build(CityMapPageData d)
        {
            _mapArea.Clear();
            // Quartiers d'abord (dessous), puis les points (dessus).
            foreach (var b in d.Boroughs)
            {
                var poly = new HdPolygon();
                poly.style.position = Position.Absolute;
                poly.style.left = poly.style.right = poly.style.top = poly.style.bottom = 0;
                var pts = new Vector2[b.Polygon.Length];
                for (int i = 0; i < pts.Length; i++) pts[i] = new Vector2(b.Polygon[i].x, 1f - b.Polygon[i].y);
                poly.SetPoints(pts);
                var id = b.Id;
                poly.RegisterCallback<ClickEvent>(_ => { if (IsLocked(id)) LockedTapped?.Invoke(TagOf(id)); });
                _polygons[b.Id] = poly;
                _mapArea.Add(poly);
                var tag = new Label { pickingMode = PickingMode.Ignore };
                tag.AddToClassList("hd-map__tag");
                tag.style.left = Length.Percent(b.LabelAt.x * 100f);
                tag.style.top = Length.Percent(b.LabelAt.y * 100f);
                _tags[b.Id] = tag;
                _mapArea.Add(tag);
            }
            foreach (var p in d.Points)
            {
                var pt = new Point { Root = new UnityEngine.UIElements.Button() };
                pt.Root.RemoveFromClassList("unity-button");
                pt.Root.AddToClassList("hd-map__point");
                pt.Root.style.left = Length.Percent(p.Position.x * 100f);
                pt.Root.style.top = Length.Percent((1f - p.Position.y) * 100f);
                pt.Label = new Label { pickingMode = PickingMode.Ignore }; pt.Label.AddToClassList("hd-map__plabel");
                pt.Sub = new Label { pickingMode = PickingMode.Ignore }; pt.Sub.AddToClassList("hd-map__psub");
                pt.Root.Add(pt.Label); pt.Root.Add(pt.Sub);
                var point = pt;
                string label = p.Label;
                pt.Root.clicked += () =>
                {
                    if (!string.IsNullOrEmpty(point.ZoneId)) ZoneSelected?.Invoke(point.ZoneId);
                    else LockedTapped?.Invoke(label);
                };
                _points.Add(pt);
                _mapArea.Add(pt.Root);
            }
            _built = true;
        }

        static void ShowPoint(Point p, ZonePointData d)
        {
            p.ZoneId = d.ZoneId ?? "";
            p.Label.text = d.Label; p.Sub.text = d.Sub;
            bool locked = d.Status == ZoneStatus.Locked, done = d.Status == ZoneStatus.Done;
            p.Root.EnableInClassList("hd-map__point--locked", locked);
            p.Root.EnableInClassList("hd-map__point--done", done);
            p.Root.EnableInClassList("hd-map__point--selected", d.Selected);
        }

        bool IsLocked(string id) => _lockedBoroughs.TryGetValue(id, out var l) && l;
        string TagOf(string id) => _tagText.TryGetValue(id, out var t) ? t : id;

        void Paint(BoroughData b)
        {
            var poly = _polygons[b.Id];
            _lockedBoroughs[b.Id] = b.Locked;
            _tagText[b.Id] = b.Tag;
            poly.Fill = b.Locked ? Theme.WithAlpha(Theme.Surface3, 0.85f) : b.Highlight ? Theme.WithAlpha(Theme.Glow, 0.9f) : Theme.WithAlpha(Color.Lerp(Theme.SkyTop, Theme.Surface2, 0.5f), 0.95f);
            poly.Outline = b.Locked ? Theme.WithAlpha(Theme.Navy, 0.5f) : Theme.Primary;
            poly.OutlineWidth = b.Locked ? 4f : 7f;
            poly.Repaint();
            var tag = _tags[b.Id];
            tag.text = b.Name + " · " + b.Tag;
            tag.style.color = b.Locked ? Theme.TextMuted : Theme.Text;
        }
    }
}
