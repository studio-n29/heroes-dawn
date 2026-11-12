#nullable enable
using N29.HeroesDawn.Presentation.UiToolkit;
using NUnit.Framework;
using UnityEngine;

namespace N29.HeroesDawn.Tests.Presentation
{
    public sealed class HdFitTests
    {
        [TestCase(1920f, 1080f, 1f)]
        [TestCase(2400f, 1080f, 1f)]
        [TestCase(1662f, 1247f, 0.8656f)]
        [TestCase(2133f, 960f, 0.8889f)]
        [TestCase(0f, 0f, 1f)]
        [Description("Vérifie que le facteur d'ajustement vaut 1 pour un panneau d'au moins 1920×1080 et réduit la mise en page dans l'axe le plus court — parce qu'un facteur supérieur à 1 agrandirait les écrans larges et un facteur erroné décalerait le HUD du combat en 4:3 et 20:9")]
        public void Scale_ShrinksOnlyTheShortAxis_NeverEnlarges(float width, float height, float expected)
            => Assert.That(HdFit.Scale(new Vector2(width, height)), Is.EqualTo(expected).Within(0.001f));
    }
}
