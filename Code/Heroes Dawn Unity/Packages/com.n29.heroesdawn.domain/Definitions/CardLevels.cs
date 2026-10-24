using System.Collections.Generic;

namespace N29.HeroesDawn.Definitions
{
    /// <summary>
    /// Niveaux des cartes de compétence : une carte en doublon et des Crédits la font monter (niveau 5 au plus).
    /// Chaque niveau augmente de 15 % les dégâts, soins et boucliers de la carte.
    /// </summary>
    public static class CardLevels
    {
        public const int Max = 5;
        public const int PercentPerLevel = 15;

        /// <summary>Exemplaires en doublon consommés pour passer du niveau <paramref name="level"/> au suivant.</summary>
        public static int CopiesToUpgrade(int level) => level;

        /// <summary>Crédits dépensés pour passer du niveau <paramref name="level"/> au suivant.</summary>
        public static int CreditsToUpgrade(int level) => 250 * level;

        /// <summary>Efficacité de la carte à ce niveau, en % (100 au niveau 1).</summary>
        public static int EffectPercent(int level) => 100 + PercentPerLevel * (System.Math.Max(1, level) - 1);

        /// <summary>La compétence de la carte à ce niveau : dégâts, soins et boucliers augmentés ; les autres effets sont inchangés.</summary>
        public static SkillDef Scale(SkillDef skill, int level)
        {
            if (level <= 1) return skill;
            int pct = EffectPercent(level);
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
