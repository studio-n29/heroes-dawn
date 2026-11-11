#nullable enable
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.Characters3D
{
    /// <summary>Première passe : règle l'import des FBX de personnages 3D (rig humanoïde, textures extraites) et écrit dans le journal ce que Unity y a trouvé.</summary>
    public static class CharacterImportDiscovery
    {
        public const string Root = "Assets/_Project/Art/Models3D";
        public static readonly string[] Names = { "agent_d", "agent_m", "enemy_soldier", "enemy_gangster", "enemy_creature" };

        [MenuItem("Heroes Dawn/Personnages 3D/1. Régler l'import des modèles")]
        public static void Run()
        {
            foreach (var name in Names) ConfigureCharacter(name);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            foreach (var name in Names) Report(name);
        }

        static ModelImporter Importer(string path) => (ModelImporter)AssetImporter.GetAtPath(path);

        static void ConfigureCharacter(string name)
        {
            string dir = Root + "/" + name;
            var body = Importer(dir + "/character.fbx");
            body.animationType = ModelImporterAnimationType.Human;
            body.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            body.importAnimation = false;
            body.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            body.SaveAndReimport();
            string texDir = dir + "/Textures";
            if (!Directory.Exists(texDir)) Directory.CreateDirectory(texDir);
            body.ExtractTextures(texDir);
            AssetDatabase.Refresh();
            var avatar = AssetDatabase.LoadAssetAtPath<Avatar>(dir + "/character.fbx");
            foreach (var clip in new[] { "walk", "run", "actions" })
            {
                var imp = Importer(dir + "/" + clip + ".fbx");
                imp.animationType = ModelImporterAnimationType.Human;
                imp.avatarSetup = ModelImporterAvatarSetup.CopyFromOther;
                imp.sourceAvatar = avatar;
                imp.importAnimation = true;
                imp.materialImportMode = ModelImporterMaterialImportMode.None;
                imp.SaveAndReimport();
            }
        }

        static void Report(string name)
        {
            string dir = Root + "/" + name;
            var avatar = AssetDatabase.LoadAssetAtPath<Avatar>(dir + "/character.fbx");
            Debug.Log($"[Perso3D] {name} avatar valide={(avatar != null && avatar.isValid)} humanoide={(avatar != null && avatar.isHuman)}");
            foreach (var file in new[] { "character", "walk", "run", "actions" })
            {
                var clips = AssetDatabase.LoadAllAssetsAtPath(dir + "/" + file + ".fbx").OfType<AnimationClip>().Where(c => !c.name.StartsWith("__preview")).ToList();
                Debug.Log($"[Perso3D] {name}/{file}.fbx clips: " + string.Join(" | ", clips.Select(c => c.name + " (" + c.length.ToString("0.00") + " s)")));
            }
            var mats = AssetDatabase.LoadAllAssetsAtPath(dir + "/character.fbx").OfType<Material>().ToList();
            Debug.Log($"[Perso3D] {name} matériaux: " + string.Join(", ", mats.Select(m => m.name + (m.shader != null ? "/" + m.shader.name : ""))) + " ; textures: " + string.Join(", ", Directory.GetFiles(dir + "/Textures").Where(f => !f.EndsWith(".meta")).Select(Path.GetFileName)));
        }
    }
}
