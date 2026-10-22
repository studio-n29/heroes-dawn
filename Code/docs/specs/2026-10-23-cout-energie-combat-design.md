# Coût des compétences et énergie par tick (combat)

> Spec du 2026-10-23. Source : `01_Game_Design/02_Combat_Evolutions.md` (« Coût d'utilisation des compétences, régénération par tick »). Première étape de la refonte du combat ; la synergie, les 2 compétences de base et la pause d'animation suivent.

## Règles
- Chaque unité (alliée ou ennemie) a une **énergie** de 0 à 100 (`EnergyMax`). Elle commence à 30 (`EnergyStart`).
- Elle gagne **3 énergie par pas de la frise** (`EnergyRegenPerTick`), plafonnée au maximum. Pas de régénération pendant la pause d'action (un allié prêt attend l'ordre) : prendre son temps ne rapporte rien.
- Chaque compétence a un **coût** (`SkillDef.Cost`). Elle est refusée si l'énergie est inférieure au coût ; l'usage retire le coût. La compétence de base (A1) coûte 0 : un agent peut toujours agir.
- **La recharge par tour disparaît du jeu.** Le champ `Cooldown` reste dans les données mais n'a plus d'effet : sans coût explicite, le coût vaut 10 par tour de recharge historique (A2 = 30, A3 = 50).
- L'IA ennemie et le mode Auto suivent les mêmes règles (ils ne choisissent que des compétences payables).

## Données et outils
- `skills.json` : champ `cost` (0 à 100) ; l'outil Compétences l'affiche et le valide. Export CSV : colonne « coût » à la place de « recharge ».
- Équilibrage : trois réglages dans `BalanceConfig` (énergie max, de départ, régénération par pas).

## Interface
- Cartes de compétence : pastille de coût (rouge si l'énergie manque) ; les cartes non payables sont grisées comme avant.
- Carte d'unité : la petite jauge sous les PV montre l'énergie (bleue) ; la frise du haut montre le temps.

## Tests
`EnergyTests` (coût par défaut, énergie de départ, régénération et plafond, refus et dépense, pas de régénération pendant la pause, IA qui ne triche pas et combat qui se termine). Edit Mode 687 réussis, Play Mode 53 réussis.

## Reste de la refonte du combat
Synergie (bonus passif, niveaux de lien, effets de composition), 2 compétences de base par agent, temps d'animation de résolution et animations par famille de compétence.
