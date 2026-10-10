#nullable enable
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Definitions
{
    /// <summary>Chemin entre deux cases : coût en PM (1 par défaut) et courbe de Bézier optionnelle (vide : courbe organique générée).</summary>
    public sealed class LinkDef
    {
        public CaseId A { get; }
        public CaseId B { get; }
        /// <summary>PM dépensés pour emprunter ce chemin (au moins 1).</summary>
        public int Cost { get; }
        /// <summary>Points de contrôle de la courbe de A vers B ; vide : courbe générée par <see cref="Domain.Maps.LinkCurves"/>.</summary>
        public LinkCurve? Curve { get; }

        public LinkDef(CaseId a, CaseId b, int cost = 1, LinkCurve? curve = null)
        {
            A = a; B = b; Cost = cost < 1 ? 1 : cost; Curve = curve;
        }
    }

    /// <summary>Points de contrôle d'une courbe de Bézier cubique, en unités de grille (de A vers B).</summary>
    public readonly struct LinkCurve
    {
        public float C1X { get; }
        public float C1Y { get; }
        public float C2X { get; }
        public float C2Y { get; }
        public LinkCurve(float c1x, float c1y, float c2x, float c2y) { C1X = c1x; C1Y = c1y; C2X = c2x; C2Y = c2y; }
    }
}
