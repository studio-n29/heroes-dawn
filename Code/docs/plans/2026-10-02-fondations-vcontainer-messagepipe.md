# Sous-projet 1 — Fondations (VContainer, MessagePipe, UniTask, use cases) · Plan d'implémentation

> **Pour les agents :** SOUS-COMPÉTENCE REQUISE : `superpowers:subagent-driven-development` (recommandé) ou `superpowers:executing-plans` pour exécuter ce plan tâche par tâche. Les étapes utilisent des cases à cocher (`- [ ]`).

**Objectif :** les dépendances passent par VContainer, les commandes par un use case par intention (`UniTask<Result>`) et les événements par MessagePipe. Le jeu reste identique pour le joueur.

**Architecture :** migration progressive en 7 tâches, chacune commitable, testée et jouable. Les classes `*Service` sont supprimées dès qu'elles ne sont plus utilisées. L'UI actuelle (écrans construits par code) est recâblée sur les use cases et le bus ; les présentateurs et les prefabs relèvent du sous-projet 3.

**Stack :** Unity 6000.6.3f1 ; VContainer 1.19.0, UniTask 2.5.11, MessagePipe 1.8.2, MessagePipe.VContainer 1.8.2 (OpenUPM) ; NUnit (Unity Test Framework, EditMode et PlayMode).

**Spec :** `Code/docs/specs/2026-10-02-fondations-vcontainer-messagepipe-design.md`. Elle fait autorité ; ce plan l'argumente.

## Contraintes globales
- **Code partagé :** `Core`, `Definitions` et `Domain.*` ne référencent ni VContainer, ni MessagePipe, ni UniTask.
- **Règles inchangées :** mêmes `DomainError` (codes et clés), mêmes montants, même ordre des effets que la méthode de service remplacée (spec §5.1). Aucune modification de `RulesVersion`.
- **Use cases :**
  - forme `I<Nom>UseCase` + `sealed class <Nom>UseCase` dans `Application/Features/<Zone|Combat|Hub|Auth>/` ;
  - namespace `N29.HeroesDawn.Application.Features.<Feature>` ;
  - une méthode `ExecuteAsync(..., CancellationToken ct = default)` ;
  - séquence : vérifier → appliquer → publier → `RequestSave()`.
- **Messages :** `readonly struct` dans `Application/Messages/`, namespace `N29.HeroesDawn.Application.Messages`, publiés **après** la mise à jour de l'état.
- **Annulation :** `OperationCanceledException` n'est jamais convertie en `Result`.
- **UI :**
  - un clic lance `XxxAsync().Forget()` ;
  - la méthode `async UniTaskVoid XxxAsync()` attend le use case avec `this.GetCancellationTokenOnDestroy()` ;
  - aucun `async void` nouveau ;
  - les abonnements MessagePipe sont rangés dans un `DisposableBagBuilder` et libérés dans `OnDestroy`.
- **Fichiers :** `#nullable enable` dans tout nouveau fichier C# du jeu ; un type public par fichier, sauf les messages, regroupés par feature.

## Commandes (bash, Unity **fermé** sur ce projet)
```bash
UNITY="/c/Program Files/Unity/Hub/Editor/6000.6.3f1/Editor/Unity.exe"
PROJ="C:/Users/dnath/Documents/Heroes Dawn/Code/Heroes Dawn Unity"
# EditMode (ou PlayMode : -testPlatform PlayMode)
"$UNITY" -batchmode -nographics -projectPath "$PROJ" -runTests -testPlatform EditMode -testResults "$TMP/r.xml" -logFile "$TMP/u.log"
grep -o '<test-run [^>]*' "$TMP/r.xml" | grep -oE '(total|passed|failed)="[0-9]+"'
grep -E "error CS" "$TMP/u.log"
```
- Un test : `-testFilter "<Namespace>.<Classe>.<Test>"`.
- Point de départ : **EditMode 36/36**, **PlayMode 0 test**.
- Les tests PlayMode n'utilisent pas `-nographics`, car l'UI a besoin du rendu du Canvas.

## Points d'attention (Review Focus)
1. **Double clic pendant un `await`** (connexion, dé, déplacement, fin de combat) : une seule exécution du use case. Chaque gestionnaire teste et pose un drapeau `_busy` avant l'`await`. Test PlayMode à la tâche 6.
2. **Sauvegarde de l'utilisateur écrasée par les tests PlayMode :** les tests remplacent `IProfileGateway` et `IAuthGateway` par des doubles en mémoire avec `LifetimeScope.Enqueue`. Le test vérifie que le double a bien reçu l'écriture, ce qui prouve que le vrai fichier n'a pas été touché. Tâche 1.
3. **Abonnements MessagePipe qui survivent à une déconnexion/reconnexion :** après `SignOut` puis une nouvelle connexion, un `WalletChanged` ne déclenche qu'**un** rafraîchissement du QG. Test PlayMode à la tâche 6.
4. **Combat terminé par « victoire debug » :** `VerifyCombat` renvoie `null` (pas de faux « rejeu divergent »). Test EditMode à la tâche 4.
5. **`LeaveZone` sur une zone validée vs non validée :** les deux vident la partie ; les récompenses déjà créditées restent ; aucune récompense n'est créditée deux fois. Test EditMode à la tâche 3.

---

## Carte des fichiers
```
Assets/_Project/
├─ Application/
│  ├─ Messages/  SessionMessages.cs (WalletChanged, RosterChanged, ToastRequested, SaveStateChanged)
│  │             ZoneMessages.cs (ZoneValidated)  CombatMessages.cs (CombatFinished)
│  ├─ Session/   ISessionReadModel.cs  IWalletReadModel.cs  ITeamMemberReadModel.cs  GameSession.cs (modifié)
│  │             SaveScheduler.cs (remplace Features/SaveService.cs)
│  ├─ Features/Zone/    ZoneActions.cs  StartZone…  RollDice…  UseBonusDie…  MoveTo…  EndMove…
│  │                    ResolveMiniGame…  ResolveNarrative…  ResolveRest…  LeaveZone…  (+ ZoneErrors, NarrativeChoice, ZoneRewards déplacés)
│  ├─ Features/Combat/  ActiveCombat.cs  IActiveCombatReadModel.cs  BeginCombat…  SetAutoCombat…  FinishCombat…
│  │                    VerifyCombat…  ForceCombatWin…  (+ CombatErrors, CombatReport, FatigueChange déplacés)
│  ├─ Features/Hub/     ToggleSquad…  TrainMember…  RecruitMember…  HealRoster…  (+ HubErrors déplacé)
│  ├─ Features/Auth/    FatigueRecovery.cs  SignInAsGuest…  SignOut…  ResetProgress…  (+ SignInReport déplacé)
│  ├─ Ports/Ports.cs (Task → UniTask)
│  └─ Events/GameEvents.cs, Features/*Service.cs   (supprimés)
├─ Composition/  RootLifetimeScope.cs (nouveau)  GameBootstrap.cs (modifié)
├─ Infrastructure/Simulated/*.cs (Task → UniTask)
└─ Presentation/  UiContext.cs (UiServices), UiRoot.cs, Screens/LoginScreen.cs, Hub/HubScreen.cs, Board/BoardScreen.cs,
                  Combat/CombatScreen.cs, Popups/CasePopups.cs, Common/Widgets.cs (modifiés)
Assets/Tests/
├─ Domain/Application/  TestApp.cs  SaveSchedulerTests.cs  ZoneUseCaseTests.cs  CombatUseCaseTests.cs
│                       HubUseCaseTests.cs  AuthUseCaseTests.cs  DependencyGuardTests.cs
├─ Domain/ApplicationFlowTests.cs (réécrit progressivement)
└─ PlayMode/  BootFlowTests.cs  PlayModeDoubles.cs
```
(`…` = `I<Nom>UseCase.cs` + `<Nom>UseCase.cs`.)

---

### Tâche 1 : Packages, `RootLifetimeScope` sur les services actuels, test de démarrage

**Fichiers :**
- Modifier : `Packages/manifest.json` ; les asmdef `Application`, `Infrastructure`, `Presentation`, `Composition`, `Tests.Domain`, `Tests.PlayMode` (références de la spec §4) ; `Composition/GameBootstrap.cs`
- Créer : `Composition/RootLifetimeScope.cs`, `Tests/PlayMode/BootFlowTests.cs`, `Tests/PlayMode/PlayModeDoubles.cs`

**Interfaces :**
- Produit :
  - `public sealed class RootLifetimeScope : VContainer.Unity.LifetimeScope` : `Configure(IContainerBuilder)` enregistre exactement ce que `GameBootstrap.Awake` construit aujourd'hui. Toutes les registrations sont en `Lifetime.Singleton`. Les ports sont enregistrés par interface : `IAuthGateway`, `IProfileGateway`, `ICombatVerifier`, `ISeedProvider`, `IClock`, `ILogger`. `RegisterBuildCallback` crée `UiRoot.Create(transform, container.Resolve<UiServices>())`.
  - `GameBootstrap` : garde `AutoStart`, les réglages d'application (framerate, orientation) et `OnApplicationPause`/`OnApplicationQuit`. Il ajoute `RootLifetimeScope` à son GameObject et expose `public RootLifetimeScope Scope`. `Session` et `Ui` sont lus depuis le conteneur.
  - `PlayModeDoubles` (tests) : `MemoryProfileGateway : IProfileGateway` avec `int Saves` et `Dictionary Store` ; `InstantAuthGateway : IAuthGateway` avec l'id `"playmode-test"`.

- [ ] **Étape 1 : Ajouter les packages**
Dans `manifest.json` :
- ajouter `"jp.hadashikick"` et `"com.cysharp"` aux `scopes` du registre OpenUPM ;
- ajouter les dépendances `"jp.hadashikick.vcontainer": "1.19.0"`, `"com.cysharp.unitask": "2.5.11"`, `"com.cysharp.messagepipe": "1.8.2"`, `"com.cysharp.messagepipe.vcontainer": "1.8.2"`.

Ajouter ensuite les références d'asmdef de la spec §4 (noms d'assembly : `VContainer`, `UniTask`, `MessagePipe`, `MessagePipe.VContainer`).

- [ ] **Étape 2 : Vérifier la résolution** : lancer la suite EditMode.
Attendu : `total="36" passed="36"`, aucune `error CS`, et les 4 packages présents dans `packages-lock.json`.
Si `Access … denied` dans `u.log` : supprimer `Library/PackageCache/.tmp-*` et relancer.

- [ ] **Étape 3 : Écrire le test PlayMode en échec** (`N29.HeroesDawn.Tests.PlayMode.BootFlowTests`)
```csharp
[UnityTest] public IEnumerator Boot_ShowsLoginScreen()
{   var profiles = new MemoryProfileGateway();
    using (LifetimeScope.Enqueue(b => { b.RegisterInstance<IProfileGateway>(profiles); b.RegisterInstance<IAuthGateway>(new InstantAuthGateway()); }))
    {   GameBootstrap.StartForTests();                       // crée [Heroes Dawn] si absent
        for (int i = 0; i < 300 && LoginVisible() == false; i++) yield return null;
        Assert.That(GameBootstrap.Instance!.Scope.Container, Is.Not.Null);
        Assert.That(GameBootstrap.Instance.Scope.Container.Resolve<IProfileGateway>(), Is.SameAs(profiles)); // Review Focus 2
        Assert.That(LoginVisible(), Is.True); } }
static bool LoginVisible() { var l = Object.FindAnyObjectByType<LoginScreen>(); return l != null && l.gameObject.activeInHierarchy; }
[SetUp] public void SetUp() => GameBootstrap.DestroyForTests();
[TearDown] public void TearDown() => GameBootstrap.DestroyForTests();
```
Comportement attendu :
- `GameBootstrap.StartForTests()` : public static, appelle `AutoStart()`.
- `GameBootstrap.DestroyForTests()` : détruit `[Heroes Dawn]` et l'`EventSystem`, remet `Instance` à `null`.
- `[SetUp]` appelle `DestroyForTests()` **avant** `Enqueue`. Le démarrage automatique (`RuntimeInitializeOnLoadMethod`) a lieu à l'entrée en Play Mode, avant tout code de test. L'instance qu'il crée n'affiche que l'écran de connexion, sans lecture ni écriture de profil ; on la remplace par une instance construite avec les doubles.

- [ ] **Étape 4 : Lancer en PlayMode, vérifier l'échec.** Attendu : erreurs de compilation (`StartForTests`, `Scope`, `RootLifetimeScope` introuvables).

- [ ] **Étape 5 : Implémenter `RootLifetimeScope` et adapter `GameBootstrap`**
Si `Resolve<IProfileGateway>()` ne renvoie pas le double (l'enregistrement ajouté par `Enqueue` ne remplace pas le précédent) : `RootLifetimeScope` n'enregistre les ports par défaut que s'ils n'ont pas été fournis. Il les expose via `public static Action<IContainerBuilder>? TestOverrides`, appelée en dernier. Noter alors la décision dans le journal de suivi.

- [ ] **Étape 6 : Lancer PlayMode puis EditMode.** Attendu : PlayMode `1/1`, EditMode `36/36`.

- [ ] **Étape 7 : Vérification manuelle en batchmode** : `-executeMethod` n'est pas nécessaire, le test PlayMode fait foi. Contrôler en plus que `u.log` ne contient ni `VContainerException` ni `NullReferenceException`.

- [ ] **Étape 8 : Commit** — `feat(archi): VContainer, MessagePipe, UniTask ; RootLifetimeScope`

---

### Tâche 2 : Messages MessagePipe et `SaveScheduler` (fin de `GameEvents`)

**Fichiers :**
- Créer : `Application/Messages/SessionMessages.cs`, `Application/Session/SaveScheduler.cs`, `Tests/Domain/Application/TestApp.cs`, `Tests/Domain/Application/SaveSchedulerTests.cs`
- Supprimer : `Application/Features/SaveService.cs`, `Application/Events/GameEvents.cs`. `ToastKind` déménage dans `SessionMessages.cs`.
- Modifier : les services (`IPublisher<…>` au lieu de `GameEvents`, `SaveScheduler` au lieu de `SaveService`), `RootLifetimeScope`, `UiServices`, `UiRoot`, `HubScreen`, `BoardScreen`, `Widgets.cs`, `ApplicationFlowTests` (construction via `TestApp`)

**Interfaces :**
- Produit (messages) : `WalletChanged(int Credits, int Pr)`, `RosterChanged`, `ToastRequested(string Text, ToastKind Kind)`, `SaveStateChanged(bool Saving)`.
- Produit `SaveScheduler(GameSession, IProfileGateway, IClock, ILogger, IPublisher<SaveStateChanged>)` avec `void RequestSave()` et `UniTask SaveAsync(CancellationToken ct)`. Sémantique identique à `SaveService`. Tant que les ports sont en `Task` (jusqu'à la tâche 6), utiliser `await _profiles.SaveAsync(...).AsUniTask()`.
- Produit `TestApp` (tests). Il construit sans Unity :
  - le bus : `BuiltinContainerBuilder` + `AddMessagePipe()` + `AddMessageBroker<T>()` pour chaque message ;
  - la session `PrototypeContent.Build()` ;
  - les doubles `MemoryProfiles`, `FakeAuth`, `Seeds`, `Clock`, `Log`, `Verifier`, sortis de `ApplicationFlowTests`.

  Il expose `Session`, `Profiles`, `Clock`, `Save` (`SaveScheduler`), `Publisher<T>()` et `Received<T>() → IReadOnlyList<T>`, qui enregistre tout message publié depuis la construction. Les tâches suivantes y ajoutent la construction des use cases.
- `UiServices` : remplace `GameEvents Events` par `ISubscriber<WalletChanged> Wallet`, `ISubscriber<RosterChanged> Roster`, `ISubscriber<ToastRequested> Toasts` et `ISubscriber<SaveStateChanged> Saving`.

- [ ] **Étape 1 : Tests en échec**
```csharp
[Test] public void RequestSave_PublishesSavingTrueThenFalse_AndWritesProfile()
{   var app = new TestApp(); app.SignInDirect("p1");      // pose Session.Account sans passer par l'auth
    app.Save.RequestSave();
    Assert.That(app.Profiles.Saves, Is.EqualTo(1));
    Assert.That(app.Received<SaveStateChanged>().Select(m => m.Saving), Is.EqualTo(new[] { true, false })); }
[Test] public void RequestSave_WhileSaving_RunsExactlyOneMoreWrite()
{   var app = new TestApp(); app.SignInDirect("p1"); app.Profiles.BlockNextSave();   // la 1re écriture reste en attente
    app.Save.RequestSave(); app.Save.RequestSave(); app.Save.RequestSave();
    app.Profiles.ReleaseBlockedSave();
    Assert.That(app.Profiles.Saves, Is.EqualTo(2)); }
[Test] public void RequestSave_WithoutAccount_DoesNothing()
{   var app = new TestApp(); app.Save.RequestSave(); Assert.That(app.Profiles.Saves, Is.EqualTo(0)); }
```
- `MemoryProfiles.BlockNextSave()` : la prochaine `SaveAsync` renvoie une tâche non terminée (`UniTaskCompletionSource`).
- `MemoryProfiles.ReleaseBlockedSave()` : termine cette tâche.

- [ ] **Étape 2 : Vérifier l'échec** (`-testFilter N29.HeroesDawn.Tests.Domain.Application.SaveSchedulerTests`). Attendu : types introuvables.
- [ ] **Étape 3 : Implémenter messages et `SaveScheduler`, puis recâbler services, scope et UI**
- Enregistrer dans le scope : `var o = builder.RegisterMessagePipe();` puis `builder.RegisterMessageBroker<T>(o)` pour chaque message.
- Côté UI : `UiRoot` s'abonne à `ToastRequested`. `HubScreen` et `BoardScreen` s'abonnent dans `OnShow` et libèrent dans `OnHide`, comme les `+=`/`-=` actuels. `Widgets` (indicateur de sauvegarde) s'abonne à `SaveStateChanged`.
- [ ] **Étape 4 : Lancer EditMode puis PlayMode.** Attendu : `failed="0"` partout ; les 5 tests de `ApplicationFlowTests` passent sans changement de leurs assertions.
- [ ] **Étape 5 : Commit** — `refactor(archi): bus MessagePipe et SaveScheduler, fin de GameEvents`

---

### Tâche 3 : Use cases Zone, `ISessionReadModel`, recâblage du plateau

**Fichiers :**
- Créer : `Application/Session/ISessionReadModel.cs`, `IWalletReadModel.cs`, `ITeamMemberReadModel.cs` ; `Application/Messages/ZoneMessages.cs` ; `Application/Features/Zone/ZoneActions.cs` et les 9 use cases ; `Tests/Domain/Application/ZoneUseCaseTests.cs`
- Déplacer dans `Features/Zone/` : `ZoneErrors`, `NarrativeChoice`, `ZoneRewards` (même namespace, déjà `N29.HeroesDawn.Application.Features`, à changer en `.Features.Zone`)
- Modifier : `GameSession` (implémente les interfaces de lecture), `ZoneService` (délègue à `ZoneActions` ; seuls `ResolveCombat` et `Abandon` restent, pour `CombatService`), `UiServices`, `BoardScreen`, `HubScreen` (`StartZone`), `CasePopups`, `ApplicationFlowTests` (tests Zone via use cases)

**Interfaces :**
- Produit :
  - `ISessionReadModel` (spec §5.4) ;
  - `GameSession : ISessionReadModel`, avec implémentations explicites pour `Roster` et `Squad` typées `ITeamMemberReadModel` ;
  - `TeamMember : ITeamMemberReadModel` ; `Wallet : IWalletReadModel`.
- Produit `static class ZoneActions` :
  - `Result<IReadOnlyList<BoardEvent>> Apply(GameSession s, Func<ZoneRun, IReadOnlyList<BoardEvent>> action, ZoneEffects fx)` : reprend `Do` et `Apply` de `ZoneService`, à l'identique ;
  - `ZoneEffects` : `IPublisher<WalletChanged>`, `IPublisher<ZoneValidated>`, `SaveScheduler`, `ILogger`. Publie `ZoneValidated(ZoneRewards)` à la place de `LastRewards`.
- Produit `ZoneValidated(ZoneRewards Rewards)`.
- Produit 9 use cases (signatures spec §5.1). `ILeaveZoneUseCase.ExecuteAsync()` vide `Run` et `RunZone`, puis `RequestSave()`. Sans partie en cours : `Result.Ok()` sans effet.
- `UiServices` gagne `ISessionReadModel Session`, qui remplace `GameSession`, les 9 interfaces Zone et `ISubscriber<ZoneValidated> ZoneValidated`.

- [ ] **Étape 1 : Tests en échec** (`ZoneUseCaseTests`, avec `TestApp` + `app.SignInDirect` + `app.Zone.Start.ExecuteAsync(PrototypeContent.Zone11)`)
```csharp
[Test] public void Start_UnknownZone_FailsWithUnknownZone()          // code "zone.unknown"
[Test] public void Start_SquadExhausted_Fails()                       // toutes fatigues à 100 → "zone.exhausted"
[Test] public void Roll_WithoutRun_FailsWithNoRun()                   // "zone.no_run"
[Test] public void Roll_TwiceInARow_SecondFailsWithNotAllowed()       // "zone.not_allowed", Run inchangé
[Test] public void ResourceCase_CreditsWallet_AndPublishesWalletChanged()
[Test] public void Narrative_Credits_Adds250_Research_Adds10Pr_Morale_RemovesFatigue15()
[Test] public void Rest_Take_RemovesRestFatigueReduction_Skip_KeepsFatigue()
[Test] public void MiniGame_Success_AddsMiniGameCreditsAndPr_Failure_AddsNothing()
[Test] public void ValidatingZone_FirstTime_Publishes1500Credits40Pr_SecondTime_ReplayCredits()
[Test] public void Leave_AfterValidation_And_Leave_Midway_ClearRun_NoDoubleReward()   // Review Focus 5
```
- Pour amener le pion sur une case donnée, `TestApp` expose `ForcePm(int)` (via `ZoneRun.SetPmForTests`, déjà utilisé par les tests du domaine) et `PathTo(CaseType)`, qui cherche une destination du type voulu dans `Run.Destinations`.
- Les montants sont lus depuis `Balance`, `ZoneActions` et `MetaRules`, jamais écrits en dur, sauf 250/10/15 qui sont des constantes publiques (`ResolveNarrativeUseCase.Credits/Pr/Morale`).
- [ ] **Étape 2 : Vérifier l'échec.**
- [ ] **Étape 3 : Implémenter** les interfaces de lecture, `ZoneActions`, les use cases et l'enregistrement dans le scope ; `ZoneService` délègue à `ZoneActions`.
- [ ] **Étape 4 : Recâbler l'UI**
  - `BoardScreen` : `Roll`, `UseBonusDie`, `EndMove`, `MoveTo`, `LeaveZone` (remplace `Abandon` et `CloseCompletedRun`). L'écran de zone validée s'abonne à `ZoneValidated` au lieu de lire `LastRewards`.
  - `CasePopups` : les trois `Resolve*`.
  - `HubScreen` : `StartZone`.
  - Pour chaque action : drapeau `_busy` posé avant l'`await` et retiré dans un `finally`.
- [ ] **Étape 5 : Lancer EditMode et PlayMode.** Attendu : `failed="0"`.
- [ ] **Étape 6 : Commit** — `refactor(archi): use cases Zone et ISessionReadModel`

---

### Tâche 4 : Use cases Combat et `ActiveCombat` (fin de `ZoneService` et `CombatService`)

**Fichiers :**
- Créer : `Features/Combat/ActiveCombat.cs`, `IActiveCombatReadModel.cs`, les 5 use cases ; `Application/Messages/CombatMessages.cs` ; `Tests/Domain/Application/CombatUseCaseTests.cs`
- Déplacer dans `Features/Combat/` : `CombatErrors`, `CombatReport`, `FatigueChange`
- Supprimer : `Features/CombatService.cs`, `Features/ZoneService.cs`
- Modifier : `RootLifetimeScope`, `UiServices`, `BoardScreen` (`BeginCombat`), `CombatScreen`, `ApplicationFlowTests`

**Interfaces :**
- Consomme : `ZoneActions.Apply` (tâche 3), pour `ZoneRun.ResolveCombat` et la sortie de zone si l'escouade est épuisée (même effet que `LeaveZone`).
- Produit `ActiveCombat : IActiveCombatReadModel` :
  - `Simulation`, `Setup`, `Case`, `Fighters` (`IReadOnlyList<ITeamMemberReadModel>`), `DebugUsed` ;
  - `internal void Start(...)` et `public void Clear()`.
- Produit `CombatFinished(CombatReport Report)`.
- Produit 5 use cases (spec §5.1) :
  - `BeginCombat`, `SetAutoCombat`, `FinishCombat` et `VerifyCombat` reproduisent `CombatService` à l'identique ;
  - `ForceCombatWin` pose `DebugUsed = true` ;
  - `FinishCombat` publie `WalletChanged`, `RosterChanged` et `CombatFinished`.
- `UiServices` gagne `IActiveCombatReadModel Combat` et les 5 interfaces. `CombatScreen` appelle `ActiveCombat.Clear()` via une méthode de `IActiveCombatReadModel`, `void Clear()`. Exception assumée : vider l'affichage n'est pas une règle de jeu.

- [ ] **Étape 1 : Tests en échec**
```csharp
[Test] public void Begin_WithoutPendingCombat_Fails()                 // "combat.none"
[Test] public void Begin_EnemyLevel_IsSquadAverage_PlusRankOffset()
[Test] public void Finish_Victory_CreditsByRank_FatiguePerWin_AdvancesBoard()
[Test] public void Finish_Defeat_FatiguePerWinPlusLossExtra_PawnBack()
[Test] public void Finish_DefeatWithSquadExhausted_LeavesZone_ReportToHq()
[Test] public void Finish_PublishesCombatFinished_WithSameReport()
[Test] public void Verify_AfterForceWin_ReturnsNull()                 // Review Focus 4
[Test] public void Verify_NormalAutoCombat_ReturnsTrue()
[Test] public void SetAuto_PersistsInSession_AndSubmitsToSimulation()
```
Pour atteindre un combat : `TestApp.ReachCombat()` lance le dé et suit `PathTo(CaseType.Combat)` jusqu'à `Phase == InCombat`. Le combat se joue avec `Simulation.Step(1)` en boucle, en auto.
- [ ] **Étape 2 : Vérifier l'échec.**
- [ ] **Étape 3 : Implémenter** ; supprimer `CombatService` et `ZoneService` ; recâbler `BoardScreen` et `CombatScreen` (handlers `_busy` pour `FinishCombat`).
- [ ] **Étape 4 : Lancer EditMode et PlayMode.** Attendu : `failed="0"`. Le test `Zone_Combat_Flow_AppliesRewardsAndFatigue` de `ApplicationFlowTests` passe en version use cases avec les mêmes assertions.
- [ ] **Étape 5 : Commit** — `refactor(archi): use cases Combat, ActiveCombat`

---

### Tâche 5 : Use cases Hub (fin de `HubService`)

**Fichiers :**
- Créer : `Features/Hub/` (4 use cases + `HubErrors` déplacé) ; `Tests/Domain/Application/HubUseCaseTests.cs`
- Supprimer : `Features/HubService.cs`
- Modifier : `GameSession` (`AvailableRecruits()`), `RootLifetimeScope`, `UiServices`, `HubScreen`, `ApplicationFlowTests`

**Interfaces :**
- Produit : `ToggleSquad`, `TrainMember`, `RecruitMember` et `HealRoster` (spec §5.1). Chacun publie `WalletChanged` et/ou `RosterChanged`, exactement comme `HubService`, puis `RequestSave()`.

- [ ] **Étape 1 : Tests en échec**
```csharp
[Test] public void ToggleSquad_DuringRun_FailsInMission()            // "hub.in_mission"
[Test] public void ToggleSquad_LastMember_FailsSquadEmpty_FifthMember_FailsSquadFull()
[Test] public void Train_CostsTrainCostOfLevel_AndLevelsUp_MaxLevel_Fails()
[Test] public void Recruit_CostsRecruitCostByRarity_JoinsSquadIfRoom_AlreadyOwned_Fails()
[Test] public void Heal_NoFatigue_FailsNothingToHeal_Else_CostsInfirmaryCost_ReducesAll()
[Test] public void AvailableRecruits_ExcludesOwned()
```
- [ ] **Étape 2 : Vérifier l'échec.**
- [ ] **Étape 3 : Implémenter, supprimer `HubService`, recâbler `HubScreen`.**
- [ ] **Étape 4 : Lancer EditMode et PlayMode.** Attendu : `failed="0"`.
- [ ] **Étape 5 : Commit** — `refactor(archi): use cases Hub`

---

### Tâche 6 : Use cases Auth, ports en UniTask (fin de `AuthService`)

**Fichiers :**
- Créer : `Features/Auth/` (`FatigueRecovery.cs`, 3 use cases, `SignInReport` déplacé) ; `Tests/Domain/Application/AuthUseCaseTests.cs`
- Supprimer : `Features/AuthService.cs`
- Modifier : `Ports/Ports.cs` (`Task` → `UniTask`), `Infrastructure/Simulated/*.cs`, `SaveScheduler` (retirer `.AsUniTask()`), `PlayModeDoubles`, `TestApp`, `RootLifetimeScope`, `UiServices`, `LoginScreen`, `HubScreen`, `ApplicationFlowTests`, `BootFlowTests`

**Interfaces :**
- Produit `static class FatigueRecovery` : `int Apply(IReadOnlyList<TeamMember> roster, DateTime nowUtc, long lastSeenUtcTicks)`. Il renvoie le total récupéré, avec les mêmes règles que `AuthService.RecoverFatigue` (moins d'une minute : 0 ; plafond 100 ; `MetaRules.FatigueRecoveryPerMinute`).
- Produit `SignInAsGuest`, `SignOut` et `ResetProgress` (spec §5.1). `SignInAsGuest` reproduit les deux erreurs `auth.network` et `profile.load` et attend la sauvegarde (`await SaveAsync`).
- `LoginScreen` : `OnPlay` devient `public void Play()`, qui appelle `PlayAsync().Forget()`. Le garde `_busy` est conservé.
- Le test PlayMode appelle `Play()` directement.

- [ ] **Étape 1 : Tests en échec**
```csharp
// AuthUseCaseTests (EditMode)
[Test] public void SignIn_NewProfile_StarterTeam_AndSaves()
[Test] public void SignIn_ExistingProfile_RecoversFatigue_ByElapsedMinutes()
[Test] public void SignIn_GatewayThrows_ReturnsAuthNetwork()
[Test] public void SignIn_ProfileLoadThrows_ReturnsProfileLoad()
[Test] public void SignIn_Cancelled_Throws()                          // OperationCanceledException non convertie
[Test] public void ResetProgress_DeletesProfile_StarterTeam_PublishesWalletAndRoster()
// BootFlowTests (PlayMode)
[UnityTest] public IEnumerator GuestSignIn_ShowsHub()                 // Play() → HubScreen actif ; profiles.Saves ≥ 1
[UnityTest] public IEnumerator DoublePlay_SignsInOnce()               // Play(); Play(); → InstantAuthGateway.Calls == 1 (Review Focus 1)
[UnityTest] public IEnumerator SignOutThenSignIn_WalletChangedRefreshesHubOnce()  // Review Focus 3
```
Pour Review Focus 3 : `HubScreen` expose `public int RefreshCount` (compteur incrémenté dans `Refresh`). C'est un compteur de diagnostic, sans logique.
- [ ] **Étape 2 : Vérifier l'échec.**
- [ ] **Étape 3 : Implémenter**, passer les ports en UniTask, supprimer `AuthService`, recâbler `LoginScreen` et `HubScreen`.
- [ ] **Étape 4 : Lancer EditMode et PlayMode.** Attendu : `failed="0"` ; PlayMode `4/4`.
- [ ] **Étape 5 : Commit** — `refactor(archi): use cases Auth, ports UniTask`

---

### Tâche 7 : Garde-fou des dépendances, nettoyage, documentation

**Fichiers :**
- Créer : `Tests/Domain/Application/DependencyGuardTests.cs`
- Modifier : `ApplicationFlowTests` (plus aucune référence à un `*Service`), `04_Tech_Unity/02_Couches_Assemblies_Dossiers.md` (références d'assemblies réelles), `04_Communication.md` (exemple de code aligné sur les noms réels si besoin), `05_Production/Decisions.md`, `Code/README.md`

- [ ] **Étape 1 : Test du garde-fou**
```csharp
[TestCase(typeof(N29.HeroesDawn.Core.Result))]
[TestCase(typeof(N29.HeroesDawn.Definitions.BalanceConfig))]
[TestCase(typeof(N29.HeroesDawn.Domain.Board.ZoneRun))]
[TestCase(typeof(N29.HeroesDawn.Domain.Combat.CombatSimulation))]
public void SharedAssemblies_DoNotReferenceFrameworkPackages(Type marker)
{   var refs = marker.Assembly.GetReferencedAssemblies().Select(a => a.Name).ToArray();
    Assert.That(refs, Has.None.StartsWith("VContainer").And.None.StartsWith("MessagePipe").And.None.StartsWith("UniTask")); }
[Test] public void NoServiceClassesRemain()
{   var app = typeof(N29.HeroesDawn.Application.Session.GameSession).Assembly;
    Assert.That(app.GetTypes().Where(t => t.Name.EndsWith("Service")), Is.Empty); }
```
- [ ] **Étape 2 : Lancer.** Attendu : réussite immédiate. C'est un garde-fou de non-régression : le comportement existe déjà après les tâches 1 à 6. On vérifie qu'il échoue bien en ajoutant temporairement `class DummyService {}` dans `Application`, puis on le retire.
- [ ] **Étape 3 : Documentation**
  - `Decisions.md` : `2026-10-02 | Versions figées : VContainer 1.19.0, UniTask 2.5.11, MessagePipe 1.8.2 (+ .VContainer) via OpenUPM | Remplace « versions à figer au démarrage de M1 »` et `2026-10-02 | Sous-projet 1 : use cases + MessagePipe + RootLifetimeScope ; UI encore construite par code (prefabs au sous-projet 3)`.
  - `02_Couches` : les asmdef réelles et leurs références (spec §4).
  - `Code/README.md` : une ligne « Architecture : use cases dans `Application/Features/<Feature>/`, messages dans `Application/Messages/`, composition dans `RootLifetimeScope` ».
- [ ] **Étape 4 : Lancer EditMode et PlayMode.** Attendu : `failed="0"`.
- [ ] **Étape 5 : Commit** — `chore(archi): garde-fou des dépendances, docs fondations`

## Définition de « terminé »
- Aucune classe `*Service` ni `GameEvents` dans `Application`.
- Toutes les intentions de l'UI passent par un `I…UseCase`, et tous les événements par MessagePipe.
- EditMode et PlayMode verts en batchmode.
- Le jeu se joue comme avant : connexion → QG → zone 1.1 → combat → résultats → retour.
