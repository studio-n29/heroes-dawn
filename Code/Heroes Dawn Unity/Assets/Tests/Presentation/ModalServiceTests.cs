using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Tests.Presentation.Fakes;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    public sealed class ModalServiceTests
    {
        FakeModalHost _host = null!;
        ModalService _modals = null!;

        [SetUp]
        public void SetUp()
        {
            _host = new FakeModalHost();
            _modals = new ModalService(_host);
        }

        [Test, Description("Vérifie qu'une modale demandée pendant qu'une autre est ouverte attend sa fermeture, puis s'ouvre — parce qu'au plus une modale est affichée à la fois (maquettes §3, Review Focus 1)")]
        public void Open_WhileOneOpen_WaitsUntilClosed()
        {
            var first = _modals.OpenAsync<FakePopup>(CancellationToken.None);
            Assert.That(first.Status, Is.EqualTo(UniTaskStatus.Succeeded));
            var second = _modals.OpenAsync<FakePopup>(CancellationToken.None);
            Assert.That(second.Status, Is.EqualTo(UniTaskStatus.Pending));
            Assert.That(_host.Spawned.Count, Is.EqualTo(1));

            _modals.Close(first.GetAwaiter().GetResult());
            Assert.That(second.Status, Is.EqualTo(UniTaskStatus.Succeeded));
            Assert.That(_host.Spawned.Count, Is.EqualTo(2));
            Assert.That(_modals.AnyOpen, Is.True);
        }

        [Test, Description("Vérifie que Retour sur une modale non annulable ne fait rien et la laisse ouverte — parce qu'un choix de case est obligatoire (spec §7)")]
        public void TryCancelTop_NonCancelable_ReturnsFalseAndKeepsModal()
        {
            var v = _modals.OpenAsync<FakePopup>(CancellationToken.None).GetAwaiter().GetResult();
            Assert.That(_modals.TryCancelTop(), Is.False);
            Assert.That(v.CancelRequests, Is.Zero);
            Assert.That(_modals.AnyOpen, Is.True);
        }

        [Test, Description("Vérifie que Retour annule seulement la modale affichée, pas celles en attente — parce que Retour ferme la modale du dessus (spec §7, Review Focus 1)")]
        public void TryCancelTop_Cancelable_RaisesCancelRequestedOnTopOnly()
        {
            var top = new FakePopup { Cancelable = true };
            var queued = new FakePopup { Cancelable = true };
            var n = 0;
            _host.Register<FakePopup>(() => n++ == 0 ? top : queued);
            _modals.OpenAsync<FakePopup>(CancellationToken.None);
            _modals.OpenAsync<FakePopup>(CancellationToken.None);

            Assert.That(_modals.TryCancelTop(), Is.True);
            Assert.That(top.CancelRequests, Is.EqualTo(1));
            Assert.That(queued.CancelRequests, Is.Zero);
        }

        [Test, Description("Vérifie qu'une demande annulée pendant son attente n'ouvre jamais de modale — parce qu'un écran détruit ne doit pas laisser une fenêtre orpheline (Review Focus 4)")]
        public void OpenAsync_CancelledWhileQueued_NeverSpawns()
        {
            var first = _modals.OpenAsync<FakePopup>(CancellationToken.None).GetAwaiter().GetResult();
            var cts = new CancellationTokenSource();
            var queued = _modals.OpenAsync<FakePopup>(cts.Token);
            cts.Cancel();
            Assert.That(queued.Status, Is.EqualTo(UniTaskStatus.Canceled));

            _modals.Close(first);
            Assert.That(_host.Spawned.Count, Is.EqualTo(1));
            Assert.That(_modals.AnyOpen, Is.False);
        }

        [Test, Description("Vérifie que Retour sur une confirmation répond « non » et la ferme — parce que Retour équivaut à Annuler (spec §7)")]
        public void ConfirmAsync_Back_ReturnsFalse()
        {
            var answer = _modals.ConfirmAsync(new ConfirmData("t", "b", "ok", "non", Tone.Danger), CancellationToken.None);
            Assert.That(_modals.TryCancelTop(), Is.True);
            Assert.That(answer.Status, Is.EqualTo(UniTaskStatus.Succeeded));
            Assert.That(answer.GetAwaiter().GetResult(), Is.False);
            Assert.That(_host.Despawned.Count, Is.EqualTo(1));
            Assert.That(_modals.AnyOpen, Is.False);
        }
    }
}
