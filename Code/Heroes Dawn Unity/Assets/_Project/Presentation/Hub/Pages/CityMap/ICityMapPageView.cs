#nullable enable
using System;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Hub
{
    public interface ICityMapPageView : IView
    {
        event Action<string> ZoneSelected;
        event Action DeployClicked;
        event Action<string> LockedTapped;
        /// <summary>Bouton « Chapitres » touché : ouvre la page Chapitres.</summary>
        event Action ChaptersClicked;
        void Show(CityMapPageData data);
    }
}
