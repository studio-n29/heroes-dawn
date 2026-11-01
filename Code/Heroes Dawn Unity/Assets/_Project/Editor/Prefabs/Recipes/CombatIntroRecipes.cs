#nullable enable
using N29.HeroesDawn.Presentation.Combat;
using N29.HeroesDawn.Presentation.Common;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>Bandeau d'un combattant à l'entrée en combat : plaque tramée, filet de famille, portrait, nom et détail.</summary>
    public sealed class CombatIntroBannerRecipe : IPrefabRecipe
    {
        public string RelativePath => "Widgets/CombatIntroBanner.prefab";
        public int Order => 3;

        public GameObject Build(PrefabContext ctx)
        {
            var root = PrefabContext.NewRoot("CombatIntroBanner", 760, 150);
            root.Pref(760, 150);
            var plate = Ui.Img(root, "Plate", Theme.WithAlpha(Theme.Outline, 0.92f), Sprites.Halftone);
            plate.rectTransform.Stretch();
            var accent = Ui.Img(root, "Accent", Theme.Primary);
            accent.rectTransform.sizeDelta = new Vector2(10f, 0f);
            accent.rectTransform.anchorMin = new Vector2(0f, 0f);
            accent.rectTransform.anchorMax = new Vector2(0f, 1f);
            accent.rectTransform.pivot = new Vector2(0f, 0.5f);

            var portraitRoot = Ui.Rect(root, "PortraitRoot");
            portraitRoot.At(0f, 0.5f, 22, 0, 120, 120);
            var portrait = ctx.Nest<PortraitView>("Widgets/Portrait.prefab", portraitRoot, "Portrait");
            ((RectTransform)portrait.transform).Stretch();

            var name = Ui.Txt(root, "", 42, Color.white, TextAnchor.MiddleLeft, FontKind.Title, "Name");
            name.rectTransform.anchorMin = new Vector2(0f, 0.45f);
            name.rectTransform.anchorMax = new Vector2(1f, 0.95f);
            name.rectTransform.offsetMin = new Vector2(170, 0);
            name.rectTransform.offsetMax = new Vector2(-24, 0);
            name.AutoSize(42);
            name.Inked();
            var sub = Ui.Txt(root, "", 26, Theme.TextMuted, TextAnchor.MiddleLeft, FontKind.Body, "Sub");
            sub.rectTransform.anchorMin = new Vector2(0f, 0.05f);
            sub.rectTransform.anchorMax = new Vector2(1f, 0.45f);
            sub.rectTransform.offsetMin = new Vector2(170, 0);
            sub.rectTransform.offsetMax = new Vector2(-24, 0);

            var view = root.gameObject.AddComponent<CombatIntroBannerView>();
            ctx.Wire(view, "accent", accent);
            ctx.Wire(view, "portraitRoot", portraitRoot);
            ctx.Wire(view, "portrait", portrait);
            ctx.Wire(view, "nameText", name);
            ctx.Wire(view, "subText", sub);
            return root.gameObject;
        }
    }

    /// <summary>Entrée en combat : voile, agents à gauche, ennemis à droite, « VS » au centre, bouton Passer.</summary>
    public sealed class CombatIntroPopupRecipe : IPrefabRecipe
    {
        public string RelativePath => "Popups/CombatIntroPopup.prefab";
        public int Order => 12;

        public GameObject Build(PrefabContext ctx)
        {
            var root = PrefabContext.NewRoot("CombatIntroPopup");
            var group = root.gameObject.AddComponent<CanvasGroup>();

            var dim = Ui.Img(root, "Dim", Theme.WithAlpha(Theme.Navy, 0.9f), null, true);
            dim.rectTransform.Stretch();
            var tap = dim.gameObject.AddComponent<Button>();
            tap.transition = Selectable.Transition.None;
            var nav = tap.navigation;
            nav.mode = Navigation.Mode.None;
            tap.navigation = nav;

            var allyTitle = Ui.LocTxt(root, "combat.intro.allies", 32, Theme.Primary, TextAnchor.LowerLeft, FontKind.Title, "AllyTitle");
            allyTitle.rectTransform.At(0f, 0.5f, 40, 235, 760, 50);
            var enemyTitle = Ui.LocTxt(root, "combat.intro.enemies", 32, Theme.Danger, TextAnchor.LowerRight, FontKind.Title, "EnemyTitle");
            enemyTitle.rectTransform.At(1f, 0.5f, -40, 235, 760, 50);

            var allies = Ui.Rect(root, "Allies");
            allies.At(0f, 0.5f, 40, -20, 760, 640);
            allies.pivot = new Vector2(0f, 0.5f);
            allies.VLayout(14, 0, TextAnchor.MiddleLeft);
            var enemies = Ui.Rect(root, "Enemies");
            enemies.At(1f, 0.5f, -40, -20, 760, 640);
            enemies.pivot = new Vector2(1f, 0.5f);
            enemies.VLayout(14, 0, TextAnchor.MiddleRight);

            var vs = Ui.Txt(root, "VS", 120, Theme.Primary, TextAnchor.MiddleCenter, FontKind.Title, "Vs");
            vs.rectTransform.At(0.5f, 0.5f, 0, -20, 300, 160);
            vs.Inked();

            var skip = Ui.LocBtn(root, "combat.intro.skip", Theme.Surface3, Theme.Text, null!, 30, FontKind.Title, "Skip");
            skip.Rect.At(0.5f, 0f, 0, 60, 260, 80);

            var view = root.gameObject.AddComponent<CombatIntroPopupView>();
            ctx.Wire(view, "group", group);
            ctx.Wire(view, "tapArea", tap);
            ctx.Wire(view, "skip", skip.Button);
            ctx.Wire(view, "allyColumn", allies);
            ctx.Wire(view, "enemyColumn", enemies);
            ctx.Wire(view, "allyTitle", allyTitle);
            ctx.Wire(view, "enemyTitle", enemyTitle);
            ctx.Wire(view, "bannerPrefab", ctx.Asset("Widgets/CombatIntroBanner.prefab").GetComponent<CombatIntroBannerView>());
            return root.gameObject;
        }
    }
}
