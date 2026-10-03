#nullable enable
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Audio
{
    /// <summary>Habille les toasts d'un son selon leur nature (info, récompense, alerte, erreur).</summary>
    public sealed class AudibleToastService : IToastService
    {
        readonly IToastService _inner;
        readonly IAudioService _audio;

        public AudibleToastService(IToastService inner, IAudioService audio)
        {
            _inner = inner;
            _audio = audio;
        }

        public void Show(string text, ToastKind kind)
        {
            _inner.Show(text, kind);
            _audio.Play(For(kind));
        }

        public static SfxId For(ToastKind kind) => kind switch
        {
            ToastKind.Reward => SfxId.ToastReward,
            ToastKind.Warning => SfxId.ToastWarning,
            ToastKind.Error => SfxId.UiError,
            _ => SfxId.ToastInfo,
        };
    }
}
