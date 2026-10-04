using System.Collections.Generic;
using N29.HeroesDawn.Application.Features.Tutorial;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    public sealed class TutorialGuideTests
    {
        TestApp _app = null!;
        TutorialGuide T => _app.Tutorial;

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.SignInDirect("p1");
        }

        [Test, Description("Vérifie que la partie guidée commence à l'étape « Bienvenue » et suit l'ordre des maquettes (QG, Équipe, Mission, Carte, Objectifs, Dé, Déplacement, Combat, Compétence, Résultats) — parce que chaque étape apprend une notion à son tour (spec §6)")]
        public void Steps_FollowTheDocumentedOrder()
        {
            Assert.That(T.Active);
            Assert.That(T.Current, Is.EqualTo(TutorialStep.Welcome));
            Assert.That(T.Acknowledge());
            Assert.That(T.Current, Is.EqualTo(TutorialStep.OpenTeam));
            T.Signal(TutorialSignal.TeamOpened);
            Assert.That(T.Acknowledge());
            T.Signal(TutorialSignal.MapOpened);
            T.Signal(TutorialSignal.BoardOpened);
            Assert.That(T.Acknowledge());
            T.Signal(TutorialSignal.Rolled);
            T.Signal(TutorialSignal.Moved);
            Assert.That(T.Current, Is.EqualTo(TutorialStep.CombatAtb));
            Assert.That(T.Acknowledge());
            T.Signal(TutorialSignal.SkillUsed);
            Assert.That(T.Current, Is.EqualTo(TutorialStep.Results));
            T.Signal(TutorialSignal.ResultsClosed);
            Assert.That(T.Active, Is.False);
            Assert.That(T.Current, Is.EqualTo(TutorialStep.Finished));
        }

        [Test, Description("Vérifie qu'un geste fait hors de l'étape attendue n'avance rien — parce que les étapes sont bloquantes et dans l'ordre")]
        public void WrongSignal_DoesNothing()
        {
            T.Signal(TutorialSignal.Rolled);
            T.Signal(TutorialSignal.SkillUsed);
            Assert.That(T.Current, Is.EqualTo(TutorialStep.Welcome));
            Assert.That(T.Acknowledge());
            Assert.That(T.Acknowledge(), Is.False, "OpenTeam n'est pas une étape « Compris »");
        }

        [Test, Description("Vérifie que l'étape atteinte survit à la sauvegarde — parce que quitter puis revenir reprend à l'étape en cours (spec §6)")]
        public void Progress_IsSavedInProfile()
        {
            T.Acknowledge();
            T.Signal(TutorialSignal.TeamOpened);
            var other = new TestApp();
            other.Session.ApplyProfile(_app.Session.ToProfile(_app.Time.UtcNow));
            Assert.That(other.Tutorial.Current, Is.EqualTo(TutorialStep.Squad));
        }

        [Test, Description("Vérifie que « Passer » termine le tutoriel tout de suite et le marque passé — parce que le joueur expérimenté ne doit pas être bloqué")]
        public void Skip_FinishesImmediately()
        {
            T.Skip();
            Assert.That(T.Active, Is.False);
            Assert.That(T.Skipped, Is.True);
            Assert.That(T.Acknowledge(), Is.False);
        }

        [Test, Description("Vérifie que revoir le tutoriel le relance depuis le début sans oublier les astuces déjà vues — parce que Paramètres ▸ Support doit permettre de le refaire")]
        public void Restart_StartsOver_KeepsSeenHints()
        {
            T.Skip();
            Assert.That(T.Hint("combo"), Is.True);
            T.Restart();
            Assert.That(T.Active);
            Assert.That(T.Current, Is.EqualTo(TutorialStep.Welcome));
            T.Skip();
            Assert.That(T.Hint("combo"), Is.False, "déjà vue");
        }

        [Test, Description("Vérifie qu'un joueur qui a déjà validé une zone n'est pas renvoyé au tutoriel — parce qu'une sauvegarde d'avant le tutoriel est celle d'un joueur qui connaît le jeu")]
        public void ExistingPlayer_SkipsTutorial()
        {
            _app.Session.CompletedZones.Add("zone_ny_1_1");
            T.BeginIfNeeded();
            Assert.That(T.Active, Is.False);
        }

        [Test, Description("Vérifie que les astuces n'apparaissent qu'après la partie guidée, une seule fois chacune — parce qu'elles ne doivent pas se superposer aux étapes bloquantes")]
        public void Hints_OnlyAfterTutorial_OncePerHint()
        {
            var raised = new List<string>();
            T.HintRaised += raised.Add;
            Assert.That(T.Hint("combo"), Is.False, "tutoriel en cours");
            T.Skip();
            Assert.That(T.Hint("combo"), Is.True);
            Assert.That(T.Hint("combo"), Is.False);
            Assert.That(T.Hint("auto"), Is.True);
            Assert.That(raised, Is.EqualTo(new[] { "combo", "auto" }));
        }

        [Test, Description("Vérifie que seules les deux étapes de combat figent le combat, et seulement quand une bulle est affichée — parce que « combat figé pendant une bulle » est la règle du tutoriel (spec §6)")]
        public void OnlyCombatSteps_PauseCombat_WhilePresenting()
        {
            T.Presenting = true;
            Assert.That(T.PausesCombat, Is.False);
            T.Acknowledge(); T.Signal(TutorialSignal.TeamOpened); T.Acknowledge(); T.Signal(TutorialSignal.MapOpened);
            T.Signal(TutorialSignal.BoardOpened); T.Acknowledge(); T.Signal(TutorialSignal.Rolled); T.Signal(TutorialSignal.Moved);
            Assert.That(T.Current, Is.EqualTo(TutorialStep.CombatAtb));
            Assert.That(T.PausesCombat, Is.True);
            T.Presenting = false;
            Assert.That(T.PausesCombat, Is.False);
        }

        [Test, Description("Vérifie que fermer les résultats termine la partie guidée même si des étapes de combat ont été sautées — parce que le joueur ne doit jamais rester coincé dans le tutoriel")]
        public void ResultsClosed_FinishesEvenIfStepsSkipped()
        {
            T.Acknowledge(); T.Signal(TutorialSignal.TeamOpened); T.Acknowledge(); T.Signal(TutorialSignal.MapOpened);
            T.Signal(TutorialSignal.BoardOpened); T.Acknowledge(); T.Signal(TutorialSignal.Rolled); T.Signal(TutorialSignal.Moved);
            T.Signal(TutorialSignal.ResultsClosed);
            Assert.That(T.Active, Is.False);
        }
    }
}
