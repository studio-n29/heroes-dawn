#nullable enable
using N29.HeroesDawn.Presentation.Combat;
using UnityEngine;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.UiToolkit
{
    /// <summary>Lignes communes des fenêtres UI Toolkit (fatigue et expérience avant → après).</summary>
    public static class HdPopupRows
    {
        public static void FillFatigue(VisualElement host, FatigueRowData[]? rows)
        {
            host.Clear();
            if (rows == null) return;
            foreach (var r in rows)
            {
                var row = new VisualElement { pickingMode = PickingMode.Ignore };
                row.AddToClassList("hd-poprow");
                var name = new Label(r.Name) { pickingMode = PickingMode.Ignore };
                name.AddToClassList("hd-poprow__name");
                var bar = new VisualElement { pickingMode = PickingMode.Ignore };
                bar.AddToClassList("hd-bar"); bar.AddToClassList("hd-poprow__bar");
                var fill = new VisualElement { pickingMode = PickingMode.Ignore };
                fill.AddToClassList("hd-bar__fill");
                fill.style.width = Length.Percent(Mathf.Clamp01(r.After01) * 100f);
                fill.style.unityBackgroundImageTintColor = Common.Theme.FatigueColor(r.After);
                bar.Add(fill);
                var label = new Label(string.IsNullOrEmpty(r.Delta) ? r.Label : r.Label + "  " + r.Delta) { pickingMode = PickingMode.Ignore };
                label.AddToClassList("hd-poprow__label");
                row.Add(name); row.Add(bar); row.Add(label);
                host.Add(row);
            }
        }

        /// <summary>Illustration en fond d'un élément (ou aucun fond).</summary>
        public static void SetArt(VisualElement element, Sprite? art)
            => element.style.backgroundImage = art != null ? new StyleBackground(art) : new StyleBackground(StyleKeyword.Null);
    }
}
