# Combat : lisibilité et manipulation de l'ordre d'action (sous-projets J et K)

## J — Lisibilité
- Icônes d'action sous chaque agent en éventail de 4 (`SkillIconView`) : 1 et 2 propres à l'agent, 3 lourde, 4 réservée aux cartes du grimoire (verrouillée en attendant). Numéro, initiale, recharge (nombre + secteur), anneau quand lançable, blanc quand choisie.
- Frise agrandie au centre : bande de 1500 × 170, contour doré, marqueurs de 80 px, porte « 0 / ACTION » pulsée, zone de préparation en rouge.
- Rythme : `BalanceConfig.TimelineSpeedPercent` (70 par défaut) ralentit le remplissage des jauges.
- Le panneau de compétences de gauche disparaît ; le tutoriel désigne les icônes de l'agent prêt (marqueur vivant seulement quand une compétence est lançable).

## K — Ordre d'action
- `EffectType.PushBack` (Impact) : repousse un ennemi de N % de la frise ; ignore l'Ancrage et les unités prêtes ; annule l'action annoncée si l'ennemi repasse sous la zone de préparation ; un seul recul par action de la cible (`KnockbackGuard`).
- `EffectType.Advance` (Élan) : propulse un allié de N % ; à 100 % il devient prêt aussitôt (`GaugeFullEvent`, début de tour) et la frise se fige : il agit avant tout ennemi. Sans effet sur le lanceur ni sur une unité déjà prête.
- Contenu : Anchor A1 et Rempart A1 (recul 15 %), Verre A2 (recul 30 %), Arc A2 (élan 35 %), Étincelle A2 (élan 60 %).
