#nullable enable
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>
    /// Sonde de performance des builds de développement (étape 0 de la refonte UI Toolkit) : toutes les 5 s, images par seconde moyennes,
    /// pire image, 95e et 99e centiles du temps d'image, mémoire gérée et totale. Journal « [HD-PERF] » (adb logcat -s Unity) et fichier
    /// <c>perf.csv</c> dans le dossier de données du jeu. N'existe pas dans un build de production (Debug.isDebugBuild faux).
    /// </summary>
    public sealed class PerfProbe : MonoBehaviour
    {
        const float WindowSeconds = 5f;
        readonly List<float> _frames = new List<float>(600);
        float _elapsed;
        string? _csv;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (!Debug.isDebugBuild) return;
            var go = new GameObject("PerfProbe");
            DontDestroyOnLoad(go);
            go.AddComponent<PerfProbe>();
        }

        void Start()
        {
            _csv = Path.Combine(UnityEngine.Application.persistentDataPath, "perf.csv");
            try { File.WriteAllText(_csv, "temps_s;scene;fps_moyen;pire_ms;p95_ms;p99_ms;gere_mo;total_mo;texture_mo\n"); }
            catch { _csv = null; }
        }

        void Update()
        {
            float dt = Time.unscaledDeltaTime;
            _frames.Add(dt * 1000f);
            _elapsed += dt;
            if (_elapsed < WindowSeconds) return;
            Report();
            _frames.Clear();
            _elapsed = 0f;
        }

        void Report()
        {
            var sorted = new List<float>(_frames);
            sorted.Sort();
            float sum = 0f; foreach (var f in sorted) sum += f;
            float avgMs = sum / sorted.Count;
            float p95 = sorted[Mathf.Min(sorted.Count - 1, (int)(sorted.Count * 0.95f))];
            float p99 = sorted[Mathf.Min(sorted.Count - 1, (int)(sorted.Count * 0.99f))];
            float worst = sorted[sorted.Count - 1];
            double mono = Profiler.GetMonoUsedSizeLong() / 1048576.0;
            double total = Profiler.GetTotalAllocatedMemoryLong() / 1048576.0;
            double tex = Texture.currentTextureMemory / 1048576.0;
            string scene = SceneManager.GetActiveScene().name;
            var line = new StringBuilder()
                .Append(Time.realtimeSinceStartup.ToString("F0")).Append(';').Append(scene).Append(';')
                .Append((1000f / avgMs).ToString("F1")).Append(';').Append(worst.ToString("F1")).Append(';')
                .Append(p95.ToString("F1")).Append(';').Append(p99.ToString("F1")).Append(';')
                .Append(mono.ToString("F1")).Append(';').Append(total.ToString("F1")).Append(';').Append(tex.ToString("F1")).ToString();
            Debug.Log($"[HD-PERF] {scene} fps={1000f / avgMs:F1} pire={worst:F1} ms p95={p95:F1} p99={p99:F1} géré={mono:F0} Mo total={total:F0} Mo textures={tex:F0} Mo");
            if (_csv != null) { try { File.AppendAllText(_csv, line + "\n"); } catch { _csv = null; } }
        }
    }
}
