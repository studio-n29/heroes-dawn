#nullable enable
using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace N29.HeroesDawn.Tests.PlayMode
{
    /// <summary>
    /// Prototype UI Toolkit de l'écran titre : rendu aux formats cibles (PC 16:9, mobile 20:9, Switch, 4:3) et navigation manette.
    /// Sert à trancher UI Toolkit / uGUI sur des mesures. Explicite : lancé à la demande.
    /// </summary>
    public sealed class UiToolkitSpikeTests
    {
        const string Uxml = "Assets/_Project/UIToolkit/Spike/TitleSpike.uxml";

        static string Folder => Path.GetFullPath(Path.Combine(ScreenCaptureTests.Folder, "../uitk-spike"));

        static (UIDocument doc, RenderTexture rt, GameObject go) Build(int w, int h)
        {
            var rt = new RenderTexture(w, h, 24);
            var settings = ScriptableObject.CreateInstance<PanelSettings>();
            settings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            settings.referenceResolution = new Vector2Int(1920, 1080);
            settings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            settings.match = 0.5f;
            settings.targetTexture = rt;
            var go = new GameObject("UiToolkitSpike");
            var doc = go.AddComponent<UIDocument>();
            doc.panelSettings = settings;
#if UNITY_EDITOR
            doc.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Uxml);
#endif
            return (doc, rt, go);
        }

        /// <summary>Le conteneur de modèle du document doit remplir le panneau (point de passage commun de toutes les vues du jeu).</summary>
        static void Fill(UIDocument doc)
        {
            var template = doc.rootVisualElement.Q<TemplateContainer>();
            template.style.flexGrow = 1;
            template.style.width = Length.Percent(100);
            template.style.height = Length.Percent(100);
        }

        static void Save(RenderTexture rt, string name)
        {
            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();
            RenderTexture.active = prev;
            Directory.CreateDirectory(Folder);
            File.WriteAllBytes(Path.Combine(Folder, name + ".png"), tex.EncodeToPNG());
            Object.Destroy(tex);
        }

        [UnityTest, Explicit("Prototype UI Toolkit"), Timeout(120000), Description("Vérifie que le prototype UI Toolkit de l'écran titre se rend aux formats cibles et que la manette déplace le focus puis valide — parce que la décision UI Toolkit repose sur ces mesures")]
        public IEnumerator Title_RendersAtTheTargetFormats()
        {
            foreach (var (w, h, name) in new[] { (1920, 1080, "title-1920x1080"), (2160, 972, "title-2160x972-mobile"), (1280, 720, "title-1280x720-switch"), (1280, 960, "title-1280x960-4x3") })
            {
                var (doc, rt, go) = Build(w, h);
                for (int i = 0; i < 4; i++) yield return null;
                Fill(doc);
                for (int i = 0; i < 8; i++) yield return null;
                Assert.That(doc.rootVisualElement, Is.Not.Null);
                Assert.That(doc.rootVisualElement.Q<Button>("play"), Is.Not.Null, "bouton Jouer");
                Save(rt, name);
                Object.Destroy(go);
                rt.Release();
                yield return null;
            }
        }

        [UnityTest, Explicit("Prototype UI Toolkit"), Timeout(120000), Description("Vérifie que le prototype UI Toolkit de l'écran titre se rend aux formats cibles et que la manette déplace le focus puis valide — parce que la décision UI Toolkit repose sur ces mesures")]
        public IEnumerator Title_GamepadNavigation_MovesFocusAndSubmits()
        {
            var (doc, rt, go) = Build(1920, 1080);
            for (int i = 0; i < 4; i++) yield return null;
            Fill(doc);
            for (int i = 0; i < 8; i++) yield return null;
            var root = doc.rootVisualElement;
            var play = root.Q<Button>("play");
            var connect = root.Q<Button>("connect");
            int clicks = 0;
            play.clicked += () => clicks++;
            play.Focus();
            yield return null;
            Assert.That(root.focusController.focusedElement, Is.SameAs(play), "focus initial sur Jouer");
            using (var down = NavigationMoveEvent.GetPooled(NavigationMoveEvent.Direction.Down)) { down.target = root.focusController.focusedElement; root.focusController.focusedElement.SendEvent(down); }
            yield return null;
            Assert.That(root.focusController.focusedElement, Is.SameAs(connect), "la direction Bas de la manette déplace le focus");
            using (var up = NavigationMoveEvent.GetPooled(NavigationMoveEvent.Direction.Up)) { up.target = root.focusController.focusedElement; root.focusController.focusedElement.SendEvent(up); }
            yield return null;
            Assert.That(root.focusController.focusedElement, Is.SameAs(play), "la direction Haut revient sur Jouer");
            using (var submit = NavigationSubmitEvent.GetPooled()) { submit.target = play; play.SendEvent(submit); }
            yield return null;
            Assert.That(clicks, Is.EqualTo(1), "Valider (A) déclenche le bouton");
            Save(rt, "title-focus-play");
            Object.Destroy(go);
            rt.Release();
        }
    }
}
