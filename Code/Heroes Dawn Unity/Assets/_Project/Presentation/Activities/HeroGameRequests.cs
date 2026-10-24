#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Domain.Activities;
using N29.HeroesDawn.Domain.Activities.Games;
using N29.HeroesDawn.Presentation.Mvp;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Activities
{
    /// <summary>Quel mini-jeu 2D de héros la fenêtre fait jouer.</summary>
    public enum HeroGameKind { Deflect, RooftopRun }

    /// <summary>Un mini-jeu 2D de héros : la partie (rempart ou course des toits), les textes, l'illustration du héros.</summary>
    public sealed class HeroGameRequest
    {
        public HeroGameKind Kind { get; set; }
        public string Title { get; set; } = "";
        public string Instructions { get; set; } = "";
        public string Start { get; set; } = "";
        public string Abandon { get; set; } = "";
        /// <summary>« Interceptés 3 / 14 », « Civils sauvés 2 / 10 » (score, objectif).</summary>
        public string ScoreFormat { get; set; } = "{0} / {1}";
        public string TimeFormat { get; set; } = "{0} s";
        public string WonText { get; set; } = "";
        public string LostText { get; set; } = "";
        public Sprite? HeroArt { get; set; }
        public DeflectGame? Deflect { get; set; }
        public RooftopRunGame? Run { get; set; }
    }

    /// <summary>Fenêtre des mini-jeux 2D de héros ; rend le résultat (réussite et score) quand la partie est finie ou abandonnée.</summary>
    public interface IHeroGamePopupView : IPopupView
    {
        UniTask<ActivityResult> PlayAsync(HeroGameRequest request, CancellationToken ct);
    }
}
