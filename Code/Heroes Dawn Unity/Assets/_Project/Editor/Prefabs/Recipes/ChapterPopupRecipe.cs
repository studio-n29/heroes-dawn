#nullable enable
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>Ouverture d'un chapitre : image large (facultative), intitulé, texte d'ouverture, bouton Continuer.</summary>
    public sealed class ChapterPopupRecipe : IPrefabRecipe
    {
        public string RelativePath => "Popups/ChapterPopup.prefab";
        public int Order => 10;

        public GameObject Build(PrefabContext ctx)
        {
            var root = PrefabContext.NewRoot("ChapterPopup");
            var group = root.gameObject.AddComponent<CanvasGroup>();
            var (window, content, title) = PopupKit.Window(root, 1400, 760, Theme.Primary);

            var holder = Ui.Rect(content, "ArtHolder");
            holder.At(0f, 1f, 0, 0, 1300, 260);
            holder.gameObject.AddComponent<RectMask2D>();
            var art = Ui.Img(holder, "Art", Color.white, null);
            art.gameObject.AddComponent<AspectRatioFitter>().aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            art.gameObject.SetActive(false);

            var text = Ui.Txt(content, "", 32, Theme.Text, TextAnchor.UpperLeft, FontKind.Body, "Text");
            text.rectTransform.At(0f, 1f, 0, -280, 1300, 250);

            var buttons = Ui.Rect(content, "Buttons").BottomBand(96, 0);
            buttons.HLayout(40, 0, TextAnchor.MiddleCenter);
            var next = Ui.Btn(buttons, "", Theme.Primary, Theme.TextDark, null!, 40, FontKind.Title, "Continue");
            next.Rect.Pref(420, 96);

            var view = root.gameObject.AddComponent<ChapterPopupView>();
            ctx.Wire(view, "heading", title);
            ctx.Wire(view, "text", text);
            ctx.Wire(view, "art", art);
            ctx.Wire(view, "continueButton", next);
            ctx.Wire(view, "window", window);
            ctx.Wire(view, "group", group);
            return root.gameObject;
        }
    }
}
