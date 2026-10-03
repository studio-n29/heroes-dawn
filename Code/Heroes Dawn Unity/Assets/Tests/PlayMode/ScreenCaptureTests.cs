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

        [UnityTest, Explicit("Captures à la demande"), Description("Produit une capture de la connexion, du QG, du plateau et du combat — parce que le passage à TextMeshPro change le rendu des polices et doit être relu à l'œil avant toute conversion (spec §11)")]
        public IEnumerator CaptureEveryScreen()
        {
            yield return PlayModeHarness.Boot(new MemoryProfileGateway(), new InstantAuthGateway());
            yield return Capture("01-connexion");

            Object.FindAnyObjectByType<LoginView>()!.PlayButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<HubView>() && !Flow.IsTransitioning);
            yield return Capture("02-qg");

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
