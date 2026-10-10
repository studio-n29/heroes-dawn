#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Maps
{
    public enum MapShapeKind { Rect, Circle, Line }

    /// <summary>
    /// Une forme du fond de carte (pixels, origine en bas à gauche). Rect : coins (X0,Y0)-(X1,Y1) ; Circle : centre (X0,Y0), rayon Size ;
    /// Line : (X0,Y0)-(X1,Y1), épaisseur Size. Couleur 0xAARRGGBB (alpha 255 = opaque).
    /// </summary>
    public readonly struct MapShape
    {
        public MapShapeKind Kind { get; }
        public int X0 { get; }
        public int Y0 { get; }
        public int X1 { get; }
        public int Y1 { get; }
        public int Size { get; }
        public int Color { get; }

        MapShape(MapShapeKind kind, int x0, int y0, int x1, int y1, int size, int color)
        {
            Kind = kind; X0 = x0; Y0 = y0; X1 = x1; Y1 = y1; Size = size; Color = color;
        }

        public static MapShape Rect(int x0, int y0, int x1, int y1, int color)
            => new MapShape(MapShapeKind.Rect, System.Math.Min(x0, x1), System.Math.Min(y0, y1), System.Math.Max(x0, x1), System.Math.Max(y0, y1), 0, color);
        public static MapShape Circle(int x, int y, int radius, int color) => new MapShape(MapShapeKind.Circle, x, y, 0, 0, radius, color);
        public static MapShape Line(int x0, int y0, int x1, int y1, int thickness, int color) => new MapShape(MapShapeKind.Line, x0, y0, x1, y1, thickness, color);

        public MapShape WithColor(int color) => new MapShape(Kind, X0, Y0, X1, Y1, Size, color);
    }

    /// <summary>Zone peinte par le level designer sur la carte : un décor différent du fond (forêt dans une ville, lac…). Pixels.</summary>
    public readonly struct MapRegion
    {
        public Biome Biome { get; }
        public int X { get; }
        public int Y { get; }
        public int Radius { get; }
        public MapRegion(Biome biome, int x, int y, int radius) { Biome = biome; X = x; Y = y; Radius = radius; }
    }

    /// <summary>Fond de carte vectoriel généré : formes à dessiner dans l'ordre (le premier est le sol).</summary>
    public sealed class MapBackground
    {
        public int Width { get; }
        public int Height { get; }
        public IReadOnlyList<MapShape> Shapes { get; }
        public Biome Biome { get; }
        public Weather Weather { get; }

        public MapBackground(int width, int height, IReadOnlyList<MapShape> shapes, Biome biome, Weather weather)
        {
            Width = width; Height = height; Shapes = shapes; Biome = biome; Weather = weather;
        }
    }
}
