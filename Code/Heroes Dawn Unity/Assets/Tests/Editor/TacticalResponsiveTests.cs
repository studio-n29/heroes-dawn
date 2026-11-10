using N29.HeroesDawn.Presentation;
using N29.HeroesDawn.Presentation.Common;
using NUnit.Framework;
using UnityEngine;

namespace N29.HeroesDawn.Tests.Editor
{
    public sealed class TacticalResponsiveTests
    {
        [TestCase(1920, 1080, 0)]
        [TestCase(2160, 972, 90)]
        [TestCase(2048, 1536, 0)]
        [TestCase(2560, 1080, 120)]
        [TestCase(1280, 720, 40)]
        [Description("Le viewport sûr garde toute la composition paysage malgré les encoches, le ratio et la résolution")]
        public void SafeViewport_RetainsCompleteLandscapeComposition(int width, int height, int inset)
        {
            var safe = new Rect(inset, 20, width - 2 * inset, height - 40);
            var reference = N29.HeroesDawn.Presentation.SafeArea.ReferenceFor(safe, width, height);
            float scale = Mathf.Min(width / reference.x, height / reference.y);
            Assert.That(safe.width / scale, Is.GreaterThanOrEqualTo(1919.9f));
            Assert.That(safe.height / scale, Is.GreaterThanOrEqualTo(1079.9f));
        }
    }
}

