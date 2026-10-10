#nullable enable
using System;
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
    public interface IBoutiquePageView : IListPageView { }

    public sealed class BoutiquePagePresenter : ListPagePresenter<IBoutiquePageView>
    {
        readonly Boutique _shop;
        readonly ISessionReadModel _session;
        readonly IModalService _modals;
        readonly CollectionBook _collection;
        readonly HubNavigator _nav;
        bool _buying;

        public BoutiquePagePresenter(IBoutiquePageView view, Boutique shop, ISessionReadModel session, IModalService modals, ILocalization loc,
                                     IToastService toasts, ISubscriber<MetaChanged> meta, ISubscriber<WalletChanged> wallet,
                                     ISubscriber<RosterChanged> roster, IClock clock, CollectionBook collection, HubNavigator nav)
            : base(view, loc, toasts, meta, wallet, roster, clock)
        {
            _shop = shop; _session = session; _modals = modals; _collection = collection; _nav = nav;
        }

        public override string Title => Loc.T("page.shop");

        protected override void Render()
        {
            bool gift = _shop.DailyGiftAvailable;
            View.ShowHeader(new ListHeaderData
            {
                Title = Loc.F("shop.gems", _session.Wallet.Gems),
                Line = Loc.T("shop.simulated"),
                Progress = -1f,
                A = RowButtonData.Of(gift ? Loc.F("shop.gift", MetaFormat.RewardText(Boutique.DailyGift, Loc)) : Loc.T("shop.gift_done"), gift, Loc.T("shop.gift_done_reason"), 0),
                B = RowButtonData.Of(Loc.T("shop.grimoire"), true, "", 3),
            });

            var rows = new System.Collections.Generic.List<InfoRowData>();
            foreach (var p in Boutique.Packs)
            {
                var r = InfoRowData.Basic(Loc.F("shop.pack", p.Gems), Loc.F("shop.pack_price", p.PriceText));
                r.Tag = "G"; r.TagColor = Theme.Perception;
                r.A = RowButtonData.Of(p.PriceText, !_buying, "", 0);
                rows.Add(r);
            }
            foreach (var e in Boutique.Exchanges)
            {
                var r = InfoRowData.Basic(Loc.T("shop." + e.Id), Loc.F("shop.cost_gems", e.GemCost));
                r.Tag = e.FullRest ? "+" : e.Reward.Pr > 0 ? "PR" : "CR";
                r.TagColor = e.FullRest ? Theme.Success : e.Reward.Pr > 0 ? Theme.Pr : Theme.Credits;
                bool can = _session.Wallet.Gems >= e.GemCost;
                r.A = RowButtonData.Of(Loc.F("shop.exchange", e.GemCost), can, Loc.F("shop.need_gems", e.GemCost - _session.Wallet.Gems), 1);
                rows.Add(r);
            }
            // Objets du jour (rotation quotidienne) puis booster de cartes.
            var title = InfoRowData.Basic(Loc.T("shop.items_title"));
            title.Dim = true;
            rows.Add(title);
            var offers = _collection.ItemOffers();
            for (int i = 0; i < offers.Count; i++)
            {
                var it = offers[i].Item;
                var r = InfoRowData.Basic(CollectionText.ItemName(Loc, it), CollectionText.RarityName(Loc, it.Rarity) + " · " + CollectionText.ItemEffects(Loc, it));
                r.Tag = CollectionText.Tag(CollectionText.ItemName(Loc, it));
                r.TagColor = CollectionText.RarityColor(it.Rarity);
                int lacking = it.Price - _session.Wallet.Credits;
                r.A = offers[i].Bought ? RowButtonData.Of(Loc.T("shop.item_sold"), false, "", 3)
                                       : RowButtonData.Of(Loc.F("shop.item_price", it.Price), lacking <= 0, Loc.F("shop.item_missing", lacking), 0);
                r.Dim = offers[i].Bought;
                rows.Add(r);
            }
            var booster = _session.Content.Collection.Booster(N29.HeroesDawn.Definitions.BoosterDef.StandardId);
            var br = InfoRowData.Basic(Loc.T("shop.booster_row"), Loc.T("shop.booster_sub"));
            br.Tag = "B"; br.TagColor = Theme.Perception;
            int prMissing = booster.PricePr - _session.Wallet.Pr;
            br.A = RowButtonData.Of(Loc.F("grimoire.buy", booster.PricePr), prMissing <= 0, Loc.F("grimoire.buy_reason", prMissing), 1);
            rows.Add(br);
            View.ShowRows(rows.ToArray());
        }

        protected override void OnHeader(int index)
        {
            if (index == 1) { _nav.Open(HubPage.Grimoire); return; }
            Report(_shop.ClaimDailyGift(), "shop.gift_taken");
        }

        protected override void OnRowButton(int row, int button)
        {
            int packs = Boutique.Packs.Count;
            if (row < packs) BuyAsync(Boutique.Packs[row]).Forget();
            else if (row - packs < Boutique.Exchanges.Count)
            {
                var ex = Boutique.Exchanges[row - packs];
                if (Report(_shop.Exchange(ex.Id), Loc.T("shop.exchanged"))) { }
            }
            else
            {
                int k = row - packs - Boutique.Exchanges.Count - 1; // 0 : titre de section
                if (k >= 0 && k < CollectionBook.ItemOfferCount)
                {
                    var offer = _collection.ItemOffers()[k];
                    if (Report(_collection.BuyItem(k), Loc.F("shop.item_bought", CollectionText.ItemName(Loc, offer.Item)))) Refresh();
                }
                else if (k == CollectionBook.ItemOfferCount)
                {
                    if (Report(_collection.BuyBooster(N29.HeroesDawn.Definitions.BoosterDef.StandardId), Loc.T("grimoire.bought"))) Refresh();
                }
            }
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
