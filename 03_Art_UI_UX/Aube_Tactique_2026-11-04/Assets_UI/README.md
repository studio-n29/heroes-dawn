# Heroes Dawn — Kit complet d’assets UI Aube tactique

Référence : `03_Art_UI_UX/Aube_Tactique_2026-11-04/index.html`, confirmée par l’utilisateur le 5 novembre 2026. Les 21 planches et leurs 84 vues sont inventoriées dans `coverage.json`, avec les descriptions reprises du HTML.

## Emplacement dans Unity

`Assets/_Project/Art/UI/AubeTactiqueKit/`

Le kit est une bibliothèque d’assets séparés. Son placement dans `Art`, en dehors de `Resources`, permet de choisir les fichiers à référencer lors de l’intégration plutôt que d’inclure automatiquement toute la bibliothèque dans une build.

`catalogue.html` présente les fichiers par catégorie, avec recherche, aperçu sur fond marine ou damier et accès aux sources. Son assemblage de boutique est une démonstration HTML des fichiers livrés. Le catalogue ne représente pas une capture des menus Unity.

## Contenu

- **310 composants graphiques PNG** : marque, panneaux, boutons et états, onglets, cadres de famille et de rareté, anneaux de portrait, cartes, contrôles, jauges, champs, badges, chronologie, dé, lignes de parcours, masques, ornements et effets UI.
- **310 sources SVG** correspondantes, modifiables, et un générateur reproductible dans `build-kit.ps1`.
- **64 cadrages de portraits** : tête carrée et buste 2:3 pour les 32 illustrations d’agents et mentors. Les fichiers `.asset` sont de vrais sprites Unity qui pointent vers les textures originales copiées dans le kit. Les pixels et les visages sont conservés. Les sources SVG de prévisualisation gardent les mêmes cadrages.
- **32 nouvelles illustrations de production** : carte de New York, plateau Red Hook, Paris Supernova ; paquets, cadeau, gemmes et crédits ; quatre ressources à alpha réel ; cinq décors d’activités ; onze portraits ennemis correspondant aux identifiants du catalogue du jeu.
- **17 illustrations Aube tactique déjà produites**, regroupées dans `Illustrations/Production`.
- **109 illustrations originales réutilisées** : 32 agents et mentors, 19 personnages génériques, 48 cartes et 10 objets. Les copies sont identiques aux fichiers du projet.
- **5 fichiers de police et leurs licences** : Barlow Regular/SemiBold et Barlow Condensed Regular/SemiBold/ExtraBoldItalic.

Les nouveaux portraits ennemis sont des interprétations graphiques des rôles et descriptions existants : leurs identifiants correspondent aux données, sans changement de mécanique ou de caractéristiques. Les personnages génériques restent identifiés comme tels.

## Charte et assemblage

Palette : marine `#101C29`, ivoire `#ECE7DC`, laiton `#D6AF5B`. Les panneaux ont un coin supérieur droit coupé et des filets fins. La marque The Dawn utilise une étoile à **quatre branches**. Les signes de famille sont combinés à la couleur et aux libellés.

| Élément | Assets à assembler | Réglage conseillé |
|---|---|---|
| Action principale | `Buttons/button-primary-*`, `Icons/dawn`, texte titre | Fond ivoire, texte marine ; icône teintable ; 9-slice |
| Action secondaire | `Buttons/button-secondary-*`, pictogramme et texte | Texte ivoire ; taille et marges identiques à l’action principale |
| Action dangereuse | `Buttons/button-danger-*`, libellé explicite | Garder le rouge pour une conséquence ou une alerte |
| Navigation | `Tabs/tab-*-active/inactive/disabled`, `IconsGold/*` | Une sélection visible ; libellés localisables séparés |
| Fenêtre | `Surfaces/surface-window`, titre, corps, boutons | Cadre 9-slice ; contenu indépendant et défilant si nécessaire |
| Carte | `Frames/card-*`, `Masks/mask-chamfer`, illustration, `Cards/card-caption` | Illustration masquée ; nom, rareté, coûts et description en texte |
| Portrait | `Portraits/*-head.asset`, `Masks/mask-circle`, `Frames/portrait-*` | Image Simple ; conserver le ratio carré et le cadrage livré |
| Grand portrait | `Portraits/*-bust.asset`, cadre de carte | Ratio 2:3 ; la texture reste celle du personnage d’origine |
| Ressources | `Economy/resource-pill`, `IconsColor/credits/pr/gems`, valeur | Icône à gauche, valeur alignée à droite ; aucun prix dans l’image |
| Jauge | `Controls/bar-track`, `Controls/bar-*-fill`, valeur | Remplissage horizontal ; conserver le texte au-dessus du fond |
| Slider | Jauge et `Controls/slider-thumb` | Poignée Simple, ratio conservé ; zone tactile indépendante |
| Toggle | `Controls/toggle-on/off/disabled` | Image Simple au ratio 2:1 ; libellé séparé |
| Champ | `Inputs/field-normal/focus/error/disabled`, texte et icône | 9-slice ; message d’erreur dans un texte indépendant |
| Infobulle | `Surfaces/surface-tooltip`, `Decorations/pointer-*` | Pointeur Simple au ratio carré ; fond opaque sous le texte |
| Plateau | Décor Red Hook, `Board/route-line-*`, `Indicators/route-node-*`, dé | Décor et nœuds séparés ; les chemins viennent des données |
| Chronologie | `Indicators/timeline-*`, portrait ou initiale, valeur | Les marqueurs restent indépendants du fond |
| Mini-jeu | Décor dédié, `Controls/joystick-*`, contrôles et consignes | Les objets interactifs et collisions restent des éléments du jeu |

Les images de boutique et décors sont livrés sans commandes ni prix incrustés. Les textes des maquettes et leurs montants sont des exemples de composition. Les labels doivent venir de la localisation et des données du jeu. Les contrôles et les textes ne sont pas rasterisés dans les fonds.

## Polices

Titres : Barlow Condensed ExtraBold Italic. Corps : Barlow Regular ou SemiBold, sans contour. Les TTF et licences sont dans `Fonts`. Les assets TextMeshPro existants du projet restent disponibles dans `Assets/_Project/Resources/Fonts` pour le branchement des textes Unity.

À la base logique 1920 × 1080, utiliser environ 28–32 pour le corps et 36–52 pour les titres selon la hiérarchie, puis vérifier le résultat sur appareil. Les PNG de pictogrammes sont prévus pour être réduits, avec `preserveAspect` activé. Les textes, marges sûres et zones tactiles relèvent de la composition des interfaces.

## Import Unity

Menu : **Heroes Dawn > Direction artistique > Importer le kit complet Aube tactique**.

Commande batch : `-executeMethod N29.HeroesDawn.EditorTools.Assets.AubeTactiqueKitImport.Batch`.

Le menu configure les textures en sprites simples, sans mipmaps, avec alpha et `Clamp`. Les composants UI sont non compressés pour garder les filets nets ; les grandes illustrations utilisent la compression haute qualité et une taille maximale de 2048. Les bordures 9-slice viennent du manifeste, dans l’ordre **gauche, bas, droite, haut**. Dans uGUI, choisir `Image.Type.Sliced` pour les supports extensibles ; les icônes, anneaux, portraits et poignées restent en `Simple`.

L’import crée les 64 sprites de cadrage et conserve les textures sources dans le kit. `unity-import-report.json` et `validation.json` décrivent l’état de la livraison. L’importeur se limite aux fichiers du kit ; le branchement des nouveaux assets dans les menus constitue une étape d’intégration distincte.

## Sources et traçabilité

`manifest.json` répertorie chaque fichier, sa catégorie, ses dimensions, ses bordures, son usage et sa source. `Sources/` contient les SVG dans le projet Unity. `coverage.json` associe les planches à leurs groupes de composants et illustrations.

`art-generation.json` archive les prompts exacts et l’utilisation de l’outil imagegen intégré. Les nouvelles peintures ont été produites avec cet outil. Les éléments UI géométriques sont dessinés en vectoriel et exportés en PNG. Les originaux du projet sont copiés avec leurs pixels conservés.

Les fichiers de génération sont conservés dans `03_Art_UI_UX/Aube_Tactique_2026-11-04/Assets_UI/` : `build-kit.ps1`, `assemble-kit.ps1`, `build-catalogue.ps1`. Ils rendent le kit reproductible. Les retouches manuelles des sources doivent être archivées avant une nouvelle génération globale.

## Planche des composants prioritaires

Ouvrir [composants.html](composants.html) dans le kit Unity pour comparer les 25 états de boutons, les sept remplissages de jauges à 25/60/100 %, les contrôles et les icônes. Cette planche utilise les PNG du kit et peut afficher un fond damier. Génération : build-components.ps1. Vérification des fichiers : validate-kit.ps1.
