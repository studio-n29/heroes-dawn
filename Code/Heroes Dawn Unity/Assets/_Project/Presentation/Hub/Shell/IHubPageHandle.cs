#nullable enable
using System;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Page ouverte : son titre ; Dispose détruit le présentateur, son scope et la vue (une seule fois).</summary>
    public interface IHubPageHandle : IDisposable
    {
        string Title { get; }
    }
}
