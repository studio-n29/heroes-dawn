# Script de New York : « la ville vitrine »

*Date : 2026-11-16 · Statut : **chapitres 2 (Queens), 3 (Harlem) et 4 (Manhattan) écrits, à relire** ; chapitre 1 (Brooklyn) relu contre le code, complété. **Décision utilisateur du 2026-11-16 : le script fait foi** ; le code existant de Queens, Harlem et Manhattan (créé en parallèle) devra être aligné sur ce script. Design : `Code/docs/specs/2026-11-16-narration-new-york-moscou-design.md`. Trame et thèse : `09_Quetes_New_York.md`. Bible : `08_Bible_Narrative.md`. Format de zone et clés : voir le mode d'emploi de `14_Script_Moscou.md` (identique).*

## Table des locuteurs

| Id | Nom affiché | Rôle |
|---|---|---|
| `agent_d` | Agent D | Personnage joueur (route New York) |
| `agent_m` | Agent M | Personnage joueur (route Moscou) ; à New York à l'acte III |
| `ilona_varga` | Ilona Varga | Équipe de départ |
| `giulia_rossi` | Giulia Rossi | Équipe de départ, New York |
| `lucas_mendes` | Lucas Mendes | Équipe de départ, New York |
| `rayner` | Brie Rayner | Fondatrice de The Dawn |
| `stroke` | Général Stroke | Dirige l'AMIF ; chapitres 1 (Titan) et 4 |
| `crocodile` | Crocodile | Boss de Coney Island (chapitre 1) |
| `regisseur` | Elias Voss, le Régisseur | Boss de Queens |
| `indic` | Un indic | Astoria |
| `braise` | Dante Reyes, « Braise » | Chef du gang de Harlem, boss du chapitre 3 |
| `dr_colby` | Dr Amina Colby | Médecin de la clinique de Harlem |
| `commercant` | Un commerçant | Marché de Flushing |
| `captif` | Un captif | Apex libéré du convoi de Jamaica |
| `agent_amif` | Un agent de l'AMIF | Garde d'élite de Manhattan (chapitre 4 seulement) |
| `voix` | Voix de propagande | Écrans de Times Square |
| `habitant` | Un habitant | Harlem ; tient le rôle de Théo pour Agent M |
| `theo` | Théo | Ami d'enfance d'Agent D, mort (évoqué) |
| `darius_brooks` | Darius Brooks (« Anchor ») | Voix de l'équipe à Brooklyn (code existant) |
| `brick` | Brick | Boss de Red Hook (chapitre 1) |
| `lieutenant` | Lieutenant de Crocodile | Élite de Brownsville |
| `general` | Le Général | Stroke, en fin de chapitre 1 (code existant) |
| `enforcer` | Homme de main | Garde ou escorte d'élite |
| `narrateur` | — | Texte sans locuteur |

*Noms validés par l'utilisateur le 2026-11-16 : Elias Voss, Dante Reyes « Braise », Théo, Dr Amina Colby.*

---

## Chapitre 1 : Brooklyn

Id : `ny_brooklyn`. **Soupçon.** Acte I pour Agent D, acte III pour Agent M. **Chapitre déjà jouable** : les répliques ci-dessous sont celles du code (`LocEpic.cs`, qui remplace `LocNarration.cs` au chargement), reprises **mot pour mot** avec leurs clés actuelles. Les ajouts sont marqués « ajout proposé ».

**Texte d'ouverture (existant)** : *Red Hook, Gowanus, Dumbo : Brooklyn est le premier front. L'AMIF et les gangs, qui prétendent se combattre, se partagent la ville en secret. L'Aube y fait ses preuves, mais chaque succès attire un peu plus l'attention de l'AMIF.*

**Ton de Brooklyn** : épique, lyrique, le plus « saga » de New York (ton du texte existant). Premier front : l'Aube prouve ce qu'elle vaut et l'AMIF la remarque.

**Locuteurs du code** : `brick`, `darius_brooks` (voix de l'équipe, « Anchor »), `lieutenant` (lieutenant de Crocodile), `crocodile`, `stroke`, `general` (le Général), `rayner`. Ils s'ajoutent à la table ci-dessus.

### Rulings de cohérence (script faisant foi)

- **Sens du trafic.** Brooklyn (code) : l'AMIF enchaîne des Apex à Red Hook puis les vend à Crocodile. Queens (script) : le Syndicat remet des captifs à l'AMIF. **Décision** : l'AMIF est un courtier. Elle reçoit des captifs (Syndicat, Queens) et en revend à des gangs (Crocodile, Brooklyn). Brooklyn donne le soupçon ; Queens donne la preuve par le registre. Aucune réplique existante n'est modifiée ; une réplique de Queens a été précisée en conséquence (2.4, `boss.after.1`).
- **`speaker.dockmaster` = « Contremaître »** (code, Red Hook) : ne pas confondre avec le Contremaître de Moscou (Fiodor Gromov). Le nom affiché de `dockmaster` devrait devenir « Chef de quai » au branchement.

### Variantes d'ouverture du chapitre

| Parcours | Phrase ajoutée à l'ouverture |
|---|---|
| Agent D (acte I) | *Il revient sur les quais où l'on vend ce qu'on ne voit pas. Brooklyn est le premier endroit où il se montre.* |
| Agent M (acte III) | *Elle a vu à Moscou comment une agence peut se payer un gang. Brooklyn lui montre comment on s'en débarrasse.* |

### Correspondance slots → cases (code actuel)

| Zone | Slot | Case |
|---|---|---|
| `zone_ny_1_1` | `boss.before/after` (Brick) | n23 |
| `zone_ny_1_2` | `elite.before/after` (lieutenant) ; `exit` | n14 ; n19 |
| `zone_ny_1_3` | `boss.before/after` (Crocodile) | n18 |
| `zone_ny_1_4` | `boss.before/after` (Titan, Stroke, Rayner) | n18 |

---

#### `zone_ny_1_1` : Red Hook, les quais

**Fiche.** Enjeu : infiltrer le quai, couper le courant, trouver les documents de transport, arrêter l'échange de Brick, fuir. Ton : nuit sans lune, rouille, sel. Preuve : documents de transport (premières pièces du soupçon).

**Trame.** (1) L'Aube infiltre le port pendant la nuit. (2) Mini-jeu du générateur : le courant est coupé. (3) L'équipe trouve des documents de transport. (4) Elle arrête l'échange mené par Brick. (5) Elle fuit avant le retour de l'AMIF.

**Scènes (code, `LocEpic.cs`).**

| Clé | Locuteur | FR |
|---|---|---|
| `narration.ny_1_1.n23.before.0` | brick | « Vous avez traversé mon royaume de rouille et de sel pour mourir sur ce quai. Admirable. Inutile. » |
| `narration.ny_1_1.n23.before.1` | darius_brooks | Anchor, tu es le mur. Frost, Echo : couvrez-le. Cette nuit nous appartient, ou personne ne la verra finir. |
| `narration.ny_1_1.n23.after.0` | darius_brooks | Brick s'écroule parmi les conteneurs, et le vacarme du port se tait. Les cages sont ouvertes, les registres parlent : la vérité est entre vos mains. |
| `narration.ny_1_1.n23.after.1` | darius_brooks | Partez avant l'aube. L'AMIF reviendra, et quand elle reviendra, elle trouvera des héros. |

*Ajouts proposés (pas encore dans le code) :* `narration.ny_1_1.intro.0` — narrateur : « Red Hook dort sous les grues. Au bout des quais, des cages attendent leur cargaison. » ; `narration.ny_1_1.intro.0` — darius_brooks : « Les documents sont là. Des numéros de lots, des horaires, un sigle d'agence. Ce n'est pas un trafic de rue : c'est une procédure. »

**Fragment (proposé).** `ny_redhook_docks` — **Des cages sous les grues** : *Il y avait des cages sur le quai, et des numéros peints dessus. Tu as compris que ceux qu'on enchaînait n'avaient pas de nom pour ceux qui les vendaient. Cette nuit-là, l'Aube est devenue un nom.*

**QG.** Existant dans le code (`qg.fil.zone_ny_1_1`, `archives.zone.zone_ny_1_1`).

---

#### `zone_ny_1_2` : Brownsville, les otages

**Fiche.** Enjeu : s'introduire, neutraliser le lieutenant de Crocodile, libérer les otages, les mettre à l'abri. Ton : cellules, couloirs. Le nom de Crocodile apparaît derrière les cellules.

**Scènes (code).**

| Clé | Locuteur | FR |
|---|---|---|
| `narration.ny_1_2.n14.before.0` | lieutenant | « Crocodile m'a confié les clés de l'enfer. Entrez : il vous garde une place. » |
| `narration.ny_1_2.n14.before.1` | darius_brooks | Derrière lui, des innocents. Devant nous, une porte. On ne s'arrête pas, pas ce soir. |
| `narration.ny_1_2.n14.after.0` | lieutenant | « Vous croyez avoir gagné ? Vous venez d'allumer la mèche. Crocodile fera pleuvoir le feu sur vos têtes. » |
| `narration.ny_1_2.n19.after.0` | darius_brooks | Les chaînes tombent. Les otages respirent l'air libre et, pour la première fois depuis longtemps, lèvent les yeux vers un ciel qui ne leur ment pas. |
| `narration.ny_1_2.n19.after.1` | darius_brooks | Mais les ombres s'allongent. Crocodile n'oublie jamais, et ce qu'il jure, il l'écrit dans le sang. |

*Ajout proposé :* `narration.ny_1_2.intro.0` — narrateur : « Brownsville se ferme à l'approche de l'Aube. Derrière les grilles, quelqu'un a peur de ce qui va arriver. »

**Fragment (proposé).** `ny_brownsville_cells` — **Les cellules de Crocodile** : *Derrière chaque porte, un nom que personne ne prononçait. Tu as ouvert les cellules une à une, et chaque fois, quelqu'un levait les yeux comme si la lumière était une promesse.*

**QG.** Existant dans le code.

---

#### `zone_ny_1_3` : Coney Island, le piège

**Fiche.** Enjeu : évacuer les civils, briser le piège, vaincre Crocodile. Ton : parc d'attractions éteint, manèges, cendres. Crocodile (Antonio Kribelchi) est mafieux méta-humain.

**Scènes (code).**

| Clé | Locuteur | FR |
|---|---|---|
| `narration.ny_1_3.n18.before.0` | crocodile | « Toute l'Aube sur mon île… Je n'aurais pas osé rêver pareil cadeau. Soyez les bienvenus à vos propres funérailles. » |
| `narration.ny_1_3.n18.before.1` | darius_brooks | Nous savions que c'était un piège. Qu'importe : quand le monde appelle, on répond. Les civils d'abord, puis lui. |
| `narration.ny_1_3.n18.after.0` | crocodile | « Impossible… Je suis… un roi… » |
| `narration.ny_1_3.n18.after.1` | darius_brooks | Crocodile s'effondre, et les manèges éteints de Coney Island se taisent. La victoire a un goût de cendre : certaines blessures ne guérissent qu'en légende. |

*Ajout proposé :* `narration.ny_1_3.intro.0` — narrateur : « Coney Island s'allume pour la dernière fois. Les manèges tournent à vide, et quelqu'un rit dans les haut-parleurs. »

**Fragment (proposé).** `ny_coney_trap` — **Un roi sur une île** : *Il t'attendait au bout du piège, et tu y es allé quand même. Les civils sont sortis avant le dernier éclat de rire. Certaines victoires sentent la cendre.*

**QG.** Existant dans le code.

---

#### `zone_ny_1_4` : Dumbo et le pont de Brooklyn

**Fiche.** Enjeu : percer le cordon de l'AMIF, couper la surveillance, tenir le pont face au Titan, puis scène de Rayner. Ton : acier, drapeaux, face-à-face. **Stroke apparaît ici par son Titan** (selon la bible) ; la scène finale oppose Rayner à Stroke.

**Scènes (code).**

| Clé | Locuteur | FR |
|---|---|---|
| `narration.ny_1_4.n18.before.0` | stroke | « Vous êtes cernés. Déposez vos pouvoirs aux pieds de l'Histoire, et peut-être l'Histoire vous épargnera-t-elle. » |
| `narration.ny_1_4.n18.before.1` | darius_brooks | Tenez la ligne. Un pont n'est qu'un serment de pierre : aujourd'hui, c'est nous qui le tenons. |
| `narration.ny_1_4.n18.after.0` | rayner | « Assez ! Stroke, j'ai de quoi mettre la ville entière face à vos petits secrets. Faut-il les dire ici, devant vos hommes ? » |
| `narration.ny_1_4.n18.after.1` | stroke | « … Repli. Tout le monde. » |
| `narration.ny_1_4.n18.after.2` | general | « Crocodile portera le crime. Une ombre de plus au tableau : il fera un coupable admirable. » |
| `narration.ny_1_4.n18.after.3` | general | « Souvenez-vous de ce jour, Rayner. Quand les Accords de Salem seront signés, l'Aube devra répondre de tout. » |

*Ajout proposé :* `narration.ny_1_4.chapter_end.0` — narrateur : « Brooklyn tient. Mais l'Aube sait désormais que la ville n'est qu'un jeu entre agences et gangs, et que les Accords de Salem sont son prochain enjeu. »

**Variante Agent M (acte III) proposée.** `narration.ny_1_4.n18.after.4` — agent_m : « Stroke recule devant un secret. Moscou m'a appris que les gens comme lui ne reculent que pour mieux revenir. »

**Fragment (proposé).** `ny_dumbo_bridge` — **Le pont de pierre** : *Un pont n'est qu'un serment de pierre. Tu l'as tenu jusqu'à ce que quelqu'un d'autre prononce le nom que tu n'osais pas dire. Stroke a reculé, et tu as compris que la guerre ne faisait que commencer.*

**QG.** Existant dans le code.

## Chapitre 2 : Queens

Id : `ny_queens`. **Preuve de la collusion.** Acte I pour Agent D, acte III pour Agent M.

**Texte d'ouverture (existant)** : *Flushing, Astoria, Jamaica : dans le Queens, le Syndicat fait régner la peur sur les marchés et les gares de triage. Des captifs, des indics, des caméras : l'Aube découvre que le Syndicat n'agit pas seul.*

**Ton de New York** : visibilité et ironie. Tout le monde regarde, personne ne dit rien. Les caméras sont partout, les blagues servent à tenir. L'AMIF est **présente par ses sigles, ses listes et son registre**, pas par ses agents : Stroke est absent de ce chapitre.

**Variantes par parcours** : voir le mode d'emploi de `14_Script_Moscou.md` (suffixes `.d` et `.m` pour les variantes qui remplacent une réplique de base).

**Rappel de canon** : preuve n°1 = *le registre de transport* (Queens 2.4). Elle démontre que le Syndicat livre des Apex captifs à l'AMIF, qui lui fournit la surveillance.

### Variantes d'ouverture du chapitre

| Parcours | Phrase ajoutée à l'ouverture |
|---|---|
| Agent D (acte I) | *Il connaît le bruit de ces rues. Il découvre qui les écoute.* |
| Agent M (acte III) | *Elle a déjà vu ce schéma à Moscou : un gang, une agence, un contrat.* |

### Correspondance slots → cases

| Zone | Graphe | Entrée (`intro.*`) | Élite (`elite.*`) | Fin |
|---|---|---|---|---|
| `zone_ny_2_1` | C | n04 (`intro`) | n14 | n22 (`exit`) |
| `zone_ny_2_2` | A | n03 (`intro`) | n14 | n19 (`exit`) |
| `zone_ny_2_3` | D | n03 (`intro`) | n13 | n21 (`exit`) |
| `zone_ny_2_4` | B | n04 (`intro`) | n14 | n18 (`boss.*`, `chapter_end`) |

*Règle (plateaux du code, session « Plateaux de jeux New York et Moscou », 2026-11-16) : une narration est attachée à trois cases par zone : la **case d'entrée** (`intro`, ajoutée sur décision de l'utilisateur du 2026-11-16 : `n04` dans les graphes B et C, `n03` dans les graphes A et D), l'élite fixe (`elite.before/after`) et la dernière case (`exit`, ou `boss.*` en 2.4, 3.4 et 4.4). L'ancien slot `mid` est fusionné dans `intro`.*

---

#### `zone_ny_2_1` : Flushing, marché sous caméras

**Fiche.** Enjeu : repérer les caméras du Syndicat et les couper sans alerter les commerçants. Ton : marché bondé, enseignes lumineuses, regards en coin. Scène clé : les caméras portent le sigle d'un prestataire de l'AMIF. Preuve : aucune encore, seulement un sigle.

**Plateau (code).** Élite : Collecteur (syn_collector). Objectifs, dans l'ordre : n04 (atteindre) : Entrer dans le marché ; n13 (mini-jeu) : Couper les caméras en silence ; n14 (combat) : Vaincre le Collecteur ; n22 (atteindre) : Relever le sigle des caméras.

**Trame.**
1. L'Aube traverse le marché de Flushing, où chaque étal a une caméra au-dessus de la caisse. Les commerçants font comme si elles n'existaient pas.
2. Repérage des caméras : l'équipe les compte, les cartographie, en évitant d'effrayer les clients.
3. Une commerçante prévient en chuchotant : « Elles nous voient, elles ne nous aident jamais. »
4. L'escorte du Syndicat (élite) arrive : « On ne coupe pas ce qui protège le quartier. »
5. Un boîtier ouvert montre un sigle de prestataire de l'AMIF.

**Scènes.**

| Clé | Locuteur | FR |
|---|---|---|
| `narration.ny_2_1.intro.0` | narrateur | Flushing grouille. Chaque étal a sa caméra au-dessus de la caisse, et personne ne lève les yeux. |
| `narration.ny_2_1.intro.1` | giulia_rossi | Dix caméras sur un marché d'oignons. Le Syndicat ne rate pas une transaction. |
| `narration.ny_2_1.intro.2` | lucas_mendes | Ou quelqu'un lui prête le matériel. |
| `narration.ny_2_1.intro.3` | commercant | Elles tournent depuis l'été. Elles nous voient. Elles ne nous aident jamais. |
| `narration.ny_2_1.intro.4` | agent_d | Si on les coupe d'un coup, tout le marché le saura. On y va étal par étal. |
| `narration.ny_2_1.intro.5` | commercant | Vous êtes l'Aube. Faites vite, avant que quelqu'un le raconte. |
| `narration.ny_2_1.elite.before.0` | enforcer | On ne coupe pas ce qui protège le quartier. Surtout pas pour des visiteurs. |
| `narration.ny_2_1.elite.before.1` | ilona_varga | « Protéger » : tu es sûr que c'est le mot qu'ils t'ont donné ? |
| `narration.ny_2_1.elite.after.0` | lucas_mendes | Regardez le boîtier. Ce n'est pas un modèle de rue. Il y a un sigle gravé dessous. |
| `narration.ny_2_1.elite.after.1` | giulia_rossi | Un prestataire de l'AMIF. Les caméras du Syndicat sont louées à l'agence. |
| `narration.ny_2_1.exit.0` | ilona_varga | Un gang qui loue ses yeux à une agence d'État. On remonte la piste jusqu'à Astoria. |
| `narration.ny_2_1.exit.1` | agent_d | Ou on suit les caméras. Elles vont toutes dans la même direction. |

**Variantes.**
- **Agent D** : `intro.4` comme écrit (il connaît le quartier et les commerçants). **Agent M** : `narration.ny_2_1.intro.4.m` — `agent_m` : « Je vois ce que chaque caméra a filmé hier. Coupons-les dans l'ordre : celles qui regardent la caisse en dernier. »
- **Agent M, reconnaissance (acte III)** : `narration.ny_2_1.exit.2` — agent_m : « À Moscou, c'étaient des hologrammes. Ici, des caméras. Le même sourire poli derrière. »

**Fragment.** `ny_flushing_cameras` — **Des yeux qui louent** : *Dix caméras sur un marché d'oignons, et personne ne levait les yeux. Elles voyaient chaque pièce, chaque visage, chaque Apex qui hésitait devant un étal. Sous l'une d'elles, un sigle d'agence : ce n'est pas le Syndicat qui t'observe, c'est celui qui lui loue la vue.*

**QG.** Archive : *À Flushing, les caméras du marché appartiennent au Syndicat, mais leurs boîtiers portent le sigle d'un prestataire de l'AMIF.* Fil : *Le Syndicat loue ses yeux à l'AMIF. Direction Astoria.*

---

#### `zone_ny_2_2` : Astoria, les indics

**Fiche.** Enjeu : retourner un indic sans le mettre en danger. Ton : ruelles, cafés fermés à moitié, voix basses. Scène clé : l'indic avoue que le Syndicat remet des listes d'Apex à l'AMIF.

**Plateau (code).** Élite : Collecteur (syn_collector). Objectifs, dans l'ordre : n03 (atteindre) : Trouver le point de rendez-vous ; n13 (mini-jeu) : Rattraper l'indic ; n14 (combat) : Briser l'escorte de l'indic ; n19 (atteindre) : Mettre l'indic à l'abri.

**Trame.**
1. À Astoria, les indics du Syndicat se retrouvent dans un café de nuit. L'Aube observe sans entrer.
2. Agent D ou l'équipe repère un indic qui a peur : il vend des noms pour que sa famille ne soit pas touchée.
3. L'élite du Syndicat surveille la salle ; il faut la neutraliser sans mettre l'indic en vue.
4. L'indic parle : chaque semaine, le Syndicat remet une liste d'Apex à un contact de l'AMIF.
5. Il demande à être caché ; l'Aube lui assure un refuge.

**Scènes.**

| Clé | Locuteur | FR |
|---|---|---|
| `narration.ny_2_2.intro.0` | narrateur | Astoria, une heure du matin. Le café est à moitié fermé, la salle à moitié pleine. Tous parlent à voix basse. |
| `narration.ny_2_2.intro.1` | lucas_mendes | Celui du fond n'a pas touché son café. Il regarde la porte toutes les trente secondes. |
| `narration.ny_2_2.intro.2` | indic | Je ne suis pas une balance. Je suis un type qui a une fille. |
| `narration.ny_2_2.intro.3` | agent_d | On ne vous demande pas de trahir. On vous demande de dire ce qu'on ne peut plus ignorer. |
| `narration.ny_2_2.intro.4` | ilona_varga | Il ne ment pas. Il a peur pour quelqu'un, pas pour lui. |
| `narration.ny_2_2.elite.before.0` | enforcer | Tout le monde reste assis. Il y a des visiteurs qui n'ont pas été invités. |
| `narration.ny_2_2.elite.after.0` | giulia_rossi | Dehors, vite. Avant que les autres comprennent. |
| `narration.ny_2_2.exit.0` | indic | Chaque jeudi, le Syndicat apporte une liste. Des noms, des quartiers, des pouvoirs. Un type en costume la prend, sans jamais donner son nom. |
| `narration.ny_2_2.exit.1` | giulia_rossi | Un costume de l'AMIF ? |
| `narration.ny_2_2.exit.2` | indic | Un costume qui ne se salit jamais. Les nouveaux Apex apparaissent sur la liste avant même d'avoir peur. |
| `narration.ny_2_2.exit.3` | ilona_varga | On t'emmène dans un refuge. Ta fille aussi. |

**Variantes.**
- **Agent D** : `intro.3` comme écrit. **Agent M** : `narration.ny_2_2.intro.3.m` — `agent_m` : « Je vois ce qui t'est arrivé ce matin. On ne te demande pas de parler : on te demande de nous laisser écouter. » *(sa vision remplace un dialogue)*.
- **Agent M, reconnaissance** : `narration.ny_2_2.exit.4` — agent_m : « À Moscou, on parle de clients. Ici, de listes. C'est le même mot. »

**Fragment.** `ny_astoria_informant` — **La liste du jeudi** : *Il n'était pas une balance, il était un père. Chaque jeudi, une liste changeait de mains : des noms, des quartiers, des pouvoirs encore endormis. Quelqu'un dans un costume impeccable la rangeait sans lever les yeux.*

**QG.** Archive : *À Astoria, un indic du Syndicat avoue que les listes d'Apex sont remises chaque semaine à un contact de l'AMIF.* Fil : *Les listes du jeudi mènent à Jamaica.*

---

#### `zone_ny_2_3` : Triage de Jamaica, convoi

**Fiche.** Enjeu : intercepter un convoi de captifs. Ton : gare de triage, lumières sodium, acier. Scène clé : les captifs sont des Apex éveillés, destinés à l'AMIF.

**Plateau (code).** Élite : Collecteur (syn_collector). Objectifs, dans l'ordre : n11 (mini-jeu) : Fouiller les wagons ; n13 (combat) : Briser l'escorte du convoi ; n20 (mini-jeu) : Identifier les captifs ; n21 (atteindre) : Libérer le convoi.

**Trame.**
1. Le convoi quitte la gare de triage de Jamaica à l'aube : wagons sans marque, escorte du Syndicat.
2. L'Aube se positionne sur les voies, évite les caméras.
3. L'escorte d'élite protège le dernier wagon.
4. Dans le wagon, des Apex éveillés, drogués et menottés.
5. L'un d'eux dit ce qu'on leur a promis : « une protection ». Elle venait de l'AMIF.

**Scènes.**

| Clé | Locuteur | FR |
|---|---|---|
| `narration.ny_2_3.intro.0` | narrateur | La gare de triage de Jamaica dort sous les lampes orange. Un convoi sans marque attend à la voie 14. |
| `narration.ny_2_3.intro.1` | lucas_mendes | Pas d'étiquette, pas de numéro, et une escorte armée. Ce n'est pas du fret. |
| `narration.ny_2_3.intro.2` | giulia_rossi | Dans les wagons, il y a des gens. Je les entends respirer. |
| `narration.ny_2_3.intro.3` | agent_d | Alors on ne ralentit pas. On ouvre le dernier wagon. |
| `narration.ny_2_3.elite.before.0` | enforcer | Ce convoi est sous contrat. Reculez, ou le contrat vous inclura. |
| `narration.ny_2_3.elite.before.1` | ilona_varga | Il récite. Il ne sait même pas pour qui il travaille. |
| `narration.ny_2_3.elite.after.0` | captif | Vous êtes… de l'Aube ? On nous avait dit qu'on partait vers un endroit sûr. |
| `narration.ny_2_3.elite.after.1` | giulia_rossi | Qui vous l'a dit ? |
| `narration.ny_2_3.elite.after.2` | captif | Un homme avec un badge. Il disait que l'agence se chargerait de nous. |
| `narration.ny_2_3.exit.0` | ilona_varga | Des Apex éveillés, livrés comme des colis. À l'agence. |
| `narration.ny_2_3.exit.1` | agent_d | Le registre de ce convoi sera au terminal de fret. C'est là qu'on le trouvera. |

**Variantes.**
- **Agent D** : `intro.3` comme écrit. **Agent M** : `narration.ny_2_3.intro.3.m` — `agent_m` : « Je vois qui est monté dans ces wagons, et ce qu'on leur a promis. On ne ralentit pas. »
- **Agent M, reconnaissance** : `narration.ny_2_3.exit.2` — agent_m : « À Moscou, on les appelle des trophées. Ici, des captifs. »

**Fragment.** `ny_jamaica_convoy` — **Voie 14** : *Les wagons n'avaient ni nom ni numéro, et pourtant tout y était attendu. Tu as entendu des respirations derrière l'acier, et une voix qui répétait qu'on partait vers un endroit sûr. Elle n'avait jamais eu de raison de se méfier d'un badge.*

**QG.** Archive : *Le convoi de Jamaica transporte des Apex éveillés, drogués et menottés, destinés à l'AMIF. L'Aube les libère.* Fil : *Le registre de ce convoi est au terminal de fret.*

---

#### `zone_ny_2_4` : Terminal de fret

**Fiche.** Enjeu : boss, le Régisseur. Ton : bureau en haut d'une grue, tableaux de bord, contrats. Le Régisseur ne dit ni « gang » ni « crime » : seulement « contrat ». Scène clé : le registre de transport prouve la collusion — **preuve n°1**.

**Plateau (code).** Élite : Collecteur (syn_collector). Boss : LE RÉGISSEUR. Objectifs, dans l'ordre : n04 (atteindre) : Entrer dans le terminal ; n11 (mini-jeu) : Ouvrir les cages ; n14 (combat) : Briser la garde du quai ; n18 (combat) : Vaincre le Régisseur.

**Trame.**
1. Le terminal de fret est le centre logistique du Syndicat : conteneurs, grues, une salle de contrôle vitrée.
2. L'Aube fouille les bureaux et récupère des extraits du registre.
3. L'élite du terminal (gardes) tombe.
4. Le Régisseur reçoit l'Aube comme un client en retard.
5. Combat : phases « Le contrat est signé » (sous 66 %) et « Résiliation » (sous 33 %).
6. Après sa chute, le registre complet est saisi. Le Régisseur lâche une dernière phrase avant d'être emmené.

**Scènes.**

| Clé | Locuteur | FR |
|---|---|---|
| `narration.ny_2_4.intro.0` | narrateur | Le terminal de fret domine la baie. Au sommet d'une grue, une salle vitrée surveille chaque conteneur. |
| `narration.ny_2_4.intro.1` | lucas_mendes | De là-haut, on voit tout. C'est exactement ce que veut un gestionnaire. |
| `narration.ny_2_4.intro.2` | giulia_rossi | Des livraisons, des horaires, des numéros… et à côté, le même sigle que sur les caméras. |
| `narration.ny_2_4.intro.3` | agent_d | On ne prend pas des extraits. On prend le registre entier. |
| `narration.ny_2_4.elite.before.0` | enforcer | La direction ne reçoit pas sans rendez-vous. |
| `narration.ny_2_4.elite.after.0` | ilona_varga | Il n'y a plus que lui là-haut. Et il nous attend. |
| `narration.ny_2_4.boss.before.0` | regisseur | Vous êtes en retard. Le dernier convoi est parti sans vous, et pourtant vous êtes là. Quelle manière de livrer. |
| `narration.ny_2_4.boss.before.1` | regisseur | Je ne dirige pas un gang. Je gère un contrat. Les contrats ne s'arrêtent pas parce que quelqu'un le regrette. |
| `narration.ny_2_4.boss.before.2` | agent_d | Des gens ont été enfermés dans vos wagons. |
| `narration.ny_2_4.boss.before.3` | regisseur | Des gens ont été transportés. Il y a une différence : l'une rapporte et l'autre se plaint. |
| `narration.ny_2_4.boss.phase1.0` | regisseur | Le contrat est signé. Vous n'avez plus qu'à l'exécuter. |
| `narration.ny_2_4.boss.phase2.0` | regisseur | Résiliation. Je n'aime pas perdre un client. |
| `narration.ny_2_4.boss.after.0` | giulia_rossi | Le registre ! Chaque livraison porte le même signataire : l'agence. |
| `narration.ny_2_4.boss.after.1` | lucas_mendes | Le Syndicat remet les captifs. L'AMIF fournit la surveillance, et revend ce qu'elle ne garde pas. Tout est consigné. |
| `narration.ny_2_4.boss.after.2` | ilona_varga | C'est la preuve que Brooklyn nous laissait deviner : ils ne se combattent pas, ils s'associent. |
| `narration.ny_2_4.boss.after.3` | regisseur | Vous croyez abattre un gang. Vous venez de déranger un contrat. |
| `narration.ny_2_4.chapter_end.0` | narrateur | Les captifs sont libérés. Le registre de transport est en sûreté : première preuve de la collusion entre le Syndicat et l'AMIF. |
| `narration.ny_2_4.chapter_end.1` | ilona_varga | Garde-le. Nous en aurons besoin à Salem. |

**Variantes.**
- **Agent D** : `boss.before.2` comme écrit. **Agent M** : `narration.ny_2_4.boss.before.2.m` — `agent_m` : « Je vois ces wagons pleins, et ce que vous touchez pour chacun. »
- **Agent M, reconnaissance** : `narration.ny_2_4.boss.after.4` — agent_m : « Moscou avait les oligarques. New York a un intendant. La logique est identique : les personnes en marchandise. »

**Fragment.** `ny_terminal_register` — **Le registre de transport** : *Tout était consigné : les dates, les wagons, les noms de ceux qu'on avait attrapés. À côté de chaque ligne, un seul signataire, et ce n'était pas le Syndicat. Les captifs n'étaient pas livrés à un gang, mais à une agence.*

**QG.** Archive : *Au terminal de fret, l'Aube saisit le registre de transport : la preuve que le Syndicat livre des Apex captifs à l'AMIF, qui lui fournit la surveillance.* Fil : *Preuve n°1 en main. Cap sur Harlem.*

## Chapitre 3 : Harlem

Id : `ny_harlem`. **Coût humain et choix marquant.** Acte I pour Agent D, acte III pour Agent M.

**Texte d'ouverture (reformulé)** : *À Harlem, le blackout a laissé des cicatrices que les gangs exploitent. Des blocs brûlent, des familles fuient. L'Aube doit choisir qui protéger en premier, et à quel prix.*

**Ton de Harlem** : le plus intime de New York. Moins d'ironie, plus de silences. Pas de culte, pas de prophétie : un gang qui profite du chaos pour vider des immeubles. L'AMIF est absente de ce chapitre ; la collusion se lit seulement dans le nom du Régisseur, que Braise cite comme un ancien client.

**Rappel de canon** : le choix de la zone 3.3 **ne modifie que l'épilogue**. Aucun mort ne résulte du choix : la zone non défendue subit une perte visible (un lieu fermé, un blessé), pas une disparition.

### Variantes d'ouverture du chapitre

| Parcours | Phrase ajoutée à l'ouverture |
|---|---|
| Agent D (acte I) | *C'est son quartier. Il en connaît chaque trottoir, chaque escalier de secours.* |
| Agent M (acte III) | *Ce n'est pas son quartier. Elle apprend ce que coûte une ville qui brûle.* |

### Correspondance slots → cases

| Zone | Graphe | Entrée (`intro.*`) | Élite (`elite.*`) | Fin |
|---|---|---|---|---|
| `zone_ny_3_1` | D | n03 (`intro`) | n13 | n21 (`exit`) |
| `zone_ny_3_2` | B | n04 (`intro`) | n14 | n18 (`exit`) |
| `zone_ny_3_3` | A | n03 (`intro`) | n14 | n19 (`choice` et `branch_*` joués AVANT `exit.*` : le choix doit être fait avant les répliques de sortie) |
| `zone_ny_3_4` | C | n04 (`intro`) | n14 | n22 (`boss.*`, `chapter_end`, épilogues) |

*Règle (plateaux du code, session « Plateaux de jeux New York et Moscou », 2026-11-16) : une narration est attachée à trois cases par zone : la **case d'entrée** (`intro`, ajoutée sur décision de l'utilisateur du 2026-11-16 : `n04` dans les graphes B et C, `n03` dans les graphes A et D), l'élite fixe (`elite.before/after`) et la dernière case (`exit`, ou `boss.*` en 2.4, 3.4 et 4.4). L'ancien slot `mid` est fusionné dans `intro`.*

---

#### `zone_ny_3_1` : Bloc incendié

**Fiche.** Enjeu : évacuer les habitants avant l'effondrement. Ton : fumée, escaliers, voisins qui descendent des enfants. Scène clé : un gang a mis le feu pour chasser les locataires.

**Plateau (code).** Élite : Lieutenant de Braise (hrl_lieutenant). Objectifs, dans l'ordre : n03 (atteindre) : Atteindre l'immeuble en flammes ; n11 (mini-jeu) : Évacuer les familles piégées ; n13 (combat) : Vaincre le lieutenant incendiaire ; n20 (mini-jeu) : Ramener les derniers habitants des toits.

**Trame.**
1. L'Aube arrive devant un immeuble en feu ; les pompiers manquent de courant et de pression.
2. Évacuation étage par étage : Agent D (ou M) trace des passages, l'équipe porte les blessés.
3. Une élite du gang garde le rez-de-chaussée et empêche les habitants de sortir.
4. Après le combat, des bidons d'essence sont trouvés au sous-sol : l'incendie est volontaire.
5. Une vieille locataire dit que des hommes sont passés la veille « proposer un rachat ».

**Scènes.**

| Clé | Locuteur | FR |
|---|---|---|
| `narration.ny_3_1.intro.0` | narrateur | Le blackout a laissé les immeubles de Harlem sans courant, sans pression, sans pompiers. Celui-ci brûle depuis une heure. |
| `narration.ny_3_1.intro.1` | giulia_rossi | Il reste des gens à tous les étages. J'entends des enfants. |
| `narration.ny_3_1.intro.2` | habitant | Mon voisin dit que c'était un court-circuit. Un court-circuit qui sent l'essence ? |
| `narration.ny_3_1.intro.3` | agent_d | On ouvre l'escalier de service. Je vous fais une passerelle. Tenez la rampe et ne regardez pas en bas. |
| `narration.ny_3_1.intro.4` | lucas_mendes | Le plafond du troisième ne tiendra pas. On a quatre minutes. |
| `narration.ny_3_1.elite.before.0` | enforcer | Personne ne sort. Les locataires ont reçu une offre, ils vont l'accepter. |
| `narration.ny_3_1.elite.before.1` | ilona_varga | Il parle d'offre pendant que le bloc brûle. Il n'a même pas honte. |
| `narration.ny_3_1.elite.after.0` | giulia_rossi | Des bidons, dans la cave. Quelqu'un a préparé ça. |
| `narration.ny_3_1.exit.0` | habitant | Des hommes sont venus hier pour nous proposer de racheter l'immeuble. Nous avons dit non. |
| `narration.ny_3_1.exit.1` | ilona_varga | Ils ont changé la réponse eux-mêmes. |

**Variantes.**
- **Agent D** : `intro.3` comme écrit (sa passerelle est un dessin matérialisé). **Agent M** : `narration.ny_3_1.intro.3.m` — `agent_m` : « Je vois quel étage cédera en premier. On évacue dans cet ordre. »
- **Agent D, rappel personnel** : `narration.ny_3_1.exit.2` — agent_d : « Je suis né à trois rues d'ici. J'ai dessiné sur ce mur quand j'avais quinze ans. Il n'en reste plus rien. »

**Fragment.** `ny_harlem_fire` — **Le bloc qui brûlait** : *Les habitants avaient dit non à l'offre de rachat. Dans la nuit, le feu a donné une autre réponse. Tu as vu des familles descendre dans la fumée avec leurs photos sous le bras, comme si l'on pouvait sauver un quartier en emportant ses visages.*

**QG.** Archive : *À Harlem, un bloc incendié volontairement par un gang a été évacué à temps. Les habitants avaient refusé une offre de rachat.* Fil : *Le gang vide les blocs de Harlem. La clinique est la prochaine cible.*

---

#### `zone_ny_3_2` : Clinique de fortune

**Fiche.** Enjeu : tenir la clinique face à une attaque. Ton : néons faibles, groupes électrogènes, draps blancs. Scène clé : le Dr Colby refuse de partir : « Ils ne soignent qu'ici. »

**Plateau (code).** Élite : Lieutenant de Braise (hrl_lieutenant). Objectifs, dans l'ordre : n04 (atteindre) : Entrer dans la clinique ; n11 (mini-jeu) : Rassembler les soins ; n14 (combat) : Repousser l'assaut du gang ; n18 (atteindre) : Tenir la porte jusqu'au bout.

**Trame.**
1. La clinique, installée dans une ancienne église, soigne tout le quartier sans papiers ni questions. Les blessés de l'incendie y sont amenés.
2. Le Dr Colby refuse d'évacuer : si elle part, ses patients n'ont nulle part où aller.
3. Le gang attaque pour vider le bâtiment ; l'élite défonce l'entrée.
4. L'équipe tient la clinique pendant que les infirmiers déplacent les patients.
5. Colby remercie sans ménagement : elle a déjà trop enterré.

**Scènes.**

| Clé | Locuteur | FR |
|---|---|---|
| `narration.ny_3_2.intro.0` | narrateur | La clinique occupe une ancienne église. Un groupe électrogène tousse dans la cour, et une file de blessés attend à la porte. |
| `narration.ny_3_2.intro.1` | giulia_rossi | Il n'y a ni badge ni formulaire. Ils entrent, c'est tout. |
| `narration.ny_3_2.intro.2` | dr_colby | Évacuer ? Les hôpitaux du centre n'admettent personne de ce quartier. Ils ne soignent qu'ici. Si je pars, ils n'ont nulle part. |
| `narration.ny_3_2.intro.3` | ilona_varga | Elle ne cherche pas à être courageuse. Elle a fait ses comptes. |
| `narration.ny_3_2.intro.4` | agent_d | Alors on tient la clinique. Dites-nous seulement quelle porte ne doit jamais tomber. |
| `narration.ny_3_2.elite.before.0` | enforcer | On vide le bâtiment. Ça sera plus simple pour tout le monde. |
| `narration.ny_3_2.elite.before.1` | dr_colby | Plus simple pour qui ? Pas pour les gens couchés derrière moi. |
| `narration.ny_3_2.elite.after.0` | lucas_mendes | L'entrée tient. Les patients sont à l'abri. |
| `narration.ny_3_2.exit.0` | dr_colby | Je ne vais pas vous remercier. J'ai déjà trop remercié des gens qui n'ont pas pu rester. |
| `narration.ny_3_2.exit.1` | dr_colby | Restez, si vous voulez. Le gang n'a pas fini. |

**Variantes.**
- **Agent D** : `intro.4` comme écrit. **Agent M** : `narration.ny_3_2.intro.4.m` — `agent_m` : « Je vois par où ils vont entrer. Dites-moi seulement quelle porte vous voulez garder. »

**Fragment.** `ny_harlem_clinic` — **La clinique de l'église** : *Il n'y avait pas de formulaire, pas de badge, pas de questions. Seulement des draps blancs sous des vitraux fendus. Quand le docteur a dit qu'elle ne partirait pas, tu as compris que certains endroits n'existent que parce que quelqu'un refuse de les quitter.*

**QG.** Archive : *À Harlem, une clinique de fortune installée dans une ancienne église soigne tout le quartier. Le Dr Colby refuse d'évacuer, malgré l'attaque du gang.* Fil : *La clinique tient. Le marché est le prochain point faible.*

---

#### `zone_ny_3_3` : Marché de rue

**Fiche.** Enjeu : choix. Le gang lance deux attaques simultanées, l'une sur la clinique, l'autre sur le marché. Il faut défendre une zone en premier. Ton : étals, musique de radio, voix qui s'inquiètent. **Le choix ne change que l'épilogue.**

**Plateau (code).** Élite : Lieutenant de Braise (hrl_lieutenant). Objectifs, dans l'ordre : n03 (atteindre) : Atteindre le marché ; n13 (mini-jeu) : Mettre les étals à l'abri ; n14 (combat) : Briser les pillards ; n19 (atteindre) : Choisir quoi défendre en premier.

**Trame.**
1. Le marché de rue de Harlem se réveille sous les bâches. Des vendeurs préparent leurs étals ; la clinique est à deux rues.
2. Deux messagers arrivent en même temps : le gang attaque la clinique d'un côté et le marché de l'autre.
3. L'équipe se coupe en deux plans possibles ; Ilona résume la situation, Agent D ou M décide.
4. **Choix** : défendre la clinique d'abord, ou le marché d'abord.
5. Combat et sortie. La zone non défendue subit une perte visible, visible dans l'épilogue.

**Scènes.**

| Clé | Locuteur | FR |
|---|---|---|
| `narration.ny_3_3.intro.0` | narrateur | Les vendeurs du marché montent leurs bâches avant le jour. À deux rues, la clinique s'allume. |
| `narration.ny_3_3.intro.1` | commercant | Ici, tout le monde se connaît. Si le marché tombe, les gens n'ont plus rien à manger. |
| `narration.ny_3_3.intro.2` | lucas_mendes | Deux alertes en même temps. Le gang attaque la clinique et le marché. |
| `narration.ny_3_3.intro.3` | giulia_rossi | On ne peut pas couvrir les deux. Pas assez de monde. |
| `narration.ny_3_3.choice.0` | ilona_varga | C'est ton choix. Quoi que tu décides, l'autre côté va en payer le prix. |
| `narration.ny_3_3.choice.1` | narrateur | Choix : défendre la clinique en premier, ou le marché en premier. |
| `narration.ny_3_3.branch_clinic.0` | agent_d | La clinique d'abord. Des gens y sont couchés qui ne peuvent pas courir. |
| `narration.ny_3_3.branch_clinic.1` | commercant | Le marché… Vous nous laissez ? |
| `narration.ny_3_3.branch_clinic.2` | agent_d | Je reviens. Cachez ce que vous pouvez. |
| `narration.ny_3_3.branch_market.0` | agent_d | Le marché d'abord. Des familles y vivent de ce qu'elles vendent aujourd'hui. |
| `narration.ny_3_3.branch_market.1` | dr_colby | La clinique ? Vous nous laissez ? |
| `narration.ny_3_3.branch_market.2` | agent_d | Tenez la porte. Je reviens. |
| `narration.ny_3_3.exit.0` | giulia_rossi | On a tenu un côté. Pour l'autre, on arrive après. |
| `narration.ny_3_3.exit.1` | ilona_varga | On gagne rarement sans rien perdre. Retiens ce que ça coûte. |

**Variantes.**
- Les répliques `branch_clinic.0` et `branch_market.0` sont dites par le personnage joueur : **Agent D** comme écrit ; **Agent M** : même sens avec « je vois » (`narration.ny_3_3.branch_clinic.0.m` — agent_m : « La clinique d'abord. Je vois les lits, les perfusions : ils ne pourraient pas fuir. » ; `narration.ny_3_3.branch_market.0.m` — agent_m : « Le marché d'abord. Je vois les familles qui y comptent leurs derniers dollars. »).

**Fragment.** `ny_harlem_market` — **Deux alertes** : *Deux alertes au même moment, deux endroits que tu aimais déjà. Tu savais qu'une seule main ne peut pas tenir deux portes. Ce que tu as choisi d'abord, le quartier le saura. Ce que tu as laissé, il s'en souviendra.*

**QG.** Archive : *À Harlem, l'Aube défend la clinique ou le marché en premier, pendant que le gang attaque les deux. Chaque choix laisse une perte visible.* Fil : *Le chef du gang attend dans la rue de l'atelier.*

---

#### `zone_ny_3_4` : Rue de l'atelier

**Fiche.** Enjeu : boss, Dante Reyes, dit « Braise ». Ton : une rue étroite, des ateliers fermés, un trottoir marqué de craie. Scène clé : Agent D retrouve le trottoir où son ami Théo est mort. Pour Agent M, un habitant du quartier tient ce rôle.

**Plateau (code).** Élite : Lieutenant de Braise (hrl_lieutenant). Boss : BRAISE. Objectifs, dans l'ordre : n13 (mini-jeu) : Rattraper les guetteurs sur les toits ; n14 (combat) : Prendre le trottoir ; n21 (mini-jeu) : Forcer l'atelier ; n22 (combat) : Vaincre Braise.

**Trame.**
1. L'Aube entre dans la rue de l'atelier, quartier général du gang ; des tags marquent chaque mur.
2. Le gang tient la rue par des gardes d'élite.
3. Braise attend sur le trottoir. Il parle du blackout comme d'une occasion : « une ville qui brûle se vend mieux ».
4. Combat : phases « Ça brûle » (sous 66 %) et « Tout le bloc » (sous 33 %).
5. Après sa chute, l'épilogue du chapitre (selon le choix de 3.3) montre le prix.

**Scènes.**

| Clé | Locuteur | FR |
|---|---|---|
| `narration.ny_3_4.intro.0` | narrateur | La rue de l'atelier est étroite, les volets fermés. Des tags couvrent les murs : la signature du gang. |
| `narration.ny_3_4.intro.1` | lucas_mendes | Il n'y a personne dans cette rue, mais les fenêtres nous regardent. |
| `narration.ny_3_4.intro.2` | giulia_rossi | Ils nous attendent. Chaque porte est tenue. |
| `narration.ny_3_4.elite.before.0` | enforcer | Braise a dit : personne ne passe. Il a aussi dit que vous essaieriez. |
| `narration.ny_3_4.elite.after.0` | ilona_varga | La rue est ouverte. Il est au bout, sur le trottoir. |
| `narration.ny_3_4.boss.before.0` | braise | Vous voilà. Je vous attendais plus tôt, mais le feu prend toujours son temps. |
| `narration.ny_3_4.boss.before.1` | braise | Le blackout a vidé la ville de ses règles. Moi, j'ai juste ramassé ce qui traînait. |
| `narration.ny_3_4.boss.before.2` | braise | Le Régisseur payait bien, avant que vous lui dérangiez son contrat. Un bloc libre vaut plus qu'un bloc habité. |
| `narration.ny_3_4.boss.before.3` | ilona_varga | Il ne s'excuse pas. Il explique, comme si c'était une évidence. |
| `narration.ny_3_4.boss.phase1.0` | braise | Ça brûle. C'est le mot que j'ai appris. |
| `narration.ny_3_4.boss.phase2.0` | braise | Tout le bloc. Si je tombe, je ne tombe pas seul. |
| `narration.ny_3_4.boss.after.0` | braise | Vous ne comprenez pas. Si ce n'est pas moi, ce sera un autre. |
| `narration.ny_3_4.boss.after.1` | ilona_varga | Peut-être. Mais ce soir, c'est toi qui tombes. |
| `narration.ny_3_4.chapter_end.0` | narrateur | Le chef du gang est tombé. Le quartier respire un peu, mais le prix est visible dans la rue. |

**Variantes (scène personnelle).**
- **Agent D** : au trottoir, `narration.ny_3_4.boss.before.4.d` — agent_d : « C'est ici. C'est ici que Théo est tombé. J'ai tracé la barrière à la craie, juste là. Elle a tenu une seconde. » ; `narration.ny_3_4.boss.after.2.d` — agent_d : « Théo, je ne l'ai pas sauvé. Mais cette rue-là, je l'ai reprise. »
- **Agent M** : `narration.ny_3_4.boss.before.4.m` — habitant : « C'est ici que mon frère a été abattu. Il vendait du pain. Il n'avait jamais rien fait. » ; `narration.ny_3_4.boss.after.2.m` — agent_m : « Je vois ce que ce trottoir a vu. On n'oubliera pas son nom. » *(les deux répliques sont jouées devant le même trottoir, avec un habitant à la place de Théo)*.

**Fragment.** `ny_harlem_chalk` — **Le trottoir de craie** : *Il y avait encore un trait de craie sur le bord du trottoir. Tu as eu l'impression que la barrière tiendrait toujours, juste une seconde de plus. Tu as compris qu'on ne répare pas ce qui est arrivé ici : on fait en sorte que ça n'arrive plus.*

**QG.** Archive : *À Harlem, l'Aube met fin au règne du gang de Braise dans la rue de l'atelier. Le quartier garde les traces du prix payé.* Fil : *Direction Manhattan, où tout converge.*

---

### Épilogues du chapitre (selon le choix de 3.3)

Les épilogues sont joués **après** `chapter_end`. Aucun mort : une perte visible et nommée.

**Si la clinique a été défendue en premier** : le marché a brûlé. Les étals sont perdus ; les familles ont tout à reconstruire.

| Clé | Locuteur | FR |
|---|---|---|
| `narration.ny_3.epilogue.clinic.0` | narrateur | Au matin, la clinique est intacte. Le marché, lui, n'est plus qu'une rangée de bâches noircies. |
| `narration.ny_3.epilogue.clinic.1` | commercant | Nous avons sauvé nos vies. Pas nos étals. Je ne vous le reproche pas, mais je m'en souviendrai. |
| `narration.ny_3.epilogue.clinic.2` | dr_colby | Les gens du marché viennent de se faire soigner chez moi. Ils n'ont jamais eu autant de raisons de nous dire merci, et jamais aussi peu envie de le faire. |

**Si le marché a été défendu en premier** : la clinique a été saccagée. Elle ferme plusieurs semaines ; le Dr Colby est blessée mais vivante.

| Clé | Locuteur | FR |
|---|---|---|
| `narration.ny_3.epilogue.market.0` | narrateur | Au matin, le marché est debout. La clinique, elle, a les vitraux brisés et les portes arrachées. |
| `narration.ny_3.epilogue.market.1` | dr_colby | Je suis blessée, pas vaincue. Mais ma clinique fermera quelques semaines. Les malades iront où ils pourront. |
| `narration.ny_3.epilogue.market.2` | commercant | On a gardé le marché. On a perdu la clinique. Je ne sais pas si c'était la bonne décision. |

**Fin commune** (jouée dans les deux cas) :

| Clé | Locuteur | FR |
|---|---|---|
| `narration.ny_3.epilogue.common.0` | ilona_varga | On n'a pas pu tout sauver. Cela ne fait pas de vous un mauvais héros. Cela fait de vous un héros qui regarde ce qu'il laisse derrière. |
| `narration.ny_3.epilogue.common.1` | giulia_rossi | Manhattan, maintenant. Là où tout se décide. |

## Chapitre 4 : Manhattan

Id : `ny_manhattan`. **Face-à-face.** Acte I pour Agent D, acte III pour Agent M.

**Texte d'ouverture (existant)** : *Midtown, Times Square, le siège de l'ONU : tout converge vers Manhattan. Les cercles de pouvoir, l'AMIF et l'Aube s'y affrontent à visage découvert. C'est ici que la ville bascule.*

**Ton de Manhattan** : le plus public. Tout se dit devant des caméras, des écrans, des délégués. L'AMIF parle enfin à visage découvert : le Général Stroke est calme, administratif, convaincu d'agir pour le bien commun. Brie Rayner réapparaît pour défendre les Apex devant les négociateurs.

**Rappels de canon** : Stroke n'apparaît à l'écran qu'en 4.3 (discours public) et 4.4 (boss) ; dans ce chapitre, il **n'est pas tué** : il se retire en promettant que « Salem décidera ». Les noms des financeurs de l'AMIF et du Syndicat **se recoupent**, mais restent incomplets : la liste est un fil vers Paris et Salem, pas une révélation finale. Les Accords de Salem sont annoncés publiquement ; leur contenu se joue à Salem.

### Variantes d'ouverture du chapitre

| Parcours | Phrase ajoutée à l'ouverture |
|---|---|
| Agent D (acte I) | *Il voit l'AMIF à visage découvert pour la première fois : jusque-là, elle n'avait qu'un Titan et des listes.* |
| Agent M (acte III) | *Elle a vu l'AMIF prélever à Paris et ses clients sourire à Moscou. Ici, elle les voit réunis.* |

### Correspondance slots → cases

| Zone | Graphe | Entrée (`intro.*`) | Élite (`elite.*`) | Fin |
|---|---|---|---|---|
| `zone_ny_4_1` | A | n03 (`intro`) | n14 | n19 (`exit`) |
| `zone_ny_4_2` | D | n03 (`intro`) | n13 | n21 (`exit`) |
| `zone_ny_4_3` | C | n04 (`intro`) | n14 | n22 (`exit`) ; la scène `stage.*` est branchée sur la case n17 (After) |
| `zone_ny_4_4` | B | n04 (`intro`) | n14 | n18 (`boss.*`, `chapter_end`) |

*Règle (plateaux du code, session « Plateaux de jeux New York et Moscou », 2026-11-16) : une narration est attachée à trois cases par zone : la **case d'entrée** (`intro`, ajoutée sur décision de l'utilisateur du 2026-11-16 : `n04` dans les graphes B et C, `n03` dans les graphes A et D), l'élite fixe (`elite.before/after`) et la dernière case (`exit`, ou `boss.*` en 2.4, 3.4 et 4.4). L'ancien slot `mid` est fusionné dans `intro`.*

---

#### `zone_ny_4_1` : Times Square

**Fiche.** Enjeu : reprendre les écrans à la propagande de l'AMIF. Ton : néons, publicités, slogans qui se superposent. Scène clé : la propagande présente l'Aube comme une menace.

**Plateau (code).** Élite : Chef de la sécurité (mnh_chief). Objectifs, dans l'ordre : n03 (atteindre) : Entrer dans Times Square ; n13 (mini-jeu) : Pirater les écrans ; n14 (combat) : Vaincre le chef de la régie ; n19 (atteindre) : Reprendre la diffusion.

**Trame.**
1. Times Square diffuse en boucle un clip de l'AMIF : l'Aube y est montrée en destructrice, avec des images de Brooklyn montées à l'envers.
2. Les passants regardent, hésitent, détournent les yeux.
3. L'équipe atteint la régie technique ; l'élite de sécurité l'en empêche.
4. Au lieu d'une preuve, l'Aube diffuse ce qu'elle a vraiment fait : l'évacuation du bloc de Harlem, un enfant dans les bras.
5. La foule change de regard. Ilona rappelle que la preuve complète doit attendre.

**Scènes.**

| Clé | Locuteur | FR |
|---|---|---|
| `narration.ny_4_1.intro.0` | narrateur | Times Square ne dort jamais. Ce soir, chaque écran répète le même message : l'Aube est une menace. |
| `narration.ny_4_1.intro.1` | voix | « Les méta-humains non encadrés mettent en danger les civils. L'AMIF vous protège. » |
| `narration.ny_4_1.intro.2` | giulia_rossi | Ils ont repris les images de Brooklyn. À l'envers. |
| `narration.ny_4_1.intro.3` | lucas_mendes | La régie est au sous-sol. Si on y arrive, on peut remplacer le flux. |
| `narration.ny_4_1.intro.4` | agent_d | On ne leur répond pas avec des mots. On leur montre. |
| `narration.ny_4_1.elite.before.0` | agent_amif | Cette zone est sous contrôle. Merci de reculer. Vous êtes filmés. |
| `narration.ny_4_1.elite.before.1` | ilona_varga | Il ne dit pas « interdit ». Il dit « filmés ». Il veut que ça se voie. |
| `narration.ny_4_1.elite.after.0` | lucas_mendes | Régie atteinte. Le flux est à nous. |
| `narration.ny_4_1.exit.0` | narrateur | Sur tous les écrans, l'évacuation du bloc de Harlem : un enfant, des bras, une porte qui tient. |
| `narration.ny_4_1.exit.1` | ilona_varga | Pas le registre, pas encore. Une preuve diffusée trop tôt sera démentie. Montrons d'abord ce qu'on est. |
| `narration.ny_4_1.exit.2` | giulia_rossi | Ça suffit pour ce soir. Les gens ont regardé. |

**Variantes.**
- **Agent D** : `intro.4` comme écrit. **Agent M** : `narration.ny_4_1.intro.4.m` — `agent_m` : « Je vois ce que ces écrans montreront dans une minute, si on les laisse faire. Remplaçons-le avant. »
- **Agent M, reconnaissance** : `narration.ny_4_1.exit.3` — agent_m : « À Moscou, c'étaient des hologrammes. Ici, des panneaux. La mise en scène est la même. »

**Fragment.** `ny_times_square` — **Des écrans à l'envers** : *Chaque écran racontait ton histoire, mais à l'envers. Tu y voyais l'incendie sans les mains qui avaient porté les enfants. Pour une nuit, tu as repris la parole : tu n'as rien dit, tu as seulement montré.*

**QG.** Archive : *À Times Square, l'Aube prend la régie des écrans et diffuse l'évacuation de Harlem à la place de la propagande de l'AMIF.* Fil : *Les cercles de pouvoir attendent à Midtown.*

---

#### `zone_ny_4_2` : Tours de Midtown

**Fiche.** Enjeu : infiltrer les cercles de pouvoir. Ton : verre, moquette, silence cher. Scène clé : les noms des financeurs de l'AMIF et du Syndicat se recoupent.

**Plateau (code).** Élite : Chef de la sécurité (mnh_chief). Objectifs, dans l'ordre : n11 (mini-jeu) : Infiltrer les cercles de pouvoir ; n13 (combat) : Vaincre la sécurité privée ; n20 (mini-jeu) : Franchir les toits des tours ; n21 (atteindre) : Récupérer la liste des financeurs.

**Trame.**
1. L'Aube pénètre dans une tour de verre pendant un gala de fondations ; badges prêtés, regards polis.
2. Les couloirs mènent à une salle où sont affichées des listes de « partenaires ».
3. La sécurité privée (élite) veille sur la salle.
4. Giulia et Lucas comparent deux listes : celles de l'AMIF et celles du registre de Queens. Plusieurs noms reviennent.
5. Une fondation citée porte un nom qui ressemble à des archives des années 80, sans que l'équipe sache encore lesquelles.

**Scènes.**

| Clé | Locuteur | FR |
|---|---|---|
| `narration.ny_4_2.intro.0` | narrateur | La tour de verre domine Midtown. Au quarante-deuxième étage, un gala réunit des fondations, des conseils, des sourires bien payés. |
| `narration.ny_4_2.intro.1` | giulia_rossi | On est les seuls à ne pas porter de badge doré. |
| `narration.ny_4_2.intro.2` | lucas_mendes | Une salle de « partenaires ». Les noms sont affichés comme des trophées. |
| `narration.ny_4_2.intro.3` | agent_d | On ne vole rien. On compare. |
| `narration.ny_4_2.elite.before.0` | enforcer | Cette salle est réservée. Veuillez rejoindre le salon. |
| `narration.ny_4_2.elite.before.1` | ilona_varga | Il a l'air poli. Il est très inquiet. |
| `narration.ny_4_2.elite.after.0` | giulia_rossi | J'ai la liste de l'AMIF. Toi, celle du registre de Queens. |
| `narration.ny_4_2.elite.after.1` | lucas_mendes | Trois noms. Quatre. Cinq. Ils sont sur les deux. |
| `narration.ny_4_2.exit.0` | giulia_rossi | Ceux qui paient l'AMIF paient aussi le Syndicat. Ce n'est pas une collusion, c'est un budget commun. |
| `narration.ny_4_2.exit.1` | ilona_varga | Et là : une fondation dont le nom me dit quelque chose. Des archives des années 80. |
| `narration.ny_4_2.exit.2` | agent_d | Pas ce soir. Mais on garde le nom. |

**Variantes.**
- **Agent D** : `intro.3` comme écrit. **Agent M** : `narration.ny_4_2.intro.3.m` — `agent_m` : « Je vois qui a signé ces noms, et dans quel ordre. On ne vole rien : on lit. »
- **Agent M, reconnaissance** : `narration.ny_4_2.exit.3` — agent_m : « Orlan, le Passeur, ces financeurs… Ce sont les mêmes personnes, en plus polies. »

**Fragment.** `ny_midtown_list` — **Des noms sur deux listes** : *Tu pensais que le Syndicat et l'AMIF étaient deux ennemis qui s'arrangeaient. Tu as compris qu'ils partageaient une comptabilité. Cinq noms revenaient sur les deux listes, comme une signature qu'on ne cherche même plus à cacher.*

**QG.** Archive : *Dans une tour de Midtown, l'Aube découvre que plusieurs financeurs de l'AMIF et du Syndicat se recoupent. Un nom renvoie à des archives des années 80.* Fil : *L'annonce des Accords se prépare à l'ONU.*

---

#### `zone_ny_4_3` : Siège de l'ONU

**Fiche.** Enjeu : protéger l'annonce des Accords de Salem. Ton : hall de marbre, drapeaux, micros, délégués. Scène clé : Rayner et Stroke s'affrontent publiquement.

**Plateau (code).** Élite : Capitaine de l'AMIF (amif_captain). Objectifs, dans l'ordre : n13 (mini-jeu) : Protéger les délégués ; n14 (combat) : Vaincre le capitaine de l'AMIF ; n21 (mini-jeu) : Tenir le hall de l'assemblée ; n22 (atteindre) : Assister à l'annonce des Accords.

**Trame.**
1. Les négociateurs se rassemblent à l'ONU pour l'annonce des Accords. Rayner doit prendre la parole.
2. Des saboteurs tentent de couper la retransmission ; l'Aube sécurise la salle.
3. L'élite de l'AMIF essaie d'évacuer la salle « pour la sécurité des délégués ».
4. Rayner monte sur scène. Stroke apparaît à son tour et répond en direct.
5. L'affrontement est public : l'un parle de protection et de contrôle, l'autre de reconnaissance sans soumission.

**Scènes.**

| Clé | Locuteur | FR |
|---|---|---|
| `narration.ny_4_3.intro.0` | narrateur | Le hall de l'ONU est plein de drapeaux, de micros et de regards. Dans une heure, les Accords de Salem seront annoncés. |
| `narration.ny_4_3.intro.1` | giulia_rossi | Quelqu'un coupe déjà les câbles de la retransmission. |
| `narration.ny_4_3.intro.2` | lucas_mendes | Sabotage à l'étage technique. Si l'annonce ne passe pas, tout ça n'a servi à rien. |
| `narration.ny_4_3.intro.3` | agent_d | On tient la salle. Personne ne coupe cette retransmission. |
| `narration.ny_4_3.elite.before.0` | agent_amif | Pour la sécurité des délégués, la salle est évacuée. Merci de coopérer. |
| `narration.ny_4_3.elite.before.1` | ilona_varga | Évacuer la salle, c'est étouffer l'annonce. Il le sait. |
| `narration.ny_4_3.elite.after.0` | giulia_rossi | La salle est tenue. Rayner peut parler. |
| `narration.ny_4_3.stage.0` | rayner | Les Apex ne demandent pas la permission d'exister. Ils demandent d'être reconnus, pas contrôlés. |
| `narration.ny_4_3.stage.1` | stroke | Madame Rayner parle de reconnaissance. Je parle de responsabilité. Un pouvoir qu'on ne mesure pas est un pouvoir qu'on ne protège pas. |
| `narration.ny_4_3.stage.2` | rayner | Mesurer, prélever, enfermer : ce n'est pas protéger. C'est posséder. |
| `narration.ny_4_3.stage.3` | stroke | Les chiffres de Paris, madame, ne mentent pas. Je ne possède personne. Je compte. |
| `narration.ny_4_3.stage.4` | rayner | Alors comptez aussi ceux que vous avez enfermés à Queens. Il y en avait beaucoup. |
| `narration.ny_4_3.exit.0` | narrateur | La salle retient son souffle. Pour la première fois, la voix de l'AMIF et celle de The Dawn s'opposent devant tous. |
| `narration.ny_4_3.exit.1` | ilona_varga | Il a gardé son calme. Il en gardera encore. Il nous attend dehors. |

**Variantes.**
- **Agent D** : `intro.3` comme écrit. **Agent M** : `narration.ny_4_3.intro.3.m` — `agent_m` : « Je vois les mains qui couperont les câbles dans dix secondes. Je les arrête avant. »
- **Agent D, première rencontre** : `narration.ny_4_3.stage.5.d` — agent_d : « C'est lui. Il n'a pas de visage méchant. C'est ce qui est le plus effrayant. » **Agent M** : `narration.ny_4_3.stage.5.m` — agent_m : « Les chiffres de Paris. J'y étais. Ils n'ont pas dit tout ce qu'ils ont compté. »

**Fragment.** `ny_un_stage` — **Deux voix** : *Une voix parlait de protection, l'autre de liberté. Elles disaient presque les mêmes mots, mais pas pour les mêmes gens. Le monde écoutait. Pour la première fois, personne ne pouvait faire semblant de ne pas entendre.*

**QG.** Archive : *À l'ONU, Rayner et Stroke s'opposent publiquement devant les négociateurs, pendant que l'Aube protège la retransmission de l'annonce des Accords.* Fil : *Stroke attend sur l'esplanade.*

---

#### `zone_ny_4_4` : Esplanade de l'ONU

**Fiche.** Enjeu : boss, le Général Stroke. Ton : esplanade ouverte, drapeaux, lumière froide, flashes. Stroke est calme, administratif, convaincu. Il n'est pas tué : il se retire en promettant que « Salem décidera ». Phases : « Procédure d'urgence » (sous 66 %) et « Protocole de rétention » (sous 33 %).

**Plateau (code).** Élite : Capitaine de l'AMIF (amif_captain). Boss : LE GÉNÉRAL STROKE. Objectifs, dans l'ordre : n04 (atteindre) : Gagner l'esplanade ; n11 (mini-jeu) : Évacuer la foule ; n14 (combat) : Briser la ligne de l'AMIF ; n18 (combat) : Vaincre le Général Stroke.

**Trame.**
1. Dehors, la foule filme. Stroke attend sur l'esplanade, escorté de quelques gardes d'élite.
2. L'Aube franchit la garde.
3. Stroke offre un dernier arrangement : une inscription volontaire des agents de l'Aube sur les registres.
4. L'Aube refuse. Combat.
5. Défait, Stroke se retire en public, sans fuir : il a toujours un plan pour Salem.

**Scènes.**

| Clé | Locuteur | FR |
|---|---|---|
| `narration.ny_4_4.intro.0` | narrateur | L'esplanade de l'ONU est inondée de lumière. Des caméras de presse bordent le périmètre. Au centre, un homme seul en uniforme gris. |
| `narration.ny_4_4.intro.1` | lucas_mendes | Il n'a pas ses gardes de tout à l'heure. Il a voulu qu'on le voie seul. |
| `narration.ny_4_4.elite.before.0` | agent_amif | Le Général a demandé à vous parler. Seul à seul. |
| `narration.ny_4_4.elite.before.1` | ilona_varga | « Seul à seul » devant mille caméras. |
| `narration.ny_4_4.elite.after.0` | giulia_rossi | La garde est tombée. Plus rien entre lui et nous. |
| `narration.ny_4_4.boss.before.0` | stroke | Je vous félicite. Vous avez tenu votre salle, repris vos écrans, retourné quelques indics. Il ne manque que la suite logique. |
| `narration.ny_4_4.boss.before.1` | stroke | Inscrivez-vous. Volontairement. Je vous garantis votre rang, votre immunité, votre autonomie. L'Aube deviendrait une section de l'agence. |
| `narration.ny_4_4.boss.before.2` | agent_d | Votre agence prélève ce qu'elle appelle protéger. On a vu les wagons. |
| `narration.ny_4_4.boss.before.3` | stroke | Vous avez vu des wagons. J'ai vu des milliers de morts à Paris. Nous ne sommes pas d'accord sur la comptabilité. |
| `narration.ny_4_4.boss.phase1.0` | stroke | Procédure d'urgence. Je regrette qu'on en arrive là. |
| `narration.ny_4_4.boss.phase2.0` | stroke | Protocole de rétention. Rien de personnel, rien d'humain : seulement de l'ordre. |
| `narration.ny_4_4.boss.after.0` | stroke | Vous croyez avoir gagné. L'esplanade vous regarde, mais c'est à Salem que tout se règle. |
| `narration.ny_4_4.boss.after.1` | ilona_varga | Il se retire. Il n'a pas l'air vaincu. |
| `narration.ny_4_4.boss.after.2` | stroke | Salem décidera. |
| `narration.ny_4_4.chapter_end.0` | narrateur | Les Accords de Salem sortent au grand jour. Stroke s'est retiré, mais sa promesse pèse sur la ville. |
| `narration.ny_4_4.chapter_end.1` | rayner | Les négociations commencent à Salem. C'est là que tout se jouera. |

**Variantes.**
- **Agent D** : `boss.before.2` comme écrit. **Agent M** : `narration.ny_4_4.boss.before.2.m` — `agent_m` : « À Paris, j'ai vu ce que vous prélevez. Des milliers de morts, vous dites ? Vous avez aussi gardé les restes. »
- **Agent D, première rencontre** : `narration.ny_4_4.boss.after.3.d` — agent_d : « Je ne sais pas ce qu'il prépare à Salem. Mais il sait qu'on y sera. » **Agent M** : `narration.ny_4_4.boss.after.3.m` — agent_m : « Salem. Là où Moscou et Paris se rejoignent. Il sait que j'y serai aussi. »

**Fragment.** `ny_un_esplanade` — **Salem décidera** : *Il ne t'a pas menacé. Il a parlé de comptabilité, de chiffres, d'ordre. Il était seul sur l'esplanade, calme, convaincu. Tu as compris qu'on ne bat pas un homme comme lui d'un seul coup : on lui retire sa scène.*

**QG.** Archive : *Sur l'esplanade de l'ONU, l'Aube affronte le Général Stroke. Il se retire en promettant que Salem décidera. Les Accords sont désormais publics.* Fil : *Direction Salem, où se joueront les Accords.*

---

## Décision : slots `intro` et `mid` (2026-11-16)

L'utilisateur a choisi d'**ajouter une case d'entrée à narration par zone** (`n04` dans les graphes B et C, `n03` dans les graphes A et D). Le slot `mid` n'existe plus : ses répliques sont fusionnées dans `intro` (une seule scène d'entrée, renumérotée). L'ajout de ces cases est à faire côté plateaux par la session « Plateaux de jeux New York et Moscou ».
