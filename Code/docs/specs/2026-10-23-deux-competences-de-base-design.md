# Deux compétences de base uniques par agent

> Spec du 2026-10-23. Source : `01_Game_Design/02_Combat_Evolutions.md` (« Clarification : 2 compétences de base par agent (compétences uniques) »).

## Règles
- Chaque agent (30 agents numérotés, Agent D et Agent M) a **deux compétences de base** au lieu de trois : la 1re, légère et gratuite ; la 2e, spéciale, qui coûte 30 énergie (40 pour un contrôle).
- Les compétences sont **propres à l'agent** : un nom à lui (ex. « Jet de flammes » et « Brasier » pour Mei-Lin Chen) et un statut lié à son pouvoir (Brûlure, Gel, Choc, Corrosion, Entrave, Vulnérable, Marque côté ennemi ; Régénération, Hâte, Bouclier, Esquive, Ancrage côté allié), ajouté à la base de son rôle.
- Base par rôle : tank (coup + recul ; provocation + bouclier), dégâts (coup ; zone), contrôle (coup ; entrave + recul), soutien (coup léger ; soin + élan), précision (coup + marque ; vulnérabilité + jauge ennemie).
- **L'éventail du combat montre trois places** : les deux compétences, puis la carte du grimoire équipée (place « + » tant qu'aucune carte n'est équipée). Une 2e carte prend la 4e place, visible seulement si elle existe.

## Données et outils
- `PrototypeAgents.Kit` construit les deux compétences ; `Flavors` donne le statut propre à chaque agent ; les noms sont dans `LocDawnAgents`.
- `agents.json` / `skills.json` régénérés (64 compétences). L'outil Agents n'accepte plus que deux compétences par agent.
- Les anciens agents de test gardent leurs trois compétences (`LegacyRoster`).
- `UnitFactory.SkillsWithCards` : la carte 1 s'ajoute en 3e place pour un agent à deux compétences (elle remplaçait la 3e compétence avant).

## Limites
- Les chiffres (dégâts, pourcentages de statut) viennent des gabarits par rôle : seuls les noms et les statuts distinguent les agents. Des effets entièrement sur mesure par agent restent à écrire avec le game design.

## Tests
`AgentRosterTests` (2 compétences, 1re gratuite et 2e payante, kits variés, cartes en 3e et 4e place), `AgentsPagePresenterTests` (noms uniques, plus de nom générique). Edit Mode 708 réussis, Play Mode 53 réussis.
