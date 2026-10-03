#nullable enable
using N29.HeroesDawn.Presentation.Common;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Screens
{
    /// <summary>Écran plein cadre construit par code. Joue le rôle de présentateur pour sa vue (MVP simplifié du M1).</summary>
    public abstract class UiScreen : MonoBehaviour
    {
        protected UiContext Ctx = null!;
        CanvasGroup _group = null!;
        public RectTransform Rect => (RectTransform)transform;

        bool _shown;

        public void Init(UiContext ctx)
        {
            gameObject.SetActive(false);
            Ctx = ctx;
            _group = gameObject.AddComponent<CanvasGroup>();
            Build();
        }

        /// <summary>Crée un écran plein cadre sous <paramref name="parent"/> (inactif jusqu'à <see cref="Show"/>).</summary>
        public static T Create<T>(RectTransform parent, string name, UiContext ctx) where T : UiScreen
        {
            var rt = Ui.Rect(parent, name).Stretch();
            rt.gameObject.SetActive(false);
            var screen = rt.gameObject.AddComponent<T>();
            screen.Init(ctx);
            return screen;
        }

        public void Show()
        {
            transform.SetAsLastSibling();
            gameObject.SetActive(true);
        }

        public void Hide() => gameObject.SetActive(false);

        // OnShow/OnHide suivent l'activation réelle : écran affiché/masqué, ou scène entière masquée par la machine d'états.
        void OnEnable()
        {
            if (Ctx == null || _shown) return;
            _shown = true;
            FadeIn();
            OnShow();
        }

        void OnDisable()
        {
            if (!_shown) return;
            _shown = false;
            OnHide();
        }

        protected abstract void Build();
        public virtual void OnShow() { }
        public virtual void OnHide() { }

        internal void FadeIn()
        {
            _group.alpha = 0f;
            Tween.Fade(_group, 1f, 0.2f);
        }

        /// <summary>Fond commun : couleur sombre + dégradé.</summary>
        protected void Backdrop(Color top, Color bottom)
        {
            var bg = Ui.Img(Rect, "Backdrop", bottom, null, true);
            bg.rectTransform.Stretch();
            var grad = Ui.Img(Rect, "Gradient", top, Sprites.VerticalGradient);
            grad.rectTransform.Stretch();
        }
        /// <summary>Illustration plein écran (mode « couvrir », sans déformation) avec voile sombre.</summary>
        protected Image? ArtBackdrop(Sprite? art, float alpha, float veil)
        {
            if (art == null) return null;
            var holder = Ui.Rect(Rect, "Art").Stretch();
            var mask = holder.gameObject.AddComponent<RectMask2D>();
            mask.enabled = true;
            var img = Ui.Img(holder, "Illustration", new Color(1, 1, 1, alpha), art);
            var fitter = img.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = art.rect.width / art.rect.height;
            if (veil > 0f)
            {
                var v = Ui.Img(holder, "Veil", new Color(0, 0, 0, veil));
                v.rectTransform.Stretch();
            }
            return img;
        }
    }

}
