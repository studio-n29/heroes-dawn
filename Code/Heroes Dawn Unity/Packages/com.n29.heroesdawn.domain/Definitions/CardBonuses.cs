using System.Collections.Generic;

namespace N29.HeroesDawn.Definitions
{
    /// <summary>
    /// Bonus des cartes équipées par un agent (bloc A de la 1.0.0) : +15 % si le type de la carte est boosté par sa position,
    /// +5 % si la carte est de sa famille ; les deux se cumulent (jusqu à +20 %). Réservé aux alliés : les ennemis n en reçoivent jamais.
    /// </summary>
    public static class CardBonuses
    {
        public const int PositionPercent = 15, FamilyPercent = 5;

        /// <summary>Total en % pour une carte portée par un agent : +15 si le type est boosté par sa position, +5 si même famille.</summary>
        public static int Percent(SkillCardDef card, MemberDef member)
        {
            int total = 0;
            var boosted = Positions.BoostedTypes(member.Position);
            for (int i = 0; i < boosted.Count; i++) if (boosted[i] == card.Nature) { total += PositionPercent; break; }
            if (card.Family == member.Family) total += FamilyPercent;
            return total;
        }

        /// <summary>
        /// La compétence avec sa valeur principale augmentée de <paramref name="bonusPercent"/> % : dégâts, soins, boucliers,
        /// recul, élan et jauge ; chance des altérations (plafonnée à 100 %) et durée (arrondie). 0 : inchangée.
        /// </summary>
        public static SkillDef Apply(SkillDef skill, int bonusPercent)
        {
            if (bonusPercent <= 0) return skill;
            int pct = 100 + bonusPercent;
            return skill.MapEffects(e =>
            {
                switch (e.Type)
                {
                    case EffectType.Damage:
                    case EffectType.Heal:
                    case EffectType.Shield:
                    case EffectType.PushBack:
                    case EffectType.Advance:
                    case EffectType.GaugeDelta:
                        return new EffectDef(e.Type, e.Value * pct / 100, e.Status, e.Duration, e.ChancePercent);
                    case EffectType.ApplyStatus:
                        int chance = e.ChancePercent >= 100 ? e.ChancePercent : System.Math.Min(100, e.ChancePercent * pct / 100);
                        return new EffectDef(e.Type, e.Value, e.Status, (e.Duration * pct + 50) / 100, chance);
                    default:
                        return e;
                }
            });
        }
    }
}
