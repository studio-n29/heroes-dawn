#nullable enable
using N29.HeroesDawn.Presentation.Activities;
using N29.HeroesDawn.Presentation.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>Bouton d'ingrédient de la potion (étroit, haut, couleur par propriété).</summary>
    public sealed class IngredientButtonRecipe : IPrefabRecipe
    {
        public string RelativePath => "Widgets/IngredientButton.prefab";
        public int Order => 0;

        public GameObject Build(PrefabContext ctx)
        {
            var holder = PrefabContext.NewRoot("holder", 112, 200);
            var b = Ui.Btn(holder, "", Theme.Primary, Theme.TextDark, null!, 22, FontKind.Body, "IngredientButton");
            b.Rect.SetParent(null, false);
            Object.DestroyImmediate(holder.gameObject);
            b.Rect.sizeDelta = new Vector2(112, 200);
            b.Rect.Pref(112, 200);
            return b.gameObject;
        }
    }

    /// <summary>Choix d'un agent de l'arène (large, deux lignes : nom de code et compétences).</summary>
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

    /// <summary>La potion magique : livre d'indices à gauche ; ingrédients, quantité, étapes, retour et « Préparer » à droite.</summary>
    public sealed class PotionPopupRecipe : IPrefabRecipe
    {
        public string RelativePath => "Popups/PotionPopup.prefab";
        public int Order => 11;

        public GameObject Build(PrefabContext ctx)
        {
            var (root, content, title) = CasePopupKit.Build("PotionPopup", 1740, 920, Theme.Success, out var window);
            var intro = Ui.Txt(content, "", 30, Theme.Text, TextAnchor.MiddleLeft, FontKind.Body, "Intro");
            intro.rectTransform.At(0f, 1f, 0, 0, 1100, 50);
            var attempts = Ui.Txt(content, "", 30, Theme.Primary, TextAnchor.MiddleRight, FontKind.Title, "Attempts");
            attempts.rectTransform.At(1f, 1f, 0, 0, 500, 50);

            var bookFill = Ui.Panel(content, "Book", Theme.WithAlpha(Theme.Primary, 0.18f), 4f, 6f, false);
            bookFill.Frame().At(0f, 0f, 0, 0, 640, 740);
            Ui.LocTxt(bookFill.transform, "stage.potion.book_title", 34, Theme.Primary, TextAnchor.MiddleCenter, FontKind.Title, "BookTitle").rectTransform.At(0.5f, 1f, 0, -10, 600, 52);
            var book = Ui.Txt(bookFill.transform, "", 25, Theme.Text, TextAnchor.UpperLeft, FontKind.Body, "BookText");
            book.rectTransform.Stretch(18, 14, 18, 70);

            var ingredientRow = Ui.Rect(content, "Ingredients").At(1f, 1f, 0, -62, 1000, 210);
            ingredientRow.HLayout(8, 0, TextAnchor.MiddleLeft);

            var qtyRow = Ui.Rect(content, "QtyRow").At(1f, 1f, 0, -290, 1000, 70);
            qtyRow.HLayout(14, 0, TextAnchor.MiddleLeft);
            var minus = Ui.Btn(qtyRow, "-", Theme.Surface3, Theme.Text, null!, 44, FontKind.Title, "QtyMinus");
            minus.Rect.Pref(70, 70);
            var qty = Ui.Txt(qtyRow, "", 32, Theme.Text, TextAnchor.MiddleCenter, FontKind.Title, "QtyText");
            qty.Pref(300, 70);
            var plus = Ui.Btn(qtyRow, "+", Theme.Surface3, Theme.Text, null!, 44, FontKind.Title, "QtyPlus");
            plus.Rect.Pref(70, 70);

            var steps = Ui.Txt(content, "", 28, Theme.Text, TextAnchor.UpperLeft, FontKind.Body, "Steps");
            steps.rectTransform.At(1f, 1f, 0, -372, 1000, 190);
            var feedback = Ui.Txt(content, "", 25, Theme.Text, TextAnchor.UpperLeft, FontKind.Body, "Feedback");
            feedback.rectTransform.At(1f, 1f, 0, -566, 1000, 170);

            var buttons = Ui.Rect(content, "Buttons").At(1f, 0f, 0, 0, 1000, 90);
            buttons.HLayout(12, 0, TextAnchor.MiddleRight);
            var undo = Ui.LocBtn(buttons, "stage.potion.undo", Theme.Surface3, Theme.Text, null!, 28, FontKind.Title, "Undo");
            undo.Rect.Pref(190, 84);
            var clear = Ui.LocBtn(buttons, "stage.potion.clear", Theme.Surface3, Theme.Text, null!, 28, FontKind.Title, "Clear");
            clear.Rect.Pref(190, 84);
            var brew = Ui.LocBtn(buttons, "stage.potion.brew", Theme.Primary, Theme.TextDark, null!, 40, FontKind.Title, "Brew");
            brew.Rect.Pref(380, 84);
            RecipeKit.Shine(brew);
            var quit = Ui.LocBtn(buttons, "stage.quit", Theme.Danger, Color.white, null!, 26, FontKind.Title, "Quit");
            quit.Rect.Pref(190, 84);

            var view = root.gameObject.AddComponent<PotionPopupView>();
            CasePopupKit.WireBase(ctx, view, title, window);
            ctx.Wire(view, "intro", intro);
            ctx.Wire(view, "book", book);
            ctx.Wire(view, "ingredientRow", ingredientRow);
            ctx.Wire(view, "ingredientPrefab", ctx.Asset("Widgets/IngredientButton.prefab").GetComponent<UiButton>());
            ctx.Wire(view, "qtyMinus", minus);
            ctx.Wire(view, "qtyPlus", plus);
            ctx.Wire(view, "qtyText", qty);
            ctx.Wire(view, "steps", steps);
            ctx.Wire(view, "attempts", attempts);
            ctx.Wire(view, "feedback", feedback);
            ctx.Wire(view, "undoButton", undo);
            ctx.Wire(view, "clearButton", clear);
            ctx.Wire(view, "brewButton", brew);
            ctx.Wire(view, "quitButton", quit);
            return root.gameObject;
        }
    }

    /// <summary>Le dialogue : réplique en haut, réponses au choix, réaction et progression.</summary>
    public sealed class DialoguePopupRecipe : IPrefabRecipe
    {
        public string RelativePath => "Popups/DialoguePopup.prefab";
        public int Order => 11;

        public GameObject Build(PrefabContext ctx)
        {
            var (root, content, title) = CasePopupKit.Build("DialoguePopup", 1400, 780, Theme.Perception, out var window);
            title.color = Color.white;
            title.Inked();
            var progress = Ui.Txt(content, "", 26, Theme.TextMuted, TextAnchor.MiddleRight, FontKind.Body, "Progress");
            progress.rectTransform.At(1f, 1f, 0, 0, 400, 40);
            var prompt = Ui.Txt(content, "", 40, Theme.Text, TextAnchor.UpperCenter, FontKind.Body, "Prompt");
            prompt.rectTransform.Stretch(20, 440, 20, 50);
            var choices = Ui.Rect(content, "Choices").At(0.5f, 0f, 0, 80, 1240, 350);
            choices.VLayout(12, 0, TextAnchor.LowerCenter);
            var reaction = Ui.Txt(content, "", 34, Theme.Text, TextAnchor.MiddleCenter, FontKind.Title, "Reaction");
            reaction.rectTransform.At(0.5f, 0f, 0, 10, 1200, 60);

            var view = root.gameObject.AddComponent<DialoguePopupView>();
            CasePopupKit.WireBase(ctx, view, title, window);
            ctx.Wire(view, "prompt", prompt);
            ctx.Wire(view, "progress", progress);
            ctx.Wire(view, "reaction", reaction);
            ctx.Wire(view, "choiceRow", choices);
            ctx.Wire(view, "choicePrefab", ctx.Asset("Widgets/ChoiceButton.prefab").GetComponent<UiButton>());
            return root.gameObject;
        }
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
