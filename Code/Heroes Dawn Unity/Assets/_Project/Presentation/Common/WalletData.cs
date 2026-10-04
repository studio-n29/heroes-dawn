namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Valeurs affichées par la barre de ressources ; Gemmes en lecture seule, toujours 0 tant que la monnaie premium n'existe pas.</summary>
    public readonly struct WalletData
    {
        public readonly int Credits;
        public readonly int Pr;
        public readonly int Gems;
        public WalletData(int credits, int pr, int gems = 0) { Credits = credits; Pr = pr; Gems = gems; }
    }
}
