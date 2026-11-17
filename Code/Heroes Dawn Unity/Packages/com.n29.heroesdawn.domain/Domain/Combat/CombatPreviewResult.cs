#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Combat
{
    /// <summary>Un statut qui apparaît ou disparaît d'une unité, avec la chance (1 à 100 %) que cela arrive d'après les échantillons.</summary>
    public readonly struct StatusChange
    {
        public StatusType Status { get; }
        public int ChancePercent { get; }
        public StatusChange(StatusType status, int chancePercent) { Status = status; ChancePercent = chancePercent; }
    }

    /// <summary>
    /// Effet prévu d'une carte sur une unité. Les valeurs « After » sont des moyennes arrondies des échantillons ; Min et Max donnent la plage ;
    /// « Variable » est vrai dès que les échantillons ne s'accordent pas (critique, dispersion, chance, cible aléatoire).
    /// </summary>
    public sealed class UnitPreview
    {
        public int Id { get; set; }
        public int HpBefore { get; set; }
        public int MaxHp { get; set; }
        public int HpAfter { get; set; }
        public int HpMin { get; set; }
        public int HpMax { get; set; }
        public int ShieldBefore { get; set; }
        public int ShieldAfter { get; set; }
        public int EnergyBefore { get; set; }
        public int EnergyAfter { get; set; }
        public int PointBefore { get; set; }
        public int PointAfter { get; set; }
        public int PointMin { get; set; }
        public int PointMax { get; set; }
        /// <summary>Chances (0 à 100 %) que l'unité soit vaincue par la carte.</summary>
        public int DefeatPercent { get; set; }
        /// <summary>Compétence annoncée sur la frise avant la carte (-1 : aucune) et chance (0 à 100 %) que l'annonce soit annulée.</summary>
        public int PlannedBefore { get; set; }
        public int PlanCancelledPercent { get; set; }
        public IReadOnlyList<StatusChange> Added { get; set; } = System.Array.Empty<StatusChange>();
        public IReadOnlyList<StatusChange> Removed { get; set; } = System.Array.Empty<StatusChange>();

        public bool HpVariable => HpMin != HpMax;
        public bool PointVariable => PointMin != PointMax;
        public bool ShieldVariable { get; set; }

        /// <summary>Un seul élément de l'aperçu de cette unité n'est pas certain.</summary>
        public bool IsVariable
        {
            get
            {
                if (HpVariable || PointVariable || ShieldVariable) return true;
                if (DefeatPercent > 0 && DefeatPercent < 100) return true;
                if (PlanCancelledPercent > 0 && PlanCancelledPercent < 100) return true;
                foreach (var s in Added) if (s.ChancePercent < 100) return true;
                foreach (var s in Removed) if (s.ChancePercent < 100) return true;
                return false;
            }
        }

        /// <summary>La carte change quelque chose pour cette unité.</summary>
        public bool HasChange
            => HpAfter != HpBefore || HpVariable || ShieldAfter != ShieldBefore || ShieldVariable || PointAfter != PointBefore || PointVariable
               || EnergyAfter != EnergyBefore || DefeatPercent > 0 || PlanCancelledPercent > 0 || Added.Count > 0 || Removed.Count > 0;
    }

    /// <summary>Aperçu complet d'une commande : une entrée par unité présente avant la carte, plus l'issue du combat.</summary>
    public sealed class CombatPreviewResult
    {
        readonly List<UnitPreview> _units;
        public IReadOnlyList<UnitPreview> Units => _units;
        public int Samples { get; }
        /// <summary>Chances (0 à 100 %) que la carte termine le combat par une victoire, par une défaite, ou fasse entrer la vague suivante.</summary>
        public int VictoryPercent { get; }
        public int DefeatPercent { get; }
        public int NextWavePercent { get; }

        public UnitPreview Unit(int id) => _units[id];

        public CombatPreviewResult(List<UnitPreview> units, int samples, int victory, int defeat, int nextWave)
        {
            _units = units; Samples = samples; VictoryPercent = victory; DefeatPercent = defeat; NextWavePercent = nextWave;
        }
    }
}
