# Campagne de Brooklyn (état au 2026-10-12)

Source : texte fourni par l'équipe de game design. Le contenu est repris tel quel (orthographe d'origine) ; la zone 1.1 correspond à la zone « Red Hook — Les Quais » du prototype.

| Zone | Arrondissement | Lieu | Contexte | Objectifs | Conclusion |
|---|---|---|---|---|---|
| 1.1 | Brooklyn | Red Hook | Plusieurs rapports informent que l'AMIF fait transporter des méta-humains kidnappé par le port de Red Hook, mais attaquer l'AMIF de front sonnerait la fin de la Guilde et de sa crédibilité. Vous devez trouver des preuves. | 01 : infiltrer le quai (atteindre une case) ; 02 : couper le courant (mini-jeu générateur) ; 03 : trouver les documents de transport (atteindre une case) ; 04 : arrêter l'échange en cours (combat) ; 05 : fuir le quai | Vous découvrez le traitement des méta-humains et la vente aux mafieux, vous préparez un plan d'intervention. |
| 1.2 | Brooklyn | Bronwsville | La situation est plus critique que ce que l'on pensé, l'AMIF traite avec Antonio Kribelchi, dit « Crocodile », un mafieux méta-humains qui fait du trafique de méta-humains. L'AMIF les arrêtent pour leur vendre et financer leur armement. Votre équipe s'introduit dans le quartier de Brownsville pour libérer les otages. | - | Les otages sont libérés mais Crocodile est plus que remonté, il compte bien vous faire payer. |
| 1.3 | Brooklyn | Coney Island | Après l'opération de Brownsville, Crocodile et ses hommes ont décidé de défier la Guilde en prenant Coney Island en otage pour faire venir votre équipe. C'est un piège, mais vous devez sauver les innocents. | - | Vous réussissez à vaincre Crocodile mais Coney Island a été ravagé dans les affrontements. |
| 1.4 | Brooklyn | Dumbo & Brooklyn Bridge | Après l'incident de Coney Island, l'AMIF a quadrillé Brooklyn afin de neutraliser votre équipe et le gang de Crocodile d'un seul coup. | - | L'affrontement est stoppé par Rayner qui interviennent en menaçant Stroke de révéler ses petites affaires. Le Général arrête Crocodile, désigné comme coupable des évènements, mais met en garde Rayner que le tour de la Guilde viendra une fois les Accords de Salem signés. |

## Notes d'intégration (à valider)
- Les objectifs des zones 1.2 à 1.4 ne sont pas encore définis (« - »).
- Personnages nommés : Antonio Kribelchi dit « Crocodile » (mafieux méta-humain), Rayner (Guilde), Stroke, le Général (AMIF).
- Événement à venir : signature des Accords de Salem.

## Intégration au jeu (version 0.8.1)
Les quatre zones sont jouables (spec `Code/docs/specs/2026-10-12-campagne-brooklyn-design.md`) : 1.1 Red Hook (les cinq objectifs de l'équipe), 1.2 Brownsville (infiltrer, neutraliser le lieutenant de Crocodile, libérer les otages, les mettre à l'abri), 1.3 Coney Island (évacuer les civils, briser le piège, vaincre Crocodile), 1.4 Dumbo et le pont de Brooklyn (percer le cordon de l'AMIF, couper son réseau, tenir le pont puis scène de Rayner). Déblocage linéaire.
