# Plateau et déplacement (v2) — design

> Statut : demandé par l'utilisateur (2026-10-07), implémenté sur `feature/plateau-deplacement` (départ de `main`).
> Sous-projet **B** du découpage A → B → C → E → D (voir `2026-10-07-combat-active-timeline-design.md`).
> Complète `01_Game_Design/01_Plateau.md` (déplacement, brouillard) ; la règle d'arrêt forcé du GDD n'est pas modifiée.

## 1. Règles demandées
1. Plateau **plus grand**, avec la possibilité de **naviguer d'une case à l'autre**.
2. Pendant le déplacement, **toutes les cases disponibles, même cachées**, sont en surbrillance.
3. En mode déplacement, **sélectionner une case ouvre un pop-up** qui la décrit ; la caméra **dézoome** et montre le chemin **illuminé d'une autre couleur** ; le pop-up porte un bouton **« S'y rendre »** qui valide le déplacement.

## 2. Décisions de design
- **« Cachées » = sous le brouillard** (confirmé par l'utilisateur). Une case cachée à portée des PM est surlignée (« ? », badge = distance). Comme le GDD impose un **arrêt forcé** sur toute case qui ouvre le brouillard, on ne peut pas s'arrêter sur une case cachée : sa fiche annonce « Zone inconnue » et « S'y rendre » mène au **premier arrêt forcé** du chemin (`FogReachPlan.StopAt`), où le brouillard s'ouvre ; les PM restants permettent de repartir. Le chemin au-delà de l'arrêt est dessiné en clair (`LinkMark.Beyond`).
- **Domaine** : `MovementRules.FindFogReach` (cases cachées à 1..PM pas, arrêts ignorés, sans retour immédiat) ; `ZoneRun.FogReach` est recalculé avec `Destinations` et vidé quand les PM sont versés. Aucune règle de déplacement existante ne change.
- **Pop-up = fiche non modale** dans le prefab Board (pas de voile : le joueur doit voir le chemin). Contenu : type de case, effet, coût en PM, PM restants, arrêt forcé (combat / ouverture du brouillard), case déjà résolue. Toucher une autre destination remplace la fiche ; Annuler / Échap la ferme (et recentre sur le pion) avant tout « Retour au QG ».
- **Déplacement en deux temps** : toucher = prévisualiser, « S'y rendre » (ou Entrée) = jouer. Les tests PlayMode passent par `PlayModeHarness.TapNode` (toucher + confirmer).
- **Plateau agrandi** : espacement ×1,6 (`StepX` 170, `StepY` 250) dans une fenêtre à caméra : glisser pour se déplacer, molette ou boutons +/- pour zoomer (0,4 à 1,6), « PION » recentre ; la caméra suit le pion après un déplacement et dézoome sur le chemin (en laissant la place à la fiche).
- **Navigation case à case** : flèches / WASD / croix de la manette déplacent un anneau blanc le long des liens visibles ; Tab, boutons « < CASE » / « CASE > » et épaules de la manette passent d'une destination surlignée à l'autre ; Entrée / bouton A de la manette valide (= toucher). Raccourci « Terminer le déplacement » : **T** (Entrée sert maintenant à « S'y rendre »).
- Pincer pour zoomer (tactile) : hors périmètre ; les boutons +/- et le glisser couvrent le mobile.

## 3. Hors périmètre
Éditeur de plateaux (sous-projet E : positions, liens, fond de carte généré), contenu des pools de cases, tutoriel mis à jour pour la fiche « S'y rendre ».
