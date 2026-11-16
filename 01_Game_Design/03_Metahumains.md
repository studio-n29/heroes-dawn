# Méta-humains : familles, positions, rangs AMIF, synergies, progression

> Résumé dans le GDD : `00_GDD/GDD_Heroes_Dawn.md` §7 · Statut : **réaligné sur la version 0.10.1 (2026-10-23)**
> ⚑ = valeur proposée à l'origine ; **toutes validées le 2026-11-16** (voir `05_Production/Decisions.md`).
> Kits : `06_Kits_Competences.md` · Fiches de lore : `02_Lore_Narration/04_Agents_The_Dawn.md` · Synergies chiffrées : `02_Combat.md` · Stats : tableur, onglet Stats
> L'organisation du joueur s'appelle **The Dawn** (l'Aube) ; l'ancienne « Guilde » et son roster de 35 recrues de New York sont archivés dans `02_Lore_Narration/Archive_v1/`.

## Objectif du système
Donner au joueur une collection qu'il **construit** (pas de hasard sur les agents) et des équipes dont la force vient des **synergies** entre familles et des **liens** entre agents, pour porter les piliers « Mon équipe, mes choix » et « Vivre l'Apex Universe ».

## Le personnage joueur
Au début du jeu, le joueur choisit **Agent D** (New York) ou **Agent M** (Moscou). Ce choix décide de son personnage dans l'équipe, de son équipe de départ et de l'**ordre des quatre actes** (D : New York, Paris, Moscou, Salem ; M : Moscou, Paris, New York, Salem). Un seul des deux existe dans une partie. **Agent M est affiché « en création » tant que Moscou n'a pas de zones jouables.**

| | Agent D | Agent M |
|---|---|---|
| Pouvoir | Matérialise ce que son esprit imagine | Perçoit le passé, le présent et le futur de ce qui l'entoure |
| Famille, position, rang | Matière, Contrôle, rang 5 | Perception, Précision à distance, rang 5 |
| Compagnons de départ | Darius Brooks, Mei-Lin Chen, Freya Lindqvist | Sam Okafor, Céleste Moreau, Haruto Nakamura |

## Anatomie d'un agent
| Élément | Détail |
|---|---|
| Identité | Nom, pouvoir, fiche de lore (`04_Agents_The_Dawn.md`) |
| Famille | Matière, Élémentaire, Physique/Bio ou Perception |
| Position | Première ligne, Attaquant, Contrôle, Soutien ou Précision à distance (fixe les stats de base et les types de cartes boostés) |
| Rang AMIF | 1 à 5 (multiplicateur de stats ×1 / ×1,15 / ×1,30 / ×1,45 / ×1,65) ; rangs 4 et 5 : par arc ou par prouesse, jamais en boutique |
| Kit | **2 compétences de base** propres à l agent + **3 cartes équipables** du grimoire (5 places en combat) |
| Niveau | 1 à 50 |
| Fatigue | 0-100 % |
| Liens | Niveau de lien (1 à 5) avec chaque autre agent, gagné en combattant ensemble |

## Le roster : 30 agents numérotés + 2 personnages joueurs + 10 cryptides
| Zone | Agents | Remarque |
|---|---|---|
| New York | 3, 4, 8, 10, 12, 15, 23, 24, 27, 30 | Agents de départ de la route D |
| Paris | 2, 6, 11, 13, 17, 18, 21, 26, 28, 29 | |
| Moscou | 1, 5, 7, 9, 14, 16, 19, 20, 22, 25 | Agents de départ de la route M |
| Salem | S1 à S10 (cryptides) | Créés dans les années 80, soignés par Brie Rayner ; **pas encore dans le jeu** (acte IV) |

Répartition des 30 agents numérotés par rang AMIF : **rang 5 : 1 · rang 4 : 5 · rang 3 : 6 · rang 2 : 8 · rang 1 : 10** (avec Agent D et Agent M au rang 5 : 3 · 5 · 6 · 8 · 10 pour les 32 agents du jeu ; avec les 10 cryptides : 3 · 6 · 10 · 13 · 10). Par famille : Matière 7, Élémentaire 6, Physique/Bio 8, Perception 9 (déséquilibre à revoir ⚑). Familles, rôles et obtention de chaque agent : `Code/docs/specs/2026-10-22-agents-the-dawn-design.md`.

## Familles
La famille est la **catégorie de l'agent** (fondée sur la nature de son pouvoir) et le **type de pouvoir** des cartes du grimoire : la famille ne limite plus l équipement : une carte de la famille de l agent reçoit +5 % d efficacité.

| Famille | Rôle dominant | Couleur UI | Profils du lore |
|---|---|---|---|
| Matière | Contrôle, création | Ambre | Animation de matière, magnétisme, alchimie, transposition, poche dimensionnelle, gravité |
| Élémentaire | Dégâts, altérations | Cyan | Feu, eau, gaz, glace, son, ondes électromagnétiques |
| Physique / Bio | Tank, soutien | Rouge | Métamorphose, énergie cinétique, sang, ADN, intangibilité, vampirisme |
| Perception | Précision, contrôle, détection | Violet | Télépathie, illusions, sens décuplés, invisibilité, temps, contrôle mental, téléportation |

## Obtention des agents
| Voie | Règle | Agents |
|---|---|---|
| Départ | Dans l'équipe dès la première connexion | Le personnage joueur + 3 compagnons |
| Boutique | 3 agents par jour à 9 h (tirage pondéré par rang 45 / 25 / 20 / 8 / 2 ⚑ — seuls les agents de boutique y entrent), coût en PR : **rang 1 150 / 2 : 220 / 3 : 450 / 4 : 800 / 5 : 1 200** ⚑, renouvellement gratuit puis payant | 14 agents (dont Amara Hughes, après la zone 1.1) |
| Arc narratif | Rejoint l'équipe à la fin d'une zone | 4 agents |
| Événement de week-end | Rejoint l'équipe si la zone est validée un samedi ou un dimanche | 4 agents |
| Prouesse | Valider une zone avec un seul agent, ou sans aucune défaite | 4 agents |

Les zones de ces règles sont celles de Brooklyn (1.1 à 1.3) en attendant les zones de Paris, Moscou et Salem.

## Synergies
Détail chiffré dans `02_Combat.md` (section « Synergie d'équipe ») :
1. **Bonus passif de familles** : dix synergies (six croisées, quatre résonances), cumul plafonné à 30 %.
2. **Liens** : +25 % d'effet passif par niveau de lien (niveaux 2 à 5 à 3, 8, 15, 25 victoires communes), et des **bonus propres à chaque paire** selon le couple de rôles (voir « Bonus de paire des liens »).
3. **Effets de composition** : Quatuor (4 familles), Trinité (tank, dégâts, soutien), Double front (2 tanks), Étincelle (2 contrôles).

### Bonus de paire des liens
En plus de l'amplification des synergies, chaque paire d'agents débloque ses propres bonus aux niveaux de lien 2 à 5, selon le **couple de rôles** des deux agents (15 couples). Ils s'appliquent **aux deux agents de la paire**, seulement quand ils sont dans la même escouade, et se cumulent entre paires dans les plafonds suivants (par agent) : +12 % par statistique, +15 points de critique, bouclier d'ouverture 20 % des PV, jauge de départ +20 %, énergie de départ +20, énergie par pas +1. Chaque niveau **ajoute** un bonus à ceux des niveaux précédents ; au niveau 1, aucun bonus. Mécaniques existantes seules (statistiques, critique, bouclier d'ouverture, jauge de départ, énergie). Valeurs de départ, à ajuster au playtest. Spec : `Code/docs/specs/2026-10-27-liens-bonus-de-paire-design.md`.

| Couple de positions (Tank = Première ligne, Dégâts = Attaquant, Précision = Précision à distance) | Thème | Niv. 2 | Niv. 3 | Niv. 4 | Niv. 5 |
|---|---|---|---|---|---|
| Tank + Tank | Double rempart | DEF +4 % | PV +4 % | Bouclier 8 % | Bouclier +7 % |
| Tank + Dégâts | Rempart et lame | PV +3 % | ATQ +3 % | Énergie de départ +10 | Critique +5 % |
| Tank + Contrôle | Étau | DEF +3 % | VIT +3 % | Jauge de départ +10 % | Énergie de départ +10 |
| Tank + Soutien | Veilleurs | PV +4 % | DEF +3 % | Bouclier 8 % | Énergie +1 par pas |
| Tank + Précision | Garde et regard | DEF +3 % | Critique +4 % | Jauge de départ +10 % | Critique +4 % |
| Dégâts + Dégâts | Double tranchant | ATQ +4 % | ATQ +4 % | Critique +5 % | Énergie de départ +10 |
| Dégâts + Contrôle | Marteau et étau | ATQ +3 % | VIT +3 % | Énergie de départ +10 | Jauge de départ +10 % |
| Dégâts + Soutien | Lame soutenue | ATQ +3 % | PV +3 % | Énergie de départ +10 | Énergie +1 par pas |
| Dégâts + Précision | Tir croisé | ATQ +3 % | Critique +4 % | Critique +4 % | ATQ +4 % |
| Contrôle + Contrôle | Tisseurs | VIT +4 % | VIT +4 % | Jauge de départ +10 % | Énergie de départ +10 |
| Contrôle + Soutien | Sérénité | PV +3 % | VIT +3 % | Énergie de départ +10 | Énergie +1 par pas |
| Contrôle + Précision | Piège | VIT +3 % | Critique +4 % | Jauge de départ +10 % | Critique +4 % |
| Soutien + Soutien | Cercle de soin | PV +4 % | PV +4 % | Bouclier 8 % | Énergie +1 par pas |
| Soutien + Précision | Appui précis | PV +3 % | Critique +4 % | Énergie +1 par pas | Critique +4 % |
| Précision + Précision | Double regard | Critique +4 % | VIT +3 % | Critique +4 % | Jauge de départ +10 % |

## Progression
### Niveaux (1 → 50)
- XP par l'**entraînement au salon** (temps réel) et par les **combats**.
- Courbe : l'XP demandée augmente de 10 % par niveau. Environ 29 jours d'entraînement pour passer un membre de 1 à 50 (tableur, onglet Progression).
- **Plafond par acte** ⚑ : niveau max = 10 × acte le plus avancé atteint, toutes villes confondues (acte I : 10, … acte IV : 40). Évite qu'un joueur fasse monter un seul membre trop vite.

### Paliers de compétences ⚑
- Aux niveaux **10, 20, 30, 40, 50**. Un palier atteint doit être **validé en Crédits** (2 000 → 32 000, ×2 par palier). Pas encore implémenté.
- Effets génériques : voir `06_Kits_Competences.md`.

### Entraînement au salon
- Emplacements : 2 au niveau 1 du salon, jusqu'à 6 (voir QG).
- Les **objets placés** donnent un bonus d'XP (+25 % en moyenne) ; ils s'achètent en Crédits et se gagnent dans les coffres.
- Un membre à l'entraînement **récupère aussi** sa fatigue à vitesse normale.

### Fatigue hors combat
- Récupération naturelle : **5 % par heure** (20 h pour 100 %).
- Infirmerie : ×2 (jusqu'à ×3 au niveau 10), emplacements limités.
- Objets : trousse de soin (−25 %), accélérateur premium.

## Données / paramètres
Multiplicateurs de rang, courbe d XP, coûts des paliers, fatigue : tableur (Paramètres, Progression, Stats). Agents et compétences : outils de design (`Code/docs/Guide_Outils_de_design.md`).

## Rangs AMIF, positions et bonus de cartes (bloc A de la 1.0.0)

Spec : `Code/docs/specs/2026-11-10-rangs-positions-cartes-design.md`.

| Rang | Libellé | Stats |
|---|---|---|
| 5 | Capacités extraordinaires pouvant représenter une menace planétaire | ×1,65 |
| 4 | Capacités majeures pouvant représenter une menace à grande échelle | ×1,45 |
| 3 | Capacités significatives pouvant représenter une menace sérieuse | ×1,30 |
| 2 | Capacité notable pouvant représenter une menace ponctuelle | ×1,15 |
| 1 | Capacités secondaires ne représentant pas une menace directe | ×1 |

Eda et Brie Rayner sont rang 5 mais non recrutables (PNJ, allié d'arc ou boss).

| Position (ancien rôle) | Types de cartes boostés |
|---|---|
| Première ligne (Tank) | Défensif |
| Attaquant (Dégâts) | Offensif |
| Contrôle (Contrôle) | Contrôle |
| Soutien (Soutien) | Soutien |
| Précision à distance (Précision) | Offensif, Contrôle |

**Bonus des cartes équipées** : +15 % d'efficacité si le type de la carte est boosté par la position de l'agent, +5 % si la carte est de sa famille ; les deux se cumulent (jusqu'à +20 %). Les ennemis n'en reçoivent jamais.

**Table des rangs** (validée le 2026-11-16) : rang 5 : Agent D, Agent M, Victor Sorel · rang 4 : Odile Ferrand, Ada Okonkwo, Eero Nieminen, Astrid Hallström, Louis Dubois · rang 3 : Yelena Ivanova, Marco Bellini, Zofia Kowalczyk, Mateo Vasquez, Clément Lefèvre, Youssef Haddad · rang 2 : Ilona Varga, Haruto Nakamura, Noah Kaplan, Céleste Moreau, Rin Takeda, Giulia Rossi, Kwame Abara, Mei-Lin Chen · rang 1 : Darius Brooks, Freya Lindqvist, Lucas Mendes, Amara Hughes, Fatou Diallo, Séverine Morel, Sam Okafor, Tunde Oyelaran, Ji-Hoon Park, Anna Petrov. Cryptides de Salem (provisoire) : S5 rang 4 ; S1, S3, S6, S9 rang 3 ; S2, S4, S7, S8, S10 rang 2.
