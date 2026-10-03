#nullable enable
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace N29.HeroesDawn.EditorTools.Scenes
{
    /// <summary>
    /// Dans l'éditeur, Play démarre toujours par la scène Boot, quelle que soit la scène ouverte.
    /// Jamais pendant une exécution du Test Runner : sa scène d'initialisation (« InitTestScene… ») doit rester
    /// la scène de départ, sinon les tests PlayMode ne démarrent pas.
    /// </summary>
    [InitializeOnLoad]
    public static class BootSceneStarter
    {
        static BootSceneStarter()
        {
            EditorSceneManager.playModeStartScene = null;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        static void OnPlayModeChanged(PlayModeStateChange change)
        {
            if (change != PlayModeStateChange.ExitingEditMode) return;
            EditorSceneManager.playModeStartScene = IsTestRunnerScene(SceneManager.GetActiveScene().name)
                ? null
                : AssetDatabase.LoadAssetAtPath<SceneAsset>(SceneGenerator.PathOf("Boot"));
        }

        /// <summary>Scène temporaire créée par le Test Runner avant d'entrer en Play Mode.</summary>
        public static bool IsTestRunnerScene(string activeSceneName) => activeSceneName.StartsWith("InitTestScene");
    }
}
