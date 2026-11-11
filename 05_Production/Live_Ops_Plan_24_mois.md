# Plan LiveOps — 24 mois après le lancement (M0 → M+24)

> Statut : **proposition à relire** (rédigée le 2026-11-09) · Remplace la partie « Rythme » de `Live_Ops.md`.
> Les mois sont **relatifs** : M0 = lancement du jeu, date non fixée. M+3 = trois mois après le lancement.

## 1. Hypothèses de départ (validées avec Djason)

| Sujet | Hypothèse |
|---|---|
| Contenu à M0 | Les **4 villes** (New York, Paris, Moscou, Salem), **tous les agents** (les 30 numérotés + les 10 cryptides de Salem), **1 repaire par ville**, les **arènes** (tournois). |
| Équipe | **1 à 2 personnes**, rythme tenable (voir §3). |
| Modèle économique | F2P, pass saisonnier de 6 semaines, **pas de gacha**, **pas de pay-to-win**, cosmétiques sans effet en jeu. |
| Coût en production | **0 €** : Supabase gratuit, aucune branche payante, aucune migration sans accord explicite. |
| Cross-progression | **Aucune** avec les autres jeux de l'Apex Universe (décision 2026-09). L'écho avec Hell Rising est narratif et cosmétique. |
| Plateformes | Lancement sur Steam, Android et iOS. **À confirmer** : la `Roadmap.md` parle encore d'un Early Access Steam suivi du mobile. |

## 2. Principes

1. **Faire vivre avant d'ajouter** : la phase 1 réutilise le moteur déjà livré.
2. **Une majeure s'appuie sur la précédente** : chaque phase a un pilier.
3. **Les mineures ne demandent aucun build** : elles passent par des données.
4. **On décide sur des chiffres** : trois points de décision (§8) peuvent réorienter la suite.
5. **Le mois de souffle est sacré** : après une majeure, correctifs et équilibrage seulement.

## 3. Cadence

| Élément | Rythme | Contenu |
|---|---|---|
| **Saison** | 6 semaines (18 saisons en 24 mois) | Pass gratuit + premium, cosmétiques, 1 événement |
| **Mineure** | Toutes les 2 semaines | Événement, rotation de boutique, équilibrage, cosmétique, repaire hebdomadaire, correctifs. **Données seules.** |
| **Majeure** | Toutes les ~3 mois (8 en 24 mois) | Gros contenu ou nouveau système. **Mise à jour sur les stores.** |
| **Gel de contenu** | 2 semaines avant une majeure | Plus d'ajout, uniquement des tests |
| **Mois de souffle** | Après chaque majeure | Correctifs et équilibrage |
| **Saison tampon** | Une saison sur deux | Événements recyclés, pass allégé |

Les majeures XL (M+9, M+15, M+24) demandent une **préproduction lancée une fenêtre plus tôt** (voir §9).

## 4. Vue d'ensemble

| Phase | Période | Pilier | Majeures |
|---|---|---|---|
| **1** | M0 → M+6 | Faire vivre l'existant | M+3 Repaires II · M+6 Arène classée |
| **2** | M+6 → M+12 | Nouvelles recrues | M+9 Recrues II · M+12 Endgame |
| **3** | M+12 → M+18 | Compétition et social | M+15 Guilde · M+18 Arène par équipes |
| **4** | M+18 → M+24 | Écho de Hell Rising | M+21 Prélude · **M+24 Retour d'Eda** |

## 5. Détail des majeures

### Phase 1 — Faire vivre l'existant (M0 → M+6)

**Mineures** : saisons 1 à 4, premiers événements, recalage des KPIs et de l'équilibrage, finition des voix et des langues.

| Majeure | Taille | Contenu | Critère de sortie |
|---|---|---|---|
| **M+3 Repaires II** | M | Un deuxième repaire par ville (4 nouveaux boss). Réutilise le moteur de repaires de la 0.15.0. | Chaque repaire est jouable et équilibré par playtest. |
| **M+6 Arène classée** | M | Ligues, récompenses de saison, cosmétiques exclusifs. Tables Supabase de classement (§7). | Classement de saison fonctionnel et protégé par des plafonds. |

### Phase 2 — Nouvelles recrues (M+6 → M+12)

| Majeure | Taille | Contenu | Critère de sortie |
|---|---|---|---|
| **M+9 Recrues II** | XL | 8 à 10 agents **inédits**, avec un récit court par ville : lore, art, kit de compétences, équilibrage, voix. Ils se gagnent en jouant, jamais par gacha. | Kits équilibrés, aucun ne domine le classement. |
| **M+12 Endgame** | L | Mode difficile étendu, défis de fin de partie, nouvelles mécaniques de cartes. | Un joueur de fin de partie a de quoi jouer chaque semaine. |

### Phase 3 — Compétition et social (M+12 → M+18)

| Majeure | Taille | Contenu | Critère de sortie |
|---|---|---|---|
| **M+15 Guilde** | XL | Coopération légère et repaires à deux. Option « sans chat » pour limiter la modération. Conformité RGPD à vérifier. | Une guilde peut jouer ensemble sans risque de modération. |
| **M+18 Arène par équipes** | L | Saisons par équipes, événements communautaires. Réutilise le classement. | Le classement par équipes tient la charge. |

### Phase 4 — Écho de Hell Rising (M+18 → M+24)

Hell Rising est un autre jeu de Studio N29 dans l'Apex Universe. Sa sortie coïncide avec les 2 ans de Heroes Dawn.

| Majeure | Taille | Contenu | Critère de sortie |
|---|---|---|---|
| **M+21 Prélude** | M | Indices à Salem, signaux de Brie Rayner, événement de teasing, petites quêtes. Prépare l'arc sans le dévoiler. | Les joueurs comprennent qu'un événement majeur approche. |
| **M+24 Retour d'Eda** | XL | **Nouvel arc** avec le retour d'Eda, en écho aux événements de la fin de Hell Rising. Cet arc remplace l'« Acte V » et l'« Anniversaire » envisagés plus tôt. | Arc jouable de bout en bout, classification d'âge vérifiée. |

**Cadre de l'arc d'Eda**
- **Statut d'Eda : PNJ, boss et allié d'arc.** Il n'est pas recrutable et n'a pas de nouveau kit à équilibrer.
- **Canon** : Eda est en Enfer (Œuvre 2, *La Croisade Infernale*). Le canon prévoit que Rayner ouvre une brèche ciblée pour le ramener. Rayner est déjà à Salem, au complexe N29, dans Heroes Dawn.
- **R7** : aucune scène de suicide ni d'automutilation. Eda paraît par ses actes héroïques et sa résurrection.
- **Spoilers** : le contenu exact de l'écho dépend de la fin de Hell Rising. Ces détails restent ouverts (§11).
- **Pas de cross-progression** : une éventuelle récompense cosmétique commune ne nécessite aucun compte partagé.
- **Fenêtre flottante** : M+24 est ancré sur la date de sortie confirmée de Hell Rising. Le contenu est gelé 3 semaines avant. Si Hell Rising dépasse M+27, un Anniversaire autonome prend le relais (plan B).
- **Pic de trafic** : la sortie de Hell Rising peut amener de nouveaux joueurs. Un point de décision en M+22 vérifie la charge serveur et l'onboarding.

## 6. Catalogue de mineures

| Type | Fréquence | Exemple |
|---|---|---|
| Événement de saison | 1 par saison | Mini-plateau thématique de 10 à 14 jours |
| Rotation de boutique | Hebdomadaire | Cosmétiques (3 tenues par saison au pass, 1 par événement, 1 thème de QG, 2 dés ou pions) |
| Repaire hebdomadaire | Hebdomadaire | Rotation parmi les repaires existants |
| Équilibrage | Toutes les 2 semaines | Paramètres `BalanceConfig` modifiés à distance |
| Mise à jour de la boutique de gemmes | Selon les KPIs | Offres de bienvenue, packs |

## 7. Socle technique (gratuit)

**État actuel (vérifié le 2026-11-09)**
- Le package **Addressables n'est pas installé** et le dossier `Assets/_Project/AddressableGroups` est vide.
- **Supabase** ne contient que `hd_profiles` (migrations 001 et 002).
- `04_Tech_Unity/06_Backend_Donnees.md` décrit encore l'ancien plan Unity Gaming Services : il sera à réaligner sur Supabase.
- Les achats sont simulés (`IPurchaseGateway`).

**À mettre en place (avant M0 ou en M+1)**
1. **Calendrier JSON** hébergé en statique gratuit : saisons, événements, boutique, paramètres d'équilibrage. Le jeu garde le dernier calendrier connu s'il est hors ligne.
2. **Tables `hd_seasons` et `hd_arena_scores`** avec sécurité par ligne, plafonds de plausibilité et classement réservé aux comptes liés. Le niveau gratuit de Supabase doit être vérifié.
3. **Majeures** par mise à jour sur les stores (art, boss, agents). Pas d'Addressables : trop de complexité pour 1 à 2 personnes.

**Limite acceptée** : le jeu est hors ligne par défaut, donc le classement peut être triché. Le plan vise des plafonds raisonnables, pas une protection parfaite.

## 8. KPIs et points de décision

Cibles de départ, à recaler sur les premières données : rétention 40 % / 15 % / 6 % à J1 / J7 / J30, 3+ sessions par jour, conversion payeurs de 2 à 3 %, achat du pass premium de plus de 40 % parmi les payeurs (`Live_Ops.md`).

| Point | Quand | Si… | Alors… |
|---|---|---|---|
| **D1** | M+3, avant Repaires II | J7 < 15 % | Priorité à la rétention (onboarding, objectifs quotidiens) avant tout contenu |
| **D2** | M+6, après l'Arène classée | Moins de 20 % des joueurs actifs participent | Revoir les récompenses avant l'Arène par équipes |
| **D3** | M+12 | Conversion < 1 % | Revoir cosmétiques et pass avant l'investissement social |
| **D4** | M+22 | Charge serveur ou onboarding insuffisants | Renforcer avant la sortie de Hell Rising |

## 9. Charge estimée

Unités : semaine-personne (sp), estimations **à recaler** sur la vitesse réelle des lots des versions 0.14.0 et 0.15.0.

| Taille | Estimation |
|---|---|
| S | 2 à 3 sp |
| M | 4 à 6 sp |
| L | 7 à 10 sp |
| XL | 10 à 14 sp |

Une fenêtre de 12 semaines à 1,5 personne représente ~18 sp. Les mineures en consomment ~35 %, il reste donc ~12 sp pour la majeure. **Les majeures XL touchent cette limite** : leur préproduction (lore, art, prototypes) commence dans la fenêtre précédente.

| Majeure | Taille | Préproduction à lancer |
|---|---|---|
| M+9 Recrues II | XL | Dès M+6 |
| M+15 Guilde | XL | Dès M+12 |
| M+24 Retour d'Eda | XL | Dès M+21, prélude M+21 inclus |

## 10. Risques

Les risques R1 à R12 de `Risques.md` restent valables. Nouveaux risques :

| # | Risque | Impact | Probabilité | Parade |
|---|---|---|---|---|
| R13 | Épuisement de l'équipe | Élevé | Élevée | Saisons tampons, mois de souffle, gel de contenu |
| R14 | Dépendance au niveau gratuit de Supabase | Moyen | Moyenne | Vérifier les quotas, surveiller, repli hors ligne |
| R15 | Triche sur les classements | Moyen | Élevée | Plafonds, comptes liés, pas de promesse d'intégrité totale |
| R16 | Modération des guildes | Élevé | Moyenne | Option « sans chat », conformité RGPD |
| R17 | Localisation en 5 langues à chaque mineure | Moyen | Élevée | Externaliser ou limiter les textes des mineures |
| R18 | Voix absentes (aucun fichier à ce jour) | Moyen | Élevée | Les sous-titres font foi ; chaque nouvel agent aggrave le retard |
| R19 | Glissement de la date de Hell Rising | Moyen | Moyenne | Fenêtre flottante et plan B (§5) |
| R20 | Spoilers croisés entre Hell Rising et Heroes Dawn | Moyen | Moyenne | Coordonner les calendriers de communication |

## 11. Questions ouvertes

- Quelle est la fin de Hell Rising, et quels événements faut-il faire résonner dans l'arc d'Eda ?
- Quelle date, même approximative, pour Hell Rising ? Elle fixe le M+24 réel.
- Lancement simultané Steam, Android et iOS, ou Early Access d'abord ?
- Le niveau gratuit de Supabase suffit-il pour le classement ?
- Quelle récompense cosmétique partagée avec Hell Rising, si elle existe ?
- Les cryptides de Salem comptent-ils bien parmi « tous les agents » disponibles à M0 ?

## Rappel — tournois à M0

Les tournois JcJ (bloc D de la 1.0.0) sont en place dès M0 : 8 tournois annuels, 10 duels par jour. Les arènes classées par ligues (M+6) et par équipes (M+18) s appuient sur le même serveur (`hd_arena_*`). **À mesurer avant le lancement** : quotas Realtime du palier gratuit (connexions simultanées) ; la migration `002_hd_arena.sql` reste à appliquer sur accord explicite.
