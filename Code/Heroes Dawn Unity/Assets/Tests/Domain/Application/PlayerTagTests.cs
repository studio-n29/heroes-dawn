using System.Linq;
using N29.HeroesDawn.Application.Features.Meta;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    /// <summary>Tag personnel du joueur (« Pseudo#1234 ») et lecture des tags d'amis.</summary>
    public sealed class PlayerTagTests
    {
        [Test, Description("Vérifie que le tag est stable pour un même identifiant (valeurs repères p1 → 1274, player-2 → 7023) — parce qu'un ami doit retrouver toujours le même tag")]
        public void Of_IsStableAndPinned()
        {
            Assert.That(PlayerTag.Of("p1", "Dorian"), Is.EqualTo("Dorian#1274"));
            Assert.That(PlayerTag.Of("player-2", "Dorian"), Is.EqualTo("Dorian#7023"));
            Assert.That(PlayerTag.Of("p1", "Dorian"), Is.EqualTo(PlayerTag.Of("p1", "Dorian")));
        }

        [Test, Description("Vérifie que le pseudo est nettoyé (espaces, #, vide → Invité, 20 caractères au plus) — parce que le tag doit toujours être lisible et valide")]
        public void Of_CleansThePseudo()
        {
            Assert.That(PlayerTag.Of("p1", "  Ma#ra  "), Is.EqualTo("Mara#1274"));
            Assert.That(PlayerTag.Of("p1", ""), Is.EqualTo("Invité#1274"));
            Assert.That(PlayerTag.Of("p1", "   "), Is.EqualTo("Invité#1274"));
            Assert.That(PlayerTag.Of("p1", "#"), Is.EqualTo("Invité#1274"));
            string longName = new string('x', 30);
            Assert.That(PlayerTag.Of("p1", longName), Is.EqualTo(new string('x', PlayerTag.MaxNameLength) + "#1274"));
        }

        [Test, Description("Vérifie que tout tag produit se relit (code de 0 à 9999) pour des identifiants et pseudos variés — parce qu'un tag affiché doit toujours être un tag valide")]
        public void Of_AlwaysParses()
        {
            foreach (var pseudo in new[] { "", "a", "Ñandú" })
                for (int i = 0; i < 200; i++)
                {
                    string tag = PlayerTag.Of("id" + i, pseudo);
                    Assert.That(PlayerTag.TryParse(tag, out _, out int code), Is.True, tag);
                    Assert.That(code, Is.InRange(0, 9999));
                }
        }

        [Test, Description("Vérifie que seul le format « nom#4 chiffres » est accepté (nom de 1 à 20 caractères sans #) — parce qu'un ami s'ajoute en tapant son tag")]
        public void TryParse_AcceptsOnlyNameHashFourDigits()
        {
            Assert.That(PlayerTag.TryParse("Mara#0042", out var name, out int code), Is.True);
            Assert.That((name, code), Is.EqualTo(("Mara", 42)));
            Assert.That(PlayerTag.TryParse("  Mara#0042 ", out name, out code), Is.True);
            Assert.That((name, code), Is.EqualTo(("Mara", 42)));
            foreach (var bad in new[] { "Mara", "Mara#42", "Mara#00420", "#0042", "Mara#00a2", "Ma#ra#0042", "", new string('x', 21) + "#0042" })
                Assert.That(PlayerTag.TryParse(bad, out _, out _), Is.False, bad);
        }

        [Test, Description("Vérifie que le code est complété par des zéros — parce que le format est toujours sur quatre chiffres")]
        public void Format_PadsTheCode()
        {
            Assert.That(PlayerTag.Format("Mara", 7), Is.EqualTo("Mara#0007"));
        }
    }
}
