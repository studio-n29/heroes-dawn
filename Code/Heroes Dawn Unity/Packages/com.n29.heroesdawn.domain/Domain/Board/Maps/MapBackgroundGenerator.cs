#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Maps
{
    /// <summary>
    /// Fond de carte 2D d'un plateau : plan stylisé simple façon carte en ligne (« par défaut » : sol clair, routes blanches à contour,
    /// bâtiments gris, parcs verts, eau bleue) décliné par biome (ville, port, industrie, forêt, neige, désert), avec des zones peintes
    /// (région) d'un autre biome, puis teinté par la météo (jour, nuit avec fenêtres éclairées, neige, pluie, brouillard).
    /// Calcul entier et graine : le même plateau donne toujours le même fond.
    /// </summary>
    public static class MapBackgroundGenerator
    {
        const int Opaque = unchecked((int)0xFF000000);

        static int C(int rgb) => Opaque | rgb;

        // Palette « plan par défaut ».
        const int Casing = 0xC9C6BE, RoadFill = 0xFFFFFF, MajorFill = 0xF9D98A, MajorCasing = 0xE2B24F;

        public static MapBackground Generate(int width, int height, ulong seed, Biome biome, Weather weather, IReadOnlyList<MapRegion>? regions = null)
        {
            var shapes = new List<MapShape>();
            var buildings = new List<MapShape>();
            var root = new Pcg32(seed);

            shapes.Add(MapShape.Rect(0, 0, width, height, C(Land(biome))));
            Fill(biome, 0, 0, width, height, null, shapes, buildings, root.Fork(1));

            if (regions != null)
            {
                int i = 0;
                foreach (var r in regions)
                {
                    shapes.Add(MapShape.Circle(r.X, r.Y, r.Radius, C(Land(r.Biome))));
                    Fill(r.Biome, r.X - r.Radius, r.Y - r.Radius, r.X + r.Radius, r.Y + r.Radius, r, shapes, buildings, root.Fork((ulong)(10 + i++)));
                }
            }

            WeatherPass(shapes, buildings, width, height, weather, root.Fork(99));
            return new MapBackground(width, height, shapes, biome, weather);
        }

        static int Land(Biome b)
        {
            switch (b)
            {
                case Biome.Forest: return 0xE3EED8;
                case Biome.Snow: return 0xF6F9FC;
                case Biome.Desert: return 0xF3E4C0;
                case Biome.Industrial: return 0xE9E6EA;
                case Biome.Harbor: return 0xEFEBE2;
                default: return 0xF2EFE9;
            }
        }

        static bool Inside(MapRegion? clip, int x, int y)
        {
            if (!clip.HasValue) return true;
            long dx = x - clip.Value.X, dy = y - clip.Value.Y;
            return dx * dx + dy * dy <= (long)clip.Value.Radius * clip.Value.Radius;
        }

        static void Fill(Biome biome, int x0, int y0, int x1, int y1, MapRegion? clip, List<MapShape> s, List<MapShape> buildings, IRandom rng)
        {
            switch (biome)
            {
                case Biome.Forest: Forest(x0, y0, x1, y1, clip, s, rng, snowy: false); break;
                case Biome.Snow: Forest(x0, y0, x1, y1, clip, s, rng, snowy: true); break;
                case Biome.Desert: Desert(x0, y0, x1, y1, clip, s, rng); break;
                case Biome.Industrial: Industrial(x0, y0, x1, y1, clip, s, buildings, rng); break;
                case Biome.Harbor: Harbor(x0, y0, x1, y1, clip, s, buildings, rng); break;
                default: City(x0, y0, x1, y1, clip, s, buildings, rng); break;
            }
        }

        // ------------------------------------------------------------------ Routes

        static void Road(List<MapShape> s, int ax, int ay, int bx, int by, int width, bool major = false)
        {
            s.Add(MapShape.Line(ax, ay, bx, by, width + 4, C(major ? MajorCasing : Casing)));
            s.Add(MapShape.Line(ax, ay, bx, by, width, C(major ? MajorFill : RoadFill)));
        }

        // ------------------------------------------------------------------ Biomes

        static void City(int x0, int y0, int x1, int y1, MapRegion? clip, List<MapShape> s, List<MapShape> buildings, IRandom rng)
        {
            // Quadrillage de rues irrégulier ; chaque îlot reçoit des bâtiments, quelques-uns un parc.
            var xs = Steps(x0, x1, 150, 230, rng);
            var ys = Steps(y0, y1, 130, 200, rng);
            for (int i = 0; i + 1 < xs.Count; i++)
                for (int j = 0; j + 1 < ys.Count; j++)
                {
                    int ax = xs[i] + 8, bx = xs[i + 1] - 8, ay = ys[j] + 8, by = ys[j + 1] - 8;
                    if (bx - ax < 24 || by - ay < 24 || !Inside(clip, (ax + bx) / 2, (ay + by) / 2)) continue;
                    if (rng.Chance(12)) { s.Add(MapShape.Rect(ax, ay, bx, by, C(0xC8FACC))); s.Add(MapShape.Circle((ax + bx) / 2, (ay + by) / 2, Math.Min(bx - ax, by - ay) / 4, C(0xA8E6AE))); continue; }
                    int parts = rng.Range(1, 4);
                    int w = (bx - ax) / parts;
                    for (int p = 0; p < parts; p++)
                    {
                        int h = rng.Range((by - ay) * 55 / 100, by - ay + 1);
                        var b = MapShape.Rect(ax + p * w + 3, ay, ax + (p + 1) * w - 3, ay + h, C(0xE0DCD4));
                        s.Add(b);
                        buildings.Add(b);
                    }
                }
            foreach (int x in xs) if (Inside(clip, x, (y0 + y1) / 2)) Road(s, x, y0, x, y1, 7);
            foreach (int y in ys) if (Inside(clip, (x0 + x1) / 2, y)) Road(s, x0, y, x1, y, 7);
            // Une avenue principale en diagonale.
            Road(s, x0, y0 + (y1 - y0) / 5, x1, y1 - (y1 - y0) / 4, 12, major: true);
        }

        static void Harbor(int x0, int y0, int x1, int y1, MapRegion? clip, List<MapShape> s, List<MapShape> buildings, IRandom rng)
        {
            // Mer en bas (un tiers), jetées et entrepôts le long de la côte, route côtière.
            int shore = y0 + (y1 - y0) / 3;
            for (int x = x0; x < x1; x += 40)
            {
                int bump = rng.Range(-22, 23);
                if (Inside(clip, x + 20, shore)) s.Add(MapShape.Rect(x, y0, x + 40, shore + bump, C(0xAADAFF)));
            }
            for (int x = x0 + 60; x < x1 - 60; x += rng.Range(130, 220))
            {
                if (!Inside(clip, x, shore)) continue;
                s.Add(MapShape.Rect(x, shore - 20, x + 26, shore + (y0 - shore) / 3, C(0xD9D5CB))); // jetée
            }
            for (int x = x0 + 20; x < x1 - 120; x += rng.Range(110, 170))
            {
                int y = shore + rng.Range(30, 80);
                if (!Inside(clip, x + 50, y + 30)) continue;
                var b = MapShape.Rect(x, y, x + rng.Range(70, 110), y + rng.Range(40, 70), C(0xDAD6CE));
                s.Add(b); buildings.Add(b);
            }
            Road(s, x0, shore + 24, x1, shore + 24, 9, major: true);
            for (int x = x0 + 80; x < x1; x += rng.Range(140, 220))
                if (Inside(clip, x, shore + 100)) Road(s, x, shore + 24, x, y1, 6);
        }

        static void Industrial(int x0, int y0, int x1, int y1, MapRegion? clip, List<MapShape> s, List<MapShape> buildings, IRandom rng)
        {
            // Grands lots à entrepôts, voie ferrée en pointillés, réservoirs.
            int by = y0 + (y1 - y0) / 2;
            for (int x = x0 + 10; x < x1 - 100; x += rng.Range(130, 200))
                for (int y = y0 + 10; y < y1 - 80; y += rng.Range(110, 160))
                {
                    if (!Inside(clip, x + 50, y + 35)) continue;
                    s.Add(MapShape.Rect(x - 4, y - 4, x + 124, y + 84, C(0xE2DFE6)));
                    var b = MapShape.Rect(x, y, x + rng.Range(70, 120), y + rng.Range(40, 80), C(0xD3D0CA));
                    s.Add(b); buildings.Add(b);
                    if (rng.Chance(30)) s.Add(MapShape.Circle(x + rng.Range(10, 110), y + rng.Range(10, 70), rng.Range(10, 18), C(0xC4C1BA)));
                }
            for (int x = x0; x < x1; x += 24) if (Inside(clip, x, by)) s.Add(MapShape.Rect(x, by - 2, x + 12, by + 2, C(0x9A9A9A))); // voie ferrée
            Road(s, x0, y0 + (y1 - y0) / 4, x1, y0 + (y1 - y0) / 4, 8, major: true);
            for (int x = x0 + 70; x < x1; x += rng.Range(160, 250)) if (Inside(clip, x, y0 + (y1 - y0) / 3)) Road(s, x, y0, x, y1, 6);
        }

        static void Forest(int x0, int y0, int x1, int y1, MapRegion? clip, List<MapShape> s, IRandom rng, bool snowy)
        {
            int tree = snowy ? 0xDCE9E3 : 0xA9D8A0, tree2 = snowy ? 0xC3D8CF : 0x8CC084, lake = snowy ? 0xD3E8F4 : 0xAADAFF;
            // Lac et sentiers, puis une forêt de cercles.
            int lx = x0 + (x1 - x0) * rng.Range(20, 70) / 100, ly = y0 + (y1 - y0) * rng.Range(25, 70) / 100;
            if (Inside(clip, lx, ly)) { s.Add(MapShape.Circle(lx, ly, rng.Range(50, 90), C(lake))); s.Add(MapShape.Circle(lx + 30, ly - 10, rng.Range(30, 55), C(lake))); }
            int px = x0, py = y0 + (y1 - y0) / 2;
            while (px < x1)
            {
                int nx = px + rng.Range(70, 130), ny = py + rng.Range(-60, 61);
                if (Inside(clip, px, py)) Road(s, px, py, nx, ny, 4);
                px = nx; py = Math.Max(y0 + 20, Math.Min(y1 - 20, ny));
            }
            int count = Math.Max(1, (x1 - x0) * (y1 - y0) / 700);
            for (int i = 0; i < count; i++)
            {
                int x = rng.Range(x0, x1), y = rng.Range(y0, y1);
                if (!Inside(clip, x, y)) continue;
                int dx = x - lx, dy = y - ly;
                if (dx * dx + dy * dy < 80 * 80) continue; // pas d'arbres dans le lac
                int r = rng.Range(9, 20);
                s.Add(MapShape.Circle(x, y, r, C(rng.Chance(50) ? tree : tree2)));
            }
        }

        static void Desert(int x0, int y0, int x1, int y1, MapRegion? clip, List<MapShape> s, IRandom rng)
        {
            int count = Math.Max(1, (x1 - x0) * (y1 - y0) / 9000);
            for (int i = 0; i < count; i++)
            {
                int x = rng.Range(x0, x1), y = rng.Range(y0, y1);
                if (!Inside(clip, x, y)) continue;
                int r = rng.Range(40, 110);
                s.Add(MapShape.Circle(x, y, r, C(rng.Chance(50) ? 0xEBD6A4 : 0xF8EDD2))); // dunes
            }
            int ox = rng.Range(x0 + 80, Math.Max(x0 + 81, x1 - 80)), oy = rng.Range(y0 + 60, Math.Max(y0 + 61, y1 - 60));
            if (Inside(clip, ox, oy)) { s.Add(MapShape.Circle(ox, oy, 38, C(0xAADAFF))); s.Add(MapShape.Circle(ox, oy, 52, C(0xBFDDA8))); s.Add(MapShape.Circle(ox, oy, 38, C(0xAADAFF))); }
            int roadY = y0 + (y1 - y0) / 2;
            int px = x0;
            while (px < x1)
            {
                int nx = px + rng.Range(120, 220), ny = roadY + rng.Range(-50, 51);
                if (Inside(clip, px, roadY)) Road(s, px, roadY, nx, ny, 6, major: true);
                px = nx; roadY = Math.Max(y0 + 20, Math.Min(y1 - 20, ny));
            }
        }

        // ------------------------------------------------------------------ Météo

        static void WeatherPass(List<MapShape> shapes, List<MapShape> buildings, int w, int h, Weather weather, IRandom rng)
        {
            if (weather == Weather.Day) return;
            for (int i = 0; i < shapes.Count; i++) shapes[i] = shapes[i].WithColor(Tint(shapes[i].Color, weather));
            int area = w * h;
            switch (weather)
            {
                case Weather.Night:
                    // Fenêtres éclairées dans une partie des bâtiments.
                    foreach (var b in buildings)
                    {
                        if (!rng.Chance(45) || b.X1 - b.X0 < 14 || b.Y1 - b.Y0 < 14) continue;
                        int n = rng.Range(2, 5);
                        for (int k = 0; k < n; k++)
                        {
                            int x = rng.Range(b.X0 + 3, b.X1 - 7), y = rng.Range(b.Y0 + 3, b.Y1 - 7);
                            shapes.Add(MapShape.Rect(x, y, x + 4, y + 4, C(0xFFE9A8)));
                        }
                    }
                    break;
                case Weather.Rain:
                    for (int i = 0; i < area / 2500; i++)
                    {
                        int x = rng.Range(0, w), y = rng.Range(0, h);
                        shapes.Add(MapShape.Line(x, y, x - 6, y - 18, 1, unchecked((int)0x66DCE6F2)));
                    }
                    break;
                case Weather.Snow:
                    for (int i = 0; i < area / 1500; i++)
                        shapes.Add(MapShape.Circle(rng.Range(0, w), rng.Range(0, h), rng.Range(1, 3), unchecked((int)0xCCFFFFFF)));
                    break;
                case Weather.Fog:
                    for (int i = 0; i < 14; i++)
                        shapes.Add(MapShape.Circle(rng.Range(0, w), rng.Range(0, h), rng.Range(60, 160), unchecked((int)0x30D8DEE4)));
                    break;
            }
        }

        static int Tint(int argb, Weather weather)
        {
            int a = (argb >> 24) & 0xFF, r = (argb >> 16) & 0xFF, g = (argb >> 8) & 0xFF, b = argb & 0xFF;
            switch (weather)
            {
                case Weather.Night: r = r * 40 / 100; g = g * 46 / 100; b = b * 70 / 100; break;
                case Weather.Snow: r = Mix(r, 255, 30); g = Mix(g, 255, 30); b = Mix(b, 255, 30); break;
                case Weather.Rain:
                {
                    int grey = (r + g + b) / 3;
                    r = (r * 70 + grey * 30) * 78 / 10000; g = (g * 70 + grey * 30) * 78 / 10000; b = (b * 70 + grey * 30) * 78 / 10000;
                    break;
                }
                case Weather.Fog: r = Mix(r, 0xB4, 40); g = Mix(g, 0xBA, 40); b = Mix(b, 0xC0, 40); break;
            }
            return (a << 24) | (Clamp(r) << 16) | (Clamp(g) << 8) | Clamp(b);
        }

        static int Mix(int a, int b, int percent) => (a * (100 - percent) + b * percent) / 100;
        static int Clamp(int v) => v < 0 ? 0 : v > 255 ? 255 : v;

        static List<int> Steps(int from, int to, int min, int max, IRandom rng)
        {
            var list = new List<int>();
            for (int v = from + rng.Range(min / 3, min); v < to; v += rng.Range(min, max + 1)) list.Add(v);
            if (list.Count < 2) { list.Clear(); list.Add(from + (to - from) / 3); list.Add(from + 2 * (to - from) / 3); }
            return list;
        }
    }
}
