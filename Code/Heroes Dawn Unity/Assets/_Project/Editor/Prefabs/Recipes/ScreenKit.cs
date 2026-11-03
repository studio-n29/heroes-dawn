#nullable enable
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>Gabarit des écrans plein cadre (reprise de l'ancien UiScreen : fonds, illustration, fondu).</summary>
    static class ScreenKit
    {
        /// <summary>Racine d'écran étirée, avec son groupe de fondu (branché sur la vue par <see cref="WireFade"/>).</summary>
        public static RectTransform Root(string name)
        {
            var rt = PrefabContext.NewRoot(name);
            rt.gameObject.AddComponent<CanvasGroup>();
            return rt;
        }

        public static void WireFade(PrefabContext ctx, ViewBase view) => ctx.Wire(view, "fadeGroup", view.GetComponent<CanvasGroup>());

        /// <summary>Fond commun : ciel pastel (dégradé haut → bas), lueur pêche à l'horizon et skyline en bas.</summary>
        public static void Backdrop(RectTransform rt, Color top, Color bottom)
        {
            Ui.Img(rt, "Backdrop", bottom, null, true).rectTransform.Stretch();
            // VerticalGradient est opaque en haut : la couleur « top » recouvre progressivement « bottom ».
            Ui.Img(rt, "Gradient", top, Sprites.VerticalGradient).rectTransform.Stretch();
            var glow = Ui.Img(rt, "Glow", Theme.WithAlpha(Theme.Glow, 0.22f), Sprites.Cloud);
            glow.rectTransform.At(0.5f, 0f, 0f, 60f, 1100f, 420f);
            var skyline = Ui.Img(rt, "Skyline", Theme.WithAlpha(Theme.Outline, 0.55f), Sprites.Skyline);
            var sk = skyline.rectTransform;
            sk.anchorMin = new Vector2(0f, 0f);
            sk.anchorMax = new Vector2(1f, 0f);
            sk.pivot = new Vector2(0.5f, 0f);
            sk.anchoredPosition = Vector2.zero;
            sk.sizeDelta = new Vector2(0f, 240f);
        }

        /// <summary>Illustration plein écran (mode « couvrir ») avec voile sombre.</summary>
        public static Image? ArtBackdrop(RectTransform rt, Sprite? art, float alpha, float veil)
        {
            // Décor illustré du Drive (3 variantes, attribuées par nom d'écran) à la place du fond d'origine ; opacité réduite pour la lisibilité.
            int pick = 0;
            foreach (char c in rt.name) pick += c;
            art = AgentArt.Environment(pick) ?? art;
            if (art == null) return null;
            var holder = Ui.Rect(rt, "Art").Stretch();
            holder.gameObject.AddComponent<RectMask2D>();
            var img = Ui.Img(holder, "Illustration", new Color(1, 1, 1, alpha * 0.55f), art);
            var fitter = img.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = art.rect.width / art.rect.height;
            if (veil > 0f) Ui.Img(holder, "Veil", Theme.WithAlpha(Theme.SkyMid, veil * 0.55f)).rectTransform.Stretch();
            return img;
        }
    }
}
