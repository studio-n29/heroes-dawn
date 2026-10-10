#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Popups;
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
            var root = PrefabContext.NewRoot("ShopCard", 400, 540);
            root.Pref(400, 540);
            var halo = Ui.Img(root, "Halo", Theme.WithAlpha(Theme.Primary, 0.3f), Sprites.Rounded);
            halo.type = Image.Type.Sliced;
            halo.rectTransform.At(0.5f, 0.5f, 0, 0, 428, 528);
            var haloPulse = halo.gameObject.AddComponent<Pulse>();
            haloPulse.Amplitude = 0.035f;
            haloPulse.Speed = 3f;
            halo.gameObject.SetActive(false);
            var card = Ui.Rect(root, "Card").At(0.5f, 0.5f, 0, 0, 400, 500);
            var rim = Ui.Img(card, "Rim", Theme.Primary, Sprites.Rounded, true);
            rim.type = Image.Type.Sliced;
            rim.rectTransform.Stretch();
            var fill = Ui.Img(card, "Fill", Theme.Surface, Sprites.Rounded, true);
            fill.type = Image.Type.Sliced;
            fill.rectTransform.Stretch(8, 8, 8, 8);
            var tap = card.gameObject.AddComponent<Button>();
            tap.targetGraphic = fill;
            var nav = tap.navigation;
            nav.mode = Navigation.Mode.None;
            tap.navigation = nav;
            var body = fill.gameObject.AddComponent<CanvasGroup>();
            var strip = Ui.Img(fill.transform, "Family", Theme.Primary, Sprites.Rounded);
            strip.rectTransform.TopBand(10, 0);
            var portrait = ctx.Nest<PortraitView>("Widgets/Portrait.prefab", fill.transform, "Portrait");
            ((RectTransform)portrait.transform).At(0.5f, 1f, 0, -24, 110, 110);
            var name = Ui.Txt(fill.transform, "", 40, Color.white, TextAnchor.MiddleCenter, FontKind.Title, "Name");
            name.rectTransform.At(0.5f, 1f, 0, -140, 360, 48);
            name.AutoSize(40);
            name.Inked();
            var rarity = Ui.Txt(fill.transform, "", 26, Theme.Primary, TextAnchor.MiddleCenter, FontKind.Title, "Rarity");
            rarity.rectTransform.At(0.5f, 1f, 0, -190, 360, 32);
            var role = Ui.Txt(fill.transform, "", 22, Theme.TextMuted, TextAnchor.MiddleCenter, FontKind.Regular, "Role");
            role.rectTransform.At(0.5f, 1f, 0, -224, 360, 28);
            var stats = Ui.Txt(fill.transform, "", 20, Theme.Text, TextAnchor.MiddleCenter, FontKind.Regular, "Stats");
            stats.rectTransform.At(0.5f, 1f, 0, -258, 360, 28);
            var skills = Ui.Txt(fill.transform, "", 20, Theme.TextMuted, TextAnchor.UpperCenter, FontKind.Regular, "Skills");
            skills.rectTransform.At(0.5f, 1f, 0, -292, 360, 72);
            var recruit = Ui.Btn(fill.transform, "", Theme.Pr, Theme.TextDark, null!, 30, FontKind.Title, "Recruit");
            recruit.Rect.At(0.5f, 0f, 0, 16, 352, 96);
            var inTeam = Ui.Txt(card, "", 46, Theme.Primary, TextAnchor.MiddleCenter, FontKind.Title, "InTeam");
            inTeam.rectTransform.At(0.5f, 0f, 0, 30, 360, 80);
            inTeam.rectTransform.localRotation = Quaternion.Euler(0, 0, 8);
            inTeam.Inked();
            inTeam.gameObject.SetActive(false);
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
            return root.gameObject;
        }
    }

    /// <summary>Page Recruter : règle et compte à rebours en haut, 3 cartes au centre, progression et Renouveler en bas.</summary>
    public sealed class RecruitPageRecipe : IPrefabRecipe
    {
        public string RelativePath => "Pages/Recruit.prefab";
        public int Order => 10;

        public GameObject Build(PrefabContext ctx)
        {
            var rt = ScreenKit.Root("RecruitPage");
            var rule = Ui.Txt(rt, "", 26, Theme.Text, TextAnchor.MiddleLeft, FontKind.Body, "Rule");
            rule.rectTransform.At(0f, 1f, 0, 0, 1080, 48);
            var countdown = Ui.Txt(rt, "", 36, Theme.Primary, TextAnchor.MiddleRight, FontKind.Title, "Countdown");
            countdown.rectTransform.At(1f, 1f, 0, 0, 620, 48);
            countdown.Inked(1.5f);
            var row = Ui.Rect(rt, "Cards").At(0.5f, 0.5f, 0, 20, 1300, 540);
            row.HLayout(50, 0, TextAnchor.MiddleCenter);
            var cards = new List<Object>();
            for (int i = 0; i < 3; i++) cards.Add(ctx.Nest<ShopCardView>("Widgets/ShopCard.prefab", row, "Card" + i));
            var empty = Ui.Txt(rt, "", 40, Theme.Text, TextAnchor.MiddleCenter, FontKind.Title, "Empty");
            empty.rectTransform.At(0.5f, 0.5f, 0, 0, 1200, 200);
            empty.Inked();
            empty.gameObject.SetActive(false);
            var progress = Ui.Txt(rt, "", 30, Theme.TextMuted, TextAnchor.MiddleLeft, FontKind.Title, "Progress");
            progress.rectTransform.At(0f, 0f, 0, 0, 700, 96);
            var reroll = Ui.Btn(rt, "", Theme.Credits, Theme.TextDark, null!, 32, FontKind.Title, "Reroll");
            reroll.Rect.At(1f, 0f, 0, 0, 480, 96);
            var registry = Ui.LocBtn(rt, "agents.open", Theme.Surface3, Theme.Text, null!, 32, FontKind.Title, "Registry");
            registry.Rect.At(0.5f, 0f, 60, 0, 360, 96);

            var view = rt.gameObject.AddComponent<RecruitPageView>();
            ScreenKit.WireFade(ctx, view);
            ctx.Wire(view, "rule", rule);
            ctx.Wire(view, "countdown", countdown);
            ctx.WireArray(view, "cards", cards);
            ctx.Wire(view, "empty", empty);
            ctx.Wire(view, "progress", progress);
            ctx.Wire(view, "reroll", reroll);
            ctx.Wire(view, "registry", registry);
            return rt.gameObject;
        }
    }

    /// <summary>Confirmation d'achat (M31) : héros et combo phare à gauche, solde avant / coût / après à droite, Annuler / Recruter.</summary>
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
            var combo = Ui.Txt(content, "", 26, Theme.Text, TextAnchor.UpperLeft, FontKind.Body, "Combo");
            combo.rectTransform.At(0f, 1f, 0, -166, 620, 70);

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
            ctx.Wire(view, "combo", combo);
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
