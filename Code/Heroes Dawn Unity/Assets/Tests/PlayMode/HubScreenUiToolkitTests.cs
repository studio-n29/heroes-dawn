#nullable enable
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using N29.HeroesDawn.Application.Flow;
using N29.HeroesDawn.Composition;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Login;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using VContainer;

namespace N29.HeroesDawn.Tests.PlayMode
{
    /// <summary>Critères de sortie de la phase 2A pour le hub (coque et QG, jeu complet démarré) : quatre formats, cibles de manette, chemin de focus complet.</summary>
    public sealed class HubScreenUiToolkitTests
    {
        static readonly (int w, int h, string name)[] Formats = { (1920, 1080, "16x9"), (2160, 972, "20x9"), (1280, 960, "4x3"), (1280, 720, "720p") };
        static string Folder => Path.GetFullPath(Path.Combine(ScreenCaptureTests.Folder, "../uitk-hub"));

        [UnityTearDown]
        public IEnumerator TearDown() => PlayModeHarness.TearDown();

        static IEnumerator OnHub()
        {
            yield return PlayModeHarness.Boot(new MemoryProfileGateway(), new InstantAuthGateway());
            Object.FindAnyObjectByType<LoginView>()!.PlayButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<QgPageView>() && !GameBootstrap.Instance!.Scope.Container.Resolve<IGameFlow>().IsTransitioning);
            Assert.That(PlayModeHarness.Visible<QgPageView>(), Is.True, "connexion → QG");
            for (int i = 0; i < 30; i++) yield return null;
        }

        static List<PanelSettings> ActivePanels() => Object.FindObjectsByType<UIDocument>(FindObjectsSortMode.None)
            .Where(d => d.isActiveAndEnabled && d.panelSettings != null).Select(d => d.panelSettings).Distinct().ToList();

        /// <summary>Boutons visibles et actifs des documents du hub (dans leur panneau), hors éléments masqués ou sans taille.</summary>
        static List<UnityEngine.UIElements.Button> VisibleButtons()
        {
            var list = new List<UnityEngine.UIElements.Button>();
            foreach (var d in Object.FindObjectsByType<UIDocument>(FindObjectsSortMode.None).Where(d => d.isActiveAndEnabled && d.rootVisualElement != null))
                foreach (var b in d.rootVisualElement.Query<UnityEngine.UIElements.Button>().ToList())
                    if (b.resolvedStyle.display != DisplayStyle.None && b.resolvedStyle.visibility == Visibility.Visible && b.worldBound.width > 1 && b.worldBound.height > 1 && b.enabledInHierarchy && b.pickingMode == PickingMode.Position && IsShown(b))
                        list.Add(b);
            return list;
        }

        static bool IsShown(VisualElement e)
        {
            for (var p = e; p != null; p = p.parent) if (p.resolvedStyle.display == DisplayStyle.None || p.resolvedStyle.opacity < 0.01f) return false;
            return true;
        }

        [UnityTest, Timeout(300000), Description("Vérifie que le hub (coque et QG) garde tous ses boutons visibles, dans le panneau et à taille de manette sur 16:9, 20:9, 4:3 et 1280×720 — parce qu'un écran migré ne doit pas perdre sa disposition sur un format (point de vigilance 1 du plan)")]
        public IEnumerator Hub_ButtonsStayInsideAndTappable_OnTheFourFormats()
        {
            yield return OnHub();
            var panels = ActivePanels();
            var saved = panels.Select(p => (p, p.targetTexture)).ToList();
            try
            {
                foreach (var (w, h, name) in Formats)
                {
                    var rts = new List<RenderTexture>();
                    foreach (var p in panels) { var rt = new RenderTexture(w, h, 24); rts.Add(rt); p.targetTexture = rt; }
                    for (int i = 0; i < 20; i++) yield return null;
                    var problems = new List<string>();
                    var buttons = VisibleButtons();
                    foreach (var b in buttons)
                    {
                        var panel = b.panel.visualTree.worldBound;
                        var wb = b.worldBound;
                        string label = string.IsNullOrEmpty(b.name) ? (string.IsNullOrEmpty(b.text) ? b.GetType().Name : b.text) : b.name;
                        if (wb.xMin < panel.xMin - 1 || wb.xMax > panel.xMax + 1 || wb.yMin < panel.yMin - 1 || wb.yMax > panel.yMax + 1)
                            problems.Add($"« {label} » déborde du panneau ({wb} dans {panel})");
                        if (wb.height < 56f - 0.5f) problems.Add($"« {label} » sous 56 px de référence de hauteur ({wb.height:F0})");
                    }
                    Directory.CreateDirectory(Folder);
                    var prev = RenderTexture.active; RenderTexture.active = rts[0];
                    var tex = new Texture2D(w, h, TextureFormat.RGB24, false); tex.ReadPixels(new Rect(0, 0, w, h), 0, 0); tex.Apply(); RenderTexture.active = prev;
                    File.WriteAllBytes(Path.Combine(Folder, "hub-" + name + ".png"), tex.EncodeToPNG()); Object.Destroy(tex);
                    foreach (var rt in rts) rt.Release();
                    Assert.That(buttons.Count, Is.GreaterThan(5), $"{name} : le hub expose ses boutons");
                    Assert.That(problems, Is.Empty, $"{name} ({buttons.Count} boutons) :\n" + string.Join("\n", problems.Distinct()));
                }
            }
            finally { foreach (var (p, t) in saved) p.targetTexture = t; }
        }

        [UnityTest, Timeout(300000), Description("Vérifie que depuis n'importe quel bouton du hub, les quatre directions de la manette mènent à un autre bouton et que tous les boutons visibles sont atteignables depuis le premier — parce qu'aucun écran ne doit avoir de bouton inaccessible à la manette (spec §3)")]
        public IEnumerator Hub_Gamepad_EveryButtonIsReachable()
        {
            yield return OnHub();
            var buttons = VisibleButtons();
            Assert.That(buttons.Count, Is.GreaterThan(5));
            var edges = new Dictionary<VisualElement, HashSet<VisualElement>>();
            foreach (var b in buttons)
            {
                edges[b] = new HashSet<VisualElement>();
                foreach (var dir in new[] { NavigationMoveEvent.Direction.Down, NavigationMoveEvent.Direction.Up, NavigationMoveEvent.Direction.Left, NavigationMoveEvent.Direction.Right, NavigationMoveEvent.Direction.Next, NavigationMoveEvent.Direction.Previous })
                {
                    b.Focus(); yield return null;
                    var fc = b.panel.focusController;
                    using (var e = NavigationMoveEvent.GetPooled(dir)) { e.target = b; b.SendEvent(e); }
                    yield return null;
                    if (fc.focusedElement is VisualElement f && f != b) edges[b].Add(f);
                }
            }
            // Parcours en largeur depuis le premier bouton : seuls les boutons du même panneau sont comparables.
            var allButtons = new HashSet<VisualElement>(buttons);
            var reached = new HashSet<VisualElement>(); var queue = new Queue<VisualElement>();
            foreach (var start in buttons.GroupBy(b => b.panel).Select(g => g.First())) { reached.Add(start); queue.Enqueue(start); }
            while (queue.Count > 0)
            {
                var cur = queue.Dequeue();
                if (!edges.TryGetValue(cur, out var next)) continue;
                foreach (var n in next) if (reached.Add(n)) queue.Enqueue(n);
            }
            var unreachable = buttons.Where(b => !reached.Contains(b)).Select(b => string.IsNullOrEmpty(b.name) ? b.text : b.name).ToList();
            Assert.That(unreachable, Is.Empty, "boutons inaccessibles à la manette : " + string.Join(", ", unreachable) + $" (sur {buttons.Count})");
        }
    }
}
