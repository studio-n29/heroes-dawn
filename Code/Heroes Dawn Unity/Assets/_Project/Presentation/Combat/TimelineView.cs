#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Presentation.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Combat
{
    /// <summary>
    /// Frise chronologique du combat (widget du prefab Combat) : de 100 (départ) à 0 (point d action : c est son tour), zone de préparation
    /// (les 20 derniers points avant 0) en rouge, alliés sous la ligne et ennemis au-dessus, avec l action annoncée.
    /// </summary>
    public sealed class TimelineView : MonoBehaviour
    {
        const float RowOffset = 26f;

        [SerializeField] RectTransform track = null!;
        [SerializeField] RectTransform prepZone = null!;
        [SerializeField] TimelineMarkerView markerPrefab = null!;

        readonly List<TimelineMarkerView> _markers = new List<TimelineMarkerView>();
        readonly Dictionary<int, TimelineMarkerView> _byId = new Dictionary<int, TimelineMarkerView>();

        public IReadOnlyList<TimelineMarkerView> Markers => _markers;

        public void Show(TimelineData d)
        {
            var rect = track.rect;
            prepZone.anchorMin = new Vector2(d.PrepStart01, 0f);
            int readyAllies = 0, readyEnemies = 0; // les unités prêtes s empilent à 100 % : on les étale vers la gauche
            for (int i = 0; i < d.Markers.Length; i++)
            {
                var m = d.Markers[i];
                if (!_byId.TryGetValue(m.Id, out var view))
                {
                    view = Instantiate(markerPrefab, track);
                    _byId[m.Id] = view;
                    _markers.Add(view);
                    view.name = (m.Ally ? "Ally_" : "Enemy_") + m.Id;
                }
                view.Show(m);
                var rt = view.Rect;
                rt.anchorMin = rt.anchorMax = new Vector2(Mathf.Clamp01(m.Position01), 0.5f);
                float spread = 0f;
                if (m.Alive && m.Position01 >= 0.999f) spread = -64f * (m.Ally ? readyAllies++ : readyEnemies++);
                rt.anchoredPosition = new Vector2(spread, m.Ally ? -RowOffset : RowOffset);
            }
        }

        /// <summary>Nouveau combat : on repart sans pastilles.</summary>
        public void Clear()
        {
            foreach (var m in _markers) if (m != null) Destroy(m.gameObject);
            _markers.Clear();
            _byId.Clear();
        }
    }
}
