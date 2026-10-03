#nullable enable
using N29.HeroesDawn.Presentation;
using N29.HeroesDawn.Presentation.Board;
using N29.HeroesDawn.Presentation.Screens;
using UnityEngine;

namespace N29.HeroesDawn.Composition
{
    /// <summary>Scope de la scène Board : crée et affiche le BoardScreen.</summary>
    public sealed class BoardLifetimeScope : SceneScreenScope
    {
        protected override UiScreen CreateScreen(RectTransform root, UiContext ctx) => UiScreen.Create<BoardScreen>(root, "Board", ctx);
    }
}
