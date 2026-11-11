#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Shop
{
    /// <summary>Tirage de la boutique : rang pondéré, puis héros uniforme dans le rang, sans doublon.</summary>
    public static class ShopDraw
    {
        static readonly int[] Order = { 1, 2, 3, 4, 5 };

        /// <summary>Recrues non possédées dont la condition d'entrée est remplie, dans l'ordre du contenu.</summary>
        public static List<MemberDef> Eligible(IReadOnlyList<MemberDef> members, Func<MemberId, bool> owned, Func<ZoneId, bool> zoneCompleted)
        {
            var list = new List<MemberDef>();
            foreach (var m in members)
            {
                if (owned(m.Id) || !m.InShop) continue;
                if (m.UnlockZone is ZoneId z && !zoneCompleted(z)) continue;
                list.Add(m);
            }
            return list;
        }

        /// <summary>Jusqu'à <paramref name="slots"/> héros ; un rang sans candidat est ignorée (poids renormalisés).</summary>
        public static List<MemberId> Draw(IRandom rng, IReadOnlyList<MemberDef> eligible, ShopWeights w, int slots = 3)
        {
            var candidates = new List<MemberDef>(eligible);
            var result = new List<MemberId>();
            while (result.Count < slots && candidates.Count > 0)
            {
                int total = 0;
                foreach (var r in Order) if (Has(candidates, r)) total += Math.Max(0, w.For(r));
                int chosen = 1;
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
                // Tous les poids restants à 0 : on prend la premier rang présent.
                if (!found) foreach (var r in Order) if (Has(candidates, r)) { chosen = r; break; }

                var pool = candidates.FindAll(m => m.Rank == chosen);
                var pick = pool[rng.Range(0, pool.Count)];
                result.Add(pick.Id);
                candidates.Remove(pick);
            }
            return result;
        }


        /// <summary>Tirage des lots de Composants du jour : type uniforme ; les derniers lots sont Raffinés, les autres Communs (quantité et prix fixés par <see cref="ShopRules"/>).</summary>
        public static System.Collections.Generic.List<ComponentLot> DrawComponentLots(IRandom rng, int count)
        {
            var lots = new System.Collections.Generic.List<ComponentLot>();
            for (int i = 0; i < count; i++)
                {
                bool refined = i >= count - ShopRules.RefinedLotsPerDay;
                lots.Add(new ComponentLot((ComponentKind)rng.Range(0, ComponentKinds.Count), refined ? ComponentGrade.Refined : ComponentGrade.Common,
                    refined ? ShopRules.RefinedLotCount : ShopRules.CommonLotCount, refined ? ShopRules.RefinedLotPrice : ShopRules.CommonLotPrice));
            }
            return lots;
        }

        static bool Has(List<MemberDef> candidates, int r)
        {
            foreach (var m in candidates) if (m.Rank == r) return true;
            return false;
        }
    }
}
