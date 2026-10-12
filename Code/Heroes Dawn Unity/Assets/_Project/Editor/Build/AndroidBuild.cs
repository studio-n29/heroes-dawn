#nullable enable
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.Build
{
    /// <summary>
    /// Construit l'APK Android (IL2CPP, ARM64, clé de débogage : installable hors Play Store) avec l'identité du jeu.
    /// Batchmode : -executeMethod N29.HeroesDawn.EditorTools.Build.AndroidBuild.BuildApkBatch -apkPath &lt;fichier.apk&gt; [-appVersion 0.7.0] -quit.
    /// </summary>
    public static class AndroidBuild
    {
        [MenuItem("Heroes Dawn/Build/APK Android")]
        static void BuildFromMenu()
        {
            var path = EditorUtility.SaveFilePanel("APK Android", "", "HeroesDawn.apk", "apk");
            if (!string.IsNullOrEmpty(path)) Build(path);
        }

        public static void BuildApkBatch()
        {
            var args = Environment.GetCommandLineArgs();
            int i = Array.IndexOf(args, "-apkPath");
            if (i < 0 || i + 1 >= args.Length) throw new ArgumentException("-apkPath <fichier.apk> manquant");
            int v = Array.IndexOf(args, "-appVersion");
            if (v >= 0 && v + 1 < args.Length) ApplyVersion(args[v + 1]);
            var result = Build(args[i + 1]);
            if (Application.isBatchMode) EditorApplication.Exit(result == BuildResult.Succeeded ? 0 : 1);
        }

        /// <summary>Version affichée (« 0.7.0 ») et code de version Android (0.7.0 → 700).</summary>
        public static void ApplyVersion(string version)
        {
            var parts = version.Split('.').Select(int.Parse).ToArray();
            if (parts.Length != 3) throw new ArgumentException("-appVersion attend MAJEUR.MINEUR.CORRECTIF");
            PlayerSettings.bundleVersion = version;
            PlayerSettings.Android.bundleVersionCode = parts[0] * 10000 + parts[1] * 100 + parts[2];
        }

        public static BuildResult Build(string apkPath)
        {
            GameBranding.Apply();
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            EditorUserBuildSettings.buildAppBundle = false;
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(apkPath))!);
            var options = new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                locationPathName = apkPath,
                target = BuildTarget.Android,
                options = BuildOptions.None,
            };
            var report = BuildPipeline.BuildPlayer(options);
            Debug.Log($"[Heroes Dawn] APK : {report.summary.result}, {report.summary.totalSize / (1024 * 1024)} Mo, {report.summary.totalErrors} erreur(s) → {apkPath}");
            return report.summary.result;
        }
    }
}
