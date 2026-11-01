using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Login;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>Vignette d'un avatar de la galerie (150 × 150) : cadre, numéro, bouton.</summary>
    public sealed class AvatarTileRecipe : IPrefabRecipe
    {
        public string RelativePath => "Widgets/AvatarTile.prefab";
        public int Order => 0;

        public GameObject Build(PrefabContext ctx)
        {
            var fill = RecipeKit.PanelRoot("AvatarTile", Theme.Frost, 5f, 8f, true);
            var rootRt = fill.Frame();
            rootRt.sizeDelta = new Vector2(150, 150);
            rootRt.Pref(150, 150);
            var frame = rootRt.GetComponent<Image>();
            var label = Ui.Txt(fill.transform, "", 60, Theme.Text, TextAnchor.MiddleCenter, FontKind.Title, "Number");
            label.rectTransform.Stretch();
            var button = rootRt.gameObject.AddComponent<Button>();
            button.targetGraphic = frame;
            button.transition = Selectable.Transition.None;
            var nav = button.navigation;
            nav.mode = Navigation.Mode.None;
            button.navigation = nav;

            var view = rootRt.gameObject.AddComponent<AvatarTileView>();
            ctx.Wire(view, "frame", frame);
            ctx.Wire(view, "label", label);
            ctx.Wire(view, "button", button);
            return rootRt.gameObject;
        }
    }

    /// <summary>Première connexion : popup plein écran (cinématique de lancement, création du personnage : nom et avatar).</summary>
    public sealed class IntroPopupRecipe : IPrefabRecipe
    {
        public string RelativePath => "Popups/IntroPopup.prefab";
        public int Order => 10;

        public GameObject Build(PrefabContext ctx)
        {
            var root = PrefabContext.NewRoot("IntroPopup");
            var group = root.gameObject.AddComponent<CanvasGroup>();
            var dim = Ui.Img(root, "Dim", Theme.Navy, null, true);
            dim.rectTransform.Stretch();

            // ---- Cinématique : le lecteur construit ses couches au lancement (vidéos et voix off) ; le prefab ne porte que le toucher et « Passer »
            var cinematic = Ui.Rect(root, "Cinematic");
            cinematic.Stretch();
            var player = cinematic.gameObject.AddComponent<CinematicVideoPlayer>();
            var tap = Ui.Img(cinematic, "Tap", new Color(0, 0, 0, 0), null, true);
            tap.rectTransform.Stretch();
            var tapButton = tap.gameObject.AddComponent<Button>();
            tapButton.transition = Selectable.Transition.None;
            var nav = tapButton.navigation;
            nav.mode = Navigation.Mode.None;
            tapButton.navigation = nav;
            var skip = Ui.LocBtn(cinematic, "intro.skip", Theme.WithAlpha(Theme.Navy, 0.7f), Theme.TextOnDark, null!, 30, FontKind.Title, "Skip");
            skip.Rect.At(1f, 1f, -20, -16, 170, 64);
            skip.Rect.pivot = Vector2.one;
            skip.Label.fontSize = 24f;

            // ---- Personnage : titre, nom saisi, galerie d'avatars, bouton « Commencer »
            var characterStage = Ui.Rect(root, "Character");
            characterStage.Stretch();
            var characterBg = Ui.Img(characterStage, "Bg", Theme.SkyMid, Sprites.VerticalGradient);
            characterBg.rectTransform.Stretch();
            characterBg.raycastTarget = false;
            var characterTitle = Ui.Txt(characterStage, "", 64, Theme.Text, TextAnchor.MiddleCenter, FontKind.Title, "Title");
            characterTitle.rectTransform.At(0.5f, 1f, 0, -60, 1400, 100);
            characterTitle.Inked();

            var nameBox = Ui.Img(characterStage, "NameBox", Theme.Surface3, Sprites.Panel, true);
            nameBox.rectTransform.At(0.5f, 0.5f, 0, 230, 720, 96);
            var viewport = Ui.Rect(nameBox.transform, "Viewport").Stretch(20, 8, 20, 8);
            viewport.gameObject.AddComponent<RectMask2D>();
            var nameText = Ui.Txt(viewport, "", 40, Theme.Text, TextAnchor.MiddleCenter, FontKind.Body, "Text");
            nameText.rectTransform.Stretch();
            var nameHint = Ui.Txt(viewport, "", 40, Theme.TextMuted, TextAnchor.MiddleCenter, FontKind.Body, "Hint");
            nameHint.rectTransform.Stretch();
            var nameField = nameBox.gameObject.AddComponent<TMP_InputField>();
            nameField.textViewport = viewport;
            nameField.textComponent = nameText;
            nameField.placeholder = nameHint;
            nameField.lineType = TMP_InputField.LineType.SingleLine;

            var avatarRow = Ui.Rect(characterStage, "Avatars");
            avatarRow.At(0.5f, 0.5f, 0, -80, 1100, 170);
            avatarRow.HLayout(30, 0, TextAnchor.MiddleCenter);
            var avatarPrefab = ctx.Asset("Widgets/AvatarTile.prefab").GetComponent<AvatarTileView>();

            var characterConfirm = Ui.LocBtn(characterStage, "onboarding.character.confirm", Theme.Primary, Theme.TextDark, null!, 46, FontKind.Title, "Confirm");
            characterConfirm.Rect.At(0.5f, 0f, 0, 60, 520, 104);
            characterStage.gameObject.SetActive(false);

            var view = root.gameObject.AddComponent<IntroPopupView>();
            ctx.Wire(view, "group", group);
            ctx.Wire(view, "cinematic", cinematic);
            ctx.Wire(view, "player", player);
            ctx.Wire(view, "tapArea", tapButton);
            ctx.Wire(view, "skip", skip);
            ctx.Wire(view, "characterStage", characterStage);
            ctx.Wire(view, "characterTitle", characterTitle);
            ctx.Wire(view, "nameField", nameField);
            ctx.Wire(view, "nameHint", nameHint);
            ctx.Wire(view, "avatarRow", avatarRow);
            ctx.Wire(view, "avatarPrefab", avatarPrefab);
            ctx.Wire(view, "characterConfirm", characterConfirm);
            return root.gameObject;
        }
    }
}
