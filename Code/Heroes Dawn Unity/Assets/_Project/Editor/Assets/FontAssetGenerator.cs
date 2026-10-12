#nullable enable
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace N29.HeroesDawn.EditorTools.Assets
{
    /// <summary>
    /// Crée les polices TextMeshPro du jeu depuis Resources/Fonts/*.ttf : un Font Asset dynamique par police
    /// (atlas rempli à la volée, vidé au build) et un matériau « Inked » (contour noir + ombre portée, lettrage comics).
    /// Importe d'abord les ressources essentielles TMP si elles manquent. Appelé par Heroes Dawn ▸ Setup ▸ Générer les assets d'interface.
    /// </summary>
    public static class FontAssetGenerator
    {
        /// <summary>Chemin Resources (sans « Resources/ ») des polices TMP, chargées à l'exécution par <c>Ui</c>.</summary>
        public const string ResourcesFolder = "Fonts/TMP";
        const string Folder = "Assets/_Project/Resources/" + ResourcesFolder;
        const string SourceFolder = "Assets/_Project/Resources/Fonts";
        static readonly string[] Fonts = { "BarlowCondensed-ExtraBoldItalic", "BarlowCondensed-SemiBold", "BarlowCondensed-Regular" };

        public static void GenerateAll()
        {
            EnsureEssentials();
            Directory.CreateDirectory(Folder);
            foreach (var name in Fonts) Generate(name);
            AssetDatabase.SaveAssets();
        }

        static void EnsureEssentials()
        {
            if (TMP_Settings.instance != null) return;
            TMP_PackageResourceImporter.ImportResources(true, false, false);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            if (TMP_Settings.instance == null)
                throw new System.InvalidOperationException("Ressources essentielles TMP importées : relancer la génération (elles ne sont visibles qu'au lancement suivant).");
        }

        static void Generate(string name)
        {
            var path = $"{Folder}/{name} SDF.asset";
            var fa = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (fa == null)
            {
                var font = AssetDatabase.LoadAssetAtPath<Font>($"{SourceFolder}/{name}.ttf");
                fa = TMP_FontAsset.CreateFontAsset(font, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic);
                fa.name = name + " SDF";
                AssetDatabase.CreateAsset(fa, path);
                fa.atlasTextures[0].name = name + " SDF Atlas";
                AssetDatabase.AddObjectToAsset(fa.atlasTextures[0], fa);
                fa.material.name = name + " SDF Material";
                AssetDatabase.AddObjectToAsset(fa.material, fa);
            }
            var so = new SerializedObject(fa);
            var clear = so.FindProperty("m_ClearDynamicDataOnBuild");
            if (clear != null) { clear.boolValue = true; so.ApplyModifiedPropertiesWithoutUndo(); }
            EditorUtility.SetDirty(fa);

            var inkedPath = $"{Folder}/{name} SDF Inked.mat";
            var inked = AssetDatabase.LoadAssetAtPath<Material>(inkedPath);
            if (inked == null)
            {
                inked = new Material(fa.material) { name = name + " SDF Inked" };
                AssetDatabase.CreateAsset(inked, inkedPath);
            }
            inked.shader = fa.material.shader;
            inked.SetTexture(ShaderUtilities.ID_MainTex, fa.atlasTexture);
            // HUD pastel : plus de contour noir, une ombre douce bleu marine sous le lettrage.
            inked.DisableKeyword(ShaderUtilities.Keyword_Outline);
            inked.SetFloat(ShaderUtilities.ID_OutlineWidth, 0f);
            inked.SetFloat(ShaderUtilities.ID_FaceDilate, 0f);
            inked.EnableKeyword(ShaderUtilities.Keyword_Underlay);
            inked.SetColor(ShaderUtilities.ID_UnderlayColor, new Color(0.106f, 0.145f, 0.314f, 0.45f));
            inked.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, 0.4f);
            inked.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, -0.4f);
            inked.SetFloat(ShaderUtilities.ID_UnderlaySoftness, 0.3f);
            EditorUtility.SetDirty(inked);
        }
    }
}
