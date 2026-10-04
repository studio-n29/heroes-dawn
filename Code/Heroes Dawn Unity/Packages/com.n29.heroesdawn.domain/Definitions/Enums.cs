namespace N29.HeroesDawn.Definitions
{
    public enum Family { Matter, Elemental, PhysicalBio, Perception }
    public enum Role { Tank, Damage, Control, Support, Precision }
    public enum Rarity { Agent, Elite, Legend }
    public enum CaseType { Entry, Empty, Combat, MiniGame, Narrative, Resource, Rest }
    public enum CombatRank { Easy, Normal, Elite, Boss }
    public enum SkillSlot { A1, A2, A3, Passive }
    public enum TargetRule { SingleEnemy, AllEnemies, SingleAlly, AllAllies, Self }
    public enum EffectType { Damage, Heal, ApplyStatus, GaugeDelta, Shield }
    public enum StatusType { None, Burn, Freeze, Shock, Corrosion, Blind, Vulnerable, Bind, Mark, Taunt, Shield, Regen, Haste, Evade, Anchor }
    public enum ObjectiveType { DefeatCombat, ReachCase, CompleteMiniGame, ResolveNarrative }
}
