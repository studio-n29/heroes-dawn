# Cinématique de lancement : état tel qu'il est dans Unity

*Version 0.14.1. Ordre et fichiers tirés de `CinematicVideoSequence.cs`, comportement de `CinematicVideoPlayer.cs`. Textes des voix off repris des WAV (`Resources/CinematicVideo/Voice`), d'après `05b_Cinematique_Dialogues_v2.md`. Colonne « Musique » : prompts de style pour Suno, à générer plus tard par séquence.*

---

## 1. Fonctionnement du lecteur

| Élément | Valeur dans le code |
|---|---|
| Vidéos | `StreamingAssets/Cinematic/Video/s01.mp4` à `s46.mp4`, muettes |
| Voix off | `Resources/CinematicVideo/Voice/vo_<id>.wav` (écrans noirs : `vo_S05-5` etc.) |
| Bandeaux noirs | en haut et en bas, 7,5 % de la hauteur chacun |
| Fondu | 0,5 s, toujours vers le noir (entre deux plans, pas de fondu enchaîné) |
| Délai voix | 0,4 s après le début du plan ou de l'écran noir |
| Silence après la voix | 0,4 s |
| Écran noir sans voix | 1 s |
| Durée d'un plan | la durée de la vidéo, ou la voix (+ délai et silence) si elle est plus longue ; la dernière image reste affichée |
| Thème principal | coupé pendant la cinématique, rétabli au plan S42 (l'Aube) |
| Toucher | passe au pas suivant |
| « Passer » | termine la cinématique |

---

## 2. Séquences musicales (Suno)

| Séquence | Plans | Ambiance | Prompt Suno (style) |
|---|---|---|---|
| Ouverture | S01 | Silence, une note grave tenue | `dark ambient, single low sustained cello drone, sparse, cinematic, 40 BPM` |
| 1973, Khost | S02 à S04 | Archive, laboratoire, mystère | `1970s analog cold synth strings, eerie, slow, mysterious, science fiction, 70 BPM` |
| 1981, Apex Unit | S05 à S10 | Guerre froide, montée militaire | `cold war military percussion, distant snare rolls, tense brass, slow build to heroic anthem, 90 BPM` |
| URSS et 1991 | S11 à S13 | Chœur slave, puis apaisement | `slavic male choir, somber, then calm piano and strings for fall of the Berlin wall, cinematic` |
| 2026, chute | S15 à S19 | Piano isolé, pluie, puis tension | `solo piano, very slow, melancholic, rain ambience, then tense low drone with heartbeat pulse, 60 BPM` |
| X Society | S20 à S22 | Basse industrielle menaçante | `industrial bass, menacing distorted synth, dark pulsing groove, 95 BPM, cinematic` |
| Rituel, Mephisto | S23 à S24 | Chœur infernal | `infernal choir chanting, low war drums, dark orchestral, hellish, slow` |
| Rebelles | S25 à S32 | Fuite, puis refuge, puis menace | `fast escape strings, then tender piano for a safehouse, then ominous low strings, cinematic` |
| Paris, climax | S33 à S41 | Apocalypse, chœurs, supernova | `epic apocalyptic orchestral climax, full choir, brass and drums, then silence and a single high note` |
| L'Aube | S42 à S46 | Thème principal, espoir | `bright heroic orchestral theme, rising strings, hopeful dawn, triumphant, 100 BPM` |

Écrans noirs : pas de musique, seulement un souffle ou un silence (la voix est seule).

---

## 3. Déroulé pas à pas (54 pas : 46 plans, 8 écrans noirs)

| # | Id | Type | Vidéo | Voix off | Musique |
|---|---|---|---|---|---|
| 1 | S01 | Plan | s01.mp4 | « Chaque aube commence dans la nuit la plus noire... » | Ouverture |
| 2 | S02 | Plan | s02.mp4 | « Dans les années 70, un généticien a fait une découverte qui allait bouleverser le monde. » | 1973, Khost |
| 3 | S03 | Plan | s03.mp4 | « Le docteur Frank Khost avait trouvé le moyen de modifier les gènes humains, d'une façon extraordinaire... » | 1973, Khost |
| 4 | S04 | Plan | s04.mp4 | « ...une modification capable d'offrir des aptitudes surnaturelles. » | 1973, Khost |
| 5 | S05-5 | **Écran noir** | — | « Alors il fallait la crème de la crème. » | Souffle, silence |
| 6 | S05 | Plan | s05.mp4 | « Il n'a pas fallu longtemps pour que le gouvernement américain, en pleine Guerre froide, en fasse un nouveau projet d'armement. » | 1981, Apex Unit |
| 7 | S06 | Plan | s06.mp4 | « Adam West, un soldat patriote qui avait perdu un bras au Vietnam, capable désormais de remodeler ce bras avec tout ce qu'il touche. » | 1981, Apex Unit |
| 8 | S07 | Plan | s07.mp4 | « Moi, Brie Rayner, jeune scientifique de la NASA... jusqu'à ce que mon cerveau devienne si rapide que je contrôle les flux électriques. » | 1981, Apex Unit |
| 9 | S08 | Plan | s08.mp4 | « John Guerin, un artiste français qui peut désormais modeler son corps comme bon lui chante. » | 1981, Apex Unit |
| 10 | S09 | Plan | s09.mp4 | « Et Orik Bronson, le plus agile des snipers : un regard qui perce les murs, une ouïe qui détecte une brindille à plus de trois kilomètres. » | 1981, Apex Unit |
| 11 | S10 | Plan | s10.mp4 | « On nous a appelés l'Apex Unit, le porte-drapeau de l'Occident face à l'URSS. » | 1981, Apex Unit (anthem) |
| 12 | S10-5 | **Écran noir** | — | « Eux aussi avaient leur méta-humain. Il s'appelait Viktor, et il contrôlait la matière elle-même... » | Souffle, silence |
| 13 | S11 | Plan | s11.mp4 | *(pas de voix)* | URSS et 1991 |
| 14 | S12 | Plan | s12.mp4 | « Puis l'URSS est tombée, le mur de Berlin avec elle. Tout ce pourquoi nous avions été modifiés n'avait plus de raison d'être. » | URSS et 1991 |
| 15 | S13 | Plan | s13.mp4 | « Et Viktor... a disparu, envolé, comme s'il n'avait jamais existé. » | URSS et 1991 |
| 16 | S12-5 | **Écran noir** | — | « Nous sommes devenus des porte-drapeaux, une belle figure et une force de dissuasion... jusqu'en 2026. » | Souffle, silence |
| 17 | S14 | Plan | s14.mp4 | *(pas de voix)* | URSS et 1991 (apaisement) |
| 18 | S15 | Plan | s15.mp4 | « Un jeune garçon, Eda, menait une vie triste et dépressive. Le poids était devenu si lourd qu'il a décidé d'en finir. » | 2026, chute |
| 19 | S16 | Plan | s16.mp4 | « Il a été emmené d'urgence à l'hôpital Salem, mais il était trop tard : son cœur a cessé de battre. » | 2026, chute |
| 20 | S17 | Plan | s17.mp4 | « Mort. Jeune. Seul. » | 2026, chute |
| 21 | S15-5 | **Écran noir** | — | « ... Enfin, si tout avait été normal. » | Souffle, silence |
| 22 | S18 | Plan | s18.mp4 | « Il a rouvert les yeux avec un feu nouveau qui brûlait en lui : une force jaillissante, presque divine. » | 2026, chute (tension) |
| 23 | S19 | Plan | s19.mp4 | « Ses flammes noires allaient bientôt attirer l'attention du monde, qui découvrirait une nouvelle aussi folle que vraie... » | 2026, chute (tension) |
| 24 | S17-5 | **Écran noir** | — | « Le premier méta-humain naturel était né. » | Souffle, silence |
| 25 | S20 | Plan | s20.mp4 | « Une information qui a aussitôt attiré l'attention d'Orik Kovvolt, puissant oligarque russe caché aux États-Unis, à la tête de la X Society, en quête d'une solution au problème « Apex Unit ». » | X Society |
| 26 | S18-5 | **Écran noir** | — | « Il s'est approché d'Eda, qui l'a envoyé paître à sa façon. » | Souffle, silence |
| 27 | S21 | Plan | s21.mp4 | « Orik n'était pas homme à plaisanter. Il a tué tous les proches d'Eda : ses amis, sa famille, sa maison... » | X Society |
| 28 | S22 | Plan | s22.mp4 | « Enragé, Eda a affronté Orik après avoir anéanti ses hommes en un éclair. Orik s'est révélé être Viktor Volkov, et a affronté les flammes noires d'Eda dans son repaire. » | X Society (combat) |
| 29 | S21-5 | **Écran noir** | — | « Malheureusement pour Eda, tout cela faisait partie de son plan : un plan diabolique, visant à réveiller une créature du chaos avec le sang du jeune garçon... » | Souffle, silence |
| 30 | S23 | Plan | s23.mp4 | *(pas de voix)* | Rituel, Mephisto |
| 31 | S24 | Plan | s24.mp4 | « Mephisto, le général des Enfers, a pris possession du corps de Viktor, prêt à ravager le monde et à ouvrir une porte vers les Enfers. » | Rituel, Mephisto |
| 32 | S23-5 | **Écran noir** | — | « Eda a réussi à s'enfuir et a tenté de chercher de l'aide, mais... personne ne l'a écouté. Sauf moi. » | Souffle, silence |
| 33 | S25 | Plan | s25.mp4 | « Eda et moi avons pris la route, sur la trace de Mephisto, traités comme des criminels par le gouvernement après notre fuite. » | Rebelles |
| 34 | S26 | Plan | s26.mp4 | *(pas de voix)* | Rebelles (refuge, piano) |
| 35 | S27 | Plan | s27.mp4 | *(pas de voix)* | Rebelles (menace) |
| 36 | S28 | Plan | s28.mp4 | *(pas de voix)* | Rebelles (menace) |
| 37 | S29 | Plan | s29.mp4 | *(pas de voix)* | Rebelles (silence, acouphène) |
| 38 | S30 | Plan | s30.mp4 | *(pas de voix)* | Rebelles (colère, orchestre) |
| 39 | S31 | Plan | s31.mp4 | *(pas de voix)* | Rebelles (révélation, sombre) |
| 40 | S32 | Plan | s32.mp4 | *(pas de voix)* | Rebelles (abandon, piano) |
| 41 | S33 | Plan | s33.mp4 | *(pas de voix)* | Paris, climax |
| 42 | S34 | Plan | s34.mp4 | *(pas de voix)* | Paris, climax |
| 43 | S35 | Plan | s35.mp4 | *(pas de voix)* | Paris, climax (retrait de la musique) |
| 44 | S36 | Plan | s36.mp4 | *(pas de voix)* | Paris, climax (battement de cœur) |
| 45 | S37 | Plan | s37.mp4 | *(pas de voix)* | Paris, climax (phoenix doré, explosion) |
| 46 | S38 | Plan | s38.mp4 | *(pas de voix)* | Paris, climax |
| 47 | S39 | Plan | s39.mp4 | *(pas de voix)* | Paris, climax (aspiration) |
| 48 | S40 | Plan | s40.mp4 | *(pas de voix)* | Silence, puis une seule note aiguë |
| 49 | S41 | Plan | s41.mp4 | *(pas de voix)* | Paris, climax (souffle cosmique) |
| 50 | S42 | Plan | s42.mp4 | *(pas de voix)* | L'Aube (thème principal rétabli) |
| 51 | S43 | Plan | s43.mp4 | *(pas de voix)* | L'Aube |
| 52 | S44 | Plan | s44.mp4 | *(pas de voix)* | L'Aube (montée du thème) |
| 53 | S45 | Plan | s45.mp4 | *(pas de voix)* | L'Aube |
| 54 | S46 | Plan | s46.mp4 | *(pas de voix)* | L'Aube (impact final) |

---

## 4. Points ouverts

- **S26 à S46** : pas de texte de voix dans la v2 ; les plans défilent muets.
- **S09** : le nom « Orik Bronson » vient du script, la fiche dit « Erik Bronson ».
- **S14** : l'image `cin_14` manque côté illustrations ; la vidéo `s14` est en place.
- **Correspondance `s##` / `S##`** : supposée, à confirmer sur les fichiers.
- **Poids** : l'APK 0.14.1 fait 794 Mo, dont les vidéos.
- **Musique** : les prompts sont des pistes de style, à ajuster après écoute ; Suno ne fait pas de synchronisation exacte, donc les coupes se font au montage.
