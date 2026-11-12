using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Application.Features.Analytics;
using N29.HeroesDawn.Application.Features.Combat;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    public sealed class AnalyticsTests
    {
        sealed class Recorder : IAnalytics
        {
            public readonly List<(string Name, IReadOnlyDictionary<string, object>? Params)> Events = new List<(string, IReadOnlyDictionary<string, object>?)>();
            public readonly List<bool> Consents = new List<bool>();
            public void Track(string name, IReadOnlyDictionary<string, object>? parameters = null) => Events.Add((name, parameters));
            public void SetConsent(bool granted) => Consents.Add(granted);
        }

        [Test, Description("Vérifie qu'aucun événement n'est transmis sans consentement — parce que la mesure est opt-in (RGPD) : un profil jamais interrogé ne mesure rien")]
        public void WithoutConsent_NothingIsTracked()
        {
            var rec = new Recorder();
            var gate = new ConsentGatedAnalytics(rec, () => false);
            gate.Track(AnalyticsEvents.StateEntered);
            gate.Track(AnalyticsEvents.CombatFinished, new Dictionary<string, object> { ["victory"] = true });
            Assert.That(rec.Events, Is.Empty);
            Assert.That(rec.Consents, Is.Empty, "le service n'est même pas démarré");
        }

        [Test, Description("Vérifie qu'avec le consentement l'événement passe et que la collecte du service démarre une seule fois — parce que le service ne doit être initialisé qu'après le oui du joueur")]
        public void WithConsent_TracksAndStartsCollectionOnce()
        {
            var rec = new Recorder();
            var gate = new ConsentGatedAnalytics(rec, () => true);
            gate.Track("a"); gate.Track("b");
            Assert.That(rec.Events.Select(e => e.Name), Is.EqualTo(new[] { "a", "b" }));
            Assert.That(rec.Consents, Is.EqualTo(new[] { true }));
        }

        [Test, Description("Vérifie que retirer le consentement arrête la collecte et que les événements suivants sont ignorés — parce que le joueur doit pouvoir se rétracter à tout moment")]
        public void WithdrawingConsent_StopsCollection()
        {
            var rec = new Recorder();
            bool consent = true;
            var gate = new ConsentGatedAnalytics(rec, () => consent);
            gate.Track("a");
            consent = false;
            gate.Sync();
            gate.Track("b");
            Assert.That(rec.Events.Select(e => e.Name), Is.EqualTo(new[] { "a" }));
            Assert.That(rec.Consents, Is.EqualTo(new[] { true, false }));
        }

        [Test, Description("Vérifie qu'un profil neuf ou ancien n'a pas consenti par défaut — parce qu'un réglage hérité à « vrai » d'avant l'opt-in ne vaut pas accord")]
        public void NewSettings_HaveNotConsented()
        {
            var s = new SettingsSave();
            Assert.That(s.consentStats, Is.False);
            Assert.That(s.consentAnswered, Is.False);
        }

        [Test, Description("Vérifie que la passerelle traduit un changement d'état, un combat et une zone en événements nommés avec leurs paramètres — parce que les tableaux de bord reposent sur ces noms")]
        public void Bridge_TranslatesGameMessagesToEvents()
        {
            var app = new TestApp();
            var rec = new Recorder();
            var bridge = new AnalyticsBridge(rec, app.Subscriber<GameStateChanged>(), app.Subscriber<CombatFinished>(), app.Subscriber<ZoneValidated>(), app.Subscriber<MetaChanged>());
            bridge.Start();
            app.Publisher<GameStateChanged>().Publish(new GameStateChanged(N29.HeroesDawn.Application.Flow.GameState.Hub, N29.HeroesDawn.Application.Flow.GameState.Board));
            var names = rec.Events.Select(e => e.Name).ToList();
            Assert.That(names, Is.EqualTo(new[] { AnalyticsEvents.StateEntered }));
            Assert.That(rec.Events[0].Params!["state"], Is.EqualTo("Board"));
            Assert.That(rec.Events[0].Params!["from"], Is.EqualTo("Hub"));
        }

        [Test, Description("Vérifie que démarrer la passerelle deux fois ne double pas les événements — parce qu'un abonnement en double fausserait toutes les mesures")]
        public void Bridge_StartTwice_DoesNotDuplicate()
        {
            var app = new TestApp();
            var rec = new Recorder();
            var bridge = new AnalyticsBridge(rec, app.Subscriber<GameStateChanged>(), app.Subscriber<CombatFinished>(), app.Subscriber<ZoneValidated>(), app.Subscriber<MetaChanged>());
            bridge.Start(); bridge.Start();
            app.Publisher<GameStateChanged>().Publish(new GameStateChanged(N29.HeroesDawn.Application.Flow.GameState.Boot, N29.HeroesDawn.Application.Flow.GameState.Hub));
            Assert.That(rec.Events.Count, Is.EqualTo(1));
        }
    }
}
