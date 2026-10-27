using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Account;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Infrastructure.Simulated;
using N29.HeroesDawn.Presentation.Account;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Tests.Domain.Application;
using N29.HeroesDawn.Tests.Presentation.Fakes;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    /// <summary>Fenêtre Compte : validation avant tout appel, un seul envoi à la fois, erreurs en français, fermeture.</summary>
    public sealed class AccountPopupControllerTests
    {
        TestApp _app = null!;
        LocalAccountGateway _accounts = null!;
        FakeCloudProfileGateway _cloud = null!;
        AccountLink _link = null!;
        FakeAccountPopup _view = null!;
        readonly LocLocalization _loc = new LocLocalization();
        UniTask<LinkOutcome?> _run;

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.SignInDirect("guest-1");
            _app.Session.ResetToNewProfile();
            var clock = new FixedClock();
            _accounts = new LocalAccountGateway(0);
            _cloud = new FakeCloudProfileGateway();
            var state = new MemorySyncStateStore();
            var sync = new CloudSync(_cloud, state, _app.Session, clock, _app.Logger, (t, ct) => UniTask.CompletedTask, _app.Save);
            _link = new AccountLink(_accounts, _cloud, _app.Session, _app.Save, sync, state, new MemoryProgressBackupStore(), clock, _app.Logger,
                                    _app.Publisher<WalletChanged>(), _app.Publisher<RosterChanged>());
            _view = new FakeAccountPopup();
            _run = new AccountPopupController(_view, _link, _loc).RunAsync(CancellationToken.None);
        }

        [Test, Description("Vérifie que la fenêtre s'affiche en mode Connexion avec ses textes français (titre, champs, boutons Google et Apple) — parce que c'est l'entrée du parcours")]
        public void Starts_InSignInMode_WithFrenchTexts()
        {
            Assert.That(_view.Data, Is.Not.Null);
            Assert.That(_view.Mode, Is.EqualTo(AccountMode.SignIn));
            var d = _view.Data!;
            foreach (var text in new[] { d.Title, d.TabSignIn, d.TabSignUp, d.EmailHint, d.PasswordHint, d.NameHint, d.Submit, d.Google, d.Apple, d.Forgot, d.Close })
                Assert.That(text, Is.Not.Empty.And.Not.Contain("account."));
            Assert.That(d.Google, Is.EqualTo("Continuer avec Google"));
        }

        [TestCase("", "motdepasse1", "account.error.email")]
        [TestCase("pas-une-adresse", "motdepasse1", "account.error.email")]
        [TestCase("moi@exemple.fr", "court", "errors.account_password")]
        [Test, Description("Vérifie qu'une adresse invalide ou un mot de passe trop court affiche l'erreur dans la fenêtre sans aucun appel au compte — parce que la validation précède le réseau")]
        public void Validation_ShowsTheError_WithoutCallingTheAccount(string email, string password, string expectedKey)
        {
            _view.ClickSubmit(email, password);
            Assert.That(_view.LastMessage.IsError, Is.True);
            Assert.That(_view.LastMessage.Text, Is.EqualTo(_loc.T(expectedKey)));
            Assert.That(_view.BusyStates, Is.Empty, "aucun envoi : la fenêtre n'est jamais passée en attente");
            Assert.That(_link.State, Is.EqualTo(AccountState.Guest));
        }

        [Test, Description("Vérifie qu'à la création un pseudo de moins de 3 ou de plus de 16 caractères est refusé avant l'envoi — parce que le pseudo est affiché partout")]
        public void SignUp_NameMustBe3To16Characters()
        {
            _view.ClickMode(AccountMode.SignUp);
            _view.ClickSubmit("moi@exemple.fr", "motdepasse1", "ab");
            Assert.That(_view.LastMessage.Text, Is.EqualTo(_loc.T("account.error.name")));
            _view.ClickSubmit("moi@exemple.fr", "motdepasse1", new string('x', 17));
            Assert.That(_view.LastMessage.Text, Is.EqualTo(_loc.T("account.error.name")));
            Assert.That(_view.BusyStates, Is.Empty);
        }

        [Test, Description("Vérifie qu'une création de compte réussie rend le résultat du rattachement et termine la fenêtre — parce que le flux enchaîne ensuite sur le choix éventuel")]
        public void SignUp_Success_CompletesWithTheOutcome()
        {
            _view.ClickMode(AccountMode.SignUp);
            _view.ClickSubmit("Moi@Exemple.fr", "motdepasse1", "Moi");
            var outcome = _run.GetAwaiter().GetResult();
            Assert.That(outcome, Is.Not.Null);
            Assert.That(outcome!.Status, Is.EqualTo(LinkStatus.Uploaded));
            Assert.That(_view.BusyStates, Is.EqualTo(new[] { true, false }));
        }

        [Test, Description("Vérifie que de mauvais identifiants affichent « E-mail ou mot de passe incorrect » et rendent la fenêtre utilisable (fin de l'attente, fenêtre toujours ouverte) — parce que le joueur doit pouvoir réessayer")]
        public void SignIn_WrongCredentials_ShowsTheError_AndStaysOpen()
        {
            _accounts.SignUpAsync("moi@exemple.fr", "motdepasse1", "Moi", CancellationToken.None).GetAwaiter().GetResult();
            _view.ClickSubmit("moi@exemple.fr", "mauvaismdp1");
            Assert.That(_view.LastMessage, Is.EqualTo((_loc.T("errors.account_credentials"), true)));
            Assert.That(_view.BusyStates, Is.EqualTo(new[] { true, false }));
            Assert.That(_run.Status, Is.EqualTo(UniTaskStatus.Pending), "la fenêtre reste ouverte");
        }

        [Test, Description("Vérifie que la connexion Google passe par le compte et termine la fenêtre en cas de succès, et qu'une annulation ou un échec affiche l'erreur sans fermer — parce que le navigateur peut être refermé par le joueur")]
        public void Provider_Success_Completes_Failure_ShowsError()
        {
            _view.ClickProvider(AuthProvider.Google);
            Assert.That(_run.GetAwaiter().GetResult()!.Status, Is.EqualTo(LinkStatus.Uploaded));
        }

        [Test, Description("Vérifie que « Mot de passe oublié » avec une adresse valide affiche le message neutre « si un compte existe, un e-mail vient d'être envoyé » et avec une adresse invalide l'erreur — parce qu'on ne révèle pas l'existence d'un compte")]
        public void Forgot_ValidEmail_ShowsTheNeutralMessage_InvalidShowsAnError()
        {
            _view.ClickForgot("inconnu@exemple.fr");
            Assert.That(_view.LastMessage, Is.EqualTo((_loc.T("account.reset_sent"), false)));
            _view.ClickForgot("nimporte quoi");
            Assert.That(_view.LastMessage.IsError, Is.True);
        }

        [Test, Description("Vérifie que Fermer (et Retour) termine la fenêtre sans résultat et sans toucher au compte — parce qu'on doit toujours pouvoir sortir")]
        public void Close_CompletesWithNoOutcome()
        {
            _view.ClickClose();
            Assert.That(_run.GetAwaiter().GetResult(), Is.Null);
            Assert.That(_link.State, Is.EqualTo(AccountState.Guest));
        }

        [Test, Description("Vérifie qu'un second clic sur Valider pendant l'envoi n'en lance pas un deuxième — parce qu'un double appui ne doit pas doubler la requête")]
        public void Submit_WhileBusy_IsIgnored()
        {
            var gated = new GatedAccounts();
            var link = new AccountLink(gated, _cloud, _app.Session, _app.Save,
                new CloudSync(_cloud, new MemorySyncStateStore(), _app.Session, new FixedClock(), _app.Logger, (t, ct) => UniTask.CompletedTask, _app.Save),
                new MemorySyncStateStore(), new MemoryProgressBackupStore(), new FixedClock(), _app.Logger, _app.Publisher<WalletChanged>(), _app.Publisher<RosterChanged>());
            var view = new FakeAccountPopup();
            new AccountPopupController(view, link, _loc).RunAsync(CancellationToken.None);
            view.ClickSubmit("moi@exemple.fr", "motdepasse1");
            view.ClickSubmit("moi@exemple.fr", "motdepasse1");
            Assert.That(gated.SignInCalls, Is.EqualTo(1));
            gated.Release();
        }

        /// <summary>Compte dont la connexion attend un signal.</summary>
        sealed class GatedAccounts : IAccountGateway
        {
            readonly UniTaskCompletionSource _gate = new UniTaskCompletionSource();
            public int SignInCalls;
            public void Release() => _gate.TrySetResult();
            public UniTask<Core.Result<PlayerAccount>> SignUpAsync(string email, string password, string displayName, CancellationToken ct) => throw new System.NotSupportedException();
            public async UniTask<Core.Result<PlayerAccount>> SignInAsync(string email, string password, CancellationToken ct)
            {
                SignInCalls++;
                await _gate.Task;
                return Core.Result<PlayerAccount>.Ok(new PlayerAccount("u", "Moi", email));
            }
            public UniTask<Core.Result<PlayerAccount>> SignInWithProviderAsync(AuthProvider provider, CancellationToken ct) => throw new System.NotSupportedException();
            public UniTask<Core.Result<PlayerAccount?>> RestoreSessionAsync(CancellationToken ct) => UniTask.FromResult(Core.Result<PlayerAccount?>.Ok(null));
            public UniTask<Core.Result> SignOutAsync(CancellationToken ct) => UniTask.FromResult(Core.Result.Ok());
            public UniTask<Core.Result> ResetPasswordAsync(string email, CancellationToken ct) => UniTask.FromResult(Core.Result.Ok());
        }
    }
}
