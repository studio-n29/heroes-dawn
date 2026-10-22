# Heroes Dawn — Game Design Document

> Version : 3.0 · Mise à jour : 2026-10-23 · Auteur : Djason (Studio N29) · **Réaligné sur la version 0.10.1 du jeu**
> Statut : **en production (Early Access visé le 14/11/2026, sortie le 14/12/2026)**. Le canon a changé le 2026-10-22 : l'organisation du joueur est **The Dawn**, le joueur choisit **Agent D ou Agent M**, 30 agents numérotés + 10 cryptides de Salem, 4 actes (New York, Paris, Moscou, Salem). Le roster de 35 recrues de New York et la campagne de Brooklyn en 5 plateaux sont archivés (`02_Lore_Narration/Archive_v1/`).

## 1. Pitch
Jeu mobile de gestion-RPG dans l'**Apex Universe**, l'univers partagé de tous les projets de Studio N29 (né de l'œuvre d'origine *Apex Unit*). Le joueur dirige un agent de **The Dawn (l'Aube)**, l'organisation qui protège les méta-humains et les civils, recrute et entraîne des méta-humains — que tout le monde surnomme **« les Apex »**, en référence à l'Apex Unit d'origine — et les engage sur des plateaux de jeu où il affronte combats et mini-jeux.

## 1bis. Vision
### Public cible
- **Fans de super-héros, profil casual+** : attirés par l'univers et les personnages, profondeur tactique modérée.
### Sessions
- **5-10 minutes** par session.
- Les **zones sont reprenables** : une zone dure 15-30 min au total mais la progression est sauvegardée à chaque case ; une session = quelques lancers de dé, un combat, un passage au QG.
### Points forts (USP)
1. **Plateau + combat** : jeu de plateau (dé, brouillard de guerre, choix de chemins) couplé à un combat en équipe sur frise (énergie, synergies et liens entre agents) — rare sur mobile.
2. **Juste, sans pay-to-win** : recrutement au choix, pas de gacha, l'argent ne fait gagner que du temps.
### Piliers de design
Chaque fonctionnalité doit servir au moins un pilier.
1. **Chaque lancer compte** — le plateau offre toujours un choix intéressant (chemin, dé bonus, brouillard).
2. **Mon équipe, mes choix** — le joueur construit son équipe et ses synergies, il ne subit pas le hasard.
3. **Vivre l'Apex Universe** — chaque ville raconte une histoire ; personnages canon et lore post-Supernova.
### Références
| Jeu | Ce qu'on retient |
|---|---|
| Summoners War | Combat à compétences, mode auto/manuel, collection |
| Marvel Strike Force | Super-héros, équipes, synergies |
| Mario Party / Dicey Dungeons | Plateau et dé avec choix tactiques |

## 2. Fiche technique
| | |
|---|---|
| Plateformes | Android, iOS, PC (Steam) — mobile-first |
| Moteur | Unity 6.6 |
| Backend | Unity Gaming Services — connexion requise |
| Langues | FR, EN, ES, PT-BR, DE |
| Rendu | Majoritairement 2D |
| Orientation | Paysage |
| Mode | PvE ; PvP futur envisagé en arène asynchrone |
| Relation à Apex Unit | Spin-off autonome de l'Apex Universe, pas de cross-progression |
| Genre | Gestion d'équipe + RPG + jeu de plateau + combat sur frise (temps actif, énergie) |

## 2bis. Périmètre de contenu (réaligné 2026-10-23)
| Élément | Quantité | Note |
|---|---|---|
| Personnages joueurs | 2 (Agent D, Agent M) | Le joueur en choisit un à la première connexion ; **Agent M indisponible tant que Moscou n'a pas de zones** |
| Actes | 4 : New York, Paris, Moscou, Salem | Ordre selon le personnage (D : NY, Paris, Moscou, Salem ; M : Moscou, Paris, NY, Salem) |
| Zones jouables | 4 (Brooklyn : 1.1 à 1.4) | Paris, Moscou et Salem restent à créer avec l'outil de level design |
| Cases par zone | ~25 | ~5 combats, 2 mini-jeux, 2 événements par passage |
| Objectifs par zone | 3 | |
| Agents | 30 numérotés (18 Agents / 8 Élites / 4 Légendes) + 2 personnages joueurs | 10 cryptides de Salem prévus pour l'acte IV |
| Types d'ennemis | 4 (voyou, gros bras, pyro de rue + boss) pour Brooklyn | Le reste avec les zones suivantes |
| Mini-jeux | 4 | 1 par format |
| Synergies | 10 bonus de familles + liens entre agents + 4 effets de composition | Remplace les combos |
| Pièces du QG | 5 | |

### Stratégie de production (équipe de 1-2 personnes)
- **Rig 2D partagé** : squelette d'animation commun, seuls les visuels changent par personnage.
- **Assets** : majoritairement des **portraits**, quelques **modèles 3D low poly** ; **génération assistée par IA**.
- **Lancement en 2 vagues** pour les recrues.
- Voir `05_Production/Risques.md`.

## 3. Univers & contexte
- Se déroule **cinq ans après la Supernova de Paris**. Tous les méta-humains ont subi une modification génétique cette nuit-là ; leur pouvoir est resté latent et ne s'est activé que lors d'un choc psychologique brutal. Dans le monde, on les appelle **« les Apex »**.
- **The Dawn (l'Aube)**, fondée par **Brie Rayner**, aide les méta-humains et les civils ; symbole : étoile dorée à quatre branches. Les membres de l'**Apex Unit** d'origine (Adam West, John Guerin, Erik Bronson) sont des **mentors PNJ**.
- **Forces en présence** : l'**AMIF** (étudie et prélève les capacités et les résidus de la Supernova), les **gangs de New York**, les **cultes de Mephisto et d'Eda** à Paris, les **oligarques et la mafia de Moscou**, les **Accords de Salem** (traité visant à contrôler les méta-humains).
- **Le joueur choisit Agent D** (illustrateur de Harlem : ce qu'il imagine devient réel) **ou Agent M** (restauratrice de fresques à Moscou : elle voit le passé, le présent et le futur). Ce choix fixe l'ordre des quatre actes ; Salem conclut toujours le parcours.
- **Actes** : I New York (ou Moscou), II Paris, III Moscou (ou New York), IV Salem (complexe N29 où Rayner soigne les expériences ratées des années 80).
- **Structure d'un acte** : une ville, plusieurs zones débloquées progressivement ; chaque zone se valide en remplissant 3 objectifs.
- **Après le dénouement** : mode difficile et endgame (expéditions + objectifs secondaires).
- Lore détaillé : `02_Lore_Narration/` (`03_Histoire_et_Actes.md`, `04_Agents_The_Dawn.md`, `05_Cinematique_Lancement.md`) ; sources originales dans `Sources/`.

## 4. Boucle de jeu
1. Explorer le plateau d'une zone (dé → déplacement → cases).
2. Résoudre les cases : combats, mini-jeux, objectifs.
3. Gagner ressources, dont Points de Recherche (PR).
4. Recruter / entraîner / gérer la fatigue au QG.
5. Repartir sur des zones plus difficiles.
6. Fin de ville (acte 5) → nouvelle ville, mode difficile, farm endgame.
7. **Après les 5 villes** : mode difficile de toutes les villes, événement de chaque saison, **Crise mondiale** (chaque semaine, des modificateurs sur les zones terminées : ennemis renforcés, récompenses doublées), complétion des Archives et des succès, puis l'arène asynchrone (PvP) quand elle sortira.

## 5. Plateau (gameplay actif)
### Structure
- Chaque ville contient **5 plateaux de 3 zones** chacun.
- Plateau = graphe : chemins menant à des cases uniques.
### Déplacement
- Jet de dé → **Points de Mouvement (PM)**.
- Après le lancer, tous les chemins accessibles avec les PM obtenus sont affichés.
- PM non utilisés → stockés dans une **jauge** qui donne un **dé supplémentaire**.
- Dé bonus utilisable à tout moment via un bouton : le joueur s'arrête, lance, ajoute les PM, choisit sa destination. Il peut **révéler de nouveaux chemins**, pas seulement allonger les existants.
### Brouillard de guerre
- Dévoilé selon la position du joueur.
- En arrivant sur une case qui ouvre du brouillard, le joueur **s'arrête** pour continuer ou changer de chemin.
- Se **referme** quand le joueur quitte la zone ; à la réouverture, le contenu de chaque case est **retiré aléatoirement dans une pool propre à la case**.
- Les chemins restant similaires, le joueur peut mémoriser un itinéraire sur la partie dévoilée.
### Cases
- **Combat** : arrêt obligatoire. **Rang de difficulté fixe** défini en amont (facile / normal / élite…), indépendant du chemin.
- **Mise à niveau des ennemis** : leur niveau s'aligne sur celui de l'équipe ; le rang de la case ajoute un écart constant (l'équipe étant transférable entre villes).
### Mode difficile
- Chaque **zone validée** débloque immédiatement sa version difficile (ennemis plus forts, meilleures récompenses).
- **Mini-jeu** : épreuve courte (détail `01_Game_Design/08_Mini_Jeux.md`), 4 formats :
  - **Test de pouvoir** : lié à l'archétype du membre choisi (viser en TK, esquiver en Physique…).
  - **Puzzle** : piratage, déminage, circuits.
  - **QTE / réflexe** : actions rapides chronométrées.
  - **Choix de membre** : on envoie le membre le plus adapté, réussite selon ses stats.
- **Événement narratif** : choix scénarisé avec conséquences.
- **Ressource / Coffre** : gain direct (PR, monnaie, objets, accélérateurs).
- **Repos / Soin** : réduit la fatigue de l'équipe en mission.
### Fin de zone
- Une zone est validée quand le joueur a rempli **X objectifs**, dans n'importe quel ordre (pas de case Boss obligatoire).
- Objectifs possibles : **vaincre des combats clés**, **réussir des mini-jeux**, **atteindre des cases précises** (cachées dans le brouillard), **résoudre un événement narratif** avec une issue donnée.
- **3 objectifs** par zone ; un objectif rempli **reste rempli** même si le brouillard se referme. Validation (1re fois) : 1 500 Crédits, 40 PR, 150 XP de pass. Détail : `01_Game_Design/01_Plateau.md`.

## 6. Combat
- Combat sur une **frise** de 10 points : chaque unité avance d'un point à chaque pas (0,35 s) ; à 10, c'est son tour, et pour un allié la frise se fige le temps qu'il choisisse (30 s). Les actions ennemies sont **annoncées** dans la zone de préparation et peuvent être **parées** ou **interrompues**.
- **Énergie** : chaque compétence coûte de l'énergie (A1 gratuite, A2 : 30), qui se régénère à chaque pas ; plus de recharge par tour.
- **Pause de résolution** (1,3 s) après chaque compétence, avec une **animation par famille** (feu, givre, foudre, acide, chaînes, marque, soin, bouclier, hâte, temps, impact).
- **Équipe de 4 méta-humains**, deux camps face à face en 2×2 autour d'un terrain central, frise en haut, éventail de compétences en bas.
- **Synergie d'équipe** : bonus passif de familles, liens entre agents (niveaux 1 à 5), effets de composition (Quatuor, Trinité, Double front, Étincelle).
- IA ennemie selon la difficulté ; joueur en **mode auto ou manuel**.
### Défaite
- L'équipe **recule sur la case précédente** et **gagne de la fatigue** ; le joueur peut réessayer ou contourner.
- Si **toute l'équipe est à 100 % de fatigue**, retour **instantané au QG**.
### Fatigue en combat
- Chaque combat **coûte de la fatigue**. À 100 %, le membre ne peut plus combattre ; **malus de stats** progressif au-delà de 60 %.
- Valeurs : +12 % de fatigue par combat, +15 % en défaite ; malus jusqu'à −30 % à 100 %. Récompenses : 200 / 300 / 500 Crédits (Facile / Normal / Élite), 1 000 pour un boss.
- Règles complètes, formules, IA, bestiaire : `01_Game_Design/02_Combat.md`.

## 7. Équipe : méta-humains
### Compétences
- Chaque agent : **2 compétences de base** propres à lui (A1 légère et gratuite, A2 spéciale payante) ; la **3e place** du combat est une **carte du grimoire**. Détail : `01_Game_Design/06_Kits_Competences.md`.
- Rattaché à une **famille de pouvoirs** (voir Synergies).
### Synergies
- **Familles** (alignées sur le lore, voir `02_Lore_Narration/02_Profils_Pouvoirs.md`) : **Matière** (ambre), **Élémentaire** (cyan), **Physique / Bio** (rouge), **Perception** (violet).
- **Bonus passif** : chaque paire de familles dans l'escouade donne à toute l'équipe un bonus permanent de statistique (plafond 30 %).
- **Liens** : les victoires remportées ensemble font monter le niveau de lien de deux agents ; chaque niveau amplifie de 25 % le bonus passif de leurs familles.
- **Effets de composition** : Quatuor (4 familles), Trinité (tank, dégâts, soutien), Double front (2 tanks), Étincelle (2 contrôles).
- Détail chiffré : `01_Game_Design/02_Combat.md` et `01_Game_Design/03_Metahumains.md`.
### Recrutement
- **Équipe de départ** : le personnage joueur + 3 compagnons.
- Via **Points de Recherche (PR)** : **boutique éphémère** (3 agents par jour à 9 h, coût fixe en PR : 150 / 450 / 1 200 ; pas de gacha sur les agents), **arcs narratifs** (4 agents), **événements de week-end** (4) et **prouesses** (4).
- Renouvellement de la boutique : 1 gratuit/jour, puis Crédits, puis premium. Pas de doublons.
- **3 rangs de rareté** : Agent / Élite / Légende — coût en PR et puissance croissants. Les membres de l'Apex Unit ne sont pas recrutables (PNJ).

### Entraînement & progression
- Entraînement **automatique au salon** grâce aux objets placés.
- Niveaux → **paliers de compétences** (effets additionnels), validés en Crédits.
- **Niveau max : 50** pour tous les membres.
- Accélération : monnaie premium ou accélérateurs gagnés en jeu.
### Fatigue
- Jauge de fatigue par membre en mission.
- Recharge avec le temps ou via objets.

## 8. Économie & monétisation
- Modèle **F2P + pass saisonnier** ; le $ fait **uniquement gagner du temps** (rien d'exclusif hors cosmétiques).
- **3 monnaies** : Crédits (soft), PR (recrutement), Premium ($).
- Puits de Crédits : amélioration QG, paliers de compétences, consommables de mission, objets du salon, renouvellement de pool.
- **Pass** : saisons de 6 semaines, piste gratuite + premium ; XP via objectifs secondaires et missions.
- **Cosmétiques** (sans effet en jeu) : tenues de recrues, décor du QG, dés et pions ; via pass, boutique, succès et événements.
- Détail : `01_Game_Design/04_Economie.md`.
- Valeurs : Agent / Élite / Légende = 150 / 450 / 1 200 PR ; pool de 5 ; renouvellement 3 000 Crédits puis 50 gemmes ; packs de gemmes de 0,99 € à 49,99 € ; pass premium 9,99 €.

## 9. Méta / QG
| Pièce | Rôle |
|---|---|
| Salon d'entraînement | Entraînement automatique via objets placés |
| Infirmerie | Récupération de fatigue accélérée, emplacements limités |
| Salle des opérations | Lance les expéditions idle ; niveaux = plus d'emplacements |
| Labo de recherche | Génère des PR passivement avec le temps |
| Archives / Lore | Collection : fiches membres, histoire des villes, scènes débloquées |
- Pièces améliorables en Crédits.
- 10 niveaux par pièce (5 000 Crédits pour le niv. 2, ×1,35 ensuite), plafonnés à 2 × l'acte atteint. Effets par niveau : `01_Game_Design/07_QG_Expeditions.md`.

### 9.1 Expéditions & objectifs secondaires
- Se déroulent dans les **zones déjà débloquées** de la campagne.
- Deux formats :
  - **Envoi idle chronométré** : membres non utilisés envoyés X heures, retour avec PR/objets + fatigue.
  - **Mini-plateaux jouables** : courts, rejouables (farm, événements).
- **Objectifs secondaires** apparaissant en **temps réel** : périodiquement (ex. toutes les 5 h), X objectifs apparaissent aléatoirement dans les zones disponibles.
- Durées 1 h / 4 h / 8 h / 12 h (250 à 2 000 Crédits, 5 à 38 PR, +10 à +50 % de fatigue) ; objectifs secondaires : 3 toutes les 5 h, 9 en attente maximum, expiration 24 h. Détail : `01_Game_Design/07_QG_Expeditions.md`.

## 10. Direction artistique, UX & audio
- **Style comics moderne** ; couleur + icône par famille (Matière ambre, Élémentaire cyan, Physique/Bio rouge, Perception violet).
- **Hub** : le QG en vue en coupe. **Plateau** : carte illustrée vue de dessus.
- **Onboarding** : prologue jouable (~5 min), systèmes débloqués pendant l'Acte 1.
- **Combat** : frise pleine largeur en haut, deux camps en 2×2 autour d'un terrain central (portrait, PV, énergie), 3 cartes de compétence en éventail en bas (coût en énergie), boutons Auto, vitesse et Retraite. PC : souris + raccourcis clavier.
- **Audio** : thème par arrondissement, doublage partiel des répliques de combat, banques sous licence.
- Détail : `03_Art_UI_UX/01_UX_Ecrans.md`, `03_Art_UI_UX/03_Maquettes_Screen_Flow.md` (wireframes, maquettes, screen flow), `03_Art_UI_UX/02_Direction_Artistique_Audio.md`.

## 10bis. Technique
- Unity 6.6, Unity Gaming Services (Authentication, Cloud Save, Economy, Cloud Code, Remote Config, Analytics).
- **Économie validée côté serveur** ; client data-driven (ScriptableObjects), combat simulé indépendamment du rendu.
- **Architecture** : couches Clean (Domaine C# pur → Application → Infrastructure / Présentation MVP), simulation déterministe ; **tout dans Unity** : backend UGS (scripts Cloud Code JS, Economy, Remote Config) déployé depuis l'éditeur, contrôles serveur légers.
- Détail : `04_Tech_Unity/00_Vue_Ensemble.md` et suivants.

## 10ter. Live-ops, obligations, production
- **Live-ops** : saison de 6 semaines + 1 événement ; nouvelle ville ~tous les 6 mois ; KPIs dans `05_Production/Live_Ops.md`.
- **Obligations** : checklist stores, RGPD, IA, classification d'âge dans `05_Production/Obligations_Legales.md`.
- **Production** : ~12 mois, sortie **Early Access Steam** puis mobile — `05_Production/Roadmap.md` ; risques dans `05_Production/Risques.md`.

## 11. Questions ouvertes
- [x] Types de cases et récompenses
- [x] Combat : règles, formules, IA, récompenses, bestiaire
- [x] Synergies : bonus passif, liens entre agents, effets de composition ✔ (remplacent les combos)
- [x] Expéditions et objectifs secondaires : durées, récompenses, plafonds
- [x] Économie : valeurs de départ, packs, pass, missions, consommables
- [x] Campagne : 4 actes (New York, Paris, Moscou, Salem), ordre selon Agent D ou Agent M ✔ ; **zones de Paris, Moscou et Salem à créer**
- [x] Orientation écran : paysage
- [x] PvP futur : arène asynchrone (équipe défensive IA) — hors lancement
- [x] Cosmétiques : types, sources, volumes, prix indicatifs
- [x] 30 agents numérotés + 2 personnages joueurs + 10 cryptides (fiches de lore) ✔ ; **portraits à produire**
- [x] Kits : 2 compétences de base uniques par agent (noms et statuts), énergie ✔ ; **effets sur mesure par agent et cartes du grimoire à compléter**
- [x] Lieutenants des Actes 2 à 4 ; mentors de l'Apex Unit par acte
- [ ] **Calage en playtest** des valeurs de départ (`01_Game_Design/05_Equilibrage.xlsx`) — à faire pendant le prototype et l'alpha
- [ ] **Recommandations ⚑ à confirmer en playtest** : énergie (départ 30, régénération 3), pause de résolution 1,3 s, paliers des liens (3, 8, 15, 25 victoires), plafond de niveau par acte, déséquilibre des familles (Perception 9, Élémentaire 6)

## 12. Index des specs détaillées
| Système | Fichier |
|---|---|
| Plateau | `01_Game_Design/01_Plateau.md` |
| Combat | `01_Game_Design/02_Combat.md` |
| Méta-humains & synergies | `01_Game_Design/03_Metahumains.md` |
| Économie | `01_Game_Design/04_Economie.md` |
| QG & expéditions | `01_Game_Design/07_QG_Expeditions.md` |
| Mini-jeux | `01_Game_Design/08_Mini_Jeux.md` |
| Histoire, agents, cinématique | `02_Lore_Narration/03_Histoire_et_Actes.md`, `04_Agents_The_Dawn.md`, `05_Cinematique_Lancement.md` |
| Équilibrage (tableur) | `01_Game_Design/05_Equilibrage.xlsx` |
| Lore | `02_Lore_Narration/` |
| UX & écrans | `03_Art_UI_UX/01_UX_Ecrans.md` |
| Maquettes & screen flow | `03_Art_UI_UX/03_Maquettes_Screen_Flow.md` |
| DA & audio | `03_Art_UI_UX/02_Direction_Artistique_Audio.md` |
| Kits de compétences | `01_Game_Design/06_Kits_Competences.md` |
| Architecture logicielle | `04_Tech_Unity/` (00 à 09) |
| Roadmap, risques, live-ops, obligations | `05_Production/` |
