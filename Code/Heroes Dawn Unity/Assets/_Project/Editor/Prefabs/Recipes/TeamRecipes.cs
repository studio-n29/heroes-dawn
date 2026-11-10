#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Tutorial;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Wip;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{

    /// <summary>Page Équipe (UI Toolkit) : filtres et tri, grille de cartes, escouade, synergies et compositions à droite.</summary>
    public sealed class TeamPageRecipe : IPrefabRecipe
    {
        public string RelativePath => "Pages/Team.prefab";
        public int Order => 10;
        public GameObject Build(PrefabContext ctx) => N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<TeamPageView>("TeamPage", "Team/Team.uxml", 10);
    }

    /// <summary>
    /// Fiche d'un membre (maquette) : grande illustration à gauche, niveau, histoire, stats et paliers au centre, quatre compétences,
    /// trois objets et agents à forte affinité à droite. Le panneau d'inventaire (maquettes « cartes » et « objets ») recouvre la grille de compétences.
    /// </summary>
    public sealed class MemberSheetRecipe : IPrefabRecipe
    {
        public string RelativePath => "Pages/MemberSheet.prefab";
        public int Order => 10;
        public GameObject Build(PrefabContext ctx) => N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<MemberSheetView>("MemberSheet", "MemberSheet/MemberSheet.uxml", 10);
    }
}
