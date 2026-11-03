#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Domain.Combat;

namespace N29.HeroesDawn.Application.Features.Meta
{
    /// <summary>
    /// Compteurs de statistiques du profil dans <see cref="MetaSave.stats"/> : combats gagnés et perdus, Crédits gagnés au total,
    /// cases traversées, ennemis battus, et l'usage des agents et des cartes de compétence en combat (base des favoris).
    /// </summary>
    public static class Stats
    {
        public const string CombatsWon = "combatsWon", CombatsLost = "combatsLost", CreditsEarned = "creditsEarned";
        public const string Tiles = "tiles", Enemies = "enemies";
        /// <summary>Préfixes des compteurs d'usage : « use:&lt;agent&gt; » et « skill:&lt;compétence&gt; ».</summary>
        public const string AgentUsePrefix = "use:", SkillUsePrefix = "skill:";

        /// <summary>Nombre favoris affichés pour les agents et pour les cartes.</summary>
        public const int FavoriteCount = 3;

        public static int Get(MetaSave m, string id) => m.GetCounter(m.stats, id);

        public static void Add(MetaSave m, string id, int n) => m.SetCounter(m.stats, id, m.GetCounter(m.stats, id) + n);

        /// <summary>Ajoute les cases traversées par les déplacements du plateau.</summary>
        public static void RecordMoves(MetaSave m, IReadOnlyList<BoardEvent> events)
        {
            foreach (var e in events)
                if (e is PawnMovedEvent moved && moved.Path.Count > 0) Add(m, Tiles, moved.Path.Count);
        }

        /// <summary>Ajoute, depuis le journal d'un combat, les ennemis battus et l'usage de chaque agent et compétence.</summary>
        public static void RecordCombat(MetaSave m, CombatSimulation sim)
        {
            foreach (var e in sim.Log)
            {
                if (e is UnitDefeatedEvent defeated && sim.Unit(defeated.Unit).Side == Side.Enemy) Add(m, Enemies, 1);
                else if (e is SkillUsedEvent used && sim.Unit(used.Actor).Side == Side.Ally)
                {
                    Add(m, AgentUsePrefix + sim.Unit(used.Actor).DefId, 1);
                    Add(m, SkillUsePrefix + used.Skill.Value, 1);
                }
            }
        }

        /// <summary>Les <paramref name="count"/> éléments les plus utilisés d'un préfixe (identifiants sans préfixe), du plus au moins utilisé.</summary>
        public static IReadOnlyList<string> Favorites(MetaSave m, string prefix, int count)
        {
            return m.stats
                .Where(c => c.id.StartsWith(prefix, StringComparison.Ordinal) && c.value > 0)
                .OrderByDescending(c => c.value).ThenBy(c => c.id, StringComparer.Ordinal)
                .Take(count)
                .Select(c => c.id.Substring(prefix.Length))
                .ToList();
        }
    }
}
