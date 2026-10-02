#nullable enable
using System;
using System.IO;
using N29.HeroesDawn.Application.Events;
using N29.HeroesDawn.Application.Features;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Infrastructure.Platform;
using N29.HeroesDawn.Infrastructure.Simulated;
using N29.HeroesDawn.Presentation;
using UnityEngine;

namespace N29.HeroesDawn.Composition
{
    /// <summary>
    /// Racine de composition du prototype M1 (injection manuelle ; VContainer prendra le relais au M2).
    /// Démarre automatiquement en mode Play, quelle que soit la scène ouverte : aucune configuration de scène requise.
    /// </summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        public static GameBootstrap? Instance { get; private set; }

        public GameSession Session { get; private set; } = null!;
        public UiRoot Ui { get; private set; } = null!;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoStart()
        {
            if (Instance != null) return;
            var go = new GameObject("[Heroes Dawn]");
            DontDestroyOnLoad(go);
            go.AddComponent<GameBootstrap>();
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

            // ---- Contenu & état
            var log = new UnityLogger();
            var content = PrototypeContent.Build(LoadBalance(log));
            Session = new GameSession(content);
            var events = new GameEvents();

            // ---- Adaptateurs (réseau simulé : placeholders des services UGS du M2)
            var clock = new SystemClock();
            var seeds = new LocalSeedProvider();
            var auth = new FakeAuthGateway();
            var profiles = new LocalProfileGateway();
            var verifier = new LocalCombatVerifier(content);

            // ---- Services applicatifs
            var save = new SaveService(Session, profiles, clock, log, events);
            var authService = new AuthService(Session, auth, profiles, clock, save, events, log);
            var hub = new HubService(Session, save, events);
            var zones = new ZoneService(Session, seeds, save, events, log);
            var combat = new CombatService(Session, seeds, zones, save, events, verifier, log);

            // ---- Interface
            var services = new UiServices(Session, authService, hub, zones, combat, events, log, PrototypeContent.Zone11);
            Ui = UiRoot.Create(transform, services);
            log.Info($"Prototype M1 prêt — {content.Members.Count} recrues, {content.Zones.Count} zone(s). Données : {UnityEngine.Application.persistentDataPath}");
        }

        /// <summary>Équilibrage importé du tableur (Resources/Balance/balance.json) ; valeurs par défaut sinon.</summary>
        static BalanceConfig LoadBalance(UnityLogger log)
        {
            var asset = Resources.Load<TextAsset>("Balance/balance");
            if (asset == null) return new BalanceConfig();
            var parsed = BalanceJson.Deserialize(asset.text);
            if (parsed.IsSuccess) return parsed.Value;
            log.Warn("Équilibrage illisible (balance.json) : valeurs par défaut");
            return new BalanceConfig();
        }

        void OnApplicationPause(bool paused)
        {
            if (paused) SaveNow();
        }

        void OnApplicationQuit() => SaveNow();

        /// <summary>Sauvegarde synchrone de secours (la version asynchrone peut ne pas aboutir à la fermeture).</summary>
        void SaveNow()
        {
            var account = Session?.Account;
            if (account == null) return;
            try
            {
                var dir = Path.Combine(UnityEngine.Application.persistentDataPath, "profiles");
                Directory.CreateDirectory(dir);
                var id = account.PlayerId;
                foreach (var c in Path.GetInvalidFileNameChars()) id = id.Replace(c, '_');
                File.WriteAllText(Path.Combine(dir, id + ".json"), JsonUtility.ToJson(Session!.ToProfile(DateTime.UtcNow), true));
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
