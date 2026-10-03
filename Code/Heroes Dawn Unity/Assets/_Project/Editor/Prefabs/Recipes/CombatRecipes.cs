#nullable enable
using N29.HeroesDawn.Presentation.Combat;
using N29.HeroesDawn.Presentation.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    static class CombatKit
    {
        /// <summary>Bande horizontale ancrée en haut du parent : marge gauche, marge droite, haut, hauteur.</summary>
        public static RectTransform Row(this RectTransform rt, float left, float right, float top, float height)
        {
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0, 1);
            rt.offsetMin = new Vector2(left, top - height);
            rt.offsetMax = new Vector2(-right, top);
            return rt;
        }
    }

    /// <summary>Nombre flottant (reprise de l'ancien UnitCard.Float).</summary>
    public sealed class FloatingTextRecipe : IPrefabRecipe
    {
        public string RelativePath => "Widgets/FloatingText.prefab";
        public int Order => 0;

        public GameObject Build(PrefabContext ctx)
        {
            var root = PrefabContext.NewRoot("FloatingText", 360, 70);
            root.anchorMin = root.anchorMax = root.pivot = new Vector2(0.5f, 0.5f);
            var t = Ui.Txt(root, "", 44, Color.white, TextAnchor.MiddleCenter, FontKind.Title, "Label");
            t.rectTransform.Stretch();
            t.Inked();
            var group = root.gameObject.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            var view = root.gameObject.AddComponent<FloatingTextView>();
            ctx.Wire(view, "label", t);
            ctx.Wire(view, "group", group);
            return root.gameObject;
        }
    }

    /// <summary>Carte d'unité (reprise de l'ancien UnitCard.Create) ; la taille allié / ennemi est réglée à l'exécution.</summary>
    public sealed class UnitCardRecipe : IPrefabRecipe
    {
        public string RelativePath => "Widgets/UnitCard.prefab";
        public int Order => 5;

        public GameObject Build(PrefabContext ctx)
        {
            var fill = RecipeKit.PanelRoot("UnitCard", Theme.Surface, 4f, 7f, true);
            var frame = fill.Frame();
            frame.sizeDelta = new Vector2(430, 180);
            var layout = frame.Pref(430, 180);
            var group = frame.gameObject.AddComponent<CanvasGroup>();

            var target = Ui.Img(frame, "TargetRing", Theme.Danger, Sprites.Rounded);
            target.rectTransform.Stretch(-10, -10, -10, -10);
            target.transform.SetAsFirstSibling();
            target.gameObject.AddComponent<Pulse>().Amplitude = 0.03f;
            target.gameObject.SetActive(false);
            var select = Ui.Img(frame, "SelectRing", Theme.Primary, Sprites.Rounded);
            select.rectTransform.Stretch(-8, -8, -8, -8);
            select.transform.SetAsFirstSibling();
            select.gameObject.SetActive(false);

            var strip = Ui.Img(fill.transform, "Family", Theme.Primary, Sprites.RoundedSmall);
            strip.rectTransform.TopBand(10, 0);
            var portrait = ctx.Nest<PortraitView>("Widgets/Portrait.prefab", fill.transform, "Portrait");
            ((RectTransform)portrait.transform).At(0f, 1f, 12, -20, 96, 96);
            const float x = 122, right = 18;
            var unitName = Ui.Txt(fill.transform, "", 32, Color.white, TextAnchor.UpperLeft, FontKind.Title, "Name");
            unitName.rectTransform.Row(x, right, -14, 38);
            unitName.Inked();
            var hp = Ui.Bar(fill.transform, "Hp", Theme.Hp, Theme.SurfaceDark, true, 20);
            ((RectTransform)hp.transform).Row(x, right, -56, 28);
            var gauge = Ui.Bar(fill.transform, "Gauge", Theme.Gauge, Theme.SurfaceDark);
            gauge.Smooth = false;
            ((RectTransform)gauge.transform).Row(x, right, -90, 16);
            var statuses = Ui.Txt(fill.transform, "", 20, Theme.Shield, TextAnchor.UpperLeft, FontKind.Body, "Statuses");
            statuses.rectTransform.Row(x, right, -112, 50);
            var ko = Ui.Txt(fill.transform, "K.O.", 64, Theme.Danger, TextAnchor.MiddleCenter, FontKind.Title, "KO");
            ko.rectTransform.Stretch();
            ko.Inked();
            ko.gameObject.SetActive(false);

            var pulse = frame.gameObject.AddComponent<Pulse>();
            pulse.Amplitude = 0.035f;
            pulse.Speed = 7f;
            pulse.enabled = false;
            var button = frame.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            var nav = button.navigation;
            nav.mode = Navigation.Mode.None;
            button.navigation = nav;

            var view = frame.gameObject.AddComponent<UnitCardView>();
            ctx.Wire(view, "fill", fill);
            ctx.Wire(view, "select", select);
            ctx.Wire(view, "target", target);
            ctx.Wire(view, "strip", strip);
            ctx.Wire(view, "portrait", portrait);
            ctx.Wire(view, "unitName", unitName);
            ctx.Wire(view, "hp", hp);
            ctx.Wire(view, "gauge", gauge);
            ctx.Wire(view, "statuses", statuses);
            ctx.Wire(view, "ko", ko);
            ctx.Wire(view, "readyPulse", pulse);
            ctx.Wire(view, "group", group);
            ctx.Wire(view, "button", button);
            ctx.Wire(view, "layout", layout);
            ctx.Wire(view, "floatPrefab", ctx.Asset("Widgets/FloatingText.prefab").GetComponent<FloatingTextView>());
            return frame.gameObject;
        }
    }

    /// <summary>Bouton de compétence (reprise de l'ancien CombatScreen.Build, panneau des compétences).</summary>
    public sealed class SkillButtonRecipe : IPrefabRecipe
    {
        public string RelativePath => "Widgets/SkillButton.prefab";
        public int Order => 0;

        public GameObject Build(PrefabContext ctx)
        {
            var holder = PrefabContext.NewRoot("holder", 440, 146);
            var b = Ui.Btn(holder, "", Theme.Primary, Theme.TextDark, null!, 32, FontKind.Title, "SkillButton");
            b.Rect.SetParent(null, false);
            Object.DestroyImmediate(holder.gameObject);
            b.Rect.sizeDelta = new Vector2(440, 146);
            b.Rect.Pref(440, 146);
            b.Label.alignment = TextAlignmentOptions.Top;
            b.Label.rectTransform.Stretch(12, 60, 12, 8);
            var desc = Ui.Txt(b.Fill.transform, "", 20, Theme.TextDark, TextAnchor.UpperCenter, FontKind.Body, "Desc");
            desc.rectTransform.Stretch(14, 8, 14, 60);
            var view = b.gameObject.AddComponent<SkillButtonView>();
            ctx.Wire(view, "button", b);
            ctx.Wire(view, "description", desc);
            return b.gameObject;
        }
    }

    /// <summary>Ligne de fatigue des résultats (reprise de l'ancien CombatScreen.ShowResults).</summary>
    public sealed class FatigueRowRecipe : IPrefabRecipe
    {
        public string RelativePath => "Widgets/FatigueRow.prefab";
        public int Order => 0;

        public GameObject Build(PrefabContext ctx)
        {
            var row = PrefabContext.NewRoot("FatigueRow", 900, 52);
            row.Pref(900, 52);
            var n = Ui.Txt(row, "", 28, Color.white, TextAnchor.MiddleLeft, FontKind.Title, "Name");
            n.rectTransform.At(0f, 0.5f, 0, 0, 220, 50);
            var bar = Ui.Bar(row, "Bar", Theme.Success, Theme.SurfaceDark, true, 20);
            ((RectTransform)bar.transform).At(0f, 0.5f, 230, 0, 520, 30);
            var delta = Ui.Txt(row, "", 26, Theme.Fatigue, TextAnchor.MiddleRight, FontKind.Title, "Delta");
            delta.rectTransform.At(1f, 0.5f, 0, 0, 130, 50);
            var view = row.gameObject.AddComponent<FatigueRowView>();
            ctx.Wire(view, "memberName", n);
            ctx.Wire(view, "bar", bar);
            ctx.Wire(view, "delta", delta);
            return row.gameObject;
        }
    }

    /// <summary>Résultats du combat (reprise de l'ancien CombatScreen.ShowResults).</summary>
    public sealed class CombatResultsPopupRecipe : IPrefabRecipe
    {
        public string RelativePath => "Popups/CombatResultsPopup.prefab";
        public int Order => 10;

        public GameObject Build(PrefabContext ctx)
        {
            var (root, content, title) = CasePopupKit.Build("CombatResultsPopup", 1100, 720, Theme.Primary, out var window);
            var band = window.Find("TitleBand/Fill").GetComponent<Image>();
            var reward = Ui.Txt(content, "", 50, Theme.Primary, TextAnchor.UpperCenter, FontKind.Title, "Reward");
            reward.rectTransform.At(0.5f, 1f, 0, -6, 900, 60);
            reward.Inked();
            var fatigueTitle = Ui.Txt(content, "", 28, Theme.TextMuted, TextAnchor.UpperLeft, FontKind.Body, "FatigueTitle");
            fatigueTitle.rectTransform.At(0.5f, 1f, 0, -76, 900, 36);
            var list = Ui.Rect(content, "Fatigue").At(0.5f, 1f, 0, -118, 900, 260);
            list.VLayout(10, 0, TextAnchor.UpperCenter);
            var verify = Ui.Txt(content, "", 22, Theme.TextMuted, TextAnchor.MiddleCenter, FontKind.Regular, "Verify");
            verify.rectTransform.At(0.5f, 0f, 0, 118, 900, 34);
            var toHq = Ui.Txt(content, "", 26, Theme.Danger, TextAnchor.MiddleCenter, FontKind.Body, "ToHq");
            toHq.rectTransform.At(0.5f, 0f, 0, 150, 900, 34);
            var cont = Ui.Btn(content, "", Theme.Primary, Theme.TextDark, null!, 42, FontKind.Title, "Continue");
            cont.Rect.At(0.5f, 0f, 0, 0, 420, 100);
            var view = root.gameObject.AddComponent<CombatResultsPopupView>();
            CasePopupKit.WireBase(ctx, view, title, window);
            ctx.Wire(view, "titleBand", band);
            ctx.Wire(view, "reward", reward);
            ctx.Wire(view, "fatigueTitle", fatigueTitle);
            ctx.Wire(view, "fatigueList", list);
            ctx.Wire(view, "fatigueRowPrefab", ctx.Asset("Widgets/FatigueRow.prefab").GetComponent<FatigueRowView>());
            ctx.Wire(view, "verify", verify);
            ctx.Wire(view, "toHq", toHq);
            ctx.Wire(view, "continueButton", cont);
            return root.gameObject;
        }
    }

    /// <summary>Combat (reprise de l'ancien CombatScreen.Build).</summary>
    public sealed class CombatRecipe : IPrefabRecipe
    {
        public string RelativePath => "Screens/Combat.prefab";
        public int Order => 20;

        public GameObject Build(PrefabContext ctx)
        {
            var rt = ScreenKit.Root("Combat");
            ScreenKit.Backdrop(rt, Theme.WithAlpha(Theme.Danger, 0.22f), Theme.Hex(0x120E14));
            ScreenKit.ArtBackdrop(rt, Sprites.BackgroundLandscape, 1f, 0.8f);

            // ---- Barre du haut
            var top = Ui.Panel(rt, "TopBar", Theme.SurfaceDark, 0f, 0f, true).Frame().TopBand(90);
            var retreat = Ui.LocBtn(top, "combat.retreat", Theme.Surface3, Theme.Text, null!, 30, FontKind.Title, "Retreat");
            retreat.Rect.At(0f, 0.5f, 20, 0, 240, 64);
            var title = Ui.Txt(top, "", 44, Theme.Primary, TextAnchor.MiddleCenter, FontKind.Title, "Title");
            title.rectTransform.At(0.5f, 0.5f, 0, 0, 900, 64);
            title.Inked();
            var auto = Ui.LocBtn(top, "combat.auto", Theme.Surface3, Theme.Text, null!, 32, FontKind.Title, "Auto");
            auto.Rect.At(1f, 0.5f, -170, 0, 170, 64);
            var speed = Ui.Btn(top, "", Theme.Surface3, Theme.Text, null!, 32, FontKind.Title, "Speed");
            speed.Rect.At(1f, 0.5f, -20, 0, 130, 64);
            var debug = Ui.LocBtn(top, "combat.debug_win", Theme.Hex(0x5B2A6B), Color.white, null!, 20, FontKind.Body, "DebugWin");
            debug.Rect.At(0f, 0.5f, 280, 0, 220, 50);

            // ---- Ennemis (haut), alliés (bas)
            var enemyRow = Ui.Rect(rt, "Enemies").At(0.5f, 1f, 0, -130, 1500, 210);
            enemyRow.HLayout(40, 0, TextAnchor.MiddleCenter);
            var allyRow = Ui.Rect(rt, "Allies").At(0.5f, 0f, 0, 24, 1840, 190);
            allyRow.HLayout(24, 0, TextAnchor.MiddleCenter);

            // ---- Journal (gauche)
            var logFill = Ui.Panel(rt, "Log", Theme.WithAlpha(Theme.SurfaceDark, 0.85f), 3f, 4f, false);
            logFill.Frame().At(0f, 0.5f, 24, 70, 560, 250);
            var logText = Ui.Txt(logFill.transform, "", 21, Theme.TextMuted, TextAnchor.LowerLeft, FontKind.Regular, "Lines");
            logText.rectTransform.Stretch(14, 10, 14, 10);

            // ---- Bandeau central, consigne
            var banner = Ui.Txt(rt, "", 70, Theme.Primary, TextAnchor.MiddleCenter, FontKind.Title, "Banner");
            banner.rectTransform.At(0.5f, 0.5f, 120, 80, 1100, 110);
            banner.Inked();
            var bannerGroup = banner.gameObject.AddComponent<CanvasGroup>();
            bannerGroup.blocksRaycasts = false;
            var hint = Ui.Txt(rt, "", 32, Color.white, TextAnchor.MiddleCenter, FontKind.Body, "Hint");
            hint.rectTransform.At(0.5f, 0f, 0, 400, 1000, 44);
            hint.Inked();

            // ---- Compétences
            var skillPanel = Ui.Rect(rt, "Skills").At(0.5f, 0f, 60, 236, 1380, 150);
            skillPanel.HLayout(20, 0, TextAnchor.MiddleCenter);
            var skills = new Object[3];
            for (int i = 0; i < 3; i++) skills[i] = ctx.Nest<SkillButtonView>("Widgets/SkillButton.prefab", skillPanel, "Skill" + (i + 1));

            // ---- Combo
            var combo = Ui.Btn(rt, "", Theme.Danger, Color.white, null!, 40, FontKind.Title, "Combo");
            combo.Rect.At(1f, 0f, -24, 400, 360, 130);
            combo.Label.Inked();
            combo.gameObject.AddComponent<Pulse>().Amplitude = 0.07f;
            combo.gameObject.SetActive(false);

            var view = rt.gameObject.AddComponent<CombatView>();
            ScreenKit.WireFade(ctx, view);
            ctx.Wire(view, "retreat", retreat);
            ctx.Wire(view, "auto", auto);
            ctx.Wire(view, "speed", speed);
            ctx.Wire(view, "debugWin", debug);
            ctx.Wire(view, "combo", combo);
            ctx.Wire(view, "title", title);
            ctx.Wire(view, "hint", hint);
            ctx.Wire(view, "logText", logText);
            ctx.Wire(view, "banner", banner);
            ctx.Wire(view, "bannerGroup", bannerGroup);
            ctx.Wire(view, "enemyRow", enemyRow);
            ctx.Wire(view, "allyRow", allyRow);
            ctx.Wire(view, "skillPanel", skillPanel);
            ctx.WireArray(view, "skills", skills);
            ctx.Wire(view, "unitCardPrefab", ctx.Asset("Widgets/UnitCard.prefab").GetComponent<UnitCardView>());
            return rt.gameObject;
        }
    }
}
