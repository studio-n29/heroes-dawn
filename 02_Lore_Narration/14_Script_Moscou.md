# Script de Moscou : « le progrès bâti sur un mensonge »

*Date : 2026-11-16 · Statut : **chapitres 1 à 4 écrits, à relire** (Moscou 2 à 4 validés ; noms propres validés). **Le script fait foi** (décision du 2026-11-16) ; le code existant de ces trois chapitres devra être aligné. Design : `Code/docs/specs/2026-11-16-narration-new-york-moscou-design.md`. Trame et thèse : `10_Quetes_Moscou.md`. Bible : `08_Bible_Narrative.md`.*

Ce fichier contient les **dialogues, événements et variantes** de Moscou. Les répliques sont en français ; l'anglais viendra en seconde passe.

## Mode d'emploi

**Clés** : `narration.<zone>.<slot>.<i>` (i = rang de la réplique dans le slot, à partir de 0). Slots : `intro` (case d'entrée, scène d'ouverture de la zone ; l'ancien slot `mid` y est fusionné), `elite.before` / `elite.after` (autour du combat d'élite), `exit` (case de sortie), `boss.before` / `boss.after`, `chapter_end` (dernière scène du chapitre). Les slots ne sont pas des numéros de cases : le tableau *Correspondance* de chaque chapitre dit à quelle case jouable chaque slot est branché quand le plateau existe.

**Ton de Moscou** : politesse et froideur. Les antagonistes sont courtois, jamais menaçants à voix haute : ils parlent de clients, d'accueil, de procédure. L'AMIF n'est **jamais à l'écran** avant le chapitre 4.

**Variantes par parcours** : une réplique de base `X` est jouée telle qu'elle est écrite dans le tableau, quel que soit le parcours. Si une variante remplace `X`, elle porte une clé propre : `X.d` (Agent D) ou `X.m` (Agent M) ; elle remplace `X` pour ce parcours. Les répliques ajoutées pour un seul parcours (reconnaissances, indices de la cathédrale) ont leur propre clé et ne sont jouées que pour ce parcours.

**Rappels de canon** : preuve n°2 = *le registre du monastère* (partie 1, ch. 1) et *les dossiers du laboratoire* (partie 2, ch. 2). Le Passeur ne dit jamais son nom. L'attentat de la cathédrale n'est pas expliqué avant le chapitre 4 : avant, seulement des indices ambigus (au plus un par zone).

## Table des locuteurs

| Id | Nom affiché | Rôle |
|---|---|---|
| `agent_m` | Agent M | Personnage joueur (route Moscou). Voit passé, présent, futur de ce qui l'entoure |
| `agent_d` | Agent D | Personnage joueur (route New York) ; à Moscou à l'acte III |
| `ilona_varga` | Ilona Varga | Équipe de départ ; lit les émotions et les peurs ; mentor d'Agent M |
| `giulia_rossi` | Giulia Rossi | Équipe de départ |
| `lucas_mendes` | Lucas Mendes | Équipe de départ |
| `passeur` | Le Passeur | Boss du chapitre 1 ; ne dit jamais son nom |
| `enforcer` | Homme de main | Garde ou escorte des ennemis d'élite |
| `contremaitre` | Fiodor Gromov, le Contremaître | Boss du chapitre 2 |
| `orlan` | Dmitri Orlan | Boss du chapitre 3 |
| `conseiller` | Arkadi Zorin, le Conseiller | Boss du chapitre 4 |
| `ilia` | Ilia | Frère d'Agent M (flashbacks, chapitre 4) |
| `ouvrier` | Un ouvrier | Raffinerie de Kapotnya |
| `technicien` | Un technicien | Chef du laboratoire clandestin (2.3) |
| `apex_libere` | Un Apex libéré | Prélevé, libéré au laboratoire (2.3) |
| `commissaire` | Le commissaire | Enchères de Sadovoye (3.2) |
| `technicien_holo` | Technicien d'hologrammes | Studio (3.3) |
| `civil_moscou` | Un civil | Rues sous couvre-feu (4.1) |
| `agent_amif` | Un agent de l'AMIF | Envoyé en civil (4.1, 4.3) ; première apparition de l'AMIF à Moscou |
| `rayner` | Brie Rayner | Fondatrice de The Dawn (apparitions brèves, voix) |
| `narrateur` | — | Texte de narration sans locuteur |

*Noms validés par l'utilisateur le 2026-11-16 : Fiodor Gromov, Arkadi Zorin, Ilia.*

---

## Chapitre 1 : Sergueiv

Id : `mos_sergueiv`. **Un accueil trop parfait.** Acte I pour Agent M, acte III pour Agent D.

**Texte d'ouverture (existant)** : *Sergueiv, ses monastères et sa ligne de train futuriste : Moscou s'est armée face aux changements du monde, mais elle accueille l'Aube. Derrière la façade, quelqu'un tire déjà les ficelles.*

### Variantes d'ouverture du chapitre

| Parcours | Phrase ajoutée à l'ouverture |
|---|---|
| Agent M (acte I) | *Elle connaît ces rues, ces codes, ces sourires. Elle n'a jamais regardé ce qu'ils couvraient.* |
| Agent D (acte III) | *Il a déjà vu un gang et une agence marcher ensemble. Ici, personne ne s'en cache : tout est poli.* |

### Correspondance slots → cases (d'après le design Agent M)

| Zone | Entrée (`intro`) | elite.before / after | exit / boss | Graphe |
|---|---|---|---|---|
| `zone_mos_1_1` | n03 | n14 | n19 (exit) | A |
| `zone_mos_1_2` | n04 | n14 | n18 (exit) | B |
| `zone_mos_1_3` | n04 | n14 | n18 (exit) | B |
| `zone_mos_1_4` | n04 | n14 (élite de garde) | n18 (boss) | B |

Décision du 2026-11-16 : la case d'entrée (`n03` en graphe A, `n04` en graphe B) porte le slot `intro` ; l'ancien slot `mid` y est fusionné. Les plateaux de Sergueiv étant ceux du design Agent M, l'ajout de cette narration d'entrée est à vérifier avec la session « Plateaux de jeux New York et Moscou ».

---

#### `zone_mos_1_1` : Gare du maglev

**Fiche.** Enjeu : traverser l'accueil officiel. Ton : cérémonie, hologrammes, sourires. Scène clé : un cargo scellé n'est inscrit sur aucun manifeste. Preuve : aucune encore, seulement une anomalie.

**Trame.**
1. L'Aube descend du maglev sous un portique d'hologrammes qui la salue par son nom. Un chef de quai la félicite comme une délégation d'État ; rien n'est improvisé.
2. Agent M voit les quais balayés « avant » : le personnel a nettoyé pendant la nuit pour des invités qui n'étaient pas encore arrivés.
3. En se glissant derrière les quais, l'équipe tombe sur un cargo scellé voie 9, sans nom ni manifeste.
4. L'escorte du cargo (homme de main d'élite) les prie de rebrousser chemin, en termes d'hospitalité.
5. Après le combat, Agent M voit les camions qui ont déposé le cargo, trois jours avant leur arrivée.
6. L'équipe quitte la gare par les archives du monastère.

**Scènes.**

| Clé | Locuteur | FR |
|---|---|---|
| `narration.mos_1_1.intro.0` | narrateur | Les hologrammes saluent l'Aube par son nom. Les quais sont balayés, les sourires parfaits. |
| `narration.mos_1_1.intro.1` | giulia_rossi | On dirait qu'ils attendaient un chef d'État. |
| `narration.mos_1_1.intro.2` | ilona_varga | Ils attendaient quelqu'un. Je ne sais pas encore qui. |
| `narration.mos_1_1.intro.3` | agent_m | Ces quais ont été lavés cette nuit. Je vois les balais, les seaux… et personne n'a dormi. |
| `narration.mos_1_1.intro.4` | lucas_mendes | Un accueil de cette qualité, ça se prépare des jours à l'avance. |
| `narration.mos_1_1.intro.5` | ilona_varga | Ou ça sert à nous garder les yeux sur la façade. |
| `narration.mos_1_1.elite.before.0` | enforcer | Bienvenue à Moscou. Le comité d'accueil vous prie de rebrousser chemin. |
| `narration.mos_1_1.elite.before.1` | ilona_varga | Un accueil trop parfait cache toujours quelque chose. |
| `narration.mos_1_1.elite.after.0` | agent_m | Ce cargo scellé n'est sur aucun manifeste. Je vois les camions qui l'ont déposé… trois jours avant nous. |
| `narration.mos_1_1.elite.after.1` | lucas_mendes | Trois jours ? Avant même qu'on décide de venir ? |
| `narration.mos_1_1.elite.after.2` | agent_m | Avant même qu'on décide de venir. |
| `narration.mos_1_1.exit.0` | ilona_varga | On sort par les archives du monastère. Si quelqu'un nous y attend, ce n'est pas pour nous souhaiter la bienvenue. |

**Variantes.**
- **Agent M** : `intro.3` est dit par elle (elle reconnaît le personnel). **Agent D** : `narration.mos_1_1.intro.3.d` — `agent_d` : « Ils ont lavé ces quais pour nous. À New York, on cache les caméras, pas les balais. »
- **Agent M, indice cathédrale** (un seul, ambigu) : `narration.mos_1_1.intro.6` — agent_m : « Cette odeur de cire et d'encens sur un quai de gare… je la connais. » *(joué seulement pour Agent M ; sens révélé au chapitre 4)*.

**Fragment.** `mos_maglev_welcome` — **L'accueil parfait** : *Les hologrammes saluaient l'Aube par son nom, les quais étaient balayés, les sourires, parfaits. Un cargo scellé dormait pourtant sur la voie 9, sans nom et sans manifeste. Moscou t'ouvrait grand les bras, et regardait ailleurs.*

**QG.** Archive : *À la gare du maglev, l'Aube est accueillie comme une délégation d'État. Un cargo scellé, absent de tous les manifestes, y a été déposé trois jours plus tôt.* Fil : *Qui a déposé le cargo de la voie 9 ?*

---

#### `zone_mos_1_2` : Monastère-archives

**Fiche.** Enjeu : fouiller les archives. Ton : pierre, cire, fresques. Scène clé : un registre caché derrière une fresque. Preuve : **preuve n°2, partie 1** (registre du monastère).

**Trame.**
1. Le monastère sert aussi d'archives de la ville : calme, voûtes, un moine-archiviste qui garde la porte sans poser de questions.
2. Agent M, restauratrice, repère une fresque repeinte récemment : le pigment est trop neuf.
3. Mini-jeu des rouages : la fresque pivote.
4. Le gardien des archives (élite) surgit et ferme les lieux « au public ».
5. Derrière la fresque, un registre : des noms, des dates, des livraisons. La fin renvoie vers Kapotnya.

**Scènes.**

| Clé | Locuteur | FR |
|---|---|---|
| `narration.mos_1_2.intro.0` | narrateur | Le monastère est silencieux. Les archives de Sergueiv dorment sous les voûtes, au bout d'un couloir de cierges. |
| `narration.mos_1_2.intro.1` | lucas_mendes | Les cierges sont électriques. Même les saints ont une facture d'énergie. |
| `narration.mos_1_2.intro.2` | agent_m | Cette fresque a été repeinte. Moins de dix jours. Le pigment ne ment pas. |
| `narration.mos_1_2.intro.3` | giulia_rossi | Quelqu'un a pris la peine de peindre par-dessus un mur. Pour cacher quoi ? |
| `narration.mos_1_2.intro.4` | agent_m | Je le vois… et je ne vois pas la couleur, je vois la porte. |
| `narration.mos_1_2.elite.before.0` | enforcer | Les archives sont fermées au public. |
| `narration.mos_1_2.elite.before.1` | agent_m | Pas pour la restauratrice de la fresque. |
| `narration.mos_1_2.elite.after.0` | agent_m | Derrière la fresque, un registre. Je l'avais vu avant de le toucher. |
| `narration.mos_1_2.elite.after.1` | lucas_mendes | Des noms… des dates… des livraisons. Ce sont des ventes. |
| `narration.mos_1_2.exit.0` | ilona_varga | Un registre, la moitié d'une preuve. L'autre moitié est du côté des raffineries. |
| `narration.mos_1_2.exit.1` | agent_m | Alors on part pour Kapotnya. |

**Variantes.**
- **Agent M** : `intro.2` est dit par elle (restauratrice). **Agent D** : `narration.mos_1_2.intro.2.d` — `agent_d` : « Je dessine des murs, pas des fresques. Mais celui-ci a été retouché, j'en suis sûr. » ; `narration.mos_1_2.intro.4.d` — agent_d : « Je vois le trait qui ne va pas. Je ne vois pas pourquoi. »
- **Agent M, indice cathédrale** : `narration.mos_1_2.intro.5` — agent_m : « Même couloir de cierges que chez moi… avant que tout bascule. » *(sens révélé au chapitre 4)*.

**Fragment.** `mos_monastery_register` — **Le registre sous la fresque** : *Tu as restauré mille fresques sans jamais chercher ce qu'elles cachaient. Celle-ci couvrait un registre : des noms, des dates, des livraisons. Quelqu'un avait peint par-dessus une vérité, et l'avait confiée aux saints.*

**QG.** Archive : *Dans les archives du monastère, une fresque récemment repeinte cachait un registre de livraisons. C'est la première moitié de la preuve du marché noir.* Fil : *Le registre mentionne Kapotnya.*

---

#### `zone_mos_1_3` : Voie du maglev

**Fiche.** Enjeu : poursuivre le convoi. Ton : course, tunnels, éclairage de fête. Scène clé : caisses marquées d'un code soviétique. Preuve : indice vers l'ancien Volkov (piste vers Salem).

**Trame.**
1. L'équipe rattrape un convoi qui file sous la ville, éclairé comme une fête (mini-jeu de sauvetage sur les toits).
2. L'escorte du convoi (élite) nie son existence.
3. Les caisses ouvertes portent un code soviétique.
4. Ilona demande à Agent M de se souvenir du code.

**Scènes.**

| Clé | Locuteur | FR |
|---|---|---|
| `narration.mos_1_3.intro.0` | narrateur | Le maglev file sous la ville, éclairé comme une fête. Un convoi roule en parallèle, sans lumière. |
| `narration.mos_1_3.intro.1` | giulia_rossi | Un convoi qui ne figure sur aucun horaire, sur une ligne qui affiche tout. |
| `narration.mos_1_3.intro.2` | agent_m | Je vois les caisses… avant qu'on les charge. Ce sont les mêmes que sur la voie 9. |
| `narration.mos_1_3.intro.3` | lucas_mendes | Alors c'est la même filière. |
| `narration.mos_1_3.elite.before.0` | enforcer | Ce convoi n'existe pas. Vous n'avez rien vu. |
| `narration.mos_1_3.elite.before.1` | ilona_varga | Il le pense. C'est ça qui me gêne : il a peur pour lui, pas pour la cargaison. |
| `narration.mos_1_3.elite.after.0` | agent_m | Ces caisses portent un code soviétique. Quelqu'un déterre de vieilles recherches. |
| `narration.mos_1_3.elite.after.1` | lucas_mendes | Soviétique ? Cinq ans après la Supernova, qui exhume ça ? |
| `narration.mos_1_3.exit.0` | ilona_varga | Un code d'avant l'Union sur des caisses de cette année. Retiens-le. |
| `narration.mos_1_3.exit.1` | agent_m | Je le retiens. Il ne s'efface pas. |

**Variantes.**
- **Agent M** : `intro.2` par elle (vision du passé des caisses). **Agent D** : `narration.mos_1_3.intro.2.d` — `agent_d` : « Je ne vois pas leur passé, mais je vois qu'on a emballé ces caisses à la main. Quelqu'un tient à elles. »

**Fragment.** `mos_convoy_crates` — **Des caisses marquées** : *Le convoi filait sous la ville, éclairé comme une fête. Les caisses portaient un code que personne n'utilise plus. Il y avait des mains neuves sur de très vieilles recherches.*

**QG.** Archive : *Le convoi du maglev transporte des caisses marquées d'un code soviétique. Quelqu'un exhume d'anciennes recherches.* Fil : *Que contiennent les caisses au code soviétique ?*

---

#### `zone_mos_1_4` : Dépôt du maglev

**Fiche.** Enjeu : boss, le Passeur. Ton : bureau feutré dans un entrepôt, comptabilité, courtoisie. Le Passeur ne dit jamais « ennemi » ni « victime » : seulement « client ». Fin : l'accueil visait à tenir l'Aube loin de Kapotnya.

**Trame.**
1. Le dépôt est un entrepôt ordonné, comme une boutique : étiquettes, prix, clients.
2. L'équipe fouille le dépôt (mini-jeu de caisses) et trouve des livraisons à venir.
3. La garde du dépôt (élite) tombe.
4. Le Passeur accueille l'Aube comme un rendez-vous manqué.
5. Combat : le Passeur compte ce qu'il peut vendre de chacun. Phases : « Les clients arrivent » (sous 66 %), « Tout doit partir » (sous 33 %).
6. Après le combat, il avoue que l'accueil entier était sa mise en scène, pour éloigner l'Aube de Kapotnya.

**Scènes.**

| Clé | Locuteur | FR |
|---|---|---|
| `narration.mos_1_4.intro.0` | narrateur | Le dépôt est rangé comme une boutique. Chaque caisse porte une étiquette, chaque étiquette un prix. |
| `narration.mos_1_4.intro.1` | giulia_rossi | Un entrepôt de contrebande avec un système de rangement plus propre que le mien. |
| `narration.mos_1_4.intro.2` | agent_m | Ces caisses partent ce soir. Je vois le camion, le chemin, l'adresse : une raffinerie. |
| `narration.mos_1_4.intro.3` | lucas_mendes | Donc tout converge vers Kapotnya. |
| `narration.mos_1_4.elite.before.0` | enforcer | Le dépôt n'est pas ouvert. Vous avez dû vous tromper de guichet. |
| `narration.mos_1_4.elite.after.0` | ilona_varga | Il n'avait pas peur de nous. Il avait peur de celui qui est derrière. |
| `narration.mos_1_4.boss.before.0` | passeur | Vous êtes en avance. D'ordinaire mes clients prennent rendez-vous. |
| `narration.mos_1_4.boss.before.1` | passeur | Je ne dis pas victimes. Je ne dis pas adversaires. Je dis : clients. |
| `narration.mos_1_4.boss.before.2` | ilona_varga | N'écoute pas. Il compte déjà ce qu'il peut vendre de nous. |
| `narration.mos_1_4.boss.before.3` | passeur | Les pouvoirs ont un prix, mademoiselle Varga. Les vôtres aussi. |
| `narration.mos_1_4.boss.phase1.0` | passeur | Les clients arrivent. Je regrette : la boutique ferme tôt ce soir. |
| `narration.mos_1_4.boss.phase2.0` | passeur | Tout doit partir. Vous compris. |
| `narration.mos_1_4.boss.after.0` | passeur | Vous croyez avoir trouvé quelque chose ? On vous a laissé trouver. L'accueil, c'est moi qui l'ai organisé. |
| `narration.mos_1_4.boss.after.1` | agent_m | Alors c'était pour nous tenir loin de Kapotnya. |
| `narration.mos_1_4.boss.after.2` | passeur | Je n'ai jamais dit « loin ». J'ai dit « accueillis ». |
| `narration.mos_1_4.boss.after.3` | ilona_varga | Alors c'est là que nous irons. |
| `narration.mos_1_4.chapter_end.0` | narrateur | La Dawn comprend que l'accueil visait à la tenir loin de Kapotnya. Le convoi suivant part à l'aube. |

**Variantes.**
- **Agent M** : `narration.mos_1_4.boss.before.3.m` — passeur (s'adresse à elle) plutôt qu'à Ilona : « Votre vue sur le passé vaut cher, mademoiselle. Les enchères sont ouvertes. » **Agent D** : `narration.mos_1_4.boss.before.3.d` — passeur (adressé à D) : « Une imagination qui devient matière. Voilà un produit que je n'ai jamais eu en rayon. »
- **Agent M, indice cathédrale** : `narration.mos_1_4.boss.after.4` — passeur : « La cire, l'encens, les cierges… c'est un parfum qui revient souvent chez mes clients. » *(joué seulement pour Agent M ; sens révélé au chapitre 4)*.

**Fragment.** `mos_depot_passeur` — **Le Passeur** : *Il ne disait jamais victime, jamais ennemi : seulement client. Dans son dépôt, tout avait un prix, même l'accueil qu'on t'avait réservé. Ce soir, tu as compris que la ville entière avait été invitée à regarder ailleurs.*

**QG.** Archive : *Le Passeur dirigeait le dépôt du maglev comme une boutique. L'accueil de l'Aube était sa mise en scène, pour l'éloigner de Kapotnya.* Fil : *Direction Kapotnya, ses raffineries et son convoi.*

---

## Chapitre 2 : Kapotnya

Id : `mos_kapotnya`. **Preuve du marché noir.** Acte I pour Agent M, acte III pour Agent D.

**Texte d'ouverture (existant)** : *Kapotnya, ses raffineries et son brouillard industriel : les oligarques y cachent ce qu'ils ne veulent pas montrer. L'Aube suit des convois, des fuites et une mafia alliée à l'AMIF dans l'ombre.*

**Rappels.** L'AMIF n'est **jamais à l'écran** : elle n'existe que dans les dossiers et dans la bouche de ses « clients ». Preuve n°2, partie 2 : les dossiers du laboratoire clandestin. Les indices de la cathédrale continuent (cire, encens, cierges) ; les annotations de Volkov apparaissent pour la première fois (piste vers Salem).

### Variantes d'ouverture du chapitre

| Parcours | Phrase ajoutée à l'ouverture |
|---|---|
| Agent M (acte I) | *Elle connaît ce brouillard : sa ville le respire depuis toujours. Elle n'a jamais su ce qu'il couvrait.* |
| Agent D (acte III) | *Il a déjà vu un gang et une agence marcher ensemble. Ici, c'est un État qui regarde ailleurs.* |

### Correspondance slots → cases

| Zone | Graphe | Entrée (`intro.*`) | Élite (`elite.*`) | Fin |
|---|---|---|---|---|
| `zone_mos_2_1` | B | n04 (`intro`) | n14 | n18 (`exit`) |
| `zone_mos_2_2` | C | n04 (`intro`) | n14 | n22 (`exit`) |
| `zone_mos_2_3` | D | n03 (`intro`) | n13 | n21 (`exit`) |
| `zone_mos_2_4` | C | n04 (`intro`) | n14 | n22 (`boss.*`, `chapter_end`) |

*Règle (plateaux du code, session « Plateaux de jeux New York et Moscou », 2026-11-16) : une narration est attachée à trois cases par zone : la **case d'entrée** (`intro`, ajoutée sur décision de l'utilisateur du 2026-11-16 : `n04` dans les graphes B et C, `n03` dans les graphes A et D), l'élite fixe (`elite.before/after`) et la dernière case (`exit`, ou `boss.*` en 2.4, 3.4 et 4.4). L'ancien slot `mid` est fusionné dans `intro`.*

---

#### `zone_mos_2_1` : Raffinerie

**Fiche.** Enjeu : colmater une fuite et sauver les ouvriers. Ton : brouillard, torchères, gaz. Scène clé : la fuite est volontaire, elle couvre un transfert.

**Plateau (code).** Élite : Surveillant de site (kap_surveillant). Objectifs, dans l'ordre : n04 (atteindre) : Franchir le portail de la raffinerie ; n11 (mini-jeu) : Colmater la fuite ; n14 (combat) : Briser la garde de la raffinerie ; n18 (atteindre) : Sortir les ouvriers.

**Trame.**
1. Une fuite de gaz vient d'éclater à la raffinerie ; les ouvriers sont bloqués.
2. L'Aube colmate la fuite et ouvre les passages.
3. Le surveillant (élite) gêne l'évacuation : « Le transfert ne s'arrête pas pour un accident. »
4. Agent M voit la vanne ouverte à la main, dix minutes avant l'alerte.
5. Un ouvrier avoue qu'on leur avait demandé de quitter le site « pour la nuit ».

**Scènes.**

| Clé | Locuteur | FR |
|---|---|---|
| `narration.mos_2_1.intro.0` | narrateur | Kapotnya respire un brouillard jaune. Les torchères éclairent les tuyaux comme des cierges. |
| `narration.mos_2_1.intro.1` | lucas_mendes | Alarme de gaz sur le bloc C. Il reste des gens à l'intérieur. |
| `narration.mos_2_1.intro.2` | agent_m | La vanne était ouverte avant l'alarme. Quelqu'un l'a tournée à la main. |
| `narration.mos_2_1.intro.3` | giulia_rossi | Pourquoi provoquer une fuite sur sa propre raffinerie ? |
| `narration.mos_2_1.elite.before.0` | enforcer | L'évacuation est suspendue. Le transfert ne s'arrête pas pour un accident. |
| `narration.mos_2_1.elite.before.1` | ilona_varga | Il parle de transfert avant de parler des ouvriers. Il vient de se trahir. |
| `narration.mos_2_1.elite.after.0` | ouvrier | On nous a dit de quitter le site pour la nuit. Quelques-uns ont refusé. |
| `narration.mos_2_1.elite.after.1` | agent_m | La fuite sert d'écran : pendant qu'on regarde les flammes, on charge autre chose. |
| `narration.mos_2_1.exit.0` | ilona_varga | Une fuite volontaire pour couvrir un transfert. Les camions partent vers la route du convoi. |
| `narration.mos_2_1.exit.1` | agent_m | Alors on les suit. |

**Variantes.**
- **Agent M** : `intro.2` par elle (vision de la vanne). **Agent D** : `narration.mos_2_1.intro.2.d` — `agent_d` : « Je vois la trace d'une main sur cette vanne. Quelqu'un l'a ouverte exprès. »
- **Agent M, indice cathédrale** : `narration.mos_2_1.intro.4` — agent_m : « Les torchères de la raffinerie… de loin, on dirait des cierges. Chez moi, tout brûlait comme ça. »

**Fragment.** `mos_refinery_leak` — **La fuite qui servait d'écran** : *Les torchères brûlaient comme des cierges sur un autel de tuyaux. La fuite n'était pas un accident : on avait ouvert la vanne pour que tous regardent les flammes pendant qu'on chargeait autre chose dans l'ombre.*

**QG.** Archive : *À la raffinerie de Kapotnya, l'Aube colmate une fuite volontaire qui servait à couvrir un transfert de marchandises.* Fil : *Les camions du transfert prennent la route du convoi.*

---

#### `zone_mos_2_2` : Route du convoi

**Fiche.** Enjeu : intercepter les camions. Ton : route de nuit, brume, phares éteints. Scène clé : les camions transportent des résidus de la brèche.

**Plateau (code).** Élite : Surveillant de site (kap_surveillant). Objectifs, dans l'ordre : n13 (mini-jeu) : Rattraper les camions ; n14 (combat) : Briser l'escorte du convoi ; n21 (mini-jeu) : Inspecter les caisses de résidus ; n22 (atteindre) : Suivre la route jusqu'au bout.

**Trame.**
1. L'Aube poursuit les camions sur la route industrielle.
2. Elle bloque le premier camion : le chef de convoi (élite) refuse d'ouvrir.
3. Dans la cargaison, des caisses lumineuses : des résidus de la brèche de la Supernova.
4. Le chef de convoi dit qu'il livre « à des clients ordinaires ».
5. Les résidus portent le code soviétique déjà vu à Sergueiv.

**Scènes.**

| Clé | Locuteur | FR |
|---|---|---|
| `narration.mos_2_2.intro.0` | narrateur | La route du convoi traverse la brume. Les camions roulent sans phares, comme s'ils connaissaient chaque virage. |
| `narration.mos_2_2.intro.1` | giulia_rossi | Quatre camions, une escorte, et pas un seul feu allumé. |
| `narration.mos_2_2.intro.2` | agent_m | Les camions sont chargés de ce que nous avons vu sur la voie 9. Mais plus lourd, plus dangereux. |
| `narration.mos_2_2.elite.before.0` | enforcer | Ce convoi est autorisé. Vous avez la permission de reculer. |
| `narration.mos_2_2.elite.before.1` | lucas_mendes | Autorisé par qui ? Il ne répond pas. |
| `narration.mos_2_2.elite.after.0` | giulia_rossi | Les caisses brillent. Ce sont des résidus de la Supernova. |
| `narration.mos_2_2.elite.after.1` | ilona_varga | Des fragments de la brèche, transportés comme du charbon. |
| `narration.mos_2_2.exit.0` | agent_m | Même code soviétique que sur les caisses de Sergueiv. Quelqu'un relie les deux. |
| `narration.mos_2_2.exit.1` | ilona_varga | Alors le laboratoire est au bout de cette route. |

**Variantes.**
- **Agent M** : `intro.2` par elle. **Agent D** : `narration.mos_2_2.intro.2.d` — `agent_d` : « Ces caisses ont été emballées par des mains qui savent ce qu'elles portent. »

**Fragment.** `mos_convoy_residues` — **Du charbon qui brille** : *Les résidus de la brèche voyageaient dans des caisses de charbon, sous une bâche. Ils brillaient d'une lumière qui n'appartenait à aucune lampe. Tu as pensé à Paris, et à ceux qui s'étaient demandé pourquoi tout le monde voulait cette lueur.*

**QG.** Archive : *Sur la route de Kapotnya, l'Aube intercepte des camions chargés de résidus de la brèche. Les caisses portent un code soviétique.* Fil : *La route mène à un laboratoire clandestin.*

---

#### `zone_mos_2_3` : Laboratoire clandestin

**Fiche.** Enjeu : libérer des Apex prélevés. Ton : couloirs blancs, tubes, voix étouffées. Scène clé : les dossiers montrent l'AMIF comme client : **preuve n°2, partie 2** (dossiers du laboratoire).

**Plateau (code).** Élite : Surveillant de site (kap_surveillant). Objectifs, dans l'ordre : n11 (mini-jeu) : Fouiller les dossiers ; n13 (combat) : Vaincre les techniciens ; n20 (mini-jeu) : Libérer les Apex ; n21 (atteindre) : Quitter le laboratoire.

**Trame.**
1. Le laboratoire est caché dans un ancien entrepôt frigorifique.
2. L'Aube trouve des Apex prélevés, attachés, affaiblis.
3. Le chef du laboratoire (élite) tente de détruire les dossiers.
4. Après le combat, les dossiers montrent un client récurrent : l'AMIF. Des annotations signées « Volkov » courent dans les marges.
5. Un Apex libéré dit qu'on lui a pris « ce qui brille » en lui.

**Scènes.**

| Clé | Locuteur | FR |
|---|---|---|
| `narration.mos_2_3.intro.0` | narrateur | Sous l'entrepôt frigorifique, un laboratoire. Les couloirs sont blancs, les tubes pleins d'une lumière qui n'est pas de l'électricité. |
| `narration.mos_2_3.intro.1` | lucas_mendes | Je n'aime pas cette lumière. Elle a la couleur de Paris. |
| `narration.mos_2_3.intro.2` | apex_libere | Ne… ne me touchez pas. Ils m'ont déjà pris ce qui brillait en moi. |
| `narration.mos_2_3.intro.3` | ilona_varga | Tu es en sécurité. Respire. On te sort. |
| `narration.mos_2_3.elite.before.0` | technicien | Les dossiers ne sortent pas. Je les détruirai plutôt que de vous les laisser. |
| `narration.mos_2_3.elite.before.1` | agent_m | Je vois ses mains avant qu'il ne bouge. Il va jeter les dossiers dans l'incinérateur. |
| `narration.mos_2_3.elite.after.0` | giulia_rossi | Les dossiers sont intacts. Toutes les livraisons, tous les noms de clients. |
| `narration.mos_2_3.elite.after.1` | lucas_mendes | Un client revient à chaque page : l'AMIF. |
| `narration.mos_2_3.elite.after.2` | ilona_varga | Et dans les marges, des annotations signées « Volkov ». Des chiffres d'avant l'Union. |
| `narration.mos_2_3.exit.0` | apex_libere | Merci. Je croyais que personne ne viendrait. |
| `narration.mos_2_3.exit.1` | agent_m | Les dossiers du laboratoire, avec le registre du monastère : le marché noir est prouvé. |
| `narration.mos_2_3.exit.2` | ilona_varga | Il manque encore celui qui les paie. |

**Variantes.**
- **Agent M** : `elite.before.1` par elle (vision). **Agent D** : `narration.mos_2_3.elite.before.1.d` — `agent_d` : « Il n'a pas l'air d'un chimiste. Il a l'air d'un homme qui a peur de ceux qui lisent ces papiers. »
- **Agent M, indice cathédrale** : `narration.mos_2_3.intro.4` — agent_m : « L'odeur… cette cire brûlée, ici ? Sous un laboratoire ? » *(sens révélé au chapitre 4)*.

**Fragment.** `mos_lab_files` — **Les dossiers du laboratoire** : *Tout était rangé, daté, signé : des Apex, des prélèvements, des clients. Un client revenait à chaque page. Dans les marges, une écriture ancienne annotait chaque ligne, comme si les morts continuaient à tenir les comptes.*

**QG.** Archive : *Dans le laboratoire clandestin de Kapotnya, l'Aube libère des Apex prélevés et saisit les dossiers montrant l'AMIF comme client : seconde partie de la preuve du marché noir.* Fil : *Les dossiers désignent le quai de chargement.*

---

#### `zone_mos_2_4` : Quai de chargement

**Fiche.** Enjeu : boss, le Contremaître (Fiodor Gromov). Ton : quai de nuit, grues, bordereaux. Gromov ne pose jamais de question : il vérifie, charge et signe. Scène clé : « Orlan paie. » Phases : « Dernier chargement » (sous 66 %) et « Tout est signé » (sous 33 %).

**Plateau (code).** Élite : Surveillant de site (kap_surveillant). Boss : LE CONTREMAÎTRE. Objectifs, dans l'ordre : n13 (mini-jeu) : Franchir les grues ; n14 (combat) : Briser la garde du quai ; n21 (mini-jeu) : Faire parler les dockers ; n22 (combat) : Vaincre le Contremaître.

**Trame.**
1. Le quai de Kapotnya charge le dernier camion avant l'aube.
2. La garde du quai (élite) retient l'Aube.
3. Gromov reçoit l'Aube comme un client en retard.
4. Combat.
5. Défait, il lâche un nom : « Orlan paie. »

**Scènes.**

| Clé | Locuteur | FR |
|---|---|---|
| `narration.mos_2_4.intro.0` | narrateur | Le quai de chargement s'étend sous les grues. Un dernier camion attend, les phares éteints. |
| `narration.mos_2_4.intro.1` | giulia_rossi | Tous les bordereaux portent la même signature, ligne après ligne. |
| `narration.mos_2_4.elite.before.0` | enforcer | Ce quai est fermé. Le chef de quai vous recevra, mais pas ce soir. |
| `narration.mos_2_4.elite.after.0` | ilona_varga | La garde est tombée. Il attend sur la passerelle. |
| `narration.mos_2_4.boss.before.0` | contremaitre | Gromov, chef de quai. Vous êtes en retard : le dernier camion est déjà parti. |
| `narration.mos_2_4.boss.before.1` | contremaitre | Je ne pose pas de questions. Je vérifie les bordereaux et je signe. C'est pour cela qu'on me paie. |
| `narration.mos_2_4.boss.before.2` | agent_m | Par qui ? |
| `narration.mos_2_4.boss.before.3` | contremaitre | Un chef de quai ne répond pas à cette question. Un chef de quai pèse, compte, expédie. |
| `narration.mos_2_4.boss.phase1.0` | contremaitre | Dernier chargement. Je regrette de vous faire attendre. |
| `narration.mos_2_4.boss.phase2.0` | contremaitre | Tout est signé. Vous n'avez plus rien à contester. |
| `narration.mos_2_4.boss.after.0` | contremaitre | Assez. Je ne suis qu'un chef de quai : je compte, je charge, je signe. |
| `narration.mos_2_4.boss.after.1` | contremaitre | Orlan paie. |
| `narration.mos_2_4.boss.after.2` | agent_m | Orlan. |
| `narration.mos_2_4.chapter_end.0` | narrateur | L'Aube sait désormais pour qui travaillent les camions. Le nom d'Orlan mène aux tours de Sadovoye Koltso. |

**Variantes.**
- **Agent M** : `boss.before.2` par elle. **Agent D** : `narration.mos_2_4.boss.before.2.d` — `agent_d` : « Qui signe vos chèques ? »
- **Agent M, indice cathédrale** : `narration.mos_2_4.boss.after.3` — contremaitre : « La cire sur les caisses ? Une habitude des hommes qui nous les confient. Une habitude d'église. » *(sens révélé au chapitre 4)*.

**Fragment.** `mos_quay_gromov` — **Un chef de quai** : *Il ne posait pas de questions. Il pesait, comptait, signait. Chaque bordereau portait le même nom, et tu as compris que le mal le plus poli est celui qui se contente de signer.*

**QG.** Archive : *Au quai de chargement de Kapotnya, l'Aube vainc le Contremaître. Il lâche un nom avant de tomber : « Orlan paie. »* Fil : *Direction Sadovoye Koltso et les tours d'Orlan.*

---

## Chapitre 3 : Sadovoye Koltso

Id : `mos_sadovoye`. **Le pouvoir se met en scène.** Acte I pour Agent M, acte III pour Agent D.

**Texte d'ouverture (existant)** : *Sur le Jardin, l'anneau de la ville, les tours des oligarques brillent de mille hologrammes. L'Aube comprend que le pouvoir ne se cache plus : il se met en scène.*

**Rappels.** Ton le plus froid et le plus visuel de Moscou : hologrammes, vitres, enchères polies. L'AMIF reste hors écran. Orlan avoue travailler pour « quelqu'un au-dessus » sans le nommer.

### Variantes d'ouverture du chapitre

| Parcours | Phrase ajoutée à l'ouverture |
|---|---|
| Agent M (acte I) | *Elle a grandi à l'ombre de ces tours. Elle n'en avait jamais vu l'intérieur.* |
| Agent D (acte III) | *Il a vu des gangs cacher leur pouvoir. Ici, il l'expose, comme une vitrine.* |

### Correspondance slots → cases

| Zone | Graphe | Entrée (`intro.*`) | Élite (`elite.*`) | Fin |
|---|---|---|---|---|
| `zone_mos_3_1` | C | n04 (`intro`) | n14 | n22 (`exit`) |
| `zone_mos_3_2` | A | n03 (`intro`) | n14 | n19 (`exit`) |
| `zone_mos_3_3` | B | n04 (`intro`) | n14 | n18 (`exit`) |
| `zone_mos_3_4` | D | n03 (`intro`) | n13 | n21 (`boss.*`, `chapter_end`) |

*Règle (plateaux du code, session « Plateaux de jeux New York et Moscou », 2026-11-16) : une narration est attachée à trois cases par zone : la **case d'entrée** (`intro`, ajoutée sur décision de l'utilisateur du 2026-11-16 : `n04` dans les graphes B et C, `n03` dans les graphes A et D), l'élite fixe (`elite.before/after`) et la dernière case (`exit`, ou `boss.*` en 2.4, 3.4 et 4.4). L'ancien slot `mid` est fusionné dans `intro`.*

---

#### `zone_mos_3_1` : Tours de verre

**Fiche.** Enjeu : infiltrer les étages privés. Ton : verre, moquette, silence cher. Scène clé : un salon affiche des « trophées » : des pouvoirs en bocaux.

**Plateau (code).** Élite : Majordome (sad_majordome). Objectifs, dans l'ordre : n04 (atteindre) : Atteindre les étages privés ; n13 (mini-jeu) : Se faire passer pour un invité ; n14 (combat) : Vaincre le maître de salon ; n21 (mini-jeu) : Examiner les trophées.

**Trame.**
1. L'Aube se glisse dans une tour grâce à des badges prêtés.
2. Dans les étages privés, un salon expose des trophées sous cloche.
3. La sécurité (élite) garde l'accès.
4. Dans les bocaux, de la lumière : des pouvoirs prélevés.
5. Agent M reconnaît la teinte de ce qu'on a pris à l'Apex du laboratoire.

**Scènes.**

| Clé | Locuteur | FR |
|---|---|---|
| `narration.mos_3_1.intro.0` | narrateur | Les tours de verre se reflètent dans le Jardin. À l'intérieur, des couloirs silencieux, des ascenseurs sans bruit. |
| `narration.mos_3_1.intro.1` | giulia_rossi | Tout est trop propre. Même le silence a été nettoyé. |
| `narration.mos_3_1.intro.2` | lucas_mendes | Ce salon… il y a des bocaux. Avec de la lumière dedans. |
| `narration.mos_3_1.intro.3` | agent_m | Ce ne sont pas des bocaux. Ce sont des trophées. |
| `narration.mos_3_1.elite.before.0` | enforcer | Les invités sont priés de rester au salon d'accueil. |
| `narration.mos_3_1.elite.before.1` | ilona_varga | Il sourit pendant qu'il bloque la porte. Le sourire fait partie du service. |
| `narration.mos_3_1.elite.after.0` | giulia_rossi | Chaque bocal a une étiquette, un nom, un prix. |
| `narration.mos_3_1.exit.0` | agent_m | C'est la lumière qu'on a prise à l'Apex du laboratoire. Je la reconnais. |
| `narration.mos_3_1.exit.1` | ilona_varga | Ils l'exposent comme un tableau. On monte aux enchères. |

**Variantes.**
- **Agent M** : `intro.3` par elle. **Agent D** : `narration.mos_3_1.intro.3.d` — `agent_d` : « On expose des gens comme on expose des tableaux. Quelqu'un en tire de la fierté. »
- **Agent M, indice cathédrale** : `narration.mos_3_1.exit.2` — agent_m : « Même parfum de cire dans cet ascenseur. Quelqu'un laisse sa signature partout. »

**Fragment.** `mos_towers_trophies` — **Des trophées en bocaux** : *Sous des cloches de verre, des lumières attendaient des acheteurs. Chaque bocal avait une étiquette, un nom, un prix. Tu as compris que le pouvoir de Moscou n'était pas celui des Apex, mais celui de ceux qui les collectionnent.*

**QG.** Archive : *Dans les tours de verre de Sadovoye Koltso, l'Aube découvre un salon de trophées : des pouvoirs d'Apex conservés en bocaux et exposés à la vente.* Fil : *Les enchères se tiennent à l'étage inférieur.*

---

#### `zone_mos_3_2` : Enchères privées

**Fiche.** Enjeu : perturber la vente. Ton : salle feutrée, enchérisseurs masqués, commissaire poli. Scène clé : un Apex est mis aux enchères sous les yeux de l'Aube.

**Plateau (code).** Élite : Homme de main (mos_enforcer). Objectifs, dans l'ordre : n03 (atteindre) : Entrer dans la salle des ventes ; n13 (mini-jeu) : Perturber les enchères ; n14 (combat) : Briser la garde du lot ; n19 (atteindre) : Libérer l'Apex mis en vente.

**Trame.**
1. L'Aube se mêle aux invités d'une vente privée.
2. Le commissaire annonce le lot suivant : un Apex éveillé, présenté comme une pièce rare.
3. L'élite surveille les enchérisseurs et les issues.
4. L'Aube perturbe la vente et libère le lot.
5. Le commissaire n'élève jamais la voix : « La vente reprend. »

**Scènes.**

| Clé | Locuteur | FR |
|---|---|---|
| `narration.mos_3_2.intro.0` | narrateur | La salle est tamisée. Des enchérisseurs masqués lèvent un doigt, jamais la voix. |
| `narration.mos_3_2.intro.1` | commissaire | Mesdames, messieurs, le lot suivant : un Apex éveillé, de rang cinq, en excellent état. |
| `narration.mos_3_2.intro.2` | giulia_rossi | Un lot. Ils disent « lot ». |
| `narration.mos_3_2.intro.3` | agent_m | Je vois l'homme qu'ils présentent, sa vie avant la vente. On le vend avec son passé. |
| `narration.mos_3_2.elite.before.0` | enforcer | La vente est privée. Veuillez regagner vos places. |
| `narration.mos_3_2.elite.before.1` | ilona_varga | Il ne menace pas. Il demande poliment. C'est pire. |
| `narration.mos_3_2.elite.after.0` | lucas_mendes | Les issues sont à nous. Le lot est libre. |
| `narration.mos_3_2.exit.0` | commissaire | La vente reprend, mesdames, messieurs. Un incident sans importance. |
| `narration.mos_3_2.exit.1` | ilona_varga | Un incident sans importance. Pour eux, nous sommes déjà oubliés. |

**Variantes.**
- **Agent M** : `intro.3` par elle (vision). **Agent D** : `narration.mos_3_2.intro.3.d` — `agent_d` : « Je dessinerais bien un barreau devant chaque lot. Mais ce n'est pas encore le moment. »

**Fragment.** `mos_auction` — **Un lot** : *Ils disaient « lot » comme on dit « tableau ». Personne dans la salle n'a élevé la voix. Tu as compris que la plus grande cruauté ne crie jamais, elle enchérit.*

**QG.** Archive : *Aux enchères privées de Sadovoye, l'Aube perturbe la vente d'un Apex et libère le lot sous les yeux des enchérisseurs.* Fil : *Un studio d'hologrammes prépare une réponse.*

---

#### `zone_mos_3_3` : Studio d'hologrammes

**Fiche.** Enjeu : démonter une fausse vidéo. Ton : plateau noir, projecteurs, visages synthétiques. Scène clé : la vidéo montre The Dawn attaquant des civils.

**Plateau (code).** Élite : Homme de main (mos_enforcer). Objectifs, dans l'ordre : n04 (atteindre) : Atteindre la régie ; n11 (mini-jeu) : Désynchroniser les projecteurs ; n14 (combat) : Neutraliser les opérateurs ; n18 (atteindre) : Démonter la fausse vidéo.

**Trame.**
1. Un studio d'hologrammes prépare une vidéo montrant l'Aube attaquant des civils.
2. L'Aube arrive avant la diffusion.
3. Les techniciens (élite) protègent les serveurs.
4. L'Aube démonte la vidéo et récupère les enregistrements d'origine.
5. La vidéo est démentie, mais elle a déjà circulé.

**Scènes.**

| Clé | Locuteur | FR |
|---|---|---|
| `narration.mos_3_3.intro.0` | narrateur | Le studio est un plateau noir. Des projecteurs dessinent des visages qui n'existent pas. |
| `narration.mos_3_3.intro.1` | technicien_holo | Dans une heure, la ville verra l'Aube frapper des civils. Elle aura tout vu, et rien vécu. |
| `narration.mos_3_3.intro.2` | lucas_mendes | Ils ont copié nos silhouettes, nos insignes, nos mouvements. |
| `narration.mos_3_3.intro.3` | agent_m | Je vois la vidéo avant qu'elle n'existe. Quelqu'un l'a répétée cent fois. |
| `narration.mos_3_3.elite.before.0` | enforcer | Les serveurs sont sous protection. Vous ne verrez que ce que nous diffusons. |
| `narration.mos_3_3.elite.after.0` | giulia_rossi | Les enregistrements originaux sont là. Chaque plan truqué est daté. |
| `narration.mos_3_3.exit.0` | ilona_varga | On démonte la vidéo. Avec ses dates, elle prouve elle-même qu'elle est fausse. |
| `narration.mos_3_3.exit.1` | agent_m | Mais elle a déjà circulé. Quelque part, quelqu'un la croit. |

**Variantes.**
- **Agent M** : `intro.3` par elle. **Agent D** : `narration.mos_3_3.intro.3.d` — `agent_d` : « Ils ont copié mes lignes, mes couleurs, mes habitudes. Même mes erreurs. Quelqu'un étudie. »

**Fragment.** `mos_holo_studio` — **La fausse vidéo** : *Ils t'avaient filmée en train de faire ce que tu n'avais jamais fait. Chaque geste était parfait, chaque ombre exacte. Tu as compris que le mensonge le plus dangereux est celui qui te ressemble.*

**QG.** Archive : *Au studio d'hologrammes, l'Aube démonte une fausse vidéo où elle attaquerait des civils. Le démenti est publié, mais la rumeur circule.* Fil : *La tour Orlan domine l'anneau.*

---

#### `zone_mos_3_4` : Tour Orlan

**Fiche.** Enjeu : boss, Dmitri Orlan. Ton : étage panoramique, hologrammes de la ville à ses pieds. Orlan est poli, froid, sûr de lui. Il avoue travailler pour « quelqu'un au-dessus ». Phases : « Dernière enchère » (sous 66 %) et « Liquidation » (sous 33 %).

**Plateau (code).** Élite : Majordome (sad_majordome). Boss : DMITRI ORLAN. Objectifs, dans l'ordre : n11 (mini-jeu) : Ouvrir les coffres de la tour ; n13 (combat) : Briser la sûreté de la tour ; n20 (mini-jeu) : Forcer le bureau d'Orlan ; n21 (combat) : Vaincre Dmitri Orlan.

**Trame.**
1. L'Aube monte à l'étage d'Orlan.
2. La garde de la tour (élite) tombe.
3. Orlan accueille l'Aube comme un hôte parfait.
4. Combat.
5. Défait, il avoue qu'il n'est pas le dernier maillon.

**Scènes.**

| Clé | Locuteur | FR |
|---|---|---|
| `narration.mos_3_4.intro.0` | narrateur | L'étage d'Orlan est une vitre sur toute la ville. Les hologrammes de Moscou tournent à ses pieds comme des jouets. |
| `narration.mos_3_4.intro.1` | giulia_rossi | Il nous regarde arriver depuis un étage. Il a tout vu avant nous. |
| `narration.mos_3_4.elite.before.0` | enforcer | Monsieur Orlan reçoit sur rendez-vous. Je regrette. |
| `narration.mos_3_4.elite.after.0` | ilona_varga | La porte est ouverte. Il nous attend. |
| `narration.mos_3_4.boss.before.0` | orlan | Bienvenue. Vous avez vu mes trophées, mes enchères, mes caméras. Je suis honoré de votre curiosité. |
| `narration.mos_3_4.boss.before.1` | orlan | Moscou vous a accueillis, mademoiselle. Je n'ai fait que vous offrir un spectacle à la hauteur. |
| `narration.mos_3_4.boss.before.2` | agent_m | Des gens ne sont pas un spectacle. |
| `narration.mos_3_4.boss.before.3` | orlan | Tout est un spectacle, mademoiselle. La seule question est de savoir qui tient la salle. |
| `narration.mos_3_4.boss.phase1.0` | orlan | Dernière enchère. Je vous laisse la mise. |
| `narration.mos_3_4.boss.phase2.0` | orlan | Liquidation. Tout doit partir, moi compris. |
| `narration.mos_3_4.boss.after.0` | orlan | Vous croyez m'avoir démasqué. Je ne suis qu'un exécutant, comme tant d'autres. |
| `narration.mos_3_4.boss.after.1` | agent_m | Pour qui ? |
| `narration.mos_3_4.boss.after.2` | orlan | Pour quelqu'un au-dessus. Quelqu'un qui n'a jamais eu besoin de signer. |
| `narration.mos_3_4.chapter_end.0` | narrateur | La fausse vidéo est démentie, mais la rumeur circule. Orlan a avoué : quelqu'un tire encore les ficelles, plus haut. |

**Variantes.**
- **Agent M** : `narration.mos_3_4.boss.before.1` (base, déjà adressée à elle) pour Agent M ; `boss.before.2` est dit par elle. **Agent D** : `narration.mos_3_4.boss.before.1.d` — orlan : « Moscou vous a accueillis, monsieur. » ; `narration.mos_3_4.boss.before.2.d` — `agent_d` : « Des gens ne sont pas un spectacle. »
- **Agent M, indice cathédrale** : `narration.mos_3_4.boss.after.3` — orlan : « Quelqu'un qui aime l'encens et la cire. Un homme d'ordre, mademoiselle. » *(sens révélé au chapitre 4)*.

**Fragment.** `mos_orlan_tower` — **Quelqu'un au-dessus** : *Il t'a reçu comme un hôte parfait, dans une tour qui dominait la ville. Quand il a avoué ne pas être le dernier maillon, tu as compris que le mensonge de Moscou n'avait pas qu'un étage.*

**QG.** Archive : *À la tour Orlan, l'Aube vainc l'oligarque. Il avoue travailler pour « quelqu'un au-dessus ». La fausse vidéo est démentie, mais la rumeur circule.* Fil : *Direction la Place Rouge.*

---

## Chapitre 4 : La Place Rouge

Id : `mos_place_rouge`. **Face-à-face et choix de confiance.** Acte I pour Agent M, acte III pour Agent D.

**Texte d'ouverture (existant)** : *La Place Rouge, le Kremlin, le couvre-feu : le dernier chapitre de Moscou est un face-à-face. L'Aube doit décider si elle fait confiance à ceux qui l'accueillent, ou à ceux qui la manipulent.*

**Rappels.** Premier chapitre où **l'AMIF paraît à l'écran** (un envoyé en civil, 4.3). Le Conseiller (Arkadi Zorin) orchestre le complot de la faction interne. Le choix de 4.4 (arrêter ou écouter) **ne modifie que l'épilogue**. Dans les deux cas, les salles soviétiques mènent à Salem. Les indices de la cathédrale (cire, encens, cierges) trouvent ici leur sens.

### Variantes d'ouverture du chapitre

| Parcours | Phrase ajoutée à l'ouverture |
|---|---|
| Agent M (acte I) | *Elle revient à la ville qu'elle croyait connaître, sur la place où tout a commencé pour elle.* |
| Agent D (acte III) | *Il arrive sur la place où un État décide de ce qu'on voit. Il a déjà vu ce que cache une vitrine.* |

### Correspondance slots → cases

| Zone | Graphe | Entrée (`intro.*`) | Élite (`elite.*`) | Fin |
|---|---|---|---|---|
| `zone_mos_4_1` | D | n03 (`intro`) | n13 | n21 (`exit`) |
| `zone_mos_4_2` | B | n04 (`intro`) | n14 | n18 (`exit`) |
| `zone_mos_4_3` | C | n04 (`intro`) | n14 | n22 (`exit`) |
| `zone_mos_4_4` | B | n04 (`intro`) | n14 | n18 (`boss.*`, puis `choice` et `branch_*` après la dernière `boss.after.*` jouée ; `boss.after.1` n'existe qu'en `.m` et `.d`, sans base ; `chapter_end`, épilogues) |

*Règle (plateaux du code, session « Plateaux de jeux New York et Moscou », 2026-11-16) : une narration est attachée à trois cases par zone : la **case d'entrée** (`intro`, ajoutée sur décision de l'utilisateur du 2026-11-16 : `n04` dans les graphes B et C, `n03` dans les graphes A et D), l'élite fixe (`elite.before/after`) et la dernière case (`exit`, ou `boss.*` en 2.4, 3.4 et 4.4). L'ancien slot `mid` est fusionné dans `intro`.*

---

#### `zone_mos_4_1` : Rues sous couvre-feu

**Fiche.** Enjeu : protéger les civils bloqués. Ton : rues vides, patrouilles, volets fermés. Scène clé : des forces spéciales ferment les rues, sans ordre officiel.

**Plateau (code).** Élite : Commandant de la garde (pr_commandant). Objectifs, dans l'ordre : n11 (mini-jeu) : Dégager les civils bloqués ; n13 (combat) : Briser le cordon des forces spéciales ; n20 (mini-jeu) : Tenir le barrage ; n21 (atteindre) : Atteindre la grille des tunnels.

**Trame.**
1. Le couvre-feu tombe sans annonce. Des civils sont bloqués dehors.
2. Des forces spéciales sans insigne ferment les rues.
3. Leur chef (élite) dit n'avoir reçu aucun ordre écrit.
4. L'Aube ouvre un passage et met les civils à l'abri.
5. Un homme en civil, badge dissimulé, observe de loin : première apparition d'un envoyé de l'AMIF.

**Scènes.**

| Clé | Locuteur | FR |
|---|---|---|
| `narration.mos_4_1.intro.0` | narrateur | Le couvre-feu tombe sans annonce. Les volets se ferment, les rues se vident, et des civils restent dehors. |
| `narration.mos_4_1.intro.1` | giulia_rossi | Aucune sirène, aucun avis. Juste des patrouilles qui ne portent pas d'insigne. |
| `narration.mos_4_1.intro.2` | civil_moscou | Ils nous ont dit de rentrer. Mais la porte est de l'autre côté de leurs barrages. |
| `narration.mos_4_1.intro.3` | agent_m | On vous ouvre un passage. Suivez-nous, ne courez pas. |
| `narration.mos_4_1.elite.before.0` | enforcer | Ordre de fermeture. Pas d'ordre écrit, mais un ordre. |
| `narration.mos_4_1.elite.before.1` | ilona_varga | Il ne sait pas qui l'a donné. Il obéit quand même. |
| `narration.mos_4_1.elite.after.0` | lucas_mendes | Les civils sont à l'abri. Les barrages sont levés. |
| `narration.mos_4_1.exit.0` | giulia_rossi | Quelqu'un nous regarde, là-bas, sous le porche. Pas de costume, pas d'insigne. |
| `narration.mos_4_1.exit.1` | ilona_varga | Il a un badge dissimulé sous son manteau. Je connais ce sigle. |
| `narration.mos_4_1.exit.2` | agent_m | L'AMIF. Elle était à Moscou depuis le début : elle ne s'est montrée qu'ici. |

**Variantes.**
- **Agent M** : `intro.3` comme écrit. **Agent D** : `narration.mos_4_1.intro.3.d` — `agent_d` : « Suivez-moi. Je trace le chemin. Ne regardez pas les barrages. »
- **Agent D, reconnaissance** : `narration.mos_4_1.exit.3` — agent_d : « New York avait Stroke. Ici, l'AMIF reste dans l'ombre d'un État. C'est plus patient. »

**Fragment.** `mos_curfew` — **Sans ordre officiel** : *Les rues se fermaient sans ordre, et personne n'osait demander qui les fermait. Tu as vu un homme sous un porche, un badge caché sous un manteau. Moscou avait appris à obéir à des ordres qui n'existent pas.*

**QG.** Archive : *Sous le couvre-feu de la Place Rouge, l'Aube protège des civils bloqués et repère un envoyé de l'AMIF en civil.* Fil : *Les tunnels sous le Kremlin mènent au conseil.*

---

#### `zone_mos_4_2` : Tunnels sous le Kremlin

**Fiche.** Enjeu : atteindre le conseil. Ton : pierre humide, câbles anciens, silence. Scène clé : le tunnel contient d'anciennes salles de recherche soviétiques.

**Plateau (code).** Élite : Commandant de la garde (pr_commandant). Objectifs, dans l'ordre : n04 (atteindre) : Descendre sous le Kremlin ; n11 (mini-jeu) : Traverser les salles de recherche ; n14 (combat) : Vaincre le gardien des tunnels ; n18 (atteindre) : Atteindre le conseil.

**Trame.**
1. L'Aube descend dans les tunnels sous le Kremlin.
2. Elle traverse d'anciennes salles de recherche soviétiques.
3. Des gardes (élite) défendent les accès.
4. Dans les salles, des planches d'anatomie, des dossiers « Khost », des cages.
5. Agent M reconnaît la cire et l'encens : l'odeur de la cathédrale. Une indication mène vers Salem.

**Scènes.**

| Clé | Locuteur | FR |
|---|---|---|
| `narration.mos_4_2.intro.0` | narrateur | Sous le Kremlin, les tunnels sentent la pierre mouillée et la cire. Des câbles anciens courent le long des murs. |
| `narration.mos_4_2.intro.1` | lucas_mendes | Ces tunnels datent d'avant l'Union. Quelqu'un les entretient. |
| `narration.mos_4_2.intro.2` | giulia_rossi | Des salles de recherche. Des cages. Des planches d'anatomie… Des Apex, avant même que le mot existe. |
| `narration.mos_4_2.intro.3` | agent_m | Je vois les mains qui ont travaillé ici. Elles écrivent comme dans les marges du laboratoire. |
| `narration.mos_4_2.elite.before.0` | enforcer | Le conseil vous attend. Personne n'a dit qu'il vous attendait entier. |
| `narration.mos_4_2.elite.after.0` | ilona_varga | Les accès sont libres. Le conseil est au bout. |
| `narration.mos_4_2.exit.0` | giulia_rossi | Un mot revient sur les dossiers : « Khost ». |
| `narration.mos_4_2.exit.1` | ilona_varga | Je ne sais pas ce que c'est, mais ces dossiers parlent d'un endroit loin d'ici. Salem. |
| `narration.mos_4_2.exit.2` | agent_m | Alors tout ça, c'est le début d'autre chose. |

**Variantes.**
- **Agent M** : `intro.3` par elle. **Agent D** : `narration.mos_4_2.intro.3.d` — `agent_d` : « Ces cages ont la taille d'un homme. Je préfère ne pas dessiner ce qu'il y avait dedans. »
- **Agent M, indice cathédrale** : `narration.mos_4_2.intro.4` — agent_m : « Cette odeur… cire, encens, pierre froide. C'est celle de la cathédrale, avant l'explosion. Je ne comprends pas. »

**Fragment.** `mos_kremlin_tunnels` — **Sous le Kremlin** : *Les tunnels sentaient la cire, comme une église enterrée. Dans les salles de recherche, les mots en marge étaient les mêmes que ceux du laboratoire. Tu as compris que l'histoire de Moscou ne finissait pas à ses murs.*

**QG.** Archive : *Sous le Kremlin, l'Aube traverse d'anciennes salles de recherche soviétiques. Les dossiers portent un nom : « Khost ». La piste mène à Salem.* Fil : *Le conseil attend dans la salle du fond.*

---

#### `zone_mos_4_3` : Salle du conseil

**Fiche.** Enjeu : entendre l'offre du Conseiller. Ton : salle ronde, boiseries, lumière basse. Scène clé : il propose une alliance contre l'AMIF, en échange de la « neutralité » de The Dawn.

**Plateau (code).** Élite : Capitaine de l'AMIF (amif_captain). Objectifs, dans l'ordre : n04 (atteindre) : Traverser l'antichambre ; n13 (mini-jeu) : Tenir sa place à la table ; n14 (combat) : Repousser l'équipe de l'AMIF ; n22 (atteindre) : Entendre l'offre du Conseiller.

**Trame.**
1. L'Aube pénètre dans la salle du conseil : table ronde, chaises vides, un seul homme debout.
2. Le Conseiller (Arkadi Zorin) l'accueille avec courtoisie.
3. L'élite de garde (cercle du pouvoir) se tient prête.
4. Il propose une alliance contre l'AMIF, à condition que l'Aube reste neutre dans les affaires de Moscou.
5. L'envoyé de l'AMIF, présent dans l'ombre, est démasqué.

**Scènes.**

| Clé | Locuteur | FR |
|---|---|---|
| `narration.mos_4_3.intro.0` | narrateur | La salle du conseil est ronde, boisée, silencieuse. Une seule chaise est occupée. |
| `narration.mos_4_3.intro.1` | conseiller | Soyez les bienvenus. J'ai longtemps attendu une visite aussi précise. |
| `narration.mos_4_3.intro.2` | conseiller | L'AMIF vous pourchasse. Elle prélève et revend vos semblables. Je peux la chasser de cette ville. |
| `narration.mos_4_3.intro.3` | conseiller | En échange de votre neutralité. Laissez Moscou régler ses affaires, et l'Aube sera libre de partir. |
| `narration.mos_4_3.intro.4` | ilona_varga | Il parle de neutralité. Il parle de liberté. Il ne parle ni de ses trophées ni de ses laboratoires. |
| `narration.mos_4_3.elite.before.0` | agent_amif | Il est temps de vous retirer. Ce dossier n'est pas le vôtre. |
| `narration.mos_4_3.elite.before.1` | conseiller | Vous n'avez pas été invité, monsieur. Mes hommes vont vous raccompagner. |
| `narration.mos_4_3.elite.after.0` | giulia_rossi | L'envoyé est tombé. L'AMIF s'est montrée, et elle a perdu. |
| `narration.mos_4_3.exit.0` | conseiller | Vous voyez : je tiens mes promesses. Reste la vôtre. |
| `narration.mos_4_3.exit.1` | agent_m | Une promesse à un homme qui se tient derrière tout ça. |

**Variantes.**
- **Agent M** : `narration.mos_4_3.intro.1.m` — conseiller : « Mademoiselle, je vous attendais surtout vous. » **Agent D** : `intro.1` comme écrit.
- **Agent M, indice cathédrale** : `narration.mos_4_3.intro.5` — agent_m : « La cire sur ses mains. Le même parfum. C'est lui. »

**Fragment.** `mos_council_offer` — **L'offre** : *Il t'a offert la fin de l'AMIF en échange de ton silence. Il n'a pas élevé la voix. Tu as compris qu'on ne ment pas toujours : parfois, on propose seulement ce qui arrange.*

**QG.** Archive : *À la salle du conseil, le Conseiller offre à l'Aube une alliance contre l'AMIF, en échange de sa neutralité. Un envoyé de l'AMIF est démasqué.* Fil : *Le Conseiller attend la réponse.*

---

#### `zone_mos_4_4` : Salle du conseil, le Conseiller

**Fiche.** Enjeu : boss, Arkadi Zorin. Ton : calme absolu, courtoisie glacée. **Choix** après le combat : l'arrêter ou l'écouter. Pour Agent M, il révèle que l'attentat de la cathédrale était orchestré. Phases : « Dernière offre » (sous 66 %) et « Réalité nécessaire » (sous 33 %).

**Plateau (code).** Élite : Commandant de la garde (pr_commandant). Boss : LE CONSEILLER. Objectifs, dans l'ordre : n11 (mini-jeu) : Consulter le dossier du conseil ; n14 (combat) : Briser la garde personnelle ; n18 (combat) : Vaincre le Conseiller.

**Trame.**
1. L'Aube refuse la neutralité. Le Conseiller regrette, sans colère.
2. Combat.
3. Défait, il demande un dernier mot : **choix** du joueur.
4. **Arrêter** : il est livré aux autorités ; **écouter** : il livre ce qu'il sait de l'AMIF et de Salem.
5. Dans les deux cas, les salles soviétiques mènent à Salem.

**Scènes.**

| Clé | Locuteur | FR |
|---|---|---|
| `narration.mos_4_4.intro.0` | narrateur | La salle du conseil se vide. Les gardes reculent. Le Conseiller reste seul à la table. |
| `narration.mos_4_4.elite.before.0` | enforcer | Le Conseiller ne vous ferait pas de mal. Pas lui. Il délègue. |
| `narration.mos_4_4.elite.after.0` | ilona_varga | Ses derniers hommes sont tombés. Il n'a plus que ses mots. |
| `narration.mos_4_4.boss.before.0` | conseiller | Vous refusez. C'est un choix, et je le respecte. Je ne suis pas un homme qui insiste. |
| `narration.mos_4_4.boss.before.1` | conseiller | Moscou n'a pas besoin de héros. Elle a besoin d'ordre. Vous êtes un accident qui ne sait pas qu'il en est un. |
| `narration.mos_4_4.boss.before.2` | agent_m | Un accident ? Les gens que vous avez vendus n'étaient pas un accident. |
| `narration.mos_4_4.boss.before.3` | conseiller | Ils étaient une ressource. La ville avait faim. |
| `narration.mos_4_4.boss.phase1.0` | conseiller | Dernière offre. Elle n'est plus négociable. |
| `narration.mos_4_4.boss.phase2.0` | conseiller | Réalité nécessaire. Je regrette qu'on en arrive là. |
| `narration.mos_4_4.boss.after.0` | conseiller | Vous avez gagné. Je demande seulement quelques mots avant que vous décidiez de moi. |
| `narration.mos_4_4.choice.0` | ilona_varga | Tu peux l'arrêter, ou l'écouter. Quoi que tu choisisses, Moscou en gardera le souvenir. |
| `narration.mos_4_4.choice.1` | narrateur | Choix : arrêter le Conseiller, ou l'écouter. |
| `narration.mos_4_4.branch_arrest.0` | agent_m | Assez. Vous répondrez de vos actes devant ceux que vous avez trahis. |
| `narration.mos_4_4.branch_arrest.1` | conseiller | Je répondrai. Mais ils ne sauront jamais tout. |
| `narration.mos_4_4.branch_listen.0` | agent_m | Dites. Je vous écoute. |
| `narration.mos_4_4.branch_listen.1` | conseiller | L'AMIF a un projet, mademoiselle. À Salem, quelque chose se prépare. Mes recherches y mènent. Je n'en suis pas l'auteur. |
| `narration.mos_4_4.chapter_end.0` | narrateur | Le Conseiller est vaincu. Les salles soviétiques s'ouvrent vers un passage ancien : il mène à Salem. |

**Variantes (révélation de la cathédrale).**
- **Agent M** : `narration.mos_4_4.boss.after.1.m` — conseiller : « Vous voulez savoir pourquoi la cathédrale ? Il fallait un choc. Un vrai. J'avais besoin d'un Apex qui apprendrait à voir. Votre frère devait être le prix. » ; `after.2` — agent_m : « Ilia… Vous l'avez blessé pour m'éveiller. » ; `after.3` — agent_m : « Alors cette nuit-là n'était pas un hasard. Vous l'aviez prévue. »
- **Agent M, flashback** : `narration.mos_4_4.boss.after.4` — ilia : « Mara… je suis là. Je suis toujours là. » *(voix off, brève ; Ilia est vivant mais blessé)*.
- **Agent D** : `narration.mos_4_4.boss.after.1.d` — conseiller : « Vous êtes venu de loin pour comprendre un système. J'aurais pu vous expliquer, mais personne ne veut jamais entendre. » *(pas de révélation personnelle ; la scène reste politique)*.
- **Cire, encens, cierges** (payoff pour Agent M) : `narration.mos_4_4.boss.after.5` — conseiller : « Mes hommes marquent leurs caisses à la cire des cierges. Une vieille habitude d'église. Elle m'a trahi plus d'une fois. »

**Fragment.** `mos_place_rouge_choice` — **L'homme de l'ordre** : *Il ne criait pas. Il parlait d'ordre, de ressources, de faim. Quand tu as compris que ta vie avait été un moyen, tu as eu le choix : le livrer ou l'entendre. Les deux se paient.*

**QG.** Archive : *Dans la salle du conseil, l'Aube vainc le Conseiller. Le choix de l'arrêter ou de l'écouter façonne l'épilogue. Les salles soviétiques mènent à Salem.* Fil : *Direction Salem.*

---

### Épilogues du chapitre (selon le choix de 4.4)

Joués après `chapter_end`. Dans les deux cas, le passage vers Salem s'ouvre.

**Si le Conseiller a été arrêté** : il est remis aux autorités ; la faction interne se retourne contre l'Aube.

| Clé | Locuteur | FR |
|---|---|---|
| `narration.mos_4.epilogue.arrest.0` | narrateur | Au matin, le Conseiller est remis aux autorités. Le cercle du pouvoir se fige, puis se retourne contre l'Aube. |
| `narration.mos_4.epilogue.arrest.1` | ilona_varga | Nous avons fait ce qui devait l'être. Moscou ne nous le pardonnera pas de sitôt. |

**Si le Conseiller a été écouté** : il livre ce qu'il sait, mais l'Aube lui doit une dette.

| Clé | Locuteur | FR |
|---|---|---|
| `narration.mos_4.epilogue.listen.0` | narrateur | Au matin, le Conseiller disparaît, laissant derrière lui des noms et une piste. L'Aube est libre, mais elle lui doit quelque chose. |
| `narration.mos_4.epilogue.listen.1` | ilona_varga | Nous savons enfin ce qui se joue à Salem. Reste à savoir ce que ça nous coûtera. |

**Fin commune** (dans les deux cas) :

| Clé | Locuteur | FR |
|---|---|---|
| `narration.mos_4.epilogue.common.0` | giulia_rossi | Un passage sous les salles soviétiques. Il mène plus loin que Moscou. |
| `narration.mos_4.epilogue.common.1` | ilona_varga | Salem. C'est là que tout se rejoint. |

---

## Décision : slots `intro` et `mid` (2026-11-16)

L'utilisateur a choisi d'**ajouter une case d'entrée à narration par zone** (`n04` dans les graphes B et C, `n03` dans les graphes A et D). Le slot `mid` n'existe plus : ses répliques sont fusionnées dans `intro` (une seule scène d'entrée, renumérotée). L'ajout de ces cases est à faire côté plateaux par la session « Plateaux de jeux New York et Moscou ».
