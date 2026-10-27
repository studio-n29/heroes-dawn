#nullable enable
using System.Collections.Generic;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Textes du Profil, des amis et des soins sur la fiche d'agent (lot F).</summary>
    public static partial class Loc
    {
        static partial void ExtendProfile(Dictionary<string, string> d)
        {
            d["page.profile"] = "Profil";
            d["page.team_care"] = "Équipe et soins";
            d["sheet.heal"] = "Soigner";
            d["hub.heal_member"] = "Soigner · {0} CR";
            d["hub.healed_member"] = "{0} est soigné.";
            d["profile.header_line"] = "{0} · {1} · niveau de pass {2}";
            d["profile.copy"] = "COPIER MON TAG";
            d["profile.copied"] = "Tag copié";
            d["profile.add"] = "AJOUTER UN AMI";
            d["profile.open"] = "OUVRIR";
            d["profile.registry"] = "Registre des agents";
            d["profile.registry.sub"] = "Tous les agents de la ville et comment les obtenir.";
            d["profile.archives"] = "Archives";
            d["profile.archives.sub"] = "Recrues, lore, scènes débloquées et musiques.";
            d["profile.stat.combatsWon"] = "Combats gagnés";
            d["profile.stat.combatsLost"] = "Combats perdus";
            d["profile.stat.zones"] = "Zones validées";
            d["profile.stat.agents"] = "Agents possédés";
            d["profile.stat.topLevel"] = "Plus haut niveau d'agent";
            d["profile.stat.bestLink"] = "Meilleur lien (victoires ensemble)";
            d["profile.stat.crafted"] = "Objets fabriqués";
            d["profile.stat.credits"] = "Crédits gagnés";
            d["profile.friends"] = "AMIS";
            d["profile.friends_empty"] = "Aucun ami pour l'instant";
            d["profile.friends_empty_sub"] = "Ajoute un ami avec son tag (Nom#1234). Les amis en ligne arrivent avec le compte.";
            d["profile.friend_pending"] = "{0} · en attente de connexion en ligne";
            d["profile.friend_remove"] = "RETIRER";
            d["profile.friend_removed"] = "Ami retiré";
            d["profile.friend_added"] = "Ami ajouté";
            d["profile.add_title"] = "AJOUTER UN AMI";
            d["profile.add_hint"] = "Son tag (Nom#1234)";
            d["profile.add_ok"] = "AJOUTER";
            d["errors.friends_invalid"] = "Tag invalide : écris le nom, #, puis quatre chiffres (Nom#1234).";
            d["errors.friends_self"] = "C'est ton propre tag.";
            d["errors.friends_duplicate"] = "Cet ami est déjà dans ta liste.";
            d["errors.friends_full"] = "Ta liste d'amis est pleine (50 maximum).";
        }
    }
}
