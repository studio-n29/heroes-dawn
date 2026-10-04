#nullable enable
using System;
using Cysharp.Threading.Tasks;
using MessagePipe;
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
        bool _buying;

        public BoutiquePagePresenter(IBoutiquePageView view, Boutique shop, ISessionReadModel session, IModalService modals, ILocalization loc,
                                     IToastService toasts, ISubscriber<MetaChanged> meta, ISubscriber<WalletChanged> wallet,
                                     ISubscriber<RosterChanged> roster, IClock clock)
            : base(view, loc, toasts, meta, wallet, roster, clock)
        {
            _shop = shop; _session = session; _modals = modals;
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
            View.ShowRows(rows.ToArray());
        }

        protected override void OnHeader(int index)
        {
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
