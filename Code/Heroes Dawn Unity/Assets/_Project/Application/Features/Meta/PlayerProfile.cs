#nullable enable
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Application.Features.Combat;

namespace N29.HeroesDawn.Application.Features.Meta
{
    /// <summary>Statistiques de jeu affichées sur la page Profil.</summary>
    public readonly struct ProfileStats
    {
        public readonly int CombatsWon, CombatsLost, ZonesCleared, Agents, TopAgentLevel, BestLinkWins, CraftedItems, CreditsEarned, Tiles, Enemies;
        /// <summary>Agents les plus utilisés en combat (identifiants, du plus au moins utilisé).</summary>
        public readonly IReadOnlyList<string> FavoriteAgents;
        /// <summary>Cartes de compétence les plus utilisées en combat (identifiants, du plus au moins utilisé).</summary>
        public readonly IReadOnlyList<string> FavoriteSkills;

        public ProfileStats(int combatsWon, int combatsLost, int zonesCleared, int agents, int topAgentLevel, int bestLinkWins, int craftedItems, int creditsEarned,
                            int tiles, int enemies, IReadOnlyList<string> favoriteAgents, IReadOnlyList<string> favoriteSkills)
        {
            CombatsWon = combatsWon; CombatsLost = combatsLost; ZonesCleared = zonesCleared; Agents = agents;
            TopAgentLevel = topAgentLevel; BestLinkWins = bestLinkWins; CraftedItems = craftedItems; CreditsEarned = creditsEarned;
            Tiles = tiles; Enemies = enemies; FavoriteAgents = favoriteAgents; FavoriteSkills = favoriteSkills;
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

        /// <summary>Succès du joueur (progression, déblocage, jour).</summary>
        public Achievements Achievements() => new Achievements(_c, this);

        /// <summary>Rang affiché sur la carte, d'après le niveau de pass (clé de localisation).</summary>
        public string RankKey => ProfileRank.KeyFor(PassLevel);

        /// <summary>Avatar de la carte : un des six de la galerie.</summary>
        public string Avatar => PlayerAvatars.Resolve(_c.Meta.onboarding.avatar);

        /// <summary>Change l'avatar pour le suivant de la galerie.</summary>
        public void CycleAvatar()
        {
            var gallery = PlayerAvatars.Gallery;
            int i = 0;
            for (int k = 0; k < gallery.Count; k++) if (gallery[k] == Avatar) i = k;
            _c.Meta.onboarding.avatar = gallery[(i + 1) % gallery.Count];
            _c.Touch(MetaArea.Profile);
        }

        /// <summary>Fond de la carte de profil.</summary>
        public string Background => ProfileBackgrounds.Resolve(_c.Meta.profileBackground);

        /// <summary>Passe au fond suivant de la carte de profil.</summary>
        public void CycleBackground()
        {
            _c.Meta.profileBackground = ProfileBackgrounds.Next(_c.Meta.profileBackground);
            _c.Touch(MetaArea.Profile);
        }

        public ProfileStats Stats()
        {
            var meta = _c.Meta;
            var roster = _c.Session.Roster;
            return new ProfileStats(
                Features.Meta.Stats.Get(meta, Features.Meta.Stats.CombatsWon), Features.Meta.Stats.Get(meta, Features.Meta.Stats.CombatsLost),
                _c.Session.CompletedZones.Count, roster.Count, roster.Count == 0 ? 0 : roster.Max(m => m.Level),
                meta.links.Count == 0 ? 0 : meta.links.Max(l => l.wins), meta.crafted.Count,
                Features.Meta.Stats.Get(meta, Features.Meta.Stats.CreditsEarned),
                Features.Meta.Stats.Get(meta, Features.Meta.Stats.Tiles), Features.Meta.Stats.Get(meta, Features.Meta.Stats.Enemies),
                Features.Meta.Stats.Favorites(meta, Features.Meta.Stats.AgentUsePrefix, Features.Meta.Stats.FavoriteCount),
                Features.Meta.Stats.Favorites(meta, Features.Meta.Stats.SkillUsePrefix, Features.Meta.Stats.FavoriteCount));
        }
    }
}
