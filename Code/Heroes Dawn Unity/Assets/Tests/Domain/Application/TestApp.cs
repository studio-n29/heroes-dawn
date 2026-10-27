#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Auth;
using N29.HeroesDawn.Application.Features.Combat;
using N29.HeroesDawn.Application.Features.Hub;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Features.Tutorial;
using N29.HeroesDawn.Application.Flow;
using N29.HeroesDawn.Application.Features.Shop;
using N29.HeroesDawn.Application.Features.Zone;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Domain.Combat;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    /// <summary>
    /// Application complète sans Unity : vrai bus MessagePipe, session du prototype, ports factices.
    /// Enregistre tous les messages publiés depuis la construction (<see cref="Received{T}"/>).
    /// </summary>
    public sealed class TestApp
    {
        public sealed class MemoryProfiles : IProfileGateway
        {
            public readonly Dictionary<string, ProfileData> Store = new Dictionary<string, ProfileData>();
            public int Saves;
            UniTaskCompletionSource? _blocked;
            public bool ThrowOnLoad;
            bool _blockNext;

            public void BlockNextSave() => _blockNext = true;
            public void ReleaseBlockedSave() => _blocked?.TrySetResult();

            public UniTask<ProfileData?> LoadAsync(string id, CancellationToken ct)
            {
                if (ThrowOnLoad) throw new System.IO.IOException("disque indisponible");
                return UniTask.FromResult(Store.TryGetValue(id, out var p) ? p : (ProfileData?)null);
            }

            public UniTask SaveAsync(string id, ProfileData data, CancellationToken ct)
            {
                Store[id] = data;
                Saves++;
                if (!_blockNext) return UniTask.CompletedTask;
                _blockNext = false;
                _blocked = new UniTaskCompletionSource();
                return _blocked.Task;
            }

            public void Delete(string id) => Store.Remove(id);
        }

        public sealed class FakeAuth : IAuthGateway
        {
            public bool ThrowOnSignIn;
            public bool HonourCancellation;

            public UniTask<Result<PlayerAccount>> SignInAsGuestAsync(CancellationToken ct)
            {
                if (HonourCancellation) ct.ThrowIfCancellationRequested();
                if (ThrowOnSignIn) throw new System.Net.WebException("réseau coupé");
                return UniTask.FromResult(Result<PlayerAccount>.Ok(new PlayerAccount("p1", "Invité-TEST")));
            }
            public void SignOut() { }
        }

        public sealed class Seeds : ISeedProvider { ulong _n = 11; public ulong NextSeed(string p) => _n += 7919; }
        public sealed class Clock : IClock
        {
            public DateTime UtcNow { get; set; } = new DateTime(2026, 9, 30, 12, 0, 0, DateTimeKind.Utc);
            public DateTime LocalNow { get; set; } = new DateTime(2026, 9, 30, 12, 0, 0);
        }
        public sealed class Log : ILogger { public void Info(string m) { } public void Warn(string m) { } public void Error(string m) { } }
        public sealed class Verifier : ICombatVerifier
        {
            public UniTask<bool> VerifyAsync(CombatSetup s, IReadOnlyList<CombatInput> i, ulong h, CancellationToken ct)
                => UniTask.FromResult(CombatReplay.Run(TestContent.Build(), s, i).StateHash == h);
        }

        readonly IServiceProvider _bus;
        readonly Dictionary<Type, List<object>> _received = new Dictionary<Type, List<object>>();

        public GameSession Session { get; }
        public MemoryProfiles Profiles { get; } = new MemoryProfiles();
        public FakeAuth Auth { get; } = new FakeAuth();
        public Clock Time { get; } = new Clock();
        public Seeds SeedSource { get; } = new Seeds();
        public Log Logger { get; } = new Log();
        public Verifier CombatVerifier { get; } = new Verifier();
        public SaveScheduler Save { get; }
        public ZoneEffects Effects { get; }
        public ZoneUseCases Zone { get; }
        public CombatUseCases Combat { get; }
        public HubUseCases Hub { get; }
        public AuthUseCases Account { get; }
        public ShopUseCases Shop { get; }
        public MetaContext MetaCtx { get; }
        public PassProgress Pass { get; }
        public DailyMissions Daily { get; }
        public SecondaryObjectives Secondary { get; }
        public Expeditions Operations { get; }
        public ResearchLab Lab { get; }
        public Workshop Workshop { get; }
        public LocalFriendsPort Friends { get; }
        public PlayerProfile Profile { get; }
        public N29.HeroesDawn.Application.Features.World.ChapterBook Chapters { get; }
        public ArchiveBook Archives { get; }
        public MetaTracker Tracker { get; }
        public Boutique Boutique { get; }
        public N29.HeroesDawn.Application.Features.Collection.CollectionBook Collection { get; }
        public MetaOverview Overview { get; }
        public PlayerSettings Settings { get; }
        public TutorialGuide Tutorial { get; }
        public QuestLog Quests { get; }
        public InstantPurchase Purchases { get; } = new InstantPurchase();

        public sealed class InstantPurchase : IPurchaseGateway
        {
            public bool Fail;
            public UniTask<Result> PurchaseAsync(string productId, CancellationToken ct)
                => UniTask.FromResult(Fail ? Result.Fail(MetaErrors.PaymentFailed) : Result.Ok());
        }

        public TestApp()
        {
            var b = new BuiltinContainerBuilder();
            b.AddMessagePipe();
            b.AddMessageBroker<WalletChanged>();
            b.AddMessageBroker<RosterChanged>();
            b.AddMessageBroker<ToastRequested>();
            b.AddMessageBroker<SaveStateChanged>();
            b.AddMessageBroker<ZoneValidated>();
            b.AddMessageBroker<CombatFinished>();
            b.AddMessageBroker<NavigateRequested>();
            b.AddMessageBroker<CombatResultsClosed>();
            b.AddMessageBroker<ReturnedToBoard>();
            b.AddMessageBroker<ShopChanged>();
            b.AddMessageBroker<MetaChanged>();
            b.AddMessageBroker<MetaRewardEarned>();
            b.AddMessageBroker<GameStateChanged>();
            _bus = b.BuildServiceProvider();
            Record<WalletChanged>();
            Record<RosterChanged>();
            Record<ToastRequested>();
            Record<SaveStateChanged>();
            Record<ZoneValidated>();
            Record<CombatFinished>();
            Record<NavigateRequested>();
            Record<CombatResultsClosed>();
            Record<ReturnedToBoard>();
            Record<ShopChanged>();
            Record<MetaChanged>();
            Record<MetaRewardEarned>();

            Session = new GameSession(TestContent.BuildCore());
            Save = new SaveScheduler(Session, Profiles, Time, Logger, Publisher<SaveStateChanged>());
            Effects = new ZoneEffects(Session, Publisher<WalletChanged>(), Publisher<ZoneValidated>(), Save, Logger);
            Zone = new ZoneUseCases(this);
            Combat = new CombatUseCases(this);
            Hub = new HubUseCases(this);
            Account = new AuthUseCases(this);
            Shop = new ShopUseCases(this);
            MetaCtx = new MetaContext(Session, Save, Time, Publisher<WalletChanged>(), Publisher<MetaChanged>());
            Pass = new PassProgress(MetaCtx);
            Daily = new DailyMissions(MetaCtx, Pass);
            Secondary = new SecondaryObjectives(MetaCtx, Pass);
            Operations = new Expeditions(MetaCtx, Pass);
            Lab = new ResearchLab(MetaCtx);
            Workshop = new Workshop(MetaCtx);
            Friends = new LocalFriendsPort(MetaCtx);
            Profile = new PlayerProfile(MetaCtx, Pass);
            Chapters = new N29.HeroesDawn.Application.Features.World.ChapterBook(MetaCtx);
            Archives = new ArchiveBook(MetaCtx);
            Boutique = new Boutique(MetaCtx, Purchases, Publisher<RosterChanged>());
            Collection = new N29.HeroesDawn.Application.Features.Collection.CollectionBook(MetaCtx, Publisher<RosterChanged>());
            Quests = new QuestLog(MetaCtx, Pass);
            Overview = new MetaOverview(Pass, Daily, Secondary, Operations, Lab, Archives, Boutique, Quests);
            Settings = new PlayerSettings(MetaCtx);
            Tutorial = new TutorialGuide(MetaCtx);
            Tracker = new MetaTracker(MetaCtx, Daily, Pass, Secondary, Quests, Subscriber<CombatFinished>(), Subscriber<ZoneValidated>(), Subscriber<RosterChanged>(),
                                      Subscriber<WalletChanged>(), Subscriber<GameStateChanged>(), Publisher<MetaRewardEarned>());
        }

        public IPublisher<T> Publisher<T>() => _bus.GetRequiredService<IPublisher<T>>();
        public ISubscriber<T> Subscriber<T>() => _bus.GetRequiredService<ISubscriber<T>>();

        void Record<T>()
        {
            var list = new List<object>();
            _received[typeof(T)] = list;
            Subscriber<T>().Subscribe(m => list.Add(m!));
        }

        public IReadOnlyList<T> Received<T>()
        {
            var result = new List<T>();
            foreach (var o in _received[typeof(T)]) result.Add((T)o);
            return result;
        }

        /// <summary>Pose un compte sans passer par l'authentification.</summary>
        public void SignInDirect(string playerId) => Session.Account = new PlayerAccount(playerId, "Invité-TEST");

        public sealed class ZoneUseCases
        {
            public readonly IStartZoneUseCase Start;
            public readonly IRollDiceUseCase Roll;
            public readonly IUseBonusDieUseCase Bonus;
            public readonly IMoveToUseCase Move;
            public readonly IEndMoveUseCase End;
            public readonly IResolveMiniGameUseCase MiniGame;
            public readonly IPlanActivityUseCase PlanActivity;
            public readonly IResolveActivityUseCase Activity;
            public readonly IResolveNarrativeUseCase Narrative;
            public readonly IResolveRestUseCase Rest;
            public readonly ILeaveZoneUseCase Leave;

            public ZoneUseCases(TestApp a)
            {
                var roster = a.Publisher<RosterChanged>();
                Start = new StartZoneUseCase(a.Session, a.SeedSource, a.Logger);
                Roll = new RollDiceUseCase(a.Effects);
                Bonus = new UseBonusDieUseCase(a.Effects);
                Move = new MoveToUseCase(a.Effects);
                End = new EndMoveUseCase(a.Effects);
                Activity = new ResolveActivityUseCase(a.Effects);
                PlanActivity = new PlanActivityUseCase(a.Effects);
                MiniGame = new ResolveMiniGameUseCase(Activity);
                Narrative = new ResolveNarrativeUseCase(a.Effects, roster);
                Rest = new ResolveRestUseCase(a.Effects, roster);
                Leave = new LeaveZoneUseCase(a.Session, a.Save, a.Logger);
            }
        }

        public sealed class CombatUseCases
        {
            public readonly ActiveCombat Active = new ActiveCombat();
            public readonly IBeginCombatUseCase Begin;
            public readonly ISetAutoCombatUseCase SetAuto;
            public readonly IFinishCombatUseCase Finish;
            public readonly IVerifyCombatUseCase Verify;
            public readonly IForceCombatWinUseCase ForceWin;

            public CombatUseCases(TestApp a)
            {
                Begin = new BeginCombatUseCase(a.Session, a.SeedSource, Active, a.Logger);
                SetAuto = new SetAutoCombatUseCase(a.Session, Active, a.Save);
                Finish = new FinishCombatUseCase(Active, a.Effects, a.Publisher<RosterChanged>(), a.Publisher<CombatFinished>(), a.Zone.Leave);
                Verify = new VerifyCombatUseCase(Active, a.CombatVerifier, a.Logger);
                ForceWin = new ForceCombatWinUseCase(Active);
            }
        }

        public sealed class HubUseCases
        {
            public readonly IToggleSquadUseCase ToggleSquad;
            public readonly ITrainMemberUseCase Train;
            public readonly IHealRosterUseCase Heal;
            public readonly IHealMemberUseCase HealMember;
            public readonly SquadPresets Presets;

            public HubUseCases(TestApp a)
            {
                var w = a.Publisher<WalletChanged>();
                var r = a.Publisher<RosterChanged>();
                ToggleSquad = new ToggleSquadUseCase(a.Session, a.Save, r);
                Train = new TrainMemberUseCase(a.Session, a.Save, w, r);
                Heal = new HealRosterUseCase(a.Session, a.Save, w, r);
                HealMember = new HealMemberUseCase(a.Session, a.Save, w, r);
                Presets = new SquadPresets(a.Session, a.Save, r);
            }
        }

        public sealed class AuthUseCases
        {
            public readonly ISignInAsGuestUseCase SignIn;
            public readonly ISignOutUseCase SignOut;
            public readonly IResetProgressUseCase Reset;

            public AuthUseCases(TestApp a)
            {
                var w = a.Publisher<WalletChanged>();
                var r = a.Publisher<RosterChanged>();
                SignIn = new SignInAsGuestUseCase(a.Session, a.Auth, a.Profiles, a.Time, a.Save, w, r, a.Logger);
                SignOut = new SignOutUseCase(a.Session, a.Auth, a.Save);
                Reset = new ResetProgressUseCase(a.Session, a.Profiles, a.Save, w, r);
            }
        }

        public sealed class ShopUseCases
        {
            public readonly IOpenShopUseCase Open;
            public readonly IRecruitFromShopUseCase Recruit;
            public readonly IRerollShopUseCase Reroll;
            public readonly IMarkShopSeenUseCase MarkSeen;

            public ShopUseCases(TestApp a)
            {
                var shop = a.Publisher<ShopChanged>();
                Open = new OpenShopUseCase(a.Session, a.Time, a.Save, shop);
                Recruit = new RecruitFromShopUseCase(a.Session, a.Time, a.Save, Open, a.Publisher<WalletChanged>(), a.Publisher<RosterChanged>(), shop);
                Reroll = new RerollShopUseCase(a.Session, a.Time, a.Save, Open, a.Publisher<WalletChanged>(), shop);
                MarkSeen = new MarkShopSeenUseCase(a.Session, a.Save, shop);
            }
        }

        /// <summary>Place le pion de la zone 1.1 (brouillard levé) avec <paramref name="pm"/> PM en main.</summary>
        public ZoneRun Place(string position, int pm)
        {
            var zone = Session.Content.Zone(PrototypeContent.Zone11);
            var run = ZoneRun.ForTests(Session.Content, zone, 3UL, pm, true, position);
            Session.Run = run;
            Session.RunZone = zone;
            return run;
        }

        /// <summary>
        /// Joue la zone 1.1 directement sur le domaine jusqu'à la case n23 gagnée (2 objectifs sur 3) ;
        /// il ne reste qu'à atteindre n24 avec 1 PM en main.
        /// </summary>
        public void DriveZone11ToLastStep()
        {
            var run = Place("n09", 1);
            Step(run, "n08", ZoneRunPhase.InCaseInteraction); run.ResolveCase(true);          // générateur (mini-jeu)
            run.SetPmForTests(2); Step(run, "n10", ZoneRunPhase.InCaseInteraction); run.ResolveCase(false);
            run.SetPmForTests(1); Step(run, "n11", ZoneRunPhase.InCombat); run.ResolveCombat(true);
            run.SetPmForTests(2); Step(run, "n13", ZoneRunPhase.InCaseInteraction); run.ResolveCase(true);
            run.SetPmForTests(2); Step(run, "n18", ZoneRunPhase.AwaitingRoll);
            run.SetPmForTests(1); Step(run, "n19", ZoneRunPhase.InCombat); run.ResolveCombat(true);
            run.SetPmForTests(1); Step(run, "n20", ZoneRunPhase.InCaseInteraction); run.ResolveCase(true);
            run.SetPmForTests(1); Step(run, "n23", ZoneRunPhase.InCombat); run.ResolveCombat(true);   // entrepôt (combat)
            run.SetPmForTests(1);
            if (!run.Destinations.ContainsKey(new CaseId("n24"))) throw new InvalidOperationException("n24 inaccessible");
        }

        static void Step(ZoneRun run, string to, ZoneRunPhase expected)
        {
            run.MoveTo(new CaseId(to));
            if (run.Phase != expected) throw new InvalidOperationException($"Après {to} : phase {run.Phase}, attendue {expected}");
        }
    }
}
