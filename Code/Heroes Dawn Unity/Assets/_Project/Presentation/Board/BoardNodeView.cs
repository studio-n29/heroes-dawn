#nullable enable
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Tutorial;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Board
{
    /// <summary>
    /// Case du plateau (widget prefab) : disque coloré par type, glyphe, rang, objectif, surbrillance de déplacement
    /// (destination, case cachée à portée, chemin, cible), coût et anneau de navigation (case sélectionnée au clavier).
    /// </summary>
    public sealed class BoardNodeView : MonoBehaviour
    {
        [SerializeField] Image highlight = null!;
        [SerializeField] Image focus = null!;
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
        bool _pulse;

        public Button Button => button;

        void Awake() => _marker = frame.GetComponent<TutorialTargetMarker>();

        /// <summary>Contour jaune des cases accessibles : il pulse doucement (fixe en mouvement réduit).</summary>
        void Update()
        {
            if (!_pulse || !highlight.gameObject.activeSelf) return;
            if (Feedback.MotionSettings.Reduced) { highlight.transform.localScale = Vector3.one; return; }
            float k = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 5f);
            highlight.transform.localScale = Vector3.one * (1f + 0.08f * k);
            var c = highlight.color; c.a = 0.65f + 0.35f * k; highlight.color = c;
        }

        public void Init(BoardMapNode n)
        {
            objective.gameObject.SetActive(n.IsObjective);
            objectiveTag.gameObject.SetActive(n.IsObjective);
            highlight.gameObject.SetActive(false);
            focus.gameObject.SetActive(false);
            costBadge.SetActive(false);
        }

        /// <summary>Anneau de navigation case à case (clavier / manette).</summary>
        public void SetFocused(bool on)
        {
            if (focus.gameObject.activeSelf != on) focus.gameObject.SetActive(on);
        }

        public void Apply(BoardNodeState s)
        {
            bool fogReach = s.Mark == NodeMark.FogDestination;
            if (!s.Revealed)
            {
                disc.color = Theme.WithAlpha(Theme.Surface2, fogReach ? 0.85f : 0.55f);
                frame.color = fogReach ? Theme.Info : Theme.WithAlpha(Theme.Ink, 0.5f);
                label.text = "?";
                label.color = fogReach ? Theme.Info : Theme.WithAlpha(Theme.TextMuted, 0.6f);
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
            bool marked = s.Mark != NodeMark.None;
            highlight.gameObject.SetActive(marked);
            if (marked) highlight.color = HighlightColor(s.Mark);
            _pulse = s.Mark == NodeMark.Destination;
            if (!_pulse) highlight.transform.localScale = Vector3.one;
            bool real = s.Mark == NodeMark.Destination || s.Mark == NodeMark.Target;
            if (_marker != null) _marker.enabled = s.Mark == NodeMark.Destination; // le tutoriel désigne les cases à portée
            bool badge = s.DestinationCost >= 0 && (real || fogReach);
            costBadge.SetActive(badge);
            if (badge) cost.text = s.DestinationCost.ToString();
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
}
