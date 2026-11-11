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
        FakeMusicContext _context = null!;

        [SetUp]
        public void SetUp()
        {
            var b = new BuiltinContainerBuilder();
            b.AddMessagePipe();
            b.AddMessageBroker<GameStateChanged>();
            var bus = b.BuildServiceProvider();
            _publish = bus.GetRequiredService<IPublisher<GameStateChanged>>();
            _audio = new FakeAudio();
            _context = new FakeMusicContext();
            _director = new MusicDirector(bus.GetRequiredService<ISubscriber<GameStateChanged>>(), _audio, _context);
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

        [Test, Description("Vérifie que chaque ville a son thème de plateau et que l'inconnu retombe sur Board — parce que la musique doit toujours jouer")]
        public void For_Board_PicksCityTheme()
        {
            Assert.That(MusicDirector.For(GameState.Board, "new_york", false, false), Is.EqualTo(MusicId.BoardNewYork));
            Assert.That(MusicDirector.For(GameState.Board, "paris", false, false), Is.EqualTo(MusicId.BoardParis));
            Assert.That(MusicDirector.For(GameState.Board, "moscow", false, false), Is.EqualTo(MusicId.BoardMoscow));
            Assert.That(MusicDirector.For(GameState.Board, "salem", false, false), Is.EqualTo(MusicId.BoardSalem));
            Assert.That(MusicDirector.For(GameState.Board, null, false, false), Is.EqualTo(MusicId.Board));
            Assert.That(MusicDirector.For(GameState.Board, "inconnue", false, false), Is.EqualTo(MusicId.Board));
        }

        [Test, Description("Vérifie que le boss a son thème, le combat ordinaire le sien, le QG et la boutique chacun le leur — parce que les moments forts portent l'orchestre")]
        public void For_CombatHubLogin()
        {
            Assert.That(MusicDirector.For(GameState.Combat, "paris", true, false), Is.EqualTo(MusicId.Boss));
            Assert.That(MusicDirector.For(GameState.Combat, "paris", false, false), Is.EqualTo(MusicId.Combat));
            Assert.That(MusicDirector.For(GameState.Hub, null, false, true), Is.EqualTo(MusicId.Shop));
            Assert.That(MusicDirector.For(GameState.Hub, null, false, false), Is.EqualTo(MusicId.Headquarters));
            Assert.That(MusicDirector.For(GameState.Login, null, false, false), Is.EqualTo(MusicId.MainTheme));
            Assert.That(MusicDirector.For(GameState.Boot, null, false, false), Is.EqualTo(MusicId.None));
        }

        [Test, Description("Vérifie que l'entrée sur le plateau joue le thème de la ville de la zone — parce que chaque ville a son ambiance")]
        public void Board_PlaysCityTheme()
        {
            _context.City = "moscow";
            _publish.Publish(new GameStateChanged(GameState.Hub, GameState.Board));
            Assert.That(_audio.MusicRequests[0], Is.EqualTo((MusicId.BoardMoscow, false)));
        }

        [Test, Description("Vérifie qu'un combat de boss lance le thème du boss — parce que c'est le moment orchestral du jeu")]
        public void Combat_Boss_PlaysBossTheme()
        {
            _context.BossFight = true;
            _publish.Publish(new GameStateChanged(GameState.Board, GameState.Combat));
            Assert.That(_audio.MusicRequests[0], Is.EqualTo((MusicId.Boss, false)));
        }

        [Test, Description("Vérifie qu'après un boss le retour au plateau reprend le thème de la ville et non celui du boss — parce que l'exploration continue")]
        public void BossToBoard_ResumesCityTheme()
        {
            _context.City = "salem";
            _context.BossFight = false;
            _publish.Publish(new GameStateChanged(GameState.Combat, GameState.Board));
            Assert.That(_audio.MusicRequests[0], Is.EqualTo((MusicId.BoardSalem, true)));
        }

        [Test, Description("Vérifie qu'une zone inconnue retombe sur le plateau générique — parce qu'aucune zone ne doit rendre le jeu muet")]
        public void Board_UnknownZone_FallsBackToBoard()
        {
            _context.City = null;
            _publish.Publish(new GameStateChanged(GameState.Hub, GameState.Board));
            Assert.That(_audio.MusicRequests[0], Is.EqualTo((MusicId.Board, false)));
        }

        [Test, Description("Vérifie que la boutique a sa musique et que le QG revient en la quittant — parce que la boutique est un lieu à part")]
        public void Hub_ShopPage_SwitchesTheme()
        {
            _publish.Publish(new GameStateChanged(GameState.Login, GameState.Hub));
            _context.SetShop(true);
            _context.SetShop(false);
            Assert.That(_audio.MusicRequests.ConvertAll(r => r.Music), Is.EqualTo(new[] { MusicId.Headquarters, MusicId.Shop, MusicId.Headquarters }));
        }

        [Test, Description("Vérifie qu'un signal de boutique hors du QG est ignoré — parce que le plateau ne doit pas changer de musique")]
        public void ShopSignal_OutsideHub_Ignored()
        {
            _context.City = "paris";
            _publish.Publish(new GameStateChanged(GameState.Hub, GameState.Board));
            _context.SetShop(true);
            Assert.That(_audio.MusicRequests.Count, Is.EqualTo(1));
        }

        [Test, Description("Vérifie que deux signaux de boutique consécutifs ne relancent pas la musique — parce qu'un doublon recréerait un fondu inutile")]
        public void ShopSignal_Repeated_NoDuplicate()
        {
            _publish.Publish(new GameStateChanged(GameState.Login, GameState.Hub));
            _context.SetShop(true);
            _context.SetShop(true);
            Assert.That(_audio.MusicRequests.Count, Is.EqualTo(2));
        }

        [Test, Description("Vérifie qu'un changement d'état relâche l'atténuation d'une fanfare — parce qu'une victoire ne doit pas laisser le thème suivant étouffé")]
        public void StateChange_ReleasesStingerDuck()
        {
            _publish.Publish(new GameStateChanged(GameState.Combat, GameState.Board));
            Assert.That(_audio.Ducks, Does.Contain((DuckLayer.Stinger, 1f)));
        }
    }
}
