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

        /// <summary>Caméra : recentre sur une case (zoom conservé).</summary>
        void CenterOn(CaseId id, bool animate);

        /// <summary>Caméra : dézoome pour montrer toutes les cases données (chemin à prévisualiser), en laissant la place à la carte d'information.</summary>
        void FitTo(IReadOnlyList<CaseId> ids, bool animate);

        /// <summary>Navigation case à case : la case sélectionnée au clavier / à la manette (déplacée le long des liens).</summary>
        CaseId? Focused { get; }

        /// <summary>Déplace le pion d'un trait le long des tracés des chemins, sans s'arrêter aux cases traversées.</summary>
        UniTask MovePawnAsync(IReadOnlyList<CaseId> path, CancellationToken ct);
    }
}
