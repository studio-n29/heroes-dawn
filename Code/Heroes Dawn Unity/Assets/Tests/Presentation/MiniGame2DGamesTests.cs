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

namespace N29.HeroesDawn.Tests.Presentation
{
    /// <summary>Chaque mini-jeu 2D du lot H, joué via la case Mini-jeu : la fenêtre commune s'ouvre avec le bon pilote, la bonne partie et les bons textes.</summary>
    public sealed class MiniGame2DGamesTests
    {
        TestApp _app = null!;
        FakeModalHost _host = null!;
        FakeToasts _toasts = null!;
        FakeCaseResolutions _resolve = null!;
        FakeMiniGame2DPopup _window = null!;
        readonly LocLocalization _loc = new LocLocalization();

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _host = new FakeModalHost();
            _window = new FakeMiniGame2DPopup();
            _host.Register<IMiniGame2DPopupView>(() => _window);
            _host.Register<IActivityPopupView>(() => new FakeActivityPopup());
            _toasts = new FakeToasts();
            _resolve = new FakeCaseResolutions();
        }

        MiniGame2DRequest Play(string activityId)
        {
            var plan = new FakePlanActivity(_app.Session.Content, activityId);
            new CasePopupPresenter(new ModalService(_host), _resolve, _resolve, plan, _resolve, _app.Session, _toasts, _loc)
                .RunAsync(new CaseOutcome(new CaseId("n07"), CaseType.MiniGame, CombatRank.Easy, null), CancellationToken.None).GetAwaiter().GetResult();
            Assert.That(_window.Opened, Is.EqualTo(1), activityId + " ouvre la fenêtre commune");
            return _window.Request!;
        }

        [Test, Description("Vérifie que « Le rythme » ouvre la fenêtre commune avec une partie de rythme, son titre « LE RYTHME », sa consigne propre et un bandeau « Notes x / y » — parce que c'est le premier jeu de la coque")]
        public void Rhythm_OpensTheCommonWindow()
        {
            var r = Play("rhythm_rooftop");
            Assert.That(r.Driver.Game, Is.InstanceOf<RhythmGame>());
            Assert.That(r.Title, Is.EqualTo("LE RYTHME"));
            Assert.That(r.Instructions, Is.EqualTo(_loc.T("mg2d.rhythm.instructions")));
            Assert.That(r.Driver.Hud.Score, Does.StartWith("Notes 0 / 24"));
            Assert.That(r.Driver.Hud.TimeLeft, Is.LessThan(0f));
            Assert.That(r.Driver.Hud.Lives, Is.EqualTo(5));
        }

        [Test, Description("Vérifie que « Tir franc » ouvre la fenêtre commune avec une partie de tir, son titre, sa consigne et un bandeau « Touches x / y » avec les lancers restants — parce que c'est un jeu à maintenir et relâcher")]
        public void FreeThrow_OpensTheCommonWindow()
        {
            var r = Play("free_throw_court");
            Assert.That(r.Driver.Game, Is.InstanceOf<FreeThrowGame>());
            Assert.That(r.Title, Is.EqualTo("TIR FRANC"));
            Assert.That(r.Instructions, Is.EqualTo(_loc.T("mg2d.free_throw.instructions")));
            Assert.That(r.Driver.Hud.Score, Is.EqualTo("Touches 0 / 3"));
            Assert.That(r.Driver.Hud.Lives, Is.EqualTo(5));
        }

        [Test, Description("Vérifie que « Caisse au choix » ouvre la fenêtre commune avec une partie de caisses, son titre, sa consigne et un bandeau « Caisses x / y » avec les vies — parce que c'est le jeu du curseur ping-pong")]
        public void CratePick_OpensTheCommonWindow()
        {
            var r = Play("crate_pick_depot");
            Assert.That(r.Driver.Game, Is.InstanceOf<CratePickGame>());
            Assert.That(r.Title, Is.EqualTo("CAISSE AU CHOIX"));
            Assert.That(r.Instructions, Is.EqualTo(_loc.T("mg2d.crate_pick.instructions")));
            Assert.That(r.Driver.Hud.Score, Is.EqualTo("Caisses 0 / 3"));
            Assert.That(r.Driver.Hud.Lives, Is.EqualTo(3));
        }

        [Test, Description("Vérifie qu'« Analyse ADN » ouvre la fenêtre commune avec une partie de brin, son titre, sa consigne, un bandeau « Parasites x / y », un temps et des vies — parce que c'est le jeu des nœuds 2D")]
        public void Dna_OpensTheCommonWindow()
        {
            var r = Play("dna_lab");
            Assert.That(r.Driver.Game, Is.InstanceOf<DnaAnalysisGame>());
            Assert.That(r.Title, Is.EqualTo("ANALYSE ADN"));
            Assert.That(r.Instructions, Is.EqualTo(_loc.T("mg2d.dna_analysis.instructions")));
            Assert.That(r.Driver.Hud.Score, Is.EqualTo("Parasites 0 / 6"));
            Assert.That(r.Driver.Hud.TimeLeft, Is.EqualTo(45f));
            Assert.That(r.Driver.Hud.Lives, Is.EqualTo(3));
        }

        [Test, Description("Vérifie qu'« Engrenage » ouvre la fenêtre commune avec une partie d'engrenages, son titre, sa consigne et un bandeau qui donne les engrenages entraînés et les coups restants, sans vies ni minuteur — parce que seuls les coups comptent")]
        public void Gears_OpensTheCommonWindow()
        {
            var r = Play("gears_workshop");
            Assert.That(r.Driver.Game, Is.InstanceOf<GearsGame>());
            Assert.That(r.Title, Is.EqualTo("ENGRENAGE"));
            Assert.That(r.Instructions, Is.EqualTo(_loc.T("mg2d.gears.instructions")));
            Assert.That(r.Driver.Hud.Score, Does.Contain("/ 6").And.Contain("Coups"));
            Assert.That(r.Driver.Hud.Lives, Is.LessThan(0));
            Assert.That(r.Driver.Hud.TimeLeft, Is.LessThan(0f));
        }

        [Test, Description("Vérifie qu'« Ingénierie de secours » ouvre la fenêtre commune avec une partie d'assemblage, son titre, sa consigne, un bandeau « Pièces x / y » et les essais en vies — parce que c'est le jeu du manuel")]
        public void Rescue_OpensTheCommonWindow()
        {
            var r = Play("rescue_tool");
            Assert.That(r.Driver.Game, Is.InstanceOf<RescueEngineeringGame>());
            Assert.That(r.Title, Is.EqualTo("INGÉNIERIE DE SECOURS"));
            Assert.That(r.Instructions, Is.EqualTo(_loc.T("mg2d.rescue_engineering.instructions")));
            Assert.That(r.Driver.Hud.Score, Is.EqualTo("Pièces 0 / 5"));
            Assert.That(r.Driver.Hud.Lives, Is.EqualTo(3));
        }

        [Test, Description("Vérifie que les 14 pièces de la réserve ont un nom français — parce qu'un identifiant brut s'afficherait sinon sur un bouton")]
        public void Rescue_EveryPartHasAFrenchName()
        {
            foreach (var part in RescueEngineeringGame.Vocabulary)
                Assert.That(_loc.Has("mg2d.rescue.part." + part), Is.True, part);
        }

        [Test, Description("Vérifie qu'« Infiltration » ouvre la fenêtre commune avec une partie de déplacement au joystick, son titre, sa consigne, un bandeau d'alerte, un temps et des vies — parce que c'est le jeu du joystick virtuel")]
        public void Infiltration_OpensTheCommonWindow_WithTheStick()
        {
            var r = Play("infiltration_camp");
            Assert.That(r.Driver.Game, Is.InstanceOf<InfiltrationGame>());
            Assert.That(r.Driver.UsesStick, Is.True);
            Assert.That(r.Title, Is.EqualTo("INFILTRATION"));
            Assert.That(r.Instructions, Is.EqualTo(_loc.T("mg2d.infiltration.instructions")));
            Assert.That(r.Driver.Hud.Score, Is.EqualTo("Alerte 0 %"));
            Assert.That(r.Driver.Hud.TimeLeft, Is.EqualTo(60f));
            Assert.That(r.Driver.Hud.Lives, Is.EqualTo(3));
        }

        [Test, Description("Vérifie que la survie en neige ouvre la fenêtre commune 2D (et plus la fenêtre 3D) avec le joystick, la jauge de survie en pourcentage et le temps — parce que le PDF la demande en 2D vue du dessus")]
        public void Survival_OpensTheCommonWindow_WithTheStick()
        {
            var r = Play("survival_snow");
            Assert.That(r.Driver.Game, Is.InstanceOf<SnowSurvivalMiniGame>());
            Assert.That(r.Driver.UsesStick, Is.True);
            Assert.That(r.Title, Is.EqualTo("SURVIE EN NEIGE"));
            Assert.That(r.Instructions, Is.EqualTo(_loc.T("mg2d.snow_survival.instructions")));
            Assert.That(r.Driver.Hud.Score, Is.EqualTo("Survie 100 %"));
            Assert.That(r.Driver.Hud.TimeLeft, Is.EqualTo(90f));
        }
    }
}
