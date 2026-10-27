# Recette du compte joueur (lot I)

Ce guide liste ce que je n'ai pas pu vérifier moi-même : tout ce qui demande un vrai téléphone, un navigateur et les réglages du tableau de bord Supabase. Les tests automatiques couvrent le reste (simulateurs, faux client HTTP, écrans, règles de la base).

## Avant de commencer
1. Régler Supabase comme décrit dans `Assets/_Project/Backend/README.md` (Google, Apple, adresses de retour, confirmation d'e-mail).
2. Enregistrer le schéma de lien `heroesdawn://auth` dans les réglages de build (Android : filtre d'intention ; iOS : URL scheme).
3. Construire le jeu sur un appareil (le fichier `Resources/Supabase/supabase.json` doit être présent).

## Parcours à vérifier
| # | Étape | Résultat attendu |
|---|---|---|
| 1 | Titre ▸ Se connecter ▸ Créer un compte (e-mail, mot de passe 8+, pseudo) | « E-mail de confirmation envoyé » si la confirmation est exigée, sinon compte lié (« ta progression est sauvegardée en ligne ») |
| 2 | Cliquer le lien de confirmation reçu, revenir, se connecter | Connexion réussie ; le bouton du titre montre l'e-mail |
| 3 | Jouer quelques minutes puis fermer le jeu | La progression part en ligne (à la mise en arrière-plan) |
| 4 | Installer sur un second appareil, se connecter au même compte | « Progression du compte récupérée » (appareil neuf) |
| 5 | Sur les deux appareils, avancer chacun hors ligne puis se reconnecter | Fenêtre « Quelle progression garder ? » avec les deux versions ; aucun écrasement avant le choix |
| 6 | Choisir « Garder cet appareil » puis rouvrir l'autre appareil | L'autre appareil reçoit la progression choisie ; l'autre version est dans `persistentDataPath/backups` |
| 7 | Continuer avec Google | Le navigateur s'ouvre, puis retour dans le jeu connecté (lien `heroesdawn://auth`) |
| 8 | Continuer avec Apple (iOS en priorité) | Idem |
| 9 | Fermer le navigateur sans terminer la connexion Google | Message « La connexion a échoué ou a été annulée », le jeu reste utilisable |
| 10 | Mot de passe oublié avec une adresse existante puis inconnue | Même message neutre dans les deux cas ; l'e-mail n'arrive que pour l'adresse existante |
| 11 | Profil ▸ Synchroniser | « Progression synchronisée » |
| 12 | Profil ▸ Se déconnecter (confirmer) | Retour invité avec la progression de l'appareil ; plus d'envoi en ligne |
| 13 | Mode avion pendant une partie | Le jeu continue ; l'envoi reprend au retour du réseau |
| 14 | Lancer le jeu deux jours plus tard | Session restaurée sans ressaisir le mot de passe |

## À signaler
Tout message d'erreur brut (non français), tout écrasement sans choix, tout blocage de l'écran pendant une connexion, tout jeton visible dans les journaux.
