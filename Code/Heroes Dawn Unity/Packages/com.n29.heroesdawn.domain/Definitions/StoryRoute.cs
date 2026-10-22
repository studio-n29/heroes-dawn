#nullable enable
using System.Collections.Generic;

namespace N29.HeroesDawn.Definitions
{
    /// <summary>
    /// Parcours de l'histoire d'un personnage joueur : l'ordre des quatre actes (une ville chacun) et l'équipe de départ.
    /// Source : 02_Lore_Narration/03_Histoire_et_Actes.md, section « Parcours selon le personnage choisi ».
    /// </summary>
    public sealed class StoryRoute
    {
        public string Id { get; }
        /// <summary>Identifiant de l'agent joueur (Agent D ou Agent M).</summary>
        public string HeroId { get; }
        /// <summary>Ville de chaque acte, dans l'ordre (acte I en premier).</summary>
        public IReadOnlyList<string> ActCities { get; }
        /// <summary>Agent joueur et trois compagnons, en escouade au début de la partie.</summary>
        public IReadOnlyList<string> StarterTeam { get; }
        /// <summary>Vrai si la première ville du parcours est jouable (zones créées).</summary>
        public bool Available { get; }

        public StoryRoute(string id, string heroId, IReadOnlyList<string> actCities, IReadOnlyList<string> starterTeam, bool available)
        {
            Id = id; HeroId = heroId; ActCities = actCities; StarterTeam = starterTeam; Available = available;
        }
    }

    public static class StoryRoutes
    {
        public const string AgentD = "agent_d";
        public const string AgentM = "agent_m";

        public const string NewYork = "new_york";
        public const string Paris = "paris";
        public const string Moscow = "moscow";
        public const string Salem = "salem";

        /// <summary>Agent D : New York, Paris, Moscou, Salem.</summary>
        public static readonly StoryRoute D = new StoryRoute(AgentD, AgentD,
            new[] { NewYork, Paris, Moscow, Salem },
            new[] { AgentD, "darius_brooks", "mei_lin_chen", "freya_lindqvist" }, true);

        /// <summary>Agent M : Moscou, Paris, New York, Salem. Indisponible tant que les zones de Moscou ne sont pas créées.</summary>
        public static readonly StoryRoute M = new StoryRoute(AgentM, AgentM,
            new[] { Moscow, Paris, NewYork, Salem },
            new[] { AgentM, "sam_okafor", "celeste_moreau", "haruto_nakamura" }, false);

        public static readonly IReadOnlyList<StoryRoute> All = new[] { D, M };

        /// <summary>Parcours de ce personnage ; à défaut (profil sans choix) celui d'Agent D.</summary>
        public static StoryRoute For(string? id)
        {
            foreach (var r in All) if (r.Id == id) return r;
            return D;
        }

        /// <summary>Ville d'un quartier ou d'une zone (les quartiers de New York appartiennent tous à la ville de New York).</summary>
        public static string CityOfBorough(string borough)
        {
            switch (borough)
            {
                case "brooklyn": case "queens": case "bronx": case "manhattan": case "staten": return NewYork;
                case "paris": return Paris;
                case "moscow": return Moscow;
                case "salem": return Salem;
                default: return NewYork;
            }
        }
    }
}
