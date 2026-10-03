namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Valeurs affichées par la barre de ressources.</summary>
    public readonly struct WalletData
    {
        public readonly int Credits;
        public readonly int Pr;
        public WalletData(int credits, int pr) { Credits = credits; Pr = pr; }
    }
}
