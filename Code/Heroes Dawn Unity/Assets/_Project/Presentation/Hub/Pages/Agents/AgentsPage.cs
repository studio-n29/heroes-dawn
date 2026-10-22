#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Hub
{
    public interface IAgentsPageView : IListPageView { }

    /// <summary>
    /// Registre des agents : tous les agents de la ville. Ceux de l'équipe d'abord, puis ceux à débloquer avec la condition
    /// (boutique, arc narratif, événement du week-end — mis en avant le jour même —, prouesse). Toucher un agent de l'équipe ouvre sa fiche.
    /// </summary>
    public sealed class AgentsPagePresenter : ListPagePresenter<IAgentsPageView>
    {
        readonly ISessionReadModel _session;
        readonly HubNavigator _nav;
        readonly IClock _clockSource;
        readonly List<MemberDef?> _rows = new List<MemberDef?>();

        public AgentsPagePresenter(IAgentsPageView view, ISessionReadModel session, HubNavigator nav, ILocalization loc, IToastService toasts,
                                   ISubscriber<MetaChanged> meta, ISubscriber<WalletChanged> wallet, ISubscriber<RosterChanged> roster, IClock clock)
            : base(view, loc, toasts, meta, wallet, roster, clock)
        {
            _session = session; _nav = nav; _clockSource = clock;
        }

        public override string Title => Loc.T("page.agents");

        bool Owned(MemberDef d) => _session.Roster.Any(m => m.Id == d.Id);

        /// <summary>Comment obtenir l'agent, en une phrase.</summary>
        public string How(MemberDef d)
        {
            var u = d.Unlock;
            if (u != null && u.Kind == UnlockKind.Hero) return Loc.T("agents.how.Hero");
            if (u == null || u.Kind == UnlockKind.Shop)
            {
                if (d.UnlockZone is Core.ZoneId z) return Loc.F("agents.how.Shop_locked", ZoneName(z));
                return Loc.T("agents.how.Shop");
            }
            string zone = u.Zone.HasValue ? ZoneName(u.Zone.Value) : "?";
            switch (u.Kind)
            {
                case UnlockKind.Arc: return Loc.F("agents.how.Arc", zone);
                case UnlockKind.Event:
                    return u.IsEventDay(_clockSource.LocalNow) ? Loc.F("agents.how.Event_today", zone) : Loc.F("agents.how.Event", zone);
                default: return Loc.F("agents.how.Feat." + u.Feat, zone);
            }
        }

        string ZoneName(Core.ZoneId z)
        {
            foreach (var zone in _session.Content.Zones) if (zone.Id == z) return zone.Code;
            return z.Value;
        }

        protected override void Render()
        {
            // Le personnage joueur non choisi n est pas listé : un seul des deux existe dans une partie.
            var all = _session.Content.Members.Where(d => d.Unlock?.Kind != UnlockKind.Hero || Owned(d)).ToList();
            int owned = all.Count(Owned);
            View.ShowHeader(new ListHeaderData
            {
                Title = Loc.T("agents.title"),
                Line = Loc.F("agents.line", owned, all.Count, all.Count - owned),
                Progress = all.Count == 0 ? -1f : owned / (float)all.Count,
                ProgressLabel = owned + " / " + all.Count,
            });

            _rows.Clear();
            var rows = new List<InfoRowData>();
            int Rank(MemberDef d)
            {
                if (Owned(d)) return 0;
                var u = d.Unlock;
                if (u != null && u.Kind == UnlockKind.Event && u.IsEventDay(_clockSource.LocalNow)) return 1; // événement du jour en tête des agents à débloquer
                return 2 + (u == null ? 3 : (int)u.Kind == 0 ? 3 : (int)u.Kind - 1);
            }
            foreach (var d in all.OrderBy(Rank).ThenByDescending(d => d.Rarity).ThenBy(d => d.Id))
            {
                bool own = Owned(d);
                string name = Loc.MemberName(d);
                var r = InfoRowData.Basic(name, own ? Loc.RoleName(d.Role) + " · " + Loc.FamilyName(d.Family) : How(d));
                r.Tag = name.Substring(0, 1).ToUpperInvariant();
                r.TagColor = own ? Theme.FamilyColor(d.Family) : Theme.Surface3;
                var kind = d.Unlock?.Kind ?? UnlockKind.Shop;
                r.Badge = own ? Loc.T("agents.owned") : Loc.T("agents.kind." + kind);
                r.Dim = !own;
                r.Highlight = !own && kind == UnlockKind.Event && d.Unlock!.IsEventDay(_clockSource.LocalNow);
                rows.Add(r);
                _rows.Add(d);
            }
            View.ShowRows(rows.ToArray());
        }

        protected override void OnRowSelected(int row)
        {
            if (row < 0 || row >= _rows.Count || _rows[row] == null) return;
            var d = _rows[row]!;
            if (Owned(d)) _nav.Open(HubPage.MemberSheet, d.Id);
        }
    }
}
