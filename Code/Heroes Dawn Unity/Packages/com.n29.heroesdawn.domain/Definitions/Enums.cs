namespace N29.HeroesDawn.Definitions
{
    public enum Family { Matter, Elemental, PhysicalBio, Perception }
    public enum Role { Tank, Damage, Control, Support, Precision }
    public enum Rarity { Agent, Elite, Legend }
    public enum CaseType { Entry, Empty, Combat, MiniGame, Narrative, Resource, Rest }
    public enum CombatRank { Easy, Normal, Elite, Boss }
    public enum SkillSlot { A1, A2, A3, Passive }
    public enum TargetRule { SingleEnemy, AllEnemies, SingleAlly, AllAllies, Self }
    public enum EffectType
    {
        Damage, Heal, ApplyStatus, GaugeDelta, Shield,
        /// <summary>Impact : repousse la cible en arrière sur la frise (Value = % de la frise) ; une seule fois par action de la cible.</summary>
        PushBack,
        /// <summary>Élan : propulse la cible vers le point d'action (Value = % de la frise) ; à 100 % elle agit aussitôt, avant toute action ennemie.</summary>
        Advance,
    }
    /// <summary>Famille d une compétence : Dégâts, Soins, Protection, Boost, Gestion du temps.</summary>
    public enum SkillCategory { Damage, Heal, Protection, Boost, TimeManagement }
    public enum StatusType { None, Burn, Freeze, Shock, Corrosion, Blind, Vulnerable, Bind, Mark, Taunt, Shield, Regen, Haste, Evade, Anchor }
    public enum ObjectiveType { DefeatCombat, ReachCase, CompleteMiniGame, ResolveNarrative }
}
