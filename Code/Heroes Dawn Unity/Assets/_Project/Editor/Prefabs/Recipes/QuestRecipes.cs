#nullable enable
using N29.HeroesDawn.Presentation.Hub;
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>Suivi des quêtes du QG : document UI Toolkit (Screens/QuestLog/QuestLog.uxml).</summary>
    public sealed class QuestLogPopupRecipe : IPrefabRecipe
    {
        public string RelativePath => "Popups/QuestLogPopup.prefab";
        public int Order => 10;

        public GameObject Build(PrefabContext ctx)
            => N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<QuestLogPopupView>("QuestLogPopup", "QuestLog/QuestLog.uxml", 100);
    }
}
