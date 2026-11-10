# Rangs AMIF, positions et étoiles des cartes — Plan d'implémentation

> **Pour les agents :** sous-compétence requise : superpowers:executing-plans (exécution native, choix habituel du projet) ou superpowers:subagent-driven-development. Les étapes utilisent des cases à cocher (`- [ ]`).

**Objectif :** remplacer rôle et rareté d'agent par position et rang AMIF (1 à 5), passer les cartes à 5 étoiles et 4 types, porter les emplacements de cartes à 3 avec équipement libre et bonus de position et de famille.

**Architecture :** tout vit dans le domaine (`com.n29.heroesdawn.domain`) et l'application ; les données prototypes (`PrototypeAgents`, `PrototypeCollection`, `agents.json`) sont mises à jour. La présentation n'est modifiée que pour que le projet compile. La session « UI mise à jour Aube tactique » refait les écrans : aucun prefab, aucune recette (`Editor/Prefabs`), aucun écran n'est modifié ici.

**Stack :** C# Unity 6000.6.3f1, NUnit (Edit Mode), JSON de contenu.

**Spec :** `Code/docs/specs/2026-11-10-rangs-positions-cartes-design.md`

## Contraintes globales

- Textes, commentaires, messages de commit en **français** ; commits conventionnels (`feat(domaine): …`).
- Chaque test porte `[Description("Vérifie que … — parce que …")]`.
- Les tests se lancent **Unity fermé**, en batch (commande de `Code/SETUP.md`, §Tests) :
  `"/c/Program Files/Unity/Hub/Editor/6000.6.3f1/Editor/Unity.exe" -batchmode -nographics -projectPath "C:/Users/dnath/Documents/Heroes Dawn/Code/Heroes Dawn Unity" -runTests -testPlatform EditMode -testResults "$TEMP/editmode.xml" -logFile "$TEMP/unity.log"` ; on lit le résumé dans `$TEMP/editmode.xml` (attributs `passed` / `failed`).
- L'arbre de travail contient des modifications d'autres chantiers (recettes d'UI, prefabs, `Decisions.md`…) : **ne jamais utiliser `git add -A` ni `git add .`** ; ajouter uniquement les fichiers de la tâche, par chemin.
- Pas de push, pas de fusion.
- Valeurs exactes de la spec : multiplicateurs de rang ×1 / ×1,15 / ×1,30 / ×1,45 / ×1,65 (en pourcentages 100 / 115 / 130 / 145 / 165) ; bonus de position +15 %, de famille +5 % (cumul +20 %) ; 3 cartes équipables ; boosters 50 / 28 / 15 / 6 / 1 ; étoiles 12 / 12 / 12 / 8 / 4.
- Valeurs **ajoutées par ce plan** (la spec ne les fixe pas, à relire) ⚑ : coût de recrutement par rang 150 / 220 / 450 / 800 / 1200 ; poids de boutique par rang 45 / 25 / 20 / 8 / 2 (seuls les agents `InShop` y entrent).

## Revue ciblée (cas que la spec sous-entend)

1. Sauvegarde 0.15.0 avec 2 cartes équipées (ou carte en 2e place seule) : chargée dans 3 emplacements, aucune carte perdue ni dupliquée.
2. Sauvegarde avec la même carte deux fois sur un agent : la seconde est ignorée au chargement ; en jeu, l'équipement du doublon est refusé.
3. Contenu JSON sans champ `position` ou `rank` (anciens fichiers, agents de test) : valeurs par défaut (Première ligne, rang 1), pas d'exception.
4. Agent à 5 compétences (2 + 3 cartes) en combat : la simulation, la recharge et l'IA ennemie ne plantent pas et les 5 sont jouables.
5. Un ennemi (côté `Side.Enemy`) ne reçoit jamais le bonus de position ni de famille, même s'il a des cartes.
6. Boosters : un rang d'étoile absent du jeu (par exemple aucune carte ★5) retombe sur l'étoile inférieure ; la pitié garantit ★3 ou mieux.

## Structure des fichiers

| Fichier | Rôle |
|---|---|
| `Packages/com.n29.heroesdawn.domain/Definitions/Enums.cs` | `Role` → `Position` ; suppression de `Rarity` |
| `…/Definitions/AgentRanks.cs` (nouveau) | multiplicateur, coût, poids de boutique par rang |
| `…/Definitions/Positions.cs` (nouveau) | types de cartes boostés par position |
| `…/Definitions/MemberDef.cs` | `Position Position`, `int Rank` (remplace `Rarity` et `PowerTier`) |
| `…/Definitions/CollectionDefs.cs` | `CardNature.Control`, déduction à 4 types, `SkillCardDef.Stars` |
| `…/Definitions/CardBonuses.cs` (nouveau) | bonus de position et de famille |
| `…/Domain/Board/CollectionRules.cs` | `CardSlots = 3`, équipement libre, tirage par étoiles |
| `…/Domain/Combat/UnitFactory.cs` | jusqu'à 5 compétences, bonus appliqués aux cartes des alliés |
| `Assets/_Project/Content/Prototype/PrototypeAgents.cs`, `PrototypeCollection.cs`, `PrototypeContent.cs` | rangs, positions, étoiles, types |
| `Assets/_Project/Authoring/ContentData.cs`, `ContentDataConverter.cs`, `Resources/Content/agents.json` | champs `position` et `rank` |
| `Assets/_Project/Application/Session/GameSession.cs`, `Features/Collection/CollectionBook.cs` | 3 emplacements, migration au chargement, refus du doublon |
| `Assets/Tests/Domain/*.cs`, `Assets/Tests/Application/*.cs` | tests (nouveaux fichiers `PositionTests`, `AgentRankTests`, `CardStarsTests`, `CardBonusTests`) |

---

### Tâche 1 : Branche et référence de départ

**Fichiers :** aucun fichier du dépôt.

- [ ] **Étape 1 : créer la branche.** `git switch -c feature/0-16-rangs-positions-cartes` depuis la branche courante (les modifications non commitées d'autres chantiers suivent, c'est voulu : voir contraintes). Vérifier `git status --short | head` pour noter les fichiers déjà modifiés avant la tâche 2.
- [ ] **Étape 2 : lancer tous les tests Edit Mode** (commande des contraintes). Noter le nombre de réussis et d'échecs dans la conversation : c'est la référence. Attendu : aucun échec ou, sinon, la liste des échecs préexistants est notée pour ne pas les attribuer à ce plan.

---

### Tâche 2 : `Role` devient `Position`

**Fichiers :**
- Modifier : `Definitions/Enums.cs`, `MemberDef.cs`, `EnemyDef.cs`, `PairBonuses.cs`, `SynergyLinks.cs`, `Domain/Combat/CombatUnit.cs`, `CombatSimulation.cs`, `UnitFactory.cs`, `Domain/Board/Activities/ActivityLevel.cs`, `Games/SnowSurvivalGame.cs` ; `Assets/_Project/Authoring/ContentData.cs`, `ContentDataConverter.cs`, `Content/Prototype/PrototypeAgents.cs`, `PrototypeContent.cs`, `PrototypeZones.cs` ; `Editor/Balance/ContentCsvExport.cs`, `Editor/Design/DesignWindows.cs` ; les fichiers de `Presentation/` listés par `grep -rlw Role` (compilation uniquement) ; `Resources/Content/agents.json`.
- Créer : `Packages/com.n29.heroesdawn.domain/Definitions/Positions.cs`.
- Tester : `Assets/Tests/Domain/PositionTests.cs` (nouveau) ; adapter les tests existants qui écrivent `Role.`.

**Interfaces — produit :**
```csharp
public enum Position { FrontLine, Attacker, Controller, Support, Ranged }   // valeurs 0..4, mêmes que l'ancien Role (Tank, Damage, Control, Support, Precision)
public static class Positions
{
    public static IReadOnlyList<CardNature> BoostedTypes(Position p);
}
```

- [ ] **Étape 1 : écrire `PositionTests`.**
  - `Les_valeurs_de_Position_gardent_celles_de_l_ancien_role` : `(int)Position.FrontLine == 0`, `Attacker == 1`, `Controller == 2`, `Support == 3`, `Ranged == 4` (parce que `agents.json` et les sauvegardes stockent l'entier).
  - `BoostedTypes_suit_la_table_de_la_spec` (écrit à la tâche 4, car il dépend de `CardNature.Control`) : FrontLine → `{Defensive}` ; Attacker → `{Offensive}` ; Controller → `{Control}` ; Support → `{Support}` ; Ranged → `{Offensive, Control}`.
- [ ] **Étape 2 : renommer.** Remplacer le type `Role` par `Position` dans tout le code (`grep -rlw "Role" --include=*.cs Assets Packages`) avec un `sed -i` par fichier, y compris les noms de membres : `Role.Tank`→`Position.FrontLine`, `Role.Damage`→`Position.Attacker`, `Role.Control`→`Position.Controller`, `Role.Support`→`Position.Support`, `Role.Precision`→`Position.Ranged`. Les propriétés `.Role` deviennent `.Position`. Vérifier avec `grep -rIw "Role" --include=*.cs Assets Packages` : plus aucune occurrence du type (les mots « rôle » en commentaires français peuvent rester). Ne pas toucher à `Loc*.cs` autrement que pour les identifiants C#.
- [ ] **Étape 3 : JSON.** Dans `ContentData.cs`, le champ `role` devient `position` (même entier) ; dans `agents.json`, remplacer `"role":` par `"position":` (`sed -i 's/"role":/"position":/'`) ; `ContentDataConverter` lit/écrit `position`.
- [ ] **Étape 4 : `Positions.cs`.** `BoostedTypes` a besoin de `CardNature.Control` : la créer à la tâche 4, pas ici. Dans cette tâche, `Positions.cs` n'existe pas encore et seul le premier test est écrit.
- [ ] **Étape 5 : lancer les tests Edit Mode.** Attendu : même résultat que la tâche 1.
- [ ] **Étape 6 : commit** — `git add` des fichiers modifiés par `sed` (liste via `git diff --name-only` restreinte aux chemins de la tâche) et de `PositionTests.cs` ; message : `refactor(domaine): le rôle devient la position (Première ligne, Attaquant, Contrôle, Soutien, Précision à distance)`.

---

### Tâche 3 : Rang AMIF à la place de la rareté d'agent et de `PowerTier`

**Fichiers :**
- Créer : `Definitions/AgentRanks.cs`, `Assets/Tests/Domain/AgentRankTests.cs`.
- Modifier : `Enums.cs` (supprimer `Rarity`), `MemberDef.cs`, `BalanceConfig.cs` (poids de boutique), `Domain/Shop/ShopWeights.cs`, `Domain/Shop/ShopDraw.cs`, `GameSession.cs` (`RecruitCost`), `Application/Features/Shop/RecruitFromShopUseCase.cs`, `ShopDrawing.cs`, `Tutorial/TutorialGuide.cs:152`, `PrototypeAgents.cs`, `PrototypeContent.cs`, `ContentData.cs`, `ContentDataConverter.cs`, `agents.json`, `Editor/Balance/BalanceSheetMapper.cs`, `ContentCsvExport.cs`, fichiers de présentation (compilation : `MemberCards.cs`, `MemberCardData.cs`, `AgentsPage.cs`, `Loc*.cs`) ; tests `AgentRosterTests`, `ShopRulesTests`, `PowerTierTests` (supprimé, remplacé), `LegacyRoster`, `RecruitPagePresenterTests`, `DesignToolsDataTests`, `ThemeTests`.

**Interfaces — produit :**
```csharp
public static class AgentRanks
{
    public const int Min = 1, Max = 5;
    public static int Clamp(int rank);                       // ramène dans 1..5
    public static int StatPercent(int rank);                 // 100, 115, 130, 145, 165
    public static int RecruitCost(int rank);                 // 150, 220, 450, 800, 1200 ⚑
    public static int ShopWeight(int rank);                  // 45, 25, 20, 8, 2 ⚑
}
// MemberDef : public int Rank { get; }   // remplace Rarity et PowerTier ; constructeur ... Position position, int rank, StatBlock ...
// ShopDraw.Rank(MemberDef) / ShopWeights.For(int rank) à la place de For(Rarity)
```
`stats(Position, int rank)` remplace `stats(Role, Rarity)` : le bloc de stats d'un agent se calcule avec `StatPercent(rank)` à la place de l'ancien multiplicateur ×1 / ×1,2 / ×1,45.

- [ ] **Étape 1 : écrire `AgentRankTests`.**
  - `StatPercent_donne_100_115_130_145_165` ; `Clamp_ramene_0_a_1_et_9_a_5`.
  - `Les_42_agents_ont_un_rang_et_la_repartition_3_6_10_13_10` : contenu prototype complet, rangs 5/4/3/2/1 = 3 / 6 / 10 / 13 / 10 (D, M, Victor Sorel au rang 5 ; les 10 cryptides S1 à S10 n'étant pas encore dans le jeu, ce test compte les 32 agents présents : rang 5 : 3, rang 4 : 5, rang 3 : 6, rang 2 : 8, rang 1 : 10).
  - `Chaque_agent_a_le_rang_de_la_table_de_la_spec` : trois agents témoins — `victor_sorel` 5, `odile_ferrand` 4, `anna_petrov` 1.
  - `Un_agent_sans_rang_dans_le_contenu_est_au_rang_1` (cas JSON ancien).
  - Adapter `ShopRulesTests` : poids par rang ; `Un_rang_5_n_est_jamais_propose_en_boutique_sans_deblocage_boutique` (les agents `InShop` seulement).
- [ ] **Étape 2 : `AgentRanks.cs`** avec les valeurs ci-dessus.
- [ ] **Étape 3 : `MemberDef`** : remplacer `Rarity` et `PowerTier` par `Rank` (borné par `AgentRanks.Clamp`). Supprimer l'énumération `Rarity`. Adapter `ShopWeights` (`From(BalanceConfig)` lit `AgentRanks.ShopWeight`), `ShopDraw.Rank`, `RecruitCost(int rank)`, `TutorialGuide` (condition « rang 1 » à la place de `Rarity.Agent`).
- [ ] **Étape 4 : `PrototypeAgents`** : le dictionnaire `PowerTiers` devient `Ranks`, rempli avec la **table de la spec** (§1, rangs validés) pour les 32 agents ; `Entry.Rarity` disparaît. Rangs : `agent_d` 5, `agent_m` 5, `victor_sorel` 5 ; `odile_ferrand`, `ada_okonkwo`, `eero_nieminen`, `astrid_hallstrom`, `louis_dubois` 4 ; `yelena_ivanova`, `marco_bellini`, `zofia_kowalczyk`, `mateo_vasquez`, `clement_lefevre`, `youssef_haddad` 3 ; `ilona_varga`, `haruto_nakamura`, `noah_kaplan`, `celeste_moreau`, `rin_takeda`, `giulia_rossi`, `kwame_abara`, `mei_lin_chen` 2 ; `darius_brooks`, `freya_lindqvist`, `lucas_mendes`, `amara_hughes`, `fatou_diallo`, `severine_morel`, `sam_okafor`, `tunde_oyelaran`, `ji_hoon_park`, `anna_petrov` 1. `LegacyRoster` (agents de test) : mapper Agent→1, Elite→3, Legend→4 pour garder les tests existants.
- [ ] **Étape 5 : JSON.** `ContentData.AgentRecord` : `rarity` → `rank` (entier 1 à 5, absent → 1). Régénérer `agents.json` : ancienne rareté 0/1/2 remplacée par le rang de la table ci-dessus (script ponctuel dans la scratchpad, relu avant d'écraser ; le champ `rank` est ajouté, `rarity` supprimé).
- [ ] **Étape 6 : `BalanceConfig`** : retirer `ShopWeightAgent/Elite/Legend` et leur ligne dans `BalanceSheetMapper` (les poids viennent de `AgentRanks`). La documentation est faite à la tâche 9.
- [ ] **Étape 7 : présentation (compilation seule).** `MemberCards` remplit `PowerTier` avec `def.Rank` (clé `power.tier` conservée) et le champ `Rarity` de `MemberCardData` avec la valeur numérique du rang ; l'affichage ne change pas. Si un texte demande une rareté d'agent, ajouter les clés `rank.1` à `rank.5` dans `Loc.cs` et `LocEnglish.cs` (libellés de la spec §1).
- [ ] **Étape 8 : lancer les tests Edit Mode.** Attendu : tous verts (les tests de rareté modifiés incluent).
- [ ] **Étape 9 : commit** — `feat(domaine): rang AMIF de 1 à 5 à la place de la rareté d'agent et de la puissance`.

---

### Tâche 4 : Types de cartes à 4 valeurs

**Fichiers :** `Definitions/CollectionDefs.cs`, `Definitions/Positions.cs`, `PrototypeCollection.cs`, tests `Assets/Tests/Domain/CardNatureTests.cs` (existant : adapter) et `PositionTests.cs`.

- [ ] **Étape 1 : tests.**
  - `La_deduction_donne_Control_pour_un_statut_de_controle_sur_ennemi` : carte à effet principal `ApplyStatus` Entrave sur `SingleEnemy` → `Control` ; `PushBack` sur ennemi → `Control` ; `Damage` seul → `Offensive` ; `Shield` → `Defensive` ; `Heal` → `Support` ; `Advance` sur allié → `Support`.
  - `BoostedTypes_suit_la_table_de_la_spec` (dans `PositionTests.cs`, voir tâche 2).
  - `Les_cartes_prototypes_ont_les_types_attendus` : `card_shackle`, `card_prism_burst`, `card_mark` → `Control` ; `card_ember`, `card_inferno`, `card_monolith` → `Offensive`.
- [ ] **Étape 2 : code.** `CardNature { Offensive, Defensive, Control, Support }` (l'ordre des valeurs existantes : `Offensive`=0, `Defensive`=1, `Support`=2 → insérer `Control` en **dernier** pour ne pas changer les entiers existants : `{ Offensive, Defensive, Support, Control }`). `CardNatures.Of` : statut de contrôle = `Bind, Shock, Freeze, Mark, Vulnerable, Blind` ; le reste de la table de la spec §2. `Positions.BoostedTypes` (tâche 2).
- [ ] **Étape 3 : données.** Dans `PrototypeCollection`, mettre `CardNature.Control` explicitement sur les cartes dont l'effet principal est un contrôle sur ennemi : `card_shackle`, `card_prism_burst`, `card_mark`, `card_guard_break`, `card_ram_strike` (recul) — vérifier chaque carte du catalogue : tout effet principal `ApplyStatus` d'un statut ci-dessus ou `PushBack` seul sur ennemi, sinon laisser inchangé.
- [ ] **Étape 4 : tests Edit Mode** verts. **Commit** : `feat(domaine): type de carte Contrôle (4 types) et déduction mise à jour`.

---

### Tâche 5 : Étoiles de cartes (1 à 5) et boosters

**Fichiers :** `CollectionDefs.cs` (`SkillCardDef`), `CollectionRules.cs` (`RollStars`, `PickCard`, `OpenBooster`), `BoosterDef` (poids, pitié), `PrototypeCollection.cs`, `CollectionText.cs` et `Boutique/*` (compilation seule), `ContentCsvExport.cs`, tests `Assets/Tests/Domain/CardStarsTests.cs` (nouveau) + adaptation des tests de boosters existants.

**Interfaces — produit :**
```csharp
// SkillCardDef : public int Stars { get; }   // 1..5, remplace Rarity ; constructeur: (CardId id, string nameKey, Family family, int stars, SkillDef skill, ...)
// CollectionRules : public static int RollStars(IRandom rng, IReadOnlyList<int> weights);   // 1..5
// BoosterDef.StandardWeights = { 50, 28, 15, 6, 1 } ; PityEvery inchangé ; pitié = au moins une carte ★3
```
Les **objets** gardent `ItemRarity` et `RollRarity` (bloc B).

- [ ] **Étape 1 : tests.**
  - `La_repartition_des_48_cartes_est_12_12_12_8_4` (étoiles 1 à 5).
  - `Chaque_carte_a_l_etoile_de_la_table` : témoins — `card_monolith` 5, `card_inferno` 4, `card_shackle` 3, `card_borrowed_mask` 2, `card_ember` 2, `card_concrete_rampart` 1.
  - `RollStars_respecte_les_poids` (graine fixe, 10 000 tirages, tolérance ±2 points sur 50/28/15/6/1).
  - `Une_etoile_absente_retombe_sur_l_etoile_inferieure` : collection sans ★5 et tirage ★5 → une carte ★4.
  - `La_pitie_garantit_une_carte_3_etoiles_ou_mieux`.
- [ ] **Étape 2 : code.** Remplacer `ItemRarity Rarity` par `int Stars` dans `SkillCardDef` (borné 1..5) ; `OpenBooster` et `PickCard` utilisent les étoiles (`hasRare` devient `Stars >= 3`) ; `CollectionDef`, `CollectionOps` et tout appelant de `card.Rarity` compilent (présentation : afficher `card.Stars` à la place, sans changer la mise en page).
- [ ] **Étape 3 : données.** L'aide `C(string id, Family f, CardNature n, int stars, …)` dans `PrototypeCollection` ; table complète (id → étoiles) :
  - ★5 : `card_monolith`, `card_chain_lightning`, `card_telluric_fist`, `card_time_rift`.
  - ★4 : `card_living_forge`, `card_prism_burst`, `card_ice_carapace`, `card_inferno`, `card_living_bastion`, `card_regen_field`, `card_silent_takedown`, `card_time_veil`.
  - ★3 : `card_reflecting_prism`, `card_support_cables`, `card_shackle`, `card_storm`, `card_spark_rush`, `card_power_flux`, `card_bloodletting`, `card_challenge`, `card_bulwark_charge`, `card_guard_break`, `card_shared_watch`, `card_precog`.
  - ★2 : `card_borrowed_mask`, `card_neon_halo`, `card_shared_pulse`, `card_premonition`, `card_glass_shards`, `card_ember`, `card_ram_strike`, `card_second_wind`, `card_tonic_current`, `card_precise_shot`, `card_guiding_echo`, `card_mark`.
  - ★1 : `card_concrete_rampart`, `card_rivet_armor`, `card_healing_molt`, `card_glass_wall`, `card_frost_wall`, `card_steam_shield`, `card_soothing_mist`, `card_steel_skin`, `card_deep_anchor`, `card_fresh_blood`, `card_side_step`, `card_camouflage`.
  Les effets des cartes ne changent pas dans ce plan (équilibrage au tableur, hors lot).
- [ ] **Étape 4 : tests Edit Mode** verts. **Commit** : `feat(domaine): cartes de 1 à 5 étoiles, boosters 50/28/15/6/1`.

---

### Tâche 6 : Trois emplacements et équipement libre

**Fichiers :** `CollectionRules.cs` (`CardSlots`, `CanEquip(SkillCardDef, …)`), `GameSession.cs` (`TeamMember.Cards`, `LoadEquipment`), `CollectionBook.cs` (`EquipCard`, nouvelle erreur), `UnitFactory.cs` (`SkillsWithCards`), tests `Assets/Tests/Application/CollectionBookTests.cs` / `GameSessionTests` (noms à confirmer avec `grep -rl EquipCard Assets/Tests`).

**Interfaces — produit :**
```csharp
public const int CardSlots = 3;                                                   // CollectionRules
public static bool CanEquip(SkillCardDef card, IEnumerable<CardId?> equipped);    // faux si la carte est déjà équipée (doublon)
// CollectionErrors.DuplicateCard = "collection.duplicate_card"  ; EquipCard n'exige plus de carte en emplacement précédent
// TeamMember.Cards : new CardId?[CollectionRules.CardSlots]
```

- [ ] **Étape 1 : tests.**
  - `On_equipe_une_carte_d_une_autre_famille` (avant : `WrongFamily` ; maintenant : `Ok`).
  - `On_equipe_trois_cartes_puis_la_quatrieme_emplacement_n_existe_pas` (`NoSlot`).
  - `Le_meme_carte_deux_fois_sur_un_agent_est_refusee` (`DuplicateCard`).
  - `On_peut_equiper_l_emplacement_3_sans_les_deux_premiers`.
  - Chargement : `Une_sauvegarde_a_deux_cartes_donne_deux_cartes_et_un_emplacement_libre` ; `Un_doublon_dans_la_sauvegarde_est_ignore` ; `Une_carte_seule_en_2e_place_est_remontee_en_1re` (garde la compaction actuelle, étendue à 3 emplacements).
  - `SkillsWithCards` : `Un_agent_a_cinq_competences_avec_trois_cartes` ; `Les_cartes_sont_ajoutees_dans_l_ordre_des_emplacements` ; `Un_agent_a_l_ancien_kit_de_trois_competences_remplace_la_3e` (comportement `LegacyRoster` conservé).
- [ ] **Étape 2 : code.** Mettre à jour `CardSlots`, `Cards = new CardId?[CollectionRules.CardSlots]`, `CanEquip` (plus de test de famille), `EquipCard` (suppression de `NeedFirstCard` et `WrongFamily` comme conditions, ajout de `DuplicateCard`), `LoadEquipment` (même règles, compaction généralisée), `SkillsWithCards` (toutes les cartes non nulles ajoutées ; plus de test `card.Family != d.Family`).
- [ ] **Étape 3 : texte d'erreur** `errors.collection_duplicate_card` : « Cet agent porte déjà cette carte. » (français et anglais).
- [ ] **Étape 4 : simulation.** Vérifier par un test de combat (`CombatSimulationTests` existant, nouveau cas `Cinq_competences_sont_jouables_sans_erreur`) qu'un allié à 5 compétences agit, que la recharge de chacune est suivie et que l'IA ennemie ne dépend pas d'un nombre fixe ; corriger le compte à `CombatSimulation.Actions.cs:22` si besoin. Le `SkillSlot` des cartes n'a que A3/A4 : si deux cartes partagent le même `Slot`, vérifier que rien ne les indexe par `Slot` ; sinon indexer par position dans la liste. **Limite connue à lever :** `CombatSimulation.Ai.cs` plafonne les compétences à 4 (`i < 4` dans la boucle de `ChooseAction`, `slot < 4` dans `CanUseSlot`, `i < 4` dans `FindSlot`) : la 5e compétence d'un allié ne serait jamais jouée. Remplacer ces plafonds par `ActiveSkillCount(u)` et ajouter un test où la 5e compétence (la 3e carte) est utilisée par l'IA et par une commande de joueur. Vérifier aussi `CombatSimulation.Actions.cs`, `CombatPresenter.cs` (indices de compétence) avec `grep -n "< 4\|<= 3" `.
- [ ] **Étape 5 : tests Edit Mode** verts. **Commit** : `feat(domaine): trois cartes équipables, équipement libre, refus du doublon`.

---

### Tâche 7 : Bonus de position et de famille

**Fichiers :** créer `Definitions/CardBonuses.cs` ; modifier `UnitFactory.cs` ; test `Assets/Tests/Domain/CardBonusTests.cs`.

**Interfaces — produit :**
```csharp
public static class CardBonuses
{
    public const int PositionPercent = 15, FamilyPercent = 5;
    /// <summary>Total en % pour une carte portée par un agent : +15 si le type est boosté par sa position, +5 si même famille.</summary>
    public static int Percent(SkillCardDef card, MemberDef member);
}
```
`UnitFactory.SkillsWithCards` applique `CardLevels.ScalePercent(skill, 100 + Percent)` à chaque carte d'un allié, **après** le palier de la carte (`CardLevels.SkillAt`) et **avant** `EmpoweredSkills`. Les ennemis n'utilisent pas ce chemin (tests).

- [ ] **Étape 1 : tests.**
  - `Percent_vaut_20_pour_un_type_booste_de_la_meme_famille` ; `15` type booste/autre famille ; `5` autre type/même famille ; `0` ni l'un ni l'autre.
  - `Un_Ranged_booste_Offensive_et_Control_mais_pas_Defensive`.
  - `Un_attaquant_avec_une_carte_offensive_inflige_15_pourcent_de_plus` : dégâts de la carte au niveau 1 (sans palier) 120 → 138 pour une carte hors famille (via `UnitFactory.Ally`).
  - `Un_ennemi_ne_recoit_pas_le_bonus` (Revue 5).
  - `Le_bonus_ne_depasse_jamais_20_pourcent`.
- [ ] **Étape 2 : implémenter** `CardBonuses.Percent` (appuyé sur `Positions.BoostedTypes`) et l'appel dans `SkillsWithCards`.
- [ ] **Étape 3 : tests Edit Mode** verts. **Commit** : `feat(domaine): bonus de position (+15 %) et de famille (+5 %) sur les cartes`.

---

### Tâche 8 : Migration de sauvegarde et contenu JSON ancien

**Fichiers :** tests `Assets/Tests/Application/SaveMigrationTests.cs` (nouveau) ; corrections éventuelles dans `GameSession.cs` / `ContentDataConverter.cs`.

- [ ] **Étape 1 : tests** (revue 1 à 3).
  - `Une_sauvegarde_0_15_0_se_charge_avec_les_cartes_conservees` : fichier de sauvegarde type avec `cards: ["card_ember", "card_regen_field"]` sur un agent → deux cartes équipées, troisième emplacement vide.
  - `Le_chargement_est_idempotent` (charger, sauvegarder, recharger : même résultat).
  - `Un_agent_json_sans_position_ni_rang_se_charge_en_Premiere_ligne_rang_1`.
  - `Les_agents_obtenus_gardent_leur_progression` (niveau, EXP, fatigue inchangés).
- [ ] **Étape 2 : corriger** ce qui échoue (valeurs par défaut dans `ContentDataConverter` : `position` absent = 0, `rank` absent = 1).
- [ ] **Étape 3 : tests Edit Mode** verts. **Commit** : `test(application): migration des sauvegardes 0.15.0 vers positions, rangs et trois cartes`.

---

### Tâche 9 : Documentation, décisions et vérification finale

**Fichiers :** `01_Game_Design/02_Combat.md`, `03_Metahumains.md`, `04_Economie.md`, `06_Kits_Competences.md`, `02_Lore_Narration/04_Agents_The_Dawn.md`, `05_Production/Decisions.md`, `Code/docs/Guide_Outils_de_design.md` si les fenêtres d'outils citent « rôle » ou « rareté ».

- [ ] **Étape 1 : mettre à jour** les documents de la spec §6 : rôles → positions, rangs AMIF (libellés et multiplicateurs), table des rangs des agents, types de cartes à 4 valeurs, étoiles des 48 cartes, 3 emplacements, bonus de position et de famille, taux des boosters. Le rang de chaque agent apparaît dans sa fiche de `04_Agents_The_Dawn.md`.
- [ ] **Étape 2 : `Decisions.md`** — ajouter à la fin du tableau, sans toucher aux modifications existantes non commitées (les relire avant) : une ligne par décision de la spec (position remplace le rôle ; rang AMIF 1 à 5 ; famille en bonus secondaire ; 4 types de cartes ; 5 étoiles et répartition ; 3 cartes + 2 fixes, équipement libre ; bonus +15 % / +5 %).
- [ ] **Étape 3 : vérification.** Lancer tous les tests Edit Mode : attendu aucun échec nouveau par rapport à la tâche 1. Ouvrir la sortie de compilation (`$TEMP/unity.log`) : aucune erreur `CS`. Ne pas lancer les tests Play Mode ni de captures (les écrans appartiennent à la session UI).
- [ ] **Étape 4 : relire** `git diff --stat` : aucun fichier de `Editor/Prefabs`, `Prefabs/`, `Scenes/` n'est touché par ce plan.
- [ ] **Étape 5 : commit** — `docs(conception): rangs, positions, types et étoiles de cartes` (n'ajouter que les documents de l'étape 1 ; pour `Decisions.md`, utiliser `git add -p` afin de ne pas embarquer les lignes d'autres chantiers).

---

## Lot UI à prévoir après la fusion de la refonte (hors plan)

5e place de l'éventail de combat, badges de rang, de position et d'étoiles, 3e emplacement du grimoire, filtre à 4 types, indicateur de bonus, libellés `rank.*`, `position.*`, `card.nature.control`.

## Auto-relecture

- Couverture de la spec : §1 (tâches 2, 3), §2 (4, 5, 6, 7), §3 (2 à 6), §4 (8), §5 (hors plan, signalé), §6 (9), §7 (tests dans chaque tâche).
- Écart avec la spec : elle citait `cards.json` ; les cartes sont écrites dans `PrototypeCollection.cs` (pas de fichier JSON). `PowerTier` existait déjà (1 à 5) et est **remplacé** par le rang (la table validée prévaut sur l'ancienne, par exemple Eero Nieminen passe de 5 à 4 et Agent D de 4 à 5).
- Les 10 cryptides ne sont pas encore dans le jeu : leurs rangs seront saisis à leur création (acte IV).
