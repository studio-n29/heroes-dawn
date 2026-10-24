using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Combat
{
    public sealed class AllyEntry
    {
        public MemberId Member { get; }
        public int Level { get; }
        public int FatiguePercent { get; }
        /// <summary>Bonus / malus de statistiques en % (ex. -15 après une potion ratée) ; 0 = aucun.</summary>
        public int StatPercent { get; }
        /// <summary>Objets equipes (au plus 2).</summary>
        public IReadOnlyList<ItemId> Items { get; }
        /// <summary>Cartes equipees : emplacement 3 puis 4 (une entree vide = pas de carte).</summary>
        public IReadOnlyList<CardId?> Cards { get; }
        /// <summary>Niveau de chaque carte équipée (même ordre que <see cref="Cards"/>) ; absent : niveau 1.</summary>
        public IReadOnlyList<int> CardLevels { get; }
        public AllyEntry(MemberId member, int level, int fatiguePercent, int statPercent = 0, IReadOnlyList<ItemId>? items = null, IReadOnlyList<CardId?>? cards = null, IReadOnlyList<int>? cardLevels = null)
        {
            Member = member; Level = level; FatiguePercent = fatiguePercent; StatPercent = statPercent;
            Items = items ?? System.Array.Empty<ItemId>();
            Cards = cards ?? System.Array.Empty<CardId?>();
            CardLevels = cardLevels ?? System.Array.Empty<int>();
        }
    }

    public sealed class EnemyEntry
    {
        public EnemyId Enemy { get; }
        public int Level { get; }
        public EnemyEntry(EnemyId enemy, int level) { Enemy = enemy; Level = level; }
    }

    public sealed class CombatSetup
    {
        public ulong Seed { get; }
        public CombatRank Rank { get; }
        public IReadOnlyList<AllyEntry> Allies { get; }
        /// <summary>Ennemis de la première vague.</summary>
        public IReadOnlyList<EnemyEntry> Enemies { get; }
        /// <summary>Vagues successives d'ennemis (au moins une) ; la vague 1 est <see cref="Enemies"/>.</summary>
        public IReadOnlyList<IReadOnlyList<EnemyEntry>> Waves { get; }
        public bool Auto { get; }
        /// <summary>Niveaux de lien des paires d agents de l escouade (une paire absente : niveau 1).</summary>
        public IReadOnlyList<LinkEntry> Links { get; private set; } = System.Array.Empty<LinkEntry>();

        public CombatSetup WithLinks(IReadOnlyList<LinkEntry> links) { Links = links; return this; }

        public int LinkLevel(MemberId a, MemberId b)
        {
            foreach (var l in Links) if (l.Is(a, b)) return l.Level;
            return 1;
        }

        public CombatSetup(ulong seed, CombatRank rank, IReadOnlyList<AllyEntry> allies, IReadOnlyList<EnemyEntry> enemies, bool auto)
            : this(seed, rank, allies, new[] { enemies }, auto) { }

        public CombatSetup(ulong seed, CombatRank rank, IReadOnlyList<AllyEntry> allies, IReadOnlyList<IReadOnlyList<EnemyEntry>> waves, bool auto)
        {
            Seed = seed; Rank = rank; Allies = allies; Waves = waves; Enemies = waves[0]; Auto = auto;
        }
    }
}
