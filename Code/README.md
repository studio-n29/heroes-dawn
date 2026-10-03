# Heroes Dawn — Code

Dépôt du jeu **Heroes Dawn** (Studio N29) : Unity 6.6, architecture en couches (Clean) + MVP.
Documentation : `../00_GDD`, `../04_Tech_Unity` (architecture), `docs/plans` (plan du prototype M1).

## Arborescence
| Dossier | Contenu |
|---|---|
| `Heroes Dawn Unity/` | Projet Unity 6.6 (6000.6.3f1, URP) |
| `Heroes Dawn Unity/Packages/com.n29.heroesdawn.domain/` | Code du domaine : Core, Definitions, Domain (C# pur, testable) |
| `Heroes Dawn Unity/Assets/_Project/` | Application, Content, Infrastructure, Presentation, Composition, Editor |
| `Heroes Dawn Unity/Assets/Tests/` | Tests EditMode (`Domain/`, `Presentation/`, `Editor/`) et PlayMode |
| `Heroes Dawn Unity/Assets/_Project/Prefabs/` | Écrans, popups et widgets en prefabs (générés par les recettes de `Editor/Prefabs/Recipes`, puis retouchables) |
| `Heroes Dawn Unity/Assets/_Project/Audio/` | Musiques (`Music/<thème>`) et effets (`Sfx/<effet>`, Kenney CC0) ; catalogue `Settings/AudioCatalog.asset` reconstruit par *Heroes Dawn ▸ Setup ▸ Audio* |
| `Heroes Dawn Unity/Assets/_Project/Backend/` | Backend UGS : scripts Cloud Code JS, monnaies Economy, Remote Config (déployés par *Window ▸ Deployment*) |
| `Heroes Dawn Unity/Assets/_Project/Editor/Balance/` | Outils d'équilibrage : import du tableur, simulateur (menu *Heroes Dawn ▸ Équilibrage*) |
| `docs/plans/` | Plans d'implémentation |

## Mise en route
Voir `SETUP.md`.

**Tout se passe dans Unity** (décision 2026-10-02) : aucun script ni projet .NET hors du projet Unity.

**Architecture** : use cases dans `Application/Features/<Feature>/` (un par intention, `UniTask<Result>`), messages dans `Application/Messages/` (MessagePipe), composition dans `Composition/RootLifetimeScope.cs` (VContainer). L'UI est en MVP : vues `MonoBehaviour` dans des prefabs (`…View`), présentateurs C# purs (`…Presenter`) qui lisent la session par `ISessionReadModel` et appellent les use cases ; voir `../04_Tech_Unity/07_Presentation_Flux.md`.

## Règles
Les 12 règles d'architecture et les conventions sont dans `../04_Tech_Unity/08_Regles_Conventions.md`.
