#nullable enable
using N29.HeroesDawn.Presentation.Audio;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    public sealed class VoicePoolTests
    {
        const int Slots = 128;

        [Test, Description("Vérifie qu'une voix libre est prise en premier — parce qu'on ne coupe jamais un son quand une voix est disponible")]
        public void FreeVoice_IsUsedFirst()
        {
            var pool = new VoicePool(3, Slots);
            var playing = new[] { true, false, true };
            Assert.That(pool.Acquire(SfxId.Hit, 128, 3, 0f, 0f, playing), Is.EqualTo(1));
        }

        [Test, Description("Vérifie que le même effet déclenché deux fois dans son délai n'est joué qu'une fois — parce que plusieurs coups dans le même tick de combat saturent le son")]
        public void Cooldown_DropsRepeat()
        {
            var pool = new VoicePool(4, Slots);
            var playing = new bool[4];
            Assert.That(pool.Acquire(SfxId.Hit, 128, 3, 0.05f, 1f, playing), Is.GreaterThanOrEqualTo(0));
            Assert.That(pool.Acquire(SfxId.Hit, 128, 3, 0.05f, 1.02f, playing), Is.EqualTo(-1));
            Assert.That(pool.Acquire(SfxId.Hit, 128, 3, 0.05f, 1.06f, playing), Is.GreaterThanOrEqualTo(0));
        }

        [Test, Description("Vérifie que, toutes les voix occupées, un effet important vole la plus ancienne voix moins importante et qu'un effet mineur est abandonné — parce que le nombre de voix est borné sur mobile")]
        public void FullPool_StealsOldestLowerPriority()
        {
            var pool = new VoicePool(2, Slots);
            var playing = new bool[2];
            int a = pool.Acquire(SfxId.UiClick, 100, 2, 0f, 0f, playing);
            playing[a] = true;
            int b = pool.Acquire(SfxId.ToastInfo, 80, 2, 0f, 1f, playing);
            playing[b] = true;
            Assert.That(pool.Acquire(SfxId.AllyReady, 50, 2, 0f, 2f, playing), Is.EqualTo(-1));
            Assert.That(pool.Acquire(SfxId.Victory, 255, 1, 0f, 3f, playing), Is.EqualTo(a));
        }

        [Test, Description("Vérifie qu'au-delà de sa limite d'instances un effet relance sa plus ancienne instance au lieu d'occuper une voix libre — parce que dix impacts superposés n'apportent rien et privent les autres effets")]
        public void MaxInstances_ReusesOwnOldestVoice()
        {
            var pool = new VoicePool(4, Slots);
            var playing = new bool[4];
            int first = pool.Acquire(SfxId.Hit, 128, 2, 0f, 0f, playing);
            playing[first] = true;
            int second = pool.Acquire(SfxId.Hit, 128, 2, 0f, 1f, playing);
            playing[second] = true;
            Assert.That(pool.Acquire(SfxId.Hit, 128, 2, 0f, 2f, playing), Is.EqualTo(first));
        }
    }
}
