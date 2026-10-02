using System.Collections.Generic;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Domain.Board
{
    /// <summary>GDD §5 — brouillard : rayon de visibilité, refermé en quittant la zone.</summary>
    public sealed class FogOfWar
    {
        readonly BoardGraph _g;
        readonly int _radius;
        readonly HashSet<CaseId> _revealed = new HashSet<CaseId>();

        public IReadOnlyCollection<CaseId> Revealed => _revealed;

        public FogOfWar(BoardGraph g, int radius) { _g = g; _radius = radius; }

        public bool IsRevealed(CaseId id) => _revealed.Contains(id);

        /// <summary>Vrai si se tenir sur cette case révélerait au moins une case cachée.</summary>
        public bool WouldReveal(CaseId at)
        {
            foreach (var id in _g.WithinRadius(at, _radius)) if (!_revealed.Contains(id)) return true;
            return false;
        }

        /// <summary>Révèle autour de la case ; renvoie les nouvelles cases, triées.</summary>
        public IReadOnlyList<CaseId> RevealAround(CaseId at)
        {
            var news = new List<CaseId>();
            foreach (var id in _g.WithinRadius(at, _radius)) if (_revealed.Add(id)) news.Add(id);
            return news;
        }

        public void Close() => _revealed.Clear();

        public void Restore(IEnumerable<CaseId> ids)
        {
            _revealed.Clear();
            foreach (var id in ids) _revealed.Add(id);
        }
    }
}
