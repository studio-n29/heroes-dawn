using System.Linq;
using System.Threading;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Infrastructure.Simulated;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    /// <summary>Ports de compte et de sauvegarde cloud (lot I) : simulateurs en mémoire et textes d'erreur.</summary>
    public sealed class AccountPortsTests
    {
        static readonly CancellationToken Ct = CancellationToken.None;

        static ProfileData Profile(int credits) => new ProfileData { credits = credits };

        [Test, Description("Vérifie que la connexion par Google rend deux fois le même compte, qu'Apple en crée un autre et que le fournisseur est correct — parce qu'un même fournisseur doit retrouver le même joueur")]
        public void Local_SignInWithProvider_CreatesAndReusesAnAccount()
        {
            var g = new LocalAccountGateway(0);
            var a = g.SignInWithProviderAsync(AuthProvider.Google, Ct).GetAwaiter().GetResult().Value;
            var b = g.SignInWithProviderAsync(AuthProvider.Google, Ct).GetAwaiter().GetResult().Value;
            var c = g.SignInWithProviderAsync(AuthProvider.Apple, Ct).GetAwaiter().GetResult().Value;
            Assert.That(b.PlayerId, Is.EqualTo(a.PlayerId));
            Assert.That(c.PlayerId, Is.Not.EqualTo(a.PlayerId));
            Assert.That((a.Provider, c.Provider), Is.EqualTo((AuthProvider.Google, AuthProvider.Apple)));
        }

        [Test, Description("Vérifie que la session restaurée est celle du dernier compte connecté puis vide après la déconnexion — parce que le jeu rouvre la session au lancement")]
        public void Local_RestoreSession_ReturnsTheSignedInAccount_ThenNullAfterSignOut()
        {
            var g = new LocalAccountGateway(0);
            Assert.That(g.RestoreSessionAsync(Ct).GetAwaiter().GetResult().Value, Is.Null);
            g.SignUpAsync("Moi@Exemple.fr ", "motdepasse1", "Moi", Ct).GetAwaiter().GetResult();
            g.SignInAsync("moi@exemple.fr", "motdepasse1", Ct).GetAwaiter().GetResult();
            var restored = g.RestoreSessionAsync(Ct).GetAwaiter().GetResult().Value;
            Assert.That(restored!.Email, Is.EqualTo("moi@exemple.fr"));
            Assert.That(g.SignOutAsync(Ct).GetAwaiter().GetResult().IsSuccess, Is.True);
            Assert.That(g.RestoreSessionAsync(Ct).GetAwaiter().GetResult().Value, Is.Null);
        }

        [Test, Description("Vérifie que la réinitialisation du mot de passe réussit même pour une adresse inconnue (on ne révèle pas l'existence d'un compte) mais refuse une adresse invalide — parce qu'un message différent permettrait de deviner les comptes")]
        public void Local_ResetPassword_UnknownEmail_StillSucceeds_InvalidIsRefused()
        {
            var g = new LocalAccountGateway(0);
            Assert.That(g.ResetPasswordAsync("inconnu@exemple.fr", Ct).GetAwaiter().GetResult().IsSuccess, Is.True);
            var bad = g.ResetPasswordAsync("pas-une-adresse", Ct).GetAwaiter().GetResult();
            Assert.That(bad.IsFailure, Is.True);
            Assert.That(bad.Error!.LocKey, Is.EqualTo("errors.account_email"));
        }

        [Test, Description("Vérifie que le simulateur de sauvegarde cloud accepte la première écriture, puis les écritures conditionnelles à la bonne date, et refuse une mauvaise date sans rien écraser — parce que c'est le contrat anti-perte du port")]
        public void FakeCloud_FirstWrite_ThenConditionalWrites_AndConflictKeepsData()
        {
            var cloud = new FakeCloudProfileGateway();
            Assert.That(cloud.GetAsync(Ct).GetAwaiter().GetResult().Value, Is.Null);
            var first = cloud.UpsertAsync(Profile(10), null, Ct).GetAwaiter().GetResult().Value;
            Assert.That(first.Status, Is.EqualTo(CloudWriteStatus.Ok));
            var again = cloud.UpsertAsync(Profile(11), null, Ct).GetAwaiter().GetResult().Value;
            Assert.That(again.Status, Is.EqualTo(CloudWriteStatus.Conflict), "une ligne existe déjà : première écriture refusée");
            var ok = cloud.UpsertAsync(Profile(20), first.UpdatedAt, Ct).GetAwaiter().GetResult().Value;
            Assert.That(ok.Status, Is.EqualTo(CloudWriteStatus.Ok));
            Assert.That(ok.UpdatedAt, Is.Not.EqualTo(first.UpdatedAt));
            var stale = cloud.UpsertAsync(Profile(30), first.UpdatedAt, Ct).GetAwaiter().GetResult().Value;
            Assert.That(stale.Status, Is.EqualTo(CloudWriteStatus.Conflict));
            Assert.That(cloud.GetAsync(Ct).GetAwaiter().GetResult().Value!.Profile.credits, Is.EqualTo(20), "le conflit n'a rien écrasé");
        }

        [Test, Description("Vérifie que le simulateur rend une copie du profil (modifier la copie ne change pas la ligne) — parce qu'aucune référence ne doit être partagée avec la session")]
        public void FakeCloud_ReturnsACopy()
        {
            var cloud = new FakeCloudProfileGateway();
            var source = Profile(5);
            cloud.UpsertAsync(source, null, Ct).GetAwaiter().GetResult();
            source.credits = 999;
            var read = cloud.GetAsync(Ct).GetAwaiter().GetResult().Value!;
            Assert.That(read.Profile.credits, Is.EqualTo(5));
            read.Profile.credits = 777;
            Assert.That(cloud.GetAsync(Ct).GetAwaiter().GetResult().Value!.Profile.credits, Is.EqualTo(5));
        }

        [Test, Description("Vérifie qu'un échec simulé rend une erreur « sauvegarde en ligne indisponible » sans rien modifier, puis que l'opération suivante réussit — parce que le réseau peut tomber sans perdre de données")]
        public void FakeCloud_SimulatedFailure_ReturnsErrorAndKeepsData()
        {
            var cloud = new FakeCloudProfileGateway();
            cloud.UpsertAsync(Profile(1), null, Ct).GetAwaiter().GetResult();
            cloud.FailNext = true;
            var failed = cloud.UpsertAsync(Profile(2), cloud.UpdatedAt, Ct).GetAwaiter().GetResult();
            Assert.That(failed.IsFailure, Is.True);
            Assert.That(failed.Error!.LocKey, Is.EqualTo("errors.account_cloud"));
            Assert.That(cloud.GetAsync(Ct).GetAwaiter().GetResult().Value!.Profile.credits, Is.EqualTo(1));
        }
    }
}
