# Direction artistique : HUD « Marvel's Spider-Man » pastel (sous-projet 2)

## But
Remplacer l'habillage actuel (très sombre, « BD » avec gros contours noirs, polices Bangers/ComicNeue) par un HUD moderne, lumineux, pastel, d'inspiration Marvel's Spider-Man : verre givré, coins coupés, filets clairs, un accent corail, typographie condensée italique en majuscules. Le rendu doit sembler premium et rester lisible.

Maquette validée : `assets/2026-10-12-da-hud-pastel.png` (combat à gauche, QG à droite).

## Intention validée
- Lumineux, coloré mais doux (pastel), pas saturé, pas sombre.
- Comics/MCU moderne et premium, non cartoon : pas de gros contours noirs, pas d'ombres dures, pas de bulles envahissantes.
- Le verre est un « faux verre » (translucide, dégradé, reflet, filet) sans flou temps réel, pour tenir sur mobile. Un vrai flou n'est envisagé que sur quelques panneaux fixes, plus tard.

## Périmètre
Habillage uniquement : palette, composants UI, sprites, polices, repeinture des écrans existants. Hors périmètre : portraits d'agents, fusion boutique/recrutement, tutoriel, refonte de la disposition des écrans (sous-projets suivants), animations de transition.

## 1. Jetons (`Presentation/Common/Theme.cs`)
Les noms existants sont conservés (aucun appel ne casse), leurs valeurs changent :
- Fonds : ciel pastel (haut bleu glacé `#A9CDFA`, milieu lavande `#B7C4F5`/`#D3C0F0`, bas rose poudré `#F6C4D2`), lueur pêche à l'horizon.
- Surfaces : blanc givré (`Frost`, alpha 0,78 à 0,62) ; surface active `Navy` `#1B2550`.
- Texte : `Navy` par défaut, blanc sur fond foncé ou corail ; `TextMuted` bleu-gris `#3B4A8A`.
- Action et danger : `Accent` corail `#FF6B5E` (dégradé `#FF8F84` vers `#F0566A`).
- Nouveaux jetons : `Accent`, `AccentSoft`, `Navy`, `Frost`, `FrostEdge`, `SkyTop`, `SkyMid`, `SkyBottom`, `Glow`.
- Familles, raretés, types de case, PV, fatigue : mêmes teintes, adoucies en pastel, pour garder la reconnaissance du gameplay.
- Agents : une couleur signature pastel par agent (jaune beurre, bleu ciel, lavande, pêche pour les ennemis).

## 2. Composants (`Ui`, `Sprites`, `UiAssetBaker`)
- `Ui.Panel` : panneau verre givré à coins coupés (sprite 9-slice), filet clair, barre d'accent à gauche ; variantes `Accent` et `Navy`. Plus de contour noir ni d'ombre dure : une ombre douce.
- `Ui.Btn` : bouton biseauté, dégradé corail (principal) ou givré (secondaire) ; l'état désactivé reste grisé.
- `Ui.Bar` : barre fine, bords nets, sans contour noir.
- `Ui.Inked` : contour de texte remplacé par une ombre légère ou un décalage de couleur.
- Sprites ajoutés (cuits par `UiAssetBaker`) : panneau coin coupé 9-slice, losange, reflet diagonal, silhouette de ville.
- Polices : Barlow Condensed (italique, majuscules), en remplacement de Bangers et ComicNeue ; `FontAssetGenerator` produit les assets TMP.

## 3. Écrans (ordre de livraison, un lot chacun)
1. Jetons, composants, sprites, polices (socle).
2. Combat : frise à losanges, bloc « GO » corail au point d'action, cartes d'agents biseautées avec reflet, anneaux de compétences pastel (actif en `Navy` cerclé de corail), légende « Au tour de … » placée pour ne pas recouvrir les cartes.
3. Hub et menus : fond ciel + skyline, onglets de navigation (actif en `Navy`).
4. Plateau, popups, narration et le reste, y compris les 17 fichiers qui portent des couleurs en dur (balayage et remplacement par des jetons).
5. Captures avant/après de tous les écrans principaux.

## 4. Garde-fous
- Les tests EditMode et PlayMode existants restent verts (tutoriel inclus) ; les valeurs de couleur ou de taille qu'ils vérifient sont mises à jour.
- Test de contraste : chaque paire texte/fond du thème atteint un ratio minimal (4,5:1 pour le texte courant, 3:1 pour le grand texte).
- Chaque test porte un `[Description("Vérifie que … — parce que …")]`.
- Captures envoyées à l'utilisateur à chaque lot pour validation.

## Questions ouvertes
- Licence et rendu de Barlow Condensed en TMP (accents français) : à vérifier au lot 1.
- Lisibilité du pastel sur les petits éléments : si insuffisante, durcir `Navy` ou `Accent` localement sans toucher à la direction.

## Mise à jour (2026-10-12) : organisation du combat
Sur demande de l'utilisateur (croquis), l'écran de combat est réorganisé dans ce même lot :
- Frise en haut au centre ; ennemis en rangée sous la frise ; alliés en colonne à gauche (portrait rond, nom, PV, jauge, statuts) ; combo et compétences d'avatar en bas à gauche ; journal à droite.
- Éventail de 4 cartes de compétence en bas au centre, visible seulement pour l'agent dont c'est le tour ; cet agent a un contour lumineux.
- Appui maintenu sur une carte : description de l'effet au-dessus de l'éventail, jusqu'au relâchement.
- Polices : Barlow Condensed (téléchargée avec l'accord de l'utilisateur, licence OFL).
