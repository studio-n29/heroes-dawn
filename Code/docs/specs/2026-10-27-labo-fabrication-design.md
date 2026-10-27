# Lot E — Labo de recherche : fabrication d'objets (scraps, recettes, qualité aléatoire)

Date : 2026-10-27 · Version 0.14.0 · Source : PDF « N29 HQ » (playtest 0.13.0), point « Le système d'objet/d'équipement (Laboratoire de recherche) ».

## Objet

Le labo permet de fabriquer des objets avec des **scraps** récupérés en jeu. Une **recette** (débloquée en jeu) dit quels scraps dépenser. L'objet obtenu a des statistiques tirées au hasard et une **qualité** qui dépend du niveau de **compétence de création** du joueur pour ce type d'objet (comme un métier dans WoW ou Diablo).

Décisions validées :

- Deux familles d'objets : les objets de boutique et de drop restent inchangés (stats fixes) ; les objets fabriqués sont des exemplaires uniques. Les deux s'équipent dans les mêmes 2 emplacements (types différents).
- 5 types de scraps : un par type d'objet (arme, armure, outil, implant) et un scrap rare commun, le Noyau.
- Scraps : drops de combat. Recettes : drops de Boss et d'Élite.
- Un niveau de compétence de création par type d'objet, qui monte en fabriquant.
- Fabrication instantanée.

Hors lot : refonte de l'interface (lot F), démontage d'objets, achat de recettes, expéditions qui rapportent des scraps, boutique de scraps.

## 1. Règles (domaine, sans Unity)

Fichier : `Packages/com.n29.heroesdawn.domain/Definitions/Crafting.cs` (types et table) et `Domain/Board/CraftingRules.cs` (tirages).

### Scraps

`ScrapKind { Weapon, Armor, Tool, Implant, Core }`. Les quatre premiers correspondent à `ItemType` (arme, armure, outil, implant) ; `ScrapKinds.For(ItemType)` fait la correspondance.

### Recettes

`RecipeDef(id, nameKey, type, rarity, difficulty, scrapKind, scrapCost, coreCost, stats[], perk, perkValue)` :

- `difficulty` de 1 à 10.
- `stats` : de une à deux statistiques parmi PV, ATQ, DEF, VIT (en %), chacune avec une fourchette `[min, max]`.
- `perk` / `perkValue` : effet spécial existant (`ItemPerk`), uniquement pour les recettes Épiques, valeur fixe non multipliée par la qualité.

Catalogue `Recipes.All` : 12 recettes, 3 par type d'objet.

| Rareté | Difficulté | Coût | Stats (fourchette de base) |
|---|---|---|---|
| Commune | 1 | 6 scraps du type | 1 stat, 8 à 12 % |
| Rare | 4 | 12 scraps du type + 2 Noyaux | 2 stats, 8 à 14 % chacune |
| Épique | 7 | 20 scraps du type + 5 Noyaux | 2 stats, 10 à 16 % chacune + 1 effet spécial |

Les noms et les stats choisies par recette (cohérentes avec le type : arme = ATQ, armure = PV/DEF, outil = VIT, implant = effet spécial) sont fixés au plan. Les 4 recettes Communes (une par type) sont connues dès le début.

### Qualité

`CraftQuality { Rough, Standard, Fine, Superior, Masterwork }` — « Grossier, Standard, Soigné, Supérieur, Chef-d'œuvre » ; multiplicateurs de stats 80, 100, 115, 130, 150 %.

`edge = compétence du type − difficulté de la recette`. Probabilités en % (somme = 100) :

| edge | Grossier | Standard | Soigné | Supérieur | Chef-d'œuvre |
|---|---|---|---|---|---|
| ≤ −3 | 60 | 35 | 5 | 0 | 0 |
| −2 à −1 | 35 | 50 | 13 | 2 | 0 |
| 0 | 15 | 50 | 25 | 9 | 1 |
| +1 à +2 | 5 | 35 | 35 | 20 | 5 |
| +3 à +4 | 0 | 15 | 40 | 30 | 15 |
| ≥ +5 | 0 | 5 | 30 | 40 | 25 |

### Statistiques tirées

Chaque stat de la recette est tirée uniformément (entiers) dans sa fourchette, puis multipliée par le pourcentage de la qualité et arrondie à l'entier le plus proche. Une stat n'est jamais inférieure à 1.

### Compétence de création

4 compétences (une par type), niveaux 1 à 10, 1 au départ. Expérience gagnée par fabrication : `10 + 4 × difficulté` si `edge ≤ 2`, sinon 5. Expérience pour passer du niveau `n` au niveau `n + 1` : `40 × n`. Au niveau 10, l'expérience n'augmente plus.

### Drops

- Scraps à chaque victoire (rang du combat) : Facile 1 à 2, Normal 2 à 3, Élite 4 à 6, Boss 4 à 6, du type tiré au hasard (uniforme sur les 4 types).
- Noyaux : Élite 50 % de chance d'1 Noyau ; Boss 2 Noyaux.
- Recette : Élite 25 %, Boss 60 % ; la recette est tirée uniformément parmi les recettes inconnues. Si toutes sont connues : 5 scraps d'un type tiré au hasard à la place.
- Tous les tirages passent par `IRandom`.

## 2. Données et sauvegarde

`MetaSave` (liste d'ajouts en fin de classe, valeurs par défaut vides) :

- `scraps` : compteurs par `ScrapKind`.
- `recipes` : identifiants des recettes connues.
- `craftXp` : expérience par type.
- `crafted` : liste de `CraftedItemSave { id, recipeId, quality, hp, atk, def, spd }`.
- `craftedNext` : prochain numéro d'exemplaire (`crafted_1`, `crafted_2`…).

Une sauvegarde ancienne (champs absents) se charge sans erreur : compteurs à 0, 4 recettes de départ ajoutées au premier chargement, aucun objet fabriqué.

## 3. Intégration

- `CollectionDef.Register(ItemDef)` ajoute un objet fabriqué ; refuse l'instance partagée `CollectionDef.Empty` (exception explicite). Le catalogue d'une session doit donc être une instance propre.
- `ApplyProfile` réenregistre tous les objets fabriqués avant de relire les équipements des agents ; un identifiant inconnu est ignoré comme aujourd'hui. Les objets équipés sont retrouvés après chargement.
- Un objet fabriqué est stocké comme les autres objets : `CollectionOps.AddItem(session, id)` avec 1 exemplaire.
- `Workshop` (couche Application, `Features/Meta/Workshop.cs`) :
  - `Views()` : recettes connues avec coût, difficulté, scraps suffisants et probabilités par qualité ;
  - `Craft(recipeId)` : dépense les scraps, tire la qualité et les stats, gagne de l'expérience, enregistre l'objet et publie les changements (`MetaArea.Lab`) ; échecs : recette inconnue, scraps insuffisants ;
  - `GrantDrops(rank, boss)` : applique les drops de combat et renvoie un rapport (scraps, recette obtenue).
- Les drops de combat appellent `Workshop.GrantDrops` à côté de `RollCombatDrop` ; le rapport de combat affiche les scraps et la recette.
- Nom d'affichage d'un objet fabriqué : nom de la recette + qualité (« Gilet kevlar soigné »). Illustration : celle du type d'objet.

## 4. Interface (minimale, le reste est au lot F)

- Onglet « Atelier » sur la page du labo : barre des 5 scraps ; liste des recettes (coût, difficulté, bouton Fabriquer grisé si scraps insuffisants) ; niveau et barre d'expérience par type.
- Fenêtre de résultat après fabrication : qualité obtenue et stats tirées.
- Inventaire et fiches d'agent : repère de qualité (couleur et mot) à côté du nom d'un objet fabriqué.
- Textes français dans `Loc` (nouveau fichier de textes dans le même style que `LocCombat`).

## 5. Erreurs et cas limites

- Scraps insuffisants : ni dépense ni objet. Recette inconnue : erreur `Unknown`.
- Aucune limite d'inventaire.
- Les tirages de fabrication n'utilisent pas le générateur du combat : la rejouabilité des combats est inchangée.

## 6. Tests

- Domaine : les 12 recettes (types, coûts, difficultés, fourchettes) ; probabilités qui somment à 100 pour chaque `edge` et sont monotones (plus d'écart ne baisse jamais les chances des paliers supérieurs) ; stats tirées dans les fourchettes × multiplicateur ; expérience et niveaux.
- Application : fabrication refusée sans assez de scraps ; scraps dépensés et objet ajouté ; expérience et montée de niveau ; aller-retour de sauvegarde avec un objet fabriqué équipé ; ancienne sauvegarde sans les nouveaux champs ; drops (bornes par rang, recette inconnue tirée, toutes connues).
- Présentation : textes, présentateur de l'atelier, capture PlayMode de l'onglet.
- Contrôle : `CollectionDef.Empty` refuse `Register`.

## Revue finale — points d'attention

- Un objet fabriqué équipé survit à la sauvegarde et au rechargement.
- Aucun doublon d'identifiant d'exemplaire après chargement.
- Aucune régression sur la boutique du jour et les drops d'objets existants.
