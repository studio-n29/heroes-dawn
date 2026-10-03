#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Presentation.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Board
{
    /// <summary>Carte de la zone : cases, liens, brouillard, pion, destinations. Aucune règle ici : tout vient du ZoneRun.</summary>
    public sealed class BoardMapView : MonoBehaviour
    {
        const float StepX = 106f;
        const float StepY = 190f;
        const float NodeSize = 86f;

        sealed class NodeView
        {
            public NodeDef Def = null!;
            public RectTransform Root = null!;
            public Image Disc = null!;
            public Image Frame = null!;
            public TMP_Text Label = null!;
            public TMP_Text Sub = null!;
            public Image Highlight = null!;
            public TMP_Text Cost = null!;
            public Image Objective = null!;
            public TMP_Text ObjectiveTag = null!;
        }

        sealed class LinkView
        {
            public CaseId A;
            public CaseId B;
            public Image Line = null!;
        }

        readonly Dictionary<CaseId, NodeView> _nodes = new Dictionary<CaseId, NodeView>();
        readonly List<LinkView> _links = new List<LinkView>();
        RectTransform _rt = null!;
        RectTransform _linkLayer = null!;
        RectTransform _nodeLayer = null!;
        RectTransform _pawn = null!;
        ZoneRun? _built;
        Coroutine? _move;

        public event Action<CaseId>? NodeTapped;
        public bool Animating { get; private set; }

        public static BoardMapView Create(RectTransform parent)
        {
            var rt = Ui.Rect(parent, "Map");
            var v = rt.gameObject.AddComponent<BoardMapView>();
            v._rt = rt;
            v._linkLayer = Ui.Rect(rt, "Links").Stretch();
            v._nodeLayer = Ui.Rect(rt, "Nodes").Stretch();
            return v;
        }

        public static Vector2 PosOf(NodeDef n) => new Vector2((n.X - 8) * StepX, (n.Y - 2) * StepY);

        public void Build(ZoneRun run, Family leaderFamily, string leaderName)
        {
            if (_built == run) return;
            _built = run;
            _linkLayer.Clear();
            _nodeLayer.Clear();
            _nodes.Clear();
            _links.Clear();

            foreach (var l in run.Graph.Zone.Links)
            {
                var a = PosOf(run.Graph.Node(l.A));
                var b = PosOf(run.Graph.Node(l.B));
                var line = Ui.Img(_linkLayer, "Link", Theme.Surface3);
                var lrt = line.rectTransform;
                lrt.anchorMin = lrt.anchorMax = new Vector2(0.5f, 0.5f);
                lrt.pivot = new Vector2(0.5f, 0.5f);
                lrt.anchoredPosition = (a + b) * 0.5f;
                lrt.sizeDelta = new Vector2(Vector2.Distance(a, b), 12f);
                lrt.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg);
                var outline = line.gameObject.AddComponent<Outline>();
                outline.effectColor = Theme.Ink;
                outline.effectDistance = new Vector2(2, 2);
                _links.Add(new LinkView { A = l.A, B = l.B, Line = line });
            }

            var objectiveTargets = new HashSet<CaseId>();
            foreach (var o in run.Graph.Zone.Objectives) objectiveTargets.Add(o.Target);

            foreach (var id in run.Graph.AllIds)
            {
                var def = run.Graph.Node(id);
                var nv = new NodeView { Def = def };
                nv.Root = Ui.Rect(_nodeLayer, "Node_" + id.Value).At(0.5f, 0.5f, 0, 0, NodeSize + 40, NodeSize + 40);
                nv.Root.anchoredPosition = PosOf(def);

                nv.Highlight = Ui.Img(nv.Root, "Highlight", Theme.Primary, Sprites.Ring);
                nv.Highlight.rectTransform.At(0.5f, 0.5f, 0, 0, NodeSize + 36, NodeSize + 36);
                var pulse = nv.Highlight.gameObject.AddComponent<Pulse>();
                pulse.Amplitude = 0.1f;
                nv.Highlight.gameObject.SetActive(false);

                nv.Objective = Ui.Img(nv.Root, "Objective", Theme.Primary, Sprites.Ring);
                nv.Objective.rectTransform.At(0.5f, 0.5f, 0, 0, NodeSize + 18, NodeSize + 18);
                nv.Objective.gameObject.SetActive(objectiveTargets.Contains(id));

                nv.Frame = Ui.Img(nv.Root, "Frame", Theme.Ink, Sprites.Circle, true);
                nv.Frame.rectTransform.At(0.5f, 0.5f, 0, 0, NodeSize, NodeSize);
                var captured = id;
                nv.Frame.MakeClickable(() => NodeTapped?.Invoke(captured));
                nv.Disc = Ui.Img(nv.Frame.transform, "Disc", Theme.Surface3, Sprites.Circle);
                nv.Disc.rectTransform.Stretch(5, 5, 5, 5);
                nv.Label = Ui.Txt(nv.Disc.transform, "", 34, Color.white, TextAnchor.MiddleCenter, FontKind.Title, "Glyph");
                nv.Label.rectTransform.Stretch();
                nv.Label.Inked(2f);

                nv.Sub = Ui.Txt(nv.Root, "", 20, Theme.TextMuted, TextAnchor.UpperCenter, FontKind.Body, "Sub");
                nv.Sub.rectTransform.At(0.5f, 0f, 0, -14, 160, 26);
                nv.Sub.Inked(1.2f);

                nv.ObjectiveTag = Ui.Txt(nv.Root, "OBJ", 20, Theme.Primary, TextAnchor.LowerCenter, FontKind.Title, "ObjTag");
                nv.ObjectiveTag.rectTransform.At(0.5f, 1f, 0, 12, 120, 26);
                nv.ObjectiveTag.Inked(1.5f);
                nv.ObjectiveTag.gameObject.SetActive(objectiveTargets.Contains(id));

                var costBadge = Ui.Img(nv.Root, "CostBadge", Theme.Primary, Sprites.Circle);
                costBadge.rectTransform.At(1f, 1f, -2, -2, 40, 40);
                nv.Cost = Ui.Txt(costBadge.transform, "", 24, Theme.TextDark, TextAnchor.MiddleCenter, FontKind.Title, "Cost");
                nv.Cost.rectTransform.Stretch();
                costBadge.gameObject.SetActive(false);

                _nodes.Add(id, nv);
            }

            // Pion : portrait du chef d'escouade, flottant au-dessus de la case.
            var pawn = Portrait.Create(_nodeLayer, leaderName, leaderFamily, 70);
            _pawn = pawn.rectTransform;
            _pawn.anchorMin = _pawn.anchorMax = new Vector2(0.5f, 0.5f);
            _pawn.pivot = new Vector2(0.5f, 0f);
            var shadow = pawn.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.6f);
            shadow.effectDistance = new Vector2(6, -6);
            _pawn.anchoredPosition = PawnPos(run.Position);
        }

        Vector2 PawnPos(CaseId id) => PosOf(_nodes[id].Def) + new Vector2(0, 22);

        public void Refresh(ZoneRun run, bool movePawn = true)
        {
            var doneIds = new HashSet<string>(run.CompletedObjectives);
            var doneTargets = new HashSet<CaseId>();
            foreach (var o in run.Graph.Zone.Objectives) if (doneIds.Contains(o.Id)) doneTargets.Add(o.Target);

            foreach (var kv in _nodes)
            {
                var id = kv.Key;
                var nv = kv.Value;
                bool revealed = run.Fog.IsRevealed(id);
                bool spent = run.IsSpent(id);
                bool dest = run.Phase == ZoneRunPhase.ChoosingDestination && run.Destinations.ContainsKey(id);
                var def = nv.Def;

                if (!revealed)
                {
                    nv.Disc.color = Theme.WithAlpha(Theme.Surface2, 0.55f);
                    nv.Frame.color = Theme.WithAlpha(Theme.Ink, 0.5f);
                    nv.Label.text = "?";
                    nv.Label.color = Theme.WithAlpha(Theme.TextMuted, 0.6f);
                    nv.Sub.text = "";
                }
                else
                {
                    var c = Theme.CaseColor(def.Type, def.Rank);
                    if (spent) c = Color.Lerp(c, Theme.Surface3, 0.65f);
                    nv.Disc.color = c;
                    nv.Frame.color = Theme.Ink;
                    nv.Label.color = Color.white;
                    nv.Label.text = Glyph(def.Type, spent);
                    nv.Sub.text = def.Type == CaseType.Combat && !spent ? Loc.RankName(def.Rank) : "";
                    nv.Sub.color = def.Rank == CombatRank.Elite ? Theme.Primary : Theme.TextMuted;
                }

                bool isObj = nv.Objective.gameObject.activeSelf;
                if (isObj)
                {
                    bool done = doneTargets.Contains(id);
                    nv.Objective.color = done ? Theme.Success : Theme.Primary;
                    nv.ObjectiveTag.color = done ? Theme.Success : Theme.Primary;
                    nv.ObjectiveTag.text = done ? "OK" : "OBJ";
                }

                nv.Highlight.gameObject.SetActive(dest);
                nv.Cost.transform.parent.gameObject.SetActive(dest);
                if (dest) nv.Cost.text = run.Destinations[id].Cost.ToString();
            }

            foreach (var l in _links)
            {
                bool a = run.Fog.IsRevealed(l.A), b = run.Fog.IsRevealed(l.B);
                l.Line.gameObject.SetActive(a || b);
                l.Line.color = a && b ? Theme.Surface3 : Theme.WithAlpha(Theme.Surface3, 0.3f);
            }

            if (movePawn && !Animating && _pawn != null) _pawn.anchoredPosition = PawnPos(run.Position);
            if (_pawn != null) _pawn.SetAsLastSibling();
        }

        static string Glyph(CaseType t, bool spent)
        {
            switch (t)
            {
                case CaseType.Entry: return "QG";
                case CaseType.Combat: return spent ? "OK" : "VS";
                case CaseType.MiniGame: return spent ? "OK" : "MJ";
                case CaseType.Narrative: return spent ? "OK" : "!";
                case CaseType.Resource: return "$";
                case CaseType.Rest: return spent ? "OK" : "Zz";
                default: return "";
            }
        }

        /// <summary>Déplace le pion case par case (petits bonds), puis appelle <paramref name="done"/>.</summary>
        public void AnimatePath(IReadOnlyList<CaseId> path, Action done)
        {
            if (_move != null) Tween.Stop(_move);
            if (path.Count == 0 || _pawn == null) { done(); return; }
            Animating = true;
            StepAlong(path, 0, done);
        }

        void StepAlong(IReadOnlyList<CaseId> path, int index, Action done)
        {
            if (this == null) return;
            if (index >= path.Count)
            {
                Animating = false;
                done();
                return;
            }
            Vector2 from = _pawn.anchoredPosition;
            Vector2 to = PawnPos(path[index]);
            _move = Tween.Run(0.2f, k =>
            {
                if (_pawn == null) return;
                var p = Vector2.Lerp(from, to, k);
                p.y += Mathf.Sin(k * Mathf.PI) * 34f;
                _pawn.anchoredPosition = p;
            }, () =>
            {
                if (_pawn != null) Tween.Punch(_pawn, 0.1f, 0.12f);
                StepAlong(path, index + 1, done);
            }, this, Tween.Linear);
        }

        public void JumpPawn(CaseId to)
        {
            if (_pawn != null) _pawn.anchoredPosition = PawnPos(to);
        }
    }
}
