#nullable enable
using System.IO;
using UnityEditor;
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.Assets
{
    /// <summary>
    /// Crée les matériaux d'interface (Resources/UI/Materials) à partir des shaders HD/UI/* : hachures des zones verrouillées,
    /// reflet des boutons d'action, habillage « comics » des fonds. Idempotent ; appelé avant la génération des prefabs.
    /// Menu : Heroes Dawn ▸ Setup ▸ Générer les matériaux d'interface.
    /// </summary>
    public static class UiMaterialBuilder
    {
        public const string Folder = "Assets/_Project/Resources/UI/Materials";

        static readonly (string Name, string Shader)[] Materials =
        {
            ("UiHatch", "HD/UI/Hatch"),
            ("UiShine", "HD/UI/Shine"),
            ("UiBackdrop", "HD/UI/Backdrop"),
        };

        [MenuItem("Heroes Dawn/Setup/Générer les matériaux d'interface")]
        public static void EnsureAll()
        {
            Directory.CreateDirectory(Folder);
            AssetDatabase.Refresh();
            foreach (var (name, shaderName) in Materials)
            {
                string path = $"{Folder}/{name}.mat";
                if (AssetDatabase.LoadAssetAtPath<Material>(path) != null) continue;
                var shader = Shader.Find(shaderName);
                if (shader == null) { Debug.LogError("[Heroes Dawn] Shader introuvable : " + shaderName); continue; }
                var mat = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(mat, path);
            }
            AssetDatabase.SaveAssets();
        }
    }
}
