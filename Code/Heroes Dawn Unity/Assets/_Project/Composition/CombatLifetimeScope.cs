#nullable enable
using N29.HeroesDawn.Presentation;
using N29.HeroesDawn.Presentation.Combat;
using N29.HeroesDawn.Presentation.Screens;
using UnityEngine;

namespace N29.HeroesDawn.Composition
{
    /// <summary>Scope de la scène Combat : crée et affiche le CombatScreen.</summary>
    public sealed class CombatLifetimeScope : SceneScreenScope
    {
        protected override UiScreen CreateScreen(RectTransform root, UiContext ctx) => UiScreen.Create<CombatScreen>(root, "Combat", ctx);
    }
}
