#nullable enable
using System;
using System.Collections.Generic;

namespace N29.HeroesDawn.Application.Ports
{
    /// <summary>Inventaire d'objets, collection de cartes, boosters en réserve et boutique d'objets du jour (dans le profil, avec la méta-progression).</summary>
    [Serializable]
    public sealed class CollectionSave
    {
        /// <summary>Objets possédés (identifiant, nombre) ; ceux qui sont équipés sont comptés ici aussi.</summary>
        public List<CounterSave> items = new List<CounterSave>();
        /// <summary>Cartes possédées (identifiant, nombre).</summary>
        public List<CounterSave> cards = new List<CounterSave>();
        /// <summary>Niveau des cartes améliorées (identifiant, niveau) ; une carte absente est de niveau 1.</summary>
        public List<CounterSave> cardLevels = new List<CounterSave>();
        /// <summary>Boosters non ouverts (identifiant, nombre).</summary>
        public List<CounterSave> boosters = new List<CounterSave>();
        /// <summary>Boosters sans carte Rare ou mieux d'affilée (pitié).</summary>
        public int pity;
        /// <summary>Nombre de boosters ouverts : fait varier la graine de chaque tirage.</summary>
        public int opened;
        /// <summary>Jour des offres d'objets mémorisées et rangs déjà achetés ce jour-là.</summary>
        public string itemShopDay = "";
        public List<int> itemShopBought = new List<int>();
    }
}
