using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Meta;
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
    /// <summary>Entrée en combat : un bandeau par combattant, agents à gauche et ennemis à droite, introductions répétées abrégées (lot L7 de la 0.15.0).</summary>
    public sealed class CombatIntroTests
    {
        sealed class FakeCombatIntroPopup : FakePopup, ICombatIntroView
        {
            public CombatIntroData? Data;
            public bool Hold;
            UniTaskCompletionSource? _done;

            public UniTask PlayAsync(CombatIntroData data, CancellationToken ct)
            {
                Data = data;
                if (!Hold) return UniTask.CompletedTask;
                _done = new UniTaskCompletionSource();
                return _done.Task;
            }

            public void Release() => _done?.TrySetResult();
        }

        TestApp _app = null!;
        FakeCombatView _view = null!;
        FakeCombatIntroPopup _intro = null!;
        CombatIntros _intros = null!;
        CombatSimulation _sim = null!;
        CombatPresenter _p = null!;
        FakeModalHost _host = null!;
        ModalService _modals = null!;

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
            _intro = new FakeCombatIntroPopup();
            _host = new FakeModalHost();
            _host.Register<ICombatResultsPopupView>(() => new FakeCombatResultsPopup());
            _host.Register<ICombatIntroView>(() => _intro);
            _modals = new ModalService(_host);
            _intros = new CombatIntros(_app.MetaCtx) { Enabled = true };
        }

        CombatPresenter Show()
        {
            _view = new FakeCombatView();
            var p = new CombatPresenter(_view, _app.Combat.Active, _app.Session, _app.Combat.SetAuto, _app.Combat.Finish, _app.Combat.Verify,
                                        _app.Combat.ForceWin, _app.Publisher<CombatResultsClosed>(), _app.Publisher<NavigateRequested>(),
                                        _modals, new FakeToasts(), new LocLocalization(), new FakeFrameClock(), new BackButtonRouter(new FakeBackInput(), _modals),
                                        intros: _intros);
            _view.Show();
            p.Start();
            return p;
        }

        [Test, Description("Vérifie que l'entrée en combat montre tous les alliés d'un côté et tous les ennemis vivants de l'autre, avec leurs titres — parce que le joueur voit qui s'affronte avant la première action")]
        public void Intro_ListsTheSquadAndTheEnemies()
        {
            _p = Show();
            var data = _intro.Data;
            Assert.That(data, Is.Not.Null, "l'entrée en combat s'est ouverte");
            Assert.That(data!.Allies.Count, Is.EqualTo(_sim.Units.Count(u => u.Side == Side.Ally)));
            Assert.That(data.Enemies.Count, Is.EqualTo(_sim.Units.Count(u => u.Side == Side.Enemy && u.IsAlive)));
            Assert.That(data.Allies.Concat(data.Enemies).All(f => f.Name.Length > 0 && f.Sub.Length > 0), Is.True, "nom et rôle sur chaque bandeau");
            Assert.That(data.AllyTitle, Is.Not.Empty);
            Assert.That(data.EnemyTitle, Is.Not.Empty);
            _p.Dispose();
        }

        [Test, Description("Vérifie que la première entrée en combat est complète puis mémorisée, et que la même rencontre rejouée est marquée répétée — parce que les introductions répétées doivent pouvoir être abrégées")]
        public void Intro_IsMarkedRepeat_OnTheSecondEncounter()
        {
            _p = Show();
            Assert.That(_intro.Data!.Repeat, Is.False, "première fois");
            string key = CombatIntros.KeyOf(_sim.Units.Where(u => u.Side == Side.Enemy).Select(u => u.DefId));
            Assert.That(_intros.Seen(key), Is.True, "mémorisée à la fin de l'introduction");
            _p.Dispose();

            _intro.Data = null;
            _p = Show();
            Assert.That(_intro.Data!.Repeat, Is.True, "deuxième fois");
            _p.Dispose();
        }

        [Test, Description("Vérifie que désactivée, l'entrée en combat ne s'ouvre pas — parce que les parcours automatiques ne doivent pas attendre les bandeaux")]
        public void Disabled_OpensNothing()
        {
            _intros.Enabled = false;
            _p = Show();
            Assert.That(_intro.Data, Is.Null);
            _p.Dispose();
        }

        [Test, Description("Vérifie que la simulation ne bouge pas tant que l'entrée en combat est ouverte, puis avance une fois fermée — parce que le combat ne doit pas commencer derrière les bandeaux")]
        public void Combat_WaitsForTheIntro()
        {
            _intro.Hold = true;
            _p = Show();
            int start = _sim.Tick;
            for (int i = 0; i < 30; i++) _p.Tick();
            Assert.That(_sim.Tick, Is.EqualTo(start), "gelé pendant l'introduction");
            _intro.Release();
            for (int i = 0; i < 30; i++) _p.Tick();
            Assert.That(_sim.Tick, Is.GreaterThan(start), "reprend ensuite");
            _p.Dispose();
        }
    }
}
