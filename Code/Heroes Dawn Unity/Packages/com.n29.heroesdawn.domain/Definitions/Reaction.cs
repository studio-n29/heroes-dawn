#nullable enable
namespace N29.HeroesDawn.Definitions
{
    /// <summary>Événement qui déclenche une réaction, relatif au camp du porteur (valeurs sérialisées en entiers : n'ajouter qu'en fin).</summary>
    public enum ReactionTrigger
    {
        /// <summary>Un ennemi du porteur reçoit le statut <see cref="ReactionDef.Status"/>.</summary>
        EnemyReceivesStatus,
        /// <summary>Un allié du porteur (porteur compris) reçoit le statut <see cref="ReactionDef.Status"/>.</summary>
        AllyReceivesStatus,
        /// <summary>Un ennemi du porteur est vaincu.</summary>
        EnemyDefeated,
        /// <summary>Un autre allié du porteur est vaincu.</summary>
        AllyDefeated,
        /// <summary>Le porteur perd des PV sous le coup d'une unité (ennemi ou allié) ; jamais par la brûlure, ni quand le bouclier absorbe tout.</summary>
        CarrierHit,
        /// <summary>Le porteur inflige un coup critique.</summary>
        CarrierCrit,
    }

    /// <summary>
    /// Réaction (P2) installée sur le lanceur par un effet <see cref="EffectType.ArmReaction"/> (une fois par lancer, après les effets sur les cibles) : pendant <see cref="Turns"/> tours du porteur,
    /// chaque <see cref="Trigger"/> applique <see cref="Effect"/> (valeur fixe, jamais remise à l'échelle par niveau ni bonus).
    /// Cible : l'unité concernée par l'événement si <see cref="OnTriggerUnit"/> (receveur du statut, unité vaincue, attaquant du porteur, ennemi touché par son critique) ;
    /// sinon la règle <see cref="Target"/> + <see cref="Pick"/> d'une compétence lancée par le porteur (<see cref="TargetRule.Self"/> : le porteur).
    /// Sans cible nommée sur la fiche : déclencheur pour les deux événements « reçoit un statut », le porteur sinon (choix fait à l'import).
    /// </summary>
    public sealed class ReactionDef
    {
        public ReactionTrigger Trigger { get; }
        /// <summary>Statut attendu (EnemyReceivesStatus, AllyReceivesStatus) ; None sinon.</summary>
        public StatusType Status { get; }
        /// <summary>Durée en tours du porteur (décrémentée au début de chacun de ses tours).</summary>
        public int Turns { get; }
        public EffectDef Effect { get; }
        public TargetRule Target { get; }
        public TargetPick Pick { get; }
        public StatusType PickStatus { get; }
        /// <summary>Vise l'unité concernée par l'événement (cible « déclencheur ») au lieu de <see cref="Target"/>.</summary>
        public bool OnTriggerUnit { get; }

        public ReactionDef(ReactionTrigger trigger, StatusType status, int turns, EffectDef effect, TargetRule target = TargetRule.Self,
                           TargetPick pick = TargetPick.Default, StatusType pickStatus = StatusType.None, bool onTriggerUnit = false)
        {
            Trigger = trigger; Status = status; Turns = turns; Effect = effect;
            Target = target; Pick = pick; PickStatus = pickStatus; OnTriggerUnit = onTriggerUnit;
        }
    }
}
