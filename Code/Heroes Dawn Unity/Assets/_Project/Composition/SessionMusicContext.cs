#nullable enable
using System;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Audio;

namespace N29.HeroesDawn.Composition
{
    /// <summary>
    /// Contexte musical lu dans la session : ville de la zone en cours (<c>RunZone.Borough</c> → ville), combat de boss en attente,
    /// et page Boutique signalée par le hub.
    /// </summary>
    public sealed class SessionMusicContext : IMusicContext, IMusicSignals
    {
        readonly GameSession _session;
        bool _shopOpen;

        public SessionMusicContext(GameSession session) => _session = session;

        public string? City => _session.RunZone == null ? null : StoryRoutes.CityOfBorough(_session.RunZone.Borough);
        public bool BossFight => _session.Run?.PendingCase?.Rank == CombatRank.Boss;
        public bool ShopOpen => _shopOpen;
        public event Action? Changed;

        public void SetShopOpen(bool open)
        {
            if (_shopOpen == open) return;
            _shopOpen = open;
            Changed?.Invoke();
        }
    }
}
