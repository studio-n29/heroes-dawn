using System.Collections.Generic;
using System.Linq;
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

        /// <summary>Joue l'interface de base puis le niveau tutoriel, jusqu'à la fermeture des résultats.</summary>
        void ThroughLevel()
        {
            T.Acknowledge();
            T.Signal(TutorialSignal.MapOpened);
            T.Signal(TutorialSignal.BoardOpened);
            T.Acknowledge();
            T.Signal(TutorialSignal.Rolled);
            T.Signal(TutorialSignal.Moved);
            T.Acknowledge();
            T.Signal(TutorialSignal.SkillUsed);
            T.Signal(TutorialSignal.ResultsClosed);
        }

        [Test, Description("Vérifie que la partie guidée suit l'ordre voulu : interface de base (lancement de partie), niveau tutoriel, puis progression — parce que le joueur doit d'abord jouer avant de gérer son équipe")]
        public void Steps_FollowBasicsThenLevelThenProgression()
        {
            Assert.That(T.Active);
            Assert.That(T.Current, Is.EqualTo(TutorialStep.Welcome));
            Assert.That(T.Acknowledge());
            Assert.That(T.Current, Is.EqualTo(TutorialStep.StartMission));
            T.Signal(TutorialSignal.MapOpened);
            Assert.That(T.Current, Is.EqualTo(TutorialStep.Deploy));
            T.Signal(TutorialSignal.BoardOpened);
            Assert.That(T.Current, Is.EqualTo(TutorialStep.Objectives));
            Assert.That(T.Acknowledge());
            T.Signal(TutorialSignal.Rolled);
            T.Signal(TutorialSignal.Moved);
            Assert.That(T.Current, Is.EqualTo(TutorialStep.CombatAtb));
            Assert.That(T.Acknowledge());
            T.Signal(TutorialSignal.SkillUsed);
            Assert.That(T.Current, Is.EqualTo(TutorialStep.Results));
            T.Signal(TutorialSignal.ResultsClosed);
            Assert.That(T.Current, Is.EqualTo(TutorialStep.OpenTeam));
            T.Signal(TutorialSignal.TeamOpened);
            Assert.That(T.Acknowledge());
            var hub = new[] { (TutorialStep.OpenSheet, TutorialSignal.SheetOpened), (TutorialStep.Train, TutorialSignal.Trained), (TutorialStep.EquipSlot, TutorialSignal.SlotOpened),
                              (TutorialStep.EquipPick, TutorialSignal.ItemEquipped) };
            foreach (var (step, signal) in hub)
            {
                Assert.That(T.Current, Is.EqualTo(step));
                T.Signal(signal);
            }
            Assert.That(T.Active, Is.False);
            Assert.That(T.Current, Is.EqualTo(TutorialStep.Finished));
        }

        [Test, Description("Vérifie que le niveau tutoriel est signalé comme tel de « Mission » aux résultats seulement — parce que la carte doit lancer ce niveau court et pas la zone 1.1 pendant cette phase")]
        public void LevelPhase_CoversMissionToResults()
        {
            Assert.That(T.InLevelPhase, Is.False, "Bienvenue");
            T.Acknowledge();
            Assert.That(T.InLevelPhase, Is.True, "Mission");
            ThroughLevelFrom(TutorialStep.StartMission);
            Assert.That(T.InLevelPhase, Is.False, "progression");
        }

        void ThroughLevelFrom(TutorialStep _)
        {
            T.Signal(TutorialSignal.MapOpened);
            T.Signal(TutorialSignal.BoardOpened);
            T.Acknowledge();
            T.Signal(TutorialSignal.Rolled);
            T.Signal(TutorialSignal.Moved);
            T.Acknowledge();
            T.Signal(TutorialSignal.SkillUsed);
            T.Signal(TutorialSignal.ResultsClosed);
        }

        [Test, Description("Vérifie que finir le niveau tutoriel offre un agent gratuit — parce que la fin du niveau est la récompense promise au joueur")]
        public void FinishingTheLevel_GivesAFreeAgent()
        {
            int before = _app.Session.Roster.Count;
            ThroughLevel();
            Assert.That(_app.Session.Roster.Count, Is.EqualTo(before + 1));
            
        }

        [Test, Description("Vérifie que l'agent gratuit n'est donné qu'une fois — parce que fermer deux fois les résultats ne doit pas remplir l'équipe")]
        public void FreeAgent_IsGrantedOnce()
        {
            ThroughLevel();
            int count = _app.Session.Roster.Count;
            T.Signal(TutorialSignal.ResultsClosed);
            Assert.That(_app.Session.Roster.Count, Is.EqualTo(count));
        }

        [Test, Description("Vérifie que passer le tutoriel avant la fin du niveau donne quand même l'agent gratuit — parce que le joueur ne doit pas perdre la récompense en sautant")]
        public void Skip_BeforeTheLevelEnds_StillGivesTheAgent()
        {
            int before = _app.Session.Roster.Count;
            T.Skip();
            Assert.That(_app.Session.Roster.Count, Is.EqualTo(before + 1));
        }

        [Test, Description("Vérifie qu'un geste du hub fait trop tôt n'avance rien — parce que les étapes de progression s'enchaînent dans l'ordre")]
        public void HubGestures_OutOfOrder_DoNothing()
        {
            ThroughLevel();
            T.Signal(TutorialSignal.TeamOpened);
            T.Acknowledge();
            Assert.That(T.Current, Is.EqualTo(TutorialStep.OpenSheet));
            T.Signal(TutorialSignal.ItemEquipped);
            T.Signal(TutorialSignal.Trained);
            Assert.That(T.Current, Is.EqualTo(TutorialStep.OpenSheet));
            T.Signal(TutorialSignal.SheetOpened);
            Assert.That(T.Current, Is.EqualTo(TutorialStep.Train));
        }

        [Test, Description("Vérifie que la partie guidée compte 15 étapes — parce que la barre de progression « n / 15 » et la sauvegarde en dépendent")]
        public void StepCount_IsFifteen()
        {
            Assert.That(TutorialGuide.StepCount, Is.EqualTo(15));
        }

        [Test, Description("Vérifie qu'un geste fait hors de l'étape attendue n'avance rien — parce que les étapes sont bloquantes et dans l'ordre")]
        public void WrongSignal_DoesNothing()
        {
            T.Signal(TutorialSignal.Rolled);
            T.Signal(TutorialSignal.SkillUsed);
            Assert.That(T.Current, Is.EqualTo(TutorialStep.Welcome));
            Assert.That(T.Acknowledge());
            Assert.That(T.Acknowledge(), Is.False, "StartMission n'est pas une étape « Compris »");
        }

        [Test, Description("Vérifie que l'étape atteinte survit à la sauvegarde — parce que quitter puis revenir reprend à l'étape en cours (spec §6)")]
        public void Progress_IsSavedInProfile()
        {
            T.Acknowledge();
            T.Signal(TutorialSignal.MapOpened);
            var other = new TestApp();
            other.Session.ApplyProfile(_app.Session.ToProfile(_app.Time.UtcNow));
            Assert.That(other.Tutorial.Current, Is.EqualTo(TutorialStep.Deploy));
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
            int before = _app.Session.Roster.Count;
            T.BeginIfNeeded();
            Assert.That(T.Active, Is.False);
            Assert.That(_app.Session.Roster.Count, Is.EqualTo(before), "pas d'agent gratuit pour un joueur confirmé");
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
            T.Acknowledge(); T.Signal(TutorialSignal.MapOpened);
            T.Signal(TutorialSignal.BoardOpened); T.Acknowledge(); T.Signal(TutorialSignal.Rolled); T.Signal(TutorialSignal.Moved);
            Assert.That(T.Current, Is.EqualTo(TutorialStep.CombatAtb));
            Assert.That(T.PausesCombat, Is.True);
            T.Presenting = false;
            Assert.That(T.PausesCombat, Is.False);
        }

        [Test, Description("Vérifie que fermer les résultats mène à la progression même si des étapes de combat ont été sautées — parce que le joueur ne doit jamais rester coincé dans le niveau")]
        public void ResultsClosed_MovesOnEvenIfStepsSkipped()
        {
            T.Acknowledge(); T.Signal(TutorialSignal.MapOpened);
            T.Signal(TutorialSignal.BoardOpened); T.Acknowledge(); T.Signal(TutorialSignal.Rolled); T.Signal(TutorialSignal.Moved);
            T.Signal(TutorialSignal.ResultsClosed);
            Assert.That(T.Current, Is.EqualTo(TutorialStep.OpenTeam));
        }
    }
}
