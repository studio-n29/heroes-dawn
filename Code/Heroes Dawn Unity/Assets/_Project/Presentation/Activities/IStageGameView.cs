#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Domain.Activities;
using N29.HeroesDawn.Domain.Activities.Games;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Activities
{
    public enum StageKind { Arena }

    /// <summary>Un agent proposé pour l'arène (nom de code, famille et compétences en une ligne).</summary>
    public struct AgentChoice
    {
        public string Name;
        public string Detail;
    }

    /// <summary>Ce qu'il faut pour jouer une activité 3D : le niveau généré, la partie (ou la fabrique d'arène par agent choisi) et les textes.</summary>
    public sealed class StageRequest
    {
        public StageKind Kind { get; set; }
        public string Title { get; set; } = "";
        public string Instructions { get; set; } = "";
        public ActivityLevel Level { get; set; } = null!;
        /// <summary>Arène : un agent à choisir dans l'escouade, puis la partie est créée pour lui (index dans <see cref="Agents"/>).</summary>
        public IReadOnlyList<AgentChoice> Agents { get; set; } = Array.Empty<AgentChoice>();
        public Func<int, ArenaGame>? ArenaFactory { get; set; }
        public ILocalization Loc { get; set; } = null!;
    }

    /// <summary>Fenêtre d'une activité 3D (arène de combat) ; renvoie le résultat de la partie.</summary>
    public interface IStageGameView : IPopupView
    {
        UniTask<ActivityResult> PlayAsync(StageRequest request, CancellationToken ct);
    }
}
