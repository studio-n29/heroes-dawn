# Synergie : bonus passif, liens entre agents, effets d'équipe

> Spec du 2026-10-23. Source : `01_Game_Design/02_Combat_Evolutions.md` (« Modification sur la synergie : bonus passif, lien entre agent unique (niveau de synergie qui augmente l'effet du bonus passif), création d'effet unique déterminé par la combinaison d'équipe »).

## Trois couches
1. **Bonus passif** (inchangé) : chaque paire de familles dans l'escouade donne à toute l'équipe un bonus permanent de statistique, plafonné à 30 % par statistique.
2. **Lien entre deux agents précis** : chaque victoire remportée ensemble le fait progresser. Paliers : 3, 8, 15 et 25 victoires communes pour les niveaux 2 à 5. Chaque niveau ajoute 25 % à l'effet passif de la synergie que forment leurs deux familles (100, 125, 150, 175, 200 %), toujours sous le plafond de 30 %. Exemple : la résonance vitale (+4 % de PV) passe à +8 % au niveau 5.
3. **Effets uniques de composition** (appliqués à chaque allié au début du combat) :

| Effet | Condition | Effet |
|---|---|---|
| Quatuor | 4 familles différentes | +10 % de chances de critique |
| Trinité | un tank, un agent de dégâts et un soutien | +1 énergie par pas de la frise |
| Double front | 2 tanks ou plus | bouclier d'ouverture de 10 % des PV |
| Étincelle | 2 agents de contrôle ou plus | +20 énergie au départ |

Les effets se cumulent (Quatuor et Trinité ensemble, par exemple).

## Données
- `MetaSave.links` : victoires communes par paire (identifiants triés), enregistrées dans le profil. Un profil sans liens garde tous ses liens au niveau 1.
- `CombatSetup.WithLinks` transmet les niveaux de lien des paires de l'escouade au moteur ; sans lien, niveau 1 (simulateur d'équilibrage et rejeu inchangés).
- `FinishCombatUseCase` ajoute une victoire à chaque paire d'agents engagés quand le combat est gagné ; une défaite n'en ajoute pas.

## Interface
- Écran Équipe : chaque synergie affiche son bonus amplifié et le niveau de lien (« +8 % PV (lien niv. 5) ») ; les effets de composition actifs sont listés dessous.
- Journal de combat : une ligne par synergie (avec le niveau de lien) et une par effet de composition, au début du combat.

## Hors périmètre
- Écran dédié aux liens (progression, victoires restantes) ; récompenses de niveau de lien ; textes de lore de chaque lien.
- Effets propres à des duos nommés d'agents ; équilibrage fin des pourcentages.

## Tests
`LinkAndTeamEffectTests` (paliers, multiplicateur, plafond, niveau de lien en combat, détection et application des 4 effets, progression des paires), `CombatUseCaseTests` (victoire = lien +1, défaite = rien), `TeamPagePresenterTests`. Edit Mode 704 réussis, Play Mode 53 réussis.
