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

    /// <summary>Première connexion : popup plein écran (cinématique de lancement, choix du personnage joueur).</summary>
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

            // ---- Cinématique : le lecteur construit ses couches au lancement ; le prefab ne porte que le toucher et « Passer »
            var cinematic = Ui.Rect(root, "Cinematic");
            cinematic.Stretch();
            var player = cinematic.gameObject.AddComponent<CinematicPlayer>();
            var tap = Ui.Img(cinematic, "Tap", new Color(0, 0, 0, 0), null, true);
            tap.rectTransform.Stretch();
            var tapButton = tap.gameObject.AddComponent<Button>();
            tapButton.transition = Selectable.Transition.None;
            var nav = tapButton.navigation;
            nav.mode = Navigation.Mode.None;
            tapButton.navigation = nav;
            var skip = Ui.LocBtn(cinematic, "intro.skip", Theme.WithAlpha(Theme.Navy, 0.7f), Theme.TextOnDark, null!, 30, FontKind.Title, "Skip");
            skip.Rect.At(1f, 1f, -40, -40, 260, 96);

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
            ctx.Wire(view, "player", player);
            ctx.Wire(view, "tapArea", tapButton);
            ctx.Wire(view, "skip", skip);
            ctx.Wire(view, "campaignStage", campaignStage);
            ctx.Wire(view, "campaignTitle", campaignTitle);
            ctx.Wire(view, "cardRow", cardRow);
            ctx.Wire(view, "cardPrefab", cardPrefab);
            ctx.Wire(view, "campaignConfirm", campaignConfirm);
            return root.gameObject;
        }

    }
}
