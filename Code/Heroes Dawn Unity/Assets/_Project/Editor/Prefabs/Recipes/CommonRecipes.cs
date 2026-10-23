#nullable enable
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Tutorial;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>Aides partagées par les recettes.</summary>
    static class RecipeKit
    {
        /// <summary>Désigne un élément au tutoriel (marqueur nommé ; <paramref name="enabled"/> faux : activé à l exécution).</summary>
        public static TutorialTargetMarker Mark(Component c, TutorialTarget id, bool enabled = true)
        {
            var m = c.gameObject.AddComponent<TutorialTargetMarker>();
            var so = new UnityEditor.SerializedObject(m);
            so.FindProperty("id").enumValueIndex = (int)id;
            so.ApplyModifiedPropertiesWithoutUndo();
            m.enabled = enabled;
            return m;
        }

        /// <summary>Matériau d interface généré par <c>UiMaterialBuilder</c> (Resources/UI/Materials) ; null s il n existe pas encore.</summary>
        /// <summary>Reflet animé sur un bouton d action (shader HD/UI/Shine).</summary>
        public static void Shine(UiButton b)
        {
            var m = Mat("UiShine");
            if (m != null) b.Fill.material = m;
        }

        public static Material? Mat(string name)
            => UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Resources/UI/Materials/" + name + ".mat");

        /// <summary>Case de BD dont le cadre est la racine du prefab ; renvoie le fond intérieur (parent du contenu).</summary>
        public static Image PanelRoot(string name, Color fill, float border = 4f, float shadow = 6f, bool raycast = false)
        {
            var holder = new GameObject("holder", typeof(RectTransform));
            var inner = Ui.Panel(holder.transform, name, fill, border, shadow, raycast);
            var frame = inner.Frame();
            frame.SetParent(null, false);
            Object.DestroyImmediate(holder);
            return inner;
        }

        /// <summary>Texte à taille ajustable (bornée à 20 px minimum, règle des maquettes).</summary>
        public static TextMeshProUGUI AutoSize(this TextMeshProUGUI t, float max)
        {
            t.enableAutoSizing = true;
            t.fontSizeMax = max;
            t.fontSizeMin = Mathf.Min(max, 20f);
            return t;
        }
    }

    /// <summary>Portrait provisoire : anneau, disque de famille, ombre, initiale, couronne de boss.</summary>
    public sealed class PortraitRecipe : IPrefabRecipe
    {
        public string RelativePath => "Widgets/Portrait.prefab";
        public int Order => 0;

        public GameObject Build(PrefabContext ctx)
        {
            var root = PrefabContext.NewRoot("Portrait", 96, 96);
            var ring = root.gameObject.AddComponent<Image>();
            ring.sprite = Sprites.Circle;
            ring.color = Theme.FrostEdge;
            ring.raycastTarget = false;
            var disc = Ui.Img(root, "Disc", Theme.Surface3, Sprites.Circle);
            disc.rectTransform.Stretch(4, 4, 4, 4);
            disc.gameObject.AddComponent<Mask>().showMaskGraphic = true;
            var bust = Ui.Img(disc.transform, "Bust", Theme.Navy, Sprites.Bust);
            bust.rectTransform.Stretch();
            var art = Ui.Img(disc.transform, "Art", Color.white, Sprites.Circle);
            art.rectTransform.Stretch();
            art.gameObject.AddComponent<AspectRatioFitter>().aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            art.gameObject.SetActive(false);
            var shade = Ui.Img(disc.transform, "Shade", Theme.WithAlpha(Theme.Navy, 0.25f), Sprites.VerticalGradient);
            shade.rectTransform.Stretch();
            shade.rectTransform.localScale = new Vector3(1, -1, 1);
            var initial = Ui.Txt(disc.transform, "?", 48, Color.white, TextAnchor.LowerCenter, FontKind.Title, "Initial").AutoSize(40);
            initial.rectTransform.Stretch();
            initial.Inked();
            var boss = Ui.Img(root, "Boss", Theme.Primary, Sprites.Ring);
            boss.rectTransform.Stretch(-6, -6, -6, -6);
            boss.gameObject.SetActive(false);
            var view = root.gameObject.AddComponent<PortraitView>();
            ctx.Wire(view, "disc", disc);
            ctx.Wire(view, "bust", bust);
            ctx.Wire(view, "art", art);
            ctx.Wire(view, "initial", initial);
            ctx.Wire(view, "boss", boss.gameObject);
            return root.gameObject;
        }
    }

    /// <summary>Pastille de ressource (Crédits ou PR) : badge rond + valeur.</summary>
    public sealed class ResourcePillRecipe : IPrefabRecipe
    {
        public string RelativePath => "Widgets/ResourcePill.prefab";
        public int Order => 0;

        public GameObject Build(PrefabContext ctx)
        {
            var fill = RecipeKit.PanelRoot("ResourcePill", Theme.SurfaceDark, 3f, 4f);
            var frame = fill.Frame();
            frame.sizeDelta = new Vector2(230, 62);
            var badge = Ui.Img(fill.transform, "Badge", Theme.Credits, Sprites.Circle);
            badge.rectTransform.At(0f, 0.5f, 6, 0, 46, 46);
            var tag = Ui.Txt(badge.transform, "CR", 20, Theme.TextDark, TextAnchor.MiddleCenter, FontKind.Title, "Tag");
            tag.rectTransform.Stretch();
            var value = Ui.Txt(fill.transform, "0", 34, Color.white, TextAnchor.MiddleRight, FontKind.Title, "Value");
            value.rectTransform.Stretch(60, 0, 16, 0);
            return frame.gameObject;
        }
    }

    /// <summary>Barre de ressources : indicateur de sauvegarde + pastilles Crédits et PR (imbriquées).</summary>
    public sealed class WalletRecipe : IPrefabRecipe
    {
        public string RelativePath => "Widgets/Wallet.prefab";
        public int Order => 5;

        public GameObject Build(PrefabContext ctx)
        {
            var root = PrefabContext.NewRoot("Wallet", 700, 70);
            root.HLayout(16, 0, TextAnchor.MiddleRight);
            var saving = Ui.Txt(root, "", 22, Theme.TextMuted, TextAnchor.MiddleRight, FontKind.Regular, "Saving");
            saving.Pref(170, 60);
            var credits = Pill(ctx, root, "PillCR", "CR", Theme.Credits);
            var pr = Pill(ctx, root, "PillPR", "PR", Theme.Pr);
            var view = root.gameObject.AddComponent<WalletView>();
            ctx.Wire(view, "credits", credits);
            ctx.Wire(view, "pr", pr);
            ctx.Wire(view, "saving", saving);
            return root.gameObject;
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

    /// <summary>Un message éphémère : récitatif jaune, bande d'accent, texte sur une ligne.</summary>
    public sealed class ToastRecipe : IPrefabRecipe
    {
        public string RelativePath => "Widgets/Toast.prefab";
        public int Order => 0;

        public GameObject Build(PrefabContext ctx)
        {
            var fill = RecipeKit.PanelRoot("Toast", Theme.WithAlpha(Theme.Primary, 0.78f), 4f, 0f);
            var frame = fill.Frame();
            frame.GetComponent<Image>().color = Theme.WithAlpha(Theme.Navy, 0.55f); // translucide : le plateau reste lisible dessous
            frame.Pref(-1, 64);
            var h = frame.HLayout(0, 0);
            h.padding = new RectOffset(34, 34, 12, 12);
            fill.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            var accent = Ui.Img(fill.transform, "Accent", Theme.Info, Sprites.RoundedSmall);
            var art = accent.rectTransform;
            art.anchorMin = new Vector2(0, 0);
            art.anchorMax = new Vector2(0, 1);
            art.pivot = new Vector2(0, 0.5f);
            art.sizeDelta = new Vector2(14, -8);
            art.anchoredPosition = new Vector2(6, 0);
            var label = Ui.Txt(frame, "", 30, Theme.TextDark, TextAnchor.MiddleCenter, FontKind.Body, "Label");
            label.textWrappingMode = TextWrappingModes.NoWrap;
            var group = frame.gameObject.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            var view = frame.gameObject.AddComponent<ToastItemView>();
            ctx.Wire(view, "label", label);
            ctx.Wire(view, "accent", accent);
            ctx.Wire(view, "group", group);
            return frame.gameObject;
        }
    }

    /// <summary>Pile des messages éphémères (haut de l'écran).</summary>
    public sealed class ToastsRecipe : IPrefabRecipe
    {
        public string RelativePath => "Common/Toasts.prefab";
        public int Order => 10;

        public GameObject Build(PrefabContext ctx)
        {
            var root = PrefabContext.NewRoot("Toasts");
            var stack = Ui.Rect(root, "Stack").At(0.5f, 1f, 0, -110, 900, 400);
            var v = stack.VLayout(10, 0, TextAnchor.UpperCenter);
            v.childForceExpandWidth = false;
            v.childControlWidth = true;
            var view = root.gameObject.AddComponent<ToastsView>();
            ctx.Wire(view, "stack", stack);
            ctx.Wire(view, "toastPrefab", ctx.Asset("Widgets/Toast.prefab").GetComponent<ToastItemView>());
            return root.gameObject;
        }
    }

    /// <summary>Couche des modales (les popups y sont instanciées).</summary>
    public sealed class ModalLayerRecipe : IPrefabRecipe
    {
        public string RelativePath => "Common/ModalLayer.prefab";
        public int Order => 10;

        public GameObject Build(PrefabContext ctx)
        {
            var root = PrefabContext.NewRoot("ModalLayer");
            root.gameObject.AddComponent<ModalLayer>();
            return root.gameObject;
        }
    }

    /// <summary>Confirmation : voile, case de BD, bandeau de titre, texte, Annuler / Valider.</summary>
    public sealed class ConfirmPopupRecipe : IPrefabRecipe
    {
        public string RelativePath => "Popups/ConfirmPopup.prefab";
        public int Order => 10;

        public GameObject Build(PrefabContext ctx)
        {
            var root = PrefabContext.NewRoot("ConfirmPopup");
            var group = root.gameObject.AddComponent<CanvasGroup>();
            var (window, content, title) = PopupKit.Window(root, 980, 480, Theme.Primary);
            var body = Ui.Txt(content, "", 34, Theme.Text, TextAnchor.MiddleCenter, FontKind.Body, "Body");
            body.rectTransform.Stretch(0, 120, 0, 10);
            var row = Ui.Rect(content, "Buttons").BottomBand(96, 0);
            row.HLayout(40, 0, TextAnchor.MiddleCenter);
            var cancel = Ui.Btn(row, "", Theme.Surface3, Theme.Text, null!, 36, FontKind.Title, "Cancel");
            cancel.Rect.Pref(300, 90);
            var ok = Ui.Btn(row, "", Theme.Primary, Theme.TextDark, null!, 40, FontKind.Title, "Ok");
            ok.Rect.Pref(340, 90);
            var view = root.gameObject.AddComponent<ConfirmPopupView>();
            ctx.Wire(view, "title", title);
            ctx.Wire(view, "body", body);
            ctx.Wire(view, "ok", ok);
            ctx.Wire(view, "cancel", cancel);
            ctx.Wire(view, "window", window);
            ctx.Wire(view, "group", group);
            return root.gameObject;
        }
    }

    /// <summary>Gabarit commun des popups (reprise de l'ancienne ModalLayer.Open).</summary>
    static class PopupKit
    {
        /// <summary>Voile bloquant + fenêtre centrée avec bandeau de titre ; renvoie (fenêtre, zone de contenu, titre).</summary>
        public static (RectTransform Window, RectTransform Content, TextMeshProUGUI Title) Window(RectTransform root, float width, float height, Color band)
        {
            var dim = Ui.Img(root, "Dim", Theme.Overlay, null, true);
            dim.rectTransform.Stretch();
            var fill = Ui.Panel(root, "Window", Theme.Surface, 6f, 12f, true);
            var window = fill.Frame().At(0.5f, 0.5f, 0, 0, width, height);
            var bandFill = Ui.Panel(window, "TitleBand", band, 4f, 0f, false);
            bandFill.Frame().At(0.5f, 1f, 0, 30, Mathf.Min(width - 80, 760), 84);
            var title = Ui.Txt(bandFill.transform, "", 50, Theme.TextDark, TextAnchor.MiddleCenter, FontKind.Title, "Title").AutoSize(50);
            title.rectTransform.Stretch(12, 0, 12, 0);
            var content = Ui.Rect(window, "Content").Stretch(36, 30, 36, 80);
            return (window, content, title);
        }
    }
}
