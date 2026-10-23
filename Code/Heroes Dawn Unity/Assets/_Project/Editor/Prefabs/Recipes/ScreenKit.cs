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

        /// <summary>Fond commun : fond nuit (dégradé + vignettage). Les couleurs reçues, héritées du ciel pastel, sont ignorées.</summary>
        public static void Backdrop(RectTransform rt, Color top, Color bottom)
        {
            // Le fond d'écran reçoit les touchers (le relais de la coque s'appuie dessus), contrairement au fond nuit par défaut.
            Ui.NightBackdrop(rt).raycastTarget = true;
        }

        /// <summary>Les illustrations de fond sont pastel et hors thème sombre : elles ne sont plus affichées (les assets restent importés).</summary>
        public static Image? ArtBackdrop(RectTransform rt, Sprite? art, float alpha, float veil) => null;
    }
}
