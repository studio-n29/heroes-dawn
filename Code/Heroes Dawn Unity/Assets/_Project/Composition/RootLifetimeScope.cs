#nullable enable
using MessagePipe;
using N29.HeroesDawn.Application.Flow;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Features.Auth;
using N29.HeroesDawn.Application.Features.Combat;
using N29.HeroesDawn.Application.Features.Hub;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Features.Tutorial;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Application.Features.Shop;
using N29.HeroesDawn.Application.Features.Zone;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Infrastructure.Platform;
using N29.HeroesDawn.Infrastructure.Simulated;
using N29.HeroesDawn.Presentation;
using N29.HeroesDawn.Presentation.Audio;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Tutorial;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Presentation.Login;
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
        [SerializeField] ScreenCatalog? catalog;
        [SerializeField] AudioCatalog? audioCatalog;

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
            builder.RegisterMessageBroker<ShopChanged>(pipe);
            builder.RegisterMessageBroker<ToastRequested>(pipe);
            builder.RegisterMessageBroker<SaveStateChanged>(pipe);
            builder.RegisterMessageBroker<ZoneValidated>(pipe);
            builder.RegisterMessageBroker<CombatFinished>(pipe);
            builder.RegisterMessageBroker<NavigateRequested>(pipe);
            builder.RegisterMessageBroker<CombatResultsClosed>(pipe);
            builder.RegisterMessageBroker<ReturnedToBoard>(pipe);
            builder.RegisterMessageBroker<GameStateChanged>(pipe);
            builder.RegisterMessageBroker<MetaChanged>(pipe);
            builder.RegisterMessageBroker<MetaRewardEarned>(pipe);

            // ---- Ports (réseau simulé : placeholders des services UGS du M2)
            builder.Register<IClock, SystemClock>(Lifetime.Singleton);
            builder.Register<ISeedProvider>(_ => new LocalSeedProvider(), Lifetime.Singleton);
            builder.Register<IAuthGateway>(_ => new FakeAuthGateway(), Lifetime.Singleton);
            builder.Register<IProfileGateway>(_ => new LocalProfileGateway(), Lifetime.Singleton);
            builder.Register<ICombatVerifier>(_ => new LocalCombatVerifier(content), Lifetime.Singleton);
            builder.Register<ISceneLoader, UnitySceneLoader>(Lifetime.Singleton);
            builder.Register<IPurchaseGateway>(_ => new FakePurchaseGateway(), Lifetime.Singleton);

            // ---- Flux de jeu (seule autorité sur les scènes)
            builder.Register<GameFlowMachine>(Lifetime.Singleton).AsSelf().As<IGameFlow>();

            // ---- Services applicatifs
            builder.Register<SaveScheduler>(Lifetime.Singleton);

            // ---- Use cases : compte
            builder.Register<SignInAsGuestUseCase>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<SignOutUseCase>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<ResetProgressUseCase>(Lifetime.Singleton).AsImplementedInterfaces();

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

            // ---- Use cases : boutique éphémère
            builder.Register<OpenShopUseCase>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<RecruitFromShopUseCase>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<RerollShopUseCase>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<MarkShopSeenUseCase>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<HealRosterUseCase>(Lifetime.Singleton).AsImplementedInterfaces();

            // ---- Méta-progression : pass, missions, opérations, labo, archives, boutique, réglages
            builder.Register<MetaContext>(Lifetime.Singleton);
            builder.Register<PassProgress>(Lifetime.Singleton);
            builder.Register<DailyMissions>(Lifetime.Singleton);
            builder.Register<SecondaryObjectives>(Lifetime.Singleton);
            builder.Register<Expeditions>(Lifetime.Singleton);
            builder.Register<ResearchLab>(Lifetime.Singleton);
            builder.Register<ArchiveBook>(Lifetime.Singleton);
            builder.Register<Boutique>(Lifetime.Singleton);
            builder.Register<PlayerSettings>(Lifetime.Singleton);
            builder.Register<MetaOverview>(Lifetime.Singleton);
            builder.Register<TutorialGuide>(Lifetime.Singleton);
            builder.Register<MetaTracker>(Lifetime.Singleton);

            // ---- Use cases : combat
            builder.Register<ActiveCombat>(Lifetime.Singleton).AsSelf().As<IActiveCombatReadModel>();
            builder.Register<BeginCombatUseCase>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<SetAutoCombatUseCase>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<FinishCombatUseCase>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<VerifyCombatUseCase>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<ForceCombatWinUseCase>(Lifetime.Singleton).AsImplementedInterfaces();

            // ---- Interface (écrans et popups depuis le catalogue de prefabs)
            var screens = catalog != null ? catalog
                : throw new System.InvalidOperationException("ScreenCatalog manquant sur RootLifetimeScope : relancer Heroes Dawn ▸ Setup ▸ Générer les scènes.");
            builder.RegisterInstance(screens);
            builder.RegisterComponentInHierarchy<UiRoot>().AsSelf().As<IScreenVisibility>();
            builder.Register<IFrameClock, UnityFrameClock>(Lifetime.Singleton);
            builder.Register<IAppQuit, UnityAppQuit>(Lifetime.Singleton);

            // ---- Audio (sources créées une fois ; musique pilotée par l'état du jeu)
            var rig = AudioRig.Create(transform, audioCatalog != null ? audioCatalog.Voices : 10);
            builder.RegisterInstance(rig);
            builder.RegisterEntryPoint(c => new AudioService(rig, audioCatalog, c.Resolve<IFrameClock>(), log), Lifetime.Singleton);
            builder.RegisterEntryPoint<MusicDirector>();
            builder.RegisterEntryPoint<SettingsApplier>();
            builder.RegisterEntryPoint<TutorialDirector>();

            // Modales et toasts habillés de sons (et la musique s'atténue sous une modale).
            builder.Register<IPopupHost>(c => { var ui = c.Resolve<UiRoot>(); ui.EnsureLayers(screens); return new AudiblePopupHost(ui.Modals, c.Resolve<IAudioService>()); }, Lifetime.Singleton);
            builder.Register<IToastService>(c => { var ui = c.Resolve<UiRoot>(); ui.EnsureLayers(screens); return new AudibleToastService(ui.Toasts, c.Resolve<IAudioService>()); }, Lifetime.Singleton);
            builder.Register<IModalService, ModalService>(Lifetime.Singleton);
            builder.Register<ILocalization, LocLocalization>(Lifetime.Singleton);
            builder.Register<IBackInput, InputSystemBackInput>(Lifetime.Singleton);
            builder.RegisterEntryPoint<BackButtonRouter>().AsSelf();
            builder.RegisterEntryPoint<ToastRelay>();
            builder.Register<ILoginView>(c => { var ui = c.Resolve<UiRoot>(); ui.EnsureLayers(screens); return ui.Login; }, Lifetime.Singleton);
            builder.RegisterEntryPoint<LoginPresenter>().AsSelf();
            builder.RegisterEntryPoint<FlowStarter>(); // en dernier : après l'abonnement des présentateurs

            builder.RegisterBuildCallback(c =>
            {
                c.Resolve<MetaTracker>().Start();
                Ui = c.Resolve<UiRoot>();
                Ui.EnsureLayers(screens);
                UiSoundRelay.Attach(Ui.gameObject, c.Resolve<IAudioService>()); // boutons de Boot : connexion, modales
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
