# Campagne de Brooklyn : quatre zones jouables (version 0.8.1)

## But
Rendre jouable la campagne de Brooklyn fournie par l'équipe (`02_Lore_Narration/06_Campagne_Brooklyn.md`) : quatre zones, objectifs cohérents avec l'histoire, ennemis et textes à l'unisson, déblocage linéaire.

## Les quatre zones
Le graphe de cases des zones existantes est conservé (équilibrage déjà testé) ; objectifs, ennemis, textes et narration sont réécrits. La zone 1.4 est nouvelle (reprend le graphe de l'ancienne zone du pont).

| Zone | Identifiant | Lieu | Objectifs |
|---|---|---|---|
| 1.1 | `zone_ny_1_1` | Red Hook | 1 Infiltrer le quai (atteindre une case) ; 2 Couper le courant (mini-jeu du générateur) ; 3 Trouver les documents de transport (atteindre une case) ; 4 Arrêter l'échange en cours (combat contre Brick) ; 5 Fuir le quai (atteindre la sortie). Les cinq objectifs de l'équipe, à l'identique. |
| 1.2 | `zone_ny_1_2` | Brownsville | 1 S'introduire dans le quartier ; 2 Neutraliser le lieutenant de Crocodile (élite) ; 3 Libérer les otages (mini-jeu, cellules) ; 4 Mettre les otages à l'abri (atteindre le point d'évacuation). |
| 1.3 | `zone_ny_1_3` | Coney Island | 1 Évacuer les civils pris au piège (mini-jeu) ; 2 Briser le piège des hommes de Crocodile (élite) ; 3 Vaincre Crocodile (boss). |
| 1.4 | `zone_ny_1_4` | Dumbo et pont de Brooklyn | 1 Percer le cordon de l'AMIF (élite) ; 2 Couper le réseau de surveillance de l'AMIF (mini-jeu) ; 3 Tenir le pont face au Titan de l'AMIF (boss), puis scène de Rayner. |

Déblocage linéaire : 1.2 demande 1.1, 1.3 demande 1.2, 1.4 demande 1.3.

## Ennemis
- Nouveau : **Crocodile** (Antonio Kribelchi, boss de 1.3, mafieux méta-humain), **soldat de l'AMIF** et **tireur de l'AMIF** (rencontres de 1.4).
- Renommés : « Chef du canal » → **Lieutenant de Crocodile** (élite de 1.2) ; « Titan du pont » → **Titan de l'AMIF** (boss de 1.4).

## Narration (boîte de dialogue existante)
- 1.1 : après Brick, les preuves sont là, il faut fuir.
- 1.2 : le lieutenant avertit que Crocodile se vengera ; fin : les otages sont libres.
- 1.3 : Crocodile attendait l'équipe (piège) ; fin : Coney Island est ravagée.
- 1.4 : scène de Rayner qui menace Stroke, le Général arrête Crocodile et avertit Rayner (Accords de Salem).

## Carte de la ville
Quatre points sur Brooklyn : Red Hook, Brownsville, Coney Island, Dumbo et pont de Brooklyn.

## Mise en œuvre
Les zones sont définies dans le code du prototype puis importées en assets (`ZoneBaker.ImportPrototype`) ; textes dans les fichiers de localisation ; tests de contenu mis à jour (nombre de zones, déblocages, objectifs). APK 0.8.1.

## Hors périmètre
Illustrations de fond des nouvelles zones ; équilibrage fin ; zones des autres arrondissements.
