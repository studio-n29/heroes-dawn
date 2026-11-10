# Atelier de fabrication — Plan d'implémentation

> **Pour les agents :** sous-compétence requise : superpowers:executing-plans (exécution native) ou superpowers:subagent-driven-development. Cases à cocher (`- [ ]`).

**Objectif :** supprimer le Laboratoire de recherche, remplacer scraps et compétence de création par des Composants, des catalyseurs et des objets de 1 à 6 étoiles.

**Architecture :** domaine (`com.n29.heroesdawn.domain`) pour les types, recettes et tirages ; application (`Features/Meta`, `Features/Shop`, `Session`, `Ports/MetaSave`) pour les opérations et la migration ; présentation modifiée seulement pour compiler.

**Stack :** C# Unity 6000.6.3f1, NUnit Edit Mode.

**Spec :** `Code/docs/specs/2026-11-10-atelier-fabrication-design.md`

## Contraintes globales

- Français pour textes, commentaires et commits conventionnels ; chaque test a `[Description("Vérifie que … — parce que …")]`.
- Tests en batch, Unity fermé (commande de `Code/SETUP.md`) ; résumé lu dans `$TEMP/editmode.xml` (`passed` / `failed`).
- L'arbre de travail contient d'autres chantiers : `git add` **par chemin**, jamais `-A` ni `.` ; pas de push.
- Aucun prefab, scène ni recette `Editor/Prefabs` n'est modifié (session UI). Les écrans `LabPage*` ne reçoivent que les corrections de compilation.
- Valeurs de la spec : étoile de départ 2 / 4 / 5 ; tirage −1 25 %, 0 50 %, +1 25 % ; multiplicateurs ★1 à ★6 = 80 / 100 / 115 / 130 / 150 / 175 ; coûts 6 / 8 / 10 et qualités minimales Commun / Raffiné / Raffiné ; catalyseur Élite 10 %, Boss 30 %.
- Ce plan s'exécute après le plan A ou sur la même branche (`feature/0-16-rangs-positions-cartes`), jamais en parallèle sur les mêmes fichiers (`GameSession.cs`, `Loc*.cs`).

## Revue ciblée

1. Sauvegarde 0.15.0 avec des scraps, des Noyaux, des recherches en cours (`labActive`) et des objets fabriqués : tout est converti, rien de négatif, rien perdu.
2. Chargement répété d'une sauvegarde déjà migrée : aucun double remboursement ni double conversion.
3. Fabrication avec Composants insuffisants, mauvaise qualité (Commun pour une recette Rare) ou catalyseur absent du stock : refus sans rien dépenser.
4. Une recette dont la statistique orientée existe déjà ne dépasse jamais 3 statistiques.
5. Étoile 6 : atteignable seulement avec des Composants Exceptionnels ; jamais 0 ni 7 quel que soit le tirage.

## Structure des fichiers

| Fichier | Rôle |
|---|---|
| `Definitions/Crafting.cs` | `ComponentKind`, `ComponentGrade`, `CatalystKind`, `RecipeSource`, `RecipeDef` revu, 12 recettes |
| `Definitions/CraftedItems.cs` | étoiles à la place de la qualité, clé de nom `craft.item.<recette>.<étoiles>` |
| `Domain/Board/CraftingRules.cs` | étoiles, statistiques avec catalyseur, sources de Composants ; plus de compétence |
| `Application/Features/Meta/CraftingOps.cs` | stocks de Composants et de catalyseurs, objets fabriqués |
| `Application/Features/Meta/Workshop.cs` | `Craft(recipeId, grade, catalyst?)` |
| `Application/Features/Meta/ResearchLab.cs` | **supprimé** (et `LabBonuses`) |
| `Application/Features/Meta/MetaMigration.cs` (nouveau) | migration idempotente et remboursement |
| `Application/Ports/MetaSave.cs` | `components`, `catalysts`, `CraftedItemSave.stars` ; champs de labo conservés en lecture seule pour la migration |
| `Application/Features/Shop/BuyScrapOfferUseCase.cs`, `Session/ShopScrapOffer.cs` | offre de Composants |
| `Assets/Tests/Domain/ComponentCraftingTests.cs`, `Assets/Tests/Application/WorkshopComponentsTests.cs`, `Assets/Tests/Application/MetaMigrationTests.cs` | tests |

---

### Tâche 1 : Types et règles de fabrication (domaine)

**Fichiers :** modifier `Crafting.cs`, `CraftedItems.cs`, `CraftingRules.cs` ; créer `Assets/Tests/Domain/ComponentCraftingTests.cs` ; adapter `CraftingDefsTests.cs`, `CraftingDropTests.cs`.

**Interfaces — produit :**
```csharp
public enum ComponentKind { Weapon, Armor, Tool, Implant }
public enum ComponentGrade { Common, Refined, Exceptional }
public enum CatalystKind { Metal, Energy, Bio, Crystal }
public enum RecipeSource { Starter, Adventure, Lair, Tournament }
// RecipeDef : ... int ComponentCost, ComponentGrade MinGrade, RecipeSource Source (plus de Difficulty, ScrapCost, CoreCost)
public static class CraftStars {
    public const int Min = 1, Max = 6;
    public static int Base(ComponentGrade g);            // 2, 4, 5
    public static int Percent(int stars);                // 80,100,115,130,150,175
    public static RecipeStatKind Oriented(CatalystKind c); // Metal→Def, Energy→Atk, Bio→Hp, Crystal→Spd
}
// CraftingRules
public static int RollStars(IRandom rng, ComponentGrade grade);
public static CraftedStats RollStats(IRandom rng, RecipeDef recipe, int stars, CatalystKind? catalyst);
public static CraftDrops RollDrops(IRandom rng, CombatRank rank, bool boss, IReadOnlyList<string> unknownRecipes);   // CraftDrops.Components : int[4*3] (index kind*3+grade), Catalyst : CatalystKind?, NewRecipe
// CraftedItems.NameKey(string recipeId, int stars) ; TryParseNameKey(string, out string recipeId, out int stars) ; ToDef(string id, RecipeDef r, int stars, CraftedStats s)
```

- [ ] **Étape 1 : écrire `ComponentCraftingTests`.**
  - `L_etoile_de_depart_suit_la_qualite` : Common 2, Refined 4, Exceptional 5.
  - `RollStars_reste_entre_1_et_6_et_ne_donne_6_qu_avec_Exceptional` : 10 000 tirages par qualité ; Common ∈ [1,3], Refined ∈ [3,5], Exceptional ∈ [4,6].
  - `RollStars_repartit_25_50_25` : Refined, 10 000 tirages, ±2 points.
  - `Percent_donne_80_100_115_130_150_175`.
  - `RollStats_sans_catalyseur_reste_dans_la_plage_multipliee` : recette Rare (8–14), ★3 → chaque stat ∈ [9, 16] (arrondi, min 1).
  - `RollStats_avec_catalyseur_ajoute_la_stat_orientee_sans_depasser_3` ; `Une_stat_deja_presente_est_tiree_dans_la_moitie_haute` (recette Atk, catalyseur Energy, ★2 : valeur ∈ [10, 12]).
  - `RollDrops` : Facile ne donne que du Commun (1–2) ; Élite donne du Raffiné ; Boss donne du Raffiné et parfois de l'Exceptionnel ; aucun catalyseur en Facile/Normal.
  - `Les_12_recettes_gardent_leurs_couts` : Commune 6 / Commun, Rare 8 / Raffiné, Épique 10 / Raffiné ; sources : 4 `Starter`, 8 `Adventure`.
  - `Le_nom_d_un_objet_fabriqué_se_relit` : `TryParseNameKey(NameKey("pulse_rifle", 5))` → ("pulse_rifle", 5).
- [ ] **Étape 2 : implémenter** les types, `RecipeDef`, le catalogue (mêmes ids, mêmes plages), `CraftStars`, `RollStars` (un seul tirage `rng.Range(0, 100)` : <25 → −1, <75 → 0, sinon +1), `RollStats`, `RollDrops` ; supprimer `ScrapKind`, `CraftQuality`, `CraftQualities`, `QualityOdds`, `XpForLevel`, `SkillLevel`, `XpGain`, `MaxSkill`.
- [ ] **Étape 3 : ne pas lancer Unity maintenant.** Supprimer `ScrapKind` et `CraftQuality` casse la compilation de l'application et de la présentation jusqu'à la tâche 4 : enchaîner les tâches 2 à 4, puis lancer les tests (au besoin d'abord `-testFilter ComponentCraftingTests`, puis la suite complète).
- [ ] **Étape 4 : pas de commit ici** : le commit unique des tâches 1 à 4 est à la fin de la tâche 4.

---

### Tâche 2 : Application — stocks, fabrication, boutique

**Fichiers :** modifier `CraftingOps.cs`, `Workshop.cs`, `MetaSave.cs`, `BuyScrapOfferUseCase.cs`, `IBuyScrapOfferUseCase.cs`, `ShopScrapOffer.cs`, `ShopState.cs`, `ShopDrawing.cs`, `IShopReadModel.cs`, `CombatTypes.cs`, `GameSession.cs`, `ResolveNarrativeUseCase.cs`, `ZoneTypes.cs`, `Ports.cs` (`scraps` → composants) ; créer `Assets/Tests/Application/WorkshopComponentsTests.cs` ; adapter `CraftingOpsTests`, `WorkshopTests`, `ShopScrapTests`, `MetaServiceTests`, `TestApp.cs`.

**Interfaces — produit :**
```csharp
// CraftingOps
public static int Components(GameSession s, ComponentKind kind, ComponentGrade grade);
public static void AddComponents(GameSession s, ComponentKind kind, ComponentGrade grade, int count);
public static bool TrySpendComponents(GameSession s, ComponentKind kind, ComponentGrade grade, int count);
public static int Catalysts(GameSession s, CatalystKind kind);
public static void AddCatalyst(GameSession s, CatalystKind kind, int count);
public static ItemDef AddCrafted(GameSession s, RecipeDef recipe, int stars, CraftedStats stats);
// MetaSave : List<CounterSave> components (id "Weapon.Common"), List<CounterSave> catalysts (id "Metal") ; CraftedItemSave.stars
// Workshop
public Result<CraftOutcome> Craft(string recipeId, ComponentGrade grade, CatalystKind? catalyst);   // CraftOutcome : Item, Stars, Stats
// MetaErrors.NotEnoughComponents, MetaErrors.GradeTooLow, MetaErrors.NoCatalyst
// ShopScrapOffer → ShopComponentOffer(ComponentKind kind, ComponentGrade grade, int count, int price)
```

- [ ] **Étape 1 : écrire `WorkshopComponentsTests`.**
  - `Fabriquer_depense_les_composants_et_le_catalyseur` : recette Commune `scrap_blade`, 6 Commun Arme + 1 Énergie → inventaire −6 / −1, objet ajouté avec ATQ présente.
  - `Une_qualite_trop_basse_est_refusee_sans_rien_depenser` : recette Rare avec grade Common → `GradeTooLow`, stocks inchangés.
  - `Des_composants_insuffisants_sont_refuses` → `NotEnoughComponents`.
  - `Un_catalyseur_absent_du_stock_est_refuse` → `NoCatalyst`.
  - `Une_recette_inconnue_est_refusee`.
  - `L_etoile_depend_du_grade` : 100 fabrications avec graines variées en Exceptional → étoiles ∈ [4,6] ; en Common ∈ [1,3].
  - `La_boutique_de_composants_vend_des_lots_Commun_et_Raffine` ; `Acheter_un_lot_ajoute_les_composants`.
  - `GrantDrops_verse_composants_catalyseur_et_recette`.
- [ ] **Étape 2 : implémenter.** `Workshop.Craft` : vérifie recette connue, grade ≥ `MinGrade`, composants suffisants, catalyseur en stock ; dépense ; graine `CollectionRules.Seed(playerId, "craft", craftedNext)` comme aujourd'hui ; `RollStars` puis `RollStats` ; `AddCrafted` ; `_c.Touch(MetaArea.Lab)` conservé (la zone s'appelle toujours `Lab`, renommage hors lot). `RecipeView` : `(Recipe, CanAfford(grade)…)` — fournir `RecipeView.CanCraft(ComponentGrade)`, `BaseStars(grade)`. `ShopDrawing` : les lots de boutique du jour sont `ShopComponentOffer` (Commun : 6 pour 120 Crédits ; Raffiné : 3 pour 240 Crédits ⚑), type tiré par la graine du jour.
- [ ] **Étape 3 : tests.** Voir fin de la tâche 4.

---

### Tâche 3 : Objets fabriqués et migration de sauvegarde

**Fichiers :** créer `Application/Features/Meta/MetaMigration.cs`, `Assets/Tests/Application/MetaMigrationTests.cs` ; modifier `CraftingOps.RegisterSaved`, `MetaSave.cs` (champs `scraps`, `craftXp`, `labLevels`, `labActive`, `labEndUtcTicks` **conservés** pour la lecture), `GameSession.cs` (appel de la migration au chargement du profil).

**Interfaces — produit :**
```csharp
public static class MetaMigration { public static void ToComponents(MetaSave m, Func<int,int> refund /* optionnel */, out int refundedCredits); }
// Ordre d'appel : au chargement, avant CraftingOps.RegisterSaved ; les Crédits remboursés sont ajoutés au portefeuille de la session.
```

- [ ] **Étape 1 : écrire `MetaMigrationTests`.**
  - `Les_scraps_deviennent_des_composants_Commun` : scraps Weapon 7, Armor 3 → `Weapon.Common` 7, `Armor.Common` 3 ; liste `scraps` vidée.
  - `Les_Noyaux_deviennent_des_catalyseurs_en_rotation` : 5 Noyaux → Métal 2, Énergie 1, Biologique 1, Cristal 1.
  - `Les_recherches_sont_remboursees` : `training` niveau 2, `loot` niveau 1 → (500+1000) + 500 = 2 000 Crédits ; `labLevels` vidé ; recherche active annulée (`labActive = ""`, `labEndUtcTicks = 0`) sans remboursement du niveau en cours.
  - `Les_anciens_objets_fabriques_prennent_qualite_plus_1` : `quality` 0 → `stars` 1 ; 4 → 5 ; stats inchangées.
  - `La_migration_est_idempotente` : deux passes = une passe (composants, catalyseurs, Crédits remboursés).
  - `Les_niveaux_de_competence_sont_supprimes` : `craftXp` vidé, aucun remboursement.
  - `Une_sauvegarde_vide_ne_change_rien`.
- [ ] **Étape 2 : implémenter.** Les nouveaux champs `components` / `catalysts` / `stars` sont ajoutés ; `stars == 0` signale un ancien objet. Les listes d'origine sont vidées par la migration, ce qui la rend idempotente. Le remboursement par recherche de niveau `n` vaut `Σ_{i=0}^{n-1} 500 × (i+1)`.
- [ ] **Étape 3 : tests** en fin de tâche 4.

---

### Tâche 4 : Suppression du Laboratoire de recherche et compilation

**Fichiers :** supprimer `ResearchLab.cs` (y compris `LabBonuses`, `LabTech`, `LabTechView`) ; modifier `MetaOverview.cs` (`Lab`, `LabBadge`), `RootLifetimeScope.cs` (enregistrement), `GameSession.cs` (`TrainCostFor`, bonus de Crédits, récupération et récompense de zone), `MetaCore.cs`, `LabPage.cs`, `LabPageView.cs`, `HubPageFactory.cs`, `Loc.cs`, `LocCrafting.cs`, `LocMeta.cs`, `CollectionText.cs` (compilation seulement) ; tests : retirer ou adapter `MetaServiceTests` (recherches), `LabPlayTests`, `LabCraftingTests`, `WorkshopPageTests`, `CraftingTextTests`.

- [ ] **Étape 1 : écrire/adapter les tests.**
  - `Le_prix_d_entrainement_n_a_plus_de_reduction` : `TrainCostFor(n) == MetaRules.TrainCost(n)`.
  - `Les_credits_de_combat_et_la_recuperation_reviennent_a_la_base` (pas de +5 %, récupération 1 par minute).
  - `Plus_aucun_service_de_recherche_n_est_enregistre` (le conteneur ne fournit plus `ResearchLab`).
  - Adapter les tests de présentation de l'atelier à la nouvelle API (`Craft(recipeId, grade, catalyst)`).
- [ ] **Étape 2 : supprimer et recâbler.** `grep -rn "ResearchLab\|LabBonuses\|labLevels\|TrainCostPercent\|CombatCreditsPercent\|RecoveryPerMinute\|ZoneRewardPercent" Assets` doit ne plus rien renvoyer en dehors de `MetaSave.cs` (lecture) et `MetaMigration.cs`. `LabPage` : retirer la partie « recherches » ; la partie fabrication utilise `Workshop` avec le grade `Common` et sans catalyseur par défaut (l'écran définitif est celui du lot UI). `LocCrafting` : libellés d'étoiles `craft.stars.1` à `craft.stars.6` (« ★ », « ★★ », …) et des Composants/catalyseurs (clés `component.<type>.<qualité>`, `catalyst.<nom>`, français et anglais).
- [ ] **Étape 3 : lancer tous les tests Edit Mode.** Attendu : zéro erreur de compilation (`$TEMP/unity.log`, pas de `error CS`), `failed="0"` dans `$TEMP/editmode.xml` ; sinon corriger. Comparer au nombre de tests avant le plan.
- [ ] **Étape 4 : commit** (un seul pour les tâches 1 à 4, car le projet ne compile pas entre elles) — `git add` des fichiers listés ci-dessus et des nouveaux tests, message : `feat(atelier): composants, catalyseurs et objets de 1 à 6 étoiles; suppression du laboratoire de recherche`.

---

### Tâche 5 : Documentation

**Fichiers :** `01_Game_Design/04_Economie.md`, `00_GDD/GDD_Heroes_Dawn.md` (section Atelier / Labo), `05_Production/Decisions.md`, `Code/docs/specs/2026-10-27-labo-fabrication-design.md`, `Code/docs/Guide_Outils_de_design.md` si elle cite les scraps.

- [ ] **Étape 1 : mettre à jour** les documents de la spec §4 ; ajouter en tête de l'ancienne spec : « Remplacée par `2026-11-10-atelier-fabrication-design.md` ».
- [ ] **Étape 2 : `Decisions.md`** : lignes ajoutées sans toucher aux autres modifications non commitées du fichier (`git add -p`).
- [ ] **Étape 3 : commit** — `docs(conception): atelier de fabrication, composants et suppression du labo`.

---

## Lot UI à prévoir (hors plan)
Onglet Atelier (liste des recettes, choix de la qualité de Composants et du catalyseur, aperçu des étoiles et des plages), inventaire de Composants, boutique de Composants, retrait de l'entrée « Laboratoire » de la navigation. La session « Aube tactique » en a été informée.

## Auto-relecture
- Couverture : §1 (tâche 1), §2 (tâches 2, 3), suppression du labo (4), §4 documentation (5), §5 tests (dans chaque tâche).
- Écart avec la spec : la « recherche » du labo (`ResearchLab`) est distincte de la fabrication (`Workshop`) dans le code ; la spec et la migration le traitent (remboursement des Crédits).
- Les tâches 1 à 4 ne compilent qu'ensemble (suppression de `ScrapKind` et de `CraftQuality`) : elles se terminent par un commit unique.
