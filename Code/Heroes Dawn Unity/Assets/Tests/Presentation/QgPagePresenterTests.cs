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
            _p = new QgPagePresenter(_view, _app.Session, _nav, mission, _app.Time, loc, _app.Overview, _app.Subscriber<RosterChanged>(),
                                     _app.Subscriber<N29.HeroesDawn.Application.Features.Meta.MetaChanged>());
            _view.Show();
            _p.Start();
            return _p;
        }

        RoomCardData Room(HubPage p) => _view.Last.Rooms.Single(r => r.Page == p);

        [Test, Description("Vérifie que la carte Équipe et soins annonce les agents fatigués, sinon ceux à entraîner — parce que les deux anciennes pièces n'en font plus qu'une")]
        public void CareCard_AnnouncesTiredThenToTrain()
        {
            _app.Session.Find(new MemberId("ancre"))!.Level = 50;
            _app.Session.Find(new MemberId("givre"))!.Level = 50;
            Make();
            Assert.That(Room(HubPage.Team).Info, Is.EqualTo("2 membres à entraîner"));
            Assert.That(_view.Last.Rooms.Count(r => r.Page == HubPage.Team), Is.EqualTo(1), "une seule carte pour les agents");
            _p.Dispose();
            _app.Session.Find(new MemberId("echo"))!.Fatigue = 40;
            _view = new FakeQgPageView();
            Make();
            Assert.That(Room(HubPage.Team).Info, Is.EqualTo("1 membre fatigué"));
            Assert.That(Room(HubPage.Team).Badge, Is.EqualTo(1));
        }

        [Test, Description("Vérifie que le QG montre six pièces illustrées (salon, labo, opérations, grimoire, inventaire, registre) et plus les Archives, déplacées dans Profil — parce que le QG est simplifié")]
        public void Rooms_AreSixAndNoArchives()
        {
            Make();
            Assert.That(_view.Last.Rooms.Select(r => r.Page).ToArray(),
                        Is.EqualTo(new[] { HubPage.Team, HubPage.Lab, HubPage.Operations, HubPage.Grimoire, HubPage.Inventory, HubPage.Agents }));
            Assert.That(Room(HubPage.Operations).Info, Is.EqualTo(new LocLocalization().T("qg.ops.idle")));
            Assert.That(Room(HubPage.Lab).Info, Is.EqualTo(new LocLocalization().T("qg.lab.idle")));
        }

        [Test, Description("Vérifie que le QG ne montre plus les missions du jour (réservées au Pass) mais les activités du jour, et l'objectif secondaire en icône — parce que la quête principale et les quêtes secondaires sont séparées")]
        public void Activities_ReplaceDailyMissions_AndSecondaryIsAnIcon()
        {
            Make();
            Assert.That(_view.Last.DailyLines.Length, Is.EqualTo(1), "aucune zone terminée : un message d'invitation");
            Assert.That(_view.Last.SecondaryActive, Is.False);
            _p.Dispose();
            _app.Session.CompletedZones.Add(PrototypeContent.Zone11.Value);
            _view = new FakeQgPageView();
            Make();
            Assert.That(_view.Last.DailyLines.Length, Is.EqualTo(1), "une zone terminée : une activité");
            Assert.That(_view.Last.DailyLines[0], Does.Contain("0 /"));
            Assert.That(_view.Last.SecondaryActive, Is.True);
            Assert.That(_view.Last.SecondaryText, Does.Contain("Red Hook"));
        }

        [Test, Description("Vérifie que toucher le panneau des activités ouvre le suivi des quêtes, pas le Pass — parce que les missions du jour ne sont plus au QG")]
        public void ActivitiesPanel_DoesNotOpenPass()
        {
            Make();
            _view.ClickDaily();
            Assert.That(_nav.Current.Page, Is.EqualTo(HubPage.Qg));
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
            Assert.That(_view.Last.FilText, Is.EqualTo(new LocLocalization().T("qg.fil.zone_ny_1_2")), "la zone suivante prend le relais");
            _p.Dispose();
            _app.Session.CompletedZones.Add(PrototypeZones.Zone12.Value);
            _app.Session.CompletedZones.Add(PrototypeZones.Zone13.Value);
            _view = new FakeQgPageView();
            Make();
            Assert.That(_view.Last.FilText, Is.EqualTo(new LocLocalization().T("qg.fil.zone_ny_1_4")), "la dernière zone de Brooklyn");
            _p.Dispose();
            _app.Session.CompletedZones.Add(PrototypeZones.Zone14.Value);
            _view = new FakeQgPageView();
            Make();
            Assert.That(_view.Last.FilText, Is.EqualTo(new LocLocalization().T("qg.fil.done")));
        }

        [Test, Description("Vérifie que toucher une pièce ouvre sa page — parce que le QG est la porte d'entrée des pièces (M10 ③)")]
        public void RoomClick_OpensRoomPage()
        {
            Make();
            _view.ClickRoom(HubPage.Lab);
            Assert.That(_nav.Current.Page, Is.EqualTo(HubPage.Lab));
        }

        [Test, Description("Vérifie que la carte Équipe et soins ouvre l'onglet Équipe — parce que l'entraînement et les soins se font sur la fiche des agents")]
        public void CareCard_OpensTheTeamTab()
        {
            Make();
            _view.ClickRoom(HubPage.Team);
            Assert.That(_nav.Current.Page, Is.EqualTo(HubPage.Team));
            Assert.That(_nav.IsOnTab, Is.True);
        }

        [Test, Description("Vérifie que MISSION ouvre la carte de New York au lieu de lancer la zone directement — parce que le joueur choisit sa zone sur la carte (M60)")]
        public void MissionClick_OpensCityMap()
        {
            Make();
            Assert.That(_view.Last.MissionEnabled, Is.True);
            _view.ClickMission();
            Assert.That(_nav.Current.Page, Is.EqualTo(HubPage.CityMap));
            Assert.That(_app.Session.Run, Is.Null, "rien n'est lancé tant que le joueur n'a pas déployé");
            Assert.That(_app.Received<NavigateRequested>().Count(n => n.Target == GameState.Board), Is.Zero);
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
