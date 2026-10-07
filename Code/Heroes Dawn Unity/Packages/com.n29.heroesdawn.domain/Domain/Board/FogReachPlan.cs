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
        /// <summary>Distance totale jusqu'à la case cachée, en pas.</summary>
        public int Distance => FullPath.Count;

        public FogReachPlan(CaseId target, IReadOnlyList<CaseId> fullPath, CaseId stopAt, IReadOnlyList<CaseId> stopPath)
        {
            Target = target; FullPath = fullPath; StopAt = stopAt; StopPath = stopPath;
        }
    }
}
