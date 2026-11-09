#nullable enable
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Reflow of the shared list/collection templates from the available safe viewport.</summary>
    [ExecuteAlways]
    public sealed class TacticalPageLayout : MonoBehaviour
    {
        Vector2 _size;
        int _state = -1;
        void OnEnable() => Reflow();
        void LateUpdate()
        {
            var size = ((RectTransform)transform).rect.size;
            var header = transform.Find("Header/Fill");
            int state = 0;
            if (header != null)
                foreach (Transform child in header) state = state * 31 + (child.gameObject.activeSelf ? 1 : 0);
            var detail = transform.Find("Detail/Fill");
            if (detail != null) foreach (Transform child in detail) state = state * 31 + (child.gameObject.activeSelf ? 1 : 0);
            if (size != _size || state != _state) { _state = state; Reflow(); }
        }

        public void Reflow()
        {
            _size = ((RectTransform)transform).rect.size;
            var header = transform.Find("Header/Fill");
            if (header != null)
            {
                float right = 18;
                foreach (var name in new[] { "ButtonA", "ButtonB", "Search" })
                {
                    var rt = header.Find(name) as RectTransform;
                    if (rt == null || !rt.gameObject.activeSelf) continue;
                    float width = name == "Search" ? 300 : 320;
                    rt.anchorMin = rt.anchorMax = new Vector2(1, .5f);
                    rt.pivot = new Vector2(1, .5f);
                    rt.sizeDelta = new Vector2(width, name == "Search" ? 80 : 96);
                    rt.anchoredPosition = new Vector2(-right, 0);
                    right += width + 14;
                }
                foreach (var name in new[] { "Title", "Line", "Bar" })
                {
                    var rt = header.Find(name) as RectTransform;
                    if (rt == null) continue;
                    float bottom = name == "Title" ? -64 : name == "Line" ? -106 : -138;
                    float top = name == "Title" ? -10 : name == "Line" ? -66 : -114;
                    rt.anchorMin = rt.anchorMax = Vector2.up;
                    rt.anchorMax = Vector2.one;
                    rt.pivot = new Vector2(0, 1);
                    rt.offsetMin = new Vector2(22, bottom);
                    rt.offsetMax = new Vector2(-right - 12, top);
                }
            }
            var grid = GetComponentInChildren<GridLayoutGroup>(true);
            if (grid != null && grid.transform.parent is RectTransform viewport)
            {
                float width = Mathf.Max(240, viewport.rect.width - grid.padding.horizontal);
                bool shop = GetTypeName().Contains("Boutique");
                int columns = Mathf.Clamp(Mathf.FloorToInt((width + 14) / (shop ? 310 : 250)), 1, shop ? 3 : 4);
                grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = columns;
                float cell = (width - (columns - 1) * grid.spacing.x) / columns;
                grid.cellSize = new Vector2(cell, shop ? Mathf.Min(380, cell * 1.15f) : Mathf.Min(300, cell * .92f));
            }
            var detail = transform.Find("Detail/Fill");
            if (detail != null)
            {
                int count = 0;
                foreach (Transform child in detail) if (child.name.StartsWith("Button") && child.gameObject.activeSelf) count++;
                if (count > 0)
                {
                    float width = (((RectTransform)detail).rect.width - 24 - (count - 1) * 12) / count;
                    int i = 0;
                    foreach (Transform child in detail)
                    {
                        if (!child.name.StartsWith("Button") || !child.gameObject.activeSelf) continue;
                        var rt = (RectTransform)child;
                        rt.anchorMin = rt.anchorMax = Vector2.zero; rt.pivot = Vector2.zero;
                        rt.sizeDelta = new Vector2(width, 96); rt.anchoredPosition = new Vector2(12 + i++ * (width + 12), 12);
                    }
                }
            }
        }
        string GetTypeName() => gameObject.name;
    }
}

