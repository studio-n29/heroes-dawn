#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Domain.Board;

namespace N29.HeroesDawn.Application.Features.Zone
{
    /// <summary>
    /// Relaie une action au domaine du plateau et applique ses conséquences méta :
    /// refus « non autorisé », ressources ramassées, validation de zone (récompenses + <see cref="ZoneValidated"/>).
    /// </summary>
    public static class ZoneActions
    {
        public static Result<IReadOnlyList<BoardEvent>> Apply(ZoneEffects fx, Func<ZoneRun, IReadOnlyList<BoardEvent>> action)
        {
            var run = fx.Session.Run;
            if (run == null) return Result<IReadOnlyList<BoardEvent>>.Fail(ZoneErrors.NoRun);
            IReadOnlyList<BoardEvent> events;
            try
            {
                events = action(run);
            }
            catch (InvalidOperationException e)
            {
                fx.Log.Warn("Action de plateau refusée : " + e.Message);
                return Result<IReadOnlyList<BoardEvent>>.Fail(ZoneErrors.NotAllowed);
            }
            ApplyEvents(fx, events);
            return Result<IReadOnlyList<BoardEvent>>.Ok(events);
        }

        static void ApplyEvents(ZoneEffects fx, IReadOnlyList<BoardEvent> events)
        {
            var s = fx.Session;
            var b = s.Content.Balance;
            bool wallet = false;
            bool collection = false;
            ZoneRewards? validated = null;
            foreach (var e in events)
            {
                switch (e)
                {
                    case ResourceCollectedEvent rc:
                        s.Wallet.Add(rc.Credits, rc.Pr);
                        wallet = true;
                        break;
                    case ItemFoundEvent found:
                        Collection.CollectionOps.AddItem(s, found.Item);
                        collection = true;
                        break;
                    case BoosterEarnedEvent earned:
                        Collection.CollectionOps.AddBooster(s, earned.Booster);
                        collection = true;
                        break;
                    case ZoneValidatedEvent _:
                        var zoneId = s.RunZone != null ? s.RunZone.Id.Value : "";
                        bool first = s.CompletedZones.Add(zoneId);
                        validated = first
                            ? new ZoneRewards(b.ZoneValidationCredits, b.ZoneValidationPr, true)
                            : new ZoneRewards(MetaRules.ZoneReplayCredits, 0, false);
                        int pct = Meta.LabBonuses.ZoneRewardPercent(s.Meta);
                        validated = new ZoneRewards(validated.Credits * pct / 100, validated.Pr * pct / 100, validated.FirstClear, first ? 1 : 0);
                        if (first) { Collection.CollectionOps.AddBooster(s, Definitions.BoosterDef.StandardId); collection = true; }
                        s.Wallet.Add(validated.Credits, validated.Pr);
                        wallet = true;
                        fx.Log.Info($"Zone {zoneId} validée (1re fois : {first})");
                        break;
                }
            }
            if (wallet) fx.Wallet.PublishWallet(s);
            if (validated != null) fx.Validated.Publish(new ZoneValidated(validated));
            if (wallet || collection) fx.Save.RequestSave();
        }
    }
}
