# Illustration prompts — cinematics

One block per image: copy the whole block (prompt + negative prompt) and paste it into the generator. Content: character reference sheets, the 46 shots of the launch cinematic (from `02_Lore_Narration/05_Cinematique_Lancement.md`), and one establishing image per chapter (New York and Moscow).

## How to use

1. **Character sheets first**: generate and keep one sheet per character, then use it as the character reference on every shot where they appear. This is what keeps faces consistent.
2. **Style reference**: generate S37 (the Golden Phoenix) first; if the render suits you, use it as the style reference for all other shots.
3. Generate 4 variants per shot and keep the best. Save the image as `<shot id>.jpg` (for example `S02.jpg`) in `03_Art_UI_UX/Illustrations/Cinematique/`, and chapter images as `<chapter id>.jpg` in `03_Art_UI_UX/Illustrations/Chapitres/`.
4. Every shot has its own animation prompt (image-to-video, 5 s clips). The block under it already ends with the safety rule `subtle motion only, characters nearly still, no morphing, no camera shake, 5 seconds`.
5. Sensitive shots (marked) use the reinforced negative prompt: never show the weapon or the act, only the state of mind before and the hospital after.

## Character sheets

Each sheet is generated as a full-body and face reference on a neutral background.

### KHOST

```
character reference sheet, front view and three-quarter view, full body and face close-up, neutral dark grey background, Frank Khost, 1970s geneticist in his fifties, gaunt face, thin grey hair combed back, round wire glasses, white lab coat over brown knit vest, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

### WEST

```
character reference sheet, front view and three-quarter view, full body and face close-up, neutral dark grey background, Adam West, American army veteran in his thirties, square jaw, buzz cut, left arm missing at the shoulder, olive and grey military fatigues, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

### RAYNER

```
character reference sheet, front view and three-quarter view, full body and face close-up, neutral dark grey background, Brie Rayner, British woman in her early thirties, sharp intelligent eyes with faint electric blue glow, short auburn bob, blue and white NASA engineer flight jacket, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

### RAYNER_2026

```
character reference sheet, front view and three-quarter view, full body and face close-up, neutral dark grey background, Brie Rayner, same face, ageless, short auburn bob, long blue and white tactical coat with glowing electric circuit lines, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

### GUERIN

```
character reference sheet, front view and three-quarter view, full body and face close-up, neutral dark grey background, John Guerin, French painter in his late twenties, long dark curly hair, thin mustache, paint-stained shirt under black and gold coat, skin with faint shifting cellular patterns, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

### BRONSON

```
character reference sheet, front view and three-quarter view, full body and face close-up, neutral dark grey background, Erik Bronson, Swedish sniper in his thirties, braided pale blond beard, ice-blue eyes, white winter camouflage, long scoped rifle, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

### VOLKOV

```
character reference sheet, front view and three-quarter view, full body and face close-up, neutral dark grey background, Viktor Volkov, Soviet super-soldier in his forties, massive build, shaved head, scar across the lip, heavy grey and red Soviet greatcoat, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

### ORIK

```
character reference sheet, front view and three-quarter view, full body and face close-up, neutral dark grey background, Orik Kovvolt, tall gaunt man in his sixties, slicked-back white hair, black tailored long coat, cold pale eyes, red X emblem on armband, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

### MEPHISTO

```
character reference sheet, front view and three-quarter view, full body and face close-up, neutral dark grey background, Mephisto, demon wearing Volkov's massive body, cracked charcoal skin with glowing red fissures, curved black horns, burning red eyes, tattered black cloak, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

### EDA

```
character reference sheet, front view and three-quarter view, full body and face close-up, neutral dark grey background, Eda Ryle, 25-year-old man, lean and pale, shaggy black hair, dark circles under his eyes, charcoal hoodie under black jacket, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

### EDA_NOIR

```
character reference sheet, front view and three-quarter view, full body and face close-up, neutral dark grey background, Eda Ryle, 25-year-old man, lean and pale, shaggy black hair, dark circles under his eyes, charcoal hoodie under black jacket, black and toxic green flames crawling over his arms and shoulders, green glowing eyes, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

### EDA_DORE

```
character reference sheet, front view and three-quarter view, full body and face close-up, neutral dark grey background, Eda Ryle, 25-year-old man, lean and pale, shaggy black hair, dark circles under his eyes, charcoal hoodie under black jacket, body wreathed in molten golden flames forming faceted armor, immense wings of golden fire, radiant gold eyes, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

## Launch cinematic (46 shots)

### Sequence 0 — OUVERTURE (0:00 – 0:06)

#### S01 · La braise · 6 s

Framing: très gros plan, centré

```
a single glowing golden ember floating in absolute darkness, faint smoke trails, tiny sparks, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality, warm amber dawn light

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

Animation prompt:

```
ember slowly drifting upward and pulsing softly, sparks fading, subtle motion only, characters nearly still, no morphing, no camera shake, 5 seconds
```

### Sequence 1 — 1973, FRANK KHOST (0:06 – 0:22)

#### S02 · Le laboratoire · 6 s

Framing: plan large, légère plongée

```
Frank Khost, 1970s geneticist in his fifties, gaunt face, thin grey hair combed back, round wire glasses, white lab coat over brown knit vest seen from behind, working alone at a cluttered 1970s genetics laboratory bench at night, single desk lamp, glass vials and microscopes, chalkboard covered with DNA diagrams, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality, muted sepia and teal color grade, film grain, archival mood

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

Animation prompt:

```
slow push-in toward the scientist, lamp light flickering faintly, dust motes floating, subtle motion only, characters nearly still, no morphing, no camera shake, 5 seconds
```

#### S03 · L'ADN · 5 s

Framing: très gros plan

```
extreme close-up of a scientist's hands holding a glass vial, a glowing blue double helix of light rising from the liquid, reflections in wire glasses, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality, muted sepia and teal color grade, film grain

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

Animation prompt:

```
helix slowly rotating and shimmering, liquid gently swirling, subtle motion only, characters nearly still, no morphing, no camera shake, 5 seconds
```

#### S04 · Le regard de Khost · 5 s *(optional)*

Framing: gros plan, contre-plongée

```
Frank Khost, 1970s geneticist in his fifties, gaunt face, thin grey hair combed back, round wire glasses, white lab coat over brown knit vest low angle close-up portrait, blue glow of the helix reflected on his round glasses, determined and obsessive expression, dark laboratory background, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality, muted sepia and teal color grade

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

Animation prompt:

```
slow push-in, glow pulsing on the lenses, subtle motion only, characters nearly still, no morphing, no camera shake, 5 seconds
```

### Sequence 2 — 1981, L'APEX UNIT (0:22 – 0:55)

#### S05 · La salle de guerre · 5 s

Framing: plan large, plongée

```
Cold War military war room in 1981, generals in olive uniforms standing around a large table map of the world lit from below, cigarette smoke, red telephones, dark bunker, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality, muted sepia and teal color grade, film grain

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

Animation prompt:

```
smoke drifting slowly, slow pull-back, map lights blinking, subtle motion only, characters nearly still, no morphing, no camera shake, 5 seconds
```

#### S06 · Adam West · 5 s

Framing: plan moyen, contre-plongée

```
Adam West, American army veteran in his thirties, square jaw, buzz cut, left arm missing at the shoulder, olive and grey military fatigues in a concrete military testing chamber, touching a concrete pillar with his right hand while a new left arm forms from faceted grey concrete fragments at his shoulder, debris floating, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality, muted sepia and teal color grade

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

Animation prompt:

```
concrete fragments slowly drifting toward his shoulder, dust falling, subtle motion only, characters nearly still, no morphing, no camera shake, 5 seconds
```

#### S07 · Brie Rayner · 5 s

Framing: gros plan, trois-quarts

```
Brie Rayner, British woman in her early thirties, sharp intelligent eyes with faint electric blue glow, short auburn bob, blue and white NASA engineer flight jacket in a NASA control room full of 1980s monitors, thin electric blue arcs dancing between her fingertips, data reflected in her glowing eyes, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality, muted sepia and teal color grade

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

Animation prompt:

```
electric arcs crackling softly between fingers, monitor light flickering, subtle motion only, characters nearly still, no morphing, no camera shake, 5 seconds
```

#### S08 · John Guerin · 5 s

Framing: plan rapproché

```
John Guerin, French painter in his late twenties, long dark curly hair, thin mustache, paint-stained shirt under black and gold coat, skin with faint shifting cellular patterns in an artist studio turned laboratory, half of his face shifting into faceted cellular patterns like a living sculpture, canvases in background, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality, muted sepia and teal color grade

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

Animation prompt:

```
faceted patterns slowly rippling across his skin, paint dust floating, subtle motion only, characters nearly still, no morphing, no camera shake, 5 seconds
```

#### S09 · Erik Bronson · 5 s

Framing: très gros plan sur l'œil, puis profondeur

```
Erik Bronson, Swedish sniper in his thirties, braided pale blond beard, ice-blue eyes, white winter camouflage, long scoped rifle lying prone in snow with his scoped rifle, extreme close-up of his ice-blue eye glowing, through his vision we see a ghostly outline of people behind a concrete wall, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality, muted sepia and teal color grade

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

Animation prompt:

```
snowflakes falling, eye glow pulsing slowly, subtle motion only, characters nearly still, no morphing, no camera shake, 5 seconds
```

#### S10 · L'Apex Unit · 6 s

Framing: plan large, contre-plongée héroïque

```
Adam West, American army veteran in his thirties, square jaw, buzz cut, left arm missing at the shoulder, olive and grey military fatigues, Brie Rayner, British woman in her early thirties, sharp intelligent eyes with faint electric blue glow, short auburn bob, blue and white NASA engineer flight jacket, John Guerin, French painter in his late twenties, long dark curly hair, thin mustache, paint-stained shirt under black and gold coat, skin with faint shifting cellular patterns and Erik Bronson, Swedish sniper in his thirties, braided pale blond beard, ice-blue eyes, white winter camouflage, long scoped rifle standing together in a dark military hangar, low angle heroic group shot, searchlight beams behind them, olive and grey military palette, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality, muted sepia and teal color grade

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

Animation prompt:

```
slow push-in, searchlights sweeping, dust in light beams, subtle motion only, characters nearly still, no morphing, no camera shake, 5 seconds
```

### Sequence 3 — VOLKOV ET 1991 (0:55 – 1:15)

#### S11 · Viktor Volkov · 6 s

Framing: plan large, contre-plongée

```
Viktor Volkov, Soviet super-soldier in his forties, massive build, shaved head, scar across the lip, heavy grey and red Soviet greatcoat standing in a snowy Soviet military base at night, steel girders and tanks bending and reshaping around him like liquid metal, red floodlights, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality, muted sepia and teal color grade, red accents

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

Animation prompt:

```
metal slowly warping around him, snow swirling, subtle motion only, characters nearly still, no morphing, no camera shake, 5 seconds
```

#### S12 · La chute du bloc · 5 s *(optional)*

Framing: plan large

```
crowd at night celebrating on top of a crumbling concrete wall, fireworks, red flags falling, silhouettes, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality, muted sepia and teal color grade, film grain

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

Animation prompt:

```
fireworks blooming slowly, flags drifting down, subtle motion only, characters nearly still, no morphing, no camera shake, 5 seconds
```

#### S13 · La disparition · 4 s

Framing: plan moyen

```
an abandoned heavy grey and red Soviet greatcoat lying alone in fresh snow in an empty frozen courtyard, footprints vanishing, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality, muted sepia and teal color grade

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

Animation prompt:

```
snow slowly covering the coat, wind, subtle motion only, characters nearly still, no morphing, no camera shake, 5 seconds
```

#### S14 · Les protecteurs · 5 s

Framing: plan large, contre-plongée

```
Adam West, American army veteran in his thirties, square jaw, buzz cut, left arm missing at the shoulder, olive and grey military fatigues, Brie Rayner, British woman in her early thirties, sharp intelligent eyes with faint electric blue glow, short auburn bob, blue and white NASA engineer flight jacket, John Guerin, French painter in his late twenties, long dark curly hair, thin mustache, paint-stained shirt under black and gold coat, skin with faint shifting cellular patterns and Erik Bronson, Swedish sniper in his thirties, braided pale blond beard, ice-blue eyes, white winter camouflage, long scoped rifle on a rooftop above a 1990s city at night, watching over it, city lights below, heroic silhouettes with rim light, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality, muted sepia and teal color grade

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

Animation prompt:

```
slow lateral parallax, city lights twinkling, subtle motion only, characters nearly still, no morphing, no camera shake, 5 seconds
```

### Sequence 4 — 2026, PHOENIX BIRTH (1:15 – 1:42)

#### S15 · Eda seul · 6 s *(sensitive: reinforced negative prompt)*

Framing: plan large, de dos

```
Eda Ryle, 25-year-old man, lean and pale, shaggy black hair, dark circles under his eyes, charcoal hoodie under black jacket seen from behind, sitting alone on the floor of a dark small apartment, back against the bed, rain streaming down the window, city lights blurred outside, empty and heavy atmosphere, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality, cold steel blue color grade

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face, gore, visible wounds, blood splatter, gun, weapon pointed at head, corpses, dead bodies
```

Animation prompt:

```
rain trickling down the window, slow push-in, he stays still, subtle motion only, characters nearly still, no morphing, no camera shake, 5 seconds
```

#### S16 · L'hôpital · 5 s *(sensitive: reinforced negative prompt)*

Framing: gros plan

```
close-up of a hospital heart monitor showing an almost flat line with a single faint spike, frost forming on the screen edges, cold blue room behind, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality, cold steel blue color grade

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face, gore, visible wounds, blood splatter, gun, weapon pointed at head, corpses, dead bodies
```

Animation prompt:

```
single slow pulse on the monitor, frost spreading slightly, subtle motion only, characters nearly still, no morphing, no camera shake, 5 seconds
```

#### S17 · L'animation suspendue · 5 s *(sensitive: reinforced negative prompt)*

Framing: plan moyen, plongée

```
Eda Ryle, 25-year-old man, lean and pale, shaggy black hair, dark circles under his eyes, charcoal hoodie under black jacket lying motionless on a hospital bed, pale skin covered in thin frost, eyes closed, peaceful face, cold light from a window shifting from night to day, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality, cold steel blue color grade

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face, gore, visible wounds, blood splatter, gun, weapon pointed at head, corpses, dead bodies
```

Animation prompt:

```
light slowly sweeping across the room, frost glittering, subtle motion only, characters nearly still, no morphing, no camera shake, 5 seconds
```

#### S18 · Le réveil · 5 s

Framing: très gros plan sur les yeux

```
extreme close-up of Eda Ryle, 25-year-old man, lean and pale, shaggy black hair, dark circles under his eyes, charcoal hoodie under black jacket eyes snapping open, irises glowing toxic green, reflections of black and green flames in the pupils, frost melting on his eyelashes, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality, cold steel blue color grade, sickly black and green flame accents

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

Animation prompt:

```
green glow intensifying, frost melting into droplets, subtle motion only, characters nearly still, no morphing, no camera shake, 5 seconds
```

#### S19 · Le secret · 4 s

Framing: plan rapproché

```
Eda Ryle, 25-year-old man, lean and pale, shaggy black hair, dark circles under his eyes, charcoal hoodie under black jacket in a dark rainy alley, hood up, closing his fist around a small black and green flame to hide it, worried glance over his shoulder, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality, cold steel blue color grade, sickly black and green flame accents

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

Animation prompt:

```
flame flickering between his fingers then dimming, subtle motion only, characters nearly still, no morphing, no camera shake, 5 seconds
```

### Sequence 5 — LA X SOCIETY (1:42 – 2:10)

#### S20 · Orik Kovvolt · 5 s

Framing: gros plan, contre-plongée, visage à moitié dans l'ombre

```
Orik Kovvolt, tall gaunt man in his sixties, slicked-back white hair, black tailored long coat, cold pale eyes, red X emblem on armband standing in a dark empty warehouse, half of his face in shadow, cold smile, red emblem glowing on his armband, armed men in silhouette behind him, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality, cold steel blue color grade, red accents

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

Animation prompt:

```
slow push-in, dust in a single beam of light, subtle motion only, characters nearly still, no morphing, no camera shake, 5 seconds
```

#### S21 · Le prix · 6 s *(sensitive: reinforced negative prompt)*

Framing: plan large, de dos

```
Eda Ryle, 25-year-old man, lean and pale, shaggy black hair, dark circles under his eyes, charcoal hoodie under black jacket kneeling from behind in front of a suburban family house burning at night, flames from every window, smoke rising, his silhouette tiny against the fire, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality, cold steel blue color grade, orange fire glow

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face, gore, visible wounds, blood splatter, gun, weapon pointed at head, corpses, dead bodies
```

Animation prompt:

```
flames flickering, embers rising, smoke drifting, subtle motion only, characters nearly still, no morphing, no camera shake, 5 seconds
```

#### S22 · Le masque tombe · 5 s

Framing: plan rapproché, champ-contrechamp dans une seule image

```
Eda Ryle, 25-year-old man, lean and pale, shaggy black hair, dark circles under his eyes, charcoal hoodie under black jacket, black and toxic green flames crawling over his arms and shoulders, green glowing eyes in the foreground gripping Orik Kovvolt, tall gaunt man in his sixties, slicked-back white hair, black tailored long coat, cold pale eyes, red X emblem on armband by the collar, Orik's face cracking like porcelain to reveal the scarred face of Viktor Volkov, Soviet super-soldier in his forties, massive build, shaved head, scar across the lip, heavy grey and red Soviet greatcoat underneath, black and green fire between them, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality, cold steel blue color grade, sickly black and green flame accents

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

Animation prompt:

```
cracks slowly spreading across the face, flames swirling, subtle motion only, characters nearly still, no morphing, no camera shake, 5 seconds
```

#### S23 · Le rituel · 5 s *(sensitive: reinforced negative prompt)*

Framing: plan large, plongée

```
an ancient occult machine of black iron gears and stone circles in an underground chamber, channels of glowing red light flowing through carved runes toward its core, two figures fighting on the circle, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality, cold steel blue color grade, red glow

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face, gore, visible wounds, blood splatter, gun, weapon pointed at head, corpses, dead bodies
```

Animation prompt:

```
red light flowing through the runes, gears slowly turning, subtle motion only, characters nearly still, no morphing, no camera shake, 5 seconds
```

#### S24 · Mephisto · 6 s

Framing: plan moyen, contre-plongée extrême

```
Mephisto, demon wearing Volkov's massive body, cracked charcoal skin with glowing red fissures, curved black horns, burning red eyes, tattered black cloak rising from the machine, red fissures splitting the body, horns tearing through, crimson smoke pouring out, demonic eyes burning, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality, cold steel blue color grade, crimson glow

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

Animation prompt:

```
crimson smoke billowing upward, fissures pulsing with light, subtle motion only, characters nearly still, no morphing, no camera shake, 5 seconds
```

### Sequence 6 — LES REBELLES (2:10 – 2:55)

#### S25 · La traque · 5 s

Framing: plan large, contre-plongée

```
Eda Ryle, 25-year-old man, lean and pale, shaggy black hair, dark circles under his eyes, charcoal hoodie under black jacket and Brie Rayner, same face, ageless, short auburn bob, long blue and white tactical coat with glowing electric circuit lines running across a wet highway overpass at night, helicopter searchlights pinning them, armed tactical agents in olive and grey, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality, cold steel blue color grade

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

Animation prompt:

```
searchlight beams sweeping, rain falling, slight parallax, subtle motion only, characters nearly still, no morphing, no camera shake, 5 seconds
```

#### S26 · Le refuge · 5 s

Framing: gros plan à deux

```
Eda Ryle, 25-year-old man, lean and pale, shaggy black hair, dark circles under his eyes, charcoal hoodie under black jacket and Brie Rayner, same face, ageless, short auburn bob, long blue and white tactical coat with glowing electric circuit lines in a dim safehouse, foreheads touching, eyes closed, a single candle between them, intimate and tender, warm candlelight against cold shadows, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality, cold steel blue color grade

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

Animation prompt:

```
candle flame flickering gently, faint breathing motion, subtle motion only, characters nearly still, no morphing, no camera shake, 5 seconds
```

#### S27 · Le complexe de l'Oregon · 5 s

Framing: plan large

```
vast underground military complex in a forest mountain, rows of shattered containment tanks, deformed humanoid creatures with twisted faceted bodies crawling out, emergency red lights, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality, cold steel blue color grade, red alarm lights

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

Animation prompt:

```
red lights rotating, mist spilling from broken tanks, creatures barely moving, subtle motion only, characters nearly still, no morphing, no camera shake, 5 seconds
```

#### S28 · Le Vatican · 5 s

Framing: plan très large

```
a vast Renaissance basilica dome and colonnade square under siege at dusk, explosions, tactical helicopters, metahumans clashing in the air, smoke columns, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality, cold steel blue color grade, orange explosions

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

Animation prompt:

```
smoke rising, slow push-in, distant explosions flashing, subtle motion only, characters nearly still, no morphing, no camera shake, 5 seconds
```

#### S29 · Rayner touchée · 5 s *(sensitive: reinforced negative prompt)*

Framing: plan moyen, ralenti

```
Brie Rayner, same face, ageless, short auburn bob, long blue and white tactical coat with glowing electric circuit lines falling backward in slow motion amid debris, her electric aura shattering into fading blue sparks, eyes wide, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality, cold steel blue color grade

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face, gore, visible wounds, blood splatter, gun, weapon pointed at head, corpses, dead bodies
```

Animation prompt:

```
blue sparks slowly dispersing, debris floating, extreme slow motion, subtle motion only, characters nearly still, no morphing, no camera shake, 5 seconds
```

#### S30 · La colère · 5 s

Framing: plan très large, contre-plongée

```
Eda Ryle, 25-year-old man, lean and pale, shaggy black hair, dark circles under his eyes, charcoal hoodie under black jacket, black and toxic green flames crawling over his arms and shoulders, green glowing eyes screaming at the center of an immense firestorm of black and green flames engulfing the basilica, dome collapsing, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality, cold steel blue color grade, sickly black and green flame accents

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

Animation prompt:

```
flames roaring outward, debris rising, embers, subtle motion only, characters nearly still, no morphing, no camera shake, 5 seconds
```

#### S31 · La révélation · 5 s *(sensitive: reinforced negative prompt)*

Framing: gros plan

```
Mephisto, demon wearing Volkov's massive body, cracked charcoal skin with glowing red fissures, curved black horns, burning red eyes, tattered black cloak behind Eda Ryle, 25-year-old man, lean and pale, shaggy black hair, dark circles under his eyes, charcoal hoodie under black jacket, black and toxic green flames crawling over his arms and shoulders, green glowing eyes, whispering into his ear, a long black blade emerging through Eda's chest seen only in silhouette against fire, Eda's face frozen in shock, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality, cold steel blue color grade, crimson glow

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face, gore, visible wounds, blood splatter, gun, weapon pointed at head, corpses, dead bodies
```

Animation prompt:

```
embers drifting, flames slowly dimming around Eda, subtle motion only, characters nearly still, no morphing, no camera shake, 5 seconds
```

#### S32 · L'abandon · 5 s

Framing: plan large, plongée

```
Eda Ryle, 25-year-old man, lean and pale, shaggy black hair, dark circles under his eyes, charcoal hoodie under black jacket sitting on the floor of an abandoned safehouse, head down, arms on knees, extinguished candle beside him, grey morning light through broken blinds, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality, cold steel blue color grade

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

Animation prompt:

```
dust floating in light rays, very slow push-in, subtle motion only, characters nearly still, no morphing, no camera shake, 5 seconds
```

### Sequence 7 — PARIS (2:55 – 3:35)

#### S33 · La porte des Enfers · 6 s

Framing: plan très large

```
the Eiffel Tower at night channeling crimson lightning into a swirling wormhole opening in the sky above Paris, hellfire visible through the portal, city below in chaos, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality, crimson hellfire sky

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

Animation prompt:

```
wormhole slowly rotating, lightning pulsing along the tower, subtle motion only, characters nearly still, no morphing, no camera shake, 5 seconds
```

#### S34 · La bataille · 5 s

Framing: plan large, contre-plongée

```
Adam West, American army veteran in his thirties, square jaw, buzz cut, left arm missing at the shoulder, olive and grey military fatigues, John Guerin, French painter in his late twenties, long dark curly hair, thin mustache, paint-stained shirt under black and gold coat, skin with faint shifting cellular patterns and Erik Bronson, Swedish sniper in his thirties, braided pale blond beard, ice-blue eyes, white winter camouflage, long scoped rifle fighting demonic creatures on the Trocadero esplanade, the Eiffel Tower and portal behind them, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality, crimson hellfire sky

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

Animation prompt:

```
slight parallax, sparks and embers flying, subtle motion only, characters nearly still, no morphing, no camera shake, 5 seconds
```

#### S35 · L'appel · 5 s

Framing: gros plan au sol

```
Brie Rayner, same face, ageless, short auburn bob, long blue and white tactical coat with glowing electric circuit lines lying wounded on the ground among rubble, reaching one hand toward the sky, faint blue sparks around her fingers, tears, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality, crimson hellfire sky

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

Animation prompt:

```
sparks flickering weakly, embers falling, subtle motion only, characters nearly still, no morphing, no camera shake, 5 seconds
```

#### S36 · La transmutation · 5 s

Framing: très gros plan sur les yeux

```
extreme close-up of Eda Ryle, 25-year-old man, lean and pale, shaggy black hair, dark circles under his eyes, charcoal hoodie under black jacket eyes, black and green flames in his irises turning into molten gold, tear evaporating into golden sparks, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality, blinding molten gold highlights

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

Animation prompt:

```
green slowly giving way to gold, sparks rising, subtle motion only, characters nearly still, no morphing, no camera shake, 5 seconds
```

#### S37 · Le Phoenix Doré · 6 s

Framing: plan large, contre-plongée héroïque

```
Eda Ryle, 25-year-old man, lean and pale, shaggy black hair, dark circles under his eyes, charcoal hoodie under black jacket, body wreathed in molten golden flames forming faceted armor, immense wings of golden fire, radiant gold eyes rising above the Paris rooftops, immense wings of golden fire spread wide, low angle heroic shot, crimson sky splitting with golden light behind him, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality, blinding molten gold highlights

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

Animation prompt:

```
wings slowly unfolding, golden embers swirling, light rays, subtle motion only, characters nearly still, no morphing, no camera shake, 5 seconds
```

#### S38 · L'âme arrachée · 5 s

Framing: plan rapproché

```
Eda Ryle, 25-year-old man, lean and pale, shaggy black hair, dark circles under his eyes, charcoal hoodie under black jacket, body wreathed in molten golden flames forming faceted armor, immense wings of golden fire, radiant gold eyes tearing a writhing crimson spectral demon soul out of Viktor Volkov, Soviet super-soldier in his forties, massive build, shaved head, scar across the lip, heavy grey and red Soviet greatcoat's body, golden fire versus red smoke, Volkov's body collapsing, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality, blinding molten gold highlights, crimson smoke

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

Animation prompt:

```
red smoke stretching and being pulled, golden fire flaring, subtle motion only, characters nearly still, no morphing, no camera shake, 5 seconds
```

#### S39 · Le portail · 5 s

Framing: plan très large, Eda minuscule

```
Eda Ryle, 25-year-old man, lean and pale, shaggy black hair, dark circles under his eyes, charcoal hoodie under black jacket, body wreathed in molten golden flames forming faceted armor, immense wings of golden fire, radiant gold eyes tiny silhouette flying into the vast swirling hellfire wormhole above the Eiffel Tower, golden trail behind him, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality, crimson hellfire sky, blinding molten gold highlights

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

Animation prompt:

```
slow push toward the portal, golden trail lingering, subtle motion only, characters nearly still, no morphing, no camera shake, 5 seconds
```

#### S40 · La Supernova · 5 s

Framing: plan centré

```
a colossal white and gold supernova implosion erupting inside a hellish crimson dimension, shockwave rings of golden light, demonic landscape disintegrating, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality, blinding molten gold highlights

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

Animation prompt:

```
light expanding outward slowly, shockwave rings, subtle motion only, characters nearly still, no morphing, no camera shake, 5 seconds
```

#### S41 · Le blackout · 6 s

Framing: vue orbitale

```
planet Earth seen from orbit at night, a golden shockwave ring sweeping across the globe, city lights going dark continent by continent, aurora of gold in the atmosphere, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality, blinding molten gold highlights

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

Animation prompt:

```
golden ring slowly spreading, city lights blinking out, subtle motion only, characters nearly still, no morphing, no camera shake, 5 seconds
```

### Sequence 8 — L'AUBE (3:35 – 4:05)

#### S42 · L'edenite · 5 s

Framing: plan moyen, contre-plongée

```
ruins of a Paris street at dawn, faceted luminous crystals of a new teal-gold mineral growing from the rubble and cracked stone, damaged Haussmann buildings, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality, warm amber dawn light, gold accents

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

Animation prompt:

```
crystals pulsing softly with light, dust floating in dawn rays, subtle motion only, characters nearly still, no morphing, no camera shake, 5 seconds
```

#### S43 · L'éveil *(optionnel)* · 5 s

Framing: plan rapproché

```
an ordinary young woman in a crowded subway car staring at her own hand glowing with faint light, other passengers unaware, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality, warm amber dawn light

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

Animation prompt:

```
glow pulsing softly in her palm, subtle train motion, subtle motion only, characters nearly still, no morphing, no camera shake, 5 seconds
```

#### S44 · La fondation · 6 s

Framing: plan large, contre-plongée héroïque

```
Brie Rayner, same face, ageless, short auburn bob, long blue and white tactical coat with glowing electric circuit lines standing on a rooftop at sunrise with Adam West, American army veteran in his thirties, square jaw, buzz cut, left arm missing at the shoulder, olive and grey military fatigues, John Guerin, French painter in his late twenties, long dark curly hair, thin mustache, paint-stained shirt under black and gold coat, skin with faint shifting cellular patterns and Erik Bronson, Swedish sniper in his thirties, braided pale blond beard, ice-blue eyes, white winter camouflage, long scoped rifle behind her, a golden four-pointed star emblem on her shoulder, the city skyline glowing, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality, warm amber dawn light, gold accents

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

Animation prompt:

```
sun rising slowly, coats moving in the wind, lens flare, subtle motion only, characters nearly still, no morphing, no camera shake, 5 seconds
```

#### S45 · Cinq ans plus tard · 5 s

Framing: plan large, de dos

```
two young agents seen from behind, a man and a woman in dark faceted hero jackets with golden four-pointed star emblems, standing on a ledge facing a sprawling city at sunrise, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality, warm amber dawn light, gold accents

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

Animation prompt:

```
slow push-in over their shoulders, sunlight spreading, subtle motion only, characters nearly still, no morphing, no camera shake, 5 seconds
```

#### S46 · Titre · 6 s

Framing: centré

```
a golden four-pointed star of fire igniting in darkness, faceted golden light rays, embers, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality, warm amber dawn light

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

Animation prompt:

```
star igniting and pulsing, light rays expanding, subtle motion only, characters nearly still, no morphing, no camera shake, 5 seconds
```

## Chapter openers (8 images)

One establishing image per chapter, shown when the chapter starts. The player character is a silhouette so the same image works for any hero build.

### New York · Chapitre 1 — Brooklyn — `ny_ch1_brooklyn`

```
backlit silhouette of Agent D, a young man with wavy mid-length hair, rectangular glasses and a faceted hero jacket marked with a small golden four-pointed star standing on a rusted dock crane arm, Red Hook docks at dusk with stacked containers, the Brooklyn Bridge and the burning Manhattan skyline across the water, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality, gang-ridden harbor, harbor fog and drifting embers, dock rust orange, harbor steel blue and ember red color palette

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

### New York · Chapitre 2 — Queens — `ny_ch2_queens`

```
backlit silhouette of Agent D, a young man with wavy mid-length hair, rectangular glasses and a faceted hero jacket marked with a small golden four-pointed star crossing an overpass, a hand glowing violet, elevated subway line in Queens above a crowded night market, neon signs, a Unisphere globe frame in the distance, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality, tense street life, rain-slick neon and steam from food carts, neon magenta, subway green and wet asphalt black color palette

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

### New York · Chapitre 3 — Harlem — `ny_ch3_harlem`

```
backlit silhouette of Agent D, a young man with wavy mid-length hair, rectangular glasses and a faceted hero jacket marked with a small golden four-pointed star sketching glowing shapes in the air on a brownstone stoop, Harlem brownstone street with murals, a church steeple and fire escapes, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality, warm and wounded, golden hour smoke between the buildings, brownstone red, mural gold and dusk indigo color palette

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

### New York · Chapitre 4 — Manhattan — `ny_ch4_manhattan`

```
backlit silhouette of Agent D, a young man with wavy mid-length hair, rectangular glasses and a faceted hero jacket marked with a small golden four-pointed star on a skyscraper ledge facing the camera, city lights below, Midtown Manhattan canyon of glass towers and a cracked Times Square billboard wall, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality, decisive and cold, helicopter searchlights cutting the smoke, glass blue, searchlight white and hazard orange color palette

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

### Moscou · Chapitre 1 — Sergueiv — `mos_ch1_sergueiv`

```
backlit silhouette of Agent M, a young woman with long wavy platinum hair, geometric glasses and a short leather coat marked with a small golden four-pointed star standing at the gate of an old monastery, Sergiyev Posad monastery with blue and gold onion domes, snowy walls and a futuristic maglev line behind it, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality, quiet watchfulness, snowfall and floating holographic icons, monastery blue, old gold and snow white color palette

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

### Moscou · Chapitre 2 — Kapotnya — `mos_ch2_kapotnya`

```
backlit silhouette of Agent M, a young woman with long wavy platinum hair, geometric glasses and a short leather coat marked with a small golden four-pointed star walking along a pipeline gantry, Kapotnya refinery district with flare stacks and steaming pipes at night, a cold smog sky, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality, oppressive industrial dread, orange flare light on freezing fog, flare orange, smog grey and toxic teal color palette

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

### Moscou · Chapitre 3 — Sadovoye Koltso — `mos_ch3_sadovoye`

```
backlit silhouette of Agent M, a young woman with long wavy platinum hair, geometric glasses and a short leather coat marked with a small golden four-pointed star on a bridge over the ring road, the Garden Ring boulevard at night, Stalinist skyscrapers glowing with holograms, endless lines of headlights, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality, glittering and watched, oligarch towers looming overhead, oligarch gold, hologram cyan and midnight violet color palette

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```

### Moscou · Chapitre 4 — La Place Rouge — `mos_ch4_place_rouge`

```
backlit silhouette of Agent M, a young woman with long wavy platinum hair, geometric glasses and a short leather coat marked with a small golden four-pointed star facing the Kremlin walls, glasses glowing gold, Red Square with the Kremlin towers, Saint Basil's domes and a cobblestone square emptied by a curfew, stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on faces, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality, final showdown stillness, a red sunset and slow snow, kremlin red, cobblestone grey and sunset gold color palette

Negative prompt: photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face
```
