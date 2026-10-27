# Cartes : 10 niveaux, paliers d'effets et 4 nouvelles mécaniques (lot C1 de la 0.14.0)

> Spec du 2026-10-26. Source : document N29 HQ « Playtest 0.13.0 — Préparation de la version 0.14.0 », point « Niveau des cartes de compétences » (effet principal + effets secondaires à des paliers de niveau). Le lot C est découpé : **C1** (ce document : moteur) puis **C2** (36 nouvelles cartes, 12 par famille, 4 par nature). S'appuie sur le lot B (`2026-10-26-categories-agents-cartes-objets-design.md`).

## Décisions
- Les cartes montent jusqu'au **niveau 10** ; **paliers aux niveaux 3, 6 et 10**, chacun ajoutant un ou plusieurs effets secondaires.
- Un palier **ajoute des effets** à la liste de la compétence (pas de compétence réécrite par palier).
- Quatre **nouveaux types d'effet** dans le moteur de combat : **Vol de vie**, **Purge**, **Rebond**, **Exécution**.

## Niveaux et économie (`Definitions/CardLevels.cs`)
- `Max = 10`, `PercentPerLevel = 10` : puissance des dégâts, soins et boucliers de base = 100 % + 10 % par niveau au-dessus de 1 (140 % au niveau 5, 190 % au niveau 10).
- `CopiesToUpgrade(level) = (level + 1) / 2` (division entière) : 1, 1, 2, 2, 3, 3, 4, 4, 5 → 6 doublons jusqu'au niveau 5, 25 jusqu'au niveau 10.
- `CreditsToUpgrade(level) = 200 * level` → 2 000 Crédits jusqu'au niveau 5, 9 000 jusqu'au niveau 10.
- `TierLevels = { 3, 6, 10 }`.
- **Cartes déjà montées** : niveau sauvegardé conservé (1 à 5 reste valide), aucun remboursement. Une carte niveau 5 passe de 160 % à 140 % de puissance de base mais gagne son effet du palier 3.

## Paliers (`Definitions/CollectionDefs.cs`)
- `public sealed class CardTier { public int Level { get; } public IReadOnlyList<EffectDef> Effects { get; } }`.
- `SkillCardDef.Tiers` (`IReadOnlyList<CardTier>`, vide par défaut) ; constructeur : paramètre facultatif `IReadOnlyList<CardTier>? tiers = null`, placé après `nature`.
- `CardLevels.SkillAt(SkillCardDef card, int level)` → `SkillDef` : effets de base passés par `ScalePercent(EffectPercent(level))`, **puis** les effets des paliers dont `Level <= level`, dans l'ordre des paliers, **non augmentés**. Même identifiant, emplacement, cible et coût que la compétence de base.
- `UnitFactory` (combat) et `CollectionText.CardLine` utilisent `SkillAt` à la place de `Scale`.

## Les 4 mécaniques (`Definitions/Enums.cs` : `EffectType` ; `Domain/Combat/CombatSimulation.Actions.cs` : `ApplyEffects`)
Les effets restent appliqués **cible par cible, dans l'ordre de la liste**, chacun avec sa chance de déclenchement.

| Type | `Value` | Règle |
|---|---|---|
| `Lifesteal` | % | Le lanceur récupère `Value` % des dégâts infligés **à cette cible** par les effets précédents de la même compétence (après absorption par un bouclier), plafonnés à ses PV max. Aucun dégât : rien. Émet `HealEvent` sur le lanceur. |
| `Purge` | nombre de statuts (0 = tous) | Cible alliée : retire ses statuts négatifs (Burn, Freeze, Shock, Corrosion, Blind, Vulnerable, Bind, Mark). Cible ennemie : retire ses statuts positifs (Shield, Regen, Haste, Evade, Anchor, Taunt). Les plus récents d'abord. Émet un `StatusExpiredEvent` par statut retiré, puis `PurgedEvent(cible, nombre)` si au moins un a été retiré. |
| `Bounce` | % | Un autre ennemi vivant, hors des cibles de la compétence, est tiré au sort avec le générateur du combat ; il subit les dégâts du **premier effet `Damage`** de la compétence à `Value` % de sa puissance, avec la formule normale (critique, Marque, Corrosion, Vulnérable, Esquive, Aveuglement). Émet `BounceEvent(source, cible)` puis les événements de dégâts habituels. Pas d'autre ennemi ou pas d'effet `Damage` : rien (et aucun tirage). |
| `Execute` | % | Si la cible a moins de `BalanceConfig.ExecuteThresholdPercent` (30) % de ses PV max **au moment de l'effet `Damage`**, ces dégâts sont multipliés par `(100 + Value) / 100`. La position de l'effet `Execute` dans la liste ne compte pas. Émet `ExecutedEvent(cible)` quand le bonus s'applique. |

- Nouveaux événements (`Domain/Combat/CombatEvents.cs`) : `BounceEvent(Tick, From, To)`, `ExecutedEvent(Tick, Target)`, `PurgedEvent(Tick, Target, Count)`.
- Déterminisme : le seul tirage nouveau est celui du rebond ; la simulation et `LocalCombatVerifier` passent par le même code et restent identiques.
- **Contrainte de contenu** : un effet s'applique aux cibles de la carte ; une carte qui vise un ennemi ne porte pas d'effet allié (Heal, Shield, statut positif, Advance) ; `Bounce` n'apparaît que sur une carte qui a un effet `Damage`.

## Contenu : paliers des 12 cartes actuelles (`Content/Prototype/PrototypeCollection.cs`)

| Carte | Niveau 3 | Niveau 6 | Niveau 10 |
|---|---|---|---|
| card_glass_wall | Purge 1 | Anchor 1 t | Regen 2 t |
| card_shackle | Vulnerable 2 t | Purge 1 | Shock 1 t |
| card_prism_burst | Corrosion 2 t (30 %) | Execute +30 % | Purge 1 |
| card_ember | Burn 2 t | Bounce 50 % | Execute +50 % |
| card_storm | Blind 1 t (30 %) | Lifesteal 15 % | Vulnerable 2 t |
| card_inferno | Lifesteal 10 % | Corrosion 2 t | Execute +40 % |
| card_second_wind | Purge 0 (totale) | Shield 10 % (2 t) | Regen 2 t |
| card_bulwark_charge | Lifesteal 20 % | Bounce 50 % | Vulnerable 2 t |
| card_regen_field | Purge 1 | Shield 8 % (2 t) | Haste 1 t |
| card_mark | Execute +30 % | Bounce 40 % | Blind 1 t |
| card_precog | Evade 1 t | Purge 0 (totale) | Shield 15 % (2 t) |
| card_time_rift | Regen 2 t | Purge 1 | Evade 1 t |

## Présentation
- **Textes** (`Loc.DescribeSkill` et clés associées) : « Vol de vie {0} % », « Purge ({0}) », « Purge totale », « Rebond {0} % », « Exécution +{0} % sous {1} % PV ».
- **Grimoire** : badge « Niv. x/10 » ; dans la fiche détaillée, bloc **Paliers** : palier atteint en couleur précédé de « ● », palier à venir grisé, ex. « ● Niv. 3 · Brûlure 2 t » / « Niv. 6 · Rebond 50 % ». Bouton Améliorer avec les nouveaux coûts ; s'il fait atteindre un palier, il l'annonce : « Améliorer → débloque Rebond 50 % ». Ouverture de booster inchangée (carte au niveau 1).
- **Combat** : `BounceEvent` → éclat de la 1re cible vers la 2e avec l'impact de la famille de la carte, puis le chiffre de dégâts ; `ExecutedEvent` → onomatopée « EXÉCUTION ! » (style des onomatopées existantes) ; `PurgedEvent` → texte flottant « PURGE », les icônes de statut retirées disparaissent comme à l'expiration ; vol de vie → chiffre de soin habituel sur le lanceur.

## Documentation
- `01_Game_Design/06_Kits_Competences.md`, section « Cartes du grimoire » : niveaux 1 à 10, coûts, paliers, les 4 mécaniques. La section « Paliers de compétences (niv. 10 / 20 / …) » concerne les compétences des agents et reste inchangée.
- `05_Production/Decisions.md` : trois lignes (10 niveaux et économie ; paliers 3/6/10 en effets ajoutés ; 4 nouvelles mécaniques).

## Hors périmètre
Les 36 nouvelles cartes (C2) ; la refonte de l'économie des boosters ; l'usage des nouvelles mécaniques par les compétences des agents.

## Tests
- **Domaine** : `CopiesToUpgrade`, `CreditsToUpgrade`, `EffectPercent` sur les 10 niveaux ; `SkillAt` aux niveaux 2, 3, 5, 6, 9, 10 (effets ajoutés au bon niveau, effets de palier non augmentés). Chaque mécanique sur graine fixe : vol de vie après absorption d'un bouclier et plafonné aux PV max ; purge sur un allié et sur un ennemi, avec limite et totale ; rebond avec un autre ennemi, sans autre ennemi, sans effet `Damage` ; exécution à 31 % et à 29 % des PV, effet placé avant puis après les dégâts. Une simulation avec rebond et sa vérification par `LocalCombatVerifier` donnent le même résultat.
- **Contenu** : chaque carte a exactement 3 paliers, aux niveaux 3, 6 et 10 ; aucune carte visant un ennemi ne porte d'effet allié ; `Bounce` seulement sur une carte avec `Damage`.
- **Application** : `UpgradeCard` jusqu'au niveau 10 avec les nouveaux coûts, refus au-delà ; une sauvegarde avec une carte niveau 5 se recharge au niveau 5.
- **Présentation** : bloc des paliers (atteint / à venir) et bouton qui annonce le palier ; textes des 4 mécaniques ; le présentateur de combat traite les trois nouveaux événements.
