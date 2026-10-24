#nullable enable
using System;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Hub
{
    public interface IQgPageView : IView
    {
        event Action<HubPage> RoomClicked;
        event Action MissionClicked;
        event Action DailyClicked;
        event Action SecondaryClicked;
        /// <summary>Icône d'alerte Boutique touchée : ouvre son pop-up.</summary>
        event Action ShopAlertClicked;
        event Action QuestsClicked;
        void Show(QgPageData data);
    }
}
