#nullable enable
using System;
using N29.HeroesDawn.Presentation.Audio;

namespace N29.HeroesDawn.Tests.Presentation.Fakes
{
    /// <summary>Contexte musical réglable : ville, combat de boss, boutique ouverte.</summary>
    public sealed class FakeMusicContext : IMusicContext
    {
        bool _shop;

        public string? City { get; set; }
        public bool BossFight { get; set; }
        public bool ShopOpen => _shop;
        public event Action? Changed;

        public void SetShop(bool open)
        {
            _shop = open;
            Changed?.Invoke();
        }
    }
}
