#nullable enable
using System.Collections.Generic;
using System.Linq;

namespace N29.HeroesDawn.Application.Features.Meta
{
    /// <summary>
    /// Galerie des avatars proposés à la création du personnage du joueur. Le joueur incarne un agent qu'il crée lui-même ;
    /// Agent D et Agent M sont ses mentors, pas des personnages jouables.
    /// </summary>
    public static class PlayerAvatars
    {
        public static readonly IReadOnlyList<string> Gallery = new[] { "avatar_1", "avatar_2", "avatar_3", "avatar_4", "avatar_5", "avatar_6" };

        /// <summary>Avatar retenu : celui choisi, s'il existe dans la galerie ; sinon le premier.</summary>
        public static string Resolve(string? chosen) => Gallery.Contains(chosen ?? "") ? chosen! : Gallery[0];
    }
}
