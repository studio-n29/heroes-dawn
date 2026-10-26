# Catégories des agents, des cartes et des objets — plan d'implémentation (lot B 0.14.0)

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Donner une nature (Offensive / Défensive / Soutien) à chaque carte et un type (Arme / Armure / Outil / Implant) à chaque objet, interdire deux objets du même type sur un agent, et afficher et filtrer ces catégories dans le grimoire et l'équipement.

**Architecture:** Deux enums et deux déductions pures dans le paquet domaine (`Definitions`), une règle dans `CollectionRules`, l'erreur et la migration dans `CollectionBook` / `GameSession`, puis l'affichage (pastilles en texte enrichi, barre de filtres commune aux pages en grille).

**Tech Stack:** Unity 6000.6.3f1, C# (nullable), NUnit (Test Framework), VContainer, MessagePipe, UniTask, prefabs générés par recettes (`Editor/Prefabs/Recipes`).

**Spec:** `Code/docs/specs/2026-10-26-categories-agents-cartes-objets-design.md`

## Global Constraints
- Chemins relatifs à `Code/Heroes Dawn Unity/`. Domaine : `Packages/com.n29.heroesdawn.domain/`. Jeu : `Assets/_Project/`. Tests : `Assets/Tests/`.
- Enums dans `Packages/com.n29.heroesdawn.domain/Definitions/CollectionDefs.cs`, à côté de `ItemRarity` et `ItemPerk` : `CardNature { Offensive, Defensive, Support }`, `ItemType { Weapon, Armor, Tool, Implant }` (cet ordre : les entiers sont écrits dans `items.json`).
- Chaque test porte `[Test, Description("Vérifie que … — parce que …")]` (contrôlé par `TestDescriptionsTests`).
- Textes en français dans `Assets/_Project/Presentation/Common/LocCollection.cs` ; clés : `card.nature.offensive|defensive|support`, `item.type.weapon|armor|tool|implant`, `filters.all`, `errors.collection_same_type`, `equip.type_taken`.
- Textes : `errors.collection_same_type` = « Cet agent porte déjà un objet de ce type. » ; `equip.type_taken` = « Type déjà porté ».
- Pas de remplacement automatique d'un objet du même type.
- Tests en ligne de commande (éditeur Unity fermé) :
  - EditMode : `"/c/Program Files/Unity/Hub/Editor/6000.6.3f1/Editor/Unity.exe" -batchmode -nographics -projectPath "C:/Users/dnath/Documents/Heroes Dawn/Code/Heroes Dawn Unity" -runTests -testPlatform EditMode -testResults "$TEMP/editmode.xml" -logFile "$TEMP/unity.log"` (ajouter `-testFilter "<Namespace>.<Classe>"` pour une classe) ; résumé : `grep -o '<test-run [^>]*' "$TEMP/editmode.xml"`.
  - PlayMode : même commande sans `-nographics`, avec `-testPlatform PlayMode`.
- Commits : messages en français, préfixe conventionnel, terminés par `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.

## Review Focus
- **Échange dans un même emplacement** : remplacer l'armure portée par une autre armure depuis la fiche agent doit marcher (l'objet de l'emplacement visé ne compte pas comme « type déjà porté »). → test Task 5.
- **`items.json` sans champ `type`** (fichier créé avant ce lot ou objet ajouté à la main) : le type est déduit, il ne vaut pas Arme par défaut. → test Task 3.
- **Sauvegarde à deux objets du même type mêlés à un objet inconnu** : l'objet inconnu est ignoré, le premier valide est gardé, le doublon de type retourne à l'inventaire. → test Task 4.
- **Filtres sans résultat** (ex. Perception × Défensive) : une ligne « aucune carte » apparaît, pas une grille vide. → test Task 6.
- **Filtres conservés après une action** (amélioration d'une carte, ouverture d'un booster) : le rafraîchissement ne remet pas les filtres à « Toutes ». → test Task 6.

---

### Task 1: Nature des cartes (domaine)

**Files:**
- Modify: `Packages/com.n29.heroesdawn.domain/Definitions/CollectionDefs.cs` (enum `CardNature`, classe `CardNatures`, `SkillCardDef`)
- Test: `Assets/Tests/Domain/CollectionRulesTests.cs`

**Interfaces:**
- Produces: `enum CardNature { Offensive, Defensive, Support }` ; `static class CardNatures { public static CardNature Of(SkillDef skill); }` ; `SkillCardDef.Nature` (`CardNature`) ; constructeur `SkillCardDef(CardId id, string nameKey, Family family, ItemRarity rarity, SkillDef skill, CardNature? nature = null)` (nul → `CardNatures.Of(skill)`).

- [ ] **Step 1: Write the failing tests** — dans `CollectionRulesTests`, un test paramétré `CardNatures_Of_FollowsTheBaseEffectAndTarget` avec `[TestCase]` construisant un `SkillDef` (slot A3, recharge 5) :

| Effet de base | Cible | Attendu |
|---|---|---|
| `Damage 100` | `SingleEnemy` | `Offensive` |
| `ApplyStatus Bind` | `SingleEnemy` | `Offensive` |
| `PushBack 25` | `AllEnemies` | `Offensive` |
| `GaugeDelta -20` | `SingleEnemy` | `Offensive` |
| `Shield 15` | `AllAllies` | `Defensive` |
| `Heal 35` | `SingleAlly` | `Support` |
| `ApplyStatus Haste` | `Self` | `Support` |
| `Advance 50` | `SingleAlly` | `Support` |
| `GaugeDelta 20` | `AllAllies` | `Support` |
| (aucun effet) | `SingleEnemy` | `Offensive` |

  Et `SkillCardDef_Nature_IsTheGivenOneOrTheDeducedOne` : sans nature → `CardNatures.Of(skill)` ; avec `CardNature.Support` sur une carte de dégâts → `Support`.

- [ ] **Step 2: Run the tests** (`-testFilter N29.HeroesDawn.Tests.Domain.CollectionRulesTests`) — Expected: compilation en échec (`CardNature` inconnu).
- [ ] **Step 3: Implement** `CardNature`, `CardNatures.Of` (règle : `Shield` → Défensive ; `Heal` → Soutien ; sinon cible ennemie (`SingleEnemy`, `AllEnemies`) → Offensive, cible alliée ou soi → Soutien ; sans effet → Offensive) et la propriété/paramètre de `SkillCardDef`. `SkillDef.Category` ne change pas.
- [ ] **Step 4: Run the tests** — Expected: PASS.
- [ ] **Step 5: Commit** — `feat(cartes): nature des cartes (offensive, défensive, soutien) déduite de l'effet`.

### Task 2: Type des objets et règle d'équipement (domaine)

**Files:**
- Modify: `Packages/com.n29.heroesdawn.domain/Definitions/CollectionDefs.cs` (enum `ItemType`, classe `ItemTypes`, `ItemDef`)
- Modify: `Packages/com.n29.heroesdawn.domain/Domain/Board/CollectionRules.cs` (surcharge `CanEquip`)
- Test: `Assets/Tests/Domain/CollectionRulesTests.cs`

**Interfaces:**
- Produces: `enum ItemType { Weapon, Armor, Tool, Implant }` ; `static class ItemTypes { public static ItemType Of(ItemPerk perk, int hp, int atk, int def, int spd); }` ; `ItemDef.Type` ; constructeur `ItemDef(..., int price = 0, ItemType? type = null)` (dernier paramètre, nul → déduction) ; `CollectionRules.CanEquip(ItemDef item, IEnumerable<ItemDef> equipped)` → `bool`.

- [ ] **Step 1: Write the failing tests**
  - `ItemTypes_Of_UsesThePerkFirst` : `SkillPower` → Implant ; `CritChance` → Weapon ; `OpeningShield` (avec atk 10, hp 15, comme phoenix_core) → Armor ; `StartGauge` → Tool.
  - `ItemTypes_Of_UsesTheStrongestStatWithoutPerk` : atk 8 → Weapon ; hp 10 → Armor ; def 10 → Armor ; spd 6 → Tool ; égalités : atk 10 = def 10 → Weapon ; hp 8 = spd 8 → Armor ; tout à 0 → Weapon.
  - `CanEquip_RefusesASecondItemOfTheSameType` : armure + [armure] → faux ; armure + [arme] → vrai ; armure + [] → vrai.
- [ ] **Step 2: Run the tests** — Expected: compilation en échec.
- [ ] **Step 3: Implement** `ItemType`, `ItemTypes.Of` (effet spécial : `SkillPower` Implant, `CritChance` Weapon, `OpeningShield` Armor, `StartGauge` Tool ; sinon max de (atk, max(hp, def), spd) avec priorité atk, puis hp/def, puis spd), `ItemDef.Type`, et `CanEquip` (aucun équipé du même `Type`).
- [ ] **Step 4: Run the tests** — Expected: PASS ; tout `CollectionRulesTests` vert.
- [ ] **Step 5: Commit** — `feat(objets): type d'objet (arme, armure, outil, implant) et règle « pas deux du même type »`.

### Task 3: Contenu et outil de design

**Files:**
- Modify: `Assets/_Project/Content/Prototype/PrototypeCollection.cs` (helpers `I` et `C` : paramètre de type / nature ; valeurs des 12 objets et 12 cartes)
- Modify: `Assets/_Project/Authoring/ContentData.cs` (`ItemRecord.type`)
- Modify: `Assets/_Project/Authoring/ContentDataConverter.cs` (`ToDef(ItemRecord)`, `FromDef(ItemDef, string)`)
- Modify: `Assets/_Project/Resources/Content/items.json` (champ `type` sur les 10 objets)
- Test: `Assets/Tests/Editor/DesignToolsDataTests.cs`

**Interfaces:**
- Consumes: `ItemType`, `ItemTypes.Of`, `CardNature`, `CardNatures.Of` (Tasks 1-2).
- Produces: `ItemRecord.type` de type `ItemType`, initialisé à `(ItemType)(-1)` (sentinelle « non saisi » : `JsonUtility` garde l'initialiseur quand le champ manque) ; `ToDef` passe `Enum.IsDefined(typeof(ItemType), i.type) ? i.type : (ItemType?)null` ; `FromDef` écrit toujours `i.Type`.

- [ ] **Step 1: Write the failing tests**
  - `Catalog_EveryCardAndItemHasAnExplicitCategory_EqualToItsDeduction` : pour `PrototypeCollection.Build()`, chaque carte a `Nature == CardNatures.Of(card.Skill)` et chaque objet `Type == ItemTypes.Of(...)` ; pour `ContentDataLibrary.Load().Items` (fichier `items.json`), chaque enregistrement brut a un `type` défini (`Enum.IsDefined`) égal à la déduction.
  - `ItemRecord_WithoutType_GetsTheDeducedType` : `JsonUtility.FromJson<ItemsFile>("{\"items\":[{\"id\":\"x\",\"def\":10}]}")` puis `ContentDataConverter.ToDef` → `ItemType.Armor` (pas `Weapon`).
  - `ItemRecord_RoundTrip_KeepsTheType` : `FromDef` puis `ToDef` d'un objet `Tool` → `Tool`.
- [ ] **Step 2: Run the tests** (`-testFilter N29.HeroesDawn.Tests.Editor.DesignToolsDataTests`) — Expected: FAIL (`items.json` sans `type`, sentinelle absente).
- [ ] **Step 3: Implement** le champ, le convertisseur (tooltip : « Type d'objet : un agent ne porte pas deux objets du même type. »), puis saisir les valeurs de la spec : cartes Offensive (card_prism_burst, card_shackle, card_ember, card_storm, card_inferno, card_bulwark_charge, card_mark), Défensive (card_glass_wall), Soutien (card_second_wind, card_regen_field, card_precog, card_time_rift) ; objets Arme (steel_knuckles 0, titan_gauntlets, apex_module), Armure (kevlar_vest, riot_shield, reactive_plating, phoenix_core), Outil (running_shoes, adrenaline_patch, time_shard), Implant (power_gauntlet, arcane_focus). Dans `items.json`, `"type"` après `"price"`, en entier (Weapon 0, Armor 1, Tool 2, Implant 3).
- [ ] **Step 4: Run the tests** — Expected: PASS.
- [ ] **Step 5: Commit** — `feat(contenu): nature des 12 cartes et type des objets (code, items.json, outil Objets)`.

### Task 4: Erreur d'équipement et migration des sauvegardes (application)

**Files:**
- Modify: `Assets/_Project/Application/Features/Collection/CollectionBook.cs` (`CollectionErrors.SameItemType`, `EquipItem`)
- Modify: `Assets/_Project/Application/Session/GameSession.cs` (`LoadEquipment`)
- Modify: `Assets/_Project/Presentation/Common/LocCollection.cs` (`errors.collection_same_type`)
- Test: `Assets/Tests/Domain/Application/CollectionBookTests.cs`

**Interfaces:**
- Consumes: `CollectionRules.CanEquip(ItemDef, IEnumerable<ItemDef>)` (Task 2).
- Produces: `CollectionErrors.SameItemType = new DomainError("collection.same_type", "errors.collection_same_type")`. Ordre des refus dans `EquipItem` : membre inconnu, objet inconnu, `NoSlot`, **`SameItemType`**, `NotOwned` (le test existant `EquipItem_NeedsStock_AndAtMostTwoPerAgent` reste vrai tel quel).

- [ ] **Step 1: Write the failing tests**
  - `EquipItem_RefusesASecondItemOfTheSameType` : avec kevlar_vest et riot_shield en stock, équiper kevlar_vest sur Ancre réussit, riot_shield renvoie `"collection.same_type"`, running_shoes réussit ; riot_shield reste disponible (`AvailableItems == 1`).
  - `LoadedSave_WithTwoItemsOfTheSameType_ReturnsTheSecondToTheStock` : profil dont Verre porte `["unknown_item", "kevlar_vest", "riot_shield"]` (stock : 1 de chaque) ; après `ApplyProfile`, `Items == [kevlar_vest]` et `AvailableItems(riot_shield) == 1`. Même chose rechargée une 2e fois (idempotent).
  - `LoadedSave_Valid_IsUntouched` : arme + armure → les deux restent.
- [ ] **Step 2: Run the tests** (`-testFilter N29.HeroesDawn.Tests.Domain.Application.CollectionBookTests`) — Expected: FAIL.
- [ ] **Step 3: Implement** l'erreur et son texte ; dans `LoadEquipment`, n'ajouter un objet que s'il est connu, qu'il reste une place et que `CanEquip(item, objets déjà ajoutés)` ; mettre à jour le commentaire XML (« un objet … d'un type déjà porté est ignoré »). `GameSession` n'a pas de logger : pas de ligne de log (écart assumé par rapport à la spec).
- [ ] **Step 4: Run the tests** — Expected: PASS.
- [ ] **Step 5: Commit** — `feat(objets): refus d'un 2e objet du même type et sauvegardes remises en règle au chargement`.

### Task 5: Pastilles et objets grisés (présentation)

**Files:**
- Modify: `Assets/_Project/Presentation/Hub/CollectionText.cs` (`NatureChip`, `ItemTypeChip`)
- Modify: `Assets/_Project/Presentation/Common/LocCollection.cs` (`card.nature.*`, `item.type.*`, `equip.type_taken`)
- Modify: `Assets/_Project/Presentation/Hub/Pages/Grimoire/GrimoirePage.cs` (sous-titre)
- Modify: `Assets/_Project/Presentation/Hub/Pages/Inventory/InventoryPage.cs` (sous-titre)
- Modify: `Assets/_Project/Presentation/Hub/Pages/Loadout/LoadoutPage.cs` (bouton Équiper grisé)
- Modify: `Assets/_Project/Presentation/Hub/Pages/MemberSheet/MemberSheetData.cs` (`SheetTile.Dim`), `MemberSheetPresenter.cs` (choix d'objet), `SheetTileView.cs` (rendu grisé)
- Test: `Assets/Tests/Presentation/CollectionPagesPresenterTests.cs`, `Assets/Tests/Presentation/MemberSheetPresenterTests.cs`

**Interfaces:**
- Consumes: `SkillCardDef.Nature`, `ItemDef.Type`, `CollectionRules.CanEquip(ItemDef, IEnumerable<ItemDef>)`, `CollectionErrors.SameItemType`.
- Produces: `CollectionText.NatureChip(ILocalization loc, CardNature n)` et `CollectionText.ItemTypeChip(ILocalization loc, ItemType t)` → `string` en texte enrichi `"<color=#RRGGBB>● LIBELLÉ</color>"`. Couleurs : Offensive `Theme.Danger`, Défensive `Theme.Info`, Soutien `Theme.Success` ; Arme `Theme.Danger`, Armure `Theme.Info`, Outil `Theme.Primary`, Implant `Theme.Pr`. `SheetTile.Dim` (`bool`).

- [ ] **Step 1: Write the failing tests**
  - `Grimoire_ShowsTheNatureOfEachCard` : la ligne « Mur de verre » (card_glass_wall) contient le libellé de `card.nature.defensive`.
  - `Inventory_ShowsTheTypeOfEachItem` : kevlar_vest possédé → la ligne contient le libellé de `item.type.armor`.
  - `Loadout_GreysAnItemWhoseTypeIsAlreadyWorn` : Verre porte kevlar_vest ; riot_shield en stock → bouton A désactivé, raison = `equip.type_taken` ; running_shoes → bouton actif.
  - `MemberSheet_ItemPicker_DimsTakenTypes_ButAllowsSwappingTheSameSlot` : Verre porte kevlar_vest (emplacement 1) et running_shoes (emplacement 2) ; en ouvrant le choix de l'emplacement 1, riot_shield n'est **pas** grisé et l'échange réussit (Verre porte riot_shield + running_shoes) ; en ouvrant l'emplacement 2, riot_shield est grisé (`Dim`, `Sub` = `equip.type_taken`) et le toucher affiche le toast de `errors.collection_same_type` sans rien changer.
- [ ] **Step 2: Run the tests** (`-testFilter "N29.HeroesDawn.Tests.Presentation.CollectionPagesPresenterTests;N29.HeroesDawn.Tests.Presentation.MemberSheetPresenterTests"`) — Expected: FAIL.
- [ ] **Step 3: Implement** — grimoire : la pastille précède la ligne actuelle (`NatureChip + " · " + CardLine`) ; équipement : `ItemTypeChip` avant la rareté ; Loadout : `enabled = room && CanEquip(it, objets portés)`, raison `room ? equip.type_taken : loadout.no_slot_reason` ; fiche : la tuile est grisée si un objet porté **hors emplacement visé** a le même type, son action montre l'erreur en toast ; `SheetTileView` applique `Dim` (alpha 0,45 comme `InfoRowData.Dim`).
- [ ] **Step 4: Run the tests** — Expected: PASS, et toutes les classes `Presentation` vertes.
- [ ] **Step 5: Commit** — `feat(ui): nature des cartes, type des objets et objets d'un type déjà porté grisés`.

### Task 6: Barre de filtres du grimoire et de l'équipement

**Files:**
- Create: `Assets/_Project/Presentation/Hub/Pages/Lists/FilterBarData.cs` (`FilterGroupData`, `IFilterBarView`)
- Modify: `Assets/_Project/Presentation/Hub/Pages/Lists/CardGridPageView.cs` (implémente `IFilterBarView`)
- Modify: `Assets/_Project/Editor/Prefabs/Recipes/CollectionPageRecipes.cs` (`CardGridKit.Build` : rangée de filtres sous l'en-tête, masquée sans filtres)
- Modify: `Assets/_Project/Presentation/Hub/Pages/Grimoire/GrimoirePage.cs`, `Inventory/InventoryPage.cs` (+ leurs interfaces de vue)
- Modify: `Assets/Tests/Presentation/MetaPagesPresenterTests.cs` (`FakeListView` : ajouter `IFilterBarView` et `IInventoryPageView`, qu'il n'implémente pas encore)
- Modify: `Assets/_Project/Presentation/Common/LocCollection.cs` (`filters.all`, `grimoire.none_filtered`, `inventory.none_filtered`)
- Regenerate: `Pages/Grimoire.prefab`, `Pages/Inventory.prefab`, `Pages/Loadout.prefab`, `Pages/Agents.prefab`, `Pages/Lounge.prefab`
- Test: `Assets/Tests/Presentation/CollectionPagesPresenterTests.cs`

**Interfaces:**
- Produces:
  - `public struct FilterGroupData { public string[] Labels; public int Selected; }`
  - `public interface IFilterBarView { event Action<int, int> FilterClicked; void ShowFilters(FilterGroupData[] groups); }` (`FilterClicked(group, option)` ; tableau vide → barre masquée).
  - `IGrimoirePageView : IListPageView, IFilterBarView` ; `IInventoryPageView : IListPageView, IFilterBarView`.
  - Grimoire : groupe 0 = `filters.all` + 4 familles (ordre de `Family`), groupe 1 = `filters.all` + 3 natures (ordre de `CardNature`). Équipement : groupe 0 = `filters.all` + 4 types (ordre de `ItemType`). Option 0 = « Toutes ».
  - Textes : `grimoire.none_filtered` = « Aucune carte pour ces filtres. », `inventory.none_filtered` = « Aucun objet pour ce filtre. ».

- [ ] **Step 1: Write the failing tests**
  - `Grimoire_FiltersByFamilyAndNature` : `FilterClicked(0, 1 + (int)Family.Matter)` puis `FilterClicked(1, 1 + (int)CardNature.Defensive)` → seule la ligne card_glass_wall ; `ShowFilters` a reçu `Selected` = 1 et 2.
  - `Grimoire_EmptyFilter_ShowsANote` : Perception × Défensive → une seule ligne, `Dim`, titre `grimoire.none_filtered`.
  - `Grimoire_KeepsItsFilters_AfterAnUpgrade` : filtre Élémentaire actif, carte card_ember améliorée via `RowButtonClicked` → les lignes restent filtrées sur Élémentaire.
  - `Grimoire_FocusedCard_PresetsItsFamily` : `HubNavigator.FocusCard = "card_mark"` → filtre famille = Perception, nature = Toutes.
  - `Inventory_FiltersByType` : kevlar_vest et steel_knuckles possédés ; filtre Armure → seule kevlar_vest ; filtre Implant → note `inventory.none_filtered`.
- [ ] **Step 2: Run the tests** — Expected: FAIL (compilation).
- [ ] **Step 3: Implement** — vue : boutons créés depuis le prefab `UiButton` existant de l'en-tête (un groupe par rangée, l'option choisie en style 0, les autres en style 3) ; présenteurs : sélection gardée dans des champs (non remise à zéro par `Render`), `ShowFilters` appelé à chaque `Render`, `OnFilter` → `Refresh()` ; recette : rangée `Filters` (hauteur 64) entre l'en-tête et la grille, la grille descend d'autant seulement si la page a des filtres (`ShowFilters` avec tableau vide → rangée masquée, grille à sa place actuelle).
- [ ] **Step 4: Regenerate the prefabs** — `-executeMethod N29.HeroesDawn.EditorTools.Prefabs.PrefabGenerator.GenerateAllBatch -quit -overwrite Pages/Grimoire.prefab,Pages/Inventory.prefab,Pages/Loadout.prefab,Pages/Agents.prefab,Pages/Lounge.prefab` ; vérifier que `git status` ne liste que ces 5 prefabs côté `Prefabs/`.
- [ ] **Step 5: Run the tests** — EditMode complet + PlayMode `HubNavigationFlowTests;CollectionPlayTests` — Expected: tout vert (dont `PrefabStructureTests`, `NoCodeBuiltScreensTests`).
- [ ] **Step 6: Commit** — `feat(ui): filtres par famille et nature au grimoire, par type à l'équipement`.

### Task 7: Documentation et vérification finale

**Files:**
- Modify: `01_Game_Design/06_Kits_Competences.md` (section « Nature des cartes » : les trois natures, la table de déduction de la spec, nature saisie par carte)
- Modify: `01_Game_Design/03_Metahumains.md` (une phrase : la famille est la catégorie de l'agent, et le type de pouvoir des cartes)
- Modify: `00_GDD/GDD_Heroes_Dawn.md` (objets : 4 types, pas deux du même type par agent)
- Modify: `05_Production/Decisions.md` (3 lignes datées 2026-10-26, renvoi à la spec)

- [ ] **Step 1: Write the docs** — reprendre les tables et règles de la spec sans les reformuler.
- [ ] **Step 2: Run the full suites** — EditMode et PlayMode complets — Expected: 0 échec ; noter les totaux.
- [ ] **Step 3: Commit** — `docs: catégories des cartes et des objets (GDD, kits, méta-humains, décisions)` (avec les rapports `Code/docs/test-reports/latest-*.html` régénérés).
