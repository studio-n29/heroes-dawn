#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Presentation.Board
{
    /// <summary>Carte du plateau : cases, liens, pion. Ne calcule jamais une distance ni une règle.</summary>
    public interface IBoardMapView
    {
        event Action<CaseId>? NodeTapped;
        void Build(BoardMapData data);
        void Refresh(IReadOnlyList<BoardNodeState> nodes, IReadOnlyList<BoardLinkState> links, CaseId pawnAt, bool movePawn);

        /// <summary>Déplace le pion case par case (petits bonds).</summary>
        UniTask MovePawnAsync(IReadOnlyList<CaseId> path, CancellationToken ct);
    }
}
