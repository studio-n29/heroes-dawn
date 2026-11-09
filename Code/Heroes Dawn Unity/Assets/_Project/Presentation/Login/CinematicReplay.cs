#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Audio;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Login
{
    /// <summary>
    /// Rejoue la cinématique longue depuis le jeu (page Chapitres) : même fenêtre que le lancement, avec sous-titres, voix de chaque réplique quand
    /// elle existe, musique de cinématique, puis retour à la musique d'avant. Le montage de lancement, lui, se joue au démarrage (FlowStarter).
    /// </summary>
    public sealed class CinematicReplay
    {
        readonly IModalService _modals;
        readonly ILocalization _loc;
        readonly IAudioService? _audio;
        readonly IVoicePlayer? _voice;

        public CinematicReplay(IModalService modals, ILocalization loc, IAudioService? audio = null, IVoicePlayer? voice = null)
        {
            _modals = modals; _loc = loc; _audio = audio; _voice = voice;
        }

        /// <summary>Joue la version longue ; se termine à la fin, à « Passer » ou à l'annulation.</summary>
        public async UniTask PlayFullAsync(CancellationToken ct)
        {
            if (!CinematicSwitch.Enabled) return;
            var before = _audio?.CurrentMusic ?? MusicId.None;
            var view = await _modals.OpenAsync<IIntroPopupView>(ct);
            try
            {
                _audio?.PlayMusic(MusicId.Cinematic, 0.5f);
                await view.PlayCinematicAsync(new IntroCinematicTexts { Skip = _loc.T("intro.skip") },
                    level => _audio?.Duck(DuckLayer.Modal, level, level > 0.5f ? 2.5f : 0.4f), ct,
                    CinematicKind.Full, key => _voice?.Play(key));
            }
            finally
            {
                _voice?.Stop();
                _audio?.Duck(DuckLayer.Modal, 1f, 0.4f);
                _modals.Close(view);
                if (before != MusicId.None) _audio?.PlayMusic(before, 1f, true);
            }
        }
    }
}

namespace N29.HeroesDawn.Presentation.Login
{
    /// <summary>Interrupteur global : la cinématique est retirée du jeu pour l'instant (lancement et rejeu). Remettre à true pour la réactiver.</summary>
    public static class CinematicSwitch
    {
        public static bool Enabled = false;
    }
}
