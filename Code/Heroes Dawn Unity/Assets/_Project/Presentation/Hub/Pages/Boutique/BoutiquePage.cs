#nullable enable
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Collection;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Hub
{
    public interface IBoutiquePageView : IListPageView
    {
        /// <summary>Onglet de la boutique touché (index de <see cref="ShopTab"/>).</summary>
        event Action<int> ShopTabClicked;
        void ShowShopTab(int active);
    }

    /// <summary>
    /// Boutique unique à onglets : Héros (page Recruter), Boosters (stock, ouverture animée, achat), Objets du jour, Gemmes et crédits
    /// (paquets simulés, échanges), Cadeau du jour. L'onglet affiché est mémorisé par <see cref="HubNavigator.ShopTab"/>.
    /// </summary>
    public sealed class BoutiquePagePresenter : ListPagePresenter<IBoutiquePageView>
    {
        readonly Boutique _shop;
        readonly ISessionReadModel _session;
        readonly IModalService _modals;
        readonly CollectionBook _collection;
        readonly HubNavigator _nav;
        readonly List<Action> _actions = new List<Action>();
        bool _buying;
        bool _opening;

        public BoutiquePagePresenter(IBoutiquePageView view, Boutique shop, ISessionReadModel session, IModalService modals, ILocalization loc,
                                     IToastService toasts, ISubscriber<MetaChanged> meta, ISubscriber<WalletChanged> wallet,
                                     ISubscriber<RosterChanged> roster, IClock clock, CollectionBook collection, HubNavigator nav)
            : base(view, loc, toasts, meta, wallet, roster, clock)
        {
            _shop = shop; _session = session; _modals = modals; _collection = collection; _nav = nav;
        }

        public override string Title => Loc.T("page.shop");

        /// <summary>Onglet affiché : les héros ont leur propre page, on retombe sur les boosters.</summary>
        ShopTab Tab => _nav.ShopTab == ShopTab.Heroes ? ShopTab.Boosters : _nav.ShopTab;

        protected override void OnStart()
        {
            base.OnStart();
            View.ShopTabClicked += OnShopTab;
        }

        protected override void OnDispose()
        {
            View.ShopTabClicked -= OnShopTab;
            base.OnDispose();
        }

        void OnShopTab(int tab)
        {
            var t = (ShopTab)tab;
            if (t == ShopTab.Heroes) { _nav.SelectShopTab(t); return; }
            _nav.ShopTab = t;
            Refresh();
        }

        protected override void Render()
        {
            _actions.Clear();
            View.ShowShopTab((int)Tab);
            bool gift = _shop.DailyGiftAvailable;
            View.ShowHeader(new ListHeaderData
            {
                Title = Loc.F("shop.gems", _session.Wallet.Gems),
                Line = Loc.T("shop.simulated"),
                Progress = -1f,
                A = RowButtonData.Of(gift ? Loc.F("shop.gift", MetaFormat.RewardText(Boutique.DailyGift, Loc)) : Loc.T("shop.gift_done"), gift, Loc.T("shop.gift_done_reason"), 0),
                B = RowButtonData.Of(Loc.T("shop.grimoire"), true, "", 3),
            });

            var rows = new List<InfoRowData>();
            switch (Tab)
            {
                case ShopTab.Boosters: AddBoosters(rows); break;
                case ShopTab.Items: AddItems(rows); break;
                case ShopTab.Gems: AddGems(rows); break;
                default: AddGift(rows, gift); break;
            }
            View.ShowRows(rows.ToArray());
        }

        void Add(List<InfoRowData> rows, InfoRowData row, Action? action)
        {
            rows.Add(row);
            _actions.Add(action ?? (() => { }));
        }

        void AddBoosters(List<InfoRowData> rows)
        {
            var defs = _session.Content.Collection;
            var booster = defs.Booster(N29.HeroesDawn.Definitions.BoosterDef.StandardId);
            int stock = _collection.BoosterStock(N29.HeroesDawn.Definitions.BoosterDef.StandardId);
            var stockRow = InfoRowData.Basic(Loc.F("shop.booster_stock", stock), Loc.T("shop.booster_sub"));
            stockRow.Tag = "B"; stockRow.TagColor = Theme.Perception;
            stockRow.A = RowButtonData.Of(Loc.T("shop.booster_open"), stock > 0 && !_opening, Loc.T("shop.booster_none"), 0);
            stockRow.Highlight = stock > 0;
            Add(rows, stockRow, () => OpenBoosterAsync().Forget());

            var buyRow = InfoRowData.Basic(Loc.T("shop.booster_row"), Loc.F("shop.booster_cards", booster.Cards));
            buyRow.Tag = "PR"; buyRow.TagColor = Theme.Pr;
            int missing = booster.PricePr - _session.Wallet.Pr;
            buyRow.A = RowButtonData.Of(Loc.F("grimoire.buy", booster.PricePr), missing <= 0, Loc.F("grimoire.buy_reason", missing), 1);
            Add(rows, buyRow, () =>
            {
                if (Report(_collection.BuyBooster(N29.HeroesDawn.Definitions.BoosterDef.StandardId), Loc.T("grimoire.bought"))) Refresh();
            });
        }

        void AddItems(List<InfoRowData> rows)
        {
            var offers = _collection.ItemOffers();
            for (int i = 0; i < offers.Count; i++)
            {
                int index = i;
                var it = offers[i].Item;
                var r = InfoRowData.Basic(CollectionText.ItemName(Loc, it), CollectionText.RarityName(Loc, it.Rarity) + " · " + CollectionText.ItemEffects(Loc, it));
                r.Tag = CollectionText.Tag(CollectionText.ItemName(Loc, it));
                r.TagColor = CollectionText.RarityColor(it.Rarity);
                int lacking = it.Price - _session.Wallet.Credits;
                r.A = offers[i].Bought ? RowButtonData.Of(Loc.T("shop.item_sold"), false, "", 3)
                                       : RowButtonData.Of(Loc.F("shop.item_price", it.Price), lacking <= 0, Loc.F("shop.item_missing", lacking), 0);
                r.Dim = offers[i].Bought;
                Add(rows, r, () =>
                {
                    if (Report(_collection.BuyItem(index), Loc.F("shop.item_bought", CollectionText.ItemName(Loc, it)))) Refresh();
                });
            }
        }

        void AddGems(List<InfoRowData> rows)
        {
            foreach (var p in Boutique.Packs)
            {
                var pack = p;
                var r = InfoRowData.Basic(Loc.F("shop.pack", p.Gems), Loc.F("shop.pack_price", p.PriceText));
                r.Tag = "G"; r.TagColor = Theme.Perception;
                r.A = RowButtonData.Of(p.PriceText, !_buying, "", 0);
                Add(rows, r, () => BuyAsync(pack).Forget());
            }
            foreach (var e in Boutique.Exchanges)
            {
                var ex = e;
                var r = InfoRowData.Basic(Loc.T("shop." + e.Id), Loc.F("shop.cost_gems", e.GemCost));
                r.Tag = e.FullRest ? "+" : e.Reward.Pr > 0 ? "PR" : "CR";
                r.TagColor = e.FullRest ? Theme.Success : e.Reward.Pr > 0 ? Theme.Pr : Theme.Credits;
                bool can = _session.Wallet.Gems >= e.GemCost;
                r.A = RowButtonData.Of(Loc.F("shop.exchange", e.GemCost), can, Loc.F("shop.need_gems", e.GemCost - _session.Wallet.Gems), 1);
                Add(rows, r, () => Report(_shop.Exchange(ex.Id), Loc.T("shop.exchanged")));
            }
        }

        void AddGift(List<InfoRowData> rows, bool available)
        {
            var r = InfoRowData.Basic(Loc.T("shop.gift_title"), MetaFormat.RewardText(Boutique.DailyGift, Loc));
            r.Tag = "+"; r.TagColor = Theme.Credits;
            r.Highlight = available;
            r.Dim = !available;
            r.A = RowButtonData.Of(available ? Loc.T("shop.gift_claim") : Loc.T("shop.gift_done"), available, Loc.T("shop.gift_done_reason"), 0);
            Add(rows, r, ClaimGift);
        }

        void ClaimGift() => Report(_shop.ClaimDailyGift(), "shop.gift_taken");

        protected override void OnHeader(int index)
        {
            if (index == 1) { _nav.Open(HubPage.Grimoire); return; }
            ClaimGift();
        }

        protected override void OnRowButton(int row, int button)
        {
            if (row >= 0 && row < _actions.Count) _actions[row]();
        }

        async UniTaskVoid OpenBoosterAsync()
        {
            if (_opening) return;
            _opening = true;
            try
            {
                await BoosterOpening.OpenAndShowAsync(_collection, _session.Content.Collection, _modals, Loc, Toasts, Lifetime);
            }
            catch (OperationCanceledException) { }
            finally { _opening = false; if (!Disposed) Refresh(); }
        }

        async UniTaskVoid BuyAsync(GemPack pack)
        {
            if (_buying) return;
            _buying = true;
            try
            {
                bool ok = await _modals.ConfirmAsync(new ConfirmData(Loc.T("shop.confirm.title"), Loc.F("shop.confirm.body", pack.Gems, pack.PriceText),
                                                                     Loc.T("shop.confirm.ok"), Loc.T("ui.cancel"), Tone.Primary), Lifetime);
                if (!ok || Disposed) return;
                var r = await _shop.BuyPackAsync(pack.Id, Lifetime);
                if (Disposed) return;
                if (r.IsFailure) Toasts.Show(Loc.Error(r.Error!), ToastKind.Error);
                else Toasts.Show(Loc.F("shop.bought", r.Value), ToastKind.Reward);
            }
            catch (OperationCanceledException) { }
            finally { _buying = false; if (!Disposed) Refresh(); }
        }
    }
}
