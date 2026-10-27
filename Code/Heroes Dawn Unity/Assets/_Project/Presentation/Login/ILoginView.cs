#nullable enable
using System;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Login
{
    /// <summary>Écran titre : bouton Jouer, état de connexion, animation d'arrivée du logo.</summary>
    public interface ILoginView : IView
    {
        event Action? PlayClicked;
        /// <summary>Bouton « Se connecter » (compte joueur).</summary>
        event Action? AccountClicked;
        void SetAccountLabel(string label);
        void SetBusy(bool busy, string status);
        void PlayIntro();
    }
}
