#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Popups;
using N29.HeroesDawn.Presentation.Tutorial;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>
    /// Carte de la boutique (M30 adapté), 400 × 500 dans un emplacement de 540 de haut (place pour la Légende surélevée) :
    /// halo derrière, cadre de rareté, corps estompé une fois recruté, tampon « Dans l'équipe ».
    /// </summary>
    public sealed class ShopCardRecipe : IPrefabRecipe
    {
        public string RelativePath => "Widgets/ShopCard.prefab";
        public int Order => 6;

        public GameObject Build(PrefabContext ctx)
        {
            var root = PrefabContext.NewRoot("ShopCard", 300, 560);
            root.Pref(300, 560);
            var halo = Ui.Img(root, "Halo", Theme.WithAlpha(Theme.Primary, 0.3f), Sprites.Rounded);
            halo.type = Image.Type.Sliced;
            halo.rectTransform.At(0.5f, 0.5f, 0, 0, 328, 548);
            var haloPulse = halo.gameObject.AddComponent<Pulse>();
            haloPulse.Amplitude = 0.035f;
            haloPulse.Speed = 3f;
            halo.gameObject.SetActive(false);
            var card = Ui.Rect(root, "Card").At(0.5f, 0.5f, 0, 0, 300, 520);
            var rim = Ui.Img(card, "Rim", Theme.Primary, Sprites.Rounded, true);
            rim.type = Image.Type.Sliced;
            rim.rectTransform.Stretch();
            var fill = Ui.Img(card, "Fill", Theme.Surface, Sprites.Rounded, true);
            fill.type = Image.Type.Sliced;
            fill.rectTransform.Stretch(6, 6, 6, 6);
            var tap = card.gameObject.AddComponent<Button>();
            tap.targetGraphic = fill;
            var nav = tap.navigation;
            nav.mode = Navigation.Mode.None;
            tap.navigation = nav;
            var body = fill.gameObject.AddComponent<CanvasGroup>();
            // Portrait pleine hauteur (planche 10.1) : l'illustration remplit la carte, cadrée sur le visage ; nom et prix en bas.
            var holder = Ui.Rect(fill.transform, "ArtHolder").Stretch();
            holder.gameObject.AddComponent<RectMask2D>();
            var bigArt = Ui.Img(holder, "Art", Color.white, null);
            bigArt.raycastTarget = false;
            bigArt.gameObject.SetActive(false);
            var shade = Ui.Img(holder, "Shade", Theme.WithAlpha(Theme.Outline, 0.92f), Sprites.VerticalGradient);
            shade.raycastTarget = false;
            shade.rectTransform.BottomBand(300);
            var strip = Ui.Img(fill.transform, "Family", Theme.Primary, Sprites.Rounded);
            strip.rectTransform.TopBand(8, 0);
            var familyBadge = Ui.Img(fill.transform, "FamilyBadge", Theme.Primary, Sprites.Circle);
            familyBadge.rectTransform.At(0f, 1f, 14, -24, 44, 44);
            familyBadge.raycastTarget = false;
            var portrait = ctx.Nest<PortraitView>("Widgets/Portrait.prefab", fill.transform, "Portrait");
            ((RectTransform)portrait.transform).At(0.5f, 1f, 0, -20, 240, 190);
            portrait.gameObject.SetActive(false); // repli : silhouette quand l'agent n'a pas d'illustration (voir ShopCardView)
            var name = Ui.Txt(fill.transform, "", 40, Theme.Text, TextAnchor.MiddleCenter, FontKind.Title, "Name");
            name.rectTransform.At(0.5f, 0f, 0, 150, 276, 48);
            name.AutoSize(40);
            name.Inked();
            var rarity = Ui.Txt(fill.transform, "", 24, Theme.Primary, TextAnchor.MiddleCenter, FontKind.Title, "Rarity");
            rarity.rectTransform.At(0.5f, 0f, 0, 120, 276, 30);
            var role = Ui.Txt(fill.transform, "", 22, Theme.TextMuted, TextAnchor.MiddleCenter, FontKind.Regular, "Role");
            role.rectTransform.At(0.5f, 1f, 0, -318, 360, 28);
            role.gameObject.SetActive(false); // détails dans le panneau de droite
            var stats = Ui.Txt(fill.transform, "", 20, Theme.Text, TextAnchor.MiddleCenter, FontKind.Regular, "Stats");
            stats.rectTransform.At(0.5f, 1f, 0, -350, 360, 28);
            stats.gameObject.SetActive(false);
            var skills = Ui.Txt(fill.transform, "", 20, Theme.TextMuted, TextAnchor.UpperCenter, FontKind.Regular, "Skills");
            skills.rectTransform.At(0.5f, 1f, 0, -380, 360, 20);
            skills.gameObject.SetActive(false);
            var recruit = Ui.Btn(fill.transform, "", Theme.Primary, Theme.TextDark, null!, 30, FontKind.Title, "Recruit");
            recruit.Rect.At(0.5f, 0f, 0, 12, 268, 96);
            RecipeKit.Mark(recruit, TutorialTarget.RecruitCard);
            var inTeam = Ui.Txt(card, "", 46, Theme.Primary, TextAnchor.MiddleCenter, FontKind.Title, "InTeam");
            inTeam.rectTransform.At(0.5f, 0f, 0, 30, 360, 80);
            inTeam.rectTransform.localRotation = Quaternion.Euler(0, 0, 8);
            inTeam.Inked();
            inTeam.gameObject.SetActive(false);
            var select = Ui.Img(card, "Selection", Theme.Accent, Sprites.Rounded);
            select.type = Image.Type.Sliced;
            select.raycastTarget = false;
            select.rectTransform.Stretch(-8, -8, -8, -8);
            select.transform.SetAsFirstSibling();
            select.gameObject.SetActive(false);
            var view = root.gameObject.AddComponent<ShopCardView>();
            ctx.Wire(view, "card", card);
            ctx.Wire(view, "tap", tap);
            ctx.Wire(view, "rim", rim);
            ctx.Wire(view, "halo", halo);
            ctx.Wire(view, "strip", strip);
            ctx.Wire(view, "portrait", portrait);
            ctx.Wire(view, "memberName", name);
            ctx.Wire(view, "rarity", rarity);
            ctx.Wire(view, "role", role);
            ctx.Wire(view, "stats", stats);
            ctx.Wire(view, "skills", skills);
            ctx.Wire(view, "recruit", recruit);
            ctx.Wire(view, "inTeam", inTeam);
            ctx.Wire(view, "body", body);
            ctx.Wire(view, "bigArt", bigArt);
            ctx.Wire(view, "familyBadge", familyBadge);
            ctx.Wire(view, "selection", select);
            return root.gameObject;
        }
    }

    /// <summary>Page Recruter : règle et compte à rebours en haut, 3 cartes au centre, progression et Renouveler en bas.</summary>
    public sealed class RecruitPageRecipe : IPrefabRecipe
    {
        public string RelativePath => "Pages/Recruit.prefab";
        public int Order => 10;

        public GameObject Build(PrefabContext ctx) => N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<RecruitPageView>("RecruitPage", "Recruit/Recruit.uxml", 10);
    }

    /// <summary>Confirmation d'achat (M31) : héros et synergie phare à gauche, solde avant / coût / après à droite, Annuler / Recruter.</summary>
    public sealed class RecruitConfirmPopupRecipe : IPrefabRecipe
    {
        public string RelativePath => "Popups/RecruitConfirmPopup.prefab";
        public int Order => 10;

        public GameObject Build(PrefabContext ctx)
        {
            var root = PrefabContext.NewRoot("RecruitConfirmPopup");
            var group = root.gameObject.AddComponent<CanvasGroup>();
            var (window, content, title) = PopupKit.Window(root, 1180, 660, Theme.Pr);

            // ---- Héros
            var portrait = ctx.Nest<PortraitView>("Widgets/Portrait.prefab", content, "Portrait");
            ((RectTransform)portrait.transform).At(0f, 1f, 0, -6, 140, 140);
            var name = Ui.Txt(content, "", 46, Color.white, TextAnchor.MiddleLeft, FontKind.Title, "Name");
            name.rectTransform.At(0f, 1f, 160, -4, 470, 54);
            name.AutoSize(46);
            name.Inked();
            var rarity = Ui.Txt(content, "", 28, Theme.Primary, TextAnchor.MiddleLeft, FontKind.Title, "Rarity");
            rarity.rectTransform.At(0f, 1f, 160, -60, 470, 34);
            var role = Ui.Txt(content, "", 24, Theme.TextMuted, TextAnchor.MiddleLeft, FontKind.Regular, "Role");
            role.rectTransform.At(0f, 1f, 160, -96, 470, 30);
            var synergy = Ui.Txt(content, "", 26, Theme.Text, TextAnchor.UpperLeft, FontKind.Body, "Synergy");
            synergy.rectTransform.At(0f, 1f, 0, -166, 620, 70);

            // ---- Solde
            var before = Line(content, "recruit.confirm.before", "Before", 0, Theme.Text);
            var cost = Line(content, "recruit.confirm.cost", "Cost", -52, Theme.Danger);
            var after = Line(content, "recruit.confirm.after", "After", -104, Theme.Success);
            var missing = Ui.Txt(content, "", 34, Theme.Danger, TextAnchor.MiddleRight, FontKind.Title, "Missing");
            missing.rectTransform.At(1f, 1f, 0, -160, 440, 46);
            missing.Inked(1.5f);
            var howTo = Ui.Txt(content, "", 24, Theme.TextMuted, TextAnchor.MiddleCenter, FontKind.Body, "HowToEarn");
            howTo.rectTransform.At(0.5f, 0f, 0, 112, 1080, 60);

            var buttons = Ui.Rect(content, "Buttons").BottomBand(96, 0);
            buttons.HLayout(40, 0, TextAnchor.MiddleCenter);
            var cancel = Ui.Btn(buttons, "", Theme.Surface3, Theme.Text, null!, 36, FontKind.Title, "Cancel");
            cancel.Rect.Pref(300, 96);
            var ok = Ui.Btn(buttons, "", Theme.Pr, Theme.TextDark, null!, 40, FontKind.Title, "Ok");
            ok.Rect.Pref(360, 96);

            var view = root.gameObject.AddComponent<RecruitConfirmPopupView>();
            ctx.Wire(view, "title", title);
            ctx.Wire(view, "portrait", portrait);
            ctx.Wire(view, "memberName", name);
            ctx.Wire(view, "rarity", rarity);
            ctx.Wire(view, "role", role);
            ctx.Wire(view, "synergy", synergy);
            ctx.Wire(view, "before", before);
            ctx.Wire(view, "cost", cost);
            ctx.Wire(view, "after", after);
            ctx.Wire(view, "missing", missing);
            ctx.Wire(view, "howToEarn", howTo);
            ctx.Wire(view, "ok", ok);
            ctx.Wire(view, "cancel", cancel);
            ctx.Wire(view, "window", window);
            ctx.Wire(view, "group", group);
            return root.gameObject;
        }

        /// <summary>Ligne « libellé ……… valeur » de la colonne de solde ; renvoie la valeur.</summary>
        static TMPro.TextMeshProUGUI Line(RectTransform content, string key, string name, float y, Color valueColor)
        {
            Ui.LocTxt(content, key, 26, Theme.TextMuted, TextAnchor.MiddleLeft, FontKind.Body, name + "Label").rectTransform.At(1f, 1f, -200, y, 240, 44);
            var value = Ui.Txt(content, "", 34, valueColor, TextAnchor.MiddleRight, FontKind.Title, name);
            value.rectTransform.At(1f, 1f, 0, y, 200, 44);
            return value;
        }
    }
}
