#nullable enable
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Board
{
    /// <summary>D'où vient un booster : exploit de jeu, progression narrative, mini-jeu ou boutique.</summary>
    public enum BoosterSource { Exploit, Narrative, MiniGame, Shop }

    /// <summary>Résultat de l'ouverture d'un booster : les cartes tirées et le nouveau compteur de pitié.</summary>
    public sealed class BoosterOpening
    {
        public IReadOnlyList<CardId> Cards { get; }
        public int Pity { get; }
        public BoosterOpening(IReadOnlyList<CardId> cards, int pity) { Cards = cards; Pity = pity; }
    }

    /// <summary>Règles déterministes de la collection : tirages d'objets et de cartes, boutique du jour, compatibilité des cartes.</summary>
    public static class CollectionRules
    {
        public const string StandardBooster = BoosterDef.StandardId;
        public const int MaxItems = 2;
        public const int CardSlots = 2;

        /// <summary>Poids de rareté d'un objet trouvé en événement : Commun, Rare, Épique, Légendaire.</summary>
        public static readonly IReadOnlyList<int> FoundItemWeights = new[] { 60, 28, 10, 2 };

        /// <summary>Raretés par ordre croissant.</summary>
        public static ItemRarity RollRarity(IRandom rng, IReadOnlyList<int> weights)
        {
            int total = 0;
            foreach (var w in weights) total += System.Math.Max(0, w);
            if (total <= 0) return ItemRarity.Common;
            int roll = rng.Range(0, total);
            for (int i = 0; i < weights.Count; i++)
            {
                roll -= System.Math.Max(0, weights[i]);
                if (roll < 0) return (ItemRarity)System.Math.Min(i, (int)ItemRarity.Legendary);
            }
            return ItemRarity.Common;
        }

        /// <summary>Objet de la rareté tirée (ou de la rareté inférieure la plus proche si le jeu n'en a pas) ; null si le jeu n'a aucun objet.</summary>
        public static ItemDef? RollItem(CollectionDef c, IRandom rng, IReadOnlyList<int>? weights = null)
        {
            if (c.Items.Count == 0) return null;
            var rarity = RollRarity(rng, weights ?? FoundItemWeights);
            for (int r = (int)rarity; r >= 0; r--)
            {
                var pool = c.Items.Where(i => (int)i.Rarity == r).OrderBy(i => i.Id).ToList();
                if (pool.Count > 0) return pool[rng.Range(0, pool.Count)];
            }
            var all = c.Items.OrderBy(i => i.Id).ToList();
            return all[rng.Range(0, all.Count)];
        }

        /// <summary>Objets proposés en boutique ce jour-là : les mêmes pour tous les joueurs, renouvelés chaque jour.</summary>
        public static IReadOnlyList<ItemDef> DailyItemOffers(CollectionDef c, string day, int count)
        {
            var h = new Fnv1a64();
            h.Add(day.Length);
            h.Add(day);
            h.Add(777);
            var rng = new Pcg32(h.Value);
            var pool = c.Items.Where(i => i.Price > 0).OrderBy(i => i.Id).ToList();
            var offers = new List<ItemDef>();
            while (offers.Count < count && pool.Count > 0)
            {
                int i = rng.Range(0, pool.Count);
                offers.Add(pool[i]);
                pool.RemoveAt(i);
            }
            return offers;
        }

        /// <summary>Ouvre un booster : tire ses cartes selon les poids ; la pitié garantit une carte Rare ou mieux au bout de N boosters sans.</summary>
        public static BoosterOpening OpenBooster(CollectionDef c, BoosterDef booster, ulong seed, int pity)
        {
            var rng = new Pcg32(seed);
            var cards = new List<SkillCardDef>();
            for (int i = 0; i < booster.Cards; i++)
            {
                var rarity = RollRarity(rng, booster.Weights);
                cards.Add(PickCard(c, rarity, rng));
            }
            bool hasRare = cards.Any(x => x.Rarity >= ItemRarity.Rare);
            int newPity = hasRare ? 0 : pity + 1;
            if (!hasRare && booster.PityEvery > 0 && newPity >= booster.PityEvery && cards.Count > 0)
            {
                cards[cards.Count - 1] = PickCard(c, ItemRarity.Rare, rng);
                newPity = 0;
            }
            return new BoosterOpening(cards.Select(x => x.Id).ToList(), newPity);
        }

        static SkillCardDef PickCard(CollectionDef c, ItemRarity rarity, IRandom rng)
        {
            for (int r = (int)rarity; r >= 0; r--)
            {
                var pool = c.Cards.Where(x => (int)x.Rarity == r).OrderBy(x => x.Id).ToList();
                if (pool.Count > 0) return pool[rng.Range(0, pool.Count)];
            }
            var all = c.Cards.OrderBy(x => x.Id).ToList();
            return all[rng.Range(0, all.Count)];
        }

        /// <summary>Une carte ne s'équipe que sur un agent de sa catégorie (famille).</summary>
        public static bool CanEquip(SkillCardDef card, MemberDef member) => card.Family == member.Family;

        /// <summary>Graine d'un tirage : joueur, source et compteur, pour qu'un serveur puisse le recalculer.</summary>
        public static ulong Seed(string playerId, string what, int counter)
        {
            var h = new Fnv1a64();
            h.Add(playerId.Length);
            h.Add(playerId);
            h.Add(what.Length);
            h.Add(what);
            h.Add(counter);
            return h.Value;
        }
    }
}
