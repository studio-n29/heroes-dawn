#nullable enable
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Narration;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>Boîte de dialogue de narration : voile cliquable, avatar, plaque de nom, bulle de bande dessinée, indication, bouton Passer.</summary>
    public sealed class DialogueBoxRecipe : IPrefabRecipe
    {
        public string RelativePath => "Popups/DialogueBox.prefab";
        public int Order => 11;

        public GameObject Build(PrefabContext ctx)
        {
            var root = PrefabContext.NewRoot("DialogueBox");
            var group = root.gameObject.AddComponent<CanvasGroup>();

            var dim = Ui.Img(root, "Dim", Theme.WithAlpha(Theme.Navy, 0.3f), null, true);
            dim.rectTransform.Stretch();
            // Voile cinÃ©ma : un dÃ©gradÃ© sombre qui monte du bas de l'Ã©cran, pour que la rÃ©plique se lise sur n'importe quel fond.
            var shade = Ui.Img(root, "Shade", Theme.WithAlpha(Theme.Outline, 0.92f), Sprites.VerticalGradient);
            shade.raycastTarget = false;
            shade.rectTransform.anchorMin = new Vector2(0f, 0f);
            shade.rectTransform.anchorMax = new Vector2(1f, 0.62f);
            shade.rectTransform.offsetMin = shade.rectTransform.offsetMax = Vector2.zero;
            var tap = dim.gameObject.AddComponent<Button>();
            tap.transition = Selectable.Transition.None;
            var nav = tap.navigation;
            nav.mode = Navigation.Mode.None;
            tap.navigation = nav;

            var strip = Ui.Rect(root, "Strip").At(0.5f, 0f, 0, 40, 1800, 400);

            var bubbleFill = Ui.Panel(strip, "Bubble", Theme.WithAlpha(Theme.Navy, 0.94f), 5f, 12f, false);
            var bubble = bubbleFill.Frame();
            bubble.anchorMin = new Vector2(0f, 0f);
            bubble.anchorMax = new Vector2(1f, 1f);
            bubble.offsetMin = new Vector2(380, 20);
            bubble.offsetMax = new Vector2(-30, -70);

            var body = Ui.Txt(bubbleFill.transform, "", 40, Color.white, TextAnchor.MiddleLeft, FontKind.Body, "Body");
            body.rectTransform.Stretch(40, 50, 40, 34);
            body.lineSpacing = 6;
            body.overflowMode = TMPro.TextOverflowModes.Ellipsis;
            body.enableAutoSizing = true;
            body.fontSizeMin = 28;
            body.fontSizeMax = 44;
            var hint = Ui.LocTxt(bubbleFill.transform, "narration.next", 24, Theme.Primary, TextAnchor.LowerRight, FontKind.Title, "Hint");
            hint.rectTransform.At(1f, 0f, -26, 10, 420, 36);
            hint.gameObject.AddComponent<Pulse>().Amplitude = 0.04f;

            // Queue de la bulle : un losange encré, posé sur le bord de la bulle côté avatar.
            var tail = Ui.Img(strip, "Tail", Theme.WithAlpha(Theme.Ink, 0f), Sprites.Rounded);
            tail.rectTransform.At(0f, 0.5f, 324, 20, 46, 46);
            tail.rectTransform.localRotation = Quaternion.Euler(0, 0, 45f);
            var tailFill = Ui.Img(tail.transform, "Fill", Theme.WithAlpha(Color.white, 0f), Sprites.Rounded);
            tailFill.rectTransform.Stretch(6, 6, 6, 6);

            var avatarRoot = Ui.Rect(strip, "Avatar").At(0f, 0.5f, 30, 20, 340, 400);
            avatarRoot.pivot = new Vector2(0f, 0.5f);
            var portrait = ctx.Nest<PortraitView>("Widgets/Portrait.prefab", avatarRoot, "Portrait");
            ((RectTransform)portrait.transform).At(0.5f, 1f, 0, -6, 320, 320);
            var plateFill = Ui.Panel(avatarRoot, "NamePlate", Theme.Primary, 4f, 6f, false);
            plateFill.Frame().At(0.5f, 0f, 0, 6, 330, 70);
            var name = Ui.Txt(plateFill.transform, "", 38, Theme.TextDark, TextAnchor.MiddleCenter, FontKind.Title, "Name");
            name.rectTransform.Stretch(6, 2, 6, 2);
            name.AutoSize(38);

            var skip = Ui.LocBtn(root, "narration.skip", Theme.Surface3, Theme.Text, null!, 30, FontKind.Title, "Skip");
            skip.Rect.At(1f, 1f, -40, -40, 220, 80);

            var view = root.gameObject.AddComponent<DialogueBoxView>();
            ctx.Wire(view, "group", group);
            ctx.Wire(view, "tapArea", tap);
            ctx.Wire(view, "avatarRoot", avatarRoot);
            ctx.Wire(view, "portrait", portrait);
            ctx.Wire(view, "namePlate", plateFill);
            ctx.Wire(view, "nameText", name);
            ctx.Wire(view, "bubble", bubble);
            ctx.Wire(view, "tail", tail.rectTransform);
            ctx.Wire(view, "body", body);
            ctx.Wire(view, "hint", hint);
            ctx.Wire(view, "skip", skip.Button);
            return root.gameObject;
        }
    }
}
