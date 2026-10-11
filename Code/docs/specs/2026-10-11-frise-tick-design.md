# Frise de combat par tick (sous-projet 1 : frise et tour)

## But
Remplacer la jauge continue du combat Active Timeline par une frise à points discrets. Le joueur voit clairement où en est chaque unité, décide de ses actions au bon moment, et peut interrompre ou parer pendant une pause courte.

## Périmètre
Ce document couvre la frise, le tour d'un agent, la reprise après action, la fenêtre d'interruption et les rencontres sur un même point. L'habillage visuel, la boutique, le tutoriel et les portraits font l'objet de sous-projets séparés.

## 1. Frise
- 10 points entre le départ (point 0) et le point d'action (point 10).
- Chaque point est vide ou occupé par une unité (alliée ou ennemie).
- Le temps avance d'un point toutes les 0,5 s, pour toutes les unités.
- La vitesse (`Spd`) ne règle plus le remplissage. Elle n'intervient plus dans la frise.
- Environ 5 s entre deux tours d'une unité partie du départ.
- Les unités vivantes démarrent à des points aléatoires entre 0 et 2, pour que les premiers tours ne tombent pas tous en même temps (graine déterministe).

## 2. Reprise après action
Une unité repart sur la frise selon le poids de sa compétence :
- Léger (emplacement 1) : point 3.
- Intermédiaire (emplacement 2) : point 2.
- Lourd (emplacement 3 et cartes 4) : point 0.
- Combo : point -2, en canalisation (il lui faut rattraper deux points de plus).

## 3. Tour d'un agent
- Quand une unité atteint le point d'action, c'est son tour. La frise s'arrête tant que le tour n'est pas joué.
- Allié (joueur) : 30 s maximum pour choisir. L'éventail de compétences apparaît en bas de l'écran. Une compétence générale se pose sur le board ; une compétence ciblée se pose ensuite sur une cible.
- Ennemi (IA) : choix presque immédiat, après un délai aléatoire court entre 0,5 et 1,5 s, pour simuler la réflexion.
- Si le joueur ne choisit pas dans les 30 s, l'unité passe son tour et repart au point de reprise léger (point 3).

## 4. Points vides et fenêtre d'interruption
- Chaque point vide traversé par la frise met celle-ci en pause de 1,5 s.
- Pendant cette pause, le joueur peut lancer une interruption : une compétence d'avatar prête (cf. `CombatSimulation.Avatar`, recharge en temps actif).
- Les compétences d'avatar ne se lancent que pendant cette fenêtre, ou au tour de l'agent concerné. Hors de ces moments, elles sont désactivées.
- Pas de choix dans les 1,5 s : la frise reprend son cours.

## 5. Rencontres sur un même point
- Deux alliés sur le même point : combo (le combo existant, déclenché par la rencontre).
- Un allié et un ennemi sur le même point : parade. L'action de l'ennemi est annulée, et l'ennemi repart au point de reprise léger (point 3).
- Deux ennemis sur le même point : rien.
- Une parade ne coûte rien à l'allié (à confirmer dans le plan si besoin).

## 6. Ce qui change dans le code (vue d'ensemble, pas le plan)
- `CombatSimulation` : la boucle de tick avance point par point ; la jauge en milli-points disparaît au profit d'un index de point.
- `GaugeAfter` : les positions de reprise deviennent des index de point.
- `TimelineView` : affichage en points discrets avec les segments de ton croquis.
- Les interruptions reprennent les compétences d'avatar existantes, avec la fenêtre de pause.

## Hors périmètre
- Habillage (couleurs, icônes, portraits).
- Boutique et recrutement fusionnés.
- Tutoriel.

## Questions ouvertes (à trancher dans le plan)
- Valeurs exactes du délai IA (0,5 à 1,5 s) : à régler après essais.
- Ce qui se passe si une unité est repoussée (recul) : elle recule d'un point ou de plusieurs ? (aujourd'hui, un recul en pourcentage). À convertir en nombre de points.
- Affichage de l'éventail de compétences et de la fenêtre de 1,5 s : maquette à faire avec le sous-projet « combat lisible ».
