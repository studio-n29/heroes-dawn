#nullable enable
using System;
using System.IO;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Infrastructure.Simulated;
using N29.HeroesDawn.Presentation;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;

namespace N29.HeroesDawn.Composition
{
    /// <summary>
    /// Point d'entrée du jeu : démarre automatiquement en mode Play, quelle que soit la scène ouverte.
    /// Règle l'application (cadence, orientation), crée le <see cref="RootLifetimeScope"/> et gère la sauvegarde de sortie.
    /// </summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        public static GameBootstrap? Instance { get; private set; }

        public RootLifetimeScope Scope { get; private set; } = null!;
        public GameSession Session => Scope.Container.Resolve<GameSession>();
        public UiRoot Ui => Scope.Ui!;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoStart()
        {
            if (Instance != null) return;
            var go = new GameObject("[Heroes Dawn]");
            DontDestroyOnLoad(go);
            go.AddComponent<GameBootstrap>();
        }

        /// <summary>Tests PlayMode : démarre le jeu (les doubles sont fournis par <c>LifetimeScope.Enqueue</c>).</summary>
        public static void StartForTests() => AutoStart();

        /// <summary>Tests PlayMode : détruit immédiatement l'instance courante et l'EventSystem.</summary>
        public static void DestroyForTests()
        {
            if (Instance != null)
            {
                var go = Instance.gameObject;
                Instance = null;
                DestroyImmediate(go);
            }
            var es = FindAnyObjectByType<EventSystem>();
            if (es != null) DestroyImmediate(es.gameObject);
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            UnityEngine.Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            if (UnityEngine.Application.isMobilePlatform)
            {
                Screen.autorotateToPortrait = false;
                Screen.autorotateToPortraitUpsideDown = false;
                Screen.autorotateToLandscapeLeft = true;
                Screen.autorotateToLandscapeRight = true;
                Screen.orientation = ScreenOrientation.AutoRotation;
            }
            Scope = gameObject.AddComponent<RootLifetimeScope>(); // construit le conteneur dans son Awake
        }

        void OnApplicationPause(bool paused)
        {
            if (paused) SaveNow();
        }

        void OnApplicationQuit() => SaveNow();

        /// <summary>
        /// Sauvegarde synchrone de secours (la version asynchrone peut ne pas aboutir à la fermeture).
        /// Uniquement avec la sauvegarde locale réelle : jamais avec un double de test.
        /// </summary>
        void SaveNow()
        {
            if (Scope == null || Scope.Container == null) return;
            if (!(Scope.Container.Resolve<IProfileGateway>() is LocalProfileGateway)) return;
            var session = Session;
            var account = session.Account;
            if (account == null) return;
            try
            {
                var dir = Path.Combine(UnityEngine.Application.persistentDataPath, "profiles");
                Directory.CreateDirectory(dir);
                var id = account.PlayerId;
                foreach (var c in Path.GetInvalidFileNameChars()) id = id.Replace(c, '_');
                File.WriteAllText(Path.Combine(dir, id + ".json"), JsonUtility.ToJson(session.ToProfile(DateTime.UtcNow), true));
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Heroes Dawn] Sauvegarde de sortie impossible : " + e.Message);
            }
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
