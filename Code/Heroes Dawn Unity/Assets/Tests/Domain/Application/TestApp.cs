#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
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
            TaskCompletionSource<bool>? _blocked;
            bool _blockNext;

            public void BlockNextSave() => _blockNext = true;
            public void ReleaseBlockedSave() => _blocked?.TrySetResult(true);

            public Task<ProfileData?> LoadAsync(string id, CancellationToken ct)
                => Task.FromResult(Store.TryGetValue(id, out var p) ? p : (ProfileData?)null);

            public Task SaveAsync(string id, ProfileData data, CancellationToken ct)
            {
                Store[id] = data;
                Saves++;
                if (!_blockNext) return Task.CompletedTask;
                _blockNext = false;
                _blocked = new TaskCompletionSource<bool>();
                return _blocked.Task;
            }

            public void Delete(string id) => Store.Remove(id);
        }

        public sealed class FakeAuth : IAuthGateway
        {
            public Task<Result<PlayerAccount>> SignInAsGuestAsync(CancellationToken ct)
                => Task.FromResult(Result<PlayerAccount>.Ok(new PlayerAccount("p1", "Invité-TEST")));
            public void SignOut() { }
        }

        public sealed class Seeds : ISeedProvider { ulong _n = 11; public ulong NextSeed(string p) => _n += 7919; }
        public sealed class Clock : IClock { public DateTime UtcNow { get; set; } = new DateTime(2026, 9, 30, 12, 0, 0, DateTimeKind.Utc); }
        public sealed class Log : ILogger { public void Info(string m) { } public void Warn(string m) { } public void Error(string m) { } }
        public sealed class Verifier : ICombatVerifier
        {
            public Task<bool> VerifyAsync(CombatSetup s, IReadOnlyList<CombatInput> i, ulong h, CancellationToken ct)
                => Task.FromResult(CombatReplay.Run(PrototypeContent.Build(), s, i).StateHash == h);
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

        public TestApp()
        {
            var b = new BuiltinContainerBuilder();
            b.AddMessagePipe();
            b.AddMessageBroker<WalletChanged>();
            b.AddMessageBroker<RosterChanged>();
            b.AddMessageBroker<ToastRequested>();
            b.AddMessageBroker<SaveStateChanged>();
            _bus = b.BuildServiceProvider();
            Record<WalletChanged>();
            Record<RosterChanged>();
            Record<ToastRequested>();
            Record<SaveStateChanged>();

            Session = new GameSession(PrototypeContent.Build());
            Save = new SaveScheduler(Session, Profiles, Time, Logger, Publisher<SaveStateChanged>());
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
    }
}
