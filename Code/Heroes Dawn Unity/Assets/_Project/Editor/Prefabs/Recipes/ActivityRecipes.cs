#nullable enable
using N29.HeroesDawn.Presentation.Activities;
using N29.HeroesDawn.Presentation.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    public sealed class AgentButtonRecipe : IPrefabRecipe
    {
        public string RelativePath => "Widgets/AgentButton.prefab";
        public int Order => 0;

        public GameObject Build(PrefabContext ctx)
        {
            var holder = PrefabContext.NewRoot("holder", 330, 150);
            var b = Ui.Btn(holder, "", Theme.Info, Theme.TextDark, null!, 34, FontKind.Title, "AgentButton");
            b.Rect.SetParent(null, false);
            Object.DestroyImmediate(holder.gameObject);
            b.Rect.sizeDelta = new Vector2(330, 150);
            b.Rect.Pref(330, 150);
            return b.gameObject;
        }
    }
    /// <summary>La potion magique : fenêtre UI Toolkit (livre d'indices, ingrédients, quantité, étapes, « Préparer »).</summary>
    public sealed class PotionPopupRecipe : IPrefabRecipe
    {
        public string RelativePath => "Popups/PotionPopup.prefab";
        public int Order => 10;
        public GameObject Build(PrefabContext ctx) => N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<PotionPopupView>("PotionPopup", "Popups/Potion.uxml", N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.ModalOrder);
    }

    /// <summary>Le dialogue : réplique en haut, réponses au choix, réaction et progression.</summary>
    public sealed class DialoguePopupRecipe : IPrefabRecipe
    {
        public string RelativePath => "Popups/DialoguePopup.prefab";
        public int Order => 10;
        public GameObject Build(PrefabContext ctx) => N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<N29.HeroesDawn.Presentation.Activities.DialoguePopupView>("DialoguePopup", "Popups/Dialogue.uxml", N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.ModalOrder);
    }

    /// <summary>Activités 3D (survie, pêche, arène) : image de la scène, HUD, joystick, deux boutons, introduction et choix de l'agent.</summary>
    public sealed class StagePopupRecipe : IPrefabRecipe
    {
        public string RelativePath => "Popups/StagePopup.prefab";
        public int Order => 11;

        public GameObject Build(PrefabContext ctx)
        {
            var (root, content, title) = CasePopupKit.Build("StagePopup", 1840, 1000, Theme.Info, out var window);
            var viewportFrame = Ui.Panel(content, "ViewportFrame", Theme.SurfaceDark, 4f, 4f, false);
            viewportFrame.Frame().Stretch(0, 0, 0, 0);
            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(RawImage)).GetComponent<RawImage>();
            viewport.transform.SetParent(viewportFrame.transform, false);
            viewport.rectTransform.Stretch(6, 6, 6, 6);
            viewport.raycastTarget = false;
            viewport.color = Color.white;

            var line1 = Ui.Txt(content, "", 34, Color.white, TextAnchor.MiddleLeft, FontKind.Title, "Line1");
            line1.rectTransform.At(0f, 1f, 24, -14, 700, 50); line1.Inked();
            var line2 = Ui.Txt(content, "", 28, Color.white, TextAnchor.MiddleLeft, FontKind.Body, "Line2");
            line2.rectTransform.At(0f, 1f, 24, -62, 900, 44); line2.Inked();
            var barBox = Ui.Rect(content, "BarRoot").At(1f, 1f, -24, -18, 560, 40);
            var bar = Ui.Bar(barBox, "Bar", Theme.Success, Theme.SurfaceDark, true, 22);
            ((RectTransform)bar.transform).Stretch();
            var banner = Ui.Txt(content, "", 80, Color.white, TextAnchor.MiddleCenter, FontKind.Title, "Banner");
            banner.rectTransform.At(0.5f, 0.5f, 0, 120, 1500, 130); banner.Inked();

            var quit = Ui.LocBtn(content, "stage.quit", Theme.Danger, Color.white, null!, 24, FontKind.Title, "Quit");
            quit.Rect.At(1f, 1f, -24, -76, 190, 56);

            // Commandes : joystick à gauche, deux boutons d'action à droite.
            var controls = Ui.Rect(content, "Controls").Stretch();
            var stickBg = Ui.Img(controls, "Stick", Theme.WithAlpha(Color.white, 0.18f), Sprites.Circle, true);
            stickBg.rectTransform.At(0f, 0f, 70, 60, 240, 240);
            var knob = Ui.Img(stickBg.transform, "Knob", Theme.WithAlpha(Color.white, 0.55f), Sprites.Circle);
            knob.rectTransform.At(0.5f, 0.5f, 0, 0, 100, 100);
            knob.raycastTarget = false;
            var stick = stickBg.gameObject.AddComponent<VirtualStick>();
            ctx.Wire(stick, "knob", knob.rectTransform);
            var a = Ui.Btn(controls, "", Theme.Primary, Theme.TextDark, null!, 30, FontKind.Title, "ButtonA");
            a.Rect.At(1f, 0f, -60, 60, 220, 150);
            var holdA = a.gameObject.AddComponent<HoldButton>();
            var b = Ui.Btn(controls, "", Theme.Info, Theme.TextDark, null!, 30, FontKind.Title, "ButtonB");
            b.Rect.At(1f, 0f, -300, 60, 220, 150);
            var holdB = b.gameObject.AddComponent<HoldButton>();

            // Introduction (consignes, choix de l'agent, Commencer) : par-dessus la scène.
            var introFill = Ui.Panel(content, "Intro", Theme.WithAlpha(Theme.Frost, 0.96f), 4f, 6f, true);
            var intro = introFill.Frame();
            intro.Stretch(200, 120, 200, 120);
            var introTitle = Ui.Txt(introFill.transform, "", 56, Theme.Primary, TextAnchor.MiddleCenter, FontKind.Title, "IntroTitle");
            introTitle.rectTransform.At(0.5f, 1f, 0, -20, 1200, 80); introTitle.Inked();
            var introBody = Ui.Txt(introFill.transform, "", 30, Theme.Text, TextAnchor.UpperCenter, FontKind.Body, "IntroBody");
            introBody.rectTransform.Stretch(40, 330, 40, 120);
            var agentRow = Ui.Rect(introFill.transform, "Agents").At(0.5f, 0f, 0, 150, 1300, 160);
            agentRow.HLayout(14, 0, TextAnchor.MiddleCenter);
            var start = Ui.LocBtn(introFill.transform, "stage.start", Theme.Primary, Theme.TextDark, null!, 46, FontKind.Title, "Start");
            start.Rect.At(0.5f, 0f, 0, 30, 460, 100);
            RecipeKit.Shine(start);

            var view = root.gameObject.AddComponent<StageGameView>();
            CasePopupKit.WireBase(ctx, view, title, window);
            ctx.Wire(view, "viewport", viewport);
            ctx.Wire(view, "line1", line1);
            ctx.Wire(view, "line2", line2);
            ctx.Wire(view, "banner", banner);
            ctx.Wire(view, "bar", bar);
            ctx.Wire(view, "barRoot", barBox.gameObject);
            ctx.Wire(view, "controls", controls);
            ctx.Wire(view, "stick", stick);
            ctx.Wire(view, "buttonA", a);
            ctx.Wire(view, "buttonB", b);
            ctx.Wire(view, "holdA", holdA);
            ctx.Wire(view, "holdB", holdB);
            ctx.Wire(view, "introPanel", intro);
            ctx.Wire(view, "introTitle", introTitle);
            ctx.Wire(view, "introBody", introBody);
            ctx.Wire(view, "agentRow", agentRow);
            ctx.Wire(view, "agentPrefab", ctx.Asset("Widgets/AgentButton.prefab").GetComponent<UiButton>());
            ctx.Wire(view, "startButton", start);
            ctx.Wire(view, "quitButton", quit);
            return root.gameObject;
        }
    }
}
