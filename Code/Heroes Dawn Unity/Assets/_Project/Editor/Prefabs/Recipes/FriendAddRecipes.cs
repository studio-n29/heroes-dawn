#nullable enable
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>Saisie du tag d'un ami : titre, champ de texte sur une ligne, Ajouter / Annuler.</summary>
    public sealed class FriendAddPopupRecipe : IPrefabRecipe
    {
        public string RelativePath => "Popups/FriendAddPopup.prefab";
        public int Order => 10;

        public GameObject Build(PrefabContext ctx)
        {
            var root = PrefabContext.NewRoot("FriendAddPopup");
            var group = root.gameObject.AddComponent<CanvasGroup>();
            var (window, content, title) = PopupKit.Window(root, 1000, 460, Theme.Primary);

            var box = Ui.Img(content, "Input", Theme.Surface3, Sprites.Panel, true);
            box.rectTransform.At(0f, 1f, 0, -40, 900, 110);
            var viewport = Ui.Rect(box.transform, "Viewport").Stretch(20, 10, 20, 10);
            viewport.gameObject.AddComponent<RectMask2D>();
            var text = Ui.Txt(viewport, "", 38, Theme.Text, TextAnchor.MiddleLeft, FontKind.Body, "Text");
            text.rectTransform.Stretch();
            text.raycastTarget = true;
            var hint = Ui.Txt(viewport, "", 38, Theme.TextMuted, TextAnchor.MiddleLeft, FontKind.Body, "Hint");
            hint.rectTransform.Stretch();
            var field = box.gameObject.AddComponent<TMP_InputField>();
            field.textViewport = viewport;
            field.textComponent = text;
            field.placeholder = hint;
            field.lineType = TMP_InputField.LineType.SingleLine;
            field.characterLimit = PlayerTagLimits.MaxInput;

            var buttons = Ui.Rect(content, "Buttons").BottomBand(96, 0);
            buttons.HLayout(40, 0, TextAnchor.MiddleCenter);
            var cancel = Ui.Btn(buttons, "", Theme.Surface3, Theme.Text, null!, 34, FontKind.Title, "Cancel");
            cancel.Rect.Pref(340, 96);
            var ok = Ui.Btn(buttons, "", Theme.Success, Theme.TextDark, null!, 40, FontKind.Title, "Ok");
            ok.Rect.Pref(420, 96);

            var view = root.gameObject.AddComponent<FriendAddPopupView>();
            ctx.Wire(view, "title", title);
            ctx.Wire(view, "field", field);
            ctx.Wire(view, "hint", hint);
            ctx.Wire(view, "ok", ok);
            ctx.Wire(view, "cancel", cancel);
            ctx.Wire(view, "window", window);
            ctx.Wire(view, "group", group);
            return root.gameObject;
        }

        /// <summary>Limite de saisie : 20 caractères de nom, #, 4 chiffres, avec une marge pour les espaces.</summary>
        static class PlayerTagLimits { public const int MaxInput = 32; }
    }
}
