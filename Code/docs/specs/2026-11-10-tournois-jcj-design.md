# Bloc D — Tournois JcJ (hors ligne et en ligne)

> Spec du 2026-11-10. Source : point de développement « finaliser la 1.0.0 », partie « Les tournois ». Bloc D sur 5. Dépend du bloc A (positions, pour l'IA des équipes adverses) et du bloc B (Composants, recettes, pour les récompenses). Remplace le tournoi simulé de la 0.15.0 (`Application/Features/Meta/Tournament.cs`).

## Contexte
« The Dawn organise des compétitions régulièrement afin que les agents puissent se rencontrer et partager leurs compétences lors de duels. »

## Décisions validées

### 1. Calendrier
Fichier de données statique `Resources/Content/tournaments.json` : `id`, `nameKey`, `start` et `end` (`MM-JJ`, la fin peut tomber l'année suivante), `rewardsId`. Rotation de départ ⚑ :

| Tournoi | Dates |
|---|---|
| Nouvel An | 29/12 → 10/01 |
| Saint-Valentin | 07/02 → 16/02 |
| Printemps | 20/03 → 03/04 |
| Été | 14/06 → 28/06 |
| Vacances | 14/07 → 31/07 |
| Rentrée | 01/09 → 14/09 |
| Halloween | 20/10 → 03/11 |
| Noël | 01/12 → 26/12 |

Un seul tournoi est actif à la fois ; entre deux tournois, la page affiche le suivant.

### 2. Un duel
- **Équipe de défense** : capture de l'équipe du joueur (agents, niveaux, cartes avec niveaux, objets, liens), envoyée au serveur à chaque connexion et à chaque changement d'équipe.
- **Hors ligne** : l'adversaire est la capture d'un joueur de score proche (jamais soi-même). Son équipe est pilotée par l'IA selon la position de ses agents. La simulation tourne en local avec une graine fournie par le serveur.
- **En ligne** : file d'attente sur un canal Supabase Realtime. Sans adversaire au bout de **15 secondes**, le duel passe automatiquement en hors ligne.
- **Simulation canonique** : la même simulation tourne sur les deux appareils, avec l'hôte (identifiant de joueur le plus petit) dans le camp `Ally` et l'invité dans le camp `Enemy` ; chaque écran affiche son propre camp. La simulation attend la commande du camp dont une unité est prête ; elle est identique des deux côtés, donc seule la commande est échangée (pas d'état). Un joueur qui laisse passer **30 s** passe son tour (commande `Pass`) ; sans message de l'adversaire pendant **45 s**, il est considéré comme parti.
- **Empreinte** : tous les 20 ticks, chaque appareil envoie une empreinte de l'état (`StateHash`) ; si elles diffèrent, le duel est **annulé sans pénalité ni point**.
- Le tournoi demande un compte connecté ; hors connexion la page l'indique.

### 3. Points
- Victoire hors ligne : **+10** ; victoire en ligne : **+20** ; défaite : 0 (jamais de perte).
- Quitter un duel en ligne : **−10**, sans descendre sous 0 ; une désynchronisation annule sans pénalité. Quitter un duel hors ligne : sans pénalité, le duel n'est pas compté.
- Plafond de **10 duels par jour et par tournoi** ; le serveur borne aussi les points par jour (un client peut être modifié).

### 4. Classement et récompenses
- Un classement par tournoi : points, départage par la date d'atteinte du score. Affichage : top 100 et rang du joueur.
- Récompenses réclamables jusqu'à **7 jours** après la fin.
- **Paliers de points** ⚑ : 50 Bronze, 150 Argent, 300 Or, 500 Platine, 800 Diamant ; chaque palier donne des Crédits, des Composants et un cosmétique de palier.
- **Top 10 %** : lot intermédiaire. **Top 3** : cadre et titre exclusifs, une recette rare (source `Tournament`), des catalyseurs et un cosmétique exclusif.
- Les cosmétiques sont des identifiants (`cosmetic:<id>`) rangés dans une liste du profil ; le catalogue et l'affichage sont définis au bloc E.

### 5. Serveur Supabase
Script versionné `Assets/_Project/Backend/Supabase/002_hd_arena.sql`, **non appliqué sans accord explicite de l'utilisateur** :
- `hd_arena_defenses(user_id, snapshot jsonb, rating int, updated_at)` ;
- `hd_arena_scores(tournament_id, user_id, points int, reached_at timestamptz)` ;
- `hd_arena_matches(id, tournament_id, host_id, guest_id nullable, mode, result, points, created_at)` : journal et plafonds ;
- fonction `hd_submit_duel(...)` : applique les points, le plafond de 10 duels par jour et par tournoi, la borne quotidienne et le départage ;
- règles RLS : lecture publique du classement et des captures d'adversaire, écriture limitée à son propre `user_id` ou par `hd_submit_duel`.
Préfixe `hd_` obligatoire ; les tables `hq_*` ne sont jamais touchées. Aucune clé `service_role` dans le dépôt.

### 6. Existant supprimé
`Tournament.cs`, `TournamentSave` et le classement factice sont supprimés. Les points de l'ancien tournoi ne sont pas conservés (jamais publié).

## Tests
Calendrier (passage d'année, aucun tournoi actif, tournoi suivant) ; points et plafonds ; paliers et top 3 ; simulation de duel (déterminisme, canonicité des deux côtés, empreinte identique) ; désynchronisation ; repli en hors ligne ; départ en ligne et hors ligne ; passerelle en mémoire à la place du réseau.

## Hors périmètre
Écrans (page Tournoi, file d'attente, duel en ligne, résumé, classement, réclamation) : lot UI. Application de la migration SQL sur le projet Supabase : sur accord de l'utilisateur. Arène classée par ligues (M+6) et arène par équipes (M+18) : plan LiveOps.

## Points ouverts ⚑
Dates et thèmes des 8 tournois ; paliers et lots ; quotas Realtime du palier gratuit (limite de connexions simultanées) à vérifier avant le lancement ; protection contre les clients modifiés limitée aux plafonds (pas de vérification serveur des combats avant l'arène classée).
