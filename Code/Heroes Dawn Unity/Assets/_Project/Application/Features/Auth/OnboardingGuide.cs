#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Application.Features.Collection;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Application.Features.Auth
{
    /// <summary>Un personnage joueur proposé à la première connexion (Agent D ou Agent M) ; son parcours fixe l ordre des actes.</summary>
    public sealed class CampaignOption
    {
        public string Id { get; }
        public bool Available { get; }
        public CampaignOption(string id, bool available) { Id = id; Available = available; }
        public string NameKey => "campaign." + Id + ".name";
        public string DescKey => "campaign." + Id + ".desc";
    }

    public static class Campaigns
    {
        public const string AgentD = StoryRoutes.AgentD;
        public const string AgentM = StoryRoutes.AgentM;

        public static readonly IReadOnlyList<CampaignOption> All = BuildAll();

        static IReadOnlyList<CampaignOption> BuildAll()
        {
            var list = new List<CampaignOption>();
            foreach (var r in StoryRoutes.All) list.Add(new CampaignOption(r.Id, r.Available));
            return list;
        }
    }
    /// <summary>
    /// Première connexion : un profil qui n'a encore validé aucune zone passe par la cinématique puis le choix du personnage joueur
    /// (Agent D ou Agent M). Il n'y a plus de pseudo : le joueur incarne cet agent, avatar du chef d'unité, qui devient le nom du compte.
    /// Le personnage joueur ne combat pas : l'effectif de départ ne compte qu'une recrue, et un objet de départ sert au tutoriel d'équipement.
    /// </summary>
    public sealed class OnboardingGuide
    {
        readonly MetaContext _c;

        public OnboardingGuide(MetaContext context) { _c = context; }

        OnboardingSave Save => _c.Meta.onboarding;

        public bool Needed => !Save.done && _c.Session.CompletedZones.Count == 0;
        /// <summary>Nom du compte : celui du personnage joueur choisi (vide avant le choix).</summary>
        public string Pseudo => Save.pseudo;
        public string Campaign => Save.campaign;
        /// <summary>Personnage joueur choisi (Agent D par défaut).</summary>
        public string Hero => string.IsNullOrEmpty(Save.hero) ? StoryRoutes.AgentD : Save.hero;

        bool AllExist(IReadOnlyList<string> ids)
        {
            foreach (var id in ids)
            {
                bool found = false;
                foreach (var m in _c.Session.Content.Members) if (m.Id.Value == id) { found = true; break; }
                if (!found) return false;
            }
            return true;
        }

        /// <summary>Termine la première connexion. Renvoie la clé d'erreur du choix, ou null si tout est enregistré.</summary>
        public string? Complete(string campaign)
        {
            var route = StoryRoutes.For(campaign);
            if (route.Id != campaign || !route.Available) return "onboarding.campaign.unavailable";
            Save.pseudo = route.HeroName;
            Save.campaign = route.ActCities[0];
            Save.hero = route.Id;
            if (AllExist(route.StarterTeam)) _c.Session.StartWithTeam(route.StarterTeam);
            GrantStarterItem();
            Save.done = true;
            ApplyPseudo();
            _c.Touch(MetaArea.Onboarding);
            return null;
        }

        /// <summary>Un objet courant offert au départ : le tutoriel apprend à l'équiper sur la recrue.</summary>
        void GrantStarterItem()
        {
            ItemDef? pick = null;
            foreach (var it in _c.Session.Content.Collection.Items)
                if (pick == null || it.Rarity < pick.Rarity || (it.Rarity == pick.Rarity && string.CompareOrdinal(it.Id.Value, pick.Id.Value) < 0)) pick = it;
            if (pick != null) CollectionOps.AddItem(_c.Session, pick.Id);
        }

        /// <summary>Un profil existant garde le nom de son personnage joueur à la reconnexion.</summary>
        public void ApplyPseudo()
        {
            var account = _c.Session.Account;
            if (account == null || string.IsNullOrEmpty(Save.pseudo)) return;
            _c.Session.Account = new PlayerAccount(account.PlayerId, Save.pseudo);
        }
    }
}
