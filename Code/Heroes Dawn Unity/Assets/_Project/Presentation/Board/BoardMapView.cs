#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Domain.Maps;
using N29.HeroesDawn.Presentation.Combat.Hud;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Feedback;
using N29.HeroesDawn.Presentation.Tutorial;
using N29.HeroesDawn.Presentation.UiToolkit;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Board
{
    /// <summary>Tracé des chemins du plateau (UI Toolkit) : une polyligne par lien, avec contour, couleur et épaisseur par lien.</summary>
    public sealed class BoardLinksElement : VisualElement
    {
        /// <summary>Boîte de dessin centrée sur l'origine de la carte (en pixels).</summary>
        public const float HalfWidth = 5000f, HalfHeight = 4000f;

        public sealed class Line
        {
            public Vector2[] Points = Array.Empty<Vector2>();
            public Color Color;
            public float Thickness = 12f;
            public bool Visible = true;
        }

        public readonly List<Line> Lines = new List<Line>();

        public BoardLinksElement()
        {
            pickingMode = PickingMode.Ignore;
            style.position = Position.Absolute;
            style.left = -HalfWidth; style.top = -HalfHeight;
            style.width = HalfWidth * 2f; style.height = HalfHeight * 2f;
            generateVisualContent += Paint;
        }

        static Vector2 Local(Vector2 p) => new Vector2(p.x + HalfWidth, -p.y + HalfHeight);

        void Paint(MeshGenerationContext ctx)
        {
            var painter = ctx.painter2D;
            painter.lineJoin = LineJoin.Round; painter.lineCap = LineCap.Round;
            foreach (var pass in new[] { 0, 1 })
                foreach (var l in Lines)
                {
                    if (!l.Visible || l.Points.Length < 2) continue;
                    painter.lineWidth = pass == 0 ? l.Thickness + 4f : l.Thickness;
                    painter.strokeColor = pass == 0 ? Theme.Ink : l.Color;
                    painter.BeginPath();
                    painter.MoveTo(Local(l.Points[0]));
                    for (int i = 1; i < l.Points.Length; i++) painter.LineTo(Local(l.Points[i]));
                    painter.Stroke();
                }
        }
    }

    /// <summary>Case du plateau (UI Toolkit) : disque coloré par type, glyphe, rang, objectif, surbrillance, coût et anneau de navigation.</summary>
    public sealed class BoardNodeHud
    {
        public const float Size = 86f;
        public VisualElement Root { get; } = new VisualElement();
        public UnityEngine.UIElements.Button Frame { get; } = new UnityEngine.UIElements.Button();
        readonly VisualElement _highlight = new VisualElement(), _focus = new VisualElement(), _objective = new VisualElement(), _disc = new VisualElement(), _icon = new VisualElement(), _cost = new VisualElement(), _anchor = new VisualElement();
        readonly Label _glyph = new Label(), _sub = new Label(), _tag = new Label(), _costText = new Label();
        bool _pulse, _isObjective;

        public bool Focused { get; private set; }

        public BoardNodeHud(BoardMapNode n)
        {
            _isObjective = n.IsObjective;
            Root.name = "Node_" + n.Id.Value;
            Root.AddToClassList("hd-board__node");
            _highlight.AddToClassList("hd-board__hl"); _focus.AddToClassList("hd-board__focus"); _objective.AddToClassList("hd-board__obj");
            Frame.RemoveFromClassList("unity-button"); Frame.AddToClassList("hd-board__frame");
            _disc.AddToClassList("hd-board__disc"); _glyph.AddToClassList("hd-board__glyph"); _icon.AddToClassList("hd-board__icon");
            _sub.AddToClassList("hd-board__sub"); _tag.AddToClassList("hd-board__tag"); _cost.AddToClassList("hd-board__nodecost"); _costText.AddToClassList("hd-board__nodecosttext");
            _anchor.AddToClassList("hd-board__anchor");
            _disc.Add(_glyph); _disc.Add(_icon); Frame.Add(_disc); _cost.Add(_costText); Frame.Add(_anchor);
            foreach (var e in new VisualElement[] { _highlight, _focus, _objective, Frame, _sub, _tag, _cost }) Root.Add(e);
            foreach (var e in new VisualElement[] { _highlight, _focus, _objective, _disc, _glyph, _icon, _sub, _tag, _cost, _costText, _anchor }) e.pickingMode = PickingMode.Ignore;
            _highlight.style.display = DisplayStyle.None; _focus.style.display = DisplayStyle.None; _cost.style.display = DisplayStyle.None;
            _objective.style.display = n.IsObjective ? DisplayStyle.Flex : DisplayStyle.None; _tag.style.display = n.IsObjective ? DisplayStyle.Flex : DisplayStyle.None;
            _anchor.style.display = DisplayStyle.None;
        }

        public VisualElement AnchorElement => _anchor;

        public void SetFocused(bool on) { Focused = on; _focus.style.display = on ? DisplayStyle.Flex : DisplayStyle.None; }

        public void Tick()
        {
            if (!_pulse) return;
            if (MotionSettings.Reduced) { HudAnim.SetScale(_highlight, 1f); return; }
            float k = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 5f);
            HudAnim.SetScale(_highlight, 1f + 0.08f * k);
            _highlight.style.opacity = 0.65f + 0.35f * k;
        }

        static void Border(VisualElement e, Color c) => e.style.borderTopColor = e.style.borderBottomColor = e.style.borderLeftColor = e.style.borderRightColor = c;

        public void Apply(BoardNodeState s)
        {
            bool fogReach = s.Mark == NodeMark.FogDestination;
            if (!s.Revealed)
            {
                _disc.style.backgroundColor = Theme.WithAlpha(Theme.Surface2, fogReach ? 0.85f : 0.55f);
                Border(Frame, fogReach ? Theme.Info : Theme.WithAlpha(Theme.Ink, 0.5f));
                _glyph.text = "?";
                Icon("?");
                _glyph.style.color = fogReach ? Theme.Info : Theme.WithAlpha(Theme.TextMuted, 0.6f);
                _sub.text = "";
            }
            else
            {
                var c = Theme.CaseColor(s.Type, s.Rank);
                if (s.Spent) c = Color.Lerp(c, Theme.Surface3, 0.65f);
                _disc.style.backgroundColor = c;
                Border(Frame, Theme.Ink);
                _glyph.style.color = Color.white;
                _glyph.text = s.Glyph;
                Icon(s.Glyph);
                _sub.text = s.Sub;
                _sub.style.color = s.SubElite ? Theme.Primary : Theme.TextMuted;
            }
            if (_isObjective)
            {
                var oc = s.ObjectiveDone ? Theme.Success : Theme.Primary;
                Border(_objective, oc);
                _tag.style.color = oc;
                _tag.text = s.ObjectiveDone ? "OK" : "OBJ";
            }
            bool marked = s.Mark != NodeMark.None;
            _highlight.style.display = marked ? DisplayStyle.Flex : DisplayStyle.None;
            if (marked) Border(_highlight, HighlightColor(s.Mark));
            _pulse = s.Mark == NodeMark.Destination;
            if (!_pulse) { HudAnim.SetScale(_highlight, 1f); _highlight.style.opacity = 1f; }
            _anchor.style.display = s.Mark == NodeMark.Destination ? DisplayStyle.Flex : DisplayStyle.None; // le tutoriel désigne les cases à portée
            bool real = s.Mark == NodeMark.Destination || s.Mark == NodeMark.Target;
            bool badge = s.DestinationCost >= 0 && (real || fogReach);
            _cost.style.display = badge ? DisplayStyle.Flex : DisplayStyle.None;
            if (badge) _costText.text = s.DestinationCost.ToString();
        }

        /// <summary>Pictogramme de l'extension Aube tactique à la place des lettres (VS, MJ, !, $, Zz) ; les lettres restent en repli.</summary>
        void Icon(string glyph)
        {
            string? file = glyph == "VS" ? "crossed-swords" : glyph == "!" ? "node-alert" : glyph == "$" ? "node-crate" : glyph == "Zz" ? "tent" : glyph == "MJ" ? "target" : null;
            var sprite = file != null ? TacticalArt.Ext("Icons", file) : null;
            _icon.style.backgroundImage = sprite != null ? new StyleBackground(Background.FromSprite(sprite)) : new StyleBackground(StyleKeyword.None);
            _icon.style.display = sprite != null ? DisplayStyle.Flex : DisplayStyle.None;
            _glyph.style.display = sprite == null ? DisplayStyle.Flex : DisplayStyle.None;
        }

        static Color HighlightColor(NodeMark m)
        {
            switch (m)
            {
                case NodeMark.FogDestination: return Theme.WithAlpha(Theme.Info, 0.8f);
                case NodeMark.Path: return Theme.WithAlpha(Theme.Info, 0.9f);
                case NodeMark.Target: return Color.white;
                default: return Theme.Reach;
            }
        }
    }

    /// <summary>Nuage de brouillard de guerre : dérive lentement au-dessus d'une case cachée, se dissipe quand elle est révélée.</summary>
    public sealed class FogCloud
    {
        const float MaxAlpha = 0.78f, FadeSpeed = 1.4f;
        public VisualElement Root { get; } = new VisualElement();
        Vector2 _home;
        float _phase, _alpha, _target, _w, _h;
        Color _tint = Color.white;

        public bool Visible => _alpha > 0.01f || _target > 0f;
        public bool Fogged => _target > 0f;
        public float Alpha => _alpha;

        public FogCloud(Vector2 home, float size, float phase, Color tint, bool fogged)
        {
            _home = home; _phase = phase; _tint = tint;
            _w = size; _h = size * 0.72f;
            Root.AddToClassList("hd-board__fog"); Root.pickingMode = PickingMode.Ignore;
            Root.style.width = _w; Root.style.height = _h;
            Root.style.backgroundImage = new StyleBackground(Background.FromSprite(Sprites.Cloud));
            _target = fogged ? 1f : 0f; _alpha = _target;
            Place(Vector2.zero);
            Apply();
        }

        public void SetFogged(bool fogged) => _target = fogged ? 1f : 0f;

        void Place(Vector2 drift)
        {
            Root.style.left = _home.x + drift.x - _w * 0.5f;
            Root.style.top = -(_home.y + drift.y) - _h * 0.5f;
        }

        public void Tick()
        {
            _alpha = Mathf.MoveTowards(_alpha, _target, FadeSpeed * Time.unscaledDeltaTime);
            if (!MotionSettings.Reduced && _alpha > 0.01f)
            {
                float t = Time.unscaledTime;
                Place(new Vector2(Mathf.Sin(t * 0.30f + _phase) * 22f, Mathf.Cos(t * 0.23f + _phase * 1.3f) * 14f));
            }
            Apply();
        }

        void Apply()
        {
            Root.style.unityBackgroundImageTintColor = new Color(_tint.r, _tint.g, _tint.b, _alpha * MaxAlpha);
            Root.style.display = _alpha > 0.003f ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    /// <summary>
    /// Carte du plateau (UI Toolkit) : instancie les cases et les liens d'après les données de la partie, place le pion et l'anime case par case.
    /// Plateau agrandi dans une fenêtre à caméra : glisser pour se déplacer, molette / boutons pour zoomer, flèches ou croix directionnelle pour
    /// naviguer d'une case à l'autre, dézoom automatique sur le chemin prévisualisé. Ne calcule jamais une règle de jeu.
    /// </summary>
    public sealed class BoardMapView : IBoardMapView
    {
        const float StepX = 170f, StepY = 250f;
        const float MinZoom = 0.4f, MaxZoom = 1.6f, DefaultZoom = 0.9f;
        /// <summary>Largeur laissée libre à droite pour la carte d'information quand on dézoome sur un chemin.</summary>
        const float PreviewInset = 560f;
        const float FitPadding = 170f;
        const float PawnSpeed = 900f;
        static readonly Vector2 PawnOffset = new Vector2(0, 22);

        readonly VisualElement _viewport, _content, _nodeLayer, _fogLayer, _costLayer, _pawn;
        readonly BoardLinksElement _links;
        readonly Component _owner;
        readonly Dictionary<CaseId, BoardNodeHud> _nodes = new Dictionary<CaseId, BoardNodeHud>();
        readonly Dictionary<CaseId, Vector2> _positions = new Dictionary<CaseId, Vector2>();
        readonly Dictionary<CaseId, List<(CaseId To, int Link)>> _neighbors = new Dictionary<CaseId, List<(CaseId To, int Link)>>();
        readonly Dictionary<CaseId, List<FogCloud>> _fog = new Dictionary<CaseId, List<FogCloud>>();
        readonly List<Label> _costs = new List<Label>();
        readonly Dictionary<(CaseId, CaseId), Vector2[]> _curves = new Dictionary<(CaseId, CaseId), Vector2[]>();
        readonly List<CaseId> _cycle = new List<CaseId>();
        readonly Label _pawnInitial = new Label();
        VisualElement? _background;
        bool _animating, _dragging, _down;
        float _zoom = DefaultZoom;
        Vector2 _pan;
        Rect _bounds;
        CaseId _pawnAt;
        CaseId? _focused;
        Vector2 _pawnPos, _downAt;
        int _cameraToken;

        public event Action<CaseId>? NodeTapped;

        public BoardMapView(VisualElement viewport, Component owner)
        {
            _viewport = viewport; _owner = owner;
            _content = new VisualElement { pickingMode = PickingMode.Ignore };
            _content.style.position = Position.Absolute; _content.style.left = Length.Percent(50); _content.style.top = Length.Percent(50);
            _content.style.width = 0; _content.style.height = 0;
            _links = new BoardLinksElement();
            _costLayer = new VisualElement { pickingMode = PickingMode.Ignore };
            _fogLayer = new VisualElement { pickingMode = PickingMode.Ignore };
            _nodeLayer = new VisualElement { pickingMode = PickingMode.Ignore };
            _pawn = new VisualElement { pickingMode = PickingMode.Ignore };
            _pawn.AddToClassList("hd-board__pawn"); _pawnInitial.AddToClassList("hd-board__pawninitial"); _pawn.Add(_pawnInitial);
            foreach (var e in new VisualElement[] { _links, _costLayer, _fogLayer, _nodeLayer }) _content.Add(e);
            viewport.Add(_content);
            viewport.RegisterCallback<PointerDownEvent>(OnDown);
            viewport.RegisterCallback<PointerMoveEvent>(OnMove);
            viewport.RegisterCallback<PointerUpEvent>(OnUp);
            viewport.RegisterCallback<WheelEvent>(e => { if (Mathf.Abs(e.delta.y) > 0.01f) ZoomBy(e.delta.y < 0 ? 1.15f : 1f / 1.15f); e.StopPropagation(); });
        }

        public CaseId? Focused => _focused;
        public float Zoom => _zoom;
        public IEnumerable<FogCloud> FogClouds { get { foreach (var l in _fog.Values) foreach (var c in l) yield return c; } }
        public HdButtonRef NodeButton(CaseId id) => new HdButtonRef(_nodes[id].Frame, _owner);

        static Vector2 GridToPixels(float x, float y) => new Vector2((x - 8f) * StepX, (y - 2f) * StepY);
        static Vector2 PosOf(BoardMapNode n) => GridToPixels(n.X, n.Y);
        Vector2 PawnPos(CaseId id) => _positions[id] + PawnOffset;
        Vector2 ViewSize => _viewport.layout.size.x > 1f ? _viewport.layout.size : new Vector2(1880f, 780f);

        public void Build(BoardMapData d)
        {
            BuildBackground(d);
            _links.Lines.Clear(); _costLayer.Clear(); _nodeLayer.Clear(); _fogLayer.Clear();
            _nodes.Clear(); _positions.Clear(); _neighbors.Clear(); _fog.Clear(); _costs.Clear(); _cycle.Clear(); _curves.Clear();
            _focused = null;
            foreach (var n in d.Nodes) _positions[n.Id] = PosOf(n);
            ComputeBounds();
            for (int i = 0; i < d.Links.Length; i++)
            {
                var l = d.Links[i];
                var points = CurvePixels(l.Curve);
                _curves[(l.A, l.B)] = points;
                var reversed = (Vector2[])points.Clone(); Array.Reverse(reversed);
                _curves[(l.B, l.A)] = reversed;
                _links.Lines.Add(new BoardLinksElement.Line { Points = points, Color = Theme.Surface3 });
                var badge = new Label(l.Cost + " PM") { name = "LinkCost_" + l.A.Value + "_" + l.B.Value, pickingMode = PickingMode.Ignore };
                badge.AddToClassList("hd-board__cost");
                var mid = CurvePoint(points, 0.5f);
                badge.style.left = mid.x - 52f; badge.style.top = -mid.y - 26f;
                _costLayer.Add(badge); _costs.Add(badge);
                AddNeighbor(l.A, l.B, i); AddNeighbor(l.B, l.A, i);
            }
            _links.MarkDirtyRepaint();
            var tint = new Color(0.46f, 0.54f, 0.66f, 1f);
            int k = 0;
            foreach (var n in d.Nodes)
            {
                var list = new List<FogCloud>();
                for (int i = 0; i < 2; i++, k++)
                {
                    var cloud = new FogCloud(_positions[n.Id] + new Vector2(i == 0 ? -46f : 52f, i == 0 ? 30f : -38f), i == 0 ? 470f : 400f, k * 1.7f, tint, true);
                    _fogLayer.Add(cloud.Root); list.Add(cloud);
                }
                _fog[n.Id] = list;
            }
            foreach (var n in d.Nodes)
            {
                var node = new BoardNodeHud(n);
                node.Root.style.left = _positions[n.Id].x - BoardNodeHud.Size * 0.5f - 20f;
                node.Root.style.top = -_positions[n.Id].y - BoardNodeHud.Size * 0.5f - 20f;
                var id = n.Id;
                node.Frame.clicked += () => { if (!_dragging) NodeTapped?.Invoke(id); };
                HdTutorialAnchor.Attach(_owner, node.AnchorElement, TutorialTarget.BoardDestination, id.Value);
                _nodeLayer.Add(node.Root); _nodes.Add(id, node);
            }
            HdPortrait.Fill(_pawn, _pawnInitial, d.Pawn);
            _nodeLayer.Add(_pawn);
            _pawnAt = d.PawnAt;
            SetPawn(PawnPos(d.PawnAt));
            _zoom = DefaultZoom;
            CenterOn(d.PawnAt, false);
        }

        void SetPawn(Vector2 p)
        {
            _pawnPos = p;
            _pawn.style.left = p.x - 35f; _pawn.style.top = -p.y - 70f;
        }

        /// <summary>Fond de carte généré par l'outil de level design : une image derrière les liens, cadrée sur la grille qu'elle couvre.</summary>
        void BuildBackground(BoardMapData d)
        {
            _background?.RemoveFromHierarchy();
            _background = null;
            if (string.IsNullOrEmpty(d.BackgroundKey)) return;
            var sprite = Resources.Load<Sprite>("Zones/" + d.BackgroundKey);
            if (sprite == null) return;
            var bg = new VisualElement { pickingMode = PickingMode.Ignore };
            float w = (d.BgMaxX - d.BgMinX) * StepX, h = (d.BgMaxY - d.BgMinY) * StepY;
            float cx = ((d.BgMinX + d.BgMaxX) / 2f - 8f) * StepX, cy = ((d.BgMinY + d.BgMaxY) / 2f - 2f) * StepY;
            bg.style.position = Position.Absolute;
            bg.style.left = cx - w * 0.5f; bg.style.top = -cy - h * 0.5f; bg.style.width = w; bg.style.height = h;
            bg.style.backgroundImage = new StyleBackground(Background.FromSprite(sprite));
            bg.style.unityBackgroundImageTintColor = new Color(0.55f, 0.6f, 0.7f, 0.3f); // l'illustration de l'écran reste visible derrière le plateau
            _background = bg;
            _content.Insert(0, bg);
        }

        void AddNeighbor(CaseId from, CaseId to, int link)
        {
            if (!_neighbors.TryGetValue(from, out var list)) _neighbors[from] = list = new List<(CaseId, int)>();
            list.Add((to, link));
        }

        void ComputeBounds()
        {
            bool first = true;
            Vector2 min = Vector2.zero, max = Vector2.zero;
            foreach (var p in _positions.Values)
            {
                if (first) { min = max = p; first = false; continue; }
                min = Vector2.Min(min, p); max = Vector2.Max(max, p);
            }
            _bounds = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        public void Refresh(IReadOnlyList<BoardNodeState> nodes, IReadOnlyList<BoardLinkState> links, CaseId pawnAt, bool movePawn)
        {
            _cycle.Clear();
            foreach (var s in nodes)
            {
                if (!_nodes.TryGetValue(s.Id, out var n)) continue;
                n.Apply(s);
                if (_fog.TryGetValue(s.Id, out var clouds)) foreach (var cloud in clouds) cloud.SetFogged(!s.Revealed);
                if (s.Mark == NodeMark.Destination || s.Mark == NodeMark.FogDestination || s.Mark == NodeMark.Target) _cycle.Add(s.Id);
            }
            for (int i = 0; i < links.Count && i < _links.Lines.Count; i++)
            {
                var l = links[i];
                var line = _links.Lines[i];
                line.Visible = l.Visible;
                line.Thickness = l.Mark == LinkMark.Path ? 22f : l.Mark == LinkMark.Beyond ? 16f : 12f;
                line.Color = l.Mark == LinkMark.Path ? Theme.Info
                           : l.Mark == LinkMark.Beyond ? Theme.WithAlpha(Theme.Info, 0.5f)
                           : l.Dim ? Theme.WithAlpha(Theme.Surface3, 0.3f) : Theme.Surface3;
                _costs[i].style.display = l.Visible ? DisplayStyle.Flex : DisplayStyle.None;
                _costs[i].EnableInClassList("hd-board__cost--on", l.Mark == LinkMark.Path);
            }
            _links.MarkDirtyRepaint();
            _pawnAt = pawnAt;
            if (_focused.HasValue && !_nodes.ContainsKey(_focused.Value)) _focused = null;
            if (movePawn && !_animating) SetPawn(PawnPos(pawnAt));
            _pawn.BringToFront();
        }

        static Vector2[] CurvePixels(BezierCurve curve)
        {
            int segments = Mathf.Clamp(Mathf.CeilToInt(curve.Length() * StepX / 40f), 6, 28);
            var grid = curve.Sample(segments);
            var px = new Vector2[grid.Length];
            for (int i = 0; i < grid.Length; i++) px[i] = GridToPixels(grid[i].X, grid[i].Y);
            return px;
        }

        static Vector2 CurvePoint(IReadOnlyList<Vector2> pts, float t)
        {
            float total = 0f;
            for (int i = 1; i < pts.Count; i++) total += Vector2.Distance(pts[i - 1], pts[i]);
            float target = total * Mathf.Clamp01(t);
            for (int i = 1; i < pts.Count; i++)
            {
                float seg = Vector2.Distance(pts[i - 1], pts[i]);
                if (target <= seg || i == pts.Count - 1) return Vector2.Lerp(pts[i - 1], pts[i], seg < 1e-3f ? 0f : Mathf.Clamp01(target / seg));
                target -= seg;
            }
            return pts[0];
        }

        List<Vector2> TravelPoints(IReadOnlyList<CaseId> path)
        {
            var pts = new List<Vector2>();
            var at = StartOf(path[0]);
            foreach (var step in path)
            {
                if (_curves.TryGetValue((at, step), out var curve)) foreach (var p in curve) pts.Add(p + PawnOffset);
                else if (_positions.TryGetValue(step, out var to)) pts.Add(to + PawnOffset);
                at = step;
            }
            return pts;
        }

        CaseId StartOf(CaseId first)
        {
            var best = _pawnAt;
            float bestDist = float.MaxValue;
            if (_neighbors.TryGetValue(first, out var list))
                foreach (var (to, _) in list)
                {
                    float d = Vector2.Distance(_pawnPos, PawnPos(to));
                    if (d < bestDist) { bestDist = d; best = to; }
                }
            return best;
        }

        public async UniTask MovePawnAsync(IReadOnlyList<CaseId> path, CancellationToken ct)
        {
            if (path.Count == 0) return;
            var pts = TravelPoints(path);
            if (pts.Count < 2) return;
            var cumulative = new float[pts.Count];
            for (int i = 1; i < pts.Count; i++) cumulative[i] = cumulative[i - 1] + Vector2.Distance(pts[i - 1], pts[i]);
            float total = cumulative[pts.Count - 1];
            _animating = true;
            try
            {
                var done = new UniTaskCompletionSource();
                float duration = Mathf.Clamp(total / PawnSpeed, 0.25f, 3f);
                SetPawn(pts[0]);
                HudAnim.Run(_pawn, duration, k =>
                {
                    float d = total * k;
                    int i = 1;
                    while (i < pts.Count - 1 && cumulative[i] < d) i++;
                    float seg = cumulative[i] - cumulative[i - 1];
                    SetPawn(Vector2.Lerp(pts[i - 1], pts[i], seg < 1e-3f ? 1f : Mathf.Clamp01((d - cumulative[i - 1]) / seg)));
                }, () => { SetPawn(pts[pts.Count - 1]); HudAnim.Punch(_pawn, 0.1f, 0.12f); done.TrySetResult(); });
                await done.Task.AttachExternalCancellation(ct);
            }
            finally { _animating = false; }
        }

        // ------------------------------------------------------------------ Caméra

        public void CenterOn(CaseId id, bool animate)
        {
            if (!_positions.TryGetValue(id, out var p)) return;
            MoveCamera(_zoom, ClampPan(-p * _zoom, _zoom), animate);
        }

        public void FitTo(IReadOnlyList<CaseId> ids, bool animate)
        {
            bool first = true;
            Vector2 min = Vector2.zero, max = Vector2.zero;
            foreach (var id in ids)
            {
                if (!_positions.TryGetValue(id, out var p)) continue;
                if (first) { min = max = p; first = false; continue; }
                min = Vector2.Min(min, p); max = Vector2.Max(max, p);
            }
            if (first) return;
            min -= Vector2.one * FitPadding; max += Vector2.one * FitPadding;
            var size = max - min;
            var view = ViewSize;
            float effW = Mathf.Max(200f, view.x - PreviewInset);
            float zoom = Mathf.Clamp(Mathf.Min(effW / Mathf.Max(1f, size.x), view.y / Mathf.Max(1f, size.y)), MinZoom, 1.2f);
            Vector2 center = (min + max) * 0.5f;
            MoveCamera(zoom, new Vector2(-PreviewInset * 0.5f, 0f) - center * zoom, animate);
        }

        public void ZoomBy(float factor)
        {
            float zoom = Mathf.Clamp(_zoom * factor, MinZoom, MaxZoom);
            Vector2 pan = _pan * (zoom / _zoom);
            MoveCamera(zoom, ClampPan(pan, zoom), true);
        }

        public void Recenter() => CenterOn(_pawnAt, true);

        void MoveCamera(float zoom, Vector2 pan, bool animate)
        {
            int token = ++_cameraToken;
            if (!animate || !HudAnim.Allowed) { Apply(zoom, pan); return; }
            float z0 = _zoom;
            Vector2 p0 = _pan;
            HudAnim.Run(_content, 0.35f, k => { if (token == _cameraToken) Apply(Mathf.Lerp(z0, zoom, k), Vector2.Lerp(p0, pan, k)); }, null, HudAnim.EaseOutCubic);
        }

        void Apply(float zoom, Vector2 pan)
        {
            _zoom = zoom; _pan = pan;
            HudAnim.SetScale(_content, zoom);
            HudAnim.SetTranslate(_content, pan.x, -pan.y);
        }

        Vector2 ClampPan(Vector2 pan, float zoom)
        {
            if (_positions.Count == 0) return pan;
            var center = -pan / zoom;
            center.x = Mathf.Clamp(center.x, _bounds.xMin - StepX, _bounds.xMax + StepX);
            center.y = Mathf.Clamp(center.y, _bounds.yMin - StepY, _bounds.yMax + StepY);
            return -center * zoom;
        }

        // ------------------------------------------------------------------ Glisser

        void OnDown(PointerDownEvent e)
        {
            if (e.button != 0) return;
            _down = true; _dragging = false; _downAt = e.position;
        }

        void OnMove(PointerMoveEvent e)
        {
            if (!_down) return;
            if (!_dragging)
            {
                if (Vector2.Distance(e.position, _downAt) < 10f) return;
                _dragging = true;
                _cameraToken++;
                _viewport.CapturePointer(e.pointerId);
            }
            var delta = (Vector2)e.deltaPosition;
            Apply(_zoom, ClampPan(_pan + new Vector2(delta.x, -delta.y), _zoom));
        }

        void OnUp(PointerUpEvent e)
        {
            if (!_down) return;
            _down = false;
            if (_dragging) { _viewport.ReleasePointer(e.pointerId); _viewport.schedule.Execute(() => _dragging = false).ExecuteLater(30); }
        }

        // ------------------------------------------------------------------ Navigation case à case

        /// <summary>Chaque image : pulsation des cases accessibles, brouillard qui dérive, touches de navigation.</summary>
        public void Tick()
        {
            foreach (var n in _nodes.Values) n.Tick();
            foreach (var l in _fog.Values) foreach (var c in l) c.Tick();
            if (_positions.Count == 0) return;
            if (Keys.Pressed(Key.LeftArrow) || Keys.Pressed(Key.A)) Navigate(Vector2.left);
            else if (Keys.Pressed(Key.RightArrow) || Keys.Pressed(Key.D)) Navigate(Vector2.right);
            else if (Keys.Pressed(Key.UpArrow) || Keys.Pressed(Key.W)) Navigate(Vector2.up);
            else if (Keys.Pressed(Key.DownArrow) || Keys.Pressed(Key.S)) Navigate(Vector2.down);
            else if (Keys.Pressed(Key.Tab)) CycleFocus(1);
            else if (Keys.Pressed(Key.Equals) || Keys.Pressed(Key.NumpadPlus)) ZoomBy(1.25f);
            else if (Keys.Pressed(Key.Minus) || Keys.Pressed(Key.NumpadMinus)) ZoomBy(0.8f);
            var pad = Gamepad.current;
            if (pad == null) return;
            if (pad.dpad.left.wasPressedThisFrame) Navigate(Vector2.left);
            else if (pad.dpad.right.wasPressedThisFrame) Navigate(Vector2.right);
            else if (pad.dpad.up.wasPressedThisFrame) Navigate(Vector2.up);
            else if (pad.dpad.down.wasPressedThisFrame) Navigate(Vector2.down);
            else if (pad.leftShoulder.wasPressedThisFrame) CycleFocus(-1);
            else if (pad.rightShoulder.wasPressedThisFrame) CycleFocus(1);
            else if (pad.buttonSouth.wasPressedThisFrame) ConfirmFocus();
        }

        public void ConfirmFocus()
        {
            if (_focused.HasValue) NodeTapped?.Invoke(_focused.Value);
        }

        public bool Navigate(Vector2 direction)
        {
            var from = _focused ?? _pawnAt;
            if (!_focused.HasValue) { SetFocus(from); return true; }
            if (!_neighbors.TryGetValue(from, out var list)) return false;
            CaseId? best = null;
            float bestScore = 0.3f;
            foreach (var (to, link) in list)
            {
                if (link < _links.Lines.Count && !_links.Lines[link].Visible) continue;
                var delta = (_positions[to] - _positions[from]).normalized;
                float score = Vector2.Dot(delta, direction.normalized);
                if (score > bestScore) { bestScore = score; best = to; }
            }
            if (!best.HasValue) return false;
            SetFocus(best.Value);
            return true;
        }

        public void CycleFocus(int delta)
        {
            if (_cycle.Count == 0) return;
            int i = _focused.HasValue ? _cycle.IndexOf(_focused.Value) : -1;
            i = i < 0 ? (delta > 0 ? 0 : _cycle.Count - 1) : (i + delta + _cycle.Count) % _cycle.Count;
            SetFocus(_cycle[i]);
        }

        void SetFocus(CaseId id)
        {
            if (_focused.HasValue && _nodes.TryGetValue(_focused.Value, out var old)) old.SetFocused(false);
            _focused = id;
            if (_nodes.TryGetValue(id, out var n)) n.SetFocused(true);
            CenterOn(id, true);
        }
    }
}
