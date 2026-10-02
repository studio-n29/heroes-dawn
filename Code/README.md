# Heroes Dawn — Code

Dépôt du jeu **Heroes Dawn** (Studio N29) : Unity 6.6, architecture en couches (Clean) + MVP.
Documentation : `../00_GDD`, `../04_Tech_Unity` (architecture), `docs/plans` (plan du prototype M1).

## Arborescence
| Dossier | Contenu |
|---|---|
| `Heroes Dawn Unity/` | Projet Unity 6.6 (6000.6.3f1, URP) |
| `Heroes Dawn Unity/Packages/com.n29.heroesdawn.domain/` | Code partagé client/serveur : Core, Definitions, Domain (C# pur) |
| `Heroes Dawn Unity/Assets/_Project/` | Application, Content, Infrastructure, Presentation, Composition, Editor |
| `Heroes Dawn Unity/Assets/Tests/` | Tests EditMode (domaine, application) et PlayMode |
| `Server/` | Module Cloud Code C# (jalon M2) |
| `Tools/SharedTests/` | `dotnet test` du code partagé, hors Unity (< 10 s) |
| `Tools/BalanceSimulator/` | Simulateur d'équilibrage des combats |
| `docs/plans/` | Plans d'implémentation |

## Mise en route
Voir `SETUP.md`.

## Règles
Les 12 règles d'architecture et les conventions sont dans `../04_Tech_Unity/08_Regles_Conventions.md`.
