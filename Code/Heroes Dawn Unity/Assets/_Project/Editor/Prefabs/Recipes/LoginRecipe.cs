#nullable enable
using N29.HeroesDawn.EditorTools.UiToolkit;
using N29.HeroesDawn.Presentation.Login;
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>Écran titre : document UI Toolkit (Screens/Login/Login.uxml) porté par un objet à RectTransform, comme un prefab d'écran.</summary>
    public sealed class LoginRecipe : IPrefabRecipe
    {
        public string RelativePath => "Screens/Login.prefab";
        public int Order => 20;

        public GameObject Build(PrefabContext ctx)
            => UiToolkitSetup.NewViewObject<LoginView>("Login", "Login/Login.uxml", UiToolkitSetup.ScreenOrder);
    }
}
