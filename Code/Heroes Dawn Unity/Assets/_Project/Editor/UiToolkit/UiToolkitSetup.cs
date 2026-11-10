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
        public const string ThemePath = "Assets/_Project/UIToolkit/Theme/Hd.tss";
        public const string Screens = "Assets/_Project/UIToolkit/Screens/";

        /// <summary>Ordres d'affichage des couches : écrans, fenêtres, jetons de toast, tutoriel.</summary>
        public const int ScreenOrder = 0, ModalOrder = 100, ToastOrder = 200, TutorialOrder = 300;

        [MenuItem("Heroes Dawn/Interface/Régler le panneau UI Toolkit")]
        public static PanelSettings EnsurePanelSettings()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(PanelSettingsPath)!);
            var settings = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            bool created = settings == null;
            if (created) settings = ScriptableObject.CreateInstance<PanelSettings>();
            settings!.themeStyleSheet = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(ThemePath);
            settings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            settings.referenceResolution = new Vector2Int(1920, 1080);
            settings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            settings.match = 0.5f;
            settings.clearColor = false;
            settings.targetDisplay = 0;
            if (created) AssetDatabase.CreateAsset(settings, PanelSettingsPath);
            else EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            return settings;
        }

        /// <summary>Objet de vue : RectTransform (placé comme un prefab uGUI), UIDocument et composant de vue.</summary>
        public static GameObject NewViewObject<TView>(string name, string uxmlName, int sortingOrder) where TView : Component
        {
            var go = new GameObject(name, typeof(RectTransform));
            var doc = go.AddComponent<UIDocument>();
            doc.panelSettings = EnsurePanelSettings();
            doc.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Screens + uxmlName);
            doc.sortingOrder = sortingOrder;
            go.AddComponent<TView>();
            return go;
        }
    }
}
