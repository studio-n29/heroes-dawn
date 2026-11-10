# Repaires, boss à phases et styles d'IA — Plan d'implémentation

> **Pour les agents :** sous-compétence requise : superpowers:executing-plans (exécution native) ou superpowers:subagent-driven-development. Cases à cocher (`- [ ]`).

**Objectif :** donner un style d'IA à chaque ennemi, des jeux de compétences et un style par phase aux boss, et des récompenses de Composants et de recettes aux boss de repaire.

**Architecture :** domaine (`Definitions`, `Domain/Combat`) pour les styles et les phases ; application (`Features/World/LairBook`, `FinishCombatUseCase`) pour les récompenses ; contenu prototype pour les trois boss existants. Aucune modification de prefab ni d'écran.

**Stack :** C# Unity 6000.6.3f1, NUnit Edit Mode.

**Spec :** `Code/docs/specs/2026-11-10-repaires-boss-ia-design.md`

## Contraintes globales

- Français (textes, commentaires, commits conventionnels) ; chaque test a `[Description("Vérifie que … — parce que …")]`.
- Tests en batch, Unity fermé (commande de `Code/SETUP.md`, §Tests), résumé dans `$TEMP/editmode.xml`.
- `git add` par chemin, jamais `-A` ni `.` ; pas de push.
- **Ordre :** après le plan A (`Role` devient `Position`) et le plan B (Composants, catalyseurs, `RecipeSource.Lair`), sur la même branche. Les tâches 1 à 4 ne dépendent que du plan A ; la tâche 5 dépend du plan B.
- Valeurs de la spec : seuils 60 % (Defensive) et 50 % (Support) ; Normal applique le style une fois sur deux ; 2 à 4 phases par boss ; récompenses pleines 1 fois par jour et par repaire, moitié ensuite.

## Revue ciblée

1. Un coup qui franchit deux seuils de phase (PV 70 % → 30 %) : les deux phases s'ouvrent dans l'ordre, le dernier jeu de compétences est celui de la phase 3, un seul coup d'IA par tour.
2. Un boss dont la nouvelle phase n'a pas de jeu de compétences (`Skills` nul) garde le précédent.
3. Un style qui ne trouve aucune compétence adaptée (Support sans soin, Control sans statut) retombe sur l'attaque la plus forte, jamais d'exception ni de tour perdu.
4. Un ennemi lié (`Bind`) ou sans énergie suffisante : l'IA choisit A1, comme avant.
5. Repaire rejoué le même jour, après minuit et sur une sauvegarde sans données de récompense : pas de double récompense, pas d'exception.

## Structure des fichiers

| Fichier | Rôle |
|---|---|
| `Definitions/AiStyle.cs` (nouveau) | `AiStyle`, `AiStyles.Default(Position)` |
| `Definitions/EnemyDef.cs`, `BossPhaseDef.cs` | `Style`, phases à jeu de compétences et style |
| `Domain/Combat/CombatUnit.cs` | `Skills` modifiable en interne, `Style` |
| `Domain/Combat/CombatSimulation.Ai.cs` + `CombatSimulation.AiStyle.cs` (nouveau) | choix par style |
| `Domain/Combat/CombatSimulation.Actions.cs`, `CombatEvents.cs` | ouverture de phase avec changement de kit |
| `Definitions/Lairs.cs` | `LairRewardDef`, récompenses par boss |
| `Application/Features/World/LairBook.cs`, `Features/Combat/FinishCombatUseCase.cs` | versement des récompenses et limite quotidienne |
| `Application/Ports/MetaSave.cs` | `lairRewardDay` (jour de la dernière récompense pleine par repaire) |
| `Content/Prototype/PrototypeContent.cs`, `PrototypeZones.cs` | styles, kits et phases de Brick, Crocodile, Titan du pont |
| `Assets/Tests/Domain/AiStyleTests.cs`, `BossPhaseKitTests.cs`, `Assets/Tests/Application/LairRewardTests.cs` | tests |

---

### Tâche 1 : Styles d'IA — définitions

**Fichiers :** créer `Definitions/AiStyle.cs`, `Assets/Tests/Domain/AiStyleTests.cs` ; modifier `EnemyDef.cs`, `PrototypeContent.cs`, `PrototypeZones.cs` (construction d'`EnemyDef`).

**Interfaces — produit :**
```csharp
public enum AiStyle { Balanced, Aggressive, Defensive, Control, Support }
public static class AiStyles { public static AiStyle Default(Position position); }
// EnemyDef : public AiStyle Style { get; }  — constructeur : ... bool isBoss = false, IReadOnlyList<BossPhaseDef>? phases = null, AiStyle? style = null ; nul → AiStyles.Default(position)
```

- [ ] **Étape 1 : tests.** `Le_style_par_defaut_suit_la_position` : Attacker et Ranged → Aggressive, FrontLine → Defensive, Controller → Control, Support → Support. `Un_style_saisi_prime_sur_la_position`. `Un_ennemi_du_contenu_prototype_a_un_style` (tous les ennemis ont un style défini, ce n'est pas `Balanced` par accident : `thug` Aggressive, `brute` Defensive).
- [ ] **Étape 2 : implémenter** le fichier et le champ ; styles explicites dans le contenu prototype : `thug` Aggressive, `brute` Defensive, `street_pyro` Aggressive, `amif_marksman` Aggressive, `amif_trooper` Balanced, `amif_captain` Control ; les boss (tâche 4) ; les autres ennemis gardent le style par défaut.
- [ ] **Étape 3 : tests Edit Mode** (suite complète). **Commit** : `feat(domaine): style d'IA des ennemis (Agressif, Défensif, Contrôle, Soutien, Équilibré)`.

---

### Tâche 2 : IA par style

**Fichiers :** créer `Domain/Combat/CombatSimulation.AiStyle.cs` ; modifier `CombatSimulation.Ai.cs`, `CombatUnit.cs` (`public AiStyle Style { get; set; }`), `UnitFactory.Enemy` (`unit.Style = d.Style`) ; tests dans `AiStyleTests.cs`.

**Interfaces — produit :**
```csharp
// CombatSimulation (partial)
(int Slot, int Target) ChooseStyledAction(CombatUnit u);   // appelé par ChooseAction pour un ennemi Normal (une fois sur deux), Élite ou Boss
```
Règles de choix (spec §1). « Compétence de dégâts la plus lourde » = celle dont la somme des effets `Damage` est la plus grande parmi les compétences utilisables (énergie suffisante, pas de `Bind` hors A1). « Bouclier ou provocation » = compétence avec un effet `Shield` ou le statut `Taunt` sur soi. « Statut de contrôle » = `Bind, Shock, Freeze, Mark, Vulnerable, Blind`. Soin ou renfort = effet `Heal` ou statut positif sur un allié.

- [ ] **Étape 1 : tests** (simulations à graine fixe, un ennemi seul contre 2 alliés) :
  - `Aggressive_joue_la_competence_de_degats_la_plus_lourde_sur_l_allie_le_plus_faible`.
  - `Defensive_se_protege_sous_60_pourcent_sinon_attaque_la_plus_forte_ATQ`.
  - `Control_pose_un_statut_sur_la_cible_la_plus_rapide_sans_statut_de_controle` ; `Control_attaque_quand_toutes_les_cibles_sont_deja_controlees`.
  - `Support_soigne_un_allie_ennemi_sous_50_pourcent_sinon_attaque`.
  - `Un_style_sans_competence_adaptee_attaque_avec_la_plus_forte` (Revue 3) ; `Un_ennemi_lie_joue_A1` (Revue 4).
  - `Facile_ignore_le_style` : 200 tours, au moins 3 compétences ou cibles différentes ; `Normal_applique_le_style_environ_une_fois_sur_deux` (1 000 choix, 50 ± 8 %) ; `Elite_et_Boss_appliquent_toujours_le_style`.
  - `Le_combat_reste_deterministe` (même graine = mêmes événements).
- [ ] **Étape 2 : implémenter** `ChooseStyledAction` ; dans `ChooseAction`, un ennemi Élite/Boss appelle `ChooseStyledAction` si `Style != Balanced` (sinon logique actuelle) ; Normal : `_rng.Chance(50)` d'abord pour choisir entre style et logique Normal actuelle ; les alliés gardent leur logique. Respecter le plafond de compétences déjà levé par le plan A (`ActiveSkillCount`).
- [ ] **Étape 3 : tests Edit Mode.** **Commit** : `feat(domaine): l'IA ennemie suit le style (priorités de compétence et de cible)`.

---

### Tâche 3 : Phases de boss avec jeu de compétences et style

**Fichiers :** modifier `BossPhaseDef.cs`, `CombatUnit.cs` (`Skills` : `{ get; private set; }` + méthode interne `SetKit(IReadOnlyList<SkillDef>)`), `CombatSimulation.Actions.cs` (`AdvanceBossPhase`), `CombatEvents.cs` (`BossPhaseChangedEvent.KitChanged`), `CombatPresenter.cs` (compilation seulement, le nouvel argument est facultatif) ; créer `Assets/Tests/Domain/BossPhaseKitTests.cs`.

**Interfaces — produit :**
```csharp
// BossPhaseDef : ctor (..., int spdPercent, IReadOnlyList<SkillDef>? skills = null, AiStyle? style = null) ; propriétés Skills (nul = inchangé), Style
// CombatUnit.SetKit(IReadOnlyList<SkillDef> skills)  // remplace Skills, remet les recharges à zéro
// BossPhaseChangedEvent(int t, int unit, int phase, int phaseCount, string nameKey, bool kitChanged = false) ; propriété KitChanged
```

- [ ] **Étape 1 : tests** (compléter `BossPhaseTests` existant sans le casser) :
  - `Une_phase_remplace_le_jeu_de_competences_et_le_style` : boss à 3 phases, l'unité passe de {A1, A2, A3} à {B1, B2} puis {C1, C2, C3}, `Style` suit.
  - `Les_recharges_sont_remises_a_zero_a_l_ouverture` : une compétence en recharge est utilisable après la phase.
  - `Une_phase_sans_competences_garde_le_kit_precedent` (Revue 2).
  - `Un_coup_qui_franchit_deux_seuils_ouvre_les_deux_phases_dans_l_ordre` (Revue 1) ; `KitChanged` vrai seulement quand le kit change.
  - `Le_combat_reste_deterministe`.
  - `Un_boss_a_au_plus_3_seuils_de_phase` : la construction d'`EnemyDef` d'un boss avec 4 seuils lève `ArgumentException` ; 1 à 3 seuils sont acceptés (2 à 4 phases, phase initiale comprise).
- [ ] **Étape 2 : implémenter.** `AdvanceBossPhase` : après les bonus de stats, `if (phase.Skills != null) t.SetKit(phase.Skills)`, `if (phase.Style.HasValue) t.Style = phase.Style.Value`, événement avec `KitChanged`. `EnemyDef` valide `Phases.Count <= 3` (donc 4 niveaux au total avec la phase initiale).
- [ ] **Étape 3 : tests Edit Mode.** **Commit** : `feat(domaine): les phases de boss changent de compétences et de style`.

---

### Tâche 4 : Kits et styles par phase des trois boss existants

**Fichiers :** `Content/Prototype/PrototypeContent.cs` (Brick), `PrototypeZones.cs` (Crocodile, Titan du pont), `Presentation/Common/Loc*.cs` (noms de compétences et de phases : clés `enemies.<id>.phase2/3` existent déjà ; ajouter les noms des nouvelles compétences) ; tests `PrototypeDomainTests` / `BossPhaseKitTests`.

Valeurs de départ ⚑ (kits à affiner au playtest ; chaque compétence utilise les aides `S`, `Dmg`, `St`, `Shield` déjà présentes) :
- **Brick** (style Defensive en phase 1) : phase 1 = kit actuel ; phase 2 (66 %) : Aggressive, remplace A3 par un coup de zone plus lourd (`Dmg(110)` sur tous + `Shock`) ; phase 3 (33 %) : Aggressive, A1 `Dmg(150)`, A2 `Dmg(190)` sur 1 cible avec `Vulnerable`.
- **Crocodile** : phase 1 Control (`Bind` / entrave sur la cible la plus rapide), phase 2 Aggressive (morsure `Dmg(160)` + `Corrosion`), 2 phases.
- **Titan du pont** (style Defensive) : phase 1 Defensive (provocation + bouclier), phase 2 (66 %) Balanced avec onde de choc de zone, phase 3 (33 %) Aggressive avec A2 `Dmg(200)`.

- [ ] **Étape 1 : tests.** `Chaque_boss_de_repaire_a_au_moins_2_phases_dont_une_change_de_kit` ; `Brick_change_de_style_entre_la_phase_1_et_la_phase_3` ; `Un_combat_complet_contre_chaque_boss_se_termine` (simulation avec équipe de départ, termine sans exception en moins de 400 tours).
- [ ] **Étape 2 : saisir le contenu** ci-dessus ; textes français et anglais des nouvelles compétences.
- [ ] **Étape 3 : simulation d'équilibrage.** Lancer le simulateur de l'éditeur (menu **Heroes Dawn ▸ Équilibrage ▸ Simulateur**, ou le test d'équilibrage existant s'il y en a un) sur le Repaire de l'AMIF avec l'équipe attendue au niveau 10 : noter le taux de victoire par boss. Cible 40 à 60 % ; sinon ajuster ATQ/DEF des phases (pas les structures) et noter les valeurs finales dans la spec.
- [ ] **Étape 4 : tests Edit Mode.** **Commit** : `feat(contenu): styles, phases et kits des boss du repaire de l'AMIF`.

---

### Tâche 5 : Récompenses de repaire (après le plan B)

**Fichiers :** modifier `Definitions/Lairs.cs`, `LairBook.cs`, `FinishCombatUseCase.cs`, `MetaSave.cs` ; créer `Assets/Tests/Application/LairRewardTests.cs`.

**Interfaces — produit :**
```csharp
public sealed class LairRewardDef {
    public ComponentKind? Kind { get; }                  // nul : type tiré au hasard
    public ComponentGrade Grade { get; }
    public int Count { get; }
    public int CatalystPercent { get; }                  // chance en %
    public string? RecipeId { get; }                     // recette donnée à la première victoire
}
// LairBossDef(string caseId, string enemy, LairRewardDef reward)
// LairBook : public LairGrant GrantBossReward(MetaSave meta, GameSession s, LairDef lair, LairBossDef boss, string today) ; LairGrant { int Components; CatalystKind? Catalyst; string? Recipe; bool Reduced }
// MetaSave : List<CounterSave> lairRewardDay   // id "<repaire>:<case>" → jour (AAAAMMJJ) de la dernière récompense pleine
```
Récompenses de départ ⚑ : Brick 6 Raffiné, 15 % catalyseur, recette `plated_coat` ; Crocodile 6 Raffiné, 20 %, recette `grapple_rig` ; Titan du pont 4 Raffiné + 1 Exceptionnel, 35 %, recette `aegis_plate`. Les recettes concernées passent à `RecipeSource.Lair`.

- [ ] **Étape 1 : tests.**
  - `La_premiere_victoire_du_jour_donne_la_recompense_complete_et_la_recette`.
  - `La_deuxieme_victoire_du_meme_jour_donne_la_moitie_et_pas_de_recette` (Composants arrondis à l'entier inférieur, au moins 1 ; chance de catalyseur divisée par 2).
  - `Un_nouveau_jour_redonne_la_recompense_complete` (sans la recette déjà connue).
  - `Une_sauvegarde_sans_donnees_de_recompense_est_acceptee` (Revue 5).
  - `Le_tirage_est_deterministe` (graine joueur + repaire + jour + compteur).
  - `Les_recettes_de_repaire_ne_tombent_pas_ailleurs` : `RollDrops` ne donne jamais une recette de source `Lair`.
- [ ] **Étape 2 : implémenter** et brancher à `FinishCombatUseCase` (là où `RecordBossDefeat` est appelé), en passant le jour du jeu (`MetaContext.Today`). `RollDrops` du plan B exclut les recettes `Lair` et `Tournament` des recettes inconnues tirables.
- [ ] **Étape 3 : tests Edit Mode.** **Commit** : `feat(repaires): récompenses de Composants et de recettes, limite quotidienne`.

---

### Tâche 6 : Documentation

**Fichiers :** `01_Game_Design/02_Combat.md` (IA par style, phases), `01_Game_Design/06_Kits_Competences.md` (ennemis), `01_Game_Design/04_Economie.md` (sources de Composants), `05_Production/Decisions.md` (`git add -p`), `Code/docs/Guide_Outils_de_design.md` si l'outil d'ennemis cite l'IA.

- [ ] **Étape 1 :** mettre à jour ces documents ; consigner dans la spec les taux de victoire de la tâche 4.
- [ ] **Étape 2 : vérifier** `git diff --stat` : aucun prefab, scène ni recette d'éditeur touché. **Commit** : `docs(conception): styles d'IA, phases de boss et repaires`.

---

## Hors plan
- Écrans : page Repaire, fiche de boss avec phases et nouveaux kits (lot UI).
- Contenu des repaires de Paris, Moscou et Salem (boss, zones, textes) : lot de contenu dédié avec le narratif ; le moteur de ce plan les accueille sans changement.
- Outil de level design : si les récompenses de repaire doivent y être éditables, à ajouter plus tard.

## Auto-relecture
- Couverture : §1 (tâches 1, 2), §2 (3, 4), §3 (5), §4 (4, étape 3), documentation (6).
- « 2 à 4 phases » (phase initiale comprise) se traduit en code par 1 à 3 seuils de PV ; la spec le précise.
- La tâche 5 dépend du plan B (`ComponentGrade`, `CatalystKind`, `RecipeSource.Lair`) : ne pas l'exécuter avant.
