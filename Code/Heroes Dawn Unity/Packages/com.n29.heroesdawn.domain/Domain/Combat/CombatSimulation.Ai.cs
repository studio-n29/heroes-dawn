#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Combat
{
    /// <summary>
    /// IA (02_Combat) — alliés en auto : combo → soin si un allié &lt; 40 % → A3 → A2 → A1 sur l'ennemi le plus faible.
    /// Ennemis : Facile = au hasard ; Normal = A2/A3 dès que prêtes, cible la plus faible une fois sur deux ;
    /// Élite / Boss = priorités complètes.
    /// </summary>
    public sealed partial class CombatSimulation
    {
        void AiAct(CombatUnit u, List<CombatEvent> output)
        {
            if (u.Side == Side.Ally)
            {
                foreach (var combo in AvailableCombos())
                {
                    if (combo.A == u.Id || combo.B == u.Id)
                    {
                        if (TryPerformCombo(combo.A, combo.B, WeakestFoe(u.Side), output)) return;
                    }
                }
            }

            // Une action annoncée sur la frise (zone de préparation) est celle qui s exécute, si elle est toujours possible.
            int slot, target;
            if (u.PlannedSlot >= 0 && CanUseSlot(u, u.PlannedSlot)) { slot = u.PlannedSlot; target = u.PlannedTarget; }
            else (slot, target) = ChooseAction(u);
            u.PlannedSlot = -1;
            u.PlannedTarget = -1;
            PerformSkill(u, u.Skills[slot], slot, target, output);
        }

        /// <summary>Choix de la compétence et de la cible selon le rang (même logique qu avant, déplacée pour pouvoir annoncer l action à l avance).</summary>
        (int Slot, int Target) ChooseAction(CombatUnit u)
        {
            bool smart = u.Side == Side.Ally || Setup.Rank == CombatRank.Elite || Setup.Rank == CombatRank.Boss;
            int slot;
            if (smart)
            {
                int healSlot = FindSlot(u, EffectType.Heal);
                if (healSlot >= 0 && CanUseSlot(u, healSlot) && LowestHpPermille(u.Side) < 400) slot = healSlot;
                else if (CanUseSlot(u, 2)) slot = 2;
                else if (CanUseSlot(u, 1)) slot = 1;
                else slot = 0;
                return (slot, WeakestFoe(u.Side));
            }

            if (Setup.Rank == CombatRank.Normal)
            {
                slot = CanUseSlot(u, 2) ? 2 : CanUseSlot(u, 1) ? 1 : 0;
                int target = _rng.Chance(50) ? WeakestFoe(u.Side) : RandomFoe(u.Side);
                return (slot, target);
            }


            var usable = new List<int>();
            for (int i = 0; i < ActiveSkillCount(u) && i < 3; i++) if (CanUseSlot(u, i)) usable.Add(i);
            slot = usable.Count == 0 ? 0 : usable[_rng.Range(0, usable.Count)];
            return (slot, RandomFoe(u.Side));
        }

        bool CanUseSlot(CombatUnit u, int slot)
            => slot < ActiveSkillCount(u) && slot < 3 && u.Cooldowns[slot] == 0 && !(u.Has(StatusType.Bind) && slot > 0);

        static int FindSlot(CombatUnit u, EffectType t)
        {
            for (int i = 0; i < u.Skills.Count && i < 3; i++)
                foreach (var fx in u.Skills[i].Effects)
                    if (fx.Type == t) return i;
            return -1;
        }

        int WeakestFoe(Side self)
        {
            Side foe = self == Side.Ally ? Side.Enemy : Side.Ally;
            int best = -1;
            int bestHp = int.MaxValue;
            foreach (var x in _units)
                if (x.Side == foe && x.IsAlive && x.Hp < bestHp) { bestHp = x.Hp; best = x.Id; }
            return best;
        }

        int RandomFoe(Side self)
        {
            Side foe = self == Side.Ally ? Side.Enemy : Side.Ally;
            var alive = new List<int>();
            foreach (var x in _units) if (x.Side == foe && x.IsAlive) alive.Add(x.Id);
            return alive.Count == 0 ? -1 : alive[_rng.Range(0, alive.Count)];
        }

        long LowestHpPermille(Side side)
        {
            long best = 1000;
            foreach (var x in _units)
                if (x.Side == side && x.IsAlive) best = System.Math.Min(best, (long)x.Hp * 1000 / x.MaxHp);
            return best;
        }
    }
}
