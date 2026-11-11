#nullable enable
using N29.HeroesDawn.Presentation.Login;
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>
    /// Première connexion : fenêtre UI Toolkit (« Passer », création du personnage) ; le lecteur de cinématique reste une surface vidéo uGUI
    /// posée sous la fenêtre (il construit ses couches au lancement : vidéos et voix off).
    /// </summary>
    public sealed class IntroPopupRecipe : IPrefabRecipe
    {
        public string RelativePath => "Popups/IntroPopup.prefab";
        public int Order => 10;

        public GameObject Build(PrefabContext ctx)
        {
            var go = N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<IntroPopupView>("IntroPopup", "Popups/Intro.uxml", N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.ModalOrder);
            var cinematic = new GameObject("Cinematic", typeof(RectTransform));
            cinematic.transform.SetParent(go.transform, false);
            var crt = (RectTransform)cinematic.transform;
            crt.anchorMin = Vector2.zero; crt.anchorMax = Vector2.one; crt.offsetMin = crt.offsetMax = Vector2.zero;
            var player = cinematic.AddComponent<CinematicVideoPlayer>();
            ctx.Wire(go.GetComponent<IntroPopupView>(), "player", player);
            return go;
        }
    }
}
