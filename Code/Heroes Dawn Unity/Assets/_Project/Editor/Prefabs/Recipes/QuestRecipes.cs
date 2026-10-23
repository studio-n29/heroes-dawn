#nullable enable
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>Ligne d'une quête principale : barre d'état colorée, titre, détail et état.</summary>
    public sealed class QuestMainRowRecipe : IPrefabRecipe
    {
        public string RelativePath => "Widgets/QuestMainRow.prefab";
        public int Order => 0;

        public GameObject Build(PrefabContext ctx)
        {
            var fill = RecipeKit.PanelRoot("QuestMainRow", Theme.Surface2, 4f, 4f, false);
            var rootRt = fill.Frame();
            rootRt.sizeDelta = new Vector2(1000, 112);
            rootRt.Pref(-1, 112, 1);
            var accent = Ui.Img(fill.transform, "Accent", Theme.Primary, null);
            accent.rectTransform.anchorMin = new Vector2(0f, 0f);
            accent.rectTransform.anchorMax = new Vector2(0f, 1f);
            accent.rectTransform.pivot = new Vector2(0f, 0.5f);
            accent.rectTransform.sizeDelta = new Vector2(10f, 0f);
            accent.rectTransform.anchoredPosition = Vector2.zero;
            var title = Ui.Txt(fill.transform, "", 34, Theme.Text, TextAnchor.MiddleLeft, FontKind.Title, "Title");
            title.rectTransform.At(0f, 1f, 30, -8, 640, 44);
            title.AutoSize(34);
            var detail = Ui.Txt(fill.transform, "", 22, Theme.Text, TextAnchor.UpperLeft, FontKind.Body, "Detail");
            detail.rectTransform.At(0f, 1f, 30, -54, 780, 56);
            var state = Ui.Txt(fill.transform, "", 26, Theme.Primary, TextAnchor.MiddleRight, FontKind.Title, "State");
            state.rectTransform.At(1f, 1f, -24, -10, 240, 40);

            var view = rootRt.gameObject.AddComponent<QuestMainRowView>();
            ctx.Wire(view, "accent", accent);
            ctx.Wire(view, "title", title);
            ctx.Wire(view, "detail", detail);
            ctx.Wire(view, "state", state);
            return rootRt.gameObject;
        }
    }

    /// <summary>Ligne d'une quête secondaire : titre, description, barre de progression, récompense et bouton « Réclamer ».</summary>
    public sealed class QuestSideRowRecipe : IPrefabRecipe
    {
        public string RelativePath => "Widgets/QuestSideRow.prefab";
        public int Order => 0;

        public GameObject Build(PrefabContext ctx)
        {
            var fill = RecipeKit.PanelRoot("QuestSideRow", Theme.Frost, 4f, 4f, false);
            var rootRt = fill.Frame();
            rootRt.sizeDelta = new Vector2(1000, 132);
            rootRt.Pref(-1, 132, 1);
            var title = Ui.Txt(fill.transform, "", 32, Theme.Text, TextAnchor.MiddleLeft, FontKind.Title, "Title");
            title.rectTransform.At(0f, 1f, 24, -8, 600, 42);
            title.AutoSize(32);
            var desc = Ui.Txt(fill.transform, "", 21, Theme.Text, TextAnchor.UpperLeft, FontKind.Body, "Desc");
            desc.rectTransform.At(0f, 1f, 24, -50, 620, 44);
            var bar = Ui.Bar(fill.transform, "Bar", Theme.Primary, Theme.Surface3, true, 20);
            ((RectTransform)bar.transform).At(0f, 0f, 24, 12, 460, 26);
            var reward = Ui.Txt(fill.transform, "", 22, Theme.Fatigue, TextAnchor.MiddleLeft, FontKind.Title, "Reward");
            reward.rectTransform.At(0f, 0f, 500, 10, 300, 30);
            var claim = Ui.Btn(fill.transform, "", Theme.Success, Theme.TextDark, null!, 30, FontKind.Title, "Claim");
            claim.Rect.At(1f, 0.5f, -20, 0, 240, 84);

            var view = rootRt.gameObject.AddComponent<QuestSideRowView>();
            ctx.Wire(view, "title", title);
            ctx.Wire(view, "description", desc);
            ctx.Wire(view, "bar", bar);
            ctx.Wire(view, "reward", reward);
            ctx.Wire(view, "claim", claim);
            return rootRt.gameObject;
        }
    }

    /// <summary>Suivi des quêtes du QG : quêtes principales de la campagne puis quêtes secondaires, dans une fenêtre à défilement.</summary>
    public sealed class QuestLogPopupRecipe : IPrefabRecipe
    {
        public string RelativePath => "Popups/QuestLogPopup.prefab";
        public int Order => 10;

        public GameObject Build(PrefabContext ctx)
        {
            var root = PrefabContext.NewRoot("QuestLogPopup");
            var group = root.gameObject.AddComponent<CanvasGroup>();
            var (window, content, title) = PopupKit.Window(root, 1280, 780, Theme.Info);

            var scrollBody = Ui.VScroll(content, out var scroll);
            ((RectTransform)scrollBody.parent).Stretch(0, 100, 0, 0); // laisse la place au bouton Fermer
            scrollBody.gameObject.AddComponent<VerticalLayoutGroup>();
            var v = scrollBody.GetComponent<VerticalLayoutGroup>();
            v.spacing = 14;
            v.padding = new RectOffset(0, 0, 0, 100);
            v.childControlWidth = true;
            v.childControlHeight = true;
            v.childForceExpandWidth = true;
            v.childForceExpandHeight = false;

            var mainTitle = Ui.Txt(scrollBody, "", 34, Theme.Primary, TextAnchor.MiddleLeft, FontKind.Title, "MainTitle");
            mainTitle.rectTransform.Pref(-1, 48, 1);
            mainTitle.Inked();
            var mainList = Ui.Rect(scrollBody, "MainList");
            var ml = mainList.gameObject.AddComponent<VerticalLayoutGroup>();
            ml.spacing = 10;
            ml.childControlWidth = true; ml.childControlHeight = true; ml.childForceExpandWidth = true; ml.childForceExpandHeight = false;
            mainList.Pref(-1, -1, 1);
            var sideTitle = Ui.Txt(scrollBody, "", 34, Theme.Primary, TextAnchor.MiddleLeft, FontKind.Title, "SideTitle");
            sideTitle.rectTransform.Pref(-1, 48, 1);
            sideTitle.Inked();
            var sideEmpty = Ui.Txt(scrollBody, "", 24, Theme.TextMuted, TextAnchor.MiddleLeft, FontKind.Body, "SideEmpty");
            sideEmpty.rectTransform.Pref(-1, 44, 1);
            var sideList = Ui.Rect(scrollBody, "SideList");
            var sl = sideList.gameObject.AddComponent<VerticalLayoutGroup>();
            sl.spacing = 10;
            sl.childControlWidth = true; sl.childControlHeight = true; sl.childForceExpandWidth = true; sl.childForceExpandHeight = false;
            sideList.Pref(-1, -1, 1);

            var close = Ui.Btn(content, "", Theme.Surface3, Theme.Text, null!, 36, FontKind.Title, "Close");
            close.Rect.At(0.5f, 0f, 0, 0, 320, 84);

            var mainRow = ctx.Asset("Widgets/QuestMainRow.prefab").GetComponent<QuestMainRowView>();
            var sideRow = ctx.Asset("Widgets/QuestSideRow.prefab").GetComponent<QuestSideRowView>();
            var view = root.gameObject.AddComponent<QuestLogPopupView>();
            ctx.Wire(view, "title", title);
            ctx.Wire(view, "mainTitle", mainTitle);
            ctx.Wire(view, "sideTitle", sideTitle);
            ctx.Wire(view, "sideEmpty", sideEmpty);
            ctx.Wire(view, "mainList", mainList);
            ctx.Wire(view, "sideList", sideList);
            ctx.Wire(view, "mainRowPrefab", mainRow);
            ctx.Wire(view, "sideRowPrefab", sideRow);
            ctx.Wire(view, "close", close);
            ctx.Wire(view, "window", window);
            ctx.Wire(view, "group", group);
            return root.gameObject;
        }
    }
}
