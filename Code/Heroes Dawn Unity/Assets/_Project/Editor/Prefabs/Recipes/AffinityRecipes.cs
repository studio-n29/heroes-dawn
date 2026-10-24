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
            rootRt.sizeDelta = new Vector2(1100, 112);
            rootRt.Pref(-1, 112, 1);
            var portrait = ctx.Nest<PortraitView>("Widgets/Portrait.prefab", fill.transform, "Portrait");
            ((RectTransform)portrait.transform).At(0f, 0.5f, 18, 0, 84, 84);
            var name = Ui.Txt(fill.transform, "", 34, Theme.Text, TextAnchor.MiddleLeft, FontKind.Title, "Name");
            name.rectTransform.At(0f, 1f, 122, -12, 420, 46);
            name.AutoSize(34);
            var link = Ui.Txt(fill.transform, "", 24, Theme.TextMuted, TextAnchor.MiddleLeft, FontKind.Body, "Link");
            link.rectTransform.At(0f, 0f, 122, 12, 420, 36);
            var bonus = Ui.Txt(fill.transform, "", 28, Theme.Success, TextAnchor.MiddleLeft, FontKind.Title, "Bonus");
            bonus.rectTransform.At(1f, 0.5f, -18, 0, 520, 90);
            bonus.AutoSize(28);

            var view = rootRt.gameObject.AddComponent<AffinityRowView>();
            ctx.Wire(view, "portrait", portrait);
            ctx.Wire(view, "agentName", name);
            ctx.Wire(view, "link", link);
            ctx.Wire(view, "bonus", bonus);
            return rootRt.gameObject;
        }
    }

    /// <summary>Pop-up d'affinités : titre, rappel de la règle, liste défilante des agents avec le bonus de chaque duo, bouton Fermer.</summary>
    public sealed class AffinityPopupRecipe : IPrefabRecipe
    {
        public string RelativePath => "Popups/AffinityPopup.prefab";
        public int Order => 10;

        public GameObject Build(PrefabContext ctx)
        {
            var root = PrefabContext.NewRoot("AffinityPopup");
            var group = root.gameObject.AddComponent<CanvasGroup>();
            var (window, content, title) = PopupKit.Window(root, 1280, 800, Theme.Success);

            var subtitle = Ui.Txt(content, "", 24, Theme.TextMuted, TextAnchor.MiddleLeft, FontKind.Body, "Subtitle");
            subtitle.rectTransform.At(0f, 1f, 0, 0, 1200, 40);

            var scrollBody = Ui.VScroll(content, out _);
            ((RectTransform)scrollBody.parent).Stretch(0, 100, 0, 52); // sous le rappel, au-dessus du bouton Fermer
            var v = scrollBody.gameObject.AddComponent<VerticalLayoutGroup>();
            v.spacing = 12;
            v.padding = new RectOffset(0, 0, 0, 20);
            v.childControlWidth = true;
            v.childControlHeight = true;
            v.childForceExpandWidth = true;
            v.childForceExpandHeight = false;

            var empty = Ui.Txt(content, "", 28, Theme.TextMuted, TextAnchor.MiddleCenter, FontKind.Body, "Empty");
            empty.rectTransform.Stretch(0, 100, 0, 52);

            var close = Ui.Btn(content, "", Theme.Surface3, Theme.Text, null!, 36, FontKind.Title, "Close");
            close.Rect.At(0.5f, 0f, 0, 0, 320, 84);

            var rowPrefab = ctx.Asset("Widgets/AffinityRow.prefab").GetComponent<AffinityRowView>();
            var view = root.gameObject.AddComponent<AffinityPopupView>();
            ctx.Wire(view, "title", title);
            ctx.Wire(view, "subtitle", subtitle);
            ctx.Wire(view, "empty", empty);
            ctx.Wire(view, "list", scrollBody);
            ctx.Wire(view, "rowPrefab", rowPrefab);
            ctx.Wire(view, "close", close);
            ctx.Wire(view, "window", window);
            ctx.Wire(view, "group", group);
            return root.gameObject;
        }
    }
}
