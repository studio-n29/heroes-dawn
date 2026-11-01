#nullable enable
using N29.HeroesDawn.Application.Features.Meta;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    public sealed class PlayerAvatarsTests
    {
        [Test, Description("Vérifie qu'un avatar choisi dans la galerie est gardé, et qu'un avatar inconnu ou absent retombe sur le premier — parce que le personnage du joueur doit toujours avoir un visage")]
        public void Resolve_KeepsGalleryChoice_FallsBackToFirst()
        {
            Assert.That(PlayerAvatars.Resolve("avatar_4"), Is.EqualTo("avatar_4"));
            Assert.That(PlayerAvatars.Resolve("inconnu"), Is.EqualTo(PlayerAvatars.Gallery[0]));
            Assert.That(PlayerAvatars.Resolve(""), Is.EqualTo(PlayerAvatars.Gallery[0]));
            Assert.That(PlayerAvatars.Resolve(null), Is.EqualTo(PlayerAvatars.Gallery[0]));
        }
    }
}
