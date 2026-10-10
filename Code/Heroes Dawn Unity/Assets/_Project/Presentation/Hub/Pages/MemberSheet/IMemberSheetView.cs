#nullable enable
using System;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Hub
{
    public interface IMemberSheetView : IView
    {
        event Action PreviousClicked;
        event Action NextClicked;
        /// <summary>Action principale : Entraîner (membre possédé), Recruter (héros de la boutique, tâche 4).</summary>
        event Action PrimaryClicked;
        event Action<HubPage> RoomClicked;
        /// <summary>Equipement : objets et cartes de l agent.</summary>
        event Action LoadoutClicked;
        void Show(MemberSheetData data);
    }
}
