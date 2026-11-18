#nullable enable
using System;
using UnityEngine;
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
        readonly Dictionary<int, Action> _openActions = new Dictionary<int, Action>();
        bool _buying;
        bool _opening;
        readonly N29.HeroesDawn.Presentation.Audio.IAudioService? _audio;
        readonly CosmeticShop? _cosmetics;

        public BoutiquePagePresenter(IBoutiquePageView view, Boutique shop, ISessionReadModel session, IModalService modals, ILocalization loc,
                                     IToastService toasts, ISubscriber<MetaChanged> meta, ISubscriber<WalletChanged> wallet,
                                     ISubscriber<RosterChanged> roster, IClock clock, CollectionBook collection, HubNavigator nav, N29.HeroesDawn.Presentation.Audio.IAudioService? audio = null,
                                     CosmeticShop? cosmetics = null)
            : base(view, loc, toasts, meta, wallet, roster, clock)
        {
            _shop = shop; _session = session; _modals = modals; _collection = collection; _nav = nav; _audio = audio; _cosmetics = cosmetics;
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
            _openActions.Clear();
            View.ShowShopTab((int)Tab);
            View.ShowHeader(new ListHeaderData
            {
                Title = Loc.T("page.shop"),
                Line = Loc.F("shop.wallet", _session.Wallet.Credits, _session.Wallet.Pr, _session.Wallet.Gems),
                Progress = -1f,
                // Le cadeau du jour se réclame dans le Pass (L2 de la 0.15.0).
                A = RowButtonData.Of(Loc.T("shop.daily_pass"), true, "", 3),
                B = RowButtonData.Of(Loc.T("shop.grimoire"), true, "", 3),
            });

            var rows = new List<InfoRowData>();
            switch (Tab)
            {
                case ShopTab.Boosters: AddBoosters(rows); break;
                case ShopTab.Gift: AddGift(rows); break;
                default: AddGems(rows); break;
            }
            View.ShowRows(rows.ToArray());
        }

        void Add(List<InfoRowData> rows, InfoRowData row, Action? action)
        {
            rows.Add(row);
            _actions.Add(action ?? (() => { }));
        }

        static readonly string[] PackIds =
        {
            N29.HeroesDawn.Definitions.BoosterDef.StandardId,
            N29.HeroesDawn.Definitions.BoosterDef.OperationId,
            N29.HeroesDawn.Definitions.BoosterDef.MissionId,
        };

        /// <summary>Une tuile par pack : contenu et chances en fiche, achat (bouton A) et ouverture de la réserve (bouton B).</summary>
        void AddBoosters(List<InfoRowData> rows)
        {
            var defs = _session.Content.Collection;
            foreach (var id in PackIds)
            {
                if (!defs.HasBooster(id)) continue;
                var packId = id;
                var booster = defs.Booster(packId);
                int stock = _collection.BoosterStock(packId);
                int sum = 0;
                foreach (int w in booster.Weights) sum += w;
                int rare = 0;
                for (int i = 1; i < booster.Weights.Count; i++) rare += booster.Weights[i];
                int rarePercent = sum > 0 ? Mathf.RoundToInt(100f * rare / sum) : 0;
                string contents = Loc.T("shop.pack_contents") + "\n• " + Loc.F("shop.pack_cards", booster.Cards)
                                  + "\n• " + Loc.F("shop.pack_chances", rarePercent, booster.PityEvery)
                                  + "\n\n" + Loc.F("shop.pack_stock", stock);
                var row = InfoRowData.Basic(Loc.T("boosters." + packId + ".name"), contents);
                row.Tag = "PR"; row.TagColor = packId == N29.HeroesDawn.Definitions.BoosterDef.OperationId ? Theme.Info : packId == N29.HeroesDawn.Definitions.BoosterDef.MissionId ? Theme.Accent : Theme.Perception;
                row.Art = BoosterArt(packId);
                row.Badge = stock > 0 ? booster.PricePr + " PR · ×" + stock : booster.PricePr + " PR";
                row.Highlight = stock > 0 && packId == N29.HeroesDawn.Definitions.BoosterDef.StandardId;
                int missing = booster.PricePr - _session.Wallet.Pr;
                row.A = RowButtonData.Of(Loc.F("shop.pack_buy", booster.PricePr), missing <= 0, Loc.F("grimoire.buy_reason", missing), 0);
                row.B = RowButtonData.Of(Loc.F("shop.pack_open", stock), stock > 0 && !_opening, Loc.T("shop.booster_none"), 0);
                Add(rows, row, () =>
                {
                    if (Report(_collection.BuyBooster(packId), Loc.T("grimoire.bought"))) Refresh();
                });
                _openActions[rows.Count - 1] = () => OpenBoosterAsync(packId).Forget();
            }
        }

        /// <summary>Onglet Cadeau : le cadeau du jour (même état que dans le Pass).</summary>
        void AddGift(List<InfoRowData> rows)
        {
            bool gift = _shop.DailyGiftAvailable;
            var r = InfoRowData.Basic(Loc.T("shop.gift_title"), MetaFormat.RewardText(Boutique.DailyGift, Loc) + "\n\n" + Loc.T("shop.gift_sub"));
            r.Tag = "+"; r.TagColor = gift ? Theme.Credits : Theme.Surface3;
            r.Art = AgentArt.Shop("gems_small");
            r.Badge = gift ? MetaFormat.RewardText(Boutique.DailyGift, Loc) : Loc.T("shop.gift_done");
            r.Highlight = gift;
            r.Dim = !gift;
            r.A = RowButtonData.Of(gift ? Loc.T("shop.gift_claim") : Loc.T("shop.gift_done"), gift, Loc.T("shop.gift_done_reason"), 0);
            Add(rows, r, () => { if (Report(_shop.ClaimDailyGift(), "shop.gift_taken")) Refresh(); });
        }

        /// <summary>Titre de catégorie (sans bouton) : sépare les paquets achetés des échanges de gemmes.</summary>
        void AddSection(List<InfoRowData> rows, string title, string subtitle, string art)
        {
            var r = InfoRowData.Basic(title, subtitle);
            r.Tag = ""; r.TagColor = Theme.Primary;
            r.Art = AgentArt.Shop(art);
            r.Dim = true;
            Add(rows, r, null);
        }

        void AddGems(List<InfoRowData> rows)
        {
            AddSection(rows, Loc.T("shop.cat.gems"), Loc.T("shop.cat.gems_sub"), "gems_medium");
            foreach (var p in Boutique.Packs)
            {
                var pack = p;
                var r = InfoRowData.Basic(Loc.F("shop.pack", p.Gems), Loc.F("shop.desc.pack", p.Gems, p.PriceText));
                r.Tag = "G"; r.TagColor = Theme.Perception;
                r.Art = AgentArt.Shop(PackArt(p.Id));
                r.A = RowButtonData.Of(p.PriceText, !_buying, "", 0);
                r.Badge = p.PriceText;
                Add(rows, r, () => BuyAsync(pack).Forget());
            }
            AddSection(rows, Loc.T("shop.cat.comfort"), Loc.T("shop.cat.comfort_sub"), "exchange_rest");
            AddAccelerator(rows);
            AddExpeditionSlot(rows);
            AddSection(rows, Loc.T("shop.cat.exchange"), Loc.T("shop.cat.exchange_sub"), "exchange_credits");
            foreach (var e in Boutique.Exchanges)
            {
                var ex = e;
                string gain = e.FullRest ? Loc.T("shop.desc.ex_rest") : MetaFormat.RewardText(e.Reward, Loc);
                int left = _shop.ExchangesLeftToday(e);
                string limit = e.DailyLimit > 0 ? " · " + Loc.F("shop.limit_today", Math.Min(left, e.DailyLimit), e.DailyLimit) : "";
                var r = InfoRowData.Basic(Loc.T("shop." + e.Id), gain + " · " + Loc.F("shop.cost_gems", e.GemCost) + limit);
                r.Art = AgentArt.Shop(ExchangeArt(e.Id));
                r.Tag = e.FullRest ? "+" : "CR";
                r.TagColor = e.FullRest ? Theme.Success : Theme.Credits;
                bool can = _session.Wallet.Gems >= e.GemCost && left > 0;
                r.Badge = e.GemCost + " GM";
                string why = left <= 0 ? Loc.T("shop.limit_reason") : Loc.F("shop.need_gems", e.GemCost - _session.Wallet.Gems);
                r.A = RowButtonData.Of(Loc.F("shop.exchange", e.GemCost), can, why, 1);
                Add(rows, r, () => Report(_shop.Exchange(ex.Id), Loc.T("shop.exchanged")));
            }
            AddCosmetics(rows);
        }

        /// <summary>Accélérateur de 8 heures : du temps gagné, jamais de la puissance (pay-to-fast).</summary>
        void AddAccelerator(List<InfoRowData> rows)
        {
            int cost = Boutique.AcceleratorGems, hours = Boutique.AcceleratorMinutes / 60;
            var r = InfoRowData.Basic(Loc.F("shop.accel", hours), Loc.F("shop.desc.accel", hours, cost));
            r.Tag = "+"; r.TagColor = Theme.Info;
            r.Art = AgentArt.Shop("exchange_rest");
            r.Badge = cost + " GM";
            r.A = RowButtonData.Of(Loc.F("shop.exchange", cost), _session.Wallet.Gems >= cost, Loc.F("shop.need_gems", cost - _session.Wallet.Gems), 1);
            Add(rows, r, () => Report(_shop.BuyAccelerator(), Loc.T("shop.accel_bought")));
        }

        /// <summary>4e et 5e emplacements d'expédition (achat permanent) : plus d'expéditions en parallèle, pas plus de puissance.</summary>
        void AddExpeditionSlot(List<InfoRowData> rows)
        {
            int next = _shop.NextSlotNumber;
            if (next > Expeditions.MaxSlots) return;
            int cost = Boutique.SlotPrice(next);
            var r = InfoRowData.Basic(Loc.F("shop.slot", next), Loc.F("shop.desc.slot", next, cost));
            r.Tag = "+"; r.TagColor = Theme.Info;
            r.Art = AgentArt.Shop("exchange_rest");
            r.Badge = cost + " GM";
            r.A = RowButtonData.Of(Loc.F("shop.exchange", cost), _session.Wallet.Gems >= cost, Loc.F("shop.need_gems", cost - _session.Wallet.Gems), 1);
            Add(rows, r, () => ConfirmThenAsync(Loc.F("shop.slot.confirm.title", next), Loc.F("shop.slot.confirm.body", next, cost),
                                                () => _shop.BuyExpeditionSlot(), Loc.T("shop.slot_bought")).Forget());
        }

        /// <summary>Rotation hebdomadaire de cosmétiques (tenues, thèmes de QG, dés, pions) : l'apparence seule, aucune statistique.</summary>
        void AddCosmetics(List<InfoRowData> rows)
        {
            if (_cosmetics == null) return;
            AddSection(rows, Loc.T("shop.cat.cosmetics"), Loc.T("shop.cat.cosmetics_sub"), "gift");
            var offers = _cosmetics.WeeklyNow();
            if (offers.Count == 0)
            {
                var none = InfoRowData.Basic(Loc.T("shop.cosmetics_none"), Loc.T("shop.cosmetics_none_sub"));
                none.Tag = ""; none.TagColor = Theme.Surface3; none.Dim = true;
                none.Art = AgentArt.Shop("gift");
                Add(rows, none, null);
                return;
            }
            foreach (var o in offers)
            {
                string id = o.Def.Id; int price = o.Def.Price;
                string name = Loc.Has(o.Def.NameKey) ? Loc.T(o.Def.NameKey) : id;
                var r = InfoRowData.Basic(name, Loc.F("shop.desc.cosmetic", price));
                r.Tag = "C"; r.TagColor = Theme.Accent;
                r.Art = AgentArt.Shop("gift");
                r.Badge = price + " GM";
                r.A = RowButtonData.Of(Loc.F("shop.exchange", price), _session.Wallet.Gems >= price, Loc.F("shop.need_gems", price - _session.Wallet.Gems), 1);
                Add(rows, r, () => ConfirmThenAsync(Loc.T("shop.cosmetic.confirm.title"), Loc.F("shop.cosmetic.confirm.body", name, price),
                                                    () => _cosmetics!.Buy(id), Loc.T("shop.cosmetic_bought")).Forget());
            }
        }

        /// <summary>Confirmation d'une dépense de Gemmes élevée, puis exécution (rien n'est dépensé si le joueur refuse).</summary>
        async UniTaskVoid ConfirmThenAsync(string title, string body, Func<N29.HeroesDawn.Core.Result> action, string successText)
        {
            if (_buying) return;
            _buying = true;
            try
            {
                bool ok = await _modals.ConfirmAsync(new ConfirmData(title, body, Loc.T("shop.confirm.spend"), Loc.T("ui.cancel"), Tone.Primary), Lifetime);
                if (!ok || Disposed) return;
                Report(action(), successText);
            }
            catch (OperationCanceledException) { }
            finally { _buying = false; if (!Disposed) Refresh(); }
        }

        static readonly Dictionary<string, Sprite?> BoosterArts = new Dictionary<string, Sprite?>();

        /// <summary>
        /// Paquet de booster Aube tactique recadré sur le paquet (l'image source garde des marges sombres de chaque côté).
        /// Les packs Opération et Mission reprennent le même paquet avec une teinte propre à chacun.
        /// </summary>
        static Sprite? BoosterArt(string packId)
        {
            if (BoosterArts.TryGetValue(packId, out var cached) && cached != null) return cached;
            var full = TacticalArt.Load("shop-booster");
            if (full == null) return AgentArt.Shop("booster");
            var r = full.rect;
            float margin = r.width * 0.2f;
            var crop = new Rect(r.x + margin, r.y, r.width - 2f * margin, r.height);
            float shift = packId == N29.HeroesDawn.Definitions.BoosterDef.OperationId ? 0.58f : packId == N29.HeroesDawn.Definitions.BoosterDef.MissionId ? -0.08f : 0f;
            if (shift == 0f) return BoosterArts[packId] = Sprite.Create(full.texture, crop, new Vector2(0.5f, 0.5f), full.pixelsPerUnit);
            return BoosterArts[packId] = HueShifted(full, crop, shift);
        }

        static Sprite HueShifted(Sprite source, Rect area, float shift)
        {
            var tex = source.texture;
            var rt = RenderTexture.GetTemporary(tex.width, tex.height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(tex, rt);
            var previous = RenderTexture.active;
            RenderTexture.active = rt;
            int w = Mathf.RoundToInt(area.width), h = Mathf.RoundToInt(area.height);
            var copy = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            copy.ReadPixels(new Rect(area.x, area.y, w, h), 0, 0);
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);
            var pixels = copy.GetPixels32();
            for (int i = 0; i < pixels.Length; i++)
            {
                Color.RGBToHSV(pixels[i], out float hue, out float sat, out float val);
                var c = Color.HSVToRGB(Mathf.Repeat(hue + shift, 1f), sat, val);
                pixels[i] = new Color32((byte)(c.r * 255f), (byte)(c.g * 255f), (byte)(c.b * 255f), pixels[i].a);
            }
            copy.SetPixels32(pixels);
            copy.Apply(false, false);
            return Sprite.Create(copy, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), source.pixelsPerUnit);
        }

        /// <summary>Icône d'un paquet de gemmes selon sa taille.</summary>
        static string PackArt(string packId) => packId == "gems_100" ? "gems_small" : packId == "gems_520" ? "gems_medium" : "gems_large";

        static string ExchangeArt(string id) => id == "ex_rest" ? "exchange_rest" : "exchange_credits";

        protected override void OnHeader(int index)
        {
            if (index == 0) { _nav.Open(HubPage.Pass); return; }
            _nav.Open(HubPage.Grimoire);
        }

        protected override void OnRowButton(int row, int button)
        {
            if (button == 1 && _openActions.TryGetValue(row, out var open)) { open(); return; }
            if (button == 0 && row >= 0 && row < _actions.Count) _actions[row]();
        }

        async UniTaskVoid OpenBoosterAsync(string boosterId)
        {
            if (_opening) return;
            _opening = true;
            try
            {
                await BoosterOpening.OpenAndShowAsync(_collection, _session.Content.Collection, _modals, Loc, Toasts, Lifetime, CollectionText.ReferenceStats(_session), boosterId, _audio);
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
