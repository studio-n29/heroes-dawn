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
            // -development : build de mesure (sonde PerfProbe active, profileur connectable), jamais pour la diffusion.
            // -fpsOverlay : compteur d'images par seconde à l'écran (FpsOverlay), dans un build de production pour des mesures réelles.
            var result = Build(args[i + 1], development: Array.IndexOf(args, "-development") >= 0, fpsOverlay: Array.IndexOf(args, "-fpsOverlay") >= 0);
            if (UnityEngine.Application.isBatchMode) EditorApplication.Exit(result == BuildResult.Succeeded ? 0 : 1);
        }

        /// <summary>Version affichée (« 0.7.0 ») et code de version Android (0.7.0 → 700).</summary>
        public static void ApplyVersion(string version)
        {
            var parts = version.Split('.').Select(int.Parse).ToArray();
            if (parts.Length != 3) throw new ArgumentException("-appVersion attend MAJEUR.MINEUR.CORRECTIF");
            PlayerSettings.bundleVersion = version;
            PlayerSettings.Android.bundleVersionCode = parts[0] * 10000 + parts[1] * 100 + parts[2];
        }

        /// <summary>
        /// App Bundle pour le Play Store. -aabPath &lt;fichier.aab&gt; [-appVersion x.y.z]. Sans keystore configuré dans les réglages du joueur,
        /// Unity signe avec la clé de débogage : le Play Store refuse ce bundle, il faut le signer avec la clé d'envoi.
        /// </summary>
        public static void BuildAabBatch()
        {
            var args = Environment.GetCommandLineArgs();
            int i = Array.IndexOf(args, "-aabPath");
            if (i < 0 || i + 1 >= args.Length) throw new ArgumentException("-aabPath <fichier.aab> manquant");
            int v = Array.IndexOf(args, "-appVersion");
            if (v >= 0 && v + 1 < args.Length) ApplyVersion(args[v + 1]);
            var result = Build(args[i + 1], bundle: true);
            if (UnityEngine.Application.isBatchMode) EditorApplication.Exit(result == BuildResult.Succeeded ? 0 : 1);
        }

        /// <summary>Build Windows 64 bits (IL2CPP selon les réglages). -exePath &lt;dossier/HeroesDawn.exe&gt; [-appVersion x.y.z].</summary>
        public static void BuildWindowsBatch()
        {
            var args = Environment.GetCommandLineArgs();
            int i = Array.IndexOf(args, "-exePath");
            if (i < 0 || i + 1 >= args.Length) throw new ArgumentException("-exePath <dossier/HeroesDawn.exe> manquant");
            int v = Array.IndexOf(args, "-appVersion");
            if (v >= 0 && v + 1 < args.Length) ApplyVersion(args[v + 1]);
            GameBranding.Apply();
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneWindows64)
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(args[i + 1]))!);
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                locationPathName = args[i + 1],
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None,
            });
            Debug.Log($"[Heroes Dawn] Windows : {report.summary.result}, {report.summary.totalSize / (1024 * 1024)} Mo, {report.summary.totalErrors} erreur(s) → {args[i + 1]}");
            if (UnityEngine.Application.isBatchMode) EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
        }

        public static BuildResult Build(string apkPath, bool bundle = false, bool development = false, bool fpsOverlay = false)
        {
            GameBranding.Apply();
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            EditorUserBuildSettings.buildAppBundle = bundle;
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(apkPath))!);
            var options = new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                locationPathName = apkPath,
                target = BuildTarget.Android,
                options = development ? BuildOptions.Development :BuildOptions.None,
                extraScriptingDefines = fpsOverlay ? new[] { "HD_FPS_OVERLAY" } : Array.Empty<string>(),
            };
            var report = BuildPipeline.BuildPlayer(options);
            Debug.Log($"[Heroes Dawn] {(bundle ? "AAB" : "APK")} : {report.summary.result}, {report.summary.totalSize / (1024 * 1024)} Mo, {report.summary.totalErrors} erreur(s) → {apkPath}");
            return report.summary.result;
        }
    }
}
