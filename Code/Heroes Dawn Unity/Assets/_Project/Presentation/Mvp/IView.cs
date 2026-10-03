#nullable enable
using System;

namespace N29.HeroesDawn.Presentation.Mvp
{
    /// <summary>Vue d'un écran : signale son affichage et son masquage (activation réelle, y compris scène masquée).</summary>
    public interface IView
    {
        event Action? Shown;
        event Action? Hidden;
        bool IsVisible { get; }
    }
}
