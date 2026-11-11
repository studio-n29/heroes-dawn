using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Infrastructure.Platform;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    /// <summary>Passerelle d'achats réels (bloc E) avec un faux magasin : le vrai SDK n'est jamais appelé.</summary>
    public sealed class UnityIapGatewayTests
    {
        sealed class FakeStore : IStoreAdapter
        {
            public event Action<string, string>? Purchased;
            public event Action<string, StoreFailure>? Failed;
            public bool CanConnect = true;
            public int Connects;
            /// <summary>Transactions payées mais jamais confirmées, remises à la connexion (comme le SDK).</summary>
            public readonly List<(string Product, string Txn)> Unconfirmed = new List<(string, string)>();
            public readonly List<string> Confirmed = new List<string>();
            public StoreFailure? NextFailure;
            int _n;

            public UniTask<bool> ConnectAsync(IReadOnlyList<string> productIds, CancellationToken ct)
            {
                Connects++;
                if (!CanConnect) return UniTask.FromResult(false);
                foreach (var u in Unconfirmed.ToList()) Purchased?.Invoke(u.Product, u.Txn);
                return UniTask.FromResult(true);
            }

            public void Purchase(string productId)
            {
                if (NextFailure.HasValue) { var f = NextFailure.Value; NextFailure = null; Failed?.Invoke(productId, f); return; }
                var txn = "T" + (++_n);
                Unconfirmed.Add((productId, txn));
                Purchased?.Invoke(productId, txn);
            }

            public void Confirm(string productId, string transactionId)
            {
                Confirmed.Add(transactionId);
                Unconfirmed.RemoveAll(u => u.Txn == transactionId);
            }

            public void Dispose() { }
        }

        TestApp _app = null!;
        FakeStore _store = null!;

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.SignInDirect("p1");
            _store = new FakeStore();
        }

        static string Code<T>(N29.HeroesDawn.Core.Result<T> r) => r.Error?.Code ?? "ok";
        static string Code(N29.HeroesDawn.Core.Result r) => r.Error?.Code ?? "ok";

        Boutique ShopOn(UnityIapGateway gw) => new Boutique(_app.MetaCtx, gw, _app.Publisher<RosterChanged>());
        PassProgress PassOn(UnityIapGateway gw) => new PassProgress(_app.MetaCtx, TestApp.TestPassRewards, gw);

        [Test, Description("Vérifie qu'un achat réussi donne les Gemmes puis seulement ensuite est confirmé au store — parce qu'une confirmation avant l'octroi ferait perdre l'achat si l'appli s'arrête")]
        public void Un_achat_reussi_est_confirme_apres_l_octroi()
        {
            var gw = new UnityIapGateway(_store);
            int gems = _app.Session.Wallet.Gems;
            var r = ShopOn(gw).BuyPackAsync("gems_1080", CancellationToken.None).GetAwaiter().GetResult();
            Assert.That(r.IsSuccess);
            Assert.That(_app.Session.Wallet.Gems, Is.EqualTo(gems + 1080));
            Assert.That(_store.Confirmed, Is.EqualTo(new[] { "T1" }));
            Assert.That(gw.Pending, Is.Empty);
        }

        [Test, Description("Vérifie qu'un achat annulé ne donne rien, n'est pas confirmé et laisse une erreur propre — parce qu'annuler ne coûte rien au joueur")]
        public void Un_achat_annule_ne_donne_rien()
        {
            var gw = new UnityIapGateway(_store);
            _store.NextFailure = StoreFailure.Cancelled;
            int gems = _app.Session.Wallet.Gems;
            var r = ShopOn(gw).BuyPackAsync("gems_100", CancellationToken.None).GetAwaiter().GetResult();
            Assert.That(Code(r), Is.EqualTo("meta.payment_cancelled"));
            Assert.That(_app.Session.Wallet.Gems, Is.EqualTo(gems));
            Assert.That(_store.Confirmed, Is.Empty);
        }

        [Test, Description("Vérifie qu'un produit hors de la liste des sept consommables est refusé sans appeler le store — parce qu'un identifiant inconnu ne doit jamais partir vers un paiement")]
        public void Un_produit_inconnu_est_refuse()
        {
            var gw = new UnityIapGateway(_store);
            var r = gw.PurchaseAsync("gems_550", CancellationToken.None).GetAwaiter().GetResult();
            Assert.That(Code(r), Is.EqualTo("meta.unknown"));
            Assert.That(_store.Connects, Is.EqualTo(0));
            Assert.That(UnityIapGateway.Products, Is.EquivalentTo(new[] { "gems_100", "gems_520", "gems_1080", "gems_2250", "gems_6000", "welcome", "pass_premium" }));
        }

        [Test, Description("Vérifie qu'un achat payé mais jamais octroyé (appli fermée) est repris au lancement suivant, octroyé et confirmé une seule fois, même si la reprise est rappelée — parce qu'un joueur qui a payé doit recevoir son achat sans double octroi")]
        public void Un_achat_en_attente_est_repris_au_lancement_une_seule_fois()
        {
            _store.Unconfirmed.Add(("gems_520", "T-old"));
            _store.Unconfirmed.Add((PassTrack.PremiumProduct, "T-pass"));
            var gw = new UnityIapGateway(_store);
            int announced = 0;
            gw.PendingChanged += () => announced++;
            gw.StartAsync(CancellationToken.None).GetAwaiter().GetResult();
            Assert.That(announced, Is.GreaterThan(0), "la reprise est annoncée");
            var recovery = new PurchaseRecovery(gw, ShopOn(gw), PassOn(gw), _app.Session);
            int gems = _app.Session.Wallet.Gems;
            Assert.That(recovery.Resume(), Is.EqualTo(2));
            Assert.That(recovery.Resume(), Is.EqualTo(0), "plus rien à reprendre");
            Assert.That(_app.Session.Wallet.Gems, Is.EqualTo(gems + 520));
            Assert.That(_app.Session.Meta.passPremium);
            Assert.That(_store.Confirmed, Is.EquivalentTo(new[] { "T-old", "T-pass" }));
        }

        [Test, Description("Vérifie qu'une transaction octroyée mais non confirmée (arrêt entre les deux) n'est pas octroyée une seconde fois au lancement suivant, seulement confirmée — parce que la revue exige zéro double octroi")]
        public void Une_transaction_deja_octroyee_n_est_pas_octroyee_deux_fois()
        {
            _store.Unconfirmed.Add(("gems_100", "T-crash"));
            var first = new UnityIapGateway(_store);
            first.StartAsync(CancellationToken.None).GetAwaiter().GetResult();
            var shop = ShopOn(first);
            shop.Deliver(Boutique.FindPack("gems_100")!, "T-crash"); // octroi fait, arrêt avant la confirmation
            int gems = _app.Session.Wallet.Gems;
            var second = new UnityIapGateway(_store); // lancement suivant : le store remet encore la transaction
            second.StartAsync(CancellationToken.None).GetAwaiter().GetResult();
            var recovery = new PurchaseRecovery(second, ShopOn(second), PassOn(second), _app.Session);
            Assert.That(recovery.Resume(), Is.EqualTo(1));
            Assert.That(_app.Session.Wallet.Gems, Is.EqualTo(gems), "pas de seconde livraison");
            Assert.That(_store.Confirmed, Is.EqualTo(new[] { "T-crash" }));
        }

        [Test, Description("Vérifie qu'une connexion impossible au magasin et une erreur du store rendent des erreurs localisables sans rien donner — parce que le joueur doit voir un message clair, pas un échec muet")]
        public void Une_erreur_du_store_donne_une_erreur_localisee()
        {
            var down = new FakeStore { CanConnect = false };
            var r = new UnityIapGateway(down).PurchaseAsync("gems_100", CancellationToken.None).GetAwaiter().GetResult();
            Assert.That(r.Error!.Code, Is.EqualTo("meta.store"));
            Assert.That(r.Error!.LocKey, Is.EqualTo("errors.store_unavailable"));

            var gw = new UnityIapGateway(_store);
            _store.NextFailure = StoreFailure.Other;
            var other = gw.PurchaseAsync("gems_100", CancellationToken.None).GetAwaiter().GetResult();
            Assert.That(other.Error!.Code, Is.EqualTo("meta.payment"));
            Assert.That(other.Error!.LocKey, Is.EqualTo("errors.meta_payment"));
        }

        [Test, Description("Vérifie que le pass premium acheté par la passerelle réelle est confirmé après l'activation — parce que le même ordre octroi puis confirmation vaut pour le pass")]
        public void Le_pass_premium_est_confirme_apres_l_activation()
        {
            var gw = new UnityIapGateway(_store);
            var r = PassOn(gw).BuyPremiumAsync(CancellationToken.None).GetAwaiter().GetResult();
            Assert.That(r.IsSuccess);
            Assert.That(_app.Session.Meta.passPremium);
            Assert.That(_store.Confirmed, Is.EqualTo(new[] { "T1" }));
        }
    }
}
