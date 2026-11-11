#nullable enable
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Popups;
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>Toasts : document UI Toolkit (Screens/Toasts/Toasts.uxml).</summary>
    public sealed class ToastsRecipe : IPrefabRecipe
    {
        public string RelativePath => "Common/Toasts.prefab";
        public int Order => 10;

        public GameObject Build(PrefabContext ctx)
            => N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<ToastsView>("Toasts", "Toasts/Toasts.uxml", N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.ToastOrder);
    }

    /// <summary>Couche des modales (les popups y sont instanciées).</summary>
    public sealed class ModalLayerRecipe : IPrefabRecipe
    {
        public string RelativePath => "Common/ModalLayer.prefab";
        public int Order => 10;

        public GameObject Build(PrefabContext ctx)
        {
            var root = PrefabContext.NewRoot("ModalLayer");
            root.gameObject.AddComponent<ModalLayer>();
            return root.gameObject;
        }
    }

    /// <summary>Confirmation : document UI Toolkit (Screens/Confirm/Confirm.uxml).</summary>
    public sealed class ConfirmPopupRecipe : IPrefabRecipe
    {
        public string RelativePath => "Popups/ConfirmPopup.prefab";
        public int Order => 10;

        public GameObject Build(PrefabContext ctx)
            => N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<ConfirmPopupView>("ConfirmPopup", "Confirm/Confirm.uxml", N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.ModalOrder);
    }
}
