#nullable enable
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.EditorTools.UiToolkit
{
    /// <summary>Réglages du panneau UI Toolkit du jeu (thème, mise à l'échelle) et fabrication des objets de vue.</summary>
    public static class UiToolkitSetup
    {
        public const string PanelSettingsPath = "Assets/_Project/Resources/UiToolkit/HdPanelSettings.asset";
        /// <summary>Panneau des couches au-dessus de tout le jeu (fenêtres, toasts, tutoriel) : il passe devant les canvas uGUI.</summary>
        public const string OverlaySettingsPath = "Assets/_Project/Resources/UiToolkit/HdOverlayPanelSettings.asset";
        public const float OverlayPanelOrder = 50f;
        public const string ThemePath = "Assets/_Project/UIToolkit/Theme/Hd.tss";
        public const string Screens = "Assets/_Project/UIToolkit/Screens/";

        /// <summary>Ordres d'affichage des couches : écrans, fenêtres, jetons de toast, tutoriel.</summary>
        public const int ScreenOrder = 0, ModalOrder = 100, ToastOrder = 200, TutorialOrder = 300;

        [MenuItem("Heroes Dawn/Interface/Régler le panneau UI Toolkit")]
        public static PanelSettings EnsurePanelSettings() { EnsurePanelSettings(true); return EnsurePanelSettings(false); }

        public static PanelSettings EnsurePanelSettings(bool overlay)
        {
            string path = overlay ? OverlaySettingsPath : PanelSettingsPath;
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            var settings = AssetDatabase.LoadAssetAtPath<PanelSettings>(path);
            bool created = settings == null;
            if (created) settings = ScriptableObject.CreateInstance<PanelSettings>();
            settings!.themeStyleSheet = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(ThemePath);
            settings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            settings.referenceResolution = new Vector2Int(1920, 1080);
            settings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            settings.match = 0.5f;
            settings.clearColor = false;
            settings.targetDisplay = 0;
            settings.sortingOrder = overlay ? OverlayPanelOrder : 0f;
            if (created) AssetDatabase.CreateAsset(settings, path);
            else EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            return settings;
        }

        /// <summary>Objet de vue : RectTransform (placé comme un prefab uGUI), UIDocument et composant de vue.</summary>
        public static GameObject NewViewObject<TView>(string name, string uxmlName, int sortingOrder) where TView : Component
        {
            var go = new GameObject(name, typeof(RectTransform));
            var doc = go.AddComponent<UIDocument>();
            doc.panelSettings = EnsurePanelSettings(sortingOrder >= ModalOrder);
            doc.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Screens + uxmlName);
            doc.sortingOrder = sortingOrder;
            go.AddComponent<TView>();
            return go;
        }
    }
}
