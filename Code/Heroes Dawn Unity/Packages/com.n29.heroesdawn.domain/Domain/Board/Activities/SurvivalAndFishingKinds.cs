#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Activities
{
    /// <summary>
    /// Mini-jeu historique (barre de réflexe) : pas de niveau 3D ; les paramètres de la barre restent dans l'équilibrage du prototype.
    /// Utilisé par défaut sur les cases Mini-jeu sans activité assignée.
    /// </summary>
    public sealed class ReflexKind : IActivityKind
    {
        public string Id => ActivityKinds.Reflex;
        public IReadOnlyList<ParameterDef> Schema { get; } = new[] { new ParameterDef("extra_time_percent", 0, 0, 100) };

        public ActivityLevel Generate(ActivityContext c)
            => new ActivityLevel(c.Seed, c.Environment, 0, 0, new LevelPrimitive[0], new LevelMarker[0]);
    }

    /// <summary>
    /// Survie en neige (3D) : se déplacer dans la neige, l'herbe haute ou le sable avec une jauge de survie qui descend et se recharge près d'un totem,
    /// pour trouver un objet défini. Le biome change la répartition des terrains, la météo les couleurs.
    /// </summary>
    public sealed class SnowSurvivalKind : IActivityKind
    {
        const int Field = 6000;

        public string Id => ActivityKinds.SnowSurvival;

        public IReadOnlyList<ParameterDef> Schema { get; } = new[]
        {
            new ParameterDef("time_limit_s", 90, 20, 300),
            // jauge sur 1000 : perte par seconde en terrain normal
            new ParameterDef("drain_permille_per_s", 25, 1, 200),
            new ParameterDef("refill_permille_per_s", 120, 10, 500),
            new ParameterDef("totem_count", 3, 1, 6),
            new ParameterDef("totem_radius_m", 4, 1, 12),
            new ParameterDef("target_distance_m", 30, 10, 80),
            new ParameterDef("obstacle_count", 12, 0, 40),
            // terrain lent (herbe haute, sable profond) : ralentissement en %
            new ParameterDef("terrain_slow_percent", 35, 0, 80),
        };

        public ActivityLevel Generate(ActivityContext c)
        {
            var rng = new Pcg32(c.Seed).Fork(11);
            var pal = EnvironmentPalette.For(c.Environment);
            var prims = new List<LevelPrimitive>();
            var markers = new List<LevelMarker> { new LevelMarker("spawn", 0, 0, 0) };

            prims.Add(new LevelPrimitive(PrimitiveShape.Plane, 0, 0, 0, Field, 1, Field, "ground", pal.Ground));

            // Terrains : la répartition dépend du biome (neige, herbe haute, sable).
            var (snow, grass, sand) = Weights(c.Environment.Biome);
            for (int i = 0; i < 16; i++)
            {
                int roll = rng.Range(0, 100);
                var kind = roll < snow ? TerrainKind.Snow : roll < snow + grass ? TerrainKind.TallGrass : TerrainKind.Sand;
                var (x, z) = Gen.Around(rng, 400, 2600);
                int size = rng.Range(600, 1401);
                prims.Add(new LevelPrimitive(PrimitiveShape.Cube, x, 3, z, size, 6, size, "terrain_" + kind.ToString().ToLowerInvariant(), pal.Terrain(kind)));
            }

            // Totems : lieux de recharge de la jauge (cylindre + flamme).
            int totems = c.Param("totem_count");
            for (int i = 0; i < totems; i++)
            {
                var (x, z) = Gen.Around(rng, 800, 2200);
                prims.Add(new LevelPrimitive(PrimitiveShape.Cylinder, x, 80, z, 40, 160, 40, "totem", pal.Totem));
                prims.Add(new LevelPrimitive(PrimitiveShape.Sphere, x, 190, z, 60, 60, 60, "fire", pal.Fire));
                markers.Add(new LevelMarker("totem_" + i, x, 0, z));
            }

            // Obstacles : blocs et arbres stylisés.
            for (int i = 0; i < c.Param("obstacle_count"); i++)
            {
                var (x, z) = Gen.Around(rng, 300, 2800);
                bool tree = rng.Chance(50);
                if (tree) prims.Add(new LevelPrimitive(PrimitiveShape.Cylinder, x, 120, z, 60, 240, 60, "tree", pal.Foliage));
                else prims.Add(new LevelPrimitive(PrimitiveShape.Cube, x, 60, z, rng.Range(100, 301), 120, rng.Range(100, 301), "obstacle", pal.Obstacle));
            }

            // Objet à trouver : loin du départ, sur un cercle de rayon choisi.
            int dist = c.Param("target_distance_m") * 100;
            var d = Gen.Dir16[rng.Range(0, 16)];
            int tx = d.X * dist / 1000, tz = d.Z * dist / 1000;
            prims.Add(new LevelPrimitive(PrimitiveShape.Sphere, tx, 50, tz, 80, 80, 80, "target", pal.Target));
            markers.Add(new LevelMarker("target", tx, 0, tz));

            var facts = new Dictionary<string, int> { ["totems"] = totems, ["target_distance_cm"] = dist };
            return new ActivityLevel(c.Seed, c.Environment, Field, Field, prims, markers, facts);
        }

        /// <summary>Répartition (neige, herbe haute, sable) en % selon le biome.</summary>
        static (int Snow, int Grass, int Sand) Weights(Biome b)
        {
            switch (b)
            {
                case Biome.Snow: return (70, 25, 5);
                case Biome.Desert: return (5, 15, 80);
                case Biome.Forest: return (10, 80, 10);
                case Biome.Harbor: return (10, 25, 65);
                case Biome.Industrial: return (15, 35, 50);
                default: return (20, 50, 30);
            }
        }
    }

    /// <summary>
    /// Pêche (3D, façon Animal Crossing NH) : un minuteur, pêcher le plus de poissons possible ; la quantité donne des pièces au prorata.
    /// </summary>
    public sealed class FishingKind : IActivityKind
    {
        const int Field = 4000;

        public string Id => ActivityKinds.Fishing;

        public IReadOnlyList<ParameterDef> Schema { get; } = new[]
        {
            new ParameterDef("time_limit_s", 60, 15, 180),
            new ParameterDef("fish_count", 10, 3, 30),
            new ParameterDef("bite_window_ms", 900, 300, 3000),
            new ParameterDef("bite_wait_min_ms", 800, 200, 5000),
            new ParameterDef("bite_wait_max_ms", 3000, 500, 10000),
            // nombre de poissons à pêcher pour réussir (en dessous : échec, mais les pièces restent au prorata)
            new ParameterDef("success_fish", 4, 1, 20),
            new ParameterDef("pond_radius_m", 14, 6, 30),
        };

        public ActivityLevel Generate(ActivityContext c)
        {
            var rng = new Pcg32(c.Seed).Fork(12);
            var pal = EnvironmentPalette.For(c.Environment);
            int pond = c.Param("pond_radius_m") * 100;
            var prims = new List<LevelPrimitive>
            {
                new LevelPrimitive(PrimitiveShape.Plane, 0, 0, 0, Field, 1, Field, "ground", pal.Ground),
                new LevelPrimitive(PrimitiveShape.Cylinder, 0, -5, 0, pond * 2, 10, pond * 2, "water", pal.Water),
                new LevelPrimitive(PrimitiveShape.Cube, 0, 10, -pond - 100, 200, 20, 600, "dock", pal.Obstacle),
            };
            var markers = new List<LevelMarker> { new LevelMarker("spawn", 0, 0, -pond - 300) };

            // Roseaux et rochers sur la berge.
            for (int i = 0; i < 10; i++)
            {
                var d = Gen.Dir16[rng.Range(0, 16)];
                int r = pond + rng.Range(80, 400);
                prims.Add(new LevelPrimitive(PrimitiveShape.Cylinder, d.X * r / 1000, 60, d.Z * r / 1000, 30, 120, 30, "reed", pal.Foliage));
            }

            int fish = c.Param("fish_count");
            for (int i = 0; i < fish; i++)
            {
                var (x, z) = Gen.Around(rng, 100, pond - 150);
                int size = rng.Range(30, 71);
                prims.Add(new LevelPrimitive(PrimitiveShape.Capsule, x, -20, z, size, size / 2, size / 2, "fish", pal.Accent));
                markers.Add(new LevelMarker("fish_" + i, x, -20, z));
            }

            var facts = new Dictionary<string, int> { ["fish"] = fish, ["pond_radius_cm"] = pond };
            return new ActivityLevel(c.Seed, c.Environment, Field, Field, prims, markers, facts);
        }
    }

    /// <summary>
    /// Arène de combat (3D hack'n'slash, caméra top-down) : un agent choisi, deux compétences, déplacement à 360°,
    /// des vagues d'ennemis aux comportements simples (0 = poursuite, 1 = charge, 2 = tir).
    /// </summary>
    public sealed class CombatArenaKind : IActivityKind
    {
        public string Id => ActivityKinds.CombatArena;

        public IReadOnlyList<ParameterDef> Schema { get; } = new[]
        {
            new ParameterDef("waves", 3, 1, 8),
            new ParameterDef("enemies_per_wave", 4, 1, 12),
            new ParameterDef("arena_radius_m", 12, 6, 30),
            new ParameterDef("obstacle_count", 6, 0, 20),
            new ParameterDef("enemy_hp_percent", 100, 25, 300),
            new ParameterDef("enemy_damage_percent", 100, 25, 300),
        };

        public ActivityLevel Generate(ActivityContext c)
        {
            var rng = new Pcg32(c.Seed).Fork(13);
            var pal = EnvironmentPalette.For(c.Environment);
            int radius = c.Param("arena_radius_m") * 100;
            int waves = c.Param("waves"), perWave = c.Param("enemies_per_wave");
            var prims = new List<LevelPrimitive>
            {
                new LevelPrimitive(PrimitiveShape.Cylinder, 0, -10, 0, radius * 2, 20, radius * 2, "arena", pal.Ground),
            };
            var markers = new List<LevelMarker> { new LevelMarker("player", 0, 0, 0) };

            // Murs : un bloc par direction.
            foreach (var d in Gen.Dir16)
                prims.Add(new LevelPrimitive(PrimitiveShape.Cube, d.X * radius / 1000, 100, d.Z * radius / 1000, 260, 200, 260, "wall", pal.Obstacle));

            for (int i = 0; i < c.Param("obstacle_count"); i++)
            {
                var (x, z) = Gen.Around(rng, 300, radius - 400);
                prims.Add(new LevelPrimitive(PrimitiveShape.Cylinder, x, 100, z, 120, 200, 120, "pillar", pal.GroundAlt));
            }

            var facts = new Dictionary<string, int> { ["waves"] = waves, ["enemies_per_wave"] = perWave, ["enemies_total"] = waves * perWave, ["arena_radius_cm"] = radius };
            for (int w = 0; w < waves; w++)
            {
                facts["wave" + w + "_pattern"] = (w + rng.Range(0, 3)) % 3;
                for (int i = 0; i < perWave; i++)
                {
                    var d = Gen.Dir16[(i * 16 / perWave + w) % 16];
                    markers.Add(new LevelMarker("spawn_w" + w + "_" + i, d.X * (radius - 200) / 1000, 0, d.Z * (radius - 200) / 1000));
                }
            }
            return new ActivityLevel(c.Seed, c.Environment, radius * 2, radius * 2, prims, markers, facts);
        }
    }
}
