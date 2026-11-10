#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Popups;
using N29.HeroesDawn.Presentation.Tutorial;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{

    /// <summary>Page Recruter : règle et compte à rebours en haut, 3 cartes au centre, progression et Renouveler en bas.</summary>
    public sealed class RecruitPageRecipe : IPrefabRecipe
    {
        public string RelativePath => "Pages/Recruit.prefab";
        public int Order => 10;

        public GameObject Build(PrefabContext ctx) => N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<RecruitPageView>("RecruitPage", "Recruit/Recruit.uxml", 10);
    }

    /// <summary>Confirmation d'achat (M31) : héros et synergie phare à gauche, solde avant / coût / après à droite, Annuler / Recruter.</summary>
    public sealed class RecruitConfirmPopupRecipe : IPrefabRecipe
    {
        public string RelativePath => "Popups/RecruitConfirmPopup.prefab";
        public int Order => 10;
        public GameObject Build(PrefabContext ctx) => N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<N29.HeroesDawn.Presentation.Popups.RecruitConfirmPopupView>("RecruitConfirmPopup", "Popups/RecruitConfirm.uxml", N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.ModalOrder);
    }
}
