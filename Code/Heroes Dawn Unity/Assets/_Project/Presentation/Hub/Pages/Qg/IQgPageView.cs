#nullable enable
using System;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Hub
{
    public interface IQgPageView : IView
    {
        event Action<HubPage> RoomClicked;
        event Action MissionClicked;
        void Show(QgPageData data);
    }
}
