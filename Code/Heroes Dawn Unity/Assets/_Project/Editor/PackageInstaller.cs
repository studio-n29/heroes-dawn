using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace N29.HeroesDawn.EditorTools
{
    /// <summary>
    /// Installe les packages Unity dans la version recommandée pour l'éditeur courant,
    /// puis branche les références d'assemblies qui en dépendent.
    /// Menu : Heroes Dawn ▸ Setup.
    /// </summary>
    public static class PackageInstaller
    {
        static readonly string[] PrototypePackages =
        {
            "com.unity.inputsystem",
            "com.unity.test-framework",
            "com.unity.addressables",
            "com.unity.localization",
            "com.unity.2d.animation",
            "com.unity.nuget.newtonsoft-json",
            "com.kyrylokuzyk.primetween", // registre OpenUPM déclaré dans manifest.json
        };

        static readonly string[] BackendPackages =
        {
            "com.unity.services.authentication",
            "com.unity.services.cloudcode",
            "com.unity.services.cloudsave",
            "com.unity.services.economy",
            "com.unity.remote-config",
            "com.unity.services.analytics",
            "com.unity.purchasing",
            "com.unity.services.deployment",
        };

        const string PresentationAsmdef = "Assets/_Project/Presentation/N29.HeroesDawn.Presentation.asmdef";
        const string CompositionAsmdef = "Assets/_Project/Composition/N29.HeroesDawn.Composition.asmdef";
        const string PlayModeTestsAsmdef = "Assets/Tests/PlayMode/N29.HeroesDawn.Tests.PlayMode.asmdef";

        static AddAndRemoveRequest _request;

        [MenuItem("Heroes Dawn/Setup/1. Installer les packages du prototype")]
        static void InstallPrototype() => Install(PrototypePackages);

        [MenuItem("Heroes Dawn/Setup/2. Brancher les références d'assemblies (après l'étape 1)")]
        public static void PatchAssemblyReferences()
        {
            var presentationRefs = new[] { "Unity.TextMeshPro", "Unity.InputSystem", "PrimeTween.Runtime" };
            Patch(PresentationAsmdef, presentationRefs, "Newtonsoft.Json.dll");
            Patch(CompositionAsmdef, new[] { "Unity.InputSystem" }, null);
            Patch(PlayModeTestsAsmdef, presentationRefs, null);
            AssetDatabase.Refresh();
            Debug.Log("[Heroes Dawn] Références d'assemblies branchées (TextMeshPro, Input System, PrimeTween, Newtonsoft.Json).");
        }

        [MenuItem("Heroes Dawn/Setup/3. Installer les packages backend (UGS)")]
        static void InstallBackend() => Install(BackendPackages);

        /// <summary>Batchmode (sans -quit) : installe les packages backend puis ferme Unity (code 0 = succès).</summary>
        public static void InstallBackendBatch()
        {
            Install(BackendPackages);
            EditorApplication.update += ExitWhenDone;
        }

        static void ExitWhenDone()
        {
            if (_request == null || !_request.IsCompleted) return;
            EditorApplication.update -= ExitWhenDone;
            EditorApplication.Exit(_request.Status == StatusCode.Success ? 0 : 1);
        }

        static void Install(string[] packages)
        {
            if (_request != null && !_request.IsCompleted)
            {
                Debug.LogWarning("[Heroes Dawn] Une installation est déjà en cours.");
                return;
            }
            Debug.Log($"[Heroes Dawn] Installation : {string.Join(", ", packages)}");
            _request = Client.AddAndRemove(packages, null);
            EditorApplication.update += Poll;
        }

        static void Poll()
        {
            if (_request == null || !_request.IsCompleted) return;
            EditorApplication.update -= Poll;
            if (_request.Status == StatusCode.Success)
                Debug.Log("[Heroes Dawn] Packages installés. Lance maintenant « Heroes Dawn ▸ Setup ▸ 2. Brancher les références d'assemblies ».");
            else
                Debug.LogError($"[Heroes Dawn] Échec de l'installation : {_request.Error?.message}");
        }

        [Serializable]
        sealed class AsmdefData
        {
            public string name;
            public string rootNamespace;
            public string[] references = Array.Empty<string>();
            public string[] includePlatforms = Array.Empty<string>();
            public string[] excludePlatforms = Array.Empty<string>();
            public bool allowUnsafeCode;
            public bool overrideReferences;
            public string[] precompiledReferences = Array.Empty<string>();
            public bool autoReferenced = true;
            public string[] defineConstraints = Array.Empty<string>();
            public bool noEngineReferences;
        }

        static void Patch(string path, string[] references, string precompiled)
        {
            if (!File.Exists(path)) { Debug.LogWarning($"[Heroes Dawn] Introuvable : {path}"); return; }
            var data = JsonUtility.FromJson<AsmdefData>(File.ReadAllText(path));
            data.references = data.references.Union(references).ToArray();
            if (!string.IsNullOrEmpty(precompiled))
            {
                data.overrideReferences = true;
                data.precompiledReferences = data.precompiledReferences.Union(new[] { precompiled }).ToArray();
            }
            File.WriteAllText(path, JsonUtility.ToJson(data, true));
        }
    }
}
