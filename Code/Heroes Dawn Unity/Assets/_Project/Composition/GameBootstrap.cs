#nullable enable
using System;
using System.IO;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Infrastructure.Simulated;
using N29.HeroesDawn.Presentation;
using UnityEngine;
using VContainer;

namespace N29.HeroesDawn.Composition
{
    /// <summary>
    /// Point d'entrée du jeu, posé dans la scène Boot (index 0 des Build Settings) à côté du <see cref="RootLifetimeScope"/>.
    /// Règle l'application (cadence, orientation) et gère la sauvegarde de sortie.
    /// </summary>
    [RequireComponent(typeof(RootLifetimeScope))]
    public sealed class GameBootstrap : MonoBehaviour
    {
        public static GameBootstrap? Instance { get; private set; }

        public RootLifetimeScope Scope { get; private set; } = null!;
        public GameSession Session => Scope.Container.Resolve<GameSession>();
        public UiRoot Ui => Scope.Ui!;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Scope = GetComponent<RootLifetimeScope>(); // conteneur déjà construit (LifetimeScope s'exécute avant)
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
