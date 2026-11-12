#nullable enable
using System.Collections;
using System.IO;
using MessagePipe;
using N29.HeroesDawn.Application.Flow;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Presentation.Board;
using N29.HeroesDawn.Presentation.Combat;
using N29.HeroesDawn.Presentation.Narration;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using VContainer;

namespace N29.HeroesDawn.Tests.PlayMode
{
    /// <summary>Critères de sortie de la phase 2A pour le HUD du combat (UI Toolkit sur l'arène uGUI, combat de boss démarré) : quatre formats, cibles de manette, chemin de focus.</summary>
    public sealed class CombatScreenUiToolkitTests
    {
        static string Folder => Path.GetFullPath(Path.Combine(ScreenCaptureTests.Folder, "../uitk-combat"));

        [UnityTearDown]
        public IEnumerator TearDown() => PlayModeHarness.TearDown();

        static IEnumerator InCombat()
        {
            yield return PlayModeHarness.Boot(new MemoryProfileGateway(), new InstantAuthGateway());
            Object.FindAnyObjectByType<N29.HeroesDawn.Presentation.Login.LoginView>()!.PlayButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<N29.HeroesDawn.Presentation.Hub.QgPageView>() && !ActivityGamesPlayTests.Flow.IsTransitioning);
            var c = ActivityGamesPlayTests.Container;
            var session = c.Resolve<GameSession>();
            var zone = session.Content.Zone(PrototypeContent.Zone11);
            session.Run = ZoneRun.ForTests(session.Content, zone, 3UL, 3, true, "n22");
            session.RunZone = zone;
            c.Resolve<IPublisher<NavigateRequested>>().Publish(new NavigateRequested(GameState.Board));
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<BoardView>() && !ActivityGamesPlayTests.Flow.IsTransitioning);
            PlayModeHarness.TapNode(new CaseId("n23"));
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<DialogueBoxView>() != null || SceneManager.GetSceneByName(SceneNames.Combat).isLoaded, 900);
            var box = Object.FindAnyObjectByType<DialogueBoxView>();
            if (box != null) box.SkipButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => SceneManager.GetSceneByName(SceneNames.Combat).isLoaded && !ActivityGamesPlayTests.Flow.IsTransitioning, 900);
            var view = Object.FindAnyObjectByType<CombatView>()!;
            yield return PlayModeHarness.WaitFor(() => view.Hud.BossVisible, 300);
            yield return new WaitForSeconds(2.5f); // distribution des cartes et annonces terminées
        }

        /// <summary>Boîte englobante des éléments visibles donnés (un conteneur plus large que son contenu ne compte pas).</summary>
        static Rect? Union(System.Collections.Generic.IEnumerable<VisualElement> items)
        {
            Rect? r = null;
            foreach (var e in items)
            {
                if (e.resolvedStyle.display == DisplayStyle.None || e.worldBound.width < 2) continue;
                var b = e.worldBound;
                r = r == null ? b : Rect.MinMaxRect(Mathf.Min(r.Value.xMin, b.xMin), Mathf.Min(r.Value.yMin, b.yMin), Mathf.Max(r.Value.xMax, b.xMax), Mathf.Max(r.Value.yMax, b.yMax));
            }
            return r;
        }

        /// <summary>Contrôles de mise en page : colonnes d'unités, barre du boss, frise, journal et éventail ne se chevauchent pas et tiennent dans le panneau.</summary>
        static void LayoutChecks(string format, System.Collections.Generic.List<string> problems)
        {
            var hud = Object.FindAnyObjectByType<CombatHudView>()!.GetComponent<UIDocument>().rootVisualElement;
            var panel = hud.panel.visualTree.worldBound;
            var parts = new System.Collections.Generic.List<(string name, Rect box)>();
            void Add(string name)
            {
                var e = hud.Q<VisualElement>(name);
                if (e == null || e.resolvedStyle.display == DisplayStyle.None || e.worldBound.width < 2) return;
                parts.Add((name, e.worldBound));
            }
            foreach (var n in new[] { "allies", "enemies", "boss", "timeline" }) Add(n);
            var chips = Union(hud.Q<VisualElement>("bonuses").Children()); if (chips != null) parts.Add(("synergies", chips.Value));
            var log = hud.Q<Label>("log")?.parent; if (log != null && log.resolvedStyle.display != DisplayStyle.None) parts.Add(("journal", log.worldBound));
            var cards = Union(hud.Query<VisualElement>(className: "hd-skill").ToList()); if (cards != null) parts.Add(("éventail", cards.Value));
            foreach (var (name, box) in parts)
                if (box.xMin < panel.xMin - 1 || box.xMax > panel.xMax + 1 || box.yMin < panel.yMin - 1 || box.yMax > panel.yMax + 1)
                    problems.Add($"{format} : « {name} » sort du panneau ({box} dans {panel})");
            for (int i = 0; i < parts.Count; i++)
                for (int j = i + 1; j < parts.Count; j++)
                {
                    var a = parts[i].box; var b = parts[j].box;
                    // Tolérance de 2 px (bordures) ; le conteneur « bonuses » peut toucher l'éventail sans le recouvrir.
                    var inter = Rect.MinMaxRect(Mathf.Max(a.xMin, b.xMin), Mathf.Max(a.yMin, b.yMin), Mathf.Min(a.xMax, b.xMax), Mathf.Min(a.yMax, b.yMax));
                    if (inter.width > 2f && inter.height > 2f) problems.Add($"{format} : « {parts[i].name} » et « {parts[j].name} » se chevauchent ({inter.width:F0}×{inter.height:F0})");
                }
        }

        /// <summary>Boutons du HUD du combat (le document du HUD).</summary>
        static System.Func<UnityEngine.UIElements.Button, bool> InHud()
            => b => b.panel != null && Object.FindAnyObjectByType<CombatHudView>() is CombatHudView v && v.GetComponent<UIDocument>().rootVisualElement.Contains(b);

        [UnityTest, Timeout(400000), Description("Vérifie que le HUD du combat (retraite, AUTO, vitesse, pause) garde ses boutons dans le panneau et à taille de manette sur 16:9, 20:9, 4:3 et 1280×720 — parce qu'il se superpose à l'arène sur tous les formats")]
        public IEnumerator CombatHud_ButtonsStayInsideAndTappable_OnTheFourFormats()
        {
            yield return InCombat();
            yield return UiToolkitAcceptance.CheckFormats(Folder, "combat", InHud(), 2, LayoutChecks);
        }

        [UnityTest, Timeout(400000), Description("Vérifie que tous les boutons du HUD du combat sont atteignables à la manette — parce que la retraite, AUTO et la vitesse doivent se jouer à la croix directionnelle")]
        public IEnumerator CombatHud_Gamepad_EveryButtonIsReachable()
        {
            yield return InCombat();
            yield return UiToolkitAcceptance.CheckGamepadReach("combat", InHud(), 2);
        }
    }
}
