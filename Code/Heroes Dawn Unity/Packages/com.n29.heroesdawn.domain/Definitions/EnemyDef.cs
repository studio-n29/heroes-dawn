using System;
using System.Collections.Generic;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Definitions
{
    public sealed class EnemyDef
    {
        public EnemyId Id { get; }
        public string NameKey { get; }
        public Role Role { get; }
        public Family Family { get; }
        public StatBlock BaseStats { get; }
        public IReadOnlyList<SkillDef> Skills { get; }
        public bool IsBoss { get; }
        /// <summary>Phases d'un boss, de la première à la dernière (seuils décroissants) ; vide : le combat n'a qu'une phase.</summary>
        public IReadOnlyList<BossPhaseDef> Phases { get; }

        public EnemyDef(EnemyId id, string nameKey, Role role, Family family, StatBlock baseStats, IReadOnlyList<SkillDef> skills, bool isBoss = false,
                        IReadOnlyList<BossPhaseDef> phases = null)
        {
            Id = id; NameKey = nameKey; Role = role; Family = family; BaseStats = baseStats; Skills = skills; IsBoss = isBoss;
            Phases = phases ?? Array.Empty<BossPhaseDef>();
            if (Phases.Count > 0 && !isBoss) throw new ArgumentException("Seul un boss a des phases : " + id.Value);
            for (int i = 0; i < Phases.Count; i++)
            {
                int pct = Phases[i].ThresholdPercent;
                if (pct < 1 || pct > 99) throw new ArgumentException("Seuil de phase hors de 1 à 99 % : " + id.Value);
                if (i > 0 && pct >= Phases[i - 1].ThresholdPercent) throw new ArgumentException("Seuils de phase non décroissants : " + id.Value);
            }
        }
    }
}
