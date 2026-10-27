#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Activities
{
    /// <summary>Les types d'activité des mini-jeux 2D du lot H : la partie vit dans une classe de <c>Games</c>, le niveau généré est vide (comme Rempart).</summary>
    static class MiniGameLevel
    {
        public static ActivityLevel Empty(ActivityContext c)
            => new ActivityLevel(c.Seed, c.Environment, 0, 0, new LevelPrimitive[0], new LevelMarker[0]);
    }

    /// <summary>« Le rythme » (voir <see cref="Games.RhythmGame"/>).</summary>
    public sealed class RhythmKind : IActivityKind
    {
        public string Id => ActivityKinds.Rhythm;

        public IReadOnlyList<ParameterDef> Schema { get; } = new[]
        {
            new ParameterDef("notes", 24, 8, 60),
            new ParameterDef("lanes", 3, 3, 4),
            new ParameterDef("lives", 5, 1, 9),
            new ParameterDef("speed_percent", 100, 50, 220),
        };

        public ActivityLevel Generate(ActivityContext c) => MiniGameLevel.Empty(c);
    }

    /// <summary>« Tir franc » (voir <see cref="Games.FreeThrowGame"/>).</summary>
    public sealed class FreeThrowKind : IActivityKind
    {
        public string Id => ActivityKinds.FreeThrow;

        public IReadOnlyList<ParameterDef> Schema { get; } = new[]
        {
            new ParameterDef("throws", 5, 2, 12),
            new ParameterDef("hits", 3, 1, 8),
            new ParameterDef("speed_percent", 100, 50, 220),
        };

        public ActivityLevel Generate(ActivityContext c) => MiniGameLevel.Empty(c);
    }

    /// <summary>« Caisse au choix » (voir <see cref="Games.CratePickGame"/>).</summary>
    public sealed class CratePickKind : IActivityKind
    {
        public string Id => ActivityKinds.CratePick;

        public IReadOnlyList<ParameterDef> Schema { get; } = new[]
        {
            new ParameterDef("rounds", 3, 1, 8),
            new ParameterDef("lives", 3, 1, 9),
            new ParameterDef("speed_percent", 100, 50, 220),
        };

        public ActivityLevel Generate(ActivityContext c) => MiniGameLevel.Empty(c);
    }

    /// <summary>« Analyse ADN » (voir <see cref="Games.DnaAnalysisGame"/>).</summary>
    public sealed class DnaAnalysisKind : IActivityKind
    {
        public string Id => ActivityKinds.DnaAnalysis;

        public IReadOnlyList<ParameterDef> Schema { get; } = new[]
        {
            new ParameterDef("time_limit_s", 45, 15, 120),
            new ParameterDef("parasites", 6, 3, 16),
            new ParameterDef("lives", 3, 1, 9),
        };

        public ActivityLevel Generate(ActivityContext c) => MiniGameLevel.Empty(c);
    }

    /// <summary>« Engrenage » (voir <see cref="Games.GearsGame"/>).</summary>
    public sealed class GearsKind : IActivityKind
    {
        public string Id => ActivityKinds.Gears;

        public IReadOnlyList<ParameterDef> Schema { get; } = new[]
        {
            new ParameterDef("gears", 6, 3, 12),
            new ParameterDef("moves", 12, 4, 40),
        };

        public ActivityLevel Generate(ActivityContext c) => MiniGameLevel.Empty(c);
    }
}
