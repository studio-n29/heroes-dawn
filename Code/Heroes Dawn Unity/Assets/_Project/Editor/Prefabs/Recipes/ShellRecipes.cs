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
            var root = PrefabContext.NewRoot("HubWallet", 770, 70);
            root.HLayout(14, 0, TextAnchor.MiddleRight);
            var saving = Ui.Txt(root, "", 22, Theme.TextMuted, TextAnchor.MiddleRight, FontKind.Regular, "Saving");
            saving.Pref(150, 60);
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
            pill.Pref(190, 62);
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
            var go = N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<HubShellView>("Hub", "Hub/Hub.uxml", N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.ScreenOrder);
            var relay = go.AddComponent<UiTapRelay>();
            // Zone de page (uGUI) entre les barres, zones sûres de 88 px sur les côtés.
            var pageArea = Ui.Rect((RectTransform)go.transform, "PageArea").Stretch(Safe, NavH + 12, Safe, TopH + 12);

            var view = go.GetComponent<HubShellView>();
            ctx.Wire(view, "pageArea", pageArea);
            ctx.Wire(view, "relay", relay);
            return go;
        }
    }
}
