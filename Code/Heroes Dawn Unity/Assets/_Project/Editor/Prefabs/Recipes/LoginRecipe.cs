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

            RectTransform logo;
            var titleSprite = Sprites.Title;
            if (titleSprite != null)
            {
                var img = Ui.Img(rt, "Title", Color.white, titleSprite);
                img.preserveAspect = true;
                logo = img.rectTransform.At(0.5f, 1f, 0, -20, 1000, 580);
            }
            else
            {
                var title = Ui.LocTxt(rt, "game.title", 128, Theme.Primary, TextAnchor.MiddleCenter, FontKind.Title, "Title");
                title.Inked();
                logo = title.rectTransform.At(0.5f, 1f, 0, -120, 1400, 200);
            }

            var tagline = Ui.LocTxt(rt, "game.tagline", 36, Theme.Text, TextAnchor.MiddleCenter, FontKind.Body, "Tagline");
            tagline.rectTransform.At(0.5f, 0f, 0, 250, 1400, 50);
            tagline.Inked();

            var play = Ui.LocBtn(rt, "login.play", Theme.Primary, Theme.TextDark, null!, 52, FontKind.Title, "Play");
            play.Rect.At(0.5f, 0f, 0, 120, 560, 110);
            var pulse = play.gameObject.AddComponent<Pulse>();
            pulse.Amplitude = 0.04f;
            pulse.Speed = 3.5f;

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
            Ui.LocTxt(rt, "game.build", 22, Theme.Navy, TextAnchor.LowerRight, FontKind.Regular, "Build").rectTransform.At(1f, 0f, -40, 70, 600, 40);

            var view = rt.gameObject.AddComponent<LoginView>();
            ScreenKit.WireFade(ctx, view);
            ctx.Wire(view, "play", play);
            ctx.Wire(view, "status", status);
            ctx.Wire(view, "spinner", spinner);
            ctx.Wire(view, "logo", logo);
            return rt.gameObject;
        }
    }
}
