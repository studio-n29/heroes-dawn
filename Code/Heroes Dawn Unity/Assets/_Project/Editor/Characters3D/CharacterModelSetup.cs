#nullable enable
using System.IO;
using System.Linq;
using N29.HeroesDawn.Presentation.Characters3D;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.Characters3D
{
    /// <summary>
    /// Fabrique les personnages 3D à partir des FBX de Meshy : réglage de l'import (rig humanoïde, animations en boucle), matériau sans éclairage,
    /// contrôleur d'animation (Idle, Walk, Run, Attack, Cast, Hit, Death) et prefab dans Resources/Characters3D, plus les palettes de couleurs des ennemis.
    /// </summary>
    public static class CharacterModelSetup
    {
        const string Resources_ = "Assets/_Project/Resources/Characters3D";

        [MenuItem("Heroes Dawn/Personnages 3D/2. Générer les prefabs et les palettes")]
        public static void Run()
        {
            CharacterImportDiscovery.Run();
            Directory.CreateDirectory(Resources_ + "/Materials");
            foreach (var name in CharacterImportDiscovery.Names) Build(name);
            EnsurePalettes();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Perso3D] prefabs et palettes générés.");
        }

        static AnimationClip? Clip(string path, System.Func<string, bool> match)
            => AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().FirstOrDefault(c => !c.name.StartsWith("__preview") && match(c.name));

        static void SetLoops(string path, System.Func<string, bool> loops)
        {
            var imp = (ModelImporter)AssetImporter.GetAtPath(path);
            var clips = imp.defaultClipAnimations;
            foreach (var c in clips) { c.loopTime = loops(c.name); c.loopPose = c.loopTime; }
            imp.clipAnimations = clips;
            imp.SaveAndReimport();
        }

        static void Build(string name)
        {
            string dir = CharacterImportDiscovery.Root + "/" + name;
            SetLoops(dir + "/walk.fbx", _ => true);
            SetLoops(dir + "/run.fbx", _ => true);
            SetLoops(dir + "/actions.fbx", n => n.ToLowerInvariant().StartsWith("idle"));

            // ---- Matériau sans éclairage : la scène des activités n'a pas de lumière ; la texture porte déjà les ombres.
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(dir + "/Textures/Material_0_baseColor.png");
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            string matPath = Resources_ + "/Materials/" + name + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null) { mat = new Material(shader); AssetDatabase.CreateAsset(mat, matPath); }
            mat.shader = shader;
            mat.SetTexture("_BaseMap", tex);
            mat.SetColor("_BaseColor", Color.white);
            EditorUtility.SetDirty(mat);

            // ---- Contrôleur d'animation : un état par animation, le passage de l'un à l'autre est piloté par Character3DView.
            string ctrlPath = dir + "/" + name + ".controller";
            AssetDatabase.DeleteAsset(ctrlPath);
            var controller = AnimatorController.CreateAnimatorControllerAtPath(ctrlPath);
            var sm = controller.layers[0].stateMachine;
            void State(string stateName, AnimationClip? clip)
            {
                if (clip == null) return;
                var s = sm.AddState(stateName);
                s.motion = clip;
                if (sm.defaultState == null || stateName == Character3DView.Idle) sm.defaultState = s;
            }
            string actions = dir + "/actions.fbx";
            State(Character3DView.Idle, Clip(actions, n => n.ToLowerInvariant().StartsWith("idle")));
            State(Character3DView.Walk, Clip(dir + "/walk.fbx", _ => true));
            State(Character3DView.Run, Clip(dir + "/run.fbx", _ => true));
            State(Character3DView.Attack, Clip(actions, n => n.Contains("Punch")));
            State(Character3DView.Cast, Clip(actions, n => n.Contains("Spell")));
            State(Character3DView.Hit, Clip(actions, n => n.Contains("Hit_Reaction")));
            State(Character3DView.Death, Clip(actions, n => n.ToLowerInvariant().Contains("dying")));
            EditorUtility.SetDirty(controller);

            // ---- Prefab : le modèle, son animateur, ses rendus sans ombre, la vue.
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(dir + "/character.fbx");
            var avatar = AssetDatabase.LoadAssetAtPath<Avatar>(dir + "/character.fbx");
            var go = Object.Instantiate(model);
            go.name = name;
            var animator = go.GetComponent<Animator>() ?? go.AddComponent<Animator>();
            animator.avatar = avatar;
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            var renderers = go.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                r.sharedMaterials = r.sharedMaterials.Select(_ => mat).ToArray();
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                r.receiveShadows = false;
                if (r is SkinnedMeshRenderer smr) smr.updateWhenOffscreen = true;
            }
            var view = go.GetComponent<Character3DView>() ?? go.AddComponent<Character3DView>();
            view.Setup(animator, renderers);
            string prefabPath = Resources_ + "/" + name + ".prefab";
            PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);
            Debug.Log($"[Perso3D] prefab {prefabPath} : {controller.layers[0].stateMachine.states.Length} états");
        }

        /// <summary>Crée les palettes d'ennemis si elles n'existent pas (les réglages déjà faits dans l'outil sont conservés).</summary>
        public static EnemyPalettes EnsurePalettes()
        {
            string path = Resources_ + "/EnemyPalettes.asset";
            var assetPalettes = AssetDatabase.LoadAssetAtPath<EnemyPalettes>(path);
            if (assetPalettes != null) return assetPalettes;
            Directory.CreateDirectory(Resources_);
            var created = ScriptableObject.CreateInstance<EnemyPalettes>();
            created.kinds = EnemyPalettes.Defaults();
            AssetDatabase.CreateAsset(created, path);
            return created;
        }
    }
}
