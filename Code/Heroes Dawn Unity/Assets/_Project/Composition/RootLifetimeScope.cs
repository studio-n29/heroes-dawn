#nullable enable
using MessagePipe;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Features;
using N29.HeroesDawn.Application.Features.Combat;
using N29.HeroesDawn.Application.Features.Hub;
using N29.HeroesDawn.Application.Features.Zone;
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
            builder.Register<GameSession>(Lifetime.Singleton).AsSelf().As<ISessionReadModel>();
            builder.RegisterInstance(PrototypeContent.Zone11);
            var pipe = builder.RegisterMessagePipe();
            builder.RegisterMessageBroker<WalletChanged>(pipe);
            builder.RegisterMessageBroker<RosterChanged>(pipe);
            builder.RegisterMessageBroker<ToastRequested>(pipe);
            builder.RegisterMessageBroker<SaveStateChanged>(pipe);
            builder.RegisterMessageBroker<ZoneValidated>(pipe);
            builder.RegisterMessageBroker<CombatFinished>(pipe);

            // ---- Ports (réseau simulé : placeholders des services UGS du M2)
            builder.Register<IClock, SystemClock>(Lifetime.Singleton);
            builder.Register<ISeedProvider>(_ => new LocalSeedProvider(), Lifetime.Singleton);
            builder.Register<IAuthGateway>(_ => new FakeAuthGateway(), Lifetime.Singleton);
            builder.Register<IProfileGateway>(_ => new LocalProfileGateway(), Lifetime.Singleton);
            builder.Register<ICombatVerifier>(_ => new LocalCombatVerifier(content), Lifetime.Singleton);

            // ---- Services applicatifs
            builder.Register<SaveScheduler>(Lifetime.Singleton);
            builder.Register<AuthService>(Lifetime.Singleton);

            // ---- Use cases : plateau
            builder.Register<ZoneEffects>(Lifetime.Singleton);
            builder.Register<StartZoneUseCase>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<RollDiceUseCase>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<UseBonusDieUseCase>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<MoveToUseCase>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<EndMoveUseCase>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<ResolveMiniGameUseCase>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<ResolveNarrativeUseCase>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<ResolveRestUseCase>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<LeaveZoneUseCase>(Lifetime.Singleton).AsImplementedInterfaces();

            // ---- Use cases : QG
            builder.Register<ToggleSquadUseCase>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<TrainMemberUseCase>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<RecruitMemberUseCase>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<HealRosterUseCase>(Lifetime.Singleton).AsImplementedInterfaces();

            // ---- Use cases : combat
            builder.Register<ActiveCombat>(Lifetime.Singleton).AsSelf().As<IActiveCombatReadModel>();
            builder.Register<BeginCombatUseCase>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<SetAutoCombatUseCase>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<FinishCombatUseCase>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<VerifyCombatUseCase>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<ForceCombatWinUseCase>(Lifetime.Singleton).AsImplementedInterfaces();

            // ---- Interface
            builder.Register<UiServices>(Lifetime.Singleton);

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
