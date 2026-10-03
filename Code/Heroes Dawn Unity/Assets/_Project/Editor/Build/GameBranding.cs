#nullable enable
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.Build
{
    /// <summary>Identité du jeu dans les réglages de build : nom, studio, identifiant, version, logo en icône, paysage.</summary>
    public static class GameBranding
    {
        public const string ProductName = "Heroes Dawn";
        public const string CompanyName = "Studio N29";
        public const string AndroidIdentifier = "com.studion29.heroesdawn";
        public const string IconPath = "Assets/_Project/Art/UI/HD_Logo.png";

        [MenuItem("Heroes Dawn/Build/Appliquer le nom et le logo du jeu")]
        public static void Apply()
        {
            PlayerSettings.productName = ProductName;
            PlayerSettings.companyName = CompanyName;
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, AndroidIdentifier);
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Standalone, AndroidIdentifier);

            var logo = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
            if (logo == null) throw new System.InvalidOperationException("Logo introuvable : " + IconPath + " (Heroes Dawn ▸ Setup ▸ Générer les assets d'interface)");
            PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { logo }, IconKind.Any);

            // Tous les écrans sont conçus en paysage 16:9.
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            AssetDatabase.SaveAssets();
        }
    }
}
