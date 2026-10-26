using System.Collections.Generic;

namespace N29.HeroesDawn.Definitions
{
    /// <summary>
    /// Niveaux des cartes de compétence : des doublons et des Crédits la font monter (niveau 10 au plus).
    /// Chaque niveau augmente de 10 % les dégâts, soins et boucliers de base ; les niveaux 3, 6 et 10 débloquent un palier d'effets.
    /// </summary>
    public static class CardLevels
    {
        public const int Max = 10;
        public const int PercentPerLevel = 10;

        /// <summary>Niveaux qui débloquent un palier d'effets secondaires.</summary>
        public static readonly int[] TierLevels = { 3, 6, 10 };

        /// <summary>Exemplaires en doublon consommés pour passer du niveau <paramref name="level"/> au suivant.</summary>
        public static int CopiesToUpgrade(int level) => (level + 1) / 2;

        /// <summary>Crédits dépensés pour passer du niveau <paramref name="level"/> au suivant.</summary>
        public static int CreditsToUpgrade(int level) => 200 * level;

        /// <summary>Efficacité de la carte à ce niveau, en % (100 au niveau 1).</summary>
        public static int EffectPercent(int level) => 100 + PercentPerLevel * (System.Math.Max(1, level) - 1);

        /// <summary>La compétence de la carte à ce niveau : dégâts, soins et boucliers augmentés ; les autres effets sont inchangés.</summary>
        public static SkillDef Scale(SkillDef skill, int level) => ScalePercent(skill, EffectPercent(level));

        /// <summary>La compétence avec dégâts, soins et boucliers à <paramref name="pct"/> % (100 : inchangée).</summary>
        public static SkillDef ScalePercent(SkillDef skill, int pct)
        {
            if (pct == 100) return skill;
            var list = new List<EffectDef>(skill.Effects.Count);
            foreach (var e in skill.Effects)
            {
                bool scaled = e.Type == EffectType.Damage || e.Type == EffectType.Heal || e.Type == EffectType.Shield;
                list.Add(scaled ? new EffectDef(e.Type, e.Value * pct / 100, e.Status, e.Duration, e.ChancePercent) : e);
            }
            return new SkillDef(skill.Id, skill.Slot, skill.Cooldown, skill.Target, list, skill.Cost);
        }
    }
}
