# Rapport de tests — phase P0 des capacités (2026-11-16)

Branche `feature/capacites-p0`, Unity 6000.6.3f1, batch Windows. P0 ne change aucun comportement : `SkillDef.Abilities` est la source, `Effects` une vue.


## Conditions des mesures (à lire d'abord)
- Toutes les exécutions (EditMode, PlayMode et l'enregistrement de la référence de combat) ont été faites sur un arbre de travail qui contenait le travail non commité de l'utilisateur, et non sur une copie propre de la branche. Cela inclut du contenu : `PrototypeHarlem`, `PrototypeKapotnya`, `PrototypeManhattan`, `PrototypePlaceRouge`, `PrototypeQueens` et `PrototypeSadovoye` (`.cs`, non suivis) et des lignes non commitées de `PrototypeContent.cs` qui enregistrent ces quartiers.
- La preuve avant/après reste valable : le même contenu de l'arbre de travail était présent à l'enregistrement de la référence et à sa vérification.
- Les 7 échecs hors périmètre (3 EditMode, 4 PlayMode) sont attribués à ce travail non commité, mais n'ont PAS été confirmés sur un `develop` propre.
- `05_Production/Decisions.md` porte la ligne de décision P0 dans l'arbre de travail, mais elle n'est pas commitée : le fichier contient déjà des modifications non commitées de l'utilisateur.

## Limites du garde-fou (volontaires)
Le filet de sécurité se limite à : empreinte des événements + Id/Hp/Énergie par unité + sonde du générateur ; les cartes n'utilisent que la graine 7 ; les deux empreintes `SkillPower` sont identiques ; certaines empreintes d'ennemis sont identiques (l'équipe de niveau 40 les tue probablement avant qu'ils n'agissent) ; 26 cartes sur 48 ont des empreintes identiques à deux de leurs quatre niveaux. Son renforcement est la première tâche du plan P1 : il exige un réenregistrement sur le code d'avant la migration, impossible maintenant.

## Lignes orphelines de la référence
60 des 90 lignes `enemy:*` de `combat-golden.txt` (ennemis `hrl_*`, `kap_*`, `mnh_*`, `pr_*`, `sad_*`, `syn_*`, 10 chacun) viennent de contenu de quartiers non commité à l'enregistrement. Sans ce contenu elles n'ont pas de scénario et ne sont simplement pas exercées. Ne pas les supprimer.

## Écart des ignorés avec le plan (plan : 8 EditMode, 25 PlayMode ; mesuré : 5 et 28)
- PlayMode +3 : les 3 tests `UiToolkitStep0Tests` (Explicit : Coexistence, Fonts, Memory), absents de la liste de la baseline ; le reste (25) correspond exactement à la liste de la baseline.
- EditMode -3 : mesuré 5 = `CombatGoldenTests.RecordGolden` (Explicit, nouveau) + 4 tests Ignored (`CinematicVideoSequenceTests`, `LaunchCinematicAndVoiceTests` x3). La baseline ne nomme pas ses 8 ignorés EditMode : cause de l'écart non déterminée.
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

## Dérive de la référence après les mesures (revue finale)
Après les mesures ci-dessus (EditMode complet réussi pour `Golden_MatchesTheRecordedDigests`), l'utilisateur a modifié, non commité, `Domain/Combat/CombatEvents.cs` (`BossPhaseChangedEvent` reçoit une liste `Lines`) et `CombatSimulation.Actions.cs`. Un nouvel EditMode fait ensuite échouer `Golden_MatchesTheRecordedDigests` sur des lignes `enemy:*` (par ex. `enemy:brick:s1`). Cause attribuée à cette modification de l'utilisateur, non à P0 (le commit P0 n'a pas changé) ; non confirmée sur une copie propre. À reconfirmer sur un arbre propre avant de fusionner.
