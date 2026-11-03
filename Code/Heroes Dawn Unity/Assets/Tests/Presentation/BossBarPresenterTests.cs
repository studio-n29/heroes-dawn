using System.Linq;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Domain.Combat;
using N29.HeroesDawn.Presentation.Combat;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Tests.Domain.Application;
using N29.HeroesDawn.Tests.Presentation.Fakes;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    /// <summary>Combat de boss : barre de vie élargie, phase affichée, télégraphie du changement de phase (lot L8 de la 0.15.0).</summary>
    public sealed class BossBarPresenterTests
    {
        TestApp _app = null!;
        FakeCombatView _view = null!;
        CombatSimulation _sim = null!;
        CombatUnit _boss = null!;
        CombatPresenter _p = null!;

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.Account.SignIn.ExecuteAsync().GetAwaiter().GetResult();
            var zone = _app.Session.Content.Zone(PrototypeContent.Zone11);
            _app.Session.Run = ZoneRun.ForTests(_app.Session.Content, zone, 3UL, 3, true, "n22");
            _app.Session.RunZone = zone;
            _app.Zone.Move.ExecuteAsync(new CaseId("n23")).GetAwaiter().GetResult();
            _sim = _app.Combat.Begin.ExecuteAsync().GetAwaiter().GetResult().Value;
            _boss = _sim.Units.First(u => u.Side == Side.Enemy && u.IsBoss);
            var modals = new ModalService(new FakeModalHost());
            _view = new FakeCombatView();
            _p = new CombatPresenter(_view, _app.Combat.Active, _app.Session, _app.Combat.SetAuto, _app.Combat.Finish, _app.Combat.Verify,
                                     _app.Combat.ForceWin, _app.Publisher<CombatResultsClosed>(), _app.Publisher<NavigateRequested>(),
                                     modals, new FakeToasts(), new LocLocalization(), new FakeFrameClock(), new BackButtonRouter(new FakeBackInput(), modals));
            _view.Show();
            _p.Start();
        }

        [TearDown]
        public void TearDown() => _p.Dispose();

        [Test, Description("Vérifie que le combat du boss de la zone affiche sa barre élargie avec son nom, sa phase 1 sur 3, ses PV pleins et les deux repères de seuil — parce que le joueur doit lire où en est le boss")]
        public void BossBar_ShowsNamePhaseHpAndThresholds()
        {
            Assert.That(_view.Boss.HasValue, Is.True, "barre affichée");
            var bar = _view.Boss!.Value;
            Assert.That(bar.Name, Is.EqualTo("BRICK"));
            Assert.That(bar.Phase, Is.EqualTo(new LocLocalization().F("combat.boss.phase", 1, 3)));
            Assert.That(bar.Hp01, Is.EqualTo(1f).Within(0.001f));
            Assert.That(bar.HpText, Is.EqualTo(_boss.Hp + " / " + _boss.MaxHp));
            Assert.That(bar.Marks, Is.EqualTo(new[] { 0.66f, 0.33f }));
        }

        [Test, Description("Vérifie qu'un coup qui fait passer le boss sous 66 % annonce la phase 2 (bandeau, nom de la phase, éclat sur la barre) et gèle la frise le temps de lire — parce que le changement de phase doit se voir venir")]
        public void PhaseChange_IsTelegraphed_AndPausesTheTimeline()
        {
            _boss.Hp = _boss.MaxHp * 66 / 100 + 1;
            _view.ClickAuto();
            for (int i = 0; i < 6000 && _boss.PhaseReached == 0 && _boss.IsAlive; i++) _p.Tick();
            Assume.That(_boss.PhaseReached, Is.GreaterThan(0), "un allié a frappé le boss");
            var loc = new LocLocalization();
            Assert.That(_view.Banners.Any(b => b.Text == loc.F("combat.boss.phase_banner", 2) && b.Tone == Tone.Danger), Is.True, "bandeau PHASE 2");
            Assert.That(_p.PauseLeft, Is.GreaterThan(1f), "la frise attend");
            _p.Tick();
            Assert.That(_view.BossPhaseFlashes, Is.EqualTo(1), "un seul éclat");
            Assert.That(_view.Boss!.Value.Phase, Does.StartWith(loc.F("combat.boss.phase", 2, 3)));
            Assert.That(_view.Boss!.Value.Phase, Does.Contain(loc.T("enemies.brick.phase2").ToUpperInvariant()), "nom de la phase");
            Assert.That(_view.Log.Any(l => l.Contains(loc.T("enemies.brick.phase2"))), Is.True, "ligne du journal");
        }
    }
}
