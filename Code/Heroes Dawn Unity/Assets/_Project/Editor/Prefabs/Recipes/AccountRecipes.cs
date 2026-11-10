#nullable enable
using N29.HeroesDawn.Presentation.Account;
using N29.HeroesDawn.Presentation.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>Fenêtre Compte : onglets, e-mail, mot de passe, pseudo, Valider, Mot de passe oublié, Google, Apple, Fermer, attente.</summary>
    public sealed class AccountPopupRecipe : IPrefabRecipe
    {
        public string RelativePath => "Popups/AccountPopup.prefab";
        public int Order => 10;

        public GameObject Build(PrefabContext ctx)
            => N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<AccountPopupView>("AccountPopup", "Account/Account.uxml", N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.ModalOrder);
    }

    /// <summary>Choix de progression : deux cartes (appareil, compte) avec chiffres et bouton chacune, Plus tard.</summary>
    public sealed class ProgressChoicePopupRecipe : IPrefabRecipe
    {
        public string RelativePath => "Popups/ProgressChoicePopup.prefab";
        public int Order => 10;

        public GameObject Build(PrefabContext ctx)
            => N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<ProgressChoicePopupView>("ProgressChoicePopup", "ProgressChoice/ProgressChoice.uxml", N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.ModalOrder);
    }
}
