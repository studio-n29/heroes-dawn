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
            var fill = RecipeKit.PanelRoot("BoosterCard", Theme.Frost, 5f, 8f, false);
            var rootRt = fill.Frame();
            rootRt.sizeDelta = new Vector2(300, 420);
            rootRt.Pref(300, 420);
            var frame = rootRt.GetComponent<Image>();

            // ---- Face
            var face = Ui.Rect(fill.transform, "Face").Stretch();
            var shine = Ui.Img(face, "Shine", new Color(1f, 1f, 1f, 0.45f), Sprites.Shine);
            shine.rectTransform.Stretch();
            var glyph = Ui.Txt(face, "A", 150, Theme.Navy, TextAnchor.MiddleCenter, FontKind.Title, "Glyph");
            glyph.rectTransform.At(0.5f, 1f, 0, -40, 260, 200);
            var name = Ui.Txt(face, "", 40, Theme.Navy, TextAnchor.MiddleCenter, FontKind.Title, "Name");
            name.rectTransform.At(0.5f, 0f, 0, 78, 270, 100);
            name.AutoSize(40);
            var rarity = Ui.Txt(face, "", 28, Theme.Navy, TextAnchor.MiddleCenter, FontKind.Body, "Rarity");
            rarity.rectTransform.At(0.5f, 0f, 0, 28, 270, 40);
            var badgeFill = Ui.Panel(face, "NewBadge", Theme.Accent, 3f, 4f, false);
            badgeFill.Frame().At(0.5f, 1f, 0, -8, 230, 44);
            var newText = Ui.Txt(badgeFill.transform, "", 26, Theme.TextOnDark, TextAnchor.MiddleCenter, FontKind.Title, "Text");
            newText.rectTransform.Stretch();

            // ---- Dos
            var back = Ui.Img(fill.transform, "Back", Theme.Navy, Sprites.Panel);
            back.rectTransform.Stretch();
            var backShine = Ui.Img(back.transform, "Shine", new Color(1f, 1f, 1f, 0.2f), Sprites.Shine);
            backShine.rectTransform.Stretch();
            var mark = Ui.Txt(back.transform, "?", 160, Theme.Glow, TextAnchor.MiddleCenter, FontKind.Title, "Mark");
            mark.rectTransform.Stretch();

            var view = rootRt.gameObject.AddComponent<BoosterCardView>();
            ctx.Wire(view, "frame", frame);
            ctx.Wire(view, "faceRoot", face.gameObject);
            ctx.Wire(view, "back", back.gameObject);
            ctx.Wire(view, "glyph", glyph);
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
            var group = root.gameObject.AddComponent<CanvasGroup>();
            var dim = Ui.Img(root, "Dim", Theme.WithAlpha(Theme.Navy, 0.94f), null, true);
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
            var burst = Ui.Img(pack, "Burst", Theme.WithAlpha(Theme.Glow, 0.55f), Sprites.Burst);
            burst.rectTransform.Stretch(-120, -120, -120, -120);
            burst.raycastTarget = false;
            var packFill = Ui.Panel(pack, "Body", Theme.Accent, 5f, 10f, false);
            packFill.Frame().Stretch();
            var packShine = Ui.Img(packFill.transform, "Shine", new Color(1f, 1f, 1f, 0.4f), Sprites.Shine);
            packShine.rectTransform.Stretch();
            var packMark = Ui.Txt(packFill.transform, "B", 220, Theme.TextOnDark, TextAnchor.MiddleCenter, FontKind.Title, "Mark");
            packMark.rectTransform.Stretch();
            packMark.Inked();

            // ---- Cartes
            var row = Ui.Rect(root, "Cards").At(0.5f, 0.5f, 0, 10, 1400, 440);
            row.HLayout(40, 0, TextAnchor.MiddleCenter);
            var cardPrefab = ctx.Asset("Widgets/BoosterCard.prefab").GetComponent<BoosterCardView>();

            var hint = Ui.LocTxt(root, "booster.tap", 44, Theme.TextOnDark, TextAnchor.MiddleCenter, FontKind.Title, "Hint");
            hint.rectTransform.At(0.5f, 0f, 0, 110, 1000, 70);
            hint.gameObject.AddComponent<Pulse>().Amplitude = 0.04f;

            var skip = Ui.LocBtn(root, "booster.skip", Theme.Surface2, Theme.Navy, null!, 30, FontKind.Title, "Skip");
            skip.Rect.At(1f, 1f, -40, -40, 260, 72);
            var cont = Ui.LocBtn(root, "ui.continue", Theme.Accent, Theme.TextOnDark, null!, 44, FontKind.Title, "Continue");
            cont.Rect.At(0.5f, 0f, 0, 50, 460, 104);

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
