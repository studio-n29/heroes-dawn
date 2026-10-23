#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Tutorial;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Wip;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>Barre de ressources du hub (M10 ①) : sauvegarde, Crédits, PR, Gemmes (BIENTÔT).</summary>
    public sealed class HubWalletRecipe : IPrefabRecipe
    {
        public string RelativePath => "Widgets/HubWallet.prefab";
        public int Order => 5;

        public GameObject Build(PrefabContext ctx)
        {
            var root = PrefabContext.NewRoot("HubWallet", 920, 70);
            root.HLayout(14, 0, TextAnchor.MiddleRight);
            var saving = Ui.Txt(root, "", 22, Theme.TextMuted, TextAnchor.MiddleRight, FontKind.Regular, "Saving");
            saving.Pref(170, 60);
            var credits = Pill(ctx, root, "PillCR", "CR", Theme.Credits);
            var pr = Pill(ctx, root, "PillPR", "PR", Theme.Pr);
            var gems = Pill(ctx, root, "PillGM", "GM", Theme.Perception);
            var view = root.gameObject.AddComponent<HubWalletView>();
            ctx.Wire(view, "credits", credits);
            ctx.Wire(view, "pr", pr);
            ctx.Wire(view, "gems", gems);
            ctx.Wire(view, "saving", saving);
            ctx.Wire(view, "creditsDelta", Delta(credits));
            ctx.Wire(view, "prDelta", Delta(pr));
            return root.gameObject;
        }

        /// <summary>Écart flottant (+150 / −450) sous la pastille, masqué au repos.</summary>
        static N29.HeroesDawn.Presentation.Feedback.FloatDelta Delta(TMP_Text value)
        {
            var pill = value.transform.parent.parent;
            var t = Ui.Txt(pill, "", 26, Theme.Success, TextAnchor.MiddleCenter, FontKind.Title, "Delta");
            t.rectTransform.At(0.5f, 0f, 0, -40, 200, 34);
            t.Inked(1.5f);
            t.raycastTarget = false;
            var fd = t.gameObject.AddComponent<N29.HeroesDawn.Presentation.Feedback.FloatDelta>();
            t.gameObject.SetActive(false);
            return fd;
        }

        static TMP_Text Pill(PrefabContext ctx, RectTransform parent, string name, string tag, Color accent)
        {
            var pill = (RectTransform)ctx.Nest("Widgets/ResourcePill.prefab", parent, name).transform;
            pill.Pref(230, 62);
            pill.Find("Fill/Badge").GetComponent<Image>().color = accent;
            pill.Find("Fill/Badge/Tag").GetComponent<TMP_Text>().text = tag;
            return pill.Find("Fill/Value").GetComponent<TMP_Text>();
        }
    }

    /// <summary>
    /// Coque du hub (M10) : barre haute (Retour, titre, ressources, réglages), zone de page entre les zones sûres,
    /// barre de navigation basse à 5 onglets avec badges. Les onglets et le titre sont remplis par le présentateur.
    /// </summary>
    public sealed class HubShellRecipe : IPrefabRecipe
    {
        public string RelativePath => "Screens/Hub.prefab";
        public int Order => 20;

        const float TopH = 116, NavH = 116, Safe = 88;

        public GameObject Build(PrefabContext ctx)
        {
            var rt = ScreenKit.Root("Hub");
            ScreenKit.Backdrop(rt, Theme.SkyTop, Theme.SkyBottom);
            ScreenKit.ArtBackdrop(rt, Sprites.BackgroundLandscape, 1f, 0.8f);
            var relay = rt.gameObject.AddComponent<UiTapRelay>();

            // ---- Zone de page (entre les barres, zones sûres de 88 px sur les côtés)
            var pageArea = Ui.Rect(rt, "PageArea").Stretch(Safe, NavH + 12, Safe, TopH + 12);

            // ---- Barre haute
            var top = Ui.Img(rt, "TopBar", Theme.WithAlpha(Theme.Frost, 0.92f), null, true);
            top.rectTransform.TopBand(TopH);
            var back = Ui.LocBtn(top.transform, "nav.back", Theme.Surface3, Theme.Text, null!, 26, FontKind.Title, "Back");
            back.Rect.At(0f, 0.5f, Safe - 70, 0, 160, 96);
            back.gameObject.SetActive(false);
            var title = Ui.Txt(top.transform, "", 56, Theme.Text, TextAnchor.MiddleLeft, FontKind.Title, "Title");
            title.rectTransform.At(0f, 0.5f, Safe + 110, 0, 820, 96);
            title.AutoSize(56);
            title.Inked();
            var settings = Ui.LocBtn(top.transform, "nav.settings", Theme.Surface2, Theme.Text, null!, 24, FontKind.Title, "Settings");
            settings.Rect.At(1f, 0.5f, -(Safe - 70), 0, 170, 96);
            var wallet = ctx.Nest<HubWalletView>("Widgets/HubWallet.prefab", top.transform, "Wallet");
            ((RectTransform)wallet.transform).At(1f, 0.5f, -(Safe - 70) - 190, 0, 920, 70);

            // ---- Barre de navigation basse
            var nav = Ui.Img(rt, "NavBar", Theme.WithAlpha(Theme.Frost, 0.95f), null, true);
            nav.rectTransform.BottomBand(NavH);
            var row = Ui.Rect(nav.transform, "Tabs").Stretch(Safe - 40, 10, Safe - 40, 10);
            row.HLayout(16, 0, TextAnchor.MiddleCenter);
            var tabs = new List<Object>();
            var badges = new List<Object>();
            foreach (var page in HubNavigator.Tabs)
            {
                var tab = Ui.Btn(row, "", Theme.WithAlpha(Theme.Surface, 0f), Theme.TextMuted, null!, 36, FontKind.Title, "Tab" + page);
                tab.Rect.Pref(330, 96);
                var badge = Ui.Img(tab.transform, "Badge", Theme.Danger, Sprites.Circle);
                badge.rectTransform.At(1f, 1f, 6, 10, 48, 48);
                var badgeText = Ui.Txt(badge.transform, "", 26, Color.white, TextAnchor.MiddleCenter, FontKind.Title, "Text");
                badgeText.rectTransform.Stretch();
                var pulse = badge.gameObject.AddComponent<Pulse>();
                pulse.Amplitude = 0.1f;
                pulse.Speed = 4f;
                badge.gameObject.SetActive(false);
                if (page == HubPage.Team) RecipeKit.Mark(tab, TutorialTarget.TabTeam);
                tabs.Add(tab);
                badges.Add(badgeText);
            }
            // Bloc sous l'onglet actif : il glisse d'un onglet à l'autre (hors de la disposition des onglets).
            var indicator = Ui.Img(nav.transform, "TabIndicator", Theme.Primary, Sprites.RoundedSmall);
            indicator.rectTransform.At(0.5f, 0f, 0, 2, 300, 8);
            indicator.raycastTarget = false;

            var view = rt.gameObject.AddComponent<HubShellView>();
            ScreenKit.WireFade(ctx, view);
            ctx.Wire(view, "title", title);
            ctx.Wire(view, "back", back);
            ctx.Wire(view, "settings", settings);
            ctx.Wire(view, "wallet", wallet);
            ctx.Wire(view, "pageArea", pageArea);
            ctx.Wire(view, "relay", relay);
            ctx.WireArray(view, "tabs", tabs);
            ctx.WireArray(view, "badges", badges);
            ctx.Wire(view, "tabIndicator", indicator.rectTransform);
            return rt.gameObject;
        }
    }
}
