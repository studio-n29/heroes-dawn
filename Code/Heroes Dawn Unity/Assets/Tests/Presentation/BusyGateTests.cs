using N29.HeroesDawn.Presentation.Mvp;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    public sealed class BusyGateTests
    {
        [Test, Description("Vérifie qu'une seconde entrée est refusée tant que la première n'est pas sortie — parce que c'est la garde anti-double clic commune à tous les écrans (spec §6)")]
        public void TryEnter_TwiceWithoutExit_SecondFails()
        {
            var g = new BusyGate();
            Assert.That(g.TryEnter(), Is.True);
            Assert.That(g.TryEnter(), Is.False);
            Assert.That(g.IsBusy, Is.True);
        }

        [Test, Description("Vérifie qu'après la sortie on peut entrer à nouveau — parce qu'un échec doit laisser le joueur réessayer")]
        public void Exit_AllowsReentry()
        {
            var g = new BusyGate();
            g.TryEnter();
            g.Exit();
            Assert.That(g.IsBusy, Is.False);
            Assert.That(g.TryEnter(), Is.True);
        }
    }
}
