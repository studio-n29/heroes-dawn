#nullable enable
namespace N29.HeroesDawn.Definitions
{
    /// <summary>Archétype d'une carte (famille de jeu) : Brûlure, Entrave, Traque, Tempo, Rempart, Renouveau.</summary>
    public enum Archetype { Burn, Restrain, Hunt, Tempo, Bulwark, Renewal }

    /// <summary>Rôle d'une carte dans son archétype : Amorce, Gain, Pont ou Base.</summary>
    public enum CardRole { Primer, Payoff, Bridge, Basic }
}
