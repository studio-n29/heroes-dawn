#nullable enable
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Application.Features.World
{
    /// <summary>Un boss d'un repaire et s'il est déjà tombé.</summary>
    public readonly struct LairBossView
    {
        public readonly LairBossDef Boss;
        public readonly bool Defeated;
        public LairBossView(LairBossDef boss, bool defeated) { Boss = boss; Defeated = defeated; }
    }

    /// <summary>
    /// Progression des repaires, gardée dans le profil : boss vaincus (retenus d'une partie à l'autre) et nombre de repaires terminés.
    /// Les enregistrements sont statiques sur <see cref="MetaSave"/> : ils sont appelés depuis la fin de combat et la validation de zone.
    /// </summary>
    public sealed class LairBook
    {
        readonly MetaContext _c;

        public LairBook(MetaContext context) { _c = context; }

        static string Key(string lairId, string caseId) => lairId + ":" + caseId;
        static string ClearsKey(string lairId) => "lair_clears:" + lairId;

        public bool Unlocked(LairDef lair) => _c.Session.CompletedZones.Contains(lair.RequiresZone);

        public IReadOnlyList<LairBossView> Bosses(LairDef lair)
            => lair.Bosses.Select(b => new LairBossView(b, _c.Meta.lairBosses.Contains(Key(lair.Id, b.Case)))).ToList();

        public int DefeatedCount(LairDef lair) => Bosses(lair).Count(b => b.Defeated);

        public int Clears(LairDef lair) => _c.Meta.GetCounter(_c.Meta.stats, ClearsKey(lair.Id));

        /// <summary>Un combat de la zone vient d'être gagné : si c'est un boss de repaire, il est retenu.</summary>
        public static void RecordBossDefeat(MetaSave meta, string zoneId, string caseId)
        {
            var lair = Lairs.OfZone(zoneId);
            if (lair == null || lair.Bosses.All(b => b.Case != caseId)) return;
            string key = Key(lair.Id, caseId);
            if (!meta.lairBosses.Contains(key)) meta.lairBosses.Add(key);
        }

        /// <summary>La zone d'un repaire vient d'être validée : un repaire terminé de plus.</summary>
        public static void RecordClear(MetaSave meta, string zoneId)
        {
            var lair = Lairs.OfZone(zoneId);
            if (lair == null) return;
            meta.SetCounter(meta.stats, ClearsKey(lair.Id), meta.GetCounter(meta.stats, ClearsKey(lair.Id)) + 1);
        }
    }
}
