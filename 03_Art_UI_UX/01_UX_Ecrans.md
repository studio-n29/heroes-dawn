# UX — Écrans, parcours, contrôles

> Orientation paysage. Tactile d'abord, souris/clavier sur PC (Steam).
> Maquettes, wireframes annotés et screen flow : `03_Maquettes_Screen_Flow.md`.

## Navigation (maquettes 2026-10-04)
- **Barre de navigation basse** permanente sur le hub : `QG · ÉQUIPE · RECRUTER · PASS · BOUTIQUE` (un menu = un onglet, badges par onglet).
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
