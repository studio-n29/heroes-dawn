#nullable enable
using System.Collections.Generic;
using System.Linq;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Collection;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Hub
{
    public interface IInventoryPageView : IListPageView, IFilterBarView { }

    /// <summary>
    /// Inventaire : les objets possédés (équipements trouvés en combat ou achetés) avec leurs bonus, le nombre d'exemplaires,
    /// et sur quel agent ils sont équipés. L'équipement se change depuis la fiche de l'agent.
    /// </summary>
    public sealed class InventoryPagePresenter : ListPagePresenter<IInventoryPageView>
    {
        readonly GameSession _session;
        readonly HubNavigator? _nav;

        public InventoryPagePresenter(IInventoryPageView view, GameSession session, ILocalization loc, IToastService toasts,
                                      ISubscriber<MetaChanged> meta, ISubscriber<WalletChanged> wallet, ISubscriber<RosterChanged> roster, IClock clock,
                                      HubNavigator? nav = null)
            : base(view, loc, toasts, meta, wallet, roster, clock)
        {
            _session = session; _nav = nav;
        }

        /// <summary>Filtre choisi : 0 = tous les types, sinon 1 + le type. Gardé d'un rafraîchissement à l'autre.</summary>
        int _type;

        protected override void OnStart()
        {
            base.OnStart();
            View.FilterClicked += OnFilter;
        }

        protected override void OnDispose()
        {
            View.FilterClicked -= OnFilter;
            base.OnDispose();
        }

        void OnFilter(int group, int option)
        {
            _type = option;
            Refresh();
        }

        public override string Title => Loc.T("page.inventory");

        protected override void Render()
        {
            var defs = _session.Content.Collection;
            var owned = defs.AllItems.Where(i => CollectionOps.OwnedItems(_session, i.Id) > 0)
                                  .OrderByDescending(i => i.Rarity).ThenBy(i => i.Id).ToList();
            int total = owned.Sum(i => CollectionOps.OwnedItems(_session, i.Id));
            int catalogOwned = owned.Count(i => !i.Id.Value.StartsWith(CraftedItems.IdPrefix));
            int equipped = _session.Roster.Sum(m => m.Items.Count);
            View.ShowHeader(new ListHeaderData
            {
                Title = Loc.T("inventory.header"),
                Line = Loc.F("inventory.line", total, equipped),
                Progress = defs.Items.Count == 0 ? -1f : catalogOwned / (float)defs.Items.Count,
                ProgressLabel = catalogOwned + " / " + defs.Items.Count,
                A = RowButtonData.Of(Loc.T("inventory.team"), true, "", 0),
            });

            View.ShowFilters(new[]
            {
                new FilterGroupData
                {
                    Labels = new[] { Loc.T("filters.all") }.Concat(System.Enum.GetValues(typeof(ItemType)).Cast<ItemType>().Select(t => CollectionText.ItemTypeName(Loc, t))).ToArray(),
                    Selected = _type,
                },
            });
            var rows = new List<InfoRowData>();
            foreach (var it in owned.Where(i => _type == 0 || (int)i.Type == _type - 1))
            {
                int count = CollectionOps.OwnedItems(_session, it.Id);
                var wearers = _session.Roster.Where(m => m.Items.Contains(it.Id)).Select(m => Loc.MemberName(m.Def)).ToList();
                string subtitle = CollectionText.ItemTypeChip(Loc, it.Type) + " · " + CollectionText.RarityName(Loc, it.Rarity) + " · " + CollectionText.ItemEffects(Loc, it)
                                  + (wearers.Count > 0 ? "\n" + Loc.F("inventory.worn", string.Join(", ", wearers)) : "");
                var r = InfoRowData.Basic(CollectionText.ItemName(Loc, it), subtitle);
                r.Tag = CollectionText.Tag(CollectionText.ItemName(Loc, it));
                r.TagColor = CollectionText.RarityColor(it.Rarity);
                r.Badge = CollectionText.ItemTypeChip(Loc, it.Type) + " " + Loc.F("inventory.count", count);
                r.Art = AgentArt.Item(it.Id.Value);
                rows.Add(r);
            }
            if (rows.Count == 0 && owned.Count > 0)
            {
                // Une pastille : sans elle, la grille prendrait la note pour un titre de section et la masquerait.
                var none = InfoRowData.Basic(Loc.T("inventory.none_filtered"));
                none.Tag = "—";
                none.Dim = true;
                rows.Add(none);
            }
            else if (rows.Count == 0)
            {
                var none = InfoRowData.Basic(Loc.T("inventory.empty"), Loc.T("inventory.empty_sub"));
                none.Dim = true;
                rows.Add(none);
            }
            View.ShowRows(rows.ToArray());
        }

        protected override void OnHeader(int index)
        {
            if (index == 0) _nav?.Open(N29.HeroesDawn.Presentation.Hub.HubPage.Team);
        }
    }
}
