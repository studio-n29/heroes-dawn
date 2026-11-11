#nullable enable
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>Boîte de dialogue de narration : fenêtre UI Toolkit (avatar, plaque de nom, bulle).</summary>
    public sealed class DialogueBoxRecipe : IPrefabRecipe
    {
        public string RelativePath => "Popups/DialogueBox.prefab";
        public int Order => 11;
        public GameObject Build(PrefabContext ctx) => N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<N29.HeroesDawn.Presentation.Narration.DialogueBoxView>("DialogueBox", "Popups/DialogueBox.uxml", N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.ModalOrder);
    }
}
