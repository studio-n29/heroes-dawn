#nullable enable
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Application.Features.World;
using N29.HeroesDawn.Application.Flow;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Composition;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Presentation.Board;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Login;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer;

namespace N29.HeroesDawn.Tests.PlayMode
{
    /// <summary>Parcours d'Agent M de bout en bout (chapitre Sergueiv) dans le jeu démarré : carte de Moscou, déploiement réel, puis les quatre zones jusqu'au Passeur et l'état de fin d'acte.</summary>
    public sealed class MoscowFullRunTests
    {
        [UnityTearDown]
        public IEnumerator TearDown() => PlayModeHarness.TearDown();

        static IObjectResolver Container => GameBootstrap.Instance!.Scope.Container;
        static IGameFlow Flow => Container.Resolve<IGameFlow>();

        /// <summary>Chemin le plus court (liens non orientés) d'une case à une autre, cases intermédiaires et arrivée comprises.</summary>
        static List<CaseId> PathBetween(ZoneDef zone, CaseId from, CaseId to)
        {
            var prev = new Dictionary<CaseId, CaseId>();
            var queue = new Queue<CaseId>();
            queue.Enqueue(from); prev[from] = from;
            while (queue.Count > 0)
            {
                var cur = queue.Dequeue();
                if (cur == to) break;
                foreach (var l in zone.Links)
                {
                    CaseId? next = l.A == cur ? l.B : l.B == cur ? l.A : (CaseId?)null;
                    if (next.HasValue && !prev.ContainsKey(next.Value)) { prev[next.Value] = cur; queue.Enqueue(next.Value); }
                }
            }
            var path = new List<CaseId>();
            for (var c = to; c != from; c = prev[c]) path.Add(c);
            path.Reverse();
            return path;
        }

        /// <summary>Joue une zone case par case (combats gagnés, cases réussies) en visitant les cibles de ses objectifs ; renvoie vrai si la zone est validée.</summary>
        static bool PlayZone(GameSession session, ZoneDef zone, out List<string> bossEnemies)
        {
            bossEnemies = new List<string>();
            var run = ZoneRun.ForTests(session.Content, zone, 5UL, 1, true, zone.Entry.Value);
            var at = zone.Entry;
            bool validated = false;
            foreach (var target in zone.Objectives.Select(o => o.Target))
            {
                foreach (var step in PathBetween(zone, at, target))
                {
                    run.SetPmForTests(1);
                    var events = run.MoveTo(step).ToList();
                    if (run.Phase == ZoneRunPhase.InCombat)
                    {
                        if (zone.Nodes.Single(n => n.Id == step).Rank == CombatRank.Boss)
                            bossEnemies.AddRange(run.PendingCase!.Encounter!.Waves.SelectMany(w => w).Select(e => e.Value));
                        events = run.ResolveCombat(true).ToList();
                    }
                    else if (run.Phase == ZoneRunPhase.InCaseInteraction) events = run.ResolveCase(true).ToList();
                    validated |= events.Any(e => e is ZoneValidatedEvent);
                    at = step;
                }
            }
            if (validated) session.CompletedZones.Add(zone.Id.Value); // même conséquence que ZoneActions (validation de zone)
            return validated;
        }

        [UnityTest, Timeout(600000), Description("Vérifie qu'un profil Agent M voit la carte de Moscou, déploie la zone M1.1 par les écrans réels, puis qu'en jouant les quatre zones jusqu'au Passeur chaque zone se valide, que le boss est le Passeur et que l'acte I se termine sur Paris sans zone ni bouton mort — parce que c'est le parcours promis par la 1.0.0")]
        public IEnumerator AgentM_NewProfile_PlaysFourZones_AndReachesTheEndOfAct()
        {
            yield return PlayModeHarness.Boot(new MemoryProfileGateway(), new InstantAuthGateway());
            Object.FindAnyObjectByType<LoginView>()!.PlayButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<QgPageView>() && !Flow.IsTransitioning);

            var session = Container.Resolve<GameSession>();
            session.Meta.onboarding.hero = StoryRoutes.AgentM; // profil Agent M (le choix du héros se fait à la première connexion)
            Assert.That(session.Route.Id, Is.EqualTo("agent_m"));
            Assert.That(StoryProgress.Current(session).City, Is.EqualTo("moscow"));
            Assert.That(ZoneProgress.Recommended(session).Id.Value, Is.EqualTo("zone_mos_1_1"));

            // Écrans réels : MISSION → carte de Moscou → première zone → plateau.
            Object.FindAnyObjectByType<QgPageView>()!.MissionButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<CityMapPageView>());
            for (int i = 0; i < 10; i++) yield return null;
            var map = Object.FindAnyObjectByType<CityMapPageView>()!;
            Assert.That(map.RowButton(3), Is.Not.Null, "quatre zones listées");
            Assert.That(map.RowButton(4), Is.Null, "pas de cinquième zone");
            Assert.That(map.PointButton("zone_mos_1_1"), Is.Not.Null, "point M1.1 sur la carte de Moscou");
            map.RowButton(0)!.onClick.Invoke();
            for (int i = 0; i < 5; i++) yield return null;
            PlayModeHarness.DeployNow();
            yield return PlayModeHarness.DismissChapterIntro();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<BoardView>() && !Flow.IsTransitioning, 600);
            Assert.That(Flow.Current, Is.EqualTo(GameState.Board));
            Assert.That(session.RunZone!.Id.Value, Is.EqualTo("zone_mos_1_1"));
            yield return PlayModeHarness.BackToHqConfirmed();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<QgPageView>() && !Flow.IsTransitioning, 600);

            // Les seize zones de Moscou (quatre quartiers), chacune jouée dès qu'elle est ouverte, jusqu'à la Place Rouge ; le Passeur clôt le premier quartier.
            var moscow = session.Content.Zones.Where(z => !z.IsTutorial && StoryRoutes.CityOfBorough(z.Borough) == StoryRoutes.Moscow).ToList();
            Assert.That(moscow.Count, Is.EqualTo(16));
            List<string> boss = new List<string>();
            var played = new List<string>();
            while (moscow.Any(z => ZoneProgress.StatusOf(session, z) != ZoneStatus.Done))
            {
                var zone = moscow.OrderBy(z => z.Code).FirstOrDefault(z => ZoneProgress.StatusOf(session, z) == ZoneStatus.Available);
                Assert.That(zone, Is.Not.Null, "une zone est toujours ouverte tant que l'acte n'est pas fini (jouées : " + string.Join(",", played) + ")");
                Assert.That(PlayZone(session, zone!, out var b), Is.True, zone!.Code + " validée");
                if (zone.Code == "M1.4") boss = b;
                Assert.That(ZoneProgress.StatusOf(session, zone), Is.EqualTo(ZoneStatus.Done), zone.Code);
                played.Add(zone.Code);
            }
            Assert.That(played.Count, Is.EqualTo(16));
            Assert.That(boss, Does.Contain("mos_passeur"), "le boss de Sergueiv est le Passeur");

            // Fin de l'acte I : Paris sans zone, plus rien d'ouvert, la carte reste sur Moscou sans bouton mort.
            var act = StoryProgress.Current(session);
            Assert.That(act.City, Is.EqualTo("paris"));
            Assert.That(act.HasZones, Is.False);
            Assert.That(session.Content.Zones.Where(z => !z.IsTutorial).Count(z => ZoneProgress.StatusOf(session, z) == ZoneStatus.Available), Is.EqualTo(0));
            var nav = Object.FindAnyObjectByType<HubLifetimeScope>()!.Container.Resolve<HubNavigator>();
            nav.Open(HubPage.CityMap);
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<CityMapPageView>());
            for (int i = 0; i < 10; i++) yield return null;
            Assert.That(Object.FindAnyObjectByType<CityMapPageView>()!.PointButton("zone_mos_1_4"), Is.Not.Null, "la carte montre toujours Moscou");
        }
    }
}
