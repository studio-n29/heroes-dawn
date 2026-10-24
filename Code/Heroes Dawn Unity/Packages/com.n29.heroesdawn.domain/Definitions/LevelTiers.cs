using System.Collections.Generic;

namespace N29.HeroesDawn.Definitions
{
    /// <summary>Un palier de niveau : à ce niveau, l'agent gagne un bonus passif permanent sur une statistique.</summary>
    public readonly struct LevelTier
    {
        public int Level { get; }
        public SynergyStat Stat { get; }
        public int Percent { get; }
        public LevelTier(int level, SynergyStat stat, int percent) { Level = level; Stat = stat; Percent = percent; }
    }

    /// <summary>
    /// Paliers de niveau des agents : des augmentations passives directement sur l'agent (par exemple +20 % d'attaque au niveau 10).
    /// Les bonus se cumulent : un agent de niveau 30 a ceux des niveaux 10, 20 et 30.
    /// </summary>
    public static class LevelTiers
    {
        public static readonly IReadOnlyList<LevelTier> All = new[]
        {
            new LevelTier(10, SynergyStat.Atk, 20),
            new LevelTier(20, SynergyStat.Hp, 20),
            new LevelTier(30, SynergyStat.Def, 20),
            new LevelTier(40, SynergyStat.Spd, 10),
            new LevelTier(50, SynergyStat.Atk, 20),
        };

        /// <summary>Bonus cumulés de tous les paliers atteints à ce niveau.</summary>
        public static SynergyBonus BonusAt(int level)
        {
            int hp = 0, atk = 0, def = 0, spd = 0;
            foreach (var t in All)
            {
                if (t.Level > level) continue;
                switch (t.Stat)
                {
                    case SynergyStat.Hp: hp += t.Percent; break;
                    case SynergyStat.Atk: atk += t.Percent; break;
                    case SynergyStat.Def: def += t.Percent; break;
                    case SynergyStat.Spd: spd += t.Percent; break;
                }
            }
            return new SynergyBonus(hp, atk, def, spd);
        }
    }
}
