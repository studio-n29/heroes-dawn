#nullable enable
using System.Collections;
using N29.HeroesDawn.Application.Flow;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Login;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace N29.HeroesDawn.Tests.PlayMode
{
    /// <summary>Suivi des quêtes avec les vrais prefabs : depuis la carte du QG jusqu'à la fenêtre.</summary>
    public sealed class QuestLogPlayTests
    {
        [UnityTearDown]
        public IEnumerator TearDown() => PlayModeHarness.TearDown();

        static IEnumerator OpenQuestLog()
        {
            yield return PlayModeHarness.Boot(new MemoryProfileGateway(), new InstantAuthGateway());
            Object.FindAnyObjectByType<LoginView>()!.PlayButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<QgPageView>());
            yield return null;
            Object.FindAnyObjectByType<QgPageView>()!.QuestsCard.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<QuestLogPopupView>() != null);
        }

        [UnityTest, Timeout(300000), Description("Vérifie que toucher la carte des quêtes du QG ouvre le suivi avec les quatre quêtes principales et une quête secondaire, et que Fermer le referme — parce que c'est le parcours du joueur")]
        public IEnumerator Qg_OpensTheQuestLog()
        {
            yield return OpenQuestLog();
            var log = Object.FindAnyObjectByType<QuestLogPopupView>()!;
            yield return null;
            Assert.That(log.MainRows.Count, Is.EqualTo(4));
            Assert.That(log.MainRows[0].Status, Is.EqualTo(1), "Red Hook est en cours");
            Assert.That(log.SideRows.Count, Is.EqualTo(1));
            log.CloseButton.Button.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<QuestLogPopupView>() == null);
        }

        [UnityTest, Explicit("Captures à la demande"), Timeout(300000), Description("Produit les captures du QG et du suivi des quêtes — parce que ces rendus se relisent à l'œil")]
        public IEnumerator CaptureTheQuestLog()
        {
            yield return PlayModeHarness.Boot(new MemoryProfileGateway(), new InstantAuthGateway());
            Object.FindAnyObjectByType<LoginView>()!.PlayButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<QgPageView>());
            yield return new WaitForSecondsRealtime(1.2f);
            yield return ScreenCaptureTests.Capture("40-qg-quetes");
            Object.FindAnyObjectByType<QgPageView>()!.QuestsCard.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<QuestLogPopupView>() != null);
            yield return new WaitForSecondsRealtime(0.8f);
            yield return ScreenCaptureTests.Capture("41-suivi-des-quetes");
        }
    }
}
