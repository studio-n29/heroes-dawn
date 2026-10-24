#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>Formulaire de retour de playtest : quatre questions notées de 1 à 5, un champ de commentaire, Envoyer / Plus tard.</summary>
    public sealed class PlaytestFormPopupRecipe : IPrefabRecipe
    {
        public string RelativePath => "Popups/PlaytestFormPopup.prefab";
        public int Order => 10;

        public GameObject Build(PrefabContext ctx)
        {
            var root = PrefabContext.NewRoot("PlaytestFormPopup");
            var group = root.gameObject.AddComponent<CanvasGroup>();
            var (window, content, title) = PopupKit.Window(root, 1280, 900, Theme.Primary);

            var intro = Ui.Txt(content, "", 26, Theme.TextMuted, TextAnchor.MiddleLeft, FontKind.Body, "Intro");
            intro.rectTransform.At(0f, 1f, 0, 0, 1200, 56);

            var labels = new List<Object>();
            var ratings = new List<Object>();
            for (int q = 0; q < 4; q++)
            {
                float y = -64 - q * 104;
                var label = Ui.Txt(content, "", 30, Theme.Text, TextAnchor.MiddleLeft, FontKind.Title, "Question" + q);
                label.rectTransform.At(0f, 1f, 0, y, 640, 96);
                label.AutoSize(30);
                labels.Add(label);
                for (int r = 0; r < 5; r++)
                {
                    var b = Ui.Btn(content, "", Theme.Surface3, Theme.Text, null!, 40, FontKind.Title, "Rate" + q + "_" + (r + 1));
                    b.Rect.At(0f, 1f, 660 + r * 112, y, 100, 96);
                    ratings.Add(b);
                }
            }

            var commentLabel = Ui.Txt(content, "", 28, Theme.Primary, TextAnchor.MiddleLeft, FontKind.Title, "CommentLabel");
            commentLabel.rectTransform.At(0f, 1f, 0, -484, 1200, 44);

            var box = Ui.Img(content, "Comment", Theme.Surface3, Sprites.Panel, true);
            box.rectTransform.At(0f, 1f, 0, -532, 1208, 150);
            var viewport = Ui.Rect(box.transform, "Viewport").Stretch(16, 10, 16, 10);
            viewport.gameObject.AddComponent<RectMask2D>();
            var text = Ui.Txt(viewport, "", 28, Theme.Text, TextAnchor.UpperLeft, FontKind.Body, "Text");
            text.rectTransform.Stretch();
            text.raycastTarget = true;
            var hint = Ui.Txt(viewport, "", 28, Theme.TextMuted, TextAnchor.UpperLeft, FontKind.Body, "Hint");
            hint.rectTransform.Stretch();
            var field = box.gameObject.AddComponent<TMP_InputField>();
            field.textViewport = viewport;
            field.textComponent = text;
            field.placeholder = hint;
            field.lineType = TMP_InputField.LineType.MultiLineNewline;
            field.characterLimit = 600;

            var buttons = Ui.Rect(content, "Buttons").BottomBand(96, 0);
            buttons.HLayout(40, 0, TextAnchor.MiddleCenter);
            var later = Ui.Btn(buttons, "", Theme.Surface3, Theme.Text, null!, 34, FontKind.Title, "Later");
            later.Rect.Pref(340, 96);
            var send = Ui.Btn(buttons, "", Theme.Success, Theme.TextDark, null!, 40, FontKind.Title, "Send");
            send.Rect.Pref(420, 96);

            var view = root.gameObject.AddComponent<PlaytestFormPopupView>();
            ctx.Wire(view, "title", title);
            ctx.Wire(view, "intro", intro);
            ctx.WireArray(view, "questionLabels", labels);
            ctx.WireArray(view, "ratingButtons", ratings);
            ctx.Wire(view, "commentLabel", commentLabel);
            ctx.Wire(view, "comment", field);
            ctx.Wire(view, "commentHint", hint);
            ctx.Wire(view, "send", send);
            ctx.Wire(view, "later", later);
            ctx.Wire(view, "window", window);
            ctx.Wire(view, "group", group);
            return root.gameObject;
        }
    }
}
