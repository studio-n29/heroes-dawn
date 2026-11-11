#nullable enable
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>Entrée en combat : fenêtre UI Toolkit (un bandeau par combattant).</summary>
    public sealed class CombatIntroPopupRecipe : IPrefabRecipe
    {
        public string RelativePath => "Popups/CombatIntroPopup.prefab";
        public int Order => 12;
        public GameObject Build(PrefabContext ctx) => N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<N29.HeroesDawn.Presentation.Combat.CombatIntroPopupView>("CombatIntroPopup", "Popups/CombatIntro.uxml", N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.ModalOrder);
    }
}
