# Combat : frise, énergie et synergies

> Résumé dans le GDD : `00_GDD/GDD_Heroes_Dawn.md` §6 · Statut : **réaligné sur la version 0.10.1 (2026-10-23)**
> Kits des agents : `06_Kits_Competences.md` · Roster : `03_Metahumains.md` et `02_Lore_Narration/04_Agents_The_Dawn.md` · Valeurs : `05_Equilibrage.xlsx` et `BalanceConfig` dans le code
> Spécifications détaillées : `Code/docs/specs/` (frise, énergie, synergie, deux compétences, pause de résolution, disposition).
> ⚑ = valeur proposée, à équilibrer en playtest.
> Les anciennes règles (jauge ATB, combos de famille, duos légendaires, compétences d'avatar) ont été remplacées ; leur ancienne description est dans l'historique git.

## Objectif du système
Des combats **courts**, lisibles sur mobile, où le joueur crée de la valeur par ses **choix d'équipe** (familles, rôles, liens entre agents) et par le **moment** où il agit (pilier « Mon équipe, mes choix »). Le mode Auto permet de rejouer vite.

## La frise
- Chaque unité occupe un point de la **frise** : 10 points entre le départ (0) et le **point d'action** (10). Toutes les unités avancent d'un point à chaque pas : **0,35 s** (×2 par défaut ; ×1 et ×3 disponibles). Au départ, chaque unité est placée au hasard entre les points 0 et 2.
- Quand une unité atteint le point d'action, c'est son tour. Pour un allié, **la frise se fige** : le joueur a 30 s pour choisir, sinon l'agent passe son tour. Un ennemi choisit après un court délai (0,3 à 0,7 s).
- Après son action, l'unité repart sur la frise selon le poids de la compétence : **A1 (légère) au point 3**, **A2 au point 2**, **carte du grimoire au point 0**.
- **Zone de préparation** (points 8 à 10) : l'action d'un ennemi y est **annoncée** sur la frise (dès le point 5 si un allié Perception est dans l'équipe).
- **Parade** : un allié et un ennemi sur le même point annulent l'action annoncée de l'ennemi, qui repart au point 3.
- **Critique en zone de préparation** : l'action de la cible avorte et elle repart au point 4.
- **Recul** : Entrave repousse de 25 % de la frise, Choc de 15 %, Gel de 20 % (sans effet sous Ancrage).
- **Pause de résolution** : après chaque compétence jouée, la frise attend **1,3 s** (divisée par la vitesse). On voit l'animation dans le terrain et les marqueurs finir leurs déplacements sur la frise. Sans animation (mouvements réduits), pas de pause.

## Déroulement
1. Arrivée sur la case → écran de combat : alliés à gauche, ennemis à droite (1 à 4 par camp, en deux rangées décalées), frise en haut, compétences de l'agent actif en bas.
2. La frise avance. Au tour d'un allié, ses compétences s'ouvrent en éventail : **manuel** (le joueur choisit une compétence puis une cible, au toucher ou par glisser-déposer) ou **auto** (l'IA choisit).
3. **Victoire** : tous les ennemis à 0 PV (éventuellement après plusieurs vagues) ; **défaite** : tous les alliés à 0 PV. **Retraite** = défaite (recul d'une case + fatigue de défaite).

## Statistiques
| Stat | Rôle |
|---|---|
| PV | Points de vie |
| ATQ | Base des dégâts et de certains soins |
| DEF | Réduit les dégâts reçus |
| VIT | Statistique d'agent (ne règle plus le remplissage : tout le monde avance d'un point par pas) |
| Critique | 15 % de base, ×1,5 dégâts |
| Énergie | 0 à 100, voir ci-dessous |

Valeurs de base par **rôle** (Tank, Dégâts, Contrôle, Soutien, Précision) et par rareté : onglet **Stats** du tableur.

## Énergie
- Chaque unité a une **énergie** (max 100, **30 au départ** ⚑) qui se **régénère de 3 par pas** de la frise ⚑, sauf pendant le tour d'un allié qui attend l'ordre.
- Une compétence **coûte de l'énergie** : **A1 gratuite**, **A2 : 30** (40 pour un contrôle), carte du grimoire : 40 ou 50. Elle est refusée si l'énergie manque. **Il n'y a plus de recharge par tour.**
- Conséquence : un agent a toujours une action (A1), et la compétence forte se mérite en laissant l'énergie remonter (environ 10 pas pour 30).
- Les ennemis suivent les mêmes règles.

## Formules
- **Dégâts** = ATQ × % de la compétence × (1 − DEF / (DEF + K)) × multiplicateurs (critique, Vulnérabilité, bonus). K = 300 de base.
- **Variation** : ±5 % aléatoire par coup.
- **Soins** : en % de la cible (précisé dans chaque kit).
- **Brûlure** : 8 % des PV max par frise complète, versés point par point.

## Compétences
- Chaque agent a **2 compétences de base** propres à lui (nom, statut lié à son pouvoir) : A1 légère et gratuite, A2 spéciale. La **3e place** de l'éventail est une **carte du grimoire** équipée (une 2e carte prend la 4e place). Détail : `06_Kits_Competences.md`.
- Les altérations et leur durée : glossaire de `06_Kits_Competences.md`. Une même altération ne se cumule pas : une nouvelle application rafraîchit la durée.
- Chaque compétence jouée est **animée dans le terrain** selon sa **famille** : feu, givre, foudre, acide, chaînes, marque, soin, bouclier, hâte, temps, impact.

## Synergie d'équipe (remplace les combos)
Rien à déclencher : tout est actif dès le début du combat.
1. **Bonus passif de familles** : chaque paire de familles dans l'escouade donne à toute l'équipe un bonus permanent de statistique (PV, ATQ, DEF ou VIT), **plafonné à 30 %** par statistique. Dix synergies : six croisées (Matière + Élémentaire : ATQ +6 %, Matière + Physique/Bio : DEF +8 %, Matière + Perception : VIT +5 %, Élémentaire + Physique/Bio : PV +6 %, Élémentaire + Perception : ATQ +8 %, Physique/Bio + Perception : PV +8 %) et quatre résonances d'une même famille (+4 %).
2. **Lien entre deux agents** : chaque victoire remportée ensemble le fait progresser. Niveaux 2 à 5 à **3, 8, 15 et 25 victoires** communes. Chaque niveau ajoute **25 %** à l'effet passif de la synergie de leurs deux familles (100, 125, 150, 175, 200 %), sous le plafond de 30 %.
3. **Effets de composition** (se cumulent) :

| Effet | Condition | Résultat |
|---|---|---|
| Quatuor | 4 familles différentes | +10 % de chances de critique |
| Trinité | un tank, un agent de dégâts, un soutien | +1 énergie par pas |
| Double front | 2 tanks ou plus | bouclier d'ouverture de 10 % des PV |
| Étincelle | 2 agents de contrôle ou plus | +20 énergie au départ |

## IA
### Alliés (mode auto) et ennemis
L'IA ne choisit que parmi les compétences **payables** (énergie suffisante) et vise en priorité selon son rang. Le prototype applique des priorités simples ; les comportements ci-dessous sont la **cible de design** :

| Rang | Comportement |
|---|---|
| Facile | Compétences au hasard, cible au hasard |
| Normal | Utilise A2 dès qu'elle est payable, vise la cible la plus faible 50 % du temps |
| Élite | Priorités complètes (soin sous 40 % PV, compétence forte, A1 sur la cible la plus faible), vise les soigneurs en premier |
| Boss | Élite + phases : nouvelle compétence à 50 % PV |

### Niveau et puissance des ennemis
- Niveau = niveau moyen de l'équipe + écart du rang (Facile −3, Normal 0, Élite +3).
- Stats × 0,8 (Facile), × 1 (Normal), × 1,3 (Élite). Boss : PV × 3.

## Fatigue
| Événement | Fatigue |
|---|---|
| Combat gagné | +12 % pour chaque membre engagé |
| Combat perdu ou retraite | +27 % (12 + 15) |
| Au-delà de 60 % | Malus de stats progressif, jusqu'à −30 % à 100 % |
| 100 % | Le membre ne peut plus combattre |
| Toute l'équipe à 100 % | Retour immédiat au QG |

## Récompenses
| Rang | Crédits | XP par membre |
|---|---|---|
| Facile | 200 | 40 |
| Normal | 300 | 60 |
| Élite | 500 | 100 |
| Boss (mini-climax) | 1 000 | 200 |

L'XP de combat s'ajoute à l'XP d'entraînement du salon. Les victoires font aussi progresser les **liens** entre les agents engagés.

## Interface (disposition 0.10.0)
- **Frise pleine largeur** sous la barre du haut (Retraite, titre, Auto, vitesse, pause).
- **Deux camps** de part et d'autre d'un **terrain central** : 2×2 cartes décalées, chaque carte avec portrait, PV et **barre d'énergie** (bleue).
- **Terrain central** : le nom de la compétence jouée s'y affiche, avec le projectile et l'éclat de sa famille.
- **Éventail du bas** : 3 cartes pour l'agent dont c'est le tour (A1, A2, carte du grimoire), pastille de **coût en énergie** (rouge si l'énergie manque) ; appui prolongé = description ; glisser-déposer sur une cible.
- **Journal** en bas à droite : synergies, effets d'équipe et actions.
- Détail des écrans : `03_Art_UI_UX/01_UX_Ecrans.md`.

## Bestiaire de New York (acte 1 : Brooklyn)
| Ennemi | Rôle | Compétences (prototype) |
|---|---|---|
| Voyou | Dégâts | Coup 100 % ; coup lourd 160 % |
| Gros bras | Tank | Coup 90 % ; Provocation |
| Pyro de rue | Dégâts | Jet de flammes 90 % + Brûlure ; zone 70 % |
| Brick (boss, zone 1.1) | Tank | Coups lourds, Provocation, choc de zone |
| Crocodile (boss, zone 1.3) | Mafieux méta-humain | voir `Code/Heroes Dawn Unity/Assets/_Project/Content/Prototype/PrototypeZones.cs` |

Le bestiaire des actes suivants (Paris, Moscou, Salem) reste à écrire avec les zones.

## Données / paramètres
Énergie (max, départ, régénération), pas de la frise, points de reprise, zone de préparation, reculs, pause de résolution, Brûlure, critique, fatigue, écarts de rang : `BalanceConfig` (code) et onglet **Paramètres** du tableur.
