# Liens entre agents : bonus de paire — plan d'implémentation (lot D 0.14.0)

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Donner à chaque paire d'agents des bonus propres, débloqués aux niveaux de lien 2 à 5 selon le couple de rôles, appliqués en combat aux deux agents de la paire quand ils sont dans l'escouade, et affichés (affinités, équipe, journal).

**Architecture:** Table de règles pure dans le domaine (`PairBonusRules`), branchée dans `UnitFactory.Ally` (comme les effets d'objets) via un total par agent calculé dans `CombatSimulation` ; présentation en lecture seule de la même table.

**Tech Stack:** Unity 6000.6.3f1, C# (nullable), NUnit, prefabs générés par recettes.

**Spec:** `Code/docs/specs/2026-10-27-liens-bonus-de-paire-design.md`

## Global Constraints
- Chemins relatifs à `Code/Heroes Dawn Unity/`. Domaine : `Packages/com.n29.heroesdawn.domain/`. Jeu : `Assets/_Project/`. Tests : `Assets/Tests/`.
- Niveaux de lien inchangés : `LinkRules` (3, 8, 15, 25 victoires), `CombatSetup.LinkLevel(MemberId, MemberId)` (1 par défaut). Aucun changement de sauvegarde.
- Plafonds par agent (`PairBonusTotal`) : `Hp, Atk, Def, Spd ≤ 12`, `Crit ≤ 15`, `OpeningShield ≤ 20`, `StartGauge ≤ 20`, `StartEnergy ≤ 20`, `EnergyPerStep ≤ 1`.
- La table des 15 couples de la spec fait foi (rôles `Role { Tank, Damage, Control, Support, Precision }`). Niveau 1 : aucun bonus.
- Chaque test porte `[Test, Description("Vérifie que … — parce que …")]`. Tests en batch, éditeur fermé (`Code/SETUP.md`).
- Commits en français, préfixe conventionnel, terminés par `Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>`.

## Review Focus
- **Un seul des deux agents de la paire dans l'escouade** : aucun bonus, pour aucun des deux. → test Task 2.
- **Une paire A+B enregistrée dans l'ordre inverse (B+A) dans `Links`** : même niveau, même bonus. → test Task 2.
- **Trois agents dont deux paires de niveau 5 sur le même agent** : le total de l'agent est plafonné, celui des deux autres ne l'est que par ses propres paires. → test Task 2.
- **Combat Auto avec des liens niveau 5 sur quatre agents** : se termine et se rejoue à l'identique. → test Task 2.
- **Profil sans aucun lien, ou avec un lien vers un agent retiré du jeu** : se charge, aucune erreur à l'affichage des affinités. → test Task 4.

---

### Task 1: Règles des bonus de paire (domaine)

**Files:**
- Create: `Packages/com.n29.heroesdawn.domain/Definitions/PairBonuses.cs`
- Test: `Assets/Tests/Domain/PairBonusRulesTests.cs`

**Interfaces:**
- Produces:
  - `enum PairBonusKind { HpPercent, AtkPercent, DefPercent, SpdPercent, CritPercent, OpeningShieldPercent, StartGaugePercent, StartEnergy, EnergyPerStep }`
  - `readonly struct PairBonus { PairBonusKind Kind; int Value; PairBonus(PairBonusKind kind, int value) }`
  - `readonly struct PairBonusTotal { int Hp, Atk, Def, Spd, Crit, OpeningShield, StartGauge, StartEnergy, EnergyPerStep; bool IsNone }` (valeurs déjà plafonnées)
  - `static class PairBonusRules { const int MinLevel = 2; const int MaxLevel = 5; PairBonus At(Role a, Role b, int level) /* 2..5, ordre des rôles indifférent */; IReadOnlyList<PairBonus> Of(Role a, Role b, int level) /* cumul 2..level, vide si level < 2, borné à 5 */; string ThemeKey(Role a, Role b) /* "link.theme.<r1>_<r2>" en minuscules, rôles dans l'ordre de l'enum */; PairBonusTotal Total(IEnumerable<PairBonus> bonuses) }`

- [ ] **Step 1: Write the failing tests** (`PairBonusRulesTests`) :
  - `Table_Has15Pairs_AndNothingAtLevel1` : pour chaque couple de rôles (a ≤ b dans l'ordre de l'enum), `Of(a, b, 1)` et `Of(a, b, 0)` vides ; `Of(a, b, 5).Count == 4` ; `Of(a, b, 3).Count == 2`.
  - `Table_MatchesTheSpec` : lignes de la spec : (Tank, Tank) niv. 2 = `DefPercent 4`, niv. 4 = `OpeningShieldPercent 8`, niv. 5 = `OpeningShieldPercent 7` ; (Damage, Precision) niv. 3 = `CritPercent 4`, niv. 5 = `AtkPercent 4` ; (Support, Support) niv. 5 = `EnergyPerStep 1` ; (Precision, Precision) niv. 5 = `StartGaugePercent 10` ; (Control, Control) niv. 4 = `StartGaugePercent 10`.
  - `OrderOfRoles_DoesNotMatter` : pour les 25 combinaisons ordonnées, `Of(a, b, 5)` est égal à `Of(b, a, 5)` (mêmes types et valeurs) et `ThemeKey(a, b) == ThemeKey(b, a)`.
  - `LevelAbove5_IsClampedTo5`.
  - `Total_SumsThenCaps` : douze bonus `HpPercent 4` → `Hp == 12` ; `CritPercent 4` × 5 → `Crit == 15` ; `OpeningShieldPercent 8` × 4 → 20 ; `StartGaugePercent 10` × 3 → 20 ; `StartEnergy 10` × 3 → 20 ; `EnergyPerStep 1` × 3 → 1 ; liste vide → `IsNone`.
- [ ] **Step 2: Run** `-testFilter N29.HeroesDawn.Tests.Domain.PairBonusRulesTests` — Expected: compilation en échec.
- [ ] **Step 3: Implement** les types et la table des 15 couples (spec, section « Table des 15 couples ») sous forme de tableau indexé par couple de rôles et par niveau ; `ThemeKey` renvoie les clés `link.theme.tank_tank`, `link.theme.tank_damage`, … (rôles en minuscules, séparés par `_`, premier = plus petit dans l'ordre de l'enum).
- [ ] **Step 4: Run** — Expected: PASS.
- [ ] **Step 5: Commit** — `feat(liens): règles des bonus de paire par couple de rôles`.

### Task 2: Bonus de paire en combat

**Files:**
- Modify: `Packages/com.n29.heroesdawn.domain/Domain/Combat/UnitFactory.cs` (`Ally`)
- Modify: `Packages/com.n29.heroesdawn.domain/Domain/Combat/CombatSimulation.cs` (constructeur : calcul du total de chaque allié)
- Test: `Assets/Tests/Domain/PairBonusCombatTests.cs`

**Interfaces:**
- Consumes: `PairBonusTotal`, `PairBonusRules.Of/Total`, `CombatSetup.LinkLevel`.
- Produces: `UnitFactory.Ally(int id, MemberDef d, AllyEntry e, BalanceConfig b, CollectionDef? collection = null, SynergyBonus synergy = default, PairBonusTotal pair = default)` (paramètre ajouté à la fin).

- [ ] **Step 1: Write the failing tests** (`PairBonusCombatTests`, même fabrique `Sim(string[] allies, params LinkEntry[] links)` que `LinkAndTeamEffectTests`, avec des agents de rôles connus du catalogue de test) :
  - `StatBonuses_GoToBothAgentsOfThePairOnly` : paire (tank, soutien) niveau 3 avec un troisième agent : PV des deux agents de la paire = base × (100 + 4) / 100 (± 2), DEF × 103 / 100 (selon la table : Tank + Soutien niv. 2 PV +4, niv. 3 DEF +3) ; le troisième agent inchangé.
  - `EachBrick_IsApplied` : une paire à niveau 5 pour chaque brique observable : critique (`CritBonusPercent`, couple Dégâts + Précision niveau 5 = 8 au total), bouclier d'ouverture (`Statuses` contient un `Shield` de `MaxHp × 8 / 100` au niveau 4 pour Tank + Tank), énergie de départ (`Energy` = énergie de départ + 10 au niveau 4 pour Dégâts + Soutien, plafonnée à `MaxEnergy`), jauge de départ (`StartPointBonus` > 0 au niveau 4 pour Tank + Contrôle), énergie par pas (`EnergyRegenBonus` +1 au niveau 5 pour Soutien + Soutien).
  - `NoBonus_IfOnlyOneAgentIsInTheSquad` : (Review Focus) lien niveau 5 enregistré mais un seul des deux agents engagé → unité identique à celle sans lien.
  - `LinkOrderInTheSetup_DoesNotMatter` : `LinkEntry(a, b, 5)` et `LinkEntry(b, a, 5)` donnent les mêmes PV.
  - `AnAgentInThreePairs_IsCapped` : quatre agents du même rôle Tank, tous liés deux à deux au niveau 5 → bouclier d'ouverture = 20 % des PV (plafond), pas 3 × 15.
  - `Level1_ChangesNothing` : avec `Links` vides ou tous au niveau 1, les unités ont exactement les mêmes PV, ATQ, DEF, VIT, énergie et statuts qu'avant (comparer à une simulation sans appel à `WithLinks`).
  - `SavedLinks_StillLoadAndKeepTheirLevel` (`Assets/Tests/Domain/Application/CollectionBookTests.cs` ou `LinkAndTeamEffectTests`) : un profil avec `MetaSave.links` (25 victoires entre deux agents) et un profil sans lien se chargent sans erreur par `ApplyProfile` ; le niveau de lien lu après chargement est 5 pour le premier et 1 pour le second.
  - `AutoCombatWithLevel5Links_EndsAndReplays` : (Review Focus) escouade de 4, toutes les paires au niveau 5, mode Auto, jusqu'à `IsOver`, puis `CombatReplay.Run(...).StateHash == sim.StateHash()`.
- [ ] **Step 2: Run** `-testFilter N29.HeroesDawn.Tests.Domain.PairBonusCombatTests` — Expected: FAIL.
- [ ] **Step 3: Implement** — dans `CombatSimulation`, avant la création des alliés : pour chaque allié i, `bonuses` = concaténation de `PairBonusRules.Of(roleI, roleJ, setup.LinkLevel(memberI, memberJ))` pour tout autre allié j, puis `PairBonusTotal pair = PairBonusRules.Total(bonuses)`, passé à `UnitFactory.Ally`. Dans `Ally` : `hpP += pair.Hp` etc. ; `crit += pair.Crit` ; `shield += pair.OpeningShield` ; `startGauge += pair.StartGauge` ; après la création : `unit.Energy = Math.Min(unit.MaxEnergy, unit.Energy + pair.StartEnergy)` et `unit.EnergyRegenBonus += pair.EnergyPerStep`.
- [ ] **Step 4: Run** `PairBonusCombatTests;LinkAndTeamEffectTests;TickTimelineTests` puis EditMode complet — Expected: PASS.
- [ ] **Step 5: Commit** — `feat(combat): bonus de paire appliqués aux deux agents liés`.

### Task 3: Textes des thèmes et des bonus

**Files:**
- Modify: `Assets/_Project/Presentation/Common/LocCombat.cs` (ou `Loc.cs` : à côté des textes de synergie)
- Modify: `Assets/_Project/Presentation/Mvp/ILocalization.cs`, `LocLocalization.cs` (`PairTheme(Role a, Role b)`, `PairBonusText(PairBonus b)`)
- Test: `Assets/Tests/Presentation/PairBonusTextTests.cs`

**Interfaces:**
- Produces: `ILocalization.PairTheme(Role a, Role b) : string` (nom du thème) ; `ILocalization.PairBonusText(PairBonus bonus) : string` (« PV +4 % », « Bouclier d'ouverture 8 % », « Jauge de départ +10 % », « Énergie de départ +10 », « Énergie +1 par pas », « Critique +4 % », « ATQ +4 % », « DEF +4 % », « VIT +3 % »).

- [ ] **Step 1: Write the failing tests** : chacun des 15 couples a un nom de thème non vide, différent de sa clé, et les 15 noms sont distincts (« Double rempart », « Rempart et lame », « Étau », « Veilleurs », « Garde et regard », « Double tranchant », « Marteau et étau », « Lame soutenue », « Tir croisé », « Tisseurs », « Sérénité », « Piège », « Cercle de soin », « Appui précis », « Double regard ») ; `PairBonusText` donne les 9 libellés ci-dessus pour une valeur donnée.
- [ ] **Step 2: Run** — Expected: FAIL.
- [ ] **Step 3: Implement** les 15 clés `link.theme.*`, les 9 libellés (`link.bonus.hp`, `.atk`, `.def`, `.spd`, `.crit`, `.shield`, `.gauge`, `.energy`, `.regen`) et les deux méthodes.
- [ ] **Step 4: Run** — Expected: PASS.
- [ ] **Step 5: Commit** — `feat(ui): noms des 15 thèmes de lien et libellés des bonus de paire`.

### Task 4: Fenêtre d'affinités

**Files:**
- Modify: `Assets/_Project/Presentation/Hub/Affinity/AffinityData.cs` (`AffinityRowData.Pair`)
- Modify: `Assets/_Project/Presentation/Hub/Affinity/AffinityFlow.cs` (`Row`)
- Modify: `Assets/_Project/Presentation/Hub/Affinity/AffinityRowView.cs` (champ `pairText`)
- Modify: `Assets/_Project/Editor/Prefabs/Recipes/AffinityRecipes.cs` (ligne plus haute, champ `pairText` câblé)
- Regenerate: `Widgets/AffinityRow.prefab`, `Popups/AffinityPopup.prefab`
- Modify: `Assets/_Project/Presentation/Common/LocCollection.cs` (ou `Loc.cs`) : `affinity.next_wins` = « encore {0} victoire(s) », `affinity.max` = « lien au maximum »
- Test: `Assets/Tests/Presentation/AffinityFlowTests.cs`

**Interfaces:**
- Consumes: `PairBonusRules.At/ThemeKey`, `ILocalization.PairTheme/PairBonusText`, `LinkRules.WinsToNext`.
- Produces: `AffinityRowData.Pair` (`string`, texte enrichi multi-ligne : thème, puis une ligne par niveau 2 à 5, « ✓ Niv. N · bonus » en `Theme.Success` si atteint, « Niv. N · bonus » en `Theme.TextMuted` sinon, puis « encore N victoire(s) » ou « lien au maximum »).

- [ ] **Step 1: Write the failing tests** (`AffinityFlowTests`) :
  - `Row_ShowsThePairThemeAndTiers` : Verre (Contrôle/…) et un agent de rôle connu avec 8 victoires communes (niveau 3) → `Pair` contient le nom du thème du couple de rôles, « ✓ Niv. 2 » et « ✓ Niv. 3 », contient « Niv. 4 » sans « ✓ » devant, et « encore 7 victoire(s) » (15 − 8).
  - `Row_AtMaxLevel_SaysSo` : 25 victoires → « lien au maximum », quatre « ✓ ».
  - `Row_WithNoLink_ListsAllTiersAsUpcoming` : 0 victoire → aucun « ✓ », « encore 3 victoire(s) ».
  - `Row_ForAnAgentRemovedFromTheGame_DoesNotThrow` : (Review Focus) un lien sauvegardé vers un identifiant inconnu n'apparaît pas et ne lève rien.
- [ ] **Step 2: Run** `-testFilter N29.HeroesDawn.Tests.Presentation.AffinityFlowTests` — Expected: FAIL.
- [ ] **Step 3: Implement** `Pair` dans `AffinityFlow.Row` (les 4 niveaux via `PairBonusRules.At`), le champ et l'affichage dans `AffinityRowView`, la recette (hauteur de ligne augmentée pour 6 lignes de texte ; `ctx.Wire(view, "pairText", pair)`), puis régénérer les deux prefabs : `-executeMethod N29.HeroesDawn.EditorTools.Prefabs.PrefabGenerator.GenerateAllBatch -quit -overwrite Widgets/AffinityRow.prefab,Popups/AffinityPopup.prefab` ; `git status` ne doit lister que ces deux prefabs côté `Prefabs/`.
- [ ] **Step 4: Run** `AffinityFlowTests;N29.HeroesDawn.Tests.Editor` (structure des prefabs) — Expected: PASS ; capture : lancer le test de capture qui ouvre la fenêtre d'affinités s'il existe (`ScreenCaptureTests.CaptureEveryScreen`) et regarder le rendu.
- [ ] **Step 5: Commit** — `feat(ui): thème, paliers et victoires restantes dans la fenêtre d'affinités`.

### Task 5: Écran Équipe et journal de combat

**Files:**
- Modify: `Assets/_Project/Presentation/Hub/Pages/Team/TeamPagePresenter.cs` (`Refresh` : lignes de liens ajoutées à `Synergies`)
- Modify: `Assets/_Project/Presentation/Combat/CombatPresenter.cs` (`LogSynergies` : une ligne par paire active)
- Modify: `Assets/_Project/Presentation/Common/LocCombat.cs` : `team.pair_line` = « {0} ({1} + {2}, niv. {3}) : {4} », `combat.pair` = « LIEN · {0} ({1} + {2}, niv. {3}) : {4} »
- Test: `Assets/Tests/Presentation/TeamPagePresenterTests.cs`, `Assets/Tests/Presentation/CombatPresenterTests.cs`

**Interfaces:**
- Consumes: `PairBonusRules.Of`, `ILocalization.PairTheme/PairBonusText`.

- [ ] **Step 1: Write the failing tests**
  - `Team_ListsActivePairBonuses` : escouade de deux agents liés au niveau 3 → `Synergies` contient une ligne avec le thème, les deux noms, « niv. 3 » et les deux bonus (« + » entre eux) ; au niveau 1 → aucune ligne de lien.
  - `Team_NoPairLine_ForPairsOutsideTheSquad` : lien niveau 5 avec un agent hors escouade → aucune ligne.
  - `Combat_LogsOneLinePerActivePair` : combat dont l'escouade contient une paire de niveau 2+ → une ligne du journal commence par « LIEN · » ; sans lien → aucune.
- [ ] **Step 2: Run** — Expected: FAIL.
- [ ] **Step 3: Implement** : dans les deux présentateurs, pour chaque paire d'agents de l'escouade (indices i < j), niveau via `LinkLevel`, si ≥ 2 : thème = `PairTheme(roleI, roleJ)`, bonus = `string.Join(", ", PairBonusRules.Of(...).Select(PairBonusText))`.
- [ ] **Step 4: Run** `TeamPagePresenterTests;CombatPresenterTests` puis EditMode complet — Expected: PASS.
- [ ] **Step 5: Commit** — `feat(ui): bonus de paire listés à l'écran Équipe et dans le journal de combat`.

### Task 6: Documentation et vérification finale

**Files:**
- Modify: `01_Game_Design/03_Metahumains.md` (section liens : bonus de paire par couple de rôles, table des 15 couples, plafonds)
- Modify: `05_Production/Decisions.md` (3 lignes datées 2026-10-27)

- [ ] **Step 1: Write the docs** — reprendre la table et les plafonds de la spec.
- [ ] **Step 2: Run** EditMode et PlayMode complets — Expected: 0 échec ; si un test PlayMode dépasse son délai alors qu'il passe seul, le relancer seul et noter la lenteur.
- [ ] **Step 3: Commit** — `docs: bonus de paire des liens entre agents` (avec les rapports `latest-*.html`).
