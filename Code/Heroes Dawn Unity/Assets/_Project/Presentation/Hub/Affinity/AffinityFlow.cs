#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Combat;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>
    /// Pop-up d'affinités d'un agent : pour chaque autre agent possédé, le lien (victoires communes) et l'unique bonus que donne leur duo
    /// quand ils sont ensemble en escouade. Les agents au meilleur lien viennent d'abord.
    /// </summary>
    public sealed class AffinityFlow
    {
        readonly GameSession _session;
        readonly IModalService _modals;
        readonly ILocalization _loc;

        public AffinityFlow(GameSession session, IModalService modals, ILocalization loc)
        {
            _session = session; _modals = modals; _loc = loc;
        }

        public async UniTask OpenAsync(MemberId member, CancellationToken ct)
        {
            var view = await _modals.OpenAsync<IAffinityPopupView>(ct);
            try { await view.ShowAsync(Build(member), ct); }
            finally { _modals.Close(view); }
        }

        public AffinityPopupData Build(MemberId member)
        {
            var me = _session.Find(member);
            if (me == null) return new AffinityPopupData { Title = "", Subtitle = "", Empty = "", Close = _loc.T("ui.close"), Rows = new AffinityRowData[0] };
            var rows = _session.Roster.Where(x => x.Id != member)
                .Select(x => (Member: x, Wins: TeamLinks.Wins(_session.Meta, member.Value, x.Id.Value)))
                .OrderByDescending(t => t.Wins).ThenByDescending(t => t.Member.Level)
                .Select(t => Row(me.Def, t.Member, t.Wins))
                .ToArray();
            return new AffinityPopupData
            {
                Title = _loc.F("affinity.title", _loc.MemberName(me.Def)).ToUpperInvariant(),
                Subtitle = _loc.T("affinity.subtitle"),
                Empty = _loc.T("affinity.empty"),
                Close = _loc.T("ui.close"),
                Rows = rows,
            };
        }

        AffinityRowData Row(MemberDef me, ITeamMemberReadModel other, int wins)
        {
            int level = LinkRules.Level(wins);
            var synergy = SynergyOf(me.Family, other.Def.Family);
            return new AffinityRowData
            {
                Portrait = new PortraitData(_loc.MemberName(other.Def), other.Def.Family, art: AgentArt.For(other.Def.Id.Value)),
                Name = _loc.MemberName(other.Def),
                Link = _loc.F("affinity.link", level, wins),
                Bonus = synergy == null ? _loc.T("affinity.none") : _loc.SynergyName(synergy) + " : " + _loc.SynergyEffect(synergy, level),
                HasBonus = synergy != null,
            };
        }

        /// <summary>La synergie que forment ces deux familles (leur duo donne ce seul bonus), ou null.</summary>
        SynergyDef? SynergyOf(Family a, Family b)
        {
            foreach (var s in _session.Content.Synergies)
                if ((s.A == a && s.B == b) || (s.A == b && s.B == a)) return s;
            return null;
        }
    }
}
