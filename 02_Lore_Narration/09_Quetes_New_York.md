# New York : « la ville vitrine »

*Date : 2026-11-09 · Acte I pour Agent D, acte III pour Agent M. Bible : `08_Bible_Narrative.md`.*

## Thèse

The Dawn prouve que les Apex sont des gens comme les autres, et chaque victoire attire l'AMIF. Le joueur découvre peu à peu que l'AMIF et les gangs ne se combattent pas : ils se partagent la ville.

Progression de la révélation : Brooklyn (soupçon), Queens (preuve), Harlem (coût humain), Manhattan (face-à-face).

## Antagonistes

- **Général Stroke** (AMIF) : présent à Brooklyn (par son Titan) et à Manhattan (boss).
- **Crocodile** (Antonio Kribelchi) : mafieux méta-humain, boss de Coney Island. Écarté par l'AMIF à la fin de Brooklyn.
- **Le Syndicat** : fait régner la peur à Queens.
- **Le Régisseur** *(à valider)* : intendant du Syndicat, boss de Queens.
- **Le chef du gang de Harlem** *(à valider)* : celui qui a tué l'ami d'enfance d'Agent D.

## Cadrage selon le parcours

- **Acte I (Agent D)** : le joueur découvre la ville ; les textes d'ouverture sont ceux ci-dessous.
- **Acte III (Agent M)** : le joueur a vu à Moscou comment l'AMIF s'allie aux mafias. Chaque ouverture de chapitre ajoute une phrase de reconnaissance (« Elle a déjà vu ce schéma à Moscou. »). Pas d'autre changement de zones.
- **Harlem** est le quartier d'Agent D : briefing et scène de fin personnalisés pour lui. Pour Agent M, la même scène est jouée avec un habitant du quartier.

---

## Chapitre 1 : Brooklyn *(déjà jouable)*

Id : `ny_brooklyn`. Zones existantes (voir `Code/docs/specs/2026-10-12-campagne-brooklyn-design.md`).

| Zone | Lieu | Objectifs principaux |
|---|---|---|
| 1.1 `zone_ny_1_1` | Red Hook | Infiltrer le quai, couper le courant, trouver les documents de transport, arrêter l'échange (Brick), fuir |
| 1.2 `zone_ny_1_2` | Brownsville | Neutraliser le lieutenant de Crocodile, libérer les otages, les mettre à l'abri |
| 1.3 `zone_ny_1_3` | Coney Island | Évacuer les civils, briser le piège, vaincre Crocodile |
| 1.4 `zone_ny_1_4` | Dumbo et pont de Brooklyn | Percer le cordon de l'AMIF, couper la surveillance, tenir le pont face au Titan |

**Temps fort :** Rayner menace Stroke, qui arrête Crocodile et avertit Rayner des Accords de Salem. L'AMIF écarte un gang devenu gênant.

**Texte d'ouverture (existant)** : *Red Hook, Gowanus, Dumbo : Brooklyn est le premier front. L'AMIF et les gangs, qui prétendent se combattre, se partagent la ville en secret. L'Aube y fait ses preuves, mais chaque succès attire un peu plus l'attention de l'AMIF.*

---

## Chapitre 2 : Queens

Id : `ny_queens`. **Preuve de la collusion.**

**Texte d'ouverture (existant)** : *Flushing, Astoria, Jamaica : dans le Queens, le Syndicat fait régner la peur sur les marchés et les gares de triage. Des captifs, des indics, des caméras : l'Aube découvre que le Syndicat n'agit pas seul.*

| Zone | Lieu | Enjeu | Scène clé |
|---|---|---|---|
| 2.1 `zone_ny_2_1` | Flushing, marché sous caméras | Repérer les caméras du Syndicat, les couper sans alerter les commerçants | Les caméras portent le sigle d'un prestataire de l'AMIF |
| 2.2 `zone_ny_2_2` | Astoria, les indics | Retourner un indic sans le mettre en danger | L'indic avoue que le Syndicat remet des listes d'Apex à l'AMIF |
| 2.3 `zone_ny_2_3` | Triage de Jamaica, convoi | Intercepter un convoi de captifs | Les captifs sont des Apex éveillés, destinés à l'AMIF |
| 2.4 `zone_ny_2_4` | Terminal de fret | Boss : le Régisseur | Le registre de transport prouve la collusion : **preuve n°1** |

**Fin du chapitre :** les captifs sont libérés. Le Régisseur menace : « Vous croyez abattre un gang. Vous venez de déranger un contrat. »

---

## Chapitre 3 : Harlem

Id : `ny_harlem`. **Coût humain et choix marquant.**

**Texte d'ouverture (reformulé)** : *À Harlem, le blackout a laissé des cicatrices que les gangs exploitent. Des blocs brûlent, des familles fuient. L'Aube doit choisir qui protéger en premier, et à quel prix.*

| Zone | Lieu | Enjeu | Scène clé |
|---|---|---|---|
| 3.1 `zone_ny_3_1` | Bloc incendié | Évacuer les habitants avant l'effondrement | Un gang a mis le feu pour chasser les locataires |
| 3.2 `zone_ny_3_2` | Clinique de fortune | Tenir la clinique face à une attaque | Le médecin refuse de partir : « Ils ne soignent qu'ici. » |
| 3.3 `zone_ny_3_3` | Marché de rue | **Choix** : défendre la clinique ou le marché en premier | La zone qui n'est pas défendue en subit les conséquences (épilogue) |
| 3.4 `zone_ny_3_4` | Rue de l'atelier | Boss : le chef du gang | Agent D retrouve le trottoir où son ami est mort |

**Fin du chapitre :** le chef tombe, mais le prix est visible : un bloc est perdu selon le choix du joueur.

---

## Chapitre 4 : Manhattan

Id : `ny_manhattan`. **Face-à-face.**

**Texte d'ouverture (existant)** : *Midtown, Times Square, le siège de l'ONU : tout converge vers Manhattan. Les cercles de pouvoir, l'AMIF et l'Aube s'y affrontent à visage découvert. C'est ici que la ville bascule.*

| Zone | Lieu | Enjeu | Scène clé |
|---|---|---|---|
| 4.1 `zone_ny_4_1` | Times Square | Reprendre les écrans à la propagande de l'AMIF | La propagande présente l'Aube comme une menace |
| 4.2 `zone_ny_4_2` | Tours de Midtown | Infiltrer les cercles de pouvoir | Les noms des financeurs de l'AMIF et du Syndicat se recoupent |
| 4.3 `zone_ny_4_3` | Siège de l'ONU | Protéger l'annonce des Accords de Salem | Rayner et Stroke s'affrontent publiquement |
| 4.4 `zone_ny_4_4` | Esplanade de l'ONU | Boss : Stroke | Stroke se retire en promettant que « Salem décidera » |

**Fin du chapitre :** les Accords sortent au grand jour. Le fil conduit à Salem.

---

## Points à valider

- Le Régisseur et le chef du gang de Harlem : noms propres à fixer.
- Le choix de Harlem (clinique ou marché) : confirmer qu'il ne modifie que l'épilogue.
- Détails des zones 2.x, 3.x et 4.x : objectifs précis (cases, mini-jeux, élites) définis lors d'un lot dédié.
