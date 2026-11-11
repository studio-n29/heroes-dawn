#nullable enable
using N29.HeroesDawn.Presentation.Board;
using N29.HeroesDawn.Presentation.Common;
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>Plateau : fond uGUI (ciel et décor) et HUD UI Toolkit qui porte la carte, les objectifs, l'escouade, le dé et les boutons.</summary>
    public sealed class BoardRecipe : IPrefabRecipe
    {
        public string RelativePath => "Screens/Board.prefab";
        public int Order => 20;

        public GameObject Build(PrefabContext ctx)
        {
            var rt = ScreenKit.Root("Board");
            ScreenKit.Backdrop(rt, Theme.SkyTop, Theme.SkyBottom);
            ScreenKit.ArtBackdrop(rt, Sprites.BackgroundLandscape, 1f, 0.5f); // l'illustration Red Hook reste lisible derrière le plateau

            // Panneau superposé (le fond uGUI recouvrirait le panneau d'écran), sous les fenêtres modales.
            var hudGo = N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<BoardHudView>("Hud", "Board/BoardHud.uxml", N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.ModalOrder);
            hudGo.GetComponent<UnityEngine.UIElements.UIDocument>().sortingOrder = 50;
            hudGo.transform.SetParent(rt, false);
            var hudRt = (RectTransform)hudGo.transform;
            hudRt.anchorMin = Vector2.zero; hudRt.anchorMax = Vector2.one; hudRt.offsetMin = hudRt.offsetMax = Vector2.zero;

            var view = rt.gameObject.AddComponent<BoardView>();
            ScreenKit.WireFade(ctx, view);
            ctx.Wire(view, "hud", hudGo.GetComponent<BoardHudView>());
            return rt.gameObject;
        }
    }
}
