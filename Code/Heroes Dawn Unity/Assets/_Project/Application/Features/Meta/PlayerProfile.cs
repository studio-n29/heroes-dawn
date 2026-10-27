#nullable enable
using System.Linq;
using N29.HeroesDawn.Application.Features.Combat;

namespace N29.HeroesDawn.Application.Features.Meta
{
    /// <summary>Statistiques de jeu affichées sur la page Profil.</summary>
    public readonly struct ProfileStats
    {
        public readonly int CombatsWon, CombatsLost, ZonesCleared, Agents, TopAgentLevel, BestLinkWins, CraftedItems, CreditsEarned;

        public ProfileStats(int combatsWon, int combatsLost, int zonesCleared, int agents, int topAgentLevel, int bestLinkWins, int craftedItems, int creditsEarned)
        {
            CombatsWon = combatsWon; CombatsLost = combatsLost; ZonesCleared = zonesCleared; Agents = agents;
            TopAgentLevel = topAgentLevel; BestLinkWins = bestLinkWins; CraftedItems = craftedItems; CreditsEarned = creditsEarned;
        }
    }

    /// <summary>Le profil du joueur : pseudo, tag personnel, héros, niveau de pass et statistiques (lues dans la sauvegarde et la session).</summary>
    public sealed class PlayerProfile
    {
        readonly MetaContext _c;
        readonly PassProgress _pass;

        public PlayerProfile(MetaContext context, PassProgress pass) { _c = context; _pass = pass; }

        public string Pseudo => string.IsNullOrWhiteSpace(_c.Meta.onboarding.pseudo) ? PlayerTag.GuestName : _c.Meta.onboarding.pseudo;

        public string Tag => PlayerTag.Of(_c.Session.Account?.PlayerId ?? "local", _c.Meta.onboarding.pseudo);

        public string HeroId => _c.Session.Route.HeroId;

        public int PassLevel => _pass.Level;

        public ProfileStats Stats()
        {
            var meta = _c.Meta;
            var roster = _c.Session.Roster;
            return new ProfileStats(
                Features.Meta.Stats.Get(meta, Features.Meta.Stats.CombatsWon), Features.Meta.Stats.Get(meta, Features.Meta.Stats.CombatsLost),
                _c.Session.CompletedZones.Count, roster.Count, roster.Count == 0 ? 0 : roster.Max(m => m.Level),
                meta.links.Count == 0 ? 0 : meta.links.Max(l => l.wins), meta.crafted.Count,
                Features.Meta.Stats.Get(meta, Features.Meta.Stats.CreditsEarned));
        }
    }
}
