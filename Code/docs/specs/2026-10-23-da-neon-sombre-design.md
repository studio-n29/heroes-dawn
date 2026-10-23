# Direction artistique : thème sombre néon « verre » (sous-projet D du playtest 0.10.1)

## But
Retour de playtest 0.10.1 : « polish de l'interface visuelle pour un effet AAA, avec glassmorphism, néons légers et ombrage ». Le thème pastel (spec 2026-10-12) est remplacé par un thème **sombre néon** sur tout le jeu (hub et combat). Les noms de jetons et la structure de `Ui` sont conservés : seules les valeurs et le rendu des composants changent.

## Décisions validées
- Direction : thème sombre néon partout (pas d'hybride, pas de pastel conservé).
- Néon principal : cyan électrique ; action : corail.
- Livraison : 4 lots avec captures et validation à chaque lot.
- Verre : faux verre (translucide, dégradé, reflet, filet) sans flou temps réel, pour tenir sur mobile. Un vrai flou n'est envisagé que sur quelques panneaux fixes, plus tard.

## Périmètre
Habillage uniquement : jetons, composants, sprites, shaders, repeinture des écrans existants.
Hors périmètre : refonte de disposition (sous-projet C), HUD des bonus de combat (F), animations de compétence (E), visuels de cartes (G), et **toutes les illustrations** (logo, key arts, `HD_BG_*`, skyline, cinématique d'intro, banque d'illustrations), pastel aujourd'hui et à refaire par l'utilisateur pour le thème sombre. En attendant, un fond nuit par défaut les remplace.

## 1. Jetons (`Presentation/Common/Theme.cs`)
Les noms historiques (`Surface`, `Text`, `Overlay`, `Primary`, `Danger`, `Hp`, etc.) restent et changent de valeur.
- Fonds : nuit `#0B1020` (haut) vers indigo `#1A1033` (bas).
- Verre : `Glass` `#141B38` (alpha 0,62 à 0,78) ; `GlassHi`, reflet plus clair en haut.
- Néon : `Neon` cyan `#25E6FF` (liseré et halo des éléments actifs ou sélectionnés).
- Action : `Accent` corail `#FF6B5E` (dégradé `AccentSoft` vers `AccentDark`), halo léger sur les boutons principaux.
- Texte : `Text` `#EAF1FF` ; `TextMuted` `#9AA8D4`. `Navy` n'est plus utilisé pour le texte.
- Familles, raretés, PV, fatigue, types de case : mêmes teintes de reconnaissance, resaturées pour fond sombre (le pastel les avait éclaircies via `Soft`).
- Tous les anciens jetons pastel devenus sans objet (`SkyTop`, `SkyMid`, `SkyBottom`, `Frost`, `Glow`) sont supprimés ou réaffectés ; les appels sont migrés dans le lot concerné.

## 2. Composants (`Ui`, `Sprites`, shaders, `UiAssetBaker`)
- `Ui.Panel` : verre sombre à coins coupés ; ombre en trois couches (contact, moyenne, diffuse) ; reflet haut (`GlassHi`) ; liseré cyan fin à alpha variable. Variantes `Active` (liseré et halo pleins) et `Accent` (corail).
- `Ui.Btn` : principal en dégradé corail avec halo ; secondaire en verre avec liseré cyan ; désactivé en verre grisé, sans halo.
- `Ui.Bar` : fond sombre, remplissage avec léger glow, bords nets.
- `Ui.Inked` : ombre légère et glow de texte à la place du contour noir.
- Shaders (`Resources/UI/Shaders`, matériaux via `UiMaterialBuilder`) : nouveau `HD/UI/NeonEdge` (liseré avec halo) et `HD/UI/GlassSheen` (reflet diagonal discret) ; `HD/UI/Backdrop` adapté (vignettage sombre, trame de points retirée).
- Polices : Barlow Condensed inchangée.

## 3. Lots de livraison
1. **Socle** : `Theme`, `Ui.Panel`/`Btn`/`Bar`/`Inked`, sprites, shaders, matériaux, test de contraste.
2. **Combat** : frise, cartes d'agents, anneaux de compétences, journal, fond.
3. **Hub et menus** : fond, navigation, pages.
4. **Reste** : plateau, popups, narration, balayage des 13 fichiers de `Presentation/` qui portent des couleurs en dur (remplacement par des jetons).

Captures envoyées à l'utilisateur à la fin de chaque lot.

## 4. Garde-fous
- Test de contraste automatique : chaque paire texte/fond du thème atteint 4,5:1 (texte courant) ou 3:1 (grand texte).
- Les tests EditMode et PlayMode existants restent verts (tutoriel inclus) ; les valeurs de couleur ou de taille qu'ils vérifient sont mises à jour.
- Chaque test porte un `[Description("Vérifie que … — parce que …")]`.

## Risques et questions ouvertes
- Le néon et les halos coûtent du fill-rate sur mobile : mesurer sur Android au lot 2 et réduire le nombre de couches d'ombre si besoin.
- Lisibilité du texte `TextMuted` sur verre translucide devant un fond clair (illustrations pastel encore présentes) : corrigée localement en augmentant l'alpha du verre.
- Les illustrations pastel existantes jurent avec le thème tant qu'elles ne sont pas refaites ; c'est assumé et signalé à chaque capture.
