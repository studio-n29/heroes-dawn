# Maquettes UI du jeu complet + tutoriel guidé — design

> Statut : validé par l'utilisateur (2026-10-04, « je valide tout ce que tu recommandes »).
> Sous-projets : **A** maquettes haute fidélité du jeu complet (dont Paramètres), **B** conception et maquettes du tutoriel.
> Le sous-projet **C** (intégration du tutoriel dans le prototype M1) aura sa propre spec, à partir de celle-ci.

## 1. Intention
- Remplacer les wireframes gris et les 5 maquettes stylisées de `03_Art_UI_UX/Maquettes` (jugés « M1, basiques ») par des **maquettes haute fidélité DA comics pour chaque écran du jeu final**.
- Des **menus bien séparés**, une prise en main **intuitive et lisible**.
- Des **Paramètres** complets.
- Un **tutoriel de départ didactique**, maquetté ici puis intégré au M1 avant la M2.

Critères de réussite : chaque écran du jeu final a sa maquette légendée ; un joueur qui découvre les maquettes comprend la navigation sans explication ; le tutoriel est décrit étape par étape avec ce que le joueur voit, touche et apprend ; les maquettes servent de cible directe pour retoucher les prefabs.

## 2. Production des maquettes
- **Kit HTML/CSS comics** (`03_Art_UI_UX/Maquettes/_sources/`) : `kit.css` (jetons de couleurs, composants), une page HTML par écran, fond et portraits tirés de l'illustration clé, polices Bangers / Comic Neue (OFL, fichiers du projet Unity).
- **Export PNG 1920×1080** par Microsoft Edge en mode headless (`export.ps1`) : aucun téléchargement, aucune dépendance Python.
- **Prototype cliquable** : les mêmes pages reliées par des liens (`index.html`), pour tester la prise en main dans un navigateur.
- Les anciennes images (W, S, F) et l'ancien générateur Python sont déplacés dans `Maquettes/_archive_M1/`.
- `03_Maquettes_Screen_Flow.md` est réécrit : une image par écran + légende numérotée (pastilles jaunes sur l'image).

## 3. Règles de conception (conservées et précisées)
- Paysage 1920×1080 ; zones sûres mobiles de 88 px sur les côtés ; cibles tactiles ≥ 96 px, 24 px d'écart.
- **Placement constant** : retour en haut à gauche ; titre à côté ; ressources puis ⚙ en haut à droite ; action principale en bas à droite.
- **Une action principale par écran** (jaune, ou rouge pour MISSION / danger) ; actions secondaires sombres.
- Couleur jamais seule porteuse de sens : famille = icône + couleur ; état = libellé.
- Bangers (titres, boutons, chiffres), Comic Neue (texte de plus de 4 mots), 20 px minimum.
- Raccourcis PC entre crochets sur les boutons quand l'option est active.

## 4. Navigation (sous-projet A)
### Cadre commun
- **Barre haute** (écrans du hub) : titre de l'écran, Crédits / PR / Gemmes (toucher = info-bulle d'origine + lien boutique), ⚙ Paramètres.
- **Barre de navigation basse** : `QG · ÉQUIPE · RECRUTER · PASS · BOUTIQUE`, onglet actif jaune, badges de notification par onglet.
- **MISSION** : bouton rouge flottant en bas à droite, au-dessus de la barre, sur l'onglet QG.
- **Mode mission** (carte → résultats) : sans barre basse, plein écran ; retour en haut à gauche ; ⏸ = pause (Reprendre, Paramètres, Abandonner/Retour QG).

### Retour (bouton, geste Android, Échap)
Pièce du QG → QG ; autre onglet → QG ; QG → « Quitter le jeu ? » ; mission → étape précédente ; combat → pause (jamais de sortie directe) ; modale → annuler.

### Écrans
| Réf. | Écran | Groupe |
|---|---|---|
| M01 | Titre / connexion | Entrée |
| M02 | Premier lancement : langue + consentement RGPD | Entrée |
| M03 | Chargement (astuce + illustration) | Entrée |
| M10 | QG (5 pièces, objectifs secondaires, missions du jour) | Onglet QG |
| M11 | Salon d'entraînement | Onglet QG |
| M12 | Infirmerie | Onglet QG |
| M13 | Salle des opérations (expéditions) | Onglet QG |
| M14 | Labo de recherche | Onglet QG |
| M15 | Archives | Onglet QG |
| M20 | Effectif | Onglet Équipe |
| M21 | Fiche membre | Onglet Équipe |
| M30 | Recrutement — pool du jour | Onglet Recruter |
| M31 | Confirmation de recrutement (modale) | Onglet Recruter |
| M40 | Pass saisonnier + missions du jour | Onglet Pass |
| M50 | Boutique | Onglet Boutique |
| M60 | Carte de la ville | Mission |
| M61 | Fiche de zone | Mission |
| M62 | Préparation de mission | Mission |
| M63 | Plateau — lancer | Mission |
| M64 | Plateau — destination | Mission |
| M65 | Combat — allié prêt | Mission |
| M66 | Combat — ciblage et combo | Mission |
| M67 | Mini-jeux (4 formats) | Mission |
| M68 | Événement narratif | Mission |
| M69 | Repos | Mission |
| M70 | Résultats de combat | Mission |
| M71 | Zone sécurisée | Mission |
| M72 | Pause | Mission |
| M80–M85 | Paramètres (voir §5) | Transverse |
| M90 | Kit UI | Référence |
| M91 | Screen flow | Référence |

Le contenu fonctionnel de chaque écran reprend celui des wireframes W01–W19 (GDD inchangé) ; seule l'organisation change : chaque menu permanent devient un onglet, les pièces restent dans le QG.

## 5. Paramètres (sous-projet A)
Écran plein : catégories à gauche, réglages à droite ; application immédiate ; « Réinitialiser » par catégorie ; confirmation seulement pour la langue, la suppression du compte et la qualité graphique sur PC.

| Catégorie | Réglages | Stockage |
|---|---|---|
| Audio | Général, Musique, Effets, Voix (0–100 + muet) ; couper le son en arrière-plan | Appareil |
| Graphismes & performance | Qualité Auto/Bas/Moyen/Haut ; 30/60 i/s ; économie de batterie (30 i/s + effets réduits) ; effets de combat complets/réduits ; PC : résolution, plein écran, VSync | Appareil |
| Accessibilité | Taille du texte (3) ; daltonisme (protanopie, deutéranopie, tritanopie) ; réduire les flashs ; réduire les secousses ; temps étendu des mini-jeux ; sous-titres | Compte |
| Langue | Textes (FR, EN, ES, PT, DE) ; voix (EN) | Compte |
| Notifications | Interrupteur général ; par type : entraînement terminé, expédition revenue, labo plein, membre rétabli, nouvelle pool, objectifs secondaires, fin de saison ; heures calmes (22 h–8 h) | Compte |
| Contrôles PC (PC seulement) | Raccourcis remappables (Lancer Espace, Q/W/E, 1-4, Combo C, Auto A, Vitesse S, Pause P, Retour Échap) ; afficher les touches ; caméra du plateau (sensibilité, zoom) | Appareil |
| Compte & confidentialité | Compte lié + Lier ; consentement statistiques ; exporter mes données ; supprimer mon compte (double confirmation) ; conditions, confidentialité | Compte |
| Support & aide | **Revoir le tutoriel** ; fiches d'aide vues (glossaire) ; contacter le support ; ID joueur (copier) ; version ; crédits | — |

Maquettes : M80 Audio, M81 Graphismes, M82 Accessibilité, M83 Notifications, M84 Contrôles PC, M85 Compte & Support (+ confirmation de suppression).

## 6. Tutoriel : première partie guidée (sous-projet B)
### Principes didactiques
- **Apprendre en faisant** : chaque étape demande un geste réel sur la vraie interface (zone 1.1), jamais un écran de texte seul.
- **Une notion à la fois**, au moment où elle sert ; 2 lignes de texte maximum par bulle.
- **Guide** : Brie Rayner, en médaillon, dans une bulle de BD (Comic Neue) avec un récitatif jaune pour le titre de l'étape.
- **Projecteur** : le reste de l'écran est voilé (noir 70 %), l'élément à toucher est découpé et cerclé de jaune pulsé, une main animée le désigne.
- **Étapes bloquantes** (seul l'élément désigné répond) pour le parcours de base ; **astuces non bloquantes** (carte « Astuce » refermable) à la première rencontre des autres mécaniques.
- **Pause** : le combat (temps actif) est figé pendant une bulle.
- **Reprise** : l'étape atteinte est enregistrée dans le profil ; quitter puis revenir reprend à l'étape en cours.
- **Passer** : bouton discret « Passer le tutoriel » (confirmation) ; **Revoir** depuis Paramètres ▸ Support & aide.
- Indicateur de progression discret (« 3 / 9 ») dans la bulle.

### Parcours de base (bloquant)
| # | Où | Rayner dit (sens) | Le joueur fait | Il apprend |
|---|---|---|---|---|
| 1 | QG | Bienvenue à Red Hook : voici ta section. | Touche « Continuer » | Le QG est la base |
| 2 | Onglet Équipe | Une mission se joue à 4 : compose ton escouade. | Touche un membre pour l'ajouter | Escouade, familles |
| 3 | QG | Les quais sont aux mains des pillards. | Touche MISSION puis Déployer | Lancer une mission |
| 4 | Plateau | Voici les 3 objectifs de la zone. | Touche « Compris » | Objectifs |
| 5 | Plateau | Lance le dé pour avancer. | Touche LANCER | Dé et PM |
| 6 | Plateau | Les cases cerclées sont à ta portée. | Touche la destination désignée | Déplacement, brouillard |
| 7 | Combat | Les jauges jaunes se remplissent : quand elle est pleine, ton héros agit. | Touche « Compris » (combat figé) | Temps actif (ATB) |
| 8 | Combat | Choisis une compétence, puis une cible. | Touche une compétence puis l'ennemi | Compétence, ciblage |
| 9 | Résultats | Chaque combat fatigue l'équipe. | Touche Continuer | Fatigue, récompenses |

Fin : récitatif « TUTORIEL TERMINÉ — À TOI DE JOUER ! » ; aucune récompense supplémentaire (l'économie reste côté serveur).

### Astuces contextuelles (non bloquantes, une seule fois chacune)
Combo disponible · AUTO et vitesse · dé bonus (réserve de PM) · bouton Terminer · case Événement · case Mini-jeu · case Repos · défaite (recul d'une case) · fatigue au-delà de 60 % · zone sécurisée · recrutement (première visite) · infirmerie (premier membre fatigué).

### Maquettes du tutoriel
T01 Bienvenue (QG) · T02 Escouade · T03 Mission · T04 Objectifs · T05 Lancer le dé · T06 Destination · T07 Combat — ATB · T08 Combat — compétence et cible · T09 Résultats — fatigue · T10 Astuce contextuelle (combo) · T11 Fin du tutoriel · T12 Passer le tutoriel (confirmation).

### Notes pour l'intégration M1 (sous-projet C)
- Les maquettes montrent l'interface cible ; dans le M1, chaque étape vise l'élément équivalent des prefabs actuels.
- Pistes d'architecture : un présentateur `TutorialDirector` piloté par les messages existants (`GameStateChanged`, événements du plateau et du combat), une vue `TutorialOverlayView` (voile, découpe, bulle) dans la couche modale de `Boot`, des cibles nommées enregistrées par les vues, l'étape dans le profil, les textes dans la localisation.

## 7. Hors périmètre
Code du jeu (sous-projet C), nouvelles illustrations (portraits provisoires extraits de l'illustration clé), traduction des maquettes, animations.

## 8. Livrables
- `03_Art_UI_UX/Maquettes/_sources/` (kit, pages, `export.ps1`, `index.html` cliquable).
- `03_Art_UI_UX/Maquettes/ecrans/M*.png`, `03_Art_UI_UX/Maquettes/tutoriel/T*.png`, `03_Art_UI_UX/Maquettes/reference/M90-M91.png`.
- `03_Art_UI_UX/03_Maquettes_Screen_Flow.md` réécrit ; `01_UX_Ecrans.md` (navigation, paramètres, onboarding) et `05_Production/Decisions.md` mis à jour.
