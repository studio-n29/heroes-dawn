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
    /// <summary>Écran Repaire (planche 12.4) : liste des boss, fiche du boss choisi, progression, lore et entrée confirmée.</summary>
    public sealed class LairPageTests
    {
        TestApp _app = null!;
        FakeLairPageView _view = null!;
        FakeModalHost _host = null!;
        readonly LocLocalization _loc = new LocLocalization();

        LairPagePresenter Page()
        {
            _app = new TestApp();
            _app.SignInDirect("p1");
            _view = new FakeLairPageView();
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

        [Test, Description("Vérifie que le repaire fermé affiche la zone à terminer, grise ENTRER et marque les trois boss verrouillés — parce que le joueur doit savoir quoi accomplir pour l'ouvrir")]
        public void Locked_ShowsTheRequirement_AndDisablesEnter()
        {
            var p = Page();
            Assert.That(_view.Last.CanEnter, Is.False);
            Assert.That(_view.Last.EnterReason, Is.EqualTo(_loc.F("lair.locked", _loc.T("zones.ny_1_4.short"))));
            Assert.That(_view.Last.Bosses.Length, Is.EqualTo(3));
            Assert.That(_view.Last.Bosses.All(b => b.Locked), Is.True);
            p.Dispose();
        }

        [Test, Description("Vérifie que le repaire ouvert liste ses trois boss avec le premier « à affronter » et sélectionné, et que la fiche montre son nom — parce que c'est l'écran de préparation de la planche")]
        public void Open_ListsTheBosses_WithTheFirstSelected()
        {
            var p = Page();
            OpenLair();
            Assert.That(_view.Last.CanEnter, Is.True);
            Assert.That(_view.Last.Bosses.Select(b => b.EnemyId), Is.EqualTo(new[] { "brick", "crocodile", "bridge_titan" }));
            Assert.That(_view.Last.Bosses[0].Name, Is.EqualTo(_loc.T("enemies.brick.name")));
            Assert.That(_view.Last.Bosses[0].Next, Is.True);
            Assert.That(_view.Last.Bosses[1].Next, Is.False);
            Assert.That(_view.Last.SelectedIndex, Is.EqualTo(0));
            Assert.That(_view.Last.BossName, Is.EqualTo(_loc.T("enemies.brick.name")));
            Assert.That(_view.Last.Lore, Is.EqualTo(_loc.T("lair.lore.brick")));
            Assert.That(_view.Last.ProgressText, Is.EqualTo("0 %"));
            p.Dispose();
        }

        [Test, Description("Vérifie qu'un boss vaincu est marqué vaincu, que le suivant devient l'étape à affronter et que la progression passe à 33 % — parce que la progression se lit sur la fiche")]
        public void DefeatedBoss_AdvancesTheProgress()
        {
            var p = Page();
            OpenLair();
            LairBook.RecordBossDefeat(_app.Session.Meta, "zone_lair_amif_vault", "n04");
            _app.MetaCtx.Touch(MetaArea.Profile);
            Assert.That(_view.Last.Bosses[0].Defeated, Is.True);
            Assert.That(_view.Last.Bosses[1].Next, Is.True);
            Assert.That(_view.Last.SelectedIndex, Is.EqualTo(1), "la fiche suit le prochain boss tant que le joueur n'a pas choisi");
            Assert.That(_view.Last.ProgressText, Is.EqualTo("33 %"));
            p.Dispose();
        }

        [Test, Description("Vérifie que toucher un boss de la liste affiche sa fiche (nom, lore, récompense) et que le choix survit aux rafraîchissements — parce que la planche montre un boss à la fois")]
        public void SelectingABoss_ShowsItsSheet_AndSticks()
        {
            var p = Page();
            OpenLair();
            _view.SelectBoss(2);
            Assert.That(_view.Last.SelectedIndex, Is.EqualTo(2));
            Assert.That(_view.Last.BossName, Is.EqualTo(_loc.T("enemies.bridge_titan.name")));
            Assert.That(_view.Last.Lore, Is.EqualTo(_loc.T("lair.lore.bridge_titan")));
            Assert.That(_view.Last.RewardLine, Is.Not.Empty);
            _app.MetaCtx.Touch(MetaArea.Profile);
            Assert.That(_view.Last.SelectedIndex, Is.EqualTo(2));
            p.Dispose();
        }

        [Test, Description("Vérifie que la fiche montre l'escouade avec sa fatigue maximale sous le bouton — parce que l'ancien panneau de préparation a été replié dans l'écran")]
        public void Sheet_ShowsTheSquadLine()
        {
            var p = Page();
            OpenLair();
            Assert.That(_view.Last.SquadLine, Does.Contain(_loc.F("lair.squad", _app.Session.Squad.Count)));
            p.Dispose();
        }

        [Test, Description("Vérifie qu'ENTRER demande confirmation et ne lance rien tant que le joueur refuse — parce qu'un repaire engage plusieurs combats")]
        public void Enter_AsksFirst_AndDeclineDoesNothing()
        {
            var p = Page();
            OpenLair();
            _view.ClickEnter();
            var ask = (FakeConfirmPopup)_host.Spawned.Last();
            Assert.That(ask.Asked!.Value.Title, Is.EqualTo(_loc.T("lair.amif_vault.name")));
            ask.Answer(false);
            Assert.That(_app.Session.Run, Is.Null);
            Assert.That(_view.Last.CanEnter, Is.True, "la page se réarme après un refus");
            p.Dispose();
        }

        [Test, Description("Vérifie qu'ENTRER confirmé démarre la zone du repaire — parce que le donjon se joue comme une zone sur le plateau")]
        public void Enter_Confirmed_StartsTheLairZone()
        {
            var p = Page();
            OpenLair();
            _view.ClickEnter();
            ((FakeConfirmPopup)_host.Spawned.Last()).Answer(true);
            Assert.That(_app.Session.Run, Is.Not.Null);
            Assert.That(_app.Session.RunZone!.Id.Value, Is.EqualTo("zone_lair_amif_vault"));
            p.Dispose();
        }

        [Test, Description("Vérifie qu'ENTRER ne fait rien tant que le repaire est fermé — parce que la zone est verrouillée")]
        public void Enter_WhileLocked_DoesNothing()
        {
            var p = Page();
            _view.ClickEnter();
            Assert.That(_host.Spawned.OfType<FakeConfirmPopup>().Any(), Is.False);
            p.Dispose();
        }
    }
}
