# Couches, assemblies et arborescence

## Espace de noms racine
`N29.HeroesDawn` — chaque assembly ajoute son suffixe (`N29.HeroesDawn.Domain.Combat`). **Espace de noms = assembly = dossier.**

## Assemblies (.asmdef)
| Assembly | Couche | Unity ? | Références autorisées | Contenu |
|---|---|---|---|---|
| `N29.HeroesDawn.Core` | Core | **Non** (`noEngineReferences`) | — | Ids typés, `Fixed`, `IRandom`/`Pcg32`, `Result<T>`, `IDomainEvent`, collections déterministes |
| `N29.HeroesDawn.Definitions` | Core | Non | Core | Définitions immuables du contenu (`MemberDef`, `SkillDef`, `ZoneDef`…) et `BalanceConfig` |
| `N29.HeroesDawn.Domain.*` (un par module) | Domaine | Non | Core, Definitions, autres modules **listés** (voir graphe) | État + règles + simulation |
| `N29.HeroesDawn.Application` | Application | Non | Core, Definitions, Domain.* | Use cases, ports, `GameSession`, handlers |
| `N29.HeroesDawn.Infrastructure` | Infrastructure | Oui | Application, Domain.*, Definitions, Core + SDK UGS, Addressables, IAP, Steamworks | Adaptateurs des ports |
| `N29.HeroesDawn.Content` | Infrastructure | Oui | Definitions, Core | ScriptableObjects d'édition + conversion vers Definitions |
| `N29.HeroesDawn.Presentation` | Présentation | Oui | Application (interfaces + événements), Definitions, Core, PrimeTween, MessagePipe | Vues, présentateurs, UI, audio |
| `N29.HeroesDawn.Composition` | Composition | Oui | **Tout** | LifetimeScopes VContainer, GameFlow, point d'entrée |
| `N29.HeroesDawn.Editor` | Outils | Éditeur | Tout | Éditeur de plateau, import d'équilibrage, simulateur |
| `N29.HeroesDawn.Tests.Domain` | Tests | Non | Core, Definitions, Domain.*, Application | Tests unitaires (aussi lancés avec `dotnet test`) |
| `N29.HeroesDawn.Tests.PlayMode` | Tests | Oui | Tout | Tests d'intégration en scène |

Réglages des assemblies pures : `noEngineReferences: true`, `autoReferenced: false`, `overrideReferences: true` (aucune DLL implicite).

## Graphe de dépendances
```mermaid
flowchart LR
  Composition --> Presentation & Infrastructure & Application
  Presentation --> Application
  Infrastructure --> Application
  Infrastructure --> Content
  Content --> Definitions
  Application --> Domain
  subgraph Domain["Domaine (un assembly par module)"]
    Board --> Economy
    Board --> Narrative
    Combat --> Roster
    Recruitment --> Roster
    Recruitment --> Economy
    Headquarters --> Roster
    Headquarters --> Economy
    Expeditions --> Roster
    Expeditions --> Economy
    Campaign --> Board
    Campaign --> Narrative
    Pass --> Economy
    MiniGames --> Roster
  end
  Domain --> Definitions --> Core
```

**Règles du graphe**
- Aucune flèche vers le haut. Aucune dépendance circulaire (Unity la refuse de toute façon entre assemblies).
- Deux modules du domaine qui ont besoin l'un de l'autre → l'orchestration remonte dans **Application**.
- `Presentation` ne référence **jamais** `Infrastructure` ni un `Domain.*` en écriture : elle lit des **vues en lecture seule** (interfaces `IReadOnly…`) et appelle des **use cases**.

## Partage avec le serveur
Le code de `Core`, `Definitions` et `Domain.*` vit dans un **package local** et est compilé deux fois :
- par Unity via les `.asmdef` ;
- par le module Cloud Code C# via un `.csproj` qui inclut les mêmes fichiers source (`<Compile Include="..\..\Packages\com.n29.heroesdawn.domain\**\*.cs" />`).

Cible commune : **netstandard2.1**, C# 9 (pas de fonctionnalités plus récentes dans le code partagé).

## Arborescence du dépôt
```
HeroesDawn/
├─ Unity/                                  ← projet Unity 6.6
│  ├─ Assets/
│  │  ├─ _Project/
│  │  │  ├─ Composition/                   N29.HeroesDawn.Composition
│  │  │  │  ├─ Scopes/                     RootLifetimeScope, HubScope, BoardScope, CombatScope
│  │  │  │  └─ Flow/                       GameFlowMachine, états
│  │  │  ├─ Application/                   N29.HeroesDawn.Application
│  │  │  │  ├─ Ports/                      IBackendGateway, ISaveGateway, IClock, IContentProvider…
│  │  │  │  ├─ Session/                    GameSession, ProfileSnapshot
│  │  │  │  └─ Features/<Feature>/         UseCases, Handlers, Messages
│  │  │  ├─ Infrastructure/                N29.HeroesDawn.Infrastructure
│  │  │  │  ├─ Ugs/                        Auth, CloudCode, CloudSave, Economy, RemoteConfig, Analytics
│  │  │  │  ├─ Platform/                   Steam, GooglePlay, Apple, IAP
│  │  │  │  ├─ Content/                    AddressablesContentProvider
│  │  │  │  └─ Services/                   Localization, Audio backend, Logging
│  │  │  ├─ Content/                       N29.HeroesDawn.Content (ScriptableObjects d'édition)
│  │  │  ├─ Presentation/                  N29.HeroesDawn.Presentation
│  │  │  │  ├─ Common/                     ViewBase, UI kit comics, pools, transitions
│  │  │  │  └─ Features/<Feature>/         Views/, Presenters/
│  │  │  ├─ Editor/                        N29.HeroesDawn.Editor
│  │  │  ├─ Art/  Audio/  Prefabs/  Scenes/  Localization/
│  │  │  └─ AddressableGroups/             Core, Hub, Board, Combat, City_NewYork…
│  │  └─ Tests/                            Domain/, PlayMode/
│  └─ Packages/
│     └─ com.n29.heroesdawn.domain/        ← code partagé
│        ├─ Core/
│        ├─ Definitions/
│        └─ Domain/<Module>/
├─ Server/
│  ├─ HeroesDawn.CloudCode/                module Cloud Code C# (.csproj, référence le code partagé)
│  └─ HeroesDawn.CloudCode.Tests/
├─ Tools/
│  ├─ BalanceSimulator/                    console .NET : simulations de combat en lot
│  └─ BalanceImport/                       xlsx → JSON Remote Config
└─ Docs/  → lien vers « Documents/Heroes Dawn »
```

## Organisation d'un module du domaine (exemple : Combat)
```
Domain/Combat/
├─ N29.HeroesDawn.Domain.Combat.asmdef
├─ State/          CombatState, UnitState, TeamSide, StatusInstance
├─ Rules/          AtbRules, DamageFormula, StatusRules, ComboRules, TargetingRules
├─ Ai/             AllyAutoAi, EnemyAi (par rang), BossPhases
├─ Simulation/     CombatSimulation, CombatCommand, CombatInputLog
├─ Events/         CombatEvent (UnitActed, DamageDealt, StatusApplied, ComboTriggered…)
└─ Api/            ICombatReadModel, CombatResult, CombatSetup
```

## Organisation d'une fonctionnalité (Application + Présentation)
```
Application/Features/Recruitment/
├─ UseCases/      RecruitMemberUseCase, RefreshPoolUseCase
├─ Messages/      MemberRecruited, PoolRefreshed (événements publiés sur le bus)
└─ Handlers/      (réactions transverses, voir 04_Communication)

Presentation/Features/Recruitment/
├─ Views/         RecruitmentScreenView, RecruitCardView
└─ Presenters/    RecruitmentPresenter
```
