# Sous-projet 2 — Scènes et flux de jeu (+ rapports de test) · Plan d'implémentation

> **Pour les agents :** SOUS-COMPÉTENCE REQUISE : `superpowers:subagent-driven-development` (recommandé) ou `superpowers:executing-plans` pour exécuter ce plan tâche par tâche. Les étapes utilisent des cases à cocher (`- [ ]`).

**Objectif :** le jeu démarre par la scène `Boot` et passe entre `Hub`, `Board` et `Combat` uniquement via une machine d'états, chaque scène ayant son scope VContainer. Chaque exécution de tests produit un rapport HTML qui détaille ce que chaque test prouve.

**Architecture :**
- `GameFlowMachine` est du C# pur dans `Application/Flow`. Il pilote un port `ISceneLoader` (adaptateur `UnitySceneLoader`) et le port `IScreenVisibility`, implémenté par `UiRoot`.
- Les écrans, toujours construits par code, sont créés par le scope de leur scène.
- La navigation passe par des messages MessagePipe.
- Les scènes sont générées par un menu éditeur exécutable en batchmode.

**Stack :** Unity 6000.6.3f1, VContainer 1.19.0, MessagePipe 1.8.2, UniTask 2.5.11, Unity Test Framework (API `TestRunnerApi` / `ICallbacks`).

**Spec :** `Code/docs/specs/2026-10-03-scenes-flux-design.md`. Elle fait autorité.

## Contraintes globales
- **Comportement :** le joueur ne voit aucun changement. Règles, montants et textes sont inchangés.
- **Localisation :** aucun texte d'interface en dur ; toute nouvelle clé de texte va dans le dictionnaire `Fr` de `Presentation/Common/Loc.cs`.
- **Navigation :** seule `GameFlowMachine` charge, décharge, affiche ou masque une scène. Aucun écran ne référence un autre écran.
- **Dépendances :** `Core`, `Definitions` et `Domain.*` ne référencent ni VContainer, ni MessagePipe, ni UniTask (garde-fou existant).
- **Descriptions de test :** chaque méthode de test porte `[Description("Vérifie que … — parce que …")]`. La partie « parce que » cite une règle (GDD/spec), un point de Review Focus ou un risque.
- **Style :** `#nullable enable` dans tout nouveau fichier du jeu ; un type public par fichier, sauf les messages regroupés.
- **Rapports :**
  - après chaque série de tests, une synthèse en chat : totaux, ce que la série prouve par fonctionnalité, échecs et causes, chemin du rapport HTML ;
  - dans les rapports d'étape de ce plan, la synthèse remplace le simple « total ».

## Commandes (bash, Unity **fermé**)
```bash
UNITY="/c/Program Files/Unity/Hub/Editor/6000.6.3f1/Editor/Unity.exe"
PROJ="C:/Users/dnath/Documents/Heroes Dawn/Code/Heroes Dawn Unity"
"$UNITY" -batchmode -nographics -projectPath "$PROJ" -runTests -testPlatform EditMode -testResults "$TMP/e.xml" -logFile "$TMP/e.log"
"$UNITY" -batchmode -projectPath "$PROJ" -runTests -testPlatform PlayMode -testResults "$TMP/p.xml" -logFile "$TMP/p.log"   # sans -nographics
grep -o '<test-run [^>]*' "$TMP/e.xml" | grep -oE '(total|passed|failed)="[0-9]+"'
```
- Un test : `-testFilter "<Namespace>.<Classe>.<Test>"`.
- Un menu : `-executeMethod <Namespace>.<Classe>.<Méthode> -quit`.
- Point de départ : **EditMode 76/76, PlayMode 5/5**.

## Points d'attention (Review Focus)
1. **Plateau masqué pendant le combat :** la désactivation déclenche `OnDisable`/`OnHide`. Les abonnements à vie du plateau (`ZoneValidated`, `ReturnedToBoard`) doivent survivre, sinon la validation de zone obtenue par un combat n'affiche rien au retour. Test PlayMode à la tâche 5.
2. **Double demande de navigation** (double clic sur Déployer, deux messages rapprochés) : une seule transition, aucune scène chargée deux fois. Tests EditMode (tâche 2) et PlayMode (tâche 5).
3. **Abonnements des scènes déchargées :** après le déchargement de `Hub`, un `WalletChanged` ne doit atteindre aucun écran détruit (pas de `MissingReferenceException`). Test PlayMode à la tâche 5.
4. **Play depuis l'éditeur avec une autre scène ouverte :** le jeu démarre quand même par `Boot`, et les tests PlayMode ne sont pas détournés vers `Boot`. Vérification à la tâche 3.
5. **Callbacks du Test Runner en batchmode :** le rapport HTML est produit par `-runTests`, en EditMode comme en PlayMode (rechargement de domaine). Vérification à la tâche 1.

---

## Carte des fichiers
```
Assets/_Project/
├─ Application/
│  ├─ Flow/      GameState.cs  IGameFlow.cs  GameFlowMachine.cs  IScreenVisibility.cs  SceneNames.cs
│  ├─ Messages/  FlowMessages.cs (NavigateRequested, CombatResultsClosed, ReturnedToBoard, GameStateChanged)
│  └─ Ports/     ISceneLoader.cs
├─ Infrastructure/Platform/  UnitySceneLoader.cs
├─ Composition/  RootLifetimeScope.cs (modifié)  GameBootstrap.cs (modifié)
│                HubLifetimeScope.cs  BoardLifetimeScope.cs  CombatLifetimeScope.cs  SceneScreenScope.cs
├─ Presentation/ UiRoot.cs (composant de Boot, IScreenVisibility)  UiContext.cs  UiServices.cs
│                Screens/UiScreen.cs (Show/Hide)  Screens/LoginScreen.cs  Hub/HubScreen.cs
│                Board/BoardScreen.cs  Combat/CombatScreen.cs  Common/Loc.cs (+ errors.navigation)
├─ Editor/Testing/  TestReportModel.cs  TestReportWriter.cs  TestRunReporter.cs  GitInfo.cs
├─ Editor/Scenes/   SceneGenerator.cs  BootSceneStarter.cs
└─ Scenes/          Boot.unity  Hub.unity  Board.unity  Combat.unity   (générées)
Assets/Tests/
├─ Domain/Flow/     GameFlowMachineTests.cs  FakeSceneLoader.cs
├─ Editor/          TestDescriptionsTests.cs  TestReportWriterTests.cs  SceneGeneratorTests.cs
└─ PlayMode/        BootFlowTests.cs (réécrit)  SceneFlowTests.cs  PlayModeHarness.cs
Code/docs/test-reports/  latest-EditMode.html  latest-PlayMode.html  (+ rapports datés, ignorés)
```

---

### Tâche 1 : Rapports de test et descriptions

**Fichiers :**
- Créer : `Editor/Testing/TestReportModel.cs`, `TestReportWriter.cs`, `TestRunReporter.cs`, `GitInfo.cs`
- Créer : `Tests/Editor/TestDescriptionsTests.cs`, `Tests/Editor/TestReportWriterTests.cs`
- Modifier : asmdef `N29.HeroesDawn.Editor` (références `UnityEditor.TestRunner`, `UnityEngine.TestRunner`) ; **les 81 tests existants** (ajout de `[Description]`) ; `.gitignore` racine (`Code/docs/test-reports/*` puis `!Code/docs/test-reports/latest-*.html`)

**Interfaces :**
- Produit (namespace `N29.HeroesDawn.EditorTools.Testing`) :
  - `sealed class TestReportEntry` { `string Fixture`, `string Name`, `string Description`, `TestOutcome Outcome`, `double DurationSeconds`, `string Message`, `string StackFirstLine` } ;
  - `enum TestOutcome { Passed, Failed, Skipped }` ;
  - `sealed class TestRunInfo` { `string Platform`, `DateTime StartedLocal`, `double DurationSeconds`, `string Branch`, `string Commit` } ;
  - `static class TestReportWriter` : `string Render(TestRunInfo run, IReadOnlyList<TestReportEntry> entries)`. Il produit une page HTML autonome :
    - un en-tête avec les totaux par résultat ;
    - un tableau par `Fixture`, trié par nom ;
    - tout le texte échappé (`&`, `<`, `>`, `"`) ;
    - les couleurs en variables CSS, avec `@media (prefers-color-scheme: dark)`.
  - `static class GitInfo` : `(string Branch, string Commit) Read(string repoRoot)`. Il lit `.git/HEAD` puis le fichier de la référence ; `("?", "?")` si absent.
  - `TestRunReporter : ICallbacks` (`[InitializeOnLoad]`, `TestRunnerApi.RegisterCallbacks`) :
    - `RunFinished` parcourt les feuilles (`!HasChildren`) et les convertit en `TestReportEntry` (description = `Test.Description`) ;
    - il écrit `Code/docs/test-reports/<aaaa-MM-jj_HH-mm-ss>_<Platform>.html` et `latest-<Platform>.html`, avec `Platform` = `EditMode` ou `PlayMode` ;
    - une exception est captée et journalisée en avertissement.

- [ ] **Étape 1 : Écrire les tests en échec**
```csharp
// TestReportWriterTests
[Test, Description("Vérifie que le rapport affiche nom, description, résultat et message d'échec — parce que l'utilisateur veut savoir ce qui marche et pourquoi")]
public void Render_ShowsNameDescriptionOutcomeAndFailure()
{   var html = TestReportWriter.Render(Run(), new[] {
        Entry("A", "Ok_Test", "Vérifie X — parce que Y", TestOutcome.Passed, ""),
        Entry("A", "Ko_Test", "Vérifie Z — parce que W", TestOutcome.Failed, "Expected 2 but was 3") });
    StringAssert.Contains("Ok_Test", html); StringAssert.Contains("Vérifie X — parce que Y", html);
    StringAssert.Contains("Expected 2 but was 3", html); StringAssert.Contains("1 réussi", html); StringAssert.Contains("1 échoué", html); }
[Test, Description("Vérifie que le texte des tests est échappé — parce qu'un message d'échec contenant du HTML casserait la page")]
public void Render_EscapesHtml()
{   var html = TestReportWriter.Render(Run(), new[] { Entry("A", "T", "<b>x</b>", TestOutcome.Failed, "a < b & c") });
    StringAssert.Contains("&lt;b&gt;x&lt;/b&gt;", html); StringAssert.Contains("a &lt; b &amp; c", html); StringAssert.DoesNotContain("<b>x</b>", html); }
[Test, Description("Vérifie que GitInfo lit la branche et le commit sans lancer git — parce que tout doit tourner dans Unity")]
public void GitInfo_ReadsHeadAndRef()   // dossier temporaire avec .git/HEAD = "ref: refs/heads/demo" et .git/refs/heads/demo = "abc123\n"
// TestDescriptionsTests
[Test, Description("Vérifie que chaque test du projet a une description — parce que le rapport doit dire ce que chaque test prouve")]
public void EveryTestHasADescription()
{   var missing = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetName().Name.StartsWith("N29.HeroesDawn.Tests"))
        .SelectMany(a => a.GetTypes()).SelectMany(t => t.GetMethods())
        .Where(m => m.GetCustomAttributes().Any(x => x is TestAttribute || x is UnityTestAttribute || x is TestCaseAttribute))
        .Where(m => string.IsNullOrWhiteSpace(m.GetCustomAttribute<DescriptionAttribute>()?.Properties.Get("Description") as string))
        .Select(m => m.DeclaringType!.Name + "." + m.Name).ToList();
    Assert.That(missing, Is.Empty, string.Join("\n", missing)); }
```
`Tests.Editor` ne référence pas les autres assemblies de tests : `EveryTestHasADescription` les trouve par leur nom dans l'`AppDomain`, et elles sont chargées dans l'éditeur.

- [ ] **Étape 2 : Lancer EditMode, vérifier l'échec.** Attendu : erreurs de compilation (types `TestReportWriter` / `GitInfo` introuvables).
- [ ] **Étape 3 : Implémenter les 4 classes de `Editor/Testing`.**
- [ ] **Étape 4 : Lancer EditMode.** Attendu : seul `EveryTestHasADescription` échoue, avec la liste des 81 tests sans description.
- [ ] **Étape 5 : Ajouter les descriptions aux 81 tests.** Lire chaque test et écrire ce qu'il vérifie, puis pourquoi : la règle, la spec ou le point de Review Focus qu'il couvre. Exemple :
  `[Test, Description("Vérifie qu'un deuxième lancer sans déplacement est refusé (zone.not_allowed) et ne change pas les PM — parce que le dé ne se relance qu'après le déplacement (GDD §5)")]`.
- [ ] **Étape 6 : Lancer EditMode puis PlayMode.**
  - Attendu : `failed="0"` sur les deux.
  - Vérifier (Review Focus 5) que `Code/docs/test-reports/latest-EditMode.html` **et** `latest-PlayMode.html` existent et sont datés de la minute de l'exécution, et que leurs totaux égalent ceux du XML.
  - Rédiger la synthèse en chat à partir du rapport.
- [ ] **Étape 7 : Commit** — `feat(tests): rapport HTML à chaque exécution, descriptions de tous les tests` (inclure les deux `latest-*.html`).

---

### Tâche 2 : Machine d'états, messages et ports (sans Unity)

**Fichiers :**
- Créer : `Application/Flow/GameState.cs`, `IGameFlow.cs`, `GameFlowMachine.cs`, `IScreenVisibility.cs`, `SceneNames.cs` ; `Application/Messages/FlowMessages.cs` ; `Application/Ports/ISceneLoader.cs`
- Créer : `Tests/Domain/Flow/FakeSceneLoader.cs`, `Tests/Domain/Flow/GameFlowMachineTests.cs`

**Interfaces :**
- Produit (namespace `N29.HeroesDawn.Application.Flow`) :
  - `enum GameState { Boot, Login, Hub, Board, Combat }` ;
  - `static class SceneNames` : `const string Hub = "Hub", Board = "Board", Combat = "Combat"` ;
  - `interface IScreenVisibility { void SetLoginVisible(bool visible); }` ;
  - `interface IGameFlow { GameState Current { get; } bool IsTransitioning { get; } UniTask<bool> RequestAsync(GameState target, CancellationToken ct = default); }` ;
  - `sealed class GameFlowMachine : IGameFlow, IDisposable` :
    - constructeur `(ISceneLoader scenes, IScreenVisibility screens, ISubscriber<NavigateRequested> nav, ISubscriber<CombatResultsClosed> closed, IPublisher<GameStateChanged> changed, IPublisher<ReturnedToBoard> returned, IPublisher<ToastRequested> toasts, ILogger log)` ;
    - `void Start()` : transition Boot → Login, qui appelle `SetLoginVisible(true)` ;
    - `NavigateRequested` lance `RequestAsync(Target).Forget()` ;
    - `CombatResultsClosed(report)` donne `RequestAsync(report.ReturnedToHq ? Hub : Board)`, en gardant le rapport pour publier `ReturnedToBoard(report)` après réaffichage.
- Produit (namespace `N29.HeroesDawn.Application.Messages`) : `NavigateRequested(GameState Target)`, `CombatResultsClosed(CombatReport Report)`, `ReturnedToBoard(CombatReport Report)`, `GameStateChanged(GameState From, GameState To)`.
- Produit `interface ISceneLoader { UniTask LoadAsync(string scene, CancellationToken ct); UniTask UnloadAsync(string scene, CancellationToken ct); void SetVisible(string scene, bool visible); }` (namespace `…Application.Ports`).
- **Effets par transition** (spec §6.1) :

  | Transition | Effets, dans l'ordre |
  |---|---|
  | Login → Hub | `SetLoginVisible(false)`, `Load(Hub)` |
  | Hub → Login | `Unload(Hub)`, `SetLoginVisible(true)` |
  | Hub → Board | `Unload(Hub)`, `Load(Board)` |
  | Board → Hub | `Unload(Board)`, `Load(Hub)` |
  | Board → Combat | `Load(Combat)`, `SetVisible(Board, false)` |
  | Combat → Board | `Unload(Combat)`, `SetVisible(Board, true)`, publier `ReturnedToBoard` (s'il y a un rapport) |
  | Combat → Hub | `Unload(Combat)`, `Unload(Board)`, `Load(Hub)` |

- **Après chaque transition :** `Current = target`, puis publier `GameStateChanged(from, to)`.
- **Demandes ignorées :** une demande vers l'état courant, ou reçue pendant une transition, est ignorée (`false`).
- **Transition absente du tableau :** `false`, plus `log.Warn("Navigation refusée : {from} → {to}")`.
- **Exception d'un effet :** `log.Error`, publier `ToastRequested("errors.navigation", ToastKind.Error)`, puis appliquer les effets inverses de ceux déjà faits, dans l'ordre inverse (`Load` ↔ `Unload`, `SetVisible` ↔ son contraire, `SetLoginVisible` ↔ son contraire). `Current` est inchangé ; renvoie `false`.
- `FakeSceneLoader` (tests) : `List<string> Calls` (`"load:Hub"`, `"unload:Board"`, `"show:Board"`, `"hide:Board"`), `string? FailOnLoad`, et `UniTaskCompletionSource? Gate` pour suspendre un `LoadAsync`. `FakeScreens : IScreenVisibility` ajoute `"login:on"` / `"login:off"` à la même liste.

- [ ] **Étape 1 : Écrire les tests en échec** (`GameFlowMachineTests`, chacun avec sa `[Description]`) :
```csharp
[Test] public void Start_ShowsLogin()                                    // Calls == ["login:on"], Current == Login
[Test] public void LoginToHub_HidesLogin_LoadsHub_PublishesStateChanged()
[Test] public void HubToLogin_UnloadsHub_ShowsLogin()
[Test] public void HubToBoard_UnloadsHub_LoadsBoard()
[Test] public void BoardToHub_UnloadsBoard_LoadsHub()
[Test] public void BoardToCombat_LoadsCombat_ThenHidesBoard()
[Test] public void CombatResultsClosed_Victory_UnloadsCombat_ShowsBoard_ThenPublishesReturnedToBoard()   // ordre : unload, show, puis message (rapport identique)
[Test] public void CombatResultsClosed_ReturnedToHq_UnloadsCombatThenBoard_LoadsHub()
[Test] public void HubToCombat_IsRefused_StateUnchanged()                // Review : transition non prévue
[Test] public void RequestWhileTransitioning_IsIgnored_SingleLoad()       // Gate suspend load:Board ; 2e demande → false ; Calls contient load:Board une fois (Review Focus 2)
[Test] public void RequestToCurrentState_IsIgnored()
[Test] public void LoadFailure_RollsBack_StaysInState_ToastsError()      // FailOnLoad = "Board" depuis Hub : Calls == [..., "unload:Hub", "load:Board"(échec), "load:Hub"], Current == Hub, ToastRequested("errors.navigation")
[Test] public void NavigateRequestedMessage_TriggersTransition()
```
- [ ] **Étape 2 : Vérifier l'échec** (compilation). **Étape 3 : Implémenter.** **Étape 4 : Lancer EditMode** — attendu `failed="0"` ; synthèse en chat.
- [ ] **Étape 5 : Commit** — `feat(flux): machine d'états du jeu et port de chargement de scènes`

---

### Tâche 3 : Scènes générées, scopes de scène, démarrage par Boot

Le jeu reste jouable comme aujourd'hui. `Boot` contient l'ensemble actuel : `UiRoot` et les 4 écrans pilotés par `Navigator`. Les scènes `Hub`, `Board` et `Combat` sont créées avec leurs scopes, mais ne sont pas encore chargées en jeu (tâche 4).

**Fichiers :**
- Créer : `Composition/SceneScreenScope.cs`, `HubLifetimeScope.cs`, `BoardLifetimeScope.cs`, `CombatLifetimeScope.cs` ; `Editor/Scenes/SceneGenerator.cs`, `BootSceneStarter.cs` ; `Tests/Editor/SceneGeneratorTests.cs` ; `Tests/PlayMode/PlayModeHarness.cs`
- Générer : `Assets/_Project/Scenes/Boot.unity`, `Hub.unity`, `Board.unity`, `Combat.unity`
- Modifier : `GameBootstrap.cs` (plus de `RuntimeInitializeOnLoadMethod` ni d'`AddComponent<RootLifetimeScope>` : il lit `GetComponent<RootLifetimeScope>()`) ; `RootLifetimeScope.cs` (`UiRoot` désormais composant de `Boot` : `RegisterComponentInHierarchy<UiRoot>()`, construit dans le callback par `ui.Build(services)`) ; `UiRoot.cs` (`Create` devient la méthode d'instance `Build(UiServices)`) ; `Tests/PlayMode/BootFlowTests.cs` (démarrage par `PlayModeHarness`) ; `ProjectSettings/EditorBuildSettings.asset` (via le générateur)

**Interfaces :**
- Produit :
  - `abstract class SceneScreenScope : LifetimeScope` : `[SerializeField] RectTransform screenRoot` (le canvas de la scène). `protected abstract UiScreen CreateScreen(RectTransform root, UiContext ctx)` est utilisé à la tâche 4 ; à cette tâche, `Configure` est vide.
  - `HubLifetimeScope`, `BoardLifetimeScope`, `CombatLifetimeScope : SceneScreenScope`.
  - `static class SceneGenerator` : `public static void GenerateAll()` (menu `Heroes Dawn/Setup/Générer les scènes`).
    - `Boot` : un GameObject `[Heroes Dawn]` avec `RootLifetimeScope` et `GameBootstrap`, plus un GameObject `UI` (RectTransform) avec `UiRoot`.
    - `Hub`, `Board`, `Combat` : un GameObject `[Scope]` avec le scope de scène, dont `parentReference` vaut `ParentReference.Create<RootLifetimeScope>()` (`Combat` : `ParentReference.Create<BoardLifetimeScope>()`) et `screenRoot` vise le Canvas. Plus un GameObject `Canvas` : Canvas Screen Space Overlay, `sortingOrder` 10 (`Combat` : 20), CanvasScaler 1920×1080 avec match 0,5, GraphicRaycaster, et un enfant `SafeArea` étiré (composant `SafeArea`).
    - Sauvegarde dans `Assets/_Project/Scenes/`, puis `EditorBuildSettings.scenes = [Boot, Hub, Board, Combat]`.
  - `BootSceneStarter` (`[InitializeOnLoad]`) : `EditorSceneManager.playModeStartScene = Boot`. Il s'abonne à `TestRunnerApi` (`RunStarted` → `null`, `RunFinished` → `Boot`).
  - `PlayModeHarness` (tests) :
    - `IEnumerator Boot(MemoryProfileGateway, InstantAuthGateway)` : charge `Boot` (`LoadSceneMode.Single`) à l'intérieur de `LifetimeScope.Enqueue` avec les doubles, puis attend que la connexion soit visible ;
    - `IEnumerator TearDown()` : crée une scène vide, la rend active, décharge toutes les autres, détruit les objets `DontDestroyOnLoad` restants.

- [ ] **Étape 1 : Tests en échec** (`SceneGeneratorTests`, avec descriptions) :
```csharp
[Test] public void BuildSettings_ListBootHubBoardCombat_InOrder()
[Test] public void HubAndBoardScopes_HaveRootParent_CombatScope_HasBoardParent()   // charge la scène en éditeur (EditorSceneManager.OpenScene, Additive), lit parentReference.TypeName
[Test] public void SceneCanvases_SortingOrder_Hub10_Board10_Combat20()
[Test] public void BootScene_HasGameBootstrapRootScopeAndUiRoot()
```
- [ ] **Étape 2 : Vérifier l'échec.**
- [ ] **Étape 3 : Implémenter**, puis générer en batchmode : `-executeMethod N29.HeroesDawn.EditorTools.Scenes.SceneGenerator.GenerateAll -quit`. Attendu : les 4 fichiers `.unity` et `EditorBuildSettings.asset` mis à jour (`SampleScene` retirée).
- [ ] **Étape 4 : Adapter `GameBootstrap`, `RootLifetimeScope`, `UiRoot` et `BootFlowTests`** (`PlayModeHarness.Boot` remplace `StartForTests`/`DestroyForTests`).
- [ ] **Étape 5 : Lancer EditMode puis PlayMode.**
  - Attendu : `failed="0"` ; les 5 tests de `BootFlowTests` passent en démarrant par `Boot`.
  - Review Focus 4 : les tests PlayMode ne démarrent pas par `Boot` à cause de `playModeStartScene`, puisqu'ils chargent eux-mêmes `Boot`.
  - Vérifier dans `p.log` l'absence de « could not found parent reference ».
  - Synthèse en chat.
- [ ] **Étape 6 : Commit** — `feat(scenes): scènes générées, scopes de scène, démarrage par Boot`

---

### Tâche 4 : Écrans dans leurs scènes, navigation par messages

**Fichiers :**
- Modifier : les scopes de scène (`CreateScreen` + `RegisterBuildCallback`) ; `RootLifetimeScope` :
  - enregistrer `ISceneLoader` → `UnitySceneLoader`, `GameFlowMachine` (`.AsSelf().As<IGameFlow>()`), `UiRoot.As<IScreenVisibility>()` et les 4 brokers de `FlowMessages` ;
  - le callback appelle `ui.Build(...)` puis `flow.Start()`.
- Modifier : `UiRoot` (ne crée plus que `LoginScreen`, la couche des fenêtres et la couche des messages, sur deux canvas d'ordre 10 et 100 ; implémente `IScreenVisibility`). `ToastRequested.Text` est traduit par `Loc.T` s'il est une clé connue (`Loc.Has` rendue publique si nécessaire). Ajouter `"errors.navigation"` au dictionnaire `Fr` : « Navigation impossible, réessaie. ».
- Modifier : `UiContext` (plus de références d'écrans ni de `Navigator` ; ajoute `void Navigate(GameState target)` qui publie `NavigateRequested`) ; `UiServices` (ajoute `IGameFlow Flow`, `IPublisher<NavigateRequested> Navigation`, `IPublisher<CombatResultsClosed> CombatClosed`, `ISubscriber<ReturnedToBoard> ReturnedToBoard`) ; `UiScreen` :
  - supprimer `Navigator` ;
  - `Show()` = `SetActive(true)`, `Hide()` = `SetActive(false)` ;
  - `OnShow`/`OnHide` sont appelés depuis `OnEnable`/`OnDisable` via un drapeau, pour couvrir aussi la désactivation de la racine de la scène.
- Modifier les écrans selon la spec §6.4 :
  - **LoginScreen :** `Navigate(Hub)`.
  - **HubScreen :** `Navigate(Board)` ; `Navigate(Login)`.
  - **BoardScreen :**
    - `Navigate(Hub)` dans les 3 cas ;
    - `Navigate(Combat)` après `BeginCombat` ;
    - `OnCombatReport` devient privé, abonné dans `Build` à `ReturnedToBoard` et libéré dans `OnDestroy` ; il ne garde que `ProcessEvents(report.BoardEvents)` ;
    - les gardes `Ctx.Nav.Current != this` deviennent `Ctx.S.Flow.Current != GameState.Board`.
  - **CombatScreen :**
    - `Navigate(Board)` si aucun combat ;
    - « Continuer » : `Combat.Clear()` ; si `report.ReturnedToHq`, afficher `Ctx.Toast(Loc.T("combat.to_hq"), Warning)` (déplacé depuis `BoardScreen`) ; puis publier `CombatResultsClosed(report)`.
- Créer : `Infrastructure/Platform/UnitySceneLoader.cs` :
  - chargement : `SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive).ToUniTask(cancellationToken: ct)` ;
  - déchargement : `UnloadSceneAsync` ;
  - `SetVisible` : `SetActive` sur les objets racines de la scène qui ne portent pas de `LifetimeScope`.

**Interfaces :**
- Consomme : `IGameFlow`, `GameFlowMachine`, `FlowMessages`, `ISceneLoader` et `IScreenVisibility` (tâche 2) ; les scènes et `SceneScreenScope` (tâche 3).
- Produit : `SceneScreenScope.Screen` (l'écran créé), utilisé par les tests PlayMode.

- [ ] **Étape 1 : Tests en échec** (`SceneFlowTests`, PlayMode, avec descriptions) :
```csharp
[UnityTest] public IEnumerator SignIn_LoadsHubScene_HidesLogin()
[UnityTest] public IEnumerator Deploy_UnloadsHub_LoadsBoard()
[UnityTest] public IEnumerator BoardCombatBoard_CombatSceneLoadedOverBoard_ThenUnloaded_BoardShownWithReport()
```
`ZoneRun.ForTests` est accessible via `GameSession` (résolu par le conteneur) pour placer le pion devant n03, comme dans les tests EditMode. Le test force la victoire par `IForceCombatWinUseCase` et clique « Continuer » en appelant une méthode publique de test, `CombatScreen.ContinueForTests()` (ajoutée ; elle appelle le même code que le bouton).
- [ ] **Étape 2 : Vérifier l'échec.** **Étape 3 : Implémenter.**
- [ ] **Étape 4 : Lancer EditMode puis PlayMode.** Attendu : `failed="0"` ; aucune `MissingReferenceException` ni `NullReferenceException` dans `p.log`. Synthèse en chat.
- [ ] **Étape 5 : Commit** — `feat(flux): écrans dans leurs scènes, navigation par la machine d'états`

---

### Tâche 5 : Tests de flux restants, documentation

**Fichiers :**
- Modifier : `Tests/PlayMode/SceneFlowTests.cs`
- Docs : `04_Tech_Unity/07_Presentation_Flux.md`, `09_Tests_Outils_CI.md`, `05_Production/Decisions.md`, `Code/SETUP.md`

- [ ] **Étape 1 : Tests** (avec descriptions) :
```csharp
[UnityTest] public IEnumerator LeaveZone_UnloadsBoard_LoadsHub()
[UnityTest] public IEnumerator DoubleDeploy_SingleBoardScene()                         // Review Focus 2 : deux NavigateRequested(Board) → SceneManager.sceneCount, une seule « Board »
[UnityTest] public IEnumerator WinThatValidatesZone_ShowsZoneCompleteAfterCombat()     // Review Focus 1 : DriveZone11ToLastStep version jeu (n23 → combat), victoire, Continuer → fenêtre « zone validée » avec les récompenses
[UnityTest] public IEnumerator HubUnloaded_WalletChanged_NoErrorNoDestroyedRefresh()  // Review Focus 3 : LogAssert.NoUnexpectedReceived()
[UnityTest] public IEnumerator DefeatWithExhaustedSquad_ReturnsToHub_UnloadsBoardAndCombat()
```
- [ ] **Étape 2 : Lancer.**
  - Ces tests couvrent un comportement déjà implémenté aux tâches 3 et 4. Pour prouver qu'ils peuvent échouer, casser temporairement le code sous chacun, puis restaurer. Dans l'ordre des tests ci-dessus :
    1. LeaveZone : le gestionnaire « Retour QG » ne publie plus `Navigate(Hub)`.
    2. DoubleDeploy : retirer la garde `IsTransitioning`.
    3. WinThatValidatesZone : déplacer l'abonnement `ZoneValidated` de `Build` vers `OnShow`.
    4. HubUnloaded : retirer `OnDestroy` de `HubScreen`.
    5. DefeatWithExhaustedSquad : `CombatResultsClosed` vers Board même si `ReturnedToHq`.
  - Consigner chaque échec observé dans le journal de suivi.
- [ ] **Étape 3 : Documentation**
  - `07` : remplacer « Scope VContainer… » par l'état réel :
    - scènes générées par menu ;
    - `SceneScreenScope` ;
    - tableau des transitions de la spec §6.1 ;
    - messages `NavigateRequested`, `CombatResultsClosed`, `ReturnedToBoard` ;
    - navigation « Retour » au sous-projet 3.
  - `09` : section « Rapports de test » (où, quoi, `[Description]` obligatoire, garde-fou).
  - `Decisions.md` : 2 lignes (scènes générées + machine d'états en Application ; rapport HTML à chaque exécution).
  - `Code/SETUP.md` :
    - menu *Générer les scènes* ;
    - Play démarre par `Boot` ;
    - où lire les rapports.
- [ ] **Étape 4 : Lancer EditMode puis PlayMode.** Attendu : `failed="0"`. Synthèse en chat.
- [ ] **Étape 5 : Commit** — `test(flux): tests de navigation, docs scènes et rapports`

## Définition de « terminé »
- Le jeu démarre par `Boot`. Connexion → QG → Plateau ⇄ Combat → QG passent par `GameFlowMachine`, et chaque écran vit dans sa scène et son scope.
- Plus de `Navigator`, plus de référence entre écrans.
- Chaque exécution de tests écrit un rapport HTML. Chaque test a une description, et un garde-fou l'impose.
- EditMode et PlayMode verts. La synthèse est donnée en chat après chaque série.
