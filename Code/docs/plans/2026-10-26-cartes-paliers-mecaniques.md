# Cartes : 10 niveaux, paliers et 4 mécaniques — plan d'implémentation (lot C1 0.14.0)

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Faire monter les cartes jusqu'au niveau 10, leur ajouter des effets secondaires aux niveaux 3, 6 et 10, et donner au moteur de combat quatre nouveaux effets (vol de vie, purge, rebond, exécution), affichés dans le grimoire et en combat.

**Architecture:** `CardLevels` porte l'économie et `SkillAt` (base augmentée + effets des paliers atteints) ; `SkillCardDef.Tiers` porte les paliers ; les quatre mécaniques sont de nouveaux `EffectType` traités dans `CombatSimulation.ApplyEffects` ; la présentation lit trois nouveaux événements.

**Tech Stack:** Unity 6000.6.3f1, C# (nullable), NUnit, paquet domaine `com.n29.heroesdawn.domain`.

**Spec:** `Code/docs/specs/2026-10-26-cartes-paliers-mecaniques-design.md`

## Global Constraints
- Chemins relatifs à `Code/Heroes Dawn Unity/`. Domaine : `Packages/com.n29.heroesdawn.domain/`. Jeu : `Assets/_Project/`. Tests : `Assets/Tests/`.
- Valeurs : `CardLevels.Max = 10`, `PercentPerLevel = 10`, `CopiesToUpgrade(level) = (level + 1) / 2`, `CreditsToUpgrade(level) = 200 * level`, `TierLevels = { 3, 6, 10 }`, `BalanceConfig.ExecuteThresholdPercent = 30`.
- Nouveaux `EffectType`, ajoutés **à la fin** de l'enum (les entiers existants de `skills.json` ne bougent pas) : `Lifesteal`, `Purge`, `Bounce`, `Execute`.
- Statuts négatifs (purge sur allié) : Burn, Freeze, Shock, Corrosion, Blind, Vulnerable, Bind, Mark. Positifs (purge sur ennemi) : Shield, Regen, Haste, Evade, Anchor, Taunt.
- Chaque test porte `[Test, Description("Vérifie que … — parce que …")]`.
- Tests en ligne de commande, éditeur fermé (voir `Code/SETUP.md`) : `Unity.exe -batchmode -nographics -projectPath "<projet>" -runTests -testPlatform EditMode [-testFilter "<Namespace>.<Classe>"] -testResults "$TEMP/editmode.xml"` ; PlayMode sans `-nographics`.
- Commits en français, préfixe conventionnel, terminés par `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.

## Review Focus
- **Carte d'un agent dont la puissance d'objet (`SkillPower`) est active** : les effets de palier Shield/Heal passent aussi par `EmpoweredSkills` ; le résultat doit rester cohérent (ni double augmentation par le niveau, ni perte). → test Task 2.
- **Vol de vie sur une compétence de zone** : un soin par cible touchée, plafonné aux PV max du lanceur ; un lanceur K.O. entre-temps ne se soigne pas. → test Task 3.
- **Rebond qui tue la seconde cible ou la touche alors qu'elle a Esquive** : événements cohérents (raté, K.O.), pas de rebond sur une unité déjà K.O. → test Task 4.
- **Purge d'un Bouclier ennemi** : le bouclier disparaît vraiment (les dégâts suivants de la même compétence ne sont plus absorbés). → test Task 4.
- **Sauvegarde avec une carte au niveau > 10** (fichier modifié) : niveau ramené à 10, pas d'erreur. → test Task 1.

---

### Task 1: 10 niveaux et nouvelle économie

**Files:**
- Modify: `Packages/com.n29.heroesdawn.domain/Definitions/CardLevels.cs`
- Modify: `Assets/Tests/Domain/Application/CardUpgradeTests.cs` (valeurs attendues)
- Test: `Assets/Tests/Domain/Application/CardUpgradeTests.cs`

**Interfaces:**
- Produces: `CardLevels.Max = 10`, `PercentPerLevel = 10`, `CopiesToUpgrade(int)`, `CreditsToUpgrade(int)`, `EffectPercent(int)`, `static readonly int[] TierLevels = { 3, 6, 10 }`.

- [ ] **Step 1: Write the failing tests** — `CardLevels_Economy_Over10Levels` : `CopiesToUpgrade(1..9)` = 1,1,2,2,3,3,4,4,5 ; somme = 25 ; `CreditsToUpgrade(1..9)` somme = 9 000 ; `EffectPercent(1) = 100`, `(5) = 140`, `(10) = 190`. `UpgradeCard_GoesUpTo10_ThenRefuses` : avec 30 exemplaires de card_glass_wall et 10 000 Crédits, 9 améliorations réussissent, la 10e renvoie `"collection.max_level"`. `SavedLevelAbove10_IsClampedTo10` : `cardLevels` à 12 dans la sauvegarde → `CollectionOps.CardLevel == 10`. Mettre à jour les tests existants de `CardUpgradeTests` qui supposent 5 niveaux ou +15 %.
- [ ] **Step 2: Run** `-testFilter N29.HeroesDawn.Tests.Domain.Application.CardUpgradeTests` — Expected: FAIL.
- [ ] **Step 3: Implement** les constantes et formules ; mettre à jour le commentaire XML de la classe (« niveau 10 au plus, +10 % par niveau, paliers 3/6/10 »).
- [ ] **Step 4: Run** — Expected: PASS ; puis EditMode complet vert (des tests d'UI peuvent afficher « /10 »).
- [ ] **Step 5: Commit** — `feat(cartes): 10 niveaux, +10 % par niveau, nouveaux coûts d'amélioration`.

### Task 2: Paliers et compétence au niveau

**Files:**
- Modify: `Packages/com.n29.heroesdawn.domain/Definitions/CollectionDefs.cs` (`CardTier`, `SkillCardDef.Tiers`)
- Modify: `Packages/com.n29.heroesdawn.domain/Definitions/CardLevels.cs` (`SkillAt`)
- Modify: `Packages/com.n29.heroesdawn.domain/Domain/Combat/UnitFactory.cs` (`SkillsWithCards` : `SkillAt`)
- Modify: `Assets/_Project/Presentation/Hub/CollectionText.cs` (`CardLine` : `SkillAt`)
- Test: `Assets/Tests/Domain/Application/CardUpgradeTests.cs`

**Interfaces:**
- Consumes: `CardLevels.EffectPercent`, `ScalePercent` (Task 1).
- Produces: `public sealed class CardTier { public int Level { get; } public IReadOnlyList<EffectDef> Effects { get; } public CardTier(int level, params EffectDef[] effects) }` ; `SkillCardDef.Tiers` (`IReadOnlyList<CardTier>`, jamais nul) ; constructeur `SkillCardDef(..., CardNature? nature = null, IReadOnlyList<CardTier>? tiers = null)` ; `CardLevels.SkillAt(SkillCardDef card, int level) : SkillDef`.

- [ ] **Step 1: Write the failing tests** — carte de test (Damage 100 sur SingleEnemy ; paliers 3 : Burn 2 t ; 6 : Shield 10 % 2 t ; 10 : Execute 30) :
  - `SkillAt_AddsTierEffectsAtTheirLevel` : niveau 2 → 1 effet ; 3 → 2 ; 5 → 2 ; 6 → 3 ; 9 → 3 ; 10 → 4 ; l'ordre est base puis paliers.
  - `SkillAt_ScalesOnlyTheBaseEffects` : au niveau 10, Damage = 190, Shield du palier = 10 (inchangé) ; même `Id`, `Slot`, `Target`, `Cost`.
  - `CardInCombat_UsesItsLevelAndTiers` : agent avec la carte au niveau 6 en combat → `Skills[2].Effects.Count == 3`.
  - `CardWithItemSkillPower_ScalesOnce` : agent avec la carte niveau 6 et un objet `SkillPower` 15 → Damage = 150 × 115 / 100, Shield du palier = 10 × 115 / 100 (une seule augmentation par l'objet, aucune par le niveau).
- [ ] **Step 2: Run** — Expected: compilation en échec (`CardTier`).
- [ ] **Step 3: Implement** `CardTier`, `Tiers`, `SkillAt` (effets de base via `ScalePercent(skill, EffectPercent(level))`, puis `Tiers.Where(t => t.Level <= level).SelectMany(t => t.Effects)`), puis remplacer `CardLevels.Scale(card.Skill, lvl)` par `SkillAt(card, lvl)` dans `UnitFactory` et `CollectionText.CardLine`. Garder `Scale` (utilisé ailleurs).
- [ ] **Step 4: Run** `CardUpgradeTests;N29.HeroesDawn.Tests.Domain.CollectionRulesTests` — Expected: PASS.
- [ ] **Step 5: Commit** — `feat(cartes): paliers d'effets aux niveaux 3, 6 et 10`.

### Task 3: Vol de vie et exécution (moteur)

**Files:**
- Modify: `Packages/com.n29.heroesdawn.domain/Definitions/Enums.cs` (`EffectType` : `Lifesteal, Purge, Bounce, Execute` à la fin)
- Modify: `Packages/com.n29.heroesdawn.domain/Definitions/BalanceConfig.cs` (`ExecuteThresholdPercent = 30`)
- Modify: `Packages/com.n29.heroesdawn.domain/Domain/Combat/CombatEvents.cs` (`ExecutedEvent`, `BounceEvent`, `PurgedEvent`)
- Modify: `Packages/com.n29.heroesdawn.domain/Domain/Combat/CombatSimulation.Actions.cs` (`ApplyEffects`)
- Create: `Assets/Tests/Domain/CardMechanicsTests.cs`

**Interfaces:**
- Produces: `ExecutedEvent(int tick, int target)` (`Target`), `BounceEvent(int tick, int from, int to)` (`From`, `To`), `PurgedEvent(int tick, int target, int count)` (`Target`, `Count`) — même style d'une ligne que `HealEvent`.
- Test helper (dans `CardMechanicsTests`) : `CombatSimulation Cast(TargetRule target, EffectDef[] effects, string[] enemies, out List<CombatEvent> events, Action<CombatSimulation>? before = null, int targetUnit = 1)` — catalogue `TestContent.Build().WithCollection(...)` avec une carte de test Matière « card_test » (`Cost` 0) équipée en carte 1 de « verre », lancée par `CombatCommand.UseSkill(verre, 2, targetUnit)` après `before`, comme `FourthSkill_CanBeCast_AndCostsEnergy` dans `CollectionRulesTests`.

- [ ] **Step 1: Write the failing tests** (`CardMechanicsTests`, `ChancePercent` 100 partout, critique et variance neutralisés par la graine choisie ou en lisant le `DamageEvent`) :
  - `Lifesteal_HealsThePartOfDamageThatGotThrough` : cible avec Shield qui absorbe une partie ; le `HealEvent` du lanceur = `Value` % des dégâts du `DamageEvent` (pas des dégâts absorbés).
  - `Lifesteal_IsCappedByMaxHp_AndNothingWithoutDamage` : lanceur à PV max → soin 0 ; compétence sans Damage → aucun `HealEvent` sur le lanceur.
  - `Lifesteal_OnAoe_HealsOncePerTarget` : AllEnemies sur 2 ennemis → 2 `HealEvent` sur le lanceur.
  - `Execute_AppliesBelowThreshold_WhereverItIsInTheList` : cible à 29 % PV → dégâts × 1,5 (Execute 50) et `ExecutedEvent`, avec `Execute` placé avant puis après `Damage` ; cible à 31 % → dégâts normaux, aucun `ExecutedEvent`.
- [ ] **Step 2: Run** `-testFilter N29.HeroesDawn.Tests.Domain.CardMechanicsTests` — Expected: compilation en échec.
- [ ] **Step 3: Implement** — dans `ApplyEffects`, par cible : variable `dealt` (dégâts réellement retirés aux PV, renvoyés par `DealDamage` qui devient `int`) ; `Execute` lu avant la boucle (`effects.Where(Execute).Sum(Value)`) et appliqué dans le cas `Damage` si `t.Hp * 100 < t.MaxHp * ExecuteThresholdPercent` (dégâts × (100 + bonus) / 100, puis `ExecutedEvent`) ; cas `Execute` : rien ; cas `Lifesteal` : si `dealt > 0` et lanceur vivant, soin = `min(dealt * Value / 100, MaxHp - Hp)`, `HealEvent` sur le lanceur.
- [ ] **Step 4: Run** — Expected: PASS ; `CollectionRulesTests`, `TimelineCombatTests` verts.
- [ ] **Step 5: Commit** — `feat(combat): vol de vie et exécution`.

### Task 4: Purge et rebond (moteur)

**Files:**
- Modify: `Packages/com.n29.heroesdawn.domain/Domain/Combat/CombatSimulation.Actions.cs`
- Test: `Assets/Tests/Domain/CardMechanicsTests.cs`

**Interfaces:**
- Consumes: `PurgedEvent`, `BounceEvent` (Task 3), helper `Cast`.

- [ ] **Step 1: Write the failing tests**
  - `Purge_OnAlly_RemovesNegativeStatuses_NewestFirst` : allié avec Burn puis Vulnerable puis Haste ; Purge 1 → Vulnerable retiré (Burn et Haste restent) ; `StatusExpiredEvent(Vulnerable)` puis `PurgedEvent(count 1)`.
  - `Purge_Zero_RemovesAll_OnEnemyOnlyPositives` : ennemi avec Shield, Haste, Burn ; Purge 0 → Shield et Haste retirés, Burn reste ; `PurgedEvent(count 2)` ; rien à retirer → aucun `PurgedEvent`.
  - `Purge_OfAShield_LetsTheNextDamageThrough` : effets `[Purge 0, Damage 100]` sur un ennemi avec Shield → aucun `ShieldAbsorbedEvent`.
  - `Bounce_HitsAnotherLivingEnemy` : SingleEnemy sur 3 ennemis → un `BounceEvent(cible, autre)` puis un `DamageEvent` sur l'autre ; dégâts de rebond ≈ `Value` % (même graine, comparer à une compétence Damage 50 sur cette autre cible).
  - `Bounce_DoesNothing_WithoutAnotherEnemyOrWithoutDamage` : 1 seul ennemi → aucun `BounceEvent` ; compétence sans Damage → aucun `BounceEvent` ; dans les deux cas l'état du générateur est inchangé (le tour suivant donne les mêmes événements qu'une compétence sans Bounce).
  - `Bounce_NeverTargetsADefeatedEnemy_AndRespectsEvade` : l'autre ennemi a Evade → `MissEvent` après le `BounceEvent` ; un ennemi K.O. n'est jamais choisi.
  - `Simulation_WithBounce_MatchesItsVerification` : même combat en `CombatSimulation` puis rejoué par `LocalCombatVerifier` (`Assets/_Project/Infrastructure/Simulated/LocalCombatVerifier.cs`) → même résultat.
- [ ] **Step 2: Run** — Expected: FAIL.
- [ ] **Step 3: Implement** — `Purge` : liste des statuts de la cible filtrée par le côté (allié du lanceur → négatifs, sinon positifs), parcourue de la fin vers le début, `RemoveStatus` pour chacun jusqu'à `Value` (0 = tous), puis `PurgedEvent` si au moins un. `Bounce` : premier effet `Damage` de la liste ; candidats = ennemis vivants du lanceur hors `targets` ; s'il y en a et qu'il y a un `Damage`, `_rng.Range(0, candidats.Count)`, `BounceEvent`, puis le même calcul que le cas `Damage` (extraire une méthode `HitWithDamage(actor, t, power, output) : int` réutilisée par les deux cas, Esquive/Aveuglement compris) avec `power = damage.Value * Value / 100`.
- [ ] **Step 4: Run** `CardMechanicsTests` puis EditMode complet — Expected: PASS.
- [ ] **Step 5: Commit** — `feat(combat): purge et rebond`.

### Task 5: Paliers des 12 cartes

**Files:**
- Modify: `Assets/_Project/Content/Prototype/PrototypeCollection.cs` (helper `C` : `params CardTier[] tiers` ou paramètre de liste ; table de la spec)
- Test: `Assets/Tests/Domain/CollectionRulesTests.cs`

**Interfaces:**
- Consumes: `CardTier`, nouveaux `EffectType`.

- [ ] **Step 1: Write the failing tests**
  - `EveryCard_HasThreeTiers_At3_6_10` : pour chaque carte de `PrototypeCollection.Build()`, `Tiers.Select(t => t.Level)` = `[3, 6, 10]` et chaque palier a au moins un effet.
  - `EnemyCards_CarryNoAllyEffects` : pour chaque carte dont la cible est `SingleEnemy` ou `AllEnemies`, aucun effet (base ou palier) de type Heal, Shield, Advance, ni `ApplyStatus` d'un statut positif.
  - `Bounce_OnlyOnDamagingCards` : toute carte avec un effet `Bounce` a un effet `Damage` de base.
  - `TierTable_MatchesTheSpec` : quelques points de la table : card_ember niveau 6 = Bounce 50 ; card_second_wind niveau 3 = Purge 0 ; card_inferno niveau 10 = Execute 40 ; card_glass_wall niveau 6 = ApplyStatus Anchor 1 t.
- [ ] **Step 2: Run** — Expected: FAIL.
- [ ] **Step 3: Implement** la table de la spec (section « Contenu »), dans l'ordre des cartes du fichier. Chances : 100 % sauf card_prism_burst niveau 3 (Corrosion 30 %) et card_storm niveau 3 (Blind 30 %). Boucliers de palier : durée 2.
- [ ] **Step 4: Run** — Expected: PASS ; EditMode complet vert.
- [ ] **Step 5: Commit** — `feat(contenu): paliers des 12 cartes (36 effets secondaires)`.

### Task 6: Textes et grimoire

**Files:**
- Modify: `Assets/_Project/Presentation/Common/SkillText.cs` (4 cas)
- Modify: `Assets/_Project/Presentation/Common/LocCollection.cs` (ou le fichier de textes de `fx.*`) : `fx.lifesteal` = « Vol de vie {0} % », `fx.purge` = « Purge ({0}) », `fx.purge_all` = « Purge totale », `fx.bounce` = « Rebond {0} % », `fx.execute` = « Exécution +{0} % sous {1} % PV », `grimoire.tiers` = « Paliers », `grimoire.tier_line` = « Niv. {0} · {1} », `grimoire.upgrade_unlocks` = « → débloque {0} »
- Modify: `Assets/_Project/Presentation/Hub/CollectionText.cs` (`TierLines(ILocalization, SkillCardDef, int level) : string`)
- Modify: `Assets/_Project/Presentation/Hub/Pages/Grimoire/GrimoirePage.cs` (sous-titre et bouton)
- Test: `Assets/Tests/Presentation/CollectionPagesPresenterTests.cs`

**Interfaces:**
- Consumes: `SkillCardDef.Tiers`, `CardLevels.TierLevels`, `SkillAt`.
- Produces: `CollectionText.TierLines(loc, card, level)` → texte enrichi, une ligne par palier : atteint = « ● » + `grimoire.tier_line` en `Theme.Success` ; à venir = `grimoire.tier_line` en `Theme.TextMuted`.

- [ ] **Step 1: Write the failing tests**
  - `SkillText_DescribesTheFourMechanics` : `Loc.DescribeSkill` d'une compétence `[Damage 100, Lifesteal 20, Bounce 50, Execute 30, Purge 1]` contient « Vol de vie 20 % », « Rebond 50 % », « Exécution +30 % sous 30 % PV », « Purge (1) » ; `Purge 0` → « Purge totale ».
  - `Grimoire_ShowsReachedAndLockedTiers` : card_ember possédée au niveau 3 → le sous-titre contient « ● Niv. 3 » et « Niv. 6 · Rebond 50 % » sans « ● » devant.
  - `Grimoire_UpgradeButton_AnnouncesTheTierItUnlocks` : card_ember au niveau 2 avec assez de doublons et de Crédits → libellé du bouton A contient « débloque » et « Brûlure » ; au niveau 3 → ne contient pas « débloque ».
  - `Grimoire_Badge_ShowsLevelOutOf10` : badge d'une carte possédée contient « 1/10 ».
- [ ] **Step 2: Run** — Expected: FAIL.
- [ ] **Step 3: Implement** — `SkillText` : 4 cas (seuil lu dans `BalanceConfig` par défaut, 30) ; grimoire : sous-titre = pastille + ligne de carte + `"\n<color=#…>" + Loc.T("grimoire.tiers") + "</color>\n" + TierLines` ; bouton : si `TierLevels` contient `level + 1`, ajouter `" " + Loc.F("grimoire.upgrade_unlocks", <description des effets du palier>)`.
- [ ] **Step 4: Run** `CollectionPagesPresenterTests` puis EditMode complet — Expected: PASS.
- [ ] **Step 5: Commit** — `feat(ui): paliers des cartes au grimoire et textes des 4 mécaniques`.

### Task 7: Combat (présentation)

**Files:**
- Modify: `Assets/_Project/Presentation/Combat/CombatPresenter.cs` (`Play` : 3 cas ; mémoriser le dernier `SkillFxData` lancé)
- Modify: `Assets/_Project/Presentation/Common/LocCollection.cs` (ou fichier `combat.*`) : `combat.executed` = « EXÉCUTION ! », `combat.purged` = « PURGE »
- Modify: `Assets/Tests/Presentation/Fakes/FakeCombatView.cs` (`Float` enregistre `(unitId, text)` dans `public readonly List<(int Unit, string Text)> Floats`)
- Test: `Assets/Tests/Presentation/CombatPresenterTests.cs`

**Interfaces:**
- Consumes: `BounceEvent`, `ExecutedEvent`, `PurgedEvent`.

- [ ] **Step 1: Write the failing tests** — en passant les événements au présentateur par le chemin de `Play` déjà utilisé par `CombatPresenterTests` :
  - `Combat_ShowsExecuteAndPurge` : `ExecutedEvent(cible)` → `Floats` contient (cible, « EXÉCUTION ! ») ; `PurgedEvent(cible, 2)` → (cible, « PURGE »).
  - `Combat_Bounce_PlaysAnImpactOnTheSecondTarget` : après un `SkillUsedEvent` puis `BounceEvent(1, 2)` → `LastSkillFx.ActorId == 1` et `TargetIds == [2]`, même `Kind` que la compétence lancée.
- [ ] **Step 2: Run** — Expected: FAIL.
- [ ] **Step 3: Implement** — `ExecutedEvent` : `View.Float(target, _loc.T("combat.executed"), Tone.Danger, 56)` ; `PurgedEvent` : `View.Float(target, _loc.T("combat.purged"), Tone.Info, 34)` ; `BounceEvent` : `View.PlaySkillFx(dernier SkillFxData avec ActorId = From, TargetIds = { To })` et `View.Hit(To)` ; vol de vie : rien (déjà `HealEvent`).
- [ ] **Step 4: Run** `CombatPresenterTests` puis EditMode complet — Expected: PASS.
- [ ] **Step 5: Commit** — `feat(combat): affichage du rebond, de l'exécution et de la purge`.

### Task 8: Documentation et vérification finale

**Files:**
- Modify: `01_Game_Design/06_Kits_Competences.md` (section « Cartes du grimoire » : niveaux 1 à 10, coûts, paliers 3/6/10, les 4 mécaniques ; la section « Paliers de compétences » des agents ne change pas)
- Modify: `05_Production/Decisions.md` (3 lignes datées 2026-10-26, renvoi à la spec)

- [ ] **Step 1: Write the docs** — reprendre les valeurs et la table des mécaniques de la spec.
- [ ] **Step 2: Run** EditMode et PlayMode complets ; lancer aussi `N29.HeroesDawn.Tests.PlayMode.CollectionPlayTests.CaptureTheCollectionScreens` et regarder `Code/docs/test-reports/captures/18-grimoire.png` (bloc des paliers lisible dans la fiche) — Expected: 0 échec.
- [ ] **Step 3: Commit** — `docs: cartes à 10 niveaux, paliers et nouvelles mécaniques` (avec les rapports `latest-*.html`).
