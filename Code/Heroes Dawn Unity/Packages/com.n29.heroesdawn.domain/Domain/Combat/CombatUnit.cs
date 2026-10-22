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
        /// <summary>Position sur la frise : 0 = depart, ActionPoint = point d action ; negative pendant une canalisation.</summary>
        public int Point { get; set; }
        /// <summary>Point d action de la frise (nombre de points du depart a l action).</summary>
        public int ActionPoint { get; }
        /// <summary>Énergie disponible : chaque compétence en coûte, elle se régénère à chaque pas de la frise.</summary>
        public int Energy { get; set; } = 100;
        public int MaxEnergy { get; set; } = 100;
        /// <summary>Énergie en plus à chaque pas de la frise (effet d équipe).</summary>
        public int EnergyRegenBonus { get; set; }
        public List<StatusInstance> Statuses { get; } = new List<StatusInstance>();
        public IReadOnlyList<SkillDef> Skills { get; }
        /// <summary>Compétence annoncée sur la frise (ennemis, zone de préparation) ; -1 = aucune.</summary>
        public int PlannedSlot { get; set; } = -1;
        public int PlannedTarget { get; set; } = -1;
        /// <summary>Reste de la brulure (degats par point de frise), en PV x %.</summary>
        public long BurnRemainder { get; set; }
        /// <summary>Déjà repoussée par une compétence depuis sa dernière action : une autre attaque ne la repousse pas (anti-blocage).</summary>
        public bool KnockbackGuard { get; set; }
        /// <summary>Chances de critique ajoutees par les objets (en %).</summary>
        public int CritBonusPercent { get; set; }
        /// <summary>Avance de depart sur la frise donnee par les objets (en points).</summary>
        public int StartPointBonus { get; set; }

        /// <summary>Vrai tant que l unite n a pas rattrape sa canalisation (point negatif).</summary>
        public bool IsChannelling => Point < 0;

        public bool IsAlive => Hp > 0;
        public bool IsReady => IsAlive && Point >= ActionPoint;

        public CombatUnit(int id, Side side, string defId, Family family, Role role, bool isBoss,
                          int hp, int atk, int def, int spd, int actionPoint, IReadOnlyList<SkillDef> skills)
        {
            Id = id; Side = side; DefId = defId; Family = family; Role = role; IsBoss = isBoss;
            MaxHp = hp; Hp = hp; Atk = atk; Def = def; Spd = spd; ActionPoint = actionPoint; Skills = skills;
        }

        public StatusInstance? Find(StatusType t)
        {
            foreach (var s in Statuses) if (s.Type == t) return s;
            return null;
        }

        public bool Has(StatusType t) => Find(t) != null;
    }
}
