# Retours du playtest 0.7.0 → version 0.8.0 : bugs, rythme, boutique, interface finale

## But
Traiter les retours du playtest 0.7.0 et livrer une interface complète et finale (hors portraits illustrés, remplacés par des silhouettes stylisées provisoires). Ordre validé : bugs, rythme du combat, tutoriel, boutique, finition de l'interface, puis APK 0.8.0.

## Points forts à préserver
Combat et interface générale nettement plus lisibles (organisation du combat et HUD pastel de 0.7.0).

## 1. Bugs

### 1.1 Arrêt en plein déplacement sur le plateau
Règle voulue : le pion se déplace sur tout son trajet et ne s'arrête qu'à une case de combat non résolue (seules cases d'arrêt).
- Aujourd'hui `MovementRules` arrête aussi le trajet sur les cases qui ouvrent du brouillard (`WouldReveal`) : c'est l'arrêt « en plein mouvement ».
- Nouveau comportement : le brouillard ne bloque plus. Toute case à portée (coût ≤ PM) sans traverser un combat non résolu est une destination, qu'elle soit visible ou non. Le brouillard se lève au passage du pion (les cases s'éclaircissent au fil du déplacement). Arrivé sur un combat, le pion s'arrête et le combat se lance.
- Les cases sous brouillard restent surlignées pour montrer la portée ; `FogReachPlan` et l'aperçu « arrêt au brouillard » disparaissent.
- Tests : trajet traversant une case d'ouverture de brouillard sans s'arrêter ; arrêt sur un combat ; le brouillard est levé à la fin ; les tests de déplacement existants sont mis à jour.

### 1.2 Glisser-déposer des cartes de compétence
- Appui prolongé puis glisser : la carte suit le doigt (agrandie, légèrement penchée), l'éventail laisse un vide.
- Compétence générale ou sur soi-même (sans cible) : on la dépose dans le vide, au-dessus de l'éventail (zone « terrain », surlignée pendant le glissement) ; elle se lance au relâchement.
- Compétence ciblée : on la dépose sur une carte cible valide (les cibles valides s'allument, anneau déjà existant) ; relâchement ailleurs : la carte revient dans l'éventail sans rien lancer.
- Le toucher simple (tap) reste possible comme aujourd'hui ; l'appui maintenu sans mouvement affiche toujours la description.
- Tests : dépose dans le vide lance une compétence générale ; dépose sur un ennemi lance une compétence ciblée ; dépose invalide annule ; tap et description inchangés.

## 2. Rythme et animations du combat

### 2.1 Combat plus rapide
Constat : une pause de 1,5 s après chaque pas de la frise quand le point d'action est vide donne environ 20 s entre deux tours.
- La fenêtre d'interruption ne s'ouvre plus qu'au pas qui précède l'arrivée d'une unité ennemie sur le point d'action (menace imminente) et seulement si au moins une compétence d'avatar est prête ; durée 1,2 s ; jamais en Auto.
- Pas de la frise : 0,5 s → 0,35 s. Délai de réflexion de l'IA : 0,5 à 1,5 s → 0,3 à 0,7 s.
- Vitesse ×2 par défaut au premier lancement ; ×1 et ×3 restent disponibles.
- Objectif mesurable : environ 5 s entre deux tours d'une unité partie du départ.
- Tests : pas de pause sans menace ou sans avatar prêt ; pause d'au plus 1,2 s sinon ; durée simulée entre deux tours.

### 2.2 Animations de combat
- Éventail : les cartes montent et s'ouvrent au début du tour de l'agent (0,25 s, décalées), se referment à la fin.
- Jouer une carte : elle s'envole vers la cible (ou le terrain), grossit puis s'efface avec un éclat à la couleur de la compétence.
- Attaquant : léger élan vers sa cible ; cible : secousse, flash blanc, éclats, nombre flottant (existants, accentués).
- Pastilles de la frise : glissement fluide, étincelle à l'arrivée sur le point d'action.
- Les animations respectent la vitesse du combat et sont désactivables avec « réduire les animations » (accessibilité).

## 3. Tutoriel
Les mises en valeur deviennent des rectangles à coins coupés, bien cadrés autour de l'élément (marge 8 px), avec un liseré clair pulsant et quatre équerres animées aux coins, à la place des ronds. Le voile reste découpé à la forme du rectangle.

## 4. Boutique unique à onglets et ouverture de booster
- Une seule « Boutique » à onglets : **Héros** (recrutement du jour, aujourd'hui l'onglet « Recruter »), **Boosters**, **Objets** (objets du jour), **Gemmes et crédits**, **Cadeau du jour**. L'onglet « Recruter » de la barre de navigation disparaît (4 entrées : QG, Équipe, Pass, Boutique).
- Ouverture de booster : popup plein écran. Le paquet apparaît, appui pour l'ouvrir (flash, secousse), les cartes sortent et se retournent une par une avec lueur selon la rareté (légende, élite, agent), résumé final avec « Nouveau » sur les premières obtentions et bouton Continuer. Passage rapide en un appui.
- Réutilise `CollectionBook.OpenBooster` (aucune règle de jeu modifiée).
- Tests : navigation entre onglets ; onglet Héros garde le recrutement existant ; ouverture de booster affiche autant de cartes que le tirage ; le bouton Continuer ferme.

## 5. Interface finale
- Audit des 19 écrans du document UX (`03_Art_UI_UX/01_UX_Ecrans.md`) : chacun existe, est habillé HUD pastel, lisible, sans texte de moins de 20 px, sans couleur illisible. Capture de chaque écran dans `Code/docs/test-reports/captures` et planche récapitulative.
- Silhouettes stylisées provisoires pour tous les agents et ennemis : buste (tête et épaules) à la couleur de la famille avec reflet et initiale, cadré en rond (alliés), en carte (ennemis) et en fiche. Un emplacement d'image par agent est prévu pour les vrais portraits plus tard (repli sur la silhouette).
- Écrans manquants ou incomplets relevés pendant l'audit : ajoutés dans ce lot s'ils sont petits, sinon listés dans le rapport final.

## Hors périmètre
Vrais portraits illustrés ; équilibrage ; nouveau contenu de jeu.

## Questions ouvertes (décisions prises par défaut, à corriger au besoin)
- Fenêtre d'interruption : conditionnée à la menace imminente et à un avatar prêt (au lieu de chaque point vide).
- Brouillard : levé au passage, destinations possibles dans le brouillard.
- Vitesse par défaut ×2.
