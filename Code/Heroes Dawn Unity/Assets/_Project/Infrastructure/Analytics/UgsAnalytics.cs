#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Application.Ports;
using Unity.Services.Analytics;
using Unity.Services.Core;

namespace N29.HeroesDawn.Infrastructure.Analytics
{
    /// <summary>
    /// UGS Analytics. Inerte tant que le consentement n'est pas donné : le service n'est initialisé (et la collecte démarrée) qu'au premier
    /// <see cref="SetConsent"/> à vrai. Sans projet Unity Cloud lié (ProjectSettings.cloudProjectId vide), l'initialisation échoue :
    /// l'échec est journalisé une fois et la mesure reste muette, le jeu n'est jamais bloqué.
    /// </summary>
    public sealed class UgsAnalytics : IAnalytics
    {
        readonly ILogger _log;
        readonly string _build, _platform;
        bool _requested, _ready, _failed;

        public UgsAnalytics(ILogger log, string build, string platform) { _log = log; _build = build; _platform = platform; }

        public void SetConsent(bool granted)
        {
            if (granted) { _requested = true; InitializeAsync(); return; }
            _requested = false;
            if (!_ready) return;
            try { AnalyticsService.Instance.StopDataCollection(); }
            catch (Exception e) { _log.Warn("Analytics : arrêt de la collecte impossible : " + e.Message); }
        }

        public void Track(string name, IReadOnlyDictionary<string, object>? parameters = null)
        {
            if (!_requested || _failed) return;
            if (!_ready) { InitializeAsync(); return; }
            try
            {
                var e = new CustomEvent(name) { { "build", _build }, { "platform", _platform } };
                if (parameters != null) foreach (var kv in parameters) e.Add(kv.Key, kv.Value);
                AnalyticsService.Instance.RecordEvent(e);
            }
            catch (Exception ex) { _log.Warn("Analytics : événement « " + name + " » non envoyé : " + ex.Message); }
        }

        async void InitializeAsync()
        {
            if (_ready || _failed) return;
            try
            {
                if (UnityServices.State == ServicesInitializationState.Uninitialized) await UnityServices.InitializeAsync();
                AnalyticsService.Instance.StartDataCollection();
                _ready = true;
            }
            catch (Exception e)
            {
                _failed = true;
                _log.Warn("Analytics : service indisponible (projet Unity Cloud non lié ?) — mesure désactivée : " + e.Message);
            }
        }
    }
}
