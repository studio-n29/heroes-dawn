#nullable enable
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{


    public sealed class PassPageRecipe : IPrefabRecipe
    {
        public string RelativePath => "Pages/Pass.prefab";
        public int Order => 10;
        public GameObject Build(PrefabContext ctx) => N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<PassPageView>("PassPage", "ListPage/ListPage.uxml", 10);
    }

    public sealed class BoutiquePageRecipe : IPrefabRecipe
    {
        public string RelativePath => "Pages/Boutique.prefab";
        public int Order => 10;
        public GameObject Build(PrefabContext ctx) => N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<BoutiquePageView>("BoutiquePage", "CardGrid/CardGrid.uxml", 10);
    }

    public sealed class OperationsPageRecipe : IPrefabRecipe
    {
        public string RelativePath => "Pages/Operations.prefab";
        public int Order => 10;
        public GameObject Build(PrefabContext ctx) => N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<OperationsPageView>("OperationsPage", "ListPage/ListPage.uxml", 10);
    }

    public sealed class LabPageRecipe : IPrefabRecipe
    {
        public string RelativePath => "Pages/Lab.prefab";
        public int Order => 10;
        public GameObject Build(PrefabContext ctx) => N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<LabPageView>("LabPage", "ListPage/ListPage.uxml", 10);
    }

    /// <summary>Profil du joueur : tag, accès au registre et aux archives, statistiques, amis.</summary>
    public sealed class ProfilePageRecipe : IPrefabRecipe
    {
        public string RelativePath => "Pages/Profile.prefab";
        public int Order => 10;
        public GameObject Build(PrefabContext ctx) => N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<ProfilePageView>("ProfilePage", "ListPage/ListPage.uxml", 10);
    }

    /// <summary>Page Chapitres : les 8 chapitres de l'histoire, leur statut et « Relire ».</summary>
    public sealed class ChaptersPageRecipe : IPrefabRecipe
    {
        public string RelativePath => "Pages/Chapters.prefab";
        public int Order => 10;
        public GameObject Build(PrefabContext ctx) => N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<ChaptersPageView>("ChaptersPage", "ListPage/ListPage.uxml", 10);
    }

    /// <summary>Page Repaire : étapes (boss) à gauche, préparation et bilan à droite.</summary>
    public sealed class LairPageRecipe : IPrefabRecipe
    {
        public string RelativePath => "Pages/Lair.prefab";
        public int Order => 10;
        public GameObject Build(PrefabContext ctx) => N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<LairPageView>("LairPage", "ListPage/ListPage.uxml", 10);
    }

    /// <summary>Page Tournoi : classement à gauche, manches du jour à droite.</summary>
    public sealed class TournamentPageRecipe : IPrefabRecipe
    {
        public string RelativePath => "Pages/Tournament.prefab";
        public int Order => 10;
        public GameObject Build(PrefabContext ctx) => N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<TournamentPageView>("TournamentPage", "ListPage/ListPage.uxml", 10);
    }

    public sealed class ArchivesPageRecipe : IPrefabRecipe
    {
        public string RelativePath => "Pages/Archives.prefab";
        public int Order => 10;
        public GameObject Build(PrefabContext ctx) => N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<ArchivesPageView>("ArchivesPage", "ListPage/ListPage.uxml", 10);
    }
}
