#nullable enable
using N29.HeroesDawn.Application.Features.Combat;

namespace N29.HeroesDawn.Application.Messages
{
    /// <summary>Le combat est terminé et son issue appliquée (Crédits, fatigue, plateau).</summary>
    public readonly struct CombatFinished
    {
        public readonly CombatReport Report;
        public CombatFinished(CombatReport report) { Report = report; }
    }
}
