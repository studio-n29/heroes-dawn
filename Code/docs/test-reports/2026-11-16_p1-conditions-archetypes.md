# Rapport de tests — phase P1 des capacités (2026-11-16)

Branche `feature/capacites-p1`, HEAD `e2c1c364`, Unity 6000.6.3f1, batch Windows. P1 ajoute les conditions, les échelles et le ciblage fin des capacités `OnCast`, les archétypes et rôles des cartes, des textes générés, le catalogue en données (`cards.json`) avec importeur des fiches Markdown, et une première vague de 12 cartes (60 cartes en jeu). `RulesVersion` passe à 0.2.0.

## Conditions des mesures (à lire d'abord)
- Toutes les exécutions (EditMode et PlayMode) ont été faites sur un arbre de travail qui contenait le travail non commité de l'utilisateur, et non sur une copie propre de la branche. Cela inclut des modifications de `Domain/Combat/CombatEvents.cs` (`BossPhaseChangedEvent.Lines`) et de `CombatSimulation.Actions.cs`, ainsi que des fichiers de quêtes, de carte et de Moscou.
- Les échecs hors périmètre listés plus bas sont attribués à ce travail non commité ; ils n'ont PAS été confirmés sur une copie propre.
- `05_Production/Decisions.md` porte la ligne de décision P1 dans l'arbre de travail, mais elle n'est pas commitée : le fichier contient déjà des modifications non commitées de l'utilisateur.

## Exécutions
| Mode | HEAD | Total / réussis / ignorés / échecs | Durée |
|---|---|---|---|
| EditMode | `e2c1c364` | 1843 / 1836 / 6 / 1 | 43 s de tests (56 s avec le démarrage de Unity) |
| PlayMode | `e2c1c364` | 145 / 113 / 28 / 4 | 421 s de tests (434 s avec le démarrage) |

Par rapport à P0 (EditMode 1599, PlayMode 145) : EditMode +244 tests (ceux de P1 et ceux d'autres travaux), PlayMode inchangé. Aucun nouvel échec ; les 3 échecs EditMode de P0 sont réduits à 1 (les deux tests de quêtes et de QG passent maintenant sur l'arbre actuel).

Ignorés EditMode (6) : `CombatGoldenTests.RecordGolden` et `WaveGoldenTests.RecordGoldenP1` (Explicit, protégés par variable d'environnement), plus 4 tests de cinématique (`CinematicVideoSequenceTests`, `LaunchCinematicAndVoiceTests` x3). Ignorés PlayMode (28) : identiques à P0.

## Échecs hors périmètre P1 (travaux non validés de l'utilisateur)
Aucun ne touche Domain, Combat, cartes ni capacités.
- EditMode `BuildBudgetTests.ResourcesTextures_StayWithinTheBuildBudget` : « textures sous Resources : 298 Mo estimés (budget 280 Mo) ». Textures, hors périmètre.
- PlayMode `CityMapScreenUiToolkitTests.Map_ButtonsStayInsideAndTappable_OnTheFourFormats` : « deploy » déborde du panneau (map 16x9). Interface de la carte.
- PlayMode `CityMapScreenUiToolkitTests.MoscowMap_ButtonsStayInsideAndTappable_OnTheFourFormats` : idem (map-moscow 16x9).
- PlayMode `MoscowFullRunTests.AgentM_NewProfile_PlaysFourZones_AndReachesTheEndOfAct` : « pas de cinquième zone ». Parcours de campagne.
- PlayMode `QuestLogPlayTests.Qg_OpensTheQuestLog` : attendu 4, obtenu 16. Journal de quêtes.

Ce sont les mêmes échecs qu'en P0, moins `QgPagePresenterTests` et `QuestLogFlowTests` qui passent désormais.

## Garde-fous
- `combat-golden.txt` (478 lignes) : modifié une seule fois dans ce plan, par le commit `14cadfa2` (tâche 1, ré-enregistrement voulu au départ de P1). `git log b77b5ebd..HEAD -- combat-golden.txt` ne donne que ce commit ; `git diff --stat 14cadfa2 HEAD -- combat-golden.txt` est vide.
- `combat-golden-p1.txt` (36 empreintes : 12 cartes x niveaux 1, 6, 10) : tel que commité en `e2c1c364`, aucune modification dans l'arbre de travail.
- `CombatGoldenTests` (classe entière, arbre de l'utilisateur) : 6 réussis, 1 ignoré (`RecordGolden`), 0 échec, y compris `Golden_MatchesTheRecordedDigests`. La dérive notée en P0 (lignes `enemy:*` rouges à cause de `BossPhaseChangedEvent.Lines`) n'existe plus : la référence a été ré-enregistrée en tâche 1 sur l'arbre contenant déjà cette modification. Si l'utilisateur annule ou change cette modification, la classe peut redevenir rouge sur les lignes `enemy:*` ; non vérifié sur une copie propre.
- `WaveGoldenTests` : `WaveGolden_MatchesTheRecordedDigests` et `EveryWaveScenario_CastsTheCardAtLeastOnce` réussis.
- `RulesVersion.Value` : `"0.2.0"`.
- Réussis dans cette exécution complète : `AbilityTests`, `AbilityMigrationTests`, `CardData*`, `CatalogImporter*`, `FirstWave*`, `TargetPick*`, `ConditionCombat*`, `SkillTextCondition*`, `CardArchetype*`, `CollectionRules*`, `CardStars*`, `DesignTools*` (aucun échec dans ces classes ni dans un test de carte ou de compétence).

## Limites du filet de sécurité
- Seuls les combats seedés de graine 7 sont couverts pour les cartes ; l'empreinte se limite aux événements, à Id/Hp/Énergie par unité et à une sonde du générateur.
- Golden de Sillage (`card_slipstream`) identique aux trois niveaux (1, 6, 10) : le scénario n'a qu'un seul allié (« verre »), la compétence ciblant « allié prochain » hors lanceur n'a donc aucune cible et ne produit aucun effet.
- L'IA Auto lance une compétence sans cible valide (énergie dépensée) : non couvert par un test de comportement.
- Les deux empreintes `SkillPower` sont identiques ; certaines empreintes d'ennemis le sont aussi (équipe de niveau 40 qui les tue avant qu'ils n'agissent) ; 60 des 90 lignes `enemy:*` viennent de contenu de quartiers non commité à l'enregistrement.
- Fusion (`Merge`) du catalogue : dépend du format canonique de `JsonUtility`.
- Les mesures de l'équilibre en simulation (taux de victoire, etc.) ne sont pas dans le périmètre de P1.

## Les 12 cartes de la vague (★1, `mise-en-jeu.txt`)
`card_cinder_toss`, `card_hot_iron` (Brûlure) ; `card_static_jolt`, `card_dead_weight` (Entrave) ; `card_lynx_eye`, `card_designated_target` (Traque) ; `card_running_start`, `card_slipstream` (Tempo) ; `card_decoy`, `card_shield_bash` (Rempart) ; `card_warm_compress`, `card_emergency_balm` (Renouveau). Total en jeu : 48 + 12 = 60.

## Fiches corrigées en tâche 9a
19 fiches P1 rejetées par l'importeur ont été corrigées ; l'importeur lit alors 196 fiches P1 sans rejet et ignore 54 fiches P2/P3.

## À valider par le concepteur
Choix d'équilibre ou de thème faits pour rendre les fiches importables ; à confirmer ou à refaire.
- `dawns_hand` perd `Purge 0`.
- `cautery` perd `Purge 1` sur allié brûlé.
- `blood_scent`, `magma_fist`, `clinch`, `leech_bite`, `blood_thirst`, `saving_bite` : le bonus de vol de vie devient un bonus de dégâts.
- `torn_veil` Niv.10 répète le Niv.3 avec une autre condition.
- `card_running_start` Niv.3 : `Adv 20` remplacé par `Purge 1` (l'avancée d'initiative ignore le lanceur ; règle des cartes à cible Soi).
- Règle de ciblage `1A:prochain` : exclut le lanceur (toujours prêt au lancer) ; sans autre allié, la compétence part sans cible et l'énergie est dépensée.
- Échelle `max M` : nombre maximal d'unités comptées (au plus 10), pas un plafond de total ; les fiches qui l'écrivaient en total ont été corrigées.
- « Allié » dans `AnyAllyHasStatus` et `AlliesAlive` : unités vivantes du camp du lanceur, lanceur inclus ; seuils de PV stricts en % des PV max ; `TargetPlannedAction` = emplacement prévu >= 0 ; le compte de statuts inclut tous les statuts (bouclier compris).
- Noms anglais des cartes : non implémentés (aucun mécanisme EN pour les cartes dans le code) ; repli sur le français.
- Fiche `card_united_front` : `(max 3)` (nombre d'unités comptées, et non plafond de total).

## À corriger avant mise en jeu (phase P4)
Sept fiches P1 à cible Soi portent un effet `Adv` ou `Gauge+` sans effet sur le lanceur (l'avancée d'initiative ignore le lanceur) : `card_iron_vest`, `card_stand_firm`, `card_adrenal_shell`, `card_last_gasp`, `card_hardened_post`, `card_adrenaline_surge`, `card_pursuit`. À corriger avant leur mise en jeu ; idée : ajouter cette vérification à `verifier-catalogue.ps1`.

## Cartes restantes
250 − 60 = 190 cartes restent à mettre en jeu. D'après le test de l'importeur : 196 fiches de phase P1 (dont les 48 existantes et les 12 de la vague) donc 196 − 60 = 136 fiches P1 à mettre en jeu en phase P4 ; 54 fiches de phases P2/P3 (réactions, seuils d'archétype) qui attendent ces phases. 136 + 54 = 190.

## Divers
- Dettes mineures relevées en revue (non bloquantes) : voir le journal du plan (`progress.md`), notamment des tests de condition dont les bornes exactes ne sont pas testées, la condition évaluée sur une cible déjà vaincue pour le vol de vie et le rebond, et les bonus de jauge non multiples de 10 affichés tronqués.
