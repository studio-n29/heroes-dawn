using N29.HeroesDawn.Application.Features.Meta;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    /// <summary>Formulaire de retour de playtest proposé après la zone 1.1.</summary>
    public sealed class PlaytestFeedbackTests
    {
        TestApp _app = null!;

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.SignInDirect("p1");
        }

        [Test, Description("Vérifie que le formulaire est proposé à la première validation de la zone 1.1 seulement — parce que le retour est demandé une fois, au bon moment")]
        public void Asks_OnFirstClearOfZone11_Once()
        {
            var f = new PlaytestFeedback(_app.MetaCtx).WithUrl("https://exemple.test/form");
            Assert.That(f.ShouldAsk("zone_ny_1_1", true), Is.True);
            Assert.That(f.ShouldAsk("zone_ny_1_1", false), Is.False, "rejouer la zone ne redemande pas");
            Assert.That(f.ShouldAsk("zone_ny_1_2", true), Is.False, "une autre zone non plus");
            f.MarkAsked();
            Assert.That(f.ShouldAsk("zone_ny_1_1", true), Is.False, "déjà proposé");
            Assert.That(_app.Session.Meta.playtestFormAsked, Is.True, "enregistré dans le profil");
        }

        [Test, Description("Vérifie que sans adresse de formulaire rien n'est proposé — parce qu'on ne doit pas montrer un lien vide au joueur")]
        public void WithoutUrl_NothingIsAsked()
        {
            var f = new PlaytestFeedback(_app.MetaCtx).WithUrl("");
            Assert.That(f.Enabled, Is.False);
            Assert.That(f.ShouldAsk("zone_ny_1_1", true), Is.False);
        }
    }
}
