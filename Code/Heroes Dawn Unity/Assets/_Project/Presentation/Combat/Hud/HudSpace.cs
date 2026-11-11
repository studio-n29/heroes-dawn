#nullable enable
using UnityEngine;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Combat.Hud
{
    /// <summary>Passage entre les coordonnées du panneau UI Toolkit (y vers le bas) et celles de l'écran (y vers le haut, comme les événements et les tests).</summary>
    public static class HudSpace
    {
        static Vector2 Ratio(VisualElement e)
        {
            var size = e.panel != null ? e.panel.visualTree.layout.size : new Vector2(Screen.width, Screen.height);
            if (size.x <= 0f || size.y <= 0f) size = new Vector2(Screen.width, Screen.height);
            return new Vector2(Screen.width / size.x, Screen.height / size.y);
        }

        public static Vector2 PanelToScreen(VisualElement e, Vector2 panel)
        {
            var r = Ratio(e);
            return new Vector2(panel.x * r.x, Screen.height - panel.y * r.y);
        }

        public static Vector2 ScreenToPanel(VisualElement e, Vector2 screen)
        {
            var r = Ratio(e);
            return new Vector2(screen.x / r.x, (Screen.height - screen.y) / r.y);
        }
    }
}
