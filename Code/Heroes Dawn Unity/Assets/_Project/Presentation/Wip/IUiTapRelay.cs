#nullable enable
using System;

namespace N29.HeroesDawn.Presentation.Wip
{
    /// <summary>Touchers remontés par les éléments du hub : bouton grisé avec sa raison.</summary>
    public interface IUiTapRelay
    {
        event Action<string>? ReasonTapped;
    }
}
