# Moscou (chapitre Sergueiv) : Agent M jouable — plan d'implémentation

> **Pour les agents :** exécuter tâche par tâche avec `superpowers:executing-plans` ou `superpowers:subagent-driven-development`. Les cases `- [ ]` suivent l'avancement. **Ne démarre aucune tâche « Unity » tant que l'éditeur est pris par une autre session** (une seule instance peut ouvrir le projet) ; les tâches marquées « sans Unity » peuvent se faire avant.

**Objectif :** livrer 4 zones de Moscou (chapitre Sergueiv, boss le Passeur) pour rendre le parcours d'Agent M jouable dans la 1.0.0, rc1 le 22/01/2027.

**Architecture :** les zones, ennemis et textes suivent le modèle de Brooklyn (code du prototype dans `Content/Prototype`, import en assets par `ZoneBaker`, textes dans les classes `Loc*`). Deux branchements de code sont nécessaires : reconnaître les quartiers de Moscou comme Moscou, et n'ouvrir que les zones de la ville de l'acte courant du parcours. `StoryRoutes.M` ne passe à `available: true` qu'en dernière étape.

**Stack :** Unity 6.6 (6000.6.3f1), C#, NUnit (EditMode et PlayMode), outil de level design (`Heroes Dawn ▸ Level Design`), simulateur `BalanceFullReport`.

**Spec :** `Code/docs/specs/2026-11-13-moscou-agent-m-design.md` (à lire en entier avant la tâche 1).

## Contraintes globales

- Racine du projet Unity : `Code/Heroes Dawn Unity/` ; les chemins ci-dessous sont relatifs à elle. Domaine : `Packages/com.n29.heroesdawn.domain/Definitions/`. Code de jeu : `Assets/_Project/`. Tests : `Assets/Tests/`.
- « Aube tactique » n'apparaît jamais dans le jeu. R7 (thèmes sensibles d'Eda) et R8 (« cryptides », jamais « SCP »).
- Tout texte a une version **FR et EN** (`LocEnglish.cs`). Une clé manquante ne doit jamais s'afficher telle quelle.
- Aucune zone AMIF à l'écran avant la Place Rouge (lore : alliance dans l'ombre).
- Les profils Agent D existants se chargent et se jouent comme avant (non-régression obligatoire).
- Commits en français (`feat:`, `test:`, `docs:`), jamais de push, un commit par tâche.
- Aucune migration Supabase.

## Revue : cas à ne pas oublier

- Un profil Agent D existant ne doit voir aucune zone de Moscou ouverte et garder son `Recommended()` sur Brooklyn.
- Un nouveau profil Agent M ne doit voir aucune zone de Brooklyn ouverte (carte, QG, repaire de l'AMIF, journal de quêtes).
- Après le Passeur, l'écran de l'acte courant ne doit pas planter ni proposer Brooklyn (Paris sans zone).
- Le quartier `sergueiv` ne doit jamais retomber sur New York (défaut de `CityOfBorough`).
- Deux zones ne partagent jamais le même `Code` d'affichage.

---

## Estimation

| Tâche | Contenu | Jours | Unity |
|---|---|---|---|
| 1 | Branchement par parcours | 1,5 | oui |
| 2 | Ennemis (4) | 1,5 | oui |
| 3 | Données des 4 zones | 2 | oui |
| 4 | Textes FR/EN (dialogues, zones, objectifs) | 2 | non, puis câblage oui |
| 5 | Fragments, archives, fil du QG | 1 | partiel |
| 6 | Carte de Moscou | 2 | oui |
| 7 | Import, validation des plateaux, fin d'acte | 1,5 | oui |
| 8 | Équilibrage du Passeur et des élites | 3 | oui |
| 9 | Playtest et corrections | 2 | oui |
| 10 | Relecture EN, bascule, notes | 1 | oui |
| **Total** | | **17,5** (≈ 3,5 semaines, + 10 % de marge) | |

Calendrier : voir §11 de la spec ; gel du contenu le 15/01/2027, rc1 le 22/01/2027.

---

### Task 1: Branchement par parcours (quartiers de Moscou et ouverture selon l'acte)

**Files:**
- Modify: `Packages/com.n29.heroesdawn.domain/Definitions/StoryRoute.cs` (`CityOfBorough`)
- Modify: `Assets/_Project/Application/Features/World/ZoneProgress.cs`, `Assets/_Project/Application/Features/World/StoryProgress.cs`
- Test: `Assets/Tests/Domain/StoryRouteTests.cs`, créer `Assets/Tests/Domain/Application/ZoneProgressRouteTests.cs`

**Interfaces:**
- Produces: `StoryRoutes.CityOfBorough("sergueiv"|"kapotnya"|"sadovoye"|"place_rouge") == StoryRoutes.Moscow` ; `ZoneProgress.RawStatusOf(ISessionReadModel, ZoneDef) -> ZoneStatus` (prérequis seuls, ancien comportement) ; `ZoneProgress.StatusOf` renvoie `Locked` si la ville de la zone n'est pas celle de `StoryProgress.Current(session).City` et que la zone n'est pas `Done`.
- Consumes: `StoryProgress.IsActDone` doit utiliser `RawStatusOf` (sinon récursion).

- [ ] **Step 1: Tests (rouges)**
  - `StoryRouteTests.CityOfBorough_MoscowDistricts_AreMoscow` : les 4 quartiers → `"moscow"` ; `"brooklyn"` → `"new_york"`.
  - `ZoneProgressRouteTests.AgentD_NewProfile_MoscowZoneIsLocked_BrooklynIsAvailable`.
  - `ZoneProgressRouteTests.AgentM_NewProfile_BrooklynZoneIsLocked_MoscowZoneIsAvailable` (utilise un contenu de test à une zone de Moscou, sans dépendre de la tâche 3).
  - `ZoneProgressRouteTests.DoneZone_StaysDone_WhateverTheAct`.
  - `ZoneProgressRouteTests.Recommended_FollowsTheRoute` (D → zone de Brooklyn, M → zone de Moscou).
- [ ] **Step 2: Lancer** `build/run-tests.sh` en EditMode, filtre `ZoneProgressRouteTests|StoryRouteTests` ; attendu : ÉCHEC.
- [ ] **Step 3: Implémenter** les quatre cases de `CityOfBorough`, `RawStatusOf`, le filtrage dans `StatusOf`, et `IsActDone` sur `RawStatusOf`. Garder `Recommended()` sur `StatusOf` (les zones fermées sont ignorées).
- [ ] **Step 4: Vérifier la portée** : chercher les autres appelants de `StatusOf`/`ZoneProgress` (`QuestLog`, `QgRoomLocks`, `CityMapPagePresenter`, `ArchiveBook`, `PlaytestFeedback`, `ChapterBook`) et vérifier à la main qu'aucun n'attend « ouvert quel que soit l'acte » ; noter dans le commit ceux qui changent de comportement. Vérifier aussi si `QuestLog.Main()` dérive des zones (si oui, ajouter les quêtes de Moscou en tâche 5) et si `ZoneDef.Code` est utilisé comme clé de recherche (justifie le préfixe `M`).
- [ ] **Step 5: Relancer** les tests ciblés, puis tout l'EditMode : aucun nouvel échec par rapport à la baseline. Commit : `feat: ouverture des zones selon l'acte du parcours`.

### Task 2: Ennemis de Moscou

**Files:**
- Create: `Assets/_Project/Content/Prototype/PrototypeMoscowEnemies.cs`
- Modify: `Assets/_Project/Content/Prototype/PrototypeContent.cs` (enregistrer les ennemis, comme `PrototypeZones.Enemies()`)
- Test: créer `Assets/Tests/Domain/MoscowEnemyTests.cs`

**Interfaces:**
- Produces: `PrototypeMoscowEnemies.All() -> IEnumerable<EnemyDef>` avec `mos_guard`, `mos_smuggler`, `mos_enforcer`, `mos_passeur` (statistiques et compétences du §5 de la spec) ; le Passeur a deux phases (66 % et 33 %) via `Phased`/`Phase`, style `AiStyle.Control`.

- [ ] **Step 1: Tests (rouges)** : `MoscowEnemyTests.AllFourExist_AndOnlyThePasseurIsBoss` ; `Passeur_HasTwoPhases_At66And33` ; `EveryMoscowEnemy_HasNameAndSkillTexts_InFrAndEn` (clés `enemies.<id>.name`, `skills.<id>_aN`, `enemies.mos_passeur.phase2/3`).
- [ ] **Step 2: Lancer** ; attendu : ÉCHEC.
- [ ] **Step 3: Implémenter** les quatre `EnemyDef` avec les helpers de `PrototypeZones` (`E`, `S`, `Dmg`, `St`, `Shield`, `Phased`, `Phase`, `Styled`) — extraire ces helpers dans une classe interne partagée plutôt que de les copier (DRY). Valeurs de départ = spec §5.
- [ ] **Step 4: Ajouter les textes FR** (`Assets/_Project/Presentation/Common/LocMoscow.cs`, nouveau, enregistré comme `LocChapters`) et **EN** (`LocEnglish.cs`) des noms, compétences, phases et `archives.enemy.<id>`.
- [ ] **Step 5: Tests verts**, commit : `feat: ennemis de Moscou (garde, contrebandier, homme de main, le Passeur)`.

### Task 3: Données des quatre zones

**Files:**
- Create: `Assets/_Project/Content/Prototype/PrototypeMoscowZones.cs`
- Modify: `Assets/_Project/Content/Prototype/PrototypeContent.cs` (ajouter les zones et leurs prérequis), `Packages/.../Definitions/StoryRoute.cs` non touché ici
- Test: créer `Assets/Tests/Presentation/MoscowCampaignTests.cs`

**Interfaces:**
- Produces: `PrototypeMoscowZones.Zone11..Zone14` (`ZoneId("zone_mos_1_1")`…), `Zones() -> IEnumerable<ZoneDef>`, `Requires(ZoneId)` (linéaire, 1.1 sans prérequis). Graphes A (20 cases) et B (19 cases) repris de `PrototypeZones.BuildZone12` / `BridgeNodes`, mini-jeux, objectifs et rencontres du §4 de la spec. `ZoneDef` : code `M1.1`…`M1.4`, acte 1, quartier `sergueiv`, environnement `Biome.City`.

- [ ] **Step 1: Tests (rouges)**, miroirs de `BrooklynCampaignTests` : `FourZones_InOrder_Linear` (quartier `sergueiv`, codes `M1.1`…`M1.4`, déblocage linéaire) ; `Objectives_MatchTheSpec` (types dans l'ordre : M1.1 ReachCase, CompleteMiniGame, DefeatCombat, ReachCase ; M1.2 ReachCase, CompleteMiniGame, DefeatCombat, ReachCase ; M1.3 CompleteMiniGame, DefeatCombat, ReachCase ; M1.4 CompleteMiniGame, DefeatCombat, DefeatCombat) ; `OnlyM14_HasTheBoss_Passeur` ; `NoAmifEnemy_InMoscowEncounters` ; `MiniGames_AreTheExistingOnes` (`infiltration_camp`, `gears_workshop`, `rooftop_rescue`, `crate_pick_depot`) ; `NoCodeCollision_WithBrooklyn`.
- [ ] **Step 2: Lancer** ; attendu : ÉCHEC.
- [ ] **Step 3: Implémenter** les quatre `Build*` en réutilisant les constructeurs de graphe de Brooklyn (extraire `BridgeNodes`/`BridgeLinks`/`N`/`Links`/`L` en helper partagé plutôt que dupliquer).
- [ ] **Step 4: Tests verts**, commit : `feat: quatre zones du chapitre Sergueiv (données)`.

### Task 4: Dialogues, noms et objectifs (FR/EN)

**Files:**
- Modify: `Assets/_Project/Presentation/Common/LocMoscow.cs` (FR), `LocEnglish.cs` (EN)
- Modify: `PrototypeMoscowZones.cs` (rattacher les `NarrationDef` : M1.1 n14 et n19 ; M1.2 n14 et n18 ; M1.3 n14 et n18 ; M1.4 n18)
- Test: étendre `MoscowCampaignTests`

**Interfaces:**
- Produces: clés `zones.mos_1_x.name|short|code|desc`, `objectives.mos1x.*`, `narration.mos_1_x.<nœud>.before|after.i`, `speaker.agent_m`, `speaker.passeur`, `speaker.enforcer`, `qg.fil.zone_mos_1_x`. Répliques : annexe A de la spec, copiées verbatim.

- [ ] **Step 1: Tests (rouges)** : `EveryTextOfTheCampaign_Exists` (copie adaptée de Brooklyn, clés FR) ; `EveryTextOfTheCampaign_ExistsInEnglish` ; `EverySpeaker_HasADisplayName` (`agent_m`, `ilona_varga`, `passeur`, `enforcer`) ; `NoText_ContainsAubeTactique`.
- [ ] **Step 2: Lancer** ; attendu : ÉCHEC.
- [ ] **Step 3: Rédiger** (peut se faire sans Unity, avant la tâche 3) puis câbler. Le français est écrit d'abord ; la version EN est marquée « à relire » dans le commit.
- [ ] **Step 4: Tests verts**, commit : `feat: dialogues et textes de Moscou (FR/EN)`.

### Task 5: Fragments, archives, fil du QG

**Files:**
- Modify: `Packages/com.n29.heroesdawn.domain/Definitions/MemoryFragments.cs` (4 entrées, chapitre `mos_sergueiv`)
- Modify: `LocMoscow.cs`, `LocEnglish.cs` (titres et textes : annexe B ; `archives.zone.zone_mos_1_x`)
- Test: `Assets/Tests/Domain/Application/MemoryFragmentBookTests.cs`, `MoscowCampaignTests`

**Interfaces:**
- Produces: `MemoryFragments.OfChapter("mos_sergueiv")` renvoie `mos_maglev_welcome`, `mos_monastery_register`, `mos_convoy_crates`, `mos_depot_passeur`, débloqués par `zone_mos_1_1`…`zone_mos_1_4`.

- [ ] **Step 1: Tests (rouges)** : `Fragments_OfSergueiv_AreFour_UnlockedByTheirZone` ; `EveryFragment_HasFrAndEnTitleAndText` ; `ArchivesText_ExistsForEveryMoscowZone`.
- [ ] **Step 2: Implémenter**, vérifier que la page Chapitres affiche les 4 fragments du chapitre (`ChaptersPageTests`).
- [ ] **Step 3:** si la tâche 1 a montré que `QuestLog.Main()` dérive des zones, ajouter les quêtes principales de Moscou (même barème que Brooklyn) avec leurs tests ; sinon ne rien ajouter.
- [ ] **Step 4: Tests verts**, commit : `feat: fragments et archives du chapitre Sergueiv`.

### Task 6: Carte de Moscou

**Files:**
- Modify: `Assets/_Project/Presentation/Hub/Pages/CityMap/CityMapLayout.cs` (ajouter `CityMapLayout.For(string city)` ; carte de Moscou : quartiers `sergueiv` (4 points `zone_mos_1_1`…`1_4`), `kapotnya`, `sadovoye`, `place_rouge` verrouillés « à venir »)
- Modify: `Assets/_Project/Presentation/Hub/Pages/CityMap/CityMapPagePresenter.cs` (choisir la carte selon la ville de l'acte courant), `LocMap.cs`/`LocMoscow.cs` (noms de quartiers FR/EN)
- Test: créer `Assets/Tests/Presentation/CityMapLayoutTests.cs`, étendre `CityMapScreenUiToolkitTests`

**Interfaces:**
- Produces: `CityMapLayout.For(string city) -> (IReadOnlyList<BoroughShape> Boroughs, IReadOnlyList<MapSpot> Spots)` ; `For("new_york")` renvoie les données actuelles sans changement.

- [ ] **Step 1: Tests (rouges)** : `For_NewYork_IsUnchanged` ; `For_Moscow_HasFourPlayableSpots_InSergueiv` ; `Presenter_AgentM_ShowsMoscowMap` ; `Presenter_AgentD_ShowsNewYorkMap` ; la capture 4 formats (16:9, 20:9, 4:3, 1280×720) de la carte de Moscou sans chevauchement.
- [ ] **Step 2: Implémenter**, polygones simples (anneau pour Sadovoye Koltso) ; aucune illustration de fond à la 1.0.0.
- [ ] **Step 3: Tests verts**, commit : `feat: carte de Moscou`.

### Task 7: Import, validation des plateaux, fin d'acte

**Files:**
- Modify: `Assets/_Project/Editor/LevelDesign/ZoneBaker.cs` (inclure les zones de Moscou dans `ImportPrototype`), ou équivalent
- Test: `Assets/Tests/Editor/CampaignAssetsTests.cs`, `LevelDesignToolTests.cs`, `MiniGamePlacementTests.cs`, créer `Assets/Tests/Domain/Application/MoscowActEndTests.cs`

**Interfaces:**
- Produces: `Resources/Zones/zone_mos_1_x.asset` (4) et campagne mise à jour ; état « acte I terminé » d'Agent M.

- [ ] **Step 1: Tests (rouges)** : `ZoneValidator_AcceptsTheFourMoscowZones` (aucune erreur ; avertissements listés) ; `Campaign_ContainsMoscowZones_InOrder` ; `MoscowActEndTests.AgentM_AfterM14_CurrentActIsParis_WithoutZones` ; `MoscowActEndTests.AgentM_AfterM14_NoZoneIsRecommendedOrOpen` ; `MoscowActEndTests.AgentD_Unchanged_AfterBrooklyn`.
- [ ] **Step 2: Lancer l'import** : `-executeMethod N29.HeroesDawn.EditorTools.LevelDesign.ZoneBaker.ImportPrototypeBatch -quit` ; générer les fonds (biome ville, graine fixe) ; ouvrir les 4 plateaux dans l'outil et corriger les avertissements.
- [ ] **Step 3: Vérifier à l'écran** (build de développement) : fin de la zone M1.4 → écran d'acte suivant « bientôt » sans bouton mort ; ajouter le texte de transition dans `LocMoscow.cs` si l'écran existant nomme New York.
- [ ] **Step 4: Tests verts**, commit : `feat: import des zones de Moscou et fin de l'acte I`.

### Task 8: Équilibrage du Passeur et des élites

**Files:**
- Modify: `Assets/_Project/Editor/Balance/BalanceFullReport.cs` (ajouter `mos_passeur` et l'élite `mos_enforcer`), `PrototypeMoscowEnemies.cs` (valeurs)
- Create: `Code/docs/test-reports/2026-12-XX_equilibrage-moscou.md` (date de la mesure)

- [ ] **Step 1: Mesurer** : `BalanceFullReport`, équipe de départ + objets, niveaux 5 et 10, même protocole que `docs/test-reports/2026-11-13_equilibrage-boss.md`.
- [ ] **Step 2: Régler** l'ATQ du Passeur (levier principal) jusqu'à **40-60 %** aux deux niveaux ; élites **60-85 %** ; consigner chaque essai dans le rapport.
- [ ] **Step 3: Test** : `MoscowBalanceTests.Passeur_WinRate_InTarget` (marqué `[Category("Balance")]`, graine fixe, tolérance ±5 pts) ; relancer la garde anti pay-to-win.
- [ ] **Step 4: Commit** : `feat: équilibrage du Passeur (rapport de mesure)` ; ajouter la valeur retenue dans `Decisions.md` (docs).

### Task 9: Playtest et corrections

**Files:**
- Test: créer `Assets/Tests/PlayMode/MoscowFullRunTests.cs`
- Modify: `docs/playtest-formulaire.md` (section Moscou)

- [ ] **Step 1: Test PlayMode** `MoscowFullRunTests.AgentM_NewProfile_PlaysFourZones_AndReachesTheEndOfAct` (création du profil Agent M, quatre zones, boss en mode simulation de victoire, état final).
- [ ] **Step 2: 2 sessions de playtest réel** avec le formulaire ; corriger les blocages ; gel du contenu le 15/01/2027.
- [ ] **Step 3: Commit** : `test: parcours complet d'Agent M` puis `fix:` pour les corrections.

### Task 10: Relecture EN, bascule et notes de version

**Files:**
- Modify: `Packages/.../Definitions/StoryRoute.cs` (`StoryRoutes.M` → `available: true`, commentaire mis à jour), `Assets/_Project/Application/Features/Auth/OnboardingGuide.cs`, `LocOnboarding.cs` (retrait de l'état « indisponible »)
- Modify: `Assets/Tests/Domain/Application/OnboardingTests.cs` (`OnlyAgentDIsAvailable` → `BothAgentsAreAvailable`), `Assets/Tests/Domain/StoryRouteTests.cs` (`CurrentAct_OfAgentM_StartsInMoscow` : zones créées), `Assets/Tests/Presentation/ChaptersPageTests.cs`, `Assets/Tests/Domain/Application/ChapterBookTests.cs` (Sergueiv n'est plus « bientôt » ; Kapotnya, Sadovoye, Place Rouge le restent)
- Create/Modify: `docs/release-notes-1.0.0.md` (Moscou, Agent M)

- [ ] **Step 1: Relecture EN par l'utilisateur** (annexes A et B de la spec) ; appliquer ses corrections.
- [ ] **Step 2: Mettre à jour les tests** ci-dessus (ils échouent d'abord, car ils affirment l'état précédent).
- [ ] **Step 3: Basculer** `available: true` ; relancer EditMode et PlayMode complets : 100 % verts.
- [ ] **Step 4: Cocher** la case Agent M de la phase 5 du plan de stabilisation, ajouter la ligne dans `05_Production/Decisions.md`.
- [ ] **Step 5: Commit** : `feat: Agent M jouable (bascule de la 1.0.0)`.

---

## Auto-revue

- Couverture de la spec : §3 → tâches 1 et 6 ; §4 → 3 ; §5 → 2 ; §6 → 4 et 5 ; §7 → 1, 6, 7, 10 ; §8 → 8 ; §9 → toutes ; §11 → tableau d'estimation.
- Points qui demandent une lecture du code avant de s'engager (tâche 1, étape 4) : usage de `ZoneDef.Code`, forme de `QuestLog.Main()`, appelants de `StatusOf`.
- Noms cohérents entre tâches : `RawStatusOf`, `PrototypeMoscowEnemies.All`, `PrototypeMoscowZones.Zone11..14`, `CityMapLayout.For`.
