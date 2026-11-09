#nullable enable
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>Carte d'un booster (300 × 420) : dos bleu nuit au départ, face à la couleur de la rareté, badge « Nouvelle ».</summary>
    public sealed class BoosterCardRecipe : IPrefabRecipe
    {
        public string RelativePath => "Widgets/BoosterCard.prefab";
        public int Order => 0;

        public GameObject Build(PrefabContext ctx)
        {
            var fill = RecipeKit.PanelRoot("BoosterCard", Theme.Surface, 1.5f, 3f, false);
            var rootRt = fill.Frame();
            rootRt.sizeDelta = new Vector2(340, 500);
            rootRt.Pref(340, 500);
            var frame = rootRt.GetComponent<Image>();

            // ---- Face
            var face = Ui.Rect(fill.transform, "Face").Stretch();
            var glyph = Ui.Txt(face, "A", 150, Theme.Text, TextAnchor.MiddleCenter, FontKind.Title, "Glyph");
            glyph.rectTransform.At(0.5f, 1f, 0, -40, 260, 170);
            var artHolder = Ui.Rect(face, "ArtHolder").At(.5f, 1f, 0, -24, 310, 280);
            artHolder.gameObject.AddComponent<RectMask2D>();
            var art = Ui.Img(artHolder, "Art", Color.white, null, false);
            art.gameObject.AddComponent<AspectRatioFitter>().aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            Ui.Img(face, "Caption", Theme.Surface, Sprites.Panel).rectTransform.BottomBand(184);
            var name = Ui.Txt(face, "", 34, Theme.Text, TextAnchor.MiddleCenter, FontKind.Title, "Name");
            name.rectTransform.At(0.5f, 0f, 0, 126, 308, 56);
            name.AutoSize(34);
            var rarity = Ui.Txt(face, "", 24, Theme.Text, TextAnchor.MiddleCenter, FontKind.Body, "Rarity");
            rarity.rectTransform.At(0.5f, 0f, 0, 96, 308, 32);
            var description = Ui.Txt(face, "", 22, Theme.Text, TextAnchor.UpperCenter, FontKind.Body, "Description");
            description.rectTransform.At(0.5f, 0f, 0, 10, 308, 82);
            description.AutoSize(22);
            var badgeFill = Ui.Panel(face, "NewBadge", Theme.Accent, 3f, 4f, false);
            badgeFill.Frame().At(0.5f, 1f, 0, -8, 230, 44);
            var newText = Ui.Txt(badgeFill.transform, "", 26, Theme.TextDark, TextAnchor.MiddleCenter, FontKind.Title, "Text");
            newText.rectTransform.Stretch();

            // ---- Dos
            var back = Ui.Img(fill.transform, "Back", Theme.Navy, Sprites.Panel);
            back.rectTransform.Stretch();
            var mark = Ui.Img(back.transform, "Mark", Theme.Accent, Sprites.DawnStar);
            mark.rectTransform.At(.5f, .5f, 0, 0, 130, 180);

            var view = rootRt.gameObject.AddComponent<BoosterCardView>();
            ctx.Wire(view, "frame", frame);
            ctx.Wire(view, "faceRoot", face.gameObject);
            ctx.Wire(view, "back", back.gameObject);
            ctx.Wire(view, "glyph", glyph);
            ctx.Wire(view, "art", art);
            ctx.Wire(view, "description", description);
            ctx.Wire(view, "cardName", name);
            ctx.Wire(view, "rarity", rarity);
            ctx.Wire(view, "newBadge", badgeFill.Frame().gameObject);
            ctx.Wire(view, "newText", newText);
            return rootRt.gameObject;
        }
    }

    /// <summary>Ouverture d'un booster : fond sombre cliquable, paquet, flash, rangée de cartes, Passer, Continuer.</summary>
    public sealed class BoosterOpeningPopupRecipe : IPrefabRecipe
    {
        public string RelativePath => "Popups/BoosterOpeningPopup.prefab";
        public int Order => 10;

        public GameObject Build(PrefabContext ctx)
        {
            var root = PrefabContext.NewRoot("BoosterOpeningPopup");
            Ui.Img(root, "Backdrop", Theme.Background).rectTransform.Stretch();
            ScreenKit.ArtBackdrop(root, TacticalArt.Load("hq-new-york"), .45f, .6f);
            var group = root.gameObject.AddComponent<CanvasGroup>();
            var dim = Ui.Img(root, "Dim", Theme.WithAlpha(Theme.Navy, 0.35f), null, true);
            dim.rectTransform.Stretch();
            var tap = dim.gameObject.AddComponent<Button>();
            tap.transition = Selectable.Transition.None;
            var nav = tap.navigation;
            nav.mode = Navigation.Mode.None;
            tap.navigation = nav;

            var title = Ui.LocTxt(root, "booster.title", 64, Theme.Glow, TextAnchor.MiddleCenter, FontKind.Title, "Title");
            title.rectTransform.At(0.5f, 1f, 0, -50, 1200, 90);
            title.Inked();

            // ---- Paquet : étoile lumineuse et carte scellée
            var pack = Ui.Rect(root, "Pack").At(0.5f, 0.5f, 0, 10, 380, 500);
            var burst = Ui.Img(pack, "Burst", Theme.WithAlpha(Theme.Glow, 0.18f), Sprites.Burst);
            burst.rectTransform.Stretch(-90, -90, -90, -90);
            burst.raycastTarget = false;
            var packFill = Ui.Panel(pack, "Body", Theme.Background, 1.5f, 4f, false);
            packFill.Frame().Stretch();
            var packArt = Ui.Img(packFill.transform, "PacketArt", Color.white, TacticalArt.Load("shop-booster"));
            packArt.preserveAspect = true;
            packArt.rectTransform.Stretch(8, 8, 8, 8);

            // ---- Cartes
            var row = Ui.Rect(root, "Cards").At(0.5f, 0.5f, 0, 10, 1600, 520);
            row.HLayout(40, 0, TextAnchor.MiddleCenter);
            var cardPrefab = ctx.Asset("Widgets/BoosterCard.prefab").GetComponent<BoosterCardView>();

            var hint = Ui.LocTxt(root, "booster.tap", 44, Theme.TextOnDark, TextAnchor.MiddleCenter, FontKind.Title, "Hint");
            hint.rectTransform.At(0.5f, 0f, 0, 110, 1000, 70);
            hint.gameObject.AddComponent<Pulse>().Amplitude = 0.04f;

            var skip = Ui.LocBtn(root, "booster.skip", Theme.Surface2, Theme.Text, null!, 30, FontKind.Title, "Skip");
            skip.Rect.At(1f, 1f, -40, -40, 260, 72);
            var cont = Ui.LocBtn(root, "ui.continue", Theme.Primary, Theme.TextDark, null!, 44, FontKind.Title, "Continue");
            cont.Rect.At(1f, 0f, -80, 50, 460, 104);

            var flash = Ui.Img(root, "Flash", new Color(1f, 1f, 1f, 0f), null, false);
            flash.rectTransform.Stretch();

            var view = root.gameObject.AddComponent<BoosterOpeningPopupView>();
            ctx.Wire(view, "group", group);
            ctx.Wire(view, "tapArea", tap);
            ctx.Wire(view, "pack", pack);
            ctx.Wire(view, "flash", flash);
            ctx.Wire(view, "title", title);
            ctx.Wire(view, "hint", hint);
            ctx.Wire(view, "cardRow", row);
            ctx.Wire(view, "cardPrefab", cardPrefab);
            ctx.Wire(view, "skip", skip);
            ctx.Wire(view, "continueButton", cont);
            return root.gameObject;
        }
    }
}
