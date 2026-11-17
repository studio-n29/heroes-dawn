#nullable enable
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.Build
{
    /// <summary>Identité du jeu dans les réglages de build : nom, studio, identifiant, version, logo en icône, paysage.</summary>
    public static class GameBranding
    {
        public const string ProductName = "Heroes Dawn";
        public const string CompanyName = "Studio N29";
        public const string AndroidIdentifier = "com.studion29.heroesdawn";
        public const string IconPath = "Assets/_Project/Art/UI/HD_AppIcon_AubeTactique.png";
        /// <summary>Icône adaptative Android : fond uni (couleur du bord de l icône) et icône à 64 % au centre (zone sûre).</summary>
        public const string IconBackgroundPath = "Assets/_Project/Art/UI/HD_AppIcon_AubeTactique_AndroidBackground.png";
        public const string IconForegroundPath = "Assets/_Project/Art/UI/HD_AppIcon_AubeTactique_AndroidForeground.png";

        [MenuItem("Heroes Dawn/Build/Appliquer le nom et le logo du jeu")]
        public static void Apply()
        {
            PlayerSettings.productName = ProductName;
            PlayerSettings.companyName = CompanyName;
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, AndroidIdentifier);
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Standalone, AndroidIdentifier);

            ApplyIcons();

            // Tous les écrans sont conçus en paysage 16:9.
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            AssetDatabase.SaveAssets();
        }

        /// <summary>Applique l'icône Aube tactique sans modifier les autres réglages du joueur.</summary>
        public static void ApplyIcons()
        {
            var logo = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
            if (logo == null) throw new System.InvalidOperationException("Icône introuvable : " + IconPath);
            PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { logo }, IconKind.Any);
            ApplyAndroidIcons(logo);
            AssetDatabase.SaveAssets();
            Debug.Log("[Heroes Dawn] Icône Aube tactique appliquée : défaut et tous les emplacements Android.");
        }

        /// <summary>
        /// Renseigne tous les emplacements Android pris en charge par la version de Unity,
        /// notamment chaque taille adaptative avec fond et premier plan dans la zone sûre.
        /// </summary>
        public static void ApplyAndroidIcons(Texture2D logo)
        {
            var background = AssetDatabase.LoadAssetAtPath<Texture2D>(IconBackgroundPath);
            var foreground = AssetDatabase.LoadAssetAtPath<Texture2D>(IconForegroundPath);
            if (background == null || foreground == null)
                throw new System.InvalidOperationException("Icône adaptative introuvable : " + IconBackgroundPath + " / " + IconForegroundPath);
            foreach (var kind in PlayerSettings.GetSupportedIconKinds(NamedBuildTarget.Android))
            {
                var icons = PlayerSettings.GetPlatformIcons(NamedBuildTarget.Android, kind);
                foreach (var icon in icons)
                {
                    if (icon.maxLayerCount > 1) { icon.SetTexture(background, 0); icon.SetTexture(foreground, 1); }
                    else icon.SetTexture(logo, 0);
                }
                PlayerSettings.SetPlatformIcons(NamedBuildTarget.Android, kind, icons);
            }
        }
    }

    /// <summary>Couvre aussi les builds Android lancés depuis les Build Profiles ou un autre script.</summary>
    public sealed class GameBrandingBuildProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform == BuildTarget.Android) GameBranding.ApplyIcons();
        }
    }
}
