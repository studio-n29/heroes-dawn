#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Application.Features.Meta
{
    public sealed class LabTech
    {
        public string Id { get; }
        public int MaxLevel { get; }
        public LabTech(string id, int maxLevel) { Id = id; MaxLevel = maxLevel; }
        public int CostFor(int level) => 500 * (level + 1);
        public int MinutesFor(int level) => 10 * (level + 1);
    }

    public readonly struct LabTechView
    {
        public readonly LabTech Tech;
        public readonly int Level;
        public readonly bool Active;
        public readonly TimeSpan Left;
        public LabTechView(LabTech tech, int level, bool active, TimeSpan left) { Tech = tech; Level = level; Active = active; Left = left; }
        public bool Maxed => Level >= Tech.MaxLevel;
    }

    /// <summary>Effets cumulés des recherches du labo.</summary>
    public static class LabBonuses
    {
        /// <summary>Réduction du prix d'entraînement : 4 % par niveau.</summary>
        public static int TrainCostPercent(MetaSave m) => 100 - 4 * Level(m, ResearchLab.Training);

        /// <summary>Bonus de Crédits de combat : 5 % par niveau.</summary>
        public static int CombatCreditsPercent(MetaSave m) => 100 + 5 * Level(m, ResearchLab.Loot);

        /// <summary>Points de fatigue récupérés par minute hors mission : 1 + 1 par niveau.</summary>
        public static int RecoveryPerMinute(MetaSave m) => MetaRulesRecovery + Level(m, ResearchLab.Recovery);

        /// <summary>Bonus de récompense de validation de zone : 5 % par niveau.</summary>
        public static int ZoneRewardPercent(MetaSave m) => 100 + 5 * Level(m, ResearchLab.Logistics);

        const int MetaRulesRecovery = 1;

        public static int Level(MetaSave m, string tech) => m.GetCounter(m.labLevels, tech);
    }

    /// <summary>Labo de recherche (simulé en local) : une recherche à la fois, payée en Crédits, qui dure quelques minutes.</summary>
    public sealed class ResearchLab
    {
        public const string Training = "training", Loot = "loot", Recovery = "recovery", Logistics = "logistics";

        public static readonly IReadOnlyList<LabTech> Techs = new[]
        {
            new LabTech(Training, 5), new LabTech(Loot, 5), new LabTech(Recovery, 5), new LabTech(Logistics, 5),
        };

        readonly MetaContext _c;

        public ResearchLab(MetaContext context) { _c = context; }

        public static LabTech? Find(string id)
        {
            foreach (var t in Techs) if (t.Id == id) return t;
            return null;
        }

        public bool Researching => !string.IsNullOrEmpty(_c.Meta.labActive);

        /// <summary>Termine la recherche arrivée à échéance ; renvoie sa technologie (pour le toast) ou null.</summary>
        public LabTech? Refresh()
        {
            var m = _c.Meta;
            if (!Researching || m.labEndUtcTicks > _c.UtcNow.Ticks) return null;
            var tech = Find(m.labActive);
            m.labActive = "";
            m.labEndUtcTicks = 0;
            if (tech != null) m.SetCounter(m.labLevels, tech.Id, Math.Min(tech.MaxLevel, LabBonuses.Level(m, tech.Id) + 1));
            _c.Touch(MetaArea.Lab);
            return tech;
        }

        public IReadOnlyList<LabTechView> Views()
        {
            var list = new List<LabTechView>();
            var m = _c.Meta;
            foreach (var t in Techs)
            {
                bool active = m.labActive == t.Id;
                var left = active ? new DateTime(m.labEndUtcTicks, DateTimeKind.Utc) - _c.UtcNow : TimeSpan.Zero;
                list.Add(new LabTechView(t, LabBonuses.Level(m, t.Id), active, left));
            }
            return list;
        }

        public bool IsReady => Researching && _c.Meta.labEndUtcTicks <= _c.UtcNow.Ticks;

        public Result Start(string techId)
        {
            Refresh();
            var t = Find(techId);
            if (t == null) return Result.Fail(MetaErrors.Unknown);
            if (Researching) return Result.Fail(MetaErrors.Busy);
            int level = LabBonuses.Level(_c.Meta, t.Id);
            if (level >= t.MaxLevel) return Result.Fail(MetaErrors.MaxLevel);
            if (!_c.Session.Wallet.TrySpend(t.CostFor(level), 0)) return Result.Fail(MetaErrors.NotEnoughCredits);
            _c.Meta.labActive = t.Id;
            _c.Meta.labEndUtcTicks = (_c.UtcNow + TimeSpan.FromMinutes(t.MinutesFor(level))).Ticks;
            _c.PublishWallet();
            _c.Touch(MetaArea.Lab);
            return Result.Ok();
        }

        public static int SpeedUpCost(TimeSpan left) => Math.Max(1, (int)Math.Ceiling(left.TotalMinutes / 4.0));

        public Result SpeedUp()
        {
            if (!Researching) return Result.Fail(MetaErrors.Unknown);
            var left = new DateTime(_c.Meta.labEndUtcTicks, DateTimeKind.Utc) - _c.UtcNow;
            if (left <= TimeSpan.Zero) return Result.Fail(MetaErrors.AlreadyDone);
            if (!_c.Session.Wallet.TrySpendGems(SpeedUpCost(left))) return Result.Fail(MetaErrors.NotEnoughGems);
            _c.Meta.labEndUtcTicks = _c.UtcNow.Ticks;
            _c.PublishWallet();
            Refresh();
            return Result.Ok();
        }
    }
}
