#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Tutorial;
using N29.HeroesDawn.Presentation.Hub;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{


    /// <summary>Page Carte de la ville (M60/M61) : carte à gauche, acte + liste + fiche de zone + DÉPLOYER à droite.</summary>
    public sealed class CityMapPageRecipe : IPrefabRecipe
    {
        public string RelativePath => "Pages/CityMap.prefab";
        public int Order => 10;
        public GameObject Build(PrefabContext ctx) => N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<N29.HeroesDawn.Presentation.Hub.CityMapPageView>("CityMap", "CityMap/CityMap.uxml", 10);
    }
}
