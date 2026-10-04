#nullable enable
namespace N29.HeroesDawn.Application.Session
{
    /// <summary>Portefeuille en lecture seule (l'interface ne peut pas le modifier).</summary>
    public interface IWalletReadModel
    {
        int Credits { get; }
        int Pr { get; }
        int Gems { get; }
    }
}
