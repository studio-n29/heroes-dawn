#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Activities
{
    public enum PrimitiveShape { Cube, Sphere, Cylinder, Capsule, Plane }

    /// <summary>
    /// Un élément de décor : une primitive aplat (V1 : aucun asset, que des formes et des couleurs plates qui changent avec l'environnement).
    /// Unités : centimètres ; couleur 0xRRGGBB ; <see cref="Role"/> dit à quoi sert l'élément (sol, totem, poisson…).
    /// </summary>
    public readonly struct LevelPrimitive
    {
        public PrimitiveShape Shape { get; }
        public int X { get; }
        public int Y { get; }
        public int Z { get; }
        public int SizeX { get; }
        public int SizeY { get; }
        public int SizeZ { get; }
        public string Role { get; }
        public int Color { get; }

        public LevelPrimitive(PrimitiveShape shape, int x, int y, int z, int sizeX, int sizeY, int sizeZ, string role, int color)
        {
            Shape = shape; X = x; Y = y; Z = z; SizeX = sizeX; SizeY = sizeY; SizeZ = sizeZ; Role = role; Color = color;
        }
    }

    /// <summary>Point nommé du niveau (départ, arrivée, totem, apparition d'ennemi…), en centimètres.</summary>
    public readonly struct LevelMarker
    {
        public string Name { get; }
        public int X { get; }
        public int Y { get; }
        public int Z { get; }
        public LevelMarker(string name, int x, int y, int z) { Name = name; X = x; Y = y; Z = z; }
    }

    /// <summary>
    /// Niveau d'activité généré de façon procédurale (même graine + même environnement = même niveau).
    /// Les activités 2D / textuelles (potion, dialogue) portent leurs données dans <see cref="Payload"/>.
    /// </summary>
    public sealed class ActivityLevel
    {
        public ulong Seed { get; }
        public ZoneEnvironment Environment { get; }
        public EnvironmentPalette Palette { get; }
        /// <summary>Largeur et profondeur du terrain, en centimètres.</summary>
        public int Width { get; }
        public int Depth { get; }
        public IReadOnlyList<LevelPrimitive> Primitives { get; }
        public IReadOnlyList<LevelMarker> Markers { get; }
        /// <summary>Faits chiffrés propres au type (nombre de poissons, de vagues…).</summary>
        public IReadOnlyDictionary<string, int> Facts { get; }
        public object? Payload { get; }

        public ActivityLevel(ulong seed, ZoneEnvironment environment, int width, int depth, IReadOnlyList<LevelPrimitive> primitives,
                             IReadOnlyList<LevelMarker> markers, IReadOnlyDictionary<string, int>? facts = null, object? payload = null)
        {
            Seed = seed; Environment = environment; Palette = EnvironmentPalette.For(environment);
            Width = width; Depth = depth; Primitives = primitives; Markers = markers;
            Facts = facts ?? new Dictionary<string, int>();
            Payload = payload;
        }

        public T? PayloadAs<T>() where T : class => Payload as T;

        public int Fact(string key, int fallback = 0) => Facts.TryGetValue(key, out var v) ? v : fallback;

        public bool TryMarker(string name, out LevelMarker marker)
        {
            foreach (var m in Markers) if (m.Name == name) { marker = m; return true; }
            marker = default;
            return false;
        }
    }
}
