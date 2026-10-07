namespace N29.HeroesDawn.Domain.Combat
{
    public enum CommandType { Skill, Combo, SetAuto, Retreat, TimeSkip, Avatar }

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
        public static CombatCommand UseCombo(int a, int b, int target) => new CombatCommand(CommandType.Combo, a, b, -1, target, false);
        /// <summary>Combo synchronisé : le partenaire n est pas encore prêt, l équipe dépense du Temps pour le rattraper.</summary>
        public static CombatCommand SyncCombo(int a, int b, int target) => new CombatCommand(CommandType.Combo, a, b, -1, target, true);
        /// <summary>Dépassement : l allié <paramref name="ally"/> passe à 100 % de la frise, l ennemi <paramref name="enemy"/> est repoussé et son action coupée.</summary>
        public static CombatCommand TimeSkip(int ally, int enemy) => new CombatCommand(CommandType.TimeSkip, ally, -1, -1, enemy, false);
        /// <summary>Compétence d avatar <paramref name="index"/> (catalogue), lançable à tout moment.</summary>
        public static CombatCommand UseAvatar(int index, int target) => new CombatCommand(CommandType.Avatar, -1, -1, index, target, false);
        public static CombatCommand SetAuto(bool on) => new CombatCommand(CommandType.SetAuto, -1, -1, -1, -1, on);
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
