# Narration complète de New York et de Moscou : design

> Statut : approche A validée par l'utilisateur le 2026-11-16 (document de script structuré, un chapitre à la fois, FR d'abord, EN et branchement ensuite). Le reste de ce document est une **proposition à approuver**. Les points marqués **[mon choix]** sont des appels du rédacteur.
> Sources : `02_Lore_Narration/08_Bible_Narrative.md` (prime en cas de contradiction), `09_Quetes_New_York.md`, `10_Quetes_Moscou.md`, `03_Histoire_et_Actes.md`, `04_Agents_The_Dawn.md`, `Code/docs/specs/2026-11-13-moscou-agent-m-design.md` (annexes A et B).

## 1. But et pourquoi

Les 8 chapitres de New York et de Moscou n'ont aujourd'hui qu'un squelette (lieu, enjeu, scène clé). Seuls Brooklyn (jouable) et Sergueiv (répliques en annexe du design Agent M) sont écrits. L'utilisateur veut **toute la narration** : trame, événements et dialogues de chaque zone, utilisable d'abord comme **référence d'écriture** à relire, puis **branchable dans le jeu** sans réécriture.

## 2. Périmètre

**Inclus**
- New York : Brooklyn (reprise de l'existant, relu et complété), Queens, Harlem, Manhattan : 16 zones.
- Moscou : Sergueiv (reprise de l'annexe A, complétée), Kapotnya, Sadovoye Koltso, Place Rouge : 16 zones.
- Pour chaque zone : trame, événements dans l'ordre, dialogues avant/après combat, scène de sortie, fragment de souvenir, textes d'archives et de fil du QG (versions courtes).
- Variantes de parcours : Agent D / Agent M (phrases d'ouverture, briefings, scènes personnalisées : Harlem pour D, cathédrale pour M).
- Épilogues des deux choix marquants (Harlem : clinique ou marché ; Place Rouge : arrêter ou écouter le Conseiller).
- Dialogues en **français**. Les clés EN sont une seconde passe, après validation du FR.

**Hors périmètre**
- Paris et Salem (ils fournissent seulement les points d'accroche : preuves, pistes).
- Voix, cinématiques, illustrations.
- Conception des plateaux, objectifs précis, ennemis et équilibrage (lots de level design séparés).
- Tout changement de code dans cette étape : le branchement est une étape suivante.

**Anti-objectifs** : des dialogues interchangeables d'une ville à l'autre ; une collusion AMIF/gangs révélée trop tôt (progression : soupçon, preuve, coût humain, face-à-face) ; un AMIF à l'écran à Moscou avant la Place Rouge (règle du design Agent M) ; des choix qui modifient la fin (ils ne changent que l'épilogue).

## 3. Format : fichiers et unité d'écriture

Deux fichiers : `02_Lore_Narration/13_Script_New_York.md` et `14_Script_Moscou.md`. Ils complètent `09` et `10` (trame et thèse restent là-bas) ; en cas de conflit sur un dialogue, le script prime.

Chaque **zone** suit le même gabarit :

1. **Fiche** : id (`zone_ny_2_1`), lieu, enjeu, ton, rôle dans la chaîne de preuves.
2. **Trame** : 3 à 6 événements dans l'ordre de jeu, en prose.
3. **Scènes** : tableau de répliques `clé | locuteur | FR`.
4. **Variantes** : ce qui change pour D ou pour M (phrase d'ouverture, briefing, réplique de reconnaissance).
5. **Fragment** : id, titre, texte (même ton que l'annexe B de Moscou).
6. **QG** : récit d'archive (2-3 phrases) et fil du QG (1 phrase).

**Clés par emplacement narratif, pas par numéro de case** **[mon choix]** : `narration.<zone>.<slot>.<i>` avec des slots stables : `intro` (case d'entrée ; `mid` fusionné le 2026-11-16), `elite.before`, `elite.after`, `exit`, et pour les boss `boss.before`, `boss.after`, `chapter_end`. Les zones jouables de Brooklyn et Sergueiv ont des numéros de cases (`n14`, `n18`) : un tableau de correspondance slot → case est ajouté à ces deux chapitres. Pour les zones non encore conçues, la correspondance est faite au lot de level design, sans toucher aux textes. Pourquoi : les plateaux de Queens et suivants n'existent pas encore ; des numéros de cases inventés seraient faux.

**Locuteurs** : un nom de locuteur par personnage récurrent (`agent_d`, `agent_m`, `rayner`, `stroke`, etc.), avec sa clé `speaker.*` à créer au branchement. Les agents de soutien sont pris dans les fiches de New York et de Moscou de `04_Agents_The_Dawn.md`.

**Volume cible** **[mon choix]** : par zone, environ 8 à 14 répliques (zone normale) et 15 à 25 (zone de boss). Soit environ 400 répliques pour les 32 zones. Brooklyn et Sergueiv sont retouchés, pas réécrits.

## 4. Noms propres (validés par l'utilisateur le 2026-11-16)

Tous étaient marqués « à valider » dans le lore ; validés tels que proposés :

| Rôle | Nom proposé | Note |
|---|---|---|
| Le Régisseur (boss Queens) | Elias Voss, dit « le Régisseur » | Intendant du Syndicat, parle de « contrats » |
| Chef du gang de Harlem | Dante Reyes, dit « Braise » | A tué l'ami d'enfance d'Agent D |
| Ami d'enfance d'Agent D | Théo | Mort avant l'histoire, évoqué à Harlem |
| Le Passeur | Garde le titre seul | Ne dit jamais son nom |
| Le Contremaître | Fiodor Gromov | Chef de quai de Kapotnya, paie par Orlan |
| Dmitri Orlan | Déjà nommé | Oligarque, « quelqu'un au-dessus » |
| Le Conseiller | Arkadi Zorin | Faction interne ; orchestre aussi l'attentat de la cathédrale |
| Frère d'Agent M | Ilia | Blessé à la cathédrale |
| Le médecin de la clinique de Harlem | Dr Amina Colby | « Ils ne soignent qu'ici » |

Les noms civils d'Agent D (Dorian Vale) et d'Agent M (Mara Sable) sont déjà dans les fiches ; les répliques utilisent surtout « Agent D » / « Agent M » et les noms de codes.

## 5. Décisions de canon à confirmer (proposées, une par ligne)

- **Cathédrale de Moscou** : l'attentat est orchestré par la faction interne (Conseiller) pour « éveiller » un Apex utile ; révélé au chapitre 4 pour Agent M, sous forme d'indices dès le chapitre 1.
- **Harlem** : le choix (clinique ou marché) ne change que l'épilogue ; la zone non défendue subit une perte visible (un lieu fermé, un personnage blessé), pas une mort.
- **Place Rouge** : le choix (arrêter ou écouter le Conseiller) ne change que l'épilogue ; le Conseiller reste une menace dans les deux cas, et les salles soviétiques mènent à Salem dans les deux cas.
- **AMIF à Moscou** : jamais à l'écran avant la Place Rouge ; avant, il n'existe que par ses clients (le Passeur, le Contremaître, Orlan) et ses dossiers.
- **Stroke** : à New York seulement aux chapitres 1 (par son Titan) et 4, comme dans la bible.
- **Chaîne de preuves** : les preuves n°1 (Queens 2.4) et n°2 (Sergueiv 1.2 et Kapotnya 2.3) sont montrées à l'écran et nommées de la même façon dans les dialogues et les archives.

## 6. Approches envisagées

- **A (validée)** : un script structuré par ville, un chapitre à la fois, FR d'abord. Un seul texte, relisible tel quel, converti ensuite en clés de localisation. Contre : plus lourd qu'un récit libre ; les slots sont mappés aux cases plus tard.
- B (écartée) : récit en prose puis réécriture en clés. Risque de dérive entre texte validé et texte intégré.
- C (écartée) : écrire directement dans les fichiers de localisation. Illisible pour la relecture, touche le code avant la validation du lore.

## 7. Livraison et ordre

Chaque chapitre est livré, relu et validé avant le suivant. Ordre validé :
1. Moscou 1 Sergueiv (reprise de l'annexe A, complétée).
2. New York 2 Queens, 3 Harlem, 4 Manhattan, puis Brooklyn relu.
3. Moscou 2 Kapotnya, 3 Sadovoye Koltso, 4 Place Rouge.

À la fin : relecture de cohérence transversale (preuves, accroches Paris/Salem, variantes D/M), puis seconde passe EN et branchement dans un plan à part.

## 8. Reste au rédacteur

- Le détail des échanges et des gags de ton (hors canon).
- Les noms secondaires de PNJ de zone (commerçants, indics, ouvriers).
- Le choix des agents de soutien par zone parmi les fiches (je propose, tu valides par chapitre).
- Les textes courts d'archives et de QG.

## 9. Risques

1. **Dérive de ton** entre 32 zones : atténuation, relecture transversale en fin de lot et ton fixé par ville dans le gabarit (New York : visibilité et ironie ; Moscou : politesse et froideur).
2. **Contradiction avec le code existant** (Brooklyn, Sergueiv) : atténuation, ces deux chapitres sont relus contre le code avant d'être figés.
3. **Volume** : environ 400 répliques ; livraison par chapitre pour que les corrections restent locales.
