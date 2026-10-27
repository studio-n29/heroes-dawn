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
    /// <summary>Rattachement d'un invité à un compte, choix de progression, session et déconnexion (lot I).</summary>
    public sealed class AccountServiceTests
    {
        static readonly CancellationToken Ct = CancellationToken.None;

        TestApp _app = null!;
        FixedClock _clock = null!;
        FakeCloudProfileGateway _cloud = null!;
        LocalAccountGateway _accounts = null!;
        MemorySyncStateStore _state = null!;
        MemoryProgressBackupStore _backups = null!;
        CloudSyncService _sync = null!;
        AccountService _svc = null!;

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.SignInDirect("guest-1");
            _app.Session.ResetToNewProfile();   // appareil neuf : aucune progression
            _clock = new FixedClock();
            _cloud = new FakeCloudProfileGateway();
            _accounts = new LocalAccountGateway(0);
            _state = new MemorySyncStateStore();
            _backups = new MemoryProgressBackupStore();
            _sync = new CloudSyncService(_cloud, _state, _app.Session, _clock, _app.Logger, (t, ct) => { _clock.Advance(t); return UniTask.CompletedTask; }, _app.Save);
            _svc = new AccountService(_accounts, _cloud, _app.Session, _app.Save, _sync, _state, _backups, _clock, _app.Logger,
                                      _app.Publisher<WalletChanged>(), _app.Publisher<RosterChanged>());
        }

        /// <summary>Un profil du « cloud » avec de la progression : 3 zones terminées et des Crédits reconnaissables.</summary>
        ProfileData CloudProfile(int credits = 777)
        {
            // Copie profonde : ToProfile partage l'objet Meta de la session, qu'il ne faut pas modifier ici.
            var p = UnityEngine.JsonUtility.FromJson<ProfileData>(UnityEngine.JsonUtility.ToJson(_app.Session.ToProfile(_clock.UtcNow)));
            p.credits = credits;
            p.completedZones = new List<string> { "zone_ny_1_1", "zone_ny_1_2", "zone_ny_1_3" };
            p.meta.tutorial.done = true;
            return p;
        }

        void GiveLocalProgress(int credits = 123)
        {
            _app.Session.CompletedZones.Add("zone_ny_1_1");
            _app.Session.Meta.tutorial.done = true;
            _app.Session.Wallet.Set(credits, _app.Session.Wallet.Pr);
        }

        LinkOutcome SignUp()
            => _svc.SignUpAsync("moi@exemple.fr", "motdepasse1", "Moi", Ct).GetAwaiter().GetResult().Value!;

        [Test, Description("Vérifie que l'invité qui crée un compte sans profil cloud envoie sa progression locale (première écriture) et que la synchronisation est mémorisée — parce que le compte doit sauvegarder la progression")]
        public void Link_CloudEmpty_UploadsLocal()
        {
            GiveLocalProgress(123);
            var outcome = SignUp();
            Assert.That(outcome.Status, Is.EqualTo(LinkStatus.Uploaded));
            Assert.That(_cloud.GetAsync(Ct).GetAwaiter().GetResult().Value!.Profile.credits, Is.EqualTo(123));
            Assert.That(_state.Load()!.LastUpdatedAt, Is.EqualTo(_cloud.UpdatedAt));
            Assert.That(_svc.State, Is.EqualTo(AccountState.Connected));
        }

        [Test, Description("Vérifie qu'un profil cloud existant est appliqué tel quel quand l'appareil n'a aucune progression (nouvelle installation) — parce que se connecter sur un autre appareil doit retrouver sa partie")]
        public void Link_CloudPresent_LocalFresh_PullsCloud()
        {
            _cloud.SeedFromOtherDevice(CloudProfile(777));
            var outcome = SignUp();
            Assert.That(outcome.Status, Is.EqualTo(LinkStatus.Pulled));
            Assert.That(_app.Session.Wallet.Credits, Is.EqualTo(777));
            Assert.That(_app.Session.CompletedZones.Count, Is.EqualTo(3));
            Assert.That(_backups.Saved, Is.Empty, "rien n'a été écrasé : pas de sauvegarde de secours");
        }

        [Test, Description("Vérifie que si l'appareil et le compte ont chacun de la progression et que ce n'est pas la première synchronisation inconnue, rien n'est écrit et le joueur doit choisir — parce qu'on ne devine jamais quelle partie garder")]
        public void Link_CloudPresent_LocalProgress_FirstTime_NeedsChoice_AndWritesNothing()
        {
            _cloud.SeedFromOtherDevice(CloudProfile(777));
            GiveLocalProgress(123);
            int writes = _cloud.Writes;
            var outcome = SignUp();
            Assert.That(outcome.Status, Is.EqualTo(LinkStatus.NeedsChoice));
            Assert.That(outcome.Local!.Credits, Is.EqualTo(123));
            Assert.That(outcome.Cloud!.Credits, Is.EqualTo(777));
            Assert.That(_cloud.Writes, Is.EqualTo(writes), "aucune écriture en ligne");
            Assert.That(_app.Session.Wallet.Credits, Is.EqualTo(123), "aucune modification locale");
            Assert.That(_svc.PendingChoice, Is.Not.Null);
        }

        [Test, Description("Vérifie que, quand l'appareil a déjà été synchronisé avec cette version du compte, la progression locale est simplement envoyée (pas de choix) — parce que seule cette machine a avancé")]
        public void Link_AlreadyInSync_UploadsLocal()
        {
            _cloud.SeedFromOtherDevice(CloudProfile(777));
            GiveLocalProgress(123);
            // L'état mémorisé dit : le dernier compte vu par cet appareil est la version actuelle du cloud.
            _accounts.SignUpAsync("moi@exemple.fr", "motdepasse1", "Moi", Ct).GetAwaiter().GetResult();
            var id = _accounts.SignInAsync("moi@exemple.fr", "motdepasse1", Ct).GetAwaiter().GetResult().Value.PlayerId;
            _state.Save(new CloudSyncState { UserId = id, LastUpdatedAt = _cloud.UpdatedAt, LastSyncUtcTicks = _clock.UtcNow.Ticks });
            var outcome = _svc.SignInAsync("moi@exemple.fr", "motdepasse1", Ct).GetAwaiter().GetResult().Value!;
            Assert.That(outcome.Status, Is.EqualTo(LinkStatus.Uploaded));
            Assert.That(_cloud.GetAsync(Ct).GetAwaiter().GetResult().Value!.Profile.credits, Is.EqualTo(123));
        }

        [Test, Description("Vérifie qu'un profil cloud d'une version de schéma plus récente que le jeu est refusé (erreur « mets le jeu à jour ») sans rien appliquer ni écrire — parce qu'un vieux jeu pourrait détruire des données qu'il ne comprend pas")]
        public void Link_CloudSchemaNewer_IsRefused_AndNothingChanges()
        {
            var newer = CloudProfile(777);
            newer.schemaVersion = new ProfileData().schemaVersion + 1;
            _cloud.SeedFromOtherDevice(newer);
            int writes = _cloud.Writes;
            var r = _svc.SignUpAsync("moi@exemple.fr", "motdepasse1", "Moi", Ct).GetAwaiter().GetResult();
            Assert.That(r.IsFailure, Is.True);
            Assert.That(r.Error!.LocKey, Is.EqualTo("errors.account_schema"));
            Assert.That(_app.Session.Wallet.Credits, Is.Not.EqualTo(777));
            Assert.That(_cloud.Writes, Is.EqualTo(writes));
        }

        [Test, Description("Vérifie qu'un profil cloud d'une ancienne version de schéma est appliqué comme un profil local ancien (les champs absents prennent leur valeur par défaut) — parce que les anciennes sauvegardes doivent rester lisibles")]
        public void Link_CloudOlderSchema_IsAppliedLikeALocalProfile()
        {
            var older = CloudProfile(555);
            older.schemaVersion = 2;
            _cloud.SeedFromOtherDevice(older);
            var outcome = SignUp();
            Assert.That(outcome.Status, Is.EqualTo(LinkStatus.Pulled));
            Assert.That(_app.Session.Wallet.Credits, Is.EqualTo(555));
        }

        [Test, Description("Vérifie que « Garder le compte » sauvegarde d'abord la progression locale (sauvegarde de secours), applique le cloud et mémorise la synchronisation — parce que l'appareil ne perd jamais sa partie sans filet")]
        public void KeepAccount_BacksUpLocal_AppliesCloud_UpdatesState()
        {
            _cloud.SeedFromOtherDevice(CloudProfile(777));
            GiveLocalProgress(123);
            SignUp();
            var outcome = _svc.ResolveAsync(ChoiceKind.KeepAccount, Ct).GetAwaiter().GetResult().Value!;
            Assert.That(outcome.Status, Is.EqualTo(LinkStatus.Pulled));
            Assert.That(_backups.Saved.Single().Reason, Is.EqualTo("local"));
            Assert.That(_backups.Saved.Single().Profile.credits, Is.EqualTo(123));
            Assert.That(_app.Session.Wallet.Credits, Is.EqualTo(777));
            Assert.That(_state.Load()!.LastUpdatedAt, Is.EqualTo(_cloud.UpdatedAt));
            Assert.That(_svc.PendingChoice, Is.Null);
        }

        [Test, Description("Vérifie que « Garder cet appareil » sauvegarde d'abord la version du compte (secours), puis envoie la progression locale avec la date lue — parce que le compte ne perd jamais sa partie sans filet")]
        public void KeepDevice_BacksUpCloud_UploadsWithExpected_UpdatesState()
        {
            _cloud.SeedFromOtherDevice(CloudProfile(777));
            GiveLocalProgress(123);
            SignUp();
            var outcome = _svc.ResolveAsync(ChoiceKind.KeepDevice, Ct).GetAwaiter().GetResult().Value!;
            Assert.That(outcome.Status, Is.EqualTo(LinkStatus.Uploaded));
            Assert.That(_backups.Saved.Single().Reason, Is.EqualTo("cloud"));
            Assert.That(_backups.Saved.Single().Profile.credits, Is.EqualTo(777));
            Assert.That(_cloud.GetAsync(Ct).GetAwaiter().GetResult().Value!.Profile.credits, Is.EqualTo(123));
            Assert.That(_svc.PendingChoice, Is.Null);
        }

        [Test, Description("Vérifie que si le compte a encore changé entre la lecture et « Garder cet appareil », l'envoi est refusé et le choix est reproposé avec la nouvelle version — parce que rien ne doit être écrasé en silence")]
        public void KeepDevice_ConflictAgain_ReturnsNeedsChoiceAgain_NoLoss()
        {
            _cloud.SeedFromOtherDevice(CloudProfile(777));
            GiveLocalProgress(123);
            SignUp();
            _cloud.SeedFromOtherDevice(CloudProfile(888));   // un autre appareil a encore écrit
            var outcome = _svc.ResolveAsync(ChoiceKind.KeepDevice, Ct).GetAwaiter().GetResult().Value!;
            Assert.That(outcome.Status, Is.EqualTo(LinkStatus.NeedsChoice));
            Assert.That(outcome.Cloud!.Credits, Is.EqualTo(888));
            Assert.That(_cloud.GetAsync(Ct).GetAwaiter().GetResult().Value!.Profile.credits, Is.EqualTo(888), "la version du compte est intacte");
            Assert.That(_svc.PendingChoice, Is.Not.Null);
        }

        [Test, Description("Vérifie qu'un choix demandé sans choix en attente est refusé sans rien modifier — parce qu'un double appui ne doit pas rejouer un choix")]
        public void Resolve_WithoutPendingChoice_IsRefused()
        {
            var r = _svc.ResolveAsync(ChoiceKind.KeepAccount, Ct).GetAwaiter().GetResult();
            Assert.That(r.IsFailure, Is.True);
            Assert.That(_backups.Saved, Is.Empty);
        }

        [Test, Description("Vérifie que l'inscription qui demande une confirmation d'e-mail réussit sans rattachement (aucun résultat) et laisse le joueur invité — parce que le compte n'est pas utilisable avant la confirmation")]
        public void SignUp_NeedingConfirmation_StaysGuest()
        {
            var gateway = new ConfirmingAccounts();
            var svc = new AccountService(gateway, _cloud, _app.Session, _app.Save, _sync, _state, _backups, _clock, _app.Logger,
                                         _app.Publisher<WalletChanged>(), _app.Publisher<RosterChanged>());
            var r = svc.SignUpAsync("moi@exemple.fr", "motdepasse1", "Moi", Ct).GetAwaiter().GetResult();
            Assert.That(r.IsSuccess, Is.True);
            Assert.That(r.Value, Is.Null);
            Assert.That(svc.State, Is.EqualTo(AccountState.Guest));
        }

        [Test, Description("Vérifie qu'une session restaurée au lancement rattache et synchronise sans demander d'identifiants, et qu'en l'absence de session le joueur reste invité — parce que le joueur ne doit pas se reconnecter à chaque lancement")]
        public void Restore_ValidSession_Links_NoSession_StaysGuest()
        {
            Assert.That(_svc.RestoreAsync(Ct).GetAwaiter().GetResult().Value, Is.Null);
            Assert.That(_svc.State, Is.EqualTo(AccountState.Guest));
            _accounts.SignUpAsync("moi@exemple.fr", "motdepasse1", "Moi", Ct).GetAwaiter().GetResult();
            _accounts.SignInAsync("moi@exemple.fr", "motdepasse1", Ct).GetAwaiter().GetResult();
            GiveLocalProgress(321);
            var outcome = _svc.RestoreAsync(Ct).GetAwaiter().GetResult().Value!;
            Assert.That(outcome.Status, Is.EqualTo(LinkStatus.Uploaded));
            Assert.That(_svc.State, Is.EqualTo(AccountState.Connected));
        }

        [Test, Description("Vérifie que la déconnexion efface l'état de synchronisation, arrête l'envoi et garde la progression locale (le joueur redevient invité) — parce que se déconnecter ne doit pas détruire la partie de l'appareil")]
        public void SignOut_ClearsState_StopsSync_KeepsTheLocalProfile()
        {
            GiveLocalProgress(123);
            SignUp();
            Assert.That(_svc.SignOutAsync(Ct).GetAwaiter().GetResult().IsSuccess, Is.True);
            Assert.That(_svc.State, Is.EqualTo(AccountState.Guest));
            Assert.That(_state.Load(), Is.Null);
            Assert.That(_app.Session.Wallet.Credits, Is.EqualTo(123));
            int writes = _cloud.Writes;
            _sync.NotifyLocalSaved();
            Assert.That(_cloud.Writes, Is.EqualTo(writes), "plus aucun envoi après la déconnexion");
        }

        [Test, Description("Vérifie qu'un échec réseau pendant la lecture du cloud laisse le joueur connecté (il pourra synchroniser plus tard) et rend l'erreur — parce que la panne du serveur ne doit pas bloquer la partie")]
        public void CloudDown_DuringLink_KeepsTheAccountConnected()
        {
            _cloud.FailNext = true;
            var r = _svc.SignUpAsync("moi@exemple.fr", "motdepasse1", "Moi", Ct).GetAwaiter().GetResult();
            Assert.That(r.IsFailure, Is.True);
            Assert.That(r.Error!.LocKey, Is.EqualTo("errors.account_cloud"));
            Assert.That(_svc.State, Is.EqualTo(AccountState.Connected));
        }

        [Test, Description("Vérifie que deux connexions simultanées avec les mêmes identifiants ne font qu'un seul appel de connexion et un seul rattachement (une écriture en ligne) — parce qu'un double appui ne doit pas doubler l'envoi")]
        public void DoubleSignIn_IsCoalesced()
        {
            _accounts.SignUpAsync("moi@exemple.fr", "motdepasse1", "Moi", Ct).GetAwaiter().GetResult();
            GiveLocalProgress(5);
            var gated = new GatedAccounts(_accounts);
            var svc = new AccountService(gated, _cloud, _app.Session, _app.Save, _sync, _state, _backups, _clock, _app.Logger,
                                         _app.Publisher<WalletChanged>(), _app.Publisher<RosterChanged>());
            var first = svc.SignInAsync("moi@exemple.fr", "motdepasse1", Ct);
            var second = svc.SignInAsync("moi@exemple.fr", "motdepasse1", Ct);
            gated.Release();
            Assert.That(first.GetAwaiter().GetResult().IsSuccess, Is.True);
            Assert.That(second.GetAwaiter().GetResult().IsSuccess, Is.True);
            Assert.That(gated.SignInCalls, Is.EqualTo(1));
            Assert.That(_cloud.Writes, Is.EqualTo(1));
        }

        [Test, Description("Vérifie le résumé d'un profil : niveau le plus haut, Crédits, zones terminées, date de dernière partie, et « neuf » seulement sans zone terminée ni tutoriel fini — parce que le choix de progression montre ces chiffres et que « neuf » décide de l'écrasement silencieux")]
        public void ProfileSummary_Values_AndFreshRule()
        {
            var p = new ProfileData { credits = 50, lastSeenUtcTicks = new DateTime(2026, 10, 1, 8, 0, 0, DateTimeKind.Utc).Ticks };
            p.team.Add(new MemberSave { id = "a", level = 7 }); p.team.Add(new MemberSave { id = "b", level = 12 });
            Assert.That(ProfileSummary.Of(p).IsFresh, Is.True);
            var s = ProfileSummary.Of(p);
            Assert.That((s.Level, s.Credits, s.ZonesCleared), Is.EqualTo((12, 50, 0)));
            Assert.That(s.LastSeenUtc, Is.EqualTo(new DateTime(2026, 10, 1, 8, 0, 0, DateTimeKind.Utc)));
            p.completedZones.Add("zone_ny_1_1");
            Assert.That(ProfileSummary.Of(p).IsFresh, Is.False);
            p.completedZones.Clear(); p.meta.tutorial.done = true;
            Assert.That(ProfileSummary.Of(p).IsFresh, Is.False);
        }

        /// <summary>Compte dont la connexion attend un signal (pour tester les appels simultanés).</summary>
        sealed class GatedAccounts : IAccountGateway
        {
            readonly IAccountGateway _inner;
            readonly UniTaskCompletionSource _gate = new UniTaskCompletionSource();
            public int SignInCalls;
            public GatedAccounts(IAccountGateway inner) { _inner = inner; }
            public void Release() => _gate.TrySetResult();
            public UniTask<Result<PlayerAccount>> SignUpAsync(string email, string password, string displayName, CancellationToken ct) => _inner.SignUpAsync(email, password, displayName, ct);
            public async UniTask<Result<PlayerAccount>> SignInAsync(string email, string password, CancellationToken ct)
            {
                SignInCalls++;
                await _gate.Task;
                return await _inner.SignInAsync(email, password, ct);
            }
            public UniTask<Result<PlayerAccount>> SignInWithProviderAsync(AuthProvider provider, CancellationToken ct) => _inner.SignInWithProviderAsync(provider, ct);
            public UniTask<Result<PlayerAccount?>> RestoreSessionAsync(CancellationToken ct) => _inner.RestoreSessionAsync(ct);
            public UniTask<Result> SignOutAsync(CancellationToken ct) => _inner.SignOutAsync(ct);
            public UniTask<Result> ResetPasswordAsync(string email, CancellationToken ct) => _inner.ResetPasswordAsync(email, ct);
        }

        /// <summary>Compte dont l'inscription demande de confirmer l'adresse (aucune session).</summary>
        sealed class ConfirmingAccounts : IAccountGateway
        {
            public UniTask<Result<PlayerAccount>> SignUpAsync(string email, string password, string displayName, CancellationToken ct)
                => UniTask.FromResult(Result<PlayerAccount>.Ok(new PlayerAccount("u", displayName, email, AuthProvider.Email, needsConfirmation: true)));
            public UniTask<Result<PlayerAccount>> SignInAsync(string email, string password, CancellationToken ct) => throw new NotSupportedException();
            public UniTask<Result<PlayerAccount>> SignInWithProviderAsync(AuthProvider provider, CancellationToken ct) => throw new NotSupportedException();
            public UniTask<Result<PlayerAccount?>> RestoreSessionAsync(CancellationToken ct) => UniTask.FromResult(Result<PlayerAccount?>.Ok(null));
            public UniTask<Result> SignOutAsync(CancellationToken ct) => UniTask.FromResult(Result.Ok());
            public UniTask<Result> ResetPasswordAsync(string email, CancellationToken ct) => UniTask.FromResult(Result.Ok());
        }
    }
}
