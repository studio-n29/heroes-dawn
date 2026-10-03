#nullable enable
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Audio
{
    /// <summary>Habille la couche des modales : son d'ouverture et de fermeture, musique atténuée tant qu'une modale est ouverte.</summary>
    public sealed class AudiblePopupHost : IPopupHost
    {
        const float ModalDuck = 0.55f;

        readonly IPopupHost _inner;
        readonly IAudioService _audio;

        public AudiblePopupHost(IPopupHost inner, IAudioService audio)
        {
            _inner = inner;
            _audio = audio;
        }

        public TView Spawn<TView>() where TView : class, IPopupView
        {
            var view = _inner.Spawn<TView>();
            _audio.Play(SfxId.PopupOpen);
            _audio.Duck(DuckLayer.Modal, ModalDuck, 0.3f);
            return view;
        }

        public void Despawn(IPopupView view)
        {
            _inner.Despawn(view);
            _audio.Play(SfxId.PopupClose);
            _audio.Duck(DuckLayer.Modal, 1f, 0.6f);
        }
    }
}
