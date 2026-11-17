# Narration New York et Moscou : plan de réalisation

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Écrire le script complet (trame, événements, dialogues FR) des 8 chapitres de New York et de Moscou, relisible et convertible en clés de localisation.

**Architecture:** Deux fichiers Markdown de script (`13_Script_New_York.md`, `14_Script_Moscou.md`), un gabarit unique par zone, des clés par emplacement narratif (slots) et non par numéro de case. Un chapitre livré et relu par l'utilisateur avant le suivant.

**Tech Stack:** Markdown uniquement. Aucun code Unity touché. Vérifications par `grep` sur les fichiers de script.

**Spec:** `Code/docs/specs/2026-11-16-narration-new-york-moscou-design.md` (approuvé le 2026-11-16).

## Global Constraints

- Langue : français uniquement dans cette étape ; EN en seconde passe, hors plan.
- Le dépôt n'est pas un dépôt git : pas d'étape de commit ; sauvegarde = fichier écrit.
- Clés : `narration.<zone>.<slot>.<i>` ; slots : `intro` (`mid` fusionné le 2026-11-16), `elite.before`, `elite.after`, `exit`, `boss.before`, `boss.after`, `chapter_end`. Zones : `zone_ny_X_Y`, `zone_mos_X_Y`.
- Volume : 8 à 14 répliques par zone normale, 15 à 25 par zone de boss.
- Aucun soldat ni agent de l'AMIF à l'écran à Moscou avant le chapitre 4 (`mos_place_rouge`).
- Stroke à New York seulement aux chapitres 1 (par son Titan) et 4.
- Les choix de Harlem et de la Place Rouge ne modifient que l'épilogue.
- Noms proposés du design §4 ; le Passeur ne dit jamais son nom.
- Preuve n°1 : Queens 2.4. Preuve n°2 : Sergueiv 1.2 (partie 1) et Kapotnya 2.3 (partie 2).
- Ton : New York = visibilité et ironie ; Moscou = politesse et froideur.
- Les textes d'ouverture « existants » de `09` et `10` sont repris mot pour mot.

## Review Focus

- Un locuteur sans nom affichable : chaque locuteur utilisé figure dans la table des locuteurs (tâche 0).
- Une clé en double dans un même fichier : le contrôle de la tâche 9 compte les clés uniques.
- Une zone sans variante D/M alors que le lore en exige une : Harlem (D) et Place Rouge 4.4 (M) ont une section Variantes non vide.
- Une révélation en avance (collusion AMIF/gangs avant Queens ; cathédrale avant Place Rouge hors indices) : chaque tâche de chapitre vérifie ses scènes clés contre le tableau de la bible.
- Une réplique en conflit avec le code existant de Brooklyn ou l'annexe A de Sergueiv : tâches 1 et 5 les relisent d'abord.

---

## Format de zone (référence pour toutes les tâches)

Chaque zone contient, dans cet ordre : `#### <id> : <lieu>` ; **Fiche** (enjeu, ton, rôle dans les preuves) ; **Trame** (3 à 6 événements) ; **Scènes** (tableau `clé | locuteur | FR`) ; **Variantes** (D / M) ; **Fragment** (id, titre, texte) ; **QG** (archive 2-3 phrases, fil 1 phrase).

### Task 0: Squelettes et table des locuteurs

**Files:**
- Create: `02_Lore_Narration/13_Script_New_York.md`, `02_Lore_Narration/14_Script_Moscou.md`

**Interfaces:**
- Produces: la table des locuteurs et les emplacements de chapitre que les tâches 1 à 8 remplissent.

- [ ] **Step 1:** Créer chaque fichier avec : en-tête (date, statut, sources), rappel du format de zone et des slots, **table des locuteurs** (id, nom affiché, rôle), et quatre titres de chapitre vides (`## Chapitre N : <nom>` avec le texte d'ouverture existant repris mot pour mot).
- [ ] **Step 2:** Locuteurs communs : `agent_d`, `agent_m`, `rayner`, `stroke`, `ilona_varga`, `giulia_rossi`, `lucas_mendes`. New York : `regisseur`, `braise`, `dr_colby`, `crocodile`, un indic générique `indic`. Moscou : `passeur`, `enforcer`, `contremaitre`, `orlan`, `conseiller`, `ilia`.
- [ ] **Step 3:** Vérifier : `grep -c "^## Chapitre" 02_Lore_Narration/13_Script_New_York.md` et `14_Script_Moscou.md` renvoient 4 chacun.

### Task 1: Moscou, chapitre 1 Sergueiv

**Files:**
- Modify: `02_Lore_Narration/14_Script_Moscou.md`
- Read: annexes A et B de `Code/docs/specs/2026-11-13-moscou-agent-m-design.md`

**Interfaces:**
- Consumes: la table des locuteurs (tâche 0). Produces : le tableau slot → case de Sergueiv (n14, n18, n19 d'après le design Agent M) et le ton fixé pour Moscou.

- [ ] **Step 1:** Reprendre les répliques de l'annexe A comme base, avec leurs clés converties en slots, sans les modifier.
- [ ] **Step 2:** Compléter chaque zone 1.1 à 1.4 : trame, répliques de `intro` et de `mid` manquantes, fragments (annexe B), QG, indices précoces de la cathédrale (au plus un par zone, ambigu).
- [ ] **Step 3:** Variantes : phrase d'ouverture d'Agent M (« elle connaît la ville ») et celle d'Agent D pour l'acte III (« il arrive en étranger »).
- [ ] **Step 4:** Vérifier : 4 zones présentes ; `grep -i "amif"` ne trouve aucune réplique d'un personnage de l'AMIF à l'écran (mentions par les mafieux seulement) ; le Passeur n'est jamais nommé.
- [ ] **Step 5:** Présenter le chapitre à l'utilisateur et attendre sa relecture avant la tâche 2.

### Task 2: New York, chapitre 2 Queens

**Files:**
- Modify: `02_Lore_Narration/13_Script_New_York.md`

- [ ] **Step 1:** Écrire 2.1 à 2.4 d'après le tableau de `09_Quetes_New_York.md` (caméras au sigle AMIF, aveu de l'indic, convoi de captifs, Régisseur et registre de transport = preuve n°1). Réplique du Régisseur de fin reprise telle quelle : « Vous croyez abattre un gang. Vous venez de déranger un contrat. »
- [ ] **Step 2:** Zone de boss 2.4 : 15 à 25 répliques, boss.before, boss.after, chapter_end.
- [ ] **Step 3:** Vérifier : 4 zones ; la preuve n°1 est nommée exactement « registre de transport » ; Stroke absent du chapitre.
- [ ] **Step 4:** Présenter et attendre la relecture.

### Task 3: New York, chapitre 3 Harlem

**Files:**
- Modify: `02_Lore_Narration/13_Script_New_York.md`

- [ ] **Step 1:** Écrire 3.1 à 3.4 (bloc incendié, clinique avec Dr Colby, marché, rue de l'atelier avec Braise et la mort de Théo).
- [ ] **Step 2:** Zone 3.3 : la scène de choix avec deux branches de répliques (clinique d'abord / marché d'abord).
- [ ] **Step 3:** Section **Épilogues** du chapitre : un par branche, perte visible mais sans mort, et fin commune.
- [ ] **Step 4:** Variantes : version Agent D (ami d'enfance Théo, trottoir de la craie) et version Agent M (un habitant du quartier tient le rôle de Théo).
- [ ] **Step 5:** Vérifier : section Variantes non vide en 3.4 ; les deux épilogues existent ; aucune réplique n'indique que le choix change la fin du jeu.
- [ ] **Step 6:** Présenter et attendre la relecture.

### Task 4: New York, chapitre 4 Manhattan

**Files:**
- Modify: `02_Lore_Narration/13_Script_New_York.md`

- [ ] **Step 1:** Écrire 4.1 à 4.4 (propagande, recoupement des financeurs, annonce des Accords avec Rayner et Stroke, boss Stroke qui conclut « Salem décidera »).
- [ ] **Step 2:** Zone 4.4 : 15 à 25 répliques ; Stroke et Rayner parlent ; les noms de financeurs ne sont pas révélés en entier (accroche vers Paris et Salem).
- [ ] **Step 3:** Fin de chapitre : Accords publics et fil vers Salem.
- [ ] **Step 4:** Vérifier : Stroke apparaît en 4.3 et 4.4 seulement dans ce chapitre.
- [ ] **Step 5:** Présenter et attendre la relecture.

### Task 5: New York, chapitre 1 Brooklyn (relecture et complément)

**Files:**
- Modify: `02_Lore_Narration/13_Script_New_York.md`
- Read: le code des zones de Brooklyn (chercher `zone_ny_1_1` dans `Code/Heroes Dawn Unity/Assets` et les textes `narration.` associés) et `Code/docs/specs/2026-10-12-campagne-brooklyn-design.md`

- [ ] **Step 1:** Extraire les répliques existantes de Brooklyn du code et du design, et les placer telles quelles dans le script, avec leurs clés actuelles et le tableau slot → case.
- [ ] **Step 2:** Combler les manques (trame, fragments, QG, scène Rayner-Stroke de fin de chapitre) sans changer les répliques existantes ; signaler toute contradiction avec le canon au lieu de la corriger.
- [ ] **Step 3:** Vérifier : 4 zones ; aucune réplique existante modifiée (comparer au texte source).
- [ ] **Step 4:** Présenter et attendre la relecture.

### Task 6: Moscou, chapitre 2 Kapotnya

**Files:**
- Modify: `02_Lore_Narration/14_Script_Moscou.md`

- [ ] **Step 1:** Écrire 2.1 à 2.4 (fuite volontaire, camions de résidus, laboratoire clandestin avec l'AMIF client dans les dossiers = preuve n°2 partie 2, boss Contremaître : « Orlan paie. »).
- [ ] **Step 2:** L'AMIF n'existe que dans les dossiers et les mentions de « clients », jamais à l'écran.
- [ ] **Step 3:** Variantes D et M d'ouverture ; fragments et QG.
- [ ] **Step 4:** Vérifier : 4 zones ; preuve n°2 nommée comme au chapitre 1 ; aucun agent de l'AMIF parle.
- [ ] **Step 5:** Présenter et attendre la relecture.

### Task 7: Moscou, chapitre 3 Sadovoye Koltso

**Files:**
- Modify: `02_Lore_Narration/14_Script_Moscou.md`

- [ ] **Step 1:** Écrire 3.1 à 3.4 (trophées en bocaux, enchères, fausse vidéo, boss Orlan : « quelqu'un au-dessus »).
- [ ] **Step 2:** Ton : le pouvoir se met en scène, politesse froide, hologrammes.
- [ ] **Step 3:** Fin de chapitre : la rumeur circule malgré le démenti.
- [ ] **Step 4:** Vérifier : 4 zones ; Orlan n'avoue pas le nom du Conseiller.
- [ ] **Step 5:** Présenter et attendre la relecture.

### Task 8: Moscou, chapitre 4 Place Rouge

**Files:**
- Modify: `02_Lore_Narration/14_Script_Moscou.md`

- [ ] **Step 1:** Écrire 4.1 à 4.4 (forces spéciales sans ordre officiel, salles soviétiques, offre du Conseiller, boss et choix arrêter/écouter).
- [ ] **Step 2:** Première apparition d'un représentant de l'AMIF à l'écran (permise ici seulement).
- [ ] **Step 3:** Section **Épilogues** : deux branches (arrêter / écouter) ; dans les deux, les salles soviétiques mènent à Salem.
- [ ] **Step 4:** Variante Agent M en 4.4 : le Conseiller révèle que l'attentat de la cathédrale était orchestré, avec Ilia ; version Agent D sans cette révélation personnelle.
- [ ] **Step 5:** Vérifier : section Variantes non vide en 4.4 ; deux épilogues présents.
- [ ] **Step 6:** Présenter et attendre la relecture.

### Task 9: Relecture transversale et mise à jour du canon

**Files:**
- Modify: `02_Lore_Narration/13_Script_New_York.md`, `14_Script_Moscou.md`, `09_Quetes_New_York.md`, `10_Quetes_Moscou.md`, `08_Bible_Narrative.md` (section 7 « Points à valider »), `README.md` du dossier

- [ ] **Step 1:** Contrôle des clés : lister les clés de chaque fichier et vérifier qu'elles sont uniques ; `grep -o "narration\.[a-z0-9_.]*" <fichier> | sort | uniq -d` ne renvoie rien.
- [ ] **Step 2:** Contrôle des locuteurs : chaque locuteur utilisé existe dans la table de la tâche 0.
- [ ] **Step 3:** Contrôle des 6 points de canon du design §5 et des anti-objectifs ; corriger les écarts.
- [ ] **Step 4:** Reporter dans `09`, `10` et la bible les noms validés par l'utilisateur et ajouter un renvoi vers les scripts ; mettre à jour le README du dossier.
- [ ] **Step 5:** Présenter la liste des écarts corrigés et des points restant à valider.

## Après ce plan (hors périmètre)

Seconde passe EN, conversion des slots en clés de localisation, mapping slot → case pour les zones non conçues, et branchement dans le jeu : plans séparés, une fois le français validé.
