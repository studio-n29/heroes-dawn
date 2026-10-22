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
        /// <summary>Icône adaptative Android : fond uni (couleur du bord de l icône) et icône à 64 % au centre (zone sûre).</summary>
        public const string IconBackgroundPath = "Assets/_Project/Art/UI/HD_Icon_Background.png";
        public const string IconForegroundPath = "Assets/_Project/Art/UI/HD_Icon_Foreground.png";

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
            ApplyAndroidIcons(logo);

            // Tous les écrans sont conçus en paysage 16:9.
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Sans réglage par plateforme, Android affiche l'icône Unity : on renseigne tous les emplacements
        /// (adaptative avec fond et premier plan, ronde, classique).
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
}
