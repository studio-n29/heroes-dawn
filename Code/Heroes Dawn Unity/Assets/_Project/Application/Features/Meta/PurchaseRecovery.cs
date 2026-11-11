#nullable enable
using System.Linq;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;

namespace N29.HeroesDawn.Application.Features.Meta
{
    /// <summary>Mémoire des transactions déjà octroyées : une transaction reprise après un arrêt entre l'octroi et la confirmation n'est jamais octroyée deux fois.</summary>
    public static class PurchaseLedger
    {
        public const int Keep = 100;

        /// <summary>Vrai (et note la transaction) si elle n'a pas encore été octroyée ; une transaction sans identifiant est toujours acceptée.</summary>
        public static bool TryRecord(MetaSave m, string? transactionId)
        {
            if (string.IsNullOrEmpty(transactionId)) return true;
            if (m.grantedTransactions.Contains(transactionId!)) return false;
            m.grantedTransactions.Add(transactionId!);
            if (m.grantedTransactions.Count > Keep) m.grantedTransactions.RemoveAt(0);
            return true;
        }
    }

    /// <summary>
    /// Reprend les achats payés mais pas encore octroyés (appli fermée entre le paiement et l'octroi) : octroi puis confirmation au store, une seule fois par transaction.
    /// À appeler quand un profil est chargé et chaque fois que la passerelle annonce de nouvelles transactions en attente.
    /// </summary>
    public sealed class PurchaseRecovery
    {
        readonly IPurchaseGateway _gateway;
        readonly Boutique _shop;
        readonly PassProgress _pass;
        readonly GameSession _session;
        bool _running;

        public PurchaseRecovery(IPurchaseGateway gateway, Boutique shop, PassProgress pass, GameSession session)
        {
            _gateway = gateway; _shop = shop; _pass = pass; _session = session;
        }

        /// <summary>Nombre de transactions reprises ; 0 sans compte connecté (le profil n'est pas encore chargé) ou sans transaction en attente.</summary>
        public int Resume()
        {
            if (_running || _session.Account == null || !(_gateway is IPendingPurchaseSource source)) return 0;
            _running = true;
            try
            {
                int done = 0;
                foreach (var p in source.Pending.ToList())
                {
                    if (p.ProductId == PassTrack.PremiumProduct) _pass.GrantPremium(p.TransactionId);
                    else
                    {
                        var pack = Boutique.FindPack(p.ProductId);
                        if (pack == null) continue; // produit inconnu de cette version : laissé en attente
                        _shop.Deliver(pack, p.TransactionId);
                    }
                    source.Confirm(p.ProductId);
                    done++;
                }
                return done;
            }
            finally { _running = false; }
        }
    }
}
