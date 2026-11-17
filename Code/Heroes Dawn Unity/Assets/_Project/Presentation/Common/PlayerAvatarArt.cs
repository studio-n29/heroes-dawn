#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>
    /// Table de correspondance « identifiant d'avatar de la galerie → image » (livraison 2D S-01, Resources/UI/AubeTactique/Missing20261116/Avatars).
    /// Les identifiants sont ceux de <c>PlayerAvatars.Gallery</c> et de la sauvegarde (avatar_1 à avatar_12) : ils ne changent pas ;
    /// seule l'image affichée dépend de cette table. Le jeu applique le masque rond à l'affichage (USS « hd-avatar__face »).
    /// </summary>
    public static class PlayerAvatarArt
    {
        const string Folder = "Missing20261116/Avatars/";

        static readonly Dictionary<string, string> Files = new Dictionary<string, string>
        {
            ["avatar_1"] = "recruit-01", ["avatar_2"] = "recruit-02", ["avatar_3"] = "recruit-03", ["avatar_4"] = "recruit-04",
            ["avatar_5"] = "recruit-05", ["avatar_6"] = "recruit-06", ["avatar_7"] = "recruit-07", ["avatar_8"] = "recruit-08",
            ["avatar_9"] = "family-bio", ["avatar_10"] = "family-elemental", ["avatar_11"] = "family-matter", ["avatar_12"] = "family-perception",
        };

        /// <summary>Image de l'avatar ; null pour un identifiant inconnu ou une image absente (l'appelant affiche alors un repli).</summary>
        public static Sprite? For(string? avatarId)
            => avatarId != null && Files.TryGetValue(avatarId, out var file) ? TacticalArt.Load(Folder + file) : null;
    }
}
