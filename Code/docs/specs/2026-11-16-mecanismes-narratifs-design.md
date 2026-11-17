# Mécanismes narratifs manquants : conception

> Statut : **proposition à valider par l'utilisateur avant tout code** (demandé par le script narratif de New York et Moscou, 2026-11-16).
> Sources : `02_Lore_Narration/13_Script_New_York.md` et `14_Script_Moscou.md` (clés `boss.phase*`, `choice`, `branch_*`, `epilogue.*`, `stage.*`, variantes D/M, `ilia`, phrases d'ouverture).

## 1. Ce qui existe déjà

| Besoin du script | Existe | Manque |
|---|---|---|
| Dialogue d'un boss à chaque phase | `BossPhaseDef` (seuils 66 % / 33 %, stats, nom) ; `BossPhaseChangedEvent` ; bannière et pause dans `CombatPresenter` | Des répliques portées par la phase ; leur lecture en combat |
| Choix mémorisé (Harlem 3.3, Place Rouge 4.4) | Cases Récit avec événements à choix (`ResolveNarrativeUseCase`) ; sauvegarde du profil (`MetaSave`, listes de compteurs) | Un choix d'histoire nommé, mémorisé, qui oriente des scènes ; branches ; épilogue de chapitre |
| Scène « stage » de Manhattan 4.3 | `NarrationDef` Before/After sur n'importe quelle case | Rien de nouveau (voir 4) |
| Variantes Agent D / Agent M | `session.Route.HeroId` | Un filtre par parcours sur chaque réplique |
| Voix off d'Ilia | Locuteurs via `speaker.<id>` | Le locuteur `ilia` et un affichage sans portrait |
| Phrases d'ouverture par parcours | Fenêtre d'ouverture de chapitre (`ChapterIntroFlow`, clés `chapter.<id>.intro`) | La phrase ajoutée selon le parcours |

## 2. Propositions

**2.1 Dialogue de phase.** `BossPhaseDef` reçoit une liste facultative de répliques (`NarrationLineDef`, clés `narration.<zone>.boss.phase1.0`...). `BossPhaseChangedEvent` les transporte. `CombatPresenter` les joue dans la boîte de dialogue après la bannière, le combat restant en pause. Pas d'effet sur la simulation (déterminisme et rapports d'équilibrage inchangés : seul l'affichage lit les répliques).

**2.2 Choix mémorisé.**
- Données : `ChoiceDef { Id, PromptKeys, Options[ { Id, LabelKey, BranchLines } ] }` attaché à une case (Récit) via `NarrationDef.Choice`.
- Mémorisation : nouvelle liste `narrativeChoices` dans `MetaSave` (`{ id, option }`, même schéma que `CounterSave`), lue et écrite par un service d'application `NarrativeChoices` (`Get(id)`, `Set(id, option)`). Ancien profil sans le champ : liste vide (comme `chaptersSeen`).
- Règle : le choix se fait **une seule fois** par profil ; une zone rejouée rejoue la scène de la branche déjà choisie, sans reproposer. Alternative : permettre de rechoisir au rejeu (proposée plus bas).
- Interface : deux boutons dans la boîte de dialogue après `choice.1` ; la branche choisie joue ses répliques (`branch_clinic.*`, `branch_market.*`, `branch_arrest.*`, `branch_listen.*`).
- L'objectif `obj_priority` (Harlem 3.3) se valide au choix fait.

**2.3 Épilogue de chapitre.** Registre `ChapterEpilogues` : par chapitre, une liste de scènes conditionnelles (`epilogue.<option>`, `epilogue.common`). Jouées à la validation de la dernière zone du chapitre, après `chapter_end`, avant l'écran de fin de zone. Sélection par le choix mémorisé.

**2.4 Scène « stage » (Manhattan 4.3).** Aucun mécanisme neuf : les cinq répliques `stage.0` à `stage.4` deviennent la partie After de la case de fin de la zone 4.3 (carrefour fixe n17 du graphe C), jouée avant la sortie. La réplique `stage.5` suit la règle des variantes (2.5).

**2.5 Variantes par parcours.** `NarrationLineDef` reçoit un champ `Route` facultatif (`agent_d`, `agent_m`, ou vide = tous). `NarrationLines.Build` ne garde que les répliques de tous les parcours et celles du parcours du joueur. Pour une variante qui **remplace** une réplique (ex. `intro.4` dite par Agent M au lieu d'Agent D), deux répliques de même rang portent chacune leur `Route`. Il faut donc dans le script des clés distinctes pour chaque variante (suffixe `.d` / `.m` proposé) : à confirmer avec l'auteur du script. Les indices de la cathédrale sont des répliques `Route = agent_m`.

**2.6 Voix off d'Ilia.** Locuteur `ilia` (nom « Ilia ») et drapeau de réplique « voix off » : boîte de dialogue sans portrait, texte en italique.

**2.7 Phrases d'ouverture.** `ChapterIntroFlow` ajoute, sous le texte d'ouverture, la phrase `chapter.<id>.intro.agent_d` ou `.agent_m` selon le parcours.

## 3. Points qui demandent une décision

1. **Choix rejouable ?** Définitif (proposé : simple, cohérent avec « le choix modifie l'épilogue ») ou modifiable au rejeu de la zone.
2. **Combat : les dialogues de phase interrompent-ils le combat ?** Proposé : oui, pause courte, touche pour passer.
3. **Clés de variante** : l'auteur du script ajoute des clés distinctes par parcours (`.d` / `.m`), ou on garde une réplique de base commune plus une réplique ajoutée selon le parcours.
4. **Épilogue** : où l'afficher. Proposé : après la dernière zone du chapitre, avant l'écran de récompenses.

## 4. Tests prévus

Domaine : mémorisation et relecture du choix, ancien profil sans champ ; filtre par parcours ; dialogue de phase transporté sans changer la simulation (même `StateHash` de combat avec ou sans répliques). Présentation : choix à deux boutons, branche jouée, épilogue sélectionné, locuteur `ilia` sans portrait. Ordre de travail : 2.5 et 2.6, puis 2.1, puis 2.2 et 2.3, puis 2.7.
