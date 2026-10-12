#nullable enable
using System.Collections.Generic;
using System.Linq;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;

namespace N29.HeroesDawn.Application.Features.Collection
{
    public static class CollectionErrors
    {
        public static readonly DomainError UnknownMember = new DomainError("collection.member", "errors.collection_member");
        public static readonly DomainError UnknownThing = new DomainError("collection.unknown", "errors.collection_unknown");
        public static readonly DomainError NoSlot = new DomainError("collection.no_slot", "errors.collection_no_slot");
        public static readonly DomainError NotOwned = new DomainError("collection.not_owned", "errors.collection_not_owned");
        public static readonly DomainError WrongFamily = new DomainError("collection.family", "errors.collection_family");
        public static readonly DomainError NeedFirstCard = new DomainError("collection.first_card", "errors.collection_first_card");
        public static readonly DomainError NothingEquipped = new DomainError("collection.nothing", "errors.collection_nothing");
        public static readonly DomainError SoldOut = new DomainError("collection.sold_out", "errors.collection_sold_out");
        public static readonly DomainError NoBooster = new DomainError("collection.no_booster", "errors.collection_no_booster");
        public static readonly DomainError NotForSale = new DomainError("collection.not_for_sale", "errors.collection_not_for_sale");
        public static readonly DomainError NotEnoughPr = new DomainError("collection.pr", "errors.not_enough_pr");
    }

    /// <summary>Une offre d'objet de la boutique du jour.</summary>
    /// <summary>Une carte tirée d'un booster : identifiant et « nouvelle » (absente de la collection avant l'ouverture).</summary>
    public readonly struct OpenedCard
    {
        public CardId Id { get; }
        public bool IsNew { get; }
        public OpenedCard(CardId id, bool isNew) { Id = id; IsNew = isNew; }
    }

    public sealed class ItemOffer
    {
        public ItemDef Item { get; }
        public bool Bought { get; }
        public ItemOffer(ItemDef item, bool bought) { Item = item; Bought = bought; }
    }

    /// <summary>Lecture et écriture de l'inventaire, du grimoire et des boosters dans la sauvegarde de la session.</summary>
    public static class CollectionOps
    {
        static CollectionSave Save(GameSession s) => s.Meta.collection;

        static int Get(List<CounterSave> list, string id)
        {
            foreach (var c in list) if (c.id == id) return c.value;
            return 0;
        }

        static void Add(List<CounterSave> list, string id, int delta)
        {
            foreach (var c in list)
                if (c.id == id) { c.value = System.Math.Max(0, c.value + delta); return; }
            if (delta > 0) list.Add(new CounterSave { id = id, value = delta });
        }

        // ---- Objets
        public static int OwnedItems(GameSession s, ItemId id) => Get(Save(s).items, id.Value);
        public static int EquippedItems(GameSession s, ItemId id) => s.Roster.Sum(m => m.Items.Count(i => i == id));
        public static int AvailableItems(GameSession s, ItemId id) => OwnedItems(s, id) - EquippedItems(s, id);
        public static void AddItem(GameSession s, ItemId id, int count = 1) => Add(Save(s).items, id.Value, count);

        // ---- Cartes
        public static int OwnedCards(GameSession s, CardId id) => Get(Save(s).cards, id.Value);
        public static int EquippedCards(GameSession s, CardId id) => s.Roster.Sum(m => m.Cards.Count(c => c.HasValue && c.Value == id));
        public static int AvailableCards(GameSession s, CardId id) => OwnedCards(s, id) - EquippedCards(s, id);
        public static void AddCard(GameSession s, CardId id, int count = 1) => Add(Save(s).cards, id.Value, count);

        // ---- Boosters
        public static int Boosters(GameSession s, string id) => Get(Save(s).boosters, id);
        public static void AddBooster(GameSession s, string id, int count = 1) => Add(Save(s).boosters, id, count);
    }

    /// <summary>Équipement des agents, boutique d'objets du jour, achat et ouverture des boosters.</summary>
    public sealed class CollectionBook
    {
        public const int ItemOfferCount = 4;

        readonly MetaContext _c;
        readonly IPublisher<RosterChanged> _roster;

        public CollectionBook(MetaContext context, IPublisher<RosterChanged> roster)
        {
            _c = context; _roster = roster;
        }

        GameSession Session => _c.Session;
        CollectionDef Defs => Session.Content.Collection;
        CollectionSave Save => Session.Meta.collection;

        TeamMember? Member(MemberId id) => Session.Find(id);

        /// <summary>Boosters en réserve (non ouverts).</summary>
        public int BoosterStock(string boosterId) => CollectionOps.Boosters(Session, boosterId);

        void Changed(bool roster)
        {
            if (roster) _roster.PublishRoster();
            _c.Touch(MetaArea.Collection);
        }

        // ------------------------------------------------------------------ Objets équipés

        public Result EquipItem(MemberId member, ItemId item)
        {
            var m = Member(member);
            if (m == null) return Result.Fail(CollectionErrors.UnknownMember);
            if (!Defs.HasItem(item)) return Result.Fail(CollectionErrors.UnknownThing);
            if (m.Items.Count >= CollectionRules.MaxItems) return Result.Fail(CollectionErrors.NoSlot);
            if (CollectionOps.AvailableItems(Session, item) <= 0) return Result.Fail(CollectionErrors.NotOwned);
            m.Items.Add(item);
            Changed(true);
            return Result.Ok();
        }

        public Result UnequipItem(MemberId member, ItemId item)
        {
            var m = Member(member);
            if (m == null) return Result.Fail(CollectionErrors.UnknownMember);
            if (!m.Items.Remove(item)) return Result.Fail(CollectionErrors.NothingEquipped);
            Changed(true);
            return Result.Ok();
        }

        // ------------------------------------------------------------------ Cartes équipées

        /// <summary>Équipe une carte en emplacement 3 (<paramref name="slot"/> 0) ou 4 (1) ; l'emplacement 4 demande une carte en 3.</summary>
        public Result EquipCard(MemberId member, int slot, CardId card)
        {
            var m = Member(member);
            if (m == null) return Result.Fail(CollectionErrors.UnknownMember);
            if (slot < 0 || slot >= m.Cards.Length) return Result.Fail(CollectionErrors.NoSlot);
            if (!Defs.HasCard(card)) return Result.Fail(CollectionErrors.UnknownThing);
            if (!CollectionRules.CanEquip(Defs.Card(card), m.Def)) return Result.Fail(CollectionErrors.WrongFamily);
            if (slot == 1 && !m.Cards[0].HasValue) return Result.Fail(CollectionErrors.NeedFirstCard);
            if (m.Cards[slot] != card && CollectionOps.AvailableCards(Session, card) <= 0) return Result.Fail(CollectionErrors.NotOwned);
            m.Cards[slot] = card;
            Changed(true);
            return Result.Ok();
        }

        public Result UnequipCard(MemberId member, int slot)
        {
            var m = Member(member);
            if (m == null) return Result.Fail(CollectionErrors.UnknownMember);
            if (slot < 0 || slot >= m.Cards.Length || !m.Cards[slot].HasValue) return Result.Fail(CollectionErrors.NothingEquipped);
            m.Cards[slot] = null;
            if (slot == 0 && m.Cards[1].HasValue) { m.Cards[0] = m.Cards[1]; m.Cards[1] = null; }
            Changed(true);
            return Result.Ok();
        }

        // ------------------------------------------------------------------ Boutique d'objets du jour

        void SyncShopDay()
        {
            if (Save.itemShopDay == _c.Today) return;
            Save.itemShopDay = _c.Today;
            Save.itemShopBought.Clear();
        }

        public IReadOnlyList<ItemOffer> ItemOffers()
        {
            SyncShopDay();
            var offers = CollectionRules.DailyItemOffers(Defs, _c.Today, ItemOfferCount);
            return offers.Select((o, i) => new ItemOffer(o, Save.itemShopBought.Contains(i))).ToList();
        }

        public Result BuyItem(int index)
        {
            SyncShopDay();
            var offers = CollectionRules.DailyItemOffers(Defs, _c.Today, ItemOfferCount);
            if (index < 0 || index >= offers.Count) return Result.Fail(CollectionErrors.UnknownThing);
            if (Save.itemShopBought.Contains(index)) return Result.Fail(CollectionErrors.SoldOut);
            var item = offers[index];
            if (!Session.Wallet.TrySpend(item.Price, 0)) return Result.Fail(MetaErrors.NotEnoughCredits);
            CollectionOps.AddItem(Session, item.Id);
            Save.itemShopBought.Add(index);
            _c.PublishWallet();
            Changed(false);
            return Result.Ok();
        }

        // ------------------------------------------------------------------ Boosters

        public Result BuyBooster(string boosterId)
        {
            if (!Defs.HasBooster(boosterId)) return Result.Fail(CollectionErrors.UnknownThing);
            var booster = Defs.Booster(boosterId);
            if (booster.PricePr <= 0) return Result.Fail(CollectionErrors.NotForSale);
            if (!Session.Wallet.TrySpend(0, booster.PricePr)) return Result.Fail(CollectionErrors.NotEnoughPr);
            CollectionOps.AddBooster(Session, boosterId);
            _c.PublishWallet();
            Changed(false);
            return Result.Ok();
        }

        /// <summary>Ouvre un booster de la réserve : les cartes tirées rejoignent la collection.</summary>
        public Result<IReadOnlyList<CardId>> OpenBooster(string boosterId)
        {
            if (!Defs.HasBooster(boosterId)) return Result<IReadOnlyList<CardId>>.Fail(CollectionErrors.UnknownThing);
            if (CollectionOps.Boosters(Session, boosterId) <= 0) return Result<IReadOnlyList<CardId>>.Fail(CollectionErrors.NoBooster);
            var booster = Defs.Booster(boosterId);
            string player = Session.Account?.PlayerId ?? "local";
            ulong seed = CollectionRules.Seed(player, "booster:" + boosterId, Save.opened);
            var opening = CollectionRules.OpenBooster(Defs, booster, seed, Save.pity);
            CollectionOps.AddBooster(Session, boosterId, -1);
            foreach (var card in opening.Cards) CollectionOps.AddCard(Session, card);
            Save.pity = opening.Pity;
            Save.opened++;
            Changed(false);
            return Result<IReadOnlyList<CardId>>.Ok(opening.Cards);
        }

        /// <summary>Comme <see cref="OpenBooster"/>, avec pour chaque carte tirée son caractère « nouvelle » (première exemplaire de la collection ; un doublon dans le même booster n'est plus nouveau).</summary>
        public Result<IReadOnlyList<OpenedCard>> OpenBoosterDetailed(string boosterId)
        {
            var owned = new Dictionary<CardId, int>();
            foreach (var c in Defs.Cards) owned[c.Id] = CollectionOps.OwnedCards(Session, c.Id);
            var r = OpenBooster(boosterId);
            if (r.IsFailure) return Result<IReadOnlyList<OpenedCard>>.Fail(r.Error!);
            var list = new List<OpenedCard>();
            foreach (var id in r.Value)
            {
                bool fresh = owned.TryGetValue(id, out var n) && n == 0;
                owned[id] = n + 1;
                list.Add(new OpenedCard(id, fresh));
            }
            return Result<IReadOnlyList<OpenedCard>>.Ok(list);
        }
    }
}
