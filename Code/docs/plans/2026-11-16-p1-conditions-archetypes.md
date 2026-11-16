# P1 — Conditions, échelles, ciblage fin, archétypes et catalogue en données : plan d'implémentation

> **Pour les agents d'exécution :** utiliser superpowers:subagent-driven-development, tâche par tâche. Les étapes utilisent des cases `- [ ]`.

**Objectif :** donner aux compétences des conditions, des échelles et un ciblage fin (jouées à l'utilisation), étiqueter les cartes par archétype, passer le catalogue en fichier de données lu depuis les fiches du catalogue, et mettre en jeu une première vague de 12 cartes, **sans changer le comportement des 48 cartes existantes**.

**Architecture :** `EffectDef` gagne une condition facultative (`When`) et des modificateurs de valeur (`Mods`) ; `SkillDef` gagne un mode de ciblage (`Pick`). `ApplyEffects` évalue la condition **avant** le tirage de chance de l'effet (donc sans consommer de tirage pour un effet qui ne s'applique pas) et ajoute les modificateurs à la valeur déjà calculée. Les cartes sont lues depuis `Resources/Content/cards.json` (repli : le catalogue en code). Un importeur lit les tableaux de `docs/catalogue/*.md`.

**Tech Stack :** C# (Unity 6000.6.3f1), NUnit EditMode. Domaine : `Code/Heroes Dawn Unity/Packages/com.n29.heroesdawn.domain`. Contenu : `Code/Heroes Dawn Unity/Assets/_Project`. Tests : `Assets/Tests/Domain|Editor`.

**Specs :** `docs/specs/2026-11-16-capacites-composables-synergies-design.md` (P1), `docs/specs/2026-11-16-catalogue-250-cartes.md` (vocabulaire, format), fiches : `docs/catalogue/cartes-*.md`.

## Contraintes globales
- Les 48 cartes existantes, les ennemis et les boss se comportent **exactement comme avant** : le garde-fou `CombatGoldenTests` (renforcé en tâche 1) reste vert, et sa référence n'est **jamais ré-enregistrée** après la tâche 1 (sauf le fichier distinct `combat-golden-p1.txt` de la tâche 10).
- Aucun nouveau tirage du générateur pour une compétence sans condition ni ciblage aléatoire. Une condition fausse saute l'effet **sans** tirer sa chance. Seuls `Random` et `TwoRandom` tirent (`_rng.Range`).
- Les bonus d'un modificateur s'ajoutent à la valeur **déjà** augmentée par le niveau, le bonus de position/famille et la puissance de compétence ; ils ne sont pas remis à l'échelle. Les effets d'un palier restent non augmentés.
- `Bounce` lit la valeur de base du premier `Damage` (sans modificateurs) ; `Execute` et `Lifesteal` n'ont ni condition ni modificateur en P1.
- `RulesVersion.Value` passe à `"0.2.0"` en tâche 9 (contenu de carte changé : 12 cartes ajoutées) ; pas avant.
- Domaine sans `UnityEngine`. Tests : `[Test, Description("Vérifie que … — parce que …")]` en français pour **chaque** test (règle `TestDescriptionsTests`), namespace `N29.HeroesDawn.Tests.Domain|Editor`. Assertions des tests existants inchangées, sauf celles listées en tâche 9.
- Tout nouveau fichier sous `Assets/` ou `Packages/` est commité avec son `.meta`. Git : racine `E:/Projets/HeroesDawn/principal`, branche `feature/capacites-p1` ; l'arbre contient du travail non commité de l'utilisateur : jamais `git add -A`/`.`, jamais `latest-*.html`, jamais ses fichiers (`CombatEvents.cs`, `StoryRoute.cs`, etc.).
- Unity en batch avec l'éditeur de ce projet fermé : `UNITY="/c/Program Files/Unity/Hub/Editor/6000.6.3f1/Editor/Unity.exe"`, `PROJ="E:/Projets/HeroesDawn/principal/Code/Heroes Dawn Unity"`, `"$UNITY" -batchmode -nographics -projectPath "$PROJ" -runTests -testPlatform EditMode -testFilter "<classe>" -testResults "<xml>" -logFile "<log>"` (xml et log sous `.superpowers/sdd/<plan>/`). Un autre processus Unity (projet ProjectX) peut tourner : ne pas y toucher.
- L'anglais des cartes n'existe pas dans le code (`LocEnglish.cs` ne couvre pas les cartes ; le français sert de repli) : P1 ne traduit rien.

## Faits du code (vérifiés)
- `ApplyEffects` : `CombatSimulation.Actions.cs:144-216` ; la saute des cibles mortes est à la ligne 159, le tirage `_rng.Chance(fx.ChancePercent)` à la ligne 160 (une condition s'insère entre les deux) ; `PerformSkill` l.135 ; `ResolveTargets` l.346-377 ; `ResolveSingleTarget` l.380 (la Provocation prime) ; `LowestHpRatio` l.388. `CombatUnit` : `Hp, MaxHp, Statuses, Has(), Find(), Point, ActionPoint, PlannedSlot, IsReady, IsAlive, Side, Id`.
- `MapEffects`, `CardLevels.ScalePercent` et `CardBonuses.Apply` reconstruisent les `EffectDef` avec `new EffectDef(type, value, status, duration, chance)` : les nouveaux champs seraient perdus si on ne passe pas par `EffectDef.With`.
- Contenu actuel : `Assets/_Project/Content/Prototype/PrototypeCollection.cs` (119 lignes, helpers `C(...)`, `Tiers(...)`, 48 cartes l.62-111). Données : `Assets/_Project/Authoring/{ContentData,ContentDataLibrary,ContentDataConverter,ContentDataValidator}.cs` (JsonUtility : listes et classes simples, pas de polymorphisme) ; chargement dans `Composition/RootLifetimeScope.cs:52-56`. `ContentCatalog` a `WithCollection/WithItems/WithMembers`.
- Textes : `Presentation/Common/SkillText.cs` (`Effects` itérateur, clés `fx.*`), FR dans `LocCombat.cs` ; noms de cartes `skills.<id>` dans `LocCollection.cs` (test `EveryCard_HasAName`). `CollectionText.TierEffects` réutilise `SkillText.Effects`.
- Tests qui comptent les cartes (à adapter en tâche 9) : `CollectionRulesTests.cs:244-257` (48 cartes, 12 par famille, 4 par nature), `CardStarsTests.cs:17-20` (48 ; ★ 12/12/12/8/4), `DesignToolsDataTests.cs:170-207` (`SpecNatures`), `CollectionPagesPresenterTests.cs:237-250` (4 cartes par famille × nature).
- Boosters : `CollectionRules.PickCard` tire uniformément dans les cartes de l'étoile voulue triées par id : un catalogue plus grand fonctionne tel quel (les tirages d'une graine donnée changent).

## Grammaire des cellules `Base`, `Niv.3`, `Niv.6`, `Niv.10` (phase P1)
- Cellule = suite d'éléments séparés par `, ` ou ` ; ` (hors crochets et parenthèses).
- Élément = effet suivi de zéro ou plusieurs crochets, ou crochet seul.
- Effets : `Dmg N` · `Heal N` · `Shield N` · `St <Statut> Nt` (+ ` (C %)` facultatif) · `Push N` · `Adv N` · `Gauge +N` / `Gauge -N` · `Steal N` · `Purge N` · `Bounce N` · `Exec N`. Statuts : Burn, Freeze, Shock, Corrosion, Blind, Vulnerable, Bind, Mark, Taunt, Shield, Regen, Haste, Evade, Anchor.
- Crochet condition : `[si <condition> : <corps>]`. Conditions : `cible a S` · `cible sans S` · `cible < N% PV` · `cible > N% PV` · `lanceur a S` · `lanceur < N% PV` · `un allié a S` · `cible a ≥ N statuts` · `cible a annoncé son action` · `≥ N ennemis vivants` · `≤ N ennemis vivants`.
- Corps : `+N` (bonus de valeur de l'effet qui précède, sinon du premier `Dmg/Heal/Shield/Adv/Push/Gauge` de la cellule) · `<Type> +N` (même bonus s'il existe un effet de ce type dans la cellule ; sinon **effet supplémentaire conditionnel** de ce type et de valeur N) · un effet complet (`St Taunt 2t`, `Shield 15`, `Adv 15`) = effet supplémentaire conditionnel.
- Crochet échelle : `[+N par <compte> (max M)]`. Comptes : `statut de la cible` · `ennemi vivant` · `allié vivant` · `10 % de PV manquants de la cible` · `10 % de PV manquants du lanceur`. **`max M` est le nombre maximal d'unités comptées** (1 ≤ M ≤ 10) ; le bonus total vaut N × min(compte, M). Un `max` qui dépasse 10 est une erreur de fiche à corriger (c'est un plafond de total : le diviser par N).
- Valeur de `Gauge` : en points sur 1 000, signe respecté (`Gauge -N` ralentit).
- Colonne `Cible` : `1E` `ZE` `1A` `ZA` `Soi` ; ciblage fin `1E:faible|fort|prochain|aléatoire|<Statut>` et `1A:faible|prochain|<Statut>` ; `2E:aléatoires`. Correspondance : `1E:*` → `SingleEnemy` ; `1A:*` → `SingleAlly` ; `2E:aléatoires` → `AllEnemies` + `TwoRandom` ; `faible` → `LowestHp`, `fort` → `HighestHp`, `prochain` → `NextToAct`, `aléatoire` → `Random`, `<Statut>` → `StatusHolder`.
- Les modificateurs et conditions ne sont admis que sur `Damage`, `Heal`, `Shield`, `Advance`, `PushBack`, `GaugeDelta` et `ApplyStatus` (condition seule). Toute autre combinaison est une erreur d'import.

## Points d'attention de la relecture (Review Focus)
1. Une condition fausse ne consomme aucun tirage (chance non tirée) → tâche 3.
2. Perte des champs `When`/`Mods` dans `MapEffects`, `ScalePercent`, `Apply`, `SkillAt` → tâche 2.
3. Provocation (`Taunt`) qui prime sur un ciblage fin ennemi ; ciblage sans ennemi ou allié vivant ; `TwoRandom` avec un seul ennemi → tâche 4.
4. Plafond du compte d'une échelle et valeur négative (jamais de valeur finale < 0) → tâche 3.
5. Fiches du catalogue qui sortent de la grammaire (rejet explicite, pas d'import silencieux d'un effet à moitié compris) → tâches 8 et 9.

## Fichiers
| Fichier | Rôle |
|---|---|
| `Definitions/EffectCondition.cs` (créer) | `ConditionKind`, `Condition`, `ScaleKind`, `ValueMod` |
| `Definitions/EffectDef.cs`, `SkillDef.cs`, `CardLevels.cs`, `CardBonuses.cs`, `CollectionDefs.cs` (modifier) | champs, `With`, `Pick`, archétypes, rôle |
| `Definitions/Archetype.cs` (créer) | `Archetype`, `CardRole` |
| `Domain/Combat/CombatSimulation.Actions.cs`, `CombatSimulation.Conditions.cs` (créer) | évaluation des conditions, modificateurs, ciblage |
| `Authoring/CardData.cs`, `CardDataLibrary.cs`, `CatalogImporter.cs` (créer) | enregistrements JSON, chargement, importeur Markdown |
| `Resources/Content/cards.json` (créer) | catalogue en données |
| `Presentation/Common/SkillText.cs`, `LocCombat.cs`, `LocCollection.cs` (modifier) | textes |
| `Assets/Tests/Domain/Golden/*` (modifier/créer) | garde-fou renforcé |

---

### Task 1: Garde-fou renforcé, ré-enregistré au départ de P1

**Files:**
- Modify: `Assets/Tests/Domain/Golden/CombatDigest.cs`, `CombatGoldenTests.cs`, `combat-golden.txt`

**Interfaces:**
- Produit : mêmes noms qu'avant (`CombatDigest.Of`, `CombatGoldenTests.Scenarios()`) ; la référence est le **nouveau** point de départ de P1.

- [ ] **Étape 1 : renforcer `CombatDigest.Of`.** L'état de chaque unité contient désormais `Id:Hp:Energy:Point:` + ses statuts (`Type:TurnsLeft:Value`, dans l'ordre de la liste). Les propriétés d'événement sont lues triées par nom (`StringComparer.Ordinal`) au lieu de l'ordre de réflexion.
- [ ] **Étape 2 : renforcer les scénarios.** (a) Les scénarios `card:*` tournent avec les graines 7 et 11 (clés `:s7`, `:s11`). (b) Le scénario `item-skillpower` utilise la carte `card_ember` et deux objets SkillPower distincts (`neural_chip`, `power_gauntlet`) ; un test `SkillPowerItems_GiveDifferentDigests` exige deux empreintes différentes. (c) Test `EveryEnemyScenario_HasAnEnemyCast` : chaque scénario `enemy:*` logge au moins un `SkillUsedEvent` d'une unité du camp ennemi ; si ce n'est pas vrai, baisser le niveau de l'équipe alliée (30, puis 20) jusqu'à ce que ce le soit, sans toucher aux scénarios de boss qui franchissent des phases. (d) Un scénario `duel:cards:s1` : duel dont les deux équipes portent des cartes. (e) Les scénarios `card:*` et `item-skillpower` itèrent sur une **liste figée des 48 identifiants actuels** (constante `GoldenCardIds` dans `CombatGoldenTests`, relevée dans le catalogue au moment de cette tâche), et non sur `catalog.Collection.Cards` : les cartes ajoutées plus tard (tâche 9) n'entrent donc pas dans `combat-golden.txt` et auront leur propre fichier.
- [ ] **Étape 3 : ré-enregistrer** (`HEROES_RECORD_GOLDEN=1`, filtre `...CombatGoldenTests.RecordGolden`), puis lancer toute la classe deux fois : fichier inchangé après chaque passage.
- [ ] **Étape 4 : commit séparé** `test: garde-fou renforcé et référence ré-enregistrée au départ de P1`. Dire dans le rapport que l'arbre contenait le travail non commité de l'utilisateur (dont `BossPhaseChangedEvent.Lines`) : la référence en tient compte.

### Task 2: `Condition`, `ValueMod`, `EffectDef` étendu

**Files:**
- Create: `Definitions/EffectCondition.cs`, `Assets/Tests/Domain/EffectConditionTests.cs`
- Modify: `Definitions/EffectDef.cs`, `Definitions/SkillDef.cs`, `Definitions/CardLevels.cs`, `Definitions/CardBonuses.cs`

**Interfaces:**
- Produit (`N29.HeroesDawn.Definitions`) :
  - `enum ConditionKind { TargetHasStatus, TargetLacksStatus, TargetHpBelow, TargetHpAbove, CasterHasStatus, CasterHpBelow, AnyAllyHasStatus, TargetStatusCountAtLeast, TargetPlannedAction, EnemiesAliveAtLeast, EnemiesAliveAtMost }`
  - `sealed class Condition { ConditionKind Kind; StatusType Status; int Value; Condition(ConditionKind kind, StatusType status = StatusType.None, int value = 0) }`
  - `enum ScaleKind { TargetStatusCount, EnemiesAlive, AlliesAlive, TargetMissingHpTenths, CasterMissingHpTenths }`
  - `sealed class ValueMod { Condition? When; int Bonus; ScaleKind? Per; int MaxCount; ValueMod(int bonus, Condition? when = null, ScaleKind? per = null, int maxCount = 0) }` (si `Per` est nul : bonus fixe quand `When` est vrai ; sinon `Bonus × min(compte, MaxCount)`, `MaxCount` ≥ 1 exigé).
  - `EffectDef` : nouveaux paramètres facultatifs en fin de constructeur `Condition? when = null, IReadOnlyList<ValueMod>? mods = null`, propriétés `When`, `Mods` (liste vide par défaut), méthode `EffectDef With(int? value = null, int? duration = null, int? chance = null)` qui copie **tous** les champs.
  - `SkillDef.MapEffects`, `CardLevels.ScalePercent`/`SkillAt`, `CardBonuses.Apply` reconstruisent les effets avec `With` : `When` et `Mods` sont conservés.

- [ ] **Étape 1 : écrire les tests** : `EffectDef_With_KeepsConditionAndMods` ; `EffectDef_Default_HasNoConditionAndNoMods` ; `ValueMod_Scaled_RequiresAPositiveMaxCount` (`ArgumentException`) ; `MapEffects_KeepsConditionAndMods` ; `CardBonusesApply_KeepsConditionAndMods_AndScalesTheBaseValueOnly` (`Dmg 100 + mod +40`, bonus de 20 % → valeur 120, bonus du mod toujours 40) ; `SkillAt_KeepsConditionAndModsOfTierEffects`.
- [ ] **Étape 2 : lancer** `EffectConditionTests` ; attendu : échec de compilation.
- [ ] **Étape 3 : implémenter** les types et passer les trois reconstructions par `With` (mêmes formules et arrondis qu'avant, mot pour mot).
- [ ] **Étape 4 : lancer** `EffectConditionTests`, `AbilityTests`, `CardBonusTests`, `CollectionRulesTests`, `CombatGoldenTests`. Attendu : tout vert.
- [ ] **Étape 5 : commit** `feat: Condition, ValueMod et EffectDef.With (comportement inchangé)`.

### Task 3: Évaluation des conditions et des modificateurs en combat

**Files:**
- Create: `Domain/Combat/CombatSimulation.Conditions.cs`, `Assets/Tests/Domain/ConditionCombatTests.cs`
- Modify: `Domain/Combat/CombatSimulation.Actions.cs` (`ApplyEffects`)

**Interfaces:**
- Consomme : `Condition`, `ValueMod`, `EffectDef.When/Mods` (tâche 2).
- Produit : `bool CombatSimulation.Holds(Condition c, CombatUnit actor, CombatUnit target)` et `int CombatSimulation.ModBonus(EffectDef fx, CombatUnit actor, CombatUnit target)` (internes à la simulation) ; dans `ApplyEffects` : après la saute des cibles mortes et **avant** `_rng.Chance`, `if (fx.When != null && !Holds(fx.When, actor, t)) continue;`, puis `int v = Math.Max(0, fx.Value + ModBonus(fx, actor, t))` utilisé à la place de `fx.Value` pour `Damage`, `Heal`, `Shield`, `PushBack`, `Advance`, `GaugeDelta` (le signe d'une jauge négative est conservé : appliquer le bonus en valeur absolue).

- [ ] **Étape 1 : écrire les tests** avec le montage de `CardMechanicsTests.Cast` (carte de test, coût 0, seed fixe) : une condition par `ConditionKind` vraie puis fausse (`TargetHasStatus Burn`, `TargetHpBelow 50`, `CasterHasStatus Haste`, `AnyAllyHasStatus Regen`, `TargetStatusCountAtLeast 2`, `TargetPlannedAction`, `EnemiesAliveAtLeast 3`, `EnemiesAliveAtMost 2`, etc.) ; `FalseCondition_SkipsTheEffect_WithoutDrawingItsChance` (même carte avec un effet à `ChancePercent 50` : le prochain tirage `RngProbeForTests()` est identique avec et sans l'effet conditionnel faux) ; `ModBonus_FlatWhenConditionHolds` (Dmg 100 + mod +40 : dégâts plus forts de 40 points de puissance, vérifier par `DamageEvent` à seed fixe contre une carte `Dmg 140`) ; `ModBonus_Scaled_IsCappedByMaxCount` (`+15 par statut de la cible (max 3)` avec 5 statuts : bonus 45) ; `ModBonus_PerMissingHpTenths` ; `NegativeGaugeBonus_KeepsItsSign` ; `ExtraConditionalEffect_AppliesOnlyWhenTrue` (`St Taunt 2t` sous condition) ; `Bounce_UsesTheBaseValueWithoutMods`.
- [ ] **Étape 2 : lancer** `ConditionCombatTests` ; attendu : échec.
- [ ] **Étape 3 : implémenter** `Holds` et `ModBonus` (compte `TargetMissingHpTenths` = `(MaxHp - Hp) * 10 / MaxHp`, entier) et modifier `ApplyEffects` comme décrit.
- [ ] **Étape 4 : lancer** `ConditionCombatTests`, `CardMechanicsTests`, `CombatGoldenTests`. Attendu : tout vert, référence intacte.
- [ ] **Étape 5 : commit** `feat: conditions et modificateurs de valeur en combat`.

### Task 4: Ciblage fin

**Files:**
- Create: `Assets/Tests/Domain/TargetPickTests.cs`
- Modify: `Definitions/Enums.cs` (ou nouveau `TargetPick`), `Definitions/SkillDef.cs`, `Domain/Combat/CombatSimulation.Actions.cs` (`ResolveTargets`)

**Interfaces:**
- Produit : `enum TargetPick { Default, LowestHp, HighestHp, StatusHolder, NextToAct, Random, TwoRandom }` ; `SkillDef.Pick` et `SkillDef.PickStatus` (`StatusType`), paramètres facultatifs `TargetPick pick = TargetPick.Default, StatusType pickStatus = StatusType.None` du constructeur public et de `FromAbilities`, conservés par `MapEffects`/`WithAbilities` ; `ResolveTargets` : `Pick` ne s'applique que si `Target` est `SingleEnemy`, `SingleAlly` ou `AllEnemies` (`TwoRandom`), en ignorant la cible choisie par le joueur.

- [ ] **Étape 1 : écrire les tests** : `LowestHp_PicksTheLowestPercentEnemy` ; `HighestHp` ; `StatusHolder_PicksTheCarrier_ElseFirstAlive` ; `NextToAct_PicksTheEnemyClosestToItsActionPoint` (rapport `Point / ActionPoint`, égalité : plus petit identifiant) ; `NextToAct_OnAllies` ; `Taunt_StillWinsOverAFineEnemyPick` ; `Random_DrawsOnce_AndIsDeterministicForASeed` ; `TwoRandom_PicksTwoDistinctEnemies_OrOneWhenAlone` ; `Default_ChangesNothing_AndDrawsNothing` (empreinte du générateur identique à un lancer sans `Pick`) ; `MapEffects_KeepsThePick`.
- [ ] **Étape 2 : lancer** `TargetPickTests` ; attendu : échec.
- [ ] **Étape 3 : implémenter** le ciblage dans `ResolveTargets` (les tirages utilisent `_rng.Range`, dans l'ordre des unités pour rester déterministe).
- [ ] **Étape 4 : lancer** `TargetPickTests`, `CombatGoldenTests`, `CardMechanicsTests`. Attendu : vert.
- [ ] **Étape 5 : commit** `feat: ciblage fin des compétences`.

### Task 5: Archétypes et rôles sur les cartes

**Files:**
- Create: `Definitions/Archetype.cs`, `Assets/Tests/Domain/CardArchetypeTests.cs`
- Modify: `Definitions/CollectionDefs.cs` (`SkillCardDef`), `Content/Prototype/PrototypeCollection.cs` (helper `C` et les 48 lignes)

**Interfaces:**
- Produit : `enum Archetype { Burn, Restrain, Hunt, Tempo, Bulwark, Renewal }` (Brûlure, Entrave, Traque, Tempo, Rempart, Renouveau) ; `enum CardRole { Primer, Payoff, Bridge, Basic }` (Amorce, Gain, Pont, Base) ; `SkillCardDef.Archetypes : IReadOnlyList<Archetype>` et `SkillCardDef.Role : CardRole`, paramètres facultatifs de fin de constructeur (`IReadOnlyList<Archetype>? archetypes = null, CardRole role = CardRole.Basic`).

- [ ] **Étape 1 : écrire les tests** : `EveryExistingCard_HasOneArchetype` ; `ArchetypeCounts_MatchTheCatalogueSheet` (Brûlure 2, Entrave 7, Traque 6, Tempo 9, Rempart 16, Renouveau 8) ; `ArchetypeAndRoleOfEachExistingCard_MatchTheSheet` (lit `docs/catalogue/cartes-existantes.md` par chemin relatif à `[CallerFilePath]`, compare l'archétype et le rôle de chacune des 48 lignes à la carte du même `Id`).
- [ ] **Étape 2 : lancer** `CardArchetypeTests` ; attendu : échec.
- [ ] **Étape 3 : implémenter** les types et ajouter l'archétype et le rôle aux 48 lignes de `PrototypeCollection.cs` d'après `cartes-existantes.md` (les valeurs de jeu ne changent pas).
- [ ] **Étape 4 : lancer** `CardArchetypeTests`, `CollectionRulesTests`, `CombatGoldenTests`. Attendu : vert.
- [ ] **Étape 5 : commit** `feat: archétypes et rôles des cartes (48 cartes étiquetées)`.

### Task 6: Textes générés pour conditions, modificateurs et ciblage

**Files:**
- Modify: `Presentation/Common/SkillText.cs`, `Presentation/Common/LocCombat.cs`, `Presentation/Hub/CollectionText.cs`
- Test: `Assets/Tests/Presentation/SkillTextConditionTests.cs` (créer)

**Interfaces:**
- Consomme : `EffectDef.When/Mods`, `SkillDef.Pick/PickStatus`.
- Produit : `SkillText.Effects` ajoute après la ligne d'un effet les lignes de ses modificateurs (« +40 de dégâts si la cible brûle », « +15 par statut sur la cible (max 3) ») et préfixe « Si <condition> : » les effets conditionnels ; `SkillText.Describe` ajoute le ciblage fin (« Cible : l'ennemi le plus faible », etc.). Clés françaises dans `LocCombat.cs` : `cond.<ConditionKind>`, `scale.<ScaleKind>`, `pick.<TargetPick>`, `fx.mod_flat`, `fx.mod_scaled`, `fx.conditional`.

- [ ] **Étape 1 : écrire les tests** : une phrase correcte par `ConditionKind` et par `ScaleKind` (texte exact attendu pour `TargetHasStatus Burn` et `TargetHpBelow 50`) ; `Describe_MentionsFinePick` ; `TierLine_ShowsConditionalEffect` ; `AllKeysExist_ForEveryKind` (une clé pour chaque valeur d'enum) ; un effet sans condition produit **exactement** le texte d'avant (comparer à une carte existante).
- [ ] **Étape 2 : lancer** `SkillTextConditionTests` ; attendu : échec.
- [ ] **Étape 3 : implémenter** (aucune chaîne en dur : toutes passent par `Loc.T`).
- [ ] **Étape 4 : lancer** `SkillTextConditionTests`, et les tests de texte et de grimoire existants (`CollectionPagesPresenterTests`, `CollectionBookTests`). Attendu : vert.
- [ ] **Étape 5 : commit** `feat: textes générés pour conditions, échelles et ciblage fin`.

### Task 7: Catalogue en données (`cards.json`)

**Files:**
- Create: `Authoring/CardData.cs`, `Authoring/CardDataLibrary.cs`, `Assets/Tests/Domain/CardDataTests.cs`, `Assets/_Project/Resources/Content/cards.json` (+ `.meta`), menu d'éditeur `Editor/Design/ExportCardsMenu.cs`
- Modify: `Authoring/ContentData.cs` (`EffectRecord` : champs plats `condKind, condStatus, condValue`, liste `mods` de `ModRecord{condKind, condStatus, condValue, bonus, per, maxCount}`, `SkillRecord` : `pick`, `pickStatus`), `Authoring/ContentDataConverter.cs`, `Definitions/ContentCatalog.cs` (`WithCards`), `Composition/RootLifetimeScope.cs`, `Content/Prototype/PrototypeCollection.cs`

**Interfaces:**
- Produit : `[Serializable] CardRecord { string id; Family family; int stars; CardNature nature; List<Archetype> archetypes; CardRole role; SkillRecord skill; List<TierRecord> tiers }` (`TierRecord { int level; List<EffectRecord> effects }`) ; `static class CardDataLibrary { static IReadOnlyList<SkillCardDef> Load(); static IReadOnlyList<SkillCardDef> FromJson(string json); static string ToJson(IReadOnlyList<SkillCardDef> cards) }` (`Load` lit `Resources/Content/cards.json`, repli sur `PrototypeCollection.LegacyCards()`) ; `PrototypeCollection.LegacyCards()` : les 48 cartes en code ; `PrototypeCollection.Build()` utilise `CardDataLibrary.Load()` ; `ContentCatalog.WithCards(IReadOnlyList<SkillCardDef> cards)` ; `ContentDataConverter.ToDef/FromDef` pour `SkillCardDef` conservent condition, modificateurs, `Pick`, archétypes, rôle, paliers.

- [ ] **Étape 1 : écrire les tests** : `RoundTrip_CodeCards_ThroughJson_AreEqual` (pour chacune des 48 cartes de `LegacyCards()` : `FromJson(ToJson(...))` égal champ par champ — étoiles, famille, nature, archétypes, rôle, cible, coût, effets avec condition/mods, paliers) ; `Cards_FromTheJsonFile_EqualTheCodeCards` (le fichier `cards.json` livré donne les 48 cartes, aucune de plus à ce stade) ; `JsonCatalog_PlaysTheSameCombats` (3 scénarios de `CombatGoldenTests` joués avec le catalogue issu du JSON donnent les mêmes empreintes que la référence) ; `MissingFile_FallsBackToCode`.
- [ ] **Étape 2 : lancer** `CardDataTests` ; attendu : échec.
- [ ] **Étape 3 : implémenter** les enregistrements, la bibliothèque, `WithCards`, le câblage dans `RootLifetimeScope` (le catalogue chargé reçoit les cartes du JSON) et le menu « Heroes Dawn ▸ Contenu ▸ Exporter les cartes vers cards.json » qui écrit `Resources/Content/cards.json`. Générer le fichier une fois depuis le code.
- [ ] **Étape 4 : lancer** `CardDataTests`, `DesignToolsDataTests`, `CombatGoldenTests`, `CollectionRulesTests`. Attendu : vert, référence intacte.
- [ ] **Étape 5 : commit** `feat: catalogue de cartes en données (cards.json) avec repli sur le code`.

### Task 8: Importeur des fiches Markdown

**Files:**
- Create: `Authoring/CatalogImporter.cs`, `Assets/Tests/Domain/CatalogImporterTests.cs`

**Interfaces:**
- Consomme : `CardRecord` et enregistrements (tâche 7).
- Produit : `sealed class CatalogRow { string Id, Nom, Name; int Stars; … les 16 colonnes }` ; `static class CatalogImporter { static IReadOnlyList<CatalogRow> ReadTable(string markdown); static CardRecord ToRecord(CatalogRow row); }` ; `ToRecord` lève `CatalogImportException(string cardId, string cell, string reason)` pour toute fiche de phase `P1` hors de la grammaire du plan ; les lignes de phase `P2`/`P3` ne sont pas converties (`ToRecord` lève `NotSupportedException`).

- [ ] **Étape 1 : écrire les tests** (une ligne de tableau donnée en chaîne) : une base simple (`Dmg 105, St Burn 2t`) ; un statut avec chance (`St Shock 1t (35 %)`) ; `Dmg 100 [si cible a Burn : +40]` → `Mods` avec condition et bonus 40 ; `Dmg 100, [si cible < 50% PV : Dmg +40]` → bonus de valeur (même type précédent) ; `[si cible a Vulnerable : Dmg +30]` seul dans un palier → effet supplémentaire conditionnel de 30 ; `Shield 18, St Taunt 1t [si ≥ 3 ennemis vivants : St Taunt 2t]` → effet conditionnel ; `Dmg 95 [+15 par statut de la cible (max 3)]` → échelle ; `max 40` avec `+10` → `CatalogImportException` (« max > 10 ») ; séparateur ` ; ` ; `Gauge -60` négatif ; cibles `1E:faible`, `1A:prochain`, `2E:aléatoires`, `1E:Burn` → `Target` + `Pick` ; nature, famille, archétypes (`Tempo + Brûlure` → deux), rôle ; coût 50 → coût de la compétence 50 et recharge 5 ; phase `P2` → `NotSupportedException` ; effet inconnu → `CatalogImportException`.
- [ ] **Étape 2 : lancer** `CatalogImporterTests` ; attendu : échec.
- [ ] **Étape 3 : implémenter** le lecteur de tableau (en-tête exact de la spec du catalogue) et le convertisseur selon la grammaire du plan.
- [ ] **Étape 4 : lancer** `CatalogImporterTests` ; attendu : vert.
- [ ] **Étape 5 : commit** `feat: importeur des fiches Markdown du catalogue (phase P1)`.

### Task 9: Validation du catalogue entier et première vague de 12 cartes

**Files:**
- Modify: `Assets/Tests/Domain/CatalogImporterTests.cs` (ajouts), fiches `Code/docs/catalogue/cartes-*.md` (corrections de cellules **uniquement**), `Assets/_Project/Resources/Content/cards.json`, `Presentation/Common/LocCollection.cs`, `Core/RulesVersion.cs`, tests de comptage listés plus bas
- Create: `Assets/Tests/Domain/FirstWaveTests.cs`, `Assets/Tests/Domain/Golden/combat-golden-p1.txt`

**Interfaces:**
- Consomme : `CatalogImporter`, `CardDataLibrary.ToJson`.
- La vague : `card_cinder_toss`, `card_hot_iron` (Brûlure) ; `card_static_jolt`, `card_dead_weight` (Entrave) ; `card_lynx_eye`, `card_designated_target` (Traque) ; `card_running_start`, `card_slipstream` (Tempo) ; `card_decoy`, `card_shield_bash` (Rempart) ; `card_warm_compress`, `card_emergency_balm` (Renouveau). Les 12 sont de phase `P1`, ★1.

- [ ] **Étape 1 : test de validation du catalogue** `EveryP1Row_Imports` : toutes les lignes `P1` de `cartes-*.md` (≈ 196) passent `ToRecord` sans exception ; les lignes `P2`/`P3` sont comptées et ignorées. Lancer : il échoue en listant les fiches hors grammaire.
- [ ] **Étape 2 : corriger les fiches rejetées** en ne modifiant que la cellule fautive (par exemple `(max 40)` avec `+10` devient `(max 4)`), jusqu'à zéro rejet ; relancer `verifier-catalogue.ps1` : 0 erreur.
- [ ] **Étape 3 : importer la vague** : générer les 12 enregistrements, les ajouter à `cards.json` (48 + 12 = 60), ajouter leurs 12 noms français à `LocCollection.cs` (`skills.<id>` = colonne `Nom`), passer `RulesVersion.Value` à `"0.2.0"`.
- [ ] **Étape 4 : adapter les tests de comptage** (assertions de nombre uniquement, pour 60 cartes : 15 par famille, étoiles ★1 24 / ★2 12 / ★3 12 / ★4 8 / ★5 4, natures recalculées) : `CollectionRulesTests.cs:244-257`, `CardStarsTests.cs:17-20`, `DesignToolsDataTests.cs:170-207` (`SpecNatures` : ajouter les 12), `CollectionPagesPresenterTests.cs:237-250`. Les règles de contenu (3 paliers, pas d'effet allié sur une carte ennemie, nature = nature déduite) restent inchangées et passent aussi sur les 12.
- [ ] **Étape 5 : écrire `FirstWaveTests`** : `EveryWaveCard_IsCastInAuto` ; `HotIron_DealsMoreOnABurningTarget` et `DeadWeight_…` (condition vraie/fausse, à graine fixe) ; `Slipstream_PicksTheNextAllyAndReadsHaste` ; `Decoy_GivesALongerTauntAgainstThreeEnemies` ; `EmergencyBalm_HealsMoreWhenTheAllyIsHurt` ; `WaveSheet_MatchesJson` (le JSON reflète les fiches).
- [ ] **Étape 6 : empreintes des 12 nouvelles cartes** : scénarios `wave:<id>:L1|L6|L10:s7` enregistrés dans `combat-golden-p1.txt` (fichier distinct, recorder dédié `HEROES_RECORD_GOLDEN_P1=1`) ; `combat-golden.txt` reste **intact**.
- [ ] **Étape 7 : lancer** `FirstWaveTests`, `CatalogImporterTests`, `CardDataTests`, `CombatGoldenTests`, `CollectionRulesTests`, `CardStarsTests`, `DesignToolsDataTests`, `CollectionPagesPresenterTests`. Attendu : vert.
- [ ] **Étape 8 : commit** `feat: première vague de 12 cartes à conditions (catalogue validé)`.

### Task 10: Vérification complète et documentation

**Files:**
- Create: `Code/docs/test-reports/2026-11-16_p1-conditions-archetypes.md`
- Modify: `05_Production/Decisions.md` (une ligne, **non stagée** : le fichier contient des modifications de l'utilisateur), `docs/specs/2026-11-16-capacites-composables-synergies-design.md` (P1 faite)

- [ ] **Étape 1 : EditMode complet**, puis **PlayMode complet** (`-testPlatform PlayMode`, ~7 minutes, en tâche de fond). Attendu : aucun nouvel échec par rapport à P0 ; liste des échecs hors périmètre (arbre de l'utilisateur) avec leur première ligne d'erreur ; indiquer explicitement que les mesures ont eu lieu sur un arbre non propre.
- [ ] **Étape 2 : confirmer** que `combat-golden.txt` n'a pas changé depuis la tâche 1 (`git diff --stat` vide) et noter l'état de `CombatGoldenTests` sur l'arbre de l'utilisateur.
- [ ] **Étape 3 : rapport** (totaux, durées, garde-fous, les 12 cartes, les fiches corrigées en tâche 9 avec leur nombre, **liste des cartes restantes** : 202 − 12 = 190 nouvelles cartes à mettre en jeu, dont environ 40 de phase P2 et 14 de phase P3) et ligne de `Decisions.md`.
- [ ] **Étape 4 : commit** (rapport, spec, plan ; `git add -f` pour le rapport, `test-reports` étant ignoré) : `docs: rapport de la phase P1 des capacités`.
