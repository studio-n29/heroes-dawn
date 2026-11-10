# Bloc C — Repaires, boss à phases et styles d'IA

> Spec du 2026-11-10. Source : point de développement « finaliser la 1.0.0 », parties « Les repaires », « Les combats de boss » et « Comportement IA ». Bloc C sur 5. Dépend du bloc A (positions) pour le style par défaut des ennemis et du bloc B (Composants, catalyseurs, recettes) pour les récompenses de repaire.

## Contexte
« The Dawn tente activement de localiser les organisations de méta-humains pouvant utiliser leurs compétences pour faire le mal. »

## Existant (0.15.0)
- Un repaire est une **zone** (`ZoneDef.IsLair`) en ligne droite : le Repaire de l'AMIF (`zone_lair_amif_vault`, 15 cases, boss Brick, Crocodile et le Titan du pont, repos et élites). `LairDef` / `LairBossDef` (`Definitions/Lairs.cs`) et `LairBook` (boss vaincus, nombre de repaires terminés) le suivent.
- Un boss a des **phases** (`BossPhaseDef`) qui ne changent que ATQ / DEF / VIT à des seuils de PV.
- L'IA ennemie dépend du rang du combat (`CombatSimulation.Ai.cs`), pas du style de l'ennemi.

## Décisions validées

### 1. Styles d'IA
`AiStyle { Balanced, Aggressive, Defensive, Control, Support }`.

| Style | Compétence | Cible |
|---|---|---|
| Aggressive | la compétence de dégâts la plus lourde dès qu'elle est prête | l'allié ayant le moins de PV |
| Defensive | bouclier ou provocation si lui ou le groupe est sous 60 % de PV ; sinon attaque | la menace la plus forte (ATQ la plus haute) |
| Control | statut de contrôle (Entrave, Choc, Gel, Marque, Vulnérabilité, Aveuglement) sur une cible qui n'en a pas ; sinon attaque | la cible la plus rapide sans statut de contrôle |
| Support | soin ou renfort d'un allié sous 50 % de PV ; sinon attaque | l'allié ayant le moins de PV |
| Balanced | logique actuelle (soin sous 40 %, puis A3, A2, A1) | l'allié ayant le moins de PV |

- `EnemyDef.Style` explicite ; sans style saisi, il vient de la position (bloc A) : Attaquant et Précision à distance → Aggressive, Première ligne → Defensive, Contrôle → Control, Soutien → Support.
- Le rang du combat règle la finesse : Facile garde le hasard (style ignoré), Normal applique le style une fois sur deux, Élite et Boss toujours. L'IA des alliés ne change pas.

### 2. Boss à phases
`BossPhaseDef` gagne, en plus des modificateurs de stats : `IReadOnlyList<SkillDef>? Skills` (nouveau jeu de compétences ; nul = inchangé), `AiStyle? Style` (nul = inchangé). À l'ouverture d'une phase : les compétences et le style sont remplacés, les recharges sont remises à zéro. Un boss a 2 à 4 phases, la phase initiale comprise, soit 1 à 3 seuils de PV (Brick en a 2, donc 3 phases). `BossPhaseChangedEvent` indique si le jeu de compétences a changé (`KitChanged`).

### 3. Repaires
- Format conservé : ligne de cases, boss fixes, repos et élites, suivi par `LairBook`. Rejouables.
- `LairBossDef` gagne `LairRewardDef` : Composants (qualité et quantité), chance de catalyseur, recette (source `Lair`, donnée à la première victoire seulement).
- Récompenses **complètes une fois par jour et par repaire**, réduites de moitié ensuite (quantités de Composants et chance de catalyseur ; la recette n'est donnée qu'une fois).
- À M0 : 4 repaires (un par ville). Livrés ici : le moteur et le Repaire de l'AMIF (New York) avec styles, phases et récompenses. Les repaires de Paris, Moscou et Salem demandent des boss, des zones et des textes qui n'existent pas dans le lore : lot de contenu à part, avec le narratif.
- Le repaire hebdomadaire est un événement LiveOps, hors lot.

### 4. Équilibrage
Chaque boss est joué en simulation automatique (simulateur de l'éditeur) : taux de victoire visé de 40 à 60 % avec une équipe du niveau attendu.

## Tests
Choix de compétence et de cible par style ; style par défaut selon la position ; rang du combat (Facile hasard, Normal une fois sur deux) ; changement de jeu de compétences à l'ouverture de phase, recharges remises à zéro, déterminisme (même graine, mêmes événements) ; une attaque qui franchit deux seuils ouvre les deux phases dans l'ordre ; récompenses de repaire (première victoire, deuxième du jour réduite, nouveau jour complet) ; sauvegarde sans anciennes données de récompense.

## Hors périmètre
Écrans (page Repaire, fiche de boss avec phases) : lot UI. Repaires de Paris, Moscou et Salem et leurs textes : lot de contenu avec le narratif.

## Points ouverts ⚑
Seuils et compétences des phases des trois boss existants (valeurs de départ dans le plan) ; quantités de récompenses ; taux de victoire cible à confirmer par la simulation.
