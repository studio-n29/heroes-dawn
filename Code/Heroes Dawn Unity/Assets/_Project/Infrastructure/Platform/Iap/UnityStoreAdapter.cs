#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.Purchasing;

namespace N29.HeroesDawn.Infrastructure.Platform
{
    /// <summary>
    /// Adaptateur du SDK Unity IAP 5.x (<see cref="StoreController"/>) pour <see cref="UnityIapGateway"/>. Tous les produits sont des consommables.
    /// Jamais exécuté contre un vrai store depuis ce dépôt : il faut déclarer les produits dans Google Play Console et App Store Connect puis tester en bac à sable sur appareil.
    /// Le SDK contrôle la signature des reçus (Google Play : <c>SignatureInvalid</c>) avant de livrer la commande ; aucune validation serveur pour la 1.0.0.
    /// </summary>
    public sealed class UnityStoreAdapter : IStoreAdapter
    {
        StoreController? _controller;
        readonly Dictionary<string, PendingOrder> _orders = new Dictionary<string, PendingOrder>();

        public event Action<string, string>? Purchased;
        public event Action<string, StoreFailure>? Failed;

        public async UniTask<bool> ConnectAsync(IReadOnlyList<string> productIds, CancellationToken ct)
        {
            try
            {
                var controller = new StoreController();
                controller.OnPurchasePending += OnPending;
                controller.OnPurchaseFailed += OnFailed;
                var fetched = new UniTaskCompletionSource<bool>();
                controller.OnProductsFetched += _ => fetched.TrySetResult(true);
                controller.OnProductsFetchFailed += _ => fetched.TrySetResult(false);
                _controller = controller;

                await controller.Connect();
                ct.ThrowIfCancellationRequested();
                controller.FetchProducts(productIds.Select(id => new ProductDefinition(id, ProductType.Consumable)).ToList());
                if (!await fetched.Task) return false;
                controller.FetchPurchases(); // fait remonter les commandes payées jamais confirmées (OnPurchasePending)
                return true;
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception) { return false; }
        }

        public void Purchase(string productId)
        {
            var product = _controller?.GetProductById(productId);
            if (_controller == null || product == null) { Failed?.Invoke(productId, StoreFailure.Unavailable); return; }
            _controller.PurchaseProduct(product);
        }

        public void Confirm(string productId, string transactionId)
        {
            if (_controller == null || !_orders.TryGetValue(transactionId, out var order)) return;
            _orders.Remove(transactionId);
            _controller.ConfirmPurchase(order);
        }

        void OnPending(PendingOrder order)
        {
            var productId = ProductOf(order);
            var transaction = order.Info.TransactionID;
            if (productId == null || string.IsNullOrEmpty(transaction)) return;
            _orders[transaction] = order;
            Purchased?.Invoke(productId, transaction);
        }

        void OnFailed(FailedOrder order)
        {
            var productId = ProductOf(order);
            if (productId == null) return;
            var kind = order.FailureReason == PurchaseFailureReason.UserCancelled ? StoreFailure.Cancelled
                     : order.FailureReason == PurchaseFailureReason.PurchasingUnavailable || order.FailureReason == PurchaseFailureReason.ProductUnavailable ? StoreFailure.Unavailable
                     : StoreFailure.Other;
            Failed?.Invoke(productId, kind);
        }

        static string? ProductOf(Order order)
        {
            var items = order.CartOrdered?.Items();
            return items == null || items.Count == 0 ? null : items[0].Product?.definition?.id;
        }

        public void Dispose()
        {
            if (_controller == null) return;
            _controller.OnPurchasePending -= OnPending;
            _controller.OnPurchaseFailed -= OnFailed;
            _controller = null;
        }
    }
}
