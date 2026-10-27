using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Account;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Infrastructure.Simulated;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    /// <summary>Garde-fous issus de la relecture du lot I : connexion avant « Jouer », synchronisation inactive, schéma, profil illisible, session perdue, opérations concurrentes.</summary>
    public sealed class AccountLinkSafetyTests
    {
        static readonly CancellationToken Ct = CancellationToken.None;

        TestApp _app = null!;
        FixedClock _clock = null!;
        FakeCloudProfileGateway _cloud = null!;
        LocalAccountGateway _accounts = null!;
        MemorySyncStateStore _state = null!;
        MemoryProgressBackupStore _backups = null!;
        CloudSync _sync = null!;
        AccountLink _link = null!;

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.SignInDirect("guest-1");
            _app.Session.ResetToNewProfile();
            _clock = new FixedClock();
            _cloud = new FakeCloudProfileGateway();
            _accounts = new LocalAccountGateway(0);
            _state = new MemorySyncStateStore();
            _backups = new MemoryProgressBackupStore();
            Rebuild(_accounts);
        }

        void Rebuild(IAccountGateway accounts)
        {
            _sync = new CloudSync(_cloud, _state, _app.Session, _clock, _app.Logger, (t, ct) => { _clock.Advance(t); return UniTask.CompletedTask; }, _app.Save);
            _link = new AccountLink(accounts, _cloud, _app.Session, _app.Save, _sync, _state, _backups, _clock, _app.Logger,
                                    _app.Publisher<WalletChanged>(), _app.Publisher<RosterChanged>());
        }

        ProfileData CloudProfile(int credits, int schema = 0, string agentId = "")
        {
            var p = UnityEngine.JsonUtility.FromJson<ProfileData>(UnityEngine.JsonUtility.ToJson(_app.Session.ToProfile(_clock.UtcNow)));
            p.credits = credits;
            p.completedZones = new List<string> { "zone_ny_1_1", "zone_ny_1_2" };
            p.meta.tutorial.done = true;
            if (schema > 0) p.schemaVersion = schema;
            if (agentId != "") foreach (var m in p.team) m.id = agentId;
            return p;
        }

        void LocalProgress(int credits = 123)
        {
            _app.Session.CompletedZones.Add("zone_ny_1_1");
            _app.Session.Meta.tutorial.done = true;
            _app.Session.Wallet.Set(credits, _app.Session.Wallet.Pr);
        }

        // ------------------------------------------------------------------ C1

        [Test, Description("Vérifie que se connecter depuis le titre, avant « Jouer » (aucun profil local chargé), authentifie seulement : le compte du cloud n'est ni lu ni écrit ni écrasé, et le vrai rattachement a lieu à la restauration après Jouer — parce qu'un profil neuf en mémoire ne doit jamais remplacer la progression du compte")]
        public void Link_BeforeTheGuestSignIn_OnlyAuthenticates_ThenRestoreLinksForReal()
        {
            _cloud.SeedFromOtherDevice(CloudProfile(777));
            _app.Session.Account = null;                 // au titre : la session de jeu n'est pas encore chargée
            int writes = _cloud.Writes, reads = _cloud.Reads;
            var outcome = _link.SignUpAsync("moi@exemple.fr", "motdepasse1", "Moi", Ct).GetAwaiter().GetResult().Value!;
            Assert.That(outcome.Status, Is.EqualTo(LinkStatus.Connected));
            Assert.That(_link.State, Is.EqualTo(AccountState.Connected));
            Assert.That((_cloud.Reads, _cloud.Writes), Is.EqualTo((reads, writes)), "le cloud n'a pas été touché");
            Assert.That(_sync.IsActive, Is.False);

            // Le joueur appuie sur Jouer : le profil local est chargé, puis la restauration rattache vraiment.
            _app.SignInDirect("guest-1");
            LocalProgress(123);
            _app.Save.SaveAsync(Ct).GetAwaiter().GetResult();
            Assert.That(_cloud.GetAsync(Ct).GetAwaiter().GetResult().Value!.Profile.credits, Is.EqualTo(777), "la sauvegarde locale n'a rien envoyé");
            var restored = _link.RestoreAsync(Ct).GetAwaiter().GetResult().Value!;
            Assert.That(restored.Status, Is.EqualTo(LinkStatus.NeedsChoice), "les deux ont avancé : le choix est demandé, rien n'est écrasé");
            Assert.That(_cloud.GetAsync(Ct).GetAwaiter().GetResult().Value!.Profile.credits, Is.EqualTo(777));
        }

        // ------------------------------------------------------------------ I1

        [Test, Description("Vérifie que « Synchroniser » sans synchronisation active (panne au rattachement) relance le rattachement et ne dit pas « synchronisé » à tort — parce qu'un faux succès cache que rien n'est sauvegardé en ligne")]
        public void SyncNow_WhenInactive_RelinksInsteadOfFalselySucceeding()
        {
            LocalProgress(123);
            _cloud.FailNext = true;
            var failed = _link.SignUpAsync("moi@exemple.fr", "motdepasse1", "Moi", Ct).GetAwaiter().GetResult();
            Assert.That(failed.IsFailure, Is.True);
            Assert.That(_sync.IsActive, Is.False);
            var r = _link.SyncNowAsync(Ct).GetAwaiter().GetResult();
            Assert.That(r.IsSuccess, Is.True);
            Assert.That(_sync.IsActive, Is.True, "le rattachement a abouti");
            Assert.That(_cloud.GetAsync(Ct).GetAwaiter().GetResult().Value!.Profile.credits, Is.EqualTo(123));
        }

        [Test, Description("Vérifie que « Synchroniser » sans compte connecté rend « session expirée » et que le vidage d'une synchronisation inactive est une erreur, pas un succès — parce qu'on ne doit jamais afficher « synchronisé » quand rien n'est parti")]
        public void SyncNow_WithoutAccount_Fails_AndFlushWhenInactiveFails()
        {
            Assert.That(_link.SyncNowAsync(Ct).GetAwaiter().GetResult().IsFailure, Is.True);
            Assert.That(_sync.FlushAsync(Ct).GetAwaiter().GetResult().IsFailure, Is.True);
        }

        [Test, Description("Vérifie que « Plus tard » laisse le choix en attente et que « Synchroniser » le repropose (événement) — parce que le texte promet de pouvoir choisir plus tard depuis le profil")]
        public void SyncNow_ReopensThePendingChoice()
        {
            _cloud.SeedFromOtherDevice(CloudProfile(777));
            LocalProgress(123);
            _link.SignUpAsync("moi@exemple.fr", "motdepasse1", "Moi", Ct).GetAwaiter().GetResult();
            Assert.That(_link.PendingChoice, Is.Not.Null);
            var r = _link.SyncNowAsync(Ct).GetAwaiter().GetResult();
            Assert.That(r.Value!.Status, Is.EqualTo(LinkStatus.NeedsChoice));
        }

        // ------------------------------------------------------------------ I3

        [Test, Description("Vérifie que « Garder le compte » refuse un profil du compte devenu plus récent que le jeu (écrit par un autre appareil mis à jour après le choix proposé) et n'applique rien — parce que la version de schéma se contrôle à chaque application, pas seulement au premier rattachement")]
        public void KeepAccount_RefusesANewerSchema_AppearingAfterTheChoiceWasOffered()
        {
            _cloud.SeedFromOtherDevice(CloudProfile(777));
            LocalProgress(123);
            _link.SignUpAsync("moi@exemple.fr", "motdepasse1", "Moi", Ct).GetAwaiter().GetResult();
            _cloud.SeedFromOtherDevice(CloudProfile(888, schema: new ProfileData().schemaVersion + 1));
            _link.SyncNowAsync(Ct).GetAwaiter().GetResult();     // relit le cloud : version trop récente
            var r = _link.ResolveAsync(ChoiceKind.KeepAccount, Ct).GetAwaiter().GetResult();
            Assert.That(r.IsFailure, Is.True);
            Assert.That(_app.Session.Wallet.Credits, Is.EqualTo(123));
        }

        [Test, Description("Vérifie qu'une relecture après conflit qui trouve un schéma plus récent refuse le choix (erreur « mets le jeu à jour ») — parce que le conflit détecté par la synchronisation suit la même règle")]
        public void Conflict_WithNewerSchema_DoesNotOfferTheChoice()
        {
            LocalProgress(123);
            _link.SignUpAsync("moi@exemple.fr", "motdepasse1", "Moi", Ct).GetAwaiter().GetResult();
            _cloud.SeedFromOtherDevice(CloudProfile(999, schema: new ProfileData().schemaVersion + 1));
            LocalOutcomeCapture capture = new LocalOutcomeCapture(_link);
            _sync.NotifyLocalSaved();                           // l'envoi échoue en conflit
            Assert.That(capture.Raised, Is.EqualTo(0), "aucun choix n'est proposé pour un profil trop récent");
            Assert.That(_link.PendingChoice, Is.Null);
        }

        sealed class LocalOutcomeCapture
        {
            public int Raised;
            public LocalOutcomeCapture(AccountLink link) { link.ChoiceRequired += _ => Raised++; }
        }

        // ------------------------------------------------------------------ I4

        [Test, Description("Vérifie que récupérer le compte met toujours de côté le profil local, même « neuf » — parce qu'un profil neuf peut déjà contenir des Crédits ou des recrues achetées")]
        public void Pull_AlwaysBacksUpTheLocalProfile()
        {
            _cloud.SeedFromOtherDevice(CloudProfile(777));
            _app.Session.Wallet.Set(5000, 3);                    // « neuf » (aucune zone, tutoriel non fini) mais riche
            _link.SignUpAsync("moi@exemple.fr", "motdepasse1", "Moi", Ct).GetAwaiter().GetResult();
            Assert.That(_backups.Saved.Single().Reason, Is.EqualTo("local"));
            Assert.That(_backups.Saved.Single().Profile.credits, Is.EqualTo(5000));
        }

        [Test, Description("Vérifie qu'un profil du compte dont aucun agent n'est connu de ce build est refusé (secours du compte, synchronisation non démarrée, aucun envoi qui l'écraserait) — parce que ApplyProfile retombe sur un profil neuf dans ce cas")]
        public void Pull_UnreadableCloudProfile_IsRefused_AndNeverOverwritten()
        {
            _cloud.SeedFromOtherDevice(CloudProfile(777, agentId: "agent_inconnu_de_ce_build"));
            var r = _link.SignUpAsync("moi@exemple.fr", "motdepasse1", "Moi", Ct).GetAwaiter().GetResult();
            Assert.That(r.IsFailure, Is.True);
            Assert.That(_sync.IsActive, Is.False, "aucune synchronisation : le profil neuf ne partira pas par-dessus le compte");
            Assert.That(_backups.Saved.Any(b => b.Reason == "cloud"), Is.True);
            _app.Save.SaveAsync(Ct).GetAwaiter().GetResult();
            Assert.That(_cloud.GetAsync(Ct).GetAwaiter().GetResult().Value!.Profile.credits, Is.EqualTo(777), "le compte est intact");
        }

        // ------------------------------------------------------------------ I5

        [Test, Description("Vérifie que la perte de la session (jeton refusé même après rafraîchissement) fait repasser le joueur en invité et arrête la synchronisation — parce qu'afficher « Connecté » sans sauvegarder serait un mensonge")]
        public void SessionLost_DuringSync_TurnsTheGuestBack()
        {
            LocalProgress(123);
            _link.SignUpAsync("moi@exemple.fr", "motdepasse1", "Moi", Ct).GetAwaiter().GetResult();
            Assert.That(_link.State, Is.EqualTo(AccountState.Connected));
            var lost = new SessionLostCloud(_cloud);
            var sync = new CloudSync(lost, _state, _app.Session, _clock, _app.Logger, (t, ct) => UniTask.CompletedTask, _app.Save);
            var link = new AccountLink(_accounts, lost, _app.Session, _app.Save, sync, _state, _backups, _clock, _app.Logger,
                                       _app.Publisher<WalletChanged>(), _app.Publisher<RosterChanged>());
            link.SignInAsync("moi@exemple.fr", "motdepasse1", Ct).GetAwaiter().GetResult();
            lost.Lose = true;
            sync.NotifyLocalSaved();
            Assert.That(link.State, Is.EqualTo(AccountState.Guest));
            Assert.That(sync.IsActive, Is.False);
        }

        sealed class SessionLostCloud : ICloudProfileGateway
        {
            readonly ICloudProfileGateway _inner;
            public bool Lose;
            public SessionLostCloud(ICloudProfileGateway inner) { _inner = inner; }
            public UniTask<Result<CloudProfile?>> GetAsync(CancellationToken ct) => Lose ? UniTask.FromResult(Result<CloudProfile?>.Fail(new DomainError("account.session", "errors.account_session"))) : _inner.GetAsync(ct);
            public UniTask<Result<CloudWriteResult>> UpsertAsync(ProfileData profile, string? expected, CancellationToken ct)
                => Lose ? UniTask.FromResult(Result<CloudWriteResult>.Fail(new DomainError("account.session", "errors.account_session"))) : _inner.UpsertAsync(profile, expected, ct);
        }

        // ------------------------------------------------------------------ I8

        [Test, Description("Vérifie qu'une restauration en cours et une connexion manuelle sont exécutées l'une après l'autre (pas en parallèle) et que le compte final est celui de la connexion manuelle — parce que deux rattachements simultanés mélangeraient les comptes")]
        public void Restore_AndManualSignIn_AreSerialized()
        {
            var gated = new GatedRestoreAccounts(_accounts);
            Rebuild(gated);
            LocalProgress(5);
            _accounts.SignUpAsync("a@exemple.fr", "motdepasse1", "A", Ct).GetAwaiter().GetResult();
            _accounts.SignUpAsync("b@exemple.fr", "motdepasse1", "B", Ct).GetAwaiter().GetResult();
            _accounts.SignInAsync("a@exemple.fr", "motdepasse1", Ct).GetAwaiter().GetResult();   // session mémorisée : A
            var restore = _link.RestoreAsync(Ct);                 // bloquée sur le réseau
            var signIn = _link.SignInAsync("b@exemple.fr", "motdepasse1", Ct);
            Assert.That(gated.SignInStarted, Is.False, "la connexion attend la fin de la restauration");
            gated.Release();
            restore.GetAwaiter().GetResult();
            signIn.GetAwaiter().GetResult();
            Assert.That(_link.Cloud!.Email, Is.EqualTo("b@exemple.fr"));
        }

        sealed class GatedRestoreAccounts : IAccountGateway
        {
            readonly IAccountGateway _inner;
            readonly UniTaskCompletionSource _gate = new UniTaskCompletionSource();
            public bool SignInStarted;
            public GatedRestoreAccounts(IAccountGateway inner) { _inner = inner; }
            public void Release() => _gate.TrySetResult();
            public UniTask<Result<PlayerAccount>> SignUpAsync(string e, string p, string n, CancellationToken ct) => _inner.SignUpAsync(e, p, n, ct);
            public UniTask<Result<PlayerAccount>> SignInAsync(string e, string p, CancellationToken ct) { SignInStarted = true; return _inner.SignInAsync(e, p, ct); }
            public UniTask<Result<PlayerAccount>> SignInWithProviderAsync(AuthProvider provider, CancellationToken ct) => _inner.SignInWithProviderAsync(provider, ct);
            public async UniTask<Result<PlayerAccount?>> RestoreSessionAsync(CancellationToken ct) { await _gate.Task; return await _inner.RestoreSessionAsync(ct); }
            public UniTask<Result> SignOutAsync(CancellationToken ct) => _inner.SignOutAsync(ct);
            public UniTask<Result> ResetPasswordAsync(string e, CancellationToken ct) => _inner.ResetPasswordAsync(e, ct);
        }

        // ------------------------------------------------------------------ Divers

        [Test, Description("Vérifie que demander un choix sans choix en attente rend un message propre à ce cas (« aucun choix en attente »), pas « sauvegarde en ligne indisponible » — parce que le message doit dire la vérité")]
        public void Resolve_WithoutPendingChoice_HasItsOwnError()
        {
            var r = _link.ResolveAsync(ChoiceKind.KeepAccount, Ct).GetAwaiter().GetResult();
            Assert.That(r.Error!.LocKey, Is.EqualTo("errors.account_nochoice"));
        }

        [Test, Description("Vérifie que la sauvegarde de secours en mémoire garde une copie (modifier la session ensuite ne change pas la sauvegarde) — parce que ToProfile partage l'objet Meta de la session")]
        public void BackupStore_KeepsACopy_NotTheSessionObject()
        {
            var p = _app.Session.ToProfile(_clock.UtcNow);
            p.credits = 10;
            _backups.Save("local", p, _clock.UtcNow);
            p.credits = 999;
            p.meta.tutorial.done = !p.meta.tutorial.done;
            var kept = _backups.Saved.Single().Profile;
            Assert.That(kept.credits, Is.EqualTo(10));
            Assert.That(kept.meta.tutorial.done, Is.Not.EqualTo(p.meta.tutorial.done));
        }

        [Test, Description("Vérifie que le résultat d'un envoi qui se termine après une déconnexion est ignoré (ni date attendue ni état de synchronisation réécrits) — parce que l'état d'un compte ne doit pas contaminer le suivant")]
        public void UploadFinishingAfterSignOut_IsIgnored()
        {
            LocalProgress(123);
            _link.SignUpAsync("moi@exemple.fr", "motdepasse1", "Moi", Ct).GetAwaiter().GetResult();
            var gated = new GatedUpsertCloud(_cloud);
            var sync = new CloudSync(gated, _state, _app.Session, _clock, _app.Logger, (t, ct) => UniTask.CompletedTask, _app.Save);
            sync.Start(_cloud.UpdatedAt);
            var flush = sync.FlushAsync(Ct);
            _state.Clear();
            sync.Stop();                                            // déconnexion pendant l'envoi
            gated.Release();
            flush.GetAwaiter().GetResult();
            Assert.That(_state.Load(), Is.Null, "l'état n'a pas été réécrit par l'envoi périmé");
        }

        sealed class GatedUpsertCloud : ICloudProfileGateway
        {
            readonly ICloudProfileGateway _inner;
            readonly UniTaskCompletionSource _gate = new UniTaskCompletionSource();
            public GatedUpsertCloud(ICloudProfileGateway inner) { _inner = inner; }
            public void Release() => _gate.TrySetResult();
            public UniTask<Result<CloudProfile?>> GetAsync(CancellationToken ct) => _inner.GetAsync(ct);
            public async UniTask<Result<CloudWriteResult>> UpsertAsync(ProfileData profile, string? expected, CancellationToken ct) { await _gate.Task; return await _inner.UpsertAsync(profile, expected, ct); }
        }

        [Test, Description("Vérifie qu'une sauvegarde locale qui arrive pendant un vidage en cours ne fait pas tourner la boucle d'envoi dans le vide (le jeu ne se fige pas) et qu'elle est envoyée dès la fin du vidage — parce que perdre le focus et sauvegarder arrivent en même temps")]
        public void SaveDuringAFlush_DoesNotSpin_AndIsSentAfterwards()
        {
            LocalProgress(123);
            _link.SignUpAsync("moi@exemple.fr", "motdepasse1", "Moi", Ct).GetAwaiter().GetResult();
            var gated = new GatedUpsertCloud(_cloud);
            var sync = new CloudSync(gated, _state, _app.Session, _clock, _app.Logger, (t, ct) => { _clock.Advance(t); return UniTask.CompletedTask; }, _app.Save);
            sync.Start(_cloud.UpdatedAt);
            var flush = sync.FlushAsync(Ct);                         // envoi en cours, bloqué sur le réseau
            app_Save();
            gated.Release();
            flush.GetAwaiter().GetResult();
            Assert.That(_cloud.Writes, Is.GreaterThanOrEqualTo(2), "la sauvegarde arrivée pendant le vidage est partie ensuite");
        }

        void app_Save() => _app.Save.SaveAsync(Ct).GetAwaiter().GetResult();
    }
}
