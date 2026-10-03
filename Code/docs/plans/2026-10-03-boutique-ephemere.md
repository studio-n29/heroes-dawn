# Boutique éphémère — Plan d'implémentation

> **Pour l'exécutant :** SOUS-SKILL REQUIS : superpowers:executing-plans (exécution inline, choisie par l'utilisateur), tâche par tâche. Étapes à cocher (`- [ ]`).

**Objectif :** remplacer l'onglet Recrutement du QG par une boutique éphémère de 3 héros tirés chaque jour à 9h00 locale, pondérés par rareté, avec renouvellement gratuit puis payant.

**Architecture :** règles pures dans une nouvelle assembly `Domain.Shop` (tirage déterministe `Pcg32` sur graine joueur + jour + tirage) ; état `ShopState` dans `GameSession`, sauvegardé (profil v3) ; trois cas d'usage ; `HubPresenter` + prefabs du QG pour l'écran.

**Technique :** Unity 6.6, C# (`#nullable enable`), VContainer, MessagePipe, UniTask, NUnit (EditMode/PlayMode), recettes de prefabs.

**Spec :** `Code/docs/specs/2026-10-03-boutique-ephemere-design.md` (fait autorité).

Racine : la copie de travail `C:/Users/dnath/Documents/Heroes Dawn - boutique` (branche `feature/boutique-ephemere`). Abréviations : `D/` = `Code/Heroes Dawn Unity/Packages/com.n29.heroesdawn.domain/`, `A/` = `Code/Heroes Dawn Unity/Assets/`.

## Contraintes globales
- 9h00 **heure locale** ; poids **Agent 70 / Élite 25 / Légende 5** ; renouvellement : 1er du jour gratuit, puis **100 / 200 / 400 / 800** Crédits (plafond 800) ; prix d'achat inchangés `MetaRules.RecruitCost` (150 / 450 / 1 200 PR) ; recrue achetée au niveau `MetaRules.RecruitLevel` (5).
- Graine : `Fnv1a64` de `playerId`, `day` (`yyyy-MM-dd`), `draw` → `Pcg32`. Offres **mémorisées** ; un achat ou une relance ne relance jamais le tirage.
- Condition d'entrée : `MemberDef.UnlockZone` ; au M1 `verre` ← `zone_ny_1_1` validée.
- Domaine sans Unity (`noEngineReferences: true`), garde-fou `DependencyGuardTests`.
- Un cas d'usage par intention, `UniTask<Result>` ; erreurs `DomainError` localisées ; textes dans `Fr` de `A/_Project/Presentation/Common/Loc.cs`.
- Vues MVP : la vue n'expose que `Show…(…Data)` et des événements ; présentateur C# pur ; prefabs générés par recettes.
- Chaque test porte `[Description("Vérifie que … — parce que …")]`.
- Ne jamais modifier ni commiter `00_GDD/*`, `03_Art_UI_UX/*`, `01_Game_Design/05_Equilibrage.xlsx`, `Code/_deploy/`.
- Commandes (Unity fermé sur la copie boutique) : `W=.superpowers/sdd/2026-10-03-boutique-ephemere` ; tests `$W/run-tests.sh EditMode|PlayMode [filtre]` ; génération `$W/gen.sh [-overwritePrefabs]`. Point de départ : EditMode 189/189, PlayMode 17/17.
- Rapport de tests en chat après chaque série (totaux, ce que la série prouve, échecs), `latest-*.html` commités avec la tâche.

## Points d'attention (Review Focus)
1. **9h00 passe pendant la confirmation de renouvellement** : le renouvellement est refusé (`Expired`), aucun Crédit débité, la nouvelle boutique s'affiche. Test tâche 2 (`Reroll_AfterDayChange_RefusedNoCharge`).
2. **Offre sauvegardée qui n'existe plus dans le contenu** (recrue retirée) : l'offre est ignorée au chargement, sans erreur. Test tâche 2 (`ApplyProfile_UnknownOfferId_Dropped`).
3. **Horloge reculée** (jour sauvegardé postérieur à aujourd'hui) : tout jour différent = nouveau tirage, jamais d'exception. Test tâche 2 (`Open_DayEarlierThanSaved_Redraws`).
4. **Renouvellement après un achat** : le héros acheté n'est plus jamais proposé. Test tâche 2 (`Reroll_AfterRecruit_ExcludesOwned`).
5. **Recrutement pendant une partie en cours** : la recrue rejoint l'équipe hors escouade, comme aujourd'hui. Test tâche 2 (`Recruit_DuringRun_NotInSquad`).

---

## Carte des fichiers
| Fichier | Rôle |
|---|---|
| `D/Domain/Shop/N29.HeroesDawn.Domain.Shop.asmdef` | Assembly (Core, Definitions ; `noEngineReferences`) |
| `D/Domain/Shop/ShopDay.cs` | Jour de boutique, prochain renouvellement |
| `D/Domain/Shop/ShopSeed.cs` | Graine du tirage |
| `D/Domain/Shop/ShopWeights.cs` | Poids par rareté |
| `D/Domain/Shop/ShopDraw.cs` | Éligibles et tirage |
| `D/Domain/Shop/ShopRules.cs` | Coût du renouvellement |
| `D/Definitions/MemberDef.cs`, `BalanceConfig.cs` | `UnlockZone` ; paramètres de boutique |
| `A/_Project/Application/Session/ShopState.cs` | `ShopState`, `ShopOffer`, `IShopReadModel` |
| `A/_Project/Application/Features/Shop/*.cs` | 3 cas d'usage, interfaces, `ShopErrors` |
| `A/_Project/Presentation/Hub/*` | Données, vue, carte, présentateur |
| `A/_Project/Editor/Prefabs/Recipes/HubRecipes.cs` | Onglet boutique, carte de recrue |
| `A/Tests/Domain/ShopRulesTests.cs`, `A/Tests/Domain/Application/ShopUseCaseTests.cs`, `A/Tests/Presentation/HubPresenterTests.cs`, `A/Tests/PlayMode/ShopFlowTests.cs` | Tests |

---

### Tâche 1 : Règles de la boutique (domaine)

**Fichiers :** créer les 6 fichiers `D/Domain/Shop/*` ; modifier `D/Definitions/MemberDef.cs`, `D/Definitions/BalanceConfig.cs`, `A/_Project/Content/Prototype/PrototypeContent.cs`, `A/Tests/Domain/N29.HeroesDawn.Tests.Domain.asmdef` (+ `N29.HeroesDawn.Domain.Shop`) ; créer `A/Tests/Domain/ShopRulesTests.cs`.

**Interfaces :**
- Produit (namespace `N29.HeroesDawn.Domain.Shop`) :
  - `static class ShopDay { string Of(DateTime local, int resetHour); DateTime NextReset(DateTime local, int resetHour); }`
  - `static class ShopSeed { ulong For(string playerId, string day, int draw); }`
  - `readonly struct ShopWeights(int agent, int elite, int legend) { int For(Rarity r); static ShopWeights From(BalanceConfig b); }`
  - `static class ShopDraw { List<MemberDef> Eligible(IReadOnlyList<MemberDef> members, Func<MemberId,bool> owned, Func<ZoneId,bool> zoneCompleted); List<MemberId> Draw(IRandom rng, IReadOnlyList<MemberDef> eligible, ShopWeights w, int slots = 3); }`
  - `static class ShopRules { int RerollCost(int rerollsToday, BalanceConfig b); }`
- `MemberDef` : nouveau paramètre de constructeur optionnel `ZoneId? unlockZone = null`, propriété `ZoneId? UnlockZone`.
- `BalanceConfig` (int, propriétés publiques lues par `BalanceJson`) : `ShopResetHour = 9`, `ShopWeightAgent = 70`, `ShopWeightElite = 25`, `ShopWeightLegend = 5`, `ShopRerollCost1 = 100`, `ShopRerollCost2 = 200`, `ShopRerollCost3 = 400`, `ShopRerollCost4 = 800`.

- [ ] **Étape 1 : tests en échec** — `ShopRulesTests` :
  - `Day_At0859_IsPreviousDay` : `ShopDay.Of(new DateTime(2026,10,3,8,59,0), 9)` == `"2026-10-02"` ; `Day_At0900_IsSameDay` : 9h00 → `"2026-10-03"`.
  - `Day_DstSpringForward_ResetStaysAt9` : `Of(2026-03-29 08:59)` == `"2026-03-28"`, `Of(2026-03-29 09:00)` == `"2026-03-29"`.
  - `NextReset_Before9_IsToday_After9_IsTomorrow` : 08:00 → 2026-10-03 09:00 ; 10:00 → 2026-10-04 09:00.
  - `Seed_SameInputs_Same_DifferentDayOrDraw_Different`.
  - `Draw_SameSeed_SameOffers` (deux `Pcg32` de même graine sur un contenu fictif → listes égales).
  - `Draw_NoDuplicates_NeverOwned` (1 000 graines, `Eligible` avec 2 possédés).
  - `Draw_ExhaustedRarity_WeightsRenormalized` : contenu sans Légende éligible → aucune Légende, 3 offres.
  - `Eligible_UnlockZone_RequiresCompletion` : `verre` (contenu prototype) absent sans `zone_ny_1_1`, présent avec.
  - `Draw_FewerThanThreeEligible_ReturnsFewer` (2 éligibles → 2) ; `Draw_NoEligible_ReturnsEmpty`.
  - `Draw_Distribution_Matches70_25_5` : contenu fictif 5 Agents / 5 Élites / 5 Légendes, 10 000 tirages d'**un** emplacement (`slots: 1`) → parts à ±2 points de 70 / 25 / 5.
  - `RerollCost_FreeThen100_200_400_800_Capped` : `rerollsToday` 0..5 → 0, 100, 200, 400, 800, 800.
- [ ] **Étape 2 :** `$W/run-tests.sh EditMode N29.HeroesDawn.Tests.Domain.ShopRulesTests` → attendu : échec de compilation (types absents).
- [ ] **Étape 3 :** implémenter. Tirage (non déterminé par les signatures) :
  ```
  candidats = copie de eligible (ordre du contenu)
  répéter slots fois tant que candidats non vide :
    total = Σ w.For(r) pour les raretés r présentes dans candidats
    x = rng.Range(0, total) ; parcourir Agent, Elite, Legend (raretés présentes) en soustrayant les poids → rareté choisie
    parmi candidats de cette rareté : i = rng.Range(0, n) ; ajouter, retirer des candidats
  ```
  Graine : `Fnv1a64` + `Add(playerId)`, `Add(day)`, `Add(draw)`. `Of` : `local.AddHours(-resetHour).ToString("yyyy-MM-dd", InvariantCulture)`. Verre : `UnlockZone = PrototypeContent.Zone11`.
- [ ] **Étape 4 :** relancer le filtre → toutes vertes ; puis `$W/run-tests.sh EditMode` → 189 + nouveaux, 0 échec (dont `DependencyGuardTests`, `BalanceJsonTests`).
- [ ] **Étape 5 : commit** — `feat(shop): règles de la boutique éphémère (domaine)`.

### Tâche 2 : État, sauvegarde et cas d'usage (application)

**Fichiers :** modifier `A/_Project/Application/Ports/Ports.cs` (`IClock.LocalNow`), `A/_Project/Infrastructure/Platform/PlatformServices.cs` (`SystemClock.LocalNow => DateTime.Now`), `A/_Project/Application/Session/GameSession.cs`, `ISessionReadModel.cs`, `A/_Project/Application/Messages/*` (`ShopChanged`), `A/_Project/Application/N29.HeroesDawn.Application.asmdef` (+ `Domain.Shop`), `A/_Project/Composition/RootLifetimeScope.cs`, `A/Tests/Domain/Application/TestApp.cs` ; créer `ShopState.cs`, `A/_Project/Application/Features/Shop/{IOpenShopUseCase,OpenShopUseCase,IRecruitFromShopUseCase,RecruitFromShopUseCase,IRerollShopUseCase,RerollShopUseCase,ShopErrors}.cs`, `A/Tests/Domain/Application/ShopUseCaseTests.cs`.

**Interfaces :**
- Consomme : tâche 1.
- Produit :
  - `sealed class ShopOffer { MemberId Id; bool Recruited; }` ; `interface IShopReadModel { string Day { get; } int Draw { get; } IReadOnlyList<ShopOffer> Offers { get; } }` ; `sealed class ShopState : IShopReadModel` (setters internes à l'application) ; `GameSession.Shop` (`ShopState`, vide : `Day = ""`), `ISessionReadModel.Shop` (`IShopReadModel`).
  - `ProfileData.schemaVersion = 3`, `public ShopSave? shop` ; `[Serializable] sealed class ShopSave { string day = ""; int draw; List<string> offers = new(); List<bool> recruited = new(); }` (dans `Ports.cs`). `ApplyProfile` restaure (ids inconnus ignorés), `ToProfile` écrit.
  - `readonly struct ShopChanged` (sans champ), enregistré dans `RootLifetimeScope` et `TestApp`.
  - `ShopErrors.Expired ("shop.expired", "errors.shop.expired")`, `UnknownSlot ("shop.slot", "errors.shop.slot")`, `AlreadyRecruited ("shop.recruited", "errors.shop.recruited")` ; réutilise `HubErrors.NotEnoughPr`, `HubErrors.NotEnoughCredits`.
  - `IOpenShopUseCase { UniTask<Result> ExecuteAsync(CancellationToken ct = default); }`, `IRecruitFromShopUseCase { UniTask<Result> ExecuteAsync(int slot, CancellationToken ct = default); }`, `IRerollShopUseCase { UniTask<Result> ExecuteAsync(CancellationToken ct = default); }`.
  - `TestApp.Clock` gagne `DateTime LocalNow { get; set; } = new DateTime(2026, 9, 30, 12, 0, 0)` ; `TestApp.Shop` expose `Open`, `Recruit`, `Reroll`.
- Ruling de plan : un seul compteur `Draw` (= nombre de renouvellements du jour) au lieu de `FreeRerollsUsed` + `PaidRerolls` de la spec ; coût = `ShopRules.RerollCost(Draw, balance)`. Même comportement, un champ de moins.

- [ ] **Étape 1 : tests en échec** — `ShopUseCaseTests` (horloge `TestApp.Clock`) :
  - `Open_FirstTime_DrawsUpToThreeOffers_PublishesShopChanged` ; `Open_SameDay_KeepsOffers` ; `Open_After9_NewDraw_ResetsDraw` (Draw remis à 0) ; `Open_DayEarlierThanSaved_Redraws` (Review Focus 3).
  - `Recruit_Succeeds_DebitsPr_AddsLevel5_MarksRecruited_KeepsOtherOffers` ; `Recruit_NotEnoughPr_Fails` (PR à 0) ; `Recruit_AlreadyRecruited_Fails` ; `Recruit_UnknownSlot_Fails` (slot 7) ; `Recruit_AfterDayChange_ExpiredAndReopened` ; `Recruit_DuringRun_NotInSquad` (Review Focus 5).
  - `Reroll_FirstFree_ThenCosts100` ; `Reroll_NotEnoughCredits_Fails_NoChange` ; `Reroll_AfterDayChange_RefusedNoCharge` (Review Focus 1) ; `Reroll_AfterRecruit_ExcludesOwned` (Review Focus 4).
  - `Save_ThenApply_RestoresSameShop` ; `ApplyProfile_V2WithoutShop_OpensFreshShop` ; `ApplyProfile_UnknownOfferId_Dropped` (Review Focus 2).
- [ ] **Étape 2 :** `$W/run-tests.sh EditMode N29.HeroesDawn.Tests.Domain.Application.ShopUseCaseTests` → attendu : échec de compilation.
- [ ] **Étape 3 :** implémenter. Ouverture : `day = ShopDay.Of(clock.LocalNow, b.ShopResetHour)` ; si `day != Shop.Day` → `Shop.Day = day`, `Draw = 0`, tirage `ShopDraw.Draw(new Pcg32(ShopSeed.For(account.PlayerId, day, 0)), Eligible(...), ShopWeights.From(b))`, sauvegarde, `ShopChanged`. Recruter / Renouveler appellent d'abord la même vérification de jour : si le jour a changé → réouverture puis `ShopErrors.Expired`. Recrue ajoutée comme l'actuel `RecruitMemberUseCase` (escouade si place et `Run == null`). `RecruitMemberUseCase` reste en place jusqu'à la tâche 3.
- [ ] **Étape 4 :** filtre vert, puis `$W/run-tests.sh EditMode` → 0 échec.
- [ ] **Étape 5 : commit** — `feat(shop): état, sauvegarde v3 et cas d'usage de la boutique`.

### Tâche 3 : Onglet Boutique éphémère (présentation, prefabs)

**Fichiers :** modifier `A/_Project/Presentation/Hub/{HubData,IHubView,HubView,RecruitCardView,HubPresenter}.cs`, `A/_Project/Presentation/Common/Loc.cs`, `A/_Project/Editor/Prefabs/Recipes/HubRecipes.cs`, `A/Tests/Presentation/Fakes/FakeHubView.cs` (ou fichier du double existant), `A/Tests/Presentation/HubPresenterTests.cs`, `A/Tests/PlayMode/FullRunTests.cs` ; créer `A/Tests/PlayMode/ShopFlowTests.cs` ; supprimer `RecruitMemberUseCase`, `IRecruitMemberUseCase`, `GameSession.AvailableRecruits()` / `ISessionReadModel.AvailableRecruits()` et leurs tests et enregistrements ; régénérer `Prefabs/Screens/Hub.prefab` et `Prefabs/Widgets/RecruitCard.prefab`.

**Interfaces :**
- Consomme : tâche 2 (`IOpenShopUseCase`, `IRecruitFromShopUseCase`, `IRerollShopUseCase`, `ISubscriber<ShopChanged>`, `IShopReadModel`, `IClock`).
- Produit :
  - `RecruitCardData` gagne `int Slot`, `bool Recruited`, `string StampText` ; `HubData` gagne `string ShopTitle`, `string ShopCountdown`, `string RerollLabel`, `bool RerollEnabled`.
  - `IHubView` : `event Action<int>? RecruitClicked` (emplacement, remplace `Action<MemberId>`), `event Action? RerollClicked`.
  - `RecruitCardView` : état Recruté = `CanvasGroup` à 0,45, tampon « Recruté » visible, bouton masqué ; bande de rareté `rarityBand` (Image) colorée par `Theme` selon `Rarity` ; `Button RecruitButton` conservé.
  - `HubView` : en-tête boutique (titre, compte à rebours, bouton Renouveler, visibles sur l'onglet boutique) ; `Button RerollButton` (tests PlayMode).
  - `HubPresenter` devient aussi `ITickable` : toutes les secondes au plus, si la minute de `IClock.LocalNow` a changé et que l'onglet est affiché → `IOpenShopUseCase` puis rafraîchissement.
  - Textes : `hub.tab.recruit` = « BOUTIQUE », `shop.title` = « Boutique éphémère », `shop.countdown.h` = « Nouveaux héros dans {0} h {1:00} », `shop.countdown.m` = « Nouveaux héros dans {0} min », `shop.reroll.free` = « RENOUVELER · GRATUIT », `shop.reroll.cost` = « RENOUVELER · {0} CR », `shop.reroll.confirm.title` = « Renouveler la boutique ? », `shop.reroll.confirm.body` = « 3 nouveaux héros pour {0} Crédits. », `shop.recruited` = « RECRUTÉ », `shop.empty` = « Collection de New York complète ! », `errors.shop.expired` = « La boutique s'est renouvelée. », `errors.shop.slot` = « Offre introuvable. », `errors.shop.recruited` = « Héros déjà recruté. » ; `hub.recruit.hint` supprimé.
- [ ] **Étape 1 : tests en échec** — `HubPresenterTests` (horloge `TestApp.Clock`) :
  - `ShopTab_ShowsOffers_WithCountdown` (LocalNow 2026-10-03 12:00 → « Nouveaux héros dans 21 h 00 ») ; `Countdown_UnderOneHour_ShowsMinutes` (08:48 → « dans 12 min ») ;
  - `RecruitedOffer_ShownAsRecruited` ; `NoEligible_ShowsEmptyText` ;
  - `Reroll_Free_NoConfirm` ; `Reroll_Paid_AsksConfirmThenCharges` ; `Reroll_NotEnoughCredits_ButtonDisabled` ;
  - `RecruitClick_Twice_RecruitsOnce` (BusyGate) ;
  - `Tick_DayChangesWhileShown_RefreshesOffers` (LocalNow 08:59 → 09:00, `Tick()`).
  Les anciens tests de recrutement du présentateur et de `HubUseCaseTests` sont remplacés.
- [ ] **Étape 2 :** `$W/run-tests.sh EditMode N29.HeroesDawn.Tests.Presentation.HubPresenterTests` → attendu : échec.
- [ ] **Étape 3 :** implémenter vue, présentateur, recettes, textes ; supprimer l'ancien recrutement.
- [ ] **Étape 4 :** `$W/run-tests.sh EditMode` → 0 échec (dont `PrefabStructureTests`, `NoCodeBuiltScreensTests`, `TestDescriptionsTests`).
- [ ] **Étape 5 :** `$W/gen.sh -overwritePrefabs` puis `git status -- "Code/Heroes Dawn Unity/Assets/_Project/Prefabs"` → attendu : seuls `Hub.prefab` et `RecruitCard.prefab` modifiés (sinon restaurer les autres par `git checkout --` : la génération est déterministe).
- [ ] **Étape 6 : PlayMode** — `FullRunTests` : l'onglet s'appelle toujours via `RecruitTabButton` et recrute la première carte abordable (inchangé hors noms). Nouveau `ShopFlowTests.RerollButton_ShowsNewOffers` : connexion, onglet boutique, clic Renouveler (gratuit) → `session.Shop.Draw == 1` et cartes affichées mises à jour. `$W/run-tests.sh PlayMode` → 18/18 hors capture explicite ; puis captures (`-testFilter …ScreenCaptureTests`) relues : onglet QG inchangé.
- [ ] **Étape 7 : commit** — `feat(shop): onglet Boutique éphémère au QG`.

### Tâche 4 : Documents
**Fichiers :** `04_Tech_Unity/03_Modules_Domaine.md` (module Shop : jour, graine, tirage), `04_Tech_Unity/07_Presentation_Flux.md` (onglet), `04_Tech_Unity/09_Tests_Outils_CI.md` (suites), `05_Production/Decisions.md` (ligne `2026-10-03 | Boutique éphémère : 3 héros/jour à 9h00 locale, tirage déterministe pondéré 70/25/5, renouvellement gratuit puis 100→800 Crédits | Remplace l'onglet Recrutement ; vérification serveur avec UGS`).
- [ ] **Étape 1 :** mettre à jour les docs ; `$W/run-tests.sh EditMode` → 0 échec (rapport régénéré).
- [ ] **Étape 2 : commit** — `docs(shop): boutique éphémère`.
- [ ] **Étape 3 :** en fin de chantier, remettre à l'utilisateur (sans les écrire) : texte proposé pour le GDD §Recrutement et les 8 paramètres `Shop*` à ajouter à l'onglet « Paramètres » du tableur.

## Couverture de la spec
| Spec | Tâche |
|---|---|
| §3 règles (jour, graine, éligibles, tirage, coûts) | 1 |
| §4 horloge, état, sauvegarde v3, cas d'usage, messages, erreurs, retraits | 2 (retraits en 3) |
| §4 équilibrage | 1 (`BalanceConfig`) |
| §5 écran | 3 |
| §6 erreurs | 2, 3 |
| §7 tests | 1–3 |
| §8 documents | 4 |
| Écart consigné | `ShopRerollCosts` → 4 entiers `ShopRerollCost1..4` (`BalanceJson` ne lit que des entiers) ; compteur unique `Draw` |
