# Combat en temps actif (ATB)

> Résumé dans le GDD : `00_GDD/GDD_Heroes_Dawn.md` §6 · Statut : **v1 complète**
> Kits des recrues : `06_Kits_Competences.md` · Valeurs : `05_Equilibrage.xlsx` (onglets Combat, Stats)
> ⚑ = recommandation qui touche le feeling, à confirmer en prototype.

## Objectif du système
Des combats **courts (60-90 s)**, lisibles sur mobile, où le joueur crée de la valeur par ses **choix d'équipe et de combos** (pilier « Mon équipe, mes choix »). Le mode auto permet de rejouer vite ; le mode manuel récompense la maîtrise.

## Déroulement
1. Arrivée sur la case → écran de combat (4 alliés à gauche, 1 à 5 ennemis à droite).
2. Les jauges ATB se remplissent en temps réel. Au départ, chaque jauge est remplie aléatoirement entre 0 et 20 %.
3. Quand la jauge d'un personnage est pleine, il peut agir :
   - **Manuel** : son portrait s'allume ; le joueur choisit une compétence puis une cible. Pendant ce temps, **les autres jauges continuent** de se remplir (temps actif). ⚑
   - **Auto** : l'IA alliée choisit immédiatement.
4. Après l'action, la jauge repart de 0.
5. Fin : **victoire** quand tous les ennemis sont à 0 PV ; **défaite** quand tous les alliés sont à 0 PV. Pas de limite de temps.
6. Bouton **Retraite** : compte comme une défaite (recul d'une case + fatigue de défaite).

## Statistiques
| Stat | Rôle |
|---|---|
| PV | Points de vie |
| ATQ | Base des dégâts et de certains soins |
| DEF | Réduit les dégâts reçus |
| VIT | Vitesse de remplissage de la jauge ATB |
| Précision | 100 % de base ; réduite par Aveuglement |
| Critique | 15 % de base, ×1,5 dégâts |

Valeurs de base par **rôle** (Tank, Dégâts, Contrôle, Soin/Soutien, Précision) et par rareté : onglet **Stats** du tableur.

## Formules
- **Jauge ATB** : 1 000 points ; remplissage = VIT × 4 par seconde (un personnage à VIT 100 agit toutes les 2,5 s).
- **Dégâts** = ATQ × % de la compétence × (1 − DEF / (DEF + K)) × multiplicateurs (critique, Vulnérabilité, bonus).
- **K** = 300 × (1,04)^(niveau de l'attaquant − 1), pour que la réduction reste stable à tous les niveaux.
- **Variation** : ±5 % aléatoire par coup.
- **Soins** : en % des PV max de la cible ou de l'ATQ du soigneur (précisé dans chaque kit).

## Compétences
- **A1** sans recharge ; **A2** et **A3** avec recharge en **tours du personnage** (précisée dans chaque kit).
- Les altérations et leur durée sont définies dans le glossaire de `06_Kits_Competences.md`. Une même altération ne se cumule pas : une nouvelle application rafraîchit la durée.

## Combos
- Condition : **2 alliés d'une paire valide** (combo de famille, Résonance ou duo légendaire) ont leur jauge pleine en même temps.
- Le bouton **Combo** s'allume et affiche le nom du combo. En manuel, le joueur décide de l'utiliser ou non ; en auto, l'IA l'utilise dès qu'il est disponible.
- Effet : les deux personnages agissent ensemble (animation commune) et **consomment leur tour**.
- Puissance : environ **2,5 fois une A1** en valeur totale (dégâts + effet). Recharge : **1 utilisation par paire toutes les 3 actions** de chacun des deux membres.
- Les duos légendaires remplacent le combo de leur paire de familles (ou la Résonance) et valent environ **3,5 fois une A1**.

## IA
### Alliés (mode auto)
Priorités, dans l'ordre : combo disponible → soin si un allié est sous 40 % PV → A3 disponible → A2 disponible si son effet est utile (pas d'altération déjà présente) → A1 sur la cible la plus faible.

### Ennemis selon le rang
| Rang | Comportement |
|---|---|
| Facile | Compétences au hasard, cible au hasard |
| Normal | Utilise A2 dès qu'elle est prête, vise la cible la plus faible 50 % du temps |
| Élite | Priorités complètes (comme l'IA alliée), vise les soigneurs en premier |
| Boss | Élite + phases : nouvelle compétence à 50 % PV |

### Niveau et puissance des ennemis
- Niveau = niveau moyen de l'équipe + écart du rang (Facile −3, Normal 0, Élite +3 ; mode difficile +5).
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
| Boss (mini-climax) | 1 000 | 200 + Légende de l'acte ajoutée à la pool |

L'XP de combat s'ajoute à l'XP d'entraînement du salon.

## Interface (rappel)
Portraits en bas avec jauges ATB et fatigue ; compétences au toucher ; boutons Combo, Auto, vitesse ×1/×2, Retraite. Détail : `03_Art_UI_UX/01_UX_Ecrans.md`.

## Bestiaire de New York
| Ennemi | Acte | Rôle | A1 | Spécial (R4) | Passive |
|---|---|---|---|---|---|
| Voyou néo-méta | 1 | Dégâts | Coup 100 % | Frénésie : +30 % ATQ 2 tours | — |
| Gros bras | 1 | Tank | Coup 90 % | Provocation | −15 % dégâts subis |
| Pyro de rue | 1 | Dégâts | Jet de flammes 90 %, Brûlure 30 % | Cocktail : 70 % à tous | — |
| Recruteur du Syndicat | 2 | Contrôle | Filet 80 %, Entrave 30 % | Collier : Entrave 2 tours | — |
| Chimiste | 2 | Soutien | Fiole 80 %, Corrosion 30 % | Stimulant : soigne un allié de 25 % | — |
| Exécuteur | 2 | Dégâts | Frappe 110 % | Exécution : 200 % sur une cible sous 30 % PV | — |
| Soldat AMIF | 3 | Précision | Tir 100 % | Rafale : 3 × 50 % | +15 % critique |
| Drone AMIF | 3 | Contrôle (mécanique) | Taser 70 %, Choc 20 % | Scan : Marque sur 2 alliés | Mécanique |
| Blindé AMIF | 3 | Tank | Crosse 90 % | Inhibiteur : Entrave sur 1 allié 2 tours | Immunisé au Choc |
| Fidèle de la Brèche | 4 | Dégâts | Flamme rouge 100 %, Brûlure 30 % | Prière : +20 % ATB aux alliés | — |
| Voilé | 4 | Précision | Lame 100 % | Disparition : Esquive | 20 % d'Esquive |
| Possédé | 4 | Tank | Griffe 100 % | Régénération sur soi | Revient une fois à 20 % PV |
| Porte-flamme | 5 | Élite polyvalent | Flamme 120 %, Brûlure 50 % | Colonne de feu : 90 % à tous | Immunisé à la Brûlure |

### Boss (mini-climax)
| Boss | Acte | Phase 1 | Phase 2 (sous 50 % PV) |
|---|---|---|---|
| Brick | 1 | Coups lourds, Provocation | Enrage : +40 % ATQ, charge sur le soigneur |
| Collier | 2 | Entrave sur 2 alliés | Colliers : un allié par tour ne peut plus utiliser A2/A3 jusqu'à la fin, sauf s'il est retiré (Antidote, Triage) |
| Capitaine Mercer | 3 | Tirs de précision + 2 drones | Appelle 2 drones de plus ; grenade inhibitrice (Entrave de zone) |
| Mère Voile | 4 | Invisible un tour sur deux | Crée 2 illusions (Voilés) ; révélée par Écho, Radar, Thermo |
| Le Héraut | 5 | Flammes rouge cendre, Brûlure permanente de zone | « Brèche » : à 25 % PV, invoque un Possédé par tour ; Givre et Rempart contrent la Brûlure |

## Données / paramètres
Tous les paramètres chiffrés (jauge, facteur ATB, K, critique, écarts de rang, fatigue) sont dans l'onglet **Paramètres** du tableur et exportés vers Remote Config.

## Rythme et prise en main (version 0.8.0)
- **Pas de la frise** : 0,35 s ; vitesse ×2 par défaut (×1 et ×3 disponibles) ; délai de réflexion de l'IA : 0,3 à 0,7 s ; les pauses suivent la vitesse choisie.
- **Fenêtre d'interruption** : 1,2 s, seulement au pas qui précède l'arrivée d'un ennemi au point d'action et si une compétence d'avatar est prête ; jamais en Auto.
- **Cartes de compétence** : appui prolongé = description de l'effet ; glisser-déposer : dans le vide pour une compétence générale ou sur soi, sur une carte cible pour une compétence ciblée (le toucher simple reste possible).
- **Animations** : ouverture de l'éventail au tour de l'agent, carte jouée qui s'envole vers sa cible, élan de l'attaquant, étincelle d'arrivée sur la frise ; coupées par « réduire les mouvements ».

## Déplacement sur le plateau (version 0.8.0)
Le pion suit tout son trajet : seules les cases de combat non résolues l'arrêtent. Le brouillard ne bloque plus le déplacement, toute case à portée est une destination et le brouillard se lève au passage du pion.
