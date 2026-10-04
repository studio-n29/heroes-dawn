#nullable enable
using System.Collections;
using System.IO;
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

        static IEnumerator Capture(string name)
        {
            for (int i = 0; i < 20; i++) yield return null; // fondus et mises en page terminés
            // En batchmode, la fin d'image n'arrive jamais (WaitForEndOfFrame bloque) : rendu explicite par une caméra.
            const int w = 1920, h = 1080;
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
            session.Find(new N29.HeroesDawn.Core.MemberId("echo"))!.Fatigue = 70;
            session.Find(new N29.HeroesDawn.Core.MemberId("seve"))!.Fatigue = 30;
            yield return CaptureSettled("10-qg");

            Object.FindAnyObjectByType<QgPageView>()!.RoomButton(HubPage.Salon)!.onClick.Invoke();
            yield return CaptureSettled("11-salon");
            Shell.BackButton.Button.onClick.Invoke();
            yield return null;
            Object.FindAnyObjectByType<QgPageView>()!.RoomButton(HubPage.Infirmary)!.onClick.Invoke();
            yield return CaptureSettled("12-infirmerie");

            yield return Tab(HubPage.Team);
            yield return CaptureSettled("20-equipe");
            Object.FindAnyObjectByType<TeamPageView>()!.Tiles[0].TapButton.onClick.Invoke();
            yield return CaptureSettled("21-fiche");

            yield return Tab(HubPage.Recruit);
            yield return CaptureSettled("30-recruter");
            Object.FindAnyObjectByType<RecruitPageView>()!.Card(0).RecruitButton.onClick.Invoke();
            yield return CaptureSettled("31-confirmation");
            Object.FindAnyObjectByType<N29.HeroesDawn.Presentation.Popups.RecruitConfirmPopupView>()!.CancelButton.Button.onClick.Invoke();
            yield return null;

            yield return Tab(HubPage.Pass);
            yield return CaptureSettled("40-pass-wip");
            yield return Tab(HubPage.Shop);
            yield return CaptureSettled("50-boutique-wip");
            Shell.SettingsButton.Button.onClick.Invoke();
            yield return CaptureSettled("80-parametres");
            Assert.That(File.Exists(Path.Combine(Folder, "80-parametres.png")));
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
            Container.Resolve<IPublisher<NavigateRequested>>().Publish(new NavigateRequested(GameState.Board));
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<BoardView>() && !Flow.IsTransitioning);
            yield return Capture("03-plateau");

            PlayModeHarness.TapNode(new N29.HeroesDawn.Core.CaseId("n03"));
            yield return PlayModeHarness.WaitFor(() => SceneManager.GetSceneByName(SceneNames.Combat).isLoaded && !Flow.IsTransitioning, 600);
            yield return Capture("04-combat");

            Container.Resolve<IForceCombatWinUseCase>().ExecuteAsync().Forget();
            for (int i = 0; i < 120; i++) yield return null;
            yield return Capture("05-resultats");
            Assert.That(File.Exists(Path.Combine(Folder, "05-resultats.png")));
        }
    }
}
