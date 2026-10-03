#nullable enable
using System;
using MessagePipe;
using N29.HeroesDawn.Application.Flow;
using N29.HeroesDawn.Application.Messages;
using VContainer.Unity;

namespace N29.HeroesDawn.Presentation.Audio
{
    /// <summary>
    /// Musique par état du jeu : connexion → thème principal, QG, plateau, combat. Chaque transition réussie lance un fondu
    /// enchaîné (durée du catalogue) ; au retour du combat, le thème du plateau reprend là où il avait été quitté.
    /// </summary>
    public sealed class MusicDirector : IStartable, IDisposable
    {
        readonly ISubscriber<GameStateChanged> _changed;
        readonly IAudioService _audio;
        IDisposable? _subscription;

        public MusicDirector(ISubscriber<GameStateChanged> changed, IAudioService audio)
        {
            _changed = changed;
            _audio = audio;
        }

        public void Start() => _subscription = _changed.Subscribe(OnChanged);

        void OnChanged(GameStateChanged m)
        {
            var music = For(m.To);
            if (music == MusicId.None) return;
            _audio.Duck(DuckLayer.Stinger, 1f, 0.5f); // une fanfare de fin de combat ne doit pas étouffer le thème suivant
            _audio.PlayMusic(music, -1f, resume: m.From == GameState.Combat && m.To == GameState.Board);
        }

        public static MusicId For(GameState state) => state switch
        {
            GameState.Login => MusicId.MainTheme,
            GameState.Hub => MusicId.Headquarters,
            GameState.Board => MusicId.Board,
            GameState.Combat => MusicId.Combat,
            _ => MusicId.None,
        };

        public void Dispose() => _subscription?.Dispose();
    }
}
