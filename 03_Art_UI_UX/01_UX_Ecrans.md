# UX — Écrans, parcours, contrôles

> Orientation paysage. Tactile d'abord, souris/clavier sur PC (Steam).
> Maquettes, wireframes annotés et screen flow : `03_Maquettes_Screen_Flow.md`.

## Navigation (maquettes 2026-10-04)
- **Barre de navigation basse** permanente sur le hub : `QG · ÉQUIPE · PASS · BOUTIQUE` (un menu = un onglet, badges par onglet). Depuis la 0.8.0, le recrutement est l'onglet « Héros » de la Boutique unique.
- **Boutique unique à onglets** (0.8.0) : `HÉROS · BOOSTERS · OBJETS · GEMMES ET CRÉDITS · CADEAU`. L'onglet Héros est la page Recruter (3 héros par jour). Les boosters s'ouvrent dans une popup animée : paquet à toucher, flash, cartes retournées une à une avec un éclat selon la rareté, résumé avec « Nouveau ».
- **Barre haute** : titre de l'écran, Crédits / PR / Gemmes (toucher = origine de la ressource), ⚙ Paramètres.
- **QG** (onglet par défaut) : les 5 pièces en cases de BD (toucher = y entrer), missions du jour, alertes, fil du QG, bouton **MISSION** rouge sous le pouce droit.
- **Mode mission** (carte → résultats) : plein écran sans barre de navigation ; retour en haut à gauche, ⏸ = pause (Reprendre, Paramètres, Aide, Retour au QG).
- Badges de notification : entraînement terminé, expédition revenue, labo à collecter, pool renouvelée, palier de pass.

## Liste des écrans
| # | Écran | Accès | Contenu principal |
|---|---|---|---|
| 1 | Titre / connexion | Lancement | Connexion UGS (invité, Google, Apple, Steam), choix de langue |
| 2 | QG (hub) | Défaut | Coupe du QG, notifications |
| 3 | Salon d'entraînement | QG | Membres à l'entraînement, objets placés, accélération |
| 4 | Infirmerie | QG | Jauges de fatigue, emplacements de récupération |
| 5 | Salle des opérations | QG | Expéditions idle : envoi, minuteurs, retours |
| 6 | Labo de recherche | QG | Génération de PR, collecte |
| 7 | Archives | QG | Fiches des membres, lore, scènes débloquées |
| 8 | Carte de la ville | Mission | 5 arrondissements, zones, objectifs secondaires, mode difficile |
| 9 | Préparation de mission | Carte | Choix des 4 membres, fatigue, consommables, aperçu des combos possibles |
| 10 | Plateau | Préparation | Carte illustrée, dé, jauge de dé bonus, brouillard, objectifs de zone |
| 11 | Combat | Case Combat | Voir « HUD de combat » |
| 12 | Mini-jeu | Case Mini-jeu | 4 formats |
| 13 | Événement narratif | Case Événement | Illustration, texte, 2-3 choix |
| 14 | Résultats | Fin de combat / zone | Récompenses, XP, fatigue, progression des objectifs |
| 15 | Recrutement | Hub | Pool du jour (5 cartes), coût en PR, renouvellement |
| 16 | Équipe / fiche membre | Hub | Stats, compétences, paliers, famille, combos |
| 17 | Pass saisonnier | Hub | Pistes gratuite et premium, paliers |
| 18 | Boutique | Hub | Gemmes, packs de PR, accélérateurs, cosmétiques |
| 19 | Paramètres | ⚙ (hub) ou Pause (mission) | Audio, graphismes & performance, accessibilité, langue, notifications, contrôles PC, compte & confidentialité, support & aide (dont « Revoir le tutoriel ») |

## Parcours clés
- **Session type (5-10 min)** : QG (collecter) → Mission → reprendre la zone → 2-4 lancers → 1 combat → résultats → retour QG.
- **Recrutement** : notification « nouvelle pool » → Recrutement → choisir → fiche du membre → l'envoyer au salon.
- **Fin d'acte** : mini-climax → scène → Légende ajoutée à la pool → notification.

## Onboarding : tutoriel guidé (décision 2026-10-04, remplace le prologue scripté)
Première partie guidée sur la **vraie zone 1.1** (maquettes T01–T12) : Rayner explique en bulle de BD, un projecteur désigne l'élément à toucher, le reste est bloqué.
1. Bienvenue au QG · 2. Compléter l'escouade · 3. MISSION puis Déployer · 4. Objectifs de la zone · 5. Lancer le dé · 6. Choisir une destination (brouillard) · 7. Temps actif du combat (combat figé pendant la bulle) · 8. Compétence puis cible · 9. Résultats et fatigue.
Puis **astuces contextuelles** non bloquantes à la première rencontre de chaque mécanique. Étape enregistrée dans le profil ; « Passer » avec confirmation ; « Revoir » dans Paramètres ▸ Support & aide.
5. Déblocage progressif pendant l'Acte 1 : salon (zone 1), infirmerie (zone 2), expéditions et labo (fin zone 3), pass (fin Acte 1).

## HUD de combat
- Bas de l'écran : **4 portraits** avec jauge ATB et fatigue.
- Toucher un portrait prêt → ses **3 compétences** s'affichent au-dessus.
- Droite : bouton **Combo** (apparaît quand 2 jauges pleines forment une paire), bouton **Auto**, vitesse ×1/×2.
- Toucher un ennemi = cibler.

## Contrôles PC
- Souris : identique au tactile. Clavier : 1-4 portraits, Q/W/E compétences, C combo, A auto, Espace lancer le dé.

## Accessibilité (lancement)
- Taille de texte réglable, mode daltonien (familles identifiées par icône + couleur), sous-titres, réduction des effets de flash, QTE avec option « temps étendu ».

## Lot F (0.14.0) : Profil, boutique en cartes, soins sur la fiche d’agent

- **Profil** (dernière tab) : tag personnel `Pseudo#1234` (copiable), pseudo, héros, niveau de pass ; rangées « Registre des agents » et « Archives » ; huit statistiques ; liste d’amis (ajout par tag dans une fenêtre de saisie, bouton Retirer ; amis simulés en local, « en attente de connexion en ligne »).
- **Boutique** : les onglets Boosters, Objets du jour, Gemmes et crédits et Cadeau affichent des cartes en grille (icône SVG ou illustration d’objet) ; description et bouton d’achat dans la fiche de droite. L’onglet Héros reste la page Recruter.
- **Fiche d’agent** : bouton Soigner (−30 de fatigue, 50 Crédits) à côté de l’entraînement ; plus de pages Salon ni Infirmerie. Page Équipe : bouton « Soigner toute l’équipe » (150 Crédits). Au QG : une carte « Équipe et soins ».

## Lot G (0.14.0) : narration, chapitres, dossier d'agent

- **Cinématique** : 11 plans, légendes de la narration du PDF au temps de lecture (sans voix pour l'instant ; un `vo_<plan>.wav` déposé est joué).
- **Chapitres** : 8 chapitres (New York et Moscou). Fenêtre « CHAPITRE n · Titre » au premier déploiement dans une zone du chapitre, une fois ; page Chapitres (statuts Terminé, En cours, Verrouillé, Bientôt, bouton Relire) ouverte par le bouton « Chapitres » de la carte.
- **Fiche d'agent** : bouton DOSSIER à côté de Soigner : panneau défilant avec l'identité, le lien à la Supernova, l'histoire d'origine, le pouvoir et l'apparence.

## Lot H (0.14.0) : mini-jeux 2D

Une seule fenêtre commune (`Popups/MiniGame2DPopup`) : titre, bandeau score / temps / vies, grande zone tactile avec un fond illustré teinté par l'environnement, joystick flottant (Infiltration, Survie en neige), Abandonner. Le premier appui lance la partie ; fin de partie, abandon et annulation rendent toujours la main.

| Jeu | Commande | Placement (Brooklyn) |
|---|---|---|
| Analyse ADN | toucher les molécules vertes qui pulsent | 1.1 (n08) |
| Le rythme | toucher la piste quand l'ennemi passe la ligne | 1.1 (n15) |
| Choix des cartes (dialogue) | inchangé | 1.1 (n21) |
| Caisse au choix | maintenir pour charger, relâcher sur la bonne caisse | 1.2 (n06) |
| Engrenage | toucher un engrenage pour inverser son sens | 1.2 (n13) |
| Tir franc | maintenir pour la puissance, relâcher pour lancer | 1.3 (n06) |
| Infiltration | joystick, éviter les cônes de vision | 1.3 (n11) |
| Ingénierie de secours | manuel d'indices, pièces à assembler dans l'ordre | 1.4 (n06) |
| Survie en neige (vue de dessus) | joystick, jauge de survie, totems | 1.4 (n11) |

Fonds : un SVG source par jeu (`Art/MiniGames`), rasterisé en PNG (`Resources/MiniGames`) par Heroes Dawn ▸ Setup ▸ Générer les fonds de mini-jeux.

## Lot I (0.14.0) : compte et sauvegarde cloud

- **Écran titre** : « Jouer » (invité) et, en bas à droite, « Se connecter » (devient l'e-mail du compte une fois connecté).
- **Fenêtre Compte** : onglets Connexion / Créer un compte ; e-mail, mot de passe, pseudo (création) ; « Continuer avec Google » et « Continuer avec Apple » ; « Mot de passe oublié ? » ; erreurs en français sous les champs ; voile d'attente pendant la requête.
- **Choix de progression** : deux cartes (cet appareil, le compte) avec niveau, Crédits, zones et date ; « Garder cet appareil », « Garder le compte », « Plus tard » (Retour = rien n'est écrit). L'autre version est gardée dans une sauvegarde de secours.
- **Profil** : rangée « Compte » en tête. Invité : « Lier mon compte ». Connecté : e-mail, fournisseur, dernière synchronisation, « Synchroniser », « Se déconnecter » (confirmation).
- **Synchronisation** : au lancement, à chaque sauvegarde (au plus un envoi toutes les 20 s), à la mise en arrière-plan et à la fermeture ; hors ligne, le jeu continue et réessaie.
