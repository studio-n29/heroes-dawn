#nullable enable
using System;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Mvp
{
    /// <summary>Base des vues : relaie OnEnable/OnDisable en Shown/Hidden pour le présentateur.</summary>
    public abstract class ViewBase : MonoBehaviour, IView
    {
        /// <summary>Fondu d'apparition à chaque affichage (comme les anciens écrans).</summary>
        [SerializeField, OptionalRef] CanvasGroup? fadeGroup;

        public event Action? Shown;
        public event Action? Hidden;
        public bool IsVisible { get; private set; }

        protected virtual void OnEnable()
        {
            if (fadeGroup != null)
            {
                fadeGroup.alpha = 0f;
                Common.Tween.Fade(fadeGroup, 1f, 0.2f);
            }
            IsVisible = true;
            Shown?.Invoke();
        }

        protected virtual void OnDisable()
        {
            IsVisible = false;
            Hidden?.Invoke();
        }
    }
}
