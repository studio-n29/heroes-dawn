# Rapport de tests — phase P0 des capacités (2026-11-16)

Branche `feature/capacites-p0`, Unity 6000.6.3f1, batch Windows. P0 ne change aucun comportement : `SkillDef.Abilities` est la source, `Effects` une vue.

## Garde-fou
- `combat-golden.txt` : identique à son enregistrement du commit `fb159994` (`git diff --stat fb159994 HEAD` vide).
- `RulesVersion.Value` : toujours `"0.1.0"`.
- `CombatGoldenTests` (5), `AbilityTests` (9), `AbilityMigrationTests` (3), `ContentCsvAbilityTests` (1) : réussis.

## Exécutions
| Mode | HEAD | Total / réussis / ignorés / échecs | Durée |
|---|---|---|---|
| EditMode | `966677b8` | 1599 / 1591 / 5 / 3 | 36 s |
| PlayMode | `e82bdf52` | 145 / 113 / 28 / 4 | 430 s |

- `FullRunTests` : réussi (45 s).
- Un premier EditMode sur `e82bdf52` avait 4 échecs : `TestDescriptionsTests.EveryTestHasADescription` (description manquante sur `CombatGoldenTests.RecordGolden`), corrigé par `966677b8`.
- PlayMode n'a pas été relancé après `966677b8` : ce commit ne change qu'un attribut de test EditMode, rien que PlayMode exécute.
- Les totaux diffèrent de la baseline du 2026-11-12 (1511 / 106) car d'autres travaux ont ajouté des tests depuis.

## Échecs hors périmètre P0 (travaux non validés de l'utilisateur : quêtes, carte, Moscou, textures)
Aucun ne touche Domain, Combat ou Capacités.
- EditMode `BuildBudgetTests.ResourcesTextures_StayWithinTheBuildBudget` : « textures sous Resources : 298 Mo estimés (budget 280 Mo) ».
- EditMode `QgPagePresenterTests.FilText_UsesFirstUncompletedZone` : texte attendu « Red Hook est à nous… », obtenu « Le Syndicat règne sur les marchés de Queens… ».
- EditMode `QuestLogFlowTests.Log_ShowsMainAndSideQuests` : attendu 4, obtenu 16.
- PlayMode `CityMapScreenUiToolkitTests.Map_ButtonsStayInsideAndTappable_OnTheFourFormats` : « deploy » déborde du panneau (map 16x9).
- PlayMode `CityMapScreenUiToolkitTests.MoscowMap_ButtonsStayInsideAndTappable_OnTheFourFormats` : idem (map-moscow 16x9).
- PlayMode `MoscowFullRunTests.AgentM_NewProfile_PlaysFourZones_AndReachesTheEndOfAct` : « pas de cinquième zone ».
- PlayMode `QuestLogPlayTests.Qg_OpensTheQuestLog` : attendu 4, obtenu 16.

## Ignorés
EditMode : 5 (dont `CombatGoldenTests.RecordGolden`, explicite). PlayMode : 28.
