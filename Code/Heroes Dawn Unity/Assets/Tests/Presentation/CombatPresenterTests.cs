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
    public sealed class CombatPresenterTests
    {
        TestApp _app = null!;
        FakeCombatView _view = null!;
        FakeModalHost _host = null!;
        ModalService _modals = null!;
        BackButtonRouter _router = null!;
        FakeCombatResultsPopup _results = null!;
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
            _host = new FakeModalHost();
            _results = new FakeCombatResultsPopup();
            _host.Register<ICombatResultsPopupView>(() => _results);
            _modals = new ModalService(_host);
            _router = new BackButtonRouter(new FakeBackInput(), _modals);
            _p = new CombatPresenter(_view, _app.Combat.Active, _app.Session, _app.Combat.SetAuto, _app.Combat.Finish, _app.Combat.Verify,
                                     _app.Combat.ForceWin, _app.Publisher<CombatResultsClosed>(), _app.Publisher<NavigateRequested>(),
                                     _modals, new FakeToasts(), new LocLocalization(), new FakeFrameClock(), _router);
            _view.Show();
            _p.Start();
        }

        void TickUntil(Func<bool> condition, int max = 3000)
        {
            for (int i = 0; i < max && !condition(); i++) _p.Tick();
        }

        int AliveEnemies => _sim.Units.Count(u => u.Side == Side.Enemy && u.IsAlive);

        /// <summary>Joue jusqu'à ce qu'un allié prêt ait une compétence mono-cible utilisable ; renvoie son emplacement.</summary>
        int ReadyWithSingleTargetSkill()
        {
            TickUntil(() => _view.Skills.Visible);
            Assume.That(_view.Skills.Visible, "aucun allié prêt");
            var selected = _view.States.Single(s => s.Selected).Id;
            var u = _sim.Unit(selected);
            for (int i = 0; i < CombatSimulation.ActiveSkillCount(u); i++)
                if (u.Skills[i].Target == TargetRule.SingleEnemy && _sim.CanUse(selected, i)) return i;
            Assume.That(false, "pas de compétence mono-cible prête");
            return -1;
        }

        [Test, Description("Vérifie qu'une carte est construite par unité du combat — parce que chaque allié et chaque ennemi a son portrait, ses PV et sa jauge (W11a)")]
        public void Shown_BuildsOneCardPerUnit()
        {
            Assert.That(_view.Built.Count, Is.EqualTo(_sim.Units.Count));
            Assert.That(_view.Built.Count(b => b.Ally), Is.EqualTo(_sim.Units.Count(u => u.Side == Side.Ally)));
        }

        [Test, Description("Vérifie que la simulation avance avec le temps réel, deux fois plus vite en ×2 — parce que la vitesse accélère simulation et affichage ensemble (doc 07)")]
        public void Tick_AdvancesSimulationBySpeed()
        {
            _view.ClickSpeed(); _view.ClickSpeed(); // 2 → 3 → 1 : on mesure à partir de ×1
            _view.ClickAuto(); // en Auto, la frise ne s'arrête jamais : on mesure le seul rythme des pas
            _p.Tick();
            int t0 = _sim.Tick;
            for (int i = 0; i < 20; i++) _p.Tick();
            int normal = _sim.Tick - t0;
            _view.ClickSpeed();
            int t1 = _sim.Tick;
            for (int i = 0; i < 20; i++) _p.Tick();
            Assert.That(normal, Is.GreaterThan(0), "un pas toutes les 0,35 s");
            Assert.That(_sim.Tick - t1, Is.InRange(2 * normal - 1, 2 * normal + 1));
        }

        [Test, Description("Vérifie qu'un allié prêt est sélectionné automatiquement et ses compétences affichées — parce que le plus ancien prêt est choisi d'office (W11a)")]
        public void ReadyAlly_IsAutoSelectedOldestFirst()
        {
            TickUntil(() => _sim.Units.Any(u => u.Side == Side.Ally && u.IsReady));
            _p.Tick();
            var selected = _view.States.Where(s => s.Selected).ToList();
            Assert.That(selected.Count, Is.EqualTo(1));
            Assert.That(selected[0].Ally && _sim.Unit(selected[0].Id).IsReady, Is.True);
            Assert.That(_view.Skills.Visible, Is.True);
        }

        [Test, Description("Vérifie qu'une compétence mono-cible avec plusieurs ennemis attend le choix de la cible — parce que le joueur touche la cible (W11b)")]
        public void SingleTargetSkill_WithSeveralEnemies_WaitsForTarget()
        {
            int slot = ReadyWithSingleTargetSkill();
            Assume.That(AliveEnemies, Is.GreaterThan(1));
            _view.ClickSkill(slot);
            Assert.That(_view.Hint, Is.EqualTo(Loc.T("combat.choose_target")));
            Assert.That(_view.States.Where(s => !s.Ally && s.Alive).All(s => s.Targetable), Is.True);
        }

        int ReadyWithGeneralSkill()
        {
            TickUntil(() => _view.Skills.Visible);
            Assume.That(_view.Skills.Visible, "aucun allié prêt");
            var selected = _view.States.Single(s => s.Selected).Id;
            var u = _sim.Unit(selected);
            for (int i = 0; i < CombatSimulation.ActiveSkillCount(u); i++)
            {
                var t = u.Skills[i].Target;
                if (t != TargetRule.SingleEnemy && t != TargetRule.SingleAlly && _sim.CanUse(selected, i)) return i;
            }
            Assume.That(false, "pas de compétence générale prête");
            return -1;
        }

        [Test, Description("Vérifie qu'en prenant une carte ciblée les cibles valides s'allument puis que la déposer sur un ennemi lance la compétence — parce que les compétences ciblées se jouent au glisser-déposer sur la cible")]
        public void DragTargetedSkill_OnAnEnemy_UsesIt()
        {
            int slot = ReadyWithSingleTargetSkill();
            Assume.That(AliveEnemies, Is.GreaterThan(1));
            int actor = _view.States.Single(s => s.Selected).Id;
            Assert.That(_view.States.Single(s => s.Id == actor).Skills![slot].NeedsTarget, Is.True);
            _view.BeginDragSkill(slot);
            Assert.That(_view.States.Where(s => !s.Ally && s.Alive).All(s => s.Targetable), Is.True, "les cibles valides s'allument");
            int enemy = _sim.Units.First(u => u.Side == Side.Enemy && u.IsAlive).Id;
            _view.DropSkill(slot, enemy);
            _p.Tick();
            Assert.That(_sim.Unit(actor).IsReady, Is.False, "la compétence a été lancée sur la cible");
        }

        [Test, Description("Vérifie que lâcher une carte ciblée dans le vide ou la rendre à l'éventail ne lance rien et éteint les cibles — parce qu'un lâcher invalide annule sans effet")]
        public void DragTargetedSkill_DroppedElsewhere_Cancels()
        {
            int slot = ReadyWithSingleTargetSkill();
            Assume.That(AliveEnemies, Is.GreaterThan(1));
            int actor = _view.States.Single(s => s.Selected).Id;
            _view.BeginDragSkill(slot);
            _view.DropSkill(slot, ICombatView.DropTerrain);
            Assert.That(_sim.Unit(actor).IsReady, Is.True, "rien n'est lancé");
            Assert.That(_view.States.Where(s => !s.Ally).All(s => !s.Targetable), Is.True, "les cibles sont éteintes");
            _view.BeginDragSkill(slot);
            _view.DropSkill(slot, ICombatView.DropCancel);
            Assert.That(_sim.Unit(actor).IsReady, Is.True);
        }

        [Test, Description("Vérifie qu'une compétence générale se lance en la lâchant dans le vide — parce qu'elle n'a pas de cible à viser")]
        public void DragGeneralSkill_OnTerrain_UsesIt()
        {
            int slot = ReadyWithGeneralSkill();
            int actor = _view.States.Single(s => s.Selected).Id;
            Assert.That(_view.States.Single(s => s.Id == actor).Skills![slot].NeedsTarget, Is.False);
            _view.BeginDragSkill(slot);
            _view.DropSkill(slot, ICombatView.DropTerrain);
            _p.Tick();
            Assert.That(_sim.Unit(actor).IsReady, Is.False);
        }

        [Test, Description("Vérifie que lâcher une carte sans l'avoir prise n'a aucun effet — parce qu'un lâcher parasite ne doit rien lancer")]
        public void DropWithoutDrag_IsIgnored()
        {
            int slot = ReadyWithGeneralSkill();
            int actor = _view.States.Single(s => s.Selected).Id;
            _view.DropSkill(slot, ICombatView.DropTerrain);
            Assert.That(_sim.Unit(actor).IsReady, Is.True);
        }

        [Test, Description("Vérifie que Retour pendant le ciblage l'annule — parce qu'Échap annule le ciblage (W11b, spec §7)")]
        public void Back_DuringTargeting_CancelsTargeting()
        {
            int slot = ReadyWithSingleTargetSkill();
            Assume.That(AliveEnemies, Is.GreaterThan(1));
            _view.ClickSkill(slot);
            Assert.That(_router.Dispatch(), Is.True);
            Assert.That(_view.Hint, Is.Not.EqualTo(Loc.T("combat.choose_target")));
            Assert.That(_view.States.Any(s => s.Targetable), Is.False);
        }

        [Test, Description("Vérifie que Retour sans ciblage en cours n'est pas consommé par le combat — parce que la retraite reste un geste volontaire (W11a, spec §7)")]
        public void Back_NoTargeting_NotHandled()
        {
            Assert.That(_router.Dispatch(), Is.False);
            Assert.That(_host.Spawned, Is.Empty);
        }

        [Test, Description("Vérifie qu'un combo disponible engage les deux alliés de la paire — parce que le combo consomme le tour des deux (W11b)")]
        public void Combo_SubmitsBothActors()
        {
            TickUntil(() => _view.ComboVisible);
            Assume.That(_view.ComboVisible, "aucun combo disponible");
            var option = _sim.AvailableCombos().Concat(_sim.AvailableSyncCombos()).First();
            _view.ClickCombo();
            if (_view.Hint == Loc.T("combat.choose_target"))
                _view.TapUnit(_sim.Units.First(u => u.Side == Side.Enemy && u.IsAlive).Id);
            _p.Tick();
            Assert.That(_sim.Unit(option.A).IsReady, Is.False);
            Assert.That(_sim.Unit(option.B).IsReady, Is.False);
            Assert.That(_view.Log.Any(l => l.Contains(" + ")), Is.True);
        }

        [Test, Description("Vérifie que le bouton Auto passe par le cas d'usage et s'affiche activé aussitôt — parce que l'IA joue alors pour le joueur (W11b)")]
        public void Auto_TogglesThroughUseCase()
        {
            _view.ClickAuto();
            Assert.That(_view.TopBar.AutoOn, Is.True);
            _p.Tick();
            Assert.That(_sim.Auto, Is.True);
        }

        [Test, Description("Vérifie que la vitesse démarre à ×2 puis tourne entre ×3, ×1 et ×2 — parce que le combat est rapide par défaut (spec 0.8.0)")]
        public void Speed_Cycles1To3()
        {
            Assert.That(_view.TopBar.SpeedText, Is.EqualTo(Loc.F("combat.speed", 2)), "vitesse par défaut");
            _view.ClickSpeed();
            Assert.That(_view.TopBar.SpeedText, Is.EqualTo(Loc.F("combat.speed", 3)));
            _view.ClickSpeed();
            Assert.That(_view.TopBar.SpeedText, Is.EqualTo(Loc.F("combat.speed", 1)));
            _view.ClickSpeed();
            Assert.That(_view.TopBar.SpeedText, Is.EqualTo(Loc.F("combat.speed", 2)));
        }

        [Test, Description("Vérifie que Retraite demande une confirmation — parce qu'une retraite vaut défaite et recul d'une case (W11a)")]
        public void Retreat_AsksConfirmation()
        {
            _view.ClickRetreat();
            Assert.That(_host.Spawned.OfType<FakeConfirmPopup>().Count(), Is.EqualTo(1));
        }

        [Test, Description("Vérifie qu'une modale ouverte met le combat en pause — parce que le combat s'arrête derrière une modale (maquettes §3)")]
        public void ModalOpen_PausesTicks()
        {
            _modals.OpenAsync<FakePopup>(System.Threading.CancellationToken.None);
            int t0 = _sim.Tick;
            for (int i = 0; i < 5; i++) _p.Tick();
            Assert.That(_sim.Tick, Is.EqualTo(t0));
        }

        [Test, Description("Vérifie qu'un combat terminé affiche les résultats puis, sur Continuer, oublie le combat et publie CombatResultsClosed — parce que la machine d'états renvoie alors au plateau (doc 07)")]
        public void Ended_ShowsResultsThenContinuePublishesClosed()
        {
            _app.Combat.ForceWin.ExecuteAsync().GetAwaiter().GetResult();
            TickUntil(() => _app.Received<CombatResultsClosed>().Count > 0, 50);
            Assert.That(_results.Data!.Value.Victory, Is.True);
            Assert.That(_app.Received<CombatResultsClosed>().Count, Is.EqualTo(1));
            Assert.That(_app.Combat.Active.Simulation, Is.Null);
        }

        [Test, Description("Vérifie que Retour sur les résultats vaut Continuer — parce que Retour sur une modale annulable équivaut à son bouton (spec §7)")]
        public void ResultsBack_ActsAsContinue()
        {
            _results.AutoContinue = false;
            _app.Combat.ForceWin.ExecuteAsync().GetAwaiter().GetResult();
            TickUntil(() => _results.Data != null, 50);
            Assert.That(_app.Received<CombatResultsClosed>(), Is.Empty);
            Assert.That(_router.Dispatch(), Is.True);
            Assert.That(_app.Received<CombatResultsClosed>().Count, Is.EqualTo(1));
        }

        [Test, Description("Vérifie qu'une vérification qui répond aussitôt (victoire forcée) reste affichée et n'est pas écrasée par « vérification en cours » — parce que la popup doit montrer le verdict, pas un état d'attente figé")]
        public void InstantVerification_IsNotOverwrittenByPending()
        {
            _results.AutoContinue = false;
            _app.Combat.ForceWin.ExecuteAsync().GetAwaiter().GetResult();
            TickUntil(() => _results.Data != null, 50);
            Assert.That(_results.VerifyText, Is.EqualTo(new LocLocalization().T("combat.verify.skip")));
        }
    }
}
