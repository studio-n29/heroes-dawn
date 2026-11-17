#nullable enable
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Presentation.Combat
{
    /// <summary>Mort annoncée par l'aperçu : aucune, possible (chance), ou sûre.</summary>
    public enum PreviewDefeat { None, Possible, Sure }

    /// <summary>Un statut ajouté ou retiré par la carte ; <see cref="Possible"/> : il n'arrive pas à coup sûr (pointillé).</summary>
    public struct PreviewChip
    {
        public StatusType Type;
        public string Label;
        public bool Possible;
    }

    /// <summary>Ce que la carte change sur une unité : barre de PV fantôme, delta, statuts, mort, action annulée.</summary>
    public struct UnitPreviewData
    {
        public int Id;
        public bool Ally;
        public float HpBefore01;
        public float HpAfter01;
        /// <summary>« −28 », « + 12 », « ≈ −27 » (moyenne quand c'est variable) ; vide : PV inchangés.</summary>
        public string HpDeltaText;
        /// <summary>Bouclier gagné ou perdu (« Bouclier +20 ») ; vide : inchangé.</summary>
        public string ShieldText;
        /// <summary>Un élément de cette unité n'est pas certain (critique, dispersion, chance, cible aléatoire).</summary>
        public bool Variable;
        /// <summary>Badge « variable » (vide si tout est certain).</summary>
        public string VariableLabel;
        public PreviewDefeat Defeat;
        public PreviewChip[] Added;
        public PreviewChip[] Removed;
        /// <summary>Action annoncée annulée (« Action annulée ») ; vide sinon.</summary>
        public string PlanText;
    }

    /// <summary>Fantôme de la frise : où l'unité se retrouvera après la carte.</summary>
    public struct PreviewGhostData
    {
        public int UnitId;
        public bool Ally;
        public int FromPoint;
        public int ToPoint;
        public bool Variable;
        public PreviewDefeat Defeat;
        /// <summary>« ◀ 3 » recul, « ▶ 2 » avance, « ≈ ◀ 2 » variable, « ✕ » vaincue.</summary>
        public string Text;
    }

    /// <summary>Aperçu complet affiché pendant qu'une carte est armée ou posée sur une cible.</summary>
    public sealed class CombatPreviewData
    {
        public UnitPreviewData[] Units = System.Array.Empty<UnitPreviewData>();
        public PreviewGhostData[] Ghosts = System.Array.Empty<PreviewGhostData>();
        /// <summary>« VICTOIRE », « Victoire possible », « Vague suivante » ; vide : le combat continue.</summary>
        public string Outcome = "";
    }
}
