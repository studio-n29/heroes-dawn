#nullable enable
using System;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Hub
{
    public interface ITeamPageView : IView
    {
        /// <summary>Puce de filtre touchée : 0 = toutes les familles, 1 + (int)Family sinon.</summary>
        event Action<int> FilterClicked;
        event Action<MemberSort> SortClicked;
        event Action<MemberId> MemberTapped;
        event Action<MemberId> SquadToggled;
        event Action MissionClicked;
        void Show(TeamPageData data);
    }
}
