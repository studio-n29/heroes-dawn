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
    /// Build WebGL jouable dans un navigateur mobile (iOS Safari, Android Chrome).
    /// Menu : Heroes Dawn ▸ Build ▸ WebGL (test navigateur). Batchmode :
    /// -executeMethod N29.HeroesDawn.EditorTools.Build.WebGLBuilder.BuildBatch [-buildOutput chemin] -quit
    /// </summary>
    public static class WebGLBuilder
    {
        public const string DefaultOutput = "Builds/WebGL";

        [MenuItem("Heroes Dawn/Build/WebGL (test navigateur)")]
        static void BuildFromMenu() => Build(DefaultOutput);

        public static void BuildBatch()
        {
            var args = Environment.GetCommandLineArgs();
            var i = Array.IndexOf(args, "-buildOutput");
            Build(i >= 0 && i + 1 < args.Length ? args[i + 1] : DefaultOutput);
        }

        static void Build(string output)
        {
            ConfigurePlayer();
            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = output,
                target = BuildTarget.WebGL,
                options = BuildOptions.None,
            });
            var summary = report.summary;
            Debug.Log($"[Heroes Dawn] Build WebGL : {summary.result}, {summary.totalSize / (1024 * 1024)} Mo, {summary.totalErrors} erreur(s) → {output}");
            if (UnityEngine.Application.isBatchMode) EditorApplication.Exit(summary.result == BuildResult.Succeeded ? 0 : 1);
        }

        /// <summary>Réglages adaptés aux navigateurs mobiles : mémoire bornée (iOS), pas de threads, compression gzip avec décompression JS (aucune config serveur requise).</summary>
        static void ConfigurePlayer()
        {
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.WebGL.threadsSupport = false;
            PlayerSettings.WebGL.dataCaching = true;
            PlayerSettings.WebGL.nameFilesAsHashes = true;
            PlayerSettings.WebGL.initialMemorySize = 256;
            PlayerSettings.WebGL.maximumMemorySize = 1024;
            PlayerSettings.WebGL.template = "APPLICATION:Default";
            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.WebGL, ScriptingImplementation.IL2CPP);
        }
    }
}
