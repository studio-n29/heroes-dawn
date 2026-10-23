#nullable enable
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Login;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>Carte d'une campagne (300 × 420) : nom, description et badge « disponible / en création ».</summary>
    public sealed class CampaignCardRecipe : IPrefabRecipe
    {
        public string RelativePath => "Widgets/CampaignCard.prefab";
        public int Order => 0;

        public GameObject Build(PrefabContext ctx)
        {
            var fill = RecipeKit.PanelRoot("CampaignCard", Theme.Frost, 5f, 8f, true);
            var rootRt = fill.Frame();
            rootRt.sizeDelta = new Vector2(300, 420);
            rootRt.Pref(300, 420);
            var frame = rootRt.GetComponent<Image>();
            var name = Ui.Txt(fill.transform, "", 40, Theme.Text, TextAnchor.UpperCenter, FontKind.Title, "Name");
            name.rectTransform.At(0.5f, 1f, 0, -26, 270, 120);
            name.AutoSize(40);
            var desc = Ui.Txt(fill.transform, "", 26, Theme.TextDark, TextAnchor.UpperCenter, FontKind.Body, "Desc");
            desc.rectTransform.At(0.5f, 0.5f, 0, -20, 260, 190);
            var badge = Ui.Txt(fill.transform, "", 28, Theme.Success, TextAnchor.MiddleCenter, FontKind.Title, "Badge");
            badge.rectTransform.At(0.5f, 0f, 0, 36, 270, 50);
            var button = rootRt.gameObject.AddComponent<Button>();
            button.targetGraphic = frame;
            button.transition = Selectable.Transition.None;
            var nav = button.navigation;
            nav.mode = Navigation.Mode.None;
            button.navigation = nav;

            var view = rootRt.gameObject.AddComponent<CampaignCardView>();
            ctx.Wire(view, "frame", frame);
            ctx.Wire(view, "fill", fill);
            ctx.Wire(view, "cardName", name);
            ctx.Wire(view, "description", desc);
            ctx.Wire(view, "badge", badge);
            ctx.Wire(view, "button", button);
            return rootRt.gameObject;
        }
    }

    /// <summary>Première connexion : popup plein écran (cinématique d'introduction, pseudo narratif, choix de la campagne).</summary>
    public sealed class IntroPopupRecipe : IPrefabRecipe
    {
        public string RelativePath => "Popups/IntroPopup.prefab";
        public int Order => 10;

        const int EmberCount = 18;

        public GameObject Build(PrefabContext ctx)
        {
            var root = PrefabContext.NewRoot("IntroPopup");
            var group = root.gameObject.AddComponent<CanvasGroup>();
            var dim = Ui.Img(root, "Dim", Theme.Navy, null, true);
            dim.rectTransform.Stretch();

            // ---- Cinématique : fond (dégradé ou illustration), motif provisoire, bandes de cinéma, braises, texte
            var cinematic = Ui.Rect(root, "Cinematic");
            cinematic.Stretch();
            var artRoot = Ui.Rect(cinematic, "Art");
            artRoot.Stretch();
            var back = Ui.Img(artRoot, "Back", Theme.SkyMid, null);
            back.rectTransform.Stretch(-60, -60, -60, -60);
            back.raycastTarget = false;
            var top = Ui.Img(artRoot, "Top", Theme.Navy, Sprites.VerticalGradient);
            top.rectTransform.Stretch(-60, -60, -60, -60);
            top.raycastTarget = false;
            var picture = Ui.Img(artRoot, "Picture", Color.white, null);
            picture.rectTransform.Stretch();
            picture.preserveAspect = false;
            picture.raycastTarget = false;
            picture.gameObject.SetActive(false);
            var motifs = new Object[9];
            for (int i = 0; i < motifs.Length; i++) motifs[i] = BuildMotif(artRoot, (IntroMotif)i);
            var barTop = Ui.Img(artRoot, "BarTop", new Color(0f, 0f, 0f, 0.85f), null);
            barTop.rectTransform.At(0.5f, 1f, 0, 0, 4000, 110);
            barTop.rectTransform.pivot = new Vector2(0.5f, 1f);
            barTop.raycastTarget = false;
            var barBottom = Ui.Img(artRoot, "BarBottom", new Color(0f, 0f, 0f, 0.85f), null);
            barBottom.rectTransform.At(0.5f, 0f, 0, 0, 4000, 250);
            barBottom.rectTransform.pivot = new Vector2(0.5f, 0f);
            barBottom.raycastTarget = false;

            var embers = Ui.Rect(cinematic, "Embers");
            embers.Stretch();
            var emberViews = new Object[EmberCount];
            for (int i = 0; i < EmberCount; i++)
            {
                var e = Ui.Img(embers, "Ember" + i, new Color(1f, 0.72f, 0.3f, 0f), Sprites.Circle);
                float s = 6f + (i * 7 % 11);
                e.rectTransform.sizeDelta = new Vector2(s, s);
                e.raycastTarget = false;
                emberViews[i] = e.rectTransform;
            }
            var tap = Ui.Img(cinematic, "Tap", new Color(0, 0, 0, 0), null, true);
            tap.rectTransform.Stretch();
            var tapButton = tap.gameObject.AddComponent<Button>();
            tapButton.transition = Selectable.Transition.None;
            var nav = tapButton.navigation;
            nav.mode = Navigation.Mode.None;
            tapButton.navigation = nav;
            var caption = Ui.Txt(cinematic, "", 52, Color.white, TextAnchor.MiddleCenter, FontKind.Title, "Caption");
            caption.rectTransform.At(0.5f, 0f, 0, 150, 1500, 180);
            caption.Inked();
            var hint = Ui.Txt(cinematic, "", 30, Theme.Glow, TextAnchor.MiddleCenter, FontKind.Body, "TapHint");
            hint.rectTransform.At(0.5f, 0f, 0, 50, 900, 50);
            hint.gameObject.AddComponent<Pulse>().Amplitude = 0.04f;
            var skip = Ui.LocBtn(cinematic, "intro.skip", Theme.WithAlpha(Theme.Navy, 0.7f), Theme.TextOnDark, null!, 30, FontKind.Title, "Skip");
            skip.Rect.At(1f, 1f, -40, -40, 260, 72);
            var blackout = Ui.Img(cinematic, "Blackout", Color.black, null, false);
            blackout.rectTransform.Stretch();

            // ---- Pseudo : dialogue avec l'accueil de l'Aube
            var pseudoStage = Ui.Rect(root, "Pseudo");
            pseudoStage.Stretch();
            var pseudoBg = Ui.Img(pseudoStage, "Bg", Theme.SkyTop, Sprites.VerticalGradient);
            pseudoBg.rectTransform.Stretch();
            pseudoBg.raycastTarget = false;
            var panel = Ui.Panel(pseudoStage, "Panel", Theme.Frost, 5f, 10f, true);
            panel.Frame().At(0.5f, 0.5f, 0, 0, 1300, 560);
            var speaker = Ui.Txt(panel.transform, "", 40, Theme.Primary, TextAnchor.UpperLeft, FontKind.Title, "Speaker");
            speaker.rectTransform.At(0f, 1f, 50, -30, 1000, 60);
            var line = Ui.Txt(panel.transform, "", 38, Theme.TextDark, TextAnchor.UpperLeft, FontKind.Body, "Line");
            line.rectTransform.At(0f, 1f, 50, -100, 1200, 130);
            var input = BuildInput(panel.transform);
            input.GetComponent<RectTransform>().At(0.5f, 0.5f, 0, -60, 760, 90);
            var error = Ui.Txt(panel.transform, "", 28, Theme.Danger, TextAnchor.MiddleCenter, FontKind.Body, "Error");
            error.rectTransform.At(0.5f, 0.5f, 0, -140, 1100, 44);
            var pseudoConfirm = Ui.LocBtn(panel.transform, "onboarding.pseudo.confirm", Theme.Primary, Theme.TextDark, null!, 44, FontKind.Title, "Confirm");
            pseudoConfirm.Rect.At(0.5f, 0f, 0, 40, 460, 96);
            pseudoStage.gameObject.SetActive(false);

            // ---- Campagne
            var campaignStage = Ui.Rect(root, "Campaign");
            campaignStage.Stretch();
            var campaignBg = Ui.Img(campaignStage, "Bg", Theme.SkyMid, Sprites.VerticalGradient);
            campaignBg.rectTransform.Stretch();
            campaignBg.raycastTarget = false;
            var campaignTitle = Ui.Txt(campaignStage, "", 64, Theme.Text, TextAnchor.MiddleCenter, FontKind.Title, "Title");
            campaignTitle.rectTransform.At(0.5f, 1f, 0, -60, 1400, 100);
            campaignTitle.Inked();
            var cardRow = Ui.Rect(campaignStage, "Cards");
            cardRow.At(0.5f, 0.5f, 0, 10, 1400, 440);
            cardRow.HLayout(30, 0, TextAnchor.MiddleCenter);
            var cardPrefab = ctx.Asset("Widgets/CampaignCard.prefab").GetComponent<CampaignCardView>();
            var campaignConfirm = Ui.LocBtn(campaignStage, "onboarding.campaign.confirm", Theme.Primary, Theme.TextDark, null!, 46, FontKind.Title, "Confirm");
            campaignConfirm.Rect.At(0.5f, 0f, 0, 60, 520, 104);
            campaignStage.gameObject.SetActive(false);

            var view = root.gameObject.AddComponent<IntroPopupView>();
            ctx.Wire(view, "group", group);
            ctx.Wire(view, "cinematic", cinematic);
            ctx.Wire(view, "artRoot", artRoot);
            ctx.Wire(view, "artBack", back);
            ctx.Wire(view, "artTop", top);
            ctx.Wire(view, "artPicture", picture);
            ctx.WireArray(view, "motifs", motifs);
            ctx.Wire(view, "embers", embers);
            ctx.WireArray(view, "emberViews", emberViews);
            ctx.Wire(view, "blackout", blackout);
            ctx.Wire(view, "caption", caption);
            ctx.Wire(view, "tapHint", hint);
            ctx.Wire(view, "tapArea", tapButton);
            ctx.Wire(view, "skip", skip);
            ctx.Wire(view, "pseudoStage", pseudoStage);
            ctx.Wire(view, "speaker", speaker);
            ctx.Wire(view, "line", line);
            ctx.Wire(view, "input", input);
            ctx.Wire(view, "inputError", error);
            ctx.Wire(view, "pseudoConfirm", pseudoConfirm);
            ctx.Wire(view, "campaignStage", campaignStage);
            ctx.Wire(view, "campaignTitle", campaignTitle);
            ctx.Wire(view, "cardRow", cardRow);
            ctx.Wire(view, "cardPrefab", cardPrefab);
            ctx.Wire(view, "campaignConfirm", campaignConfirm);
            return root.gameObject;
        }

        static Image Disc(Transform parent, string name, Sprite sprite, Color c, float x, float y, float w, float h, float rot = 0f)
        {
            var img = Ui.Img(parent, name, c, sprite);
            img.rectTransform.At(0.5f, 0.5f, x, y, w, h);
            img.rectTransform.localRotation = Quaternion.Euler(0, 0, rot);
            img.raycastTarget = false;
            return img;
        }

        /// <summary>Motif provisoire d'un écran de la cinématique (formes translucides claires et silhouettes sombres).</summary>
        static RectTransform BuildMotif(Transform parent, IntroMotif m)
        {
            var g = Ui.Rect(parent, "Motif_" + m);
            g.Stretch();
            var glow = new Color(1f, 0.94f, 0.82f, 0.55f);
            var dark = new Color(0.06f, 0.05f, 0.12f, 0.97f);
            switch (m)
            {
                case IntroMotif.Battle:
                    Disc(g, "Burst", Sprites.Burst, glow, 0, 40, 980, 980);
                    for (int i = -3; i <= 3; i++) Disc(g, "Hero" + i, Sprites.Bust, dark, i * 190, -120 - Mathf.Abs(i) * 14, 170, 260);
                    break;
                case IntroMotif.Wormhole:
                    for (int i = 0; i < 6; i++) Disc(g, "Ring" + i, Sprites.Ring, new Color(glow.r, glow.g, glow.b, 0.8f - i * 0.1f), 0, 20, 220 + i * 150, 220 + i * 150, i * 18f);
                    Disc(g, "Core", Sprites.Circle, new Color(0.04f, 0.02f, 0.08f, 1f), 0, 20, 170, 170);
                    break;
                case IntroMotif.Arrival:
                    Disc(g, "Halo", Sprites.Circle, new Color(glow.r, glow.g, glow.b, 0.35f), 0, 20, 760, 760);
                    Disc(g, "Figure", Sprites.Bust, dark, 0, -60, 260, 400);
                    break;
                case IntroMotif.Phoenix:
                    Disc(g, "Burst", Sprites.Burst, new Color(1f, 0.82f, 0.3f, 0.75f), 0, 40, 1100, 1100);
                    Disc(g, "WingL", Sprites.Cloud, new Color(1f, 0.78f, 0.2f, 0.9f), -300, 60, 520, 300, 25f);
                    Disc(g, "WingR", Sprites.Cloud, new Color(1f, 0.78f, 0.2f, 0.9f), 300, 60, 520, 300, -25f);
                    Disc(g, "Body", Sprites.Bust, dark, 0, -20, 240, 360);
                    break;
                case IntroMotif.Farewell:
                    Disc(g, "Halo", Sprites.Circle, new Color(glow.r, glow.g, glow.b, 0.3f), 0, 0, 900, 900);
                    Disc(g, "Eda", Sprites.Bust, dark, -170, -70, 220, 340);
                    Disc(g, "Rayner", Sprites.Bust, new Color(0.1f, 0.12f, 0.2f, 1f), 170, -80, 200, 320);
                    break;
                case IntroMotif.Portal:
                    Disc(g, "Nova", Sprites.Burst, new Color(1f, 0.9f, 0.6f, 0.95f), 0, 40, 1200, 1200);
                    Disc(g, "Core", Sprites.Circle, Color.white, 0, 40, 260, 260);
                    break;
                case IntroMotif.Wave:
                    for (int i = 0; i < 5; i++) Disc(g, "Wave" + i, Sprites.Ring, new Color(glow.r, glow.g, glow.b, 0.7f - i * 0.12f), 0, -80, 400 + i * 260, 400 + i * 260);
                    Disc(g, "Globe", Sprites.Circle, dark, 0, -80, 300, 300);
                    break;
                case IntroMotif.Dna:
                    for (int i = 0; i < 12; i++)
                    {
                        float y = -300 + i * 56;
                        float x = Mathf.Sin(i * 0.7f) * 130;
                        Disc(g, "A" + i, Sprites.Circle, new Color(glow.r, glow.g, glow.b, 0.9f), x, y, 40, 40);
                        Disc(g, "B" + i, Sprites.Circle, new Color(1f, 1f, 1f, 0.8f), -x, y, 40, 40);
                    }
                    break;
                default:
                    Disc(g, "Skyline", Sprites.Skyline, new Color(0.07f, 0.1f, 0.22f, 1f), 0, -150, 1500, 380);
                    Disc(g, "Beacon", Sprites.Burst, glow, 0, 120, 640, 640);
                    break;
            }
            g.gameObject.SetActive(false);
            return g;
        }

        /// <summary>Champ de saisie TextMeshPro : fond clair, texte et texte d'indication.</summary>
        static TMP_InputField BuildInput(Transform parent)
        {
            var bg = Ui.Panel(parent, "Input", Color.white, 4f, 0f, true);
            var frame = bg.Frame();
            var viewport = Ui.Rect(bg.transform, "Viewport");
            viewport.Stretch(24, 8, 24, 8);
            viewport.gameObject.AddComponent<RectMask2D>();
            var text = Ui.Txt(viewport, "", 44, Theme.Text, TextAnchor.MiddleLeft, FontKind.Body, "Text");
            text.rectTransform.Stretch();
            var placeholder = Ui.Txt(viewport, "", 44, Theme.TextMuted, TextAnchor.MiddleLeft, FontKind.Body, "Placeholder");
            placeholder.rectTransform.Stretch();
            placeholder.fontStyle = FontStyles.Italic;
            var field = frame.gameObject.AddComponent<TMP_InputField>();
            field.targetGraphic = bg;
            field.textViewport = viewport;
            field.textComponent = text;
            field.placeholder = placeholder;
            field.characterLimit = 16;
            field.lineType = TMP_InputField.LineType.SingleLine;
            field.fontAsset = Ui.BodyFont;
            field.pointSize = 44;
            return field;
        }
    }
}
