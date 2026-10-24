#nullable enable
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Collection;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Hub
{
    public interface IGrimoirePageView : IListPageView { }

    /// <summary>
    /// Grimoire : toutes les cartes du jeu (possédées ou pas), les boosters en réserve (ouvrir) et l'achat de boosters en PR.
    /// L'ouverture d'un booster révèle les cartes tirées dans une fenêtre.
    /// </summary>
    public sealed class GrimoirePagePresenter : ListPagePresenter<IGrimoirePageView>
    {
        readonly CollectionBook _book;
        readonly GameSession _session;
        readonly IModalService _modals;
        readonly List<SkillCardDef> _rows = new List<SkillCardDef>();
        string? _focus;

        public GrimoirePagePresenter(IGrimoirePageView view, CollectionBook book, GameSession session, IModalService modals, ILocalization loc,
                                     IToastService toasts, ISubscriber<MetaChanged> meta, ISubscriber<WalletChanged> wallet,
                                     ISubscriber<RosterChanged> roster, IClock clock, HubNavigator? nav = null)
            : base(view, loc, toasts, meta, wallet, roster, clock)
        {
            _book = book; _session = session; _modals = modals;
            _focus = nav?.FocusCard;
            if (nav != null) nav.FocusCard = null;
        }

        public override string Title => Loc.T("page.grimoire");

        CollectionDef Defs => _session.Content.Collection;

        BoosterDef Booster => Defs.Booster(BoosterDef.StandardId);

        protected override void Render()
        {
            int owned = Defs.Cards.Count(c => CollectionOps.OwnedCards(_session, c.Id) > 0);
            int boosters = CollectionOps.Boosters(_session, BoosterDef.StandardId);
            int missing = Booster.PricePr - _session.Wallet.Pr;
            View.ShowHeader(new ListHeaderData
            {
                Title = Loc.T("grimoire.header"),
                Line = Loc.F("grimoire.line", owned, Defs.Cards.Count, boosters, _session.Meta.collection.pity, Booster.PityEvery),
                Progress = Defs.Cards.Count == 0 ? -1f : owned / (float)Defs.Cards.Count,
                ProgressLabel = owned + " / " + Defs.Cards.Count,
                A = RowButtonData.Of(boosters > 0 ? Loc.T("grimoire.open") : Loc.T("grimoire.open_none"), boosters > 0, "", 0),
                B = RowButtonData.Of(Loc.F("grimoire.buy", Booster.PricePr), missing <= 0, Loc.F("grimoire.buy_reason", missing), 1),
            });

            _rows.Clear();
            var rows = new List<InfoRowData>();
            foreach (var card in Defs.Cards.OrderByDescending(c => CollectionOps.OwnedCards(_session, c.Id) > 0).ThenByDescending(c => c.Rarity).ThenBy(c => c.Family).ThenBy(c => c.Id))
            {
                int count = CollectionOps.OwnedCards(_session, card.Id);
                int level = CollectionOps.CardLevel(_session, card.Id);
                var r = InfoRowData.Basic(CollectionText.CardName(Loc, card), CollectionText.CardLine(Loc, card, CollectionText.ReferenceStats(_session), level));
                r.Tag = CollectionText.Tag(CollectionText.CardName(Loc, card));
                r.TagColor = count > 0 ? CollectionText.RarityColor(card.Rarity) : Theme.Surface3;
                r.Badge = count > 0 ? Loc.F("grimoire.owned_level", count, level, CardLevels.Max) : Loc.T("grimoire.missing");
                if (count > 0) r.A = UpgradeButton(card, level, count);
                r.Dim = count == 0;
                r.Art = AgentArt.Card(card.Id.Value);
                r.Highlight = _focus != null && card.Id.Value == _focus;
                rows.Add(r);
                _rows.Add(card);
            }
            View.ShowRows(rows.ToArray());
            _focus = null;
        }

        /// <summary>Bouton « Améliorer » de la fiche : coût en doublons et en Crédits, avec la raison quand il est grisé.</summary>
        RowButtonData UpgradeButton(SkillCardDef card, int level, int count)
        {
            if (level >= CardLevels.Max) return RowButtonData.Of(Loc.T("grimoire.max_level"), false, Loc.T("errors.collection_max_level"), 3);
            int copies = CardLevels.CopiesToUpgrade(level), credits = CardLevels.CreditsToUpgrade(level);
            int keep = System.Math.Max(1, CollectionOps.EquippedCards(_session, card.Id));
            string reason = count - copies < keep ? Loc.F("grimoire.upgrade_copies", copies)
                          : _session.Wallet.Credits < credits ? Loc.F("grimoire.upgrade_credits", credits - _session.Wallet.Credits) : "";
            return RowButtonData.Of(Loc.F("grimoire.upgrade", copies, credits), reason.Length == 0, reason, 1);
        }

        protected override void OnRowButton(int row, int button)
        {
            if (row < 0 || row >= _rows.Count || button != 0) return;
            if (Report(_book.UpgradeCard(_rows[row].Id), Loc.T("grimoire.upgraded"))) Refresh();
        }

        protected override void OnHeader(int index)
        {
            if (index == 0) OpenAsync().Forget();
            else if (Report(_book.BuyBooster(BoosterDef.StandardId), Loc.T("grimoire.bought"))) Refresh();
        }

        async UniTaskVoid OpenAsync()
        {
            try { await BoosterOpening.OpenAndShowAsync(_book, Defs, _modals, Loc, Toasts, Lifetime, CollectionText.ReferenceStats(_session)); }
            catch (System.OperationCanceledException) { }
            if (!Disposed) Refresh();
        }
    }
}
