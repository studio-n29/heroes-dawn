#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Combat;

namespace N29.HeroesDawn.EditorTools.Balance
{
    public sealed class SimulationRequest
    {
        public IReadOnlyList<MemberId> Team { get; set; } = Array.Empty<MemberId>();
        public int TeamLevel { get; set; } = 1;
        public int FatiguePercent { get; set; }
        public int RunsPerEncounter { get; set; } = 1000;
        public ulong BaseSeed { get; set; } = 1;
        public int MaxTicks { get; set; } = 20000;
    }

    public sealed class EncounterStats
    {
        public int EncounterIndex { get; set; }
        public CombatRank Rank { get; set; }
        public int Runs { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Timeouts { get; set; }
        public int AvgTicksOnWin { get; set; }
    }

    /// <summary>Rejoue en mode auto les rencontres d'une zone avec le domaine de combat (remplace la console BalanceSimulator).</summary>
    public static class BalanceSimulation
    {
        public static IReadOnlyList<EncounterStats> Run(ContentCatalog content, ZoneId zone, SimulationRequest req,
                                                        Action<int, int>? onEncounter = null)
        {
            var b = content.Balance;
            var encounters = content.Zone(zone).Encounters;
            var allies = new List<AllyEntry>();
            foreach (var m in req.Team) allies.Add(new AllyEntry(m, req.TeamLevel, req.FatiguePercent));

            var result = new List<EncounterStats>();
            for (int e = 0; e < encounters.Count; e++)
            {
                onEncounter?.Invoke(e, encounters.Count);
                var enc = encounters[e];
                // Même règle que CombatService : niveau ennemi = niveau de l'équipe + écart du rang.
                int offset = enc.Rank == CombatRank.Easy ? b.RankEasyLevelOffset
                           : enc.Rank == CombatRank.Elite || enc.Rank == CombatRank.Boss ? b.RankEliteLevelOffset : 0;
                int enemyLevel = Math.Max(1, req.TeamLevel + offset);
                var waves = new List<IReadOnlyList<EnemyEntry>>();
                foreach (var wave in enc.Waves)
                {
                    var enemies = new List<EnemyEntry>();
                    foreach (var id in wave) enemies.Add(new EnemyEntry(id, enemyLevel));
                    waves.Add(enemies);
                }

                var stats = new EncounterStats { EncounterIndex = e, Rank = enc.Rank, Runs = req.RunsPerEncounter };
                long winTicks = 0;
                for (int i = 0; i < req.RunsPerEncounter; i++)
                {
                    var seed = req.BaseSeed + (ulong)(e * 100000 + i);
                    var sim = new CombatSimulation(content, new CombatSetup(seed, enc.Rank, allies, waves, true));
                    while (!sim.IsOver && sim.Tick < req.MaxTicks) sim.Step(1);
                    if (!sim.IsOver) stats.Timeouts++;
                    else if (sim.Victory == true) { stats.Wins++; winTicks += sim.Tick; }
                    else stats.Losses++;
                }
                stats.AvgTicksOnWin = stats.Wins == 0 ? 0 : (int)(winTicks / stats.Wins);
                result.Add(stats);
            }
            return result;
        }

        public static string ToCsv(IReadOnlyList<EncounterStats> stats, int tickMs = 50)
        {
            var inv = CultureInfo.InvariantCulture;
            var sb = new StringBuilder("encounter;rank;runs;wins;losses;timeouts;win_rate_pct;avg_seconds_on_win\n");
            foreach (var s in stats)
            {
                int winRate = s.Runs == 0 ? 0 : s.Wins * 100 / s.Runs;
                double seconds = Math.Round(s.AvgTicksOnWin * tickMs / 1000.0, 1);
                sb.Append(s.EncounterIndex.ToString(inv)).Append(';').Append(s.Rank).Append(';')
                  .Append(s.Runs.ToString(inv)).Append(';').Append(s.Wins.ToString(inv)).Append(';')
                  .Append(s.Losses.ToString(inv)).Append(';').Append(s.Timeouts.ToString(inv)).Append(';')
                  .Append(winRate.ToString(inv)).Append(';').Append(seconds.ToString("0.0", inv)).Append('\n');
            }
            return sb.ToString();
        }
    }
}
