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
- `combat-golden-p1.txt` : 36 empreintes (12 cartes x niveaux 1, 6, 10) tel que commité en `e2c1c364` ; 48 depuis `d9abffc8` (+15 `wave2:`, -3 `wave:card_slipstream:*`, 2 lignes L10 modifiées : `card_designated_target` et `card_emergency_balm`). Voir « Revue finale et corrections ».
- `CombatGoldenTests` (classe entière, arbre de l'utilisateur) : 6 réussis, 1 ignoré (`RecordGolden`), 0 échec, y compris `Golden_MatchesTheRecordedDigests`. Cet état dépend de l'arbre de travail : `combat-golden.txt` a été ré-enregistré en tâche 1 sur un arbre qui contenait déjà les modifications non commitées de l'utilisateur sur `BossPhaseChangedEvent` (propriété `Lines` à l'époque de P0, devenue la méthode `GetLines()` dans son arbre actuel). La classe est verte sur cet arbre, mais elle n'a pas été vérifiée sur une copie propre de la branche : si l'utilisateur annule ou modifie ce travail, les lignes `enemy:*` (boss à phases) peuvent redevenir rouges et la référence devra alors être ré-enregistrée sciemment.
- `WaveGoldenTests` : `WaveGolden_MatchesTheRecordedDigests` et `EveryWaveScenario_CastsTheCardAtLeastOnce` réussis.
- `RulesVersion.Value` : `"0.2.0"`.
- Réussis dans cette exécution complète : `AbilityTests`, `AbilityMigrationTests`, `CardData*`, `CatalogImporter*`, `FirstWave*`, `TargetPick*`, `ConditionCombat*`, `SkillTextCondition*`, `CardArchetype*`, `CollectionRules*`, `CardStars*`, `DesignTools*` (aucun échec dans ces classes ni dans un test de carte ou de compétence).

## Limites du filet de sécurité
- Pour `combat-golden.txt` (48 cartes d'avant P1) : graines 7 et 11, empreinte = journal d'événements complet, état final des unités (PV, énergie, jauge, statuts) et sonde du générateur. Pour la vague (`combat-golden-p1.txt`) : graine 7 seulement, un seul scénario par carte et par niveau (1, 6, 10) ; le scénario d'origine n'avait qu'une alliée (corrigé par la revue finale : voir plus bas).
- Golden de Sillage (`card_slipstream`) : l'identité des trois niveaux relevée à `e2c1c364` (scénario à une seule alliée, sans cible) est levée depuis `d9abffc8` par les scénarios à deux alliés `wave2:` (empreintes différentes aux niveaux 1, 6, 10) ; il n'existe plus de scénario `wave:` pour Sillage.
- L'IA Auto ne lance plus une compétence à ciblage fin sans candidat (`PickHasTarget` dans `CanUseSlot`, testé par `Ai_DoesNotCastAPickWithNoCandidate`). Reste vrai en manuel : une carte `1A:prochain` sans autre allié part encore à vide (énergie dépensée).
- Les deux objets `SkillPower` donnent des empreintes différentes (test `SkillPowerItems_GiveDifferentDigests`) ; en revanche certaines empreintes d'ennemis sont identiques (équipe de niveau 40 qui les tue avant qu'ils n'agissent) ; 60 des 90 lignes `enemy:*` viennent de contenu de quartiers non commité à l'enregistrement.
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

## Revue finale et corrections
Revue finale de la branche (`b77b5eb..e065f72`) : « Avec corrections ». Quatre points Important, tous corrigés dans le commit `d9abffc8` (« fix: revue finale de P1 »), plus les mineurs de la liste de corrections.

### Les 4 Important
1. **Cible ignorée** : l'interface demandait une cible pour une compétence à ciblage fin (`Pick` différent de `Default`) alors que la simulation l'ignore. `NeedsTarget` est désormais faux pour ces compétences ; elles partent sans cible, comme une compétence de zone (test `NeedsTarget_IsFalseForFinePick_TrueForPlainSingleTarget`).
2. **Deux paliers Niv.10 jamais actifs** : `card_emergency_balm` et `card_designated_target` lisaient, après la base, un état que la base avait déjà changé (PV après un soin d'au moins 35 %, Marque consommée par le coup). Cellules réécrites (voir « À valider »), `cards.json` réimporté (10 lignes modifiées, les 58 autres cartes identiques octet pour octet). Test `EveryWaveTier_ChangesTheDigest` : les empreintes aux niveaux 1, 6 et 10 sont deux à deux différentes pour les 12 cartes.
3. **Sillage et IA** : `1A:prochain` écarte maintenant aussi les alliés déjà prêts (Advance et Gauge + ne font rien sur eux) ; l'IA Auto ne choisit plus une compétence à ciblage fin sans candidat (`CanUseSlot` filtre, chemin `Pick == Default` inchangé). Scénarios à deux alliés (clés `wave2:`) pour les 5 cartes à cible alliée ou Soi ; les empreintes de Sillage diffèrent aux trois niveaux. Le scénario à une alliée de Sillage (`wave:card_slipstream:*`) est supprimé : l'IA ne la lance plus seule. `combat-golden-p1.txt` : 48 empreintes (33 `wave:` + 15 `wave2:`) ; `combat-golden.txt` intact.
4. **Export qui écrase le catalogue** : `ExportCardsMenu` (menu et `ExportBatch`) refuse d'écrire si `cards.json` contient des cartes absentes du code ou est illisible ; décision pure `RefusalReason` testée.

### Mineurs corrigés
`CardDataLibrary.Validate` (conditions sur Lifesteal, Bounce, Execute, Purge ; modificateurs sur les autres types que Damage, Heal, Shield, Advance, PushBack, GaugeDelta ; `maxCount` sans échelle ; niveaux de palier hors 3, 6, 10 ou répétés) ; texte d'un bonus de jauge négative au signe de la base et « le plus en forme (PV en %) » ; `Merge` refuse un `cards.json` absent, vide ou sans les cartes du code (le test `Merge_EmptyExisting_StartsFromNothing` est remplacé par `Merge_EmptyOrIncompleteExisting_IsRefused`) ; ce rapport corrigé ; `verifier-catalogue.ps1` (avertissements) et ligne de grammaire sur `Dmg +N` dans le plan.

Tests existants adaptés (la règle « les alliés prêts ne sont plus visés » change leur situation de départ, pas leur assertion) : `TargetPickTests.NextToAct_OnAllies` (allié non prêt par construction : `Point = ActionPoint / 2` au lieu de 30, que l'allié atteignait déjà), `CardMechanicsTests.AutoMode_CastsEveryCard` et `FirstWaveTests.EveryWaveCard_IsCastInAuto` (un second allié pour les cartes `1A:prochain`).

### À valider par le concepteur
- `card_emergency_balm` Niv.10 : `[si cible < 30% PV : Shield 15]` devient `Shield 15` (inconditionnel). Sur une cible sous 30 % des PV le soin de base est de 35 % (25 % + le bonus plafonné à 10) et la laisse à 35 % des PV au moins : la condition lue après lui ne pouvait jamais être vraie.
- `card_designated_target` Niv.10 : `[si cible a Mark : St Vulnerable 2t]` devient `St Vulnerable 2t` (inconditionnel). Le coup de base consomme la Marque avant le palier ; la carte vise de toute façon le porteur de Marque.
- Règle `1A:prochain` : le lanceur et les alliés déjà prêts sont écartés ; sans candidat, l'IA ne lance pas la compétence (en manuel, elle part encore sans cible et l'énergie est dépensée).
- Sillage : son palier 10 (`Purge 1`) ne s'exprime que sur un allié affligé ; le scénario de référence donne donc Brûlure à l'allié de départ.
- Texte du ciblage `pick.HighestHp` : « Cible : le plus en forme (PV en %) » (formule libre, à confirmer).

### À corriger avant mise en jeu (P4), relevé par les nouveaux avertissements de `verifier-catalogue.ps1`
- Cartes Soi à `Adv` ou `Gauge +` inertes : les 7 déjà connues (`card_iron_vest`, `card_stand_firm`, `card_adrenal_shell`, `card_last_gasp`, `card_hardened_post`, `card_adrenaline_surge`, `card_pursuit`) plus `card_blazing_sprint` et `card_marked_bait` ; `card_thorn_hide` (Steal sans coup de base, hors réaction).
- Conditions de palier qui lisent un état modifié par la base : `card_lingering_mark` et `card_open_wound` (Niv.10, `cible a Mark` après un `Dmg`), `card_torn_veil` (Niv.3, `cible a Shield` après `Purge`), `card_vital_sap` (Niv.10) et `card_harvest` (Niv.6) (`cible < 50% PV` après un `Heal`).
- `Bounce 1` (1 % de rebond, alors que les autres cartes écrivent 40 à 60) sur `card_saving_bite` Niv.3, `card_hungry_spark` et `card_lifethirst` : probable faute de frappe, fiches non modifiées (hors jeu).

### Exécutions de la revue finale (arbre de l'utilisateur, après `d9abffc8`)
| Mode | Total / réussis / ignorés / échecs |
|---|---|
| EditMode | 1856 / 1849 / 6 / 1 (`BuildBudgetTests.ResourcesTextures_StayWithinTheBuildBudget`, étranger) |
| PlayMode | 145 / 113 / 28 / 4 (les mêmes quatre échecs étrangers : `CityMapScreenUiToolkitTests` x2, `MoscowFullRunTests`, `QuestLogPlayTests`) |
| Classes ciblées (305 tests) | 303 réussis, 0 échec, 2 ignorés (`RecordGolden`, `RecordGoldenP1`) |

`verifier-catalogue.ps1` : 0 erreur, 19 avertissements (dont les 12 nouveaux). `CombatGoldenTests` verte, `combat-golden.txt` inchangé depuis `14cadfa2`.

## Validation du concepteur (2026-11-16)

Tous les points de la section « À valider par le concepteur » ci-dessus sont **validés** (choix d'équilibre des fiches corrigées, règles de ciblage et d'échelles, texte de `pick.HighestHp`, repli en français des noms de cartes). Cette validation a été relayée le 2026-11-16 par la session d'orchestration de la version 1.0.1 pour le compte de l'utilisateur ; elle n'a pas été confirmée directement dans la conversation de travail de P1.
