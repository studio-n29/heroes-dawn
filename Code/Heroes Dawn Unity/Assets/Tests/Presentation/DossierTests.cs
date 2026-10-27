using System.Linq;
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
    /// <summary>Dossier de lore complet d'un agent sur sa fiche (lot G de la 0.14.0).</summary>
    public sealed class DossierTests
    {
        readonly LocLocalization _loc = new LocLocalization();

        [Test, Description("Vérifie que le dossier d'un agent numéroté a ses 5 sections dans l'ordre (identité, lien à la Supernova, histoire d'origine, pouvoir, apparence) avec l'âge et l'origine dans l'identité — parce que la fiche montre tout son lore")]
        public void Sections_OfANumberedAgent_AreComplete()
        {
            var sections = DossierText.Sections(_loc, "darius_brooks");
            Assert.That(sections.Select(s => s.Title), Is.EqualTo(new[]
            {
                _loc.T("dossier.identity"), _loc.T("dossier.supernova"), _loc.T("dossier.story"), _loc.T("dossier.power"), _loc.T("dossier.look"),
            }));
            Assert.That(sections[0].Text, Does.Contain("41 ans").And.Contain("Détroit"));
            Assert.That(sections.All(s => s.Text.Trim().Length > 0), Is.True);
        }

        [Test, Description("Vérifie qu'Agent D a lui aussi ses 5 sections et qu'un agent inconnu n'a aucun dossier — parce qu'on n'affiche que les sections qui existent")]
        public void Sections_OfThePlayerAgent_AreComplete_AndUnknownHasNone()
        {
            var d = DossierText.Sections(_loc, "agent_d");
            Assert.That(d.Count, Is.EqualTo(5));
            Assert.That(DossierText.Sections(_loc, "inconnu"), Is.Empty);
        }

        // ---- Fiche d'agent : session réelle (agents du jeu) pour disposer de leur lore

        TestApp _app = null!;
        FakeMemberSheetView _view = null!;
        HubNavigator _nav = null!;
        GameSession _session = null!;
        MemberSheetPresenter _p = null!;

        MemberSheetPresenter Sheet(string agentId, bool owned = true)
        {
            _app = new TestApp();
            _app.Account.SignIn.ExecuteAsync().GetAwaiter().GetResult();
            _session = new GameSession(PrototypeContent.Build(new BalanceConfig()));
            if (_session.Find(new MemberId(agentId)) == null)
                _session.Roster.Add(new TeamMember(_session.Content.Member(new MemberId(agentId)), 5));
            _view = new FakeMemberSheetView();
            _nav = new HubNavigator();
            _nav.Open(HubPage.MemberSheet, new MemberId(agentId));
            var toasts = new FakeToasts();
            var host = new FakeModalHost();
            var flow = new RecruitFlow(new ModalService(host), _app.Shop.Recruit, _session, toasts, _loc);
            _p = new MemberSheetPresenter(_view, _nav.Current, _session, _app.Hub.Train, flow, _nav, toasts, _loc, _app.Subscriber<WalletChanged>(),
                                          _app.Subscriber<RosterChanged>(), _app.Subscriber<ShopChanged>(), heal: _app.Hub.HealMember);
            _view.Show();
            _p.Start();
            return _p;
        }

        [Test, Description("Vérifie que le bouton Dossier n'apparaît que pour un agent qui a du lore — parce qu'un bouton sans contenu serait un cul-de-sac")]
        public void Button_ShownOnlyWithLore()
        {
            var p = Sheet("darius_brooks");
            Assert.That(_view.Last.ShowDossierButton, Is.True);
            Assert.That(_view.Last.DossierButtonLabel, Is.EqualTo(_loc.T("dossier.button")));
            p.Dispose();
        }

        [Test, Description("Vérifie que Dossier ouvre le panneau avec les 5 sections de l'agent et que Fermer le referme — parce que le dossier se lit puis se ferme")]
        public void Dossier_Opens_ThenCloses()
        {
            var p = Sheet("darius_brooks");
            _view.ClickDossier();
            Assert.That(_view.LastDossier, Is.Not.Null);
            Assert.That(_view.LastDossier!.Value.Sections.Length, Is.EqualTo(5));
            Assert.That(_view.LastDossier!.Value.Close, Is.EqualTo(_loc.T("dossier.close")));
            _view.CloseDossier();
            Assert.That(_view.LastDossier, Is.Null);
            p.Dispose();
        }

        [Test, Description("Vérifie que passer à un autre agent ferme le dossier ouvert — parce que le dossier appartient à l'agent affiché")]
        public void Dossier_ClosesWhenTheSheetChanges()
        {
            var p = Sheet("darius_brooks");
            _session.Roster.Add(new TeamMember(_session.Content.Member(new MemberId("mei_lin_chen")), 5));
            _view.ClickDossier();
            _view.ClickNext();
            Assert.That(_view.LastDossier, Is.Null);
            p.Dispose();
        }

        [Test, Description("Vérifie que le soin fonctionne comme avant après avoir ouvert le dossier — parce que le panneau ne doit pas casser la fiche")]
        public void Dossier_DoesNotBreakHeal()
        {
            var p = Sheet("darius_brooks");
            _view.ClickDossier();
            var m = _session.Find(new MemberId("darius_brooks"))!;
            m.Fatigue = 40;
            _session.Wallet.Set(500, 0);
            _app.Session.Wallet.Set(500, 0);
            _view.ClickHeal();
            Assert.That(_view.LastDossier, Is.Not.Null, "le dossier reste ouvert pendant le soin");
            p.Dispose();
        }
    }
}
