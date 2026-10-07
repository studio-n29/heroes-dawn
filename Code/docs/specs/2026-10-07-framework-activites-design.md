# Framework d'activités — design

> Statut : demandé par l'utilisateur (2026-10-07), implémenté sur `feature/framework-activites` (départ de `main`).
> Sous-projet **C** du découpage A → B → C → E → D. Il prépare **D** (les 5 activités jouables, une par cycle) et **E** (outil de level design : types de case, activité d'une case, biome et météo du plateau).

## 1. Demande
Cinq activités : **potion magique** (2D, cas à aider dans l'équipe, livre d'indices ; échec = malus d'agent pour tout le plateau), **survie en neige** (3D, jauge de survie, totems, objet à trouver), **pêche** (3D, minuteur, pièces au prorata des poissons), **arène de combat** (3D top-down, 2 compétences par agent, vagues simples), **dialogue** (choix de réponses ; échec = combat ou activité liée). Contraintes : niveaux de mini-jeux **procéduraux**, assets qui **changent avec l'environnement** du plateau d'où ils sont lancés, **V1 en primitives de couleur plate**, d'**autres activités à venir** et des **paramètres faciles à modifier**.

## 2. Architecture
| Pièce | Où | Rôle |
|---|---|---|
| `ActivityDef` | `Definitions` | Une activité configurée : type, paramètres, gains, malus d'échec, suite d'échec, patient. **Données pures** : ajouter une activité de plus = ajouter une `ActivityDef`. |
| `IActivityKind` + `ActivityRegistry` | `Domain.Activities` | Un **type** d'activité : schéma de paramètres (`ParameterDef` : défaut, min, max) et générateur de niveau. **Ajouter un type = une classe + `Register`.** |
| `ActivityLevel` | `Domain.Activities` | Niveau généré : primitives (forme, position, taille, rôle, couleur), repères nommés, faits chiffrés, données propres (recette, script). Unités entières (cm), aucune trigonométrie : même niveau partout. |
| `EnvironmentPalette` | `Domain.Activities` | Couleurs plates : teinte du **biome** (ville, port, industrie, forêt, neige, désert) transformée par la **météo** (jour, nuit, neige, pluie, brouillard). Les repères de jeu (cible, flamme, totem) ne sont jamais teintés. |
| `ActivityPlanner` / `ActivityPlan` | `Domain.Activities` | Prépare une activité : patient tiré dans l'escouade, graine dérivée (partie + case + activité + tentative), paramètres complets et bornés, niveau généré. |
| `ActivityRules` | `Domain.Activities` | Résultat → gains (base à la réussite + **pièces par point de score même en échec**), malus, suite. |
| `ZoneRun.PlanActivity / ResolveActivity / Maluses` | `Domain.Board` | Intégration au plateau : activité du nœud (`NodeDef.Activity`, défaut : réflexe historique), environnement de la zone (`ZoneDef.Environment`), journal `ActivityResolved`. |
| `IPlanActivityUseCase` / `IResolveActivityUseCase` | `Application` | Plan et résolution (portefeuille, sauvegarde). `IResolveMiniGameUseCase` (réflexe) passe par le même chemin. |
| `IActivityPopupView` | `Presentation` | Fenêtre d'activité générique (version de test : décrit l'activité et son niveau, le joueur simule le résultat). Chaque activité de D remplacera ce rendu par sa vraie scène. |

## 3. Types intégrés (schémas de paramètres)
- **snow_survival** : durée, perte et recharge de survie (‰/s), totems, rayon de recharge, distance de l'objet, obstacles, ralentissement du terrain. Répartition neige / herbe haute / sable selon le biome.
- **fishing** : durée, poissons, fenêtre de touche, attente min/max, poissons pour réussir, rayon de l'étang.
- **combat_arena** : vagues, ennemis par vague, rayon, obstacles, PV et dégâts ennemis (%), comportement (poursuite, charge, tir) tiré par vague.
- **potion** : ingrédients, étapes, quantité max, essais, tolérance de dosage ; recette secrète + indices du livre (propriété de chaque ingrédient, ordre, doses) ; `PotionRecipe.Evaluate`.
- **dialogue** : répliques, choix par réplique, score requis ; chaque réplique a une bonne, une mauvaise et des réponses acceptables ; `DialogueScript.Evaluate`.
- **reflex** : mini-jeu historique (barre de réflexe), utilisé par défaut.

## 4. Règles de plateau
- Réussite : case résolue (objectif de mini-jeu validé). Échec : case **à rejouer**.
- **Malus** (potion ratée) : -15 % PV / ATQ / DEF du patient jusqu'à la fin du plateau (`ZoneRun.Maluses`, cumulables, plafonnés à -50 %), appliqué au lancement de chaque combat (`AllyEntry.StatPercent`).
- **Suite d'échec** : `FollowUpDef.Combat(rang)` lance un combat depuis la case (gagner ne résout pas l'activité ; perdre fait reculer) ; `FollowUpDef.ThenActivity(id)` enchaîne une autre activité sur la même case (4 au plus par visite).
- Une partie reprise retrouve la même activité (plan idempotent tant qu'elle n'est pas résolue).

## 5. Régler une activité sans code
`Resources/Balance/activities.json` : objet JSON plat « `identifiant_activité.paramètre` : entier », ex. `{ "fishing_docks.time_limit_s": 90, "arena_training.waves": 5 }`. Les valeurs sont bornées par le schéma du type ; une clé inconnue est ignorée ; un JSON illisible laisse les valeurs par défaut. Les libellés de paramètres sont `activity.param.<clé>`.

## 6. Contenu du prototype
Activités : `reflex` (cases sans activité), `potion_field` (zone 1.3, n06), `survival_snow`, `fishing_docks` (zone 1.1, n15), `arena_training`, `dialogue_dockers` (zone 1.1, n21 ; échec = combat Facile). Environnements : 1.1 port de nuit, 1.2 zone industrielle sous la pluie, 1.3 ville dans le brouillard. `survival_snow` et `arena_training` sont définies mais pas encore placées sur une case (sous-projets D et E).

## 7. Hors périmètre
Le gameplay réel des cinq activités (scènes 3D, puzzle de potion, interface de dialogue) : sous-projet D, une activité par cycle ; l'éditeur de plateaux et le fond de carte généré : sous-projet E ; textes des dialogues et des répliques (clés `dialogue.<id>.<ligne>` à rédiger).
