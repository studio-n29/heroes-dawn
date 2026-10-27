#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Activities;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Activities
{
    /// <summary>Table des pilotes de mini-jeux 2D par type d'activité. Un type absent de la table garde la fenêtre d'activité générique.</summary>
    public static class MiniGameDrivers
    {
        static readonly Dictionary<string, Func<ActivityPlan, ILocalization, IMiniGameDriver>> Map =
            new Dictionary<string, Func<ActivityPlan, ILocalization, IMiniGameDriver>>();

        static MiniGameDrivers()
        {
            Map[ActivityKinds.Rhythm] = (plan, loc) => new RhythmDriver(plan, loc);
            Map[ActivityKinds.FreeThrow] = (plan, loc) => new FreeThrowDriver(plan, loc);
            Map[ActivityKinds.CratePick] = (plan, loc) => new CratePickDriver(plan, loc);
            Map[ActivityKinds.DnaAnalysis] = (plan, loc) => new DnaDriver(plan, loc);
            Map[ActivityKinds.Gears] = (plan, loc) => new GearsDriver(plan, loc);
            Map[ActivityKinds.RescueEngineering] = (plan, loc) => new RescueDriver(plan, loc);
            Map[ActivityKinds.Infiltration] = (plan, loc) => new InfiltrationDriver(plan, loc);
        }

        public static void Register(string kind, Func<ActivityPlan, ILocalization, IMiniGameDriver> factory) => Map[kind] = factory;

        public static void Unregister(string kind) => Map.Remove(kind);

        /// <summary>Le pilote du type de l'activité planifiée, ou null si ce type n'a pas de mini-jeu 2D.</summary>
        public static IMiniGameDriver? Create(ActivityPlan plan, ILocalization loc)
            => Map.TryGetValue(plan.Def.Kind, out var f) ? f(plan, loc) : null;
    }
}
