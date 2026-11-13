#nullable enable
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using N29.HeroesDawn.Presentation.Mvp;
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
    /// Critère « 4 formats » de la phase 2 sans capture Explicit : chaque prefab UI Toolkit du catalogue (connexion, hub, pages, fenêtres,
    /// notifications, tutoriel) est instancié sur 16:9, 20:9, 4:3 et 1280×720 ; <see cref="UiLayoutChecker"/> échoue sur tout débordement,
    /// chevauchement, bouton trop bas, texte écrasé ou « Aube tactique » affiché.
    /// </summary>
    public sealed class AllWindowsFourFormatsTests
    {
        static readonly (int w, int h, string name)[] Formats = { (1920, 1080, "16x9"), (2160, 972, "20x9"), (1280, 960, "4x3"), (1280, 720, "720p") };
        static string Folder => Path.GetFullPath(Path.Combine(ScreenCaptureTests.Folder, "../uitk-all"));

#if UNITY_EDITOR
        /// <summary>Tous les prefabs du catalogue qui portent un UIDocument (champs privés lus par sérialisation).</summary>
        internal static List<GameObject> CataloguePrefabs()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<ScreenCatalog>("Assets/_Project/Settings/ScreenCatalog.asset");
            var so = new SerializedObject(catalog);
            var list = new List<GameObject>();
            for (var it = so.GetIterator(); it.Next(true);)
                if (it.propertyType == SerializedPropertyType.ObjectReference && it.objectReferenceValue is GameObject g && !list.Contains(g)) list.Add(g);
            return list.Where(g => g.GetComponent<UIDocument>() != null).OrderBy(g => g.name).ToList();
        }
#endif

        static GameObject Spawn(GameObject prefab, int w, int h, out RenderTexture rt, out PanelSettings settings)
        {
            rt = new RenderTexture(w, h, 24);
            settings = Object.Instantiate(Resources.Load<PanelSettings>("UiToolkit/HdPanelSettings"));
            settings.targetTexture = rt;
            var holder = new GameObject("Holder_" + prefab.name, typeof(RectTransform));
            holder.SetActive(false);
            var go = Object.Instantiate(prefab, holder.transform);
            go.GetComponent<UIDocument>().panelSettings = settings;
            holder.SetActive(true);
            return holder;
        }

        /// <summary>Attend que la mise en page cesse de bouger (polices et champs multilignes se mesurent sur plusieurs images) : au moins 12 images, puis 4 images identiques de suite.</summary>
        static IEnumerator SettleLayout(GameObject holder)
        {
            for (int i = 0; i < 12; i++) yield return null;
            var doc = holder.GetComponentInChildren<UIDocument>();
            string last = ""; int same = 0;
            for (int i = 0; i < 90 && same < 4; i++)
            {
                var sig = string.Concat(UiLayoutChecker.Controls(doc.rootVisualElement).Select(c => c.worldBound.ToString()));
                same = sig == last ? same + 1 : 0; last = sig;
                yield return null;
            }
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

#if UNITY_EDITOR
        [UnityTest, Timeout(900000), Description("Vérifie que chaque écran, page et fenêtre UI Toolkit du catalogue tient dans le panneau, sans chevauchement de contrôles, avec des boutons d'au moins 56 px et sans « Aube tactique », sur 16:9, 20:9, 4:3 et 1280×720 — parce que le critère « 4 formats » ne doit plus reposer sur des captures lancées à la main")]
        public IEnumerator EveryCataloguePrefab_FitsTheFourFormats()
        {
            var prefabs = CataloguePrefabs();
            Assert.That(prefabs.Count, Is.GreaterThanOrEqualTo(20), "le catalogue expose ses prefabs UI Toolkit : " + string.Join(", ", prefabs.Select(p => p.name)));
            var all = new List<string>();
            foreach (var prefab in prefabs)
                foreach (var (w, h, name) in Formats)
                {
                    var holder = Spawn(prefab, w, h, out var rt, out var settings);
                    yield return SettleLayout(holder);
                    var doc = holder.GetComponentInChildren<UIDocument>();
                    all.AddRange(UiLayoutChecker.Analyze(doc.rootVisualElement, $"{prefab.name} {name}"));
                    if (name == "16x9" || name == "4x3") Save(rt, $"{prefab.name}-{name}");
                    Object.Destroy(holder); Object.Destroy(settings); rt.Release();
                    yield return null;
                }
            Assert.That(all, Is.Empty, $"{prefabs.Count} prefabs × 4 formats :\n" + string.Join("\n", all));
        }
#endif

        [UnityTest, Timeout(120000), Description("Vérifie que le contrôleur de mise en page échoue réellement sur un bouton qui déborde, deux boutons qui se chevauchent, un bouton trop bas et un texte « Aube tactique », et qu'il laisse passer une disposition correcte — parce qu'un test de formats qui ne peut pas échouer ne prouve rien")]
        public IEnumerator Checker_ReallyFailsOnOverflowOverlapSmallButtonAndForbiddenText()
        {
            var rt = new RenderTexture(1280, 720, 24);
            var settings = Object.Instantiate(Resources.Load<PanelSettings>("UiToolkit/HdPanelSettings"));
            settings.targetTexture = rt;
            var go = new GameObject("Checker", typeof(RectTransform));
            var doc = go.AddComponent<UIDocument>(); doc.panelSettings = settings;
            for (int i = 0; i < 3; i++) yield return null;
            var root = doc.rootVisualElement;
            void Box(float x, float y, float w, float h, string name)
            {
                var b = new Button { name = name, text = name };
                b.style.position = Position.Absolute; b.style.left = x; b.style.top = y; b.style.width = w; b.style.height = h;
                root.Add(b);
            }
            Box(100, 100, 300, 80, "ok1"); Box(100, 300, 300, 80, "ok2");
            for (int i = 0; i < 3; i++) yield return null;
            Assert.That(UiLayoutChecker.Analyze(root, "sain"), Is.Empty, "une disposition correcte passe");

            Box(1800, 100, 400, 80, "deborde"); Box(150, 120, 300, 80, "recouvre"); Box(600, 500, 300, 20, "trop_bas");
            var l = new Label("Aube tactique"); l.style.position = Position.Absolute; l.style.left = 600; l.style.top = 50; root.Add(l);
            for (int i = 0; i < 3; i++) yield return null;
            var problems = UiLayoutChecker.Analyze(root, "casse");
            var text = string.Join("\n", problems);
            Assert.That(problems.Any(p => p.Contains("déborde du panneau")), Is.True, text);
            Assert.That(problems.Any(p => p.Contains("se chevauchent")), Is.True, text);
            Assert.That(problems.Any(p => p.Contains("sous 56 px")), Is.True, text);
            Assert.That(problems.Any(p => p.Contains("Aube tactique")), Is.True, text);
            Object.Destroy(go); Object.Destroy(settings); rt.Release();
        }
    }
}
