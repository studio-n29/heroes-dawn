#nullable enable
using System.Collections.Generic;
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
    /// <summary>Règles du pseudo : 3 à 16 caractères, lettres, chiffres, espace, tiret ou tiret bas.</summary>
    public static class PseudoRules
    {
        public const int MinLength = 3;
        public const int MaxLength = 16;

        /// <summary>Clé de texte de l'erreur, ou null si le pseudo (nettoyé dans <paramref name="clean"/>) est valide.</summary>
        public static string? Validate(string? raw, out string clean)
        {
            clean = (raw ?? "").Trim();
            while (clean.Contains("  ")) clean = clean.Replace("  ", " ");
            if (clean.Length < MinLength) return "onboarding.pseudo.short";
            if (clean.Length > MaxLength) return "onboarding.pseudo.long";
            foreach (var c in clean)
                if (!(char.IsLetterOrDigit(c) || c == ' ' || c == '-' || c == '_')) return "onboarding.pseudo.chars";
            return null;
        }
    }

    /// <summary>
    /// Première connexion : un profil qui n'a encore validé aucune zone passe par la cinématique, le choix du pseudo et celui de la campagne.
    /// Le pseudo remplace le nom d'invité du compte pour toute la session (et les suivantes, il est dans le profil).
    /// </summary>
    public sealed class OnboardingGuide
    {
        readonly MetaContext _c;

        public OnboardingGuide(MetaContext context) { _c = context; }

        OnboardingSave Save => _c.Meta.onboarding;

        public bool Needed => !Save.done && _c.Session.CompletedZones.Count == 0;
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

        /// <summary>Termine la première connexion. Renvoie la clé d'erreur du pseudo, ou null si tout est enregistré.</summary>
        public string? Complete(string pseudo, string campaign)
        {
            var route = StoryRoutes.For(campaign);
            if (route.Id != campaign || !route.Available) return "onboarding.campaign.unavailable";
            var error = PseudoRules.Validate(pseudo, out var clean);
            if (error != null) return error;
            Save.pseudo = clean;
            Save.campaign = route.ActCities[0];
            Save.hero = route.Id;
            if (AllExist(route.StarterTeam)) _c.Session.StartWithTeam(route.StarterTeam);
            Save.done = true;
            ApplyPseudo();
            _c.Touch(MetaArea.Onboarding);
            return null;
        }

        /// <summary>Un profil existant garde son pseudo à la reconnexion.</summary>
        public void ApplyPseudo()
        {
            var account = _c.Session.Account;
            if (account == null || string.IsNullOrEmpty(Save.pseudo)) return;
            _c.Session.Account = new PlayerAccount(account.PlayerId, Save.pseudo);
        }
    }
}
