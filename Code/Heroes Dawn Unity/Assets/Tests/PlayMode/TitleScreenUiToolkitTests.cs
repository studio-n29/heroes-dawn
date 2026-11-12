#nullable enable
using System.Collections;
using System.IO;
using NUnit.Framework;
using N29.HeroesDawn.Presentation.Login;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace N29.HeroesDawn.Tests.PlayMode
{
    /// <summary>Critères de sortie de la phase 2A pour l'écran titre (vue de production, UI Toolkit) : quatre formats, manette, cibles et textes lisibles.</summary>
    public sealed class TitleScreenUiToolkitTests
    {
        static readonly (int w, int h, string name)[] Formats =
        {
            (1920, 1080, "16x9"), (2160, 972, "20x9"), (1280, 960, "4x3"), (1280, 720, "720p"),
        };

        static string Folder => Path.GetFullPath(Path.Combine(ScreenCaptureTests.Folder, "../uitk-title"));

        static (LoginView view, RenderTexture rt, GameObject go) Spawn(int w, int h)
        {
            var rt = new RenderTexture(w, h, 24);
            var settings = Object.Instantiate(Resources.Load<PanelSettings>("UiToolkit/HdPanelSettings"));
            settings.targetTexture = rt;
            var go = new GameObject("TitleUnderTest", typeof(RectTransform));
            go.SetActive(false);
            var doc = go.AddComponent<UIDocument>();
            doc.panelSettings = settings;
#if UNITY_EDITOR
            doc.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/_Project/UIToolkit/Screens/Login/Login.uxml");
#endif
            var view = go.AddComponent<LoginView>();
            go.SetActive(true);
            return (view, rt, go);
        }

        static void Save(RenderTexture rt, string name)
        {
            var prev = RenderTexture.active; RenderTexture.active = rt;
            var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0); tex.Apply();
            RenderTexture.active = prev;
            Directory.CreateDirectory(Folder);
            File.WriteAllBytes(Path.Combine(Folder, name + ".png"), tex.EncodeToPNG());
            Object.Destroy(tex);
        }

        [UnityTest, Timeout(120000), Description("Vérifie que l'écran titre garde Jouer et Se connecter entièrement visibles, dans l'ordre et à taille tactile sur 16:9, 20:9, 4:3 et 1280×720 — parce qu'une mise en page qui déborde sur un format est une régression d'interface (point de vigilance 1 du plan)")]
        public IEnumerator Title_KeepsActionsVisibleAndTappable_OnTheFourFormats()
        {
            foreach (var (w, h, name) in Formats)
            {
                var (view, rt, go) = Spawn(w, h);
                for (int i = 0; i < 12; i++) yield return null;
                view.PlayIntro(); yield return new WaitForSeconds(0.9f); // le logotype apparaît en fondu (0,5 s)
                Assert.That(view.Root.Q<VisualElement>("lockup").resolvedStyle.opacity, Is.GreaterThan(0.99f), $"{name} : logotype visible après l'animation d'arrivée");
                var panel = view.Root.panel.visualTree.worldBound;
                var play = view.PlayButton.Element; var account = view.AccountButton.Element;
                foreach (var (label, e) in new[] { ("Jouer", play), ("Se connecter", account) })
                {
                    var b = e.worldBound;
                    Assert.That(b.xMin, Is.GreaterThanOrEqualTo(panel.xMin - 0.5f), $"{name} : {label} déborde à gauche");
                    Assert.That(b.xMax, Is.LessThanOrEqualTo(panel.xMax + 0.5f), $"{name} : {label} déborde à droite");
                    Assert.That(b.yMin, Is.GreaterThanOrEqualTo(panel.yMin - 0.5f), $"{name} : {label} déborde en haut");
                    Assert.That(b.yMax, Is.LessThanOrEqualTo(panel.yMax + 0.5f), $"{name} : {label} déborde en bas");
                    // Cible minimale console : 56 px à la résolution de référence 1080p, soit 56/1080 de la hauteur du panneau.
                    Assert.That(b.height, Is.GreaterThanOrEqualTo(panel.height * 56f / 1080f), $"{name} : {label} trop petit pour une manette");
                    Assert.That(e.resolvedStyle.fontSize, Is.GreaterThanOrEqualTo(24f), $"{name} : texte de {label} sous 24 px de référence");
                }
                Assert.That(play.worldBound.yMax, Is.LessThanOrEqualTo(account.worldBound.yMin + 0.5f), $"{name} : Jouer au-dessus de Se connecter");
                Assert.That(play.worldBound.Overlaps(account.worldBound), Is.False, $"{name} : les boutons se chevauchent");
                Save(rt, "title-" + name);
                Object.Destroy(go); rt.Release();
                yield return null;
            }
        }

        [UnityTest, Timeout(120000), Description("Vérifie que sur la vue de production la manette a le focus sur Jouer, descend vers Se connecter, remonte et que Valider déclenche Jouer — parce que la navigation manette est une exigence de la refonte")]
        public IEnumerator Title_Gamepad_FocusMovesAndSubmitTriggersPlay()
        {
            var (view, rt, go) = Spawn(1920, 1080);
            for (int i = 0; i < 12; i++) yield return null;
            int played = 0; view.PlayClicked += () => played++;
            var root = view.Root;
            var fc = root.panel.focusController;
            Assert.That(fc.focusedElement, Is.SameAs(view.PlayButton.Element), "focus initial sur Jouer");
            Send(fc, NavigationMoveEvent.Direction.Down); yield return null;
            Assert.That(fc.focusedElement, Is.SameAs(view.AccountButton.Element), "Bas → Se connecter");
            Send(fc, NavigationMoveEvent.Direction.Up); yield return null;
            Assert.That(fc.focusedElement, Is.SameAs(view.PlayButton.Element), "Haut → Jouer");
            using (var submit = NavigationSubmitEvent.GetPooled()) { submit.target = fc.focusedElement; fc.focusedElement.SendEvent(submit); }
            yield return null;
            Assert.That(played, Is.EqualTo(1), "Valider déclenche Jouer");
            Object.Destroy(go); rt.Release();
        }

        static void Send(FocusController fc, NavigationMoveEvent.Direction d)
        {
            using var e = NavigationMoveEvent.GetPooled(d);
            e.target = fc.focusedElement; fc.focusedElement.SendEvent(e);
        }
    }
}
