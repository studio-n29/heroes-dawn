# 30 agents par ville et déblocages (sous-projet N)

## Roster
- 8 agents d'origine + 22 nouveaux (`PrototypeAgents`) = **30** pour la première ville. Quatre familles et cinq rôles couverts ; chaque agent a trois compétences construites sur son rôle et sa famille (statut signature par famille, reculs et élans pour les rôles Tank, Contrôle, Soutien).

## Quatre voies d'obtention (`MemberDef.Unlock`)
| Voie | Règle | Agents |
|---|---|---|
| Boutique | rotation quotidienne, PR (`InShop`) | 14 à acquérir + 4 de départ |
| Arc narratif | l'agent rejoint à la validation de la zone | Vanguard (1.1), Oracle et Meteor (1.2), Monolith (1.3) |
| Événement temporaire | zone validée un jour d'événement (week-end) | Forge (1.1), Tempest et Mend (1.2), Mirage (1.3) |
| Prouesse | zone validée avec un seul agent au combat, ou sans aucune défaite | Lone Wolf (1.1) et Hermit (1.3) en solo ; Phoenix (1.2) et Aegis (1.3) sans défaite |

- `AgentUnlocks.OnZoneValidated` est appelé à la validation d'une zone (`ZoneActions`), avec l'horloge du téléphone (`ZoneEffects.Clock`) ; `CatchUpArcs` rattrape les arcs déjà accomplis au chargement d'un profil.
- `GameSession.RunEngaged` : agents engagés au combat pendant le plateau en cours (remis à zéro à chaque nouveau plateau).
- La boutique n'offre jamais un agent d'arc, d'événement ou de prouesse.
- Les agents débloqués arrivent au niveau de recrue, hors escouade ; l'écran de zone sécurisée annonce leur nom (et le booster gagné).

## Interface
- Page **Registre des agents** (depuis la page Recruter) : les 30 agents, ceux de l'équipe d'abord, la condition pour les autres, l'événement du jour mis en avant ; toucher un agent de l'équipe ouvre sa fiche.
