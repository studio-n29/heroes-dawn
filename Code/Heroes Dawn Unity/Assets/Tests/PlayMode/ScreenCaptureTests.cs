#nullable enable
using System.Collections;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Combat;
using N29.HeroesDawn.Application.Flow;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Composition;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Presentation.Board;
using N29.HeroesDawn.Presentation.Combat;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Login;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using VContainer;

namespace N29.HeroesDawn.Tests.PlayMode
{
    /// <summary>
    /// Captures des écrans pour relecture visuelle (Code/docs/test-reports/captures, ignoré par git).
    /// Explicite : ne tourne que lorsqu'on le demande par -testFilter.
    /// </summary>
    public sealed class ScreenCaptureTests
    {
        [UnityTearDown]
        public IEnumerator TearDown() => PlayModeHarness.TearDown();

        static IObjectResolver Container => GameBootstrap.Instance!.Scope.Container;
        static IGameFlow Flow => Container.Resolve<IGameFlow>();

        static string Folder => Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "../../docs/test-reports/captures"));

        internal static IEnumerator Capture(string name)
        {
            for (int i = 0; i < 20; i++) yield return null; // fondus et mises en page terminés
            // En batchmode, la fin d'image n'arrive jamais (WaitForEndOfFrame bloque) : rendu explicite par une caméra.
            const int w = 2160, h = 972; // téléphone 20:9 : le cas le plus bas (zone de page de ~710 px)
            var rt = new RenderTexture(w, h, 24);
            var camGo = new GameObject("CaptureCamera");
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.targetTexture = rt;
            var canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            var modes = new RenderMode[canvases.Length];
            for (int i = 0; i < canvases.Length; i++)
            {
                modes[i] = canvases[i].renderMode;
                if (!canvases[i].isRootCanvas) continue;
                canvases[i].renderMode = RenderMode.ScreenSpaceCamera;
                canvases[i].worldCamera = cam;
                canvases[i].planeDistance = 10f - canvases[i].sortingOrder * 0.01f;
            }
            Canvas.ForceUpdateCanvases();
            cam.Render();
            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            tex.Apply();
            RenderTexture.active = prev;
            for (int i = 0; i < canvases.Length; i++) if (canvases[i] != null && canvases[i].isRootCanvas) canvases[i].renderMode = modes[i];
            Directory.CreateDirectory(Folder);
            File.WriteAllBytes(Path.Combine(Folder, name + ".png"), tex.EncodeToPNG());
            Object.Destroy(tex);
            Object.Destroy(camGo);
            rt.Release();
        }

        /// <summary>Capture après la fin des animations d'entrée (cascade, glissement : temps réel).</summary>
        static IEnumerator CaptureSettled(string name)
        {
            yield return new WaitForSecondsRealtime(1.2f);
            yield return Capture(name);
        }

        static HubShellView Shell => Object.FindAnyObjectByType<HubShellView>()!;

        static IEnumerator Tab(HubPage tab)
        {
            Shell.TabButton(tab).Button.onClick.Invoke();
            yield return null;
        }

        [UnityTest, Explicit("Captures à la demande"), Timeout(180000), Description("Produit une capture de chaque page du hub — parce que chaque page se compare à sa maquette (M10, M11, M12, M20, M21, M30, M31, M80, M85)")]
        public IEnumerator CaptureHubPages()
        {
            yield return PlayModeHarness.Boot(new MemoryProfileGateway(), new InstantAuthGateway());
            Object.FindAnyObjectByType<LoginView>()!.PlayButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<QgPageView>() && !Flow.IsTransitioning);
            var session = Container.Resolve<GameSession>();
            session.Find(new N29.HeroesDawn.Core.MemberId("darius_brooks"))!.Fatigue = 30; // la seule recrue de départ
            yield return CaptureSettled("10-qg");
            Container.Resolve<N29.HeroesDawn.Application.Features.Tutorial.TutorialGuide>().Skip();
            yield return new WaitForSecondsRealtime(0.3f);
            yield return Capture("10b-qg-sans-tuto");
            Object.FindAnyObjectByType<QgPageView>()!.MissionButton.onClick.Invoke();
            yield return CaptureSettled("13-carte");
            Shell.BackButton.Button.onClick.Invoke();
            yield return null;

            Object.FindAnyObjectByType<QgPageView>()!.RoomButton(HubPage.Lab)!.onClick.Invoke();
            yield return CaptureSettled("11-labo");
            Shell.BackButton.Button.onClick.Invoke();
            Object.FindAnyObjectByType<QgPageView>()!.RoomButton(HubPage.Lab)!.onClick.Invoke();

            yield return Tab(HubPage.Team);
            yield return CaptureSettled("20-equipe");
            Object.FindAnyObjectByType<TeamPageView>()!.Tiles[0].TapButton.onClick.Invoke();
            yield return CaptureSettled("21-fiche");
            Object.FindAnyObjectByType<MemberSheetView>()!.ItemTileButton(0).onClick.Invoke();
            yield return CaptureSettled("21b-fiche-objets");
            Object.FindAnyObjectByType<MemberSheetView>()!.SkillTileButton(2).onClick.Invoke();
            yield return CaptureSettled("21c-fiche-cartes");

            yield return Tab(HubPage.Recruit);
            yield return CaptureSettled("30-recruter");
            Object.FindAnyObjectByType<RecruitPageView>()!.Card(0).RecruitButton.onClick.Invoke();
            yield return CaptureSettled("31-confirmation");
            Object.FindAnyObjectByType<N29.HeroesDawn.Presentation.Popups.RecruitConfirmPopupView>()!.CancelButton.Button.onClick.Invoke();
            yield return null;

            yield return Tab(HubPage.Pass);
            yield return CaptureSettled("40-pass");
            yield return Tab(HubPage.Shop);
            yield return CaptureSettled("50-boutique");
            foreach (var shopTab in new[] { (ShopTab.Boosters, "50b-boutique-boosters"), (ShopTab.Items, "50c-boutique-objets"), (ShopTab.Gems, "50d-boutique-gemmes"), (ShopTab.Gift, "50e-boutique-cadeau") })
            {
                Object.FindAnyObjectByType<ShopTabsView>()!.Tab((int)shopTab.Item1).Button.onClick.Invoke();
                yield return CaptureSettled(shopTab.Item2);
            }
            yield return Tab(HubPage.Qg);
            yield return null;
            foreach (var room in new[] { (HubPage.Operations, "13-operations"), (HubPage.Lab, "14-labo"), (HubPage.Grimoire, "16-grimoire"), (HubPage.Inventory, "17-inventaire") })
            {
                Object.FindAnyObjectByType<QgPageView>()!.RoomButton(room.Item1)!.onClick.Invoke();
                yield return CaptureSettled(room.Item2);
                Shell.BackButton.Button.onClick.Invoke();
                yield return null;
            }
            yield return Tab(HubPage.Profile);
            yield return CaptureSettled("15-profil");
            Shell.SettingsButton.Button.onClick.Invoke();
            yield return CaptureSettled("80-parametres-audio");
            Object.FindAnyObjectByType<SettingsPageView>()!.CategoryButton(2).onClick.Invoke();
            yield return CaptureSettled("82-parametres-accessibilite");
            Object.FindAnyObjectByType<SettingsPageView>()!.CategoryButton(7).onClick.Invoke();
            yield return CaptureSettled("85-parametres-support");
            Assert.That(File.Exists(Path.Combine(Folder, "85-parametres-support.png")));
        }

        [UnityTest, Explicit("Captures à la demande"), Timeout(180000), Description("Produit les captures du plateau agrandi : portée avec cases cachées, fiche d'une destination (chemin illuminé, dézoom), fiche d'une case sous le brouillard — parce que la prévisualisation du déplacement se relit à l'œil")]
        public IEnumerator CaptureBoardMove()
        {
            yield return PlayModeHarness.Boot(new MemoryProfileGateway(), new InstantAuthGateway());
            Object.FindAnyObjectByType<LoginView>()!.PlayButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<QgPageView>() && !Flow.IsTransitioning);

            var session = Container.Resolve<GameSession>();
            var zone = session.Content.Zone(PrototypeContent.Zone11);
            session.Run = ZoneRun.ForTests(session.Content, zone, 3UL, 6, false);
            session.RunZone = zone;
            session.Meta.tutorial.done = true; // pas de tutoriel : la capture joue elle-même une compétence
            session.Meta.tutorial.hintsSeen.AddRange(new[] { "auto", "defeat", "fatigue", "timeline", "zone_done" }); // ni d astuce qui bloque le combat
            var run = session.Run!;
            Container.Resolve<IPublisher<NavigateRequested>>().Publish(new NavigateRequested(GameState.Board));
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<BoardView>() && !Flow.IsTransitioning);
            yield return CaptureSettled("03-plateau-portee");

            var map = Object.FindAnyObjectByType<BoardMapView>()!;
            var far = run.Destinations.OrderByDescending(d => d.Value.Cost).First().Key;
            map.NodeButton(far).onClick.Invoke();
            yield return new WaitForSecondsRealtime(0.8f);
            yield return Capture("03b-plateau-fiche");
            Assert.That(Object.FindAnyObjectByType<BoardView>()!.PreviewVisible, Is.True);

            Object.FindAnyObjectByType<BoardView>()!.PreviewCancelButton.onClick.Invoke();
            var hiddenDest = run.Destinations.Keys.First(id => !run.Fog.IsRevealed(id));
            map.NodeButton(hiddenDest).onClick.Invoke();
            yield return new WaitForSecondsRealtime(0.8f);
            yield return Capture("03c-plateau-brouillard");
            Assert.That(File.Exists(Path.Combine(Folder, "03c-plateau-brouillard.png")));
        }

        [UnityTest, Explicit("Captures à la demande"), Timeout(180000), Description("Produit la capture de la fenêtre d'activité (pêche du port de nuit) — parce que la mise en page du framework d'activités se relit à l'œil")]
        public IEnumerator CaptureActivityWindow()
        {
            yield return PlayModeHarness.Boot(new MemoryProfileGateway(), new InstantAuthGateway());
            Object.FindAnyObjectByType<LoginView>()!.PlayButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<QgPageView>() && !Flow.IsTransitioning);
            var session = Container.Resolve<GameSession>();
            var zone = session.Content.Zone(PrototypeContent.Zone11);
            var from = new BoardGraph(zone).Neighbors(new N29.HeroesDawn.Core.CaseId("n15")).First();
            session.Run = ZoneRun.ForTests(session.Content, zone, 3UL, 1, true, from.Value);
            session.RunZone = zone;
            session.Meta.tutorial.done = true; // pas de tutoriel : la capture joue elle-même une compétence
            session.Meta.tutorial.hintsSeen.AddRange(new[] { "auto", "defeat", "fatigue", "timeline", "zone_done" }); // ni d astuce qui bloque le combat
            Container.Resolve<IPublisher<NavigateRequested>>().Publish(new NavigateRequested(GameState.Board));
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<BoardView>() && !Flow.IsTransitioning);
            PlayModeHarness.TapNode(new N29.HeroesDawn.Core.CaseId("n15"));
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<N29.HeroesDawn.Presentation.Popups.ActivityPopupView>() != null);
            yield return CaptureSettled("06-activite-peche");
            Assert.That(File.Exists(Path.Combine(Folder, "06-activite-peche.png")));
        }

        [UnityTest, Explicit("Captures à la demande"), Description("Produit une capture de la connexion, du QG, du plateau et du combat — parce que le passage à TextMeshPro change le rendu des polices et doit être relu à l'œil avant toute conversion (spec §11)")]
        public IEnumerator CaptureEveryScreen()
        {
            yield return PlayModeHarness.Boot(new MemoryProfileGateway(), new InstantAuthGateway());
            yield return Capture("01-connexion");

            Object.FindAnyObjectByType<LoginView>()!.PlayButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<QgPageView>() && !Flow.IsTransitioning);
            yield return Capture("02-qg");

            Object.FindAnyObjectByType<HubShellView>()!.TabButton(HubPage.Recruit).Button.onClick.Invoke();
            for (int i = 0; i < 10; i++) yield return null;
            yield return Capture("02b-boutique");

            var session = Container.Resolve<GameSession>();
            var zone = session.Content.Zone(PrototypeContent.Zone11);
            session.Run = ZoneRun.ForTests(session.Content, zone, 3UL, 1, true, "n02");
            session.RunZone = zone;
            session.Meta.tutorial.done = true; // pas de tutoriel : la capture joue elle-même une compétence
            session.Meta.tutorial.hintsSeen.AddRange(new[] { "auto", "defeat", "fatigue", "timeline", "zone_done" }); // ni d astuce qui bloque le combat
            Container.Resolve<IPublisher<NavigateRequested>>().Publish(new NavigateRequested(GameState.Board));
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<BoardView>() && !Flow.IsTransitioning);
            yield return Capture("03-plateau");

            PlayModeHarness.TapNode(new N29.HeroesDawn.Core.CaseId("n03"));
            yield return PlayModeHarness.WaitFor(() => SceneManager.GetSceneByName(SceneNames.Combat).isLoaded && !Flow.IsTransitioning, 600);
            yield return new WaitForSecondsRealtime(5f); // le temps qu un allié soit prêt (panneau de compétences)
            yield return Capture("04-combat");

            // Animation de compétence : une compétence jouée, capture en plein vol du projectile.
            var tip = Object.FindAnyObjectByType<N29.HeroesDawn.Presentation.Tutorial.TutorialOverlayView>();
            if (tip != null) { tip.OkButton.Button.onClick.Invoke(); yield return new WaitForSecondsRealtime(0.5f); } // ferme l astuce de la frise
            var combatView = Object.FindAnyObjectByType<CombatView>()!;
            // Animations de competence (appel direct de la vue sur le combat reel) : feu vers un ennemi, puis soin sur un allie.
            int enemyId = combatView.EnemyCards()[0].UnitId;
            combatView.PlaySkillFx(new SkillFxData { ActorId = 0, TargetIds = new[] { enemyId }, Kind = SkillFxKind.Fire, Title = "Jet de flammes" }, 1.3f);
            yield return new WaitForSecondsRealtime(0.17f);
            yield return Capture("04b-combat-animation");
            yield return new WaitForSecondsRealtime(1.2f);
            combatView.PlaySkillFx(new SkillFxData { ActorId = 2, TargetIds = new[] { 0, 1 }, Kind = SkillFxKind.Heal, Title = "Choeur de guerison" }, 1.3f);
            yield return new WaitForSecondsRealtime(0.3f);
            yield return Capture("04c-combat-animation");

            Container.Resolve<IForceCombatWinUseCase>().ExecuteAsync().Forget();
            for (int i = 0; i < 120; i++) yield return null;
            yield return Capture("05-resultats");
            Assert.That(File.Exists(Path.Combine(Folder, "05-resultats.png")));
        }
    }
}
