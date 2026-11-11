#nullable enable
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>Couche du tutoriel : voile, désignation, bulle de Rayner et astuce, en UI Toolkit (panneau superposé, au-dessus des fenêtres).</summary>
    public sealed class TutorialOverlayRecipe : IPrefabRecipe
    {
        public string RelativePath => "Common/TutorialOverlay.prefab";
        public int Order => 10;

        public GameObject Build(PrefabContext ctx)
        {
            var go = N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<N29.HeroesDawn.Presentation.Tutorial.TutorialOverlayView>("TutorialOverlay", "Tutorial/TutorialOverlay.uxml", N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.TutorialOrder);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
            return go;
        }
    }
}
