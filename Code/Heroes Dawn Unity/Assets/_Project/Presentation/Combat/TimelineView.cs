#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Presentation.Common;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Combat
{
    /// <summary>
    /// Frise chronologique du combat par points (widget du prefab Combat) : une ligne de segments, du point 0 (départ, à gauche)
    /// au point d'action (à droite). Chaque segment est vide ou porte une ou plusieurs unités (alliés sous la ligne, ennemis au-dessus).
    /// Les pastilles glissent d'un point à l'autre en 0,5 s ; les segments de la zone de préparation sont teintés de rouge.
    /// </summary>
    public sealed class TimelineView : MonoBehaviour
    {
        const float RowOffset = 44f;
        const float SpreadStep = 46f;
        /// <summary>Durée d'un pas de la frise, en secondes (un point toutes les 0,5 s).</summary>
        public const float StepDurationSeconds = 0.5f;

        [SerializeField] RectTransform track = null!;
        [SerializeField] RectTransform prepZone = null!;
        [SerializeField] TimelineMarkerView markerPrefab = null!;

        /// <summary>Un segment de la frise : un point, vide ou occupé.</summary>
        public sealed class Segment
        {
            public int Point;
            public RectTransform Rect = null!;
            public Image Fill = null!;
            public readonly List<int> Occupants = new List<int>();
            public bool IsEmpty => Occupants.Count == 0;
        }

        readonly List<TimelineMarkerView> _markers = new List<TimelineMarkerView>();
        readonly Dictionary<int, TimelineMarkerView> _byId = new Dictionary<int, TimelineMarkerView>();
        readonly Dictionary<int, float> _targets = new Dictionary<int, float>();
        readonly List<Segment> _segments = new List<Segment>();
        int _points;
        int _prepStartPoint;

        public IReadOnlyList<TimelineMarkerView> Markers => _markers;
        public IReadOnlyList<Segment> Segments => _segments;
        public float StepDuration => StepDurationSeconds;

        public Segment SegmentOf(int point) => _segments[point];

        void BuildSegments(int points, int prepStartPoint)
        {
            foreach (var s in _segments) if (s.Rect != null) Destroy(s.Rect.gameObject);
            _segments.Clear();
            _points = points;
            _prepStartPoint = prepStartPoint;
            for (int p = 0; p <= points; p++)
            {
                var go = new GameObject("Segment_" + p, typeof(RectTransform), typeof(Image));
                var rt = (RectTransform)go.transform;
                rt.SetParent(track, false);
                rt.SetAsFirstSibling();
                float x = p / (float)points;
                rt.anchorMin = new Vector2(x, 0f);
                rt.anchorMax = new Vector2(x, 1f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = new Vector2(6f, 64f);
                var img = go.GetComponent<Image>();
                img.raycastTarget = false;
                img.color = SegmentColor(p, false);
                _segments.Add(new Segment { Point = p, Rect = rt, Fill = img });
            }
        }

        Color SegmentColor(int point, bool occupied)
        {
            bool prep = point >= _prepStartPoint;
            if (point == _points) return occupied ? Theme.Primary : Theme.WithAlpha(Theme.Primary, 0.55f);
            if (occupied) return prep ? Theme.Danger : Theme.Info;
            return prep ? Theme.WithAlpha(Theme.Danger, 0.35f) : Theme.WithAlpha(Theme.TextMuted, 0.35f);
        }

        public void Show(TimelineData d)
        {
            int points = Mathf.Max(1, d.Points);
            int prepPoint = Mathf.RoundToInt(d.PrepStart01 * points);
            if (_segments.Count != points + 1 || _prepStartPoint != prepPoint) BuildSegments(points, prepPoint);
            prepZone.anchorMin = new Vector2(d.PrepStart01, 0f);
            foreach (var s in _segments) s.Occupants.Clear();

            var seen = new Dictionary<(bool, int), int>();
            for (int i = 0; i < d.Markers.Length; i++)
            {
                var m = d.Markers[i];
                if (!_byId.TryGetValue(m.Id, out var view))
                {
                    view = Instantiate(markerPrefab, track);
                    _byId[m.Id] = view;
                    _markers.Add(view);
                    view.name = (m.Ally ? "Ally_" : "Enemy_") + m.Id;
                    var first = view.Rect;
                    first.anchorMin = first.anchorMax = new Vector2(Mathf.Clamp01(m.Position01), 0.5f);
                }
                view.Show(m);
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
                _targets[m.Id] = clamped / (float)points;
                var rt = view.Rect;
                rt.anchoredPosition = new Vector2(rank * SpreadStep, m.Ally ? -RowOffset : RowOffset);
            }
            foreach (var s in _segments) s.Fill.color = SegmentColor(s.Point, !s.IsEmpty);
        }

        void Update()
        {
            // Les pastilles glissent vers leur point : un point par pas, plus vite pour un grand saut (reprise, recul).
            float stepSpeed = (1f / Mathf.Max(1, _points)) / StepDurationSeconds;
            foreach (var kv in _targets)
            {
                if (!_byId.TryGetValue(kv.Key, out var view)) continue;
                var rt = view.Rect;
                float current = rt.anchorMin.x;
                float gap = Mathf.Abs(kv.Value - current);
                if (gap < 0.0005f) continue;
                float speed = Mathf.Max(stepSpeed, gap * 4f);
                float next = Mathf.MoveTowards(current, kv.Value, speed * Time.unscaledDeltaTime);
                rt.anchorMin = rt.anchorMax = new Vector2(next, 0.5f);
            }
        }

        /// <summary>Nouveau combat : on repart sans pastilles.</summary>
        public void Clear()
        {
            foreach (var m in _markers) if (m != null) Destroy(m.gameObject);
            _markers.Clear();
            _byId.Clear();
            _targets.Clear();
        }
    }
}
