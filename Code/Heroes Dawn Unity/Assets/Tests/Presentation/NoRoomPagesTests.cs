using System;
using System.IO;
using System.Linq;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Tests.Domain.Application;
using N29.HeroesDawn.Tests.Presentation.Fakes;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    /// <summary>Les pages Salon et Infirmerie n'existent plus : tout se passe sur la carte de l'agent (lot F de la 0.14.0).</summary>
    public sealed class NoRoomPagesTests
    {
        [Test, Description("Vérifie que l'énumération des pages n'a plus Salon ni Infirmerie et qu'aucun fichier de code ne les cite encore — parce que les deux pages et leurs vues ont été supprimées")]
        public void NoRoomPages_Anywhere()
        {
            var names = Enum.GetNames(typeof(HubPage));
            Assert.That(names, Does.Not.Contain("Salon"));
            Assert.That(names, Does.Not.Contain("Infirmary"));

            string assets = UnityEngine.Application.dataPath;
            string self = nameof(NoRoomPagesTests) + ".cs";
            string[] banned = { "HubPage.Salon", "HubPage.Infirmary", "LoungePage", "SalonPage", "InfirmaryPage" };
            var offenders = Directory.GetFiles(assets, "*.cs", SearchOption.AllDirectories)
                .Where(f => !f.EndsWith(self, StringComparison.Ordinal))
                .Where(f => { string text = File.ReadAllText(f); return banned.Any(text.Contains); })
                .Select(f => f.Substring(assets.Length)).ToList();
            Assert.That(offenders, Is.Empty, string.Join(", ", offenders));
        }

        [Test, Description("Vérifie qu'aucun texte d'aide ne renvoie vers l'Infirmerie supprimée et que le bouton d'équipe dit « Soigner l'équipe » — parce que les soins passent par la fiche d'agent et la page Équipe")]
        public void Texts_NoLongerSendToTheInfirmary()
        {
            var loc = new LocLocalization();
            foreach (var key in new[] { "tutorial.hint.fatigue", "hub.deploy.exhausted", "hub.healed", "tutorial.hint.infirmary" })
                Assert.That(loc.T(key), Does.Not.Contain("nfirmerie"), key);
            Assert.That(loc.F("hub.heal", "150 CR"), Does.Contain("ÉQUIPE").And.Contain("150 CR"));
        }

        [Test, Description("Vérifie que l'astuce « infirmerie » du tutoriel se déclenche à l'ouverture de la page Équipe quand un agent est fatigué — parce que le soin se fait maintenant depuis les agents")]
        public void Tutorial_InfirmaryHint_PointsToTheTeamPage()
        {
            var app = new TestApp();
            app.Account.SignIn.ExecuteAsync().GetAwaiter().GetResult();
            app.Tutorial.Skip();
            app.Session.Roster[0].Fatigue = 40;
            var nav = new HubNavigator();
            var view = new FakeHubShellView();
            var modals = new ModalService(new FakeModalHost());
            var shell = new HubShellPresenter(view, nav, new FakePageFactory(), app.Session, app.Subscriber<WalletChanged>(), app.Subscriber<SaveStateChanged>(),
                                              app.Subscriber<ShopChanged>(), app.Shop.Open, app.Time, modals, new FakeToasts(), new LocLocalization(), new FakeAppQuit(),
                                              new BackButtonRouter(new FakeBackInput(), modals), null, null, app.Tutorial);
            var raised = new System.Collections.Generic.List<string>();
            app.Tutorial.HintRaised += raised.Add;
            view.Show();
            shell.Start();
            nav.SelectTab(HubPage.Team);
            Assert.That(raised, Does.Contain("infirmary"));
            Assert.That(new LocLocalization().T("tutorial.hint.infirmary"), Does.Contain("fiche"));
            shell.Dispose();
        }
    }
}
