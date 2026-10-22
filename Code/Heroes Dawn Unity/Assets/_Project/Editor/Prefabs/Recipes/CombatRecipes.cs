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
            frame.sizeDelta = new Vector2(210, 250);
            var layout = frame.Pref(210, 250);
            var group = frame.gameObject.AddComponent<CanvasGroup>();

            var target = Ui.Img(frame, "TargetRing", Theme.Danger, Sprites.Rounded);
            target.rectTransform.Stretch(-10, -10, -10, -10);
            target.transform.SetAsFirstSibling();
            target.gameObject.AddComponent<Pulse>().Amplitude = 0.03f;
            target.gameObject.SetActive(false);
            var select = Ui.Img(frame, "SelectRing", Theme.Glow, Sprites.Panel);
            select.rectTransform.Stretch(-8, -8, -8, -8);
            select.transform.SetAsFirstSibling();
            select.gameObject.SetActive(false);

            var area = Ui.Img(fill.transform, "PortraitArea", Theme.Surface3, Sprites.Rounded);
            area.type = Image.Type.Sliced;
            area.rectTransform.Row(8, 8, -8, 140);
            Ui.Img(area.transform, "Shade", Theme.WithAlpha(Theme.Navy, 0.45f), Sprites.VerticalGradient).rectTransform.Stretch();
            var strip = Ui.Img(area.transform, "Family", Theme.Primary, Sprites.RoundedSmall);
            strip.rectTransform.anchorMin = new Vector2(0, 0);
            strip.rectTransform.anchorMax = new Vector2(1, 0);
            strip.rectTransform.pivot = new Vector2(0.5f, 0);
            strip.rectTransform.sizeDelta = new Vector2(0, 8);
            var portrait = ctx.Nest<PortraitView>("Widgets/Portrait.prefab", area.transform, "Portrait");
            ((RectTransform)portrait.transform).At(0.5f, 0.5f, 0, 12, 100, 100);
            var unitName = Ui.Txt(area.transform, "", 32, Color.white, TextAnchor.LowerLeft, FontKind.Title, "Name");
            unitName.rectTransform.Stretch(12, 12, 8, 100);
            unitName.AutoSize(32);
            unitName.Inked();
            var hp = Ui.Bar(fill.transform, "Hp", Theme.Hp, Theme.Surface3, true, 20);
            ((RectTransform)hp.transform).Row(12, 12, -152, 24);
            var gauge = Ui.Bar(fill.transform, "Gauge", Theme.Gauge, Theme.Surface3);
            gauge.Smooth = false;
            ((RectTransform)gauge.transform).Row(12, 12, -180, 12);
            var statuses = Ui.Txt(fill.transform, "", 20, Theme.TextMuted, TextAnchor.UpperLeft, FontKind.Body, "Statuses");
            statuses.rectTransform.Row(12, 12, -196, 24);
            var sub = Ui.Txt(fill.transform, "", 20, Theme.AccentDark, TextAnchor.UpperLeft, FontKind.Body, "Sub");
            sub.rectTransform.Row(12, 12, -218, 24);
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
            ctx.Wire(view, "sub", sub);
            ctx.Wire(view, "portraitArea", area);
            ctx.Wire(view, "ko", ko);
            ctx.Wire(view, "readyPulse", pulse);
            ctx.Wire(view, "group", group);
            ctx.Wire(view, "button", button);
            ctx.Wire(view, "layout", layout);
            ctx.Wire(view, "floatPrefab", ctx.Asset("Widgets/FloatingText.prefab").GetComponent<FloatingTextView>());
            return frame.gameObject;
        }
    }

    /// <summary>
    /// Carte compacte d'un allié (colonne de gauche du combat) : portrait rond cerclé de la couleur de famille, nom, PV, jauge, statuts et ligne
    /// fixe (fatigue). 440 × 108. L'agent dont c'est le tour reçoit un contour lumineux (SelectRing).
    /// </summary>
    public sealed class AllyCardRecipe : IPrefabRecipe
    {
        public string RelativePath => "Widgets/AllyCard.prefab";
        public int Order => 5;

        public GameObject Build(PrefabContext ctx)
        {
            var fill = RecipeKit.PanelRoot("AllyCard", Theme.Surface, 4f, 7f, true);
            var frame = fill.Frame();
            frame.sizeDelta = new Vector2(440, 108);
            var layout = frame.Pref(440, 108);
            var group = frame.gameObject.AddComponent<CanvasGroup>();

            var target = Ui.Img(frame, "TargetRing", Theme.Success, Sprites.Panel);
            target.rectTransform.Stretch(-8, -8, -8, -8);
            target.transform.SetAsFirstSibling();
            target.gameObject.AddComponent<Pulse>().Amplitude = 0.03f;
            target.gameObject.SetActive(false);
            var select = Ui.Img(frame, "SelectRing", Theme.Glow, Sprites.Panel);
            select.rectTransform.Stretch(-10, -10, -10, -10);
            select.transform.SetAsFirstSibling();
            select.gameObject.AddComponent<Pulse>().Amplitude = 0.025f;
            select.gameObject.SetActive(false);

            var area = Ui.Img(fill.transform, "PortraitArea", Theme.Surface3, Sprites.Circle);
            area.rectTransform.At(0f, 0.5f, 10, 0, 88, 88);
            var strip = Ui.Img(area.transform, "Family", Theme.Primary, Sprites.Ring);
            strip.rectTransform.Stretch(-4, -4, -4, -4);
            var portrait = ctx.Nest<PortraitView>("Widgets/Portrait.prefab", area.transform, "Portrait");
            ((RectTransform)portrait.transform).At(0.5f, 0.5f, 0, 0, 76, 76);
            var unitName = Ui.Txt(fill.transform, "", 28, Theme.Navy, TextAnchor.MiddleLeft, FontKind.Title, "Name");
            unitName.rectTransform.At(0f, 1f, 112, -8, 190, 32);
            unitName.AutoSize(28);
            var sub = Ui.Txt(fill.transform, "", 20, Theme.AccentDark, TextAnchor.MiddleRight, FontKind.Body, "Sub");
            sub.rectTransform.At(1f, 1f, -14, -10, 150, 28);
            var hp = Ui.Bar(fill.transform, "Hp", Theme.Hp, Theme.Surface3, true, 20);
            ((RectTransform)hp.transform).At(0f, 1f, 112, -44, 310, 24);
            if (hp.Label != null) hp.Label.color = Theme.Navy;
            var gauge = Ui.Bar(fill.transform, "Gauge", Theme.Gauge, Theme.Surface3);
            gauge.Smooth = false;
            ((RectTransform)gauge.transform).At(0f, 1f, 112, -72, 310, 10);
            var statuses = Ui.Txt(fill.transform, "", 20, Theme.TextMuted, TextAnchor.UpperLeft, FontKind.Body, "Statuses");
            statuses.rectTransform.At(0f, 0f, 112, 4, 310, 24);
            var ko = Ui.Txt(fill.transform, "K.O.", 48, Theme.Danger, TextAnchor.MiddleCenter, FontKind.Title, "KO");
            ko.rectTransform.Stretch();
            ko.Inked();
            ko.gameObject.SetActive(false);

            var pulse = frame.gameObject.AddComponent<Pulse>();
            pulse.Amplitude = 0.02f;
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
            var frame = Ui.Img(root, "Frame", Theme.Navy, Sprites.Diamond);
            frame.rectTransform.At(0.5f, 0.5f, 0, 0, 88, 88);
            var disc = Ui.Img(frame.transform, "Disc", Theme.Info, Sprites.Diamond);
            disc.rectTransform.Stretch(7, 7, 7, 7);
            var initial = Ui.Txt(disc.transform, "", 34, Color.white, TextAnchor.MiddleCenter, FontKind.Title, "Initial");
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

    /// <summary>Carte de compétence de l'éventail du bas d'écran (150 × 210) : numéro en losange, grande initiale, nom, recharge, anneau lumineux.</summary>
    public sealed class SkillIconRecipe : IPrefabRecipe
    {
        public const float Width = 150f;
        public const float Height = 210f;
        public string RelativePath => "Widgets/SkillIcon.prefab";
        public int Order => 0;

        public GameObject Build(PrefabContext ctx)
        {
            var root = PrefabContext.NewRoot("SkillIcon", Width, Height);
            root.anchorMin = root.anchorMax = root.pivot = new Vector2(0.5f, 0f);
            var group = root.gameObject.AddComponent<CanvasGroup>();
            var ring = Ui.Img(root, "Ring", Theme.Glow, Sprites.Panel);
            ring.rectTransform.Stretch(-9, -9, -9, -9);
            ring.raycastTarget = false;
            ring.gameObject.AddComponent<Pulse>().Amplitude = 0.03f;
            ring.gameObject.SetActive(false);
            var frame = Ui.Img(root, "Frame", Theme.FrostEdge, Sprites.Panel, true);
            frame.rectTransform.Stretch();
            var button = frame.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            var nav = button.navigation;
            nav.mode = Navigation.Mode.None;
            button.navigation = nav;
            var hold = frame.gameObject.AddComponent<SkillHoldTrigger>();
            var drag = frame.gameObject.AddComponent<SkillCardDrag>();
            ctx.Wire(drag, "card", root);
            ctx.Wire(drag, "group", group);
            var disc = Ui.Img(frame.transform, "Disc", Theme.Primary, Sprites.Panel);
            disc.rectTransform.Stretch(3, 3, 3, 3);
            var shine = Ui.Img(disc.transform, "Shine", new Color(1f, 1f, 1f, 0.35f), Sprites.Shine);
            shine.rectTransform.Stretch();
            var glyph = Ui.Txt(disc.transform, "A", 84, Theme.Navy, TextAnchor.MiddleCenter, FontKind.Title, "Glyph");
            glyph.rectTransform.At(0.5f, 1f, 0, -26, 134, 100);
            var label = Ui.Txt(disc.transform, "", 24, Theme.Navy, TextAnchor.MiddleCenter, FontKind.Title, "Label");
            label.rectTransform.At(0.5f, 0f, 0, 10, 134, 66);
            label.enableAutoSizing = true;
            label.fontSizeMax = 24;
            label.fontSizeMin = 20;
            var cooldownFill = Ui.Img(disc.transform, "CooldownFill", Theme.WithAlpha(Theme.Navy, 0.55f), Sprites.Circle);
            cooldownFill.rectTransform.Stretch();
            cooldownFill.type = Image.Type.Filled;
            cooldownFill.fillMethod = Image.FillMethod.Radial360;
            cooldownFill.fillOrigin = (int)Image.Origin360.Top;
            cooldownFill.fillClockwise = false;
            cooldownFill.raycastTarget = false;
            cooldownFill.gameObject.SetActive(false);
            var cooldown = Ui.Txt(disc.transform, "", 64, Color.white, TextAnchor.MiddleCenter, FontKind.Title, "Cooldown");
            cooldown.rectTransform.Stretch();
            cooldown.Inked();
            cooldown.gameObject.SetActive(false);
            var slotBadge = Ui.Img(root, "SlotBadge", Theme.Navy, Sprites.Diamond);
            slotBadge.rectTransform.At(0f, 1f, -10, 10, 44, 44);
            slotBadge.raycastTarget = false;
            var slot = Ui.Txt(slotBadge.transform, "1", 22, Color.white, TextAnchor.MiddleCenter, FontKind.Title, "Slot");
            slot.rectTransform.Stretch();

            var view = root.gameObject.AddComponent<SkillIconView>();
            ctx.Wire(view, "button", button);
            ctx.Wire(view, "disc", disc);
            ctx.Wire(view, "cooldownFill", cooldownFill);
            ctx.Wire(view, "ring", ring);
            ctx.Wire(view, "glyph", glyph);
            ctx.Wire(view, "slot", slot);
            ctx.Wire(view, "cooldown", cooldown);
            ctx.Wire(view, "label", label);
            ctx.Wire(view, "hold", hold);
            ctx.Wire(view, "drag", drag);
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
            var n = Ui.Txt(row, "", 28, Theme.Navy, TextAnchor.MiddleLeft, FontKind.Title, "Name");
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
    /// alliés en bas à droite, panneau de compétences et journal à gauche, consigne dans la bande centrale.
    /// </summary>
    public sealed class CombatRecipe : IPrefabRecipe
    {
        public string RelativePath => "Screens/Combat.prefab";
        public int Order => 20;

        /// <summary>Deux rangées de deux cartes, décalées l'une de l'autre (alignement de la 1re puis de la 2e rangée).</summary>
        static void StaggeredRows(RectTransform block, TextAnchor first, TextAnchor second)
        {
            var anchors = new[] { first, second };
            for (int i = 0; i < 2; i++)
            {
                var row = Ui.Rect(block, "Row" + i);
                row.HLayout(20, 0, anchors[i]);
                row.gameObject.AddComponent<LayoutElement>().preferredHeight = 255;
            }
        }

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
            var debug = Ui.LocBtn(top, "combat.debug_win", Theme.Navy, Color.white, null!, 20, FontKind.Body, "DebugWin");
            debug.Rect.At(1f, 0.5f, -500, 0, 200, 46);

            // ---- Terrain central (là où se joueront les animations de compétence), alliés à gauche et ennemis à droite :
            // deux rangées décalées de deux cartes (croquis du 2026-10-23).
            var arena = Ui.Img(rt, "Arena", Theme.WithAlpha(Theme.Frost, 0.16f), Sprites.Rounded);
            arena.rectTransform.At(0.5f, 0.5f, 0, -36, 1000, 520);
            arena.raycastTarget = false;
            var enemyRow = Ui.Rect(rt, "Enemies").At(1f, 1f, -36, -250, 500, 530);
            enemyRow.VLayout(20, 0, TextAnchor.UpperRight);
            RecipeKit.Mark(enemyRow, TutorialTarget.CombatEnemies);
            StaggeredRows(enemyRow, TextAnchor.UpperLeft, TextAnchor.UpperRight);
            var allyRow = Ui.Rect(rt, "Allies").At(0f, 1f, 36, -250, 500, 530);
            allyRow.VLayout(20, 0, TextAnchor.UpperLeft);
            StaggeredRows(allyRow, TextAnchor.UpperRight, TextAnchor.UpperLeft);

            // ---- Journal (haut droite, discret)
            var logFill = Ui.Panel(rt, "Log", Theme.WithAlpha(Theme.Frost, 0.55f), 3f, 0f, false);
            logFill.Frame().At(1f, 0f, -24, 24, 330, 170);
            logFill.gameObject.AddComponent<RectMask2D>();
            var logText = Ui.Txt(logFill.transform, "", 21, Theme.TextMuted, TextAnchor.LowerLeft, FontKind.Regular, "Lines");
            logText.rectTransform.Stretch(14, 10, 14, 10);

            // ---- Bandeau central, consigne
            var banner = Ui.Txt(rt, "", 76, Theme.Primary, TextAnchor.MiddleCenter, FontKind.Title, "Banner");
            banner.rectTransform.At(0.5f, 0.5f, 0, 40, 1000, 120);
            banner.Inked();
            var bannerGroup = banner.gameObject.AddComponent<CanvasGroup>();
            bannerGroup.blocksRaycasts = false;
            var hintBand = Ui.Img(rt, "HintBand", Theme.WithAlpha(Theme.Primary, 0.9f), Sprites.RoundedSmall);
            hintBand.rectTransform.At(0.5f, 0f, 0, 380, 900, 52);
            var hint = Ui.Txt(hintBand.transform, "", 30, Theme.TextDark, TextAnchor.MiddleCenter, FontKind.Title, "Hint");
            hint.rectTransform.Stretch(10, 0, 10, 0);

            // ---- Frise chronologique : grande bande mise en valeur au centre (100 à gauche → 0 à droite : à 0, c'est le tour)
            var glow = Ui.Img(rt, "TimelineGlow", Theme.WithAlpha(Theme.Primary, 0.35f), Sprites.Rounded);
            glow.rectTransform.At(0.5f, 1f, 0, -98, 2100, 132);
            var stripFill = Ui.Panel(rt, "Timeline", Theme.WithAlpha(Theme.Frost, 0.94f), 5f, 8f, false);
            var strip = stripFill.Frame().At(0.5f, 1f, 0, -104, 2084, 120);
            var track = Ui.Rect(stripFill.transform, "Track");
            track.anchorMin = new Vector2(0f, 0.5f);
            track.anchorMax = new Vector2(1f, 0.5f);
            track.pivot = new Vector2(0.5f, 0.5f);
            track.offsetMin = new Vector2(120f, -9f);
            track.offsetMax = new Vector2(-150f, 9f);
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
            gate.rectTransform.sizeDelta = new Vector2(22, 110);
            gate.rectTransform.anchoredPosition = Vector2.zero;
            gate.gameObject.AddComponent<Pulse>().Amplitude = 0.12f;
            var timeline = strip.gameObject.AddComponent<TimelineView>();
            ctx.Wire(timeline, "track", track);
            ctx.Wire(timeline, "prepZone", prep.rectTransform);
            ctx.Wire(timeline, "markerPrefab", ctx.Asset("Widgets/TimelineMarker.prefab").GetComponent<TimelineMarkerView>());


            // ---- Éventail de compétences (bas centre) : visible seulement pour l'agent dont c'est le tour ; la 1re carte à gauche penche à gauche
            var fanRoot = Ui.Rect(rt, "SkillFan").At(0.5f, 0f, 0, -24, 760, 240);
            RecipeKit.Mark(fanRoot, TutorialTarget.CombatSkills, enabled: false);
            var fanMarker = fanRoot.GetComponent<TutorialTargetMarker>();
            float[] fanX = { -190f, 0f, 190f, 380f };
            float[] fanY = { 0f, 16f, 0f, 0f };
            float[] fanRot = { 8f, 0f, -8f, -14f };
            var fan = new Object[4];
            for (int i = 0; i < 4; i++)
            {
                var card = ctx.Nest<SkillIconView>("Widgets/SkillIcon.prefab", fanRoot, "Card" + (i + 1));
                var crt = (RectTransform)card.transform;
                crt.anchorMin = crt.anchorMax = new Vector2(0.5f, 0f);
                crt.pivot = new Vector2(0.5f, 0f);
                crt.sizeDelta = new Vector2(SkillIconRecipe.Width, SkillIconRecipe.Height);
                crt.anchoredPosition = new Vector2(fanX[i], fanY[i] + 48f);
                crt.localEulerAngles = new Vector3(0f, 0f, fanRot[i]);
                fan[i] = card;
            }

            // ---- Description de l'effet : affichée pendant l'appui maintenu sur une carte
            var descFill = Ui.Panel(rt, "Description", Theme.Navy, 3f, 8f, false);
            descFill.Frame().At(0.5f, 0f, 0, 262, 700, 150);
            var descTitle = Ui.Txt(descFill.transform, "", 34, Theme.Glow, TextAnchor.UpperLeft, FontKind.Title, "DescTitle");
            descTitle.rectTransform.At(0f, 1f, 22, -10, 656, 44);
            var descText = Ui.Txt(descFill.transform, "", 26, Color.white, TextAnchor.UpperLeft, FontKind.Body, "DescText");
            descText.rectTransform.At(0f, 1f, 22, -58, 656, 86);
            descFill.Frame().gameObject.SetActive(false);

            // ---- Zone de dépose (compétence générale) : visible seulement pendant le glissement d'une carte sans cible
            var dropFill = Ui.Panel(rt, "DropZone", Theme.WithAlpha(Theme.Glow, 0.35f), 3f, 0f, false);
            dropFill.Frame().At(0.5f, 0f, 0, 262, 1000, 120);
            var dropText = Ui.LocTxt(dropFill.transform, "combat.drop_here", 44, Theme.Navy, TextAnchor.MiddleCenter, FontKind.Title, "Label");
            dropText.rectTransform.Stretch();
            dropFill.Frame().gameObject.AddComponent<Pulse>().Amplitude = 0.02f;
            dropFill.Frame().gameObject.SetActive(false);

            var view = rt.gameObject.AddComponent<CombatView>();
            ScreenKit.WireFade(ctx, view);
            ctx.Wire(view, "retreat", retreat);
            ctx.Wire(view, "auto", auto);
            ctx.Wire(view, "speed", speed);
            ctx.Wire(view, "debugWin", debug);
            ctx.Wire(view, "pause", pause);
            ctx.Wire(view, "timeline", timeline);
            ctx.Wire(view, "title", title);
            ctx.Wire(view, "hint", hint);
            ctx.Wire(view, "logText", logText);
            ctx.Wire(view, "banner", banner);
            ctx.Wire(view, "bannerGroup", bannerGroup);
            ctx.Wire(view, "enemyRow", enemyRow);
            ctx.Wire(view, "allyRow", allyRow);
            ctx.Wire(view, "unitCardPrefab", ctx.Asset("Widgets/UnitCard.prefab").GetComponent<UnitCardView>());
            ctx.Wire(view, "allyCardPrefab", ctx.Asset("Widgets/UnitCard.prefab").GetComponent<UnitCardView>());
            ctx.Wire(view, "fanRoot", fanRoot);
            ctx.WireArray(view, "fan", fan);
            ctx.Wire(view, "fanMarker", fanMarker);
            ctx.Wire(view, "descPanel", descFill.Frame());
            ctx.Wire(view, "descTitle", descTitle);
            ctx.Wire(view, "descText", descText);
            ctx.Wire(view, "dropZone", dropFill.Frame());
            return rt.gameObject;
        }
    }
}
