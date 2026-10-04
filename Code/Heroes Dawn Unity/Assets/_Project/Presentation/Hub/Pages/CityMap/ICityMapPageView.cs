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
        void Show(CityMapPageData data);
    }
}
