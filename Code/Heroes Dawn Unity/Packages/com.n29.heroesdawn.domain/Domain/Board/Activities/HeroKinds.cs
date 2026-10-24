#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Activities
{
    /// <summary>« Rempart » : mini-jeu 2D d'interception (voir <see cref="Games.DeflectGame"/>). Pas de niveau 3D : la partie se joue dans une fenêtre.</summary>
    public sealed class DeflectKind : IActivityKind
    {
        public string Id => ActivityKinds.Deflect;

        public IReadOnlyList<ParameterDef> Schema { get; } = new[]
        {
            new ParameterDef("time_limit_s", 40, 15, 120),
            new ParameterDef("target", 14, 3, 60),
            new ParameterDef("lives", 3, 1, 9),
            new ParameterDef("speed_percent", 100, 40, 250),
        };

        public ActivityLevel Generate(ActivityContext c)
            => new ActivityLevel(c.Seed, c.Environment, 0, 0, new LevelPrimitive[0], new LevelMarker[0]);
    }

    /// <summary>« Course des toits » : mini-jeu 2D de couloirs (voir <see cref="Games.RooftopRunGame"/>).</summary>
    public sealed class RooftopRunKind : IActivityKind
    {
        public string Id => ActivityKinds.RooftopRun;

        public IReadOnlyList<ParameterDef> Schema { get; } = new[]
        {
            new ParameterDef("time_limit_s", 40, 15, 120),
            new ParameterDef("target", 10, 3, 40),
            new ParameterDef("lives", 3, 1, 9),
            new ParameterDef("speed_percent", 100, 40, 250),
        };

        public ActivityLevel Generate(ActivityContext c)
            => new ActivityLevel(c.Seed, c.Environment, 0, 0, new LevelPrimitive[0], new LevelMarker[0]);
    }
}
