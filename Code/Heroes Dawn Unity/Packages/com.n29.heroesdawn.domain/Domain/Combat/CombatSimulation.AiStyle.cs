#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Combat
{
    /// <summary>
    /// IA des ennemis par style (bloc C de la 1.0.0, spec §1) : chaque style a ses priorités de compétence et de cible.
    /// Un style qui ne trouve aucune compétence adaptée retombe sur l'attaque la plus forte, jamais sur une exception.
    /// </summary>
    public sealed partial class CombatSimulation
    {
        /// <summary>Statuts de contrôle recherchés par le style Control.</summary>
        static bool IsControlStatus(StatusType s)
            => s == StatusType.Bind || s == StatusType.Shock || s == StatusType.Freeze || s == StatusType.Mark || s == StatusType.Vulnerable || s == StatusType.Blind;

        static bool IsPositiveStatus(StatusType s)
            => s == StatusType.Shield || s == StatusType.Regen || s == StatusType.Haste || s == StatusType.Evade || s == StatusType.Anchor;

        static int DamageOf(SkillDef s)
        {
            int total = 0;
            foreach (var fx in s.Effects) if (fx.Type == EffectType.Damage) total += fx.Value;
            return total;
        }

        static bool HitsFoes(SkillDef s) => s.Target == TargetRule.SingleEnemy || s.Target == TargetRule.AllEnemies;

        /// <summary>Compétence utilisable de dégâts les plus lourds (à égalité, l'emplacement le plus haut) ; l'emplacement 0 si rien n'est utilisable.</summary>
        int HeaviestDamageSlot(CombatUnit u)
        {
            int best = 0, bestDamage = -1;
            for (int i = 0; i < ActiveSkillCount(u); i++)
            {
                if (!CanUseSlot(u, i)) continue;
                int d = DamageOf(u.Skills[i]);
                if (d >= bestDamage) { bestDamage = d; best = i; }
            }
            return best;
        }

        /// <summary>Premier emplacement utilisable (du plus haut au plus bas) qui satisfait <paramref name="match"/> ; -1 sinon.</summary>
        int FindUsableSlot(CombatUnit u, System.Func<SkillDef, bool> match)
        {
            for (int i = ActiveSkillCount(u) - 1; i >= 0; i--)
                if (CanUseSlot(u, i) && match(u.Skills[i])) return i;
            return -1;
        }

        static bool IsProtective(SkillDef s)
        {
            foreach (var fx in s.Effects)
            {
                if (fx.Type == EffectType.Shield) return true;
                if (fx.Type == EffectType.ApplyStatus && fx.Status == StatusType.Taunt && s.Target == TargetRule.Self) return true;
            }
            return false;
        }

        static bool IsSupportive(SkillDef s)
        {
            if (s.Target == TargetRule.SingleEnemy || s.Target == TargetRule.AllEnemies) return false;
            foreach (var fx in s.Effects)
            {
                if (fx.Type == EffectType.Heal || fx.Type == EffectType.Shield) return true;
                if (fx.Type == EffectType.ApplyStatus && IsPositiveStatus(fx.Status)) return true;
            }
            return false;
        }

        static bool IsHealing(SkillDef s)
        {
            foreach (var fx in s.Effects) if (fx.Type == EffectType.Heal) return true;
            return false;
        }

        static bool IsControlling(SkillDef s)
        {
            if (!HitsFoes(s)) return false;
            foreach (var fx in s.Effects) if (fx.Type == EffectType.ApplyStatus && IsControlStatus(fx.Status)) return true;
            return false;
        }

        bool HasControlStatus(CombatUnit x)
        {
            foreach (var s in x.Statuses) if (IsControlStatus(s.Type)) return true;
            return false;
        }

        /// <summary>Ennemi vivant du côté <paramref name="foeOf"/> adverse qui maximise <paramref name="score"/> (premier à égalité) ; -1 s'il n'y en a pas.</summary>
        int BestFoe(Side self, System.Func<CombatUnit, bool> eligible, System.Func<CombatUnit, int> score)
        {
            Side foe = self == Side.Ally ? Side.Enemy : Side.Ally;
            int best = -1, bestScore = int.MinValue;
            foreach (var x in _units)
            {
                if (x.Side != foe || !x.IsAlive || !eligible(x)) continue;
                int sc = score(x);
                if (sc > bestScore) { bestScore = sc; best = x.Id; }
            }
            return best;
        }

        /// <summary>Compétence et cible d'un ennemi selon son style ; toujours un emplacement valide.</summary>
        (int Slot, int Target) ChooseStyledAction(CombatUnit u)
        {
            int weakest = WeakestFoe(u.Side);
            switch (u.Style)
            {
                case AiStyle.Aggressive:
                    return (HeaviestDamageSlot(u), weakest);

                case AiStyle.Defensive:
                {
                    int threat = BestFoe(u.Side, x => true, x => x.Atk);
                    bool hurt = LowestHpPermille(u.Side) < 600;
                    if (hurt && !u.Has(StatusType.Shield) && !u.Has(StatusType.Taunt))
                    {
                        int guard = FindUsableSlot(u, IsProtective);
                        if (guard >= 0) return (guard, threat);
                    }
                    return (HeaviestDamageSlot(u), threat);
                }

                case AiStyle.Control:
                {
                    int fastest = BestFoe(u.Side, x => !HasControlStatus(x), x => x.Spd);
                    if (fastest >= 0)
                    {
                        int ctrl = FindUsableSlot(u, IsControlling);
                        if (ctrl >= 0) return (ctrl, fastest);
                    }
                    return (HeaviestDamageSlot(u), weakest);
                }

                case AiStyle.Support:
                {
                    int ally = LowestHpRatio(u.Side);
                    if (ally >= 0 && _units[ally].Hp * 1000L / _units[ally].MaxHp < 500)
                    {
                        int heal = FindUsableSlot(u, IsHealing);
                        if (heal < 0) heal = FindUsableSlot(u, IsSupportive);
                        if (heal >= 0) return (heal, ally);
                    }
                    return (HeaviestDamageSlot(u), weakest);
                }

                default:
                    return ChooseBalancedAction(u);
            }
        }
    }
}
