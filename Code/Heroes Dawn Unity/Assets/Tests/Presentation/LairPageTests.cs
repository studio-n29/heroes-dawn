using System.Linq;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Features.World;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Tests.Domain.Application;
using N29.HeroesDawn.Tests.Presentation.Fakes;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    /// <summary>Page Repaire : préparation, étapes (boss), bilan et entrée confirmée (L12 de la 0.15.0).</summary>
    public sealed class LairPageTests
    {
        TestApp _app = null!;
        FakeListView _view = null!;
        FakeModalHost _host = null!;
        readonly LocLocalization _loc = new LocLocalization();

        LairPagePresenter Page()
        {
            _app = new TestApp();
            _app.SignInDirect("p1");
            _view = new FakeListView();
            _host = new FakeModalHost();
            var mission = new MissionLauncher(_app.Zone.Start, PrototypeContent.Zone11, _app.Publisher<NavigateRequested>(), new FakeToasts(), _loc);
            var p = new LairPagePresenter(_view, _app.Session, _app.Lairs, mission, new ModalService(_host), _loc, new FakeToasts(),
                                          _app.Subscriber<MetaChanged>(), _app.Subscriber<WalletChanged>(), _app.Subscriber<RosterChanged>(), _app.Time);
            _view.Show();
            ((VContainer.Unity.IStartable)p).Start();
            return p;
        }

        void OpenLair()
        {
            foreach (var z in _app.Session.Content.Zones.Where(z => z.Borough == "brooklyn")) _app.Session.CompletedZones.Add(z.Id.Value);
            _app.MetaCtx.Touch(MetaArea.Profile);
        }

        [Test, Description("Vérifie que le repaire fermé affiche la zone à terminer, grise ENTRER et estompe les trois boss — parce que le joueur doit savoir quoi accomplir pour l'ouvrir")]
        public void Locked_ShowsTheRequirement_AndDisablesEnter()
        {
            var p = Page();
            Assert.That(_view.Header.A.Enabled, Is.False);
            Assert.That(_view.Header.Line, Is.EqualTo(_loc.F("lair.locked", _loc.T("zones.ny_1_4.short"))));
            Assert.That(_view.Rows.Length, Is.EqualTo(3));
            Assert.That(_view.Rows.All(r => r.Dim), Is.True);
            p.Dispose();
        }

        [Test, Description("Vérifie que le repaire ouvert liste ses trois boss avec le premier « à affronter », l'escouade avec sa fatigue et le bilan — parce que c'est l'écran de préparation")]
        public void Open_ShowsStepsSquadAndReport()
        {
            var p = Page();
            OpenLair();
            Assert.That(_view.Header.A.Enabled, Is.True);
            Assert.That(_view.Rows[0].Title, Is.EqualTo(_loc.T("enemies.brick.name")));
            Assert.That(_view.Rows[0].Subtitle, Is.EqualTo(_loc.F("lair.boss.next", 1)));
            Assert.That(_view.Rows[0].Highlight, Is.True);
            Assert.That(_view.Rows[1].Subtitle, Is.EqualTo(_loc.F("lair.boss.later", 2)));
            Assert.That(_view.Side.Rows.Length, Is.EqualTo(3));
            Assert.That(_view.Side.Rows.Last().Subtitle, Is.EqualTo(_loc.F("lair.bilan.line", 0, 3, 0)));
            p.Dispose();
        }

        [Test, Description("Vérifie qu'un boss vaincu est marqué « Vaincu » et que le suivant devient l'étape à affronter — parce que la progression entre boss se lit sur la page")]
        public void DefeatedBoss_AdvancesTheSteps()
        {
            var p = Page();
            OpenLair();
            LairBook.RecordBossDefeat(_app.Session.Meta, "zone_lair_amif_vault", "n04");
            _app.MetaCtx.Touch(MetaArea.Profile);
            Assert.That(_view.Rows[0].Subtitle, Is.EqualTo(_loc.T("lair.boss.down")));
            Assert.That(_view.Rows[1].Subtitle, Is.EqualTo(_loc.F("lair.boss.next", 2)));
            Assert.That(_view.Header.ProgressLabel, Is.EqualTo("1 / 3"));
            p.Dispose();
        }

        [Test, Description("Vérifie qu'ENTRER demande confirmation et ne lance rien tant que le joueur refuse — parce qu'un repaire engage plusieurs combats")]
        public void Enter_AsksFirst_AndDeclineDoesNothing()
        {
            var p = Page();
            OpenLair();
            _view.ClickHeader(0);
            var ask = (FakeConfirmPopup)_host.Spawned.Last();
            Assert.That(ask.Asked!.Value.Title, Is.EqualTo(_loc.T("lair.amif_vault.name")));
            ask.Answer(false);
            Assert.That(_app.Session.Run, Is.Null);
            Assert.That(_view.Header.A.Enabled, Is.True, "la page se réarme après un refus");
            p.Dispose();
        }

        [Test, Description("Vérifie qu'ENTRER confirmé démarre la zone du repaire — parce que le donjon se joue comme une zone sur le plateau")]
        public void Enter_Confirmed_StartsTheLairZone()
        {
            var p = Page();
            OpenLair();
            _view.ClickHeader(0);
            ((FakeConfirmPopup)_host.Spawned.Last()).Answer(true);
            Assert.That(_app.Session.Run, Is.Not.Null);
            Assert.That(_app.Session.RunZone!.Id.Value, Is.EqualTo("zone_lair_amif_vault"));
            p.Dispose();
        }

        [Test, Description("Vérifie qu'ENTRER ne fait rien tant que le repaire est fermé — parce que la zone est verrouillée")]
        public void Enter_WhileLocked_DoesNothing()
        {
            var p = Page();
            _view.ClickHeader(0);
            Assert.That(_host.Spawned.OfType<FakeConfirmPopup>().Any(), Is.False);
            p.Dispose();
        }
    }
}
