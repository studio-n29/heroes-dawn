#nullable enable
using System;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Page ouverte : son titre, son tick ; Dispose détruit le présentateur, son scope et la vue (une seule fois).</summary>
    public interface IHubPageHandle : IDisposable
    {
        string Title { get; }

        /// <summary>Transmis par la coque à chaque image : les présentateurs d'un scope enfant ne sont pas tickés par VContainer.</summary>
        void Tick();
    }
}
