#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Application.Session;

namespace N29.HeroesDawn.Application.Features.Auth
{
    /// <summary>Récupération passive de fatigue hors mission, selon le temps écoulé depuis la dernière session.</summary>
    public static class FatigueRecovery
    {
        /// <summary>Applique la récupération à tout l'effectif et renvoie le total de points récupérés.</summary>
        public static int Apply(IReadOnlyList<TeamMember> roster, DateTime nowUtc, long lastSeenUtcTicks, int perMinute = MetaRules.FatigueRecoveryPerMinute)
        {
            if (lastSeenUtcTicks <= 0) return 0;
            var elapsed = nowUtc - new DateTime(lastSeenUtcTicks, DateTimeKind.Utc);
            if (elapsed.TotalMinutes < 1) return 0;
            int points = (int)Math.Min(100, elapsed.TotalMinutes * perMinute);
            int total = 0;
            foreach (var m in roster)
            {
                int before = m.Fatigue;
                m.AddFatigue(-points);
                total += before - m.Fatigue;
            }
            return total;
        }
    }
}
