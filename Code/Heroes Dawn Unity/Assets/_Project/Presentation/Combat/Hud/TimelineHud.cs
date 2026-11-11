#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Presentation.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Combat.Hud
{
    /// <summary>
    /// Frise chronologique du combat par points (UI Toolkit) : une ligne de segments, du point 0 (départ, à gauche) au point d'action (à droite).
    /// Chaque segment est vide ou porte une ou plusieurs unités (alliés sous la ligne, ennemis au-dessus). Les pastilles glissent d'un point à
    /// l'autre en 0,5 s ; les segments de la zone de préparation sont teintés de rouge.
    /// </summary>
    public sealed class TimelineHud
    {
        const float RowOffset = 18f;
        const float SpreadStep = 34f;
        /// <summary>Durée d'un pas de la frise, en secondes (un point toutes les 0,5 s).</summary>
        public const float StepDurationSeconds = 0.5f;

        /// <summary>Un segment de la frise : un point, vide ou occupé.</summary>
        public sealed class Segment
        {
            public int Point;
            public VisualElement Element = null!;
            public readonly List<int> Occupants = new List<int>();
            public bool IsEmpty => Occupants.Count == 0;
        }

        /// <summary>Pastille d'une unité sur la frise : losange de famille, initiale, anneau quand elle est prête, annonce de l'action.</summary>
        public sealed class Marker
        {
            public int Id;
            public bool Ally;
            public VisualElement Root = null!, Frame = null!, Disc = null!, Ring = null!;
            public Label Initial = null!, Plan = null!;
            public float Current;
            public float Target;
            public TimelineMarkerData Data;
            public bool WasReady;
        }

        readonly VisualElement _track, _prep, _fxLayer;
        readonly List<Segment> _segments = new List<Segment>();
        readonly List<Marker> _markers = new List<Marker>();
        readonly Dictionary<int, Marker> _byId = new Dictionary<int, Marker>();
        int _points, _prepStartPoint;

        public TimelineHud(VisualElement track, VisualElement prep, VisualElement fxLayer) { _track = track; _prep = prep; _fxLayer = fxLayer; }

        public IReadOnlyList<Segment> Segments => _segments;
        public IReadOnlyList<Marker> Markers => _markers;
        public float StepDuration => StepDurationSeconds;
        public Segment SegmentOf(int point) => _segments[point];

        void BuildSegments(int points, int prepStartPoint)
        {
            foreach (var s in _segments) s.Element.RemoveFromHierarchy();
            _segments.Clear();
            _points = points; _prepStartPoint = prepStartPoint;
            for (int p = 0; p <= points; p++)
            {
                var e = new VisualElement { pickingMode = PickingMode.Ignore };
                e.AddToClassList("hd-tl__seg");
                e.style.left = Length.Percent(p / (float)points * 100f);
                e.style.backgroundColor = SegmentColor(p, false);
                _track.Insert(0, e);
                _segments.Add(new Segment { Point = p, Element = e });
            }
        }

        Color SegmentColor(int point, bool occupied)
        {
            bool prep = point >= _prepStartPoint;
            if (point == _points) return occupied ? Theme.Primary : Theme.WithAlpha(Theme.Primary, 0.55f);
            if (occupied) return prep ? Theme.Danger : Theme.Info;
            return prep ? Theme.WithAlpha(Theme.Danger, 0.35f) : Theme.WithAlpha(Theme.TextMuted, 0.35f);
        }

        Marker Create(TimelineMarkerData m)
        {
            var root = new VisualElement { pickingMode = PickingMode.Ignore };
            root.AddToClassList("hd-tl__marker");
            var ring = new VisualElement { pickingMode = PickingMode.Ignore }; ring.AddToClassList("hd-tl__ring");
            var frame = new VisualElement { pickingMode = PickingMode.Ignore }; frame.AddToClassList("hd-tl__frame");
            var disc = new VisualElement { pickingMode = PickingMode.Ignore }; disc.AddToClassList("hd-tl__disc");
            frame.Add(disc);
            var initial = new Label { pickingMode = PickingMode.Ignore }; initial.AddToClassList("hd-tl__initial");
            var plan = new Label { pickingMode = PickingMode.Ignore }; plan.AddToClassList("hd-tl__plan");
            root.Add(ring); root.Add(frame); root.Add(initial); root.Add(plan);
            root.name = (m.Ally ? "Ally_" : "Enemy_") + m.Id;
            _track.Add(root);
            return new Marker { Id = m.Id, Ally = m.Ally, Root = root, Frame = frame, Disc = disc, Ring = ring, Initial = initial, Plan = plan, Current = Mathf.Clamp01(m.Position01) };
        }

        public void Show(TimelineData d)
        {
            int points = Mathf.Max(1, d.Points);
            int prepPoint = Mathf.RoundToInt(d.PrepStart01 * points);
            if (_segments.Count != points + 1 || _prepStartPoint != prepPoint) BuildSegments(points, prepPoint);
            _prep.style.left = Length.Percent(d.PrepStart01 * 100f);
            foreach (var s in _segments) s.Occupants.Clear();

            var seen = new Dictionary<(bool, int), int>();
            for (int i = 0; i < d.Markers.Length; i++)
            {
                var m = d.Markers[i];
                if (!_byId.TryGetValue(m.Id, out var mk)) { mk = Create(m); _byId[m.Id] = mk; _markers.Add(mk); }
                Apply(mk, m);
                if (m.Ready && m.Alive && !mk.WasReady) HudAnim.Sparks(_fxLayer, mk.Root, m.Ally ? Theme.Glow : Theme.AccentSoft, 8, 70f, 12f);
                mk.WasReady = m.Ready;
                int clamped = Mathf.Clamp(m.Point, 0, points);
                if (m.Alive) _segments[clamped].Occupants.Add(m.Id);
                // Plusieurs unités sur un même point : elles s'écartent horizontalement, l'ordre des identifiants.
                int rank = 0;
                if (m.Alive)
                {
                    var key = (m.Ally, clamped);
                    seen.TryGetValue(key, out rank);
                    seen[key] = rank + 1;
                }
                mk.Target = clamped / (float)points;
                HudAnim.SetTranslate(mk.Root, rank * SpreadStep, m.Ally ? RowOffset : -RowOffset);
            }
            foreach (var s in _segments) s.Element.style.backgroundColor = SegmentColor(s.Point, !s.IsEmpty);
        }

        static void Apply(Marker mk, TimelineMarkerData d)
        {
            mk.Data = d;
            mk.Disc.style.backgroundColor = d.Ally ? Theme.FamilyColor(d.Family) : Color.Lerp(Theme.HpEnemy, Theme.Navy, 0.15f);
            mk.Initial.text = d.Initial;
            bool ring = d.Ready || d.InPrep;
            mk.Ring.style.display = ring ? DisplayStyle.Flex : DisplayStyle.None;
            var rc = d.Ready ? Color.white : d.Ally ? Theme.Primary : Theme.Danger;
            mk.Ring.style.borderTopColor = mk.Ring.style.borderBottomColor = mk.Ring.style.borderLeftColor = mk.Ring.style.borderRightColor = rc;
            bool planned = d.PlannedSlot >= 0;
            mk.Plan.style.display = planned ? DisplayStyle.Flex : DisplayStyle.None;
            if (planned)
            {
                mk.Plan.text = d.PlannedSlot == 2 ? "A3 !!" : "A" + (d.PlannedSlot + 1) + " !";
                mk.Plan.style.color = d.PlannedSlot == 2 ? Theme.Danger : Theme.Primary;
            }
            mk.Root.style.opacity = !d.Alive ? 0f : d.Channelling ? 0.55f : 1f;
            HudAnim.SetScale(mk.Root, d.Boss ? 1.2f : 1f);
            mk.Root.style.left = Length.Percent(mk.Current * 100f);
        }

        /// <summary>Chaque image : les pastilles glissent vers leur point, un point par pas, plus vite pour un grand saut (reprise, recul).</summary>
        public void Tick()
        {
            float stepSpeed = (1f / Mathf.Max(1, _points)) / StepDurationSeconds;
            foreach (var mk in _markers)
            {
                float gap = Mathf.Abs(mk.Target - mk.Current);
                if (gap < 0.0005f) continue;
                float speed = Mathf.Max(stepSpeed, gap * 4f);
                mk.Current = Mathf.MoveTowards(mk.Current, mk.Target, speed * Time.unscaledDeltaTime);
                mk.Root.style.left = Length.Percent(mk.Current * 100f);
            }
        }

        /// <summary>Nouveau combat : on repart sans pastilles.</summary>
        public void Clear()
        {
            foreach (var m in _markers) m.Root.RemoveFromHierarchy();
            _markers.Clear(); _byId.Clear();
        }
    }
}
