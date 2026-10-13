#nullable enable
using System.Collections.Generic;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Textes des Paramètres (huit catégories, M80-M85).</summary>
    public static partial class Loc
    {
        static partial void ExtendSettings(Dictionary<string, string> d)
        {
            d["settings.cat.Audio"] = "AUDIO";
            d["settings.cat.Graphics"] = "GRAPHISMES";
            d["settings.cat.Accessibility"] = "ACCESSIBILITÉ";
            d["settings.cat.Language"] = "LANGUE";
            d["settings.cat.Notifications"] = "NOTIFICATIONS";
            d["settings.cat.Controls"] = "CONTRÔLES";
            d["settings.cat.Account"] = "COMPTE";
            d["settings.cat.Support"] = "SUPPORT & AIDE";
            d["settings.reset_category"] = "RÉINITIALISER";
            d["settings.reset_category.done"] = "Réglages remis à zéro.";
            d["settings.on"] = "ACTIVÉ";
            d["settings.off"] = "DÉSACTIVÉ";

            d["settings.master"] = "Volume général";
            d["settings.music"] = "Musique";
            d["settings.sfx"] = "Effets";
            d["settings.voice"] = "Voix";
            d["settings.mute_bg"] = "Couper le son en arrière-plan";

            d["settings.quality"] = "Qualité graphique";
            d["settings.quality.0"] = "AUTO";
            d["settings.quality.1"] = "BAS";
            d["settings.quality.2"] = "MOYEN";
            d["settings.quality.3"] = "HAUT";
            d["settings.fps60"] = "60 images par seconde";
            d["settings.battery"] = "Économie de batterie";
            d["settings.battery.hint"] = "30 images par seconde et effets réduits.";
            d["settings.full_fx"] = "Effets de combat complets";

            d["settings.flashes"] = "Réduire les flashs";
            d["settings.shakes"] = "Réduire les secousses";
            d["settings.extended_time"] = "Temps étendu des mini-jeux";
            d["settings.extended_time.hint"] = "Curseurs plus lents et zones de réussite plus larges.";
            d["settings.subtitles"] = "Sous-titres";

            d["settings.lang.texts"] = "Langue des textes";
            d["settings.lang.fr"] = "Français";
            d["settings.lang.voice"] = "Langue des voix";
            d["settings.lang.en"] = "Anglais";
            d["settings.lang.more"] = "Anglais, espagnol, portugais, allemand";

            d["settings.notif.all"] = "Notifications";
            d["settings.notif.hint"] = "Les rappels hors du jeu arriveront avec la version en ligne ; tes choix sont déjà enregistrés.";
            d["settings.notif.training"] = "Entraînement terminé";
            d["settings.notif.expedition"] = "Expédition revenue";
            d["settings.notif.lab"] = "Recherche terminée";
            d["settings.notif.recovered"] = "Membre rétabli";
            d["settings.notif.shop"] = "Nouveaux héros en boutique";
            d["settings.notif.objectives"] = "Objectifs secondaires";
            d["settings.notif.season"] = "Fin de saison";
            d["settings.notif.quiet"] = "Heures calmes (22 h - 8 h)";

            d["settings.show_keys"] = "Afficher les touches";
            d["settings.key.roll"] = "Lancer le dé"; d["settings.key.roll.v"] = "Espace";
            d["settings.key.bonus"] = "Dé bonus"; d["settings.key.bonus.v"] = "B";
            d["settings.key.end"] = "Terminer le déplacement"; d["settings.key.end.v"] = "Entrée";
            d["settings.key.skills"] = "Compétences"; d["settings.key.skills.v"] = "1 · 2 · 3";
            d["settings.key.auto"] = "Auto"; d["settings.key.auto.v"] = "A";
            d["settings.key.speed"] = "Vitesse"; d["settings.key.speed.v"] = "S";
            d["settings.key.pause"] = "Pause"; d["settings.key.pause.v"] = "P";
            d["settings.key.back"] = "Retour"; d["settings.key.back.v"] = "Échap";

            d["settings.account.player"] = "Joueur";
            d["settings.account.linked"] = "Compte lié";
            d["settings.account.guest"] = "Invité";
            d["settings.account.link_hint"] = "Lier un compte arrivera avec la version en ligne.";
            d["settings.consent"] = "Partager des statistiques anonymes";
            d["settings.export"] = "Exporter mes données";
            d["settings.export.btn"] = "EXPORTER";
            d["settings.export.hint"] = "Simulé : l'export sera disponible avec la version en ligne.";
            d["settings.export.done"] = "Export simulé : rien n'a été envoyé.";
            d["settings.reset"] = "EFFACER LA SAUVEGARDE";

            d["settings.tutorial"] = "Revoir le tutoriel";
            d["settings.tutorial.btn"] = "REVOIR";
            d["settings.tutorial.hint"] = "Rayner te guide à nouveau dans ta première mission.";
            d["settings.tutorial.started"] = "Tutoriel relancé !";
            d["settings.player_id"] = "ID joueur";
            d["settings.copy_id"] = "Copier l'ID";
            d["settings.copy_id.btn"] = "COPIER";
            d["settings.copy_id.done"] = "ID copié.";
            d["settings.contact"] = "Contacter le support";
            d["settings.contact.btn"] = "CONTACTER";
            d["settings.contact.hint"] = "Simulé dans cette version.";
            d["settings.contact.done"] = "Support simulé : écris-nous quand la version en ligne sera là !";
            d["settings.version"] = "Version";
            d["settings.credits"] = "Crédits";
            d["settings.credits.v"] = "Studio N29";
        }
    }
}
