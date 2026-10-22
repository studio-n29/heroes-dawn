#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Zone;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Domain.Combat;

namespace N29.HeroesDawn.Application.Features.Combat
{
    public sealed class FinishCombatUseCase : IFinishCombatUseCase
    {
        readonly ActiveCombat _active;
        readonly ZoneEffects _fx;
        readonly IPublisher<RosterChanged> _roster;
        readonly IPublisher<CombatFinished> _finished;
        readonly ILeaveZoneUseCase _leave;

        public FinishCombatUseCase(ActiveCombat active, ZoneEffects fx, IPublisher<RosterChanged> roster,
                                   IPublisher<CombatFinished> finished, ILeaveZoneUseCase leave)
        {
            _active = active; _fx = fx; _roster = roster; _finished = finished; _leave = leave;
        }

        public async UniTask<Result<CombatReport>> ExecuteAsync(CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            var sim = _active.Simulation;
            var pending = _active.Case;
            if (sim == null || pending == null || !sim.IsOver) return Result<CombatReport>.Fail(CombatErrors.NoPendingCombat);
            var s = _fx.Session;
            var b = s.Content.Balance;
            bool victory = sim.Victory == true;

            bool boss = false;
            foreach (var u in sim.Units) if (u.Side == Side.Enemy && u.IsBoss) boss = true;
            int credits = 0;
            if (victory)
            {
                credits = boss ? MetaRules.BossCredits
                        : pending.Rank == CombatRank.Easy ? b.CreditsEasy
                        : pending.Rank == CombatRank.Normal ? b.CreditsNormal : b.CreditsElite;
                credits = credits * Meta.LabBonuses.CombatCreditsPercent(s.Meta) / 100;
                s.Wallet.Add(credits, 0);
            }

            var changes = new List<FatigueChange>();
            int fatigue = b.FatiguePerWin + (victory ? 0 : b.FatiguePerLossExtra);
            foreach (var m in _active.FighterMembers)
            {
                s.RunEngaged.Add(m.Id.Value);
                int before = m.Fatigue;
                m.AddFatigue(fatigue);
                changes.Add(new FatigueChange(m.Id, before, m.Fatigue));
            }

            if (victory) TeamLinks.RecordVictory(s.Meta, _active.FighterMembers.Select(m => m.Id.Value).ToList());

            var res = ZoneActions.Apply(_fx, r => r.ResolveCombat(victory));
            IReadOnlyList<BoardEvent> boardEvents = res.IsSuccess ? res.Value : Array.Empty<BoardEvent>();

            bool toHq = false;
            if (!victory && s.SquadExhausted)
            {
                await _leave.ExecuteAsync(ct);
                toHq = true;
            }

            var report = new CombatReport(victory, credits, pending.Rank, boss, changes, boardEvents, toHq);
            _fx.Log.Info($"Combat terminé : {(victory ? "victoire" : "défaite")} en {sim.Tick} ticks, +{credits} Crédits");
            _fx.Wallet.PublishWallet(s);
            _roster.PublishRoster();
            _fx.Save.RequestSave();
            _finished.Publish(new CombatFinished(report));
            return Result<CombatReport>.Ok(report);
        }
    }
}
