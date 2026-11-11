#nullable enable
using System;
using MessagePipe;
using N29.HeroesDawn.Application.Flow;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Definitions;
using VContainer.Unity;

namespace N29.HeroesDawn.Presentation.Audio
{
    /// <summary>
    /// Musique par état du jeu : connexion → thème principal, QG (ou boutique), plateau de la ville en cours, combat (ou boss).
    /// Chaque transition réussie lance un fondu enchaîné (durée du catalogue) ; au retour du combat, le thème du plateau
    /// reprend là où il avait été quitté.
    /// </summary>
    public sealed class MusicDirector : IStartable, IDisposable
    {
        readonly ISubscriber<GameStateChanged> _changed;
        readonly IAudioService _audio;
        readonly IMusicContext _context;
        IDisposable? _subscription;
        GameState _state = GameState.Boot;

        public MusicDirector(ISubscriber<GameStateChanged> changed, IAudioService audio, IMusicContext context)
        {
            _changed = changed;
            _audio = audio;
            _context = context;
        }

        public void Start()
        {
            _subscription = _changed.Subscribe(OnChanged);
            _context.Changed += OnContextChanged;
        }

        void OnChanged(GameStateChanged m)
        {
            _state = m.To;
            var music = For(m.To, _context.City, _context.BossFight, _context.ShopOpen);
            if (music == MusicId.None) return;
            _audio.Duck(DuckLayer.Stinger, 1f, 0.5f); // une fanfare de fin de combat ne doit pas étouffer le thème suivant
            _audio.PlayMusic(music, -1f, resume: m.From == GameState.Combat && m.To == GameState.Board);
        }

        /// <summary>Changement de page au QG : la boutique a sa musique, les autres pages gardent celle du QG.</summary>
        void OnContextChanged()
        {
            if (_state != GameState.Hub) return;
            var music = For(GameState.Hub, null, false, _context.ShopOpen);
            if (music != _audio.CurrentMusic) _audio.PlayMusic(music);
        }

        public static MusicId For(GameState state, string? city, bool boss, bool shop) => state switch
        {
            GameState.Login => MusicId.MainTheme,
            GameState.Hub => shop ? MusicId.Shop : MusicId.Headquarters,
            GameState.Board => ForCity(city),
            GameState.Combat => boss ? MusicId.Boss : MusicId.Combat,
            _ => MusicId.None,
        };

        /// <summary>Thème de plateau d'une ville ; <see cref="MusicId.Board"/> quand la ville est inconnue.</summary>
        public static MusicId ForCity(string? city) => city switch
        {
            StoryRoutes.NewYork => MusicId.BoardNewYork,
            StoryRoutes.Paris => MusicId.BoardParis,
            StoryRoutes.Moscow => MusicId.BoardMoscow,
            StoryRoutes.Salem => MusicId.BoardSalem,
            _ => MusicId.Board,
        };

        public void Dispose()
        {
            _subscription?.Dispose();
            _context.Changed -= OnContextChanged;
        }
    }
}
