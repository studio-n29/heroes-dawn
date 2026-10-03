#nullable enable
using System.IO;
using N29.HeroesDawn.Composition;
using N29.HeroesDawn.EditorTools.Prefabs;
using N29.HeroesDawn.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VContainer.Unity;

namespace N29.HeroesDawn.EditorTools.Scenes
{
    /// <summary>
    /// Génère les scènes du jeu (Boot, Hub, Board, Combat) de façon déterministe et les inscrit dans les Build Settings.
    /// Menu : Heroes Dawn ▸ Setup ▸ Générer les scènes (exécutable en batchmode avec -executeMethod).
    /// </summary>
    public static class SceneGenerator
    {
        public const string Directory = "Assets/_Project/Scenes";
        public static string PathOf(string scene) => $"{Directory}/{scene}.unity";

        /// <summary>
        /// Menu : propose d'enregistrer les scènes modifiées, confirme l'écrasement des 4 scènes du jeu
        /// (et de leurs retouches manuelles), génère, puis rouvre les scènes qui étaient ouvertes.
        /// </summary>
        [MenuItem("Heroes Dawn/Setup/Générer les scènes")]
        static void GenerateAllFromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!EditorUtility.DisplayDialog("Générer les scènes",
                    "Les scènes Boot, Hub, Board et Combat vont être recréées et les Build Settings remplacés.\n" +
                    "Toute retouche manuelle de ces scènes sera perdue.", "Générer", "Annuler")) return;
            var setup = EditorSceneManager.GetSceneManagerSetup();
            GenerateAll();
            if (setup.Length > 0 && System.Array.TrueForAll(setup, s => File.Exists(s.path)))
                EditorSceneManager.RestoreSceneManagerSetup(setup);
        }

        /// <summary>Génération sans interaction (batchmode : -executeMethod … SceneGenerator.GenerateAll -quit).</summary>
        public static void GenerateAll()
        {
            System.IO.Directory.CreateDirectory(Directory);
            Save(BuildBoot(), "Boot");
            Save(BuildScreenScene<HubLifetimeScope>(ParentReference.Create<RootLifetimeScope>(), 10), "Hub");
            Save(BuildScreenScene<BoardLifetimeScope>(ParentReference.Create<RootLifetimeScope>(), 10), "Board");
            Save(BuildScreenScene<CombatLifetimeScope>(ParentReference.Create<BoardLifetimeScope>(), 20), "Combat");
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(PathOf("Boot"), true),
                new EditorBuildSettingsScene(PathOf("Hub"), true),
                new EditorBuildSettingsScene(PathOf("Board"), true),
                new EditorBuildSettingsScene(PathOf("Combat"), true),
            };
            AssetDatabase.SaveAssets();
            Debug.Log("[Heroes Dawn] Scènes générées : Boot, Hub, Board, Combat (Build Settings à jour).");
        }

        static Scene BuildBoot()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cam = new GameObject("Main Camera");
            var camera = cam.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.orthographic = true;
            cam.tag = "MainCamera";

            var app = new GameObject("[Heroes Dawn]");
            var rootScope = app.AddComponent<RootLifetimeScope>();
            var rso = new SerializedObject(rootScope);
            rso.FindProperty("catalog").objectReferenceValue = PrefabGenerator.EnsureCatalog();
            rso.ApplyModifiedPropertiesWithoutUndo();
            app.AddComponent<GameBootstrap>();

            // Écrans de Boot (canvas d'ordre 10) et couche commune au-dessus de toutes les scènes (canvas imbriqué d'ordre 100).
            var ui = NewCanvas("UI", 10);
            var screens = Child(SafeArea(ui.transform), "Screens");
            var overlay = Child(ui.transform, "Overlay");
            var overlayCanvas = overlay.gameObject.AddComponent<Canvas>();
            overlayCanvas.overrideSorting = true;
            overlayCanvas.sortingOrder = 100;
            overlay.gameObject.AddComponent<GraphicRaycaster>();
            var overlaySafe = SafeArea(overlay);
            var root = ui.AddComponent<UiRoot>();
            var uso = new SerializedObject(root);
            uso.FindProperty("screensRoot").objectReferenceValue = screens;
            uso.FindProperty("overlayRoot").objectReferenceValue = overlaySafe;
            uso.ApplyModifiedPropertiesWithoutUndo();
            return scene;
        }

        static GameObject NewCanvas(string name, int sortingOrder)
        {
            var canvasGo = new GameObject(name, typeof(RectTransform));
            canvasGo.layer = 5;
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();
            return canvasGo;
        }

        /// <summary>Enfant étiré sur tout son parent (calque UI).</summary>
        static RectTransform Child(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = 5;
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return rt;
        }

        static RectTransform SafeArea(Transform parent)
        {
            var rt = Child(parent, "SafeArea");
            rt.gameObject.AddComponent<N29.HeroesDawn.Presentation.SafeArea>();
            return rt;
        }

        static Scene BuildScreenScene<TScope>(ParentReference parent, int sortingOrder) where TScope : SceneScreenScope
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var canvasGo = NewCanvas("Canvas", sortingOrder);
            var rt = SafeArea(canvasGo.transform);

            var scopeGo = new GameObject("[Scope]");
            var scope = scopeGo.AddComponent<TScope>();
            scope.parentReference = parent; // Create<T>() renseigne Type : TypeName est conservé à la sauvegarde
            var so = new SerializedObject(scope);
            so.FindProperty("screenRoot").objectReferenceValue = rt;
            so.FindProperty("catalog").objectReferenceValue = PrefabGenerator.EnsureCatalog();
            so.ApplyModifiedPropertiesWithoutUndo();
            return scene;
        }

        static void Save(Scene scene, string name)
        {
            if (!EditorSceneManager.SaveScene(scene, PathOf(name)))
                throw new IOException("Impossible d'enregistrer la scène " + PathOf(name));
        }
    }
}
