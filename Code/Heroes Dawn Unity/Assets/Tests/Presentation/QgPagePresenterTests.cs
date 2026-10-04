using System;
using System.Linq;
using N29.HeroesDawn.Application.Flow;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Presentation.Wip;
using N29.HeroesDawn.Tests.Domain.Application;
using N29.HeroesDawn.Tests.Presentation.Fakes;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    public sealed class QgPagePresenterTests
    {
        TestApp _app = null!;
        FakeQgPageView _view = null!;
        HubNavigator _nav = null!;
        QgPagePresenter _p = null!;

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.Account.SignIn.ExecuteAsync().GetAwaiter().GetResult();
            _view = new FakeQgPageView();
            _nav = new HubNavigator();
        }

        QgPagePresenter Make()
        {
            var loc = new LocLocalization();
            var mission = new MissionLauncher(_app.Zone.Start, PrototypeContent.Zone11, _app.Publisher<NavigateRequested>(), new FakeToasts(), loc);
            _p = new QgPagePresenter(_view, _app.Session, _nav, mission, _app.Time, loc, _app.Subscriber<RosterChanged>());
            _view.Show();
            _p.Start();
            return _p;
        }

        RoomCardData Room(HubPage p) => _view.Last.Rooms.Single(r => r.Page == p);

        [Test, Description("Vérifie que le Salon et l'Infirmerie affichent des infos réelles (« 2 membres à entraîner », « 1 membre fatigué ») — parce que ces deux pièces fonctionnent dans ce lot (spec §4, M10)")]
        public void Rooms_ShowRealInfoForSalonAndInfirmary()
        {
            _app.Session.Find(new MemberId("ancre"))!.Level = 50;
            _app.Session.Find(new MemberId("givre"))!.Level = 50;
            _app.Session.Find(new MemberId("echo"))!.Fatigue = 40;
            Make();
            Assert.That(Room(HubPage.Salon).Info, Is.EqualTo("2 membres à entraîner"));
            Assert.That(Room(HubPage.Infirmary).Info, Is.EqualTo("1 membre fatigué"));
            Assert.That(Room(HubPage.Salon).WipId, Is.Null);
            Assert.That(Room(HubPage.Infirmary).WipId, Is.Null);
        }

        [Test, Description("Vérifie qu'Opérations, Labo et Archives sont marqués BIENTÔT — parce que ces pièces ne sont pas développées (spec §4)")]
        public void WipRooms_MarkedSoon()
        {
            Make();
            Assert.That(Room(HubPage.Operations).WipId, Is.EqualTo(WipCatalog.Operations.Id));
            Assert.That(Room(HubPage.Lab).WipId, Is.EqualTo(WipCatalog.Lab.Id));
            Assert.That(Room(HubPage.Archives).WipId, Is.EqualTo(WipCatalog.Archives.Id));
            Assert.That(_view.Last.Rooms.Length, Is.EqualTo(5));
        }

        [Test, Description("Vérifie que les missions du jour et les objectifs secondaires sont marqués BIENTÔT — parce qu'ils viennent avec le pass et le mode mission (spec §4)")]
        public void DailyMissions_And_SecondaryObjectives_MarkedSoon()
        {
            Make();
            Assert.That(_view.Last.DailyMissionsWipId, Is.EqualTo(WipCatalog.DailyMissions.Id));
            Assert.That(_view.Last.SecondaryObjectivesWipId, Is.EqualTo(WipCatalog.SecondaryObjectives.Id));
        }

        [Test, Description("Vérifie que les alertes montrent le vrai compte à rebours de la boutique — parce que le joueur doit savoir quand arrivent les nouveaux héros (M10 ⑤)")]
        public void Alerts_ShowShopCountdown()
        {
            _app.Time.LocalNow = new DateTime(2026, 10, 3, 12, 0, 0);
            Make();
            Assert.That(_view.Last.ShopAlert, Is.EqualTo("Nouveaux héros dans 21 h 00"));
            _app.Time.LocalNow = new DateTime(2026, 10, 3, 12, 1, 0);
            _p.Tick();
            Assert.That(_view.Last.ShopAlert, Is.EqualTo("Nouveaux héros dans 20 h 59"));
        }

        [Test, Description("Vérifie que le fil du QG donne l'objectif de la première zone non validée, puis un message de fin — parce que Rayner guide le joueur vers la suite (M10 ⑥)")]
        public void FilText_UsesFirstUncompletedZone()
        {
            Make();
            Assert.That(_view.Last.FilText, Is.EqualTo(new LocLocalization().T("qg.fil.zone_ny_1_1")));
            Assert.That(_view.Last.FilSpeaker, Is.EqualTo("RAYNER"));
            _p.Dispose();
            _app.Session.CompletedZones.Add(PrototypeContent.Zone11.Value);
            _view = new FakeQgPageView();
            Make();
            Assert.That(_view.Last.FilText, Is.EqualTo(new LocLocalization().T("qg.fil.done")));
        }

        [Test, Description("Vérifie que toucher une pièce ouvre sa page — parce que le QG est la porte d'entrée des pièces (M10 ③)")]
        public void RoomClick_OpensRoomPage()
        {
            Make();
            _view.ClickRoom(HubPage.Infirmary);
            Assert.That(_nav.Current.Page, Is.EqualTo(HubPage.Infirmary));
        }

        [Test, Description("Vérifie que MISSION démarre la zone et demande le plateau — parce que c'est l'action principale du QG (spec §4)")]
        public void MissionClick_LaunchesMission()
        {
            Make();
            Assert.That(_view.Last.MissionEnabled, Is.True);
            _view.ClickMission();
            Assert.That(_app.Session.Run, Is.Not.Null);
            Assert.That(_app.Received<NavigateRequested>().Count(n => n.Target == GameState.Board), Is.EqualTo(1));
        }

        [Test, Description("Vérifie que MISSION est désactivé avec la raison quand l'escouade est épuisée — parce qu'un bouton grisé doit dire pourquoi (contraintes)")]
        public void Mission_DisabledWhenSquadExhausted()
        {
            foreach (var m in _app.Session.Roster) m.Fatigue = 100;
            Make();
            Assert.That(_view.Last.MissionEnabled, Is.False);
            Assert.That(_view.Last.MissionHint, Is.EqualTo(new LocLocalization().T("hub.deploy.exhausted")));
        }
    }
}
