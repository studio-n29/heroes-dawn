# Modules du domaine

> Code C# pur, partagé client/serveur. Chaque module expose un **état** (classes internes, mutables uniquement par ses règles), des **règles** (fonctions ou systèmes sans effet de bord hors de l'état passé), des **événements** (records immuables) et une **API en lecture seule**.

## Core (fondations)
| Élément | Rôle |
|---|---|
| Ids typés | `readonly struct MemberId`, `SkillId`, `ZoneId`, `CaseId`, `EnemyId`… (évite de confondre deux `string`) |
| `Fixed` | Virgule fixe 32.32 (`long`) pour tout calcul non entier de la simulation |
| `IRandom` / `Pcg32` | Aléatoire déterministe à graine ; `Fork(stream)` pour des flux indépendants (dé, pools, combat) |
| `Result<T>` / `DomainError` | Échecs attendus (« PR insuffisants ») sans exception |
| `IDomainEvent` | Marqueur des événements du domaine |
| `DeterministicList<T>` | Liste ordonnée ; interdiction d'itérer un `Dictionary` pour décider d'un ordre |
| `GameTime` | Instant serveur (ms Unix, `long`) ; jamais `DateTime.Now` dans le domaine |

## Definitions (contenu immuable)
Records immuables construits depuis les ScriptableObjects (client) ou depuis un JSON de contenu (serveur).
| Définition | Champs principaux |
|---|---|
| `MemberDef` | Id, nom de code, famille, rôle, rareté, stats de base, 4 `SkillId`, ville |
| `SkillDef` | Id, slot (A1/A2/A3/P), recharge, ciblage, liste d'`EffectDef` |
| `EffectDef` | Type (Dégâts, Soin, Statut, Jauge, Bouclier…), valeur, durée, condition |
| `StatusDef` | Id, empilement (rafraîchit), durée par défaut, modificateurs |
| `ComboDef` | Paire (familles ou 2 `MemberId`), effets, priorité (duo > famille > résonance) |
| `EnemyDef` | Rôle, stats, kit, IA, phases (boss) |
| `BoardDef` / `ZoneDef` | Nœuds, liens, cases fixes, pools par case, 3 objectifs, entrée |
| `CasePoolDef` | Entrées pondérées (type de case + contenu) |
| `NarrativeEventDef` | Texte (clé de localisation), choix, conditions de famille, conséquences |
| `MiniGameDef` | Type, paramètres (fenêtres, durée, grilles) |
| `RoomDef`, `ExpeditionDef`, `PassSeasonDef`, `MissionDef`, `CosmeticDef` | Voir les specs correspondantes |
| `BalanceConfig` | Tous les paramètres du tableur (exportés vers Remote Config) |

## Modules métier

### Roster — les méta-humains du joueur
- **État** : `Member` (Id, niveau, XP, paliers validés, fatigue, affectation : libre / salon / infirmerie / expédition / équipe).
- **Règles** : `StatsCalculator` (base × rareté × croissance × malus de fatigue), `LevelingRules` (courbe d'XP, plafond par acte), `FatigueRules` (gain, récupération, malus), `PalierRules`.
- **API** : `IRosterReadModel` (liste, fiche, stats calculées).
- **Événements** : `MemberGainedXp`, `MemberLeveledUp`, `PalierReached`, `FatigueChanged`.

### Economy — portefeuille et coûts
- **État** : `Wallet` (Crédits, PR, Gemmes) — **copie locale** ; l'autorité est UGS Economy.
- **Règles** : `CostTable` (toutes les dépenses), `RewardBundle` (Crédits, PR, XP, objets, XP de pass), `CanAfford`.
- **Événements** : `CurrencyChanged`, `RewardGranted`.

### Recruitment — pool quotidienne
- **État** : `DailyPool` (5 offres + emplacement Légende, date, renouvellements du jour).
- **Règles** : `PoolGenerator` (tirage sans doublon parmi les recrues non possédées de la ville, graine du jour), `RefreshCostRules` (gratuit → Crédits → gemmes).
- **Événements** : `PoolGenerated`, `MemberRecruited`, `CityCollectionCompleted`.

> **État réel (boutique éphémère, 2026-10-03)** — assembly `Domain.Shop` (C# pur) : **3 offres** par jour de boutique (`ShopDay` : de 9h00 locale à 9h00 le lendemain), graine recalculable `ShopSeed` (joueur + jour + numéro de tirage → `Pcg32`), tirage `ShopDraw` (rareté pondérée Agent 70 / Élite 25 / Légende 5, renormalisée si une rareté est épuisée, puis héros uniforme, sans doublon ni héros possédé), condition d'entrée `MemberDef.UnlockZone` (Verre après la zone 1.1), coût du renouvellement `ShopRules` (gratuit puis 100 / 200 / 400 / 800 Crédits). Paramètres `Shop*` dans `BalanceConfig`. État `ShopState` dans la session (offres mémorisées, profil v3) ; cas d'usage `OpenShop`, `RecruitFromShop`, `RerollShop` ; message `ShopChanged`. Emplacement Légende dédié et renouvellement en gemmes : plus tard.

### Board — plateau d'une zone
- **État** : `ZoneRun` (position, cases révélées, contenu tiré des cases, objectifs remplis, jauge de PM, dés bonus, graine et curseur RNG, journal d'actions).
- **Règles** :
  - `DiceRules` : lancer 1d6, stockage des PM, conversion 6 PM → dé bonus (max 2).
  - `ReachabilityRules` / `MovementRules` : cases atteignables en 1..N PM (parcours en largeur ordonné), arrêts forcés (Combat, brouillard), interdiction de retour immédiat.
  - `FogRules` : rayon 2, fermeture/réouverture, retirage des pools (cases non fixes).
  - `CaseResolver` : ce que déclenche une case (ressource en passant, arrêt, combat…) → produit une `CaseOutcome`.
  - `ObjectiveRules` : validation des 3 objectifs, persistance.
- **API** : `IZoneRunReadModel` (graphe visible, destinations possibles, jauge).
- **Événements** : `DiceRolled`, `PawnMoved`, `FogRevealed`, `CaseTriggered`, `ObjectiveCompleted`, `ZoneValidated`.

### Combat — simulation ATB
- **État** : `CombatState` (unités, jauges en `Fixed`, recharges, statuts, tick courant, RNG).
- **Règles** : `AtbRules`, `DamageFormula`, `StatusRules`, `ComboRules`, `TargetingRules`, `EnemyAi` (par rang + phases), `AllyAutoAi`.
- **Simulation** : `CombatSimulation.Step(ticks)` avance le temps ; `Submit(CombatCommand)` enregistre une action du joueur à un tick donné ; `IsOver` / `Result`.
- **Sortie** : un flux ordonné de `CombatEvent` + `CombatResult` (victoire/défaite, PV restants, fatigue, XP).
- Détail : `05_Simulation_Deterministe.md`.

### Narrative — événements du plateau
- **Règles** : `NarrativeEventResolver` (choix disponibles selon l'équipe, application des conséquences, tirages pondérés), `CooldownRules` (pas de répétition avant 3 ouvertures).
- **Événements** : `NarrativeChoiceResolved`.

### MiniGames — validation des mini-jeux
- **Règles** : `InterventionRules` (probabilité affichée et tirage), `ScoreValidator` (Démonstration, Piratage, Poursuite : le client envoie le résultat ; le serveur vérifie la plausibilité : durée minimale, nombre d'actions).
- **Événements** : `MiniGameCompleted`.

### Campaign — progression d'une ville
- **État** : `CityProgress` (acte courant, zones validées, zones en difficile, Légendes débloquées).
- **Règles** : `UnlockRules` (zone suivante, difficile, Légende de l'acte), `LevelCapRules` (10 × acte), `RoomCapRules` (2 × acte).
- **Événements** : `ZoneUnlocked`, `ActCompleted`, `LegendUnlocked`, `CityCompleted`.

### Headquarters — QG
- **État** : `Room` (type, niveau), `TrainingSlot` (membre, début, objets), `InfirmarySlot`.
- **Règles** : `RoomEffects` (emplacements, bonus par niveau), `TrainingRules` (XP selon le temps écoulé), `InfirmaryRules`, `LabRules` (PR/h, stockage max).
- **Événements** : `RoomUpgraded`, `TrainingCompleted`, `LabCollected`.

### Expeditions — expéditions et objectifs secondaires
- **État** : `Expedition` (membres, zone, durée, début), `SideObjective` (zone, case, apparition, expiration).
- **Règles** : `ExpeditionRewards` (durée, bonus de famille, tirage d'objet), `SideObjectiveSpawner` (3 toutes les 5 h, max 9, expiration 24 h — calculé à partir de l'heure serveur).
- **Événements** : `ExpeditionReturned`, `SideObjectivesSpawned`.

### Pass — pass saisonnier, missions, succès
- **État** : `SeasonProgress` (XP, paliers réclamés gratuits/premium, premium acheté), `DailyMissions`, `Achievements`.
- **Règles** : `PassRules`, `MissionTracker` (écoute des faits de jeu), `AchievementRules`.
- **Événements** : `PassTierReached`, `MissionCompleted`, `AchievementUnlocked`.

### Cosmetics
- **État** : inventaire de cosmétiques, équipements (tenue par membre, thème par pièce, dé, pion).
- **Règles** : purement déclaratif (aucun effet en jeu).

## Frontières et agrégats
| Agrégat | Modifié par | Sauvegardé dans (Cloud Save) |
|---|---|---|
| Roster | Serveur (récompenses, entraînement), client (affectations d'équipe) | `roster` |
| ZoneRun | Client (simulation) puis serveur (points de contrôle) | `board_run` |
| CityProgress | Serveur | `campaign` |
| HQ + Expeditions | Serveur | `hq` |
| Pass | Serveur | `pass` |
| Wallet | **UGS Economy** (jamais Cloud Save) | — |

Règle : un use case ne modifie qu'**un agrégat** de façon autoritaire ; si plusieurs changent, c'est une opération serveur unique (voir `06_Backend_Donnees.md`).
