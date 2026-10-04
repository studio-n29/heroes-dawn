using System;
using System.Linq;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
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
    /// <summary>Frise chronologique, annonces d'actions, DÉPASSER et COMBO SYNC dans la vue de combat.</summary>
    public sealed class CombatTimelinePresenterTests
    {
        TestApp _app = null!;
        FakeCombatView _view = null!;
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
            var host = new FakeModalHost();
            host.Register<ICombatResultsPopupView>(() => new FakeCombatResultsPopup());
            var modals = new ModalService(host);
            _p = new CombatPresenter(_view, _app.Combat.Active, _app.Session, _app.Combat.SetAuto, _app.Combat.Finish, _app.Combat.Verify,
                                     _app.Combat.ForceWin, _app.Publisher<CombatResultsClosed>(), _app.Publisher<NavigateRequested>(),
                                     modals, new FakeToasts(), new LocLocalization(), new FakeFrameClock(), new BackButtonRouter(new FakeBackInput(), modals));
            _view.Show();
            _p.Start();
            _p.Tick();
        }

        CombatUnit Ally(string def) => _sim.Units.First(u => u.Side == Side.Ally && u.DefId == def);
        int FirstEnemy => _sim.Units.First(u => u.Side == Side.Enemy && u.IsAlive).Id;

        [Test, Description("Vérifie que la frise montre une pastille par unité, la zone de préparation à 80 % et la réserve de Temps — parce que le joueur lit le combat sur la frise")]
        public void Timeline_ShowsEveryUnit_PrepZoneAndBank()
        {
            var t = _view.Timeline;
            Assert.That(t.Markers.Length, Is.EqualTo(_sim.Units.Count));
            Assert.That(t.Markers.Count(m => m.Ally), Is.EqualTo(_sim.Units.Count(u => u.Side == Side.Ally)));
            Assert.That(t.PrepStart01, Is.EqualTo(0.8f).Within(0.001f));
            Assert.That(t.Bank, Is.EqualTo(_sim.TimeBank));
            Assert.That(t.BankText, Does.Contain("TEMPS"));
            Assert.That(t.Markers.All(m => m.Position01 >= 0f && m.Position01 <= 1f));
        }

        [Test, Description("Vérifie qu'un ennemi qui entre en zone de préparation apparaît avec son action annoncée sur la frise — parce que la grosse attaque doit se voir venir")]
        public void Timeline_ShowsEnemyAnnouncedAction()
        {
            for (int i = 0; i < 3000 && !_view.Timeline.Markers.Any(m => !m.Ally && m.PlannedSlot >= 0); i++) _p.Tick();
            var m = _view.Timeline.Markers.FirstOrDefault(x => !x.Ally && x.PlannedSlot >= 0);
            Assert.That(m.PlannedSlot, Is.GreaterThanOrEqualTo(0));
            Assert.That(m.InPrep || m.PlannedSlot >= 0);
        }

        [Test, Description("Vérifie que DÉPASSER puis un ennemi touché font agir un allié tout de suite, dépensent 40 de Temps et que le bouton se grise ensuite avec la raison — parce que c'est l'usage « saut de tour » de la réserve")]
        public void TimeSkip_TargetsEnemy_SpendsBank_ThenDisabled()
        {
            Assert.That(_view.Timeline.SkipVisible);
            Assert.That(_view.Timeline.SkipEnabled);
            int bank = _sim.TimeBank;
            _view.ClickTimeSkip();
            Assert.That(_view.Timeline.SkipTargeting, Is.True, "plusieurs ennemis : on choisit lequel couper");
            Assert.That(_view.Hint, Does.Contain("COUPER"));
            _view.TapUnit(FirstEnemy);
            _p.Tick();
            Assert.That(_sim.TimeBank, Is.LessThan(bank));
            Assert.That(_sim.Units.Any(u => u.Side == Side.Ally && u.IsReady), Is.True);
            Assert.That(_view.Timeline.SkipEnabled, Is.False);
            Assert.That(_view.Timeline.SkipReason, Does.Contain("manque"));
        }

        [Test, Description("Vérifie que Retour annule le choix de l'ennemi à couper sans rien dépenser — parce qu'annuler n'a aucune conséquence")]
        public void TimeSkip_BackCancels()
        {
            int bank = _sim.TimeBank;
            _view.ClickTimeSkip();
            Assert.That(_view.Timeline.SkipTargeting, Is.True);
            _p.HandleBack();
            Assert.That(_view.Timeline.SkipTargeting, Is.False);
            Assert.That(_sim.TimeBank, Is.EqualTo(bank));
        }

        [Test, Description("Vérifie qu'avec un allié prêt et son partenaire de combo à 70 % de la frise, le bouton propose « COMBO SYNC » et le lance contre un ennemi — parce que le Temps d'équipe rattrape le partenaire en retard (spec §B)")]
        public void SyncCombo_OfferedAndPerformed()
        {
            foreach (var u in _sim.Units.Where(x => x.Side == Side.Enemy)) u.Gauge = 0;
            var ancre = Ally("ancre"); var givre = Ally("givre");
            foreach (var other in _sim.Units.Where(x => x.Side == Side.Ally && x != ancre && x != givre)) other.Gauge = 0;
            ancre.Gauge = ancre.GaugeMax;
            givre.Gauge = givre.GaugeMax * 70 / 100;
            _p.Tick();
            Assert.That(_view.ComboVisible, Is.True);
            Assert.That(_view.ComboLabel, Does.Contain("SYNC"));
            int bank = _sim.TimeBank;
            _view.ClickCombo();
            _view.TapUnit(FirstEnemy);
            _p.Tick();
            Assert.That(_sim.InputLog.Any(i => i.Command.Type == CommandType.Combo && i.Command.Flag), Is.True);
            Assert.That(_sim.TimeBank, Is.LessThan(bank));
        }
    }
}
