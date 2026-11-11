# Aube tactique — livraison graphique, 9–11 novembre 2026

Cette extension répond aux manques graphiques de `Code/docs/inventaire-assets-aube-tactique.md`, avec les planches du 4 novembre comme référence artistique. Le catalogue `index.html` permet de consulter les fichiers réellement présents. Le contrôle final confirme 477 assets prévus sur 477 présents, sans manque. Une variante supplémentaire du portrait Ada est conservée, soit 478 PNG sources au total.

## Fichiers

- Sources PNG : `Code/Heroes Dawn Unity/Assets/_Project/Art/UI/AubeTactiqueExtension/`.
- Déclinaisons : sous-dossier `Exports/`, sources conservées.
- 116 sources vectorielles : `Sources/`, reproductibles avec `build-native.ps1`, `build-brand.ps1` et `build-extension.ps1`.
- Prompts exacts : `prompts-*.json` et `prompt-*.json`. Images produites par imagegen intégré, un appel par illustration.
- `manifest-generated.json` : dimensions et contrôle alpha par échantillonnage des PNG présents.
- `manifest-exports.json` : source, rectangle de découpe, dimensions finales et indicateur explicite d’agrandissement.
- `manifest-map-exports.json` : les dix calques illustrés des cinq arrondissements et leurs masques.
- `qa-*.png` : planches de contrôle.

## Correspondance avec l’inventaire

| Besoin | Livraison |
|---|---|
| I1, carte de New York | `Scenes/map-new-york`, `MapMasks`, `MapOverlays`, `Exports/Map3840x2160` |
| I2, fiches de zone | `Zones`, exports 1600 × 900 |
| I3, plateaux | `Boards`, exports 3840 × 2160 |
| I4/I7, événements et zone sécurisée | `Scenes`, formats portrait et paysage adaptés |
| I5, agents et guide | `Agents`, `Poses`, portraits carrés 512 et 128 |
| I6, huit chapitres et leurs états verrouillés | `Chapters`, `ChaptersLocked`, exports 1200 × 675 |
| I8, introduction VS | `Components/vs-banner` |
| I9, mini-jeux et activités | `MiniGames`, `MiniGameIntros`, `Poses`, `Items` |
| C1–C11 et pictogrammes complémentaires | `Icons`, `IconsGold`, `Decorations`, `BrandIcons` |
| U1–U7 et composants supplémentaires | `Components` |
| Cartes, compétences, dossiers | `Cards` (48), `Skills` (64), `Dossiers` (32) |
| Inventaire et ressources | `Items`, `Resources`, exports 256 |
| Adversaires et tournoi | `Enemies`, `Exports/Enemies128`, `Exports/Enemies512`, `Rivals` |

Les composants se combinent avec le kit existant : textes, nombres, remplissages de jauge, symboles centraux des engrenages et récompenses restent des couches indépendantes.

## Choix de contenu

Les noms et identifiants du jeu sont conservés. Ses 32 agents possèdent 64 compétences propres, soit deux par agent ; les 48 cartes de collection forment une série séparée. Aucun nom de compétence n’a été inventé pour reproduire les libellés de la maquette.

L’identité des agents suit `Prompts_illustrations_agents.md` ; Brie suit `Prompts_illustrations_apex_unit.md`. Ilona dispose de deux représentations : le portrait principal issu de sa description et `Poses/ilona-reference`, aux cheveux gris comme la référence. Ce choix ne décide pas du canon. `Agents/ada_okonkwo-cutout-v2` est la version solo corrigée utilisée dans les portraits dérivés ; la première génération est conservée.

Les fichiers `concept-*` illustrent les propositions du document de zones, sans validation des niveaux futurs. Les trois plateaux `zone_ny_1_2`, `zone_ny_1_3`, `zone_ny_1_4` correspondent à Brownsville, Coney Island et DUMBO dans le code consulté. Les images existantes des autres zones actuelles restent disponibles.

Les découpes de carte suivent les polygones de `CityMapLayout.cs`. La géographie peinte est illustrative : les masques constituent les contours exacts utilisables pour la sélection.

## Formats et contrôle

L’utilisateur a explicitement autorisé les découpes et redimensionnements par script. `export-assets.ps1` réalise les recadrages et conserve leur traçabilité ; `export-map.ps1` applique les masques d’arrondissement. Les JPG de fond sont exportés avec une qualité de 94. Les exports 4K issus de sources plus petites sont des agrandissements, signalés dans le manifeste, pas des générations natives 4K.

Les portraits ont été contrôlés sur planche à 128 px ; les autres familles sont examinées sur leurs planches de contact. Les contours semi-transparents restent à apprécier sur les fonds définitifs du jeu. Les sources générées peuvent différer des dimensions conseillées, qui sont assurées par les exports techniques.

Cette livraison porte sur les fichiers graphiques. Elle ne démontre pas leur affichage dans Unity, ne corrige pas les mises en page et ne modifie pas le gameplay. Des changements réalisés ailleurs dans le projet peuvent déjà en utiliser certains. Aucun commit ou push n’est effectué ici. L’audio mentionné « à confirmer » dans l’inventaire est hors de ce lot graphique.

## Actualisation

Exécuter `check-coverage.ps1`, puis `export-assets.ps1`, `export-map.ps1`, `qa-sheets.ps1` et `catalogue.ps1` après la fin des générations. Ces scripts travaillent dans le dossier d’extension et préservent les sources.


Les deux pictogrammes de connexion Apple et Google sont des formes vectorielles monochromes ; les brouillons raster écartés sont conservés dans RejectedBrandDrafts et ne font pas partie du dossier livré. Les versions verrouillées des chapitres sont des variantes désaturées produites avec imagegen, puis redimensionnées par le script d’export.


