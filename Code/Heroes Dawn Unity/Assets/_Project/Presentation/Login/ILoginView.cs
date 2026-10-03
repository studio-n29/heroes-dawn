#nullable enable
using System;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Login
{
    /// <summary>Écran titre : bouton Jouer, état de connexion, animation d'arrivée du logo.</summary>
    public interface ILoginView : IView
    {
        event Action? PlayClicked;
        void SetBusy(bool busy, string status);
        void PlayIntro();
    }
}
