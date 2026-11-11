#nullable enable
using N29.HeroesDawn.Presentation.Combat;
using N29.HeroesDawn.Presentation.Common;
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>Résultats du combat (reprise de l'ancien CombatScreen.ShowResults).</summary>
    public sealed class CombatResultsPopupRecipe : IPrefabRecipe
    {
        public string RelativePath => "Popups/CombatResultsPopup.prefab";
        public int Order => 10;
        public GameObject Build(PrefabContext ctx) => N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<N29.HeroesDawn.Presentation.Combat.CombatResultsPopupView>("CombatResultsPopup", "Popups/CombatResults.uxml", N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.ModalOrder);
    }

    /// <summary>
    /// Combat : fond uGUI (ciel et décor) et HUD UI Toolkit qui porte tout le reste (barre du haut, arène, cartes d'unités, frise,
    /// éventail de compétences, description, bonus, boss, journal, bandeau).
    /// </summary>
    public sealed class CombatRecipe : IPrefabRecipe
    {
        public string RelativePath => "Screens/Combat.prefab";
        public int Order => 20;

        public GameObject Build(PrefabContext ctx)
        {
            var rt = ScreenKit.Root("Combat");
            ScreenKit.Backdrop(rt, Theme.SkyTop, Theme.SkyBottom);
            ScreenKit.ArtBackdrop(rt, Sprites.BackgroundLandscape, 1f, 0.72f);

            // Panneau superposé (le fond uGUI du combat recouvrirait le panneau d'écran), mais sous les fenêtres modales.
            var hudGo = N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<CombatHudView>("Hud", "Combat/CombatHud.uxml", N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.ModalOrder);
            hudGo.GetComponent<UnityEngine.UIElements.UIDocument>().sortingOrder = 50;
            hudGo.transform.SetParent(rt, false);
            var hudRt = (RectTransform)hudGo.transform;
            hudRt.anchorMin = Vector2.zero; hudRt.anchorMax = Vector2.one; hudRt.offsetMin = hudRt.offsetMax = Vector2.zero;
            var hud = hudGo.GetComponent<CombatHudView>();

            var view = rt.gameObject.AddComponent<CombatView>();
            ScreenKit.WireFade(ctx, view);
            ctx.Wire(view, "hud", hud);
            return rt.gameObject;
        }
    }
}
