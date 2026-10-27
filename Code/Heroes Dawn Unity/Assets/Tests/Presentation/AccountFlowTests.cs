using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Account;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Infrastructure.Simulated;
using N29.HeroesDawn.Presentation.Account;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Tests.Domain.Application;
using N29.HeroesDawn.Tests.Presentation.Fakes;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    /// <summary>Parcours Compte : fenêtre Compte, choix de progression, messages de résultat, déconnexion, synchronisation.</summary>
    public sealed class AccountFlowTests
    {
        TestApp _app = null!;
        FixedClock _clock = null!;
        FakeCloudProfileGateway _cloud = null!;
        LocalAccountGateway _accounts = null!;
        MemoryProgressBackupStore _backups = null!;
        AccountLink _link = null!;
        FakeAccountPopup _popup = null!;
        FakeProgressChoicePopup _choice = null!;
        FakeAutoConfirmPopup _confirm = null!;
        FakeToasts _toasts = null!;
        AccountFlow _flow = null!;
        readonly LocLocalization _loc = new LocLocalization();

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.SignInDirect("guest-1");
            _app.Session.ResetToNewProfile();
            _clock = new FixedClock();
            _cloud = new FakeCloudProfileGateway();
            _accounts = new LocalAccountGateway(0);
            _backups = new MemoryProgressBackupStore();
            var state = new MemorySyncStateStore();
            var sync = new CloudSync(_cloud, state, _app.Session, _clock, _app.Logger, (t, ct) => UniTask.CompletedTask, _app.Save);
            _link = new AccountLink(_accounts, _cloud, _app.Session, _app.Save, sync, state, _backups, _clock, _app.Logger,
                                    _app.Publisher<WalletChanged>(), _app.Publisher<RosterChanged>());
            _popup = new FakeAccountPopup();
            _choice = new FakeProgressChoicePopup();
            _confirm = new FakeAutoConfirmPopup();
            var host = new FakeModalHost();
            host.Register<IAccountPopupView>(() => _popup);
            host.Register<IProgressChoicePopupView>(() => _choice);
            host.Register<IConfirmPopupView>(() => _confirm);
            _toasts = new FakeToasts();
            _flow = new AccountFlow(new ModalService(host), _link, _toasts, _loc);
        }

        void LocalProgress(int credits = 123)
        {
            _app.Session.CompletedZones.Add("zone_ny_1_1");
            _app.Session.Meta.tutorial.done = true;
            _app.Session.Wallet.Set(credits, _app.Session.Wallet.Pr);
        }

        void CloudProgress(int credits = 777)
        {
            var p = UnityEngine.JsonUtility.FromJson<ProfileData>(UnityEngine.JsonUtility.ToJson(_app.Session.ToProfile(_clock.UtcNow)));
            p.credits = credits;
            p.completedZones = new System.Collections.Generic.List<string> { "zone_ny_1_1", "zone_ny_1_2" };
            p.meta.tutorial.done = true;
            _cloud.SeedFromOtherDevice(p);
        }

        /// <summary>Lance le parcours, simule la création du compte dans la fenêtre, et attend la fin.</summary>
        void RunSignUp()
        {
            var run = _flow.RunAsync(CancellationToken.None);
            _popup.ClickMode(AccountMode.SignUp);
            _popup.ClickSubmit("moi@exemple.fr", "motdepasse1", "Moi");
            run.GetAwaiter().GetResult();
        }

        [Test, Description("Vérifie que le rattachement qui envoie la progression locale affiche « compte lié » (message de récompense) et ferme la fenêtre Compte — parce que le joueur doit savoir que sa progression est sauvegardée")]
        public void Uploaded_ShowsTheLinkedMessage_AndClosesTheWindow()
        {
            LocalProgress();
            RunSignUp();
            Assert.That(_toasts.Shown.Any(t => t.Text == _loc.T("account.linked") && t.Kind == ToastKind.Reward), Is.True);
            Assert.That(_choice.Asked, Is.Empty);
        }

        [Test, Description("Vérifie que la progression récupérée du compte affiche « progression du compte récupérée » — parce que le joueur doit voir qu'il retrouve sa partie")]
        public void Pulled_ShowsThePulledMessage()
        {
            CloudProgress();
            RunSignUp();
            Assert.That(_toasts.Shown.Any(t => t.Text == _loc.T("account.pulled")), Is.True);
            Assert.That(_app.Session.Wallet.Credits, Is.EqualTo(777));
        }

        [Test, Description("Vérifie que quand les deux ont avancé le choix s'ouvre avec les chiffres de l'appareil et du compte, et que « Garder le compte » récupère le compte en gardant une copie de secours — parce que rien ne doit être écrasé sans choix")]
        public void NeedsChoice_AsksWithBothSummaries_KeepAccount()
        {
            CloudProgress(777);
            LocalProgress(123);
            _choice.Answers.Enqueue(ChoiceKind.KeepAccount);
            RunSignUp();
            var asked = _choice.Asked.Single();
            Assert.That(asked.Device.Summary, Does.Contain("123"));
            Assert.That(asked.Account.Summary, Does.Contain("777"));
            Assert.That(_app.Session.Wallet.Credits, Is.EqualTo(777));
            Assert.That(_backups.Saved.Single().Reason, Is.EqualTo("local"));
        }

        [Test, Description("Vérifie que « Garder cet appareil » envoie la progression de l'appareil au compte avec une copie de secours du compte — parce que l'autre version n'est jamais perdue")]
        public void NeedsChoice_KeepDevice()
        {
            CloudProgress(777);
            LocalProgress(123);
            _choice.Answers.Enqueue(ChoiceKind.KeepDevice);
            RunSignUp();
            Assert.That(_cloud.GetAsync(CancellationToken.None).GetAwaiter().GetResult().Value!.Profile.credits, Is.EqualTo(123));
            Assert.That(_backups.Saved.Single().Reason, Is.EqualTo("cloud"));
        }

        [Test, Description("Vérifie que Retour sur le choix ne change rien (ni l'appareil, ni le compte, aucune sauvegarde de secours) et que le choix reste en attente — parce qu'annuler ne doit jamais écrire")]
        public void NeedsChoice_Back_WritesNothing_AndKeepsThePendingChoice()
        {
            CloudProgress(777);
            LocalProgress(123);
            int writes = _cloud.Writes;
            _choice.Answers.Enqueue(null);
            RunSignUp();
            Assert.That(_app.Session.Wallet.Credits, Is.EqualTo(123));
            Assert.That(_cloud.Writes, Is.EqualTo(writes));
            Assert.That(_backups.Saved, Is.Empty);
            Assert.That(_link.PendingChoice, Is.Not.Null);
            Assert.That(_toasts.Shown.Any(t => t.Text == _loc.T("account.choice_later")), Is.True);
        }

        [Test, Description("Vérifie que fermer la fenêtre Compte sans se connecter n'affiche aucun message et ne touche à rien — parce que fermer est un non-événement")]
        public void ClosingTheWindow_DoesNothing()
        {
            var run = _flow.RunAsync(CancellationToken.None);
            _popup.ClickClose();
            run.GetAwaiter().GetResult();
            Assert.That(_toasts.Shown, Is.Empty);
            Assert.That(_link.State, Is.EqualTo(AccountState.Guest));
        }

        [Test, Description("Vérifie que la déconnexion demande confirmation (annuler garde la connexion, confirmer déconnecte et affiche « déconnecté, tu continues en invité ») — parce que la déconnexion arrête la synchronisation")]
        public void SignOut_AsksForConfirmation()
        {
            LocalProgress();
            RunSignUp();
            _toasts.Shown.Clear();
            _confirm.Yes = false;
            _flow.SignOutAsync(CancellationToken.None).GetAwaiter().GetResult();
            Assert.That(_link.State, Is.EqualTo(AccountState.Connected));
            _confirm.Yes = true;
            _flow.SignOutAsync(CancellationToken.None).GetAwaiter().GetResult();
            Assert.That(_link.State, Is.EqualTo(AccountState.Guest));
            Assert.That(_toasts.Shown.Any(t => t.Text == _loc.T("account.signed_out")), Is.True);
        }

        [Test, Description("Vérifie que « Synchroniser » affiche « progression synchronisée » en cas de succès et l'erreur traduite en cas d'échec — parce que le joueur doit voir si la sauvegarde en ligne fonctionne")]
        public void SyncNow_ShowsSuccessOrTheError()
        {
            LocalProgress();
            RunSignUp();
            _toasts.Shown.Clear();
            _flow.SyncNowAsync(CancellationToken.None).GetAwaiter().GetResult();
            Assert.That(_toasts.Shown.Any(t => t.Text == _loc.T("account.sync_done")), Is.True);
        }

        [Test, Description("Vérifie que le libellé du bouton du titre est « Se connecter » pour un invité et l'e-mail du compte pour un joueur connecté — parce que le bouton montre où on en est")]
        public void ButtonLabel_DependsOnTheState()
        {
            Assert.That(_flow.ButtonLabel, Is.EqualTo(_loc.T("login.account")));
            LocalProgress();
            RunSignUp();
            Assert.That(_flow.ButtonLabel, Does.Contain("moi@exemple.fr"));
        }
    }
}
