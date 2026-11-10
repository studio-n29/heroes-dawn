#nullable enable
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>Ligne d'affinité : portrait, nom, lien, et le bonus du duo à droite.</summary>
    public sealed class AffinityRowRecipe : IPrefabRecipe
    {
        public string RelativePath => "Widgets/AffinityRow.prefab";
        public int Order => 6;

        public GameObject Build(PrefabContext ctx)
        {
            var fill = RecipeKit.PanelRoot("AffinityRow", Theme.Frost, 4f, 4f, false);
            var rootRt = fill.Frame();
            rootRt.sizeDelta = new Vector2(1100, 210);
            rootRt.Pref(-1, 210, 1);
            var portrait = ctx.Nest<PortraitView>("Widgets/Portrait.prefab", fill.transform, "Portrait");
            ((RectTransform)portrait.transform).At(0f, 0.5f, 18, 0, 84, 84);
            var name = Ui.Txt(fill.transform, "", 34, Theme.Text, TextAnchor.MiddleLeft, FontKind.Title, "Name");
            name.rectTransform.At(0f, 1f, 122, -12, 420, 46);
            name.AutoSize(34);
            var link = Ui.Txt(fill.transform, "", 24, Theme.TextMuted, TextAnchor.MiddleLeft, FontKind.Body, "Link");
            link.rectTransform.At(0f, 0f, 122, 12, 420, 36);
            var bonus = Ui.Txt(fill.transform, "", 28, Theme.Success, TextAnchor.MiddleLeft, FontKind.Title, "Bonus");
            bonus.rectTransform.At(1f, 1f, -18, -10, 520, 44);
            bonus.AutoSize(28);
            var pair = Ui.Txt(fill.transform, "", 22, Theme.Text, TextAnchor.UpperLeft, FontKind.Body, "Pair");
            pair.rectTransform.At(1f, 0f, -18, 10, 520, 150);

            var view = rootRt.gameObject.AddComponent<AffinityRowView>();
            ctx.Wire(view, "portrait", portrait);
            ctx.Wire(view, "agentName", name);
            ctx.Wire(view, "link", link);
            ctx.Wire(view, "bonus", bonus);
            ctx.Wire(view, "pairText", pair);
            return rootRt.gameObject;
        }
    }

    /// <summary>Pop-up d'affinités : titre, rappel de la règle, liste défilante des agents avec le bonus de chaque duo, bouton Fermer.</summary>
    public sealed class AffinityPopupRecipe : IPrefabRecipe
    {
        public string RelativePath => "Popups/AffinityPopup.prefab";
        public int Order => 10;
        public GameObject Build(PrefabContext ctx) => N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<N29.HeroesDawn.Presentation.Hub.AffinityPopupView>("AffinityPopup", "Popups/Affinity.uxml", N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.ModalOrder);
    }
}
