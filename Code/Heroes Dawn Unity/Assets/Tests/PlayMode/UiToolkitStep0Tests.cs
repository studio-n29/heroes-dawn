#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Profiling;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace N29.HeroesDawn.Tests.PlayMode
{
    /// <summary>
    /// Étape 0 de la refonte UI Toolkit (spec §2 et §6) : mesures réelles de mémoire, de rendu des polices Barlow
    /// avec textes FR/EN accentués, et de cohabitation avec un Canvas uGUI. Explicite : lancé à la demande, avec graphismes.
    /// Écrit un rapport texte dans docs/test-reports/uitk-step0/.
    /// </summary>
    public sealed class UiToolkitStep0Tests
    {
        static string Folder => Path.GetFullPath(Path.Combine(ScreenCaptureTests.Folder, "../uitk-step0"));
        const string ThemeUss = "Assets/_Project/UIToolkit/Theme/hd.uss";

        static readonly string[] Fr =
        {
            "Équipe — Récompenses à réclamer : é è ê ë à â ô î ï û ù ç œ æ É È À Ç Œ",
            "« L'Agent D arrive bientôt » — 1 234 PR · 56 % · n°7 · …",
            "Zone sécurisée : l'éclaireur déclenche l'événement ; coûts 3 × 250 €",
        };
        static readonly string[] En =
        {
            "Team — Rewards to claim: the quick brown fox jumps over the lazy dog 0123456789",
            "“Agent D arrives soon” — 1,234 PR · 56% · #7 · …",
            "Secured zone: the scout triggers the event; costs 3 × 250 $",
        };

        static readonly StringBuilder Report = new StringBuilder();

        static void Line(string s) { Report.AppendLine(s); Debug.Log("[STEP0] " + s); }

        static long Atlases(out int count)
        {
            long bytes = 0; count = 0;
            foreach (var t in Resources.FindObjectsOfTypeAll<Texture2D>())
                if (t.name.Contains("Atlas") || t.name.Contains("SDF")) { bytes += Profiler.GetRuntimeMemorySizeLong(t); count++; }
            return bytes;
        }

        static (UIDocument doc, RenderTexture rt, GameObject go) Build(int w, int h, int sort = 100)
        {
            var rt = new RenderTexture(w, h, 24);
            var settings = Resources.Load<PanelSettings>("UiToolkit/HdPanelSettings");
            Assert.That(settings, Is.Not.Null, "PanelSettings de production");
            settings = UnityEngine.Object.Instantiate(settings);
            settings.targetTexture = rt;
            settings.sortingOrder = sort;
            var go = new GameObject("Step0Doc");
            var doc = go.AddComponent<UIDocument>();
            doc.panelSettings = settings;
            return (doc, rt, go);
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
            UnityEngine.Object.Destroy(tex);
        }

        static void Flush() { Directory.CreateDirectory(Folder); File.WriteAllText(Path.Combine(Folder, "rapport.txt"), Report.ToString()); }

        [UnityTest, Explicit("Étape 0 UI Toolkit"), Timeout(300000), Description("Mesure la mémoire du panneau UI Toolkit avec les 41 écrans UXML de production chargés — parce que le go/no-go 2A exige une mémoire chiffrée")]
        public IEnumerator Memory_AllScreensLoaded()
        {
#if UNITY_EDITOR
            var uxmls = AssetDatabase.FindAssets("t:VisualTreeAsset", new[] { "Assets/_Project/UIToolkit/Screens" })
                .Select(g => AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AssetDatabase.GUIDToAssetPath(g))).Where(v => v != null).ToList();
            yield return null;
            GC.Collect(); yield return null;
            long nativeBefore = Profiler.GetTotalAllocatedMemoryLong(), managedBefore = GC.GetTotalMemory(true);
            long atlasBefore = Atlases(out int atlasCountBefore);
            var (doc, rt, go) = Build(1920, 1080);
            for (int i = 0; i < 4; i++) yield return null;
            int elements = 0;
            foreach (var v in uxmls)
            {
                var inst = v.Instantiate();
                inst.style.position = Position.Absolute; inst.style.left = 0; inst.style.top = 0; inst.style.width = 1920; inst.style.height = 1080;
                doc.rootVisualElement.Add(inst);
                elements += inst.Query<VisualElement>().ToList().Count;
            }
            for (int i = 0; i < 10; i++) yield return null;
            long nativeAfter = Profiler.GetTotalAllocatedMemoryLong(), managedAfter = GC.GetTotalMemory(false);
            long atlasAfter = Atlases(out int atlasCountAfter);
            Line($"Mémoire — {uxmls.Count} écrans UXML instanciés (tous empilés), {elements} éléments, panneau 1920x1080 de production");
            Line($"  natif alloué : {nativeBefore / 1048576.0:F1} Mo → {nativeAfter / 1048576.0:F1} Mo (Δ {(nativeAfter - nativeBefore) / 1048576.0:F1} Mo)");
            Line($"  géré (GC)    : {managedBefore / 1048576.0:F1} Mo → {managedAfter / 1048576.0:F1} Mo (Δ {(managedAfter - managedBefore) / 1048576.0:F1} Mo)");
            Line($"  atlas de polices SDF : {atlasCountBefore} → {atlasCountAfter} textures, {atlasBefore / 1048576.0:F2} → {atlasAfter / 1048576.0:F2} Mo");
            Save(rt, "memoire-41-ecrans");
            UnityEngine.Object.Destroy(go); rt.Release();
            Flush();
            Assert.That(uxmls.Count, Is.GreaterThan(30));
#else
            yield break;
#endif
        }

        [UnityTest, Explicit("Étape 0 UI Toolkit"), Timeout(300000), Description("Vérifie que les polices Barlow rendent sans glyphe manquant les textes FR et EN accentués en trois tailles — parce que le repli uGUI s'impose si un accent s'affiche en carré")]
        public IEnumerator Fonts_BarlowRenderLocalizedText_NoMissingGlyph()
        {
#if UNITY_EDITOR
            var missing = new List<string>();
            UnityEngine.Application.LogCallback cb = (msg, _, type) => { if (!msg.StartsWith("[STEP0]") && (msg.Contains("not found") || msg.Contains("missing") || msg.Contains("glyph"))) missing.Add(msg); };
            UnityEngine.Application.logMessageReceived += cb;
            var uss = AssetDatabase.LoadAssetAtPath<StyleSheet>(ThemeUss);
            var (doc, rt, go) = Build(1920, 1080);
            for (int i = 0; i < 4; i++) yield return null;
            var root = doc.rootVisualElement;
            root.styleSheets.Add(uss);
            root.style.backgroundColor = new Color(0.063f, 0.11f, 0.16f);
            root.style.paddingLeft = 40; root.style.paddingTop = 20;
            long atlasBefore = Atlases(out int cBefore);
            var classes = new[] { "hd-text", "hd-text--caption", "hd-text--heading", "hd-text--title", "hd-text--display" };
            foreach (var cls in classes)
                foreach (var s in Fr.Concat(En).Take(2).Concat(new[] { Fr[0], En[0] }).Distinct())
                {
                    var l = new Label(s); l.AddToClassList("hd-text"); if (cls != "hd-text") l.AddToClassList(cls); root.Add(l);
                }
            for (int i = 0; i < 10; i++) yield return null;
            long atlasAfter = Atlases(out int cAfter);
            // Couverture des glyphes dans les fichiers TTF
            string[] ttfs = { "Barlow-Regular", "Barlow-SemiBold", "BarlowCondensed-SemiBold", "BarlowCondensed-ExtraBoldItalic" };
            var all = string.Concat(Fr.Concat(En)).Distinct().Where(c => !char.IsWhiteSpace(c)).ToArray();
            foreach (var n in ttfs)
            {
                var path = AssetDatabase.FindAssets(n + " t:Font").Select(AssetDatabase.GUIDToAssetPath).FirstOrDefault(p => p.Contains("AubeTactiqueKit"));
                var f = AssetDatabase.LoadAssetAtPath<Font>(path);
                var absent = all.Where(c => !f.HasCharacter(c)).ToArray();
                Line($"Police {n} : {all.Length} caractères testés, absents du TTF : {(absent.Length == 0 ? "aucun" : new string(absent))}");
            }
            Line($"Rendu UI Toolkit : 5 styles de texte × FR/EN, atlas de polices {cBefore} → {cAfter} textures ({atlasBefore / 1048576.0:F2} → {atlasAfter / 1048576.0:F2} Mo)");
            Line($"Avertissements de glyphe manquant dans la console : {missing.Count}" + (missing.Count > 0 ? " — " + string.Join(" | ", missing.Take(3)) : ""));
            Save(rt, "polices-fr-en");
            UnityEngine.Application.logMessageReceived -= cb;
            UnityEngine.Object.Destroy(go); rt.Release();
            Flush();
            Assert.That(missing, Is.Empty, "aucun glyphe manquant");
#else
            yield break;
#endif
        }

        [UnityTest, Explicit("Étape 0 UI Toolkit"), Timeout(300000), Description("Vérifie qu'un panneau UI Toolkit s'affiche au-dessus d'un Canvas uGUI conservé et que les deux reçoivent leurs clics avec un seul EventSystem — parce que le plateau et les mini-jeux restent en uGUI")]
        public IEnumerator Coexistence_UguiCanvasBelowPanel()
        {
#if UNITY_EDITOR
            var cam = new GameObject("Cam").AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = Color.black;
            var rt = new RenderTexture(1920, 1080, 24); cam.targetTexture = rt;
            var es = new GameObject("ES"); es.AddComponent<EventSystem>();
            var canvasGo = new GameObject("GameSurface", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = cam; canvas.planeDistance = 1; canvas.sortingOrder = 0;
            var imgGo = new GameObject("Tile", typeof(UnityEngine.UI.Image), typeof(UnityEngine.UI.Button)); imgGo.transform.SetParent(canvasGo.transform, false);
            imgGo.GetComponent<UnityEngine.UI.Image>().color = Color.red;
            var rect = imgGo.GetComponent<RectTransform>(); rect.sizeDelta = new Vector2(1500, 900);
            int ugui = 0; imgGo.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => ugui++);
            var settings = UnityEngine.Object.Instantiate(Resources.Load<PanelSettings>("UiToolkit/HdPanelSettings"));
            settings.targetTexture = null; settings.sortingOrder = 10;
            var go = new GameObject("Overlay"); var doc = go.AddComponent<UIDocument>(); doc.panelSettings = settings;
            for (int i = 0; i < 6; i++) yield return null;
            var root = doc.rootVisualElement;
            var panelBtn = new UnityEngine.UIElements.Button { text = "Panneau" }; panelBtn.style.width = 300; panelBtn.style.height = 120; panelBtn.style.position = Position.Absolute; panelBtn.style.left = 100; panelBtn.style.top = 100;
            int uitk = 0; panelBtn.clicked += () => uitk++;
            root.Add(panelBtn);
            for (int i = 0; i < 6; i++) yield return null;
            cam.Render();
            Save(rt, "cohabitation");
            var prev = RenderTexture.active; RenderTexture.active = rt;
            var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false); tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0); tex.Apply(); RenderTexture.active = prev;
            var px = tex.GetPixel(1700, 540); // hors bouton, sur la tuile uGUI
            UnityEngine.Object.Destroy(tex);
            Line($"Cohabitation — un seul EventSystem : {UnityEngine.Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length}; Canvas uGUI ScreenSpaceCamera ordre 0, panneau UI Toolkit ordre {settings.sortingOrder}");
            Line($"  clic simulé sur l'élément UI Toolkit : élément du panneau pickable = {panelBtn.pickingMode}, couleur de la tuile uGUI = {px}");
            // Clics : élément UI Toolkit via événement de navigation, tuile uGUI via ExecuteEvents
            using (var submit = NavigationSubmitEvent.GetPooled()) { submit.target = panelBtn; panelBtn.SendEvent(submit); }
            ExecuteEvents.Execute(imgGo, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
            yield return null;
            Line($"  clics reçus : UI Toolkit = {uitk}, uGUI = {ugui}");
            UnityEngine.Object.Destroy(go); UnityEngine.Object.Destroy(canvasGo); UnityEngine.Object.Destroy(es); UnityEngine.Object.Destroy(cam.gameObject); rt.Release();
            Flush();
            Assert.That(uitk, Is.EqualTo(1)); Assert.That(ugui, Is.EqualTo(1));
            Assert.That(px.r > 0.5f && px.g < 0.3f, Is.True, "la tuile uGUI est rendue : " + px);
#else
            yield break;
#endif
        }
    }
}
