#nullable enable
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
            if (s.Pick != TargetPick.Default)
            {
                sb.Append(" · ");
                sb.Append(Loc.F("pick." + s.Pick, s.PickStatus == StatusType.None ? "" : Loc.StatusName(s.PickStatus)));
            }
            foreach (var line in Effects(s.Effects, owner))
            {
                sb.Append(" · ");
                sb.Append(line);
            }
            return sb.ToString();
        }

        /// <summary>Avec les statistiques de l'agent, les dégâts, soins et boucliers sont des montants bruts ; sans elles, des pourcentages. Un effet conditionnel est préfixé de sa condition ; ses modificateurs suivent, une ligne chacun.</summary>
        public static IEnumerable<string> Effects(IReadOnlyList<EffectDef> effects, SkillOwnerStats? owner = null)
        {
            foreach (var fx in effects)
            {
                string? line = BaseLine(fx, owner);
                if (line == null) continue;
                yield return fx.When != null ? Loc.F("fx.conditional", ConditionText(fx.When), line) : line;
                foreach (var mod in fx.Mods) yield return ModLine(mod);
            }
        }

        static string ConditionText(Condition c)
            => Loc.F("cond." + c.Kind, c.Status == StatusType.None ? "" : Loc.StatusName(c.Status), c.Value);

        static string ModLine(ValueMod m)
        {
            if (m.Per.HasValue)
            {
                string scaled = Loc.F("fx.mod_scaled", m.Bonus, Loc.T("scale." + m.Per.Value), m.MaxCount);
                return m.When != null ? Loc.F("fx.conditional", ConditionText(m.When), scaled) : scaled;
            }
            return m.When != null ? Loc.F("fx.mod_flat", m.Bonus, ConditionText(m.When)) : Loc.F("fx.mod_always", m.Bonus);
        }

        static string? BaseLine(EffectDef fx, SkillOwnerStats? owner)
        {
            switch (fx.Type)
            {
                case EffectType.Damage:
                    return owner.HasValue ? Loc.F("fx.damage_raw", owner.Value.Atk * fx.Value / 100) : Loc.F("fx.damage", fx.Value);
                case EffectType.Heal:
                    return owner.HasValue ? Loc.F("fx.heal_raw", owner.Value.MaxHp * fx.Value / 100) : Loc.F("fx.heal", fx.Value);
                case EffectType.Shield:
                    return owner.HasValue ? Loc.F("fx.shield_raw", owner.Value.MaxHp * fx.Value / 100) : Loc.F("fx.shield", fx.Value);
                case EffectType.PushBack: return Loc.F("fx.pushback", fx.Value);
                case EffectType.Advance: return Loc.F("fx.advance", fx.Value);
                case EffectType.GaugeDelta:
                    return fx.Value >= 0 ? Loc.F("fx.gauge_up", fx.Value / 10) : Loc.F("fx.gauge_down", -fx.Value / 10);
                case EffectType.Lifesteal: return Loc.F("fx.lifesteal", fx.Value);
                case EffectType.Purge: return fx.Value <= 0 ? Loc.T("fx.purge_all") : Loc.F("fx.purge", fx.Value);
                case EffectType.Bounce: return Loc.F("fx.bounce", fx.Value);
                case EffectType.Execute: return Loc.F("fx.execute", fx.Value, ExecuteThreshold);
                case EffectType.ApplyStatus:
                    int turns = fx.Duration >= 99 ? 1 : fx.Duration;
                    return fx.ChancePercent >= 100
                        ? Loc.F("fx.status", Loc.StatusName(fx.Status), turns)
                        : Loc.F("fx.status_chance", Loc.StatusName(fx.Status), turns, fx.ChancePercent);
                default: return null;
            }
        }
    }
}
