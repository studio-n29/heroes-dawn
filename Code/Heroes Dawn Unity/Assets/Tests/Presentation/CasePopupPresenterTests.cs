using N29.HeroesDawn.Tests.Domain;
using System.Linq;
using System.Threading;
using N29.HeroesDawn.Application.Features.Zone;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Activities;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Presentation.Board;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Presentation.Popups;
using N29.HeroesDawn.Tests.Domain.Application;
using N29.HeroesDawn.Tests.Presentation.Fakes;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    public sealed class CasePopupPresenterTests
    {
        TestApp _app = null!;
        FakeModalHost _host = null!;
        FakeToasts _toasts = null!;
        FakeCaseResolutions _resolve = null!;
        FakeRestPopup _rest = null!;
        FakeNarrativePopup _narrative = null!;
        FakeMiniGamePopup _miniGame = null!;
        FakeActivityPopup _activityPopup = null!;
        FakePlanActivity _plan = null!;
        FakeZoneCompletePopup _zone = null!;
        CasePopupPresenter _p = null!;

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _host = new FakeModalHost();
            _rest = new FakeRestPopup();
            _narrative = new FakeNarrativePopup();
            _miniGame = new FakeMiniGamePopup();
            _activityPopup = new FakeActivityPopup();
            _zone = new FakeZoneCompletePopup();
            _host.Register<IRestPopupView>(() => _rest);
            _host.Register<INarrativePopupView>(() => _narrative);
            _host.Register<IMiniGamePopupView>(() => _miniGame);
            _host.Register<IActivityPopupView>(() => _activityPopup);
            _host.Register<IZoneCompletePopupView>(() => _zone);
            _toasts = new FakeToasts();
            _resolve = new FakeCaseResolutions();
            _plan = new FakePlanActivity(_app.Session.Content);
            _p = new CasePopupPresenter(new ModalService(_host), _resolve, _resolve, _plan, _resolve, _app.Session, _toasts, new LocLocalization());
        }

        static CaseOutcome Case(CaseType t) => new CaseOutcome(new CaseId("n07"), t, CombatRank.Easy, null);

        [Test, Description("Vérifie que choisir de se reposer résout la case Repos avec « oui » et rend les événements du plateau — parce que le plateau doit jouer la suite (soin, objectifs)")]
        public void Rest_Take_ResolvesRestTrueAndReturnsEvents()
        {
            var events = _p.RunAsync(Case(CaseType.Rest), CancellationToken.None).GetAwaiter().GetResult();
            Assert.That(_resolve.Calls, Is.EqualTo(new object[] { true }));
            Assert.That(events, Is.SameAs(_resolve.Events));
        }

        [Test, Description("Vérifie que le choix « recherche » d'un événement est transmis tel quel et que les trois choix sont proposés — parce que chaque choix donne une récompense différente (GDD, événements)")]
        public void Narrative_Research_ResolvesResearch()
        {
            _p.RunAsync(Case(CaseType.Narrative), CancellationToken.None).GetAwaiter().GetResult();
            Assert.That(_resolve.Calls, Is.EqualTo(new object[] { NarrativeChoice.Research }));
            Assert.That(_narrative.Data!.Value.Choices.Select(c => c.Choice),
                        Is.EqualTo(new[] { NarrativeChoice.Credits, NarrativeChoice.Research, NarrativeChoice.Morale }));
        }

        [Test, Description("Vérifie qu'un mini-jeu réussi annonce la récompense et résout la case avec « réussi » — parce que la réussite est décidée par le client au M1 (plan M1, mini-jeu)")]
        public void MiniGame_Success_ToastsRewardAndResolvesTrue()
        {
            _p.RunAsync(Case(CaseType.MiniGame), CancellationToken.None).GetAwaiter().GetResult();
            var b = _app.Session.Content.Balance;
            Assert.That(((ActivityResult)_resolve.Calls.Single()).Success, Is.True);
            Assert.That(_toasts.Shown, Has.Some.Matches<(string Text, ToastKind Kind)>(t => t.Kind == ToastKind.Reward && t.Text == Loc.F("mg.success", b.MiniGameCredits, b.MiniGamePr)));
        }

        [Test, Description("Vérifie qu'un mini-jeu raté prévient le joueur et résout la case avec « raté » — parce qu'un échec n'apporte rien mais termine la case")]
        public void MiniGame_Failure_ToastsWarningAndResolvesFalse()
        {
            _miniGame.Answer = false;
            _p.RunAsync(Case(CaseType.MiniGame), CancellationToken.None).GetAwaiter().GetResult();
            Assert.That(((ActivityResult)_resolve.Calls.Single()).Success, Is.False);
            Assert.That(_toasts.Shown, Has.Some.Matches<(string Text, ToastKind Kind)>(t => t.Kind == ToastKind.Warning && t.Text == Loc.T("mg.fail")));
        }

        [Test, Description("Vérifie qu'une résolution en échec affiche l'erreur et ne rend aucun événement — parce que le plateau ne doit rien jouer d'une case non résolue")]
        public void UseCaseFailure_ReturnsEmptyAndToastsError()
        {
            _resolve.Fail = true;
            var events = _p.RunAsync(Case(CaseType.Rest), CancellationToken.None).GetAwaiter().GetResult();
            Assert.That(events, Is.Empty);
            Assert.That(_toasts.Shown, Has.Some.Matches<(string Text, ToastKind Kind)>(t => t.Kind == ToastKind.Error));
        }

        [Test, Description("Vérifie que la popup est fermée une fois la case résolue — parce qu'une popup restée ouverte bloquerait le plateau")]
        public void Popup_IsClosedAfterResolution()
        {
            _p.RunAsync(Case(CaseType.Narrative), CancellationToken.None).GetAwaiter().GetResult();
            Assert.That(_host.Despawned, Is.EqualTo(new object[] { _narrative }));
        }

        [Test, Description("Vérifie que les popups de case ne sont pas annulables par Retour — parce qu'un choix est requis pour terminer la case (spec §7)")]
        public void CasePopups_AreNotCancelable()
        {
            var modals = new ModalService(_host);
            foreach (var t in new[] { CaseType.Rest, CaseType.Narrative, CaseType.MiniGame })
            {
                var p = new CasePopupPresenter(modals, _resolve, _resolve, _plan, _resolve, _app.Session, _toasts, new LocLocalization());
                p.RunAsync(Case(t), CancellationToken.None).GetAwaiter().GetResult();
            }
            Assert.That(_host.Spawned.Cast<IPopupView>().All(v => !v.Cancelable), Is.True);
        }
        CasePopupPresenter Presenter(IPlanActivityUseCase plan)
            => new CasePopupPresenter(new ModalService(_host), _resolve, _resolve, plan, _resolve, _app.Session, _toasts, new LocLocalization());

        bool Toasted(ToastKind kind, string text) => _toasts.Shown.Any(t => t.Kind == kind && t.Text == text);

        [Test, Description("Vérifie qu'une première validation de zone annonce Crédits et PR — parce que la première validation rapporte aussi des PR (GDD §5)")]
        public void ZoneComplete_FirstClear_ShowsCreditsAndPr()
        {
            _p.ShowZoneCompleteAsync(new ZoneRewards(1500, 40, true), CancellationToken.None).GetAwaiter().GetResult();
            Assert.That(_zone.Data!.Value.Rewards, Is.EqualTo(Loc.F("board.zone_rewards", 1500, 40)));
            Assert.That(_host.Despawned, Does.Contain(_zone));
        }

        [Test, Description("Vérifie que la validation d'une zone annonce le booster gagné et le nom de l'agent qui rejoint l'équipe — parce que les déblocages doivent se voir à l'écran")]
        public void ZoneComplete_AnnouncesBoosterAndNewAgents()
        {
            var rewards = new ZoneRewards(1500, 40, true, 1, new[] { new MemberId("vanguard") });
            var p = new CasePopupPresenter(new ModalService(_host), _resolve, _resolve, _app.Zone.PlanActivity, _resolve, new GameSession(TestContent.Build()), _toasts, new LocLocalization());
            p.ShowZoneCompleteAsync(rewards, CancellationToken.None).GetAwaiter().GetResult();
            Assert.That(_zone.Data!.Value.Rewards, Does.Contain("+1 booster").And.Contain("Vanguard"));
        }
    }
}
