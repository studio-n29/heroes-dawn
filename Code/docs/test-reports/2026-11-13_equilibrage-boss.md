# Équilibrage : boss et courbe de difficulté (2026-11-13)

Phase 4 du plan de stabilisation. Outils : `BalanceFullReport.Run` (rapport complet) et `BalanceFullReport.BossTuning` (grille de réglage), dans `Assets/_Project/Editor/Balance/`. Contenu construit comme dans le jeu (zones et agents des outils de design, `balance.json`). **Aucune valeur du jeu n'est modifiée par ce rapport.**

## Méthode et limites
- Simulation de combat en mode auto, **200 combats par rencontre**, graine fixe. Niveau ennemi = niveau de l'équipe + écart du rang (règle de `CombatService`).
- Deux équipes : **départ** (Giulia Rossi, Ilona Varga, Lucas Mendes : 3 agents) et **essai** (Darius Brooks, Mei Lin Chen, Freya Lindqvist, Kwame Abara : 4 agents). Niveaux 3 à 15 pour le rapport, 5 et 10 pour la grille.
- **Non simulés : cartes équipées, objets, composants.** Les taux sont ceux d'équipes « nues » : un plancher. Le taux réel d'un joueur équipé sera plus élevé.
- La précision à 200 combats est d'environ ±7 points ; ne pas lire un écart de moins de 10 points comme significatif.

## 1. État actuel : les boss sont trop faciles (cible 40 à 60 %)

| Rencontre | Zone | Équipe de départ (niv. 5 / 10) | Équipe d'essai |
|---|---|---|---|
| Brick (élite à phases) | 1.1 | 100 % / 100 % | 100 % |
| Crocodile (boss) | 1.3 | 93 % / 96 % | 100 % |
| Titan du pont (boss) | 1.4 | 100 % / 100 % | 100 % |

Le niveau de l'équipe change peu le résultat (3 à 15 : de 92 à 94 % de victoires en moyenne) : l'ennemi monte avec l'équipe, la difficulté vient de la **composition** des rencontres, pas du niveau.

Autre écart : l'**élite AMIF** de la zone 1.4 (capitaine, soldat, tireur) est à **13 % (niv. 5) et 31 % (niv. 10)** pour l'équipe de départ, soit bien plus dure que le boss de la même zone. La même rencontre dans le repaire de l'AMIF donne 15 % et 29 %. À arbitrer (voir §3).

## 2. Ce qui rend un boss dangereux : son attaque, pas ses PV
Multiplier les PV d'un boss par 2, 3, 4 ou 6 **allonge** le combat (Brick : 19 s → 97 s) mais ne change **presque pas** le taux de victoire (100 % partout). Les PV ne sont donc pas le bon réglage. L'attaque, elle, joue fortement.

Taux de victoire selon le multiplicateur d'attaque du boss (PV ×1, niveau 5 / niveau 10) :

| Attaque × | Brick, départ | Brick, essai | Crocodile, départ | Crocodile, essai | Titan, départ | Titan, essai |
|---|---|---|---|---|---|---|
| 1 | 100 / 100 | 100 / 100 | 92 / 96 | 100 / 100 | 100 / 100 | 100 / 100 |
| 2 | 100 / 100 | 100 / 100 | **48 / 59** | 100 / 100 | 98 / 99 | 98 / 100 |
| 3 | 100 / 100 | 100 / 100 | 22 / 28 | 96 / 95 | 87 / 97 | 67 / 71 |
| 4 | 100 / 100 | 99 / 100 | 11 / 18 | 79 / 87 | **58 / 76** | 23 / 26 |
| 6 | 95 / 97 | 72 / 79 | 3 / 8 | 22 / 30 | 23 / 40 | 3 / 2 |
| 8 | 78 / 88 | **30 / 52** | 1 / 5 | 7 / 9 | 10 / 21 | 0 / 2 |
| 12 | **54 / 64** | 2 / 4 | 0 / 1 | 1 / 3 | 5 / 13 | 0 / 0 |

Les valeurs en gras sont dans la cible de 40 à 60 %.

## 3. Constat et décision à prendre
L'équipe de départ (3 agents) et l'équipe d'essai (4 agents) n'ont **pas** la même force : le multiplicateur qui met un boss à 50 % pour l'une l'écrase pour l'autre (Crocodile : ×2 pour 3 agents, environ ×5 pour 4 agents). Il faut donc **choisir l'équipe de référence de chaque boss** avant de régler, sans quoi un réglage est faux pour l'une des deux :

| Boss | Où | Équipe de référence plausible | Multiplicateur d'attaque indicatif |
|---|---|---|---|
| Brick | Zone 1.1, début de campagne | départ (3 agents) | environ ×12 (54 à 64 %) ; trop haut pour une première élite, à comparer avec ×8 (78 à 88 %) |
| Crocodile | Zone 1.3 | départ ou essai | ×2 (départ) ou ×5 (essai) |
| Titan du pont | Zone 1.4 | essai | environ ×3,5 (23 à 71 %) ; ×4 pour le départ (58 à 76 %) |

Une proposition qui évite les extrêmes : **régler sur l'équipe de départ avec un équipement moyen supposé**, puis vérifier en playtest, car les cartes et objets (non simulés) font monter les taux. Autrement dit, mieux vaut **ne pas régler à 50 % sur l'équipe nue** : elle sous-estime le joueur. Une cible « 40 à 60 % pour l'équipe nue » donnerait des boss trop durs pour un joueur équipé.

Je ne change aucune valeur sans ton accord : ce sont des choix de conception (valeurs ⚑ du plan).

## 4. Reste à faire
- Décider l'équipe de référence par boss et le niveau d'équipement supposé.
- Simuler les cartes équipées et les objets (le simulateur ne les connaît pas) avant de figer les valeurs.
- Décider du sort de l'élite AMIF (13 à 31 %) : l'assouplir ou la garder comme pic de difficulté assumé.
- Jouer les 2 à 3 sessions de playtest avec `docs/playtest-formulaire.md`.

## 5. Équipe de départ équipée (choix du 2026-11-13 : « départ + équipement »)

Le simulateur sait équiper les agents (`AllyEntry` : objets et cartes) ; il ne le faisait pas. J'ai ajouté des profils d'équipement tirés de la **collection réelle** du jeu (14 objets, 48 cartes) : `Loadouts.cs`. Équipe : Giulia Rossi, Ilona Varga, Lucas Mendes.

### Constat important : les cartes équipées font s'effondrer les combats en mode auto
| Profil (zone 1.3, Crocodile, niveau 5, 400 combats) | Victoires |
|---|---|
| Nu | 94 % |
| Objets seuls (2 par agent, communs ou rares) | 98 % |
| Cartes seules (3 par agent, 1 à 2 étoiles) | **8 %** |
| Moyen (objets + cartes) | **10 %** |

Zone 1.4 (Titan) : cartes seules 2 %, et avec 1 carte par agent **214 combats sur 400 n'ont pas de fin** (délai de 20 000 ticks dépassé), 188 sur 400 avec le profil fort. Les objets, eux, améliorent légèrement le taux (jusqu'à +4 points).

Deux lectures possibles, **à vérifier avant toute conclusion** :
1. Défaut de la simulation : l'IA automatique des alliés joue mal les cartes (elle semble choisir des compétences de soutien ou de protection en boucle). Les cartes réelles se jouent à la main.
2. Défaut du jeu : le bouton AUTO utilise la même IA ; un joueur qui équipe des cartes et laisse AUTO jouer pourrait voir ses combats ne pas se terminer. Si c'est le cas, c'est un défaut de gameplay, pas d'équilibrage.
Je n'ai pas tranché. Tant que ce n'est pas élucidé, **les cartes sont exclues** des réglages ci-dessous (profils « objets seuls »).

### Multiplicateur d'attaque du boss, équipe de départ avec objets (PV ×1 ; niveau 5 / niveau 10)
| Attaque × | Brick (1.1) objets moyens | Brick objets forts | Crocodile (1.3) moyens | Crocodile forts | Titan (1.4) moyens | Titan forts |
|---|---|---|---|---|---|---|
| 1 | 100 / 100 | 100 / 100 | 98 / 99 | 97 / 99 | 100 / 100 | 100 / 100 |
| 2 | 100 / 100 | 100 / 100 | 58 / 73 | 77 / 85 | 100 / 100 | 100 / 100 |
| 3 | 100 / 100 | 100 / 100 | **31 / 48** | **51 / 59** | 99 / 100 | 99 / 99 |
| 4 | 100 / 100 | 100 / 100 | 18 / 26 | 25 / 35 | 92 / 97 | 90 / 97 |
| 6 | 100 / 99 | 98 / 99 | 5 / 7 | 6 / 8 | **37 / 60** | **43 / 63** |
| 8 | 92 / 93 | 87 / 96 | 2 / 3 | 2 / 2 | 19 / 31 | 22 / 35 |
| 12 | **61 / 69** | 74 / 83 | 0 / 1 | 0 / 0 | 7 / 13 | 13 / 23 |
| 16 | **56 / 63** | **48 / 74** | 0 / 0 | 0 / 0 | 4 / 13 | 3 / 6 |

(En gras : dans ou près de la cible de 40 à 60 %.)

### Valeurs proposées (à valider)
| Boss | Attaque × proposée | Taux attendu (équipe de départ + objets) | Remarque |
|---|---|---|---|
| Crocodile | **×3** | 31 à 59 % | cible atteinte avec objets moyens ou forts |
| Titan du pont | **×6** | 37 à 63 % | idem |
| Brick | ×12 à ×16 | 48 à 74 % | **valeur extrême** : le réglage par l'attaque seule ne convient pas à Brick. Il faut revoir sa conception (puissance de ses compétences, phases, escorte) plutôt qu'un multiplicateur |

Les multiplicateurs de ces trois boss sont très différents (×3, ×6, ×12) alors que leurs attaques de base sont voisines (110 à 120) : les kits (taunt, boucliers, phases) comptent autant que l'attaque. Un multiplicateur unique serait faux.

### Ce que je ne fais pas sans ton accord
Modifier le contenu des boss (`PrototypeContent.cs`, `PrototypeZones.cs`) : ce sont des valeurs de conception ⚑.
