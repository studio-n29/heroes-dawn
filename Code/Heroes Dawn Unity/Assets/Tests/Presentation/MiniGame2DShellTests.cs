using System.Collections.Generic;
using System.Linq;
using System.Threading;
using N29.HeroesDawn.Application.Features.Zone;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Activities;
using N29.HeroesDawn.Domain.Activities.Games;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Presentation.Activities;
using N29.HeroesDawn.Presentation.Board;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Presentation.Popups;
using N29.HeroesDawn.Tests.Domain.Application;
using N29.HeroesDawn.Tests.Presentation.Fakes;
using NUnit.Framework;
using UnityEngine;

namespace N29.HeroesDawn.Tests.Presentation
{
    /// <summary>Coque commune des mini-jeux 2D (lot H) : un type avec pilote ouvre la fenêtre commune, un type sans pilote garde la fenêtre générique.</summary>
    public sealed class MiniGame2DShellTests
    {
        const string StubKind = "test_stub_2d";

        sealed class StubGame : IMiniGame
        {
            public GameStatus Status { get; set; } = GameStatus.Running;
            public int Score => 3;
            public int Target => 5;
            public float TimeLeft => -1f;
            public int Lives => -1;
            public void Step(float dt) { }
            public void Abandon() { if (Status == GameStatus.Running) Status = GameStatus.Lost; }
            public ActivityResult Result() => new ActivityResult(Status == GameStatus.Won, Score);
        }

        sealed class StubDriver : IMiniGameDriver
        {
            public readonly StubGame Stub = new StubGame();
            public int Disposed;
            public bool UsesStick => false;
            public IMiniGame Game => Stub;
            public MiniGameHud Hud => new MiniGameHud("3 / 5", -1f, -1);
            public void Build(RectTransform field) { }
            public void Tick(float dt, MiniGameInput input) { }
            public void Dispose() => Disposed++;
        }

        sealed class StubKindImpl : IActivityKind
        {
            public string Id => StubKind;
            public IReadOnlyList<ParameterDef> Schema { get; } = new ParameterDef[0];
            public ActivityLevel Generate(ActivityContext c) => new ActivityLevel(c.Seed, c.Environment, 0, 0, new LevelPrimitive[0], new LevelMarker[0]);
        }

        TestApp _app = null!;
        FakeModalHost _host = null!;
        FakeToasts _toasts = null!;
        FakeCaseResolutions _resolve = null!;
        FakeMiniGame2DPopup _window = null!;
        FakeActivityPopup _generic = null!;
        StubDriver? _driver;
        readonly LocLocalization _loc = new LocLocalization();

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _host = new FakeModalHost();
            _window = new FakeMiniGame2DPopup();
            _generic = new FakeActivityPopup();
            _host.Register<IMiniGame2DPopupView>(() => _window);
            _host.Register<IActivityPopupView>(() => _generic);
            _host.Register<IMiniGamePopupView>(() => new FakeMiniGamePopup());
            _toasts = new FakeToasts();
            _resolve = new FakeCaseResolutions();
            _driver = null;
            ActivityRegistry.Default.Register(new StubKindImpl());
            MiniGameDrivers.Register(StubKind, (plan, loc) => _driver = new StubDriver());
        }

        [TearDown]
        public void TearDown() => MiniGameDrivers.Unregister(StubKind);

        ContentCatalog With(ActivityDef extra)
        {
            var c = _app.Session.Content;
            return new ContentCatalog(c.Balance, c.Members, c.Enemies, c.Synergies, c.Zones, new[] { extra }.Concat(c.Activities).ToList());
        }

        static ActivityDef StubDef() => new ActivityDef(new ActivityId("stub2d"), StubKind, "activities.arena.name", "activities.arena.desc", credits: 50, pr: 2);

        void Run(IPlanActivityUseCase plan)
            => new CasePopupPresenter(new ModalService(_host), _resolve, _resolve, plan, _resolve, _app.Session, _toasts, _loc)
                .RunAsync(new CaseOutcome(new CaseId("n07"), CaseType.MiniGame, CombatRank.Easy, null), CancellationToken.None).GetAwaiter().GetResult();

        [Test, Description("Vérifie qu'un type d'activité qui a un pilote ouvre la fenêtre commune des mini-jeux 2D (et pas la fenêtre générique), que la case est résolue avec le résultat rendu et que le pilote est libéré — parce que chaque nouveau jeu passe par cette coque")]
        public void DriverKind_OpensTheCommonWindow_AndResolvesTheCase()
        {
            _window.Result = ActivityResult.Won(3);
            Run(new FakePlanActivity(With(StubDef()), "stub2d"));
            Assert.That(_window.Opened, Is.EqualTo(1));
            Assert.That(_generic.Data, Is.Null, "la fenêtre générique n'est pas ouverte");
            Assert.That(((ActivityResult)_resolve.Calls.Single()).Success, Is.True);
            Assert.That(_window.Request!.Driver, Is.SameAs(_driver));
            Assert.That(_driver!.Disposed, Is.GreaterThan(0), "le pilote est libéré après la partie");
        }

        [Test, Description("Vérifie qu'une partie perdue résout la case sans gain de réussite et n'ouvre la fenêtre qu'une fois — parce qu'un échec ne doit jamais boucler")]
        public void LostGame_ResolvesOnce_NoLoop()
        {
            _window.Result = ActivityResult.Lost(1);
            Run(new FakePlanActivity(With(StubDef()), "stub2d"));
            Assert.That(_window.Opened, Is.EqualTo(1));
            Assert.That(((ActivityResult)_resolve.Calls.Single()).Success, Is.False);
        }

        [Test, Description("Vérifie qu'un type d'activité sans pilote garde la fenêtre générique — parce que les activités à venir fonctionnent avant d'avoir leur jeu")]
        public void NoDriver_FallsBackToTheGenericWindow()
        {
            MiniGameDrivers.Unregister(StubKind);
            Run(new FakePlanActivity(With(StubDef()), "stub2d"));
            Assert.That(_window.Opened, Is.EqualTo(0));
            Assert.That(_generic.Data, Is.Not.Null);
        }

        [Test, Description("Vérifie que la demande porte le titre en majuscules, les consignes de l'activité à défaut de texte propre, et les textes de début, d'abandon et de fin — parce que la fenêtre ne contient aucun texte en dur")]
        public void Request_CarriesLocalizedTexts()
        {
            _window.Result = ActivityResult.Won();
            Run(new FakePlanActivity(With(StubDef()), "stub2d"));
            var r = _window.Request!;
            Assert.That(r.Title, Is.EqualTo("ARÈNE DE COMBAT"));
            Assert.That(r.Instructions, Is.EqualTo(_loc.T("activities.arena.desc")), "sans clé mg2d.<type>.instructions, la description de l'activité");
            Assert.That(r.Start, Is.EqualTo("TOUCHE POUR COMMENCER"));
            Assert.That(r.Abandon, Is.EqualTo("ABANDONNER"));
            Assert.That(r.WonText, Does.Contain("RÉUSSI"));
            Assert.That(r.LostText, Does.Contain("ÉCHEC"));
        }
    }
}
