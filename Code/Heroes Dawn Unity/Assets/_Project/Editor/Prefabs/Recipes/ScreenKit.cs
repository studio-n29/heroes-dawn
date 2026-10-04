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

        /// <summary>Fond commun : couleur sombre + dégradé.</summary>
        public static void Backdrop(RectTransform rt, Color top, Color bottom)
        {
            Ui.Img(rt, "Backdrop", bottom, null, true).rectTransform.Stretch();
            Ui.Img(rt, "Gradient", top, Sprites.VerticalGradient).rectTransform.Stretch();
        }

        /// <summary>Illustration plein écran (mode « couvrir ») avec voile sombre.</summary>
        public static Image? ArtBackdrop(RectTransform rt, Sprite? art, float alpha, float veil)
        {
            if (art == null) return null;
            var holder = Ui.Rect(rt, "Art").Stretch();
            holder.gameObject.AddComponent<RectMask2D>();
            var img = Ui.Img(holder, "Illustration", new Color(1, 1, 1, alpha), art);
            var fitter = img.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = art.rect.width / art.rect.height;
            if (veil > 0f) Ui.Img(holder, "Veil", new Color(0, 0, 0, veil)).rectTransform.Stretch();
            var comicMat = RecipeKit.Mat("UiBackdrop");
            if (comicMat != null)
            {
                var comic = Ui.Img(rt, "Comic", Color.black);
                comic.material = comicMat;
                comic.rectTransform.Stretch();
            }
            return img;
        }
    }
}
