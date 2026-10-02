#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Application.Events;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;

namespace N29.HeroesDawn.Application.Features
{
    public static class ZoneErrors
    {
        public static readonly DomainError NoRun = new DomainError("zone.no_run", "errors.no_run");
        public static readonly DomainError NotAllowed = new DomainError("zone.not_allowed", "errors.board.not_allowed");
        public static readonly DomainError SquadExhausted = new DomainError("zone.exhausted", "errors.squad_exhausted");
        public static readonly DomainError UnknownZone = new DomainError("zone.unknown", "errors.unknown_zone");
    }

    public enum NarrativeChoice { Credits, Research, Morale }

    /// <summary>Récompenses de validation de zone (GDD §5 : 1 500 Crédits + 40 PR la première fois).</summary>
    public sealed class ZoneRewards
    {
        public int Credits { get; }
        public int Pr { get; }
        public bool FirstClear { get; }
        public ZoneRewards(int credits, int pr, bool firstClear) { Credits = credits; Pr = pr; FirstClear = firstClear; }
    }

    /// <summary>Pilote une partie de zone : relaie les actions au domaine et applique leurs conséquences méta.</summary>
    public sealed class ZoneService
    {
        public const int NarrativeCredits = 250;
        public const int NarrativePr = 10;
        public const int NarrativeMorale = 15;

        readonly GameSession _s;
        readonly ISeedProvider _seeds;
        readonly SaveService _save;
        readonly GameEvents _events;
        readonly ILogger _log;

        public ZoneRewards? LastRewards { get; private set; }

        public ZoneService(GameSession session, ISeedProvider seeds, SaveService save, GameEvents events, ILogger log)
        {
            _s = session; _seeds = seeds; _save = save; _events = events; _log = log;
        }

        public ZoneRun? Run => _s.Run;
        public BalanceConfig Balance => _s.Content.Balance;

        public Result StartZone(ZoneId zoneId)
        {
            ZoneDef? zone = null;
            foreach (var z in _s.Content.Zones) if (z.Id == zoneId) zone = z;
            if (zone == null) return Result.Fail(ZoneErrors.UnknownZone);
            if (_s.SquadExhausted) return Result.Fail(ZoneErrors.SquadExhausted);
            ulong seed = _seeds.NextSeed("zone:" + zoneId.Value);
            _s.Run = new ZoneRun(_s.Content, zone, seed);
            _s.RunZone = zone;
            LastRewards = null;
            _log.Info($"Zone {zoneId} démarrée (graine {seed})");
            return Result.Ok();
        }

        public Result<IReadOnlyList<BoardEvent>> Roll() => Do(r => r.Roll());
        public Result<IReadOnlyList<BoardEvent>> UseBonusDie() => Do(r => r.UseBonusDie());
        public Result<IReadOnlyList<BoardEvent>> MoveTo(CaseId target) => Do(r => r.MoveTo(target));
        public Result<IReadOnlyList<BoardEvent>> EndMove() => Do(r => r.EndMove());

        /// <summary>Mini-jeu terminé : récompense en cas de réussite ; la case reste jouable en cas d'échec.</summary>
        public Result<IReadOnlyList<BoardEvent>> ResolveMiniGame(bool success)
        {
            var res = Do(r => r.ResolveCase(success));
            if (res.IsSuccess && success)
            {
                _s.Wallet.Add(Balance.MiniGameCredits, Balance.MiniGamePr);
                _events.RaiseWallet();
                _save.RequestSave();
            }
            return res;
        }

        public Result<IReadOnlyList<BoardEvent>> ResolveNarrative(NarrativeChoice choice)
        {
            var res = Do(r => r.ResolveCase(true));
            if (res.IsFailure) return res;
            switch (choice)
            {
                case NarrativeChoice.Credits: _s.Wallet.Add(NarrativeCredits, 0); break;
                case NarrativeChoice.Research: _s.Wallet.Add(0, NarrativePr); break;
                case NarrativeChoice.Morale: foreach (var m in _s.Squad) m.AddFatigue(-NarrativeMorale); break;
            }
            _events.RaiseWallet();
            _events.RaiseRoster();
            _save.RequestSave();
            return res;
        }

        /// <summary>Case Repos : se reposer (−30 % de fatigue pour l'escouade) ou passer (la case reste disponible).</summary>
        public Result<IReadOnlyList<BoardEvent>> ResolveRest(bool take)
        {
            var res = Do(r => r.ResolveCase(take));
            if (res.IsSuccess && take)
            {
                foreach (var m in _s.Squad) m.AddFatigue(-Balance.RestFatigueReduction);
                _events.RaiseRoster();
                _save.RequestSave();
            }
            return res;
        }

        /// <summary>Retour au QG : la partie est abandonnée (le brouillard se referme). Les récompenses déjà gagnées sont conservées.</summary>
        public void Abandon()
        {
            if (_s.Run == null) return;
            _log.Info("Zone quittée");
            _s.Run = null;
            _s.RunZone = null;
            _save.RequestSave();
        }

        /// <summary>Appelé par CombatService une fois le combat terminé.</summary>
        internal Result<IReadOnlyList<BoardEvent>> ResolveCombat(bool victory) => Do(r => r.ResolveCombat(victory));

        Result<IReadOnlyList<BoardEvent>> Do(Func<ZoneRun, IReadOnlyList<BoardEvent>> action)
        {
            var run = _s.Run;
            if (run == null) return Result<IReadOnlyList<BoardEvent>>.Fail(ZoneErrors.NoRun);
            IReadOnlyList<BoardEvent> events;
            try
            {
                events = action(run);
            }
            catch (InvalidOperationException e)
            {
                _log.Warn("Action de plateau refusée : " + e.Message);
                return Result<IReadOnlyList<BoardEvent>>.Fail(ZoneErrors.NotAllowed);
            }
            Apply(events);
            return Result<IReadOnlyList<BoardEvent>>.Ok(events);
        }

        void Apply(IReadOnlyList<BoardEvent> events)
        {
            bool wallet = false;
            foreach (var e in events)
            {
                switch (e)
                {
                    case ResourceCollectedEvent rc:
                        _s.Wallet.Add(rc.Credits, rc.Pr);
                        wallet = true;
                        break;
                    case ZoneValidatedEvent _:
                        var zoneId = _s.RunZone != null ? _s.RunZone.Id.Value : "";
                        bool first = _s.CompletedZones.Add(zoneId);
                        LastRewards = first
                            ? new ZoneRewards(Balance.ZoneValidationCredits, Balance.ZoneValidationPr, true)
                            : new ZoneRewards(MetaRules.ZoneReplayCredits, 0, false);
                        _s.Wallet.Add(LastRewards.Credits, LastRewards.Pr);
                        wallet = true;
                        _log.Info($"Zone {zoneId} validée (1re fois : {first})");
                        break;
                }
            }
            if (wallet) _events.RaiseWallet();
            if (wallet) _save.RequestSave();
        }

        /// <summary>Fin de partie après validation : retour au QG.</summary>
        public void CloseCompletedRun()
        {
            if (_s.Run != null && _s.Run.Phase == ZoneRunPhase.Completed)
            {
                _s.Run = null;
                _s.RunZone = null;
                _save.RequestSave();
            }
        }
    }
}
