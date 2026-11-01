#nullable enable
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Carte d'un héros de la boutique (M30 adapté) : cadre de rareté, halo de Légende, prix sur le bouton.</summary>
    public struct RecruitCardState
    {
        public int Slot;
        public MemberCardData Card;
        public Rarity Rarity;
        public string PriceLabel;
        public bool InTeam;
        public string InTeamText;
        public bool Affordable;
        public bool Halo;
    }

    /// <summary>Lot de scraps de la boutique du jour : texte du bouton (quantité, nom, prix) et disponibilité.</summary>
    public struct RecruitScrapState
    {
        public int Slot;
        public string Text;
        public bool Enabled;
    }

    /// <summary>Tout ce qu'affiche la page Recruter.</summary>
    public struct RecruitPageData
    {
        public string Rule;
        public string Countdown;
        public RecruitCardState[] Cards;
        /// <summary>Lots de scraps du jour (six au plus), sous les cartes.</summary>
        public RecruitScrapState[] Scraps;
        /// <summary>« Collection de New York complète ! » quand il n'y a plus rien à tirer ; vide sinon.</summary>
        public string EmptyText;
        public string RerollLabel;
        public bool RerollEnabled;
        public string Progress;
    }
}
