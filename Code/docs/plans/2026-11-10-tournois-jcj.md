# Tournois JcJ — Plan d'implémentation

> **Pour les agents :** sous-compétence requise : superpowers:executing-plans (exécution native) ou superpowers:subagent-driven-development. Cases à cocher (`- [ ]`).

**Objectif :** remplacer le tournoi simulé par de vrais duels (hors ligne contre la capture d'une équipe, en ligne en temps réel), avec calendrier annuel, points, classement et récompenses.

**Architecture :** règles pures dans le domaine (calendrier, points, paliers), extension de la simulation de combat pour qu'une équipe de joueur occupe le camp adverse, service d'application `TournamentService` derrière deux ports (`IArenaGateway` pour les données serveur, `IDuelChannel` pour le temps réel) avec implémentations en mémoire pour les tests et Supabase pour la production.

**Stack :** C# Unity 6000.6.3f1, NUnit Edit Mode, Supabase (REST, Realtime), SQL.

**Spec :** `Code/docs/specs/2026-11-10-tournois-jcj-design.md`

## Contraintes globales

- Français (textes, commentaires, commits conventionnels) ; chaque test a `[Description("Vérifie que … — parce que …")]`.
- Tests en batch, Unity fermé (commande de `Code/SETUP.md`, §Tests), résumé dans `$TEMP/editmode.xml`.
- `git add` par chemin, jamais `-A` ni `.` ; pas de push.
- **Aucune migration SQL n'est exécutée** sur Supabase sans accord explicite de l'utilisateur (tâche 6, étape 3 : s'arrêter et demander). Aucune clé `service_role` dans le dépôt ni dans le client. Tables préfixées `hd_`.
- **Ordre :** après les plans A et B, sur la même branche. Aucune modification de prefab ni d'écran ; `TournamentPage*` ne reçoit que des corrections de compilation.
- Valeurs de la spec : +10 / +20 / 0 / −10 ; 10 duels par jour et par tournoi ; file d'attente 15 s ; passer son tour 30 s ; adversaire muet 45 s ; empreinte tous les 20 ticks ; réclamation 7 jours ; paliers 50 / 150 / 300 / 500 / 800.

## Revue ciblée

1. Tournoi à cheval sur deux années (29/12 → 10/01) : actif le 31/12 et le 02/01, pas le 11/01 ni le 28/12.
2. Score à 0 et départ en ligne : reste à 0, jamais négatif.
3. Aucune capture d'adversaire disponible (début de saison, joueur seul) : le duel hors ligne propose une équipe de repli générée (jamais d'exception ni de duel bloqué).
4. Déconnexion pendant la file d'attente ou à l'instant du début : retour à l'écran du tournoi sans pénalité.
5. Plafond de 10 duels : le 11e est refusé avant d'engager le combat ; changement de jour (minuit) : compteur remis à zéro.
6. Même joueur sur deux appareils : ne s'affronte jamais lui-même ; la capture la plus récente est celle utilisée.
7. Réclamation faite deux fois ou après 7 jours : refusée, sans double récompense.

## Structure des fichiers

| Fichier | Rôle |
|---|---|
| `Definitions/TournamentDef.cs` (nouveau) | `TournamentDef`, `TournamentCalendar` |
| `Resources/Content/tournaments.json` (nouveau) | les 8 tournois |
| `Definitions/ArenaRules.cs` (nouveau) | points, plafonds, paliers, récompenses |
| `Domain/Combat/CombatSetup.cs`, `UnitFactory.cs`, `CombatSimulation*.cs` | camp adverse contrôlé par un joueur ou par l'IA, `StateHash` |
| `Application/Ports/IArenaGateway.cs`, `IDuelChannel.cs` (nouveaux) | ports serveur et temps réel |
| `Infrastructure/Simulated/InMemoryArenaGateway.cs`, `InMemoryDuelChannel.cs` (nouveaux) | faux serveur pour tests et éditeur |
| `Application/Features/Meta/TournamentService.cs` (nouveau, remplace `Tournament.cs`) | flux d'un duel, points, réclamation |
| `Application/Ports/MetaSave.cs` | `ArenaSave` remplace `TournamentSave` |
| `Infrastructure/Supabase/SupabaseArenaGateway.cs`, `SupabaseDuelChannel.cs` (nouveaux) | production |
| `Backend/Supabase/002_hd_arena.sql` (nouveau) | tables, RLS, `hd_submit_duel` |
| `Assets/Tests/Domain/TournamentCalendarTests.cs`, `ArenaRulesTests.cs`, `DuelSimulationTests.cs`, `Assets/Tests/Application/TournamentServiceTests.cs`, `Assets/Tests/Infrastructure/SupabaseArenaGatewayTests.cs` | tests |

---

### Tâche 1 : Calendrier des tournois

**Fichiers :** créer `Definitions/TournamentDef.cs`, `Resources/Content/tournaments.json`, `Assets/Tests/Domain/TournamentCalendarTests.cs`.

**Interfaces — produit :**
```csharp
public sealed class TournamentDef { public string Id; public string NameKey; public (int Month,int Day) Start; public (int Month,int Day) End; public string RewardsId; }
public static class TournamentCalendar {
    public static TournamentDef? ActiveOn(IReadOnlyList<TournamentDef> all, DateTime day);   // jour inclus aux deux bornes, gère le passage d'année
    public static TournamentDef Next(IReadOnlyList<TournamentDef> all, DateTime day);          // le prochain à démarrer, y compris l'an prochain
    public static (DateTime Start, DateTime End) Window(TournamentDef t, DateTime reference);  // dates réelles de l'édition qui contient ou suit reference
}
```

- [ ] **Étape 1 : tests.** `Nouvel_An_est_actif_le_31_12_et_le_02_01` ; `Nouvel_An_n_est_pas_actif_le_28_12_ni_le_11_01` ; `Noel_est_actif_du_01_12_au_26_12_inclus` ; `Aucun_tournoi_le_27_12` (entre Noël et Nouvel An) ; `Next_le_27_12_est_Nouvel_An` ; `Next_le_11_01_est_Saint_Valentin` ; `Window_de_Nouvel_An_le_02_01_commence_l_annee_precedente` ; `Le_fichier_JSON_contient_8_tournois_sans_chevauchement`.
- [ ] **Étape 2 : implémenter**, avec le chargement du JSON par la même mécanique que les autres contenus (`ContentData` / `Resources.Load` : suivre `Resources/Content/items.json`).
- [ ] **Étape 3 : tests Edit Mode.** **Commit** : `feat(domaine): calendrier annuel des tournois`.

---

### Tâche 2 : Points, plafonds et paliers

**Fichiers :** créer `Definitions/ArenaRules.cs`, `Assets/Tests/Domain/ArenaRulesTests.cs`.

**Interfaces — produit :**
```csharp
public enum DuelMode { Offline, Online }
public enum DuelOutcome { Win, Loss, Quit, Cancelled }
public static class ArenaRules {
    public const int WinOffline = 10, WinOnline = 20, QuitOnlinePenalty = 10, DailyDuelCap = 10, QueueSeconds = 15, PassSeconds = 30, SilenceSeconds = 45, ClaimDays = 7;
    public static readonly IReadOnlyList<int> TierPoints = new[] { 50, 150, 300, 500, 800 };
    public static int PointsDelta(DuelMode mode, DuelOutcome outcome, int currentPoints);  // jamais de score < 0
    public static bool CountsAsDuel(DuelMode mode, DuelOutcome outcome);                   // faux : Cancelled et Quit hors ligne
    public static int TierReached(int points);                                              // 0 à 5
    public static bool CanStartDuel(int duelsToday);                                        // duelsToday < DailyDuelCap
}
```

- [ ] **Étape 1 : tests.** `Victoire_hors_ligne_donne_10_en_ligne_20` ; `Defaite_ne_retire_rien` ; `Quitter_en_ligne_retire_10_sans_descendre_sous_0` (score 4 → 0, score 25 → 15) ; `Quitter_hors_ligne_ne_change_rien_et_ne_compte_pas` ; `Annule_ne_change_rien_et_ne_compte_pas` ; `Le_11e_duel_du_jour_est_refuse` ; `Les_paliers_sont_50_150_300_500_800` (`TierReached(49)=0`, `50`→1, `800`→5).
- [ ] **Étape 2 : implémenter.** **Étape 3 : tests Edit Mode.** **Commit** : `feat(domaine): points, plafond quotidien et paliers de tournoi`.

---

### Tâche 3 : Simulation de duel (camp adverse piloté par un joueur)

**Fichiers :** modifier `Domain/Combat/CombatSetup.cs`, `UnitFactory.cs`, `CombatSimulation.cs`, `CombatSimulation.Ai.cs`, `CombatSimulation.Timeline.cs`, `CombatEvents.cs` ; créer `Assets/Tests/Domain/DuelSimulationTests.cs`.

Point de départ : aujourd'hui `IsAwaitingAction` (`CombatSimulation.cs:109`) ne regarde que le camp `Ally` et les ennemis sont toujours menés par `AiAct`. Repérer toutes les dépendances au camp avec `grep -n "Side.Ally\|Side.Enemy" Packages/com.n29.heroesdawn.domain/Domain/Combat/*.cs` (notamment `PlanStartPoint` pour la prescience des agents Perception, `CombatSimulation.End.cs` pour la victoire).

**Interfaces — produit :**
```csharp
// CombatSetup
public IReadOnlyList<AllyEntry> OpponentTeam { get; private set; }        // vide : combat classique
public SideControl EnemyControl { get; private set; }                      // Ai (par défaut) ou External
public CombatSetup WithOpponentTeam(IReadOnlyList<AllyEntry> team, SideControl control);
public enum SideControl { Ai, External }
// UnitFactory
public static CombatUnit Opponent(int id, AllyEntry entry, MemberDef def, CollectionDef? collection, BalanceConfig b);   // même calcul que Ally, camp Side.Enemy
// CombatSimulation
public Side? AwaitingSide { get; }                      // camp dont une unité est prête et attend une commande (selon SideControl), nul sinon
public IReadOnlyList<CombatEvent> Submit(CombatCommand c, Side from);   // refuse une commande d'un camp qui n'a pas la main
public ulong StateHash();                               // FNV-1a sur : tick, pour chaque unité (id, PV, énergie, point de frise, statuts : type, tours, valeur)
```
Règles : un camp contrôlé par l'IA (équipe adverse hors ligne) joue avec `AiAct` (les agents ont une position, pas de style : style par défaut `AiStyles.Default(position)` du bloc C, `Balanced` sinon) ; un camp `External` attend `Submit`. Les `Pass` après 30 s sont soumis par l'appareil du joueur concerné (pas par la simulation). La victoire se calcule comme avant (camp sans unité vivante).

- [ ] **Étape 1 : tests.**
  - `Un_duel_hors_ligne_est_deterministe` : même graine, mêmes équipes = mêmes événements ; l'équipe adverse est contrôlée par l'IA.
  - `L_equipe_adverse_a_les_stats_d_un_agent_et_non_d_un_ennemi` : PV et ATQ égaux à ceux de `UnitFactory.Ally` pour la même entrée.
  - `Les_cartes_et_objets_adverses_comptent` (compétences et bonus de position/famille du bloc A appliqués à l'adversaire).
  - `En_mode_External_la_simulation_attend_la_commande_du_camp_Enemy` : `AwaitingSide == Side.Enemy`, `Submit` depuis `Side.Ally` est refusé, depuis `Side.Enemy` avance.
  - `Deux_simulations_identiques_ont_la_meme_empreinte_a_chaque_tick_de_controle` (avec les mêmes commandes) ; `Une_commande_differente_change_l_empreinte`.
  - `Le_combat_se_termine_avec_un_vainqueur` (équipes de niveau égal, IA contre IA, moins de 400 tours).
  - `Les_combats_classiques_ne_changent_pas` : un combat existant (`CombatSimulationTests`) donne exactement les mêmes événements qu'avant.
- [ ] **Étape 2 : implémenter** en gardant le chemin classique intact (aucune branche si `OpponentTeam` est vide).
- [ ] **Étape 3 : tests Edit Mode.** **Commit** : `feat(domaine): duel entre deux équipes de joueurs (camp adverse IA ou externe, empreinte d'état)`.

---

### Tâche 4 : Ports et faux serveur

**Fichiers :** créer `Application/Ports/IArenaGateway.cs`, `IDuelChannel.cs`, `Infrastructure/Simulated/InMemoryArenaGateway.cs`, `InMemoryDuelChannel.cs` ; tests dans `TournamentServiceTests.cs` (tâche 5).

**Interfaces — produit :**
```csharp
public sealed class DefenseSnapshot { public string PlayerId; public string Name; public int Rating; public List<AllyEntrySave> Team; public List<LinkEntrySave> Links; public DateTime UpdatedUtc; }
public sealed class ScoreRow { public int Rank; public string PlayerId; public string Name; public int Points; public bool IsPlayer; }
public sealed class DuelTicket { public string MatchId; public ulong Seed; public DefenseSnapshot Opponent; }
public interface IArenaGateway {
    UniTask<Result> UploadDefenseAsync(DefenseSnapshot snapshot, CancellationToken ct);
    UniTask<Result<DuelTicket>> FindOfflineOpponentAsync(string tournamentId, int rating, CancellationToken ct);   // jamais soi-même ; repli généré par le serveur
    UniTask<Result<int>> SubmitDuelAsync(string tournamentId, string matchId, DuelMode mode, DuelOutcome outcome, CancellationToken ct);   // renvoie les points totaux
    UniTask<Result<IReadOnlyList<ScoreRow>>> LeaderboardAsync(string tournamentId, int top, CancellationToken ct);
    UniTask<Result> ClaimAsync(string tournamentId, int tier, CancellationToken ct);   // refuse hors délai ou déjà réclamé
}
public interface IDuelChannel {
    UniTask<DuelTicket?> JoinQueueAsync(string tournamentId, DefenseSnapshot me, TimeSpan timeout, CancellationToken ct);   // nul : personne
    UniTask SendAsync(DuelMessage m, CancellationToken ct);                                                                  // Command, Hash, Leave
    UniTask<DuelMessage?> ReceiveAsync(TimeSpan timeout, CancellationToken ct);                                              // nul : silence
    UniTask LeaveAsync(CancellationToken ct);
}
public sealed class DuelMessage { public DuelMessageType Type; public int Tick; public CombatCommand? Command; public ulong Hash; }
```
Les implémentations en mémoire simulent des joueurs, des captures et des délais déterministes (horloge injectée).

- [ ] **Étape 1 :** écrire les interfaces et les implémentations en mémoire. **Étape 2 : commit** : `feat(application): ports d'arène et faux serveur en mémoire`.

---

### Tâche 5 : Service de tournoi

**Fichiers :** créer `Application/Features/Meta/TournamentService.cs` ; supprimer `Tournament.cs` ; modifier `MetaSave.cs` (`ArenaSave { string tournamentId; int duelsToday; string duelDay; int points; List<int> claimedTiers; List<string> cosmetics; DateTime lastDefenseUploadUtc; }` remplace `TournamentSave`), `RootLifetimeScope.cs`, `MetaOverview.cs`, `TournamentPage.cs` (compilation seule) ; créer `Assets/Tests/Application/TournamentServiceTests.cs` ; supprimer ou réécrire les tests de l'ancien tournoi.

**Interfaces — produit :**
```csharp
public sealed class TournamentService {
    public TournamentView Current();                                                  // tournoi actif ou prochain, points, palier, duels restants
    public UniTask<Result<DuelSession>> StartDuelAsync(DuelMode preferred, CancellationToken ct);   // refuse hors tournoi, hors connexion, au-delà du plafond
    public UniTask<Result<DuelReport>> FinishDuelAsync(DuelSession s, DuelOutcome outcome, CancellationToken ct);
    public UniTask<Result> ClaimTierAsync(int tier, CancellationToken ct);
    public UniTask UploadDefenseIfNeededAsync(CancellationToken ct);                  // à la connexion et à chaque changement d'équipe
}
public sealed class DuelSession { public string MatchId; public DuelMode Mode; public CombatSetup Setup; public bool IAmHost; }
```
Flux en ligne : `JoinQueueAsync` (15 s) ; si un adversaire est trouvé, hôte = identifiant de joueur le plus petit ; chaque appareil construit le même `CombatSetup` (hôte en `Ally`, invité en `Enemy`, `SideControl.External`) ; échange des commandes ; empreinte envoyée tous les 20 ticks ; différence → `Cancelled` ; silence de 45 s → l'adversaire est déclaré parti (victoire pour celui qui reste, sans pénalité pour lui). Sinon repli : `FindOfflineOpponentAsync`.

- [ ] **Étape 1 : tests** (faux serveur et faux canal) :
  - `Un_duel_hors_ligne_gagne_donne_10_points_et_compte_un_duel`.
  - `Un_duel_en_ligne_gagne_donne_20_points`.
  - `Sans_adversaire_apres_15_secondes_le_duel_passe_hors_ligne`.
  - `Quitter_en_ligne_fait_perdre_10_points_sans_descendre_sous_0` ; `Quitter_hors_ligne_ne_compte_pas`.
  - `Une_desynchronisation_annule_le_duel_sans_points`.
  - `Un_adversaire_muet_45_secondes_est_declare_parti`.
  - `Le_11e_duel_du_jour_est_refuse_avant_le_combat` ; `Minuit_remet_le_compteur_a_zero` (Revue 5).
  - `Hors_tournoi_ou_hors_connexion_le_duel_est_refuse` (messages d'erreur distincts).
  - `Sans_capture_disponible_une_equipe_de_repli_est_proposee` (Revue 3).
  - `Le_meme_joueur_ne_s_affronte_jamais` (Revue 6).
  - `Reclamer_deux_fois_ou_apres_7_jours_est_refuse` ; `Reclamer_un_palier_donne_les_recompenses_une_fois` (Crédits, Composants, cosmétiques ajoutés).
  - `La_defense_est_envoyee_a_la_connexion_et_au_changement_d_equipe`.
- [ ] **Étape 2 : implémenter**, avec `IClock` pour le temps et `MetaContext.Today` pour le jour.
- [ ] **Étape 3 : récompenses.** Fichier `Resources/Content/arena_rewards.json` : par palier, `{credits, components:[{kind?, grade, count}], cosmetic}` ; top 10 % et top 3 (recette de source `Tournament` du bloc B, catalyseurs, cosmétiques exclusifs). Valeurs de départ ⚑ à écrire dans le fichier et à relire par l'utilisateur.
- [ ] **Étape 4 : tests Edit Mode.** **Commit** : `feat(application): service de tournoi JcJ (hors ligne, en ligne, points, récompenses)`.

---

### Tâche 6 : Passerelles Supabase et script SQL

**Fichiers :** créer `Infrastructure/Supabase/SupabaseArenaGateway.cs`, `SupabaseDuelChannel.cs`, `Backend/Supabase/002_hd_arena.sql`, `Assets/Tests/Infrastructure/SupabaseArenaGatewayTests.cs` ; modifier `RootLifetimeScope.cs` (choix de l'implémentation selon `supabase.json`, comme pour le compte).

- [ ] **Étape 1 : tests** avec le faux client HTTP existant (`ISupabaseHttp`, voir les tests de `SupabaseAccountGateway`) : `UploadDefense_envoie_un_upsert_sur_hd_arena_defenses` ; `SubmitDuel_appelle_la_fonction_rpc_hd_submit_duel_avec_le_mode_et_le_resultat` ; `Leaderboard_lit_le_top_et_marque_le_joueur` ; `Une_reponse_401_rafraichit_le_jeton_puis_reessaie` ; `Les_erreurs_reseau_donnent_une_erreur_localisee`.
- [ ] **Étape 2 : écrire** les passerelles et `002_hd_arena.sql` (spec §5 : trois tables, contraintes de taille de `snapshot` ≤ 64 Ko, index sur `(tournament_id, points desc, reached_at)`, fonction `hd_submit_duel` en `security definer` avec `search_path` fixé, RLS activée sur les trois tables, aucune écriture directe sur `hd_arena_scores`). Le canal temps réel utilise Supabase Realtime (broadcast sur `arena:<tournoi>:<fenêtre de rang>`), derrière l'interface `IDuelChannel`.
- [ ] **Étape 3 : S'ARRÊTER ICI et demander à l'utilisateur** de relire `002_hd_arena.sql` : le plan ne l'applique pas. Après son accord explicite seulement, l'appliquer avec l'outil Supabase (`apply_migration`) sur le projet N29 HQ, puis lancer `get_advisors` (sécurité) et corriger les alertes.
- [ ] **Étape 4 : tests Edit Mode.** **Commit** : `feat(infrastructure): passerelles Supabase d'arène et script SQL (non appliqué)`.

---

### Tâche 7 : Documentation et vérification

**Fichiers :** `01_Game_Design/04_Economie.md` ou `01_Game_Design/07_*.md` (section Tournois, selon l'existant), `00_GDD/GDD_Heroes_Dawn.md`, `05_Production/Decisions.md` (`git add -p`), `05_Production/Live_Ops_Plan_24_mois.md` (rappel : tournois en place à M0, quotas Realtime), `Code/docs/specs/2026-10-28-compte-supabase-design.md` (renvoi vers `002_hd_arena.sql`).

- [ ] **Étape 1 :** mettre à jour ces documents. **Étape 2 :** lancer toute la suite Edit Mode, aucun échec nouveau. **Étape 3 :** `git diff --stat` ne montre aucun prefab ni scène. **Commit** : `docs(conception): tournois JcJ`.

---

## Hors plan
- Écrans : page Tournoi (calendrier, file d'attente, duel en ligne, résumé, classement, réclamation), indicateur de connexion de l'adversaire (lot UI).
- Application de la migration SQL (sur accord) et quotas Realtime du palier gratuit à mesurer avant le lancement.
- Catalogue des cosmétiques : bloc E.

## Auto-relecture
- Couverture : §1 (tâche 1), §2 (3, 5), §3 (2, 5), §4 (5), §5 (6), §6 (5), documentation (7).
- La tâche 3 modifie le cœur de la simulation : son premier test garde les combats existants identiques ; ne pas la fusionner sans que toute la suite passe.
- Risque identifié : les agents d'une équipe adverse n'ont pas de style d'IA propre (position seulement) ; acceptable pour 1.0.0.
