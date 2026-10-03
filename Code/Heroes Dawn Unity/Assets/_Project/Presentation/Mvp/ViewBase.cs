#nullable enable
using System;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Mvp
{
    /// <summary>Base des vues : relaie OnEnable/OnDisable en Shown/Hidden pour le présentateur.</summary>
    public abstract class ViewBase : MonoBehaviour, IView
    {
        public event Action? Shown;
        public event Action? Hidden;
        public bool IsVisible { get; private set; }

        protected virtual void OnEnable()
        {
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
