#nullable enable
using System.Collections;
using System.IO;
using N29.HeroesDawn.Presentation.UiToolkit;
using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace N29.HeroesDawn.Tests.PlayMode
{
    /// <summary>Outils de test UI Toolkit : document rendu dans une texture au format voulu, capture, navigation manette, clic.</summary>
    public static class UiToolkitHarness
    {
        public const string PanelSettingsPath = "Assets/_Project/Resources/UiToolkit/HdPanelSettings.asset";

        /// <summary>Formats cibles : PC 16:9, mobile 20:9, Switch portable, 4:3, 1440p.</summary>
        public static readonly (int W, int H, string Name)[] Formats =
        {
            (1920, 1080, "1920x1080"), (2160, 972, "2160x972"), (1280, 720, "1280x720"), (1280, 960, "1280x960"), (2560, 1440, "2560x1440"),
        };

        public static string Folder(string group) => Path.GetFullPath(Path.Combine(ScreenCaptureTests.Folder, "../uitk", group));

        public sealed class Rig
        {
            public GameObject Go = null!;
            public UIDocument Doc = null!;
            public RenderTexture Rt = null!;
            public PanelSettings Settings = null!;
            public VisualElement Root => Doc.rootVisualElement;
            public T View<T>() where T : Component => Go.GetComponent<T>();
        }

        /// <summary>Crée un document (UXML + vue) rendu dans une RenderTexture de la taille voulue ; le thème est celui du jeu.</summary>
        public static Rig Create<TView>(string uxmlPath, int width, int height) where TView : Component
        {
            var rig = new Rig { Rt = new RenderTexture(width, height, 24) };
#if UNITY_EDITOR
            var shared = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            // Copie du réglage du jeu avec la cible de rendu propre au test.
            rig.Settings = Object.Instantiate(shared);
#else
            rig.Settings = ScriptableObject.CreateInstance<PanelSettings>();
#endif
            rig.Settings.targetTexture = rig.Rt;
            rig.Go = new GameObject("UiToolkitRig", typeof(RectTransform));
            rig.Go.SetActive(false);
            rig.Doc = rig.Go.AddComponent<UIDocument>();
            rig.Doc.panelSettings = rig.Settings;
#if UNITY_EDITOR
            rig.Doc.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
#endif
            rig.Go.AddComponent<TView>();
            rig.Go.SetActive(true);
            return rig;
        }

        public static IEnumerator Settle(int frames = 10) { for (int i = 0; i < frames; i++) yield return null; }

        public static void Save(Rig rig, string group, string name)
        {
            var prev = RenderTexture.active;
            RenderTexture.active = rig.Rt;
            var tex = new Texture2D(rig.Rt.width, rig.Rt.height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, rig.Rt.width, rig.Rt.height), 0, 0);
            tex.Apply();
            RenderTexture.active = prev;
            Directory.CreateDirectory(Folder(group));
            File.WriteAllBytes(Path.Combine(Folder(group), name + ".png"), tex.EncodeToPNG());
            Object.Destroy(tex);
        }

        public static void Destroy(Rig rig)
        {
            Object.Destroy(rig.Go);
            rig.Rt.Release();
            Object.Destroy(rig.Settings);
        }

        /// <summary>Envoie une direction de manette à l'élément ayant le focus.</summary>
        public static void Move(Rig rig, NavigationMoveEvent.Direction direction)
        {
            var target = rig.Root.focusController.focusedElement ?? rig.Root;
            using (var e = NavigationMoveEvent.GetPooled(direction)) { e.target = target; target.SendEvent(e); }
        }

        /// <summary>Valide (bouton A) sur l'élément ayant le focus.</summary>
        public static void Submit(Rig rig)
        {
            var target = rig.Root.focusController.focusedElement ?? rig.Root;
            using (var e = NavigationSubmitEvent.GetPooled()) { e.target = target; target.SendEvent(e); }
        }

        /// <summary>Retour (bouton B) sur l'élément ayant le focus.</summary>
        public static void Cancel(Rig rig)
        {
            var target = rig.Root.focusController.focusedElement ?? rig.Root;
            using (var e = NavigationCancelEvent.GetPooled()) { e.target = target; target.SendEvent(e); }
        }
    }
}
