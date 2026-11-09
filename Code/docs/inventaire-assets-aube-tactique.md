# Inventaire des assets — Aube tactique (existant et manquant)

Date : 2026-11-09. Sources : dossier `Assets/_Project/Art`, `Assets/_Project/Resources`, `Assets/_Project/Audio`, `coverage.json` du kit (21 planches, 84 vues) et lecture visuelle des planches 06, 07 et 09.

**Fiabilité de la liste « manquant » (mise à jour après comparaison visuelle) :** j'ai ouvert l'image de référence et la capture du jeu pour **15 planches sur 21** : 01, 02, 03, 04, 05, 06, 07, 08, 09, 10 (boutique, comparée plus tôt), 11, 12, 13, 14 et 17. Pour 12, 13, 14 et 17, je n'ai regardé qu'une capture sur quatre (respectivement Pass, Tournoi, Audio, Analyse ADN).
**Les 6 dernières planches (15, 16, 18, 19, 20, 21) ont ensuite été ouvertes** (section 2.6). Limite : il n'existe pas de capture du jeu pour la plupart de leurs vues. J'ai pu comparer à une capture réelle : 15 (Notifications seulement), 18 (Infiltration seulement) et 20 (introduction du mini-jeu ADN). Pour 16, 19 (pêche, potion, arène, dialogue à choix) et 21, **aucune capture n'existe** : seuls les besoins en assets sont listés, pas les écarts de rendu.
Le fichier `06-activite-peche.png` du dossier de captures montre en réalité l'introduction du *Rythme*, pas la pêche.
La section 2.5 rapporte ce que la comparaison a montré ; elle corrige aussi la ligne « portraits d'agents » de la section 1.3.

Convention : tous les PNG du kit sont des sprites UI importés (SVG source dans `Sources/`). Les illustrations sont des JPG pleins cadre (sans transparence).

---

## 1. Ce qui existe

### 1.1 Illustrations de production (`Illustrations/Production`, 17)
| Asset | Description |
|---|---|
| `hq-new-york` | Peinture panoramique du QG de New York à l'aube (écran titre, chargement, QG). |
| `hq-moscow` | Même rôle pour le QG de Moscou. |
| `combat-red-hook` | Quais de Red Hook au crépuscule, ponts et grues : fond du combat et du plateau. |
| `room-operations` / `room-workshop` / `room-grimoire` / `room-inventory` / `room-register` | Les cinq tuiles illustrées du QG (opérations, atelier, grimoire, inventaire, registre). |
| `shop-booster` | Pack de cartes de la boutique. |
| `mini-dna_analysis`, `mini-rhythm`, `mini-crate_pick`, `mini-gears`, `mini-free_throw`, `mini-infiltration`, `mini-rescue_engineering`, `mini-snow_survival` | Une illustration de présentation par mini-jeu (écran d'introduction). |

### 1.2 Illustrations générées (`Illustrations/Generated`, 32)
| Groupe | Assets | Description |
|---|---|---|
| Décors | `background-paris-supernova` | Paris au moment de la Supernova (introduction). |
| | `background-map-new-york` | Carte illustrée de New York (une seule image). |
| | `background-board-red-hook` | Vue de dessus de Red Hook : fond du plateau. |
| | `background-hero-bulwark`, `background-hero-rooftops` | Fonds des mini-jeux héros (Rempart, Course des toits). |
| | `background-activity-arena`, `-fishing`, `-potion` | Fonds des activités. |
| Ennemis | `enemy-thug`, `-brute`, `-brick`, `-sniper`, `-street-pyro`, `-crocodile`, `-canal-chief`, `-bridge-titan`, `-amif-trooper`, `-amif-marksman`, `-amif-captain` | Portraits pleins cadre des 11 ennemis (le Titan du pont est le boss). |
| Ressources | `resource-bio-pink`, `-elemental-cyan`, `-matter-amber`, `-perception-purple` | Cristaux des quatre familles (rose, cyan, ambre, violet). |
| Boutique | `shop-credits-small/medium/large`, `shop-gems-small/medium/large`, `shop-gift-daily`, `shop-pack-mission`, `shop-pack-operation` | Tas de crédits, gemmes, cadeau du jour, deux packs de cartes distincts. |

### 1.3 Portraits d'agents (`Illustrations/Agents` + `Resources/Illustrations/Agents`, 32)
**Attention : ces portraits ne sont pas dans le style des planches de référence (voir 2.5 A).** JPG à fond plein, buste ou mi-corps : ada_okonkwo, agent_d, agent_m, amara_hughes, anna_petrov, astrid_hallstrom, celeste_moreau, clement_lefevre, darius_brooks, eero_nieminen, fatou_diallo, freya_lindqvist, giulia_rossi, haruto_nakamura, ilona_varga, ji_hoon_park, kwame_abara, louis_dubois, lucas_mendes, marco_bellini, mateo_vasquez, mei_lin_chen, noah_kaplan, odile_ferrand, rin_takeda, sam_okafor, severine_morel, tunde_oyelaran, victor_sorel, yelena_ivanova, youssef_haddad, zofia_kowalczyk.
Les cadrages « visage » (`Portraits/*-head.asset`, `*-bust.asset`, 64 fichiers) décrivent comment recadrer chaque illustration en portrait rond ou carré.

### 1.4 Cartes, objets, divers (`Resources/Illustrations`)
| Dossier | Contenu |
|---|---|
| `Cards` (48) | Une illustration par carte de compétence (ember, inferno, storm, bulwark_charge, frost_wall, time_rift…). |
| `Items` (10) | adrenaline_patch, apex_module, kevlar_vest, phoenix_core, reactive_plating, riot_shield, running_shoes, steel_knuckles, time_shard, titan_gauntlets. |
| `Characters` (19) | char_01 à char_19 : personnages génériques. |
| `Cryptides` (9) | Kraken, mante, caprin, phalène, famine, pied-grand, lupin, pierre, suceur (repaire). |
| `Shop` (8) | booster, exchange_credits/pr/rest, gems_small/medium/large, gift. |
| `Environment` (3), `Ability` (2) | env_01 à 03 ; ability_01 et 02. |
| `Resources/Zones` (4) | `zone_ny_1_1_bg` à `zone_ny_1_4_bg` : fonds des quatre zones de New York. |
| `Resources/KeyArts` (2) | KA01_Login, KA02_Equipe. |
| `Resources/Cinematic/Art` (13) | bronson, eda_phoenix, fete, greyred, guerin, lab, logo, mephisto, moscow, nyc, paris, rayner, west. |
| `Resources/MiniGames`, `Art/MiniGames` (8 + 8) | Un fond par mini-jeu : crate_pick, dna_analysis, free_throw, gears, infiltration, rescue_engineering, rhythm, snow_survival. |

### 1.5 Marque
`HD_Logo`, `HD_Title`, `HD_Icon_Background`, `HD_Icon_Foreground`, `HD_BG_Landscape`, `HD_BG_Portrait`, `N29_Logo`, `Brand/heroes-dawn-gold|ivory|navy`.

### 1.6 Kit UI (`AubeTactiqueKit`, 922 fichiers dont 374 SVG sources)
| Dossier | Contenu précis |
|---|---|
| **Buttons** (25) | 5 variantes (primary ivoire, secondary, ghost, danger, nav) × 5 états (normal, hover, pressed, focus, disabled). Coin coupé, liseré or. |
| **Surfaces** (13) | panel-navy / ivory / muted / danger / raised / surface, surface-window, -drawer, -header, -footer, -toast, -tooltip, -tutorial. |
| **Frames** (36) | Cadres de carte par famille (bio, élémentaire, matière, perception) et par rareté (common, rare, epic, legendary, mythic) × 3 états (normal, selected, locked) ; cadres de portrait (ally, enemy, boss, bio, elemental, matter, perception, selected, empty). |
| **Controls** (20) | Barres (track + fills : health, energy, fatigue, xp, loading, danger, neutral), checkbox, radio, toggle (on/off/disabled), slider-thumb, scroll track/thumb, joystick ring/thumb. |
| **Tabs** (6) | Onglets horizontaux et verticaux × actif / inactif / désactivé. |
| **Inputs** (4) | Champs normal, focus, error, disabled. |
| **Indicators** (24) | badges (gold, navy, success, danger), route-node (normal, active, locked), target-ring, selection-corners, timeline (unit, ready, danger), spinner 12 images. |
| **Board** (8) | dice-face 1 à 6, route-line normal et active (chemin du plateau). |
| **Economy** (5) | resource-pill, price-ribbon, new-ribbon, quantity-chip, level-chip. |
| **Cards** (5) | card-back (dos étoilé), card-caption, card-caption-small, nameplate, dossier-cover. |
| **Decorations** (12) | divider-simple, divider-star, accent-line, corner-ornament, 4 family-bands, 4 pointeurs (haut, bas, gauche, droite). |
| **Overlays** (8) | gradients sombres (haut, bas, gauche, droite), voiles 25 / 50 / 75 / 90 %. |
| **FX** (8) | glow bio / élémentaire / matière / perception / or / blanc, slash-streak, spark-four-point. |
| **Masks** (3) | circle, square, chamfer. |
| **Fonts** (7) | Barlow (Regular, SemiBold) et Barlow Condensed (dont ExtraBoldItalic), licence OFL. |

**Icônes `Icons` (103, trait blanc monochrome, à teinter) :** accessibility, account, arena, armor, attack, audio, auto, back, basketball, bell, booster, boss, bulwark, calendar, chapter, check, clock, close, cloud, controls, copy, core, crates, credits, dawn, defense, dialogue, dice, dna, document, down, energy, error, eye, eye-off, family-bio, family-elemental, family-matter, family-perception, fatigue, filter, fishing, flag, forward, fragment, friend, gems, gift, graphics, grimoire, heal, health, help, home, implant, info, inventory, language, level, link, location, lock, logout, mail, map, memory, menu, minus, music, operations, pass, password, pause, play, plus, pr, profile, quest, refresh, register, rescue, retreat, rhythm, run, save, search, settings, share, shop, skip, snow, sort, speed, support, team, tool, trophy, unlock, up, voice, warning, weapon, workshop.
**`IconsColor` (11) :** credits, energy, error, family-bio, -elemental, -matter, -perception, gems, health, pr, warning (en couleur).
**`IconsGold` (16) :** booster, dawn, gift, grimoire, home, inventory, operations, pass, profile, quest, register, settings, shop, team, trophy, workshop (dorés, navigation).

### 1.7 Interface générée par le code (`Resources/UI`, 28)
Formes de base : Bracket, Burst, Bust, Circle, Cloud (brouillard), DawnStar, Diamond, Halftone, Panel, PanelFrame, PanelSmall, Ring, Rounded, RoundedSmall, Shine, Skyline, VerticalGradient, glyphes (Grimoire, Inventory, Operations, Pass, Profile, Quest, Register, Shop, Team, Workshop).

### 1.8 Audio (`Assets/_Project/Audio`)
Musiques : main-theme, qg, board-01, board-02, combat-01, combat-02, four-note-awakening. Effets : clic, retour, confirmer, erreur, toasts, dé (secoué, lancé), pion, compétence, coup, coup critique, raté, K.O., combo, alerte de combat, victoire, zone terminée, entraînement. Voix de cinématique : 30 fichiers.

---

## 2. Ce qui manque pour ressembler aux planches

Chaque ligne : écran concerné, ce que l'asset doit montrer, format conseillé.

### 2.1 Illustrations
| # | Asset à créer | Écran (planche) | Description précise | Format |
|---|---|---|---|---|
| I1 | Carte de New York par arrondissement | Carte (6.1) | Fond illustré aérien de New York avec **cinq arrondissements** (Brooklyn, Manhattan, Queens, Bronx, Staten Island) détourés : une illustration de base + un calque PNG transparent par arrondissement, aux contours des polygones du jeu, pour l'état normal et l'état sélectionné (liseré or). Aujourd'hui le jeu n'a qu'un fond unique et des polygones unis. | 1 JPG 3840×2160 + 5 PNG transparents |
| I2 | Illustration de zone (fiche de zone) | Fiche de zone (6.2) | Une image par zone, cadrage horizontal, lieu reconnaissable (Red Hook : quais, grues, entrepôt « RED HOOK LES QUAIS »). Existe pour NY 1.1 à 1.4 ; manque pour les zones des actes suivants (Moscou, etc.). | JPG 1600×900 |
| I3 | Fond de plateau par zone | Plateau (7.1, 7.2) | Vue de dessus peinte du lieu avec canal, quais, entrepôts, grue ; zones claires pour poser les cases ; ton crépuscule. Existe seulement pour Red Hook. | JPG 3840×2160 |
| I4 | Scène d'événement narratif | Événement (9.2) | Une illustration par événement (cases « événement ») : par ex. « Quais paralysés », foule de civils bloqués devant les agents, sans texte ni bulle. Aujourd'hui, la popup n'a aucune image. | JPG 1200×1400 (portrait, occupe la moitié gauche) |
| I5 | Agents détourés avec effet de pouvoir | Dialogue (9.1), Victoire (9.3), Fiche d'agent (4.2), Tutoriel (13.3) | Buste ou mi-corps **sur fond transparent**, en grand (Giulia : cheveux bruns, cape rouge sombre, éclats magenta de télékinésie). Prioritaires : les 4 agents de départ et Brie Rayner (guide). Les illustrations actuelles sont des rectangles pleins : impossible de les placer devant un décor. | PNG transparent 1024×1536 |
| I6 | Illustration par chapitre | Chapitres (6.4), Archives (3.4) | Huit résumés illustrés (New York / Moscou), un visuel par chapitre, avec variantes verrouillé (désaturé). | JPG 1200×675 ×8 |
| I7 | Scène « zone sécurisée » | Zone sécurisée (9.4) | Variante apaisée du plateau (aube sur les quais) pour la moitié gauche de l'écran. | JPG 1200×1400 |
| I8 | Introduction de combat « VS » | Intro de combat (8.1) | Bande diagonale discrète, étoile centrale à quatre rayons, couleurs alliés (ivoire) / ennemis (rouge). | PNG transparent 1920×400 |
| I9 | Visuels de mini-jeux (éléments) *(à confirmer)* | Mini-jeux (17, 18, 20) | Éléments de jeu dessinés : molécules vertes et bleues, notes et pistes du rythme, caisses, roues dentées avec sens, gardes et cônes de vision, ballon et panier, totems de neige, civils, projectiles. Ils sont aujourd'hui générés par le code. | PNG transparents, 256 px |

### 2.2 Icônes (trait blanc, 128×128, comme `Icons`)
| # | Icône | Où | Description |
|---|---|---|---|
| C1 | `handshake` | Choix « Négocier » (9.2) | Deux mains qui se serrent. |
| C2 | `shield-check` | Choix « Sécuriser » (9.2) | Bouclier avec une coche. |
| C3 | `crowd` | Choix « Évacuer les civils », case groupe (7.1) | Trois silhouettes de personnes. |
| C4 | `tent` | Case Repos (7.4), titre de la fenêtre | Tente de camp, triangle avec ouverture. |
| C5 | `mountain` | Mode difficile (9.4) | Deux sommets, un picot de neige. |
| C6 | `star-threat` (plein / vide) | Menace ★★★ (7.3) | Étoile à cinq branches, deux états. |
| C7 | `crossed-swords` | Case combat, titre « Combat élite » (7.1, 7.3) | Deux épées croisées (le jeu utilise du texte « VS »). |
| C8 | `node-crate`, `node-alert`, `node-tower`, `node-chest` | Cases du plateau (7.1) | Pictogrammes des cases : caisse (ressource), point d'exclamation (événement), pylône/tour (objectif, losange), coffre (récompense). Aujourd'hui des lettres (« $ », « MJ », « Zz », « ! »). |
| C9 | `laurel-left`, `laurel-right` | « VICTOIRE » (9.3) | Demi-couronnes de laurier dorées autour du titre. |
| C10 | `medal-1/2/3`, `crown` | Tournoi (13), Pass premium (12.1) | Médailles de rang et couronne premium. |
| C11 | `bolt-pill` | Coût PM sur les liens du plateau (7.2) | Pastille sombre arrondie avec éclair doré. Peut être faite avec `energy` ; manque la pastille. |

### 2.3 Éléments d'interface
| # | Asset | Où | Description |
|---|---|---|---|
| U1 | Carte d'objectifs « papier » | Plateau (7.1) | Panneau ivoire à coin coupé, titre « Objectifs » avec étoile, séparateur, bloc « Récompense » : actuellement un panneau uni. |
| U2 | Jauge de fatigue carrée | Escouade du plateau (7.1) | Barre verte à bord arrondi au pied du portrait. Le fill existe (`bar-fatigue-fill`) ; manque le gabarit intégré au portrait. |
| U3 | Fiche de case | Combat élite (7.3) | Cadre avec bandeau d'illustration à gauche (ennemi), titre, étoiles de menace, ligne de récompenses (icônes + ×N). |
| U4 | Cases de récompense | Victoire (9.3), Zone sécurisée (9.4) | Tuile carrée sombre à liseré or avec icône et « +800 ». |
| U5 | Cartes d'EXP d'agent | Victoire (9.3) | Tuile : portrait carré, famille, « Niv. 42 », barre XP, « +480 XP ». |
| U6 | Badge « Nouveau » sur carte, ruban de rareté | Booster (11.3) *(à confirmer)* | `new-ribbon` existe ; manque la variante coin. |
| U7 | Podium / lignes de classement *(à confirmer)* | Tournoi (13.1, 13.2) | Lignes avec rang, avatar, score, surlignage du joueur. |

### 2.4 Audio *(à confirmer)*
Aucune planche ne le demande. Manque pour une expérience complète : sons de victoire d'XP, ouverture de booster, tuile de récompense, clic de tente (repos), musique de boutique et de menu.

---

### 2.5 Ce que la comparaison écran par écran a montré

**A. Le style des illustrations du jeu n'est pas celui de la référence (constat le plus important).**
- **Agents :** sur les planches, Giulia Rossi est brune, cheveux bouclés, cape rouge sombre, éclats magenta anguleux, pinceau « polygonal ». Dans le jeu (`Agents/giulia_rossi`), elle est rousse-brune, en tenue rouge sombre, dans une scène néon violette avec un taxi en lévitation : même personnage, mais une autre direction artistique. Les portraits carrés de la référence sont des **visages cadrés de près**, pas des scènes.
  → **Manque : refaire les 32 portraits d'agents dans le style de la référence** (au minimum les 4 de départ, Brie Rayner, Agent D, puis les agents qui apparaissent sur les planches : Ilona Varga, Lucas Mendes, Mei-Lin Chen, Marco…).
- **Cartes de compétence :** les planches montrent des cartes **lumineuses et stylisées** (poing fantôme bleu, barrière de fer à rayures jaunes, champ gravitationnel magenta, corps de brume…) avec un **nom différent** de ceux du jeu (jeu : Monolithe, Foudre en chaîne, Poing tellurique, Faille temporelle…). Les 48 illustrations actuelles sont des peintures sombres, apocalyptiques, d'un autre style.
  → **Manque : une illustration de carte par compétence dans le style de la référence** (cadre carré 1:1 pour l'icône de compétence, 3:4 pour la carte), et une décision sur les noms (planche ou jeu ?).
- **Icônes de compétence :** la fiche d'agent (4.2) et le combat (8.2) montrent **quatre icônes carrées** par agent (Frappe fantôme, Corps de brume, Champ gravitationnel, Barrière de fer). Le jeu affiche la carte entière, recadrée. → **Manque : 4 icônes de compétence par agent** (128×128, fond sombre, couleur de famille).

**B. Ennemis non affichés dans le combat.** Sur la capture du combat, les ennemis sont des pastilles rouges avec un « V » : les 11 illustrations d'ennemis existent mais ne sont pas utilisées, et la référence montre un portrait en cadre incliné. La référence montre aussi un **« Voyou » casqué à visière rouge** qui n'existe pas dans `Generated/enemy-*` (le jeu a `enemy-thug`, sans casque). → **Manque : portrait « Voyou » casqué, et un format portrait incliné (cadre) pour chaque ennemi.**

**C. Illustrations de fond dont la référence a besoin et que le jeu n'a pas :**
- **Fond d'écran-titre et de connexion** : héroïne de face, main tendue, éclats magenta, immeuble « THE DAWN », ponts de New York à l'aube. Le jeu affiche une esplanade sans personnage (et n'utilise pas `HD_Logo`).
- **Écran de chargement** : trois agents de dos devant l'immeuble « THE DAWN », skyline, grue. Réutilisé aussi dans le QG, le tournoi (13.1) et le combat tutoriel.
- **Mise en page « volet diagonal »** des écrans de compte, réglages, pass, profil : un grand portrait détouré à gauche, découpé en biais, un panneau à droite. Il demande des **portraits détourés** (I5).
- **Fiche de dossier d'agent** (4.3) : illustration de lieu liée au récit (ex. façade de casino de Manhattan pour Giulia). → **Manque : une illustration de lieu par agent** pour le dossier.
- **Illustration par ennemi du Repaire** (12.4) : portraits de Brick, AMIF Titan, Crocodile en grand format, cadrage visage (existent en plein cadre : à recadrer).

**D. Icônes et pictogrammes non utilisés alors qu'ils existent :**
- La barre de navigation du QG (QG, Équipe, Pass, Boutique, Profil) utilise de petites icônes en trait gris ; la référence utilise les icônes de `IconsGold` (étoile, équipe, pass, boutique, profil) avec état actif doré. Les assets existent, il faut les brancher.
- Les onglets de famille (Matière, Élémentaire, Physique/Bio, Perception) de l'équipe, du grimoire et du registre n'ont que l'étoile ; la référence met l'icône de famille de `IconsColor`. Existe aussi.
- Le menu des paramètres : la référence met une icône devant chaque rubrique (audio, graphismes, accessibilité, langue, notifications, contrôles, compte, support) ; `Icons` les contient toutes.

**E. Pictogrammes absents du kit, vus sur les planches (à ajouter à la section 2.2) :**
`google` et `apple` (logos de connexion), `smartphone` (« Cet appareil », 2.3), `hearts` pleins (vies, 17.1), `person-check`/`friend-online` (point vert en ligne), `copy` existe, `lightning-source` (source d'engrenage, 17.4), `gear` doré (engrenage), `arrow-cycle` (sens de rotation des roues), `medal`, `hex-round-chip` (puce hexagonale « 1✓ 2✓ 3▶ » du tournoi, 13.1), `ticket` (Pass), `skull`/`boss-diamond` (Repaire).

**F. Objets et ressources :**
- L'inventaire de la référence (5.3) montre des **objets en icônes détourées, brillantes** : ambre de matière, trousse de soins, cellules bleues, dossiers, caisse blindée, batterie, tissus, puce. Les 10 objets du jeu portent d'autres noms (kevlar_vest, phoenix_core…) et d'autres styles. Dans la capture de l'inventaire vide, aucun objet n'est visible, donc on ne peut pas juger leur rendu.
  → **Manque : une icône détourée par objet et par ressource dans le style de la référence** (au moins : gantelets stabilisateurs, gants renforcés, ceinture tactique, module d'analyse, gel de récupération, trousse de soins, cellule d'énergie, caisse de composants, tissu, puce).
- Atelier (3.3) : l'image de recette en grand (gants renforcés) et les composants en pastilles. Le jeu affiche des lettres dans des cercles (« L », « G », « B »). → **Manque : une illustration par recette** (objets fabricables) et une icône par composant (scrap d'arme, d'armure, d'outil).

**G. Tournoi, Pass, Profil.**
- **Tournoi :** la référence montre des **avatars** à côté de chaque rang et des laurées. Les rivaux du jeu (Pivot, Givre, Atlas, Éclat, Cyclone…) n'ont aucun portrait. → **Manque : un avatar par rival** (8 minimum) et les laurées (voir C9 de la section 2.2).
- **Pass :** la référence aligne une piste de récompenses par palier (cristaux, caisses, ceinture, gants, dossier) sur fond skyline et une héroïne détourée. Le jeu affiche une liste de lignes textuelles. → **Manque : icônes de récompense par palier** (cristal, caisse, ceinture, gants, dossier, carte) et visuel d'héroïne détourée.
- **Profil :** avatar circulaire avec skyline, icônes de statistiques (agents, opérations, missions, succès, repaire) : pictogrammes déjà dans `Icons` ; manque l'**avatar de profil par défaut** (ville au crépuscule).

**H. Mini-jeux.** La capture de l'analyse ADN confirme que les molécules sont des disques plats (vert vif, bleu pâle) reliés par des traits gris ; la référence montre des **sphères ombrées, translucides, vertes et bleues, avec liaisons lumineuses**. → **Manque (I9 confirmé pour ce jeu) :** sphères de molécule (vert, bleu, état « sélectionné » avec contour or), pistes et notes de rythme (losanges lumineux jaune, cyan, rose et plateformes à icône de famille), trois caisses métalliques avec emblème de famille, roues dentées dorées avec flèche de rotation, source (éclair) et connexion (étoile). Un **portrait détouré** de l'agent concerné apparaît à gauche de chaque mini-jeu.

**I. Écrans non couverts par une capture au moment de la comparaison :** boosters (ouverture, révélation, résumé : 11.2 à 11.4), inventaire avec objets, équipement d'agent, affinités (5.4), registre des agents, ajout d'ami, repaire, tutoriel en vignette. Je ne peux donc pas dire ce qui y manque.

### 2.6 Planches 15, 16, 18, 19, 20 et 21

**Planche 15 — Réglages B (comparée à la capture Notifications).**
- La référence a un **menu latéral avec une icône par rubrique**, des **interrupteurs dorés** (on/off) et un **portrait détouré différent par page** (Giulia, Marco, Ilona, etc.). Le jeu a des boutons texte, de grands boutons verts « ACTIVÉ » et le même portrait de Giulia sur toutes les pages (dans l'ancien style, cf. 2.5 A).
- **Icônes manquantes :** `dumbbell` (entraînement), `truck` (expéditions), `microscope` (recherche), `ticket` (pass), `moon` (heures calmes), `touch` (toucher), `swipe` (glisser), `zoom-in`, `monitor` (raccourcis clavier), `shield-check` (confidentialité), `trash` (supprimer le compte), `graduation-cap` (tutoriel), `book-open` (fiches d'aide), `id-tag` (identifiant). Existent déjà : `bell`, `controls`, `account`, `support`, `link`, `cloud`, `logout`, `copy`, `info`, `help`, `map`.
- **Illustrations manquantes :** 4 à 5 portraits détourés d'agents différents (un par page) et une vue du Kremlin pour la rubrique Support (le fond `hq-moscow` peut servir).
- **Composant manquant :** l'interrupteur doré (`toggle-on/off` existe dans `Controls`, mais il n'est pas utilisé ici).

**Planche 16 — Fenêtres (pas de capture du jeu).**
- **Pause (16.1) et Retour au QG (16.2) :** panneau central sur le plateau flouté, avec un grand portrait détouré à gauche. Besoin : portrait détouré de Giulia en mouvement (cheveux, éclats magenta), icônes `play`, `gear` (`settings` existe), `book-open`, `home` (existe), `close` (existe), `check-box-ok`.
- **Journal de quêtes (16.3) :** lignes avec barre de progression et **pastilles de récompense** (cristal ambre ×200, cristal cyan ×100, spirale violette ×50). Besoin : les icônes de ressource sur fond de tuile (cf. U4), `layers` (collecter), `bolt` (existe : `energy`), `people` (existe : `team`), pastille rouge « nouveau » (`badge-danger` existe).
- **Questionnaire (16.4) :** portrait détouré d'une agente avec bloc-notes dans la main, façade « THE DAWN » en fond ; icônes `paper-plane` (envoyer), `chat-bubble` (commentaire). Chips 1 à 5 : à dessiner avec les `badge-*`.

**Planche 18 — Mini-jeux B (comparée à la capture Infiltration).**
- **Infiltration :** la référence est une **vue de dessus détaillée** (sol de entrepôt, caisses, barils, bandes de danger, gardes en tenue sombre à casque rouge, **cônes de vision rouges translucides**, panneau de sortie vert, trajet en pointillés, agent avec anneau doré). Le jeu montre des cercles rouges, des lignes jaunes pour les cônes et un disque blanc pour le joueur. Le fond d'illustration existe (`mini-infiltration`).
  → **Manque :** sprite de garde (vue de dessus, casque rouge), sprite d'agent (vue de dessus) avec anneau, cône de vision (PNG dégradé rouge), signal de sortie (icône verte « sortie »), badge « Sortie repérée » (œil + texte), éléments d'environnement détourés (caisses, barils, bandes de danger).
- **Tir franc (18.1) :** scène avec l'héroïne en plein tir, cercle de basket, grillage, immeuble « THE DAWN ». Besoin : portrait détouré de tir, ballon, panier, jauge de puissance (à dessiner avec `bar-energy-fill`), icônes `target`, `timer` (existe : `clock`), `hand-hold`.
- **Ingénierie de secours (18.3) :** manuel d'indices en **carnet à spirale**, papier crème avec schéma, cases cochées ; composants en icônes (batterie à éclair, radiateur, puce dorée, ventilateur), outils posés. → **Manque :** texture de carnet, 4 icônes de composant, 3 pictos de spécification (alimentation, régulation, contrôle), outils.
- **Survie en neige (18.4) :** vue de dessus d'un décor de neige : arbres givrés, rochers, **totems lumineux** (obélisque doré), traces de pas, jauge de chaleur (flamme). → **Manque :** tuiles de neige, totem (inactif / actif), traces de pas, icône `flame`.
- **Non comparées à une capture :** Tir franc, Ingénierie et Survie (le jeu a bien les captures 98, 100, 101, mais je ne les ai pas ouvertes).

**Planche 19 — Activités (pas de capture du jeu).**
- **Potion (19.1) :** héroïne tenant une fiole, liste de potions avec icône de fiole colorée, ingrédients en tuiles (ambre, armoise, sel minéral) avec quantité disponible, étapes (broyer, mélanger, chauffer). → **Manque :** 4 icônes de fiole (tonique ambre, sérum violet, catalyseur bleu-violet, stabilisateur turquoise), icônes d'ingrédient `herb` (armoise), `mineral-salt`, `amber` (existe en illustration), icônes d'étape `mortar`, `flask`, `heat`, portrait détouré tenant une fiole.
- **Dialogue à choix (19.2) :** Ilona Varga en grand à gauche ; trois réponses avec icônes (étoile, poignée de main, groupe), progression en 3 points, pastille de réaction avec portrait. Besoin : portrait détouré d'Ilona (visage dur, cheveux gris), `handshake`, `crowd`, puces de progression (`route-node` peut servir).
- **Pêche (19.3) :** pêcheuse de dos, bord de l'Hudson, poisson qui saute, canne, jauge à deux poissons, bouton hameçon rond avec flèches. → **Manque :** poisson (2 tailles), éclaboussure, hameçon (existe en icône : `fishing`), flèches doubles (`forward` existe), bouton rond doré, portrait détouré.
- **Arène / survie (19.4) :** décor d'entrepôts de Moscou sous la neige, soldat casqué, joystick, **deux boutons ronds d'action** (course, explosion), icônes `sprint`, `blast`, `wave` (vague), `laurel` (laurier). → **Manque :** fond d'arène à Moscou (le jeu a `background-activity-arena`, style à vérifier), icône `sprint`, `blast`, `wave`, sprite de soldat ennemi en vue 3/4.

**Planche 20 — Héros et états (comparée à la capture d'introduction ADN).**
- **Introduction de mini-jeu (20.3) :** la référence est un écran divisé : à gauche, **Giulia devant un écran affichant une double hélice d'ADN** dans un laboratoire ; à droite, la consigne dans un bandeau ivoire, l'objectif (cible), Abandonner / Démarrer. Le jeu superpose du texte sur le plateau de jeu assombri, sans portrait ni illustration. → **Manque :** une illustration d'introduction par mini-jeu avec l'héroïne (au moins ADN, Rythme, Caisse, Engrenage, Tir franc, Infiltration, Ingénierie, Neige), alors que `Production/mini-*` en fournit déjà une par mini-jeu sans l'héroïne.
- **Fin de mini-jeu (20.4) :** illustration d'une ruelle de New York avec bannière rouge « THE DAWN », « Défi réussi », score entre laurées, tuile de récompense (ambre ×2). → **Manque :** illustration de résultat (jour clair, bannière), laurées (cf. C9), tuile de récompense (cf. U4) ; une variante « échec » est à prévoir.
- **Rempart (20.1) et Course des toits (20.2) :** héroïne qui bloque des projectiles avec un bouclier rose, ou qui court de toit en toit pour sauver un civil. → **Manque :** sprites détourés de l'héroïne (pose de blocage, course, saut), bouclier d'énergie rose, balles à traînée de flamme, civils (adulte + enfant), château d'eau, flèches tactiles `‹ ›` et `^` sur disque ivoire.

**Planche 21 — Compléments (pas de capture du jeu).**
- **Réflexe :** portrait détouré avec éclats, jauge horizontale avec zone de réussite verte en pointillés et curseur. → manque : curseur et zone de réussite (à construire avec `bar-*`), portrait.
- **Activité générique :** trois agents devant la skyline (art du chargement) et rappel des quatre familles (icônes existantes).
- **Bientôt disponible :** **scène de chantier** avec ouvriers à casque, grue, immeuble en construction et bannière « THE DAWN » (nouvelle illustration, absente) ; grande étoile à quatre rayons (`DawnStar` existe).
- **Mot de passe oublié :** Giulia en gros plan sur fond de Kremlin enneigé ; icône cadenas jaune (`lock` existe) ; pastille verte de coche (`check` existe).

**Conséquences pour la liste de la section 2 :**
- Ajouter à **2.1** : *illustration de chantier « THE DAWN »* ; *illustrations d'introduction de mini-jeu avec l'héroïne* (8) ; *illustration de résultat (ruelle de jour)* ; *tilemaps vue de dessus pour Infiltration et Survie en neige*.
- Ajouter à **2.2** : les icônes ci-dessus (planches 15, 16, 18, 19).
- Ajouter à **2.3** : l'interrupteur doré des réglages et le bloc « carnet à spirale ».
- **Compter** : environ 45 icônes absentes du kit en plus des 11 déjà listées, et au moins 20 portraits ou poses détourés différents.

### 2.7 Captures ajoutées après coup (comparaison avec les captures réelles)

Nouvelles captures dans `docs/test-reports/lots/16-9/` : `120-retour-qg-confirmation`, `121-dialogue-choix`, `122-potion`, `123-journal-quetes`, et relecture de `98-minijeu-survie`, `100-minijeu-tir-franc`, `101-minijeu-ingenierie`.

| Écran | Ce que montre le jeu | Écart avec la référence → assets concernés |
|---|---|---|
| Retour au QG (16.2) | Fenêtre « Quitter la zone ? », texte « le brouillard se refermera et la partie sera perdue », Annuler / Confirmer rouge. | Pas de portrait détouré, pas d'icône maison ni de ligne « Progression enregistrée ». Le texte est aussi d'un autre sens que la référence (qui rassure). → portrait détouré, icônes `home`, `check-box-ok`. |
| Dialogue à choix (19.2) | Titre violet, texte, trois boutons ivoire. Aucun portrait, aucune réaction, aucune progression. | → portrait détouré de l'interlocuteur (ici le contremaître), puces de progression, pastille de réaction avec portrait, icônes `handshake` et `crowd`. |
| Potion (19.1) | Six cartes plates de couleur vive avec le nom, un grand bloc de texte gris pour le livre, listes numérotées. | → 6 icônes d'ingrédient (résine, ginseng, champignon, piment, poussière d'étoile, herbe de sève), 4 icônes de fiole, icônes d'étape, texture de livre, portrait détouré du patient. |
| Journal de quêtes (16.3) | Une fenêtre à liste de textes ; le titre est masqué sous le toast d'accueil sur la capture. | Pas d'onglets « Objectifs principaux / Quotidiennes », pas de pastilles de récompense. → icônes de ressource sur tuile, icônes d'objectif (`layers`, `team`). |
| Tir franc (18.1) | Fond d'illustration (esplanade et skyline de Brooklyn), jauge verticale grise, disque jaune, cible rouge. | Pas d'héroïne, pas de ballon ni de panier : → portrait détouré de tir, ballon, panier, grillage. |
| Ingénierie de secours (18.3) | Manuel en texte long, **qui recouvre les emplacements 3, 4 et 5** ; composants en boutons texte (Corde, Aimant, Poignée…). | **Défaut de mise en page corrigé le 2026-11-09** (le manuel se réduit pour tenir au-dessus des emplacements ; capture mise à jour) ; il reste l'absence d'icônes de composant (corde, aimant, poignée, ressort, pile, crochet, fil, lame) et de carnet à spirale. |
| Survie en neige (18.4) | Fond de neige peint, **rectangles blancs** pour les obstacles, cercles orange pour les dangers, disque blanc pour l'agent. | → totems lumineux, rochers et arbres détourés, sprite d'agent vu de dessus, icône `flame` pour la chaleur. |

Ce que ces captures confirment : **les éléments de jeu des mini-jeux (obstacles, ennemis, cibles, ballon, composants) sont des formes géométriques dessinées par le code** (rectangles, cercles, disques). La liste I9 de la section 2.1 devient un constat, plus une hypothèse.

**Toujours sans capture :** pause de combat (16.1), questionnaire de playtest (16.4), pêche, arène, réflexe, activité générique, « bientôt disponible », mot de passe oublié, ajout d'ami, affinités, équipement d'agent, registre, ouverture de booster. Ils ne sont pas jugés ici.

---

## 3. Priorités proposées
0. **Décider du style des illustrations** (section 2.5 A) : le jeu et la référence ne partagent pas la même direction artistique pour les agents, les cartes et les objets. C'est le plus gros chantier : aucune mise en page ne comblera cet écart.
1. **I5** (agents détourés) : débloque dialogue, victoire, fiche d'agent et tutoriel.
2. **C8, C4, C7, C3** (pictogrammes de cases) : le plateau cesse d'afficher des lettres.
3. **I1** (arrondissements) : donne la carte de New York de la planche 6.1.
4. **I4, C1, C2** : événements narratifs avec image et choix illustrés.
5. **C9, C5, C6, U3 à U5** : victoire, zone sécurisée et fiche de case.
6. **I3, I2, I6** : un fond de plateau et une image de zone pour chaque zone et chapitre.
