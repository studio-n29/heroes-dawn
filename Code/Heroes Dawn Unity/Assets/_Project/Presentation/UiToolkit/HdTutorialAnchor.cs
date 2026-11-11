#nullable enable
using N29.HeroesDawn.Presentation.Tutorial;
using UnityEngine;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.UiToolkit
{
    /// <summary>
    /// Pont vers le tutoriel (encore en uGUI jusqu'au lot 8) : un rectangle invisible, doté d'un <see cref="TutorialTargetMarker"/>,
    /// suit l'élément UI Toolkit désigné pour que le tutoriel puisse le cerner. Il n'existe que tant que l'élément est visible.
    /// </summary>
    public sealed class HdTutorialAnchor : MonoBehaviour
    {
        VisualElement? _element;
        RectTransform? _parent;
        TutorialTargetMarker? _marker;

        public static HdTutorialAnchor Attach(Component host, VisualElement element, TutorialTarget id, string suffix = "")
        {
            // Un document recréé (ou une liste reconstruite) repose son ancre : l'ancienne, restée sur un élément détaché, disparaît.
            var previous = host.transform.Find("Anchor_" + id + suffix);
            if (previous != null) { previous.gameObject.SetActive(false); Destroy(previous.gameObject); }
            var go = new GameObject("Anchor_" + id + suffix, typeof(RectTransform));
            go.transform.SetParent(host.transform, false);
            var anchor = go.AddComponent<HdTutorialAnchor>();
            anchor._element = element;
            anchor._parent = (RectTransform)host.transform;
            anchor._marker = go.AddComponent<TutorialTargetMarker>();
            anchor._marker.Id = id;
            anchor._marker.enabled = false;
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            return anchor;
        }

        void LateUpdate()
        {
            if (_marker == null) return;
            if (_element == null || _element.panel == null || _parent == null) { if (_marker.enabled) _marker.enabled = false; return; }
            bool visible = _element.resolvedStyle.display != DisplayStyle.None && _element.resolvedStyle.visibility == Visibility.Visible
                           && _element.enabledInHierarchy && !float.IsNaN(_element.worldBound.width) && _element.worldBound.width > 0f && IsShown(_element);
            if (_marker.enabled != visible) _marker.enabled = visible;
            if (!visible) return;

            var panelSize = _element.panel.visualTree.layout.size;
            if (panelSize.x <= 0f || panelSize.y <= 0f) return;
            float fx = Screen.width / panelSize.x, fy = Screen.height / panelSize.y;
            var wb = _element.worldBound;
            var screenMin = new Vector2(wb.xMin * fx, Screen.height - wb.yMax * fy);
            var screenMax = new Vector2(wb.xMax * fx, Screen.height - wb.yMin * fy);
            var canvas = _parent.GetComponentInParent<Canvas>();
            var cam = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_parent, screenMin, cam, out var a)) return;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_parent, screenMax, cam, out var b)) return;
            var rt = (RectTransform)transform;
            rt.anchoredPosition = (a + b) * 0.5f - _parent.rect.center;
            rt.sizeDelta = new Vector2(Mathf.Abs(b.x - a.x), Mathf.Abs(b.y - a.y));
        }

        static bool IsShown(VisualElement e)
        {
            for (var p = e; p != null; p = p.parent)
                if (p.resolvedStyle.display == DisplayStyle.None) return false;
            return true;
        }
    }
}
