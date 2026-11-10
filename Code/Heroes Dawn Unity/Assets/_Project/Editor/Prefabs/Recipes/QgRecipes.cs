#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Tutorial;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Wip;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>Case de pièce du QG (M10 ③) : bouton plein, nom, info ; bandeau BIENTÔT par-dessus quand la pièce est à venir.</summary>
    public sealed class RoomCardRecipe : IPrefabRecipe
    {
        public string RelativePath => "Widgets/RoomCard.prefab";
        public int Order => 5;

        public GameObject Build(PrefabContext ctx)
        {
            var root = PrefabContext.NewRoot("RoomCard", 350, 168);
            var button = Ui.Btn(root, "", Theme.Surface2, Theme.Text, null!, 34, FontKind.Title, "Button");
            button.Rect.Stretch();
            button.Label.gameObject.SetActive(false);
            var holder = Ui.Rect(button.Fill.transform, "ArtHolder").Stretch();
            holder.gameObject.AddComponent<RectMask2D>();
            var art = Ui.Img(holder, "Art", Color.white, null);
            art.raycastTarget = false;
            art.gameObject.AddComponent<AspectRatioFitter>().aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            var shade = Ui.Img(holder, "Shade", Theme.WithAlpha(Theme.Outline, 0.88f), Sprites.VerticalGradient);
            shade.raycastTarget = false;
            shade.rectTransform.Stretch();
            var caption = Ui.Img(button.Fill.transform, "Caption", Theme.WithAlpha(Theme.Background, 0.96f), Sprites.Panel);
            caption.rectTransform.BottomBand(164);
            var title = Ui.Txt(button.Fill.transform, "", 36, Theme.Primary, TextAnchor.UpperLeft, FontKind.Title, "Title");
            title.rectTransform.At(0f, 0f, 18, 74, 171, 82);
            title.AutoSize(36);
            title.Inked();
            var info = Ui.Txt(button.Fill.transform, "", 24, Theme.Text, TextAnchor.LowerLeft, FontKind.Body, "Info");
            info.rectTransform.At(0f, 0f, 18, 14, 171, 52);
            // État (accessible ou nouveau) : étiquette en haut à droite, l'objectif reste en bas.
            var state = Ui.Txt(button.Fill.transform, "", 22, Theme.Primary, TextAnchor.UpperRight, FontKind.Title, "State");
            state.rectTransform.At(1f, 1f, -12, -10, 180, 34);
            state.Inked();
            var icon = Ui.Img(button.Fill.transform, "FunctionIcon", Theme.Accent, Sprites.Glyph("Register"));
            icon.rectTransform.At(0f, 1f, 12, -10, 32, 32);
            var wip = ctx.Nest<WipMark>("Widgets/WipMark.prefab", root, "Wip");
            ((RectTransform)wip.transform).Stretch();
            var band = (RectTransform)wip.transform.Find("Band");
            band.localScale = new Vector3(0.8f, 0.8f, 1f);
            band.anchoredPosition = new Vector2(70, -40); // sous le titre, jamais dessus
            wip.gameObject.SetActive(false);
            var badgeImg = Ui.Img(root, "Badge", Theme.Danger, Sprites.Circle);
            badgeImg.rectTransform.At(1f, 1f, 10, 10, 44, 44);
            var badgeText = Ui.Txt(badgeImg.transform, "", 26, Color.white, TextAnchor.MiddleCenter, FontKind.Title, "Text");
            badgeText.rectTransform.Stretch();
            badgeImg.gameObject.AddComponent<Pulse>().Amplitude = 0.1f;
            badgeImg.gameObject.SetActive(false);
            var view = root.gameObject.AddComponent<RoomCardView>();
            ctx.Wire(view, "button", button);
            ctx.Wire(view, "art", art);
            ctx.Wire(view, "title", title);
            ctx.Wire(view, "info", info);
            ctx.Wire(view, "state", state);
            ctx.Wire(view, "icon", icon);
            ctx.Wire(view, "wip", wip);
            ctx.Wire(view, "badge", badgeImg.gameObject);
            ctx.Wire(view, "badgeText", badgeText);
            return root.gameObject;
        }
    }

    /// <summary>Page QG : document UI Toolkit (Screens/Qg/Qg.uxml).</summary>
    public sealed class QgPageRecipe : IPrefabRecipe
    {
        public string RelativePath => "Pages/Qg.prefab";
        public int Order => 10;

        public GameObject Build(PrefabContext ctx)
            => N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<QgPageView>("QgPage", "Qg/Qg.uxml", 10);
    }
}
