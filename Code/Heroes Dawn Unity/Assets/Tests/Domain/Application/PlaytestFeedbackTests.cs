using N29.HeroesDawn.Application.Features.Meta;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    /// <summary>Formulaire de retour de playtest proposé après la zone 1.1.</summary>
    public sealed class PlaytestFeedbackTests
    {
        TestApp _app = null!;
        PlaytestFeedback _f = null!;

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.SignInDirect("p1");
            _f = new PlaytestFeedback(_app.MetaCtx);
        }

        [Test, Description("Vérifie que le formulaire est proposé à la première validation de la zone 1.1 seulement — parce que le retour est demandé une fois, au bon moment")]
        public void Asks_OnFirstClearOfZone11_Once()
        {
            Assert.That(_f.ShouldAsk("zone_ny_1_1", true), Is.True);
            Assert.That(_f.ShouldAsk("zone_ny_1_1", false), Is.False, "rejouer la zone ne redemande pas");
            Assert.That(_f.ShouldAsk("zone_ny_1_2", true), Is.False, "une autre zone non plus");
            _f.MarkAsked();
            Assert.That(_f.ShouldAsk("zone_ny_1_1", true), Is.False, "déjà proposé");
            Assert.That(_app.Session.Meta.playtestFormAsked, Is.True, "enregistré dans le profil");
        }

        [Test, Description("Vérifie que le formulaire n'est valide qu'avec quatre notes de 1 à 5 — parce qu'un retour partiel fausserait l'analyse")]
        public void Answers_NeedFourRatingsBetweenOneAndFive()
        {
            Assert.That(PlaytestFeedback.IsComplete(new PlaytestAnswers(new[] { 5, 4, 3, 2 }, "")), Is.True);
            Assert.That(PlaytestFeedback.IsComplete(new PlaytestAnswers(new[] { 5, 4, 3, 0 }, "")), Is.False);
            Assert.That(PlaytestFeedback.IsComplete(new PlaytestAnswers(new[] { 5, 4, 3 }, "")), Is.False);
            Assert.That(PlaytestFeedback.IsComplete(new PlaytestAnswers(new[] { 5, 4, 3, 6 }, "")), Is.False);
        }

        [Test, Description("Vérifie que l'envoi produit un rapport avec la version, les notes et le commentaire, conservé dans le profil — parce que c'est ce que le joueur transmet à l'équipe")]
        public void Submit_BuildsAndKeepsTheReport()
        {
            var report = _f.Submit(new PlaytestAnswers(new[] { 5, 3, 4, 2 }, " trop long "), "0.12.0", new[] { "Fun", "Clarté", "Difficulté", "Tutoriel" });
            Assert.That(report, Does.Contain("0.12.0"));
            Assert.That(report, Does.Contain("Fun : 5/5"));
            Assert.That(report, Does.Contain("Tutoriel : 2/5"));
            Assert.That(report, Does.Contain("Commentaire : trop long"));
            Assert.That(_app.Session.Meta.playtestReports, Has.Count.EqualTo(1));
        }
    }
}
