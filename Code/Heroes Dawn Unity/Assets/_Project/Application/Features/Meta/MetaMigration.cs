#nullable enable
using System;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Application.Features.Meta
{
    /// <summary>
    /// Migration d'une sauvegarde 0.15.0 vers l'Atelier de la 1.0.0 : scraps → Composants Commun, Noyaux → catalyseurs, anciens objets fabriqués → étoiles,
    /// compétences de création supprimées, recherches du labo supprimées et remboursées. Idempotente : les anciennes listes sont vidées.
    /// </summary>
    public static class MetaMigration
    {
        /// <summary>Prix en Crédits du niveau <c>level + 1</c> d'une recherche du labo supprimé : 500 × (niveau + 1).</summary>
        public static int ResearchLevelCost(int level) => 500 * (level + 1);

        /// <summary>Les cosmétiques de tournoi rangés dans l'arène passent dans la liste unique du profil (sans doublon) ; idempotent.</summary>
        public static void CosmeticsToProfile(MetaSave m)
        {
            foreach (var id in m.arena.cosmetics) CosmeticOps.Unlock(m, id);
            m.arena.cosmetics.Clear();
        }

        /// <param name="costOfLevel">Prix du passage du niveau <c>i</c> au suivant (par défaut <see cref="ResearchLevelCost"/>).</param>
        /// <param name="refundedCredits">Crédits à rendre au portefeuille (0 si rien à migrer).</param>
        public static void ToComponents(MetaSave m, Func<int, int>? costOfLevel, out int refundedCredits)
        {
            costOfLevel ??= ResearchLevelCost;

            // Scraps → Composants Commun ; Noyaux → catalyseurs en rotation Métal, Énergie, Biologique, Cristal.
            foreach (var c in m.scraps)
            {
                if (c.value <= 0) continue;
                if (c.id == "Core")
                {
                    for (int i = 0; i < c.value; i++) AddTo(m.catalysts, ((CatalystKind)(i % ComponentKinds.CatalystCount)).ToString(), 1);
                }
                else if (Enum.TryParse<ComponentKind>(c.id, out var kind))
                    AddTo(m.components, kind + "." + ComponentGrade.Common, c.value);
            }
            m.scraps.Clear();

            // Compétences de création : supprimées, sans remboursement.
            m.craftXp.Clear();

            // Recherches du labo : Crédits dépensés rendus, bonus supprimés, recherche en cours annulée sans remboursement.
            int refund = 0;
            foreach (var c in m.labLevels)
                for (int i = 0; i < c.value; i++) refund += costOfLevel(i);
            m.labLevels.Clear();
            m.labActive = "";
            m.labEndUtcTicks = 0;
            refundedCredits = refund;

            // Anciens objets fabriqués : ancienne qualité + 1 en étoiles, statistiques inchangées.
            foreach (var item in m.crafted)
                if (item.stars <= 0) item.stars = Math.Max(CraftStars.Min, Math.Min(CraftStars.Max, item.quality + 1));
        }

        static void AddTo(System.Collections.Generic.List<CounterSave> list, string id, int count)
        {
            foreach (var c in list) if (c.id == id) { c.value += count; return; }
            list.Add(new CounterSave { id = id, value = count });
        }
    }
}
