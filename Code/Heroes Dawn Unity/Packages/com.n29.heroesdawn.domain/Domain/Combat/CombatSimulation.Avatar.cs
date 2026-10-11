#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Combat
{
    /// <summary>
    /// Compétences d avatar (le joueur lui-même) : lançables seulement pendant la fenêtre d interruption (point d action vide) ou pendant le tour d un agent.
    /// La recharge ne baisse que pendant le temps actif (voir <see cref="CombatSimulation.IsAwaitingAction"/>).
    /// Elles ne consomment le tour de personne ; la puissance est celle de l'allié vivant le plus fort.
    /// </summary>
    public sealed partial class CombatSimulation
    {
        int[]? _avatarCooldowns;

        int[] AvatarCooldowns => _avatarCooldowns ??= new int[_c.AvatarSkills.Count];

        public IReadOnlyList<AvatarSkillDef> AvatarSkills => _c.AvatarSkills;

        /// <summary>Ticks de temps actif restants avant que la compétence d'avatar soit de nouveau disponible.</summary>
        public int AvatarCooldownLeft(int index) => index >= 0 && index < AvatarCooldowns.Length ? AvatarCooldowns[index] : 0;

        public bool CanUseAvatar(int index)
            => !IsOver && (InterruptWindowOpen || IsAwaitingAction) && index >= 0 && index < AvatarCooldowns.Length && AvatarCooldowns[index] == 0 && AvatarCaster() != null;

        void AdvanceAvatar()
        {
            var cd = AvatarCooldowns;
            for (int i = 0; i < cd.Length; i++) if (cd[i] > 0) cd[i]--;
        }

        CombatUnit? AvatarCaster()
        {
            CombatUnit? best = null;
            foreach (var u in _units)
                if (u.Side == Side.Ally && u.IsAlive && (best == null || u.Atk > best.Atk)) best = u;
            return best;
        }

        bool TryAvatar(int index, int target, List<CombatEvent> output)
        {
            if (!CanUseAvatar(index)) return false;
            var def = _c.AvatarSkills[index];
            var caster = AvatarCaster()!;
            var targets = ResolveTargets(caster, def.Target, target);
            AvatarCooldowns[index] = def.CooldownTicks;
            output.Add(new AvatarSkillUsedEvent(Tick, def.Id, def.Category, targets.ToArray()));
            ApplyEffects(caster, def.Effects, targets, output);
            return true;
        }
    }
}
