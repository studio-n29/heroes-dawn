# Lot G — Narration : cinématique, chapitres, dossier d'agent

Date : 2026-10-27 · Version 0.14.0 · Source : PDF « N29 HQ » (playtest 0.13.0), point « La narration » (cinématique, chapitres New York et Moscou), et demande du joueur d'afficher tout le lore sur les cartes d'agents.

## Objet

1. Remplacer le texte de la cinématique de lancement par la narration du PDF.
2. Ajouter un système de **chapitres** (New York : Brooklyn, Queens, Harlem, Manhattan ; Moscou : Sergueiv, Kapotnya, Sadovoye Koltso, Place Rouge) : fenêtre d'ouverture au premier déploiement, page Chapitres.
3. Afficher le **dossier complet de chaque agent** (tout son lore) sur sa carte.

Décisions validées :

- Cinématique : nouveau texte, anciennes voix retirées ; les voix seront fournies plus tard.
- Chapitres : cadre et carte d'ouverture ; Brooklyn jouable avec ses 4 zones actuelles, les 7 autres « Bientôt ».
- Dossier d'agent : tout le lore de `02_Lore_Narration/04_Agents_The_Dawn.md` sur la fiche (section 3, ajoutée à la demande du joueur).

Hors lot : voix off enregistrées, zones des chapitres autres que Brooklyn, mini-jeux (lot H), compte et notifications (lot I).

## 1. Cinématique avec la narration du PDF

### Plans

La version courte garde 11 plans (S20, Orik Kovvolt, est retiré : le nouveau récit ne le mentionne plus) : `S01, S02, S10, S11, S15, S24, S33, S37, S41, S44, S46`. Chaque plan garde son visuel actuel (`CinematicScript.Kept` et `Build()`).

### Texte (clés `cinematic.vo.<plan>`)

| Plan | Légende |
|---|---|
| S01 | Chaque aube naît de la nuit. *(ligne existante, hors PDF)* |
| S02 | Pendant la Guerre froide, le gouvernement américain a décidé d'utiliser les recherches d'un généticien pour créer un programme d'armement secret, aussi efficace que la bombe atomique, sans le scénario de fin du monde. |
| S10 | Après plusieurs essais ratés, le monde a découvert en grande pompe l'Apex Unit : des humains génétiquement modifiés aux pouvoirs extraordinaires, qui ont mis fin à la Guerre froide en quelques mois, malgré la tentative de l'URSS d'avoir son propre méta-humain. |
| S11 | Le monde a connu une longue période de fausse paix, où l'Apex Unit était l'étendard d'une justice manipulatrice, fondée sur le rapport de force. |
| S15 | Jusqu'en 2026, où un jeune homme a réveillé ses pouvoirs enfouis, devenant le premier méta-humain naturel de l'histoire. |
| S24 | Avec son éveil, il a entraîné le monde dans une bataille contre une créature biblique, alors au rang de légende : Mephisto. |
| S33 | C'est à Paris que Mephisto s'est opposé à l'Apex Unit et a créé une brèche entre les mondes, qui allait anéantir le monde… |
| S37 | … jusqu'à ce que le jeune homme apparaisse sur le champ de bataille, déploie ses ailes et se sacrifie dans ce qu'on appellera plus tard La Supernova. |
| S41 | Le déchaînement de radiations du jeune homme, disparu dans la brèche avec Mephisto, a altéré l'ADN d'une grande partie de l'humanité et lancé une vague de méta-humains sans précédent. |
| S44 | Aujourd'hui, cinq ans plus tard, les méta-humains tentent de trouver leur place, tandis que l'Apex Unit a laissé la place à The Dawn. |
| S46 | Le monde est partagé entre acceptation, contrôle et anéantissement de ces nouveaux humains. |

Le texte exact est celui du PDF, éventuellement raccourci par la mise en forme des légendes ; `LocCinematic.cs` porte ces textes. Les clés `cinematic.vo.S20` et les anciennes légendes inutilisées disparaissent.

### Voix et durée

- Les 12 fichiers `Resources/Cinematic/Voice/vo_*.wav` (et leurs `.meta`) sont supprimés.
- Écarts actés à l'exécution : la durée totale de la cinématique n'est plus limitée à une minute (60 à 170 s au temps de lecture) ; `CinematicShot.Voice` signifie « le plan a une légende » (S46 en a une) ; le plan S20 reste dans le découpage complet mais sort de la version courte.
- Sans voix, la légende s'affiche pendant un **temps de lecture** : `max(durée minimale du plan, 1,2 s + 0,35 s × nombre de mots)`, apparition et disparition en fondu. Avec une voix (`vo_<plan>.wav` déposé plus tard), le comportement actuel est conservé : la légende suit la voix et la durée du plan s'adapte.
- `CinematicPlayer` : le sous-titre est affiché sans attendre le démarrage de la voix quand aucun clip n'existe.

### Documentation

`02_Lore_Narration/05_Cinematique_Lancement.md` : nouvelle section « Narration du PDF (version courte) » avec la table ci-dessus, qui sert aussi de script d'enregistrement des voix (nom de fichier `vo_<plan>.wav`).

## 2. Chapitres

### Données (domaine)

`Chapters` (`Definitions/Chapters.cs`) : `ChapterDef { string Id; string City; int Order; IReadOnlyList<string> Boroughs; }` et `Chapters.All` (8 chapitres, ordre par ville) :

| Id | Ville | Quartiers de zones (`ZoneDef.Borough`) |
|---|---|---|
| `ny_brooklyn` | new_york | brooklyn |
| `ny_queens` | new_york | queens |
| `ny_harlem` | new_york | harlem |
| `ny_manhattan` | new_york | manhattan |
| `mos_sergueiv` | moscow | sergueiv |
| `mos_kapotnya` | moscow | kapotnya |
| `mos_sadovoye` | moscow | sadovoye |
| `mos_place_rouge` | moscow | place_rouge |

Les chapitres sont indépendants du dessin de la carte (5 quartiers de New York dont le Bronx et Staten Island) : seule la liste des quartiers de zones les relie aux zones.

### Statut

`ChapterStatus { Done, Current, Locked, Soon }`, calculé par `ChapterProgress.Of(session, chapter)` : `Soon` si aucune zone ne porte l'un de ses quartiers ; `Done` si toutes ses zones sont validées ; sinon `Current` pour le premier chapitre non terminé de la ville de l'acte en cours (`StoryProgress.Current`), `Locked` pour les autres (chapitres des autres villes compris, sauf `Soon`).

### Ouverture d'un chapitre

- `MetaSave.chaptersSeen` (liste d'identifiants) ; un ancien profil n'a aucun chapitre vu.
- Au déploiement dans une zone d'un chapitre pas encore vu (`CityMapPagePresenter.Deploy`), une fenêtre s'affiche avant l'ouverture du plateau : « CHAPITRE n · Titre », le texte d'ouverture, une image `Illustrations/Chapters/<id>` si elle existe, bouton Continuer. Le chapitre est marqué vu quand la fenêtre est fermée.
- « Relire » depuis la page Chapitres affiche la même fenêtre sans modifier `chaptersSeen`.

### Page Chapitres

`HubPage.Chapters` (page liste, gabarit `ListPageKit`), ouverte par un bouton « Chapitres » sur la carte de la ville. En-tête : « n / 8 chapitres terminés ». Une rangée par chapitre (celui de l'acte en cours d'abord, puis les autres de la ville, puis l'autre ville) : numéro, titre, statut (Terminé, En cours, Verrouillé, Bientôt), début du texte d'ouverture, bouton « Relire » (grisé pour un chapitre `Soon`).

### Textes d'ouverture (clés `chapter.<id>.intro`, titres `chapter.<id>.title`)

| Chapitre | Titre | Texte |
|---|---|---|
| ny_brooklyn | Brooklyn | Red Hook, Gowanus, Dumbo : Brooklyn est le premier front. L'AMIF et les gangs, qui prétendent se combattre, se partagent la ville en secret. L'Aube y fait ses preuves, mais chaque succès attire un peu plus l'attention de l'AMIF. |
| ny_queens | Queens | Flushing, Astoria, Jamaica : dans le Queens, le Syndicat fait régner la peur sur les marchés et les gares de triage. Des captifs, des indics, des caméras : l'Aube découvre que le Syndicat n'agit pas seul. |
| ny_harlem | Harlem | À Harlem, la Supernova a laissé des cicatrices que les gangs et le culte de Mephisto exploitent. Des quartiers brûlent, des familles fuient. L'Aube doit choisir qui protéger en premier, et à quel prix. |
| ny_manhattan | Manhattan | Midtown, Times Square, le siège de l'ONU : tout converge vers Manhattan. Les cercles de pouvoir, l'AMIF et l'Aube s'y affrontent à visage découvert. C'est ici que la ville bascule. |
| mos_sergueiv | Sergueiv | Sergueiv, ses monastères et sa ligne de train futuriste : Moscou s'est armée face aux changements du monde, mais elle accueille l'Aube. Derrière la façade, quelqu'un tire déjà les ficelles. |
| mos_kapotnya | Kapotnya | Kapotnya, ses raffineries et son brouillard industriel : les oligarques y cachent ce qu'ils ne veulent pas montrer. L'Aube suit des convois, des fuites et une mafia alliée à l'AMIF dans l'ombre. |
| mos_sadovoye | Sadovoye Koltso | Sur le Jardin, l'anneau de la ville, les tours des oligarques brillent de mille hologrammes. L'Aube comprend que le pouvoir ne se cache plus : il se met en scène. |
| mos_place_rouge | La Place Rouge | La Place Rouge, le Kremlin, le couvre-feu : le dernier chapitre de Moscou est un face-à-face. L'Aube doit décider si elle fait confiance à ceux qui l'accueillent, ou à ceux qui la manipulent. |

## 3. Dossier d'agent (tout le lore sur la carte)

### Source et textes

- Source unique : `02_Lore_Narration/04_Agents_The_Dawn.md`. Champs repris pour chaque agent jouable : **Âge, Sexe, Origine, Lien à la Supernova, Origin Story, Apparence (champ « Visuel »), Pouvoir**. Agent D et Agent M ont eux aussi les sept champs dans le Markdown (écart acté : la spec en supposait cinq). Les 10 cryptides de Salem sont des ennemis hors effectif : ils n'ont pas de dossier.
- Un outil d'éditeur `LoreTextGenerator` (menu *Heroes Dawn ▸ Setup ▸ Générer les textes de lore*) lit le fichier Markdown et écrit `Presentation/Common/LocLore.cs` : une clé `lore.<idAgent>.<champ>` (`age`, `sex`, `origin`, `supernova`, `story`, `look`, `power`) par texte. L'identifiant de l'agent vient du nom (« Brooks, Darius » → `darius_brooks`, accents et ponctuation retirés ; « Okafor, Samuel « Sam » » → `sam_okafor`) et de la liste des agents du jeu (`agents.json`).
- `LocLore.cs` est un fichier généré et commité ; un test relit le Markdown et vérifie qu'il est à jour.

### Fiche d'agent

- Un bouton **DOSSIER** sur la fiche (agents possédés comme héros de la boutique) ouvre un panneau défilant sur la colonne de droite avec les sections dans l'ordre : Identité (âge, sexe, origine), Lien à la Supernova, Origin Story, Pouvoir, Apparence. Un bouton Fermer le referme.
- La courte biographie actuelle de la fiche reste affichée.
- Une section sans texte est simplement omise.

## 4. Erreurs et cas limites

- Aucune voix dans `Resources/Cinematic/Voice` : la cinématique s'affiche et se termine.
- Une image de chapitre absente : la fenêtre s'affiche sans image.
- Une zone dont le quartier n'appartient à aucun chapitre : pas de fenêtre d'ouverture.
- Ancien profil : `chaptersSeen` vide.
- Un agent sans entrée dans le lore : la fiche n'affiche pas le bouton DOSSIER.

## 5. Tests

- **Cinématique** : 11 plans retenus ; chaque légende non vide ; plus de clé `cinematic.vo.S20` ; plus de fichier `vo_*.wav` ; durée d'un plan sans voix = règle ci-dessus ; avec voix = durée de la voix.
- **Chapitres** : 8 chapitres, identifiants uniques et ordre ; statuts (`Soon`, `Done`, `Current`, `Locked`, Brooklyn partiellement terminé) ; ouverture affichée une fois et enregistrée ; ancien profil sans `chaptersSeen` ; « Relire » sans effet sur `chaptersSeen` ; page Chapitres (rangées, ordre, statuts, bouton Relire, ouverture depuis la carte) ; navigation.
- **Dossier** : les 32 agents jouables ont leurs textes (Origin Story et Lien à la Supernova pour les 30 numérotés) ; `LocLore.cs` conforme au Markdown ; ouverture et fermeture du panneau sur la fiche ; Agent D et M sans sections vides.
- **PlayMode** : captures de la fenêtre de chapitre, de la page Chapitres et de la fiche avec son dossier.

## Revue finale — points d'attention

- Aucune voix manquante ne bloque la cinématique.
- La fenêtre de chapitre ne s'affiche qu'une fois par chapitre, même après relance du jeu.
- Aucun texte du dossier n'est tronqué ni vide pour les 32 agents.
- Le déploiement dans une zone reste possible si la fenêtre est fermée par Retour.
