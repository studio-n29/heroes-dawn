#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Collection;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Hub
{
    public interface ILoadoutPageView : IListPageView { }

    /// <summary>
    /// Équipement d'un agent : ses 2 objets, ses cartes 3 et 4 (compétences), puis la réserve d'objets et de cartes compatibles.
    /// Chaque ligne a ses boutons (Retirer, Équiper, → 3, → 4) ; les actions passent par <see cref="CollectionBook"/>.
    /// </summary>
    public sealed class LoadoutPagePresenter : ListPagePresenter<ILoadoutPageView>
    {
        readonly CollectionBook _book;
        readonly GameSession _session;
        readonly HubNavigator _nav;
        readonly MemberId _member;
        readonly List<Action?> _a = new List<Action?>();
        readonly List<Action?> _b = new List<Action?>();

        public LoadoutPagePresenter(ILoadoutPageView view, HubLocation location, CollectionBook book, GameSession session, HubNavigator nav,
                                    ILocalization loc, IToastService toasts, ISubscriber<MetaChanged> meta, ISubscriber<WalletChanged> wallet,
                                    ISubscriber<RosterChanged> roster, IClock clock)
            : base(view, loc, toasts, meta, wallet, roster, clock)
        {
            _book = book; _session = session; _nav = nav;
            _member = location.Member ?? throw new ArgumentException("Équipement sans agent", nameof(location));
        }

        public override string Title => Loc.T("page.loadout");

        CollectionDef Defs => _session.Content.Collection;

        protected override void Render()
        {
            var m = _session.Find(_member);
            if (m == null) { _nav.Back(); return; }
            string name = Loc.MemberName(m.Def);
            int cards = m.Cards.Count(c => c.HasValue);
            View.ShowHeader(new ListHeaderData
            {
                Title = Loc.F("loadout.header", name).ToUpperInvariant(),
                Line = Loc.F("loadout.line", m.Items.Count, CollectionRules.MaxItems, cards, CollectionRules.CardSlots, StatsLine(m)),
                Progress = -1f,
                A = RowButtonData.Of(Loc.T("loadout.grimoire"), true, "", 3),
            });

            _a.Clear(); _b.Clear();
            var rows = new List<InfoRowData>();
            void Section(string key)
            {
                var r = InfoRowData.Basic(Loc.T(key));
                r.Dim = true;
                rows.Add(r); _a.Add(null); _b.Add(null);
            }

            // ---- Objets équipés
            Section("loadout.items_title");
            for (int i = 0; i < CollectionRules.MaxItems; i++)
            {
                if (i < m.Items.Count)
                {
                    var id = m.Items[i];
                    var it = Defs.Item(id);
                    var r = ItemRow(it);
                    r.A = RowButtonData.Of(Loc.T("loadout.remove"), true, "", 2);
                    r.Badge = Loc.T("loadout.badge_equipped");
                    rows.Add(r); _a.Add(() => Do(_book.UnequipItem(_member, id), "loadout.removed", CollectionText.ItemName(Loc, it))); _b.Add(null);
                }
                else
                {
                    var r = InfoRowData.Basic(Loc.T("loadout.free_item"), Loc.T("loadout.free_item_sub"));
                    r.Tag = "+"; r.TagColor = Theme.Surface3; r.Dim = true;
                    rows.Add(r); _a.Add(null); _b.Add(null);
                }
            }

            // ---- Cartes équipées
            Section("loadout.cards_title");
            for (int slot = 0; slot < CollectionRules.CardSlots; slot++)
            {
                int s = slot;
                var card = m.Cards[slot];
                if (card.HasValue && Defs.HasCard(card.Value))
                {
                    var def = Defs.Card(card.Value);
                    var r = CardRow(def, Loc.F("loadout.card_slot", slot + 3, CollectionText.CardName(Loc, def)));
                    r.A = RowButtonData.Of(Loc.T("loadout.remove"), true, "", 2);
                    r.Badge = Loc.T("loadout.badge_equipped");
                    rows.Add(r); _a.Add(() => Do(_book.UnequipCard(_member, s), "loadout.removed", CollectionText.CardName(Loc, def))); _b.Add(null);
                }
                else
                {
                    bool locked = slot == 1 && !m.Cards[0].HasValue;
                    var r = InfoRowData.Basic(Loc.F("loadout.free_card", slot + 3),
                                              locked ? Loc.T("loadout.locked_card_sub") : Loc.F("loadout.free_card_sub", slot + 3, Loc.FamilyName(m.Def.Family)));
                    r.Tag = (slot + 3).ToString(); r.TagColor = Theme.Surface3; r.Dim = true;
                    rows.Add(r); _a.Add(null); _b.Add(null);
                }
            }

            // ---- Réserve d'objets
            Section("loadout.stock_items");
            bool anyItem = false;
            foreach (var it in Defs.AllItems.OrderByDescending(i => i.Rarity).ThenBy(i => i.Id))
            {
                int free = CollectionOps.AvailableItems(_session, it.Id);
                if (free <= 0) continue;
                anyItem = true;
                bool room = m.Items.Count < CollectionRules.MaxItems;
                bool typeFree = CollectionRules.CanEquip(it, m.Items.Select(Defs.Item));
                var r = ItemRow(it);
                r.Badge = free > 1 ? Loc.F("loadout.copies", free) : "";
                r.A = RowButtonData.Of(Loc.T("loadout.equip"), room && typeFree, Loc.T(room ? "equip.type_taken" : "loadout.no_slot_reason"), 0);
                var id = it.Id;
                rows.Add(r); _a.Add(() => Do(_book.EquipItem(_member, id), "loadout.equipped", CollectionText.ItemName(Loc, it))); _b.Add(null);
            }
            if (!anyItem) AddNote(rows, "loadout.none_items");

            // ---- Réserve de cartes compatibles
            Section("loadout.stock_cards");
            bool anyCard = false;
            foreach (var def in CollectionText.CardsFor(Defs, m.Def).OrderByDescending(c => c.Rarity).ThenBy(c => c.Id))
            {
                int free = CollectionOps.AvailableCards(_session, def.Id);
                if (free <= 0) continue;
                anyCard = true;
                var r = CardRow(def, CollectionText.CardName(Loc, def));
                r.Badge = free > 1 ? Loc.F("loadout.copies", free) : "";
                bool second = m.Cards[0].HasValue;
                r.A = RowButtonData.Of(Loc.F("loadout.to_slot", 3), true, "", 0);
                r.B = RowButtonData.Of(Loc.F("loadout.to_slot", 4), second, Loc.T("loadout.slot_reason"), 1);
                var id = def.Id;
                rows.Add(r);
                _a.Add(() => Do(_book.EquipCard(_member, 0, id), "loadout.equipped", CollectionText.CardName(Loc, def)));
                _b.Add(() => Do(_book.EquipCard(_member, 1, id), "loadout.equipped", CollectionText.CardName(Loc, def)));
            }
            if (!anyCard) AddNote(rows, "loadout.none_cards");
            View.ShowRows(rows.ToArray());
        }

        void AddNote(List<InfoRowData> rows, string key)
        {
            var r = InfoRowData.Basic(Loc.T(key));
            r.Dim = true;
            rows.Add(r); _a.Add(null); _b.Add(null);
        }

        InfoRowData ItemRow(ItemDef it)
        {
            var r = InfoRowData.Basic(CollectionText.ItemName(Loc, it),
                                      CollectionText.ItemTypeChip(Loc, it.Type) + " · " + CollectionText.RarityName(Loc, it.Rarity) + " · " + CollectionText.ItemEffects(Loc, it));
            r.Tag = CollectionText.Tag(CollectionText.ItemName(Loc, it));
            r.TagColor = CollectionText.RarityColor(it.Rarity);
            r.Art = AgentArt.Item(CollectionText.ArtId(it, Defs));
            return r;
        }

        InfoRowData CardRow(SkillCardDef def, string title)
        {
            var owner = _session.Find(_member);
            var r = InfoRowData.Basic(title, CollectionText.CardLine(Loc, def, owner != null ? CollectionText.StatsOf(owner.Def, owner.Level, _session.Content.Balance) : (SkillOwnerStats?)null, CollectionOps.CardLevel(_session, def.Id)));
            r.Tag = CollectionText.Tag(CollectionText.CardName(Loc, def));
            r.TagColor = CollectionText.RarityColor(def.Rarity);
            r.Art = AgentArt.Card(def.Id.Value);
            return r;
        }

        /// <summary>Bonus cumulés des objets équipés, en une ligne.</summary>
        string StatsLine(TeamMember m)
        {
            int hp = 0, atk = 0, def = 0, spd = 0;
            foreach (var id in m.Items)
            {
                if (!Defs.HasItem(id)) continue;
                var it = Defs.Item(id);
                hp += it.HpPercent; atk += it.AtkPercent; def += it.DefPercent; spd += it.SpdPercent;
            }
            var parts = new List<string>();
            if (hp != 0) parts.Add(Loc.F("item.fx.hp", hp));
            if (atk != 0) parts.Add(Loc.F("item.fx.atk", atk));
            if (def != 0) parts.Add(Loc.F("item.fx.def", def));
            if (spd != 0) parts.Add(Loc.F("item.fx.spd", spd));
            return parts.Count == 0 ? "—" : string.Join(" · ", parts);
        }

        void Do(Result r, string successKey, string what)
        {
            if (Report(r, Loc.F(successKey, what))) Refresh();
        }

        protected override void OnHeader(int index) => _nav.Open(HubPage.Grimoire);

        protected override void OnRowButton(int row, int button)
        {
            var list = button == 0 ? _a : _b;
            if (row >= 0 && row < list.Count) list[row]?.Invoke();
        }
    }
}
