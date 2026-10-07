#nullable enable
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Activities
{
    /// <summary>Terrain d'un niveau de survie : neige, herbe haute ou sable.</summary>
    public enum TerrainKind { Snow, TallGrass, Sand }

    /// <summary>
    /// Couleurs plates d'un environnement : la teinte de base vient du biome, la météo la transforme (nuit sombre, neige blanchie,
    /// pluie ternie, brume grisée). Calcul entier, sans aléa : mêmes couleurs pour tous les joueurs.
    /// </summary>
    public readonly struct EnvironmentPalette
    {
        public int Ground { get; }
        public int GroundAlt { get; }
        public int Water { get; }
        public int Foliage { get; }
        public int Obstacle { get; }
        public int Sky { get; }
        public int Accent { get; }
        public int Totem { get; }
        public int Fire { get; }
        public int Target { get; }

        EnvironmentPalette(int ground, int groundAlt, int water, int foliage, int obstacle, int sky, int accent)
        {
            Ground = ground; GroundAlt = groundAlt; Water = water; Foliage = foliage; Obstacle = obstacle; Sky = sky; Accent = accent;
            Totem = 0x8A5A3A; Fire = 0xFF8A2A; Target = 0xFFD23A; // repères de jeu : jamais teintés, toujours lisibles
        }

        public int Terrain(TerrainKind kind)
        {
            switch (kind)
            {
                case TerrainKind.Snow: return Mix(Ground, 0xF2F6FA, 60);
                case TerrainKind.TallGrass: return Foliage;
                default: return Mix(GroundAlt, 0xD8B878, 70);
            }
        }

        public static EnvironmentPalette For(ZoneEnvironment env)
        {
            int ground, alt, water, foliage, obstacle, accent;
            switch (env.Biome)
            {
                case Biome.Harbor: (ground, alt, water, foliage, obstacle, accent) = (0x6B6A5E, 0x8A7F6A, 0x2F6F9F, 0x5E7F4A, 0x7A4F3A, 0xE07A41); break;
                case Biome.Industrial: (ground, alt, water, foliage, obstacle, accent) = (0x4A4A4F, 0x6A5A4A, 0x4A6A5A, 0x5A6A3A, 0x8A8A90, 0xE0C020); break;
                case Biome.Forest: (ground, alt, water, foliage, obstacle, accent) = (0x4F7A3F, 0x6A8F4A, 0x2F7F8F, 0x2E6A34, 0x6B4F36, 0xE0D060); break;
                case Biome.Snow: (ground, alt, water, foliage, obstacle, accent) = (0xE8EEF5, 0xC8D6E6, 0x7FB4D8, 0x4F7A5A, 0x7A8A9A, 0xE05A4A); break;
                case Biome.Desert: (ground, alt, water, foliage, obstacle, accent) = (0xD8B878, 0xC89A58, 0x3F9FB0, 0x7A8A3A, 0x9A6A3A, 0xE0603A); break;
                default: (ground, alt, water, foliage, obstacle, accent) = (0x5A5F6B, 0x7A7F8C, 0x3A6EA5, 0x4E8C4A, 0x8C6D5A, 0xE0B341); break;
            }
            return new EnvironmentPalette(Tint(ground, env.Weather), Tint(alt, env.Weather), Tint(water, env.Weather), Tint(foliage, env.Weather),
                                          Tint(obstacle, env.Weather), SkyOf(env.Weather), accent);
        }

        static int SkyOf(Weather w)
        {
            switch (w)
            {
                case Weather.Night: return 0x0E1630;
                case Weather.Snow: return 0xDDE6EE;
                case Weather.Rain: return 0x5A6672;
                case Weather.Fog: return 0xB8C0C8;
                default: return 0x87CEEB;
            }
        }

        static int Tint(int c, Weather w)
        {
            switch (w)
            {
                case Weather.Night: return Rgb(R(c) * 45 / 100, G(c) * 50 / 100, B(c) * 70 / 100);
                case Weather.Snow: return Mix(c, 0xFFFFFF, 35);
                case Weather.Rain:
                {
                    int grey = (R(c) + G(c) + B(c)) / 3;
                    return Rgb((R(c) * 75 + grey * 25) * 75 / 10000, (G(c) * 75 + grey * 25) * 75 / 10000, (B(c) * 75 + grey * 25) * 75 / 10000);
                }
                case Weather.Fog: return Mix(c, 0xB0B8C0, 45);
                default: return c;
            }
        }

        /// <summary>Mélange a vers b ; percent = part de b (0-100).</summary>
        public static int Mix(int a, int b, int percent)
            => Rgb((R(a) * (100 - percent) + R(b) * percent) / 100, (G(a) * (100 - percent) + G(b) * percent) / 100, (B(a) * (100 - percent) + B(b) * percent) / 100);

        static int R(int c) => (c >> 16) & 0xFF;
        static int G(int c) => (c >> 8) & 0xFF;
        static int B(int c) => c & 0xFF;
        static int Rgb(int r, int g, int b) => (Clamp(r) << 16) | (Clamp(g) << 8) | Clamp(b);
        static int Clamp(int v) => v < 0 ? 0 : v > 255 ? 255 : v;
    }
}
