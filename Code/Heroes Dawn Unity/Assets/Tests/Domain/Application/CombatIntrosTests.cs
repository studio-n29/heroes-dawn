using N29.HeroesDawn.Application.Features.Meta;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    /// <summary>Mémoire des entrées en combat déjà vues.</summary>
    public sealed class CombatIntrosTests
    {
        TestApp _app = null!;

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.SignInDirect("p1");
        }

        [Test, Description("Vérifie que la clé d'un affrontement ne dépend pas de l'ordre des ennemis — parce que la même rencontre doit être reconnue à chaque fois")]
        public void KeyOf_IgnoresTheOrder()
        {
            Assert.That(CombatIntros.KeyOf(new[] { "b", "a", "a" }), Is.EqualTo(CombatIntros.KeyOf(new[] { "a", "b", "a" })));
            Assert.That(CombatIntros.KeyOf(new[] { "a", "b" }), Is.Not.EqualTo(CombatIntros.KeyOf(new[] { "a", "c" })));
        }

        [Test, Description("Vérifie qu'une entrée vue est gardée dans la sauvegarde sans doublon — parce que le joueur retrouve ses introductions abrégées à la reconnexion")]
        public void MarkSeen_IsSavedOnce()
        {
            var intros = new CombatIntros(_app.MetaCtx);
            Assert.That(intros.Seen("a,b"), Is.False);
            intros.MarkSeen("a,b");
            intros.MarkSeen("a,b");
            Assert.That(intros.Seen("a,b"), Is.True);
            Assert.That(_app.Session.Meta.seenIntros, Is.EqualTo(new[] { "a,b" }));
        }
    }
}
