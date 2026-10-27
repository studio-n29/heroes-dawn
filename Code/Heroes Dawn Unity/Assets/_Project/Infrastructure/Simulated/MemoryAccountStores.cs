#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Application.Ports;

namespace N29.HeroesDawn.Infrastructure.Simulated
{
    /// <summary>État de synchronisation gardé en mémoire (éditeur sans appareil, tests).</summary>
    public sealed class MemorySyncStateStore : ICloudSyncStateStore
    {
        CloudSyncState? _state;
        public CloudSyncState? Load() => _state;
        public void Save(CloudSyncState state) => _state = new CloudSyncState { UserId = state.UserId, LastUpdatedAt = state.LastUpdatedAt, LastSyncUtcTicks = state.LastSyncUtcTicks };
        public void Clear() => _state = null;
    }

    /// <summary>Sauvegardes de secours gardées en mémoire (tests).</summary>
    public sealed class MemoryProgressBackupStore : IProgressBackupStore
    {
        public readonly List<(string Reason, ProfileData Profile)> Saved = new List<(string Reason, ProfileData Profile)>();
        public void Save(string reason, ProfileData profile, DateTime utcNow) => Saved.Add((reason, profile));
    }
}
