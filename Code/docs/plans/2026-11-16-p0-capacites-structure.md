# P0 — Structure des capacités et garde-fou de combat : plan d'implémentation

> **Pour les agents d'exécution :** utiliser superpowers:subagent-driven-development (recommandé) ou superpowers:executing-plans, tâche par tâche. Les étapes utilisent des cases `- [ ]`.

**Objectif :** introduire la structure `Ability` dans `SkillDef` et migrer les 48 cartes, les agents, les ennemis et les boss, **sans aucun changement de comportement**, prouvé par des combats de référence figés avant la migration.

**Architecture :** `SkillDef` porte une liste d'`Ability` (déclencheur `OnCast` seul pour l'instant). `SkillDef.Effects` devient une vue en lecture seule : la concaténation, dans l'ordre, des effets des capacités `OnCast`. La simulation, l'IA, les textes et les outils continuent de lire `Effects` ; la boucle `ApplyEffects` n'est pas modifiée. Niveaux, paliers et bonus passent par deux méthodes de `SkillDef` qui préservent la structure.

**Stack :** C# (Unity 6000.6.3f1), NUnit EditMode. Domaine : `Packages/com.n29.heroesdawn.domain`. Tests : `Assets/Tests/Domain`.

**Spec :** `docs/specs/2026-11-16-capacites-composables-synergies-design.md` (phase P0 et « Précisions issues de la relecture »).

## Contraintes globales
- Aucun changement de règle ni de contenu : `RulesVersion.Value` reste `"0.1.0"`.
- Journaux d'événements **et** état final du générateur identiques avant/après, sur tout le catalogue.
- Déterminisme : aucun nouveau tirage, aucun changement d'ordre de tirage ; la boucle `ApplyEffects` reste cible par cible, effet par effet.
- Les 11 types d'effet et leurs valeurs sont inchangés ; `EffectDef` n'est pas modifié.
- Le domaine n'utilise pas `UnityEngine`. Commentaires et `Description` des tests en français, au format existant : `[Test, Description("Vérifie que … — parce que …")]`.
- Les tests existants (1 511 en EditMode au 2026-11-12) restent verts, sans modification de leurs assertions.
- Lancer Unity en batch avec l'éditeur **fermé** : `UNITY="/c/Program Files/Unity/Hub/Editor/6000.6.3f1/Editor/Unity.exe"`, `PROJ="E:/Projets/HeroesDawn/principal/Code/Heroes Dawn Unity"`, puis `"$UNITY" -batchmode -nographics -projectPath "$PROJ" -runTests -testPlatform EditMode -testFilter "<filtre>" -testResults "<xml>" -logFile "<log>"`.
- Git : la racine du dépôt est `E:/Projets/HeroesDawn/principal`. Si git refuse (« dubious ownership »), s'arrêter et demander à l'utilisateur ; ne pas modifier la configuration git.

## Points d'attention de la relecture (Review Focus)
1. Effets à chance < 100 % (ordre des tirages) → couvert par le garde-fou de la tâche 1 sur les 48 cartes.
2. Boss qui changent de kit en changeant de phase (`BossPhaseDef.Skills`, `SetKit`) → scénarios de boss avec des alliés assez forts pour franchir les seuils (tâche 1) et invariant de la tâche 4.
3. Carte au niveau 10 avec ses 3 paliers : ordre base puis paliers, paliers non augmentés → tâche 1 (niveaux 1, 3, 6, 10) et tâche 3.
4. Puissance de compétence des objets (`ItemPerk.SkillPower`) passant par `ScalePercent` → un scénario de la tâche 1 équipe un tel objet.
5. Compétence sans aucun effet (liste vide) : `BaseEffect` nul, catégorie Dégâts → tâche 2.

## Fichiers
| Fichier | Rôle |
|---|---|
| `Definitions/Ability.cs` (créer) | `AbilityTrigger`, `Ability` |
| `Definitions/SkillDef.cs` (modifier) | `Abilities`, `Effects` en vue, `EffectsOf`, `MapEffects`, `WithAbilities`, `FromAbilities` |
| `Definitions/CardLevels.cs`, `CardBonuses.cs`, `CollectionDefs.cs` (modifier) | niveaux, bonus et paliers via `SkillDef` ; `CardTier.Abilities` |
| `Domain/Combat/CombatSimulation.Actions.cs` (modifier) | `PerformSkill` lit `EffectsOf(OnCast)` ; `RngProbeForTests` |
| `Assets/Tests/Domain/Golden/CombatDigest.cs`, `CombatGoldenTests.cs`, `combat-golden.txt` (créer) | garde-fou |
| `Assets/Tests/Domain/AbilityTests.cs`, `AbilityMigrationTests.cs` (créer) | structure et invariants |

---

### Task 1: Garde-fou — combats de référence figés **avant** toute migration

**Fichiers :**
- Modifier : `Domain/Combat/CombatSimulation.Actions.cs` (ajouter `RngProbeForTests`)
- Créer : `Assets/Tests/Domain/Golden/CombatDigest.cs`, `Assets/Tests/Domain/Golden/CombatGoldenTests.cs`, `Assets/Tests/Domain/Golden/combat-golden.txt`

**Interfaces :**
- Produit : `public uint CombatSimulation.RngProbeForTests()` (tire une valeur du générateur ; à n'appeler qu'en fin de combat) ; `static string CombatDigest.Of(CombatSimulation sim)` (16 caractères hexadécimaux) ; `static IEnumerable<(string Key, Func<CombatSimulation> Build)> CombatGoldenTests.Scenarios()`.

- [ ] **Étape 1 : écrire `CombatDigest.Of`.** Empreinte `Fnv1a64` (Core) d'un texte stable : pour chaque événement de `sim.Log`, le nom du type puis chaque propriété publique dans l'ordre de déclaration (tableaux joints par `,`) ; puis, pour chaque unité, `Id:Hp:Energy` ; puis `RngProbeForTests()`. Lire l'API de `Fnv1a64` avant de l'utiliser.
- [ ] **Étape 2 : écrire `RngProbeForTests`** : `public uint RngProbeForTests() => _rng.NextUInt();` à côté de `ApplyDamageForTests`.
- [ ] **Étape 3 : écrire les scénarios** (tous en mode Auto, jusqu'à `IsOver` ou 3 000 ticks, `TestContent.Build()`), clés stables :
  - `card:<id>:L<1|3|6|10>:s7` pour chacune des cartes de `catalog.Collection.Cards` : allié `verre` niveau 5 équipé de la carte au niveau voulu (voir `AllyEntry.CardLevels` et `CardMechanicsTests.Cast` pour la construction), contre deux ennemis `brute`.
  - `item-skillpower:<id>:L6:s7` : une seule carte, avec un objet à `ItemPerk.SkillPower` équipé (le chercher dans `catalog.Collection.Items`).
  - `enemy:<id>:s1` et `enemy:<id>:s2` pour chaque `catalog.Enemies` (agents du `StarterTeam` niveau 40, pour franchir les phases des boss).
  - `duel:starter:s1` : `CombatSetup` avec `OpponentTeam` (chemin `UnitFactory.Opponent`).
- [ ] **Étape 4 : écrire les tests.** `Golden_MatchesTheRecordedDigests` : pour chaque scénario, `CombatDigest.Of` égal à la ligne `clé=empreinte` de `combat-golden.txt`, et échec explicite si une clé manque. `Digest_IsStableAcrossTwoRuns` : même scénario construit deux fois → même empreinte. `[Test, Explicit] RecordGolden` : écrit `combat-golden.txt` (clés triées).
- [ ] **Étape 5 : enregistrer la référence.** Lancer `RecordGolden` seul : `-testFilter "N29.HeroesDawn.Tests.Domain.CombatGoldenTests.RecordGolden"`. Attendu : fichier créé, au moins 200 lignes (192 pour les cartes, plus les objets, ennemis et duel).
- [ ] **Étape 6 : vérifier.** Lancer `CombatGoldenTests` en entier. Attendu : `Golden_MatchesTheRecordedDigests` et `Digest_IsStableAcrossTwoRuns` réussis. Relancer une seconde fois : même résultat.
- [ ] **Étape 7 : commit séparé** (référence figée, aucun changement de règle) : `git add` des 4 fichiers de cette tâche ; message `test: figer les combats de référence avant la migration des capacités`.

---

### Task 2: `Ability` et `SkillDef`

**Fichiers :**
- Créer : `Definitions/Ability.cs`, `Assets/Tests/Domain/AbilityTests.cs`
- Modifier : `Definitions/SkillDef.cs`

**Interfaces :**
- Produit (`namespace N29.HeroesDawn.Definitions`) :
  - `enum AbilityTrigger { OnCast }`
  - `sealed class Ability { AbilityTrigger Trigger { get; } IReadOnlyList<EffectDef> Effects { get; } Ability(AbilityTrigger trigger, IReadOnlyList<EffectDef> effects) }`
  - `SkillDef.Abilities : IReadOnlyList<Ability>` ; `SkillDef.Effects` : vue (concaténation ordonnée des effets des capacités `OnCast`) ; constructeur existant inchangé (une seule capacité `OnCast`) ; `static SkillDef FromAbilities(SkillId id, SkillSlot slot, int cooldown, TargetRule target, IReadOnlyList<Ability> abilities, int cost = -1)` ; `IReadOnlyList<EffectDef> EffectsOf(AbilityTrigger trigger)` ; `SkillDef MapEffects(Func<EffectDef, EffectDef> f)` (même structure, effets transformés, mêmes id, slot, recharge, cible, coût) ; `SkillDef WithAbilities(IEnumerable<Ability> extra)` (capacités ajoutées après les existantes).

- [ ] **Étape 1 : écrire les tests** dans `AbilityTests`, avec des `EffectDef` de test :
  - `FromEffects_WrapsInOneOnCastAbility` : `Abilities.Count == 1`, `Abilities[0].Trigger == OnCast`, `Effects` identique à la liste fournie.
  - `Effects_IsTheOrderedConcatenationOfOnCastAbilities` : deux capacités `OnCast` [A,B] et [C] → `Effects` = A, B, C.
  - `MapEffects_KeepsStructureOrderAndIdentity` : nombre de capacités et ordre conservés, `Id`, `Slot`, `Cooldown`, `Target`, `Cost` égaux.
  - `WithAbilities_AppendsAfterExistingOnes`.
  - `EmptySkill_HasNullBaseEffect_AndDamageCategory` : liste d'effets vide → `BaseEffect == null`, `Category == SkillCategory.Damage`, `Abilities.Count == 1`.
  - `CostAndBaseEffect_AreUnchanged` : `Cost == cooldown * 10` sans coût explicite ; `BaseEffect` = premier effet.
- [ ] **Étape 2 : lancer** `-testFilter "N29.HeroesDawn.Tests.Domain.AbilityTests"`. Attendu : échec de compilation (types absents).
- [ ] **Étape 3 : implémenter** `Ability.cs` et les membres de `SkillDef` ci-dessus. `Effects` est calculée une fois à la construction (liste en lecture seule) pour ne rien allouer à chaque lecture.
- [ ] **Étape 4 : lancer** `AbilityTests`. Attendu : tous réussis.
- [ ] **Étape 5 : lancer le garde-fou** `CombatGoldenTests`. Attendu : réussi (rien n'a encore changé de comportement).
- [ ] **Étape 6 : commit** `feat: Ability et SkillDef.Abilities (vue Effects inchangée)`.

---

### Task 3: Niveaux, bonus, paliers et simulation passent par les capacités

**Fichiers :**
- Modifier : `Definitions/CardLevels.cs` (`ScalePercent`, `SkillAt`), `Definitions/CardBonuses.cs` (`Apply`), `Definitions/CollectionDefs.cs` (`CardTier`), `Domain/Combat/CombatSimulation.Actions.cs` (`PerformSkill`)
- Test : `Assets/Tests/Domain/AbilityTests.cs` (ajouts)

**Interfaces :**
- Consomme : `SkillDef.MapEffects`, `SkillDef.WithAbilities`, `SkillDef.EffectsOf`, `Ability` (tâche 2).
- Produit : `CardTier.Abilities : IReadOnlyList<Ability>` (une capacité `OnCast` construite à partir de `Effects` ; `CardTier.Effects` et le constructeur restent inchangés).

- [ ] **Étape 1 : écrire les tests** :
  - `SkillAt_AddsOneAbilityPerReachedTier` : `card_ember` au niveau 6 → `Abilities.Count == 3` ; au niveau 2 → 1 ; au niveau 10 → 4 ; `Effects` = effets de base mis à l'échelle puis effets des paliers non augmentés, dans l'ordre.
  - `CardBonusesApply_PreservesAbilityCount_AndScalesEffects` : même nombre de capacités ; valeurs identiques à celles de `CardBonusTests`.
  - `ScalePercent_PreservesAbilityStructure`.
- [ ] **Étape 2 : lancer** les nouveaux tests. Attendu : échec (comptes de capacités).
- [ ] **Étape 3 : réécrire** `CardLevels.ScalePercent` et `CardBonuses.Apply` avec `MapEffects` (mêmes formules et arrondis qu'aujourd'hui, mot pour mot) ; `CardLevels.SkillAt` avec `Scale(...)` puis `WithAbilities(tier.Abilities)` pour chaque palier atteint ; `CardTier.Abilities` ; `PerformSkill` appelle `ApplyEffects(actor, skill.EffectsOf(AbilityTrigger.OnCast), …)`.
- [ ] **Étape 4 : lancer** `AbilityTests`, puis `CardBonusTests`, `CardMechanicsTests`, `CollectionRulesTests`, `CardUpgradeTests`. Attendu : tous réussis, sans modification de leurs assertions.
- [ ] **Étape 5 : lancer le garde-fou** `CombatGoldenTests`. Attendu : réussi. Un écart = régression : corriger le code, **jamais** la référence.
- [ ] **Étape 6 : commit** `refactor: niveaux, bonus et paliers via les capacités (comportement identique)`.

---

### Task 4: Invariants du catalogue et outils

**Fichiers :**
- Créer : `Assets/Tests/Domain/AbilityMigrationTests.cs`

**Interfaces :**
- Consomme : `ContentCatalog.Members`, `ContentCatalog.Enemies`, `CollectionDef.Cards`, `BossPhaseDef.Skills`, `ContentDataConverter.ToDef/FromDef` (`Assets/_Project/Authoring`).

- [ ] **Étape 1 : écrire les tests** :
  - `EverySkillOfTheCatalog_HasASingleOnCastAbility` : agents, ennemis, phases de boss, cartes de base ; `Abilities.Count == 1`, déclencheur `OnCast`, `Effects` égal aux effets de cette capacité.
  - `EveryTierOfEveryCard_HasASingleOnCastAbility`.
  - `ContentDataConverter_RoundTrip_KeepsEffects` : pour chaque compétence du catalogue, `ToDef(FromDef(s))` a les mêmes `Effects` (type, valeur, statut, durée, chance) et la même structure.
  - `ContentCsvExport_StillReadsEffects` : l'export CSV d'une carte contient ses effets (adapter au test existant `DesignToolsDataTests` si l'accès est déjà couvert ; sinon appeler la fonction d'export de la ligne de compétence).
- [ ] **Étape 2 : lancer** `AbilityMigrationTests`. Attendu : réussis (la structure est déjà en place) ; si un chemin de construction hors de `SkillDef` contourne le constructeur, corriger ce chemin.
- [ ] **Étape 3 : lancer** `DesignToolsDataTests` et `BalanceImportTests`. Attendu : réussis.
- [ ] **Étape 4 : commit** `test: invariants de migration des capacités sur tout le catalogue`.

---

### Task 5: Vérification complète et documentation

**Fichiers :**
- Créer : `docs/test-reports/2026-11-16_p0-capacites.md`
- Modifier : `principal/05_Production/Decisions.md` (une ligne), `docs/specs/2026-11-16-capacites-composables-synergies-design.md` (marquer P0 faite)

- [ ] **Étape 1 : EditMode complet** (`-testPlatform EditMode`, sans filtre). Attendu : 0 échec ; total = 1 511 + les nouveaux tests ; mêmes 8 tests ignorés qu'au 2026-11-12.
- [ ] **Étape 2 : PlayMode complet** (`-testPlatform PlayMode`). Attendu : 0 échec, mêmes 25 ignorés, `FullRunTests` réussi.
- [ ] **Étape 3 : confirmer** que `combat-golden.txt` n'a pas changé depuis le commit de la tâche 1 (`git diff --stat` sur ce fichier : vide) et que `RulesVersion.Value` vaut toujours `"0.1.0"`.
- [ ] **Étape 4 : écrire le rapport** (nombres d'exécution, durée, confirmation du garde-fou) et la ligne de `Decisions.md` : « Capacités : `SkillDef.Abilities`, `Effects` en vue ; P0 sans changement de comportement ; référence de combat figée ».
- [ ] **Étape 5 : commit** `docs: rapport de la phase P0 des capacités`.
