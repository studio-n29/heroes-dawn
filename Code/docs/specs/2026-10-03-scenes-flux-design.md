# Alignement architecture — Sous-projet 2 : Scènes et flux de jeu (+ rapports de test)

> Statut : design validé en conversation le 2026-10-03 · à relire avant le plan d'implémentation.
> Précédent : sous-projet 1 (fondations : use cases, MessagePipe, `RootLifetimeScope`), fusionné dans `integration/tout-dans-unity`.
> Suivant : sous-projet 3 (écrans MVP en prefabs générés, présentateurs, navigation « Retour »).
> Références : `04_Tech_Unity/07_Presentation_Flux.md` (scènes, scopes, machine d'états), `03_Art_UI_UX/03_Maquettes_Screen_Flow.md` (screen flow F01).

## 1. Intention
- **Demande :**
  - donner au jeu la structure de scènes et la navigation de la doc 07 ;
  - garder les écrans actuels, construits par code, en les rangeant chacun dans sa scène ;
  - produire un rapport à chaque exécution de tests : un fichier HTML généré par Unity, plus une synthèse en chat.
- **Hypothèses validées :**
  - le joueur ne voit aucune différence ;
  - périmètre du prototype : Titre, QG, Plateau, Combat (les résultats restent des fenêtres) ;
  - carte de la ville, préparation, prologue et « connexion perdue » : M2.
- **Réussite :**
  - seule la machine d'états charge, décharge, affiche ou masque les scènes ;
  - chaque scène a son conteneur VContainer, et ses abonnements disparaissent avec elle ;
  - le jeu démarre par `Boot` ;
  - suites EditMode et PlayMode vertes, avec un rapport HTML à chaque exécution.

## 2. Périmètre
**Dans le périmètre**
1. L'outil de rapport de test, et une description (`[Description]`) sur chaque test.
2. Les scènes `Boot`, `Hub`, `Board`, `Combat`, générées par un menu éditeur et inscrites dans les Build Settings.
3. Les scopes `HubLifetimeScope`, `BoardLifetimeScope` et `CombatLifetimeScope`.
4. La machine d'états `GameFlowMachine`, avec un port de chargement de scènes.
5. Les messages de navigation, à la place des appels directs entre écrans.
6. Le démarrage par `Boot`, sans démarrage automatique depuis une autre scène.
7. La mise à jour des docs 07 et 09, de `Decisions.md` et de `Code/SETUP.md`.

**Hors périmètre**
- Prefabs, présentateurs, `I…View` (sous-projet 3).
- La navigation « Retour » (◄, Échap, geste Android) : elle sera rattachée aux écrans du sous-projet 3.
- Les écrans M2 et le chargement Addressables.
- Tout changement de règle de jeu.

## 3. Approche retenue : scènes générées par un script éditeur
Un menu *Heroes Dawn ▸ Setup ▸ Générer les scènes* (exécutable en batchmode) crée ou recrée les 4 scènes de façon déterministe : objets, composants, références de scope parent. Il les inscrit ensuite dans les Build Settings, dans l'ordre `Boot`, `Hub`, `Board`, `Combat`. Les scènes générées sont commitées et pourront ensuite être retouchées à la main.

*Rejetées :*
- les scènes montées à la main, car je ne pourrais ni les créer ni les vérifier en batchmode ;
- les scopes enfants sans scènes, qui s'écartent de la doc 07 et laisseraient le sous-projet 3 sans scènes.

## 4. Rapports de test

### 4.1 Générateur
- `Editor/Testing/TestReportWriter` (`[InitializeOnLoad]`) s'abonne au Test Runner par `TestRunnerApi.RegisterCallbacks`. Il fonctionne aussi en batchmode `-runTests`.
- À chaque fin d'exécution (`RunFinished`), il écrit `Code/docs/test-reports/<aaaa-MM-jj_HH-mm-ss>_<EditMode|PlayMode>.html` et `Code/docs/test-reports/latest-<EditMode|PlayMode>.html`.
- **Contenu :**
  - un en-tête : date, plateforme, total, réussis, échoués, ignorés, durée, branche et commit Git ;
  - un tableau par classe de test, avec une ligne par test : nom lisible, **ce qu'il vérifie et pourquoi** (sa description), résultat (✔ / ✘ / ignoré), durée ;
  - pour un échec, le message et la première ligne de pile.
- **Lisibilité :** page autonome (CSS en ligne, sans dépendance), lisible en clair comme en sombre.
- **Git :** seuls les `latest-*.html` sont commités. Les rapports datés sont ignorés et servent d'historique local.
- **Robustesse :** le rapport ne bloque jamais l'exécution. Une erreur d'écriture est journalisée en avertissement.

### 4.2 Descriptions
- Chaque méthode de test (`[Test]`, `[UnityTest]`, `[TestCase]`) porte un `[Description("…")]` de la forme « Vérifie que … — parce que … ». La partie « parce que » cite la règle, la spec ou le risque couvert.
- Le garde-fou `TestDescriptionsTests.EveryTestHasADescription` (EditMode, `Tests.Editor`) parcourt par réflexion les assemblies de tests chargées. Il échoue en listant toute méthode de test sans description non vide.
- Les 81 tests existants reçoivent leur description à la première tâche du plan.

### 4.3 Synthèse en chat
Après chaque série de tests, une synthèse donne :
- les totaux ;
- ce que la série prouve, regroupé par fonctionnalité ;
- les échecs avec leur cause ;
- le chemin du rapport HTML.

## 5. Scènes et scopes

| Scène | Chargement | Scope (parent) | Contenu |
|---|---|---|---|
| `Boot` | Seule, index 0 | `RootLifetimeScope` (aucun) | `[Heroes Dawn]` (`GameBootstrap`, scope racine), EventSystem, interface commune `UiRoot` : écran de connexion, fenêtres, messages |
| `Hub` | Additive | `HubLifetimeScope` (Root) | Canvas de scène + `HubScreen` |
| `Board` | Additive | `BoardLifetimeScope` (Root) | Canvas de scène + `BoardScreen` |
| `Combat` | Additive, par-dessus `Board` | `CombatLifetimeScope` (**Board**) | Canvas de scène + `CombatScreen` |

- **Parent des scopes :** il est désigné dans la scène générée par le type du scope parent (`parentReference`). VContainer le résout parmi les scopes chargés.
- **Écran de chaque scène :** chaque scope de scène crée son écran sous le canvas de la scène, dans un rappel de construction, comme `UiRoot` aujourd'hui. Il l'enregistre (`RegisterComponent`) pour la machine d'états.
- **Ordre d'affichage :**
  | Élément | `sortingOrder` |
  |---|---|
  | Canvas des écrans de `Boot` (connexion) | 10 |
  | Canvas de `Hub` et de `Board` | 10 |
  | Canvas de `Combat` | 20 |
  | Couche commune fenêtres et messages (`Boot`) | 100 |
- **`UiContext`** ne référence plus aucun écran. Il garde les services, les fenêtres, les messages et une méthode `Navigate(GameState target)` qui publie `NavigateRequested`. La classe `Navigator` disparaît.

## 6. Flux de jeu

### 6.1 États et transitions
`GameState` : `Boot`, `Login`, `Hub`, `Board`, `Combat`.

| De → vers | Déclencheur | Effet sur les scènes |
|---|---|---|
| Boot → Login | Fin de construction du scope racine | Affiche l'écran de connexion |
| Login → Hub | Connexion réussie | Masque la connexion, charge `Hub` |
| Hub → Login | Déconnexion | Décharge `Hub`, affiche la connexion |
| Hub → Board | Déploiement réussi (`StartZone`) | Décharge `Hub`, charge `Board` |
| Board → Hub | Retour QG, zone validée (« Retour au QG ») ou partie absente | Décharge `Board`, charge `Hub` |
| Board → Combat | Case de combat (`BeginCombat` réussi) | Charge `Combat`, masque et met en pause `Board` |
| Combat → Board | Fermeture des résultats, escouade apte | Décharge `Combat`, réaffiche `Board`, puis publie `ReturnedToBoard(report)` |
| Combat → Hub | Fermeture des résultats, retour au QG (`ReturnedToHq`) | Décharge `Combat` et `Board`, charge `Hub` |

- **Demande refusée :** une transition absente du tableau est refusée. L'état ne change pas et l'événement est journalisé en avertissement.
- **Pendant une transition :** une demande reçue alors qu'une transition est en cours est ignorée (double clic, double message).

### 6.2 Machine d'états (Application, testable sans Unity)
- `Application/Flow/GameState.cs`.
- `IGameFlow` : `GameState Current`, `bool IsTransitioning`, `UniTask<bool> RequestAsync(GameState target, CancellationToken ct)`. Renvoie `false` si la demande est refusée ou ignorée.
- `GameFlowMachine : IGameFlow, IDisposable` :
  - s'abonne à `NavigateRequested` et `CombatResultsClosed` ;
  - exécute les effets du tableau via le port `ISceneLoader` ;
  - publie `GameStateChanged` après chaque transition réussie, et `ReturnedToBoard` pour Combat → Board.
- Port `ISceneLoader` (Application/Ports) :
  - `UniTask LoadAsync(string scene, CancellationToken ct)` (additif) ;
  - `UniTask UnloadAsync(string scene, CancellationToken ct)` ;
  - `void SetVisible(string scene, bool visible)`.
- Adaptateur `Infrastructure/Platform/UnitySceneLoader` :
  - `SceneManager` + UniTask ;
  - `SetVisible` active ou désactive les objets racines de la scène, sauf son scope. L'écran reçoit `OnHide`/`OnShow` et l'`Update` du plateau s'arrête : c'est la pause.
- L'écran de connexion est affiché ou masqué par la machine via `IScreenVisibility` (interface dans `Application/Flow`, implémentée par `UiRoot`), puisqu'il vit dans `Boot`.

### 6.3 Messages (Application/Messages/FlowMessages.cs)
| Message | Champs | Publié par |
|---|---|---|
| `NavigateRequested` | `GameState Target` | Écrans (via `UiContext.Navigate`) |
| `CombatResultsClosed` | `CombatReport Report` | `CombatScreen` (bouton « Continuer ») |
| `ReturnedToBoard` | `CombatReport Report` | `GameFlowMachine`, après réaffichage de `Board` |
| `GameStateChanged` | `GameState From, To` | `GameFlowMachine` |

### 6.4 Écrans (changements limités au câblage)
| Écran | Appel actuel | Remplacé par |
|---|---|---|
| `LoginScreen` | `Nav.Show(Hub)` | `Navigate(Hub)` |
| `HubScreen` | `Nav.Show(Board)` après déploiement | `Navigate(Board)` |
| `HubScreen` | `Nav.Show(Login)` après déconnexion | `Navigate(Login)` |
| `BoardScreen` | `Nav.Show(Hub)` (partie absente, retour, zone validée) | `Navigate(Hub)` |
| `BoardScreen` | `Nav.Show(Combat)` | `Navigate(Combat)` |
| `BoardScreen` | `OnCombatReport` | Devient le gestionnaire de `ReturnedToBoard`, abonné pour la vie de l'écran |
| `CombatScreen` | `Nav.Show(Board)` (aucun combat) | `Navigate(Board)` |
| `CombatScreen` | « Continuer » appelle `Board.OnCombatReport(report)` | Publie `CombatResultsClosed(report)` |

Les gardes `Ctx.Nav.Current != this` deviennent `IGameFlow.Current == GameState.Board`.

## 7. Démarrage
- `GameBootstrap` devient un composant posé dans `Boot`. Le `RuntimeInitializeOnLoadMethod` est supprimé.
- `RootLifetimeScope` est posé sur le même objet, avec `autoRun`.
- **Éditeur :** `Editor/BootSceneStarter` (`[InitializeOnLoad]`) règle `EditorSceneManager.playModeStartScene = Boot`, pour que Play démarre toujours par `Boot`. Le réglage est retiré pendant une exécution du Test Runner (`ICallbacks.RunStarted`/`RunFinished`), pour ne pas perturber les tests PlayMode.
- **Tests PlayMode :**
  - ils chargent `Boot` avec `SceneManager.LoadScene("Boot")` à l'intérieur de `LifetimeScope.Enqueue`, pour injecter les doubles ;
  - ils attendent l'écran de connexion ;
  - en `TearDown`, ils déchargent tout et détruisent les objets persistants.

## 8. Gestion des erreurs
| Situation | Comportement |
|---|---|
| Échec de chargement d'une scène (exception de `ISceneLoader`) | La machine journalise une erreur, publie `ToastRequested(errors.navigation, Error)` et revient à l'état de départ (scènes rechargées comme avant). Pas d'état intermédiaire bloqué. |
| `NavigateRequested` vers l'état courant | Ignoré sans avertissement |
| Transition non prévue | Refusée, avertissement journalisé |

## 9. Tests

### 9.1 EditMode (`Tests.Domain`), machine d'états avec un `FakeSceneLoader` qui enregistre les appels
- chaque transition du tableau 6.1 : appels attendus au chargeur, état final, `GameStateChanged` publié ;
- transition non prévue refusée (Hub → Combat) ;
- demande pendant une transition ignorée ;
- Combat → Board publie `ReturnedToBoard` après `SetVisible(Board, true)` ;
- Combat → Hub décharge `Combat` puis `Board` ;
- échec de chargement : retour à l'état de départ et message d'erreur.

### 9.2 EditMode (`Tests.Editor`)
- `EveryTestHasADescription` ;
- génération de rapport : `TestReportWriter.Render(résultat factice)` contient le nom, la description, ✔/✘ et le message d'échec, et échappe le HTML ;
- générateur de scènes : les 4 scènes existent dans les Build Settings, dans l'ordre, avec le bon type de scope et le bon parent.

### 9.3 PlayMode (remplacent et étendent `BootFlowTests`)
- démarrage : `Boot` affiche la connexion, les doubles sont injectés ;
- connexion → QG : `Hub` chargée, connexion masquée ;
- QG → Plateau → Combat → Plateau : `Combat` est chargée par-dessus `Board`, puis déchargée, et `Board` est réaffichée avec le rapport traité ;
- abandon depuis le plateau → QG ;
- double demande de navigation → une seule transition ;
- après le déchargement de `Hub`, publier `WalletChanged` ne rafraîchit plus aucun écran de QG détruit (abonnements libérés) ;
- les tests existants (double clic sur Jouer, abonnements après déconnexion) restent verts.

## 10. Ordre des étapes
1. Rapport de test et descriptions des 81 tests existants, avec le garde-fou.
2. Machine d'états, messages et port `ISceneLoader`, avec les tests EditMode (pas encore branchés sur Unity).
3. Générateur de scènes, scopes de scène et `UnitySceneLoader`. `Boot` remplace le démarrage automatique.
4. Recâblage des écrans sur la navigation par messages ; suppression de `Navigator` et des références d'écrans dans `UiContext`.
5. Tests PlayMode du flux, docs (07, 09, `Decisions.md`, `Code/SETUP.md`).

## 11. Risques
| Risque | Parade |
|---|---|
| `playModeStartScene` perturbe les tests PlayMode | Réglage retiré pendant les exécutions du Test Runner (étape 3), et vérifié par la suite PlayMode |
| Le scope de `Combat` ne trouve pas `BoardLifetimeScope` comme parent | Test du générateur sur `parentReference`, plus le test PlayMode Plateau → Combat |
| La mise en pause par désactivation déclenche `OnDisable`/`OnHide` et libère des abonnements nécessaires (validation de zone pendant le combat) | Les abonnements à vie de `BoardScreen` sont pris dans `Build` et libérés dans `OnDestroy`, pas dans `OnHide` ; test PlayMode d'une victoire qui valide la zone |
| Rapports HTML qui gonflent le dépôt | Seuls les `latest-*.html` sont commités |
| Callbacks du Test Runner absents en batchmode | Vérifié dès l'étape 1 : le rapport doit apparaître après un `-runTests` |
