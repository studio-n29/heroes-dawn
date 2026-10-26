using System.Collections.Generic;
using System.Text;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Statistiques de l'agent qui porte la compétence : permettent d'afficher les montants bruts (Atk × valeur, PV max × valeur).</summary>
    public readonly struct SkillOwnerStats
    {
        public readonly int Atk;
        public readonly int MaxHp;
        public SkillOwnerStats(int atk, int maxHp) { Atk = atk; MaxHp = maxHp; }
    }

    /// <summary>Description courte d'une compétence, générée à partir de ses effets (évite 40 textes à maintenir).</summary>
    public static class SkillText
    {
        /// <summary>Seuil de PV de l'exécution affiché dans les textes (valeur d'équilibrage par défaut).</summary>
        static readonly int ExecuteThreshold = new BalanceConfig().ExecuteThresholdPercent;

        public static string Describe(SkillDef s, SkillOwnerStats? owner = null)
        {
            var sb = new StringBuilder();
            sb.Append(Loc.T("target." + s.Target));
            foreach (var line in Effects(s.Effects, owner))
            {
                sb.Append(" · ");
                sb.Append(line);
            }
            return sb.ToString();
        }

        /// <summary>Avec les statistiques de l'agent, les dégâts, soins et boucliers sont des montants bruts ; sans elles, des pourcentages.</summary>
        public static IEnumerable<string> Effects(IReadOnlyList<EffectDef> effects, SkillOwnerStats? owner = null)
        {
            foreach (var fx in effects)
            {
                switch (fx.Type)
                {
                    case EffectType.Damage:
                        yield return owner.HasValue ? Loc.F("fx.damage_raw", owner.Value.Atk * fx.Value / 100) : Loc.F("fx.damage", fx.Value);
                        break;
                    case EffectType.Heal:
                        yield return owner.HasValue ? Loc.F("fx.heal_raw", owner.Value.MaxHp * fx.Value / 100) : Loc.F("fx.heal", fx.Value);
                        break;
                    case EffectType.Shield:
                        yield return owner.HasValue ? Loc.F("fx.shield_raw", owner.Value.MaxHp * fx.Value / 100) : Loc.F("fx.shield", fx.Value);
                        break;
                    case EffectType.PushBack: yield return Loc.F("fx.pushback", fx.Value); break;
                    case EffectType.Advance: yield return Loc.F("fx.advance", fx.Value); break;
                    case EffectType.GaugeDelta:
                        yield return fx.Value >= 0 ? Loc.F("fx.gauge_up", fx.Value / 10) : Loc.F("fx.gauge_down", -fx.Value / 10);
                        break;
                    case EffectType.Lifesteal: yield return Loc.F("fx.lifesteal", fx.Value); break;
                    case EffectType.Purge: yield return fx.Value <= 0 ? Loc.T("fx.purge_all") : Loc.F("fx.purge", fx.Value); break;
                    case EffectType.Bounce: yield return Loc.F("fx.bounce", fx.Value); break;
                    case EffectType.Execute: yield return Loc.F("fx.execute", fx.Value, ExecuteThreshold); break;
                    case EffectType.ApplyStatus:
                        int turns = fx.Duration >= 99 ? 1 : fx.Duration;
                        yield return fx.ChancePercent >= 100
                            ? Loc.F("fx.status", Loc.StatusName(fx.Status), turns)
                            : Loc.F("fx.status_chance", Loc.StatusName(fx.Status), turns, fx.ChancePercent);
                        break;
                }
            }
        }
    }
}
