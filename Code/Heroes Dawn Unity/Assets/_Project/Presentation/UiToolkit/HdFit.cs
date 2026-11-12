#nullable enable
using UnityEngine;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.UiToolkit
{
    /// <summary>
    /// Ajustement d'une mise en page conçue pour 1920×1080 (positions en pixels de référence) à un panneau plus court ou plus étroit :
    /// l'élément est dessiné à la taille de référence agrandie dans l'autre axe, puis réduit pour tenir dans le panneau (jamais agrandi).
    /// Sans effet sur un panneau d'au moins 1920×1080 logiques ou de proportions 16:9.
    /// </summary>
    public static class HdFit
    {
        public const float ReferenceWidth = 1920f, ReferenceHeight = 1080f;

        /// <summary>Facteur de réduction (≤ 1) pour que 1920×1080 tienne dans un panneau de la taille donnée.</summary>
        public static float Scale(Vector2 panelSize)
            => panelSize.x <= 0f || panelSize.y <= 0f ? 1f : Mathf.Min(1f, panelSize.x / ReferenceWidth, panelSize.y / ReferenceHeight);

        /// <summary>Applique (ou retire) la réduction sur <paramref name="content"/>, qui remplit le panneau par défaut.</summary>
        public static void Apply(VisualElement content)
        {
            if (content.panel == null) return;
            var size = content.panel.visualTree.layout.size;
            float s = Scale(size);
            if (s > 0.999f)
            {
                content.style.width = StyleKeyword.Null; content.style.height = StyleKeyword.Null;
                content.style.right = StyleKeyword.Null; content.style.bottom = StyleKeyword.Null;
                content.style.scale = StyleKeyword.Null; content.style.transformOrigin = StyleKeyword.Null;
                return;
            }
            content.style.right = StyleKeyword.Auto; content.style.bottom = StyleKeyword.Auto;
            content.style.width = size.x / s; content.style.height = size.y / s;
            content.style.transformOrigin = new TransformOrigin(0f, 0f);
            content.style.scale = new Scale(new Vector3(s, s, 1f));
        }
    }
}
