# Labo : fabrication d'objets (lot E) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fabriquer des objets au labo avec des scraps et des recettes obtenus en combat, avec des stats tirées au hasard et une qualité qui dépend d'une compétence de création par type d'objet.

**Architecture:** Règles et recettes dans le domaine (tables en code, tirages par `IRandom`). Les objets fabriqués sont des `ItemDef` uniques (`crafted_N`) enregistrés dans le `CollectionDef` de la session (catalogue séparé : ils n'entrent ni dans la boutique ni dans les drops). Persistance dans `MetaSave`, service `Workshop` à côté de `ResearchLab`, rangées « Atelier » ajoutées à la page du labo.

**Tech Stack:** Unity 6000.6.3f1, C#, NUnit, VContainer, MessagePipe, JsonUtility.

**Spec:** `Code/docs/specs/2026-10-27-labo-fabrication-design.md`

## Global Constraints

- Code dans `Code/Heroes Dawn Unity` ; domaine dans `Packages/com.n29.heroesdawn.domain` (namespaces `N29.HeroesDawn.Definitions`, `N29.HeroesDawn.Domain.Board`), application dans `Assets/_Project/Application`, présentation dans `Assets/_Project/Presentation`, tests dans `Assets/Tests`. `#nullable enable` en tête des fichiers.
- Textes joueur en français dans `Loc` ; commentaires et descriptions de tests en français (chaque `[Test]` a un `Description("Vérifie que … — parce que …")`).
- Aucun `Random` ni horloge dans le domaine : tout tirage passe par `IRandom` (`Pcg32`, `CollectionRules.Seed(playerId, what, counter)`).
- La boutique du jour, les drops d'objets et `CollectionDef.Items` (catalogue) ne changent pas.
- Une ancienne sauvegarde (champs absents) se charge sans erreur.
- Tests : `ut.sh` du dossier `.superpowers/sdd/2026-10-27-labo-fabrication/` (`ut.sh EditMode '<filtre>'`), éditeur fermé.

## Review Focus

- Un objet fabriqué équipé survit à la sauvegarde / au rechargement du profil (`ApplyProfile`) → test Task 4.
- Un identifiant `crafted_N` resté dans le catalogue d'un profil précédent ne s'équipe pas dans un autre profil → test Task 4.
- Scraps insuffisants ou recette inconnue : rien n'est dépensé, aucun objet créé → test Task 5.
- Boutique du jour et drops d'objets n'offrent jamais d'objet fabriqué → test Task 3.
- Compétence au maximum (niveau 10) : l'expérience ne dépasse pas le palier et rien ne plante → test Task 2.

---

### Task 1: Types et catalogue des recettes (domaine)

**Files:**
- Create: `Packages/com.n29.heroesdawn.domain/Definitions/Crafting.cs`
- Test: `Assets/Tests/Domain/CraftingDefsTests.cs`

**Interfaces:**
- Produces (namespace `N29.HeroesDawn.Definitions`) :
  - `enum ScrapKind { Weapon, Armor, Tool, Implant, Core }`
  - `static class ScrapKinds { const int Count = 5; static ScrapKind For(ItemType type); }` (`Weapon→Weapon` … `Implant→Implant`)
  - `enum CraftQuality { Rough, Standard, Fine, Superior, Masterwork }` et `static class CraftQualities { static int Percent(CraftQuality q); }` (80, 100, 115, 130, 150)
  - `readonly struct RecipeStat { RecipeStatKind Stat; int Min; int Max; }` avec `enum RecipeStatKind { Hp, Atk, Def, Spd }`
  - `sealed class RecipeDef { string Id; string NameKey; ItemType Type; ItemRarity Rarity; int Difficulty; int ScrapCost; int CoreCost; IReadOnlyList<RecipeStat> Stats; ItemPerk Perk; int PerkValue; }`
  - `static class Recipes { static IReadOnlyList<RecipeDef> All; static IReadOnlyList<string> Starter; static RecipeDef? Find(string id); }`

- [ ] **Step 1: Write the failing tests** (`CraftingDefsTests`) :
  - `Catalog_Has12Recipes_3PerType_OneOfEachRarity` : 12 recettes ; pour chaque `ItemType`, une Common (difficulté 1, coût 6 + 0 Noyau), une Rare (4, 12 + 2), une Epic (7, 20 + 5) ; identifiants uniques.
  - `StatRanges_MatchTheSpec` : Common = 1 stat 8 à 12 ; Rare = 2 stats 8 à 14 ; Epic = 2 stats 10 à 16 et un `Perk != None` avec `PerkValue > 0` ; les Common et Rare n'ont pas d'effet spécial.
  - `Starter_IsTheFourCommonRecipes` : `Recipes.Starter` = les 4 Common, une par type.
  - `QualityPercents` : 80, 100, 115, 130, 150 ; `ScrapKinds.For` renvoie le scrap de même nom pour chaque `ItemType`.
- [ ] **Step 2: Run** `ut.sh EditMode N29.HeroesDawn.Tests.Domain.CraftingDefsTests` — Expected: FAIL (compilation : types absents).
- [ ] **Step 3: Implement** `Crafting.cs`. Identifiants et noms (clé de texte `craft.recipe.<id>`) des 12 recettes, `stats` entre parenthèses, perk pour les Epic :
  - Weapon : `scrap_blade` « Lame de fortune » C (ATQ) · `arc_baton` « Matraque à arc » R (ATQ, VIT) · `pulse_rifle` « Fusil à impulsion » E (ATQ, VIT ; `CritChance` 8)
  - Armor : `patch_vest` « Gilet rapiécé » C (PV) · `plated_coat` « Manteau blindé » R (PV, DEF) · `aegis_plate` « Plastron égide » E (PV, DEF ; `OpeningShield` 10)
  - Tool : `spring_boots` « Bottes à ressorts » C (VIT) · `grapple_rig` « Harnais grappin » R (VIT, DEF) · `chrono_pack` « Sac chrono » E (VIT, PV ; `StartGauge` 15)
  - Implant : `neural_chip` « Puce neurale » C (ATQ) · `synapse_coil` « Bobine synaptique » R (ATQ, VIT) · `apex_core` « Noyau d'Apex » E (ATQ, PV ; `SkillPower` 10)
  Coûts et fourchettes : voir le tableau de la spec §1 ; `ScrapCost` = scraps du type de la recette, `CoreCost` = Noyaux.
- [ ] **Step 4: Run** le même filtre — Expected: PASS.
- [ ] **Step 5: Commit** — `feat(labo): types de scraps, qualités et 12 recettes de fabrication`.

### Task 2: Règles de tirage et de compétence (domaine)

**Files:**
- Create: `Packages/com.n29.heroesdawn.domain/Domain/Board/CraftingRules.cs`
- Test: `Assets/Tests/Domain/CraftingRulesTests.cs`

**Interfaces:**
- Consumes: Task 1 (`RecipeDef`, `CraftQuality`, `RecipeStatKind`, `CraftQualities.Percent`).
- Produces (namespace `N29.HeroesDawn.Domain.Board`) :
  - `readonly struct CraftedStats { int Hp, Atk, Def, Spd; }`
  - `static class CraftingRules` :
    - `const int MaxSkill = 10;`
    - `static IReadOnlyList<int> QualityOdds(int skill, int difficulty)` → 5 pourcentages (somme 100), ordre `CraftQuality`
    - `static CraftQuality RollQuality(IRandom rng, int skill, int difficulty)`
    - `static CraftedStats RollStats(IRandom rng, RecipeDef recipe, CraftQuality quality)`
    - `static int SkillLevel(int xp)` (1 à 10) ; `static int XpForLevel(int level)` (= `20 × level × (level − 1)`, expérience totale pour atteindre ce niveau) ; `static int XpGain(int skill, int difficulty)`

- [ ] **Step 1: Write the failing tests** (`CraftingRulesTests`) :
  - `Odds_SumTo100_ForEveryEdge` : pour tout `skill` de 1 à 10 et `difficulty` de 1 à 10, somme = 100.
  - `Odds_ReadFromTheSpecTable` : `QualityOdds(1, 7)` (edge −6) = {60,35,5,0,0} ; `(4, 4)` (edge 0) = {15,50,25,9,1} ; `(5, 4)` (edge +1) = {5,35,35,20,5} ; `(9, 4)` (edge +5) = {0,5,30,40,25} ; `(3, 5)` (edge −2) = {35,50,13,2,0} ; `(8, 4)` (edge +4) = {0,15,40,30,15}.
  - `Odds_NeverHurtHigherTiers_AsSkillRises` : pour une difficulté fixe, le cumul des paliers ≥ Fine ne baisse jamais quand la compétence monte.
  - `RollQuality_FollowsTheOdds` : 4000 tirages `Pcg32` à edge 0 : chaque palier à ± 3 points de sa probabilité ; le Chef-d'œuvre apparaît au moins une fois ; à edge −3 jamais de Supérieur ni de Chef-d'œuvre.
  - `RollStats_StayInRangeTimesQuality` : pour chaque recette et chaque qualité, 200 tirages : chaque stat de la recette ∈ [round(min × p/100), round(max × p/100)] et ≥ 1 ; les stats absentes de la recette valent 0.
  - `Skill_XpThresholds` : `SkillLevel(0)=1`, `SkillLevel(39)=1`, `SkillLevel(40)=2` (20×2×1), `SkillLevel(120)=3` (20×3×2), `SkillLevel(1_000_000)=10`, `XpForLevel(10)=1800`.
  - `XpGain_FullThenFlat` : `XpGain(skill 1, difficulty 7)` = 38 ; `XpGain(skill 5, difficulty 1)` (edge +4) = 5 ; `XpGain(skill 3, difficulty 1)` (edge +2) = 14.
- [ ] **Step 2: Run** `CraftingRulesTests` — Expected: FAIL.
- [ ] **Step 3: Implement** `CraftingRules`. `QualityOdds` : `edge = skill − difficulty` ramené par `Mathf`-free clamp dans [−3, +5] aux bornes de la table de la spec (−3 et moins ; −2 et −1 ; 0 ; +1 et +2 ; +3 et +4 ; +5 et plus) ; `RollQuality` : un `rng.Range(0, 100)` comparé aux cumuls ; `RollStats` : pour chaque `RecipeStat` un `rng.Range(Min, Max + 1)`, multiplié par `Percent / 100` avec arrondi `(v * p + 50) / 100`, plancher 1 ; `XpGain = edge <= 2 ? 10 + 4 × difficulty : 5`.
- [ ] **Step 4: Run** — Expected: PASS.
- [ ] **Step 5: Commit** — `feat(labo): tirage de la qualité, des stats et progression de la compétence`.

### Task 3: Catalogue des objets fabriqués dans `CollectionDef`

**Files:**
- Modify: `Packages/com.n29.heroesdawn.domain/Definitions/CollectionDefs.cs` (classe `CollectionDef`)
- Modify: `Assets/_Project/Presentation/Hub/Pages/Inventory/InventoryPage.cs`, `Loadout/LoadoutPage.cs:118`, `MemberSheet/MemberSheetPresenter.cs:407` (listes d'objets à équiper ou possédés)
- Create: `Packages/com.n29.heroesdawn.domain/Definitions/CraftedItems.cs`
- Test: `Assets/Tests/Domain/CraftedCatalogTests.cs`

**Interfaces:**
- Consumes: Task 1.
- Produces :
  - `CollectionDef.Register(ItemDef item)` (ajoute ou remplace ; lève `InvalidOperationException` sur `CollectionDef.Empty`), `CollectionDef.ClearCrafted()`, `IReadOnlyList<ItemDef> CollectionDef.Crafted`, `IReadOnlyList<ItemDef> CollectionDef.AllItems` (catalogue puis objets fabriqués). `HasItem` / `Item` résolvent aussi les objets fabriqués ; `Items`, `RollItem`, `DailyItemOffers` ne les voient jamais.
  - `static class CraftedItems` : `const string IdPrefix = "crafted_"`; `static string NameKey(string recipeId, CraftQuality q)` = `craft.item.<recipeId>.<q en minuscules>`; `static bool TryParseNameKey(string nameKey, out string recipeId, out CraftQuality q)`; `static ItemDef ToDef(string id, RecipeDef recipe, CraftQuality q, CraftedStats stats)` (rareté et type de la recette, `Price` 0, perk et `PerkValue` de la recette).

- [ ] **Step 1: Write the failing tests** (`CraftedCatalogTests`) :
  - `Registered_ItemIsFound_ButNotInTheCatalogue` : après `Register(def)`, `HasItem`/`Item` le trouvent, `AllItems` le contient, `Items` non.
  - `Register_RefusedOnEmpty` : `CollectionDef.Empty.Register(...)` lève `InvalidOperationException`.
  - `ClearCrafted_RemovesThem` : après `ClearCrafted`, `HasItem` est faux.
  - `Register_SameIdReplaces` : deux `Register` du même id gardent une seule entrée, la seconde l'emporte.
  - `ShopAndDrops_NeverOfferCrafted` : (Review Focus) avec 5 objets fabriqués enregistrés, 300 `RollItem` et toutes les `DailyItemOffers` de 30 jours ne renvoient aucun id commençant par `crafted_`.
  - `ToDef_CopiesRecipeAndRolledStats` : type, rareté, perk, stats ; `NameKey` aller-retour avec `TryParseNameKey`.
- [ ] **Step 2: Run** `CraftedCatalogTests` — Expected: FAIL.
- [ ] **Step 3: Implement** : `CollectionDef` garde un second dictionnaire et une liste `_crafted` ; `Register` / `ClearCrafted` / `Crafted` / `AllItems` comme ci-dessus. Remplacer `Items` par `AllItems` dans les trois listes de présentation citées (inventaire des objets possédés, objets proposables à l'équipement) ; le compteur « possédés / total » de l'inventaire continue d'utiliser `Items` (catalogue).
- [ ] **Step 4: Run** le filtre + `N29.HeroesDawn.Tests.Presentation` (inventaire, équipement) — Expected: PASS.
- [ ] **Step 5: Commit** — `feat(labo): catalogue des objets fabriqués dans CollectionDef`.

### Task 4: Sauvegarde et inventaire de fabrication (application)

**Files:**
- Modify: `Assets/_Project/Application/Ports/MetaSave.cs`
- Create: `Assets/_Project/Application/Features/Meta/CraftingOps.cs`
- Modify: `Assets/_Project/Application/Session/GameSession.cs` (`ApplyProfile`, avant `LoadEquipment`)
- Test: `Assets/Tests/Domain/Application/CraftingOpsTests.cs`

**Interfaces:**
- Consumes: Tasks 1 à 3.
- Produces :
  - `[Serializable] class CraftedItemSave { string id = ""; string recipeId = ""; int quality; int hp, atk, def, spd; }`
  - `MetaSave` : `List<CounterSave> scraps` (id = nom de `ScrapKind`), `List<string> recipes`, `List<CounterSave> craftXp` (id = nom de `ItemType`), `List<CraftedItemSave> crafted`, `int craftedNext`, `int scrapDrops`.
  - `static class CraftingOps` (namespace `N29.HeroesDawn.Application.Features.Meta`) :
    - `int Scraps(GameSession s, ScrapKind k)`, `void AddScraps(GameSession s, ScrapKind k, int n)`, `bool TrySpendScraps(GameSession s, ScrapKind k, int n)`
    - `bool Knows(GameSession s, string recipeId)` (les 4 recettes de départ sont toujours connues : l'ajout se fait à la lecture, sans écrire dans la sauvegarde), `void Learn(GameSession s, string recipeId)`
    - `int Xp(GameSession s, ItemType t)`, `void AddXp(GameSession s, ItemType t, int xp)`, `int Skill(GameSession s, ItemType t)`
    - `ItemDef AddCrafted(GameSession s, RecipeDef recipe, CraftQuality q, CraftedStats stats)` : numérote `crafted_<craftedNext+1>`, enregistre dans `Content.Collection`, `CollectionOps.AddItem`, renvoie la définition
    - `void RegisterSaved(GameSession s)` : `ClearCrafted()` puis réenregistre chaque `CraftedItemSave` (recette inconnue ignorée)

- [ ] **Step 1: Write the failing tests** (`CraftingOpsTests`, avec `TestApp` + `SignInDirect`) :
  - `StarterRecipes_AreKnown_OthersAreNot` : les 4 de départ `Knows`, `pulse_rifle` non ; après `Learn` oui, sans doublon si appelé deux fois.
  - `Scraps_AddSpendAndRefuse` : ajouter 6, dépenser 6 OK ; dépenser 1 de plus refusé sans changer le compteur.
  - `Xp_RaisesTheSkill` : `Skill` = 1 puis 2 après `AddXp(t, 40)`.
  - `AddCrafted_NumbersAndStoresTheItem` : deux appels donnent `crafted_1`, `crafted_2` ; `CollectionOps.OwnedItems` = 1 chacun ; `Content.Collection.HasItem` vrai.
  - `CraftedItem_EquippedSurvivesSaveAndReload` : (Review Focus) fabriquer, équiper sur un agent via `Collection.EquipItem`, sérialiser le profil (`BuildProfile` puis `JsonUtility` aller-retour), `ApplyProfile` dans une session neuve : l'agent porte toujours `crafted_1` avec les mêmes stats.
  - `StaleCraftedId_DoesNotEquipInAnotherProfile` : (Review Focus) profil A avec `crafted_1`, charger le profil B dont un agent liste `crafted_1` sans l'avoir dans `crafted` : l'objet n'est pas équipé.
  - `OldSave_WithoutCraftingFields_Loads` : un `MetaSave` créé sans ces champs (valeurs par défaut) se charge, `Knows` des 4 recettes de départ, aucun objet fabriqué.
- [ ] **Step 2: Run** `CraftingOpsTests` — Expected: FAIL.
- [ ] **Step 3: Implement** `CraftingOps` (compteurs sur le modèle de `CollectionOps`, mêmes `Get`/`Add` privés dupliqués en deux lignes) et les champs de `MetaSave` en fin de classe, avec valeurs par défaut vides. Dans `ApplyProfile`, appeler `CraftingOps.RegisterSaved(this)` juste après l'affectation de `Meta` et avant la boucle d'équipement.
- [ ] **Step 4: Run** `CraftingOpsTests;CollectionBookTests` — Expected: PASS.
- [ ] **Step 5: Commit** — `feat(labo): sauvegarde des scraps, recettes, compétences et objets fabriqués`.

### Task 5: Service `Workshop` (application)

**Files:**
- Create: `Assets/_Project/Application/Features/Meta/Workshop.cs`
- Modify: `Assets/_Project/Application/Features/Meta/MetaCore.cs` (`MetaErrors.NotEnoughScraps`), `Assets/_Project/Composition/RootLifetimeScope.cs` (`builder.Register<Workshop>(Lifetime.Singleton)`), `Assets/Tests/Domain/Application/TestApp.cs` (propriété `Workshop`)
- Test: `Assets/Tests/Domain/Application/WorkshopTests.cs`

**Interfaces:**
- Consumes: Tasks 1 à 4, `MetaContext` (`Session`, `Meta`, `Touch(MetaArea)`).
- Produces :
  - `MetaErrors.NotEnoughScraps = new DomainError("meta.scraps", "errors.not_enough_scraps")`
  - `readonly struct RecipeView { RecipeDef Recipe; bool Known; bool CanAfford; int Skill; IReadOnlyList<int> Odds; }`
  - `readonly struct SkillView { ItemType Type; int Level; int XpInLevel; int XpToNext; bool Maxed; }`
  - `readonly struct CraftOutcome { ItemDef Item; CraftQuality Quality; CraftedStats Stats; int NewSkill; bool LeveledUp; }`
  - `sealed class Workshop(MetaContext)` : `IReadOnlyList<RecipeView> Recipes()` (recettes connues seulement, dans l'ordre de `Recipes.All`), `IReadOnlyList<SkillView> Skills()` (4 types), `Result<CraftOutcome> Craft(string recipeId)`.

- [ ] **Step 1: Write the failing tests** (`WorkshopTests`) :
  - `Craft_SpendsScraps_AddsItem_GrantsXp` : recette `patch_vest` (6 scraps Armor) avec 10 scraps : succès, il reste 4, l'objet est possédé et enregistré, l'expérience Armor a augmenté de `XpGain`.
  - `Craft_Refused_WithoutEnoughScraps` : (Review Focus) 5 scraps : `meta.scraps`, scraps inchangés, aucun objet, aucune expérience.
  - `Craft_Refused_ForAnUnknownRecipe` : (Review Focus) `pulse_rifle` non apprise : `meta.unknown`, rien de dépensé même avec assez de scraps et de Noyaux.
  - `Craft_NeedsCoresForRareRecipes` : `plated_coat` (12 + 2 Noyaux) refusée avec 12 scraps et 1 Noyau ; acceptée avec 2.
  - `Craft_LevelsUp_WhenXpReachesTheThreshold` : donner 39 d'expérience, fabriquer une recette dont le gain fait passer le palier : `LeveledUp` vrai et `NewSkill` 2.
  - `Craft_AtMaxSkill_KeepsWorking` : (Review Focus) expérience à 1800 : fabrication OK, `Skill` reste 10, `SkillView.Maxed` vrai.
  - `Craft_UsesTheSavedCounter_NoDuplicateIds` : cinq fabrications donnent cinq identifiants distincts ; recharger le profil puis fabriquer donne `crafted_6`.
  - `Recipes_ShowOddsAndAffordability` : `RecipeView.Odds` = `CraftingRules.QualityOdds(skill, difficulty)` ; `CanAfford` suit les scraps.
  - `Craft_Publishes_LabArea` : un abonné à `MetaChanged` reçoit `MetaArea.Lab` après fabrication.
- [ ] **Step 2: Run** `WorkshopTests` — Expected: FAIL.
- [ ] **Step 3: Implement** `Workshop.Craft` : vérifie connue (sinon `MetaErrors.Unknown`), scraps du type puis Noyaux (sinon `NotEnoughScraps`, sans rien dépenser : tout vérifier avant de dépenser) ; générateur `Pcg32(CollectionRules.Seed(playerId ?? "local", "craft", Meta.craftedNext))` (même graine pour la qualité et les stats : qualité d'abord) ; dépense, `AddCrafted`, `AddXp(XpGain)`, `_c.Touch(MetaArea.Lab)` et `MetaArea.Collection`.
- [ ] **Step 4: Run** `WorkshopTests;MetaServiceTests` — Expected: PASS.
- [ ] **Step 5: Commit** — `feat(labo): service de fabrication Workshop`.

### Task 6: Drops de scraps et de recettes en combat

**Files:**
- Modify: `Packages/com.n29.heroesdawn.domain/Domain/Board/CraftingRules.cs` (`RollDrops`), `Assets/_Project/Application/Features/Meta/CraftingOps.cs` (`GrantDrops`), `Assets/_Project/Application/Features/Combat/FinishCombatUseCase.cs`, `Assets/_Project/Application/Features/Combat/CombatTypes.cs` (`CombatReport`)
- Test: `Assets/Tests/Domain/CraftingDropTests.cs`, `Assets/Tests/Domain/Application/CraftingOpsTests.cs` (ajouts)

**Interfaces:**
- Consumes: Tasks 1 à 4, `CombatRank` (Easy, Normal, Elite).
- Produces :
  - `readonly struct CraftDrops { IReadOnlyList<int> Scraps; string? NewRecipe; }` (`Scraps` : 5 entiers indexés par `ScrapKind`)
  - `CraftingRules.RollDrops(IRandom rng, CombatRank rank, bool boss, IReadOnlyList<string> unknownRecipes) : CraftDrops`
  - `CraftingOps.GrantDrops(GameSession s, CombatRank rank, bool boss) : CraftDrops` (graine `Seed(playerId ?? "local", "scrap", Meta.scrapDrops++)`, ajoute les scraps, apprend la recette)
  - `CombatReport.Crafting` (`CraftDrops?`, dernier paramètre optionnel du constructeur)

- [ ] **Step 1: Write the failing tests** :
  - `RollDrops_ScrapCountsFollowTheRank` : sur 300 tirages, total des 4 types (hors Noyaux) ∈ [1, 2] pour Easy, [2, 3] Normal, [4, 6] Elite et Boss ; un seul type reçoit les scraps du tirage.
  - `RollDrops_CoresOnlyForEliteAndBoss` : jamais de Noyau en Easy/Normal ; Boss : toujours 2 ; Elite : 1 dans environ 50 % des cas (± 8 points sur 600 tirages).
  - `RollDrops_RecipeChances` : Elite ≈ 25 %, Boss ≈ 60 %, jamais en Easy/Normal ; la recette est tirée parmi les inconnues fournies.
  - `RollDrops_AllRecipesKnown_GivesFiveBonusScraps` : (liste d'inconnues vide) pas de `NewRecipe` ; le total de scraps des 4 types = tirage normal + 5.
  - `RollDrops_IsDeterministic` : même graine, même résultat.
  - `GrantDrops_UpdatesTheSave` : les scraps s'ajoutent, une recette tirée est apprise (`Knows`), `Meta.scrapDrops` avance de 1.
  - `FinishCombat_Victory_GrantsScraps_Defeat_DoesNot` : via `TestApp` : une victoire fait monter le total de scraps et renseigne `CombatReport.Crafting` ; une défaite ne change rien (même scénario de combat que `CombatDropTests`/tests de `FinishCombatUseCase` existants).
- [ ] **Step 2: Run** `CraftingDropTests;CraftingOpsTests` — Expected: FAIL.
- [ ] **Step 3: Implement** : tirage dans l'ordre : type (`rng.Range(0, 4)`), quantité (`rng.Range(min, max + 1)`), Noyaux (`boss ? 2 : elite && rng.Chance(50) ? 1 : 0`), recette (`rng.Chance(boss ? 60 : elite ? 25 : 0)` puis `unknown[rng.Range(0, unknown.Count)]`, ou 5 scraps du même type si aucune inconnue). `FinishCombatUseCase` appelle `GrantDrops` après `RollDrop` quand `victory` et passe le résultat à `CombatReport`.
- [ ] **Step 4: Run** `CraftingDropTests;CraftingOpsTests;CombatDropTests` + `N29.HeroesDawn.Tests.Domain.Application` — Expected: PASS.
- [ ] **Step 5: Commit** — `feat(labo): scraps et recettes lâchés par les combats gagnés`.

### Task 7: Textes et affichage des objets fabriqués

**Files:**
- Create: `Assets/_Project/Presentation/Common/LocCrafting.cs` (clés dans `ExtendCombat`-like : suivre le même mécanisme de `LocCombat.cs`, partial `ExtendCrafting`, à brancher dans `Loc.cs` à côté de `ExtendCombat`)
- Modify: `Assets/_Project/Presentation/Hub/CollectionText.cs` (`ItemName`, `QualityName`, `QualityColor`), `Assets/_Project/Presentation/Mvp/ILocalization.cs` / `LocLocalization.cs` seulement si une méthode d'aide est nécessaire
- Test: `Assets/Tests/Presentation/CraftingTextTests.cs`

**Interfaces:**
- Consumes: Task 3 (`CraftedItems.TryParseNameKey`), Task 1.
- Produces : `CollectionText.ItemName(ILocalization loc, ItemDef item)` renvoie « <nom de la recette> <qualité en minuscules> » pour un objet fabriqué (`craft.name` = « {0} {1} ») ; `CollectionText.QualityName(ILocalization, CraftQuality)`, `CollectionText.QualityColor(CraftQuality) : Color` (Grossier = `Theme.TextMuted`, Standard = `Theme.Text`, Soigné = `Theme.Info`, Supérieur = `Theme.Perception`, Chef-d'œuvre = `Theme.Primary`). Clés : `craft.recipe.<id>` (12 noms de la Task 1), `craft.quality.<rough|standard|fine|superior|masterwork>` (Grossier, Standard, Soigné, Supérieur, Chef-d'œuvre), `craft.name`, `scrap.Weapon|Armor|Tool|Implant|Core` (« Scraps d'arme », « Scraps d'armure », « Scraps d'outil », « Scraps d'implant », « Noyaux »), `errors.not_enough_scraps` (« Il manque des scraps. »).

- [ ] **Step 1: Write the failing tests** :
  - `ItemName_OfACraftedItem_ShowsRecipeAndQuality` : `ItemName` d'un objet fabriqué `patch_vest` Fine = « Gilet rapiécé soigné » ; un objet du catalogue garde son nom actuel.
  - `EveryCraftKey_IsTranslated` : toutes les clés listées ci-dessus existent (`loc.T(key) != key`).
  - `QualityColor_DiffersByTier` : cinq couleurs distinctes.
- [ ] **Step 2: Run** `CraftingTextTests` — Expected: FAIL.
- [ ] **Step 3: Implement** les textes et `ItemName` ; dans l'inventaire et la fiche d'agent, afficher `QualityName` en étiquette colorée (`r.TagColor`) pour un objet fabriqué (même rangée que celle des objets du catalogue : seule la couleur de l'étiquette change).
- [ ] **Step 4: Run** `CraftingTextTests;N29.HeroesDawn.Tests.Presentation` — Expected: PASS.
- [ ] **Step 5: Commit** — `feat(labo): noms et qualités des objets fabriqués`.

### Task 8: Atelier sur la page du labo et rapport de combat

**Files:**
- Modify: `Assets/_Project/Presentation/Hub/Pages/Lab/LabPage.cs` (constructeur : paramètre `Workshop workshop` ajouté en dernier), `Assets/_Project/Presentation/Combat/CombatPresenter.cs` (`DropLine`), `Assets/Tests/Presentation/MetaPagesPresenterTests.cs` (appel du constructeur existant)
- Test: `Assets/Tests/Presentation/WorkshopPageTests.cs`

**Interfaces:**
- Consumes: Task 5 (`Workshop`, `RecipeView`, `SkillView`, `CraftOutcome`), Task 6 (`CombatReport.Crafting`), Task 7 (textes).
- Produces : `LabPagePresenter` ajoute, après les 4 rangées de recherche : une rangée de titre « ATELIER » (étiquette = total des 5 scraps), une rangée par compétence (« Armes niveau 3 », barre d'expérience), une rangée par recette connue (titre : nom ; sous-titre : coût en scraps, difficulté, probabilités des qualités ; bouton « Fabriquer » activé seulement si `CanAfford`, raison « Il manque des scraps »). Un clic sur le bouton d'une recette appelle `Workshop.Craft` et affiche un toast (`ToastKind.Reward`) « <objet> : <stats> » ; une erreur passe par `Report` comme les recherches. L'index d'une rangée de recette = `ResearchLab.Techs.Count + 1 + 4 + i`.

- [ ] **Step 1: Write the failing tests** (`WorkshopPageTests`, avec `FakeListView` et `TestApp` comme `Lab_StartShowsProgress_SecondRefused`) :
  - `LabPage_ShowsTheWorkshopRowsAfterTheTechs` : 4 recherches + 1 titre + 4 compétences + 4 recettes de départ = 13 rangées ; la première recette est `scrap_blade`.
  - `RecipeButton_DisabledWithoutScraps_EnabledWithThem` : sans scraps, `A.Enabled` faux avec `A.Reason == _loc.T("errors.not_enough_scraps")` ; après `AddScraps(Weapon, 6)` et un rafraîchissement, vrai.
  - `ClickingTheRecipe_CraftsAndToasts` : après le clic, 0 scrap restant, un objet fabriqué possédé, un toast `Reward` contenant le nom de l'objet.
  - `Combat_DropLine_MentionsScrapsAndNewRecipe` : un `CombatReport` avec `Crafting` donne une ligne listant les scraps obtenus et la recette apprise (`combat.craft_drop`).
- [ ] **Step 2: Run** `WorkshopPageTests` — Expected: FAIL.
- [ ] **Step 3: Implement** dans `Render()` à la suite de la boucle des recherches et dans `OnRowButton` (réutiliser `RowButtonData.Of(label, enabled, reason, style)` ; `style` 0 = bouton principal comme « Lancer ») ; textes `lab.workshop`, `lab.skill`, `lab.recipe_cost`, `lab.craft`, `lab.crafted` et `combat.craft_drop` ajoutés dans `LocCrafting.cs`. `DropLine` ajoute la ligne de scraps après la ligne d'objet.
- [ ] **Step 4: Run** `WorkshopPageTests;MetaPagesPresenterTests;CombatPresenterTests` — Expected: PASS.
- [ ] **Step 5: Commit** — `feat(labo): atelier de fabrication sur la page du labo`.

### Task 9: Documentation et suites complètes

**Files:**
- Modify: `Code/docs/specs/2026-10-27-labo-fabrication-design.md` (§4 : fenêtre de résultat remplacée par un toast ; onglet remplacé par des rangées sous les recherches), `01_Game_Design/04_Economie.md` (section scraps et recettes), `01_Game_Design/` fiche des objets (type et qualité des objets fabriqués : chercher le fichier qui décrit les objets), `05_Production/Decisions.md` (3 lignes datées 2026-10-27)

- [ ] **Step 1: Update docs** : tables des recettes, des probabilités de qualité, des drops, et décisions (objets fabriqués = exemplaires uniques ; fabrication instantanée ; compétence par type ; page du labo plutôt qu'un onglet).
- [ ] **Step 2: Run** `ut.sh EditMode` puis `ut.sh PlayMode` complets — Expected: 0 échec ; en cas de test PlayMode lent qui expire (`Potion_PlayedWithTheRealScreen_Succeeds`), le relancer seul.
- [ ] **Step 3: Commit** — `docs: fabrication d'objets au labo` (avec les rapports `Code/docs/test-reports/latest-*.html`, sans les fichiers de l'utilisateur).
