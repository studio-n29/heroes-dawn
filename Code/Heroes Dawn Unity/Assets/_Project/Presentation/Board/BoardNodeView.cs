#nullable enable
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Tutorial;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Board
{
    /// <summary>Case du plateau (widget prefab) : disque coloré par type, glyphe, rang, objectif, destination et coût.</summary>
    public sealed class BoardNodeView : MonoBehaviour
    {
        [SerializeField] Image highlight = null!;
        [SerializeField] Image objective = null!;
        [SerializeField] Image frame = null!;
        [SerializeField] Image disc = null!;
        [SerializeField] TMP_Text label = null!;
        [SerializeField] TMP_Text sub = null!;
        [SerializeField] TMP_Text objectiveTag = null!;
        [SerializeField] GameObject costBadge = null!;
        [SerializeField] TMP_Text cost = null!;
        [SerializeField] Button button = null!;

        TutorialTargetMarker? _marker;

        public Button Button => button;

        void Awake() => _marker = frame.GetComponent<TutorialTargetMarker>();

        public void Init(BoardMapNode n)
        {
            objective.gameObject.SetActive(n.IsObjective);
            objectiveTag.gameObject.SetActive(n.IsObjective);
            highlight.gameObject.SetActive(false);
            costBadge.SetActive(false);
        }

        public void Apply(BoardNodeState s)
        {
            if (!s.Revealed)
            {
                disc.color = Theme.WithAlpha(Theme.Surface2, 0.55f);
                frame.color = Theme.WithAlpha(Theme.Ink, 0.5f);
                label.text = "?";
                label.color = Theme.WithAlpha(Theme.TextMuted, 0.6f);
                sub.text = "";
            }
            else
            {
                var c = Theme.CaseColor(s.Type, s.Rank);
                if (s.Spent) c = Color.Lerp(c, Theme.Surface3, 0.65f);
                disc.color = c;
                frame.color = Theme.Ink;
                label.color = Color.white;
                label.text = s.Glyph;
                sub.text = s.Sub;
                sub.color = s.SubElite ? Theme.Primary : Theme.TextMuted;
            }
            if (objective.gameObject.activeSelf)
            {
                objective.color = s.ObjectiveDone ? Theme.Success : Theme.Primary;
                objectiveTag.color = s.ObjectiveDone ? Theme.Success : Theme.Primary;
                objectiveTag.text = s.ObjectiveDone ? "OK" : "OBJ";
            }
            bool dest = s.DestinationCost >= 0;
            highlight.gameObject.SetActive(dest);
            if (_marker != null) _marker.enabled = dest; // le tutoriel désigne les cases à portée
            costBadge.SetActive(dest);
            if (dest) cost.text = s.DestinationCost.ToString();
        }
    }
}
