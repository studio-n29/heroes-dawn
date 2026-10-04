#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Application.Features.Meta
{
    public sealed class ExpeditionTemplate
    {
        public string Id { get; }
        public int Minutes { get; }
        public int Members { get; }
        public Reward Reward { get; }
        public ExpeditionTemplate(string id, int minutes, int members, Reward reward) { Id = id; Minutes = minutes; Members = members; Reward = reward; }
    }

    /// <summary>Un emplacement de la salle des opérations.</summary>
    public readonly struct ExpeditionSlotView
    {
        public readonly int Slot;
        public readonly ExpeditionTemplate? Template;
        public readonly IReadOnlyList<string> Members;
        public readonly TimeSpan Left;
        public ExpeditionSlotView(int slot, ExpeditionTemplate? template, IReadOnlyList<string> members, TimeSpan left)
        { Slot = slot; Template = template; Members = members; Left = left; }
        public bool Empty => Template == null;
        public bool Running => Template != null && Left > TimeSpan.Zero;
        public bool Ready => Template != null && Left <= TimeSpan.Zero;
    }

    /// <summary>
    /// Salle des opérations (simulée en local) : envoie des membres hors escouade en expédition de quelques minutes à quelques heures ;
    /// à leur retour, Crédits, PR et Gemmes (un peu plus si l'équipe est de haut niveau) et un peu de fatigue.
    /// </summary>
    public sealed class OperationsService
    {
        public const int Slots = 3;
        public const int FatiguePerTrip = 10;

        public static readonly IReadOnlyList<ExpeditionTemplate> Templates = new[]
        {
            new ExpeditionTemplate("patrol", 15, 1, new Reward(credits: 250, pr: 10)),
            new ExpeditionTemplate("recon", 60, 2, new Reward(credits: 700, pr: 25, gems: 2)),
            new ExpeditionTemplate("convoy", 240, 3, new Reward(credits: 2000, pr: 60, gems: 8)),
        };

        readonly MetaContext _c;
        readonly PassService _pass;

        public OperationsService(MetaContext context, PassService pass) { _c = context; _pass = pass; }

        public static ExpeditionTemplate? Find(string id)
        {
            foreach (var t in Templates) if (t.Id == id) return t;
            return null;
        }

        public bool IsBusy(MemberId id)
        {
            foreach (var e in _c.Meta.expeditions) if (e.members.Contains(id.Value)) return true;
            return false;
        }

        /// <summary>Membres libres : hors escouade et pas déjà en expédition, du plus haut niveau au plus bas.</summary>
        public IReadOnlyList<TeamMember> Available()
            => _c.Session.Roster.Where(m => !m.InSquad && !IsBusy(m.Id)).OrderByDescending(m => m.Level).ThenBy(m => m.Id.Value, StringComparer.Ordinal).ToList();

        public IReadOnlyList<ExpeditionSlotView> SlotViews()
        {
            var list = new List<ExpeditionSlotView>(Slots);
            for (int i = 0; i < Slots; i++)
            {
                ExpeditionSave? e = _c.Meta.expeditions.FirstOrDefault(x => x.slot == i);
                if (e == null) { list.Add(new ExpeditionSlotView(i, null, Array.Empty<string>(), TimeSpan.Zero)); continue; }
                var left = new DateTime(e.endUtcTicks, DateTimeKind.Utc) - _c.UtcNow;
                list.Add(new ExpeditionSlotView(i, Find(e.templateId), e.members, left));
            }
            return list;
        }

        public int ReadyCount() => SlotViews().Count(s => s.Ready);

        public Result Start(int slot, string templateId)
        {
            var t = Find(templateId);
            if (t == null || slot < 0 || slot >= Slots) return Result.Fail(MetaErrors.Unknown);
            if (_c.Meta.expeditions.Any(e => e.slot == slot)) return Result.Fail(MetaErrors.Busy);
            var free = Available();
            if (free.Count < t.Members) return Result.Fail(MetaErrors.NoMembers);
            var e = new ExpeditionSave { slot = slot, templateId = t.Id, endUtcTicks = (_c.UtcNow + TimeSpan.FromMinutes(t.Minutes)).Ticks };
            foreach (var m in free.Take(t.Members)) e.members.Add(m.Id.Value);
            _c.Meta.expeditions.Add(e);
            _c.Touch(MetaArea.Operations);
            return Result.Ok();
        }

        public Result<Reward> Claim(int slot)
        {
            var e = _c.Meta.expeditions.FirstOrDefault(x => x.slot == slot);
            if (e == null) return Result<Reward>.Fail(MetaErrors.Unknown);
            if (e.endUtcTicks > _c.UtcNow.Ticks) return Result<Reward>.Fail(MetaErrors.NotReady);
            var t = Find(e.templateId);
            _c.Meta.expeditions.Remove(e);
            if (t == null) { _c.Touch(MetaArea.Operations); return Result<Reward>.Fail(MetaErrors.Unknown); }
            int levels = 0, n = 0;
            foreach (var id in e.members)
            {
                var m = _c.Session.Find(new MemberId(id));
                if (m == null) continue;
                levels += m.Level;
                m.AddFatigue(FatiguePerTrip);
                n++;
            }
            int bonus = n == 0 ? 0 : levels / n; // +1 % par niveau moyen
            var reward = t.Reward.Scaled(100 + bonus);
            _c.Grant(reward);
            _pass.AddXp(10 + t.Minutes / 10);
            _c.Touch(MetaArea.Operations);
            return Result<Reward>.Ok(reward);
        }

        /// <summary>Coût en Gemmes pour terminer tout de suite : 1 par tranche de 5 minutes restantes (au moins 1).</summary>
        public static int SpeedUpCost(TimeSpan left) => Math.Max(1, (int)Math.Ceiling(left.TotalMinutes / 5.0));

        public Result SpeedUp(int slot)
        {
            var e = _c.Meta.expeditions.FirstOrDefault(x => x.slot == slot);
            if (e == null) return Result.Fail(MetaErrors.Unknown);
            var left = new DateTime(e.endUtcTicks, DateTimeKind.Utc) - _c.UtcNow;
            if (left <= TimeSpan.Zero) return Result.Fail(MetaErrors.AlreadyDone);
            if (!_c.Session.Wallet.TrySpendGems(SpeedUpCost(left))) return Result.Fail(MetaErrors.NotEnoughGems);
            e.endUtcTicks = _c.UtcNow.Ticks;
            _c.PublishWallet();
            _c.Touch(MetaArea.Operations);
            return Result.Ok();
        }
    }
}
