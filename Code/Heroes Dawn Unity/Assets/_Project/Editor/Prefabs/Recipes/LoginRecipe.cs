#nullable enable
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Login;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>Écran titre (reprise de l'ancien LoginScreen.Build).</summary>
    public sealed class LoginRecipe : IPrefabRecipe
    {
        public string RelativePath => "Screens/Login.prefab";
        public int Order => 20;

        public GameObject Build(PrefabContext ctx)
        {
            var rt = ScreenKit.Root("Login");
            ScreenKit.Backdrop(rt, Theme.SkyTop, Theme.SkyBottom);
            ScreenKit.ArtBackdrop(rt, Sprites.BackgroundLandscape, 1f, 0f);
            // Dégradé sombre en bas pour la lisibilité des boutons.
            var shade = Ui.Img(rt, "BottomShade", Theme.WithAlpha(Theme.Navy, 0.3f), Sprites.VerticalGradient);
            var srt = shade.rectTransform;
            srt.anchorMin = new Vector2(0, 0);
            srt.anchorMax = new Vector2(1, 0);
            srt.pivot = new Vector2(0.5f, 0.5f);
            srt.sizeDelta = new Vector2(0, 520);
            srt.anchoredPosition = new Vector2(0, 260);
            srt.localRotation = Quaternion.Euler(0, 0, 180); // opaque en bas, transparent en haut

            Ui.Panel(rt, "Identity", Theme.WithAlpha(Theme.Background, 0.9f), 4f, 0f, false).Frame().At(1f, 1f, -72, -130, 1120, 350);
            var title = Ui.LocTxt(rt, "game.title", 112, Theme.Primary, TextAnchor.MiddleRight, FontKind.Title, "Title");
            RectTransform logo = title.rectTransform.At(1f, 1f, -100, -170, 1000, 180);
            var emblem = Ui.Img(rt, "DawnEmblem", Theme.Accent, Sprites.DawnStar);
            emblem.rectTransform.At(1f, 1f, -120, -100, 72, 72);
            Ui.Img(rt, "TitleRule", Theme.Accent).rectTransform.At(1f, 1f, -100, -366, 900, 2);

            var tagline = Ui.LocTxt(rt, "game.tagline", 36, Theme.Text, TextAnchor.MiddleCenter, FontKind.Body, "Tagline");
            tagline.rectTransform.At(1f, 1f, -100, -394, 1100, 60);
            tagline.alignment = TMPro.TextAlignmentOptions.Right;
            tagline.Inked();

            var play = Ui.LocBtn(rt, "login.play", Theme.Primary, Theme.TextDark, null!, 52, FontKind.Title, "Play");
            play.Rect.At(1f, 0f, -100, 224, 600, 110);

            var account = Ui.LocBtn(rt, "login.account", Theme.Surface3, Theme.Text, null!, 34, FontKind.Title, "Account");
            account.Rect.At(1f, 0f, -100, 110, 600, 96);

            var status = Ui.Txt(rt, "", 30, Theme.TextMuted, TextAnchor.MiddleCenter, FontKind.Body, "Status");
            status.rectTransform.At(0.5f, 0f, 0, 60, 1200, 44);

            var spin = Ui.Img(rt, "Spinner", Theme.Primary, Sprites.Ring);
            spin.type = Image.Type.Filled;
            spin.fillMethod = Image.FillMethod.Radial360;
            spin.fillAmount = 0.75f;
            // Pivot au centre : la vue le fait tourner sur lui-même (position décalée de la demi-hauteur pour rester au même endroit).
            var spinner = spin.rectTransform.At(0.5f, 0f, 0, 150 + 35, 70, 70);
            spinner.pivot = new Vector2(0.5f, 0.5f);
            spinner.gameObject.SetActive(false);

            Ui.LocTxt(rt, "game.studio", 30, Theme.Primary, TextAnchor.LowerLeft, FontKind.Title, "Studio").rectTransform.At(0f, 0f, 40, 24, 400, 50);

            var view = rt.gameObject.AddComponent<LoginView>();
            ScreenKit.WireFade(ctx, view);
            ctx.Wire(view, "play", play);
            ctx.Wire(view, "status", status);
            ctx.Wire(view, "spinner", spinner);
            ctx.Wire(view, "logo", logo);
            ctx.Wire(view, "account", account);
            return rt.gameObject;
        }
    }
}
