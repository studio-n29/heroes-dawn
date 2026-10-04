using System.Linq;
using N29.HeroesDawn.Application.Features.Hub;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Tests.Domain.Application;
using N29.HeroesDawn.Tests.Presentation.Fakes;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    public sealed class TeamPagePresenterTests
    {
        TestApp _app = null!;
        FakeTeamPageView _view = null!;
        FakeToasts _toasts = null!;
        HubNavigator _nav = null!;
        TeamPagePresenter _p = null!;
        TeamViewState _state = null!;
        readonly LocLocalization _loc = new LocLocalization();

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.Account.SignIn.ExecuteAsync().GetAwaiter().GetResult();
            _view = new FakeTeamPageView();
            _state = new TeamViewState();
            _toasts = new FakeToasts();
            _nav = new HubNavigator();
        }

        TeamPagePresenter Make(IToggleSquadUseCase toggle = null)
        {
            var mission = new MissionLauncher(_app.Zone.Start, PrototypeContent.Zone11, _app.Publisher<NavigateRequested>(), _toasts, _loc);
            _p = new TeamPagePresenter(_view, _app.Session, toggle ?? _app.Hub.ToggleSquad, mission, _nav, _state, _toasts, _loc, _app.Subscriber<RosterChanged>());
            _view.Show();
            _p.Start();
            return _p;
        }

        MemberTileData Tile(string id) => _view.Last.Members.Single(m => m.Id == new MemberId(id));

        [Test, Description("Vérifie que la grille montre tous les membres avec leur appartenance à l'escouade, et le titre leur nombre — parce que l'écran Équipe compose l'escouade (M20)")]
        public void Grid_ShowsAllMembers_WithInSquadFlag()
        {
            Make();
            Assert.That(_view.Last.Members.Length, Is.EqualTo(4));
            Assert.That(_view.Last.Members.All(m => m.InSquad), Is.True);
            Assert.That(_view.Last.Squad.Length, Is.EqualTo(4));
            Assert.That(_p.Title, Is.EqualTo("Équipe · 4 membres"));
        }

        [Test, Description("Vérifie que le filtre par famille et le tri sont appliqués à la grille — parce que l'écran Équipe filtre et trie (M20 ①②)")]
        public void Filter_And_Sort_Applied()
        {
            _app.Session.Find(new MemberId("seve"))!.Fatigue = 60;
            Make();
            _view.ClickFilter(1 + (int)Family.Elemental);
            Assert.That(_view.Last.Members.Select(m => m.Id.Value), Is.EqualTo(new[] { "givre" }));
            Assert.That(_view.Last.FilterIndex, Is.EqualTo(1 + (int)Family.Elemental));
            _view.ClickFilter(0);
            _view.ClickSort(MemberSort.Fatigue);
            Assert.That(_view.Last.Members.First().Id.Value, Is.EqualTo("seve"));
            Assert.That(_view.Last.Sort, Is.EqualTo(MemberSort.Fatigue));
        }

        [Test, Description("Vérifie que le filtre et le tri sont retrouvés après un passage par la fiche — parce que l'état utile vit hors de la vue (spec §3) ; la page est recréée à chaque retour (relecture, I1)")]
        public void Filter_And_Sort_KeptAfterLeavingPage()
        {
            Make();
            _view.ClickFilter(1 + (int)Family.Elemental);
            _view.ClickSort(MemberSort.Fatigue);
            _p.Dispose();
            _view = new FakeTeamPageView();
            Make();
            Assert.That(_view.Last.FilterIndex, Is.EqualTo(1 + (int)Family.Elemental));
            Assert.That(_view.Last.Sort, Is.EqualTo(MemberSort.Fatigue));
            Assert.That(_view.Last.Members.Select(m => m.Id.Value), Is.EqualTo(new[] { "givre" }));
        }

        [Test, Description("Vérifie que toucher le bouton d'escouade retire puis remet un membre — parce que l'escouade se compose ici (M20 ④)")]
        public void SquadToggle_AddsAndRemoves()
        {
            Make();
            _view.ToggleSquad(new MemberId("echo"));
            Assert.That(Tile("echo").InSquad, Is.False);
            Assert.That(_view.Last.Squad.Length, Is.EqualTo(3));
            _view.ToggleSquad(new MemberId("echo"));
            Assert.That(Tile("echo").InSquad, Is.True);
            Assert.That(_view.Last.Squad.Length, Is.EqualTo(4));
        }

        [Test, Description("Vérifie que retirer le dernier membre affiche l'erreur sans rien changer — parce qu'une escouade vide ne peut pas partir (Review Focus 5)")]
        public void SquadToggle_LastMember_ShowsErrorAndKeepsState()
        {
            foreach (var m in _app.Session.Roster) m.InSquad = m.Id.Value == "givre";
            Make();
            _view.ToggleSquad(new MemberId("givre"));
            Assert.That(_toasts.Shown.Last(), Is.EqualTo((_loc.T("errors.squad_empty"), ToastKind.Error)));
            Assert.That(Tile("givre").InSquad, Is.True);
        }

        [Test, Description("Vérifie qu'ajouter un 5e membre affiche « escouade complète » sans rien changer — parce que l'escouade compte 4 places (Review Focus 5)")]
        public void SquadToggle_FifthMember_ShowsSquadFull()
        {
            _app.Session.Roster.Add(new TeamMember(_app.Session.Content.Member(new MemberId("arc")), 5));
            Make();
            _view.ToggleSquad(new MemberId("arc"));
            Assert.That(_toasts.Shown.Last(), Is.EqualTo((_loc.T("errors.squad_full"), ToastKind.Error)));
            Assert.That(Tile("arc").InSquad, Is.False);
            Assert.That(_view.Last.Squad.Length, Is.EqualTo(4));
        }

        [Test, Description("Vérifie que les combos formés par l'escouade sont listés, et suivent ses changements — parce que l'écran Équipe montre les combos (M20 ⑤)")]
        public void Combos_ListedForCurrentSquad()
        {
            Make();
            Assert.That(_view.Last.Combos.Length, Is.EqualTo(6));
            Assert.That(_view.Last.Combos.Any(c => c.Contains("Résonance vitale")), Is.True);
            _view.ToggleSquad(new MemberId("seve"));
            Assert.That(_view.Last.Combos.Length, Is.EqualTo(3));
            Assert.That(_view.Last.Combos.Any(c => c.Contains("Résonance vitale")), Is.False);
        }

        [Test, Description("Vérifie qu'un membre épuisé est signalé par un libellé, pas seulement par la couleur — parce que la couleur ne porte jamais seule le sens (contraintes)")]
        public void Exhausted_ShownWithLabel()
        {
            _app.Session.Find(new MemberId("echo"))!.Fatigue = 100;
            Make();
            Assert.That(Tile("echo").Exhausted, Is.True);
            Assert.That(Tile("echo").Status, Is.EqualTo(_loc.T("team.exhausted")));
            Assert.That(Tile("givre").Status, Is.EqualTo(_loc.T("hub.squad.in")));
        }

        [Test, Description("Vérifie que toucher une carte ouvre la fiche du membre — parce que la fiche détaille stats, compétences et combos (M21)")]
        public void MemberTap_OpensSheet()
        {
            Make();
            _view.TapMember(new MemberId("givre"));
            Assert.That(_nav.Current.Page, Is.EqualTo(HubPage.MemberSheet));
            Assert.That(_nav.Current.Member, Is.EqualTo(new MemberId("givre")));
        }

        [Test, Description("Vérifie qu'une page détruite pendant un changement d'escouade n'appelle plus la vue ni les toasts — parce que la page peut être quittée à tout moment (Review Focus 3)")]
        public void Dispose_DuringToggle_NoViewCallAfterwards()
        {
            var pending = new PendingToggle();
            Make(pending);
            _view.ToggleSquad(new MemberId("echo"));
            int shows = _view.Shown.Count;
            _p.Dispose();
            pending.Done.TrySetResult(Result.Fail(HubErrors.SquadFull));
            Assert.That(_view.Shown.Count, Is.EqualTo(shows));
            Assert.That(_toasts.Shown, Is.Empty);
        }

        [Test, Description("Vérifie que « Préparer une mission » lance la zone comme MISSION — parce que c'est la même action (spec §4)")]
        public void Mission_LaunchesZone()
        {
            Make();
            _view.ClickMission();
            Assert.That(_app.Session.Run, Is.Not.Null);
        }
    }
}
