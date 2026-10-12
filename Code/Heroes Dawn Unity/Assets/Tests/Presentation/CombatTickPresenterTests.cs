using System.Linq;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Domain.Combat;
using N29.HeroesDawn.Presentation.Combat;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Tests.Domain.Application;
using N29.HeroesDawn.Tests.Presentation.Fakes;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    /// <summary>Rythme de la frise par points côté présentation : un point toutes les 0,5 s, pause de 1,5 s, délai de l'IA, 30 s au joueur (cahier des charges 2026-10-11).</summary>
    public sealed class CombatTickPresenterTests
    {
        TestApp _app = null!;
        FakeCombatView _view = null!;
        FakeFrameClock _clock = null!;
        CombatPresenter _p = null!;
        CombatSimulation _sim = null!;

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.Account.SignIn.ExecuteAsync().GetAwaiter().GetResult();
            var zone = _app.Session.Content.Zone(PrototypeContent.Zone11);
            _app.Session.Run = ZoneRun.ForTests(_app.Session.Content, zone, 3UL, 1, true, "n02");
            _app.Session.RunZone = zone;
            _app.Zone.Move.ExecuteAsync(new CaseId("n03")).GetAwaiter().GetResult();
            _sim = _app.Combat.Begin.ExecuteAsync().GetAwaiter().GetResult().Value;
            _view = new FakeCombatView();
            _clock = new FakeFrameClock { Dt = 0.1f };
            var host = new FakeModalHost();
            host.Register<ICombatResultsPopupView>(() => new FakeCombatResultsPopup());
            var modals = new ModalService(host);
            _p = new CombatPresenter(_view, _app.Combat.Active, _app.Session, _app.Combat.SetAuto, _app.Combat.Finish, _app.Combat.Verify,
                                     _app.Combat.ForceWin, _app.Publisher<CombatResultsClosed>(), _app.Publisher<NavigateRequested>(),
                                     modals, new FakeToasts(), new LocLocalization(), _clock, new BackButtonRouter(new FakeBackInput(), modals));
            _view.Show();
            _p.Start();
            _view.ClickSpeed(); _view.ClickSpeed(); // ×2 par défaut → ×3 → ×1 : les tests comptent en images de 0,1 s à ×1
        }

        void Calm()
        {
            foreach (var u in _sim.Units) u.Point = 0;
        }

        [Test, Description("Vérifie que la frise ne fait qu'un pas toutes les 0,35 s de temps réel à ×1 (quatre images de 0,1 s) — parce que le déplacement se fait point par point avec une transition de 0,35 s")]
        public void Timeline_StepsEveryStepDuration()
        {
            Calm();
            int t0 = _sim.Tick;
            for (int i = 0; i < 3; i++) _p.Tick();
            Assert.That(_sim.Tick, Is.EqualTo(t0), "0,3 s : pas encore de pas");
            _p.Tick();
            Assert.That(_sim.Tick, Is.EqualTo(t0 + 1), "0,4 s : un pas");
        }

        [Test, Description("Vérifie que la pause de 1,2 s n'a lieu que lorsqu'un ennemi va arriver au point d'action, que le joueur peut interrompre pendant ce temps et que la frise reprend ensuite — parce que les points vides sans menace ne doivent plus ralentir le combat")]
        public void ImminentEnemy_PausesTheTimelineFor1200ms()
        {
            Calm();
            var enemy = _sim.Units.First(u => u.Side == Side.Enemy);
            enemy.Point = enemy.ActionPoint - 2;
            for (int i = 0; i < 4; i++) _p.Tick();
            int tick = _sim.Tick;
            Assert.That(_sim.InterruptWindowOpen, Is.True);
            Assert.That(_p.PauseLeft, Is.EqualTo(1.2f).Within(0.01f));
            for (int i = 0; i < 11; i++) _p.Tick();
            Assert.That(_sim.Tick, Is.EqualTo(tick), "1,1 s : toujours en pause");
            for (int i = 0; i < 6; i++) _p.Tick();
            Assert.That(_sim.Tick, Is.GreaterThan(tick), "la frise a repris");
        }

        [Test, Description("Vérifie qu'aucune pause n'est ouverte quand aucun ennemi n'est sur le point d'agir — parce que c'était la cause d'un combat beaucoup trop long")]
        public void NoThreat_NeverPauses()
        {
            Calm();
            for (int i = 0; i < 20; i++) _p.Tick();
            Assert.That(_p.PauseLeft, Is.EqualTo(0f));
            Assert.That(_sim.InterruptWindowOpen, Is.False);
        }

        [Test, Description("Vérifie que la pause d'interruption passe deux fois plus vite à ×2 — parce que la vitesse du combat s'applique aux pauses comme aux pas")]
        public void Pause_ScalesWithCombatSpeed()
        {
            _view.ClickSpeed(); // ×1 → ×2
            Calm();
            var enemy = _sim.Units.First(u => u.Side == Side.Enemy);
            enemy.Point = enemy.ActionPoint - 2;
            for (int i = 0; i < 2; i++) _p.Tick();
            Assert.That(_p.PauseLeft, Is.EqualTo(1.2f).Within(0.01f));
            for (int i = 0; i < 3; i++) _p.Tick();
            Assert.That(_p.PauseLeft, Is.EqualTo(0.6f).Within(0.02f), "0,3 s réelles à ×2 = 0,6 s de pause écoulées");
            for (int i = 0; i < 3; i++) _p.Tick();
            Assert.That(_p.PauseLeft, Is.LessThanOrEqualTo(0.02f), "0,6 s réelles à ×2 = 1,2 s de pause");
        }

        [Test, Description("Vérifie que le tour d'un agent laisse 30 s au joueur, que le décompte baisse en temps réel et que passé ce délai l'unité passe son tour et repart au point 3 — parce que la frise ne doit jamais rester figée")]
        public void PlayerTurn_HasThirtySeconds_ThenPasses()
        {
            Calm();
            var ally = _sim.Units.First(u => u.Side == Side.Ally);
            ally.Point = ally.ActionPoint;
            _p.Tick();
            Assert.That(_sim.IsAwaitingAction, Is.True);
            Assert.That(_p.TurnLeft, Is.EqualTo(29.9f).Within(0.2f));
            _clock.Dt = 31f;
            _p.Tick();
            Assert.That(ally.Point, Is.EqualTo(3));
            Assert.That(_view.Log.Any(l => l.Contains("passe son tour")), Is.True);
        }

        [Test, Description("Vérifie qu'après l'action d'un ennemi l'IA marque un délai de réflexion entre 0,5 et 1,5 s avant que la frise reprenne — parce que l'IA doit paraître choisir")]
        public void EnemyAction_IsFollowedByAThinkingDelay()
        {
            Calm();
            var enemy = _sim.Units.First(u => u.Side == Side.Enemy);
            enemy.Point = enemy.ActionPoint - 1;
            for (int i = 0; i < 5; i++) _p.Tick();
            Assert.That(_p.PauseLeft, Is.InRange(0.15f, 0.71f), "délai aléatoire borné par l'équilibrage (0,3 à 0,7 s)");
        }

        [Test, Description("Vérifie qu'un ennemi qui agit en mode Auto ne déclenche ni fenêtre d'interruption ni délai — parce que l'Auto doit enchaîner")]
        public void AutoMode_NeverPauses()
        {
            _view.ClickAuto();
            _p.Tick();
            Calm();
            for (int i = 0; i < 40; i++) _p.Tick();
            Assert.That(_p.PauseLeft, Is.EqualTo(0f));
        }
    }
}
