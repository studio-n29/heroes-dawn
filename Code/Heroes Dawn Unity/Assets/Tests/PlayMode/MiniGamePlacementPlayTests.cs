#nullable enable
using System.Collections;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Domain.Activities.Games;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Presentation.Activities;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer;

namespace N29.HeroesDawn.Tests.PlayMode
{
    /// <summary>Les mini-jeux placés dans la campagne s'ouvrent depuis le vrai plateau, avec la vraie fenêtre, et se ferment sans bloquer.</summary>
    public sealed class MiniGamePlacementPlayTests
    {
        [UnityTearDown]
        public IEnumerator TearDown() => PlayModeHarness.TearDown();

        static IEnumerator OpenAndAbandon(string zoneId, string node, System.Type expectedGame)
        {
            yield return ActivityGamesPlayTests.OnBoardNext(new ZoneId(zoneId), node);
            var session = ActivityGamesPlayTests.Container.Resolve<GameSession>();
            PlayModeHarness.TapNode(new CaseId(node));
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<MiniGame2DPopupView>() != null, 600);
            var popup = Object.FindAnyObjectByType<MiniGame2DPopupView>()!;
            Assert.That(popup.Driver, Is.Not.Null);
            Assert.That(popup.Driver!.Game, Is.InstanceOf(expectedGame), zoneId + "/" + node);
            popup.AbandonButton.Button.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<MiniGame2DPopupView>() == null && session.Run!.Phase == ZoneRunPhase.AwaitingRoll, 900);
            Assert.That(session.Run!.Phase, Is.EqualTo(ZoneRunPhase.AwaitingRoll), "le plateau est rendu au joueur après l'abandon");
        }

        [UnityTest, Timeout(240000), Description("Vérifie que le bouton Retour (TryCancelTop du service de fenêtres) ferme un mini-jeu en cours comme un abandon et rend le plateau — parce que Retour doit toujours permettre de sortir")]
        public IEnumerator BackButton_ClosesTheMiniGame()
        {
            yield return ActivityGamesPlayTests.OnBoardNext(new ZoneId("zone_ny_1_2"), "n13");
            var session = ActivityGamesPlayTests.Container.Resolve<GameSession>();
            PlayModeHarness.TapNode(new CaseId("n13"));
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<MiniGame2DPopupView>() != null, 600);
            var modals = Object.FindAnyObjectByType<N29.HeroesDawn.Composition.BoardLifetimeScope>()!.Container.Resolve<N29.HeroesDawn.Presentation.Mvp.IModalService>();
            Assert.That(modals.TryCancelTop(), Is.True, "la fenêtre accepte Retour");
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<MiniGame2DPopupView>() == null && session.Run!.Phase == ZoneRunPhase.AwaitingRoll, 900);
            Assert.That(session.Run!.Phase, Is.EqualTo(ZoneRunPhase.AwaitingRoll));
        }

        [UnityTest, Timeout(240000), Description("Vérifie que la case Engrenage de la zone 1.2 (jeu au toucher) ouvre la vraie fenêtre commune avec la partie d'engrenages, et qu'Abandonner rend le plateau sans le bloquer — parce que c'était le bug du playtest")]
        public IEnumerator Gears_OnZone12_OpensAndCloses() => OpenAndAbandon("zone_ny_1_2", "n13", typeof(GearsGame));

        [UnityTest, Timeout(240000), Description("Vérifie que la case Tir franc de la zone 1.3 (jeu à maintenir et relâcher) ouvre la vraie fenêtre commune et se ferme sans bloquer — parce que c'est le jeu à geste")]
        public IEnumerator FreeThrow_OnZone13_OpensAndCloses() => OpenAndAbandon("zone_ny_1_3", "n06", typeof(FreeThrowGame));

        [UnityTest, Timeout(240000), Description("Vérifie que la case Infiltration de la zone 1.3 (jeu au joystick) ouvre la vraie fenêtre commune et se ferme sans bloquer — parce que c'est le jeu au joystick virtuel")]
        public IEnumerator Infiltration_OnZone13_OpensAndCloses() => OpenAndAbandon("zone_ny_1_3", "n11", typeof(InfiltrationGame));
    }
}
