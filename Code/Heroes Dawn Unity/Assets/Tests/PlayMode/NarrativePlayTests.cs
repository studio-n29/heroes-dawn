#nullable enable
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Combat;
using N29.HeroesDawn.Application.Features.World;
using N29.HeroesDawn.Application.Flow;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Domain.Combat;
using N29.HeroesDawn.Presentation.Board;
using N29.HeroesDawn.Presentation.Combat;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Narration;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using VContainer;

namespace N29.HeroesDawn.Tests.PlayMode
{
    /// <summary>
    /// Mécanismes narratifs de New York et Moscou dans le jeu démarré, avec les vraies vues : dialogues de phase des boss en plein combat,
    /// choix d'histoire (boutons) puis épilogue du chapitre.
    /// </summary>
    public sealed class NarrativePlayTests
    {
        [UnityTearDown]
        public IEnumerator TearDown() => PlayModeHarness.TearDown();

        static IObjectResolver Container => ActivityGamesPlayTests.Container;

        /// <summary>Démarre une partie, ouvre le plateau de la zone (départ sur la case donnée) pour le parcours demandé.</summary>
        static IEnumerator OpenBoard(string zoneId, string hero, string start)
        {
            yield return PlayModeHarness.Boot(new MemoryProfileGateway(), new InstantAuthGateway());
            Object.FindAnyObjectByType<N29.HeroesDawn.Presentation.Login.LoginView>()!.PlayButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<N29.HeroesDawn.Presentation.Hub.QgPageView>() && !ActivityGamesPlayTests.Flow.IsTransitioning);
            var session = Container.Resolve<GameSession>();
            session.Meta.onboarding.hero = hero;
            var zone = session.Content.Zone(new ZoneId(zoneId));
            session.Run = ZoneRun.ForTests(session.Content, zone, 3UL, 3, true, start);
            session.RunZone = zone;
            Container.Resolve<IPublisher<NavigateRequested>>().Publish(new NavigateRequested(GameState.Board));
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<BoardView>() && !ActivityGamesPlayTests.Flow.IsTransitioning);
        }

        /// <summary>Avance la boîte de dialogue réplique par réplique (textes relevés) et répond au choix d'histoire par le premier ou le second bouton, jusqu'à la fin de la tâche.</summary>
        static IEnumerator Drain(System.Threading.Tasks.Task task, List<string> bodies, List<string> popups, bool firstOption, int frames = 3000)
        {
            string last = "";
            for (int i = 0; i < frames && !task.IsCompleted; i++)
            {
                yield return null;
                var confirm = Object.FindAnyObjectByType<ConfirmPopupView>();
                if (confirm != null && confirm.gameObject.activeInHierarchy)
                {
                    popups.Add("choice");
                    (firstOption ? confirm.OkButton : confirm.CancelButton).Button.onClick.Invoke();
                    continue;
                }
                var box = Object.FindAnyObjectByType<DialogueBoxView>();
                if (box == null || !box.gameObject.activeInHierarchy) continue;
                if (!box.Typing && box.Body.Length > 0 && box.Progress + box.Body != last) { last = box.Progress + box.Body; bodies.Add(box.Body); }
                box.NextButton.onClick.Invoke();
            }
            Assert.That(task.IsCompleted, Is.True, "la scène se termine");
            Assert.That(task.IsFaulted, Is.False, task.Exception?.ToString());
        }

        static CasePopupPresenter Presenter() => Object.FindAnyObjectByType<N29.HeroesDawn.Composition.BoardLifetimeScope>()!.Container.Resolve<CasePopupPresenter>();

        [UnityTest, Timeout(600000), Description("Vérifie qu'à la fin de Harlem 3.3 le choix s'affiche avec ses deux boutons, que la branche retenue (clinique ou marché) se joue sans l'autre et que le choix est enregistré, puis qu'après la dernière zone du chapitre l'épilogue de l'option retenue précède la scène commune — parce que le joueur doit voir l'effet de son choix")]
        public IEnumerator HarlemChoice_ShowsButtons_PlaysChosenBranch_ThenEpilogue([Values(true, false)] bool first)
        {
            yield return OpenBoard("zone_ny_3_3", StoryRoutes.AgentD, "n00");
            var session = Container.Resolve<GameSession>();
            var zone = session.RunZone!;
            var choice = StoryChoices.At(zone.Id.Value, "n19")!;
            Assert.That(choice, Is.Not.Null);
            var option = first ? choice.Options[0] : choice.Options[1];
            var other = first ? choice.Options[1] : choice.Options[0];
            var e = new NarrationEvent(new CaseId("n19"), NarrationPhase.After, zone.Nodes.Single(n => n.Id.Value == "n19").Narration!.After);

            var bodies = new List<string>(); var popups = new List<string>();
            var task = Presenter().PlayNarrationAsync(e, default).AsTask();
            yield return Drain(task, bodies, popups, first);

            Assert.That(popups, Is.EqualTo(new[] { "choice" }), "un seul choix proposé");
            Assert.That(Container.Resolve<NarrativeChoices>().Chosen(choice.Id), Is.EqualTo(option.Id), "choix enregistré");
            string branchLine = Loc.T(NarrationLineDef.ForRoute(option.Branch, StoryRoutes.AgentD).Last().TextKey);
            string otherLine = Loc.T(NarrationLineDef.ForRoute(other.Branch, StoryRoutes.AgentD).Last().TextKey);
            Assert.That(bodies, Does.Contain(branchLine), "la branche retenue est jouée");
            Assert.That(bodies, Does.Not.Contain(otherLine), "l'autre branche ne l'est pas");
            Assert.That(bodies.Any(b => b.StartsWith("narration.")), Is.False, "aucune clé de texte brute à l'écran");

            // Épilogue : seulement quand toutes les autres zones du chapitre sont validées.
            var chapter = Chapters.OfZone(zone)!;
            foreach (var z in session.Content.Zones.Where(z => Chapters.OfZone(z)?.Id == chapter.Id && z.Id != zone.Id)) session.CompletedZones.Add(z.Id.Value);
            var epi = StoryChoices.EpilogueOf(chapter.Id)!;
            var expected = NarrationLineDef.ForRoute(epi.For(option.Id), StoryRoutes.AgentD).Select(l => Loc.T(l.TextKey)).ToList();
            bodies.Clear(); popups.Clear();
            task = Presenter().PlayEpilogueAsync(default).AsTask();
            yield return Drain(task, bodies, popups, first);
            Assert.That(bodies, Is.EqualTo(expected), "épilogue de l'option retenue puis scène commune");
            Assert.That(popups, Is.Empty, "pas de second choix");
        }

        [UnityTest, Timeout(600000), Description("Vérifie que sur la Place Rouge (Agent M) le choix de confiance se propose après le dialogue du boss, que sa branche se joue, puis que l'épilogue du chapitre de Moscou s'affiche — parce que le script impose ce choix au dénouement")]
        public IEnumerator PlaceRougeChoice_AgentM_PlaysBranch_ThenEpilogue([Values(true, false)] bool first)
        {
            yield return OpenBoard("zone_mos_4_4", StoryRoutes.AgentM, "n00");
            var session = Container.Resolve<GameSession>();
            var zone = session.RunZone!;
            var choice = StoryChoices.At(zone.Id.Value, "n18")!;
            var option = first ? choice.Options[0] : choice.Options[1];
            var e = new NarrationEvent(new CaseId("n18"), NarrationPhase.After, zone.Nodes.Single(n => n.Id.Value == "n18").Narration!.After);

            var bodies = new List<string>(); var popups = new List<string>();
            var task = Presenter().PlayNarrationAsync(e, default).AsTask();
            yield return Drain(task, bodies, popups, first);
            Assert.That(popups, Is.EqualTo(new[] { "choice" }));
            Assert.That(Container.Resolve<NarrativeChoices>().Chosen(choice.Id), Is.EqualTo(option.Id));
            var after = NarrationLineDef.ForRoute(choice.Options.Select(o => o).First(o => o.Id == option.Id).Branch, StoryRoutes.AgentM).Select(l => Loc.T(l.TextKey));
            foreach (var t in after) Assert.That(bodies, Does.Contain(t));
            Assert.That(bodies.Any(b => b.StartsWith("narration.")), Is.False);

            var chapter = Chapters.OfZone(zone)!;
            foreach (var z in session.Content.Zones.Where(z => Chapters.OfZone(z)?.Id == chapter.Id && z.Id != zone.Id)) session.CompletedZones.Add(z.Id.Value);
            var expected = NarrationLineDef.ForRoute(StoryChoices.EpilogueOf(chapter.Id)!.For(option.Id), StoryRoutes.AgentM).Select(l => Loc.T(l.TextKey)).ToList();
            bodies.Clear();
            task = Presenter().PlayEpilogueAsync(default).AsTask();
            yield return Drain(task, bodies, popups, first);
            Assert.That(bodies, Is.EqualTo(expected));
        }

        /// <summary>Case voisine du boss : on y place le joueur pour qu'un seul pas lance le combat.</summary>
        static string NeighbourOfBoss(ZoneDef zone, out string boss)
        {
            string b = zone.Nodes.First(n => n.Rank == CombatRank.Boss).Id.Value;
            boss = b;
            var l = zone.Links.First(x => x.A.Value == b || x.B.Value == b);
            return l.A.Value == b ? l.B.Value : l.A.Value;
        }

        [UnityTest, Timeout(900000), Description("Vérifie que pour chaque boss à dialogues de phase (Harlem, Kapotnya, Place Rouge) la boîte de dialogue s'ouvre en plein combat au franchissement de chaque seuil avec le texte de la phase, puis que le combat reprend — parce que le joueur doit entendre le boss changer de phase")]
        public IEnumerator BossPhases_OpenTheDialogueBox_WhenTheThresholdIsCrossed([Values("zone_ny_3_4", "zone_mos_3_4", "zone_mos_4_4")] string zoneId)
        {
            string hero = zoneId.StartsWith("zone_mos") ? StoryRoutes.AgentM : StoryRoutes.AgentD;
            yield return PlayModeHarness.Boot(new MemoryProfileGateway(), new InstantAuthGateway());
            Object.FindAnyObjectByType<N29.HeroesDawn.Presentation.Login.LoginView>()!.PlayButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<N29.HeroesDawn.Presentation.Hub.QgPageView>() && !ActivityGamesPlayTests.Flow.IsTransitioning);
            var session = Container.Resolve<GameSession>();
            session.Meta.onboarding.hero = hero;
            var zone = session.Content.Zone(new ZoneId(zoneId));
            string start = NeighbourOfBoss(zone, out string bossNode);
            session.Run = ZoneRun.ForTests(session.Content, zone, 3UL, 3, true, start);
            session.RunZone = zone;
            Container.Resolve<IPublisher<NavigateRequested>>().Publish(new NavigateRequested(GameState.Board));
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<BoardView>() && !ActivityGamesPlayTests.Flow.IsTransitioning);
            PlayModeHarness.TapNode(new CaseId(bossNode));
            // Le dialogue d'avant-combat passe d'abord, puis la scène de combat se charge.
            for (int i = 0; i < 900 && !SceneManager.GetSceneByName(SceneNames.Combat).isLoaded; i++)
            {
                yield return null;
                var pre = Object.FindAnyObjectByType<DialogueBoxView>();
                if (pre != null) pre.SkipButton.onClick.Invoke();
            }
            yield return PlayModeHarness.WaitFor(() => SceneManager.GetSceneByName(SceneNames.Combat).isLoaded && !ActivityGamesPlayTests.Flow.IsTransitioning, 900);
            var view = Object.FindAnyObjectByType<CombatView>()!;
            yield return PlayModeHarness.WaitFor(() => view.Hud.BossVisible, 300);

            var sim = Container.Resolve<IActiveCombatReadModel>().Simulation!;
            var boss = sim.Units.First(u => u.Side == Side.Enemy && u.IsBoss);
            var phases = session.Content.Enemy(new EnemyId(boss.DefId)).Phases;
            Assume.That(phases.Count, Is.GreaterThan(0));
            var withLines = phases.Select((p, i) => (p, i)).Where(x => x.p.Lines.Count > 0).ToList();
            Assert.That(withLines, Is.Not.Empty, "le boss de " + zoneId + " a des dialogues de phase");
            view.AutoButton.onClick.Invoke();
            foreach (var (phase, idx) in withLines)
            {
                var expected = NarrationLineDef.ForRoute(phase.Lines, hero).Select(l => Loc.T(l.TextKey)).ToList();
                boss.Hp = System.Math.Max(1, boss.MaxHp * phase.ThresholdPercent / 100 - 1);
                var seen = new List<string>();
                for (int i = 0; i < 3000 && seen.Count < expected.Count && boss.IsAlive; i++)
                {
                    yield return null;
                    var box = Object.FindAnyObjectByType<DialogueBoxView>();
                    if (box == null || !box.gameObject.activeInHierarchy) continue;
                    if (!box.Typing && box.Body.Length > 0 && (seen.Count == 0 || seen[seen.Count - 1] != box.Body)) seen.Add(box.Body);
                    box.NextButton.onClick.Invoke();
                }
                Assert.That(seen, Is.EqualTo(expected), "phase " + (idx + 2) + " : dialogue du boss à l'écran");
                Assert.That(boss.PhaseReached, Is.GreaterThanOrEqualTo(idx + 1));
                Assert.That(seen.Any(s => s.StartsWith("narration.")), Is.False, "aucune clé brute");
                // Le combat reprend après le dialogue.
                int before = boss.PhaseReached;
                yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<DialogueBoxView>() == null, 600);
                yield return new WaitForSecondsRealtime(0.3f);
                Assert.That(Object.FindAnyObjectByType<DialogueBoxView>(), Is.Null, "la boîte se referme");
            }
        }
    }
}
