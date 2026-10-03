using N29.HeroesDawn.EditorTools.Build;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace N29.HeroesDawn.Tests.Editor
{
    public sealed class GameBrandingTests
    {
        [Test, Description("Vérifie que l'identité du jeu est appliquée aux réglages de build : nom « Heroes Dawn », studio « Studio N29 », identifiant Android, logo en icône — parce que l'APK installé doit s'afficher avec le nom et le logo du jeu")]
        public void Apply_SetsNameCompanyIdentifierAndIcon()
        {
            GameBranding.Apply();
            Assert.That(PlayerSettings.productName, Is.EqualTo("Heroes Dawn"));
            Assert.That(PlayerSettings.companyName, Is.EqualTo("Studio N29"));
            Assert.That(PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android), Is.EqualTo("com.studion29.heroesdawn"));
            var logo = AssetDatabase.LoadAssetAtPath<Texture2D>(GameBranding.IconPath);
            Assert.That(logo, Is.Not.Null, "logo introuvable");
            var icons = PlayerSettings.GetIcons(NamedBuildTarget.Unknown, IconKind.Any);
            Assert.That(icons, Is.Not.Empty);
            Assert.That(icons[0], Is.SameAs(logo));
        }

        [Test, Description("Vérifie que le jeu ne tourne qu'en paysage — parce que tous les écrans sont conçus en 16:9 paysage (doc 07, UI)")]
        public void Apply_LandscapeOnly()
        {
            GameBranding.Apply();
            Assert.That(PlayerSettings.defaultInterfaceOrientation, Is.EqualTo(UIOrientation.AutoRotation));
            Assert.That(PlayerSettings.allowedAutorotateToLandscapeLeft && PlayerSettings.allowedAutorotateToLandscapeRight, Is.True);
            Assert.That(PlayerSettings.allowedAutorotateToPortrait || PlayerSettings.allowedAutorotateToPortraitUpsideDown, Is.False);
        }
    }
}
