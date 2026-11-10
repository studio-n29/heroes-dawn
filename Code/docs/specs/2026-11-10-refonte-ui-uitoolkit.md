# Refonte complète de l'interface — Aube tactique en UI Toolkit

Date : 2026-11-10. Branche : `feature/refonte-ui-aube-tactique` (créée depuis `develop`).
Référence visuelle : les 21 planches (84 vues) de `03_Art_UI_UX/Aube_Tactique_2026-11-04/`.
Plateformes cibles : PC (clavier et souris), mobile (tactile), console (manette ; tactile sur Switch).

**Règle d'identité (décidée le 2026-11-10).**
- Logo : le logotype des planches, « HEROES ✦ DAWN » en Barlow Condensed ExtraBold Italic avec l'étoile à quatre branches, fourni par `Art/UI/AubeTactiqueKit/Brand/heroes-dawn-ivory.png` (sur fond sombre), `-gold.png` et `-navy.png`.
- « Aube tactique » est le **nom de travail de la direction artistique**, uniquement un repère interne pour désigner cette interface : **il n'apparaît jamais dans le jeu** (ni sous le logo, ni dans un titre, ni dans un texte localisé). Un test vérifiera l'absence de cette chaîne dans les textes localisés du jeu.
- La mention « THE DAWN » avec l'étoile, en haut à droite de plusieurs planches, est une mention de marque distincte et reste.

## 1. Décision

**UI Toolkit pour toute l'interface (textes, panneaux, boutons, listes, fenêtres, HUD).**
**uGUI et rendu en jeu conservés seulement pour les surfaces de jeu** : carte du plateau (cases, liens, brouillard, pion), terrains des mini-jeux, effets (étincelles, bulles de BD). Ces surfaces restent sous leur Canvas, le panneau UI Toolkit s'affiche au-dessus. Leur migration est une option de la dernière phase, pas un prérequis.

L'architecture MVP actuelle ne change pas : les présentateurs et les interfaces `IxxxView` restent, seules les vues concrètes et leurs prefabs sont remplacés. Les 1 336 tests EditMode (présentateurs, domaine) restent valables.

### Pourquoi UI Toolkit

| Critère | UI Toolkit | uGUI (état actuel) |
|---|---|---|
| Fidélité aux planches | Mise en page flex en pourcentages et rem, 9-slice du kit, états `:hover` `:focus` `:active` `:disabled` par USS, transitions, bordures, coins arrondis, masques. Un seul fichier de jetons (couleurs, espacements, polices). | 90 prefabs générés par du code C# (`IPrefabRecipe`), positions en pixels ancrés ; chaque écran à ajuster à la main. |
| Formats d'écran (16:9, 20:9, 4:3, Switch portable 1280×720) | Réagit à la taille du panneau sans code : les mesures de l'étape 3 ci-dessous montrent l'écran titre s'adapter à quatre formats sans ligne de code. | Ancrages à régler écran par écran ; `TacticalPageLayout.Reflow` existe pour compenser. |
| Manette | Modèle de focus et de navigation intégré (`NavigationMoveEvent`, `NavigationSubmitEvent`, `:focus`). Prouvé par test (voir § 2). | Tous les boutons du jeu sont créés avec `Navigation.None` : la navigation manette est à écrire de zéro. |
| Tactile, souris, clavier | Événements de pointeur unifiés, survol, molette, focus clavier : mêmes éléments pour les trois. | Même chose, mais navigation et survol codés à la main (`UiButton`). |
| Maintenance | UXML et USS sont du texte : diff lisible, fusion possible, édition dans UI Builder. | Prefabs YAML bruyants ; un prefab régénéré réécrit des centaines de lignes. |
| Capture et tests visuels | Le panneau se rend dans une `RenderTexture` : capture déterministe à la résolution voulue (utilisé par le prototype). | Il faut basculer les canvas en `ScreenSpaceCamera` pour capturer (contournement actuel de `ScreenCaptureTests`). |
| Coût de migration | Élevé : 328 fichiers de présentation, 90 prefabs, 77 tests PlayMode à reprendre. | Nul, mais ne répond ni à la manette ni à l'exigence de fidélité. |
| Risque | Moins de contrôle sur le rendu des effets de jeu (étincelles, particules) : conservés en uGUI. Mesure de performance sur Switch à faire. | Faible. |

Ce qui a pesé : la manette et les formats d'écran sont des exigences nouvelles ; uGUI les traite par des contournements, UI Toolkit par son modèle de base. La refonte réécrit de toute façon toutes les vues.

## 2. Preuve : prototype de l'écran titre

Fichiers : `Assets/_Project/UIToolkit/Spike/TitleSpike.uxml` et `.uss`, test `Assets/Tests/PlayMode/UiToolkitSpikeTests.cs` (explicite). Captures dans `docs/test-reports/uitk-spike/`.

Résultats mesurés (2 tests réussis) :
- Rendu fidèle à la composition de la planche 1.1 (Giulia détourée à gauche, panorama de New York, logotype « HEROES ✦ DAWN » du kit sans mention « Aube tactique », boutons Jouer et Se connecter en sprites 9-slice du kit, puce de langue) avec 120 lignes d'UXML et d'USS.
- Quatre formats rendus sans modification : 1920×1080, 2160×972 (mobile 20:9), 1280×720 (Switch portable), 1280×960 (4:3). Le fond est recadré, le héros et le logo se repositionnent par pourcentages.
- **Navigation manette** : focus initial sur Jouer, direction Bas → Se connecter, Haut → Jouer, Valider → le bouton se déclenche. Aucun réglage de navigation par bouton. L'état `:focus` remplace le sprite par `button-primary-focus` (cadre doré épais).
- Pièges rencontrés, à traiter une fois dans la base commune : le conteneur de modèle du document ne remplit pas le panneau par défaut (il faut lui donner `flex-grow` et 100 % de taille) ; l'alignement du texte des boutons est en haut à gauche par défaut ; les espaces multiples d'un texte sont écrasés.

Non mesuré (à faire en étape 0, voir § 6) : performances sur Android milieu de gamme, mémoire, rendu des polices Barlow en atlas SDF, interaction entre le panneau UI Toolkit et les Canvas uGUI conservés, build Switch (le module n'est pas installé dans cet éditeur : seuls Windows, Android, iOS, Mac et WebGL le sont).

**Condition de repli** : si, à l'étape 0, une de ces mesures échoue (60 images/s sur Android de référence, mémoire du panneau, rendu des textes localisés), on revient à uGUI avec la couche commune décrite en § 4 (jetons, composants, navigation manette écrite une fois). Les spécifications de fidélité restent valables.

## 3. Entrées et plateformes

Un `PlatformProfile` unique décide, à l'exécution, de ce qui diffère :

| | PC | Mobile | Console |
|---|---|---|---|
| Entrée principale | Souris et clavier | Tactile | Manette ; tactile sur Switch portable |
| Focus visible | Au clavier seulement | Jamais | Toujours (cadre doré du kit) |
| Survol | Oui (agrandissement 1,5 %, infobulle) | Non | Non |
| Cibles minimales | 40 px | 48 dp (≈ 96 px à 1080p de référence) | 56 px (lisible depuis un canapé) |
| Zones sûres | Aucune | Encoche et barres système | Marge de 5 % (surbalayage TV) |
| Invites de touches | Touches clavier | Aucune | Boutons A / B / Y / X selon la manette |

Règles communes :
- Une classe USS sur la racine (`.input--kbm`, `.input--touch`, `.input--gamepad`) change le rendu ; elle bascule au dernier périphérique utilisé (clavier ou souris, tactile, manette).
- Mapping manette fixe : A valider, B retour, Y action secondaire (Soigner, Équiper), X détail, bouton de menu = pause/paramètres, croix et stick = focus, gâchettes = onglets précédent et suivant, stick droit = défilement.
- Aucun écran sans chemin de focus complet : le focus ne se perd jamais dans une fenêtre (il est piégé dans la fenêtre ouverte, retour B ferme).
- Taille de texte minimale 24 px à la résolution de référence 1920×1080 (soit 16 px en 720p).
- Combat et plateau : la carte et les cartes de compétence se jouent aussi à la manette (sélection d'une case par croix, d'une cible par croix, validation A). À spécifier en détail dans les lots 6 et 7.

## 4. Architecture

```
Assets/_Project/UIToolkit/
  Theme/        tokens.uss (couleurs, polices, espacements), base.uss, platform-*.uss
  Components/   HdButton, HdPanel, HdTab, HdBar, HdPortrait, HdIcon, HdResourcePill, HdFrame… (UxmlElement + USS)
  Screens/      <écran>/<écran>.uxml + .uss + <Écran>View.cs   (implémente IxxxView)
  Host/         UiToolkitHost (PanelSettings, safe area, profil), ModalStack, FocusTrap, InputRouter
```

- Panneau : `ScaleWithScreenSize`, référence 1920×1080, `MatchWidthOrHeight` réglé par profil ; zones sûres appliquées en marge de la racine.
- Polices : Barlow et Barlow Condensed en `FontAsset` SDF (TextCore), étendues à l'alphabet des deux langues du jeu.
- Jetons : les couleurs de la charte (marine #101C29, ivoire #ECE7DC, laiton #D6AF5B, danger, familles Matière / Élémentaire / Physique-Bio / Perception) sont des variables USS ; aucune couleur en dur dans un écran.
- Composants : éléments personnalisés avec attributs UXML (`<hd:Button variant="primary" icon="play" />`). Le kit et l'extension fournissent les sprites : boutons (5 variantes × 5 états), surfaces, onglets, barres, cadres de portrait et de carte, icônes.
- Vues : chaque vue ouvre un `UIDocument` ; l'API publique reste celle de l'interface (`PlayButton`, `ContinueButton`…), exposant des `Button` UI Toolkit, pour limiter la réécriture des tests PlayMode.
- Localisation : les libellés passent par les clés existantes (`Loc.T`), liées par nom d'élément (`label.name`), plus de composant `LocalizedText`.
- Surfaces de jeu : un `GameSurface` (Canvas uGUI, ordre de tri inférieur) sous le panneau UI Toolkit ; les zones d'interaction du jeu (carte, terrain de mini-jeu) sont des éléments vides du panneau dont la taille pilote la surface sous-jacente.
- Tests : pas de dépendance aux prefabs ; un harnais `UiToolkitTestKit` (clic, focus, navigation, capture en `RenderTexture`) ; une page « Composants » interne qui affiche tous les composants dans tous leurs états, sur le modèle de `composants.html` du kit.

## 5. Fidélité : comment mesurer « 100 % »

Les planches sont des images générées, pas des spécifications au pixel. « 100 % » est défini ainsi, vue par vue (84 vues) :

1. **Composition** : mêmes zones, mêmes proportions, mêmes ancrages (à ± 2 % de la largeur).
2. **Éléments** : tous les éléments visibles dans la planche existent (icônes, cadres, pastilles, illustrations) ou sont inscrits dans le suivi des assets manquants.
3. **Typographie** : famille, graisse, casse, taille relative.
4. **États** : normal, survol, focus, désactivé, sélectionné, erreur.
5. **Mesure** : pour chaque vue, un rapport de comparaison côte à côte (planche à gauche, capture à droite, fondu 50 %) produit par le harnais, et un score de similarité structurelle sur les zones fixes. Seuil d'acceptation proposé : 0,80 ; il sera recalé après les deux premiers lots.

Un écart restant est consigné dans `docs/inventaire-assets-aube-tactique.md` (assets) ou dans un registre d'écarts (mise en page).

## 6. Plan par lots

Chaque lot : réécriture des vues concernées, reprise des tests PlayMode, captures sur 5 formats (1920×1080, 2160×972, 1280×720, 1280×960, 2560×1440), score de fidélité, navigation manette complète, EditMode et PlayMode verts. Un lot se fusionne dans la branche de refonte quand il est vert.

| Lot | Contenu | Planches |
|---|---|---|
| 0 | Fondations : hôte, jetons, polices SDF, composants de base, page « Composants », zones sûres, profil de plateforme, routeur d'entrées, harnais de capture et de comparaison. **Mesures d'arrêt/continuation** : Android milieu de gamme, mémoire, cohabitation avec uGUI, build console (si le module devient disponible). | 03 (modèle), composants |
| 1 | Titre, première connexion, chargement, introduction, compte, progression, erreur, mot de passe oublié ; pile de fenêtres et toasts | 01, 02, 21 |
| 2 | Coque du hub (barre haute, navigation basse), QG, Salle des opérations, Atelier, Archives, Journal de quêtes | 03, 16 |
| 3 | Réglages (8 catégories), Support, Profil, Pass, Amis, Repaire | 14, 15, 12 |
| 4 | Équipe, fiche d'agent, dossier, registre, équipement, grimoire, inventaire, affinités | 04, 05 |
| 5 | Boutique, recrutement, boosters (confirmation, ouverture, révélation, résumé) | 10, 11 |
| 6 | Carte de la ville, chapitres, préparation d'équipe, plateau (HUD, fiches de case, repos, événements, dialogues), zone sécurisée | 06, 07, 09 |
| 7 | Combat (introduction, HUD, ciblage, boss, résultats) à la souris, au tactile et à la manette | 08, 09 |
| 8 | Mini-jeux, activités, héros, états | 17 à 20 |
| 9 | Tournoi, tutoriel, compléments, questionnaire | 13, 16, 21 |
| 10 | Suppression des recettes de prefabs et de l'ancien kit procédural, nettoyage, passage de performance, passage d'accessibilité, certification console (liste de contrôle) | — |

Les lots 1 à 5 et 8 à 9 sont indépendants entre eux une fois le lot 0 fini ; 6 et 7 dépendent du lot 0 et de la définition de la manette sur le plateau.

## 7. Risques

| Risque | Parade |
|---|---|
| Performance UI Toolkit sur mobile milieu de gamme ou Switch | Mesure à l'étape 0 ; aplatissement de la hiérarchie, `usageHints`, atlas partagé ; repli uGUI documenté |
| Textes dynamiques (FR/EN, nombres) qui débordent | Variantes de texte longues dans la page « Composants » ; règle de troncature et de retour à la ligne par composant |
| Cohabitation avec les Canvas uGUI conservés (ordre, entrées) | Une seule `EventSystem` avec `InputSystemUIInputModule` ; mesuré à l'étape 0 |
| Illustrations de la planche absentes du jeu (agents détourés, objets, mini-jeux) | Suivi dans `docs/inventaire-assets-aube-tactique.md` ; l'extension en cours de génération couvre une partie |
| Volume de la réécriture | Lots courts, fusion à chaque lot vert, aucun écran à moitié migré dans `develop` (migration derrière un interrupteur de configuration tant que tous les lots ne sont pas finis) |
| Switch non testable ici | Profil « Switch portable » simulé en 1280×720 tactile ; certification console en lot 10 |

## 7 bis. Contraintes venant du développement 1.0.0 (reçues le 2026-11-10, à ne pas implémenter dans la refonte)

Source : la session « Heroes Dawn 1.0.0 - Tournois et repaires ». Aucune action demandée maintenant ; la refonte doit seulement laisser la place.

- **Navigation du hub** : l'onglet « Laboratoire de recherche » est supprimé et remplacé par un onglet **« Atelier de fabrication »** à interface propre. La barre de navigation et le routeur de pages (lot 2) doivent traiter l'Atelier comme une page de premier niveau échangeable avec le Laboratoire : un emplacement nommé, pas de dépendance au contenu du Laboratoire.
- **Éventail de combat** : 3 cartes équipables par agent au lieu de 2, soit **5 places** (2 natives + 3 équipées). La mise en page de la main du lot 7 doit accepter 4 ou 5 cartes sans changer de composition (largeur de carte dérivée du nombre de places, jamais en dur).
- **Agents et cartes** : les agents passent à une *position* (Première ligne, Attaquant, Contrôle, Soutien, Précision à distance) et à un *rang* de 1 à 5 étoiles ; les cartes à 1 à 5 étoiles et 4 types (Offensif, Défensif, Contrôle, Soutien). Les composants « étoiles » et « puce de type » du lot 0 doivent donc être génériques (valeur 1 à 6, type libre), pas liés aux familles actuelles.
- **Objets fabriqués** : 1 à 6 étoiles. Même composant d'étoiles.
- **Plus tard** : Tournois (classement, JcJ en ligne et hors ligne) et Repaires (donjons, boss à phases) auront leurs propres écrans ; ils ne font pas partie de cette refonte.
- Les spécifications détaillées sont dans `docs/specs/` (celle des rangs, positions et cartes existe ; celle de l'Atelier arrive). Elles ne sont pas modifiées ici.

## 8. État de la branche

- Branche créée : `feature/refonte-ui-aube-tactique`.
- Présents dans l'arbre de travail mais non commités : le branchement du kit et de l'extension sur les prefabs uGUI (travail précédent), la version 0.15.3, les modifications du script de build. Ils ne sont pas inclus dans la refonte : le lot 10 supprime les prefabs. À décider : les commiter avant le lot 0, ou les laisser hors branche.
- Ajoutés par ce document : le prototype UI Toolkit de l'écran titre et son test.
