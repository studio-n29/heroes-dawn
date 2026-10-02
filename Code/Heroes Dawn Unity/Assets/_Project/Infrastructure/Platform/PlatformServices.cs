#nullable enable
using System;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Core;
using UnityEngine;

namespace N29.HeroesDawn.Infrastructure.Platform
{
    /// <summary>Graines locales (M2 : fournies par Cloud Code pour les combats et zones vérifiés).</summary>
    public sealed class LocalSeedProvider : ISeedProvider
    {
        readonly Pcg32 _rng;
        public LocalSeedProvider() : this((ulong)DateTime.UtcNow.Ticks) { }
        public LocalSeedProvider(ulong seed) { _rng = new Pcg32(seed, 7); }

        public ulong NextSeed(string purpose)
        {
            ulong hi = _rng.NextUInt();
            ulong lo = _rng.NextUInt();
            return (hi << 32) | lo;
        }
    }

    public sealed class SystemClock : IClock
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }

    public sealed class UnityLogger : Application.Ports.ILogger
    {
        public void Info(string message) => Debug.Log("[Heroes Dawn] " + message);
        public void Warn(string message) => Debug.LogWarning("[Heroes Dawn] " + message);
        public void Error(string message) => Debug.LogError("[Heroes Dawn] " + message);
    }
}
