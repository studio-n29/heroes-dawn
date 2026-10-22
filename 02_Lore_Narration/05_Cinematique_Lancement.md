# Heroes Dawn : Cinématique de lancement

*Découpage technique pour génération IA. Format « récap Diablo 3 » : images fixes légèrement animées, narration en voix off.*

---

## 1. Paramètres généraux

| Paramètre | Valeur |
|---|---|
| Durée cible | ~4 min (46 plans), réductible à ~2 min 30 en retirant les plans marqués *(optionnel)* |
| Format | 16:9, 1920×1080 minimum (idéal 4K pour recadrer), 24 i/s |
| Narratrice | Brie Rayner, à la première personne |
| Mouvement | Animation discrète : poussée de caméra lente, parallaxe, particules, cheveux et tissus, scintillement de lumière. Les personnages ne bougent presque pas. |
| Texte à l'écran | Aucun dans les images générées. Les dates et le logo s'ajoutent au montage. |
| Transitions | Fondus enchaînés lents, fondus au noir entre les séquences, « brûlure » dorée sur les moments clés |

### Choix du narrateur

Rayner a tout vécu : l'Apex Unit, la fuite avec Eda, Paris, puis la fondation de l'Aube. Sa voix fait le lien entre la légende et le joueur, comme Leah ou Tyrael dans les récaps de Diablo 3. La dernière réplique s'adresse directement au joueur.

**Direction de voix (pour la synthèse vocale)** : femme britannique, voix grave et posée, légère lassitude, accent anglais discret en français. Rythme lent, environ 130 mots par minute. Pauses longues entre les phrases. Voix presque chuchotée dans les scènes intimes (S26, S35). Aucune emphase dramatique forcée.

---

## 2. Prompts de base

### [STYLE] : à ajouter à la fin de chaque prompt image

```
stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality
```

### [NEG] : prompt négatif commun

```
photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

### [NEG+] : prompt négatif renforcé (plans sensibles S15 à S17, S21, S23, S29, S31)

```
[NEG], gore, visible wounds, blood splatter, gun, weapon pointed at head, corpses, dead bodies
```

### Palettes par époque (à ajouter après la description de scène)

| Époque | Plans | Ajout au prompt |
|---|---|---|
| 1973–1991 | S02–S14 | `muted sepia and teal color grade, film grain, archival mood` |
| 2026, chute | S15–S32 | `cold steel blue color grade, sickly black and green flame accents` |
| Paris, apogée | S33–S41 | `crimson hellfire sky, blinding molten gold highlights` |
| L'Aube | S42–S46 | `warm amber dawn light, gold accents, hopeful atmosphere` |

---

## 3. Fiches personnages

Générer d'abord une planche de référence par personnage, puis l'utiliser comme référence de personnage sur chaque plan où il apparaît. C'est la clé de la cohérence.

**Prompt de planche** :
```
character reference sheet, front view and three-quarter view, full body and face close-up, neutral dark grey background, [PERSONNAGE], [STYLE]
```

| Tag | Description à insérer |
|---|---|
| [KHOST] | `Frank Khost, 1970s geneticist in his fifties, gaunt face, thin grey hair combed back, round wire glasses, white lab coat over brown knit vest` |
| [WEST] | `Adam West, American army veteran in his thirties, square jaw, buzz cut, left arm missing at the shoulder, olive and grey military fatigues` |
| [RAYNER] | `Brie Rayner, British woman in her early thirties, sharp intelligent eyes with faint electric blue glow, short auburn bob, blue and white NASA engineer flight jacket` |
| [RAYNER_2026] | `Brie Rayner, same face, ageless, short auburn bob, long blue and white tactical coat with glowing electric circuit lines` |
| [GUERIN] | `John Guerin, French painter in his late twenties, long dark curly hair, thin mustache, paint-stained shirt under black and gold coat, skin with faint shifting cellular patterns` |
| [BRONSON] | `Erik Bronson, Swedish sniper in his thirties, braided pale blond beard, ice-blue eyes, white winter camouflage, long scoped rifle` |
| [VOLKOV] | `Viktor Volkov, Soviet super-soldier in his forties, massive build, shaved head, scar across the lip, heavy grey and red Soviet greatcoat` |
| [ORIK] | `Orik Kovvolt, tall gaunt man in his sixties, slicked-back white hair, black tailored long coat, cold pale eyes, red X emblem on armband` |
| [MEPHISTO] | `Mephisto, demon wearing Volkov's massive body, cracked charcoal skin with glowing red fissures, curved black horns, burning red eyes, tattered black cloak` |
| [EDA] | `Eda Ryle, 25-year-old man, lean and pale, shaggy black hair, dark circles under his eyes, charcoal hoodie under black jacket` |
| [EDA_NOIR] | `[EDA], black and toxic green flames crawling over his arms and shoulders, green glowing eyes` |
| [EDA_DORE] | `[EDA], body wreathed in molten golden flames forming faceted armor, immense wings of golden fire, radiant gold eyes` |

> Les apparences d'Eda, Khost, Volkov, Orik et Mephisto sont des propositions. Rayner et l'Apex Unit apparaissent sans avoir vieilli en 2026 (voir les questions à la fin).

---

## 4. Workflow de production

1. **Planches personnages** : générer chaque planche, choisir la meilleure, la garder comme référence fixe.
2. **Référence de style** : générer S37 (le Phoenix Doré) en premier. Si le rendu convient, l'utiliser comme référence de style pour tous les autres plans.
3. **Images fixes** : 4 variantes minimum par plan, garder la meilleure, agrandir en 4K.
4. **Animation** : image-to-video, clips de 5 s, avec le prompt d'animation du plan. Ralentir à 50–70 % au montage si le mouvement est trop rapide.
5. **Voix off** : générer réplique par réplique, puis caler les images sur la voix, pas l'inverse.
6. **Montage** : ajouter les dates, le grain, le glow doré et le logo final. Mixage : musique en dessous de la voix (-18 dB environ), sons d'ambiance discrets.

**Règle pour tous les prompts d'animation** : ajouter `subtle motion only, characters nearly still, no morphing, no camera shake, 5 seconds`.

---

## 5. Découpage plan par plan

### SÉQUENCE 0 · OUVERTURE (0:00 – 0:06)

**Musique** : silence, puis une note grave tenue.

#### S01 · La braise · 6 s
- **Cadrage** : très gros plan, centré
- **Image** :
```
a single glowing golden ember floating in absolute darkness, faint smoke trails, tiny sparks, [STYLE], warm amber dawn light
```
- **Animation** : `ember slowly drifting upward and pulsing softly, sparks fading`
- **VO** : « Chaque aube commence dans la nuit la plus noire. »
- **Son** : crépitement léger, souffle

---

### SÉQUENCE 1 · 1973, FRANK KHOST (0:06 – 0:22)

**Musique** : nappe de cordes froides, ton d'archive. **Titre au montage** : *1973*

#### S02 · Le laboratoire · 6 s
- **Cadrage** : plan large, légère plongée
- **Image** :
```
[KHOST] seen from behind, working alone at a cluttered 1970s genetics laboratory bench at night, single desk lamp, glass vials and microscopes, chalkboard covered with DNA diagrams, [STYLE], muted sepia and teal color grade, film grain, archival mood
```
- **Animation** : `slow push-in toward the scientist, lamp light flickering faintly, dust motes floating`
- **VO** : « En 1973, un homme a changé ce que signifiait être humain. Il s'appelait Frank Khost. »
- **Son** : ronronnement de néon, tic-tac d'horloge

#### S03 · L'ADN · 5 s
- **Cadrage** : très gros plan
- **Image** :
```
extreme close-up of a scientist's hands holding a glass vial, a glowing blue double helix of light rising from the liquid, reflections in wire glasses, [STYLE], muted sepia and teal color grade, film grain
```
- **Animation** : `helix slowly rotating and shimmering, liquid gently swirling`
- **VO** : « Il croyait pouvoir guérir le monde. »
- **Son** : tintement de verre, vibration cristalline

#### S04 · Le regard de Khost · 5 s *(optionnel)*
- **Cadrage** : gros plan, contre-plongée
- **Image** :
```
[KHOST] low angle close-up portrait, blue glow of the helix reflected on his round glasses, determined and obsessive expression, dark laboratory background, [STYLE], muted sepia and teal color grade
```
- **Animation** : `slow push-in, glow pulsing on the lenses`
- **VO** : *(silence, musique seule)*

---

### SÉQUENCE 2 · 1981, L'APEX UNIT (0:22 – 0:55)

**Musique** : percussions militaires sourdes, montée lente. **Titre au montage** : *1981*

#### S05 · La salle de guerre · 5 s
- **Cadrage** : plan large, plongée
- **Image** :
```
Cold War military war room in 1981, generals in olive uniforms standing around a large table map of the world lit from below, cigarette smoke, red telephones, dark bunker, [STYLE], muted sepia and teal color grade, film grain
```
- **Animation** : `smoke drifting slowly, slow pull-back, map lights blinking`
- **VO** : « Huit ans plus tard, en pleine Guerre froide, l'Amérique a vu dans ses travaux une arme. Une dissuasion sans retombées. »
- **Son** : murmures, téléphone lointain

#### S06 · Adam West · 5 s
- **Cadrage** : plan moyen, contre-plongée
- **Image** :
```
[WEST] in a concrete military testing chamber, touching a concrete pillar with his right hand while a new left arm forms from faceted grey concrete fragments at his shoulder, debris floating, [STYLE], muted sepia and teal color grade
```
- **Animation** : `concrete fragments slowly drifting toward his shoulder, dust falling`
- **VO** : « Adam West. Le soldat qui avait perdu un bras. »
- **Son** : craquement de pierre

#### S07 · Brie Rayner · 5 s
- **Cadrage** : gros plan, trois-quarts
- **Image** :
```
[RAYNER] in a NASA control room full of 1980s monitors, thin electric blue arcs dancing between her fingertips, data reflected in her glowing eyes, [STYLE], muted sepia and teal color grade
```
- **Animation** : `electric arcs crackling softly between fingers, monitor light flickering`
- **VO** : « Moi, Brie Rayner. Ingénieure à la NASA. »
- **Son** : grésillement électrique

#### S08 · John Guerin · 5 s
- **Cadrage** : plan rapproché
- **Image** :
```
[GUERIN] in an artist studio turned laboratory, half of his face shifting into faceted cellular patterns like a living sculpture, canvases in background, [STYLE], muted sepia and teal color grade
```
- **Animation** : `faceted patterns slowly rippling across his skin, paint dust floating`
- **VO** : « John Guerin. L'artiste. Le premier à avoir survécu. »
- **Son** : texture organique, souffle

#### S09 · Erik Bronson · 5 s
- **Cadrage** : très gros plan sur l'œil, puis profondeur
- **Image** :
```
[BRONSON] lying prone in snow with his scoped rifle, extreme close-up of his ice-blue eye glowing, through his vision we see a ghostly outline of people behind a concrete wall, [STYLE], muted sepia and teal color grade
```
- **Animation** : `snowflakes falling, eye glow pulsing slowly`
- **VO** : « Erik Bronson. Rien ne lui échappait, pas même derrière les murs. »
- **Son** : vent, battement de cœur ralenti

#### S10 · L'Apex Unit · 6 s
- **Cadrage** : plan large, contre-plongée héroïque
- **Image** :
```
[WEST], [RAYNER], [GUERIN] and [BRONSON] standing together in a dark military hangar, low angle heroic group shot, searchlight beams behind them, olive and grey military palette, [STYLE], muted sepia and teal color grade
```
- **Animation** : `slow push-in, searchlights sweeping, dust in light beams`
- **VO** : « Ils nous ont appelés l'Apex Unit. »
- **Son** : coup de percussion, montée musicale

---

### SÉQUENCE 3 · VOLKOV ET 1991 (0:55 – 1:15)

**Musique** : chœur slave grave, puis apaisement. **Titre au montage** : *URSS* puis *1991*

#### S11 · Viktor Volkov · 6 s
- **Cadrage** : plan large, contre-plongée
- **Image** :
```
[VOLKOV] standing in a snowy Soviet military base at night, steel girders and tanks bending and reshaping around him like liquid metal, red floodlights, [STYLE], muted sepia and teal color grade, red accents
```
- **Animation** : `metal slowly warping around him, snow swirling`
- **VO** : « L'Union soviétique a répondu avec un seul homme. Viktor Volkov, maître de la matière. »
- **Son** : métal qui gémit

#### S12 · La chute du bloc · 5 s *(optionnel)*
- **Cadrage** : plan large
- **Image** :
```
crowd at night celebrating on top of a crumbling concrete wall, fireworks, red flags falling, silhouettes, [STYLE], muted sepia and teal color grade, film grain
```
- **Animation** : `fireworks blooming slowly, flags drifting down`
- **VO** : « Puis le bloc soviétique s'est effondré. »
- **Son** : foule lointaine

#### S13 · La disparition · 4 s
- **Cadrage** : plan moyen
- **Image** :
```
an abandoned heavy grey and red Soviet greatcoat lying alone in fresh snow in an empty frozen courtyard, footprints vanishing, [STYLE], muted sepia and teal color grade
```
- **Animation** : `snow slowly covering the coat, wind`
- **VO** : « Volkov a disparu. »
- **Son** : vent seul

#### S14 · Les protecteurs · 5 s
- **Cadrage** : plan large, contre-plongée
- **Image** :
```
[WEST], [RAYNER], [GUERIN] and [BRONSON] on a rooftop above a 1990s city at night, watching over it, city lights below, heroic silhouettes with rim light, [STYLE], muted sepia and teal color grade
```
- **Animation** : `slow lateral parallax, city lights twinkling`
- **VO** : « Et nous sommes devenus les protecteurs du monde. »
- **Son** : la musique s'éteint, fondu au noir

---

### SÉQUENCE 4 · 2026, PHOENIX BIRTH (1:15 – 1:42)

**Musique** : piano isolé, très lent. **Titre au montage** : *2026*

> Plans sensibles : on ne montre jamais l'arme ni le geste, seulement l'état d'esprit avant et l'hôpital après. Utiliser [NEG+].

#### S15 · Eda seul · 6 s
- **Cadrage** : plan large, de dos
- **Image** :
```
[EDA] seen from behind, sitting alone on the floor of a dark small apartment, back against the bed, rain streaming down the window, city lights blurred outside, empty and heavy atmosphere, [STYLE], cold steel blue color grade
```
- **Négatif** : [NEG+]
- **Animation** : `rain trickling down the window, slow push-in, he stays still`
- **VO** : « Trente-cinq ans plus tard, un jeune homme de vingt-cinq ans n'avait plus la force de continuer. Il s'appelait Eda Ryle. »
- **Son** : pluie, silence intérieur

#### S16 · L'hôpital · 5 s
- **Cadrage** : gros plan
- **Image** :
```
close-up of a hospital heart monitor showing an almost flat line with a single faint spike, frost forming on the screen edges, cold blue room behind, [STYLE], cold steel blue color grade
```
- **Négatif** : [NEG+]
- **Animation** : `single slow pulse on the monitor, frost spreading slightly`
- **VO** : « Les médecins ont parlé d'un malaise. Son cœur ne battait plus qu'une fois par minute. »
- **Son** : bip unique, très espacé

#### S17 · L'animation suspendue · 5 s
- **Cadrage** : plan moyen, plongée
- **Image** :
```
[EDA] lying motionless on a hospital bed, pale skin covered in thin frost, eyes closed, peaceful face, cold light from a window shifting from night to day, [STYLE], cold steel blue color grade
```
- **Négatif** : [NEG+]
- **Animation** : `light slowly sweeping across the room, frost glittering`
- **VO** : « Pendant vingt-quatre heures, il est resté suspendu entre la vie et la mort. »
- **Son** : souffle glacé

#### S18 · Le réveil · 5 s
- **Cadrage** : très gros plan sur les yeux
- **Image** :
```
extreme close-up of [EDA] eyes snapping open, irises glowing toxic green, reflections of black and green flames in the pupils, frost melting on his eyelashes, [STYLE], cold steel blue color grade, sickly black and green flame accents
```
- **Animation** : `green glow intensifying, frost melting into droplets`
- **VO** : « Puis il s'est réveillé. Intact. Et brûlant d'un feu noir et vert. »
- **Son** : inspiration brutale, flamme qui s'allume

#### S19 · Le secret · 4 s
- **Cadrage** : plan rapproché
- **Image** :
```
[EDA] in a dark rainy alley, hood up, closing his fist around a small black and green flame to hide it, worried glance over his shoulder, [STYLE], cold steel blue color grade, sickly black and green flame accents
```
- **Animation** : `flame flickering between his fingers then dimming`
- **VO** : « Il a choisi de le cacher. »
- **Son** : pluie, pas qui s'éloignent

---

### SÉQUENCE 5 · LA X SOCIETY (1:42 – 2:10)

**Musique** : basse industrielle menaçante.

#### S20 · Orik Kovvolt · 5 s
- **Cadrage** : gros plan, contre-plongée, visage à moitié dans l'ombre
- **Image** :
```
[ORIK] standing in a dark empty warehouse, half of his face in shadow, cold smile, red emblem glowing on his armband, armed men in silhouette behind him, [STYLE], cold steel blue color grade, red accents
```
- **Animation** : `slow push-in, dust in a single beam of light`
- **VO** : « Un homme est venu le chercher. Orik Kovvolt, chef de la X Society. Il voulait effacer les méta-humains de la surface de la Terre. »
- **Son** : pas lourds, écho

#### S21 · Le prix · 6 s
- **Cadrage** : plan large, de dos
- **Image** :
```
[EDA] kneeling from behind in front of a suburban family house burning at night, flames from every window, smoke rising, his silhouette tiny against the fire, [STYLE], cold steel blue color grade, orange fire glow
```
- **Négatif** : [NEG+]
- **Animation** : `flames flickering, embers rising, smoke drifting`
- **VO** : « Pour forcer Eda à le suivre, il lui a tout pris. »
- **Son** : incendie, coupure brutale de la musique

#### S22 · Le masque tombe · 5 s
- **Cadrage** : plan rapproché, champ-contrechamp dans une seule image
- **Image** :
```
[EDA_NOIR] in the foreground gripping [ORIK] by the collar, Orik's face cracking like porcelain to reveal the scarred face of [VOLKOV] underneath, black and green fire between them, [STYLE], cold steel blue color grade, sickly black and green flame accents
```
- **Animation** : `cracks slowly spreading across the face, flames swirling`
- **VO** : « Eda l'a traqué. Et sous ce visage, il a trouvé Viktor Volkov. »
- **Son** : fissure, grondement

#### S23 · Le rituel · 5 s
- **Cadrage** : plan large, plongée
- **Image** :
```
an ancient occult machine of black iron gears and stone circles in an underground chamber, channels of glowing red light flowing through carved runes toward its core, two figures fighting on the circle, [STYLE], cold steel blue color grade, red glow
```
- **Négatif** : [NEG+]
- **Animation** : `red light flowing through the runes, gears slowly turning`
- **VO** : « Leur combat était un rituel. Leurs sangs mêlés ont réveillé une machine. »
- **Son** : mécanisme qui s'enclenche, chœur grave

#### S24 · Mephisto · 6 s
- **Cadrage** : plan moyen, contre-plongée extrême
- **Image** :
```
[MEPHISTO] rising from the machine, red fissures splitting the body, horns tearing through, crimson smoke pouring out, demonic eyes burning, [STYLE], cold steel blue color grade, crimson glow
```
- **Animation** : `crimson smoke billowing upward, fissures pulsing with light`
- **VO** : « Et le démon qui vivait en Volkov depuis des décennies s'est libéré. Mephisto. »
- **Son** : rugissement grave étouffé, impact musical

---

### SÉQUENCE 6 · LES REBELLES (2:10 – 2:55)

**Musique** : thème de fuite, cordes rapides puis thème d'amour en sourdine.

#### S25 · La traque · 5 s
- **Cadrage** : plan large, contre-plongée
- **Image** :
```
[EDA] and [RAYNER_2026] running across a wet highway overpass at night, helicopter searchlights pinning them, armed tactical agents in olive and grey, [STYLE], cold steel blue color grade
```
- **Animation** : `searchlight beams sweeping, rain falling, slight parallax`
- **VO** : « Les Apex l'ont rejeté. L'AMIF l'a traqué. J'ai été la seule à le croire. »
- **Son** : hélicoptère, sirènes lointaines

#### S26 · Le refuge · 5 s
- **Cadrage** : gros plan à deux
- **Image** :
```
[EDA] and [RAYNER_2026] in a dim safehouse, foreheads touching, eyes closed, a single candle between them, intimate and tender, warm candlelight against cold shadows, [STYLE], cold steel blue color grade
```
- **Animation** : `candle flame flickering gently, faint breathing motion`
- **VO** : *(chuchoté)* « Dans la fuite, nous sommes devenus bien plus que des alliés. »
- **Son** : silence, flamme

#### S27 · Le complexe de l'Oregon · 5 s
- **Cadrage** : plan large
- **Image** :
```
vast underground military complex in a forest mountain, rows of shattered containment tanks, deformed humanoid creatures with twisted faceted bodies crawling out, emergency red lights, [STYLE], cold steel blue color grade, red alarm lights
```
- **Animation** : `red lights rotating, mist spilling from broken tanks, creatures barely moving`
- **VO** : « Dans l'Oregon, Mephisto a libéré les oubliés du Protocole Khost. »
- **Son** : alarme, grognements

#### S28 · Le Vatican · 5 s
- **Cadrage** : plan très large
- **Image** :
```
a vast Renaissance basilica dome and colonnade square under siege at dusk, explosions, tactical helicopters, metahumans clashing in the air, smoke columns, [STYLE], cold steel blue color grade, orange explosions
```
- **Animation** : `smoke rising, slow push-in, distant explosions flashing`
- **VO** : « Il visait le cœur de la foi. »
- **Son** : cloches, explosions

#### S29 · Rayner touchée · 5 s
- **Cadrage** : plan moyen, ralenti
- **Image** :
```
[RAYNER_2026] falling backward in slow motion amid debris, her electric aura shattering into fading blue sparks, eyes wide, [STYLE], cold steel blue color grade
```
- **Négatif** : [NEG+]
- **Animation** : `blue sparks slowly dispersing, debris floating, extreme slow motion`
- **VO** : « Là-bas, j'ai été touchée. »
- **Son** : tout se coupe, acouphène

#### S30 · La colère · 5 s
- **Cadrage** : plan très large, contre-plongée
- **Image** :
```
[EDA_NOIR] screaming at the center of an immense firestorm of black and green flames engulfing the basilica, dome collapsing, [STYLE], cold steel blue color grade, sickly black and green flame accents
```
- **Animation** : `flames roaring outward, debris rising, embers`
- **VO** : « Et Eda a tout réduit en cendres. »
- **Son** : explosion massive, cri étouffé

#### S31 · La révélation · 5 s
- **Cadrage** : gros plan
- **Image** :
```
[MEPHISTO] behind [EDA_NOIR], whispering into his ear, a long black blade emerging through Eda's chest seen only in silhouette against fire, Eda's face frozen in shock, [STYLE], cold steel blue color grade, crimson glow
```
- **Négatif** : [NEG+]
- **Animation** : `embers drifting, flames slowly dimming around Eda`
- **VO** : « Mephisto lui a révélé la vérité. Il était le Phoenix Noir. Né des expériences de Volkov : Khost, du sang d'ange, et du sang de démon. »
- **Son** : murmure démoniaque grave

#### S32 · L'abandon · 5 s
- **Cadrage** : plan large, plongée
- **Image** :
```
[EDA] sitting on the floor of an abandoned safehouse, head down, arms on knees, extinguished candle beside him, grey morning light through broken blinds, [STYLE], cold steel blue color grade
```
- **Animation** : `dust floating in light rays, very slow push-in`
- **VO** : « Revenu une fois de plus, il a abandonné. »
- **Son** : silence, piano

---

### SÉQUENCE 7 · PARIS (2:55 – 3:35)

**Musique** : montée orchestrale complète, chœurs. Climax.

#### S33 · La porte des Enfers · 6 s
- **Cadrage** : plan très large
- **Image** :
```
the Eiffel Tower at night channeling crimson lightning into a swirling wormhole opening in the sky above Paris, hellfire visible through the portal, city below in chaos, [STYLE], crimson hellfire sky
```
- **Animation** : `wormhole slowly rotating, lightning pulsing along the tower`
- **VO** : « À Paris, Mephisto a ouvert une porte vers les Enfers. »
- **Son** : grondement colossal

#### S34 · La bataille · 5 s
- **Cadrage** : plan large, contre-plongée
- **Image** :
```
[WEST], [GUERIN] and [BRONSON] fighting demonic creatures on the Trocadero esplanade, the Eiffel Tower and portal behind them, [STYLE], crimson hellfire sky
```
- **Animation** : `slight parallax, sparks and embers flying`
- **VO** : « Les Apex ont combattu. Moi aussi. »
- **Son** : combat, chœurs

#### S35 · L'appel · 5 s
- **Cadrage** : gros plan au sol
- **Image** :
```
[RAYNER_2026] lying wounded on the ground among rubble, reaching one hand toward the sky, faint blue sparks around her fingers, tears, [STYLE], crimson hellfire sky
```
- **Animation** : `sparks flickering weakly, embers falling`
- **VO** : *(chuchoté)* « Au moment où j'allais tomber, je l'ai appelé. »
- **Son** : la musique se retire, souffle

#### S36 · La transmutation · 5 s
- **Cadrage** : très gros plan sur les yeux
- **Image** :
```
extreme close-up of [EDA] eyes, black and green flames in his irises turning into molten gold, tear evaporating into golden sparks, [STYLE], blinding molten gold highlights
```
- **Animation** : `green slowly giving way to gold, sparks rising`
- **VO** : « Il a entendu. Il a accepté ce qu'il était. »
- **Son** : battement de cœur, puis embrasement

#### S37 · Le Phoenix Doré · 6 s *(plan de référence du style)*
- **Cadrage** : plan large, contre-plongée héroïque
- **Image** :
```
[EDA_DORE] rising above the Paris rooftops, immense wings of golden fire spread wide, low angle heroic shot, crimson sky splitting with golden light behind him, [STYLE], blinding molten gold highlights
```
- **Animation** : `wings slowly unfolding, golden embers swirling, light rays`
- **VO** : « Et le Phoenix Noir est devenu le Phoenix Doré. »
- **Son** : explosion musicale, chœurs à pleine puissance

#### S38 · L'âme arrachée · 5 s
- **Cadrage** : plan rapproché
- **Image** :
```
[EDA_DORE] tearing a writhing crimson spectral demon soul out of [VOLKOV]'s body, golden fire versus red smoke, Volkov's body collapsing, [STYLE], blinding molten gold highlights, crimson smoke
```
- **Animation** : `red smoke stretching and being pulled, golden fire flaring`
- **VO** : « Il a arraché Mephisto au corps de Volkov. »
- **Son** : hurlement démoniaque qui s'étire

#### S39 · Le portail · 5 s
- **Cadrage** : plan très large, Eda minuscule
- **Image** :
```
[EDA_DORE] tiny silhouette flying into the vast swirling hellfire wormhole above the Eiffel Tower, golden trail behind him, [STYLE], crimson hellfire sky, blinding molten gold highlights
```
- **Animation** : `slow push toward the portal, golden trail lingering`
- **VO** : « Puis il a franchi la porte. »
- **Son** : aspiration, silence progressif

#### S40 · La Supernova · 5 s
- **Cadrage** : plan centré
- **Image** :
```
a colossal white and gold supernova implosion erupting inside a hellish crimson dimension, shockwave rings of golden light, demonic landscape disintegrating, [STYLE], blinding molten gold highlights
```
- **Animation** : `light expanding outward slowly, shockwave rings`
- **VO** : *(silence)*
- **Son** : silence total, puis une seule note aiguë

#### S41 · Le blackout · 6 s
- **Cadrage** : vue orbitale
- **Image** :
```
planet Earth seen from orbit at night, a golden shockwave ring sweeping across the globe, city lights going dark continent by continent, aurora of gold in the atmosphere, [STYLE], blinding molten gold highlights
```
- **Animation** : `golden ring slowly spreading, city lights blinking out`
- **VO** : « La lumière a traversé le monde. Toutes les villes se sont éteintes. Et quelque chose, en nous, a changé. »
- **Son** : grésillement électrique qui meurt, souffle cosmique

---

### SÉQUENCE 8 · L'AUBE (3:35 – 4:05)

**Musique** : thème principal de Heroes Dawn, lumineux.

#### S42 · L'edenite · 5 s
- **Cadrage** : plan moyen, contre-plongée
- **Image** :
```
ruins of a Paris street at dawn, faceted luminous crystals of a new teal-gold mineral growing from the rubble and cracked stone, damaged Haussmann buildings, [STYLE], warm amber dawn light, gold accents
```
- **Animation** : `crystals pulsing softly with light, dust floating in dawn rays`
- **VO** : « Paris en porte encore les cicatrices. Et un minerai que la Terre n'avait jamais connu : l'edenite. »
- **Son** : vibration cristalline, oiseaux lointains

#### S43 · L'éveil *(optionnel)* · 5 s
- **Cadrage** : plan rapproché
- **Image** :
```
an ordinary young woman in a crowded subway car staring at her own hand glowing with faint light, other passengers unaware, [STYLE], warm amber dawn light
```
- **Animation** : `glow pulsing softly in her palm, subtle train motion`
- **VO** : « Partout, des femmes et des hommes se sont éveillés. »
- **Son** : métro, battement de cœur

#### S44 · La fondation · 6 s
- **Cadrage** : plan large, contre-plongée héroïque
- **Image** :
```
[RAYNER_2026] standing on a rooftop at sunrise with [WEST], [GUERIN] and [BRONSON] behind her, a golden four-pointed star emblem on her shoulder, the city skyline glowing, [STYLE], warm amber dawn light, gold accents
```
- **Animation** : `sun rising slowly, coats moving in the wind, lens flare`
- **VO** : « Alors j'ai fondé l'Aube. Pour les trouver. Pour les protéger. »
- **Son** : montée du thème

#### S45 · Cinq ans plus tard · 5 s
- **Cadrage** : plan large, de dos
- **Image** :
```
two young agents seen from behind, a man and a woman in dark faceted hero jackets with golden four-pointed star emblems, standing on a ledge facing a sprawling city at sunrise, [STYLE], warm amber dawn light, gold accents
```
- **Animation** : `slow push-in over their shoulders, sunlight spreading`
- **VO** : « Cinq ans ont passé. Et l'Aube a besoin de vous. »
- **Son** : la musique retient son souffle

#### S46 · Titre · 6 s
- **Cadrage** : centré
- **Image** :
```
a golden four-pointed star of fire igniting in darkness, faceted golden light rays, embers, [STYLE], warm amber dawn light
```
- **Animation** : `star igniting and pulsing, light rays expanding`
- **VO** : *(silence)*
- **Montage** : logo *Heroes Dawn* incrusté au montage (pas en IA)
- **Son** : impact final, résonance

---

## 6. Script de narration complet

*Pour l'enregistrement en une seule passe, ou pour vérifier le rythme.*

> Chaque aube commence dans la nuit la plus noire.
>
> En 1973, un homme a changé ce que signifiait être humain. Il s'appelait Frank Khost. Il croyait pouvoir guérir le monde.
>
> Huit ans plus tard, en pleine Guerre froide, l'Amérique a vu dans ses travaux une arme. Une dissuasion sans retombées. Adam West. Le soldat qui avait perdu un bras. Moi, Brie Rayner. Ingénieure à la NASA. John Guerin. L'artiste. Le premier à avoir survécu. Erik Bronson. Rien ne lui échappait, pas même derrière les murs. Ils nous ont appelés l'Apex Unit.
>
> L'Union soviétique a répondu avec un seul homme. Viktor Volkov, maître de la matière. Puis le bloc soviétique s'est effondré. Volkov a disparu. Et nous sommes devenus les protecteurs du monde.
>
> Trente-cinq ans plus tard, un jeune homme de vingt-cinq ans n'avait plus la force de continuer. Il s'appelait Eda Ryle. Les médecins ont parlé d'un malaise. Son cœur ne battait plus qu'une fois par minute. Pendant vingt-quatre heures, il est resté suspendu entre la vie et la mort. Puis il s'est réveillé. Intact. Et brûlant d'un feu noir et vert. Il a choisi de le cacher.
>
> Un homme est venu le chercher. Orik Kovvolt, chef de la X Society. Il voulait effacer les méta-humains de la surface de la Terre. Pour forcer Eda à le suivre, il lui a tout pris. Eda l'a traqué. Et sous ce visage, il a trouvé Viktor Volkov. Leur combat était un rituel. Leurs sangs mêlés ont réveillé une machine. Et le démon qui vivait en Volkov depuis des décennies s'est libéré. Mephisto.
>
> Les Apex l'ont rejeté. L'AMIF l'a traqué. J'ai été la seule à le croire. Dans la fuite, nous sommes devenus bien plus que des alliés. Dans l'Oregon, Mephisto a libéré les oubliés du Protocole Khost. Il visait le cœur de la foi. Là-bas, j'ai été touchée. Et Eda a tout réduit en cendres. Mephisto lui a révélé la vérité. Il était le Phoenix Noir. Né des expériences de Volkov : Khost, du sang d'ange, et du sang de démon. Revenu une fois de plus, il a abandonné.
>
> À Paris, Mephisto a ouvert une porte vers les Enfers. Les Apex ont combattu. Moi aussi. Au moment où j'allais tomber, je l'ai appelé. Il a entendu. Il a accepté ce qu'il était. Et le Phoenix Noir est devenu le Phoenix Doré. Il a arraché Mephisto au corps de Volkov. Puis il a franchi la porte.
>
> La lumière a traversé le monde. Toutes les villes se sont éteintes. Et quelque chose, en nous, a changé.
>
> Paris en porte encore les cicatrices. Et un minerai que la Terre n'avait jamais connu : l'edenite. Partout, des femmes et des hommes se sont éveillés. Alors j'ai fondé l'Aube. Pour les trouver. Pour les protéger.
>
> Cinq ans ont passé. Et l'Aube a besoin de vous.

*Environ 480 mots, soit 3 min 40 de voix à 130 mots par minute.*

---

## 7. Points à valider

- **Âge de l'Apex Unit en 2026** : créés en 1981, ils auraient 70 ans ou plus. Les prompts les montrent sans avoir vieilli (effet du Protocole Khost ?). À confirmer, car ça change tous les plans de 2026.
- **Rayner au Vatican puis à Paris** : la cinématique suit ton récit (touchée au Vatican, présente à Paris). Le mot « mortellement » est retiré de la narration pour rester cohérent.
- **Volkov et Orik** : Orik est montré comme un visage d'emprunt qui se fissure sur celui de Volkov. Si Orik est simplement Volkov vieilli, S20 et S22 sont à ajuster.
- **S45** : les deux agents sont montrés de dos, sans visage, pour qu'ils restent génériques avant le choix du joueur.
