#nullable enable
using System;
using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Activities.Games;
using N29.HeroesDawn.Presentation.Activities;
using N29.HeroesDawn.Presentation.Common;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace N29.HeroesDawn.Tests.PlayMode
{
    /// <summary>
    /// Chaque mini-jeu 2D de la campagne se gagne sur la vraie fenêtre commune (point différé de la 0.14.0) : un joueur automatique
    /// joue avec les règles du jeu (cible, moment, pièces) pendant que la fenêtre fait tourner le pilote ; la réussite et le rappel pédagogique s'affichent.
    /// </summary>
    public sealed class MiniGameVictoryPlayTests
    {
        [UnityTearDown]
        public IEnumerator TearDown() => PlayModeHarness.TearDown();

        static readonly ZoneEnvironment City = new ZoneEnvironment(Biome.City, Weather.Day);

        /// <summary>Ouvre le mini-jeu sur le plateau, démarre la partie d'un toucher, laisse jouer <paramref name="bot"/> jusqu'à la fin, puis vérifie la réussite.</summary>
        static IEnumerator PlayToWin(string activityId, Func<MiniGame2DPopupView, IMiniGame, IEnumerator> bot, params (string Key, int Value)[] overrides)
        {
            yield return ActivityGamesPlayTests.OnBoardNext(PrototypeContent.Zone11, "n15");
            var plan = ActivityGamesPlayTests.PlanOf(activityId, City, overrides);
            var driver = MiniGameDrivers.Create(plan, new N29.HeroesDawn.Presentation.Mvp.LocLocalization())!;
            var request = ActivityGamesPlayTests.CasePresenter().MiniGame2DRequestFor(plan, driver);
            Assert.That(request.Theme, Does.StartWith("THÈME"), "le jeu annonce son thème éducatif");
            Assert.That(request.Learned, Does.StartWith("Tu as appris"), "et le rappelle à la réussite");
            var host = UnityEngine.Object.FindAnyObjectByType<ModalLayer>()!;
            var view = host.Spawn<IMiniGame2DPopupView>();
            var task = view.PlayAsync(request, default);
            var popup = (MiniGame2DPopupView)view;
            popup.Input.PressAt(new Vector2(0.5f, 0.5f));
            yield return null;
            popup.Input.ReleaseNow();
            yield return null;
            Assert.That(popup.Started, Is.True, "le premier toucher lance la partie");
            yield return bot(popup, driver.Game);
            for (int i = 0; i < 20 && !task.Status.IsCompleted(); i++) yield return null;
            Assert.That(task.Status.IsCompleted(), Is.True, "la fenêtre rend la main");
            Assert.That(driver.Game.Status, Is.EqualTo(GameStatus.Won));
            Assert.That(task.GetAwaiter().GetResult().Success, Is.True);
            host.Despawn(view);
        }

        static IEnumerator Until(IMiniGame game, int maxFrames = 3000)
        {
            for (int i = 0; i < maxFrames && game.Status == GameStatus.Running; i++) yield return null;
        }

        [UnityTest, Timeout(240000), Description("Vérifie que « Le rythme » se gagne sur la vraie fenêtre : toucher chaque piste au bon moment vide la séquence — parce que c'est le jeu du tempo")]
        public IEnumerator Rhythm_TappingOnBeat_Wins() => PlayToWin("rhythm_rooftop", Rhythm, ("notes", 8));

        static IEnumerator Rhythm(MiniGame2DPopupView popup, IMiniGame game)
        {
            var g = (RhythmGame)game;
            for (int i = 0; i < 3000 && g.Status == GameStatus.Running; i++)
            {
                foreach (var n in g.Notes.Where(x => x.State == NoteState.Pending).ToList())
                    if (Math.Abs(g.Song - n.Time) <= RhythmGame.PerfectWindow) g.Press(n.Lane);
                yield return null;
            }
        }

        [UnityTest, Timeout(240000), Description("Vérifie que « Tir franc » se gagne : relâcher la jauge dans la zone verte touche la cible à chaque lancer — parce que c'est le jeu de la puissance")]
        public IEnumerator FreeThrow_ReleasingInTheGreenZone_Wins() => PlayToWin("free_throw_court", FreeThrow);

        static IEnumerator FreeThrow(MiniGame2DPopupView popup, IMiniGame game)
        {
            var g = (FreeThrowGame)game;
            for (int i = 0; i < 3000 && g.Status == GameStatus.Running; i++)
            {
                if (!popup.Input.Held) popup.Input.PressAt(new Vector2(0.5f, 0.5f)); // le pilote annule une charge quand le doigt est levé
                if (!g.Charging) g.BeginCharge();
                else if (Math.Abs(g.Gauge - g.TargetCenter) <= g.TargetHalfWidth) g.Release();
                yield return null;
            }
        }

        [UnityTest, Timeout(240000), Description("Vérifie que « Caisse au choix » se gagne : relâcher quand le curseur survole la caisse récompensée trouve les trois caisses — parce que c'est le jeu du choix")]
        public IEnumerator CratePick_ReleasingOnTheRewardCrate_Wins() => PlayToWin("crate_pick_depot", CratePick);

        static IEnumerator CratePick(MiniGame2DPopupView popup, IMiniGame game)
        {
            var g = (CratePickGame)game;
            for (int i = 0; i < 3000 && g.Status == GameStatus.Running; i++)
            {
                if (!popup.Input.Held) popup.Input.PressAt(new Vector2(0.5f, 0.5f)); // le pilote annule une charge quand le doigt est levé
                if (!g.Charging) g.BeginCharge();
                else if (g.Charge >= CratePickGame.MinCharge && CratePickGame.CrateAt(g.Cursor) == g.Reward) g.Release();
                yield return null;
            }
        }

        [UnityTest, Timeout(240000), Description("Vérifie qu'« Analyse ADN » se gagne : retirer tous les parasites sans toucher un nœud sain vide le brin — parce que c'est le jeu de la biologie")]
        public IEnumerator Dna_RemovingEveryParasite_Wins() => PlayToWin("dna_lab", Dna);

        static IEnumerator Dna(MiniGame2DPopupView popup, IMiniGame game)
        {
            var g = (DnaAnalysisGame)game;
            foreach (var n in g.Nodes.Where(x => x.Parasite).ToList())
            {
                g.Tap(n.X, n.Y);
                yield return null;
            }
            yield return Until(g, 10);
        }

        [UnityTest, Timeout(240000), Description("Vérifie qu'« Engrenage » se gagne : retourner les engrenages dans le mauvais sens entraîne tout le train avant la fin des coups — parce que c'est le jeu de la mécanique")]
        public IEnumerator Gears_FlippingTheWrongOnes_Wins() => PlayToWin("gears_workshop", Gears);

        static IEnumerator Gears(MiniGame2DPopupView popup, IMiniGame game)
        {
            var g = (GearsGame)game;
            var motor = g.Gears[0];
            foreach (var gear in g.Gears.Where(x => !x.Motor).ToList())
            {
                bool required = (gear.Col + gear.Row) % 2 == (motor.Col + motor.Row) % 2 ? motor.Clockwise : !motor.Clockwise;
                if (gear.Clockwise != required) g.Toggle(gear.Id);
                yield return null;
            }
            yield return Until(g, 10);
        }

        [UnityTest, Timeout(240000), Description("Vérifie qu'« Ingénierie de secours » se gagne : choisir les cinq pièces dans l'ordre du manuel puis valider assemble l'outil — parce que c'est le jeu du manuel")]
        public IEnumerator Rescue_AssemblingTheRightParts_Wins() => PlayToWin("rescue_tool", Rescue);

        static IEnumerator Rescue(MiniGame2DPopupView popup, IMiniGame game)
        {
            var g = (RescueEngineeringGame)game;
            foreach (var part in g.Solution.ToList())
            {
                g.Choose(part);
                yield return null;
            }
            g.Submit();
            yield return Until(g, 10);
        }

        [UnityTest, Timeout(240000), Description("Vérifie qu'« Infiltration » se gagne au joystick : pousser le doigt vers la sortie, sans garde en vue, atteint la sortie avant la fin du temps — parce que c'est le jeu du joystick virtuel")]
        public IEnumerator Infiltration_WalkingToTheExit_Wins() => PlayToWin("infiltration_camp", Infiltration, ("guards", 0));

        static IEnumerator Infiltration(MiniGame2DPopupView popup, IMiniGame game)
        {
            var g = (InfiltrationGame)game;
            // La sortie est dégagée sur 0,15 autour d'elle : on pose le joueur plein ouest et on pousse le joystick plein est.
            g.PlaceForTests(new Vec2f(g.Exit.X - 0.12f, g.Exit.Y));
            for (int i = 0; i < 600 && g.Status == GameStatus.Running; i++)
            {
                if (!popup.Input.Held) popup.Input.PressAt(new Vector2(0.5f, 0.5f));
                popup.Input.DragTo(new Vector2(0.5f + MiniGameField.StickRadius, 0.5f));
                yield return null;
            }
        }
    }
}
