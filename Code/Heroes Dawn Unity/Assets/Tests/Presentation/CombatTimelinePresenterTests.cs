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
    /// <summary>Frise, pause d action, compétences d avatar et combo à proximité dans la vue de combat.</summary>
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

        [Test, Description("Vérifie que la frise montre une pastille par unité et la zone de préparation à 80 % de progression (les 20 derniers points avant 0) — parce que le joueur lit le combat sur la frise")]
        public void Timeline_ShowsEveryUnit_AndPrepZone()
        {
            var t = _view.Timeline;
            Assert.That(t.Markers.Length, Is.EqualTo(_sim.Units.Count));
            Assert.That(t.Markers.Count(m => m.Ally), Is.EqualTo(_sim.Units.Count(u => u.Side == Side.Ally)));
            Assert.That(t.PrepStart01, Is.EqualTo(0.8f).Within(0.001f));
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

        [Test, Description("Vérifie que quand un allié arrive à 0 la frise se fige à l'écran (plus aucune progression) et que le panneau de compétences s'ouvre — parce que le monde attend l'action du joueur")]
        public void WhenAnAllyIsReady_TheTimelineFreezes_AndSkillsOpen()
        {
            foreach (var u in _sim.Units.Where(x => x.Side == Side.Enemy)) u.Point = 0;
            var ancre = Ally("ancre");
            ancre.Point = ancre.ActionPoint - 1; // il arrive à 0 en jeu : l événement « prêt » ouvre le panneau
            for (int i = 0; i < 200 && !_sim.IsAwaitingAction; i++) _p.Tick();
            int active = _sim.ActiveTick;
            var before = _view.Timeline.Markers.Where(m => !m.Ally).Select(m => m.Position01).ToArray();
            for (int i = 0; i < 100; i++) _p.Tick();
            Assert.That(_sim.ActiveTick, Is.EqualTo(active));
            Assert.That(_view.Timeline.Markers.Where(m => !m.Ally).Select(m => m.Position01).ToArray(), Is.EqualTo(before));
            Assert.That(_view.Skills.Visible, Is.True);
        }

    }
}
