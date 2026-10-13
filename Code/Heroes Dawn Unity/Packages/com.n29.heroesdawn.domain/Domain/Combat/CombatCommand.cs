namespace N29.HeroesDawn.Domain.Combat
{
    public enum CommandType { Skill, SetAuto, Retreat, Pass }

    public sealed class CombatCommand
    {
        public CommandType Type { get; }
        public int Actor { get; }
        public int Partner { get; }
        public int SlotIndex { get; }
        public int Target { get; }
        public bool Flag { get; }

        CombatCommand(CommandType type, int actor, int partner, int slotIndex, int target, bool flag)
        {
            Type = type; Actor = actor; Partner = partner; SlotIndex = slotIndex; Target = target; Flag = flag;
        }

        public static CombatCommand UseSkill(int actor, int slotIndex, int target) => new CombatCommand(CommandType.Skill, actor, -1, slotIndex, target, false);
        public static CombatCommand SetAuto(bool on) => new CombatCommand(CommandType.SetAuto, -1, -1, -1, -1, on);
        /// <summary>Le joueur passe son tour (delai de 30 s ecoule) : l unite repart au point de reprise leger.</summary>
        public static CombatCommand Pass(int actor) => new CombatCommand(CommandType.Pass, actor, -1, -1, -1, false);
        public static CombatCommand Retreat() => new CombatCommand(CommandType.Retreat, -1, -1, -1, -1, false);
    }

    /// <summary>Commande journalisée avec le tick où elle a été appliquée.</summary>
    public sealed class CombatInput
    {
        public int Tick { get; }
        public CombatCommand Command { get; }
        public CombatInput(int tick, CombatCommand command) { Tick = tick; Command = command; }
    }
}
