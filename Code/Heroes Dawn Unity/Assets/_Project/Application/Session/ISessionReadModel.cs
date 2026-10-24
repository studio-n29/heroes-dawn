#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;

namespace N29.HeroesDawn.Application.Session
{
    /// <summary>
    /// Ce que l'interface peut lire de la session. Elle ne reçoit jamais <see cref="GameSession"/>.
    /// <see cref="Run"/> est exposé tel quel jusqu'au sous-projet 3 : l'interface n'appelle pas ses méthodes de mutation.
    /// </summary>
    public interface ISessionReadModel
    {
        ContentCatalog Content { get; }
        PlayerAccount? Account { get; }
        /// <summary>Nombre d'objets possédés (tous exemplaires).</summary>
        int OwnedItemCount { get; }
        /// <summary>Nombre de cartes différentes possédées.</summary>
        int OwnedCardKinds { get; }
        IWalletReadModel Wallet { get; }
        IReadOnlyList<ITeamMemberReadModel> Roster { get; }
        IReadOnlyList<ITeamMemberReadModel> Squad { get; }
        ZoneRun? Run { get; }
        ZoneDef? RunZone { get; }
        bool AutoCombat { get; }
        /// <summary>Plus aucun membre de l'escouade ne peut combattre.</summary>
        bool SquadExhausted { get; }
        /// <summary>Prix de l entraînement d un membre de ce niveau (réduction du labo comprise).</summary>
        int TrainCostFor(int level);
        IReadOnlyCollection<string> CompletedZones { get; }
        IShopReadModel Shop { get; }
        /// <summary>Parcours de l histoire du personnage joueur choisi (actes, ville de chaque acte).</summary>
        StoryRoute Route { get; }
        /// <summary>Niveau de lien (1 à 5) de deux agents : leurs victoires communes amplifient le bonus passif de leur synergie.</summary>
        int LinkLevel(Core.MemberId a, Core.MemberId b);
    }
}
