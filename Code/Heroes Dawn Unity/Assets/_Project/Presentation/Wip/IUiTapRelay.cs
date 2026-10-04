#nullable enable
using System;

namespace N29.HeroesDawn.Presentation.Wip
{
    /// <summary>Touchers remontés par les éléments du hub : élément BIENTÔT, bouton grisé avec sa raison.</summary>
    public interface IUiTapRelay
    {
        event Action<string>? WipTapped;
        event Action<string>? ReasonTapped;
    }
}
