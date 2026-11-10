#nullable enable
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Presentation.UiToolkit;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Tests.PlayMode
{
    /// <summary>Lot 0 de la refonte UI Toolkit : thème, vue de base, composants (page « Composants »), formats, manette, débordement, coût.</summary>
    public sealed class UiToolkitFoundationTests
    {
        const string Gallery = "Assets/_Project/UIToolkit/Screens/Gallery/Gallery.uxml";

        [UnityTest, Description("Vérifie que la page Composants se lie dès l'activation de la vue et se rend aux cinq formats cibles — parce que toutes les vues reposent sur ce socle et que la fidélité se juge sur captures")]
        public IEnumerator Gallery_BindsOnEnable_AndRendersAtEveryFormat()
        {
            foreach (var (w, h, name) in UiToolkitHarness.Formats)
            {
                var rig = UiToolkitHarness.Create<GalleryView>(Gallery, w, h);
                var view = rig.View<GalleryView>();
                Assert.That(view.Primary, Is.Not.Null, "la vue lie ses éléments pendant OnEnable (" + name + ")");
                Assert.That(view.IsVisible, Is.True);
                yield return UiToolkitHarness.Settle();
                Assert.That(rig.Root.Q<VisualElement>("screen").worldBound.width, Is.GreaterThan(w * 0.9f).Or.GreaterThan(1000f), "le modèle remplit le panneau");
                UiToolkitHarness.Save(rig, "composants", "composants-" + name);
                UiToolkitHarness.Destroy(rig);
                yield return null;
            }
        }

        [UnityTest, Description("Vérifie que la manette atteint chaque composant actionnable de la page (direction, valider) et que le bouton désactivé est sauté — parce qu'aucun écran ne doit piéger le focus")]
        public IEnumerator Gallery_GamepadNavigation_ReachesEveryFocusable()
        {
            HdInput.Set(HdInputMode.Gamepad);
            var rig = UiToolkitHarness.Create<GalleryView>(Gallery, 1920, 1080);
            yield return UiToolkitHarness.Settle();
            var primary = rig.Root.Q<Button>("primary");
            int clicks = 0;
            primary.clicked += () => clicks++;
            primary.Focus();
            yield return null;
            Assert.That(rig.Root.focusController.focusedElement, Is.SameAs(primary));
            // Parcours en largeur du graphe de navigation spatiale : depuis chaque élément atteint, on essaie les quatre directions.
            var visited = new HashSet<VisualElement> { primary };
            var queue = new Queue<VisualElement>();
            queue.Enqueue(primary);
            while (queue.Count > 0)
            {
                var from = queue.Dequeue();
                foreach (var direction in new[] { NavigationMoveEvent.Direction.Right, NavigationMoveEvent.Direction.Left, NavigationMoveEvent.Direction.Down, NavigationMoveEvent.Direction.Up })
                {
                    from.Focus();
                    yield return null;
                    UiToolkitHarness.Move(rig, direction);
                    yield return null;
                    if (rig.Root.focusController.focusedElement is VisualElement next && visited.Add(next)) queue.Enqueue(next);
                }
            }
            var names = visited.Select(v => string.IsNullOrEmpty(v.name) ? v.GetType().Name : v.name).ToList();
            foreach (var expected in new[] { "secondary", "ghost", "danger", "nav", "tab-a", "tab-b", "tab-c", "chip", "toggle" })
                Assert.That(names, Does.Contain(expected), "la manette atteint " + expected + " ; atteints : " + string.Join(", ", names));
            Assert.That(names, Does.Not.Contain("primary-off"), "un bouton désactivé n'a jamais le focus");
            primary.Focus();
            yield return null;
            UiToolkitHarness.Submit(rig);
            yield return null;
            Assert.That(clicks, Is.EqualTo(1), "Valider (A) déclenche le bouton focalisé");
            UiToolkitHarness.Destroy(rig);
            HdInput.Set(HdInput.DefaultMode());
        }

        [UnityTest, Description("Vérifie qu'aucun texte de la page ne déborde de son panneau ni de l'écran aux cinq formats — parce que la lisibilité passe avant la fidélité et que les textes français et anglais varient en longueur")]
        public IEnumerator Gallery_Texts_NeverOverflow()
        {
            foreach (var (w, h, name) in UiToolkitHarness.Formats)
            {
                var rig = UiToolkitHarness.Create<GalleryView>(Gallery, w, h);
                yield return UiToolkitHarness.Settle();
                var view = rig.View<GalleryView>();
                var panel = view.PanelNavy.worldBound;
                var text = view.LongText.worldBound;
                Assert.That(text.yMax, Is.LessThanOrEqualTo(panel.yMax + 1f), "le texte long reste dans son panneau (" + name + ")");
                Assert.That(text.xMax, Is.LessThanOrEqualTo(panel.xMax + 1f), "pas de débordement à droite (" + name + ")");
                var screen = rig.Root.worldBound;
                foreach (var label in rig.Root.Query<Label>().ToList())
                {
                    var b = label.worldBound;
                    if (b.width <= 0f) continue;
                    Assert.That(b.xMax, Is.LessThanOrEqualTo(screen.xMax + 2f), "« " + label.text + " » dépasse l'écran à droite (" + name + ")");
                    Assert.That(b.yMax, Is.LessThanOrEqualTo(screen.yMax + 2f), "« " + label.text + " » dépasse l'écran en bas (" + name + ")");
                }
                UiToolkitHarness.Destroy(rig);
                yield return null;
            }
        }

        [UnityTest, Description("Mesure indicative (éditeur) du coût d'image de la page Composants — le seuil protège d'une dérive, il ne remplace pas la mesure sur Android")]
        public IEnumerator Gallery_FrameCost_IsReasonable()
        {
            var rig = UiToolkitHarness.Create<GalleryView>(Gallery, 1920, 1080);
            yield return UiToolkitHarness.Settle(20);
            long before = System.GC.GetTotalMemory(false);
            float total = 0f; int frames = 90;
            for (int i = 0; i < frames; i++) { total += Time.unscaledDeltaTime; yield return null; }
            long allocated = System.GC.GetTotalMemory(false) - before;
            float ms = total / frames * 1000f;
            var count = rig.Root.Query<VisualElement>().ToList().Count;
            Debug.Log($"[Mesure UI Toolkit] page Composants : {count} éléments, {ms:F1} ms par image (éditeur, indicatif), variation mémoire gérée {allocated / 1024} Ko sur {frames} images");
            Assert.That(ms, Is.LessThan(100f), "coût d'image de la page Composants");
            UiToolkitHarness.Destroy(rig);
        }
    }
}
