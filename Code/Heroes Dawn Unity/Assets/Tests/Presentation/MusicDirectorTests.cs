#nullable enable
using MessagePipe;
using N29.HeroesDawn.Application.Flow;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Presentation.Audio;
using N29.HeroesDawn.Tests.Presentation.Fakes;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    public sealed class MusicDirectorTests
    {
        FakeAudio _audio = null!;
        IPublisher<GameStateChanged> _publish = null!;
        MusicDirector _director = null!;

        [SetUp]
        public void SetUp()
        {
            var b = new BuiltinContainerBuilder();
            b.AddMessagePipe();
            b.AddMessageBroker<GameStateChanged>();
            var bus = b.BuildServiceProvider();
            _publish = bus.GetRequiredService<IPublisher<GameStateChanged>>();
            _audio = new FakeAudio();
            _director = new MusicDirector(bus.GetRequiredService<ISubscriber<GameStateChanged>>(), _audio);
            _director.Start();
        }

        [TearDown]
        public void TearDown() => _director.Dispose();

        [Test, Description("Vérifie que chaque écran a son thème (connexion épique, QG calme, plateau, combat) — parce que la musique suit l'état du jeu, pas les écrans")]
        public void EachState_PlaysItsTheme()
        {
            _publish.Publish(new GameStateChanged(GameState.Boot, GameState.Login));
            _publish.Publish(new GameStateChanged(GameState.Login, GameState.Hub));
            _publish.Publish(new GameStateChanged(GameState.Hub, GameState.Board));
            _publish.Publish(new GameStateChanged(GameState.Board, GameState.Combat));
            Assert.That(_audio.MusicRequests, Is.EqualTo(new[]
            {
                (MusicId.MainTheme, false), (MusicId.Headquarters, false), (MusicId.Board, false), (MusicId.Combat, false),
            }));
        }

        [Test, Description("Vérifie qu'au retour du combat le thème du plateau reprend là où il avait été quitté, mais pas en venant du QG — parce que l'exploration est une seule et même séquence")]
        public void CombatToBoard_Resumes()
        {
            _publish.Publish(new GameStateChanged(GameState.Combat, GameState.Board));
            _publish.Publish(new GameStateChanged(GameState.Board, GameState.Hub));
            _publish.Publish(new GameStateChanged(GameState.Hub, GameState.Board));
            Assert.That(_audio.MusicRequests[0], Is.EqualTo((MusicId.Board, true)));
            Assert.That(_audio.MusicRequests[2], Is.EqualTo((MusicId.Board, false)));
        }

        [Test, Description("Vérifie qu'un changement d'état relâche l'atténuation d'une fanfare — parce qu'une victoire ne doit pas laisser le thème suivant étouffé")]
        public void StateChange_ReleasesStingerDuck()
        {
            _publish.Publish(new GameStateChanged(GameState.Combat, GameState.Board));
            Assert.That(_audio.Ducks, Does.Contain((DuckLayer.Stinger, 1f)));
        }
    }
}
