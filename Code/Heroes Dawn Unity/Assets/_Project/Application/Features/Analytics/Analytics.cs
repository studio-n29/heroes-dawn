#nullable enable
using System;
using System.Collections.Generic;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Ports;

namespace N29.HeroesDawn.Application.Features.Analytics
{
    /// <summary>Mesure désactivée (éditeur, tests, builds sans service).</summary>
    public sealed class NullAnalytics : IAnalytics
    {
        public void Track(string name, IReadOnlyDictionary<string, object>? parameters = null) { }
        public void SetConsent(bool granted) { }
    }

    /// <summary>
    /// Ne transmet un événement que si le joueur a répondu et accepté (opt-in : un profil qui n'a jamais été interrogé ne mesure rien,
    /// quelle que soit la valeur de son réglage). Un retrait du consentement arrête la collecte.
    /// </summary>
    public sealed class ConsentGatedAnalytics : IAnalytics
    {
        readonly IAnalytics _inner;
        readonly Func<bool> _consented;
        bool? _last;

        public ConsentGatedAnalytics(IAnalytics inner, Func<bool> consented) { _inner = inner; _consented = consented; }

        public void Track(string name, IReadOnlyDictionary<string, object>? parameters = null)
        {
            if (!_consented()) return;
            Sync();
            _inner.Track(name, parameters);
        }

        public void SetConsent(bool granted) { _last = granted; _inner.SetConsent(granted); }

        /// <summary>Aligne la collecte du service sur le consentement courant du joueur.</summary>
        public void Sync()
        {
            bool now = _consented();
            if (_last == now) return;
            _last = now;
            _inner.SetConsent(now);
        }
    }

    /// <summary>
    /// Passerelle : traduit les messages du jeu en événements de mesure. Aucun présentateur n'appelle <see cref="IAnalytics"/> directement.
    /// </summary>
    public sealed class AnalyticsBridge
    {
        readonly IAnalytics _analytics;
        readonly ConsentGatedAnalytics? _gate;
        readonly ISubscriber<GameStateChanged> _state;
        readonly ISubscriber<CombatFinished> _combat;
        readonly ISubscriber<ZoneValidated> _zone;
        readonly ISubscriber<MetaChanged> _meta;
        IDisposable? _subs;

        public AnalyticsBridge(IAnalytics analytics, ISubscriber<GameStateChanged> state, ISubscriber<CombatFinished> combat,
                               ISubscriber<ZoneValidated> zone, ISubscriber<MetaChanged> meta)
        {
            _analytics = analytics;
            _gate = analytics as ConsentGatedAnalytics;
            _state = state; _combat = combat; _zone = zone; _meta = meta;
        }

        public void Start()
        {
            if (_subs != null) return;
            var bag = DisposableBag.CreateBuilder();
            _state.Subscribe(s => _analytics.Track(AnalyticsEvents.StateEntered, new Dictionary<string, object> { ["state"] = s.To.ToString(), ["from"] = s.From.ToString() })).AddTo(bag);
            _combat.Subscribe(c => _analytics.Track(AnalyticsEvents.CombatFinished, new Dictionary<string, object>
            {
                ["victory"] = c.Report.Victory, ["boss"] = c.Report.Boss, ["rank"] = c.Report.Rank.ToString(), ["credits"] = c.Report.Credits,
            })).AddTo(bag);
            _zone.Subscribe(z => _analytics.Track(AnalyticsEvents.ZoneValidated, new Dictionary<string, object>
            {
                ["first_clear"] = z.Rewards.FirstClear, ["credits"] = z.Rewards.Credits, ["pr"] = z.Rewards.Pr,
            })).AddTo(bag);
            // Un changement de réglage peut être un changement de consentement : la collecte du service suit.
            _meta.Subscribe(_ => _gate?.Sync()).AddTo(bag);
            _subs = bag.Build();
        }
    }
}
