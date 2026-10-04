#nullable enable
using System;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Flow;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Presentation.Audio;
using N29.HeroesDawn.Presentation.Feedback;
using UnityEngine;
using VContainer.Unity;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>
    /// Applique les réglages du joueur au moteur : volumes, cadence d'images, qualité, accessibilité, son en arrière-plan.
    /// Au démarrage, à chaque changement de réglage et à l'arrivée au QG (le profil chargé remplace les réglages).
    /// </summary>
    public sealed class SettingsApplier : IStartable, IDisposable
    {
        readonly PlayerSettings _settings;
        readonly IAudioService _audio;
        readonly ISubscriber<MetaChanged> _changed;
        readonly ISubscriber<GameStateChanged> _state;
        IDisposable? _subs;

        public SettingsApplier(PlayerSettings settings, IAudioService audio, ISubscriber<MetaChanged> changed, ISubscriber<GameStateChanged> state)
        {
            _settings = settings; _audio = audio; _changed = changed; _state = state;
        }

        public void Start()
        {
            var bag = DisposableBag.CreateBuilder();
            _changed.Subscribe(m => { if (m.Area == MetaArea.Settings) Apply(); }).AddTo(bag);
            _state.Subscribe(s => { if (s.To == GameState.Hub) Apply(); }).AddTo(bag);
            _subs = bag.Build();
            UnityEngine.Application.focusChanged += OnFocus;
            Apply();
        }

        public void Apply()
        {
            var s = _settings.Current;
            float master = Mathf.Clamp01(s.master / 100f);
            _audio.MusicVolume = master * Mathf.Clamp01(s.music / 100f);
            _audio.SfxVolume = master * Mathf.Clamp01(s.sfx / 100f);

            UnityEngine.Application.targetFrameRate = s.batterySaver || !s.fps60 ? 30 : 60;
            if (s.quality > 0)
            {
                int levels = QualitySettings.names.Length;
                if (levels > 0) QualitySettings.SetQualityLevel(Mathf.Clamp(levels * s.quality / 4, 0, levels - 1), true);
            }
            MotionSettings.Reduced = s.reduceShakes || s.reduceFlashes || s.batterySaver || !s.fullCombatEffects;
            MotionSettings.ExtendedMiniGame = s.extendedMiniGameTime;
        }

        void OnFocus(bool focused) => AudioListener.pause = !focused && _settings.Current.muteInBackground;

        public void Dispose()
        {
            _subs?.Dispose();
            UnityEngine.Application.focusChanged -= OnFocus;
        }
    }
}
