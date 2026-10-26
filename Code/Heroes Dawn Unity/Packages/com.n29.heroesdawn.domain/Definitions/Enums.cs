namespace N29.HeroesDawn.Definitions
{
    public enum Family { Matter, Elemental, PhysicalBio, Perception }
    public enum Role { Tank, Damage, Control, Support, Precision }
    public enum Rarity { Agent, Elite, Legend }
    public enum CaseType { Entry, Empty, Combat, MiniGame, Narrative, Resource, Rest }
    public enum CombatRank { Easy, Normal, Elite, Boss }
    public enum SkillSlot { A1, A2, A3, Passive, A4 }
    public enum TargetRule { SingleEnemy, AllEnemies, SingleAlly, AllAllies, Self }
    public enum EffectType
    {
        Damage, Heal, ApplyStatus, GaugeDelta, Shield,
        /// <summary>Impact : repousse la cible en arrière sur la frise (Value = % de la frise) ; une seule fois par action de la cible.</summary>
        PushBack,
        /// <summary>Élan : propulse la cible vers le point d'action (Value = % de la frise) ; à 100 % elle agit aussitôt, avant toute action ennemie.</summary>
        Advance,
        /// <summary>Vol de vie : le lanceur récupère Value % des dégâts infligés à la cible par cette compétence.</summary>
        Lifesteal,
        /// <summary>Purge : retire Value statuts (0 = tous) — négatifs sur un allié, positifs sur un ennemi ; les plus récents d'abord.</summary>
        Purge,
        /// <summary>Rebond : un autre ennemi vivant subit le premier effet de dégâts de la compétence à Value % de sa puissance.</summary>
        Bounce,
        /// <summary>Exécution : dégâts de la compétence +Value % sur une cible sous le seuil de PV (BalanceConfig.ExecuteThresholdPercent).</summary>
        Execute,
    }
    /// <summary>Famille d une compétence : Dégâts, Soins, Protection, Boost, Gestion du temps.</summary>
    public enum SkillCategory { Damage, Heal, Protection, Boost, TimeManagement }
    public enum StatusType { None, Burn, Freeze, Shock, Corrosion, Blind, Vulnerable, Bind, Mark, Taunt, Shield, Regen, Haste, Evade, Anchor }
    public enum ObjectiveType { DefeatCombat, ReachCase, CompleteMiniGame, ResolveNarrative }
}
