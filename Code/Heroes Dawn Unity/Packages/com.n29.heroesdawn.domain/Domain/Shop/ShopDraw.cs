#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Shop
{
    /// <summary>Tirage de la boutique : rareté pondérée, puis héros uniforme dans la rareté, sans doublon.</summary>
    public static class ShopDraw
    {
        static readonly Rarity[] Order = { Rarity.Agent, Rarity.Elite, Rarity.Legend };

        /// <summary>Recrues non possédées dont la condition d'entrée est remplie, dans l'ordre du contenu.</summary>
        public static List<MemberDef> Eligible(IReadOnlyList<MemberDef> members, Func<MemberId, bool> owned, Func<ZoneId, bool> zoneCompleted)
        {
            var list = new List<MemberDef>();
            foreach (var m in members)
            {
                if (owned(m.Id)) continue;
                if (m.UnlockZone is ZoneId z && !zoneCompleted(z)) continue;
                list.Add(m);
            }
            return list;
        }

        /// <summary>Jusqu'à <paramref name="slots"/> héros ; une rareté sans candidat est ignorée (poids renormalisés).</summary>
        public static List<MemberId> Draw(IRandom rng, IReadOnlyList<MemberDef> eligible, ShopWeights w, int slots = 3)
        {
            var candidates = new List<MemberDef>(eligible);
            var result = new List<MemberId>();
            while (result.Count < slots && candidates.Count > 0)
            {
                int total = 0;
                foreach (var r in Order) if (Has(candidates, r)) total += Math.Max(0, w.For(r));
                Rarity chosen = Rarity.Agent;
                bool found = false;
                if (total > 0)
                {
                    int x = rng.Range(0, total);
                    foreach (var r in Order)
                    {
                        if (!Has(candidates, r)) continue;
                        int weight = Math.Max(0, w.For(r));
                        if (x < weight) { chosen = r; found = true; break; }
                        x -= weight;
                    }
                }
                // Tous les poids restants à 0 : on prend la première rareté présente.
                if (!found) foreach (var r in Order) if (Has(candidates, r)) { chosen = r; break; }

                var pool = candidates.FindAll(m => m.Rarity == chosen);
                var pick = pool[rng.Range(0, pool.Count)];
                result.Add(pick.Id);
                candidates.Remove(pick);
            }
            return result;
        }

        static bool Has(List<MemberDef> candidates, Rarity r)
        {
            foreach (var m in candidates) if (m.Rarity == r) return true;
            return false;
        }
    }
}
