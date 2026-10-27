#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Domain.Activities;
using N29.HeroesDawn.Presentation.Mvp;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Activities
{
    /// <summary>Un mini-jeu 2D à faire jouer dans la fenêtre commune : textes, fond et pilote.</summary>
    public sealed class MiniGame2DRequest
    {
        public string Title { get; set; } = "";
        public string Instructions { get; set; } = "";
        public string Start { get; set; } = "";
        public string Abandon { get; set; } = "";
        public string TimeFormat { get; set; } = "{0} s";
        public string WonText { get; set; } = "";
        public string LostText { get; set; } = "";
        public IMiniGameDriver Driver { get; set; } = null!;
        public Sprite? Background { get; set; }
        public Color BackgroundTint { get; set; } = Color.white;
    }

    /// <summary>Fenêtre commune des mini-jeux 2D ; rend le résultat quand la partie est finie, abandonnée ou annulée.</summary>
    public interface IMiniGame2DPopupView : IPopupView
    {
        UniTask<ActivityResult> PlayAsync(MiniGame2DRequest request, CancellationToken ct);
    }
}
