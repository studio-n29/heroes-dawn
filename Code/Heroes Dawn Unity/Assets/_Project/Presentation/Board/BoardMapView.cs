#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Domain.Maps;
using N29.HeroesDawn.Presentation.Common;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Board
{
    /// <summary>
    /// Carte du plateau : instancie les cases (BoardNode) et les liens (BoardLink) d'après les données de la partie,
    /// place le pion (Pawn) et l'anime case par case. Plateau agrandi dans une fenêtre à caméra : glisser pour se déplacer,
    /// molette / boutons pour zoomer, flèches ou croix directionnelle pour naviguer d'une case à l'autre, dézoom automatique
    /// sur le chemin prévisualisé. Ne calcule jamais une règle de jeu.
    /// </summary>
    public sealed class BoardMapView : MonoBehaviour, IBoardMapView, IDragHandler, IScrollHandler
    {
        /// <summary>Espacement des cases (agrandi pour la lisibilité).</summary>
        const float StepX = 170f;
        const float StepY = 250f;
        const float MinZoom = 0.4f;
        const float MaxZoom = 1.6f;
        const float DefaultZoom = 0.9f;
        /// <summary>Largeur laissée libre à droite pour la carte d'information quand on dézoome sur un chemin.</summary>
        const float PreviewInset = 560f;
        const float FitPadding = 170f;

        [SerializeField] RectTransform content = null!;
        [SerializeField] RectTransform linkLayer = null!;
        [SerializeField] RectTransform nodeLayer = null!;
        [SerializeField] BoardNodeView nodePrefab = null!;
        [SerializeField] Image linkPrefab = null!;
        [SerializeField] BoardLinkCostView linkCostPrefab = null!;
        [SerializeField] PortraitView pawnPrefab = null!;
        [SerializeField] Button zoomIn = null!;
        [SerializeField] Button zoomOut = null!;
        [SerializeField] Button recenter = null!;
        [SerializeField] Button prevCase = null!;
        [SerializeField] Button nextCase = null!;

        readonly Dictionary<CaseId, BoardNodeView> _nodes = new Dictionary<CaseId, BoardNodeView>();
        readonly Dictionary<CaseId, Vector2> _positions = new Dictionary<CaseId, Vector2>();
        readonly Dictionary<CaseId, List<(CaseId To, int Link)>> _neighbors = new Dictionary<CaseId, List<(CaseId To, int Link)>>();
        readonly List<Image[]> _links = new List<Image[]>();
        readonly List<BoardLinkCostView> _costs = new List<BoardLinkCostView>();
        /// <summary>Tracé de chaque chemin en pixels, dans le sens A vers B puis B vers A.</summary>
        readonly Dictionary<(CaseId, CaseId), Vector2[]> _curves = new Dictionary<(CaseId, CaseId), Vector2[]>();
        readonly List<bool> _linkVisible = new List<bool>();
        readonly List<CaseId> _cycle = new List<CaseId>();
        RectTransform? _pawn;
        Image? _background;
        Coroutine? _move;
        Coroutine? _camera;
        bool _animating;
        float _zoom = DefaultZoom;
        Rect _bounds;
        CaseId _pawnAt;
        CaseId? _focused;

        public event Action<CaseId>? NodeTapped;

        /// <summary>Case sélectionnée au clavier / à la manette (anneau blanc), s'il y en a une.</summary>
        public CaseId? Focused => _focused;

        /// <summary>Zoom courant de la caméra (tests).</summary>
        public float Zoom => _zoom;

        /// <summary>Bouton d'une case (tests PlayMode : toucher réel).</summary>
        public Button NodeButton(CaseId id) => _nodes[id].Button;

        static Vector2 PosOf(BoardMapNode n) => GridToPixels(n.X, n.Y);

        static Vector2 GridToPixels(float x, float y) => new Vector2((x - 8f) * StepX, (y - 2f) * StepY);

        void Awake()
        {
            if (zoomIn != null) zoomIn.onClick.AddListener(() => ZoomBy(1.25f));
            if (zoomOut != null) zoomOut.onClick.AddListener(() => ZoomBy(0.8f));
            if (recenter != null) recenter.onClick.AddListener(() => CenterOn(_pawnAt, true));
            if (prevCase != null) prevCase.onClick.AddListener(() => CycleFocus(-1));
            if (nextCase != null) nextCase.onClick.AddListener(() => CycleFocus(1));
        }

        public void Build(BoardMapData d)
        {
            BuildBackground(d);
            linkLayer.Clear();
            nodeLayer.Clear();
            _nodes.Clear();
            _positions.Clear();
            _neighbors.Clear();
            _links.Clear();
            _linkVisible.Clear();
            _cycle.Clear();
            _focused = null;
            foreach (var n in d.Nodes) _positions[n.Id] = PosOf(n);
            ComputeBounds();
            for (int i = 0; i < d.Links.Length; i++)
            {
                var l = d.Links[i];
                var points = CurvePixels(l.Curve);
                _curves[(l.A, l.B)] = points;
                var reversed = (Vector2[])points.Clone();
                Array.Reverse(reversed);
                _curves[(l.B, l.A)] = reversed;
                var segments = new Image[points.Length - 1];
                for (int s = 0; s < segments.Length; s++)
                {
                    Vector2 p = points[s], q = points[s + 1];
                    var seg = Instantiate(linkPrefab, linkLayer);
                    var rt = seg.rectTransform;
                    rt.anchoredPosition = (p + q) * 0.5f;
                    rt.sizeDelta = new Vector2(Vector2.Distance(p, q) + 3f, 12f);
                    rt.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(q.y - p.y, q.x - p.x) * Mathf.Rad2Deg);
                    segments[s] = seg;
                }
                var badge = Instantiate(linkCostPrefab, linkLayer);
                ((RectTransform)badge.transform).anchoredPosition = CurvePoint(points, 0.5f);
                badge.Show(l.Cost);
                badge.name = "LinkCost_" + l.A.Value + "_" + l.B.Value;
                _costs.Add(badge);
                _links.Add(segments);
                _linkVisible.Add(true);
                AddNeighbor(l.A, l.B, i);
                AddNeighbor(l.B, l.A, i);
            }
            foreach (var c in _costs) c.transform.SetAsLastSibling();
            foreach (var n in d.Nodes)
            {
                var node = Instantiate(nodePrefab, nodeLayer);
                ((RectTransform)node.transform).anchoredPosition = _positions[n.Id];
                node.name = "Node_" + n.Id.Value;
                node.Init(n);
                var id = n.Id;
                node.Button.onClick.AddListener(() => NodeTapped?.Invoke(id));
                _nodes.Add(id, node);
            }
            var pawn = Instantiate(pawnPrefab, nodeLayer);
            pawn.Show(d.Pawn);
            _pawn = (RectTransform)pawn.transform;
            _pawnAt = d.PawnAt;
            _pawn.anchoredPosition = PawnPos(d.PawnAt);
            _zoom = DefaultZoom;
            CenterOn(d.PawnAt, false);
        }

        /// <summary>Fond de carte généré par l'outil de level design : une image derrière les liens, cadrée sur la grille qu'elle couvre.</summary>
        void BuildBackground(BoardMapData d)
        {
            if (_background != null) Destroy(_background.gameObject);
            _background = null;
            if (string.IsNullOrEmpty(d.BackgroundKey)) return;
            var sprite = Resources.Load<Sprite>("Zones/" + d.BackgroundKey);
            if (sprite == null) return;
            var go = new GameObject("Background", typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(content, false);
            rt.SetAsFirstSibling();
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(((d.BgMinX + d.BgMaxX) / 2f - 8f) * StepX, ((d.BgMinY + d.BgMaxY) / 2f - 2f) * StepY);
            rt.sizeDelta = new Vector2((d.BgMaxX - d.BgMinX) * StepX, (d.BgMaxY - d.BgMinY) * StepY);
            _background = go.GetComponent<Image>();
            _background.sprite = sprite;
            _background.raycastTarget = false;
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
                min = Vector2.Min(min, p);
                max = Vector2.Max(max, p);
            }
            _bounds = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        Vector2 PawnPos(CaseId id) => _positions[id] + PawnOffset;

        public void Refresh(IReadOnlyList<BoardNodeState> nodes, IReadOnlyList<BoardLinkState> links, CaseId pawnAt, bool movePawn)
        {
            _cycle.Clear();
            foreach (var s in nodes)
            {
                if (!_nodes.TryGetValue(s.Id, out var n)) continue;
                n.Apply(s);
                if (s.Mark == NodeMark.Destination || s.Mark == NodeMark.FogDestination || s.Mark == NodeMark.Target) _cycle.Add(s.Id);
            }
            for (int i = 0; i < links.Count && i < _links.Count; i++)
            {
                var l = links[i];
                _linkVisible[i] = l.Visible;
                float thickness = l.Mark == LinkMark.Path ? 22f : l.Mark == LinkMark.Beyond ? 16f : 12f;
                var color = l.Mark == LinkMark.Path ? Theme.Info
                          : l.Mark == LinkMark.Beyond ? Theme.WithAlpha(Theme.Info, 0.5f)
                          : l.Dim ? Theme.WithAlpha(Theme.Surface3, 0.3f) : Theme.Surface3;
                foreach (var seg in _links[i])
                {
                    seg.gameObject.SetActive(l.Visible);
                    var rt = seg.rectTransform;
                    rt.sizeDelta = new Vector2(rt.sizeDelta.x, thickness);
                    seg.color = color;
                }
                _costs[i].gameObject.SetActive(l.Visible);
                _costs[i].SetHighlighted(l.Mark == LinkMark.Path);
            }
            _pawnAt = pawnAt;
            if (_focused.HasValue && !_nodes.ContainsKey(_focused.Value)) _focused = null;
            if (_pawn == null) return;
            if (movePawn && !_animating) _pawn.anchoredPosition = PawnPos(pawnAt);
            _pawn.SetAsLastSibling();
        }

        /// <summary>Tracé d'un chemin en pixels : segments d'au plus ~40 px, extrémités comprises.</summary>
        static Vector2[] CurvePixels(BezierCurve curve)
        {
            int segments = Mathf.Clamp(Mathf.CeilToInt(curve.Length() * StepX / 40f), 6, 28);
            var grid = curve.Sample(segments);
            var px = new Vector2[grid.Length];
            for (int i = 0; i < grid.Length; i++) px[i] = GridToPixels(grid[i].X, grid[i].Y);
            return px;
        }

        /// <summary>Point d'une polyligne à la fraction <paramref name="t"/> de sa longueur.</summary>
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

        /// <summary>Trajet complet du pion : les tracés des chemins mis bout à bout, sans arrêt aux cases traversées.</summary>
        List<Vector2> TravelPoints(IReadOnlyList<CaseId> path)
        {
            var pts = new List<Vector2>();
            var at = StartOf(path[0]);
            foreach (var step in path)
            {
                if (_curves.TryGetValue((at, step), out var curve))
                    foreach (var p in curve) pts.Add(p + PawnOffset);
                else if (_positions.TryGetValue(step, out var to)) pts.Add(to + PawnOffset);
                at = step;
            }
            return pts;
        }

        /// <summary>Case d'où part le pion pour atteindre <paramref name="first"/> : la voisine la plus proche de sa position actuelle.</summary>
        CaseId StartOf(CaseId first)
        {
            var best = _pawnAt;
            float bestDist = float.MaxValue;
            if (_pawn != null && _neighbors.TryGetValue(first, out var list))
                foreach (var (to, _) in list)
                {
                    float d = Vector2.Distance(_pawn.anchoredPosition, PawnPos(to));
                    if (d < bestDist) { bestDist = d; best = to; }
                }
            return best;
        }

        static readonly Vector2 PawnOffset = new Vector2(0, 22);

        /// <summary>Vitesse du pion le long des chemins (pixels par seconde).</summary>
        const float PawnSpeed = 900f;

        public async UniTask MovePawnAsync(IReadOnlyList<CaseId> path, CancellationToken ct)
        {
            if (_move != null) Tween.Stop(_move);
            if (path.Count == 0 || _pawn == null) return;
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
                _pawn.anchoredPosition = pts[0];
                _move = Tween.Run(duration, k =>
                {
                    if (_pawn == null) return;
                    float d = total * k;
                    int i = 1;
                    while (i < pts.Count - 1 && cumulative[i] < d) i++;
                    float seg = cumulative[i] - cumulative[i - 1];
                    _pawn.anchoredPosition = Vector2.Lerp(pts[i - 1], pts[i], seg < 1e-3f ? 1f : Mathf.Clamp01((d - cumulative[i - 1]) / seg));
                }, () =>
                {
                    if (_pawn != null) { _pawn.anchoredPosition = pts[pts.Count - 1]; Tween.Punch(_pawn, 0.1f, 0.12f); }
                    done.TrySetResult();
                }, this, Tween.Linear);
                await done.Task.AttachExternalCancellation(ct);
            }
            finally { _animating = false; }
        }

        // ------------------------------------------------------------------ Caméra

        Vector2 ViewportSize => ((RectTransform)transform).rect.size;

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
                min = Vector2.Min(min, p);
                max = Vector2.Max(max, p);
            }
            if (first) return;
            min -= Vector2.one * FitPadding;
            max += Vector2.one * FitPadding;
            var size = max - min;
            var view = ViewportSize;
            float effW = Mathf.Max(200f, view.x - PreviewInset);
            float zoom = Mathf.Clamp(Mathf.Min(effW / Mathf.Max(1f, size.x), view.y / Mathf.Max(1f, size.y)), MinZoom, 1.2f);
            Vector2 center = (min + max) * 0.5f;
            MoveCamera(zoom, new Vector2(-PreviewInset * 0.5f, 0f) - center * zoom, animate);
        }

        void ZoomBy(float factor)
        {
            float zoom = Mathf.Clamp(_zoom * factor, MinZoom, MaxZoom);
            // Zoom centré sur la fenêtre : le point au centre reste au centre.
            Vector2 pan = content.anchoredPosition * (zoom / _zoom);
            MoveCamera(zoom, ClampPan(pan, zoom), true);
        }

        void MoveCamera(float zoom, Vector2 pan, bool animate)
        {
            if (_camera != null) Tween.Stop(_camera);
            if (!animate || !isActiveAndEnabled)
            {
                Apply(zoom, pan);
                return;
            }
            float z0 = _zoom;
            Vector2 p0 = content.anchoredPosition;
            _camera = Tween.Run(0.35f, k => Apply(Mathf.Lerp(z0, zoom, k), Vector2.Lerp(p0, pan, k)), null, this, Tween.EaseOutCubic);
        }

        void Apply(float zoom, Vector2 pan)
        {
            _zoom = zoom;
            content.localScale = new Vector3(zoom, zoom, 1f);
            content.anchoredPosition = pan;
        }

        /// <summary>Le centre de la fenêtre reste dans le plateau (marge d'une case) : on ne peut pas se perdre hors carte.</summary>
        Vector2 ClampPan(Vector2 pan, float zoom)
        {
            if (_positions.Count == 0) return pan;
            var center = -pan / zoom;
            center.x = Mathf.Clamp(center.x, _bounds.xMin - StepX, _bounds.xMax + StepX);
            center.y = Mathf.Clamp(center.y, _bounds.yMin - StepY, _bounds.yMax + StepY);
            return -center * zoom;
        }

        public void OnDrag(PointerEventData e)
        {
            if (_camera != null) { Tween.Stop(_camera); _camera = null; }
            Apply(_zoom, ClampPan(content.anchoredPosition + e.delta, _zoom));
        }

        public void OnScroll(PointerEventData e)
        {
            if (Mathf.Abs(e.scrollDelta.y) < 0.01f) return;
            ZoomBy(e.scrollDelta.y > 0 ? 1.15f : 1f / 1.15f);
        }

        // ------------------------------------------------------------------ Navigation case à case

        void Update()
        {
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

        /// <summary>Valide la case sélectionnée (comme un toucher sur elle).</summary>
        public void ConfirmFocus()
        {
            if (_focused.HasValue) NodeTapped?.Invoke(_focused.Value);
        }

        /// <summary>
        /// Déplace la sélection vers la case voisine (par un lien visible) la mieux alignée avec la direction demandée.
        /// Premier appui : la sélection part du pion.
        /// </summary>
        public bool Navigate(Vector2 direction)
        {
            var from = _focused ?? _pawnAt;
            if (!_focused.HasValue)
            {
                SetFocus(from);
                return true;
            }
            if (!_neighbors.TryGetValue(from, out var list)) return false;
            CaseId? best = null;
            float bestScore = 0.3f; // au moins ~72° d'écart toléré
            foreach (var (to, link) in list)
            {
                if (link < _linkVisible.Count && !_linkVisible[link]) continue;
                var delta = (_positions[to] - _positions[from]).normalized;
                float score = Vector2.Dot(delta, direction.normalized);
                if (score > bestScore) { bestScore = score; best = to; }
            }
            if (!best.HasValue) return false;
            SetFocus(best.Value);
            return true;
        }

        /// <summary>Passe à la destination surlignée suivante / précédente (boutons ◄ ►, Tab, épaules de la manette).</summary>
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
