#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Application.Features.Collection;
using N29.HeroesDawn.Application.Features.Combat;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Domain.Combat;

namespace N29.HeroesDawn.Application.Features.Meta
{
    /// <summary>Passages entre l'équipe du joueur, sa capture de défense (envoyée au serveur) et les équipes d'un duel.</summary>
    public static class ArenaSnapshots
    {
        /// <summary>Puissance d'une équipe : 10 par niveau d'agent.</summary>
        public static int RatingOf(IEnumerable<int> levels) => levels.Sum(l => Math.Max(1, l) * 10);

        /// <summary>La capture de l'escouade du joueur (jusqu'à quatre agents, fatigue ignorée).</summary>
        public static DefenseSnapshot Capture(GameSession s, DateTime nowUtc)
        {
            var snap = new DefenseSnapshot
            {
                PlayerId = s.Account?.PlayerId ?? "",
                Name = s.Account?.DisplayName ?? "",
                UpdatedUtc = nowUtc.ToString("o", System.Globalization.CultureInfo.InvariantCulture),
            };
            var ids = new List<string>();
            foreach (var m in s.Squad)
            {
                if (snap.Team.Count >= MetaRules.SquadSize) break;
                var e = new AllyEntrySave { member = m.Id.Value, level = m.Level };
                foreach (var i in m.Items) e.items.Add(i.Value);
                foreach (var c in m.Cards)
                {
                    e.cards.Add(c.HasValue ? c.Value.Value : "");
                    e.cardLevels.Add(c.HasValue ? CollectionOps.CardLevel(s, c.Value) : 1);
                }
                snap.Team.Add(e);
                ids.Add(e.member);
            }
            foreach (var l in TeamLinks.Entries(s.Meta, ids)) snap.Links.Add(new LinkEntrySave { a = l.A.Value, b = l.B.Value, level = l.Level });
            snap.Rating = RatingOf(snap.Team.Select(t => t.level));
            return snap;
        }

        /// <summary>Les agents jouables d'une capture : un agent, un objet ou une carte inconnus du jeu (autre version) sont ignorés.</summary>
        public static IReadOnlyList<AllyEntry> ToTeam(DefenseSnapshot snap, ContentCatalog content)
        {
            var list = new List<AllyEntry>();
            foreach (var t in snap.Team)
            {
                if (list.Count >= MetaRules.SquadSize) break;
                var id = new MemberId(t.member);
                if (!content.Members.Any(m => m.Id == id)) continue;
                var items = t.items.Select(i => new ItemId(i)).Where(i => content.Collection.HasItem(i)).Take(2).ToArray();
                var cards = new List<CardId?>();
                var levels = new List<int>();
                for (int k = 0; k < t.cards.Count && k < CollectionRules.CardSlots; k++)
                {
                    bool ok = t.cards[k].Length > 0 && content.Collection.HasCard(new CardId(t.cards[k]));
                    cards.Add(ok ? new CardId(t.cards[k]) : (CardId?)null);
                    levels.Add(ok && k < t.cardLevels.Count ? Math.Max(1, Math.Min(CardLevels.Max, t.cardLevels[k])) : 1);
                }
                list.Add(new AllyEntry(id, Math.Max(1, Math.Min(100, t.level)), 0, 0, items, cards, levels));
            }
            return list;
        }

        public static IReadOnlyList<LinkEntry> ToLinks(DefenseSnapshot snap, IReadOnlyList<AllyEntry> team)
        {
            var members = new HashSet<string>(team.Select(t => t.Member.Value));
            return snap.Links.Where(l => members.Contains(l.a) && members.Contains(l.b) && l.level > 1)
                             .Select(l => new LinkEntry(new MemberId(l.a), new MemberId(l.b), Math.Min(10, l.level))).ToList();
        }

        /// <summary>Équipe de repli quand aucune capture n'est disponible : quatre agents tirés de la graine, au niveau moyen demandé.</summary>
        public static DefenseSnapshot Fallback(ContentCatalog content, int rating, ulong seed, string name = "")
        {
            var rng = new Pcg32(seed);
            var pool = content.Members.Select(m => m.Id.Value).OrderBy(x => x, StringComparer.Ordinal).ToList();
            var snap = new DefenseSnapshot { PlayerId = "fallback", Name = name, Rating = rating, UpdatedUtc = "" };
            int count = Math.Min(MetaRules.SquadSize, pool.Count);
            int level = Math.Max(1, rating / 10 / Math.Max(1, count));
            for (int i = 0; i < count; i++)
            {
                int pick = rng.Range(0, pool.Count);
                snap.Team.Add(new AllyEntrySave { member = pool[pick], level = level });
                pool.RemoveAt(pick);
            }
            return snap;
        }
    }
}
