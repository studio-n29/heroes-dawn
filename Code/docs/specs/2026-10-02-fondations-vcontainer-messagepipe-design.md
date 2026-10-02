# Alignement architecture — Sous-projet 1 : Fondations (VContainer, MessagePipe, UniTask, use cases)

> Statut : design validé en conversation le 2026-10-02 · à relire avant le plan d'implémentation.
> Sous-projets suivants (specs séparées) : **2. Scènes et flux** (`Boot`/`Hub`/`Board`/`Combat`, `GameFlowMachine`, scopes par scène) · **3. MVP écran par écran** (vues prefabs générées puis éditables, présentateurs, `…Data`).

## 1. Intention
- **Demande :** aligner le code du prototype sur l'architecture cible (`04_Tech_Unity/02`, `04`, `07`). Les vues seront des prefabs générés une fois puis éditables dans Unity (sous-projet 3). Tout reste dans Unity.
- **Hypothèses validées :**
  - le comportement du jeu reste identique (même boucle, mêmes règles, mêmes récompenses) ;
  - le Prologue et la carte de ville restent pour le M2 ;
  - les packages viennent d'OpenUPM, avec des versions figées.
- **Réussite :**
  - la suite EditMode reste verte à chaque étape et le jeu reste jouable à chaque commit ;
  - à la fin, plus aucun service « fourre-tout » ni bus maison ;
  - les dépendances passent par VContainer, les commandes par des use cases, les événements par MessagePipe.

## 2. Périmètre
**Dans le périmètre**
- Installation de VContainer, UniTask, MessagePipe et MessagePipe.VContainer, avec les références d'assemblies.
- Un use case par intention, rangé dans `Application/Features/<Zone|Combat|Hub|Auth>/`.
- Les messages MessagePipe remplacent `GameEvents`.
- Les ports passent de `Task` à `UniTask`.
- Une interface de lecture `ISessionReadModel` pour l'UI.
- `RootLifetimeScope` remplace l'injection manuelle de `GameBootstrap`.
- L'UI actuelle (écrans construits par code) est recâblée sur les use cases et le bus.
- La mise à jour des docs et de `Decisions.md`.

**Hors périmètre** (sous-projets 2 et 3)
- Les scènes, la `GameFlowMachine`, les scopes enfants.
- Les présentateurs, les interfaces `I…View`, les prefabs.
- TextMeshPro.
- Tout changement de règle de jeu.

## 3. Approche retenue : migration progressive
VContainer et MessagePipe sont installés à côté de l'existant. On migre ensuite une fonctionnalité à la fois : Zone, Combat, QG, puis Connexion et Sauvegarde. Chaque étape est commitable, testée et jouable. `GameEvents` et les classes `*Service` disparaissent à la dernière étape.

*Rejetée :* la bascule en une fois. Le jeu resterait cassé longtemps et un échec serait difficile à localiser.

## 4. Packages (OpenUPM, registre déjà déclaré dans `manifest.json`)
| Package | Version figée |
|---|---|
| `jp.hadashikick.vcontainer` | 1.19.0 |
| `com.cysharp.unitask` | 2.5.11 |
| `com.cysharp.messagepipe` | 1.8.2 |
| `com.cysharp.messagepipe.vcontainer` | 1.8.2 |

- **Registre OpenUPM :** ajouter les scopes `jp.hadashikick` et `com.cysharp` à côté de `com.kyrylokuzyk`.
- **Références d'assemblies :**
  | Assembly | Références ajoutées |
  |---|---|
  | `Application` | `UniTask`, `MessagePipe` |
  | `Infrastructure` | `UniTask` |
  | `Presentation` | `UniTask`, `MessagePipe` |
  | `Composition` | `VContainer`, `MessagePipe`, `MessagePipe.VContainer`, `UniTask` |
  | `Tests.Domain` | `UniTask`, `MessagePipe` |
  | `Tests.PlayMode` | `VContainer`, `UniTask`, `MessagePipe` |
- **Code partagé :** `Core`, `Definitions` et `Domain.*` ne référencent **aucun** de ces packages (règle 2 de `08_Regles_Conventions`).

## 5. Application

### 5.1 Use cases
Forme commune : une interface `I<Nom>UseCase` et une classe `sealed <Nom>UseCase`, avec une méthode `ExecuteAsync(..., CancellationToken ct)`. Chaque use case suit cette séquence :
1. **vérifier**, en refusant tôt avec un `DomainError` existant ;
2. **appliquer** au domaine ou à la session ;
3. **publier** les messages ;
4. **demander une sauvegarde** si l'état persistant a changé.

Chaque ligne du tableau reproduit **exactement** la méthode de service indiquée (erreurs, montants, ordre des effets).

| Feature | Use case | Signature `ExecuteAsync` | Remplace |
|---|---|---|---|
| Zone | `StartZone` | `(ZoneId) → UniTask<Result>` | `ZoneService.StartZone` |
| Zone | `RollDice` | `() → UniTask<Result<IReadOnlyList<BoardEvent>>>` | `ZoneService.Roll` |
| Zone | `UseBonusDie` | idem | `ZoneService.UseBonusDie` |
| Zone | `MoveTo` | `(CaseId) → UniTask<Result<IReadOnlyList<BoardEvent>>>` | `ZoneService.MoveTo` |
| Zone | `EndMove` | `() → …` | `ZoneService.EndMove` |
| Zone | `ResolveMiniGame` | `(bool success) → …` | `ZoneService.ResolveMiniGame` |
| Zone | `ResolveNarrative` | `(NarrativeChoice) → …` | `ZoneService.ResolveNarrative` |
| Zone | `ResolveRest` | `(bool take) → …` | `ZoneService.ResolveRest` |
| Zone | `LeaveZone` | `() → UniTask<Result>` | `ZoneService.Abandon` et `CloseCompletedRun` (un seul : quitter la partie, terminée ou non) |
| Combat | `BeginCombat` | `() → UniTask<Result<CombatSimulation>>` | `CombatService.Begin` |
| Combat | `SetAutoCombat` | `(bool) → UniTask<Result>` | `CombatService.SetAuto` |
| Combat | `FinishCombat` | `() → UniTask<Result<CombatReport>>` | `CombatService.Finish` |
| Combat | `VerifyCombat` | `() → UniTask<bool?>` | `CombatService.VerifyLastAsync` |
| Combat | `ForceCombatWin` (debug) | `() → UniTask<Result>` | `CombatService.ForceWinForDebug` |
| Hub | `ToggleSquad` | `(MemberId) → UniTask<Result>` | `HubService.ToggleSquad` |
| Hub | `TrainMember` | `(MemberId) → UniTask<Result>` | `HubService.Train` |
| Hub | `RecruitMember` | `(MemberId) → UniTask<Result>` | `HubService.Recruit` |
| Hub | `HealRoster` | `() → UniTask<Result>` | `HubService.Heal` |
| Auth | `SignInAsGuest` | `() → UniTask<Result<SignInReport>>` | `AuthService.SignInAsGuestAsync` |
| Auth | `SignOut` | `() → UniTask<Result>` | `AuthService.SignOut` |
| Auth | `ResetProgress` | `() → UniTask<Result>` | `AuthService.ResetProgress` |

**Fonctions partagées sans état (internes au feature) :**
- `ZoneActions.Apply(GameSession, ZoneRun → events)` : encapsule le `Do` + `Apply` actuels (refus `NotAllowed`, crédit des ressources, validation de zone, `ZoneValidated`).
- `FatigueRecovery.Compute(...)` : extrait de `AuthService.RecoverFatigue`.

**État de combat en cours :** il ne vit plus dans un service. C'est un objet applicatif `ActiveCombat`, singleton, qui porte `Simulation`, `Setup`, `Case`, `Fighters` et `DebugUsed`, plus une méthode `Clear()`. Les use cases Combat le partagent, et l'UI le lit à travers `IActiveCombatReadModel`.

**Requêtes :** `HubService.AvailableRecruits` devient la méthode `AvailableRecruits()` de `ISessionReadModel`. Une requête n'est pas une intention.

### 5.2 Sauvegarde
- `SaveService` devient `SaveScheduler` (non-use case, singleton) : `void RequestSave()` et `UniTask SaveAsync(CancellationToken)`.
- Sémantique identique : écritures sérialisées, une seule relance si une demande arrive pendant une écriture.
- Il publie `SaveStateChanged` au lieu d'appeler `GameEvents`.

### 5.3 Messages (MessagePipe, `Application/Messages/`)
`readonly struct`, publiés **après** la mise à jour de l'état.

| Message | Champs | Remplace / rôle |
|---|---|---|
| `WalletChanged` | `int Credits, int Pr` | `GameEvents.WalletChanged` |
| `RosterChanged` | — | `GameEvents.RosterChanged` |
| `ToastRequested` | `string Text, ToastKind Kind` | `GameEvents.Toast` |
| `SaveStateChanged` | `bool Saving` | `GameEvents.SaveStateChanged` |
| `ZoneValidated` | `ZoneRewards Rewards` | Remplace la propriété `ZoneService.LastRewards` |
| `CombatFinished` | `CombatReport Report` | Publié par `FinishCombat` ; le rapport reste aussi renvoyé |

Les messages sont enregistrés dans `RootLifetimeScope` avec `RegisterMessageBroker<T>(options)`. L'UI s'abonne par `ISubscriber<T>` et range les abonnements dans un `DisposableBag` qu'elle libère à la destruction de l'écran.

### 5.4 Lecture de la session
`GameSession` implémente `ISessionReadModel` :
| Membre | Type |
|---|---|
| `Content` | `ContentCatalog` |
| `Account` | `PlayerAccount?` |
| `Wallet` | `IWalletReadModel` (`Credits`, `Pr`) |
| `Roster` | `IReadOnlyList<ITeamMemberReadModel>` |
| `Squad` | `IReadOnlyList<ITeamMemberReadModel>` |
| `Run` | `ZoneRun?` |
| `RunZone` | `ZoneDef?` |
| `AutoCombat` | `bool` |
| `CompletedZones` | `IReadOnlyCollection<string>` |
| `AvailableRecruits()` | `IReadOnlyList<MemberDef>` |

- `ITeamMemberReadModel` expose `Id`, `Def`, `Level`, `Fatigue`, `InSquad`.
- L'UI ne reçoit **que** `ISessionReadModel`, jamais `GameSession`.
- `ZoneRun` reste exposé tel quel, ses méthodes de mutation comprises. Une revue de code interdit à l'UI de les appeler ; le modèle de lecture du plateau arrive au sous-projet 3.

### 5.5 Ports
- `IAuthGateway`, `IProfileGateway` et `ICombatVerifier` passent à `UniTask`.
- `ISeedProvider`, `IClock` et `ILogger` sont inchangés.
- Les adaptateurs de `Infrastructure/Simulated` sont adaptés en conséquence.

## 6. Composition
- `Composition/RootLifetimeScope : LifetimeScope` enregistre :
  - le contenu (`PrototypeContent.Build(balance)`, chargé comme aujourd'hui depuis `balance.json`) ;
  - `GameSession`, comme lui-même et comme `ISessionReadModel` ;
  - `ActiveCombat`, comme lui-même et comme `IActiveCombatReadModel` ;
  - `SaveScheduler` ;
  - les ports avec leurs adaptateurs simulés ;
  - tous les use cases (`Lifetime.Singleton`, `.AsImplementedInterfaces()`) ;
  - MessagePipe (`RegisterMessagePipe()` + un broker par message) ;
  - `UiRoot`, créé par un point d'entrée `IStartable`.
- **Démarrage inchangé pour le joueur :** `GameBootstrap.AutoStart` (`RuntimeInitializeOnLoadMethod`) crée le GameObject `[Heroes Dawn]` avec `RootLifetimeScope`, en `DontDestroyOnLoad`. Les scènes arrivent au sous-projet 2.
- `UiServices` porte les interfaces des use cases, `ISessionReadModel`, `IActiveCombatReadModel` et `ISubscriber<…>`, au lieu des services. Les écrans restent construits par code.
- La sauvegarde à la mise en pause et à la fermeture de l'application passe par `SaveScheduler`.

## 7. Gestion des erreurs
- **Refus métier :** `Result.Fail(DomainError)` avec les mêmes codes qu'aujourd'hui. L'UI affiche la clé de localisation, comme avant.
- **Erreur d'un port** (exception réseau ou IO) : interceptée dans le use case, journalisée par `ILogger`, renvoyée en `DomainError` (`auth.network`, `profile.load`, comme dans `AuthService`).
- **Annulation :** `OperationCanceledException` remonte toujours, elle n'est jamais convertie en `Result`.
- **Sauvegarde :** une erreur est journalisée et ne bloque jamais une action de jeu (comportement actuel).

## 8. Tests
- **Use cases, en EditMode** (`Tests/Domain/Application/…`) :
  - ports factices et vrai bus MessagePipe, construit sans Unity avec `BuiltinContainerBuilder` ;
  - un test par use case au minimum : succès, refus principal, message publié.
- **Tests existants :**
  - `ApplicationFlowTests` est réécrit pour passer uniquement par les use cases : connexion, QG, zone, combat, sauvegarde ;
  - `PrototypeDomainTests` et les tests d'outils sont inchangés.
- **Garde-fou :** un test EditMode vérifie par réflexion que les assemblies `Core`, `Definitions` et `Domain.*` ne référencent ni `VContainer`, ni `MessagePipe`, ni `UniTask`.
- **PlayMode :**
  - `Boot_ShowsLoginScreen` : le jeu démarre, `RootLifetimeScope` est construit et l'écran de connexion est affiché ;
  - `GuestSignIn_ShowsHub` : la connexion invité affiche le QG.
- **Commande :** les tests passent en batchmode, Unity fermé (voir `Code/SETUP.md`).

## 9. Ordre des étapes (une étape = un ou plusieurs commits, suite verte, jeu jouable)
1. Packages et références, `RootLifetimeScope` qui enregistre les **services actuels**, PlayMode `Boot_ShowsLoginScreen`. Aucun changement visible.
2. Messages MessagePipe et `SaveScheduler`. `GameEvents` est retiré, l'UI s'abonne au bus.
3. Use cases Zone, `ISessionReadModel`, recâblage du plateau.
4. Use cases Combat et `ActiveCombat`.
5. Use cases Hub.
6. Use cases Auth et ports en `UniTask`. PlayMode `GuestSignIn_ShowsHub`.
7. Suppression des classes `*Service` et de leurs références dans `UiServices` (qui reste jusqu'au sous-projet 3, recâblé sur les use cases). Garde-fou des dépendances. Mise à jour des docs (`02`, `04`, `07` si besoin), de `Decisions.md` (versions figées) et de `Code/README.md`.

## 10. Risques
| Risque | Parade |
|---|---|
| Résolution OpenUPM ou PackageCache bloquée (accès refusé déjà vu) | L'étape 1 commence par l'installation ; relance après purge de `Library/PackageCache/.tmp-*` |
| UniTask et `async void` dans l'UI | L'UI appelle `ExecuteAsync(...).Forget()` uniquement dans des gestionnaires de clic, après avoir bloqué les entrées ; les erreurs sont journalisées par `UniTaskScheduler.UnobservedTaskException` |
| Régression silencieuse de règles pendant le découpage | Chaque use case est testé avec les mêmes valeurs que le service qu'il remplace, avant la suppression du service |
| Abonnements MessagePipe non libérés (fuites entre deux sessions de jeu) | `DisposableBag` libéré dans `OnDestroy` de chaque écran |
