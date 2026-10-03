#nullable enable
using N29.HeroesDawn.Presentation;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Screens;
using UnityEngine;

namespace N29.HeroesDawn.Composition
{
    /// <summary>Scope de la scène Hub : crée et affiche le HubScreen.</summary>
    public sealed class HubLifetimeScope : SceneScreenScope
    {
        protected override UiScreen? CreateScreen(RectTransform root, UiContext ctx) => UiScreen.Create<HubScreen>(root, "Hub", ctx);
    }
}
