namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Barre de ressources (Crédits, PR) et indicateur de sauvegarde.</summary>
    public interface IWalletView
    {
        void Show(WalletData data);
        void SetSaving(bool saving);
    }
}
