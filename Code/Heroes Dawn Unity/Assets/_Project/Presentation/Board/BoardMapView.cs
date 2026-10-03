#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Presentation.Common;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Board
{
    /// <summary>
    /// Carte du plateau : instancie les cases (BoardNode) et les liens (BoardLink) d'après les données de la partie,
    /// place le pion (Pawn) et l'anime case par case. Ne calcule jamais une règle.
    /// </summary>
    public sealed class BoardMapView : MonoBehaviour, IBoardMapView
    {
        const float StepX = 106f;
        const float StepY = 190f;

        [SerializeField] RectTransform linkLayer = null!;
        [SerializeField] RectTransform nodeLayer = null!;
        [SerializeField] BoardNodeView nodePrefab = null!;
        [SerializeField] Image linkPrefab = null!;
        [SerializeField] PortraitView pawnPrefab = null!;

        readonly Dictionary<CaseId, BoardNodeView> _nodes = new Dictionary<CaseId, BoardNodeView>();
        readonly Dictionary<CaseId, Vector2> _positions = new Dictionary<CaseId, Vector2>();
        readonly List<Image> _links = new List<Image>();
        RectTransform? _pawn;
        Coroutine? _move;
        bool _animating;

        public event Action<CaseId>? NodeTapped;

        /// <summary>Bouton d'une case (tests PlayMode : toucher réel).</summary>
        public Button NodeButton(CaseId id) => _nodes[id].Button;

        static Vector2 PosOf(BoardMapNode n) => new Vector2((n.X - 8) * StepX, (n.Y - 2) * StepY);

        public void Build(BoardMapData d)
        {
            linkLayer.Clear();
            nodeLayer.Clear();
            _nodes.Clear();
            _positions.Clear();
            _links.Clear();
            foreach (var n in d.Nodes) _positions[n.Id] = PosOf(n);
            foreach (var l in d.Links)
            {
                Vector2 a = _positions[l.A], b = _positions[l.B];
                var line = Instantiate(linkPrefab, linkLayer);
                var rt = line.rectTransform;
                rt.anchoredPosition = (a + b) * 0.5f;
                rt.sizeDelta = new Vector2(Vector2.Distance(a, b), 12f);
                rt.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg);
                _links.Add(line);
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
            _pawn.anchoredPosition = PawnPos(d.PawnAt);
        }

        Vector2 PawnPos(CaseId id) => _positions[id] + new Vector2(0, 22);

        public void Refresh(IReadOnlyList<BoardNodeState> nodes, IReadOnlyList<BoardLinkState> links, CaseId pawnAt, bool movePawn)
        {
            foreach (var s in nodes)
                if (_nodes.TryGetValue(s.Id, out var n)) n.Apply(s);
            for (int i = 0; i < links.Count && i < _links.Count; i++)
            {
                _links[i].gameObject.SetActive(links[i].Visible);
                _links[i].color = links[i].Dim ? Theme.WithAlpha(Theme.Surface3, 0.3f) : Theme.Surface3;
            }
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
    }
}
