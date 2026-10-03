#nullable enable
using System.Collections.Generic;
using VContainer.Unity;

namespace N29.HeroesDawn.Presentation.Mvp
{
    /// <summary>
    /// Navigation « Retour » : la modale du dessus d'abord (fermée si annulable, sinon l'appui est ignoré),
    /// puis le dernier écran visible enregistré. Lit l'entrée à chaque image (point d'entrée du scope racine).
    /// </summary>
    public sealed class BackButtonRouter : ITickable
    {
        readonly IBackInput _input;
        readonly IModalService _modals;
        readonly List<IBackHandler> _handlers = new List<IBackHandler>();

        public BackButtonRouter(IBackInput input, IModalService modals)
        {
            _input = input;
            _modals = modals;
        }

        public void Push(IBackHandler h)
        {
            _handlers.Remove(h);
            _handlers.Add(h);
        }

        public void Remove(IBackHandler h) => _handlers.Remove(h);

        public void Tick()
        {
            if (_input.BackPressedThisFrame) Dispatch();
        }

        /// <summary>Traite un appui sur Retour ; true s'il a été consommé.</summary>
        public bool Dispatch()
        {
            if (_modals.AnyOpen)
            {
                _modals.TryCancelTop();
                return true;
            }
            return _handlers.Count > 0 && _handlers[_handlers.Count - 1].HandleBack();
        }
    }
}
