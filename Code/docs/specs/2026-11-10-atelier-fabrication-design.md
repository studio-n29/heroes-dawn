# Bloc B — Atelier de fabrication : Composants, catalyseurs, étoiles 1 à 6

> Spec du 2026-11-10. Source : point de développement « finaliser la 1.0.0 », partie « Création d'objet / Labo de recherche ». Remplace `2026-10-27-labo-fabrication-design.md` (lot E de la 0.14.0) pour la fabrication, et supprime la « recherche » du labo. Bloc B sur 5 ; indépendant du code du bloc A, mais les étoiles d'objets (1 à 6) suivent la logique des étoiles de cartes (1 à 5).

## Décisions validées

- Le **Laboratoire de recherche** disparaît : plus de recherches payées ni de bonus (`ResearchLab`, `LabBonuses`).
- L'**Atelier de fabrication** est un onglet à part, avec une interface propre (écrans : lot UI, session « Aube tactique », prévenue).
- Les **scraps** deviennent des **Composants** : 4 types (Arme, Armure, Outil, Implant) × 3 qualités (Commun, Raffiné, Exceptionnel). Le Noyau disparaît.
- Un **catalyseur** thématique (optionnel, un par fabrication) oriente les statistiques : Métal → DEF, Énergie → ATQ, Biologique → PV, Cristal → VIT.
- Les **recettes** s'obtiennent en récompense (aventure, repaires, tournois), plus de difficulté ni de compétence de création.
- Qualité de l'objet : **1 à 6 étoiles**, fixée par la qualité des Composants engagés puis un tirage −1 / 0 / +1. Les statistiques sont tirées dans une plage (comme Diablo III) puis multipliées par l'étoile.

## 1. Règles (domaine)

### Composants et catalyseurs
- `ComponentKind { Weapon, Armor, Tool, Implant }` (correspond à `ItemType`), `ComponentGrade { Common, Refined, Exceptional }`, `CatalystKind { Metal, Energy, Bio, Crystal }`.
- Une fabrication utilise des Composants d'**une seule qualité**.

### Recettes (12 conservées)
`RecipeDef(id, type, rarity, componentCost, minGrade, stats[], perk, perkValue, source)` :

| Rareté de la recette | Composants | Qualité minimale |
|---|---|---|
| Commune | 6 | Commun |
| Rare | 8 | Raffiné |
| Épique | 10 | Raffiné |

- `stats` : 1 à 2 statistiques (PV, ATQ, DEF, VIT) avec leur plage `[min, max]` en % (inchangées : Commune 8–12 ; Rare 8–14 ; Épique 10–16 + effet spécial fixe).
- `source` : `RecipeSource { Starter, Adventure, Lair, Tournament }`. Les 4 Communes sont `Starter`. Les autres sont posées `Adventure` jusqu'aux blocs C et D qui les répartissent.

### Étoiles
- Étoile de départ selon la qualité engagée : Commun 2, Raffiné 4, Exceptionnel 5.
- Tirage : −1 (25 %), 0 (50 %), +1 (25 %), borné à 1–6. Le 6 n'apparaît donc qu'avec des Composants Exceptionnels et un +1.
- Multiplicateur de statistiques (en %) : ★1 80 · ★2 100 · ★3 115 · ★4 130 · ★5 150 · ★6 175.

### Statistiques
- Chaque statistique de la recette est tirée dans sa plage `[min, max]`, multipliée par l'étoile, arrondie au plus proche, au moins 1 (comme aujourd'hui).
- Avec un catalyseur : la statistique orientée est ajoutée à l'objet (jusqu'à 3 statistiques) ; si la recette la contient déjà, elle est tirée dans la moitié haute de sa plage (`[(min+max)/2, max]`). La plage d'une statistique ajoutée est celle de la première statistique de la recette.

### Sources (valeurs de départ ⚑)
Après un combat gagné, un type de Composant tiré au hasard :
- Facile : 1–2 Commun. Normal : 2–3 Commun.
- Élite : 3–4 Commun + 1–2 Raffiné ; catalyseur 10 %.
- Boss : 3–4 Raffiné + 1 Exceptionnel une fois sur deux ; catalyseur 30 %.
- Recette inconnue : Boss 60 %, Élite 25 % (inchangé) ; sans recette inconnue, 5 Composants Commun de plus.
Les repaires et les tournois (blocs C et D) ajouteront leurs propres tables.

La boutique de scraps devient la **boutique de Composants** (Commun et Raffiné), même fonctionnement.

## 2. Application et sauvegarde

- `CraftingOps` : compteurs `Components(kind, grade)`, `Catalysts(kind)`, recettes connues (inchangé).
- `Workshop.Craft(recipeId, grade, catalyst?)` remplace `Craft(recipeId)` ; `RecipeView` expose le coût, la qualité minimale, l'étoile de départ et les chances (25 / 50 / 25).
- Objets fabriqués : `CraftedItemSave.stars` (1 à 6) remplace `quality` ; `ItemDef.NameKey` : `craft.item.<recette>.<étoiles>`.
- **Migration au chargement** (idempotente) :
  - Les scraps deviennent des Composants Commun du même type ; chaque Noyau devient un catalyseur, en rotation Métal, Énergie, Biologique, Cristal.
  - Les recettes connues sont conservées. Les niveaux de compétence de création sont supprimés, sans remboursement.
  - Un objet fabriqué sans champ `stars` prend l'ancienne qualité + 1 (Grossier 1 … Chef-d'œuvre 5) ; ses statistiques ne changent pas.
  - Les recherches du labo sont supprimées : les Crédits dépensés sont **remboursés** (somme de 500 × (n+1) pour chaque niveau n atteint, par recherche) ⚑ et les bonus disparaissent (prix d'entraînement, Crédits de combat, récupération, récompense de zone reviennent à la base).

## 3. Interface — hors périmètre
L'onglet Atelier (écran de fabrication, inventaire de Composants, choix de la qualité et du catalyseur, aperçu des étoiles) est à faire dans le lot UI après la refonte. Ce lot ne modifie que ce qui est nécessaire pour que le projet compile (`LabPage`, `LocCrafting`).

## 4. Documentation
`01_Game_Design/04_Economie.md` (scraps → Composants, boutique), GDD (Atelier), `Decisions.md` (suppression du labo et de la compétence de création, étoiles 1–6, catalyseurs, remboursement), `Code/docs/specs/2026-10-27-labo-fabrication-design.md` (note « remplacée par … »).

## 5. Tests
Domaine : étoile de départ par qualité, tirage −1/0/+1 et bornes 1–6, multiplicateurs, plages de statistiques (avec et sans catalyseur), sources de Composants par rang de combat. Application : fabrication (coût, qualité minimale, catalyseur), refus sans Composants suffisants, boutique de Composants. Sauvegarde : migration des scraps, des Noyaux et des anciens objets, remboursement des recherches, idempotence.

## Points ouverts ⚑
Quantités de Composants par recette, sources, chances de catalyseur et remboursement : à régler au playtest. Les sources « repaire » et « tournoi » des recettes sont fixées aux blocs C et D.
