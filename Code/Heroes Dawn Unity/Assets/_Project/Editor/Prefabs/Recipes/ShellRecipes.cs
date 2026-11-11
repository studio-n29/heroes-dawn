#nullable enable
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Wip;
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{

    /// <summary>
    /// Coque du hub (M10) : barre haute (Retour, titre, ressources, réglages), zone de page entre les zones sûres,
    /// barre de navigation basse à 5 onglets avec badges. Les onglets et le titre sont remplis par le présentateur.
    /// </summary>
    public sealed class HubShellRecipe : IPrefabRecipe
    {
        public string RelativePath => "Screens/Hub.prefab";
        public int Order => 20;

        const float TopH = 116, NavH = 116, Safe = 88;

        public GameObject Build(PrefabContext ctx)
        {
            var go = N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<HubShellView>("Hub", "Hub/Hub.uxml", N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.ScreenOrder);
            var relay = go.AddComponent<UiTapRelay>();
            // Zone de page (uGUI) entre les barres, zones sûres de 88 px sur les côtés.
            var pageArea = Ui.Rect((RectTransform)go.transform, "PageArea").Stretch(Safe, NavH + 12, Safe, TopH + 12);

            var view = go.GetComponent<HubShellView>();
            ctx.Wire(view, "pageArea", pageArea);
            ctx.Wire(view, "relay", relay);
            return go;
        }
    }
}
