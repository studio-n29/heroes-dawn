#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Definitions
{
    /// <summary>
    /// Combat défini sur une case : une table d'ennemis et un nombre de manches (vagues). À l'arrivée, chaque manche tire entre
    /// <see cref="EnemiesMin"/> et <see cref="EnemiesMax"/> ennemis dans la table (graine de la partie). Sans CombatSpec, la case
    /// utilise les rencontres de la zone (par rang).
    /// </summary>
    public sealed class CombatSpec
    {
        public IReadOnlyList<EnemyId> EnemyTable { get; }
        public int Rounds { get; }
        public int EnemiesMin { get; }
        public int EnemiesMax { get; }

        public CombatSpec(IReadOnlyList<EnemyId> enemyTable, int rounds = 1, int enemiesMin = 2, int enemiesMax = 3)
        {
            EnemyTable = enemyTable;
            Rounds = rounds < 1 ? 1 : rounds;
            EnemiesMin = enemiesMin < 1 ? 1 : enemiesMin;
            EnemiesMax = enemiesMax < EnemiesMin ? EnemiesMin : enemiesMax;
        }
    }

    /// <summary>
    /// Fond de carte généré d'une zone : clé de la ressource (Resources/Zones/&lt;clé&gt;) et rectangle de grille qu'il couvre
    /// (mêmes unités que <see cref="NodeDef.X"/> / <see cref="NodeDef.Y"/>).
    /// </summary>
    public sealed class ZoneBackground
    {
        public string Key { get; }
        public int MinX { get; }
        public int MinY { get; }
        public int MaxX { get; }
        public int MaxY { get; }

        public ZoneBackground(string key, int minX, int minY, int maxX, int maxY)
        {
            Key = key; MinX = minX; MinY = minY; MaxX = maxX; MaxY = maxY;
        }
    }

    /// <summary>Position d'une zone sur la carte de la ville (0 à 1000 depuis le coin haut gauche).</summary>
    public readonly struct ZoneMapPoint
    {
        public int XPermille { get; }
        public int YPermille { get; }
        public ZoneMapPoint(int xPermille, int yPermille) { XPermille = xPermille; YPermille = yPermille; }
    }
}
