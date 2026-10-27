# Lot H (0.14.0) : les 9 mini-jeux 2D

Source : PDF « N29 HQ » (playtest 0.13.0), tableau « Les mini-jeux ». Consigne du PDF : pour tous les assets 2D, créer des visuels uniques en SVG ou utiliser des visuels libres de droits en raccord avec la direction artistique.

## 1. But et périmètre

Faire jouer au test public les 9 mini-jeux du PDF, dans les 4 zones de Brooklyn.

| Mini-jeu du PDF | Décision |
|---|---|
| Analyse ADN, Infiltration, Engrenage, Le rythme, Tir franc, Ingénierie de secours, Caisse au choix | **À créer** (7 nouveaux types d'activité) |
| Survie en neige | **Rendu refait en 2D vue du dessus** ; la logique `SnowSurvivalGame` est conservée |
| Choix des cartes | Existe déjà (« Dialogue ») : **inchangé** |

Les activités hors PDF (Réflexe, Potion, Rempart, Course des toits, Arène 3D) restent telles quelles. L'arène garde sa fenêtre 3D.

Hors lot : nouvelle activité au-delà des 9, voix et musiques propres aux mini-jeux, classement des scores, zones des chapitres autres que Brooklyn, compte et notifications (lot I).

Décisions de cadrage prises avec l'utilisateur : périmètre « 7 nouveaux + survie en 2D » ; placement « dans les 4 zones de Brooklyn » ; visuels « mixte » (fonds et décors en SVG, éléments de jeu dessinés par le code).

## 2. Domaine : un jeu = une classe pure

Dans `Domain/Board/Activities/Games` : une classe par jeu, sans dépendance à Unity, déterministe (niveau issu de la graine de `ActivityContext`), testée en EditMode. Chaque jeu expose un état `GameStatus` (`Running`, `Won`, `Lost`), un `Score`, avance par `Tick(float dt)` et reçoit des entrées abstraites (appui, maintien, relâchement, position normalisée 0..1 de la zone, vecteur de joystick).

Chaque jeu a un type d'activité (`IActivityKind`) enregistré dans `ActivityRegistry.Default`, avec ses `ParameterDef` bornés, et une constante dans `ActivityKinds`. Le niveau généré n'a pas de primitives 3D : les données du jeu vivent dans la partie du domaine, comme pour Rempart et Course des toits.

| Type | Classe | Règles | Paramètres (défaut, min, max) |
|---|---|---|---|
| `dna_analysis` | `DnaAnalysisGame` | Un brin d'ADN est un graphe de nœuds (2D nodes). Certains nœuds portent une molécule parasite. Toucher un parasite le retire ; toucher un nœud sain coûte une vie. Gagné quand tous les parasites sont retirés, perdu à 0 vie ou à la fin du temps. | `time_limit_s` 45 (15–120), `parasites` 6 (3–16), `lives` 3 (1–9) |
| `infiltration` | `InfiltrationGame` | Vue de dessus, déplacement à 360° (joystick virtuel sur mobile). Des gardes ont un champ de vision en cône qui tourne ou patrouille ; des murs bloquent le déplacement et la vue. Être dans un cône plus de `detect_ms` coûte une vie et renvoie au départ. Gagné en atteignant la sortie. | `time_limit_s` 60 (20–180), `guards` 3 (1–8), `lives` 3 (1–9), `detect_ms` 600 (200–2000) |
| `gears` | `GearsGame` | Plusieurs engrenages sont reliés par contact (graphe). Le joueur choisit le sens de rotation de chaque engrenage libre ; deux engrenages en contact doivent tourner en sens contraires. Gagné quand le moteur entraîne tout le mécanisme sans conflit. Le niveau est généré toujours solvable. | `gears` 6 (3–12), `moves` 12 (4–40) |
| `snow_survival` | `SnowSurvivalGame` (existant) | Inchangé côté domaine : jauge de survie, totems, objet à trouver. | (inchangés) |
| `rhythm` | `RhythmGame` | Une séquence d'ennemis arrive sur 3 ou 4 pistes. Appuyer sur la piste au bon moment : fenêtres Parfait / Bon / Raté. Un Raté coûte une vie. Gagné en atteignant le score cible, perdu à 0 vie. | `notes` 24 (8–60), `lanes` 3 (3–4), `lives` 5 (1–9), `speed_percent` 100 (50–220) |
| `free_throw` | `FreeThrowGame` | Maintenir pour faire aller et venir une jauge de puissance, relâcher pour lancer. Le projectile suit une trajectoire à puissance choisie et doit toucher la cible (distance et taille tirées de la graine). `throws` essais, `hits` touches à obtenir. | `throws` 5 (2–12), `hits` 3 (1–8), `speed_percent` 100 (50–220) |
| `rescue_engineering` | `RescueEngineeringGame` | Un manuel donne des indices écrits. Le joueur choisit parmi des pièces et les assemble dans l'ordre pour former l'outil. Un mauvais choix coûte une tentative. Le manuel suffit à trouver la solution (les indices sont générés depuis la solution). | `parts` 5 (3–8), `distractors` 3 (0–6), `attempts` 3 (1–9) |
| `crate_pick` | `CratePickGame` | Trois caisses scellées sont alignées. Un curseur va et vient latéralement à vitesse constante. Maintenir charge l'attaque ; relâcher frappe la caisse sous le curseur. Une seule caisse contient la récompense ; les autres déclenchent une perte de vie. | `rounds` 3 (1–8), `lives` 3 (1–9), `speed_percent` 100 (50–220) |

Gains : une `ActivityDef` par jeu dans `PrototypeActivities`, avec Crédits et PR de réussite (100 à 350 Crédits, 5 à 10 PR selon la difficulté) et un `creditsPerScore` quand le score compte (rythme, tir franc, ADN). Les valeurs se surchargent sans code par `activities.json`.

## 3. Présentation : fenêtre commune et pilotes

- **Fenêtre `Popups/MiniGame2DPopup`** (nouvelle recette de prefab) : bandeau titre / score / temps / vies, grande zone de jeu tactile, joystick virtuel flottant (affiché seulement pour l'Infiltration et la Survie), Abandonner, superposition de début et de fin. Rempart et Course des toits gardent `HeroGamePopup`.
- **Pilote par jeu** (`IMiniGameDriver`) : construit ses éléments dans la zone, les met à jour à chaque image, relaie les entrées vers la partie du domaine et rend un `ActivityResult` (réussite, score). Fin de partie, abandon ou fermeture par Retour rendent toujours la main : jamais d'écran bloqué (bug du playtest).
- **Survie en neige** : un pilote 2D remplace le pilote 3D `SurvivalDriver` ; l'arène reste en 3D (`StageGameView`).
- **Lancement** : `CasePopupPresenter` ouvre la fenêtre commune pour les nouveaux types, comme il ouvre `HeroGamePopup` pour le rempart.
- **Visuels (mixte)** : un fond SVG par jeu (laboratoire, camp, atelier, scène, terrain, abri, entrepôt, étendue blanche), recoloré par la palette de l'environnement du plateau ; éléments de jeu (molécules, notes, curseur, gardes, engrenages, caisses) dessinés par le code en formes plates. Les SVG sources sont dans `Assets/_Project/Art/MiniGames/`. Si le paquet `com.unity.vectorgraphics` ne s'ajoute pas en mode batch, un outil d'éditeur les rasterise en PNG et les SVG restent les sources.
- **Textes** : noms, consignes, résultats, indices du manuel dans `Presentation/Common/LocMiniGames.cs`, hook `ExtendMiniGames` dans `Loc.cs`. Tout en français.
- **Tactile** : cibles de toucher ≥ 96 px là où le joueur doit viser un bouton ; les cibles de jeu (nœuds, caisses) sont plus grandes que leur dessin.

## 4. Placement dans Brooklyn

Les 9 jeux (7 nouveaux, survie en neige, dialogue) sont répartis sur les cases mini-jeu des 4 zones (`zone_ny_1_1` à `zone_ny_1_4`), au moins 2 par zone, les plus simples en début de zone et les plus durs en fin. Les zones sont modifiées par le générateur de zones existant (outil de level design), pas à la main. Les tests de campagne (`BrooklynCampaignTests`, `CampaignAssetsTests`) continuent de valider les zones. La zone tutoriel n'est pas modifiée.

## 5. Tests

- **EditMode** : par jeu, victoire, défaite, bornes des paramètres, déterminisme à graine égale, niveau toujours solvable (engrenages, ingénierie, ADN, infiltration), abandon ; schéma des paramètres de chaque type ; les 9 jeux sont placés dans les zones et chaque type est enregistré ; `CasePopupPresenter` lance chaque type via un faux écran ; textes présents pour chaque jeu.
- **PlayMode** : chaque fenêtre s'ouvre, se joue (réussite, échec, abandon) et se ferme sans bloquer ; le joystick virtuel suit le doigt.
- **Captures** : une par jeu (`CaptureMiniGames`, explicite).

## 6. Documents

Cette spec, le plan (`Code/docs/plans/2026-10-27-mini-jeux-2d.md`), `03_Art_UI_UX/01_UX_Ecrans.md` (section « Lot H »), `05_Production/Decisions.md`.

## 7. Découpage prévu des tâches

Fenêtre commune et pilotes de base (interface, joystick, recette de prefab) ; puis un jeu par tâche (domaine, type d'activité, pilote, textes, tests) ; survie en 2D ; fonds SVG ; placement dans les zones ; documents, captures et suites complètes ; relecture finale par un agent indépendant.
