#nullable enable
using System.Collections.Generic;

namespace N29.HeroesDawn.Application.Session
{
    /// <summary>Ce que l'interface lit de la boutique éphémère.</summary>
    public interface IShopReadModel
    {
        /// <summary>Jour de boutique (<c>yyyy-MM-dd</c>) ; vide tant qu'aucune boutique n'a été tirée.</summary>
        string Day { get; }
        /// <summary>Numéro du tirage du jour = nombre de renouvellements faits aujourd'hui.</summary>
        int Draw { get; }
        /// <summary>Dernier jour de boutique vu sur l'onglet Recruter ; différent de <see cref="Day"/> = badge « N ».</summary>
        string SeenDay { get; }
        IReadOnlyList<ShopOffer> Offers { get; }
    }
}
