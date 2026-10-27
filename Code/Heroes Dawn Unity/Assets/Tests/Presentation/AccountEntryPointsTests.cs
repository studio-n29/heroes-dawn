using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Account;
using N29.HeroesDawn.Application.Features.Auth;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Infrastructure.Simulated;
using N29.HeroesDawn.Presentation.Account;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Login;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Tests.Domain.Application;
using N29.HeroesDawn.Tests.Presentation.Fakes;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    /// <summary>Les deux portes d'entrée du compte : le bouton « Se connecter » de l'écran titre et le bandeau « Compte » du profil.</summary>
    public sealed class AccountEntryPointsTests
    {
        /// <summary>Parcours Compte factice : compte les ouvertures, reste ouvert tant que le test ne le termine pas.</summary>
        sealed class FakeFlow : IAccountFlow
        {
            public string ButtonLabel { get; set; } = "SE CONNECTER";
            public int Runs, SignOuts, Syncs;
            public UniTaskCompletionSource? Open;
            public async UniTask RunAsync(CancellationToken ct) { Runs++; if (Open != null) await Open.Task; }
            public UniTask SignOutAsync(CancellationToken ct) { SignOuts++; return UniTask.CompletedTask; }
            public UniTask SyncNowAsync(CancellationToken ct) { Syncs++; return UniTask.CompletedTask; }
        }

        sealed class NoSignIn : ISignInAsGuestUseCase
        {
            public UniTask<Result<SignInReport>> ExecuteAsync(CancellationToken ct = default) => UniTask.FromResult(Result<SignInReport>.Ok(new SignInReport(false, 0)));
        }

        readonly LocLocalization _loc = new LocLocalization();

        // ------------------------------------------------------------------ Écran titre

        (LoginPresenter Presenter, FakeLoginView View, FakeFlow Flow) MakeLogin()
        {
            var app = new TestApp();
            var view = new FakeLoginView();
            var flow = new FakeFlow();
            var p = new LoginPresenter(view, new NoSignIn(), app.Session, app.Publisher<NavigateRequested>(), new FakeToasts(), _loc, app.Logger, account: flow);
            view.Show();
            p.Start();
            return (p, view, flow);
        }

        [Test, Description("Vérifie que le titre affiche le libellé du bouton Compte à l'apparition de l'écran — parce que le bouton dit « Se connecter » ou montre le compte")]
        public void Login_ShowsTheAccountButtonLabel()
        {
            var (_, view, flow) = MakeLogin();
            Assert.That(view.AccountLabel, Is.EqualTo(flow.ButtonLabel));
        }

        [Test, Description("Vérifie qu'un clic sur « Se connecter » ouvre le parcours Compte, qu'un second clic pendant qu'il est ouvert n'en ouvre pas un deuxième, et que le libellé est rafraîchi à la fin — parce qu'un double appui ne doit pas empiler deux fenêtres")]
        public void Login_AccountClick_OpensOnce_AndRefreshesTheLabel()
        {
            var (_, view, flow) = MakeLogin();
            flow.Open = new UniTaskCompletionSource();
            view.ClickAccount();
            view.ClickAccount();
            Assert.That(flow.Runs, Is.EqualTo(1));
            flow.ButtonLabel = "moi@exemple.fr";
            flow.Open.TrySetResult();
            Assert.That(view.AccountLabel, Is.EqualTo("moi@exemple.fr"));
            view.ClickAccount();
            Assert.That(flow.Runs, Is.EqualTo(2), "rouvrable une fois fermé");
        }

        // ------------------------------------------------------------------ Profil

        (TestApp App, FakeListView View, AccountLink Link, FakeFlow Flow) MakeProfile(bool withAccount = true)
        {
            var app = new TestApp();
            app.SignInDirect("guest-1");
            var clock = new FixedClock();
            var cloud = new FakeCloudProfileGateway();
            var state = new MemorySyncStateStore();
            var sync = new CloudSync(cloud, state, app.Session, clock, app.Logger, (t, ct) => UniTask.CompletedTask, app.Save);
            var link = new AccountLink(new LocalAccountGateway(0), cloud, app.Session, app.Save, sync, state, new MemoryProgressBackupStore(), clock, app.Logger,
                                       app.Publisher<WalletChanged>(), app.Publisher<RosterChanged>());
            var flow = new FakeFlow();
            var view = new FakeListView();
            var p = new ProfilePagePresenter(view, app.Profile, app.Friends, new HubNavigator(), app.Session, _loc, new FakeToasts(), app.Subscriber<MetaChanged>(),
                                             app.Subscriber<WalletChanged>(), app.Subscriber<RosterChanged>(), app.Time, new ModalService(new FakeModalHost()),
                                             withAccount ? link : null, withAccount ? flow : null);
            view.Show();
            ((VContainer.Unity.IStartable)p).Start();
            return (app, view, link, flow);
        }

        [Test, Description("Vérifie que le profil d'un invité montre en premier la rangée « Compte invité » avec le bouton « Lier mon compte » qui ouvre le parcours — parce que c'est l'invitation à sauvegarder en ligne")]
        public void Profile_Guest_ShowsTheLinkRow()
        {
            var (_, view, _, flow) = MakeProfile();
            var row = view.Rows[0];
            Assert.That(row.Title, Is.EqualTo(_loc.T("profile.account.guest")));
            Assert.That(row.A.Label, Is.EqualTo(_loc.T("profile.account.link")));
            view.ClickRow(0, 0);
            Assert.That(flow.Runs, Is.EqualTo(1));
        }

        [Test, Description("Vérifie que le profil d'un joueur connecté montre son e-mail et son fournisseur avec « Synchroniser » et « Se déconnecter » qui appellent chacun le bon parcours — parce que c'est la gestion du compte")]
        public void Profile_Connected_ShowsTheAccount_SyncAndSignOut()
        {
            var (app, view, link, flow) = MakeProfile();
            link.SignUpAsync("moi@exemple.fr", "motdepasse1", "Moi", CancellationToken.None).GetAwaiter().GetResult();
            app.Publisher<MetaChanged>().Publish(default);   // le profil se redessine à chaque changement
            var row = view.Rows[0];
            Assert.That(row.Title, Does.Contain("moi@exemple.fr"));
            Assert.That(row.Subtitle, Does.Contain(_loc.T("account.provider.email")));
            Assert.That(row.A.Label, Is.EqualTo(_loc.T("profile.account.sync")));
            Assert.That(row.B.Label, Is.EqualTo(_loc.T("profile.account.signout")));
            view.ClickRow(0, 0);
            view.ClickRow(0, 1);
            Assert.That((flow.Syncs, flow.SignOuts), Is.EqualTo((1, 1)));
        }

        [Test, Description("Vérifie que sans compte fourni (anciennes configurations) le profil n'ajoute aucune rangée Compte — parce que les écrans existants ne doivent pas changer")]
        public void Profile_WithoutAccount_HasNoAccountRow()
        {
            var (_, view, _, _) = MakeProfile(withAccount: false);
            Assert.That(view.Rows.Any(r => r.Title == _loc.T("profile.account.guest")), Is.False);
        }
    }
}
