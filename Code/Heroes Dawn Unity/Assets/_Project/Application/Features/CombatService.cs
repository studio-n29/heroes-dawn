#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Domain.Combat;

namespace N29.HeroesDawn.Application.Features
{
    public static class CombatErrors
    {
        public static readonly DomainError NoPendingCombat = new DomainError("combat.none", "errors.no_combat");
        public static readonly DomainError NoFighters = new DomainError("combat.no_fighters", "errors.squad_exhausted");
    }

    public readonly struct FatigueChange
    {
        public readonly MemberId Member;
        public readonly int Before;
        public readonly int After;
        public FatigueChange(MemberId member, int before, int after) { Member = member; Before = before; After = after; }
    }

    public sealed class CombatReport
    {
        public bool Victory { get; }
        public int Credits { get; }
        public CombatRank Rank { get; }
        public bool Boss { get; }
        public IReadOnlyList<FatigueChange> Fatigue { get; }
        public IReadOnlyList<BoardEvent> BoardEvents { get; }
        public bool ReturnedToHq { get; }

        public CombatReport(bool victory, int credits, CombatRank rank, bool boss, IReadOnlyList<FatigueChange> fatigue,
                            IReadOnlyList<BoardEvent> boardEvents, bool returnedToHq)
        {
            Victory = victory; Credits = credits; Rank = rank; Boss = boss; Fatigue = fatigue; BoardEvents = boardEvents; ReturnedToHq = returnedToHq;
        }
    }

    /// <summary>
    /// Prépare un combat à partir de la case en attente, puis applique son issue
    /// (Crédits, fatigue, recul sur le plateau, retour au QG si l'escouade est épuisée).
    /// </summary>
    public sealed class CombatService
    {
        readonly GameSession _s;
        readonly ISeedProvider _seeds;
        readonly ZoneService _zones;
        readonly SaveScheduler _save;
        readonly IPublisher<WalletChanged> _wallet;
        readonly IPublisher<RosterChanged> _roster;
        readonly ICombatVerifier _verifier;
        readonly ILogger _log;
        readonly List<TeamMember> _fighters = new List<TeamMember>();

        public CombatSimulation? Current { get; private set; }
        public CombatSetup? CurrentSetup { get; private set; }
        public CaseOutcome? CurrentCase { get; private set; }
        /// <summary>Membres engagés, dans l'ordre des unités alliées (Id 0..n-1).</summary>
        public IReadOnlyList<TeamMember> Fighters => _fighters;
        public bool DebugUsed { get; private set; }

        public CombatService(GameSession session, ISeedProvider seeds, ZoneService zones, SaveScheduler save, IPublisher<WalletChanged> wallet, IPublisher<RosterChanged> roster,
                             ICombatVerifier verifier, ILogger log)
        {
            _s = session; _seeds = seeds; _zones = zones; _save = save; _wallet = wallet; _roster = roster; _verifier = verifier; _log = log;
        }

        public Result<CombatSimulation> Begin()
        {
            var run = _s.Run;
            if (run == null || run.Phase != ZoneRunPhase.InCombat || run.PendingCase?.Encounter == null)
                return Result<CombatSimulation>.Fail(CombatErrors.NoPendingCombat);
            var fighters = _s.Fighters;
            if (fighters.Count == 0) return Result<CombatSimulation>.Fail(CombatErrors.NoFighters);

            var b = _s.Content.Balance;
            var pending = run.PendingCase!;
            var encounter = pending.Encounter!;
            int avg = 0;
            foreach (var m in fighters) avg += m.Level;
            avg /= fighters.Count;
            int offset = pending.Rank == CombatRank.Easy ? b.RankEasyLevelOffset
                       : (pending.Rank == CombatRank.Elite || pending.Rank == CombatRank.Boss) ? b.RankEliteLevelOffset : 0;
            int enemyLevel = Math.Max(1, avg + offset);

            _fighters.Clear();
            var allies = new List<AllyEntry>();
            foreach (var m in fighters)
            {
                if (allies.Count >= MetaRules.SquadSize) break;
                allies.Add(new AllyEntry(m.Id, m.Level, m.Fatigue));
                _fighters.Add(m);
            }
            var enemies = new List<EnemyEntry>();
            foreach (var e in encounter.Enemies) enemies.Add(new EnemyEntry(e, enemyLevel));

            ulong seed = _seeds.NextSeed("combat:" + pending.Case.Value);
            CurrentSetup = new CombatSetup(seed, pending.Rank, allies, enemies, _s.AutoCombat);
            CurrentCase = pending;
            Current = new CombatSimulation(_s.Content, CurrentSetup);
            DebugUsed = false;
            _log.Info($"Combat {pending.Rank} sur {pending.Case} — ennemis niv. {enemyLevel} (graine {seed})");
            return Result<CombatSimulation>.Ok(Current);
        }

        public void SetAuto(bool on)
        {
            _s.AutoCombat = on;
            Current?.Submit(CombatCommand.SetAuto(on));
            _save.RequestSave();
        }

        public void ForceWinForDebug()
        {
            if (Current == null || Current.IsOver) return;
            DebugUsed = true;
            Current.ForceEnemiesDefeatedForDebug();
        }

        /// <summary>Applique l'issue du combat terminé. À appeler une seule fois, quand la simulation est finie.</summary>
        public Result<CombatReport> Finish()
        {
            var sim = Current;
            var pending = CurrentCase;
            if (sim == null || pending == null || !sim.IsOver) return Result<CombatReport>.Fail(CombatErrors.NoPendingCombat);
            var b = _s.Content.Balance;
            bool victory = sim.Victory == true;

            bool boss = false;
            foreach (var u in sim.Units) if (u.Side == Side.Enemy && u.IsBoss) boss = true;
            int credits = 0;
            if (victory)
            {
                credits = boss ? MetaRules.BossCredits
                        : pending.Rank == CombatRank.Easy ? b.CreditsEasy
                        : pending.Rank == CombatRank.Normal ? b.CreditsNormal : b.CreditsElite;
                _s.Wallet.Add(credits, 0);
            }

            var changes = new List<FatigueChange>();
            int fatigue = b.FatiguePerWin + (victory ? 0 : b.FatiguePerLossExtra);
            foreach (var m in _fighters)
            {
                int before = m.Fatigue;
                m.AddFatigue(fatigue);
                changes.Add(new FatigueChange(m.Id, before, m.Fatigue));
            }

            var res = _zones.ResolveCombat(victory);
            IReadOnlyList<BoardEvent> boardEvents = res.IsSuccess ? res.Value : Array.Empty<BoardEvent>();

            bool toHq = false;
            if (!victory && _s.SquadExhausted)
            {
                _zones.Abandon();
                toHq = true;
            }

            var report = new CombatReport(victory, credits, pending.Rank, boss, changes, boardEvents, toHq);
            _log.Info($"Combat terminé : {(victory ? "victoire" : "défaite")} en {sim.Tick} ticks, +{credits} Crédits");
            _wallet.PublishWallet(_s);
            _roster.PublishRoster();
            _save.RequestSave();
            return Result<CombatReport>.Ok(report);
        }

        /// <summary>« Vérification serveur » simulée : rejoue le combat depuis la graine et le journal d'entrées.</summary>
        public async Task<bool?> VerifyLastAsync(CancellationToken ct)
        {
            var sim = Current;
            var setup = CurrentSetup;
            if (sim == null || setup == null || DebugUsed) return null;
            try
            {
                bool ok = await _verifier.VerifyAsync(setup, sim.InputLog, sim.StateHash(), ct);
                if (!ok) _log.Warn("Rejeu du combat divergent : le serveur aurait refusé ce résultat.");
                return ok;
            }
            catch (OperationCanceledException) { return null; }
            catch (Exception e)
            {
                _log.Error("Vérification du combat : " + e.Message);
                return null;
            }
        }

        public void Clear()
        {
            Current = null;
            CurrentSetup = null;
            CurrentCase = null;
            _fighters.Clear();
        }
    }
}
