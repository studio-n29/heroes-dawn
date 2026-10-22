# Pause de résolution et animations par famille de compétence

> Spec du 2026-10-23. Source : `01_Game_Design/02_Combat_Evolutions.md` (« temps d animation de résolution de la compétence, pause pour voir tous les déplacements sur la frise et l animation de la compétence ; animation par famille de compétences »).

## Règles
- Après chaque compétence jouée (alliée ou ennemie), le combat marque une **pause de résolution** de 1,3 s (`BalanceConfig.CombatResolutionMs`, divisée par la vitesse du combat). La frise ne bouge pas : on voit l animation dans le terrain central et les reculs ou élans des marqueurs se terminer sur la frise.
- Sans animation (mouvements réduits), la vue renvoie 0 et il n y a aucune pause.
- Si un autre allié est prêt, son tour reste immédiat : la pause ne retient que la frise.

## Familles d animation
Déduites des effets de la compétence (`SkillFx.KindOf`) : sur l ennemi, le premier statut élémentaire (Brûlure : feu, Gel : givre, Choc : foudre, Corrosion : acide, Entrave : chaînes), puis Marque ou Vulnérable : marque, puis gestion du temps, sinon impact ; sur un allié : soin, bouclier ou hâte.

| Famille | Couleur | Animation à l arrivée |
|---|---|---|
| Feu | orange | projectile avec traînée, gerbe orange et rouge |
| Givre | bleu glace | éclat en losange, éclats et anneau |
| Foudre | jaune | trait rapide, étincelles larges et flash de la carte |
| Acide | vert | projectile avec traînée, gouttes |
| Chaînes | gris | anneaux qui se resserrent |
| Marque | rouge | anneau qui se resserre en cible |
| Soin | vert clair | étincelles qui montent, anneau |
| Bouclier | bleu | deux anneaux qui s ouvrent |
| Hâte | doré | étincelles rapides |
| Temps | violet | anneau inversé |
| Impact | crème | gerbe d étincelles |

Le nom de la compétence s affiche dans le terrain pendant la pause, dans la couleur de sa famille.

## Limites
- Animations procédurales (formes et étincelles), sans dessins ni sons propres à chaque famille.
- Les dégâts flottants apparaissent à l instant du lancer, pas à l arrivée du projectile.

## Tests
`SkillFxTests` (familles, ordre de priorité, couleurs distinctes, au moins 8 familles dans le roster), `CombatPresenterTests` (animation demandée, pause de résolution, aucune pause sans animation). Capture `04b` et `04c` du test de captures. Edit Mode 720 réussis, Play Mode 53 réussis.
