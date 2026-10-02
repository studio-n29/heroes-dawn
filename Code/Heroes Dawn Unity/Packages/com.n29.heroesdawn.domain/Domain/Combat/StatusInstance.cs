using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Combat
{
    public sealed class StatusInstance
    {
        public StatusType Type { get; }
        /// <summary>Tours restants (décrémentés au début de chaque tour du porteur).</summary>
        public int TurnsLeft { get; set; }
        /// <summary>Bouclier : points restants. Autres : inutilisé.</summary>
        public int Value { get; set; }
        public StatusInstance(StatusType type, int turns, int value = 0) { Type = type; TurnsLeft = turns; Value = value; }
    }
}
