# Moscou, chapitre Sergueiv : rendre Agent M jouable (1.0.0) — design

> Statut : périmètre validé par l'utilisateur le 2026-11-13 (4 réponses, voir §2). Les points marqués **[mon choix]** sont des appels du rédacteur, à relire. Document produit **sans lancer Unity** (une autre session l'utilise) ; rien n'est implémenté. Plan associé : `Code/docs/plans/2026-11-13-moscou-agent-m.md`.
> Sources : `02_Lore_Narration/10_Quetes_Moscou.md`, `03_Histoire_et_Actes.md`, spec de Brooklyn `2026-10-12-campagne-brooklyn-design.md`, outil de level design `2026-10-10-outil-level-design-design.md`, décision H1 du 2026-11-12 (`05_Production/Decisions.md`).

## 1. But et pourquoi

La H1 exige que la 1.0.0 stable livre **Agent M jouable**. Aujourd'hui `StoryRoutes.M` a `Available = false` : Moscou n'a aucune zone, seul Brooklyn est jouable. Il faut donc créer le contenu qui rend le parcours d'Agent M **complet de bout en bout dans la version 1.0.0**, sans bouton mort et avec une fin propre.

**Objectifs**
- Un parcours Agent M jouable du choix du personnage jusqu'à la fin du contenu, au même niveau de finition que Brooklyn (4 zones, objectifs, ennemis, dialogues FR/EN, fragments, archives, carte).
- Boss équilibré dans la cible 40-60 % de victoires avec **le même simulateur** que Brooklyn.
- Aucun nouveau système : données, ennemis et textes uniquement, plus le branchement par parcours.

**Hors périmètre (non-objectifs)**
- Kapotnya, Sadovoye Koltso, Place Rouge (12 zones décrites, non livrées) ; Paris et Salem (H1).
- Repaire de Moscou, nouveau mini-jeu, équipe de départ propre à Moscou, voix (sous-titres seulement, comme Brooklyn).
- Illustrations de fond des zones et de la carte de Moscou (fond généré par l'outil, comme Brooklyn).
- Accès d'Agent D à Moscou (acte III, après Paris, hors périmètre).

**Anti-objectifs** : un parcours qui paraît « coupé » (pas de boss, pas de conclusion) ; un Moscou qui ressemble à du Brooklyn recoloré ; une régression du parcours Agent D.

## 2. Décisions validées (2026-11-13)

| # | Question | Réponse validée |
|---|---|---|
| D-M1 | Nombre et choix des zones | **4 zones : chapitre Sergueiv complet** (`mos_sergueiv`, zones 1.1 à 1.4), boss : le Passeur |
| D-M2 | Ennemis | **Mix** : boss (le Passeur) et élite neufs, 2 ennemis de base neufs, le reste du bestiaire réutilisé |
| D-M3 | Mini-jeux, fragments, repaire | **Mini-jeux existants réutilisés**, 4 fragments de souvenirs, **pas de repaire** à la 1.0.0 |
| D-M4 | Équipe et fin | **Équipe de départ partagée** (Giulia Rossi, Ilona Varga, Lucas Mendes) ; **fin « à suivre »** après le Passeur (Paris hors périmètre) ; mêmes zones et mêmes boss pour Agent D à l'acte III |

## 3. Ce qu'un parcours Agent M complet exige (état des lieux)

**Déjà en place (réutilisé tel quel)**
- `StoryRoutes.M` : actes Moscou, Paris, New York, Salem ; équipe de départ partagée ; musique `BoardMoscow`, fond de QG `hq-moscow`, fiche Agent M (`LocDawnAgents`, `LocLore`).
- Les 4 chapitres de Moscou existent (`Chapters.All`, `LocChapters`) avec titre et ouverture ; `Chapters.OfZone` rattache une zone à son chapitre par le quartier (`Borough`).
- Texte et scènes clés de Sergueiv dans `10_Quetes_Moscou.md` (lieux, enjeux, fin de chapitre).
- Mécanismes de zone : plateau, cases, objectifs, narration avant/après, rencontres par rang, fragments (`MemoryFragments`), archives (`archives.zone.*`), fil du QG (`qg.fil.*`), outil de level design et validateur.

**Manque (à produire)**
1. **Contenu** : 4 zones, 4 ennemis, dialogues, 4 fragments, textes d'archives/QG, carte de Moscou.
2. **Branchement par parcours** — trois défauts relevés à la lecture du code :
   - `StoryRoutes.CityOfBorough` renvoie **New York par défaut** : les quartiers `sergueiv`, `kapotnya`, `sadovoye`, `place_rouge` ne sont pas reconnus comme Moscou. Sans correction, `StoryProgress.ZonesOf(…, "moscow")` reste vide et l'acte de Moscou ne se termine jamais.
   - `ZoneProgress.StatusOf` **ignore le parcours** : la zone 1.1 de Brooklyn n'a pas de prérequis, donc elle serait « Disponible » pour Agent M, et `Recommended()` pourrait la proposer avant Moscou. Réciproquement, la zone de Moscou serait ouverte à Agent D dès le départ.
   - `CityMapLayout` ne sait dessiner que les quartiers de New York (le `default` de `CityMapLayout` et les `MapSpot` sont tous new-yorkais).
3. **Fin de contenu** : après le Passeur, `StoryProgress.Current` passe à l'acte II (Paris, sans zone). Le comportement existant pour Agent D après Brooklyn doit être vérifié et reproduit pour Agent M.
4. **Tests** : plusieurs tests affirment aujourd'hui l'inverse (`OnboardingTests.OnlyAgentDIsAvailable`, `StoryRouteTests.CurrentAct_OfAgentM_StartsInMoscow` « sans zone créée », statut « bientôt » de Sergueiv dans `ChaptersPageTests`/`ChapterBookTests`).
5. **Bascule** : `StoryRoutes.M` passe à `available: true` en dernier, après équilibrage et playtest.

## 4. Les quatre zones

Chapitre `mos_sergueiv`, quartier `sergueiv`, ville Moscou. Déblocage linéaire (1.2 demande 1.1, etc.). Identifiants `zone_mos_1_1` à `zone_mos_1_4`. Environnement : `Biome.City` ; météo jour, brume, nuit/neige selon la zone (réglable dans l'outil). Niveaux recommandés **[mon choix]** : mêmes valeurs que les zones de Brooklyn de même rang (équipe de départ identique), à confirmer en lisant `PrototypeContent` ; l'équilibrage final fait foi.

**[mon choix] Code d'affichage `M1.1` à `M1.4`** : le code « 1.1 » est déjà pris par Brooklyn et des tests/lectures cherchent des zones par code ; un préfixe évite toute collision. Le champ « acte » de `ZoneDef` vaut 1 (acte d'Agent M).

Les graphes reprennent ceux de Brooklyn (équilibrage déjà éprouvé, 19-20 cases, 2 embranchements) : **graphe A** = Brownsville (20 cases), **graphe B** = pont (19 cases, `BridgeNodes`). Positions et liens peuvent être ajustés dans l'outil de level design ; les identifiants de cases ci-dessous suivent ceux du code de Brooklyn.

| Zone | Lieu | Graphe | Enjeu | Objectifs (type) |
|---|---|---|---|---|
| `zone_mos_1_1` M1.1 | Gare du maglev | A | Traverser l'accueil officiel | 1 Franchir l'accueil (atteindre n03) ; 2 Se glisser derrière les quais (mini-jeu `infiltration_camp`, n13) ; 3 Neutraliser l'escorte du cargo (combat élite, n14) ; 4 Quitter la gare (atteindre n19) |
| `zone_mos_1_2` M1.2 | Monastère-archives | B | Fouiller les archives | 1 Entrer dans les archives (atteindre n04) ; 2 Faire pivoter la fresque (mini-jeu `gears_workshop`, n11) ; 3 Vaincre le gardien des archives (élite, n14) ; 4 Emporter le registre (atteindre n18) |
| `zone_mos_1_3` M1.3 | Voie du maglev | B | Poursuivre le convoi | 1 Rattraper le convoi (mini-jeu `rooftop_rescue`, n11) ; 2 Briser l'escorte du convoi (élite, n14) ; 3 Ouvrir les caisses (atteindre n18) |
| `zone_mos_1_4` M1.4 | Dépôt du maglev | B | Boss : le Passeur | 1 Fouiller le dépôt (mini-jeu `crate_pick_depot`, n11) ; 2 Briser la garde du dépôt (élite, n14) ; 3 Vaincre le Passeur (boss, n18) |

Compléments identiques à Brooklyn : un mini-jeu facultatif de soin `potion_field` sur n06 dans les graphes B ; cases Repos, Ressource, Narratif, Combat selon les graphes d'origine ; case de sortie fixe avec narration ; brouillard et visibilité gérés par le moteur.

**Rencontres par rang (table de repli de chaque zone)**

| Rang | M1.1 | M1.2 | M1.3 | M1.4 |
|---|---|---|---|---|
| Facile | 2 `mos_guard` ; `mos_smuggler` + `thug` | 2 `mos_guard` | `thug` + `mos_smuggler` | `mos_guard` + `thug` (voir note) |
| Normal | `brute` + `mos_smuggler` + `mos_guard` | `mos_guard` + `mos_smuggler` ×2 | `brute` + `mos_smuggler` + `street_pyro` | `mos_guard` + `mos_smuggler` + `brute` |
| Élite | `mos_enforcer` + `mos_guard` + `mos_smuggler` | `mos_enforcer` + `mos_smuggler` + `thug` | `mos_enforcer` + `brute` + `mos_smuggler` | `mos_enforcer` + `mos_guard` ×2 |
| Boss | — | — | — | `mos_passeur` + `mos_smuggler` + `mos_guard` |

*Note M1.4 Facile* : l'AMIF agit « dans l'ombre » à Moscou ; **aucun soldat de l'AMIF à l'écran avant le chapitre 4**, on garde donc la mafia seule.

**Récompenses** : barèmes de Brooklyn inchangés (crédits et PR des mini-jeux déjà fixés dans `PrototypeActivities`, récompenses de combat, objets/composants de zone) ; aucune nouvelle économie. Les fragments se gagnent en validant la zone (§6). Aucune quête de zone ajoutée sauf si `QuestLog.Main()` est dérivée des zones (à vérifier en tâche 1).

## 5. Ennemis (mix validé)

Quatre nouvelles définitions, toutes construites sur des kits existants (même écriture que `PrototypeZones.Enemies()`) :

| Id | Rôle | Famille / position | Point de départ (PV/ATQ/DEF/VIT) | Compétences |
|---|---|---|---|---|
| `mos_guard` | Base, garde | Matière, ligne de front | 1500 / 115 / 90 / 100 | A1 coup unique 100 % ; A2 (rech. 4) bouclier 15 % + provocation |
| `mos_smuggler` | Base, tireur | Perception, distance | 900 / 135 / 60 / 110 | A1 coup unique 105 % ; A2 (rech. 4) 160 % + Marque (2 tours) |
| `mos_enforcer` | **Élite** « homme de main » | Corps physique, attaquant | 2800 / 125 / 100 / 100 | A1 110 % ; A2 (rech. 3) zone 80 % + Choc 30 % ; A3 (rech. 5) bouclier 20 % |
| `mos_passeur` | **Boss** | Matière, attaquant, IA Contrôle | 5000 / 405 / 120 / 100 | A1 « Marchandage » 120 % + Marque ; A2 (rech. 3) « Cargaison scellée » bouclier 25 % + provocation ; A3 (rech. 5) « Livraison » zone 90 % + Corrosion 40 % ; A4 (rech. 4) « Mise en demeure » 75 % + Entrave 50 % |

Phases du Passeur (même mécanisme que Crocodile et le Titan) : sous 66 % « Les clients arrivent » (ATQ +10 %, VIT +5 %, A2 remplacée par un coup unique 150 % + Corrosion) ; sous 33 % « Tout doit partir » (ATQ +25 %, DEF −10 %, VIT +10 %). Les valeurs de départ sont une **hypothèse reprise du réglage final de Crocodile** : l'équilibrage (§8) les fixe. Noms et compétences : `enemies.<id>.name`, `skills.<id>_aN` (FR/EN).

Réutilisés sans changement : `thug`, `brute`, `street_pyro`. **Pas d'AMIF à l'écran avant la Place Rouge** (cohérent avec le lore : alliance dans l'ombre). Les descriptions de bestiaire (`archives.enemy.*`) sont ajoutées pour les quatre nouveaux ennemis.

## 6. Narration, fragments, archives

**Locuteurs** **[mon choix]** : `agent_m` (le personnage joueur, clé `speaker.agent_m`), `ilona_varga` (mentor d'Agent M, membre de l'équipe de départ), `passeur`, `enforcer` (« Homme de main », clé `speaker.enforcer`). Le test existant `EverySpeaker_HasADisplayName` impose un nom affichable pour chacun.

**Cases narratives** (même structure que Brooklyn : avant/après un combat, scène de fin de zone) : M1.1 élite n14 (avant, après) + sortie n19 ; M1.2 élite n14 + sortie n18 ; M1.3 élite n14 + sortie n18 ; M1.4 boss n18 (avant, après, fin de chapitre). Les répliques complètes FR/EN sont à l'**annexe A** (à relire par l'utilisateur).

**Cadrage selon le parcours** : le lore demande une phrase d'ouverture différente pour Agent D (étranger) et Agent M (qui connaît la ville). Comme Agent D ne joue pas Moscou à la 1.0.0, **seule la variante Agent M est écrite** ; la variante Agent D est un élément du lot « Paris/Moscou acte III ».

**Fragments de souvenirs** (`MemoryFragments.All`, chapitre `mos_sergueiv`), un par zone : `mos_maglev_welcome` (M1.1), `mos_monastery_register` (M1.2), `mos_convoy_crates` (M1.3), `mos_depot_passeur` (M1.4). Textes FR/EN à l'annexe B ; clé audio facultative (pas de voix à la 1.0.0).

**Archives et QG** : pour chaque zone, `archives.zone.zone_mos_1_x` (récit de fin) et `qg.fil.zone_mos_1_x` (fil du QG) ; `zones.mos_1_x.name/short/code/desc` ; objectifs `objectives.mos1x.*`. Le test `EveryTextOfTheCampaign_Exists` de Brooklyn est dupliqué pour Moscou.

**Fin du chapitre (D-M4)** : après le Passeur, l'acte I d'Agent M est terminé. Le jeu affiche l'état existant « acte suivant bientôt » (Paris sans zone). Le comportement actuel d'Agent D après Brooklyn est reproduit tel quel ; texte de transition à ajouter si l'écran existant parle de New York **[à vérifier en tâche 7]**.

## 7. Branchement par parcours (le vrai travail de code)

1. `StoryRoutes.CityOfBorough` : ajouter `sergueiv`, `kapotnya`, `sadovoye`, `place_rouge` → Moscou.
2. **[mon choix] Ouverture des zones selon l'acte courant** : une zone n'est ouverte que si sa ville est celle de l'acte courant du parcours (ou si elle est déjà validée). Conséquences : Agent M ne voit que Moscou jusqu'à la fin de l'acte I, puis rien (Paris sans zone) ; Agent D ne voit que New York ; le repaire de l'AMIF (zone de Brooklyn) est fermé à Agent M, ce qui est cohérent. Les profils Agent D existants ne changent pas. Implémentation : `ZoneProgress.StatusOf` appelle une version « brute » (`RawStatusOf`, prérequis seuls) pour éviter la récursion avec `StoryProgress.IsActDone`, qui l'utilise lui aussi.
3. Carte de ville : `CityMapLayout.For(city)` ; carte de Moscou avec 4 quartiers (Sergueiv jouable à 4 points, trois verrouillés « à venir ») ; `CityMapPagePresenter` choisit la carte selon la ville de l'acte courant.
4. `StoryRoutes.M` : `available: true` (bascule finale), mise à jour de `OnboardingGuide` et des textes `LocOnboarding` (suppression de l'état « indisponible » d'Agent M).
5. Vérifier, sans les modifier a priori : musique `BoardMoscow`, fond `hq-moscow`, `HubShellPresenter`, `QgRoomLocks`, `QuestLog`, `ArchiveBook`, `PlaytestFeedback` (références à des zones de Brooklyn).

## 8. Équilibrage (cible)

- Boss `mos_passeur` : **40 à 60 % de victoires** avec l'équipe de départ + objets, niveaux 5 et 10, mesuré avec **le même simulateur** que Brooklyn (`BalanceFullReport`, rapport `docs/test-reports/2026-11-13_equilibrage-boss.md`). Levier connu : l'ATQ ; les PV changent peu. Réglage multiplicateur sur l'ATQ de départ jusqu'à tomber dans la fenêtre.
- Élites : **60 à 85 %** **[mon choix]** — le défaut connu de l'élite AMIF (13 à 31 %) ne doit pas être reproduit.
- Zones normales : parcours sans mort sur le chemin court avec l'équipe de départ ; garde anti pay-to-win relancée (test existant).
- Les cartes équipées en auto restent exclues de la simulation (limite connue de la phase 4) ; le playtest les couvre.

## 9. Tests (noms de travail, détail dans le plan)

EditMode / domaine : `MoscowCampaignTests` (miroir de `BrooklynCampaignTests`), `StoryRouteTests` (Moscou ↔ quartiers, acte courant d'Agent M), `ZoneProgressRouteTests` (ouverture par acte), `MoscowEnemyTests`, `MemoryFragmentBookTests` (4 fragments), `CityMapLayoutTests` (carte de Moscou). Éditeur : `CampaignAssetsTests` et `LevelDesignToolTests` (validation des 4 plateaux), `MiniGamePlacementTests`. Présentation : `ChaptersPageTests`, `OnboardingTests` mis à jour. PlayMode : un parcours d'Agent M de la création au dernier boss (`MoscowFullRunTests`). Textes : couverture FR **et EN** de toutes les clés (`LocCoverageTests` ou équivalent) ; « Aube tactique » absent.

## 10. Approches envisagées

- **A (retenue) — chapitre Sergueiv complet, 4 zones, boss le Passeur.** Contenu déjà écrit, symétrique à Brooklyn, graphes éprouvés, un seul nouveau boss. Contre : le parcours s'arrête sur une fin ouverte vers Kapotnya.
- B — une zone par chapitre jusqu'au Conseiller (arc complet avec la révélation de la cathédrale) : meilleure fin narrative pour Agent M, mais chapitres incomplets, quatre boss à créer, texte et équilibrage ×2. Écartée pour le calendrier.
- C — 2 ou 3 zones de Sergueiv : moins cher mais pas de vrai boss ni de conclusion. Écartée.

## 11. Estimation et calendrier

Effort : **≈ 17 jours de travail** (voir plan), soit ~3,5 semaines pour une personne, plus 10 % de marge. C'est au-dessus de l'hypothèse « 2 à 3 semaines » de la H1 si l'on compte l'équilibrage et le playtest.

| Période | Contenu | Besoin d'Unity |
|---|---|---|
| 23-27/11 | Rédaction des textes FR/EN, fragments, tables de données (tâches 4 et 5 en partie) | Non |
| 30/11-04/12 | Branchement par parcours + ennemis (tâches 1, 2) | Oui |
| 07-18/12 | Zones, textes branchés, carte de Moscou (tâches 3, 4, 5, 6) | Oui |
| 04-08/01 | Validation des plateaux + équilibrage du Passeur (tâches 7, 8) | Oui |
| 11-15/01 | Playtest et corrections ; **gel du contenu le 15/01** (tâche 9) | Oui |
| 18-22/01 | Relecture EN, bascule `available`, notes de version, **rc1 le 22/01** (tâche 10) | Oui |

Pas de semaine de marge avant la rc1 (la semaine 22-29/01 sert aux correctifs bloquants). Le calendrier suppose une pause du 24/12 au 01/01.

## 12. Risques

1. **Unity partagé** : une seule instance ; l'interface (phase 2, 23/11-18/12) occupe Unity en même temps. Atténuation : texte et données d'abord hors Unity ; réserver des créneaux d'éditeur ; se coordonner avec la session UI.
2. **Équilibrage** : un boss tombe facilement dans 0 % ou 100 % ; l'élite est un point faible connu. Atténuation : tâche 8 de 3 jours, deux niveaux de test.
3. **Branchement par parcours** : le changement de `StatusOf` touche la progression de tous les profils. Atténuation : tests de non-régression Agent D, bascule `available` à la fin.
4. **Textes EN** : relecture de l'utilisateur nécessaire avant le 15/01 (voir plan phase 5 : « Vous : relire »).
5. **Fin « à suivre »** perçue comme un jeu inachevé. Atténuation : le chapitre se termine sur la scène du Passeur ; l'écran d'après est l'écran standard « acte suivant bientôt ».
6. **Équipe de 1-2 personnes (R11)** : si la phase 2 déborde, le contenu de Moscou est le premier candidat à réduire (3 zones + élite en finale) — décision à reprendre avec l'utilisateur, hors de ce document.

## Annexe A — Répliques (FR / EN)

Clés : `narration.mos_1_X.nYY.before|after.i`. Locuteur entre crochets. Textes de départ, à relire.

**M1.1 — Gare du maglev**
- n14 avant 0 [enforcer] : « Bienvenue à Moscou. Le comité d'accueil vous prie de rebrousser chemin. » / "Welcome to Moscow. The welcoming committee kindly asks you to turn back."
- n14 avant 1 [ilona_varga] : « Un accueil trop parfait cache toujours quelque chose. » / "A welcome that perfect is always hiding something."
- n14 après 0 [agent_m] : « Ce cargo scellé n'est sur aucun manifeste. Je vois les camions qui l'ont déposé… trois jours avant nous. » / "This sealed cargo is on no manifest. I can see the trucks that left it here… three days before we arrived."
- n19 après 0 [ilona_varga] : « On sort par les archives du monastère. Si quelqu'un nous y attend, ce n'est pas pour nous souhaiter la bienvenue. » / "We leave through the monastery archives. If someone's waiting there, it isn't to welcome us."

**M1.2 — Monastère-archives**
- n14 avant 0 [enforcer] : « Les archives sont fermées au public. » / "The archives are closed to the public."
- n14 avant 1 [agent_m] : « Pas pour la restauratrice de la fresque. » / "Not for the woman restoring the fresco."
- n14 après 0 [agent_m] : « Derrière la fresque, un registre. Je l'avais vu avant de le toucher. » / "Behind the fresco, a ledger. I saw it before I touched it."
- n18 après 0 [ilona_varga] : « Un registre, la moitié d'une preuve. L'autre moitié est du côté des raffineries. » / "A ledger: half a proof. The other half is out by the refineries."

**M1.3 — Voie du maglev**
- n14 avant 0 [enforcer] : « Ce convoi n'existe pas. Vous n'avez rien vu. » / "This convoy doesn't exist. You saw nothing."
- n14 après 0 [agent_m] : « Ces caisses portent un code soviétique. Quelqu'un déterre de vieilles recherches. » / "These crates carry a Soviet code. Someone is digging up old research."
- n18 après 0 [ilona_varga] : « Un code d'avant l'Union sur des caisses de cette année. Retiens-le. » / "A pre-Union code on crates from this year. Remember it."

**M1.4 — Dépôt du maglev (boss)**
- n18 avant 0 [passeur] : « Vous êtes en avance. D'ordinaire mes clients prennent rendez-vous. » / "You're early. My clients usually make an appointment."
- n18 avant 1 [passeur] : « Je ne dis pas victimes. Je ne dis pas adversaires. Je dis : clients. » / "I don't say victims. I don't say adversaries. I say: customers."
- n18 avant 2 [ilona_varga] : « N'écoute pas. Il compte déjà ce qu'il peut vendre de nous. » / "Don't listen. He's already counting what he can sell of us."
- n18 après 0 [passeur] : « Vous croyez avoir trouvé quelque chose ? On vous a laissé trouver. L'accueil, c'est moi qui l'ai organisé. » / "You think you found something? We let you find it. I arranged the welcome."
- n18 après 1 [agent_m] : « Alors c'était pour nous tenir loin de Kapotnya. » / "So that was to keep us away from Kapotnya."
- n18 après 2 [ilona_varga] : « Alors c'est là que nous irons. » / "Then that is where we go."

## Annexe B — Fragments de souvenirs (FR / EN)

- `mos_maglev_welcome` — « L'accueil parfait » / "The Perfect Welcome" : « Les hologrammes saluaient l'Aube par son nom, les quais étaient balayés, les sourires, parfaits. Un cargo scellé dormait pourtant sur la voie 9, sans nom et sans manifeste. Moscou t'ouvrait grand les bras, et regardait ailleurs. » / "Holograms greeted The Dawn by name, the platforms were swept, the smiles flawless. Yet a sealed cargo sat on track 9, with no name and no manifest. Moscow opened its arms wide, and looked the other way."
- `mos_monastery_register` — « Le registre sous la fresque » / "The Ledger Under the Fresco" : « Tu as restauré mille fresques sans jamais chercher ce qu'elles cachaient. Celle-ci couvrait un registre : des noms, des dates, des livraisons. Quelqu'un avait peint par-dessus une vérité, et l'avait confiée aux saints. » / "You have restored a thousand frescoes without ever looking for what they hid. This one covered a ledger: names, dates, deliveries. Someone had painted over the truth and entrusted it to the saints."
- `mos_convoy_crates` — « Des caisses marquées » / "Marked Crates" : « Le convoi filait sous la ville, éclairé comme une fête. Les caisses portaient un code que personne n'utilise plus. Il y avait des mains neuves sur de très vieilles recherches. » / "The convoy raced beneath the city, lit like a festival. The crates bore a code nobody uses anymore. New hands on very old research."
- `mos_depot_passeur` — « Le Passeur » / "The Smuggler" : « Il ne disait jamais victime, jamais ennemi : seulement client. Dans son dépôt, tout avait un prix, même l'accueil qu'on t'avait réservé. Ce soir, tu as compris que la ville entière avait été invitée à regarder ailleurs. » / "He never said victim, never enemy: only customer. In his depot everything had a price, even the welcome prepared for you. Tonight you understood the whole city had been invited to look elsewhere."
