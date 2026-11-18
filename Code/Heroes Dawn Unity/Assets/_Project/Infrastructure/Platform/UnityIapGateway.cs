#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Infrastructure.Platform
{
    public enum StoreFailure { Cancelled, Unavailable, Other }

    /// <summary>
    /// Ce que la passerelle demande à un magasin. L'adaptateur réel (<c>UnityStoreAdapter</c>, assemblage Iap) enveloppe le SDK Unity IAP ;
    /// les tests utilisent un faux magasin. Un achat n'est confirmé qu'avec <see cref="Confirm"/>, après l'octroi.
    /// </summary>
    public interface IStoreAdapter : IDisposable
    {
        /// <summary>Un paiement est passé (produit, transaction) : nouvel achat ou transaction en attente remise au lancement.</summary>
        event Action<string, string>? Purchased;
        event Action<string, StoreFailure>? Failed;
        /// <summary>Se connecte au magasin et charge les produits ; les transactions en attente arrivent ensuite par <see cref="Purchased"/>.</summary>
        UniTask<bool> ConnectAsync(IReadOnlyList<string> productIds, CancellationToken ct);
        void Purchase(string productId);
        void Confirm(string productId, string transactionId);
    }

    /// <summary>
    /// Achats réels (Google Play, App Store) : tous les produits sont des consommables. La passerelle ne rend le succès qu'une fois le paiement passé,
    /// garde la transaction jusqu'à <see cref="Confirm"/> (appelée après l'octroi) et expose les transactions que personne n'attend pour qu'elles soient reprises.
    /// </summary>
    public sealed class UnityIapGateway : IPurchaseGateway, IPendingPurchaseSource, IDisposable
    {
        /// <summary>Les huit produits consommables à déclarer dans Google Play Console et App Store Connect (identifiants identiques).</summary>
        public static readonly IReadOnlyList<string> Products = new[] { "gems_100", "gems_520", "gems_1080", "gems_2250", "gems_6000", "gems_13000", "welcome", PassTrack.PremiumProduct };

        sealed class Entry { public string ProductId = ""; public string TransactionId = ""; public bool Claimed; }

        readonly IStoreAdapter _store;
        readonly List<Entry> _entries = new List<Entry>();
        UniTaskCompletionSource<Result>? _waiter;
        string _waitingFor = "";
        bool _connected;

        public event Action? PendingChanged;

        public UnityIapGateway(IStoreAdapter store)
        {
            _store = store;
            _store.Purchased += OnPurchased;
            _store.Failed += OnFailed;
        }

        /// <summary>Transactions payées que personne n'attend.</summary>
        public IReadOnlyList<PendingPurchase> Pending
            => _entries.Where(e => !e.Claimed).Select(e => new PendingPurchase(e.ProductId, e.TransactionId)).ToList();

        // L'achat que le joueur vient de faire (réclamé par le waiter) passe avant une transaction ancienne en attente.
        Entry? Find(string productId) => _entries.FirstOrDefault(e => e.ProductId == productId && e.Claimed) ?? _entries.FirstOrDefault(e => e.ProductId == productId);

        public string? TransactionOf(string productId) => Find(productId)?.TransactionId;

        /// <summary>Connexion au lancement : c'est elle qui fait remonter les transactions en attente.</summary>
        public async UniTask<bool> StartAsync(CancellationToken ct)
        {
            if (_connected) return true;
            _connected = await _store.ConnectAsync(Products, ct);
            return _connected;
        }

        public async UniTask<Result> PurchaseAsync(string productId, CancellationToken ct)
        {
            if (!Products.Contains(productId)) return Result.Fail(MetaErrors.Unknown);
            if (_waiter != null) return Result.Fail(MetaErrors.Busy);
            if (!await StartAsync(ct)) return Result.Fail(MetaErrors.StoreUnavailable);
            var waiter = new UniTaskCompletionSource<Result>();
            _waiter = waiter; _waitingFor = productId;
            try
            {
                using (ct.Register(() => waiter.TrySetCanceled()))
                {
                    _store.Purchase(productId);
                    return await waiter.Task;
                }
            }
            finally { _waiter = null; _waitingFor = ""; }
        }

        public void Confirm(string productId)
        {
            var e = Find(productId);
            if (e == null) return;
            _entries.Remove(e);
            _store.Confirm(e.ProductId, e.TransactionId);
        }

        void OnPurchased(string productId, string transactionId)
        {
            if (_entries.Any(e => e.TransactionId == transactionId && e.ProductId == productId)) return; // doublon de notification
            bool awaited = _waiter != null && _waitingFor == productId;
            _entries.Add(new Entry { ProductId = productId, TransactionId = transactionId, Claimed = awaited });
            if (awaited) _waiter!.TrySetResult(Result.Ok());
            else PendingChanged?.Invoke();
        }

        void OnFailed(string productId, StoreFailure failure)
        {
            if (_waiter == null || _waitingFor != productId) return;
            var error = failure == StoreFailure.Cancelled ? MetaErrors.PaymentCancelled
                      : failure == StoreFailure.Unavailable ? MetaErrors.StoreUnavailable
                      : MetaErrors.PaymentFailed;
            _waiter.TrySetResult(Result.Fail(error));
        }

        public void Dispose()
        {
            _store.Purchased -= OnPurchased;
            _store.Failed -= OnFailed;
            _store.Dispose();
        }
    }
}
