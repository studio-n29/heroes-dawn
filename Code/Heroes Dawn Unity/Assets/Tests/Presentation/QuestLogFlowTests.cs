#nullable enable
using System.Linq;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Tests.Domain.Application;
using N29.HeroesDawn.Tests.Presentation.Fakes;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    public sealed class QuestLogFlowTests
    {
        sealed class FakeQuestPopup : FakePopup, IQuestLogPopupView
        {
            UniTaskCompletionSource? _done;
            public QuestLogData Shown;
            public event System.Action<string>? ClaimClicked;
            public UniTask ShowAsync(QuestLogData data, System.Threading.CancellationToken ct) { Shown = data; _done = new UniTaskCompletionSource(); return _done.Task; }
            public void Refresh(QuestLogData data) => Shown = data;
            public void Claim(string id) => ClaimClicked?.Invoke(id);
            public void Close() => _done?.TrySetResult();
        }

        TestApp _app = null!;
        FakeModalHost _host = null!;
        FakeToasts _toasts = null!;
        QuestLogFlow _flow = null!;

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.SignInDirect("p1");
            _host = new FakeModalHost();
            _host.Register<IQuestLogPopupView>(() => new FakeQuestPopup());
            _toasts = new FakeToasts();
            _flow = new QuestLogFlow(_app.Quests, new ModalService(_host), new LocLocalization(), _toasts);
        }

        FakeQuestPopup Popup => (FakeQuestPopup)_host.Spawned.Single();

        [Test, Description("Vérifie que le suivi montre les quatre quêtes principales avec leur état et les quêtes secondaires débloquées avec leur progression — parce que c'est le contenu du suivi des quêtes du QG")]
        public void Log_ShowsMainAndSideQuests()
        {
            _flow.OpenAsync(_app.Session, default).Forget();
            var d = Popup.Shown;
            Assert.That(d.Main.Length, Is.EqualTo(4));
            Assert.That(d.Main.Select(m => m.Status), Is.EqualTo(new[] { 1, 0, 0, 0 }));
            Assert.That(d.Side.Single().Title, Is.EqualTo("Nettoyer les quais"));
            Assert.That(d.Side.Single().ProgressText, Is.EqualTo("0 / 3"));
        }

        [Test, Description("Vérifie que réclamer une quête terminée verse la récompense, annonce un message et met la liste à jour — parce que le joueur voit tout de suite le résultat")]
        public void Claim_RefreshesAndAnnounces()
        {
            _app.Quests.Record(QuestMetric.CombatsWon, 3);
            _flow.OpenAsync(_app.Session, default).Forget();
            Assert.That(Popup.Shown.Side.Single().Claimable, Is.True);
            int credits = _app.Session.Wallet.Credits;
            Popup.Claim("docks");
            Assert.That(_app.Session.Wallet.Credits, Is.GreaterThan(credits));
            Assert.That(Popup.Shown.Side.Single().Claimed, Is.True);
            Assert.That(_toasts.Shown.Any(m => m.Text.Contains("Récompense")), Is.True);
        }

        [Test, Description("Vérifie que fermer la fenêtre termine le flux et la retire — parce que Retour ferme le suivi")]
        public void Close_EndsTheFlow()
        {
            var run = _flow.OpenAsync(_app.Session, default);
            var popup = Popup;
            popup.Close();
            Assert.That(run.Status, Is.EqualTo(UniTaskStatus.Succeeded));
            Assert.That(_host.Despawned, Does.Contain(popup));
        }

        [Test, Description("Vérifie que chaque texte du suivi des quêtes existe — parce qu'une clé manquante s'afficherait telle quelle")]
        public void EveryQuestTextExists()
        {
            foreach (var q in QuestLog.Catalog) { Assert.That(Loc.Has(q.TitleKey), Is.True, q.TitleKey); Assert.That(Loc.Has(q.DescKey), Is.True, q.DescKey); }
            foreach (var key in new[] { "quests.title", "quests.main", "quests.side", "quests.side.empty", "quests.locked", "quests.main.done", "quests.state.active",
                                         "quests.state.done", "quests.state.locked", "quests.claim", "quests.claimed", "qg.fil.open" })
                Assert.That(Loc.Has(key), Is.True, key);
        }
    }
}
