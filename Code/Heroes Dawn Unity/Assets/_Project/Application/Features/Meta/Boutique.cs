#nullable enable
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Application.Features.Meta
{
    /// <summary>Paiement d'un produit en argent réel (M1 : simulé, aucun débit ; M2 : stores / UGS).</summary>
    public interface IPurchaseGateway
    {
        UniTask<Result> PurchaseAsync(string productId, CancellationToken ct);
    }

    /// <summary>
    /// Transactions payées mais pas encore confirmées au store (appli fermée entre le paiement et l'octroi) : elles sont reprises au lancement suivant,
    /// l'octroi se fait une fois, puis <see cref="Confirm"/> les retire. Seule la passerelle réelle en a ; le simulateur n'a rien en attente.
    /// </summary>
    public interface IPendingPurchaseSource
    {
        /// <summary>Transactions payées que personne n'attend (reprises au lancement), de la plus ancienne à la plus récente.</summary>
        IReadOnlyList<PendingPurchase> Pending { get; }
        /// <summary>Identifiant de la transaction non confirmée la plus ancienne de ce produit (pour ne jamais octroyer deux fois la même).</summary>
        string? TransactionOf(string productId);
        /// <summary>Appelé après l'octroi : confirme la plus ancienne transaction de ce produit, le store peut la clore.</summary>
        void Confirm(string productId);
        event System.Action? PendingChanged;
    }

    public readonly struct PendingPurchase
    {
        public readonly string ProductId, TransactionId;
        public PendingPurchase(string productId, string transactionId) { ProductId = productId; TransactionId = transactionId; }
    }

    public sealed class GemPack
    {
        public string Id { get; }
        public int Gems { get; }
        public string PriceText { get; }
        public GemPack(string id, int gems, string priceText) { Id = id; Gems = gems; PriceText = priceText; }
    }

    public sealed class GemExchange
    {
        public string Id { get; }
        public int GemCost { get; }
        public Reward Reward { get; }
        /// <summary>Repos complet : toute l'équipe retombe à 0 de fatigue.</summary>
        public bool FullRest { get; }
        public GemExchange(string id, int gemCost, Reward reward, bool fullRest = false) { Id = id; GemCost = gemCost; Reward = reward; FullRest = fullRest; }
    }

    /// <summary>Boutique : paquets de Gemmes (achat simulé), échanges Gemmes → ressources, cadeau quotidien gratuit.</summary>
    public sealed class Boutique
    {
        public static readonly Reward DailyGift = new Reward(credits: 100, gems: 1);

        public static readonly IReadOnlyList<GemPack> Packs = new[]
        {
            new GemPack("gems_100", 100, "0,99 €"),
            new GemPack("gems_520", 520, "4,99 €"),
            new GemPack("gems_1080", 1080, "9,99 €"),
            new GemPack("gems_2250", 2250, "19,99 €"),
            new GemPack("gems_6000", 6000, "49,99 €"),
        };

        /// <summary>Offre de bienvenue (une seule fois par profil) : 500 Gemmes et un accélérateur de 8 heures. Le texte du prix ne sert qu'au simulateur : le vrai prix vient du store.</summary>
        public static readonly GemPack Welcome = new GemPack("welcome", 500, "2,99 €");
        public const int WelcomeSpeedUpMinutes = 480;

        public static readonly IReadOnlyList<GemExchange> Exchanges = new[]
        {
            new GemExchange("ex_credits", 20, new Reward(credits: 1000)),
            new GemExchange("ex_pr", 30, new Reward(pr: 60)),
            new GemExchange("ex_rest", 10, new Reward(), fullRest: true),
        };

        /// <summary>Prix en Gemmes du n-ième emplacement d'expédition (le 4e et le 5e s'achètent) ; 0 pour les emplacements de base.</summary>
        public static int SlotPrice(int slotNumber) => slotNumber == 4 ? 300 : slotNumber == 5 ? 500 : 0;

        readonly MetaContext _c;
        readonly IPurchaseGateway _payments;
        readonly IPublisher<RosterChanged> _roster;

        public Boutique(MetaContext context, IPurchaseGateway payments, IPublisher<RosterChanged> roster)
        {
            _c = context; _payments = payments; _roster = roster;
        }

        public bool DailyGiftAvailable => _c.Meta.dailyGiftDay != _c.Today;

        public Result<Reward> ClaimDailyGift()
        {
            if (!DailyGiftAvailable) return Result<Reward>.Fail(MetaErrors.AlreadyClaimed);
            _c.Meta.dailyGiftDay = _c.Today;
            _c.Grant(DailyGift);
            _c.Touch(MetaArea.Boutique);
            return Result<Reward>.Ok(DailyGift);
        }

        public async UniTask<Result<int>> BuyPackAsync(string packId, CancellationToken ct)
        {
            var pack = FindPack(packId);
            if (pack == null) return Result<int>.Fail(MetaErrors.Unknown);
            // L'offre de bienvenue déjà prise est refusée avant tout paiement.
            if (pack.Id == Welcome.Id && _c.Meta.welcomeClaimed) return Result<int>.Fail(MetaErrors.AlreadyDone);
            var paid = await _payments.PurchaseAsync(pack.Id, ct);
            ct.ThrowIfCancellationRequested();
            if (paid.IsFailure) return Result<int>.Fail(paid.Error!);
            var ledger = _payments as IPendingPurchaseSource;
            Deliver(pack, ledger?.TransactionOf(pack.Id));
            ledger?.Confirm(pack.Id);
            return Result<int>.Ok(pack.Gems);
        }

        public static GemPack? FindPack(string productId)
        {
            if (productId == Welcome.Id) return Welcome;
            foreach (var p in Packs) if (p.Id == productId) return p;
            return null;
        }

        /// <summary>Verse un pack payé (jamais appelé sans confirmation du store). L'offre de bienvenue ajoute son accélérateur et ne se verse qu'une fois.</summary>
        public void Deliver(GemPack pack, string? transactionId = null)
        {
            if (!PurchaseLedger.TryRecord(_c.Meta, transactionId)) return; // déjà octroyée (arrêt entre l'octroi et la confirmation)
            bool firstWelcome = pack.Id == Welcome.Id && !_c.Meta.welcomeClaimed;
            _c.Grant(new Reward(gems: pack.Gems));
            if (firstWelcome) { _c.Meta.welcomeClaimed = true; _c.Meta.speedUpTokens += WelcomeSpeedUpMinutes; }
            _c.Meta.SetCounter(_c.Meta.purchases, pack.Id, _c.Meta.GetCounter(_c.Meta.purchases, pack.Id) + 1);
            _c.Touch(MetaArea.Boutique);
        }

        /// <summary>Achète le prochain emplacement d'expédition (achat permanent, en Gemmes) ; refusé au maximum ou sans assez de Gemmes, sans rien dépenser.</summary>
        public Result BuyExpeditionSlot()
        {
            int next = Expeditions.SlotCount(_c.Meta) + 1;
            if (next > Expeditions.MaxSlots) return Result.Fail(MetaErrors.MaxLevel);
            if (!_c.Session.Wallet.TrySpendGems(SlotPrice(next))) return Result.Fail(MetaErrors.NotEnoughGems);
            _c.Meta.expeditionSlotsBought = next - Expeditions.BaseSlots;
            _c.PublishWallet();
            _c.Touch(MetaArea.Operations);
            _c.Touch(MetaArea.Boutique);
            return Result.Ok();
        }

        public Result Exchange(string id)
        {
            GemExchange? ex = null;
            foreach (var e in Exchanges) if (e.Id == id) ex = e;
            if (ex == null) return Result.Fail(MetaErrors.Unknown);
            if (ex.FullRest)
            {
                bool tired = false;
                foreach (var m in _c.Session.Roster) if (m.Fatigue > 0) tired = true;
                if (!tired) return Result.Fail(Hub.HubErrors.NothingToHeal);
            }
            if (!_c.Session.Wallet.TrySpendGems(ex.GemCost)) return Result.Fail(MetaErrors.NotEnoughGems);
            if (ex.FullRest)
            {
                foreach (var m in _c.Session.Roster) m.Fatigue = 0;
                _roster.PublishRoster();
            }
            _c.Grant(ex.Reward);
            _c.Touch(MetaArea.Boutique);
            return Result.Ok();
        }
    }
}
