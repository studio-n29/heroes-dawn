#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Combat
{
    public sealed class CombatUnit
    {
        public int Id { get; }
        public Side Side { get; }
        public string DefId { get; }
        public Family Family { get; }
        public Role Role { get; }
        public bool IsBoss { get; }
        public int MaxHp { get; }
        public int Hp { get; set; }
        public int Atk { get; set; }
        public int Def { get; set; }
        public int Spd { get; set; }
        /// <summary>Jauge ATB en milli-points ; pleine à GaugeMax.</summary>
        public long Gauge { get; set; }
        public long GaugeMax { get; }
        /// <summary>Recharges des compétences A1, A2, A3 (0 = disponible).</summary>
        public int[] Cooldowns { get; } = new int[3];
        public List<StatusInstance> Statuses { get; } = new List<StatusInstance>();
        public IReadOnlyList<SkillDef> Skills { get; }
        public int ActionsSinceCombo { get; set; } = 99;
        /// <summary>Compétence annoncée sur la frise (ennemis, zone de préparation) ; -1 = aucune.</summary>
        public int PlannedSlot { get; set; } = -1;
        public int PlannedTarget { get; set; } = -1;
        /// <summary>Reste de la brûlure (dégâts au pas de frise) en unités de jauge × %.</summary>
        public long BurnRemainder { get; set; }

        /// <summary>Position sur la frise en % (0 = départ, 100 = point d action) ; négative pendant une canalisation.</summary>
        public int TimelinePercent => (int)(Gauge * 100 / GaugeMax);

        public bool IsAlive => Hp > 0;
        public bool IsReady => IsAlive && Gauge >= GaugeMax;

        public CombatUnit(int id, Side side, string defId, Family family, Role role, bool isBoss,
                          int hp, int atk, int def, int spd, long gaugeMax, IReadOnlyList<SkillDef> skills)
        {
            Id = id; Side = side; DefId = defId; Family = family; Role = role; IsBoss = isBoss;
            MaxHp = hp; Hp = hp; Atk = atk; Def = def; Spd = spd; GaugeMax = gaugeMax; Skills = skills;
        }

        public StatusInstance? Find(StatusType t)
        {
            foreach (var s in Statuses) if (s.Type == t) return s;
            return null;
        }

        public bool Has(StatusType t) => Find(t) != null;
    }
}
