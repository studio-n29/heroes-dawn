using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Account;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Infrastructure.Simulated;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    /// <summary>Envoi différé du profil vers le cloud : regroupement, un envoi à la fois, vidage, conflit, réessais, hors ligne.</summary>
    public sealed class CloudSyncServiceTests
    {
        static readonly CancellationToken Ct = CancellationToken.None;

        /// <summary>Cloud espion : compte les envois, mesure la concurrence, peut échouer N fois ou être bloqué.</summary>
        sealed class SpyCloud : ICloudProfileGateway
        {
            public readonly FakeCloudProfileGateway Inner = new FakeCloudProfileGateway();
            public int Calls, Concurrent, MaxConcurrent, FailCount;
            public UniTaskCompletionSource? Gate;

            public UniTask<Result<CloudProfile?>> GetAsync(CancellationToken ct) => Inner.GetAsync(ct);

            public async UniTask<Result<CloudWriteResult>> UpsertAsync(ProfileData profile, string? expectedUpdatedAt, CancellationToken ct)
            {
                Calls++;
                Concurrent++; MaxConcurrent = Math.Max(MaxConcurrent, Concurrent);
                try
                {
                    if (Gate != null) await Gate.Task;
                    if (FailCount > 0) { FailCount--; return Result<CloudWriteResult>.Fail(new DomainError("cloud.unavailable", "errors.account_cloud")); }
                    return await Inner.UpsertAsync(profile, expectedUpdatedAt, ct);
                }
                finally { Concurrent--; }
            }
        }

        TestApp _app = null!;
        FixedClock _clock = null!;
        SpyCloud _cloud = null!;
        MemorySyncStateStore _state = null!;
        CloudSyncService _sync = null!;
        List<TimeSpan> _waits = null!;
        UniTaskCompletionSource? _delayGate;

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.SignInDirect("guest-1");
            _clock = new FixedClock();
            _cloud = new SpyCloud();
            _state = new MemorySyncStateStore();
            _waits = new List<TimeSpan>();
            _delayGate = null;
            _sync = new CloudSyncService(_cloud, _state, _app.Session, _clock, _app.Logger, Delay, _app.Save);
        }

        UniTask Delay(TimeSpan t, CancellationToken ct)
        {
            _waits.Add(t);
            if (_delayGate != null) return _delayGate.Task;
            _clock.Advance(t);
            return UniTask.CompletedTask;
        }

        /// <summary>Active la synchronisation comme le fait le rattachement : première écriture faite, date mémorisée.</summary>
        void Connect()
        {
            var first = _cloud.Inner.UpsertAsync(_app.Session.ToProfile(_clock.UtcNow), null, Ct).GetAwaiter().GetResult().Value;
            _sync.Start(first.UpdatedAt);
            _cloud.Calls = 0;
        }

        [Test, Description("Vérifie que la première sauvegarde locale après une pause part tout de suite et que les suivantes, rapprochées, sont regroupées en un seul envoi après 20 s — parce qu'au plus un envoi toutes les 20 secondes")]
        public void LocalSaves_AreBatched_AtMostOneUploadPer20Seconds()
        {
            Connect();
            _sync.NotifyLocalSaved();
            Assert.That(_cloud.Calls, Is.EqualTo(1), "premier envoi immédiat");
            _delayGate = new UniTaskCompletionSource();
            for (int i = 0; i < 5; i++) _sync.NotifyLocalSaved();
            Assert.That(_cloud.Calls, Is.EqualTo(1), "regroupés : rien n'est parti pendant l'attente");
            Assert.That(_waits.Last(), Is.LessThanOrEqualTo(TimeSpan.FromSeconds(20)));
            _delayGate.TrySetResult();
            Assert.That(_cloud.Calls, Is.EqualTo(2), "un seul envoi pour les cinq sauvegardes");
        }

        [Test, Description("Vérifie qu'un seul envoi est en cours à la fois et que les modifications arrivées pendant l'envoi partent dans le suivant — parce que deux écritures simultanées se marcheraient dessus")]
        public void OnlyOneUploadAtATime_ChangesDuringUploadGoInTheNext()
        {
            Connect();
            _cloud.Gate = new UniTaskCompletionSource();
            _sync.NotifyLocalSaved();
            Assert.That(_cloud.Calls, Is.EqualTo(1));
            _sync.NotifyLocalSaved();
            _sync.NotifyLocalSaved();
            Assert.That(_cloud.Calls, Is.EqualTo(1), "l'envoi en cours n'est pas doublé");
            _cloud.Gate.TrySetResult();
            Assert.That(_cloud.Calls, Is.EqualTo(2), "les changements survenus pendant l'envoi partent ensuite");
            Assert.That(_cloud.MaxConcurrent, Is.EqualTo(1));
        }

        [Test, Description("Vérifie que le vidage (fermeture, arrière-plan) envoie immédiatement sans attendre les 20 s — parce que la dernière progression ne doit pas se perdre à la fermeture")]
        public void Flush_SendsImmediately()
        {
            Connect();
            _sync.NotifyLocalSaved();
            _delayGate = new UniTaskCompletionSource();
            _sync.NotifyLocalSaved();           // en attente des 20 s
            int before = _cloud.Calls;
            var flushed = _sync.FlushAsync(Ct).GetAwaiter().GetResult();
            Assert.That(flushed.IsSuccess, Is.True);
            Assert.That(_cloud.Calls, Is.EqualTo(before + 1), "envoi immédiat malgré l'attente");
        }

        [Test, Description("Vérifie qu'une écriture refusée parce que le compte a changé arrête les envois, lève l'événement de conflit et n'écrase rien — parce que c'est le garde-fou anti-perte")]
        public void Conflict_RaisesTheEvent_AndStopsUploading()
        {
            Connect();
            _cloud.Inner.SeedFromOtherDevice(new ProfileData { credits = 999 });   // un autre appareil a écrit
            int conflicts = 0;
            _sync.ConflictDetected += () => conflicts++;
            _sync.NotifyLocalSaved();
            Assert.That(conflicts, Is.EqualTo(1));
            Assert.That(_cloud.Inner.GetAsync(Ct).GetAwaiter().GetResult().Value!.Profile.credits, Is.EqualTo(999), "rien n'a été écrasé");
            int calls = _cloud.Calls;
            _sync.NotifyLocalSaved();
            Assert.That(_cloud.Calls, Is.EqualTo(calls), "plus d'envoi tant que le choix n'est pas fait");
            Assert.That(_sync.IsPaused, Is.True);
        }

        [Test, Description("Vérifie qu'après un échec le service réessaie à 5 s, 20 s puis 60 s, réussit si le réseau revient, et abandonne après le quatrième échec jusqu'à la prochaine sauvegarde — parce que le réseau mobile est instable")]
        public void Failure_RetriesAt5_20_60Seconds_ThenWaitsForTheNextSave()
        {
            Connect();
            _cloud.FailCount = 3;
            _sync.NotifyLocalSaved();
            Assert.That(_cloud.Calls, Is.EqualTo(4), "essai initial + 3 réessais");
            Assert.That(_waits.TakeLast(3).Select(w => (int)w.TotalSeconds), Is.EqualTo(new[] { 5, 20, 60 }));
            Assert.That(_cloud.Inner.Writes, Is.GreaterThan(1), "le 4e essai a réussi");

            _cloud.FailCount = 100;
            _waits.Clear();
            _sync.NotifyLocalSaved();
            int callsAfterGiveUp = _cloud.Calls;
            Assert.That(_waits.TakeLast(3).Select(w => (int)w.TotalSeconds), Is.EqualTo(new[] { 5, 20, 60 }));
            _cloud.FailCount = 0;
            _sync.NotifyLocalSaved();
            Assert.That(_cloud.Calls, Is.GreaterThan(callsAfterGiveUp), "la sauvegarde suivante relance l'envoi");
        }

        [Test, Description("Vérifie que sans compte connecté le service ne contacte jamais le cloud — parce que l'invité reste purement local")]
        public void NoAccount_NeverCallsTheCloud()
        {
            _sync.NotifyLocalSaved();
            _sync.FlushAsync(Ct).GetAwaiter().GetResult();
            Assert.That(_cloud.Calls, Is.EqualTo(0));
        }

        [Test, Description("Vérifie que la sauvegarde locale réussit même quand le cloud est en panne (le profil local est écrit, aucune exception) — parce que la partie ne dépend jamais du réseau")]
        public void OfflineFailure_NeverBlocksLocalSaves()
        {
            Connect();
            _cloud.FailCount = 100;
            Assert.DoesNotThrow(() => _app.Save.SaveAsync(Ct).GetAwaiter().GetResult());
            Assert.That(_app.Profiles.Saves, Is.GreaterThan(0), "le profil local a bien été écrit");
        }

        [Test, Description("Vérifie que la sauvegarde locale prévient le service de synchronisation une fois le profil local écrit — parce que c'est ce qui déclenche l'envoi")]
        public void SaveScheduler_NotifiesTheCloudSync_AfterALocalSave()
        {
            Connect();
            _app.Save.SaveAsync(Ct).GetAwaiter().GetResult();
            Assert.That(_cloud.Calls, Is.EqualTo(1));
        }

        [Test, Description("Vérifie que l'envoi réussi mémorise la nouvelle date de dernière écriture dans l'état de synchronisation — parce que la date lue sert de garde-fou à l'envoi suivant")]
        public void SuccessfulUpload_StoresTheNewUpdatedAt()
        {
            Connect();
            _sync.NotifyLocalSaved();
            Assert.That(_state.Load()!.LastUpdatedAt, Is.EqualTo(_cloud.Inner.UpdatedAt));
        }
    }
}
