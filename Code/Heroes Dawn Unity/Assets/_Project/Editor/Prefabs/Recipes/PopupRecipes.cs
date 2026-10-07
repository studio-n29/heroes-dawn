#nullable enable
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Popups;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>Base des recettes de popups de case : gabarit commun et branchement de la base de vue.</summary>
    static class CasePopupKit
    {
        public static (RectTransform Root, RectTransform Content, TextMeshProUGUI Title) Build(string name, float width, float height, Color band,
                                                                                            out RectTransform window)
        {
            var root = PrefabContext.NewRoot(name);
            root.gameObject.AddComponent<CanvasGroup>();
            var (w, content, title) = PopupKit.Window(root, width, height, band);
            window = w;
            return (root, content, title);
        }

        public static void WireBase(PrefabContext ctx, PopupViewBase view, TMP_Text title, RectTransform window)
        {
            ctx.Wire(view, "title", title);
            ctx.Wire(view, "window", window);
            ctx.Wire(view, "group", view.GetComponent<CanvasGroup>());
        }
    }

    /// <summary>Bouton de choix d'événement (pleine largeur).</summary>
    public sealed class ChoiceButtonRecipe : IPrefabRecipe
    {
        public string RelativePath => "Widgets/ChoiceButton.prefab";
        public int Order => 0;

        public GameObject Build(PrefabContext ctx)
        {
            var holder = PrefabContext.NewRoot("holder", 1000, 86);
            var b = Ui.Btn(holder, "", Theme.Primary, Theme.TextDark, null!, 30, FontKind.Body, "ChoiceButton");
            b.Rect.SetParent(null, false);
            Object.DestroyImmediate(holder.gameObject);
            b.Rect.sizeDelta = new Vector2(1000, 86);
            b.Rect.Pref(-1, 86);
            return b.gameObject;
        }
    }

    /// <summary>Case Repos (reprise de CasePopups.Rest).</summary>
    public sealed class RestPopupRecipe : IPrefabRecipe
    {
        public string RelativePath => "Popups/RestPopup.prefab";
        public int Order => 10;

        public GameObject Build(PrefabContext ctx)
        {
            var (root, content, title) = CasePopupKit.Build("RestPopup", 980, 500, Theme.Success, out var window);
            var body = Ui.Txt(content, "", 34, Theme.Text, TextAnchor.MiddleCenter, FontKind.Body, "Body");
            body.rectTransform.Stretch(0, 120, 0, 10);
            var row = Ui.Rect(content, "Buttons").BottomBand(96);
            row.HLayout(40);
            var skip = Ui.Btn(row, "", Theme.Surface3, Theme.Text, null!, 34, FontKind.Title, "Skip");
            skip.Rect.Pref(280, 90);
            var take = Ui.Btn(row, "", Theme.Success, Theme.TextDark, null!, 40, FontKind.Title, "Take");
            take.Rect.Pref(380, 90);
            var view = root.gameObject.AddComponent<RestPopupView>();
            CasePopupKit.WireBase(ctx, view, title, window);
            ctx.Wire(view, "body", body);
            ctx.Wire(view, "skip", skip);
            ctx.Wire(view, "take", take);
            return root.gameObject;
        }
    }

    /// <summary>Case Événement (reprise de CasePopups.Narrative).</summary>
    public sealed class NarrativePopupRecipe : IPrefabRecipe
    {
        public string RelativePath => "Popups/NarrativePopup.prefab";
        public int Order => 10;

        public GameObject Build(PrefabContext ctx)
        {
            var (root, content, title) = CasePopupKit.Build("NarrativePopup", 1180, 660, Theme.Perception, out var window);
            title.color = Color.white;
            title.Inked();
            var body = Ui.Txt(content, "", 32, Theme.Text, TextAnchor.UpperCenter, FontKind.Regular, "Body");
            body.rectTransform.Stretch(10, 330, 10, 20);
            var list = Ui.Rect(content, "Choices").BottomBand(310);
            list.VLayout(14, 0, TextAnchor.LowerCenter);
            var view = root.gameObject.AddComponent<NarrativePopupView>();
            CasePopupKit.WireBase(ctx, view, title, window);
            ctx.Wire(view, "body", body);
            ctx.Wire(view, "choices", list);
            ctx.Wire(view, "choicePrefab", ctx.Asset("Widgets/ChoiceButton.prefab").GetComponent<UiButton>());
            return root.gameObject;
        }
    }

    /// <summary>Case Mini-jeu (reprise de CasePopups.MiniGame et TimingMiniGame.Build).</summary>
    public sealed class MiniGamePopupRecipe : IPrefabRecipe
    {
        public string RelativePath => "Popups/MiniGamePopup.prefab";
        public int Order => 10;

        public GameObject Build(PrefabContext ctx)
        {
            var (root, content, title) = CasePopupKit.Build("MiniGamePopup", 1200, 700, Theme.Info, out var window);
            var body = Ui.Txt(content, "", 30, Theme.Text, TextAnchor.UpperCenter, FontKind.Regular, "Body");
            body.rectTransform.Stretch(10, 400, 10, 10);

            var trackFill = Ui.Panel(content, "Track", Theme.SurfaceDark, 4f, 4f, false);
            var track = trackFill.Frame().At(0.5f, 0f, 0, 230, 908, 70);
            var zone = Ui.Img(trackFill.transform, "Zone", Theme.Success, Sprites.RoundedSmall);
            var zrt = zone.rectTransform;
            zrt.anchorMin = zrt.anchorMax = new Vector2(0f, 0.5f);
            zrt.pivot = new Vector2(0.5f, 0.5f);
            zrt.sizeDelta = new Vector2(180, 56);
            var cursor = Ui.Img(trackFill.transform, "Cursor", Theme.Primary, Sprites.RoundedSmall);
            var crt = cursor.rectTransform;
            crt.anchorMin = crt.anchorMax = new Vector2(0f, 0.5f);
            crt.pivot = new Vector2(0.5f, 0.5f);
            crt.sizeDelta = new Vector2(16, 90);
            cursor.gameObject.AddComponent<Outline>().effectColor = Theme.Ink;
            var score = Ui.Txt(content, "", 28, Theme.TextMuted, TextAnchor.MiddleCenter, FontKind.Body, "Score");
            score.rectTransform.At(0.5f, 0f, 0, 170, 900, 40);
            var stop = Ui.Btn(content, "", Theme.Primary, Theme.TextDark, null!, 52, FontKind.Title, "Stop");
            stop.Rect.At(0.5f, 0f, 0, 20, 420, 110);

            var game = content.gameObject.AddComponent<TimingMiniGame>();
            ctx.Wire(game, "track", track);
            ctx.Wire(game, "cursor", crt);
            ctx.Wire(game, "zone", zrt);
            ctx.Wire(game, "zoneImage", zone);
            ctx.Wire(game, "score", score);
            ctx.Wire(game, "stop", stop);

            var view = root.gameObject.AddComponent<MiniGamePopupView>();
            CasePopupKit.WireBase(ctx, view, title, window);
            ctx.Wire(view, "body", body);
            ctx.Wire(view, "game", game);
            return root.gameObject;
        }
    }

    /// <summary>Activité hors réflexe (potion, survie, pêche, arène, dialogue) : fenêtre de test du framework d'activités.</summary>
    public sealed class ActivityPopupRecipe : IPrefabRecipe
    {
        public string RelativePath => "Popups/ActivityPopup.prefab";
        public int Order => 10;

        public GameObject Build(PrefabContext ctx)
        {
            var (root, content, title) = CasePopupKit.Build("ActivityPopup", 1300, 800, Theme.Info, out var window);
            var body = Ui.Txt(content, "", 28, Theme.Text, TextAnchor.UpperLeft, FontKind.Body, "Body");
            body.rectTransform.Stretch(24, 560, 24, 10);
            var details = Ui.Txt(content, "", 24, Theme.TextMuted, TextAnchor.UpperLeft, FontKind.Body, "Details");
            details.rectTransform.Stretch(24, 235, 24, 140);
            var wip = Ui.Txt(content, "", 24, Theme.Primary, TextAnchor.MiddleCenter, FontKind.Body, "Wip");
            wip.rectTransform.At(0.5f, 0f, 0, 130, 1240, 90);
            var win = Ui.Btn(content, "", Theme.Success, Theme.TextDark, null!, 34, FontKind.Title, "Win");
            win.Rect.At(0f, 0f, 24, 20, 580, 90);
            var fail = Ui.Btn(content, "", Theme.Danger, Color.white, null!, 34, FontKind.Title, "Fail");
            fail.Rect.At(1f, 0f, -24, 20, 580, 90);

            var view = root.gameObject.AddComponent<ActivityPopupView>();
            CasePopupKit.WireBase(ctx, view, title, window);
            ctx.Wire(view, "body", body);
            ctx.Wire(view, "details", details);
            ctx.Wire(view, "wip", wip);
            ctx.Wire(view, "win", win);
            ctx.Wire(view, "fail", fail);
            return root.gameObject;
        }
    }

    /// <summary>Zone sécurisée (reprise de BoardScreen.ShowZoneComplete).</summary>
    public sealed class ZoneCompletePopupRecipe : IPrefabRecipe
    {
        public string RelativePath => "Popups/ZoneCompletePopup.prefab";
        public int Order => 10;

        public GameObject Build(PrefabContext ctx)
        {
            var (root, content, title) = CasePopupKit.Build("ZoneCompletePopup", 1000, 520, Theme.Success, out var window);
            var rewards = Ui.Txt(content, "", 54, Theme.Primary, TextAnchor.MiddleCenter, FontKind.Title, "Rewards");
            rewards.rectTransform.Stretch(0, 130, 0, 20);
            rewards.Inked();
            var toHq = Ui.Btn(content, "", Theme.Primary, Theme.TextDark, null!, 44, FontKind.Title, "ToHq");
            toHq.Rect.At(0.5f, 0f, 0, 0, 440, 100);
            var view = root.gameObject.AddComponent<ZoneCompletePopupView>();
            CasePopupKit.WireBase(ctx, view, title, window);
            ctx.Wire(view, "rewards", rewards);
            ctx.Wire(view, "toHq", toHq);
            return root.gameObject;
        }
    }
}
