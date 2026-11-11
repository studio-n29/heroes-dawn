#nullable enable
using System;

namespace N29.HeroesDawn.Presentation.Audio
{
    /// <summary>
    /// Ce que le directeur musical doit savoir au-delà de l'état du jeu : ville de la zone en cours (plateau),
    /// combat de boss, page de boutique ouverte au QG. Lecture seule ; <see cref="Changed"/> signale un changement de page.
    /// </summary>
    public interface IMusicContext
    {
        /// <summary>Identifiant de ville (<c>StoryRoutes.NewYork</c>…) de la zone en cours ; null hors zone.</summary>
        string? City { get; }
        bool BossFight { get; }
        bool ShopOpen { get; }
        event Action? Changed;
    }
}
