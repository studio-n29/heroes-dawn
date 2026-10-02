# Heroes Dawn — Game Design Document

> Version : 2.4 · Mise à jour : 2026-09-29 · Auteur : Djason (Studio N29)
> Statut : **v2.0 — prêt pour démarrer la production (M1 Prototype)**. Tous les domaines sont cadrés ; le contenu restant (recrues, kits de compétences, cosmétiques) est listé en §11.

## 1. Pitch
Jeu mobile de gestion-RPG dans l'univers Apex Unit. Le joueur dirige une section de **La Guilde** dans une grande ville, recrute et entraîne des méta-humains, et les engage sur des plateaux de jeu où il affronte combats et mini-jeux.

## 1bis. Vision
### Public cible
- **Fans de super-héros, profil casual+** : attirés par l'univers et les personnages, profondeur tactique modérée.
### Sessions
- **5-10 minutes** par session.
- Les **zones sont reprenables** : une zone dure 15-30 min au total mais la progression est sauvegardée à chaque case ; une session = quelques lancers de dé, un combat, un passage au QG.
### Points forts (USP)
1. **Plateau + combat** : jeu de plateau (dé, brouillard de guerre, choix de chemins) couplé à un combat ATB en équipe — rare sur mobile.
2. **Juste, sans pay-to-win** : recrutement au choix, pas de gacha, l'argent ne fait gagner que du temps.
### Piliers de design
Chaque fonctionnalité doit servir au moins un pilier.
1. **Chaque lancer compte** — le plateau offre toujours un choix intéressant (chemin, dé bonus, brouillard).
2. **Mon équipe, mes choix** — le joueur construit son équipe et ses synergies, il ne subit pas le hasard.
3. **Vivre l'univers Apex** — chaque ville raconte une histoire ; personnages canon et lore post-Supernova.
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
| Relation à Apex Unit | Spin-off autonome, pas de cross-progression |
| Genre | Gestion d'équipe + RPG + jeu de plateau + combat temps actif (ATB) |

## 2bis. Périmètre de contenu (lancement — New York)
| Élément | Quantité | Note |
|---|---|---|
| Villes | 1 (New York) | 4 autres en live-ops |
| Plateaux / zones | 5 / 15 | 1 plateau = 1 acte |
| Cases par zone | ~25 | ~5 combats, 2 mini-jeux, 2 événements par passage |
| Objectifs par zone | 3 | |
| Recrues | 35 (20 Agents / 10 Élites / 5 Légendes) | Sortie en **2 vagues** : ~20 au lancement, 15 dans les premières semaines |
| Types d'ennemis | 13 + 5 boss | 3 par acte (1 pour l'Acte 5) + 5 boss de mini-climax |
| Mini-jeux | 4 | 1 par format |
| Événements narratifs | 35 | 30 génériques (pool aléatoire) + 5 scénarisés (1 par acte) |
| Combos | 7 de famille + duos légendaires | |
| Pièces du QG | 5 | |

### Stratégie de production (équipe de 1-2 personnes)
- **Rig 2D partagé** : squelette d'animation commun, seuls les visuels changent par personnage.
- **Assets** : majoritairement des **portraits**, quelques **modèles 3D low poly** ; **génération assistée par IA**.
- **Lancement en 2 vagues** pour les recrues.
- Voir `05_Production/Risques.md`.

## 3. Univers & contexte
- Se déroule **après la Supernova de Paris**.
- Pendant l'**Œuvre 1 — La Guilde** du canon : après le blackout mondial, des milliers de méta-humains naturels apparaissent ; la Guilde, dirigée par **Brie Rayner**, ouvre des sections locales.
- Les Apex (Rayner, Adam West, John Guerin, Erik Bronson) sont des **PNJ** : directrice et mentors. Eda est absent (en Enfer).
- Lore détaillé : `02_Lore_Narration/` (sources originales dans `Sources/`).
- Le joueur est chef d'une section de La Guilde dans une ville : **New York, Los Angeles, Paris, Londres, Moscou**.
- **Campagne New York** : un plateau par arrondissement — 1. Brooklyn, 2. Queens, 3. Bronx, 4. Staten Island, 5. Manhattan.
  - Menaces : gangs méta-criminels unifiés sous le **Syndicat**, **AMIF / Général Stroke**, culte des **Enfants de la Brèche**.
  - Antagoniste : **Silas Moreau, « le Héraut »**, chef du culte caché derrière le Syndicat.
  - Détail : `02_Lore_Narration/03_New_York.md`.
- Chaque ville = une campagne avec sa propre histoire.
- **Structure d'une campagne : 5 plateaux × 3 zones = 15 zones**, débloquées progressivement.
- **Arc en 5 actes** : chaque plateau = un acte avec son mini-climax ; le 5e plateau conclut l'arc principal de la ville.
- **Lancement avec New York uniquement** ; les 4 autres villes arrivent en live-ops (une par grosse mise à jour).
- **Ordre des villes libre** : dès que plusieurs villes sont disponibles, le joueur choisit.
- **Après une ville** : ville suivante, mode difficile, endgame farm (expéditions + objectifs secondaires), équipe **transférable** entre villes.
- Les villes sont un **skin narratif** : mécaniques identiques, mais **membres recrutables différents** par ville.

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
- Système **temps actif (ATB)** : chaque unité a une jauge qui se remplit en temps réel selon sa Vitesse ; le combat ne s'arrête pas pendant que le joueur choisit.
- Inspiration : Summoners War (compétences, auto/manuel), mais sans pause entre les tours.
- **Équipe de 4 méta-humains** (jusqu'à 6 paires possibles dans l'équipe).
- IA ennemie choisit ses actions selon la difficulté.
- Joueur en **mode auto ou manuel**.
### Défaite
- L'équipe **recule sur la case précédente** et **gagne de la fatigue** ; le joueur peut réessayer ou contourner.
- Si **toute l'équipe est à 100 % de fatigue**, retour **instantané au QG**.
### Fatigue en combat
- Chaque combat **coûte de la fatigue**. À 100 %, le membre ne peut plus combattre.
- **Malus de stats** progressif au-delà d'un seuil de fatigue.
- Valeurs : jauge 1 000 pts remplie à VIT × 4 par seconde ; +12 % de fatigue par combat, +15 % en défaite ; malus au-delà de 60 %, jusqu'à −30 % à 100 %. Récompenses : 200 / 300 / 500 Crédits (Facile / Normal / Élite), 1 000 pour un boss.
- Formules, IA, bestiaire (13 ennemis + 5 boss) : `01_Game_Design/02_Combat.md`.

## 7. Équipe : méta-humains
### Compétences
- Chaque membre : **3 compétences actives + 1 passive**.
- Rattaché à une **famille de pouvoirs** (voir Synergies).
### Synergies
- Combos **prédéfinis** (non émergents).
- **Familles d'archétypes** (alignées sur le lore, voir `02_Lore_Narration/02_Profils_Pouvoirs.md`) :
  1. **Matière** (matérialisation, polymorphie, transmutation — contrôle, création)
  2. **Élémentaire** (électricité, corrosif/incendiaire, thermique — dégâts, altérations)
  3. **Physique / Bio** (force, résistance, régénération, adaptation — tank, soin)
  4. **Perception** (bullet-time, écholocation, camouflage, magnétoréception — précision, esquive, détection)
- **Combos de famille** : chaque paire de familles a son combo (6 inter-familles + *Résonance* pour une même famille). Détail : `01_Game_Design/03_Metahumains.md`.
- **Duos légendaires** : quelques paires de personnages ont un combo unique (plus fort, animation dédiée) qui **remplace** le combo de famille.
- **Déclenchement** : compétence commune — quand les jauges ATB des deux membres sont pleines, un bouton Combo apparaît ; les deux consomment leur tour.
- Duos de New York : **Verre + Givre** (Tempête de cristal), **Écho + Instant** (Œil absolu) — voir `02_Lore_Narration/04_Recrues_New_York.md`.
- Effets chiffrés des combos : `01_Game_Design/03_Metahumains.md`.
### Recrutement
- Via **Points de Recherche (PR)** obtenus : en campagne, en boutique ($), via expéditions secondaires, pass.
- **Pool quotidienne** propre à chaque ville ; **choix direct** à coût fixe en PR (pas de gacha).
- Renouvellement : 1 gratuit/jour, puis Crédits, puis premium.
- Pas de doublons ; pool épuisée = collection de la ville terminée.
- **Légendes liées à l'histoire** : une par acte, elle entre dans la pool après le mini-climax.
- **3 rangs de rareté** : Agent / Élite / Légende — coût en PR et puissance croissants. Les Légendes sont les méta-humains naturels les plus puissants ; les Apex ne sont pas recrutables (PNJ).
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
- **Combat** : 4 portraits en bas, compétences au toucher, boutons Combo et Auto. PC : souris + raccourcis clavier.
- **Audio** : thème par arrondissement, doublage partiel des répliques de combat, banques sous licence.
- Détail : `03_Art_UI_UX/01_UX_Ecrans.md`, `03_Art_UI_UX/02_Direction_Artistique_Audio.md`.

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
- [x] Combos de famille + déclenchement ✔ ; duos légendaires à lister
- [x] Expéditions et objectifs secondaires : durées, récompenses, plafonds
- [x] Économie : valeurs de départ, packs, pass, missions, consommables
- [x] Campagne : 5×3 zones, arc en 5 actes, ordre libre, post-ville ✔
- [x] Orientation écran : paysage
- [x] PvP futur : arène asynchrone (équipe défensive IA) — hors lancement
- [x] Duos légendaires de New York (2)
- [x] Cosmétiques : types, sources, volumes, prix indicatifs
- [x] 10 Élites + 20 Agents de New York (fiches v1)
- [x] Kits de compétences des 35 recrues (v1, à équilibrer)
- [x] Lieutenants des Actes 2 à 4 ; mentors Apex par acte
- [ ] **Calage en playtest** des valeurs de départ (`01_Game_Design/05_Equilibrage.xlsx`) — à faire pendant le prototype et l'alpha
- [ ] **Recommandations ⚑ à confirmer en prototype** : dé 1d6 et 6 PM par dé bonus ; jauges qui continuent pendant le choix en manuel ; plafond de niveau par acte

## 12. Index des specs détaillées
| Système | Fichier |
|---|---|
| Plateau | `01_Game_Design/01_Plateau.md` |
| Combat | `01_Game_Design/02_Combat.md` |
| Méta-humains & synergies | `01_Game_Design/03_Metahumains.md` |
| Économie | `01_Game_Design/04_Economie.md` |
| QG & expéditions | `01_Game_Design/07_QG_Expeditions.md` |
| Mini-jeux | `01_Game_Design/08_Mini_Jeux.md` |
| Événements NY | `02_Lore_Narration/05_Evenements_New_York.md` |
| Équilibrage (tableur) | `01_Game_Design/05_Equilibrage.xlsx` |
| Lore & New York | `02_Lore_Narration/` |
| UX & écrans | `03_Art_UI_UX/01_UX_Ecrans.md` |
| DA & audio | `03_Art_UI_UX/02_Direction_Artistique_Audio.md` |
| Kits de compétences | `01_Game_Design/06_Kits_Competences.md` |
| Architecture logicielle | `04_Tech_Unity/` (00 à 09) |
| Roadmap, risques, live-ops, obligations | `05_Production/` |
