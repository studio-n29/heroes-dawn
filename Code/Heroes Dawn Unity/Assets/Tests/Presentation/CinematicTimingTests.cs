using N29.HeroesDawn.Presentation.Login;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    /// <summary>Temps de lecture des légendes de la cinématique quand aucune voix n'est enregistrée (lot G de la 0.14.0).</summary>
    public sealed class CinematicTimingTests
    {
        static string Words(int n) => string.Join(" ", System.Linq.Enumerable.Repeat("mot", n));

        [Test, Description("Vérifie que le nombre de mots ignore les espaces multiples et le texte vide — parce que le temps de lecture se calcule sur les mots")]
        public void Words_CountsTheWords()
        {
            Assert.That(CinematicTiming.Words("a b  c"), Is.EqualTo(3));
            Assert.That(CinematicTiming.Words(""), Is.EqualTo(0));
            Assert.That(CinematicTiming.Words("   "), Is.EqualTo(0));
        }

        [Test, Description("Vérifie que sans voix la durée d'un plan suit le temps de lecture (1,2 s + 0,35 s par mot) sans descendre sous son minimum, et que sans légende elle reste son minimum — parce que la légende doit rester lisible")]
        public void ShotSeconds_WithoutVoice_FollowsTheReadingTime()
        {
            Assert.That(CinematicTiming.ShotSeconds(3f, 0f, ""), Is.EqualTo(3f).Within(0.001f));
            Assert.That(CinematicTiming.ShotSeconds(3f, 0f, Words(20)), Is.EqualTo(8.2f).Within(0.001f));
            Assert.That(CinematicTiming.ShotSeconds(3f, 0f, "un deux"), Is.EqualTo(3f).Within(0.001f));
        }

        [Test, Description("Vérifie qu'avec une voix la durée suit la voix (0,4 s avant, 0,4 s après) — parce que la voix, quand elle existe, commande le plan")]
        public void ShotSeconds_WithVoice_FollowsTheVoice()
        {
            Assert.That(CinematicTiming.ShotSeconds(3f, 5f, "texte"), Is.EqualTo(5.8f).Within(0.001f));
            Assert.That(CinematicTiming.ShotSeconds(8f, 2f, "texte"), Is.EqualTo(8f).Within(0.001f));
        }

        [Test, Description("Vérifie que la légende apparaît en fondu, reste lisible puis disparaît avant la fin du plan, avec ou sans voix — parce qu'elle ne doit pas rester à l'écran sur le plan suivant")]
        public void SubtitleAlpha_FadesInAndOut()
        {
            Assert.That(CinematicTiming.SubtitleAlpha(0.2f, 0f, 8f), Is.EqualTo(0f).Within(0.001f));
            Assert.That(CinematicTiming.SubtitleAlpha(2f, 0f, 8f), Is.EqualTo(1f).Within(0.001f));
            Assert.That(CinematicTiming.SubtitleAlpha(7.8f, 0f, 8f), Is.EqualTo(0f).Within(0.001f));
            Assert.That(CinematicTiming.SubtitleAlpha(2f, 4f, 5f), Is.EqualTo(1f).Within(0.001f));
            Assert.That(CinematicTiming.SubtitleAlpha(4.8f, 4f, 5f), Is.EqualTo(0f).Within(0.001f));
        }
    }
}
