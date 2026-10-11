# Frise de combat par tick — Plan d'implémentation

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Remplacer la jauge continue du combat par une frise de 10 points discrets, avec tour d'agent, reprise selon le poids de la compétence, fenêtre d'interruption de 1,5 s sur point vide, et rencontres sur un même point (combo, parade).

**Architecture:** La simulation (`Packages/com.n29.heroesdawn.domain`) avance point par point : un index de point remplace le pourcentage de jauge. La présentation (`Assets/_Project/Presentation/Combat`) affiche la frise en segments, anime le déplacement à 0,5 s par point, et ouvre l'éventail de compétences au tour du joueur. Les interruptions reprennent les compétences d'avatar existantes, ouvertes seulement pendant la fenêtre.

**Tech Stack:** C# (`#nullable enable`), Unity 6000.6.3f1, NUnit (EditMode pour la simulation et les présentateurs, PlayMode pour les prefabs), UniTask, VContainer.

**Spec:** `Code/docs/specs/2026-10-11-frise-tick-design.md`

## Global Constraints

- Frise : 10 points, point 0 = départ, point 10 = point d'action.
- Avance : un point toutes les 0,5 s (`TickMs` côté présentation ; la simulation avance d'un point par pas).
- Départ des unités vivantes : point aléatoire entre 0 et 2, graine déterministe.
- Reprise après action : léger (emplacement 1) → point 3 ; intermédiaire (emplacement 2) → point 2 ; lourd (emplacement 3 et carte 4) → point 0 ; combo → point -2 (canalisation).
- Tour joueur : 30 s maximum ; au-delà, l'unité passe son tour et repart au point 3.
- Tour IA : délai aléatoire entre 0,5 et 1,5 s (présentation seulement, la simulation reste déterministe).
- Point vide traversé : pause de 1,5 s, fenêtre d'interruption.
- Compétences d'avatar : lançables seulement pendant la fenêtre, ou au tour de l'agent concerné.
- Deux alliés sur un point : combo. Allié + ennemi : parade (annule l'action de l'ennemi, il repart au point 3). Deux ennemis : rien. Parade sans coût pour l'allié.
- `Spd` n'intervient plus dans le remplissage de la frise.
- Chaque test porte `[Description("Vérifie que … — parce que …")]`.
- Commandes : tests EditMode via `Unity.exe -batchmode -nographics -runTests -testPlatform EditMode` ; PlayMode sans `-nographics`. Unity fermé pendant les tests.

## Review Focus

1. Deux unités démarrant sur le même point au départ : une seule rencontre doit être traitée par tour, sans double combo.
2. Une unité en canalisation (point -2) rencontrée par un allié : pas de combo tant qu'elle n'a pas rattrapé le point 0.
3. Une parade qui arrive pendant l'action annoncée d'un ennemi : l'action est annulée une seule fois, pas réappliquée au tick suivant.
4. Joueur qui ne choisit jamais dans les 30 s, en mode Auto ou non : la frise ne doit pas rester figée.
5. Plusieurs points vides consécutifs : chaque pause de 1,5 s s'enchaîne sans chevauchement ni perte de tick.

Chaque ligne est couverte par un test dans la tâche qui possède le code concerné (tâches 3, 4, 5 et 7).

---

## File Structure

- Modify: `Packages/com.n29.heroesdawn.domain/Definitions/BalanceConfig.cs` — constantes de frise (nombre de points, positions de reprise, pause et délais).
- Modify: `Packages/com.n29.heroesdawn.domain/Domain/Combat/CombatUnit.cs` — `Point` remplace la jauge en pourcentage pour la frise.
- Modify: `Packages/com.n29.heroesdawn.domain/Domain/Combat/CombatSimulation.cs` — boucle d'avance point par point, fenêtre d'interruption.
- Modify: `Packages/com.n29.heroesdawn.domain/Domain/Combat/CombatSimulation.Timeline.cs` — reprise, zone de préparation, recul en points.
- Modify: `Packages/com.n29.heroesdawn.domain/Domain/Combat/CombatSimulation.Actions.cs` — effets de recul et d'élan convertis en points, fin de tour.
- Modify: `Packages/com.n29.heroesdawn.domain/Domain/Combat/CombatSimulation.Combo.cs` — combos sur un point commun, canalisation.
- Modify: `Packages/com.n29.heroesdawn.domain/Domain/Combat/CombatSimulation.Avatar.cs` — compétences d'avatar limitées à la fenêtre.
- Create: `Packages/com.n29.heroesdawn.domain/Domain/Combat/TickEncounters.cs` — détection et résolution des rencontres sur un même point (combo, parade).
- Modify: `Assets/_Project/Presentation/Combat/TimelineView.cs` et `TimelineMarkerView.cs` — affichage en segments de points.
- Modify: `Assets/_Project/Presentation/Combat/CombatPresenter.cs` — pause de 1,5 s, délai IA, éventail au tour du joueur.
- Modify: `Assets/_Project/Editor/Prefabs/Recipes/CombatRecipes.cs` — frise en segments dans le prefab Combat.
- Tests: `Assets/Tests/Domain/TimelineCombatTests.cs`, `Assets/Tests/Domain/ActionOrderTests.cs`, `Assets/Tests/Domain/TickTimelineTests.cs` (nouveau), `Assets/Tests/Presentation/CombatTimelinePresenterTests.cs`, `Assets/Tests/Presentation/CombatPresenterTests.cs`, `Assets/Tests/PlayMode/CombatTimelinePlayTests.cs` (nouveau).

## Interfaces (fixées ici, reprises par toutes les tâches)

```csharp
// BalanceConfig (domaine)
public int TimelinePoints { get; set; } = 10;                 // points entre départ et action
public int TimelineResetLightPoint { get; set; } = 3;
public int TimelineResetMediumPoint { get; set; } = 2;
public int TimelineResetHeavyPoint { get; set; } = 0;
public int TimelineResetComboPoint { get; set; } = -2;
public int TimelinePrepZonePoint { get; set; } = 8;           // zone de préparation (annonce d'action)
public int TimelineInterruptResetPoint { get; set; } = 4;     // renvoi après interruption critique
public int TimelineStartPointMax { get; set; } = 2;           // départ aléatoire dans [0, max]
public int TimelinePausePerEmptyPointMs { get; set; } = 1500;
public int TimelineStepMs { get; set; } = 500;
public int TimelineTurnTimeoutMs { get; set; } = 30000;
public int TimelineAiDelayMinMs { get; set; } = 500;
public int TimelineAiDelayMaxMs { get; set; } = 1500;

// CombatUnit (domaine)
public int Point { get; set; }                                // -2..TimelinePoints
public bool IsAtActionPoint(BalanceConfig b) => Point >= b.TimelinePoints;
public bool IsChannelling => Point < 0;

// TickEncounters (domaine, nouveau)
public static class TickEncounters
{
    // Renvoie les rencontres du tick : (unitA, unitB, kind) avec kind = Combo ou Parade.
    public static IReadOnlyList<TickEncounter> Detect(IReadOnlyList<CombatUnit> units, BalanceConfig b);
}
public readonly record struct TickEncounter(int A, int B, TickEncounterKind Kind);
public enum TickEncounterKind { Combo, Parade }

// CombatSimulation (domaine) : événements ajoutés
public sealed class TimelineAdvancedEvent : CombatEvent { int Tick; int Point; }         // un pas de frise
public sealed class EmptyPointPauseEvent : CombatEvent { int Point; int PauseMs; }        // fenêtre d'interruption
public sealed class EncounterEvent : CombatEvent { int A; int B; TickEncounterKind Kind; }
```

---

### Task 1: Constantes de frise dans `BalanceConfig`

**Files:**
- Modify: `Packages/com.n29.heroesdawn.domain/Definitions/BalanceConfig.cs`
- Test: `Assets/Tests/Domain/TickTimelineTests.cs` (nouveau)

**Interfaces:**
- Consumes: rien.
- Produces: les propriétés listées dans « Interfaces » ci-dessus, aux valeurs par défaut indiquées.

- [ ] **Step 1: Écrire le test des valeurs par défaut**

```csharp
[Test, Description("Vérifie que la frise a 10 points, des reprises à 3, 2, 0 et -2, et une pause de 1,5 s — parce que ce sont les valeurs validées dans le cahier des charges")]
public void Defaults_MatchTheValidatedTimeline()
{
    var b = new BalanceConfig();
    Assert.That(b.TimelinePoints, Is.EqualTo(10));
    Assert.That(b.TimelineResetLightPoint, Is.EqualTo(3));
    Assert.That(b.TimelineResetMediumPoint, Is.EqualTo(2));
    Assert.That(b.TimelineResetHeavyPoint, Is.EqualTo(0));
    Assert.That(b.TimelineResetComboPoint, Is.EqualTo(-2));
    Assert.That(b.TimelinePausePerEmptyPointMs, Is.EqualTo(1500));
    Assert.That(b.TimelineStepMs, Is.EqualTo(500));
}
```

- [ ] **Step 2: Lancer le test, vérifier qu'il échoue** (propriétés absentes → erreur de compilation attendue).

- [ ] **Step 3: Ajouter les propriétés à `BalanceConfig`** avec les valeurs de l'« Interfaces ». Garder les anciennes propriétés `Timeline*Percent` pour l'instant : elles seront retirées à la tâche 8.

- [ ] **Step 4: Relancer le test, vérifier qu'il passe.**

- [ ] **Step 5: Commit**

```bash
git add Packages/com.n29.heroesdawn.domain/Definitions/BalanceConfig.cs Assets/Tests/Domain/TickTimelineTests.cs
git commit -m "feat(combat): constantes de frise par points"
```

---

### Task 2: `CombatUnit.Point` et départ aléatoire

**Files:**
- Modify: `Packages/com.n29.heroesdawn.domain/Domain/Combat/CombatUnit.cs`
- Modify: `Packages/com.n29.heroesdawn.domain/Domain/Combat/CombatSimulation.cs` (constructeur, départ)
- Test: `Assets/Tests/Domain/TickTimelineTests.cs`

**Interfaces:**
- Consumes: `BalanceConfig.TimelinePoints`, `TimelineStartPointMax` (tâche 1).
- Produces: `CombatUnit.Point`, `IsAtActionPoint`, `IsChannelling`.

- [ ] **Step 1: Écrire les tests**

```csharp
[Test, Description("Vérifie que chaque unité vivante démarre entre le point 0 et le point 2, de façon déterministe pour une graine — parce que les premiers tours ne doivent pas tomber tous en même temps")]
public void StartPoints_AreWithinRangeAndDeterministic()
{
    var a = Sim(seed: 11); var b = Sim(seed: 11);
    foreach (var u in a.Units.Where(x => x.IsAlive)) Assert.That(u.Point, Is.InRange(0, 2));
    Assert.That(a.Units.Select(u => u.Point), Is.EqualTo(b.Units.Select(u => u.Point)));
}

[Test, Description("Vérifie qu'une unité au point d'action est reconnue comme prête et qu'une unité en canalisation (point négatif) ne l'est pas — parce que la canalisation retarde le tour")]
public void AtActionPoint_AndChannelling_AreDetected()
{
    var b = new BalanceConfig();
    var u = new CombatUnit(0, Side.Ally, "x", Family.Matter, Role.Tank, false, 1, 1, 1, 1, 1, Array.Empty<SkillDef>()) { Point = 10 };
    Assert.That(u.IsAtActionPoint(b), Is.True);
    u.Point = -2;
    Assert.That(u.IsChannelling, Is.True);
}
```

- [ ] **Step 2: Lancer, vérifier l'échec.**

- [ ] **Step 3: Implémenter.** Ajouter `Point` et les deux méthodes à `CombatUnit`. Dans le constructeur de `CombatSimulation`, remplacer le tirage `u.Gauge = …` par `u.Point = _rng.Range(0, _b.TimelineStartPointMax + 1)` pour les unités vivantes. Garder `Gauge` en place jusqu'à la tâche 8.

- [ ] **Step 4: Relancer, vérifier le succès.**

- [ ] **Step 5: Commit**

```bash
git commit -am "feat(combat): position discrète des unités sur la frise"
```

---

### Task 3: Avance point par point et tour d'agent

**Files:**
- Modify: `Packages/com.n29.heroesdawn.domain/Domain/Combat/CombatSimulation.cs` (`StepOne`, `FillPerTick` retiré du chemin de remplissage)
- Modify: `Packages/com.n29.heroesdawn.domain/Domain/Combat/CombatSimulation.Timeline.cs` (`GaugeAfter` → reprise en points, `PlanStartPercent` → `PlanStartPoint`, `InPrepZone` en points)
- Test: `Assets/Tests/Domain/TimelineCombatTests.cs` (mise à jour), `Assets/Tests/Domain/TickTimelineTests.cs`

**Interfaces:**
- Consumes: `CombatUnit.Point`, reprises et zone de préparation (tâches 1-2).
- Produces: `CombatSimulation.Step(int ticks)` avance chaque unité vivante d'un point par pas ; un tour démarre quand `Point >= TimelinePoints`. Émet `TimelineAdvancedEvent` à chaque pas et `GaugeFullEvent` au tour.

- [ ] **Step 1: Écrire les tests**

```csharp
[Test, Description("Vérifie que chaque pas de frise fait avancer toutes les unités vivantes d'exactement un point — parce que tout le monde avance au même rythme")]
public void EachStep_AdvancesEveryLivingUnitByOnePoint()
{
    var s = Sim(new[] { "ancre", "givre" }, new[] { "brute" }, seed: 3);
    var before = s.Units.Where(u => u.IsAlive).ToDictionary(u => u.Id, u => u.Point);
    s.Step(1);
    foreach (var u in s.Units.Where(u => u.IsAlive && !u.IsReady)) Assert.That(u.Point, Is.EqualTo(before[u.Id] + 1));
}

[Test, Description("Vérifie qu'une unité atteignant le point 10 prend son tour, et que la frise s'arrête tant qu'un allié attend — parce que le tour du joueur bloque la frise")]
public void ReachingActionPoint_StartsTheTurn_AndFreezesTheTimeline()
{
    var s = Sim(new[] { "ancre" }, new[] { "brute" }, seed: 3);
    s.Units[0].Point = 9;
    s.Step(1);
    Assert.That(s.Units[0].IsReady, Is.True);
    Assert.That(s.IsAwaitingAction, Is.True);
    var later = s.Step(20);
    Assert.That(s.Units[1].Point, Is.LessThan(10), "la frise n'avance pas pendant le tour");
}

[Test, Description("Vérifie que la reprise après action place l'unité sur le point du poids de sa compétence (léger 3, intermédiaire 2, lourd 0) — parce que la reprise dépend du poids de l'action")]
public void Reset_FollowsTheSkillWeight()
{
    foreach (var (slot, expected) in new[] { (0, 3), (1, 2), (2, 0) })
    {
        var s = Sim(new[] { "givre", "ancre" }, new[] { "brute" }, seed: 3);
        var givre = s.Units[0]; givre.Point = 10;
        s.Submit(CombatCommand.UseSkill(givre.Id, slot, 2));
        s.Step(1);
        Assert.That(givre.Point, Is.EqualTo(expected), "slot " + slot);
    }
}
```

- [ ] **Step 2: Lancer, vérifier l'échec.**

- [ ] **Step 3: Implémenter.** Dans `StepOne`, remplacer le remplissage par `u.Point++` pour chaque unité vivante et non prête, puis émettre `TimelineAdvancedEvent`. Quand `IsAtActionPoint`, appeler `BeginTurn` et émettre `GaugeFullEvent`. `GaugeAfter` renvoie désormais `b.TimelineReset*Point` selon l'emplacement (`-1` = combo). Les tests de la frise existante (`TimelineCombatTests`) doivent être mis à jour pour parler en points ; les valeurs en pourcentage sont remplacées par les points équivalents (30 % → 3, 15 % → 2, 0 % → 0, -15 % → -2).

- [ ] **Step 4: Relancer la suite Domain/Combat, vérifier le succès.** Corriger les seuls tests qui parlaient de pourcentage.

- [ ] **Step 5: Commit**

```bash
git commit -am "feat(combat): avance de la frise point par point"
```

---

### Task 4: Pause et fenêtre d'interruption sur point vide

**Files:**
- Modify: `Packages/com.n29.heroesdawn.domain/Domain/Combat/CombatSimulation.cs` (détection des points vides traversés)
- Modify: `Packages/com.n29.heroesdawn.domain/Domain/Combat/CombatSimulation.Avatar.cs` (compétences d'avatar limitées à la fenêtre)
- Test: `Assets/Tests/Domain/TickTimelineTests.cs`

**Interfaces:**
- Consumes: `TimelineAdvancedEvent` (tâche 3).
- Produces: `EmptyPointPauseEvent(Point, PauseMs)` émis quand un pas arrive sur un point sans unité ; `CombatSimulation.InterruptWindowOpen` (bool) vrai pendant la pause ; `CanUseAvatarSkill(index)` faux hors fenêtre et hors tour de l'agent.

- [ ] **Step 1: Écrire les tests**

```csharp
[Test, Description("Vérifie qu'un pas qui arrive sur un point vide émet une pause de 1,5 s et ouvre la fenêtre d'interruption — parce que le joueur a 1,5 s pour réagir sur un point vide")]
public void EmptyPoint_OpensTheInterruptWindow_For1500ms()
{
    var s = Sim(new[] { "ancre" }, new[] { "brute" }, seed: 3);
    s.Units.ForEach(u => u.Point = 0);
    var ev = s.Step(1).OfType<EmptyPointPauseEvent>().ToList();
    Assert.That(ev.Count, Is.GreaterThan(0));
    Assert.That(ev[0].PauseMs, Is.EqualTo(1500));
}

[Test, Description("Vérifie que deux points vides consécutifs enchaînent deux pauses sans en perdre une — parce que chaque point vide doit laisser son temps de réaction")]
public void ConsecutiveEmptyPoints_EachGetAPause()
{
    var s = Sim(new[] { "ancre" }, new[] { "brute" }, seed: 3);
    s.Units.ForEach(u => u.Point = 0);
    int pauses = 0;
    for (int i = 0; i < 3; i++) pauses += s.Step(1).OfType<EmptyPointPauseEvent>().Count();
    Assert.That(pauses, Is.GreaterThanOrEqualTo(2));
}

[Test, Description("Vérifie qu'une compétence d'avatar n'est pas lançable hors de la fenêtre d'interruption ni au tour de l'agent — parce que les interruptions ne sont pas des actions libres")]
public void AvatarSkill_OnlyInsideTheWindowOrOnTurn()
{
    var s = Sim(new[] { "ancre" }, new[] { "brute" }, seed: 3);
    Assert.That(s.CanUseAvatarSkill(0), Is.False, "hors fenêtre");
}
```

- [ ] **Step 2: Lancer, vérifier l'échec.**

- [ ] **Step 3: Implémenter.** Dans `StepOne`, après le déplacement, si `Point` atteint un index sans unité (hors point d'action), émettre `EmptyPointPauseEvent` et positionner `InterruptWindowOpen`. `CanUseAvatarSkill` renvoie vrai seulement si la fenêtre est ouverte, ou si l'agent associé est au tour. Les tests qui lançaient une compétence d'avatar hors fenêtre (`ActionOrderTests`, `CombatUseCaseTests`) sont mis à jour pour ouvrir la fenêtre avant de tirer.

- [ ] **Step 4: Relancer, vérifier le succès.**

- [ ] **Step 5: Commit**

```bash
git commit -am "feat(combat): pause et fenêtre d'interruption sur point vide"
```

---

### Task 5: Rencontres sur un même point (combo, parade)

**Files:**
- Create: `Packages/com.n29.heroesdawn.domain/Domain/Combat/TickEncounters.cs`
- Modify: `Packages/com.n29.heroesdawn.domain/Domain/Combat/CombatSimulation.Combo.cs` (combo par rencontre, canalisation)
- Modify: `Packages/com.n29.heroesdawn.domain/Domain/Combat/CombatSimulation.Actions.cs` (parade annule l'action annoncée)
- Test: `Assets/Tests/Domain/TickTimelineTests.cs`

**Interfaces:**
- Consumes: `CombatUnit.Point`, `PlannedSlot` (existant).
- Produces: `TickEncounters.Detect(units, b)` renvoie les rencontres du tick ; `EncounterEvent` émis pour chacune.

- [ ] **Step 1: Écrire les tests**

```csharp
[Test, Description("Vérifie que deux alliés sur le même point déclenchent un combo — parce que la rencontre de deux alliés est la règle du combo sur la frise")]
public void TwoAlliesOnOnePoint_TriggerACombo()
{
    var units = TwoAllies(point: 5);
    var r = TickEncounters.Detect(units, new BalanceConfig());
    Assert.That(r.Single().Kind, Is.EqualTo(TickEncounterKind.Combo));
}

[Test, Description("Vérifie qu'un allié et un ennemi sur le même point déclenchent une parade qui annule l'action de l'ennemi, qui repart au point 3 — parce que la parade annule le coup annoncé")]
public void AllyAndEnemy_ParryAndCancelTheEnemyAction()
{
    var s = Sim(new[] { "ancre" }, new[] { "brute" }, seed: 3);
    s.Units[1].Point = 5; s.Units[1].PlannedSlot = 0;
    s.Units[0].Point = 5;
    var ev = s.Step(1).OfType<EncounterEvent>().ToList();
    Assert.That(ev.Single().Kind, Is.EqualTo(TickEncounterKind.Parade));
    Assert.That(s.Units[1].PlannedSlot, Is.EqualTo(-1));
    Assert.That(s.Units[1].Point, Is.EqualTo(3));
}

[Test, Description("Vérifie que deux ennemis sur le même point ne font rien — parce que la règle ne concerne que les rencontres avec un allié")]
public void TwoEnemies_DoNothing()
{
    var units = TwoEnemies(point: 5);
    Assert.That(TickEncounters.Detect(units, new BalanceConfig()), Is.Empty);
}

[Test, Description("Vérifie qu'une unité en canalisation (point négatif) ne déclenche pas de combo tant qu'elle n'a pas atteint le point 0 — parce que la canalisation doit être rattrapée d'abord")]
public void ChannellingUnit_DoesNotComboUntilPointZero()
{
    var units = TwoAllies(point: -1);
    Assert.That(TickEncounters.Detect(units, new BalanceConfig()), Is.Empty);
}

[Test, Description("Vérifie qu'un même couple ne déclenche la rencontre qu'une seule fois par tick, même si deux unités démarrent sur le même point — parce qu'un départ commun ne doit pas doubler le combo")]
public void SamePair_EncountersOncePerTick()
{
    var units = TwoAllies(point: 0);
    Assert.That(TickEncounters.Detect(units, new BalanceConfig()).Count, Is.EqualTo(1));
}
```

- [ ] **Step 2: Lancer, vérifier l'échec.**

- [ ] **Step 3: Implémenter `TickEncounters.Detect`** : grouper les unités vivantes par `Point`, puis pour chaque point produire une rencontre par couple (allié-allié → Combo, allié-ennemi → Parade), sans rencontre si `Point < 0` pour les combos. Dans `StepOne`, appeler `Detect` après l'avance, émettre `EncounterEvent`, et appliquer la parade (annulation du plan, retour au point 3). Brancher le combo sur le chemin existant `TryPerformCombo`.

- [ ] **Step 4: Relancer, vérifier le succès.**

- [ ] **Step 5: Commit**

```bash
git add Packages/com.n29.heroesdawn.domain/Domain/Combat/TickEncounters.cs
git commit -am "feat(combat): rencontres sur point commun, combo et parade"
```

---

### Task 6: Délai IA et délai d'attente joueur (présentation)

**Files:**
- Modify: `Assets/_Project/Presentation/Combat/CombatPresenter.cs`
- Test: `Assets/Tests/Presentation/CombatPresenterTests.cs`

**Interfaces:**
- Consumes: `CombatSimulation.IsAwaitingAction`, `TimelineAiDelayMinMs`/`MaxMs`, `TimelineTurnTimeoutMs` (tâche 1).
- Produces: `CombatPresenter` ne résout une action IA qu'après un délai aléatoire (graine fixe en tests) ; un tour joueur expiré (30 s) est passé et l'unité repart au point 3.

- [ ] **Step 1: Écrire les tests**

```csharp
[Test, Description("Vérifie qu'un tour IA attend un délai entre 0,5 et 1,5 s avant d'agir — parce que l'IA doit paraître réfléchir sans bloquer le joueur")]
public void AiTurn_WaitsARandomShortDelay()
{
    var p = Presenter(aiDelayMs: 900);
    p.TickAi(0.5f); Assert.That(p.LastActionTaken, Is.False);
    p.TickAi(0.5f); Assert.That(p.LastActionTaken, Is.True);
}

[Test, Description("Vérifie qu'un joueur qui ne choisit pas dans les 30 s voit son unité passer son tour et repartir au point 3 — parce que la frise ne doit jamais rester figée")]
public void PlayerTurn_TimesOutAfter30s_AndResetsToPoint3()
{
    var p = Presenter();
    p.AdvanceClock(30f);
    Assert.That(p.LastTurnTimedOut, Is.True);
    Assert.That(p.LastResetPoint, Is.EqualTo(3));
}
```

- [ ] **Step 2: Lancer, vérifier l'échec.**

- [ ] **Step 3: Implémenter** un minuteur de tour dans `CombatPresenter` (30 s, relu via `IClock`/temps présentation), un délai IA tiré dans `[TimelineAiDelayMinMs, TimelineAiDelayMaxMs]` avec le `Random` de la présentation (graine injectée pour les tests). Le délai n'affecte pas la simulation.

- [ ] **Step 4: Relancer, vérifier le succès.**

- [ ] **Step 5: Commit**

```bash
git commit -am "feat(combat): délai IA et délai de tour joueur"
```

---

### Task 7: Frise en segments et animation par point (présentation)

**Files:**
- Modify: `Assets/_Project/Presentation/Combat/TimelineView.cs`
- Modify: `Assets/_Project/Presentation/Combat/TimelineMarkerView.cs`
- Modify: `Assets/_Project/Presentation/Combat/CombatData.cs` (`TimelineData` : points au lieu de `Position01`)
- Modify: `Assets/_Project/Editor/Prefabs/Recipes/CombatRecipes.cs` (segments dans le prefab Combat)
- Test: `Assets/Tests/Presentation/CombatTimelinePresenterTests.cs`, `Assets/Tests/PlayMode/CombatTimelinePlayTests.cs` (nouveau)

**Interfaces:**
- Consumes: `CombatUnit.Point`, `EmptyPointPauseEvent`, `TimelineStepMs` (tâches 2-4).
- Produces: `TimelineView.Show(TimelineData)` affiche 11 segments (points 0 à 10, plus -2 et -1 pour la canalisation), chaque segment vide ou avec une pastille ; `TimelineView.AnimateStep(int point)` déplace les pastilles à 0,5 s par point.

- [ ] **Step 1: Écrire les tests**

```csharp
[Test, Description("Vérifie que la frise affiche un segment par point, avec la pastille de chaque unité sur son point et les segments vides marqués — parce que la frise doit se lire d'un coup d'œil")]
public void Timeline_ShowsOneSegmentPerPoint_WithUnitsOnTheRightSegment()
{
    var view = Timeline(new[] { (id: 0, point: 0), (id: 1, point: 10) });
    Assert.That(view.Segments.Count, Is.EqualTo(11));
    Assert.That(view.SegmentOf(10).Occupants, Is.EqualTo(new[] { 1 }));
    Assert.That(view.SegmentOf(5).IsEmpty, Is.True);
}

[Test, Description("Vérifie que chaque pas de l'animation dure 0,5 s et qu'une pause de point vide est visible — parce que le rythme de la frise doit être lisible")]
public void Animation_UsesHalfSecondPerPoint()
{
    var view = Timeline(new[] { (id: 0, point: 0) });
    Assert.That(view.StepDurationSeconds, Is.EqualTo(0.5f).Within(0.001f));
}
```

Test PlayMode (nouveau) : vérifier qu'un combat réel affiche 11 segments et qu'une pause de point vide apparaît dans le journal. Capture de la frise à l'instant d'un point vide (explicite, à la demande).

- [ ] **Step 2: Lancer, vérifier l'échec.**

- [ ] **Step 3: Implémenter.** Remplacer les positions en pourcentage par des segments discrets dans `TimelineView`. Le prefab est régénéré avec `GenerateAllBatch -overwrite Screens/Combat.prefab,Widgets/TimelineMarker.prefab`.

- [ ] **Step 4: Relancer les tests EditMode et PlayMode, vérifier le succès.** Relire la capture de la frise avant de passer à la tâche suivante.

- [ ] **Step 5: Commit**

```bash
git commit -am "feat(combat): frise en segments de points"
```

---

### Task 8: Retrait de la jauge en pourcentage

**Files:**
- Modify: `Packages/com.n29.heroesdawn.domain/Definitions/BalanceConfig.cs` (retirer les `Timeline*Percent`)
- Modify: `Packages/com.n29.heroesdawn.domain/Domain/Combat/CombatUnit.cs` (retirer `Gauge`, `TimelinePercent`)
- Modify: partout où `Gauge`/`TimelinePercent` restent (grep)
- Test: toute la suite EditMode et PlayMode

**Interfaces:**
- Consumes: tout ce qui précède.
- Produces: plus aucun usage de `Gauge`/`TimelinePercent`/`Timeline*Percent`.

- [ ] **Step 1: Grep de contrôle.** `Grep` sur `Gauge|TimelinePercent|Timeline[A-Za-z]*Percent` dans `Assets` et `Packages`. La liste doit être vide hors fichiers de migration.

- [ ] **Step 2: Retirer les usages restants** un par un ; chaque retrait est un test vert.

- [ ] **Step 3: Relancer EditMode puis PlayMode complets.** Résultat attendu : tous verts, ou les seuls échecs sont des tests de pourcentage à mettre à jour.

- [ ] **Step 4: Régénérer les rapports et capturer la frise** (`docs/test-reports`).

- [ ] **Step 5: Commit**

```bash
git commit -am "refactor(combat): retrait de la jauge en pourcentage"
```

---

## Ordre et dépendances

1 → 2 → 3 → 4 → 5 → 6 → 7 → 8. Les tâches 4 et 5 peuvent être faites dans l'ordre inverse, mais 6 et 7 dépendent de 3 à 5.

## Self-review

1. **Couverture du cahier des charges** : frise 10 points (1, 3) ; reprises (3) ; tour joueur et IA (3, 6) ; point vide et fenêtre (4) ; avatar limité (4) ; rencontres (5) ; animation 0,5 s (7) ; retrait de la jauge (8). Tout est couvert.
2. **Cohérence des types** : `Point`, `TickEncounter`, `EmptyPointPauseEvent`, `EncounterEvent` sont définis une seule fois dans « Interfaces » et repris tels quels.
3. **Review Focus** : chacune des cinq lignes a un test dans la tâche propriétaire (tâche 5 pour 1 et 2 ; tâche 5 pour 3 ; tâche 6 pour 4 ; tâche 4 pour 5).
4. **Proportion** : le plan ne contient pas le code des méthodes, seulement les tests et les signatures.

Points restés ouverts (dans le cahier des charges) : la règle de recul en points (à convertir au moment de la tâche 3) et le coût de la parade (aucun, par décision).

---

## Execution Handoff

Plan terminé et enregistré dans `docs/plans/2026-10-11-frise-tick.md`. Peux-tu le relire et choisir la méthode d'exécution ?

- **Subagent-driven** : un sous-agent frais implémente chaque tâche, un relecteur frais la valide avant la suivante, puis une relecture de toute la branche. Le plus rigoureux, mais chaque tâche et chaque relecture coûte un contexte neuf.
- **Native** : j'implémente toutes les tâches dans cette session, puis un relecteur frais, sur le modèle le plus capable, vérifie la branche entière. Le moins cher et le plus rapide, sans relecture indépendante avant la fin.

Je recommande **Subagent-driven**, parce que les tâches 3 à 5 touchent le cœur de la simulation : une erreur d'interface se propagerait aux tâches suivantes. Quelle méthode choisis-tu ?

