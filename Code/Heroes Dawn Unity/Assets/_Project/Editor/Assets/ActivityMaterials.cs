#nullable enable
using System.IO;
using UnityEditor;
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.Assets
{
    /// <summary>
    /// Matériau plat des activités 3D (URP Unlit, couleur par objet via MaterialPropertyBlock). Il vit dans Resources pour que le shader
    /// soit embarqué dans les builds (Android compris).
    /// Heroes Dawn ▸ Setup ▸ Générer le matériau des activités ; en ligne de commande :
    /// <c>-executeMethod N29.HeroesDawn.EditorTools.Assets.ActivityMaterials.CreateBatch -quit</c>.
    /// </summary>
    public static class ActivityMaterials
    {
        public const string Path = "Assets/_Project/Resources/Activities/FlatUnlit.mat";

        [MenuItem("Heroes Dawn/Setup/Générer le matériau des activités")]
        public static void Create()
        {
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path)!);
            var shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
            var mat = AssetDatabase.LoadAssetAtPath<Material>(Path);
            if (mat == null)
            {
                mat = new Material(shader) { name = "FlatUnlit" };
                AssetDatabase.CreateAsset(mat, Path);
            }
            else mat.shader = shader;
            mat.enableInstancing = true;
            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssets();
        }

        public static void CreateBatch() => Create();
    }
}
