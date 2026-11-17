#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Combat;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Combat
{
    /// <summary>Traduit un <see cref="CombatPreviewResult"/> du moteur en données d'affichage (textes localisés, fractions de barres, fantômes de frise).</summary>
    public static class CombatPreviewMapper
    {
        const string Minus = "−", Approx = "≈ ", Back = "◀ ", Forward = "▶ ", Cross = "✕";

        public static CombatPreviewData Build(CombatPreviewResult result, ILocalization loc, Func<int, bool> isAlly)
        {
            var units = new List<UnitPreviewData>();
            var ghosts = new List<PreviewGhostData>();
            foreach (var u in result.Units)
            {
                if (!u.HasChange) continue;
                var defeat = u.DefeatPercent >= 100 ? PreviewDefeat.Sure : u.DefeatPercent > 0 ? PreviewDefeat.Possible : PreviewDefeat.None;
                bool variable = u.IsVariable;
                units.Add(new UnitPreviewData
                {
                    Id = u.Id,
                    Ally = isAlly(u.Id),
                    HpBefore01 = Fraction(u.HpBefore, u.MaxHp),
                    HpAfter01 = Fraction(u.HpAfter, u.MaxHp),
                    HpDeltaText = HpDelta(u),
                    ShieldText = ShieldDelta(u, loc),
                    Variable = variable,
                    VariableLabel = variable ? loc.T("preview.variable") : "",
                    Defeat = defeat,
                    Added = Chips(u.Added, loc),
                    Removed = Chips(u.Removed, loc),
                    PlanText = u.PlannedBefore >= 0 && u.PlanCancelledPercent > 0
                        ? loc.T(u.PlanCancelledPercent >= 100 ? "preview.plan_cancelled" : "preview.plan_cancelled_maybe") : "",
                });
                if (u.PointAfter != u.PointBefore || u.PointVariable || defeat != PreviewDefeat.None)
                    ghosts.Add(Ghost(u, defeat, isAlly(u.Id)));
            }
            return new CombatPreviewData { Units = units.ToArray(), Ghosts = ghosts.ToArray(), Outcome = Outcome(result, loc) };
        }

        static float Fraction(int hp, int max) => max <= 0 ? 0f : Math.Max(0f, Math.Min(1f, hp / (float)max));

        static string HpDelta(UnitPreview u)
        {
            int d = u.HpAfter - u.HpBefore;
            if (d == 0 && !u.HpVariable) return "";
            string body = d < 0 ? Minus + (-d) : d > 0 ? "+" + d : "±0";
            return u.HpVariable ? Approx + body : body;
        }

        static string ShieldDelta(UnitPreview u, ILocalization loc)
        {
            int d = u.ShieldAfter - u.ShieldBefore;
            if (d == 0) return "";
            return loc.F("preview.shield", (d > 0 ? "+" : Minus) + Math.Abs(d));
        }

        static PreviewChip[] Chips(IReadOnlyList<StatusChange> changes, ILocalization loc)
        {
            var chips = new PreviewChip[changes.Count];
            for (int i = 0; i < chips.Length; i++)
                chips[i] = new PreviewChip { Type = changes[i].Status, Label = loc.StatusName(changes[i].Status), Possible = changes[i].ChancePercent < 100 };
            return chips;
        }

        static PreviewGhostData Ghost(UnitPreview u, PreviewDefeat defeat, bool ally)
        {
            int delta = u.PointAfter - u.PointBefore;
            string text = defeat == PreviewDefeat.Sure ? Cross
                        : delta < 0 ? Back + (-delta)
                        : delta > 0 ? Forward + delta
                        : "";
            if (u.PointVariable && defeat != PreviewDefeat.Sure) text = Approx + (text.Length > 0 ? text : "?");
            if (defeat == PreviewDefeat.Possible) text += (text.Length > 0 ? " " : "") + Cross + "?";
            return new PreviewGhostData { UnitId = u.Id, Ally = ally, FromPoint = u.PointBefore, ToPoint = u.PointAfter, Variable = u.PointVariable, Defeat = defeat, Text = text };
        }

        static string Outcome(CombatPreviewResult r, ILocalization loc)
        {
            if (r.VictoryPercent >= 100) return loc.T("preview.victory");
            if (r.VictoryPercent > 0) return loc.T("preview.victory_maybe");
            if (r.NextWavePercent > 0) return loc.T(r.NextWavePercent >= 100 ? "preview.next_wave" : "preview.next_wave_maybe");
            if (r.DefeatPercent > 0) return loc.T("preview.defeat");
            return "";
        }
    }
}
