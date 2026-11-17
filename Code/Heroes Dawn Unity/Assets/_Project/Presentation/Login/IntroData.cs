#nullable enable
using System.Collections.Generic;

namespace N29.HeroesDawn.Presentation.Login
{
    /// <summary>Launch : montage de lancement (45 s, sans texte). Full : la cinématique longue, avec sous-titres et voix.</summary>
    public enum CinematicKind { Launch, Full }

    public struct IntroCinematicTexts
    {
        public string Skip;
    }

    /// <summary>Scène de Rayner en Full Art (grand portrait plein écran) : qui parle et ses répliques ; un toucher passe à la suivante.</summary>
    public struct RaynerSceneData
    {
        public string Speaker;
        /// <summary>Identifiant du portrait (« rayner »).</summary>
        public string PortraitId;
        public IReadOnlyList<string> Lines;
        /// <summary>Texte de l'indication « toucher pour continuer ».</summary>
        public string Continue;
    }

    /// <summary>Un pouvoir du tableau de la fiche : identifiant (sauvegardé) et nom traduit.</summary>
    public readonly struct PowerOption
    {
        public readonly string Id;
        public readonly string Label;
        public PowerOption(string id, string label) { Id = id; Label = label; }
    }

    /// <summary>Une colonne du tableau des pouvoirs : une famille et ses pouvoirs.</summary>
    public readonly struct PowerColumn
    {
        public readonly string FamilyId;
        public readonly string Label;
        public readonly IReadOnlyList<PowerOption> Powers;
        public PowerColumn(string familyId, string label, IReadOnlyList<PowerOption> powers) { FamilyId = familyId; Label = label; Powers = powers; }
    }

    /// <summary>La fiche d'identification à remplir : intitulés, bornes, galerie d'avatars et tableau des pouvoirs (groupés par famille).</summary>
    public struct AgentFormData
    {
        public string Title;
        public string LastNameLabel, FirstNameLabel, AgeLabel, AvatarLabel, PowerLabel, PowerPlaceholder;
        public string Confirm;
        /// <summary>Message d'erreur à montrer (fiche refusée par le domaine) ; vide : aucun.</summary>
        public string Error;
        public int MaxNameLength, MinAge, MaxAge;
        public IReadOnlyList<string> Avatars;
        public IReadOnlyList<PowerColumn> Powers;
    }

    /// <summary>Un parcours de départ proposé à la fin : identifiant du parcours (agent_d / agent_m), ville, agent, présentation et disponibilité.</summary>
    public readonly struct StartOption
    {
        public readonly string RouteId, City, Agent, Description, UnavailableLabel;
        public readonly bool Available;
        public StartOption(string routeId, string city, string agent, string description, bool available, string unavailableLabel)
        {
            RouteId = routeId; City = city; Agent = agent; Description = description; Available = available; UnavailableLabel = unavailableLabel;
        }
    }

    public struct StartChoiceData
    {
        public string Title;
        public IReadOnlyList<StartOption> Options;
    }
}
