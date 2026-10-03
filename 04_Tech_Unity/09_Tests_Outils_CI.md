# Tests, outils et intégration continue

## Pyramide de tests
| Niveau | Outil | Portée | Quand |
|---|---|---|---|
| **Unitaires du domaine** | NUnit (Unity Test Framework), dans l'éditeur ou en batchmode | Règles : dé, accessibilité, brouillard, dégâts, statuts, combos, IA, économie, timers | À chaque sauvegarde en local, à chaque push |
| **Rejeu (golden tests)** | NUnit | Journaux de combat et de zone enregistrés → empreinte attendue | À chaque push ; un rejeu cassé = changement de règles à assumer (nouvelle `RulesVersion`) |
| **Application** | NUnit + ports factices | Use cases : succès, refus serveur, réseau coupé | À chaque push |
| **Outils et backend** | NUnit (assembly `Tests.Editor`) | Import du tableur, simulateur, format des fichiers `Backend/` | À chaque push |
| **Présentateurs** | NUnit (assembly `Tests.Presentation`) + doubles de vue (`Fakes/`) | Chaque présentateur sans Unity : ce qu'il affiche, les use cases qu'il appelle, garde contre le double clic, Retour, modales | À chaque push |
| **Boutique éphémère** | NUnit (`ShopRulesTests`, `ShopUseCaseTests`, `HubPresenterTests`) + horloge réglable (`TestApp.Time.LocalNow`) ; PlayMode `ShopFlowTests` | Jour de boutique à 9h00, graine, tirage pondéré (10 000 tirages), conditions d'entrée, achat, renouvellement, boutique expirée, sauvegarde v3 | À chaque push |
| **Prefabs et scènes** | NUnit (assembly `Tests.Editor`) | Générateur de prefabs (pas d'écrasement, retouche conservée), structure des prefabs, `ScreenCatalog`, scènes générées, aucune interface construite par code (`NoCodeBuiltScreensTests`) | À chaque push |
| **PlayMode** | Unity Test Framework | Flux de jeu sur les vraies scènes : connexion, QG, plateau, combat, résultats, Retour (`BackNavigationTests`) et une partie complète au clic (`FullRunTests`) ; captures d'écran à la demande (`ScreenCaptureTests`, explicite) | Nightly |
| **Contenu** | Validateur | Références, localisation, pools, objectifs atteignables | À chaque push touchant le contenu |
| **Manuels** | Matrice d'appareils | Performance, lisibilité, sensations | Fin de chaque jalon |

Objectif de couverture : **≥ 80 %** sur `Domain.*`, pas d'objectif chiffré sur la présentation.

### Développement piloté par les tests (domaine)
Toute règle du domaine s'écrit **test d'abord** : le test cite la règle de la spec, échoue, puis la règle est implémentée. Exemple :
```csharp
[Test] // GDD §5 — arrêt forcé sur une case Combat
public void Move_StopsOnCombatCase_AndStoresRemainingPm()
{
    var run = ZoneRunBuilder.Line("start", "res", "combat", "res2").At("start").Build();
    var result = MovementRules.Move(run, pm: 4, target: "res2");
    Assert.That(result.StoppedAt, Is.EqualTo(new CaseId("combat")));
    Assert.That(result.RemainingPm, Is.EqualTo(2));
}
```

## Rapports de test
- À chaque exécution du Test Runner (éditeur ou batchmode `-runTests`), `Editor/Testing/TestRunReporter` écrit un rapport HTML : `Code/docs/test-reports/<date>_<EditMode|PlayMode>.html` et `latest-<EditMode|PlayMode>.html` (seuls les `latest-*` sont versionnés).
- Pour chaque test : ce qu'il vérifie et pourquoi, résultat, durée, et pour un échec le message et la première ligne de pile ; en-tête avec totaux, branche et commit.
- **Description obligatoire** : chaque test porte `[Description("Vérifie que … — parce que …")]` ; le « parce que » cite la règle (GDD/spec), le point de Review Focus ou le risque couvert. Le garde-fou `TestDescriptionsTests.EveryTestHasADescription` échoue sinon.
- Après chaque série de tests, une synthèse est faite en chat (ce que la série prouve, échecs et causes, chemin du rapport).
- Pour prouver qu'un test ajouté après coup sait échouer : casser temporairement le code qu'il couvre (mutation), constater l'échec, restaurer.

## Outils éditeur
| Outil | Rôle |
|---|---|
| **Éditeur de plateau** | Fenêtre dédiée : placer les nœuds, tracer les liens, assigner types/pools/cases fixes/objectifs ; aperçu du brouillard et des destinations ; validation (pas d'impasse, ≥ 3 embranchements) |
| **Validateur de contenu** | Menu + CI : contrôle de toutes les définitions |
| **Lecteur de journaux** | Ouvre un journal de combat/zone (envoyé par un joueur ou un test) et le rejoue pas à pas dans l'éditeur |
| **Panneau de debug** (builds de dev) | Ajouter des monnaies (serveur de dev uniquement), forcer un résultat de dé, sauter les timers, afficher les ticks ATB |
| **Simulateur d'équilibrage** (*Heroes Dawn ▸ Équilibrage ▸ Simulateur*) | Rejoue des milliers de combats avec le domaine partagé : taux de victoire par composition d'équipe, durée moyenne, usage des combos, puissance de chaque recrue ; export CSV dans `BalanceReports/` |
| **Import du tableur** (*Heroes Dawn ▸ Équilibrage ▸ Importer*) | Lit l'onglet « Paramètres » de `05_Equilibrage.xlsx` → `balance.json` (client) + `balance.rc` (Remote Config) |

Tout est dans Unity (décision 2026-10-02) : aucun outil ni projet .NET hors du projet Unity.

## Environnements
| Environnement | Usage |
|---|---|
| `dev` | Développement, panneau de debug actif |
| `staging` | Tests de build, soft launch interne |
| `production` | Joueurs |
Environnements UGS séparés ; le build sélectionne le sien à la compilation.

## Intégration continue (GitHub Actions)
```mermaid
flowchart LR
  PUSH[Push / PR] --> T3[Unity EditMode<br/>GameCI : domaine, outils, backend, validateur]
  T3 --> OK{Vert ?}
  OK -->|PR| MERGE[Fusion autorisée]
  NIGHT[Chaque nuit] --> PM[PlayMode tests] --> B[Builds Android + Windows<br/>vers staging]
  TAG[Tag de version] --> REL[Builds release<br/>+ déploiement UGS]
```
- Les tests EditMode tournent en batchmode (Unity fermé) :
  `Unity.exe -batchmode -nographics -projectPath "<projet>" -runTests -testPlatform EditMode -testResults <fichier.xml>`.
- Les tests PlayMode aussi, mais **sans** `-nographics` (rendu des canvas) ; les captures : `-testPlatform PlayMode -testFilter N29.HeroesDawn.Tests.PlayMode.ScreenCaptureTests` → `Code/docs/test-reports/captures/`.
- Déploiement UGS : fenêtre *Deployment* de Unity en `dev` ; en `staging`/`production`, uniquement depuis la CI (jamais à la main).

## Ordre de mise en place (jalon M1 — Prototype)
1. Dépôt, LFS, packages (VContainer, MessagePipe, UniTask, PrimeTween), assemblies vides et graphe de dépendances.
2. `Core` (Ids, Fixed, Pcg32, Result) + tests.
3. `Domain.Board` (dé, accessibilité, brouillard, cases) + tests.
4. `Domain.Combat` (ATB, dégâts, statuts, IA simple) + tests de rejeu.
5. Scène Board + scène Combat en greybox (vues et présentateurs minimaux), ports factices (pas encore d'UGS).
6. Simulateur d'équilibrage (fenêtre éditeur) branché sur `Domain.Combat`.
7. Revue de fin de jalon : la boucle plateau → combat est-elle amusante ?
