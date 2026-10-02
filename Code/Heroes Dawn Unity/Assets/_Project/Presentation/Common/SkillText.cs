using System.Collections.Generic;
using System.Text;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Description courte d'une compétence, générée à partir de ses effets (évite 40 textes à maintenir).</summary>
    public static class SkillText
    {
        public static string Describe(SkillDef s)
        {
            var sb = new StringBuilder();
            sb.Append(Loc.T("target." + s.Target));
            foreach (var line in Effects(s.Effects))
            {
                sb.Append(" · ");
                sb.Append(line);
            }
            return sb.ToString();
        }

        public static IEnumerable<string> Effects(IReadOnlyList<EffectDef> effects)
        {
            foreach (var fx in effects)
            {
                switch (fx.Type)
                {
                    case EffectType.Damage: yield return Loc.F("fx.damage", fx.Value); break;
                    case EffectType.Heal: yield return Loc.F("fx.heal", fx.Value); break;
                    case EffectType.Shield: yield return Loc.F("fx.shield", fx.Value); break;
                    case EffectType.GaugeDelta:
                        yield return fx.Value >= 0 ? Loc.F("fx.gauge_up", fx.Value / 10) : Loc.F("fx.gauge_down", -fx.Value / 10);
                        break;
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
