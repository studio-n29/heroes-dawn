#nullable enable
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Presentation.Audio;
using N29.HeroesDawn.Tests.Presentation.Fakes;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    public sealed class AudioDecoratorTests
    {
        [Test, Description("Vérifie qu'un toast s'affiche toujours et joue le son de sa nature — parce que les récompenses et erreurs du plateau passent toutes par les toasts")]
        public void Toast_ShowsAndPlaysKindSound()
        {
            var inner = new FakeToasts();
            var audio = new FakeAudio();
            var toasts = new AudibleToastService(inner, audio);
            toasts.Show("+50", ToastKind.Reward);
            toasts.Show("Oups", ToastKind.Error);
            Assert.That(inner.Shown.Count, Is.EqualTo(2));
            Assert.That(audio.Played, Is.EqualTo(new[] { SfxId.ToastReward, SfxId.UiError }));
        }

        [Test, Description("Vérifie qu'une modale atténue la musique à l'ouverture et la rétablit à la fermeture — parce que la musique ne doit pas couvrir une décision")]
        public void Popup_DucksMusicWhileOpen()
        {
            var host = new FakeModalHost();
            var audio = new FakeAudio();
            var audible = new AudiblePopupHost(host, audio);
            var view = audible.Spawn<FakePopup>();
            Assert.That(audio.Ducks[0].Layer, Is.EqualTo(DuckLayer.Modal));
            Assert.That(audio.Ducks[0].Level, Is.LessThan(1f));
            audible.Despawn(view);
            Assert.That(audio.Ducks[1], Is.EqualTo((DuckLayer.Modal, 1f)));
            Assert.That(audio.Played, Is.EqualTo(new[] { SfxId.PopupOpen, SfxId.PopupClose }));
            Assert.That(host.Despawned, Does.Contain(view));
        }
    }
}
