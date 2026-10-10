# Outil de level design — design

> Statut : demandé par l'utilisateur (2026-10-07), implémenté sur `feature/outil-level-design` (départ de `main`). Sous-projet **E** du découpage A → B → C → E → D.

## 1. Demande
Un `EditorWindow` pour créer les plateaux : placer les cases et définir leur type (activité → laquelle ; combat → table d'ennemis et nombre de rounds), créer et relier les chemins, **générer le fond de carte** (plan 2D « par défaut » façon carte en ligne, stylisé simple, biomes forêt / ville…, météo jour / nuit / neige / pluie…, référence graphique : la carte de Spider-Man 2 PS5), et **créer / éditer la carte de campagne** (1.1, 1.2…).

## 2. Données
- **`ZoneAsset`** (`Resources/Zones/<id>.asset`, assembly `N29.HeroesDawn.Authoring`) : un plateau — cases (type, rang, fixe, position de grille, activité, table d'ennemis + manches + ennemis par manche), liens, objectifs, rencontres de repli par rang, biome de base et météo, **zones de biome peintes**, position sur la carte de la ville, clé et bornes du fond généré.
- **`CampaignAsset`** : l'ordre des zones, l'acte de chacune, le code (« acte.rang dans l'acte », surcharge possible) et les prérequis. Quand une campagne existe dans `Resources/Zones`, elle est la liste des zones créées dans l'outil ; sinon, toutes les zones du dossier.
- Au démarrage (`RootLifetimeScope`), `ZoneLibrary.Load()` convertit les assets en `ZoneDef` ; `ContentCatalog.WithZones` **remplace** une zone du code de même identifiant ou **ajoute** les nouvelles. Les textes saisis (nom, nom court, description, objectifs) sont publiés sous les clés du jeu (`Loc.Override`).
- Nouveaux éléments de domaine : `CombatSpec` (table d'ennemis + manches, `NodeDef.Combat`) avec `EncounterGenerator` ; `ZoneBackground` ; `ZoneMapPoint`. Sans `CombatSpec`, une case Combat garde les rencontres de la zone par rang (comportement d'avant).

## 3. Fond de carte
- `MapBackgroundGenerator` (domaine, entier, graine) : sol, routes à contour, bâtiments, parcs, eau, arbres, dunes, voies ferrées, jetées selon le **biome** (ville, port, industrie, forêt, neige, désert) ; zones peintes d'un autre biome (cercles) ; **météo** : teinte globale, fenêtres éclairées la nuit, traits de pluie, flocons, brume.
- `MapRasterizer` (`Authoring`) dessine les formes dans une texture ; l'outil l'enregistre en PNG (`Resources/Zones/<id>_bg.png`, sprite) à 85 × 125 px par case de grille (moitié de la taille en jeu : peu de poids).
- En jeu, `BoardMapView` place le fond derrière les liens, cadré sur la grille qu'il couvre. Pas de fond généré : fond uni, comme avant.

## 4. L'outil (Heroes Dawn ▸ Level Design)
- **Éditeur de plateaux** : canevas (grille, fond, zones peintes, liens, cases colorées par type, anneaux d'objectif / erreur / avertissement), outils **Sélection** (glisser une case), **Placer** (type au choix), **Relier** (clic sur deux cases : crée ou retire le chemin), **Supprimer**, **Zone** (glisser pour peindre un biome) ; clic droit ou Alt+glisser pour déplacer la vue, molette pour zoomer, Suppr pour effacer la case choisie ; annulation / rétablissement d'Unity.
- **Inspecteur** : plateau (identifiants, noms, acte, quartier, niveau, prérequis, biome, météo, position sur la carte de la ville), case (type, fixe, position ; combat : rang, table d'ennemis, manches, ennemis par manche ; mini-jeu : activité du catalogue), objectifs, rencontres de repli, zones de biome, fond (graine, générer / régénérer / supprimer), **validation** en direct avec saut vers la case concernée.
- **Campagne** : ordre, acte, codes calculés, surcharge de code, prérequis (et « chaîner »), nouvelle zone, ajout d'une zone existante, ouverture dans l'éditeur, position sur la carte de la ville, état de validation de chaque zone.
- **Importer les zones du prototype** : transforme les zones du code en assets (avec leur fond) et crée la campagne ; en ligne de commande `-executeMethod N29.HeroesDawn.EditorTools.LevelDesign.ZoneBaker.ImportPrototypeBatch -quit`.
- La carte de la ville utilise la position de la zone (`ZoneMapPoint`) quand elle existe, et ajoute un point pour les zones créées dans l'outil.

## 5. Validation (`ZoneValidator`)
Erreurs (plateau injouable) : entrée absente ou de mauvais type, identifiants ou liens en double, liens vers une case inconnue ou sur soi, cases superposées, cases inaccessibles depuis l'entrée, combat sans ennemi dans sa table ou avec un ennemi inconnu, combat sans aucune rencontre disponible, activité inconnue, objectif sur une case inconnue, inaccessible ou de mauvais type. Avertissements (écarts au GDD) : impasses, moins de 3 embranchements, taille hors de 15-35 cases, ≠ 3 objectifs, case d'objectif ou combat Élite non fixes, aucun objectif de combat clé.

## 6. Hors périmètre
Éditeur de dialogues et de textes de lore ; prévisualisation de la visibilité (brouillard) dans l'outil ; fond de carte peint à la main (le fond se règle par biome, météo, graine et zones peintes) ; pincement tactile.
