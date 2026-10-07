#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Core;
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
        [SerializeField] PortraitView pawnPrefab = null!;
        [SerializeField] Button zoomIn = null!;
        [SerializeField] Button zoomOut = null!;
        [SerializeField] Button recenter = null!;
        [SerializeField] Button prevCase = null!;
        [SerializeField] Button nextCase = null!;

        readonly Dictionary<CaseId, BoardNodeView> _nodes = new Dictionary<CaseId, BoardNodeView>();
        readonly Dictionary<CaseId, Vector2> _positions = new Dictionary<CaseId, Vector2>();
        readonly Dictionary<CaseId, List<(CaseId To, int Link)>> _neighbors = new Dictionary<CaseId, List<(CaseId To, int Link)>>();
        readonly List<Image> _links = new List<Image>();
        readonly List<bool> _linkVisible = new List<bool>();
        readonly List<CaseId> _cycle = new List<CaseId>();
        RectTransform? _pawn;
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

        static Vector2 PosOf(BoardMapNode n) => new Vector2((n.X - 8) * StepX, (n.Y - 2) * StepY);

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
                Vector2 a = _positions[l.A], b = _positions[l.B];
                var line = Instantiate(linkPrefab, linkLayer);
                var rt = line.rectTransform;
                rt.anchoredPosition = (a + b) * 0.5f;
                rt.sizeDelta = new Vector2(Vector2.Distance(a, b), 12f);
                rt.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg);
                _links.Add(line);
                _linkVisible.Add(true);
                AddNeighbor(l.A, l.B, i);
                AddNeighbor(l.B, l.A, i);
            }
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

        Vector2 PawnPos(CaseId id) => _positions[id] + new Vector2(0, 22);

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
                _links[i].gameObject.SetActive(l.Visible);
                var rt = _links[i].rectTransform;
                float thickness = l.Mark == LinkMark.Path ? 22f : l.Mark == LinkMark.Beyond ? 16f : 12f;
                rt.sizeDelta = new Vector2(rt.sizeDelta.x, thickness);
                _links[i].color = l.Mark == LinkMark.Path ? Theme.Info
                                : l.Mark == LinkMark.Beyond ? Theme.WithAlpha(Theme.Info, 0.5f)
                                : l.Dim ? Theme.WithAlpha(Theme.Surface3, 0.3f) : Theme.Surface3;
            }
            _pawnAt = pawnAt;
            if (_focused.HasValue && !_nodes.ContainsKey(_focused.Value)) _focused = null;
            if (_pawn == null) return;
            if (movePawn && !_animating) _pawn.anchoredPosition = PawnPos(pawnAt);
            _pawn.SetAsLastSibling();
        }

        public async UniTask MovePawnAsync(IReadOnlyList<CaseId> path, CancellationToken ct)
        {
            if (_move != null) Tween.Stop(_move);
            if (path.Count == 0 || _pawn == null) return;
            _animating = true;
            try
            {
                foreach (var step in path)
                {
                    var done = new UniTaskCompletionSource();
                    Vector2 from = _pawn.anchoredPosition, to = PawnPos(step);
                    _move = Tween.Run(0.2f, k =>
                    {
                        if (_pawn == null) return;
                        var p = Vector2.Lerp(from, to, k);
                        p.y += Mathf.Sin(k * Mathf.PI) * 34f;
                        _pawn.anchoredPosition = p;
                    }, () =>
                    {
                        if (_pawn != null) Tween.Punch(_pawn, 0.1f, 0.12f);
                        done.TrySetResult();
                    }, this, Tween.Linear);
                    await done.Task.AttachExternalCancellation(ct);
                }
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
