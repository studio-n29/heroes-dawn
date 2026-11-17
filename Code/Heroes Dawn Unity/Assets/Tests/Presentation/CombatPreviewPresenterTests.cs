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
    /// <summary>Aperçu de la carte tenue : armée (glisser commencé) ou posée sur une cible (glisser au-dessus d'une unité), sans jamais toucher au combat réel.</summary>
    public sealed class CombatPreviewPresenterTests
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
        }

        void TickUntil(Func<bool> condition, int max = 3000)
        {
            for (int i = 0; i < max && !condition(); i++) _p.Tick();
        }

        /// <summary>Attend un allié prêt et renvoie (allié, emplacement) du premier emplacement utilisable dont la règle de cible est donnée.</summary>
        (int unit, int slot) ReadyWith(Func<SkillDef, bool> match)
        {
            TickUntil(() => _view.Skills.Visible);
            Assume.That(_view.Skills.Visible, "aucun allié prêt");
            int unit = _view.States.Single(s => s.Selected).Id;
            var u = _sim.Unit(unit);
            for (int i = 0; i < CombatSimulation.ActiveSkillCount(u); i++)
                if (_sim.CanUse(unit, i) && match(u.Skills[i])) return (unit, i);
            Assume.That(false, "aucune compétence de ce type prête");
            return (-1, -1);
        }

        static string Fingerprint(CombatSimulation sim)
            => sim.Tick + "|" + sim.InputLog.Count + "|" + string.Join(";", sim.Units.Select(u => u.Id + ":" + u.Hp + "," + u.Point + "," + u.Energy + "," + u.Statuses.Count));

        [Test, Description("Vérifie qu'une compétence mono-cible glissée au-dessus d'un ennemi affiche l'aperçu de cet ennemi, et que l'effacer au lâcher n'envoie aucun ordre — parce que l'aperçu suit la cible survolée et ne joue jamais la carte")]
        public void DragOverEnemy_ShowsPreviewOfThatEnemy()
        {
            var (_, slot) = ReadyWith(s => s.Target == TargetRule.SingleEnemy && s.Pick == TargetPick.Default);
            var enemies = _sim.Units.Where(u => u.Side == Side.Enemy && u.IsAlive).Select(u => u.Id).ToList();
            Assume.That(enemies.Count, Is.GreaterThan(1), "cible évidente avec un seul ennemi");
            string before = Fingerprint(_sim);
            _view.BeginDragSkill(slot);
            Assert.That(_view.Preview, Is.Null, "pas de cible survolée : pas d'aperçu");
            _view.DragOver(slot, enemies[1]);
            Assert.That(_view.Preview, Is.Not.Null);
            Assert.That(_view.Preview!.Units.Any(u => u.Id == enemies[1]), Is.True, "l'ennemi survolé change");
            Assert.That(Fingerprint(_sim), Is.EqualTo(before), "le combat réel n'a pas bougé");
            _view.DragOver(slot, ICombatView.DropTerrain);
            Assert.That(_view.Preview, Is.Null, "plus de cible : l'aperçu s'efface");
            _view.DropSkill(slot, ICombatView.DropCancel);
            Assert.That(_view.Preview, Is.Null);
            Assert.That(Fingerprint(_sim), Is.EqualTo(before));
        }

        [Test, Description("Vérifie qu'une compétence qui ne demande pas de cible (zone, soi, ciblage fin) est prévisualisée dès qu'on la prend — parce que la carte armée montre déjà son effet")]
        public void ArmedCardWithoutTarget_ShowsPreviewImmediately()
        {
            var (_, slot) = ReadyWith(s => s.Target == TargetRule.AllEnemies || s.Target == TargetRule.Self || s.Target == TargetRule.AllAllies);
            _view.BeginDragSkill(slot);
            Assert.That(_view.Preview, Is.Not.Null);
            _view.DropSkill(slot, ICombatView.DropCancel);
            Assert.That(_view.Preview, Is.Null, "lâchée sur l'éventail : annulée, aperçu effacé");
        }

        [Test, Description("Vérifie qu'une carte posée sur sa cible efface l'aperçu et joue la carte — parce que l'aperçu précède l'ordre sans le remplacer")]
        public void Drop_OnTarget_ClearsPreviewAndPlays()
        {
            var (_, slot) = ReadyWith(s => s.Target == TargetRule.SingleEnemy && s.Pick == TargetPick.Default);
            int enemy = _sim.Units.First(u => u.Side == Side.Enemy && u.IsAlive).Id;
            _view.BeginDragSkill(slot);
            _view.DragOver(slot, enemy);
            _view.DropSkill(slot, enemy);
            Assert.That(_view.Preview, Is.Null);
            Assert.That(_view.LastPlayedCard.HasValue, Is.True, "la carte est jouée");
        }
    }
}
