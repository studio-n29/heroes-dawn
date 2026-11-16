# Plan complet des cartes (P2, P3, P4, économie, noms anglais) : plan d'implémentation

> **Pour les agents d'exécution :** utiliser superpowers:subagent-driven-development, tâche par tâche. Les étapes utilisent des cases `- [ ]`.

**Objectif :** terminer le plan des cartes pour la version 1.0.1 : réactions (P2), échelle d'archétype (P3), mise en jeu des 190 cartes restantes (250 au total), fiches corrigées, économie des doublons praticable avec 250 cartes, noms anglais, visuel de repli garanti.

**Architecture :** une réaction est un effet `ArmReaction` qui installe sur le porteur une `ReactionInstance` (durée en tours du porteur) ; la simulation la déclenche après chaque pas de frise en lisant les événements produits (file : jamais en récursion), sans effet sur les combats qui n'en arment aucune. `Consomme` et `Propage` sont trois nouveaux types d'effet. L'échelle d'archétype est un nouveau `ScaleKind` qui compte les cartes équipées de l'escouade. Les cartes P2/P3 passent par le même importeur (grammaire étendue) et le même `cards.json`.

**Tech Stack :** C# (Unity 6000.6.3f1), NUnit EditMode. Domaine : `Code/Heroes Dawn Unity/Packages/com.n29.heroesdawn.domain`. Contenu : `Assets/_Project`. Tests : `Assets/Tests/Domain|Editor|Presentation`.

**Specs :** `docs/specs/2026-11-16-capacites-composables-synergies-design.md`, `docs/specs/2026-11-16-catalogue-250-cartes.md`, fiches `docs/catalogue/cartes-*.md`, plan précédent `docs/plans/2026-11-16-p1-conditions-archetypes.md` (grammaire P1, toujours valable), rapport `docs/test-reports/2026-11-16_p1-conditions-archetypes.md`.

## Contraintes globales
- Branche `feature/capacites-p2p4` (enfant de `feature/capacites-p1`). Aucun merge, aucun push. L'arbre contient du travail non commité de l'utilisateur : jamais `git add -A`/`.`, jamais ses hunks (`git apply --cached` d'un patch de ses seuls changements quand un fichier est partagé), jamais `latest-*.html`, jamais `Decisions.md`.
- Les 60 cartes déjà en jeu, les ennemis et les boss se comportent **exactement comme avant** : `CombatGoldenTests` (référence `combat-golden.txt`, jamais ré-enregistrée) reste verte, et `combat-golden-p1.txt` aussi, sauf modification expressément justifiée par la tâche et notée dans le rapport. Les tirages du générateur ne changent pas pour une compétence sans réaction, sans `Consomme`/`Propage`, sans échelle d'archétype, sans ciblage aléatoire.
- Les réactions ne se résolvent **jamais en récursion** : les événements produits par une réaction ne déclenchent aucune réaction (anti-boucle) ; plusieurs réactions sur le même événement : par numéro d'unité croissant du porteur, puis par ordre d'armement ; une réaction se déclenche au plus une fois par événement.
- Les modificateurs de valeur ne sont pas remis à l'échelle par niveau/bonus/objets (règle P1). Les effets d'une réaction sont de **valeur fixe**, non augmentés.
- Domaine sans `UnityEngine`. Chaque test : `[Test, Description("Vérifie que … — parce que …")]` en français ; namespace `N29.HeroesDawn.Tests.Domain|Editor|Presentation`. Tout fichier neuf sous `Assets/` ou `Packages/` est commité avec son `.meta`. Les assertions des tests existants ne changent pas, sauf les nombres de cartes listés en tâche 8.
- Les enums sont sérialisés en entiers dans `cards.json` : **toute valeur d'enum nouvelle s'ajoute en fin** ; le test `SerializedEnums_AreStable` (CardDataTests) est mis à jour **uniquement par ajout** de lignes.
- Unity en batch, éditeur de ce projet fermé, **un seul Unity sur ce projet à la fois** (le projet se verrouille) : `UNITY="/c/Program Files/Unity/Hub/Editor/6000.6.3f1/Editor/Unity.exe"`, `PROJ="E:/Projets/HeroesDawn/principal/Code/Heroes Dawn Unity"`, `"$UNITY" -batchmode -nographics -projectPath "$PROJ" -runTests -testPlatform EditMode -testFilter "<classe>" -testResults "<xml>" -logFile "<log>"` (xml/logs sous `.superpowers/sdd/<plan>/`). Un autre Unity (autre projet, copie propre de vérification) peut tourner : ne pas y toucher.
- Visuel de repli : une carte sans illustration affiche déjà un visuel générique (`AgentArt.Card` retombe sur `Ability(id)`) ; la tâche 7 le garantit par un test sur les 250 cartes.

## Décisions de conception (prises ici, à valider par le concepteur)
- **Réaction armée sur le porteur** (le lanceur), quelle que soit la cible de la carte ; durée en tours **du porteur** (décrémentée à son début de tour, comme un statut) ; non retirée par `Purge` dans cette version.
- **Déclencheurs** : `EnemyReceivesStatus(statut)`, `AllyReceivesStatus(statut)`, `EnemyDefeated`, `AllyDefeated`, `CarrierHit`, `CarrierCrit` (relatifs au camp du porteur). Lus dans la liste des événements d'un pas de frise (`StatusAppliedEvent`, `UnitDefeatedEvent`, `DamageEvent` avec `Crit`).
- **Cible d'une réaction** : si la fiche nomme une cible (`sur <cible>`), même règle qu'une compétence lancée par le porteur (`TargetRule` + `TargetPick`) ; sinon l'unité concernée par l'événement quand c'est un ennemi/allié qui reçoit un statut, et le porteur dans les autres cas.
- **`Consomme`** : `ConsumeDamage` / `ConsumeHeal` retirent le statut de la cible et appliquent N × min(tours restants, M) (points de puissance, `Mark` compte 1 tour) ; **`Propage`** copie le statut (avec ses tours restants) de la cible vers tous les autres ennemis vivants.
- **Échelle d'archétype (P3)** : compte, pour le camp du lanceur, les cartes équipées (toutes unités vivantes ou non au départ du combat) portant l'archétype, la carte lancée comprise. **Les seuils d'archétype par escouade (bonus de statistiques à 2/4/6 cartes) ne sont PAS implémentés** : ils changeraient le comportement de toutes les escouades déjà équipées et relèvent de l'équilibrage ; les 14 cartes P3 n'utilisent que l'échelle.
- **Économie** : le coût en doublons d'un niveau passe de `(niveau+1)/2` à une table plus douce `1,1,1,2,2,2,3,3,3` (18 au total pour le niveau 10, au lieu de 25), et les **doublons d'autres cartes de la même étoile** comptent pour un tiers (arrondi : 3 doublons autres = 1 manquant) quand il manque des exemplaires de la carte elle-même ; le coût en Crédits ne change pas. Aucun nouveau stockage : le décompte se fait sur la collection existante.
- **Noms anglais** : dictionnaire anglais des cartes généré depuis la colonne `Name` des fiches ; repli français si absent.

## Grammaire P2 / P3 (cellules `Base`, `Niv.x` ; s'ajoute à la grammaire P1)
- `[Réaction N t : quand <événement> → <effet>]` ou `[Réaction N t : quand <événement> → <effet> sur <cible>]`. `N` de 1 à 5. Événements : `un ennemi reçoit <Statut>` · `un allié reçoit <Statut>` · `un ennemi est vaincu` · `un allié est vaincu` · `le porteur est touché` · `le porteur inflige un critique`. `<effet>` : un seul effet de la grammaire P1 (Dmg, Heal, Shield, St, Push, Adv, Gauge, Purge ; sans crochet, sans condition). `<cible>` : `Soi` · `ZE` · `ZA` · `1E:aléatoire` · `1E:faible` · `1A:prochain` · `1A:faible` · `déclencheur` (l'unité concernée par l'événement).
- `Consomme <Statut> : Dmg N par tour restant (max M)` et `Consomme <Statut> : Heal N par tour restant (max M)` (M de 1 à 10 ; `Consomme Mark : Dmg N par tour restant (max 1)`).
- `Propage <Statut>`.
- `[+N par carte <Archétype> équipée dans l'escouade (max M)]` : `N` bonus par carte, `max M` = nombre maximal de cartes comptées (1 à 10), comme `max` des échelles P1.
- Les cartes dont une cellule sort de cette grammaire sont corrigées en tâche 1 (cellule seule) ; l'importeur lève `CatalogImportException` pour tout le reste.

---

### Task 1: Fiches corrigées et normalisées (documentation seule)

**Files:**
- Modify: `Code/docs/catalogue/cartes-*.md` (cellules Base, Niv.x, Idée seulement), `Code/docs/catalogue/verifier-catalogue.ps1`
- Create: `Code/docs/catalogue/corrections-fiches.md` (journal des corrections : carte, ancienne cellule, nouvelle cellule, raison)

- [ ] **Étape 1 : corriger les fiches à avertissement** : cartes à cible `Soi` dont une cellule est inerte sur le lanceur (`Adv`, `Gauge +`, `Steal` sans `Dmg`) : card_iron_vest, card_stand_firm, card_adrenal_shell, card_last_gasp, card_hardened_post, card_adrenaline_surge, card_pursuit, thorn_hide, marked_bait, blazing_sprint (remplacer l'effet inerte par un effet qui agit sur le lanceur, de puissance équivalente, ou changer la cible vers un allié) ; conditions de palier qui lisent un état modifié par la base : lingering_mark, open_wound, torn_veil, vital_sap, harvest (rendre la condition vraie avant la base, ou inconditionnelle) ; `Bounce 1` sur saving_bite, hungry_spark, lifethirst (faute de frappe : `Bounce` est un pourcentage, 40 à 60). Le script `verifier-catalogue.ps1` ne doit plus émettre ces avertissements.
- [ ] **Étape 2 : normaliser les 54 fiches P2/P3** vers la grammaire ci-dessus : forme des réactions (`3 t` → `3t`, `(ZA)` → ` sur ZA`, apostrophes typographiques → `'`), `Consomme …` et `Propage …` comme indiqué, `[+N par carte <Archétype> équipée dans l'escouade (max M)]` avec `M` en nombre de cartes (1 à 10) : si le `max` d'origine était un plafond de total, le diviser par N ; un seul effet par réaction ; événements limités aux six formes. Ne changer ni l'id, le nom, les étoiles, la famille, la cible, le coût, l'archétype, le rôle ni la phase.
- [ ] **Étape 3 : ajouter au script** un contrôle de la grammaire P2/P3 (expressions régulières strictes sur les crochets de réaction, `Consomme`, `Propage`, échelle d'archétype) qui produit une **erreur** pour toute fiche P2/P3 hors grammaire ; lancer : 0 erreur.
- [ ] **Étape 4 : commit** `docs: fiches corrigées et normalisées pour les phases P2 et P3`.

### Task 2: Réactions (moteur)

**Files:**
- Create: `Definitions/Reaction.cs`, `Domain/Combat/CombatSimulation.Reactions.cs`, `Assets/Tests/Domain/ReactionTests.cs`
- Modify: `Definitions/Enums.cs` (`EffectType.ArmReaction` en fin), `Definitions/EffectDef.cs` (propriété `Reaction`, copiée par `With`), `Domain/Combat/CombatUnit.cs` (`Reactions`), `Domain/Combat/CombatEvents.cs` (`ReactionArmedEvent`, `ReactionTriggeredEvent`), `Domain/Combat/CombatSimulation.cs`/`.Actions.cs` (appel de `ProcessReactions`, décrément des durées au début de tour)

**Interfaces:**
- Produit : `enum ReactionTrigger { EnemyReceivesStatus, AllyReceivesStatus, EnemyDefeated, AllyDefeated, CarrierHit, CarrierCrit }` ; `sealed class ReactionDef { ReactionTrigger Trigger; StatusType Status; int Turns; EffectDef Effect; TargetRule Target; TargetPick Pick; StatusType PickStatus; bool OnTriggerUnit }` ; `EffectDef.Reaction : ReactionDef?` ; `EffectType.ArmReaction` (arme sur le **lanceur**) ; `CombatUnit.Reactions : List<ReactionInstance>` (`ReactionInstance { ReactionDef Def; int TurnsLeft; int Order }`) ; `CombatSimulation.ProcessReactions(List<CombatEvent> output, int fromIndex)`.
- Règles : drapeau interne « une réaction est armée quelque part » : sans réaction armée, `ProcessReactions` ne fait **aucun** travail ni tirage (golden intact). Appel après chaque `StepOne` et après chaque commande appliquée par `Submit(cmd, from)`, sur les événements du pas seulement. Les événements produits par le traitement d'une réaction ne sont pas relus.

- [ ] **Étape 1 : tests** (noms exacts) : `ArmReaction_ArmsTheCaster_WithTurns` ; `EnemyReceivesStatus_TriggersOnlyForThatStatus` ; `EnemyDefeated_Triggers_ForEachDefeat` ; `AllyDefeated_Triggers` ; `CarrierHit_Triggers_WhenTheCarrierLosesHp` ; `CarrierCrit_Triggers_OnTheCarriersCritical` ; `ReactionEffect_UsesFixedValue_NotScaledByLevel` ; `ReactionTarget_TriggerUnit_AndNamedTargets` (`1E:aléatoire` tire exactement un `_rng.Range`, `ZA`, `1A:prochain`) ; `Reaction_ExpiresAfterItsTurns_AtTheCarriersTurnStart` ; `ReactionsDoNotTriggerReactions_NoLoop` ; `SeveralReactions_OrderedByCarrierIdThenArming` ; `NoReaction_DrawsNothing_AndLogsNothing` (empreinte du générateur identique à un combat sans la carte) ; `DeadCarrier_DoesNotTrigger` ; `ReArming_RefreshesTheExistingReaction` (même définition : durée remise à `Turns`, pas de doublon).
- [ ] **Étape 2 : lancer** `ReactionTests` ; attendu : échec de compilation.
- [ ] **Étape 3 : implémenter** types, armement (`ApplyEffects` : `case EffectType.ArmReaction`), traitement, événements et décompte des durées.
- [ ] **Étape 4 : lancer** `ReactionTests`, `ConditionCombatTests`, `TargetPickTests`, `CardMechanicsTests`, `CollectionRulesTests` et toute la classe `CombatGoldenTests` + `WaveGoldenTests`. Attendu : vert, références intactes.
- [ ] **Étape 5 : commit** `feat: réactions armées par les cartes (déclencheurs, file, anti-boucle)`.

### Task 3: `Consomme` et `Propage`

**Files:**
- Modify: `Definitions/Enums.cs` (`EffectType.ConsumeDamage`, `ConsumeHeal`, `Spread` en fin), `Domain/Combat/CombatSimulation.Actions.cs` (`ApplyEffects`)
- Test: `Assets/Tests/Domain/ConsumeSpreadTests.cs`

**Interfaces:** `ConsumeDamage`/`ConsumeHeal` : `Status` = statut consommé, `Value` = N par tour restant, `Duration` = M (maximum de tours comptés, ≥ 1) ; `Spread` : `Status`.

- [ ] **Étape 1 : tests** : `ConsumeDamage_RemovesTheStatus_AndHitsForNTimesRemainingTurns` ; `ConsumeDamage_IsCappedByMaxTurns` ; `ConsumeMark_CountsOneTurn` ; `ConsumeHeal_HealsTheTarget` ; `Consume_DoesNothing_WhenTheStatusIsAbsent` (aucun tirage, aucun événement de dégâts) ; `ConsumeDamage_UsesTheNormalDamageFormula` (critique, défense, variance : un seul coup de puissance N × tours via `HitWithDamage`) ; `Spread_CopiesTheStatusWithRemainingTurnsToAllOtherEnemies` ; `Spread_SkipsDeadAndSourceTargets` ; `Spread_DoesNothing_WhenAbsent`.
- [ ] **Étape 2 : lancer** `ConsumeSpreadTests` ; attendu : échec.
- [ ] **Étape 3 : implémenter** dans `ApplyEffects` (après le tirage de chance, comme les autres effets) ; **ne pas** passer par `v` des modificateurs (Valeur = N, calcul propre).
- [ ] **Étape 4 : lancer** `ConsumeSpreadTests`, `ReactionTests`, `ConditionCombatTests`, `CombatGoldenTests`, `WaveGoldenTests`. Attendu : vert.
- [ ] **Étape 5 : commit** `feat: effets Consomme et Propage`.

### Task 4: Échelle d'archétype (P3) et textes des phases P2/P3

**Files:**
- Modify: `Definitions/EffectCondition.cs` (`ScaleKind.EquippedArchetypeCards` en fin ; `ValueMod.PerArchetype : Archetype?`), `Domain/Combat/CombatUnit.cs` (`EquippedArchetypes`), `Domain/Combat/UnitFactory.cs` (remplit `EquippedArchetypes` des cartes équipées), `Domain/Combat/CombatSimulation.Conditions.cs` (`CountOf`), `Presentation/Common/SkillText.cs`, `Presentation/Common/LocCombat.cs`
- Test: `Assets/Tests/Domain/ArchetypeScaleTests.cs`, `Assets/Tests/Presentation/SkillTextReactionTests.cs`

- [ ] **Étape 1 : tests de moteur** : `ArchetypeScale_CountsEquippedCardsOfTheSquad_CasterCardIncluded` ; `ArchetypeScale_IsCappedByMaxCount` ; `ArchetypeScale_IgnoresOtherArchetypes` ; `ArchetypeScale_CountsOnlyTheCastersSide` ; `NoArchetypeScale_ChangesNothing` (empreinte du générateur et des événements identique).
- [ ] **Étape 2 : tests de texte** (phrases exactes en français, une par forme) : réaction (chaque `ReactionTrigger`, avec statut, durée en tours, cible nommée ou non), `ConsumeDamage`, `ConsumeHeal`, `Spread`, échelle d'archétype (nom de l'archétype en français) ; clés de texte pour chaque valeur d'enum nouvelle (le test d'existence itère sur `Enum.GetValues`) ; un effet sans ces briques garde exactement son ancien texte.
- [ ] **Étape 3 : lancer** les deux classes ; attendu : échec.
- [ ] **Étape 4 : implémenter** moteur et textes (aucune chaîne en dur).
- [ ] **Étape 5 : lancer** `ArchetypeScaleTests`, `SkillTextReactionTests`, `SkillTextConditionTests`, `ConditionCombatTests`, `CombatGoldenTests`, `WaveGoldenTests`. Attendu : vert.
- [ ] **Étape 6 : commit** `feat: échelle d'archétype et textes des réactions, Consomme et Propage`.

### Task 5: Données et importeur pour les phases P2 et P3

**Files:**
- Modify: `Authoring/ContentData.cs` (`EffectRecord` : `reaction` plat — `reactionTrigger, reactionStatus, reactionTurns, reactionEffect, reactionTarget, reactionPick, reactionPickStatus, reactionOnTriggerUnit` ; `ModRecord` : `perArchetype`), `Authoring/ContentDataConverter.cs`, `Authoring/CardDataLibrary.cs` (`Validate` : nouveaux types et énumérations en plage, `ArmReaction` seulement avec réaction, `Consume*` avec `Duration` ≥ 1, `Spread` avec statut), `Authoring/CatalogImporter.cs` (grammaire P2/P3), `Editor/Design/ImportCatalogMenu.cs` (accepte les phases P2/P3)
- Test: `Assets/Tests/Domain/CatalogImporterP2Tests.cs`, `Assets/Tests/Domain/CardDataTests.cs` (ajouts)

- [ ] **Étape 1 : tests** : un cas par forme de la grammaire P2/P3 (chaque événement, avec et sans `sur <cible>`, `Consomme … Dmg/Heal`, `Propage`, échelle d'archétype) ; rejets : réaction sans effet, deux effets, événement inconnu, `max` > 10, statut inconnu, `Consomme` sans `par tour restant`, archétype inconnu ; `EveryP2P3Row_Imports` (les 54 fiches P2/P3 s'importent) et `EveryRow_Imports` (les 250 fiches, aucun rejet, comptes 250 / P1 196 / P2+P3 54) ; aller-retour JSON de cartes à réaction, `Consomme`, `Propage`, échelle d'archétype (champs plats : `ConditionKind` 0 et `ScaleKind` 0 ne sont pas confondus avec « absent ») ; `Validate` rejette chaque incohérence ; `SerializedEnums_AreStable` complété par ajout.
- [ ] **Étape 2 : lancer** ; attendu : échec. **Étape 3 : implémenter.**
- [ ] **Étape 4 : lancer** `CatalogImporterP2Tests`, `CatalogImporterTests`, `CardDataTests`, `ImportCatalogMenuTests`, `CombatGoldenTests`. Attendu : vert ; si une fiche P2/P3 est rejetée, corriger la **cellule** (journal dans `corrections-fiches.md`), jamais l'analyseur pour l'élargir.
- [ ] **Étape 5 : commit** `feat: importeur et données pour les cartes à réaction, Consomme, Propage et échelle d'archétype`.

### Task 6: Économie des doublons

**Files:**
- Modify: `Definitions/CardLevels.cs` (`CopiesToUpgrade`), `Application/…` (cas d'usage `UpgradeCard` et le décompte des exemplaires : trouver par recherche `UpgradeCard`, `CopiesToUpgrade`), présentation du grimoire (`Hub/Pages/Grimoire/GrimoirePage.cs`, `Presentation/Hub/CollectionText.cs`) pour le texte du coût
- Test: `Assets/Tests/Domain/Application/CardUpgradeTests.cs` (ajouts), `Assets/Tests/Domain/CardEconomyTests.cs`

- [ ] **Étape 1 : tests** : `CopiesToUpgrade_Table` (niveaux 1 à 9 : 1,1,1,2,2,2,3,3,3 ; total 18) ; `Upgrade_UsesOwnCopiesFirst` ; `Upgrade_CompletesWithOtherCardsOfTheSameStars_ThreeForOne` ; `Upgrade_RefusesOtherStars` ; `Upgrade_RefusesWhenNotEnough_AndChangesNothing` ; `Upgrade_ConsumesExactlyTheCopiesNeeded` (exemplaires propres d'abord, puis autres cartes de la même étoile, par identifiant croissant, sans descendre une carte sous un exemplaire) ; `Upgrade_KeepsCreditsCost` ; compatibilité de sauvegarde : une sauvegarde existante se recharge et les niveaux restent ; les tests existants de `CardUpgradeTests` sont adaptés **uniquement** pour les nouveaux nombres de doublons (assertions de logique inchangées).
- [ ] **Étape 2 : lancer** ; attendu : échec. **Étape 3 : implémenter** dans le domaine et le cas d'usage ; le texte du bouton « Améliorer » indique le coût (propres + équivalent d'autres cartes) via une clé de texte.
- [ ] **Étape 4 : lancer** `CardEconomyTests`, `CardUpgradeTests`, `CollectionRulesTests`, `CollectionPagesPresenterTests`, `CollectionBookTests`, `SaveMigrationTests` et `CombatGoldenTests`. Attendu : vert.
- [ ] **Étape 5 : commit** `feat: économie des doublons praticable avec 250 cartes`.

### Task 7: Noms français et anglais des 250 cartes, visuel de repli garanti

**Files:**
- Create: `Editor/Design/GenerateCardNamesMenu.cs`, `Presentation/Common/LocCollectionNames.cs` (généré : FR des 190 cartes à ajouter), `Presentation/Common/LocCollectionNamesEn.cs` (généré : EN des 250), `Assets/Tests/Presentation/CardNamesTests.cs`
- Modify: `Presentation/Common/Loc.cs`/`LocEnglish.cs` (fusion du dictionnaire anglais des cartes), `Presentation/Common/LocCollection.cs` (pas de doublon de clé avec le nouveau fichier)

- [ ] **Étape 1 : tests** : `EveryCard_HasAFrenchName_EqualToItsSheet` (250 cartes du catalogue, colonne Nom) ; `EveryCard_HasAnEnglishName_EqualToItsSheet` (colonne Name) ; `English_FallsBackToFrench_WhenMissing` ; `NoDuplicateNameKeys` ; `EveryCardHasAnArtSprite_EvenWithoutItsOwnImage` (pour chaque carte du catalogue, `AgentArt.Card(id)` renvoie un sprite non nul : l'image propre si elle existe, sinon le visuel de repli — vérifier le comportement réel de `AgentArt` en EditMode ; si `Resources.Load` n'est pas utilisable en test, tester la fonction pure de choix du repli) ; `GeneratedFilesAreUpToDate` (régénérer en mémoire depuis les fiches donne exactement le contenu des fichiers commités).
- [ ] **Étape 2 : lancer** ; attendu : échec. **Étape 3 : implémenter** l'outil (menu + méthode `-executeMethod`), l'exécuter en batch, commiter les fichiers générés avec leurs `.meta`.
- [ ] **Étape 4 : lancer** `CardNamesTests`, `CollectionPagesPresenterTests`, `TestDescriptionsTests`. Attendu : vert.
- [ ] **Étape 5 : commit** `feat: noms français et anglais des 250 cartes et repli visuel garanti`.

### Task 8: Mise en jeu des 190 cartes restantes (250 au total)

**Files:**
- Modify: `Code/docs/catalogue/mise-en-jeu.txt` (les 250 identifiants ; l'ordre des 60 premiers inchangé), `Assets/_Project/Resources/Content/cards.json`, `Core/RulesVersion.cs` (`"0.3.0"`), tests de comptage (`CollectionRulesTests`, `CardStarsTests`, `DesignToolsDataTests`, `CollectionPagesPresenterTests`, `CardArchetypeTests`, `CardDataTests`)
- Create: `Assets/Tests/Domain/AllCardsTests.cs`, `Assets/Tests/Domain/Golden/AllCardsGoldenTests.cs`, `Assets/Tests/Domain/Golden/combat-golden-p4.txt` (+ `.meta`)

- [ ] **Étape 1 : mettre en jeu** : lister les 250 ids dans `mise-en-jeu.txt`, exécuter l'outil d'import en batch ; `cards.json` passe à 250 cartes ; les **60 premières cartes gardent exactement les mêmes définitions** (test : relecture du fichier de départ et du fichier final avec le schéma courant, comparaison champ par champ ; les octets du fichier peuvent changer car JsonUtility écrit désormais les champs de réaction et d archétype pour toutes les cartes).
- [ ] **Étape 2 : tests de comptage** (nombres uniquement) : 250 cartes ; étoiles ★1 90 / ★2 70 / ★3 50 / ★4 28 / ★5 12 ; familles 62 ± 3 ; archétypes 35 / 35 / 35 / 35 / 35 / 35 / 40 neutres ; les règles de contenu (3 paliers 3/6/10, pas d'effet allié sur une carte qui cible un ennemi, nature = nature déduite hors liste explicite) s'appliquent aux 250.
- [ ] **Étape 3 : `AllCardsTests`** : `EveryCard_IsCastInAuto` (chaque carte, avec un second allié pour les cartes alliées, est lancée au moins une fois) ; `EveryCardTier_ChangesBehaviour_ForAtLeastOneScenario` (chaque palier modifie l'empreinte d'au moins un scénario de sa carte aux niveaux 1, 3, 6, 10 ; les cartes dont un palier est inerte dans tous les scénarios sont **listées dans le test avec la raison** et corrigées dans la fiche si c'est une faute) ; `EveryCard_TextIsReadable` (la description générée ne contient ni `[`, ni clé brute) ; `NoCard_CrashesACombat` (combat Auto complet avec chaque carte jusqu'à la fin).
- [ ] **Étape 4 : garde-fou** : scénarios `all:<id>:L1|L6|L10:s7` pour les 190 nouvelles cartes (second allié pour les cartes alliées) enregistrés dans `combat-golden-p4.txt` (variable `HEROES_RECORD_GOLDEN_P4=1`, test `[Explicit]` avec garde et `Description`) ; `combat-golden.txt` et `combat-golden-p1.txt` **intacts**.
- [ ] **Étape 5 : lancer** `AllCardsTests`, `AllCardsGoldenTests`, `CardDataTests`, `CatalogImporterP2Tests`, les classes de comptage, `CardMechanicsTests`, `TestDescriptionsTests`, `CombatGoldenTests`, `WaveGoldenTests`, puis tout EditMode (échecs étrangers connus : `BuildBudgetTests` seulement). Corriger les fiches (cellules) et réimporter pour tout défaut de carte révélé (carte inerte, palier mort) en le notant dans `corrections-fiches.md`.
- [ ] **Étape 6 : commit** `feat: les 250 cartes en jeu (phases P2 et P3 comprises)`.

### Task 9: Vérification complète et documentation

**Files:**
- Create: `Code/docs/test-reports/2026-11-16_plan-complet-cartes.md`
- Modify: `05_Production/Decisions.md` (une ligne, **non stagée**), `docs/specs/2026-11-16-capacites-composables-synergies-design.md` (P2, P3, P4 faites)

- [ ] **Étape 1 : EditMode complet puis PlayMode complet** (`run_in_background`) ; lister chaque échec avec sa première ligne d'erreur ; échecs étrangers connus : EditMode `BuildBudgetTests` ; PlayMode `CityMapScreenUiToolkitTests` ×2, `MoscowFullRunTests`, `QuestLogPlayTests`. Tout échec touchant cartes/combat/économie/noms = bloquant.
- [ ] **Étape 2 : confirmer** `combat-golden.txt` inchangé depuis `14cadfa2` et `combat-golden-p1.txt` inchangé depuis la fix wave de P1 (ou modifications justifiées en détail) ; état de `CombatGoldenTests` sur l'arbre de l'utilisateur.
- [ ] **Étape 3 : rapport** : totaux, durées, conditions (arbre non propre), garde-fous et leurs limites, décisions de conception ci-dessus à valider, liste des fiches corrigées (nombre), cartes sans palier actif listées, limites connues (interface : pas d'icône dédiée aux réactions, seuls des événements ; seuils d'archétype non implémentés ; équilibrage non testé en partie jouée).
- [ ] **Étape 4 : commit** (rapport avec `git add -f`, spec) : `docs: rapport du plan complet des cartes`.
