#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using N29.HeroesDawn.Presentation.Mvp;
using UnityEditor;
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.Prefabs
{
    /// <summary>
    /// Génère les prefabs d'interface depuis leurs recettes, une fois : un prefab existant n'est jamais écrasé
    /// sans demande explicite (fenêtre du menu, ou -overwritePrefabs en batchmode). Remplit ensuite le <see cref="ScreenCatalog"/>.
    /// Menu : Heroes Dawn ▸ Setup ▸ Générer les prefabs d'écran.
    /// </summary>
    public static class PrefabGenerator
    {
        public const string DefaultRoot = "Assets/_Project/Prefabs";
        public const string CatalogPath = "Assets/_Project/Settings/ScreenCatalog.asset";

        static IReadOnlyList<IPrefabRecipe>? _recipes;

        public static IReadOnlyList<IPrefabRecipe> Recipes => _recipes ??= TypeCache.GetTypesDerivedFrom<IPrefabRecipe>()
            .Where(t => !t.IsAbstract && t.GetConstructor(Type.EmptyTypes) != null)
            .Select(t => (IPrefabRecipe)Activator.CreateInstance(t))
            .OrderBy(r => r.Order).ThenBy(r => r.RelativePath, StringComparer.Ordinal)
            .ToList();

        public static GenerationReport Generate(string root, OverwritePolicy policy, ISet<string>? selected = null)
        {
            var report = new GenerationReport();
            var ctx = new PrefabContext(root);
            foreach (var recipe in Recipes)
            {
                var rel = recipe.RelativePath;
                var path = root + "/" + rel;
                bool exists = AssetDatabase.LoadAssetAtPath<GameObject>(path) != null;
                if (exists && (policy == OverwritePolicy.Never || (policy == OverwritePolicy.Selected && (selected == null || !selected.Contains(rel)))))
                {
                    report.Skipped.Add(rel);
                    continue;
                }
                GameObject? go = null;
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                    go = recipe.Build(ctx);
                    go.name = Path.GetFileNameWithoutExtension(rel);
                    PrefabUtility.SaveAsPrefabAsset(go, path, out bool ok);
                    if (!ok) throw new IOException("Enregistrement impossible : " + path);
                    (exists ? report.Overwritten : report.Created).Add(rel);
                }
                catch (Exception e)
                {
                    report.Failed.Add(rel + " : " + e.Message);
                    Debug.LogException(e);
                }
                finally
                {
                    if (go != null) UnityEngine.Object.DestroyImmediate(go);
                }
            }
            AssetDatabase.SaveAssets();
            if (root == DefaultRoot) FillCatalog();
            return report;
        }

        /// <summary>Crée le catalogue s'il manque (le générateur de scènes le branche sur les scopes).</summary>
        public static ScreenCatalog EnsureCatalog()
        {
            var c = AssetDatabase.LoadAssetAtPath<ScreenCatalog>(CatalogPath);
            if (c != null) return c;
            Directory.CreateDirectory(Path.GetDirectoryName(CatalogPath)!);
            c = ScriptableObject.CreateInstance<ScreenCatalog>();
            AssetDatabase.CreateAsset(c, CatalogPath);
            return c;
        }

        static void FillCatalog()
        {
            GameObject? Load(string rel) => AssetDatabase.LoadAssetAtPath<GameObject>(DefaultRoot + "/" + rel);
            GameObject[] InFolder(string folder)
                => !AssetDatabase.IsValidFolder(DefaultRoot + "/" + folder) ? Array.Empty<GameObject>()
                   : AssetDatabase.FindAssets("t:Prefab", new[] { DefaultRoot + "/" + folder })
                                  .Select(AssetDatabase.GUIDToAssetPath).OrderBy(p => p, StringComparer.Ordinal)
                                  .Select(AssetDatabase.LoadAssetAtPath<GameObject>).ToArray();
            var c = EnsureCatalog();
            c.Fill(Load("Screens/Login.prefab"), Load("Screens/Hub.prefab"), Load("Screens/Board.prefab"), Load("Screens/Combat.prefab"),
                   Load("Common/ModalLayer.prefab"), Load("Common/Toasts.prefab"), InFolder("Popups"), InFolder("Pages"), Load("Common/TutorialOverlay.prefab"));
            EditorUtility.SetDirty(c);
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Batchmode : -executeMethod … PrefabGenerator.GenerateAllBatch [-overwritePrefabs | -overwrite Screens/Hub.prefab,Widgets/X.prefab].
        /// Code de sortie 1 si une recette échoue.
        /// </summary>
        public static void GenerateAllBatch()
        {
            var args = Environment.GetCommandLineArgs();
            var selected = SelectedFromArgs(args);
            var policy = args.Contains("-overwritePrefabs") ? OverwritePolicy.All : selected != null ? OverwritePolicy.Selected : OverwritePolicy.Never;
            var report = Generate(DefaultRoot, policy, selected);
            Debug.Log("[Heroes Dawn] Prefabs : " + report);
            if (report.Failed.Count > 0 && Application.isBatchMode) EditorApplication.Exit(1);
        }

        /// <summary>Chemins relatifs listés après <c>-overwrite</c> (séparés par des virgules) ; null si l'option est absente.</summary>
        public static ISet<string>? SelectedFromArgs(string[] args)
        {
            int i = Array.IndexOf(args, "-overwrite");
            if (i < 0 || i + 1 >= args.Length) return null;
            return new HashSet<string>(args[i + 1].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()));
        }

        [MenuItem("Heroes Dawn/Setup/Générer les prefabs d'écran")]
        static void OpenWindow() => PrefabGeneratorWindow.Open();
    }
}
