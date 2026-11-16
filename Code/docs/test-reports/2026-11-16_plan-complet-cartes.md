# Rapport de tests — plan complet des cartes (P2, P3, P4), 2026-11-16

Branche `feature/capacites-p2p4` (enfant de `feature/capacites-p1`), HEAD mesuré `caf74d87` (fin de la fix wave de la revue finale), Unity 6000.6.3f1, batch Windows `-batchmode -nographics`. Le plan livre : réactions (P2), effets Consomme et Propage, échelle d'archétype (P3, sans seuils), importeur et données, économie des doublons, noms FR/EN, visuel de repli, 250 cartes en jeu. `RulesVersion` passe à `0.3.0`.

## Conditions des mesures (à lire d'abord)
Deux arbres ont été mesurés, avec les mêmes commandes (`-runTests`, `-testPlatform EditMode` puis `PlayMode`, sans filtre).
- **Arbre de travail de l'utilisateur** (`E:/Projets/HeroesDawn/principal`) : HEAD `caf74d87` plus environ 470 fichiers modifiés ou non suivis d'autres sessions et de l'utilisateur (interface, quêtes, contenu de quartiers, textes). Les échecs de cet arbre qui ne se reproduisent pas sur la copie propre sont attribués à ces fichiers.
- **Copie propre** : `git worktree add --detach E:/Projets/HeroesDawn/_p2p4-clean-check caf74d87`, supprimée ensuite (`worktree remove --force` puis `prune` ; `git worktree list` ne montre plus que l'arbre principal et les worktrees des autres branches). Les tests non suivis de l'utilisateur n'y existent pas : d'où 2140 tests EditMode au lieu de 2239.

**Défaut trouvé sur la copie propre : HEAD `caf74d87` ne compile PAS seul.** `Assets/Tests/Presentation/CardNamesTests.cs` (commité par la tâche 7) appelle `Loc.HasEnglish(...)` (lignes 58 et 69), méthode qui n'existe que dans le `Loc.cs` NON commité de l'utilisateur (`Loc.cs` ligne 355 de l'arbre de travail : `public static bool HasEnglish(string key) => En.ContainsKey(key);`). Sur la copie propre : `error CS0117: 'Loc' does not contain a definition for 'HasEnglish'` ×2, l'assembly de tests Presentation ne se compile pas, aucun test ne tourne (premier essai : exit=1 en 69 s, journal `t9-clean-EditMode-compilefail.log`). Pour pouvoir mesurer, cette ligne a été ajoutée **uniquement dans la copie jetable** (non commitée), puis les deux modes ont tourné. **Il faut commiter cette ligne (ou le hunk correspondant de `Loc.cs`) avant de fusionner ou de livrer la branche** ; je ne l'ai pas commitée (hors périmètre de la tâche 9 : le fichier contient des modifications de l'utilisateur). Tous les résultats « copie propre » ci-dessous sont donc « HEAD + cette seule ligne ».

## Exécutions
| Arbre | Mode | Total / réussis / ignorés / échecs | Durée des tests | Durée totale (démarrage inclus) |
|---|---|---|---|---|
| Travail de l'utilisateur | EditMode | 2239 / 2230 / 7 / 2 | 139 s | 153 s |
| Travail de l'utilisateur | PlayMode | 152 / 118 / 28 / 6 | 465 s | 480 s |
| Copie propre (HEAD + `HasEnglish`) | EditMode | 2140 / 2132 / 7 / 1 | 126 s | 309 s |
| Copie propre (HEAD + `HasEnglish`) | PlayMode | 144 / 110 / 28 / 6 | 415 s | 429 s |

Le total EditMode de l'arbre de travail est identique à la dernière exécution de la fix wave (2239 / 2230 / 2 échecs). Ignorés EditMode (7, identiques sur les deux arbres) : les trois enregistrements `[Explicit]` (`CombatGoldenTests.RecordGolden`, `WaveGoldenTests.RecordGoldenP1`, `AllCardsGoldenTests.RecordGoldenP4`) et 4 tests de cinématique. Ignorés PlayMode : 28 (identiques à P1).

## Échecs, un par un
### Arbre de l'utilisateur
Aucun échec ne touche cartes, combat, économie ni noms : tous sont étrangers.
EditMode (2) :
- `BuildBudgetTests.ResourcesTextures_StayWithinTheBuildBudget` : « textures sous Resources : 304 Mo estimés (budget 300 Mo). » Textures de l'utilisateur.
- `BuildBudgetTests.UnusedArtGroups_AreNotInResources` : « groupes exclus encore présents sous Resources : Enemies ». Idem.

PlayMode (6) :
- `AllWindowsFourFormatsTests.EveryCataloguePrefab_FitsTheFourFormats` : « 47 prefabs × 4 formats : Profile 4x3 : « avatar » et « achBack » se chevauchent (49×63 px) ». Interface du profil (travail de l'utilisateur, nouveau par rapport à P1).
- `CityMapScreenUiToolkitTests.Map_ButtonsStayInsideAndTappable_OnTheFourFormats` : « map 16x9 : « deploy » déborde du panneau ». Connu.
- `CityMapScreenUiToolkitTests.MoscowMap_ButtonsStayInsideAndTappable_OnTheFourFormats` : idem pour `map-moscow`. Connu.
- `HubScreenUiToolkitTests.Hub_ButtonsStayInsideAndTappable_OnTheFourFormats` : « hub 4x3 : « skip » sous 56 px de référence de hauteur (55) ». Interface du hub (nouveau par rapport à P1).
- `MoscowFullRunTests.AgentM_NewProfile_PlaysFourZones_AndReachesTheEndOfAct` : « pas de cinquième zone ». Connu.
- `QuestLogPlayTests.Qg_OpensTheQuestLog` : « Expected: 4 But was: 16 ». Connu.

Les cinq échecs connus de P1 se reproduisent sur l'arbre de l'utilisateur et seulement là ; deux nouveaux échecs d'interface (`AllWindowsFourFormatsTests`, `HubScreenUiToolkitTests`) apparaissent aussi uniquement là (absents de la copie propre) : travail d'interface non commité.

### Copie propre
EditMode (1) :
- `TestDescriptionsTests.EveryTestHasADescription` : « Tests sans description : `PointerHitDiagnosticTests.Hq_Buttons_ReceiveThePointer`, `...TitleScreen_PlayAndAccountButtons_ReceiveThePointer` ». Deux tests PlayMode commités sans `Description` avant ce plan (déjà constaté en P1) ; étranger. L'arbre de l'utilisateur les corrige (non commité).

PlayMode (6), tous environnementaux ou instables sous `-nographics`, absents de l'arbre de l'utilisateur :
- `ActivityGamesPlayTests.Arena_PlayedOnTheRealStage_ClearsTheWave`, `AgentsPlayTests.Registry_OpensFromRecruit_AndListsTheRegistry`, `BossCombatPlayTests.BossFight_ShowsTheBar_AndOpensPhaseTwoAtTheThreshold`, `CombatIntroPlayTests.Campaign_Combat_OpensTheIntro_AndSkipClosesIt`, `LabPlayTests.Craft_FromWorkshop_AddsItemToInventory` : `Unhandled log message: '[Error] RenderTexture.Create failed'` (environnement, listés et non comptés comme régressions).
- `ActivityGamesPlayTests.Deflect_PlayedOnTheRealWindow_Wins` : `MissingReferenceException: The object of type 'UnityEngine.GameObject' has been destroyed` : exception dans un mini-jeu juste après un test à RenderTexture en échec (même classe) ; très probablement un effet de cascade, non confirmé. Aucun lien avec les cartes.

Aucun des échecs étrangers de P1 (`BuildBudgetTests`, `CityMapScreenUiToolkitTests` ×2, `MoscowFullRunTests`, `QuestLogPlayTests`) n'apparaît sur la copie propre : ils viennent bien du travail non commité.

## Garde-fous et leurs limites
- `combat-golden.txt` : inchangé depuis `14cadfa2` (`git diff --stat 14cadfa2 HEAD -- combat-golden.txt` vide). Jamais ré-enregistré dans ce plan.
- `combat-golden-p1.txt` : dernier changement `d9abffc8` (fix de la revue finale de P1) ; inchangé depuis, par tout le plan P2-P4.
- `combat-golden-p4.txt` (570 lignes = 190 cartes × niveaux 1, 6, 10) : enregistré en `e7376f42`, ré-enregistré en `caf74d87` : 37 clés sur 570 changent, 16 cartes, toutes à réaction, toutes expliquées par la fix wave (la réaction entend sa propre carte, bouclier qui absorbe tout, valeur de réaction mise à l'échelle) ; les 533 autres sont identiques. Justification détaillée dans le rapport de la fix wave.
- Résultats sur les deux arbres (identiques) : `CombatGoldenTests` 6 réussis + 1 ignoré (`RecordGolden`) ; `WaveGoldenTests` 3 réussis + 1 ignoré ; `AllCardsGoldenTests` 1 réussi + 1 ignoré ; `AllCardsTests` 3 réussis ; `ReactionTests` 40/40 ; `CardUpgradeTests` 26/26. Les références `combat-golden*.txt` tiennent donc aussi sur l'arbre propre.
- `RulesVersion.Value == "0.3.0"`.
- `cards.json` : 250 cartes (lecture du JSON : 250 identifiants de carte distincts ; le motif `"id"` compte 500 occurrences car chaque carte porte aussi l'id de sa compétence), 1 582 888 octets (1,58 Mo, contre 185 Ko avant : `JsonUtility` écrit tous les champs, y compris les champs vides de réaction et d'archétype).
- Garde-fous du plan : `AllCardsTests` (chaque carte se lance, chaque palier change le comportement par rapport à la même carte privée du palier, textes, 250 cartes × 8 scénarios jusqu'à la fin du combat sans exception) ; `EveryReactionCard_TriggersAtLeastOnce` (chaque réaction du catalogue part au moins une fois) ; test de rejeu `Replay_WithReactions_DrivenByApplyCommands_MatchesTheDirectHash`.
- Limites : les empreintes d'or sont un filet de non-régression (graine 7, un scénario par carte et par niveau), pas une preuve d'équilibre ; pour 13 cartes deux niveaux donnent la même empreinte dans le scénario générique (l'activité des paliers est prouvée par `AllCardsTests`, pas par l'or) ; les 8 scénarios (générique + 7 dédiés : alliés à 30 % PV, alliés affligés, ennemis affligés, ennemis renforcés, trois ennemis, brutes de niveau 20, brutes à 28 % PV) ne couvrent pas toutes les combinaisons de conditions ; un palier qui ne s'exprime que dans une situation absente de ces scénarios serait déclaré inerte (liste d'exceptions actuellement vide, et le test échoue si une exception listée redevient active).

## Ce qui a été livré
- **P2 — Réactions (moteur)** : une carte arme une réaction sur le lanceur pour N de ses tours ; 6 déclencheurs (un ennemi / un allié reçoit un statut, un ennemi / un autre allié est vaincu, le porteur est touché, le porteur inflige un critique) ; file après chaque pas de frise, jamais récursive ; effet unique par réaction, 8 formes de cible ; événements `ReactionTriggeredEvent` ; incluse dans `StateHash` quand armée. Revue finale : « touché » inclut un coup absorbé par le bouclier (brûlure exclue) ; la réaction entend la carte qui l'arme ; l'effet de réaction suit niveau, bonus de position/famille et puissance de compétence des objets.
- **Consomme et Propage** : `Consomme` (retire un statut de la cible et inflige dégâts ou soin par tour restant, plafonné) et `Propage` (`Spread`, copie un statut sur les autres ennemis) ; rejet à la validation d'un Propage sur une compétence alliée.
- **P3 — Échelle d'archétype** : les modificateurs peuvent croître avec le nombre de cartes d'un archétype équipées dans l'escouade (`max` de 1 à 10), textes générés. **Les seuils d'archétype ne sont pas implémentés.**
- **Importeur et données** : l'importeur lit les 250 fiches (réactions, Consomme, Propage, échelle) ; `cards.json` en fait le catalogue ; `Validate` refuse les champs que le moteur ignorerait ; repli sur le code = `Debug.LogError`.
- **Économie des doublons** : table `1,1,1,2,2,2,3,3,3` (18 exemplaires pour le niveau 10), doublons d'autres cartes de la même étoile à 3 pour 1 (ordre : niveau max, plus d'exemplaires en trop, identifiant ; cartes équipées protégées), et pour ★4 et ★5 le levier « étoile juste en dessous au même taux » ; atomicité et coût en Crédits inchangés ; le Grimoire annonce le coût réel.
- **Noms FR/EN** des 250 cartes générés depuis les fiches (`GenerateCardNamesMenu`), repli visuel garanti (carte sans image : illustration générique).
- **250 cartes en jeu** (190 nouvelles) : toutes importées, aucun palier inerte.
- **Fiches corrigées** : 78 cellules en tâche 1 (inertes sur le lanceur 18, conditions de palier 5, `Bounce 1` 3, normalisation P2/P3 52) + 1 cellule en tâche 8 (`card_lifethirst` Niv.6 : `Bounce 50` inerte sur une carte de zone devient `St Corrosion 2t`) ; journal `Code/docs/catalogue/corrections-fiches.md`. Les 12 cellules mentionnées dans la consigne de la tâche 9 n'ont pas été retrouvées dans les journaux : seuls 78 + 1 sont documentés.
- **Liste des paliers inertes : vide** ; **fix `card_lifethirst`** (ci-dessus).
- Commits : `ee0eb45b` (plan) ; `51ed456d`, `9a086803` (fiches) ; `bbe13292`, `21b943d2` (réactions) ; `c218333f` (Consomme/Propage) ; `e0967362` (échelle, textes) ; `f2744743`, `36236025` (importeur) ; `e6cab943` (économie) ; `7f5e7baf` (noms, repli) ; `0f20fdc5` (mise en jeu des 250 cartes : c'est LUI qui active les cartes, `cards.json` + règles 0.3.0) ; `6fa3ea66` (tests de comptage) ; `e7376f42` (titre « les 250 cartes en jeu » mais contient surtout les tests, les textes et la référence p4 ; l'activation réelle est en `0f20fdc5`) ; `caf74d87` (fix wave de la revue finale).

## Décisions à valider par le concepteur
Écarts entre le plan, la spec et ce qui a été fait, et choix faits pour rendre les fiches importables. Tous sont réglables.

Écarts plan / spec :
1. **Valeur des réactions mise à l'échelle** (niveau, bonus de position/famille, puissance des objets) comme un effet de premier niveau : le plan la voulait fixe, la spec l'admet ; choix : mise à l'échelle.
2. **Porteur = lanceur** : seul le lanceur arme et déclenche la réaction ; pas de réaction « par allié » (une carte de zone alliée ne fait pas riposter toute l'équipe). Les textes disent « Riposte du lanceur » ; certains noms (`card_storm_dome`) pourraient laisser croire le contraire.
3. **Ni Purge ni icône** pour les réactions : une réaction n'est pas retirée par un `Purge` ; seuls des événements existent.
4. **Déclencheur « début de tour du porteur » non implémenté** : `card_launch_ramp`, `card_beating_heart`, `card_frozen_moment`, `card_moment_master` utilisent un des six événements (voir ci-dessous).
5. **Seuils d'archétype par escouade non implémentés** (changeraient toutes les escouades équipées) : seule l'échelle existe.
6. **Économie ★4/★5** : levier « doublons de l'étoile juste en dessous à 3 pour 1 » ajouté en revue finale ; à confirmer ou remplacer (taux de conversion, autre canal).
7. **Texte de `Mark` sans durée** (« Marque », « Marque (50 %) »), y compris pour les cartes P1 qui posent une Marque.
8. « Touché » : un coup qui porte (pas esquivé), même tout absorbé par le bouclier, brûlure exclue ; le coup d'un allié sur le porteur compte.
9. Une réaction armée entend les événements depuis le DÉBUT du lancer de la carte qui l'arme (premier armement ou réarmement).
10. Consomme et Propage : valeur fixe de grammaire, non mise à l'échelle ; `Mark` consommé AVANT le coup (plus de critique garanti) ; `Spread` émet `StatusAppliedEvent` même chez un ennemi qui avait déjà le statut (déclenche « un ennemi reçoit »).
11. Économie : cartes équipées protégées par `max(1, exemplaires équipés)` ; doublons de même étoile et d'étoile inférieure additionnables dans un même exemplaire manquant.
12. Textes : « Recule de N % sur la frise » pour le recul d'une réaction ; « au plus 1 tour » / « au plus 1 carte » au singulier ; « N de ses tours » même pour 1.

Fiches (détail dans `corrections-fiches.md`) :
- `card_beating_heart`, `card_launch_ramp` : « début de tour du porteur » remplacé par « un ennemi est vaincu » (l'intention change : soin / avance d'équipe à chaque victoire). `card_frozen_moment`, `card_moment_master` : événement de repli.
- `card_rebirth` : plafond d'échelle 8 au lieu de 5 (`max 2`).
- `card_citadel` (9), `card_squad_garden` (6), `card_kinetic_charge` (32) : plafonds arrondis, légèrement différents de l'original.
- `card_moment_master`, `card_runaway_pace` : une réaction « un allié reçoit Hâte » peut partir jusqu'à 4 fois avec le Hâte de zone du Niv.3 : risque de puissance.
- Réactions réduites à un seul effet : `scar_tissue`, `beating_heart`, `sanitary_cordon`, `hungry_blade` (Steal retiré, Dmg 50 → 60), `storm_dome`.
- `chain_reaction` (Propage dans une réaction non admis → `St Shock 1t` sur ZE), `domino_fall` (condition de Propage supprimée), `alpha_hunter` et `final_verdict` (Steal / Exec conditionnels remplacés par des bonus de dégâts), `gridlock` (échelle déplacée sur Dmg), `lingering_mark` / `frozen_moment` (cibles `1E:fort`, `1E:prochain` remplacées).
- `card_lifethirst` Niv.6 : `Bounce 50` → `St Corrosion 2t` (palier inerte).
- `card_torn_veil` Niv.10 : `[si cible a Taunt : Dmg 30]` est lu après la `Purge 1` de base : il ne s'exprime que si la Provocation reste après la purge.
- `card_blazing_sprint` : nature saisie Soutien alors que le premier effet (bouclier) déduirait Défensive ; gardée Soutien.
- Rappel de P1 encore ouvert : `dawns_hand`, `cautery`, vol de vie transformé en dégâts sur six cartes, `torn_veil` Niv.10, `card_running_start` Niv.3, etc. (voir le rapport de P1).

## Limites connues
- **Pas d'interface pour les réactions** : seuls des événements (`ReactionTriggeredEvent`) ; pas d'icône, pas d'éclat de combat, pas de décompte visible des tours restants.
- **Descriptions en français en mode anglais** : seuls les noms des 250 cartes sont traduits ; le texte généré des capacités retombe sur le français.
- **Équilibrage non testé en partie jouée** : seules des simulations déterministes à graine fixe ; marge 40-60 % de la spec non mesurée.
- **Temps de chargement de `cards.json` (1,58 Mo) non mesuré** (désérialisation `JsonUtility` + `Validate` de 250 cartes à chaque `PrototypeCollection.Build()`).
- **Estimation de l'économie** : niveau 10 d'une carte, 5 cartes par booster (taille non vérifiée), tirage en espérance sans pitié. Avant le levier ★4/★5 : environ 22 / 40 / 75 / 180 / 1100 boosters pour ★1 à ★5. Avec le levier (étoile inférieure à 3 pour 1) : ★1 à ★3 inchangées, environ 50 boosters pour une ★4 et environ 150 (160 selon le calcul de la fix wave) pour une ★5.
- **Ordre de rejeu (M1)** : `CombatReplay` re-soumet une commande journalisée au tick T AVANT le pas T, alors qu'en direct `ApplyCommands` l'applique APRÈS le pas T. Une divergence est possible si le joueur éteint l'Auto juste après qu'un allié a agi en Auto, ou si une réaction part dans ce pas. Antérieur à la branche ; `VerifyCombatUseCase` ne fait qu'avertir ; à traiter avant de rendre la vérification bloquante. Le test de rejeu écrit en fix wave est vert (pas de divergence reproduite).
- **Code mort** : la clé `scale.EquippedArchetypeCards` n'est pas utilisée ; le paramètre `count` de `GrimoirePage.UpgradeButton` est inutilisé ; la liste d'exceptions de nature est dupliquée (`CollectionRulesTests`, `DesignToolsDataTests`) ; `NoDuplicateNameKeys` ne compare que `LocCollection.cs` ; texte « Riposte du lanceur » imprécis pour soins et élan.
- Mineurs de revue différés : tests manquants (Lifesteal après `ConsumeDamage`, Spread multi-sources, cas équipé de l'économie), `FromDef` perd sans bruit `When`/`Mods`/`Reaction` d'un effet imbriqué non validé.

## Ce qui reste
- Passe d'équilibrage sur le catalogue complet (marge de victoires par archétype, risque `moment_master`, `runaway_pace`, levier ★4/★5, coût en boosters).
- Interface des réactions (icône, éclat, tours restants).
- Descriptions anglaises (générées ou traduites).
- Dépendance Content → Authoring : le contenu du jeu dépend du module d'authoring via `CardDataLibrary` ; à inverser ou à isoler.
- Cache de `PrototypeCollection.Build()` pour l'éditeur et les tests (relecture et validation du JSON à chaque appel).
- Commiter la ligne `Loc.HasEnglish` (voir « Conditions des mesures ») et, à terme, les seuils d'archétype si le concepteur les veut.
- Corriger l'ordre du rejeu (M1) avant de rendre la vérification des combats bloquante.
- Fichiers de mesure : `.superpowers/sdd/2026-11-16-p2-p4-plan-complet/t9-{dirty,clean}-{EditMode,PlayMode}.{xml,log,done}` (ignorés par git).
