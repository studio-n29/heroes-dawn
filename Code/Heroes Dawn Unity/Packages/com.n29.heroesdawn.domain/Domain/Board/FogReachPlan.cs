using System.Collections.Generic;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Domain.Board
{
    /// <summary>
    /// Case encore sous le brouillard mais à portée des PM en main : elle est surlignée (« ? ») pour montrer où mène le chemin,
    /// mais on ne peut pas encore s'y rendre — le déplacement s'arrête d'abord sur <see cref="StopAt"/> (arrêt forcé
    /// qui ouvre le brouillard), puis le joueur décide de la suite avec ses PM restants.
    /// </summary>
    public sealed class FogReachPlan
    {
        public CaseId Target { get; }
        /// <summary>Chemin complet jusqu'à la case cachée (sans la case de départ), arrêts forcés ignorés.</summary>
        public IReadOnlyList<CaseId> FullPath { get; }
        /// <summary>Premier arrêt forcé du chemin : la vraie destination du déplacement (clé de <see cref="ZoneRun.Destinations"/>).</summary>
        public CaseId StopAt { get; }
        /// <summary>Chemin réellement parcouru jusqu'à <see cref="StopAt"/>.</summary>
        public IReadOnlyList<CaseId> StopPath { get; }
        /// <summary>Coût total en PM jusqu'à la case cachée (somme des chemins).</summary>
        public int Cost { get; }

        public FogReachPlan(CaseId target, IReadOnlyList<CaseId> fullPath, int cost, CaseId stopAt, IReadOnlyList<CaseId> stopPath)
        {
            Target = target; FullPath = fullPath; Cost = cost; StopAt = stopAt; StopPath = stopPath;
        }
    }
}
