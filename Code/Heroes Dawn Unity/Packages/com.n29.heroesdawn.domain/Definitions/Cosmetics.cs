#nullable enable
using System.Collections.Generic;

namespace N29.HeroesDawn.Definitions
{
    public enum CosmeticType { Outfit, HqTheme, Dice, Pawn, Frame, Title }

    /// <summary>D'où vient un cosmétique : boutique (Gemmes), pass, tournoi, succès ou événement. Seule la source <see cref="Shop"/> est en vente.</summary>
    public enum CosmeticSource { Shop, Pass, Tournament, Achievement, Event }

    /// <summary>Un cosmétique du catalogue. Il ne change jamais une statistique ni un résultat de combat.</summary>
    public sealed class CosmeticDef
    {
        public string Id { get; }
        /// <summary>Clé de texte du nom : <c>cosmetic.&lt;id&gt;.name</c>.</summary>
        public string NameKey { get; }
        public CosmeticType Type { get; }
        /// <summary>Prix en Gemmes ; 0 hors boutique.</summary>
        public int Price { get; }
        public CosmeticSource Source { get; }

        public CosmeticDef(string id, string nameKey, CosmeticType type, int price, CosmeticSource source)
        {
            Id = id; NameKey = nameKey; Type = type; Price = price; Source = source;
        }
    }
}
