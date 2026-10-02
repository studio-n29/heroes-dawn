# Heroes Dawn — Code

Dépôt du jeu **Heroes Dawn** (Studio N29) : Unity 6.6, architecture en couches (Clean) + MVP.
Documentation : `../00_GDD`, `../04_Tech_Unity` (architecture), `docs/plans` (plan du prototype M1).

## Arborescence
| Dossier | Contenu |
|---|---|
| `Heroes Dawn Unity/` | Projet Unity 6.6 (6000.6.3f1, URP) |
| `Heroes Dawn Unity/Packages/com.n29.heroesdawn.domain/` | Code du domaine : Core, Definitions, Domain (C# pur, testable) |
| `Heroes Dawn Unity/Assets/_Project/` | Application, Content, Infrastructure, Presentation, Composition, Editor |
| `Heroes Dawn Unity/Assets/Tests/` | Tests EditMode (`Domain/`, `Editor/`) et PlayMode |
| `Heroes Dawn Unity/Assets/_Project/Backend/` | Backend UGS : scripts Cloud Code JS, monnaies Economy, Remote Config (déployés par *Window ▸ Deployment*) |
| `Heroes Dawn Unity/Assets/_Project/Editor/Balance/` | Outils d'équilibrage : import du tableur, simulateur (menu *Heroes Dawn ▸ Équilibrage*) |
| `docs/plans/` | Plans d'implémentation |

## Mise en route
Voir `SETUP.md`.

**Tout se passe dans Unity** (décision 2026-10-02) : aucun script ni projet .NET hors du projet Unity.

## Règles
Les 12 règles d'architecture et les conventions sont dans `../04_Tech_Unity/08_Regles_Conventions.md`.
