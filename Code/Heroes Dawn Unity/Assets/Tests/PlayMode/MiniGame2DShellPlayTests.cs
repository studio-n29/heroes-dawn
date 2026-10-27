#nullable enable
using System;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Domain.Activities;
using N29.HeroesDawn.Domain.Activities.Games;
using N29.HeroesDawn.Presentation.Activities;
using N29.HeroesDawn.Presentation.Common;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace N29.HeroesDawn.Tests.PlayMode
{
    /// <summary>La fenêtre commune des mini-jeux 2D avec le vrai prefab : démarrage, victoire, abandon, annulation, entrées tactiles.</summary>
    public sealed class MiniGame2DShellPlayTests
    {
        [UnityTearDown]
        public IEnumerator TearDown() => PlayModeHarness.TearDown();

        sealed class StubGame : IMiniGame
        {
            public GameStatus Status { get; set; } = GameStatus.Running;
            public int Score => 2;
            public int Target => 5;
            public float TimeLeft => 30f;
            public int Lives => 3;
            public void Step(float dt) { }
            public void Abandon() { if (Status == GameStatus.Running) Status = GameStatus.Lost; }
            public ActivityResult Result() => new ActivityResult(Status == GameStatus.Won, Score);
        }

        sealed class StubDriver : IMiniGameDriver
        {
            public readonly StubGame Stub = new StubGame();
            public int Ticks, Downs, Ups, Disposed;
            public MiniGameInput Last;
            public bool UsesStick => true;
            public IMiniGame Game => Stub;
            public MiniGameHud Hud => new MiniGameHud("2 / 5", 30f, 3);
            public void Build(RectTransform field) { }
            public void Tick(float dt, MiniGameInput input) { Ticks++; Last = input; if (input.Down) Downs++; if (input.Up) Ups++; }
            public void Dispose() => Disposed++;
        }

        static MiniGame2DRequest Request(StubDriver driver) => new MiniGame2DRequest
        {
            Title = "TEST", Instructions = "Consigne", Start = "COMMENCER", Abandon = "ABANDONNER", TimeFormat = "{0} s",
            WonText = "GAGNÉ", LostText = "PERDU", Driver = driver,
        };

        static IEnumerator Open(StubDriver driver, CancellationToken ct, Action<MiniGame2DPopupView, UniTask<ActivityResult>> opened)
        {
            yield return ActivityGamesPlayTests.OnBoardNext(PrototypeContent.Zone11, "n15");
            var host = UnityEngine.Object.FindAnyObjectByType<ModalLayer>()!;
            var view = host.Spawn<IMiniGame2DPopupView>();
            var task = view.PlayAsync(Request(driver), ct);
            opened((MiniGame2DPopupView)view, task);
        }

        [UnityTest, Timeout(120000), Description("Vérifie que le premier appui lance la partie sans être transmis au jeu, que le jeu avance ensuite, et que la victoire du jeu ferme la fenêtre avec un résultat réussi — parce que c'est le cycle de toute partie")]
        public IEnumerator Start_ThenWin_ClosesWithResult()
        {
            var driver = new StubDriver();
            MiniGame2DPopupView? popup = null; UniTask<ActivityResult> task = default;
            var open = Open(driver, default, (v, t) => { popup = v; task = t; });
            while (open.MoveNext()) yield return open.Current;
            popup!.Input.PressAt(new Vector2(0.5f, 0.5f));
            yield return null;
            Assert.That(popup.Started, Is.True);
            Assert.That(driver.Downs, Is.EqualTo(0), "le premier appui démarre la partie, il n'est pas joué");
            popup.Input.ReleaseNow();
            yield return null; yield return null;
            Assert.That(driver.Ticks, Is.GreaterThan(0));
            driver.Stub.Status = GameStatus.Won;
            yield return PlayModeHarness.WaitFor(() => task.Status.IsCompleted());
            Assert.That(popup.Finished, Is.True);
            Assert.That(task.GetAwaiter().GetResult().Success, Is.True);
        }

        [UnityTest, Timeout(120000), Description("Vérifie qu'un appui sur Abandonner avant même de commencer ferme la fenêtre avec un résultat perdu — parce qu'on doit toujours pouvoir sortir")]
        public IEnumerator Abandon_ClosesWithLostResult()
        {
            var driver = new StubDriver();
            MiniGame2DPopupView? popup = null; UniTask<ActivityResult> task = default;
            var open = Open(driver, default, (v, t) => { popup = v; task = t; });
            while (open.MoveNext()) yield return open.Current;
            popup!.AbandonButton.Button.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => task.Status.IsCompleted());
            Assert.That(task.GetAwaiter().GetResult().Success, Is.False);
        }

        [UnityTest, Timeout(120000), Description("Vérifie que l'annulation de la partie (destruction de la page, Retour) rend la main sans bloquer — parce que le bug du playtest était un écran bloqué")]
        public IEnumerator Cancel_ReturnsWithoutHanging()
        {
            var driver = new StubDriver();
            var cts = new CancellationTokenSource();
            MiniGame2DPopupView? popup = null; UniTask<ActivityResult> task = default;
            var open = Open(driver, cts.Token, (v, t) => { popup = v; task = t; });
            while (open.MoveNext()) yield return open.Current;
            cts.Cancel();
            yield return PlayModeHarness.WaitFor(() => task.Status.IsCompleted());
            Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Canceled));
        }

        [UnityTest, Timeout(120000), Description("Vérifie qu'un double appui sur Commencer ne démarre qu'une partie et qu'Abandonner après la fin ne change pas le résultat — parce qu'un appui répété ne doit pas produire deux résultats")]
        public IEnumerator DoubleTapStart_StartsOnce_AndAbandonAfterEndIsIgnored()
        {
            var driver = new StubDriver();
            MiniGame2DPopupView? popup = null; UniTask<ActivityResult> task = default;
            var open = Open(driver, default, (v, t) => { popup = v; task = t; });
            while (open.MoveNext()) yield return open.Current;
            popup!.Input.PressAt(Vector2.zero);
            popup.Input.PressAt(Vector2.zero);
            yield return null;
            Assert.That(driver.Downs, Is.EqualTo(0));
            driver.Stub.Status = GameStatus.Won;
            yield return PlayModeHarness.WaitFor(() => task.Status.IsCompleted());
            popup.AbandonButton.Button.onClick.Invoke();
            yield return null;
            Assert.That(task.GetAwaiter().GetResult().Success, Is.True, "le résultat rendu reste celui de la fin de partie");
            Assert.That(driver.Stub.Status, Is.EqualTo(GameStatus.Won));
        }

        [UnityTest, Timeout(120000), Description("Vérifie que le doigt qui sort de la zone émet un relâchement, que le joystick reste dans [-1, 1] et est nul au repos — parce qu'aucun appui ne doit rester collé d'une partie à l'autre")]
        public IEnumerator Field_PointerExit_EmitsUp_AndStickIsBounded()
        {
            var driver = new StubDriver();
            MiniGame2DPopupView? popup = null; UniTask<ActivityResult> task = default;
            var open = Open(driver, default, (v, t) => { popup = v; task = t; });
            while (open.MoveNext()) yield return open.Current;
            var field = popup!.Input;
            field.PressAt(new Vector2(0.5f, 0.5f));
            yield return null;                                   // démarre
            field.ReleaseNow();
            yield return null;
            int upsBefore = driver.Ups;
            field.PressAt(new Vector2(0.5f, 0.5f));
            field.DragTo(new Vector2(0.95f, 0.5f));
            yield return null;
            Assert.That(driver.Last.Held, Is.True);
            Assert.That(driver.Last.Stick.magnitude, Is.InRange(0.99f, 1.001f), "le joystick est borné à 1");
            field.OnPointerExit(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current));
            yield return null;
            Assert.That(driver.Ups, Is.EqualTo(upsBefore + 1), "la sortie de la zone émet un relâchement");
            Assert.That(driver.Last.Stick, Is.EqualTo(Vector2.zero), "joystick nul au repos");
            popup.AbandonButton.Button.onClick.Invoke();
        }
    }
}
