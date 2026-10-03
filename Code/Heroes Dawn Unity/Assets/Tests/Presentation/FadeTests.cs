#nullable enable
using N29.HeroesDawn.Presentation.Audio;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    public sealed class FadeTests
    {
        [Test, Description("Vérifie qu'un fondu atteint exactement sa cible à la fin de sa durée puis s'arrête — parce qu'une platine n'est libérée qu'une fois son fondu terminé")]
        public void Fade_ReachesTargetAndStops()
        {
            var f = new Fade(0f);
            f.To(1f, 1f);
            for (int i = 0; i < 9; i++) f.Step(0.1f);
            Assert.That(f.Active, Is.True);
            f.Step(0.2f);
            Assert.That(f.Value, Is.EqualTo(1f));
            Assert.That(f.Active, Is.False);
            Assert.That(f.Step(0.1f), Is.False);
        }

        [Test, Description("Vérifie qu'au milieu d'un fondu enchaîné la somme des puissances des deux platines reste à 1 — parce qu'un fondu linéaire crée un creux audible entre deux musiques")]
        public void Crossfade_KeepsConstantPower()
        {
            var rising = new Fade(0f);
            var falling = new Fade(1f);
            rising.To(1f, 2f);
            falling.To(0f, 2f);
            for (int i = 0; i < 7; i++)
            {
                rising.Step(0.25f);
                falling.Step(0.25f);
                float power = rising.Value * rising.Value + falling.Value * falling.Value;
                Assert.That(power, Is.EqualTo(1f).Within(0.001f));
            }
        }

        [Test, Description("Vérifie qu'une durée nulle saute directement à la cible — parce qu'une coupure franche doit rester possible (StopMusic(0))")]
        public void ZeroDuration_Jumps()
        {
            var f = new Fade(1f);
            f.To(0f, 0f);
            Assert.That(f.Value, Is.EqualTo(0f));
            Assert.That(f.Active, Is.False);
        }

        [Test, Description("Vérifie qu'un fondu interrompu repart de la valeur atteinte — parce qu'un changement d'écran pendant un fondu ne doit pas faire sauter le volume")]
        public void Retarget_StartsFromCurrentValue()
        {
            var f = new Fade(0f);
            f.To(1f, 1f);
            f.Step(0.5f);
            float mid = f.Value;
            f.To(0f, 1f);
            f.Step(0.0001f);
            Assert.That(f.Value, Is.EqualTo(mid).Within(0.01f));
        }
    }
}
