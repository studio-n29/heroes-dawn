#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Domain.Combat;

namespace N29.HeroesDawn.Application.Features.Combat
{
    public sealed class BeginCombatUseCase : IBeginCombatUseCase
    {
        readonly GameSession _s;
        readonly ISeedProvider _seeds;
        readonly ActiveCombat _active;
        readonly ILogger _log;

        public BeginCombatUseCase(GameSession session, ISeedProvider seeds, ActiveCombat active, ILogger log)
        {
            _s = session; _seeds = seeds; _active = active; _log = log;
        }

        public UniTask<Result<CombatSimulation>> ExecuteAsync(CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            var run = _s.Run;
            if (run == null || run.Phase != ZoneRunPhase.InCombat || run.PendingCase?.Encounter == null)
                return UniTask.FromResult(Result<CombatSimulation>.Fail(CombatErrors.NoPendingCombat));
            var fighters = _s.Fighters;
            if (fighters.Count == 0) return UniTask.FromResult(Result<CombatSimulation>.Fail(CombatErrors.NoFighters));

            var b = _s.Content.Balance;
            var pending = run.PendingCase!;
            var encounter = pending.Encounter!;
            int avg = 0;
            foreach (var m in fighters) avg += m.Level;
            avg /= fighters.Count;
            int offset = pending.Rank == CombatRank.Easy ? b.RankEasyLevelOffset
                       : (pending.Rank == CombatRank.Elite || pending.Rank == CombatRank.Boss) ? b.RankEliteLevelOffset : 0;
            int enemyLevel = Math.Max(1, avg + offset);

            var engaged = new List<TeamMember>();
            var allies = new List<AllyEntry>();
            foreach (var m in fighters)
            {
                if (allies.Count >= MetaRules.SquadSize) break;
                allies.Add(new AllyEntry(m.Id, m.Level, m.Fatigue, run.MalusPercent(m.Id)));
                engaged.Add(m);
            }
            var enemies = new List<EnemyEntry>();
            foreach (var e in encounter.Enemies) enemies.Add(new EnemyEntry(e, enemyLevel));

            ulong seed = _seeds.NextSeed("combat:" + pending.Case.Value);
            var setup = new CombatSetup(seed, pending.Rank, allies, enemies, _s.AutoCombat);
            var sim = new CombatSimulation(_s.Content, setup);
            _active.Start(sim, setup, pending, engaged);
            _log.Info($"Combat {pending.Rank} sur {pending.Case} — ennemis niv. {enemyLevel} (graine {seed})");
            return UniTask.FromResult(Result<CombatSimulation>.Ok(sim));
        }
    }
}
