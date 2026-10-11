#nullable enable
using N29.HeroesDawn.Presentation.Combat;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Tutorial;
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

    /// <summary>
    /// Carte d'unité verticale (M65) : zone de portrait teintée à la famille avec le nom en bas, PV, jauge ATB, statuts et ligne fixe
    /// (fatigue des alliés). Taille 250 × 290 réglée à l'exécution.
    /// </summary>
    public sealed class UnitCardRecipe : IPrefabRecipe
    {
        public string RelativePath => "Widgets/UnitCard.prefab";
        public int Order => 5;

        public GameObject Build(PrefabContext ctx)
        {
            var fill = RecipeKit.PanelRoot("UnitCard", Theme.Surface, 4f, 7f, true);
            var frame = fill.Frame();
            frame.sizeDelta = new Vector2(250, 290);
            var layout = frame.Pref(250, 290);
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

            var area = Ui.Img(fill.transform, "PortraitArea", Theme.Surface3, Sprites.Rounded);
            area.type = Image.Type.Sliced;
            area.rectTransform.Row(8, 8, -8, 168);
            Ui.Img(area.transform, "Shade", Theme.WithAlpha(Theme.Navy, 0.45f), Sprites.VerticalGradient).rectTransform.Stretch();
            var strip = Ui.Img(area.transform, "Family", Theme.Primary, Sprites.RoundedSmall);
            strip.rectTransform.anchorMin = new Vector2(0, 0);
            strip.rectTransform.anchorMax = new Vector2(1, 0);
            strip.rectTransform.pivot = new Vector2(0.5f, 0);
            strip.rectTransform.sizeDelta = new Vector2(0, 8);
            var portrait = ctx.Nest<PortraitView>("Widgets/Portrait.prefab", area.transform, "Portrait");
            ((RectTransform)portrait.transform).At(0.5f, 0.5f, 0, 14, 112, 112);
            var unitName = Ui.Txt(area.transform, "", 32, Color.white, TextAnchor.LowerLeft, FontKind.Title, "Name");
            unitName.rectTransform.Stretch(12, 12, 8, 120);
            unitName.AutoSize(32);
            unitName.Inked();
            var hp = Ui.Bar(fill.transform, "Hp", Theme.Hp, Theme.SurfaceDark, true, 20);
            ((RectTransform)hp.transform).Row(12, 12, -180, 26);
            var gauge = Ui.Bar(fill.transform, "Gauge", Theme.Gauge, Theme.SurfaceDark);
            gauge.Smooth = false;
            ((RectTransform)gauge.transform).Row(12, 12, -210, 14);
            var statuses = Ui.Txt(fill.transform, "", 20, Theme.TextMuted, TextAnchor.UpperLeft, FontKind.Body, "Statuses");
            statuses.rectTransform.Row(12, 12, -228, 24);
            var sub = Ui.Txt(fill.transform, "", 20, Theme.AccentDark, TextAnchor.UpperLeft, FontKind.Body, "Sub");
            sub.rectTransform.Row(12, 12, -250, 26);
            var ko = Ui.Txt(fill.transform, "K.O.", 64, Theme.Danger, TextAnchor.MiddleCenter, FontKind.Title, "KO");
            ko.rectTransform.Stretch();
            ko.Inked();
            ko.gameObject.SetActive(false);

            // Éventail des 4 compétences sous la carte : arc léger, les icônes du milieu un peu plus bas.
            var icons = new Object[4];
            float[] iconX = { -93f, -31f, 31f, 93f };
            float[] iconY = { -68f, -78f, -78f, -68f };
            for (int i = 0; i < 4; i++)
            {
                var icon = ctx.Nest<SkillIconView>("Widgets/SkillIcon.prefab", frame, "Skill" + (i + 1));
                ((RectTransform)icon.transform).At(0.5f, 0f, iconX[i], iconY[i], SkillIconRecipe.Size, SkillIconRecipe.Size);
                icons[i] = icon;
            }

            // Cible du tutoriel : les icônes de l'agent prêt (marqueur vivant seulement quand une compétence est lançable).
            var fan = Ui.Rect(frame, "SkillFan").At(0.5f, 0f, 0, -80, 260, 92);
            var readyMarker = RecipeKit.Mark(fan, TutorialTarget.CombatSkills, enabled: false);

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
            ctx.Wire(view, "sub", sub);
            ctx.Wire(view, "portraitArea", area);
            ctx.Wire(view, "ko", ko);
            ctx.Wire(view, "readyPulse", pulse);
            ctx.Wire(view, "group", group);
            ctx.Wire(view, "button", button);
            ctx.Wire(view, "layout", layout);
            ctx.Wire(view, "floatPrefab", ctx.Asset("Widgets/FloatingText.prefab").GetComponent<FloatingTextView>());
            ctx.WireArray(view, "icons", icons);
            ctx.Wire(view, "readyMarker", readyMarker);
            return frame.gameObject;
        }
    }

    /// <summary>Pastille d une unité sur la frise chronologique (56 px) : disque de famille, initiale, anneau, annonce de l action.</summary>
    public sealed class TimelineMarkerRecipe : IPrefabRecipe
    {
        public string RelativePath => "Widgets/TimelineMarker.prefab";
        public int Order => 0;

        public GameObject Build(PrefabContext ctx)
        {
            var root = PrefabContext.NewRoot("TimelineMarker", 80, 80);
            root.anchorMin = root.anchorMax = root.pivot = new Vector2(0.5f, 0.5f);
            var group = root.gameObject.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            var ring = Ui.Img(root, "Ring", Color.white, Sprites.Ring);
            ring.rectTransform.At(0.5f, 0.5f, 0, 0, 104, 104);
            ring.gameObject.AddComponent<Pulse>().Amplitude = 0.1f;
            ring.gameObject.SetActive(false);
            var frame = Ui.Img(root, "Frame", Theme.Ink, Sprites.Circle);
            frame.rectTransform.At(0.5f, 0.5f, 0, 0, 80, 80);
            var disc = Ui.Img(frame.transform, "Disc", Theme.Info, Sprites.Circle);
            disc.rectTransform.Stretch(5, 5, 5, 5);
            var initial = Ui.Txt(disc.transform, "", 40, Color.white, TextAnchor.MiddleCenter, FontKind.Title, "Initial");
            initial.rectTransform.Stretch();
            initial.Inked();
            var plan = Ui.Txt(root, "", 30, Theme.Primary, TextAnchor.LowerCenter, FontKind.Title, "Plan");
            plan.rectTransform.At(0.5f, 1f, 0, 42, 160, 36);
            plan.textWrappingMode = TextWrappingModes.NoWrap;
            plan.Inked();
            plan.gameObject.SetActive(false);
            var view = root.gameObject.AddComponent<TimelineMarkerView>();
            ctx.Wire(view, "disc", disc);
            ctx.Wire(view, "ring", ring);
            ctx.Wire(view, "initial", initial);
            ctx.Wire(view, "plan", plan);
            ctx.Wire(view, "group", group);
            return root.gameObject;
        }
    }

    /// <summary>Icône de compétence sous la carte d'un agent (72 px) : disque coloré, numéro, initiale, recharge, anneau.</summary>
    public sealed class SkillIconRecipe : IPrefabRecipe
    {
        public const float Size = 60f;
        public string RelativePath => "Widgets/SkillIcon.prefab";
        public int Order => 0;

        public GameObject Build(PrefabContext ctx)
        {
            var root = PrefabContext.NewRoot("SkillIcon", Size, Size);
            root.anchorMin = root.anchorMax = root.pivot = new Vector2(0.5f, 0.5f);
            var group = root.gameObject.AddComponent<CanvasGroup>();
            var ring = Ui.Img(root, "Ring", Color.white, Sprites.Ring);
            ring.rectTransform.At(0.5f, 0.5f, 0, 0, Size + 14, Size + 14);
            ring.raycastTarget = false;
            ring.gameObject.AddComponent<Pulse>().Amplitude = 0.07f;
            ring.gameObject.SetActive(false);
            var frame = Ui.Img(root, "Frame", Theme.Ink, Sprites.Circle, true);
            frame.rectTransform.At(0.5f, 0.5f, 0, 0, Size, Size);
            var button = frame.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            var nav = button.navigation;
            nav.mode = Navigation.Mode.None;
            button.navigation = nav;
            var disc = Ui.Img(frame.transform, "Disc", Theme.Primary, Sprites.Circle);
            disc.rectTransform.Stretch(4, 4, 4, 4);
            var glyph = Ui.Txt(disc.transform, "A", 38, Theme.TextDark, TextAnchor.MiddleCenter, FontKind.Title, "Glyph");
            glyph.rectTransform.Stretch();
            var cooldownFill = Ui.Img(disc.transform, "CooldownFill", new Color(0f, 0f, 0f, 0.55f), Sprites.Circle);
            cooldownFill.rectTransform.Stretch();
            cooldownFill.type = Image.Type.Filled;
            cooldownFill.fillMethod = Image.FillMethod.Radial360;
            cooldownFill.fillOrigin = (int)Image.Origin360.Top;
            cooldownFill.fillClockwise = false;
            cooldownFill.raycastTarget = false;
            cooldownFill.gameObject.SetActive(false);
            var cooldown = Ui.Txt(disc.transform, "", 34, Color.white, TextAnchor.MiddleCenter, FontKind.Title, "Cooldown");
            cooldown.rectTransform.Stretch();
            cooldown.Inked();
            cooldown.gameObject.SetActive(false);
            var slotBadge = Ui.Img(root, "SlotBadge", Theme.Ink, Sprites.Circle);
            slotBadge.rectTransform.At(0f, 1f, 4, -4, 28, 28);
            slotBadge.raycastTarget = false;
            var slot = Ui.Txt(slotBadge.transform, "1", 20, Color.white, TextAnchor.MiddleCenter, FontKind.Title, "Slot");
            slot.rectTransform.Stretch();

            var view = root.gameObject.AddComponent<SkillIconView>();
            ctx.Wire(view, "button", button);
            ctx.Wire(view, "disc", disc);
            ctx.Wire(view, "cooldownFill", cooldownFill);
            ctx.Wire(view, "ring", ring);
            ctx.Wire(view, "glyph", glyph);
            ctx.Wire(view, "slot", slot);
            ctx.Wire(view, "cooldown", cooldown);
            ctx.Wire(view, "group", group);
            return root.gameObject;
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
            RecipeKit.Mark(cont, TutorialTarget.ResultsContinue);
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
    /// <summary>
    /// Combat (M65/M66). Adapté à un écran de ~2160 × 972 : barre haute (retraite, titre, AUTO, vitesse, pause), ennemis en haut à droite,
    /// alliés en bas à droite, panneau de compétences et journal à gauche, combo et consigne dans la bande centrale.
    /// </summary>
    public sealed class CombatRecipe : IPrefabRecipe
    {
        public string RelativePath => "Screens/Combat.prefab";
        public int Order => 20;

        public GameObject Build(PrefabContext ctx)
        {
            var rt = ScreenKit.Root("Combat");
            ScreenKit.Backdrop(rt, Theme.SkyTop, Theme.SkyBottom);
            ScreenKit.ArtBackdrop(rt, Sprites.BackgroundLandscape, 1f, 0.72f);

            // ---- Barre du haut
            var top = Ui.Panel(rt, "TopBar", Theme.WithAlpha(Theme.Frost, 0.94f), 0f, 0f, true).Frame().TopBand(90);
            var retreat = Ui.LocBtn(top, "combat.retreat", Theme.Surface3, Theme.Danger, null!, 30, FontKind.Title, "Retreat");
            retreat.Rect.At(0f, 0.5f, 20, 0, 230, 64);
            var title = Ui.Txt(top, "", 44, Theme.Primary, TextAnchor.MiddleLeft, FontKind.Title, "Title");
            title.rectTransform.At(0f, 0.5f, 270, 0, 800, 64);
            title.Inked();
            var pause = Ui.LocBtn(top, "combat.pause", Theme.Surface2, Theme.Text, null!, 34, FontKind.Title, "Pause");
            pause.Rect.At(1f, 0.5f, -20, 0, 74, 74);
            var speed = Ui.Btn(top, "", Theme.Surface3, Theme.Text, null!, 30, FontKind.Title, "Speed");
            speed.Rect.At(1f, 0.5f, -114, 0, 150, 64);
            var auto = Ui.LocBtn(top, "combat.auto_hint", Theme.Surface3, Theme.Text, null!, 30, FontKind.Title, "Auto");
            auto.Rect.At(1f, 0.5f, -284, 0, 190, 64);
            var debug = Ui.LocBtn(top, "combat.debug_win", Theme.Hex(0x5B2A6B), Color.white, null!, 20, FontKind.Body, "DebugWin");
            debug.Rect.At(1f, 0.5f, -500, 0, 200, 46);

            // ---- Ennemis (haut droite), alliés (bas droite)
            var enemyRow = Ui.Rect(rt, "Enemies").At(1f, 1f, -30, -104, 1000, 290);
            enemyRow.HLayout(30, 0, TextAnchor.UpperCenter);
            RecipeKit.Mark(enemyRow, TutorialTarget.CombatEnemies);
            var allyRow = Ui.Rect(rt, "Allies").At(1f, 0f, -30, 104, 1100, 290);
            allyRow.HLayout(16, 0, TextAnchor.LowerCenter);

            // ---- Journal (haut gauche, discret)
            var logFill = Ui.Panel(rt, "Log", Theme.WithAlpha(Theme.Frost, 0.55f), 3f, 0f, false);
            logFill.Frame().At(0f, 1f, 24, -108, 500, 132);
            logFill.gameObject.AddComponent<RectMask2D>();
            var logText = Ui.Txt(logFill.transform, "", 21, Theme.TextMuted, TextAnchor.LowerLeft, FontKind.Regular, "Lines");
            logText.rectTransform.Stretch(14, 10, 14, 10);

            // ---- Bandeau central, consigne
            var banner = Ui.Txt(rt, "", 76, Theme.Primary, TextAnchor.MiddleCenter, FontKind.Title, "Banner");
            banner.rectTransform.At(1f, 0.5f, -30, 100, 1000, 120);
            banner.Inked();
            var bannerGroup = banner.gameObject.AddComponent<CanvasGroup>();
            bannerGroup.blocksRaycasts = false;
            var hintBand = Ui.Img(rt, "HintBand", Theme.WithAlpha(Theme.Primary, 0.9f), Sprites.RoundedSmall);
            hintBand.rectTransform.At(1f, 0f, -30, 624, 1000, 52);
            var hint = Ui.Txt(hintBand.transform, "", 30, Theme.TextDark, TextAnchor.MiddleCenter, FontKind.Title, "Hint");
            hint.rectTransform.Stretch(10, 0, 10, 0);

            // ---- Frise chronologique : grande bande mise en valeur au centre (100 à gauche → 0 à droite : à 0, c'est le tour)
            var glow = Ui.Img(rt, "TimelineGlow", Theme.WithAlpha(Theme.Primary, 0.35f), Sprites.Rounded);
            glow.rectTransform.At(1f, 0f, -22, 390, 1516, 186);
            var stripFill = Ui.Panel(rt, "Timeline", Theme.WithAlpha(Theme.Frost, 0.94f), 5f, 8f, false);
            var strip = stripFill.Frame().At(1f, 0f, -30, 398, 1500, 170);
            var track = Ui.Rect(stripFill.transform, "Track").At(0f, 0.5f, 110, 0, 1250, 18);
            var axisStart = Ui.Txt(stripFill.transform, "100", 40, Theme.TextMuted, TextAnchor.MiddleCenter, FontKind.Title, "AxisStart");
            axisStart.rectTransform.At(0f, 0.5f, 14, 0, 90, 56);
            var axisEnd = Ui.Txt(stripFill.transform, "0", 56, Theme.Primary, TextAnchor.MiddleCenter, FontKind.Title, "AxisEnd");
            axisEnd.rectTransform.At(1f, 0.5f, -20, 8, 90, 70);
            axisEnd.Inked();
            var axisLabel = Ui.LocTxt(stripFill.transform, "combat.timeline_action", 22, Theme.Primary, TextAnchor.MiddleCenter, FontKind.Title, "AxisLabel");
            axisLabel.rectTransform.At(1f, 0.5f, -20, -34, 110, 30);
            Ui.Img(track, "Line", Theme.Surface3, Sprites.RoundedSmall).rectTransform.Stretch();
            var prep = Ui.Img(track, "Prep", Theme.WithAlpha(Theme.Danger, 0.65f), Sprites.RoundedSmall);
            prep.rectTransform.anchorMin = new Vector2(0.8f, 0f);
            prep.rectTransform.anchorMax = new Vector2(1f, 1f);
            prep.rectTransform.offsetMin = new Vector2(0, -26);
            prep.rectTransform.offsetMax = new Vector2(0, 26);
            var gate = Ui.Img(track, "Gate", Theme.Primary, Sprites.RoundedSmall);
            gate.rectTransform.anchorMin = new Vector2(1f, 0f);
            gate.rectTransform.anchorMax = new Vector2(1f, 1f);
            gate.rectTransform.sizeDelta = new Vector2(10, 90);
            gate.rectTransform.anchoredPosition = Vector2.zero;
            gate.gameObject.AddComponent<Pulse>().Amplitude = 0.12f;
            var timeline = strip.gameObject.AddComponent<TimelineView>();
            ctx.Wire(timeline, "track", track);
            ctx.Wire(timeline, "prepZone", prep.rectTransform);
            ctx.Wire(timeline, "markerPrefab", ctx.Asset("Widgets/TimelineMarker.prefab").GetComponent<TimelineMarkerView>());

            // ---- Compétences d avatar (lançables à tout moment) : 5 boutons sous le combo
            var avatarPanel = Ui.Rect(rt, "Avatar").At(0f, 0f, 24, 24, 500, 104);
            avatarPanel.HLayout(6, 0, TextAnchor.MiddleCenter);
            var avatarButtons = new Object[5];
            for (int i = 0; i < 5; i++)
            {
                var ab = Ui.Btn(avatarPanel, "", Theme.Danger, Theme.TextDark, null!, 22, FontKind.Title, "Avatar" + (i + 1));
                ab.Rect.sizeDelta = new Vector2(96, 104);
                var le = ab.gameObject.AddComponent<LayoutElement>();
                le.preferredWidth = 96; le.preferredHeight = 104; le.flexibleWidth = 1;
                avatarButtons[i] = ab;
            }

            // ---- Combo
            var combo = Ui.Btn(rt, "", Theme.Danger, Color.white, null!, 40, FontKind.Title, "Combo");
            combo.Rect.At(0f, 0f, 24, 140, 500, 96);
            combo.Label.Inked();
            combo.gameObject.AddComponent<Pulse>().Amplitude = 0.07f;
            RecipeKit.Shine(combo);
            combo.gameObject.SetActive(false);

            var view = rt.gameObject.AddComponent<CombatView>();
            ScreenKit.WireFade(ctx, view);
            ctx.Wire(view, "retreat", retreat);
            ctx.Wire(view, "auto", auto);
            ctx.Wire(view, "speed", speed);
            ctx.Wire(view, "debugWin", debug);
            ctx.Wire(view, "pause", pause);
            ctx.Wire(view, "avatarPanel", avatarPanel);
            ctx.WireArray(view, "avatarButtons", avatarButtons);
            ctx.Wire(view, "timeline", timeline);
            ctx.Wire(view, "combo", combo);
            ctx.Wire(view, "title", title);
            ctx.Wire(view, "hint", hint);
            ctx.Wire(view, "logText", logText);
            ctx.Wire(view, "banner", banner);
            ctx.Wire(view, "bannerGroup", bannerGroup);
            ctx.Wire(view, "enemyRow", enemyRow);
            ctx.Wire(view, "allyRow", allyRow);
            ctx.Wire(view, "unitCardPrefab", ctx.Asset("Widgets/UnitCard.prefab").GetComponent<UnitCardView>());
            return rt.gameObject;
        }
    }
}
