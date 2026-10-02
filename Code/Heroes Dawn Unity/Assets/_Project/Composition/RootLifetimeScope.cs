#nullable enable
using N29.HeroesDawn.Application.Events;
using N29.HeroesDawn.Application.Features;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Infrastructure.Platform;
using N29.HeroesDawn.Infrastructure.Simulated;
using N29.HeroesDawn.Presentation;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using ILogger = N29.HeroesDawn.Application.Ports.ILogger;

namespace N29.HeroesDawn.Composition
{
    /// <summary>
    /// Scope racine : vit tout le jeu. Enregistre le contenu, la session, les ports (adaptateurs simulés du M1),
    /// les services applicatifs et l'interface.
    /// </summary>
    public sealed class RootLifetimeScope : LifetimeScope
    {
        public UiRoot? Ui { get; private set; }

        protected override void Configure(IContainerBuilder builder)
        {
            var log = new UnityLogger();
            var content = PrototypeContent.Build(LoadBalance(log));

            // ---- Contenu & état
            builder.RegisterInstance(content);
            builder.RegisterInstance<ILogger>(log);
            builder.Register<GameSession>(Lifetime.Singleton);
            builder.Register<GameEvents>(Lifetime.Singleton);

            // ---- Ports (réseau simulé : placeholders des services UGS du M2)
            builder.Register<IClock, SystemClock>(Lifetime.Singleton);
            builder.Register<ISeedProvider>(_ => new LocalSeedProvider(), Lifetime.Singleton);
            builder.Register<IAuthGateway>(_ => new FakeAuthGateway(), Lifetime.Singleton);
            builder.Register<IProfileGateway>(_ => new LocalProfileGateway(), Lifetime.Singleton);
            builder.Register<ICombatVerifier>(_ => new LocalCombatVerifier(content), Lifetime.Singleton);

            // ---- Services applicatifs
            builder.Register<SaveService>(Lifetime.Singleton);
            builder.Register<AuthService>(Lifetime.Singleton);
            builder.Register<HubService>(Lifetime.Singleton);
            builder.Register<ZoneService>(Lifetime.Singleton);
            builder.Register<CombatService>(Lifetime.Singleton);

            // ---- Interface
            builder.Register<UiServices>(c => new UiServices(
                c.Resolve<GameSession>(), c.Resolve<AuthService>(), c.Resolve<HubService>(), c.Resolve<ZoneService>(),
                c.Resolve<CombatService>(), c.Resolve<GameEvents>(), c.Resolve<ILogger>(), PrototypeContent.Zone11), Lifetime.Singleton);

            builder.RegisterBuildCallback(c =>
            {
                Ui = UiRoot.Create(transform, c.Resolve<UiServices>());
                log.Info($"Prototype M1 prêt — {content.Members.Count} recrues, {content.Zones.Count} zone(s). Données : {UnityEngine.Application.persistentDataPath}");
            });
        }

        /// <summary>Équilibrage importé du tableur (Resources/Balance/balance.json) ; valeurs par défaut sinon.</summary>
        static BalanceConfig LoadBalance(ILogger log)
        {
            var asset = Resources.Load<TextAsset>("Balance/balance");
            if (asset == null) return new BalanceConfig();
            var parsed = BalanceJson.Deserialize(asset.text);
            if (parsed.IsSuccess) return parsed.Value;
            log.Warn("Équilibrage illisible (balance.json) : valeurs par défaut");
            return new BalanceConfig();
        }
    }
}
