# Prototype M1 — Plan d'implémentation · Partie 1 : squelette, domaine, application

> **Pour les agents :** SOUS-COMPÉTENCE REQUISE : `superpowers:subagent-driven-development` (recommandé) ou `superpowers:executing-plans` pour exécuter ce plan tâche par tâche. Les étapes utilisent des cases à cocher (`- [ ]`).
> **Partie 2** (composition, interface, outils) : `2026-09-29-prototype-M1-partie-2-interface.md`.

**Objectif :** une boucle jouable « plateau → combat → plateau » sur la zone 1.1 (Red Hook), avec une interface lisible et agréable, pour juger si le cœur du jeu est amusant.

**Architecture :** domaine C# pur (plateau, combat) testé en TDD et partagé avec le futur serveur ; couche Application (use cases, ports avec implémentations factices, bus MessagePipe) ; présentation MVP en Unity. Pas de backend UGS au M1 : les ports ont des implémentations locales.

**Stack :** Unity 6.6 (6000.6.3f1, URP), C# 9 / netstandard2.1 pour le code partagé, VContainer, MessagePipe, UniTask, PrimeTween, Input System, uGUI + TextMeshPro, NUnit (Unity Test Framework et `dotnet test`).

**Specs :** `04_Tech_Unity/00_Vue_Ensemble.md` à `09_Tests_Outils_CI.md` ; `01_Game_Design/01_Plateau.md`, `02_Combat.md`, `03_Metahumains.md`, `06_Kits_Competences.md` ; `03_Art_UI_UX/01_UX_Ecrans.md`.

## Contraintes globales
- Code partagé (`Core`, `Definitions`, `Domain.*`) : **aucun** `using UnityEngine`, C# 9 maximum, `#nullable enable`, netstandard2.1.
- Pas de `float`/`double` dans le domaine : entiers ; pourcentages en **%** (int) ; précisions fines en **pour-mille** (int). (Le type `Fixed` prévu par l'architecture n'est introduit que si un besoin apparaît.)
- Aléatoire **uniquement** via `IRandom` (PCG32). Temps de combat en **ticks de 50 ms**.
- Ordre stable : jamais d'itération de `Dictionary`/`HashSet` pour décider d'un ordre.
- Espaces de noms `N29.HeroesDawn.<Couche>.<Module>` ; un type public par fichier.
- Paramètres de départ (tableur) : jauge ATB 1 000 pts, remplissage VIT × 4 / s, K = 300, critique 15 % ×1,5, variation ±5 %, fatigue +12 % victoire / +27 % défaite, 6 PM par dé bonus, 2 dés bonus max, rayon de brouillard 2.
- Textes affichés : clés de localisation, jamais de chaîne en dur dans l'UI.

## Points d'attention (Review Focus)
1. **Dé bonus à la réserve pleine** : avec 2 dés bonus en réserve, les PM non utilisés sont perdus (pas de 3e dé, pas de PM stockés) → test dans la tâche 3.
2. **Déplacement vers une case derrière un arrêt forcé** : une case au-delà d'un combat ou d'une case qui ouvre du brouillard ne doit **pas** être proposée → test dans la tâche 5.
3. **Aller-retour immédiat** (A → B → A dans le même déplacement) interdit, sinon farm de ressources → test dans la tâche 5.
4. **Deux alliés prêts en même temps sans combo possible** : le bouton Combo ne doit pas apparaître ; avec combo, il apparaît une seule fois → test dans la tâche 9.
5. **Rejeu** : même graine + même journal d'entrées = même empreinte, y compris quand le joueur agit tard (jauges des autres qui continuent) → test dans la tâche 9.

---

## Carte des fichiers
```
Code/
├─ README.md  .gitignore  .gitattributes  global.json
├─ Heroes Dawn Unity/                       ← créé par Unity Hub (Unity 6000.6.3f1, URP)
│  ├─ Packages/
│  │  ├─ manifest.json                     (dépendances ajoutées, tâche 0)
│  │  └─ com.n29.heroesdawn.domain/
│  │     ├─ package.json
│  │     ├─ Core/        N29.HeroesDawn.Core.asmdef + *.cs
│  │     ├─ Definitions/ N29.HeroesDawn.Definitions.asmdef + *.cs
│  │     └─ Domain/
│  │        ├─ Board/    N29.HeroesDawn.Domain.Board.asmdef + *.cs
│  │        └─ Combat/   N29.HeroesDawn.Domain.Combat.asmdef + *.cs
│  └─ Assets/
│     ├─ _Project/
│     │  ├─ Application/     N29.HeroesDawn.Application.asmdef
│     │  ├─ Content/         N29.HeroesDawn.Content.asmdef (contenu du prototype)
│     │  ├─ Infrastructure/  N29.HeroesDawn.Infrastructure.asmdef
│     │  ├─ Presentation/    N29.HeroesDawn.Presentation.asmdef
│     │  ├─ Composition/     N29.HeroesDawn.Composition.asmdef
│     │  ├─ Editor/          N29.HeroesDawn.Editor.asmdef (+ installeur de packages)
│     │  ├─ Art/ Audio/ Prefabs/ Scenes/ Localization/ Settings/
│     └─ Tests/
│        ├─ Domain/          N29.HeroesDawn.Tests.Domain.asmdef
│        └─ PlayMode/        N29.HeroesDawn.Tests.PlayMode.asmdef
├─ Server/HeroesDawn.CloudCode/            (vide au M1, prévu M2)
├─ Tools/
│  ├─ SharedTests/ HeroesDawn.SharedTests.csproj   ← `dotnet test` sur le code partagé
│  └─ BalanceSimulator/ HeroesDawn.BalanceSimulator.csproj
└─ docs/plans/                                    ← copies de ce plan
```

---

### Tâche 0 : Squelette du dépôt et du projet Unity
*(Fait le 2026-09-29 : projet `Code/Heroes Dawn Unity` créé par Djason (Unity 6000.6.3f1, URP), squelette posé et `manifest.json` fusionné par Claude. Reste : étapes 3 à 8, voir `Code/SETUP.md`.)*

**Fichiers :** `Code/README.md`, `SETUP.md`, `Setup-Scaffold.ps1`, `.gitignore`, `.gitattributes`, `.editorconfig` ; `Code/_UnityScaffold/` (asmdefs, package local `com.n29.heroesdawn.domain`, `manifest.additions.json`, `Editor/PackageInstaller.cs`, `Tests/Domain/SkeletonTests.cs`) ; `Tools/SharedTests/*.csproj`, `Tools/BalanceSimulator/*` ; `Server/README.md`.

- [ ] **Étape 1 : Créer le projet avec Unity Hub** — modèle **Universal 2D**, Unity 6.6, emplacement `Documents/Heroes Dawn/Code`, nom `Heroes Dawn Unity`. Ouvrir une fois, fermer.
- [ ] **Étape 2 : Poser le squelette** — Claude (ou `./Setup-Scaffold.ps1`) copie `_UnityScaffold/Assets` et `_UnityScaffold/Packages/com.n29.heroesdawn.domain` dans `Unity/`, et fusionne `manifest.additions.json` (VContainer, UniTask, MessagePipe, MessagePipe.VContainer en Git ; registre OpenUPM pour PrimeTween) dans `Unity/Packages/manifest.json`.
- [ ] **Étape 3 : Ouvrir Unity** — résolution des packages Git (Git et Git LFS dans le `PATH` Windows).
- [ ] **Étape 4 : `Heroes Dawn ▸ Setup ▸ 1. Installer les packages du prototype`** — Input System, Test Framework, Addressables, Localization, 2D Animation, Newtonsoft Json, PrimeTween, en version recommandée pour l'éditeur.
- [ ] **Étape 5 : `Heroes Dawn ▸ Setup ▸ 2. Brancher les références d'assemblies`** — ajoute `Unity.TextMeshPro`, `Unity.InputSystem`, `PrimeTween.Runtime` et `Newtonsoft.Json.dll` aux assemblies de présentation (elles ne sont pas référencées d'emblée pour éviter des erreurs de compilation avant l'installation).
- [ ] **Étape 6 : Player Settings** — *Active Input Handling* = *Input System Package (New)* ou *Both*.
- [ ] **Étape 7 : Vérifier** — console sans erreur ; Test Runner EditMode : `Skeleton_Compiles` vert ; `dotnet test Tools/SharedTests` vert.
- [ ] **Étape 8 : Git**
```bash
cd "Documents/Heroes Dawn/Code"
git init && git lfs install
git add . && git commit -m "chore: squelette du projet Heroes Dawn (M1)"
```

---

### Tâche 1 : Core — identifiants, résultat, aléatoire déterministe

**Fichiers :**
- Créer : `Packages/com.n29.heroesdawn.domain/Core/IsExternalInit.cs`, `CaseId.cs`, `MemberId.cs`, `EnemyId.cs`, `SkillId.cs`, `ZoneId.cs`, `DomainError.cs`, `Result.cs`, `IRandom.cs`, `Pcg32.cs`, `Fnv1a64.cs`
- Test : `Assets/Tests/Domain/Core/Pcg32Tests.cs`, `ResultTests.cs`, `IdTests.cs`

**Interfaces :**
- Produit : `readonly struct CaseId(string)` (idem `MemberId`, `EnemyId`, `SkillId`, `ZoneId`) avec `Value`, égalité, `CompareTo` ordinal ; `Result` / `Result<T>` (`IsSuccess`, `IsFailure`, `Error`, `Value`, `Ok()`, `Fail(DomainError)`) ; `DomainError(string Code, string LocKey)` ; `IRandom { uint NextUInt(); int Range(int minInclusive, int maxExclusive); bool Chance(int percent); IRandom Fork(ulong stream); ulong State { get; } }` ; `Pcg32(ulong seed, ulong stream = 54)` ; `Fnv1a64` (`Add(int)`, `Add(long)`, `Add(string)`, `ulong Value`).

- [ ] **Étape 1 : Tests en échec**
```csharp
// Assets/Tests/Domain/Core/Pcg32Tests.cs
using N29.HeroesDawn.Core;
using NUnit.Framework;

public class Pcg32Tests
{
    [Test] // Vecteur de référence PCG32 (pcg32-demo : srandom(42, 54))
    public void Seed42Stream54_MatchesReferenceVector()
    {
        var rng = new Pcg32(42, 54);
        Assert.That(rng.NextUInt(), Is.EqualTo(0xa15c02b7u));
        Assert.That(rng.NextUInt(), Is.EqualTo(0x7b47f409u));
        Assert.That(rng.NextUInt(), Is.EqualTo(0xba1d3330u));
    }

    [Test]
    public void Range_StaysWithinBounds()
    {
        var rng = new Pcg32(7);
        for (int i = 0; i < 10_000; i++)
        {
            int v = rng.Range(1, 7);
            Assert.That(v, Is.InRange(1, 6));
        }
    }

    [Test]
    public void Fork_IsDeterministic_AndIndependent()
    {
        var a = new Pcg32(99).Fork(1);
        var b = new Pcg32(99).Fork(1);
        var c = new Pcg32(99).Fork(2);
        uint va = a.NextUInt();
        Assert.That(b.NextUInt(), Is.EqualTo(va));
        Assert.That(c.NextUInt(), Is.Not.EqualTo(va));
    }
}
```
```csharp
// Assets/Tests/Domain/Core/ResultTests.cs
using N29.HeroesDawn.Core;
using NUnit.Framework;

public class ResultTests
{
    [Test]
    public void Fail_CarriesError()
    {
        var r = Result.Fail(new DomainError("not_enough_pr", "errors.not_enough_pr"));
        Assert.That(r.IsFailure, Is.True);
        Assert.That(r.Error!.Code, Is.EqualTo("not_enough_pr"));
    }

    [Test]
    public void OkOfT_CarriesValue()
    {
        var r = Result<int>.Ok(5);
        Assert.That(r.IsSuccess, Is.True);
        Assert.That(r.Value, Is.EqualTo(5));
    }
}
```
```csharp
// Assets/Tests/Domain/Core/IdTests.cs
using N29.HeroesDawn.Core;
using NUnit.Framework;

public class IdTests
{
    [Test]
    public void SameValue_AreEqual_AndSortOrdinal()
    {
        Assert.That(new CaseId("b"), Is.EqualTo(new CaseId("b")));
        Assert.That(new CaseId("a").CompareTo(new CaseId("b")), Is.LessThan(0));
    }
}
```

- [ ] **Étape 2 : Vérifier l'échec** — `dotnet test Tools/SharedTests` → erreurs de compilation (types absents).

- [ ] **Étape 3 : Implémentation**
```csharp
// Core/IsExternalInit.cs — déjà présent dans le squelette
#if !NET5_0_OR_GREATER
namespace System.Runtime.CompilerServices { internal static class IsExternalInit { } }
#endif
```
```csharp
// Core/CaseId.cs — même modèle pour MemberId, EnemyId, SkillId, ZoneId (remplacer le nom)
#nullable enable
using System;
namespace N29.HeroesDawn.Core
{
    public readonly struct CaseId : IEquatable<CaseId>, IComparable<CaseId>
    {
        public readonly string Value;
        public CaseId(string value) { Value = value ?? throw new ArgumentNullException(nameof(value)); }
        public bool Equals(CaseId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object? obj) => obj is CaseId o && Equals(o);
        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);
        public int CompareTo(CaseId other) => string.CompareOrdinal(Value, other.Value);
        public override string ToString() => Value;
        public static bool operator ==(CaseId a, CaseId b) => a.Equals(b);
        public static bool operator !=(CaseId a, CaseId b) => !a.Equals(b);
    }
}
```
```csharp
// Core/DomainError.cs
#nullable enable
namespace N29.HeroesDawn.Core
{
    public sealed class DomainError
    {
        public string Code { get; }
        public string LocKey { get; }
        public DomainError(string code, string locKey) { Code = code; LocKey = locKey; }
        public override string ToString() => Code;
    }
}
```
```csharp
// Core/Result.cs
#nullable enable
using System;
namespace N29.HeroesDawn.Core
{
    public readonly struct Result
    {
        public DomainError? Error { get; }
        public bool IsSuccess => Error == null;
        public bool IsFailure => Error != null;
        Result(DomainError? error) { Error = error; }
        public static Result Ok() => new Result(null);
        public static Result Fail(DomainError error) => new Result(error ?? throw new ArgumentNullException(nameof(error)));
    }

    public readonly struct Result<T>
    {
        readonly T _value;
        public DomainError? Error { get; }
        public bool IsSuccess => Error == null;
        public bool IsFailure => Error != null;
        public T Value => IsSuccess ? _value : throw new InvalidOperationException($"Result en échec : {Error}");
        Result(T value, DomainError? error) { _value = value; Error = error; }
        public static Result<T> Ok(T value) => new Result<T>(value, null);
        public static Result<T> Fail(DomainError error) => new Result<T>(default!, error);
        public static implicit operator Result(Result<T> r) => r.IsSuccess ? Result.Ok() : Result.Fail(r.Error!);
    }
}
```
```csharp
// Core/IRandom.cs
namespace N29.HeroesDawn.Core
{
    public interface IRandom
    {
        uint NextUInt();
        /// <summary>Entier uniforme dans [minInclusive, maxExclusive[.</summary>
        int Range(int minInclusive, int maxExclusive);
        /// <summary>Vrai avec une probabilité de percent % (0-100).</summary>
        bool Chance(int percent);
        IRandom Fork(ulong stream);
        ulong State { get; }
    }
}
```
```csharp
// Core/Pcg32.cs — PCG-XSH-RR 64/32 (O'Neill), sans biais pour Range
using System;
namespace N29.HeroesDawn.Core
{
    public sealed class Pcg32 : IRandom
    {
        ulong _state;
        readonly ulong _inc;

        public Pcg32(ulong seed, ulong stream = 54)
        {
            _state = 0UL;
            _inc = (stream << 1) | 1UL;
            NextUInt();
            _state += seed;
            NextUInt();
        }

        public ulong State => _state;

        public uint NextUInt()
        {
            ulong old = _state;
            _state = unchecked(old * 6364136223846793005UL + _inc);
            uint xorshifted = (uint)(((old >> 18) ^ old) >> 27);
            int rot = (int)(old >> 59);
            return (xorshifted >> rot) | (xorshifted << ((-rot) & 31));
        }

        public int Range(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive) throw new ArgumentOutOfRangeException(nameof(maxExclusive));
            uint bound = (uint)(maxExclusive - minInclusive);
            uint threshold = unchecked(0u - bound) % bound;
            while (true)
            {
                uint r = NextUInt();
                if (r >= threshold) return minInclusive + (int)(r % bound);
            }
        }

        public bool Chance(int percent) => percent > 0 && (percent >= 100 || Range(0, 100) < percent);

        public IRandom Fork(ulong stream)
        {
            ulong seed = ((ulong)NextUInt() << 32) | NextUInt();
            return new Pcg32(seed, stream);
        }
    }
}
```
```csharp
// Core/Fnv1a64.cs — empreinte d'état pour le rejeu
namespace N29.HeroesDawn.Core
{
    public struct Fnv1a64
    {
        const ulong Offset = 14695981039346656037UL;
        const ulong Prime = 1099511628211UL;
        ulong _hash;
        bool _started;
        public ulong Value => _started ? _hash : Offset;

        public void Add(long v)
        {
            if (!_started) { _hash = Offset; _started = true; }
            for (int i = 0; i < 8; i++) { _hash ^= (byte)(v >> (i * 8)); _hash = unchecked(_hash * Prime); }
        }
        public void Add(int v) => Add((long)v);
        public void Add(string s) { foreach (char c in s) Add((long)c); }
    }
}
```

- [ ] **Étape 4 : Vérifier le succès** — `dotnet test Tools/SharedTests` → 6 tests verts. Puis Test Runner EditMode dans Unity → verts.
- [ ] **Étape 5 : Commit** — `git commit -am "feat(core): ids typés, Result, PCG32, FNV-1a"`

---

### Tâche 2 : Definitions et contenu du prototype

**Fichiers :**
- Créer (Definitions) : `Enums.cs`, `StatBlock.cs`, `EffectDef.cs`, `SkillDef.cs`, `MemberDef.cs`, `EnemyDef.cs`, `ComboDef.cs`, `NodeDef.cs`, `LinkDef.cs`, `ObjectiveDef.cs`, `EncounterDef.cs`, `ZoneDef.cs`, `BalanceConfig.cs`, `ContentCatalog.cs`
- Créer (Content, assembly Unity-agnostique) : `Assets/_Project/Content/Prototype/PrototypeContent.cs`
- Test : `Assets/Tests/Domain/Definitions/PrototypeContentTests.cs`

**Interfaces :**
- Produit :
  - `enum Family { Matter, Elemental, PhysicalBio, Perception }`, `enum Role { Tank, Damage, Control, Support, Precision }`, `enum Rarity { Agent, Elite, Legend }`, `enum CaseType { Entry, Empty, Combat, MiniGame, Narrative, Resource, Rest }`, `enum CombatRank { Easy, Normal, Elite, Boss }`, `enum SkillSlot { A1, A2, A3, Passive }`, `enum TargetRule { SingleEnemy, AllEnemies, SingleAlly, AllAllies, Self }`, `enum EffectType { Damage, Heal, ApplyStatus, GaugeDelta, Shield }`, `enum StatusType { None, Burn, Freeze, Shock, Corrosion, Blind, Vulnerable, Bind, Mark, Taunt, Shield, Regen, Haste, Evade }`, `enum ObjectiveType { DefeatCombat, ReachCase, CompleteMiniGame, ResolveNarrative }`.
  - `StatBlock(int Hp, int Atk, int Def, int Spd)`.
  - `EffectDef(EffectType Type, int Value, StatusType Status = None, int Duration = 0, int ChancePercent = 100)` — `Value` : % de l'ATQ (Damage), % PV max (Heal/Shield), points de jauge ×1 (GaugeDelta, sur 1 000).
  - `SkillDef(SkillId Id, SkillSlot Slot, int Cooldown, TargetRule Target, IReadOnlyList<EffectDef> Effects)`.
  - `MemberDef(MemberId Id, string NameKey, Family Family, Role Role, Rarity Rarity, StatBlock BaseStats, IReadOnlyList<SkillDef> Skills)` ; `EnemyDef(EnemyId Id, string NameKey, Role Role, Family Family, StatBlock BaseStats, IReadOnlyList<SkillDef> Skills)`.
  - `ComboDef(string Id, string NameKey, Family A, Family B, SkillDef Effect)` ; `ComboDef.Matches(Family x, Family y)`.
  - `NodeDef(CaseId Id, CaseType Type, bool IsFixed, CombatRank Rank, int X, int Y)` ; `LinkDef(CaseId A, CaseId B)` ; `ObjectiveDef(string Id, ObjectiveType Type, CaseId Target, string NameKey)` ; `EncounterDef(CombatRank Rank, IReadOnlyList<EnemyId> Enemies, int Level)`.
  - `ZoneDef(ZoneId Id, string NameKey, CaseId Entry, IReadOnlyList<NodeDef> Nodes, IReadOnlyList<LinkDef> Links, IReadOnlyList<ObjectiveDef> Objectives, IReadOnlyList<EncounterDef> Encounters)`.
  - `BalanceConfig` (propriétés `init` avec les valeurs par défaut des contraintes globales).
  - `ContentCatalog(BalanceConfig, members, enemies, combos, zones)` avec `Member(MemberId)`, `Enemy(EnemyId)`, `Zone(ZoneId)`, `FindCombo(Family, Family)`.
  - `PrototypeContent.Build()` → `ContentCatalog` : 8 recrues (Ancre, Verre, Givre, Écho, Arc, Rempart, Sève, Étincelle), 4 ennemis (Voyou néo-méta, Gros bras, Pyro de rue, Brick), 7 combos de famille, zone `zone_ny_1_1` de 25 cases.

- [ ] **Étape 1 : Test en échec**
```csharp
// Assets/Tests/Domain/Definitions/PrototypeContentTests.cs
using System.Linq;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using NUnit.Framework;

public class PrototypeContentTests
{
    [Test]
    public void Zone11_IsWellFormed()
    {
        var c = PrototypeContent.Build();
        var z = c.Zone(new ZoneId("zone_ny_1_1"));
        Assert.That(z.Nodes.Count, Is.EqualTo(25));
        Assert.That(z.Objectives.Count, Is.EqualTo(3));
        Assert.That(z.Nodes.Count(n => n.Type == CaseType.Combat), Is.EqualTo(6));
        var ids = z.Nodes.Select(n => n.Id).ToHashSet();
        Assert.That(z.Links.All(l => ids.Contains(l.A) && ids.Contains(l.B)), Is.True);
        Assert.That(ids.Contains(z.Entry), Is.True);
    }

    [Test]
    public void EveryMember_HasThreeActiveSkills()
    {
        var c = PrototypeContent.Build();
        Assert.That(c.Members.Count, Is.EqualTo(8));
        Assert.That(c.Members.All(m => m.Skills.Count(s => s.Slot != SkillSlot.Passive) == 3), Is.True);
    }

    [Test]
    public void EveryFamilyPair_HasACombo()
    {
        var c = PrototypeContent.Build();
        foreach (Family a in System.Enum.GetValues(typeof(Family)))
            foreach (Family b in System.Enum.GetValues(typeof(Family)))
                Assert.That(c.FindCombo(a, b), Is.Not.Null, $"{a}+{b}");
    }
}
```

- [ ] **Étape 2 : Vérifier l'échec** — `dotnet test` → types absents.

- [ ] **Étape 3 : Implémentation des définitions**
```csharp
// Definitions/Enums.cs
namespace N29.HeroesDawn.Definitions
{
    public enum Family { Matter, Elemental, PhysicalBio, Perception }
    public enum Role { Tank, Damage, Control, Support, Precision }
    public enum Rarity { Agent, Elite, Legend }
    public enum CaseType { Entry, Empty, Combat, MiniGame, Narrative, Resource, Rest }
    public enum CombatRank { Easy, Normal, Elite, Boss }
    public enum SkillSlot { A1, A2, A3, Passive }
    public enum TargetRule { SingleEnemy, AllEnemies, SingleAlly, AllAllies, Self }
    public enum EffectType { Damage, Heal, ApplyStatus, GaugeDelta, Shield }
    public enum StatusType { None, Burn, Freeze, Shock, Corrosion, Blind, Vulnerable, Bind, Mark, Taunt, Shield, Regen, Haste, Evade }
    public enum ObjectiveType { DefeatCombat, ReachCase, CompleteMiniGame, ResolveNarrative }
}
```
```csharp
// Definitions/StatBlock.cs
namespace N29.HeroesDawn.Definitions
{
    public sealed class StatBlock
    {
        public int Hp { get; } public int Atk { get; } public int Def { get; } public int Spd { get; }
        public StatBlock(int hp, int atk, int def, int spd) { Hp = hp; Atk = atk; Def = def; Spd = spd; }
    }
}
```
```csharp
// Definitions/EffectDef.cs
namespace N29.HeroesDawn.Definitions
{
    public sealed class EffectDef
    {
        public EffectType Type { get; }
        /// <summary>Damage : % ATQ · Heal/Shield : % PV max de la cible · GaugeDelta : points sur 1 000.</summary>
        public int Value { get; }
        public StatusType Status { get; }
        public int Duration { get; }
        public int ChancePercent { get; }
        public EffectDef(EffectType type, int value, StatusType status = StatusType.None, int duration = 0, int chancePercent = 100)
        { Type = type; Value = value; Status = status; Duration = duration; ChancePercent = chancePercent; }
    }
}
```
```csharp
// Definitions/SkillDef.cs
using System.Collections.Generic;
using N29.HeroesDawn.Core;
namespace N29.HeroesDawn.Definitions
{
    public sealed class SkillDef
    {
        public SkillId Id { get; } public SkillSlot Slot { get; } public int Cooldown { get; }
        public TargetRule Target { get; } public IReadOnlyList<EffectDef> Effects { get; }
        public SkillDef(SkillId id, SkillSlot slot, int cooldown, TargetRule target, IReadOnlyList<EffectDef> effects)
        { Id = id; Slot = slot; Cooldown = cooldown; Target = target; Effects = effects; }
    }
}
```
```csharp
// Definitions/MemberDef.cs
using System.Collections.Generic;
using N29.HeroesDawn.Core;
namespace N29.HeroesDawn.Definitions
{
    public sealed class MemberDef
    {
        public MemberId Id { get; } public string NameKey { get; } public Family Family { get; } public Role Role { get; }
        public Rarity Rarity { get; } public StatBlock BaseStats { get; } public IReadOnlyList<SkillDef> Skills { get; }
        public MemberDef(MemberId id, string nameKey, Family family, Role role, Rarity rarity, StatBlock baseStats, IReadOnlyList<SkillDef> skills)
        { Id = id; NameKey = nameKey; Family = family; Role = role; Rarity = rarity; BaseStats = baseStats; Skills = skills; }
    }
}
```
```csharp
// Definitions/EnemyDef.cs
using System.Collections.Generic;
using N29.HeroesDawn.Core;
namespace N29.HeroesDawn.Definitions
{
    public sealed class EnemyDef
    {
        public EnemyId Id { get; } public string NameKey { get; } public Role Role { get; } public Family Family { get; }
        public StatBlock BaseStats { get; } public IReadOnlyList<SkillDef> Skills { get; }
        public EnemyDef(EnemyId id, string nameKey, Role role, Family family, StatBlock baseStats, IReadOnlyList<SkillDef> skills)
        { Id = id; NameKey = nameKey; Role = role; Family = family; BaseStats = baseStats; Skills = skills; }
    }
}
```
```csharp
// Definitions/ComboDef.cs
namespace N29.HeroesDawn.Definitions
{
    public sealed class ComboDef
    {
        public string Id { get; } public string NameKey { get; } public Family A { get; } public Family B { get; } public SkillDef Effect { get; }
        public ComboDef(string id, string nameKey, Family a, Family b, SkillDef effect) { Id = id; NameKey = nameKey; A = a; B = b; Effect = effect; }
        public bool Matches(Family x, Family y) => (x == A && y == B) || (x == B && y == A);
    }
}
```
```csharp
// Definitions/NodeDef.cs, LinkDef.cs, ObjectiveDef.cs, EncounterDef.cs (un fichier chacun)
using System.Collections.Generic;
using N29.HeroesDawn.Core;
namespace N29.HeroesDawn.Definitions
{
    public sealed class NodeDef
    {
        public CaseId Id { get; } public CaseType Type { get; } public bool IsFixed { get; } public CombatRank Rank { get; }
        /// <summary>Position sur la carte, en unités de grille (1 unité = 1 case de 100 px).</summary>
        public int X { get; } public int Y { get; }
        public NodeDef(CaseId id, CaseType type, bool isFixed, CombatRank rank, int x, int y)
        { Id = id; Type = type; IsFixed = isFixed; Rank = rank; X = x; Y = y; }
    }
    public sealed class LinkDef { public CaseId A { get; } public CaseId B { get; } public LinkDef(CaseId a, CaseId b) { A = a; B = b; } }
    public sealed class ObjectiveDef
    {
        public string Id { get; } public ObjectiveType Type { get; } public CaseId Target { get; } public string NameKey { get; }
        public ObjectiveDef(string id, ObjectiveType type, CaseId target, string nameKey) { Id = id; Type = type; Target = target; NameKey = nameKey; }
    }
    public sealed class EncounterDef
    {
        public CombatRank Rank { get; } public IReadOnlyList<EnemyId> Enemies { get; }
        public EncounterDef(CombatRank rank, IReadOnlyList<EnemyId> enemies) { Rank = rank; Enemies = enemies; }
    }
}
```
```csharp
// Definitions/ZoneDef.cs
using System.Collections.Generic;
using N29.HeroesDawn.Core;
namespace N29.HeroesDawn.Definitions
{
    public sealed class ZoneDef
    {
        public ZoneId Id { get; } public string NameKey { get; } public CaseId Entry { get; }
        public IReadOnlyList<NodeDef> Nodes { get; } public IReadOnlyList<LinkDef> Links { get; }
        public IReadOnlyList<ObjectiveDef> Objectives { get; } public IReadOnlyList<EncounterDef> Encounters { get; }
        public ZoneDef(ZoneId id, string nameKey, CaseId entry, IReadOnlyList<NodeDef> nodes, IReadOnlyList<LinkDef> links,
                       IReadOnlyList<ObjectiveDef> objectives, IReadOnlyList<EncounterDef> encounters)
        { Id = id; NameKey = nameKey; Entry = entry; Nodes = nodes; Links = links; Objectives = objectives; Encounters = encounters; }
    }
}
```
```csharp
// Definitions/BalanceConfig.cs — valeurs par défaut = tableur 05_Equilibrage.xlsx
namespace N29.HeroesDawn.Definitions
{
    public sealed class BalanceConfig
    {
        public int TickMs { get; init; } = 50;
        public int AtbGaugePoints { get; init; } = 1000;
        public int AtbFactorPerSecond { get; init; } = 4;
        public int DefK { get; init; } = 300;
        public int CritBasePercent { get; init; } = 15;
        public int CritMultiplierPercent { get; init; } = 150;
        public int VariancePercent { get; init; } = 5;
        public int VulnerableBonusPercent { get; init; } = 25;
        public int FatiguePerWin { get; init; } = 12;
        public int FatiguePerLossExtra { get; init; } = 15;
        public int FatigueMalusThreshold { get; init; } = 60;
        public int FatigueMalusMaxPercent { get; init; } = 30;
        public int DieFaces { get; init; } = 6;
        public int PmPerBonusDie { get; init; } = 6;
        public int MaxBonusDice { get; init; } = 2;
        public int FogRadius { get; init; } = 2;
        public int StatGrowthPermillePerLevel { get; init; } = 40;
        public int RankEasyLevelOffset { get; init; } = -3;
        public int RankEliteLevelOffset { get; init; } = 3;
        public int RankEasyStatPercent { get; init; } = 80;
        public int RankElitePercent { get; init; } = 130;
        public int BossHpPercent { get; init; } = 300;
        public int ComboCooldownActions { get; init; } = 3;
        public int ResourceCreditsMin { get; init; } = 150;
        public int ResourceCreditsMax { get; init; } = 400;
        public int RestFatigueReduction { get; init; } = 30;
    }
}
```
```csharp
// Definitions/ContentCatalog.cs
#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Core;
namespace N29.HeroesDawn.Definitions
{
    public sealed class ContentCatalog
    {
        readonly Dictionary<MemberId, MemberDef> _members = new Dictionary<MemberId, MemberDef>();
        readonly Dictionary<EnemyId, EnemyDef> _enemies = new Dictionary<EnemyId, EnemyDef>();
        readonly Dictionary<ZoneId, ZoneDef> _zones = new Dictionary<ZoneId, ZoneDef>();
        public BalanceConfig Balance { get; }
        public IReadOnlyList<MemberDef> Members { get; }
        public IReadOnlyList<EnemyDef> Enemies { get; }
        public IReadOnlyList<ComboDef> Combos { get; }

        public ContentCatalog(BalanceConfig balance, IReadOnlyList<MemberDef> members, IReadOnlyList<EnemyDef> enemies,
                              IReadOnlyList<ComboDef> combos, IReadOnlyList<ZoneDef> zones)
        {
            Balance = balance; Members = members; Enemies = enemies; Combos = combos;
            foreach (var m in members) _members.Add(m.Id, m);
            foreach (var e in enemies) _enemies.Add(e.Id, e);
            foreach (var z in zones) _zones.Add(z.Id, z);
        }
        public MemberDef Member(MemberId id) => _members[id];
        public EnemyDef Enemy(EnemyId id) => _enemies[id];
        public ZoneDef Zone(ZoneId id) => _zones[id];
        public ComboDef? FindCombo(Family a, Family b)
        {
            foreach (var c in Combos) if (c.Matches(a, b)) return c; // ordre de la liste = ordre stable
            return null;
        }
    }
}
```

- [ ] **Étape 4 : Contenu du prototype** — `PrototypeContent.Build()` construit le catalogue. Stats de base = onglet *Stats* (Agent niv. 1) × multiplicateur de rareté ; kits simplifiés depuis `06_Kits_Competences.md`.
```csharp
// Assets/_Project/Content/Prototype/PrototypeContent.cs
using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
namespace N29.HeroesDawn.Content.Prototype
{
    public static class PrototypeContent
    {
        static EffectDef Dmg(int pct) => new EffectDef(EffectType.Damage, pct);
        static EffectDef St(StatusType s, int dur, int chance = 100) => new EffectDef(EffectType.ApplyStatus, 0, s, dur, chance);
        static EffectDef Heal(int pct) => new EffectDef(EffectType.Heal, pct);
        static EffectDef Shield(int pct, int dur = 2) => new EffectDef(EffectType.Shield, pct, StatusType.Shield, dur);
        static EffectDef Gauge(int pts) => new EffectDef(EffectType.GaugeDelta, pts);
        static SkillDef S(string id, SkillSlot slot, int cd, TargetRule t, params EffectDef[] fx) => new SkillDef(new SkillId(id), slot, cd, t, fx);
        static StatBlock Stats(Role role, Rarity rarity)
        {
            var b = role switch
            {
                Role.Tank => new StatBlock(1500, 80, 120, 95), Role.Damage => new StatBlock(1000, 130, 70, 105),
                Role.Control => new StatBlock(1100, 100, 90, 110), Role.Support => new StatBlock(1100, 85, 85, 100),
                _ => new StatBlock(950, 120, 70, 115)
            };
            int m = rarity switch { Rarity.Elite => 120, Rarity.Legend => 145, _ => 100 };
            int spd = b.Spd + (rarity == Rarity.Elite ? 5 : rarity == Rarity.Legend ? 10 : 0);
            return new StatBlock(b.Hp * m / 100, b.Atk * m / 100, b.Def * m / 100, spd);
        }
        static MemberDef M(string id, Family f, Role r, Rarity ra, params SkillDef[] s)
            => new MemberDef(new MemberId(id), $"members.{id}.name", f, r, ra, Stats(r, ra), s);

        public static ContentCatalog Build()
        {
            var members = new List<MemberDef>
            {
                M("ancre", Family.PhysicalBio, Role.Tank, Rarity.Legend,
                    S("ancre_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(110), St(StatusType.Taunt, 1, 30)),
                    S("ancre_a2", SkillSlot.A2, 3, TargetRule.Self, St(StatusType.Taunt, 1), Shield(20)),
                    S("ancre_a3", SkillSlot.A3, 5, TargetRule.AllAllies, Shield(15))),
                M("verre", Family.Matter, Role.Control, Rarity.Legend,
                    S("verre_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(100), St(StatusType.Vulnerable, 2, 25)),
                    S("verre_a2", SkillSlot.A2, 4, TargetRule.SingleEnemy, St(StatusType.Bind, 1), Gauge(-300)),
                    S("verre_a3", SkillSlot.A3, 5, TargetRule.AllAllies, Shield(15))),
                M("givre", Family.Elemental, Role.Damage, Rarity.Legend,
                    S("givre_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(100), St(StatusType.Freeze, 2, 40)),
                    S("givre_a2", SkillSlot.A2, 3, TargetRule.AllEnemies, Dmg(70), Gauge(-150)),
                    S("givre_a3", SkillSlot.A3, 5, TargetRule.SingleEnemy, Dmg(220))),
                M("echo", Family.Perception, Role.Precision, Rarity.Legend,
                    S("echo_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(100), St(StatusType.Mark, 99)),
                    S("echo_a2", SkillSlot.A2, 3, TargetRule.AllEnemies, St(StatusType.Vulnerable, 2)),
                    S("echo_a3", SkillSlot.A3, 5, TargetRule.AllEnemies, Dmg(70))),
                M("arc", Family.Elemental, Role.Damage, Rarity.Elite,
                    S("arc_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(100), St(StatusType.Shock, 1, 20)),
                    S("arc_a2", SkillSlot.A2, 3, TargetRule.SingleAlly, St(StatusType.Haste, 2), Gauge(150)),
                    S("arc_a3", SkillSlot.A3, 5, TargetRule.AllEnemies, Dmg(90))),
                M("rempart", Family.PhysicalBio, Role.Tank, Rarity.Elite,
                    S("rempart_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(110)),
                    S("rempart_a2", SkillSlot.A2, 3, TargetRule.SingleAlly, Shield(25)),
                    S("rempart_a3", SkillSlot.A3, 5, TargetRule.Self, St(StatusType.Taunt, 2), Shield(30))),
                M("seve", Family.PhysicalBio, Role.Support, Rarity.Elite,
                    S("seve_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(80)),
                    S("seve_a2", SkillSlot.A2, 3, TargetRule.SingleAlly, Heal(30)),
                    S("seve_a3", SkillSlot.A3, 5, TargetRule.AllAllies, St(StatusType.Regen, 2))),
                M("etincelle", Family.Elemental, Role.Control, Rarity.Agent,
                    S("etincelle_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(100), St(StatusType.Shock, 1, 15)),
                    S("etincelle_a2", SkillSlot.A2, 3, TargetRule.SingleAlly, Gauge(150)),
                    S("etincelle_a3", SkillSlot.A3, 5, TargetRule.SingleEnemy, Dmg(150))),
            };

            EnemyDef E(string id, Role r, Family f, StatBlock st, params SkillDef[] s) => new EnemyDef(new EnemyId(id), $"enemies.{id}.name", r, f, st, s);
            var enemies = new List<EnemyDef>
            {
                E("thug", Role.Damage, Family.PhysicalBio, new StatBlock(1000, 120, 70, 100),
                    S("thug_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(100)),
                    S("thug_a2", SkillSlot.A2, 4, TargetRule.Self, Gauge(300))),
                E("brute", Role.Tank, Family.PhysicalBio, new StatBlock(1500, 80, 120, 90),
                    S("brute_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(90)),
                    S("brute_a2", SkillSlot.A2, 4, TargetRule.Self, St(StatusType.Taunt, 1))),
                E("street_pyro", Role.Damage, Family.Elemental, new StatBlock(950, 125, 65, 105),
                    S("pyro_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(90), St(StatusType.Burn, 2, 30)),
                    S("pyro_a2", SkillSlot.A2, 4, TargetRule.AllEnemies, Dmg(70))),
                E("brick", Role.Tank, Family.PhysicalBio, new StatBlock(1500 * 3, 110, 130, 95),
                    S("brick_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(120)),
                    S("brick_a2", SkillSlot.A2, 3, TargetRule.Self, St(StatusType.Taunt, 1), Shield(15)),
                    S("brick_a3", SkillSlot.A3, 5, TargetRule.AllEnemies, Dmg(90), St(StatusType.Shock, 1, 30))),
            };

            ComboDef C(string id, Family a, Family b, TargetRule t, params EffectDef[] fx)
                => new ComboDef(id, $"combos.{id}.name", a, b, S($"combo_{id}", SkillSlot.A3, 0, t, fx));
            var combos = new List<ComboDef>
            {
                C("charged_projectiles", Family.Matter, Family.Elemental, TargetRule.AllEnemies, Dmg(150), St(StatusType.Burn, 2)),
                C("forged_armor", Family.Matter, Family.PhysicalBio, TargetRule.AllAllies, Shield(30)),
                C("trap", Family.Matter, Family.Perception, TargetRule.SingleEnemy, St(StatusType.Bind, 2), Gauge(-500)),
                C("elemental_body", Family.Elemental, Family.PhysicalBio, TargetRule.AllEnemies, Dmg(120)),
                C("weak_point", Family.Elemental, Family.Perception, TargetRule.SingleEnemy, Dmg(350)),
                C("triage", Family.PhysicalBio, Family.Perception, TargetRule.AllAllies, Heal(25)),
                C("resonance_matter", Family.Matter, Family.Matter, TargetRule.AllAllies, Shield(15)),
                C("resonance_elemental", Family.Elemental, Family.Elemental, TargetRule.AllEnemies, Dmg(120)),
                C("resonance_physical", Family.PhysicalBio, Family.PhysicalBio, TargetRule.AllAllies, St(StatusType.Regen, 2)),
                C("resonance_perception", Family.Perception, Family.Perception, TargetRule.AllAllies, St(StatusType.Haste, 2)),
            };

            return new ContentCatalog(new BalanceConfig(), members, enemies, combos, new[] { Zone11() });
        }

        // Red Hook — les quais : 25 cases sur une grille, 3 embranchements, 6 combats (1 Élite fixe).
        static ZoneDef Zone11()
        {
            NodeDef N(string id, CaseType t, int x, int y, bool fixedNode = false, CombatRank r = CombatRank.Normal)
                => new NodeDef(new CaseId(id), t, fixedNode, r, x, y);
            var nodes = new List<NodeDef>
            {
                N("n00", CaseType.Entry, 0, 2, true),
                N("n01", CaseType.Resource, 1, 2), N("n02", CaseType.Empty, 2, 2),
                N("n03", CaseType.Combat, 3, 3, r: CombatRank.Easy), N("n04", CaseType.Resource, 4, 3), N("n05", CaseType.Narrative, 5, 3),
                N("n06", CaseType.Resource, 3, 1), N("n07", CaseType.Combat, 4, 1, r: CombatRank.Easy), N("n08", CaseType.MiniGame, 5, 1, true),
                N("n09", CaseType.Empty, 6, 2), N("n10", CaseType.Rest, 7, 2),
                N("n11", CaseType.Combat, 8, 3), N("n12", CaseType.Resource, 9, 3), N("n13", CaseType.Narrative, 10, 3),
                N("n14", CaseType.Resource, 8, 1), N("n15", CaseType.MiniGame, 9, 1), N("n16", CaseType.Combat, 10, 1),
                N("n17", CaseType.Empty, 11, 2), N("n18", CaseType.Resource, 12, 2),
                N("n19", CaseType.Combat, 13, 3), N("n20", CaseType.Narrative, 14, 3), N("n21", CaseType.MiniGame, 13, 1),
                N("n22", CaseType.Rest, 14, 1), N("n23", CaseType.Combat, 15, 2, true, CombatRank.Elite),
                N("n24", CaseType.Empty, 16, 2, true),
            };
            (string, string)[] pairs =
            {
                ("n00","n01"),("n01","n02"),("n02","n03"),("n02","n06"),("n03","n04"),("n04","n05"),("n06","n07"),("n07","n08"),
                ("n05","n09"),("n08","n09"),("n09","n10"),("n10","n11"),("n10","n14"),("n11","n12"),("n12","n13"),("n14","n15"),
                ("n15","n16"),("n13","n17"),("n16","n17"),("n17","n18"),("n18","n19"),("n18","n21"),("n19","n20"),("n21","n22"),
                ("n20","n23"),("n22","n23"),("n23","n24"),
            };
            var links = new List<LinkDef>();
            foreach (var (a, b) in pairs) links.Add(new LinkDef(new CaseId(a), new CaseId(b)));
            var objectives = new List<ObjectiveDef>
            {
                new ObjectiveDef("obj_warehouse", ObjectiveType.DefeatCombat, new CaseId("n23"), "objectives.ny11.warehouse"),
                new ObjectiveDef("obj_generator", ObjectiveType.CompleteMiniGame, new CaseId("n08"), "objectives.ny11.generator"),
                new ObjectiveDef("obj_crane", ObjectiveType.ReachCase, new CaseId("n24"), "objectives.ny11.crane"),
            };
            var enc = new List<EncounterDef>
            {
                new EncounterDef(CombatRank.Easy, new[] { new EnemyId("thug"), new EnemyId("thug") }),
                new EncounterDef(CombatRank.Easy, new[] { new EnemyId("thug"), new EnemyId("street_pyro") }),
                new EncounterDef(CombatRank.Normal, new[] { new EnemyId("brute"), new EnemyId("thug"), new EnemyId("street_pyro") }),
                new EncounterDef(CombatRank.Elite, new[] { new EnemyId("brick"), new EnemyId("thug") }),
            };
            return new ZoneDef(new ZoneId("zone_ny_1_1"), "zones.ny_1_1.name", new CaseId("n00"), nodes, links, objectives, enc);
        }
    }
}
```
Note : au M1, Brick sert de combat Élite d'objectif (le vrai mini-climax arrive en zone 1.3).

- [ ] **Étape 5 : Vérifier** — `dotnet test` → 3 nouveaux tests verts.
- [ ] **Étape 6 : Commit** — `git commit -am "feat(definitions): définitions immuables et contenu du prototype (zone 1.1)"`

---

### Tâche 3 : Plateau — réserve de dés (DiceBank)

**Fichiers :** Créer `Domain/Board/DiceBank.cs` · Test `Assets/Tests/Domain/Board/DiceBankTests.cs`

**Interfaces :**
- Consomme : `BalanceConfig.PmPerBonusDie`, `MaxBonusDice`.
- Produit : `sealed class DiceBank { int StoredPm; int BonusDice; DiceBank(BalanceConfig); DiceBank(BalanceConfig, int storedPm, int bonusDice); void AddUnusedPm(int pm); bool TryConsumeBonusDie(); }`

- [ ] **Étape 1 : Tests en échec**
```csharp
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;
using NUnit.Framework;

public class DiceBankTests
{
    static readonly BalanceConfig B = new BalanceConfig();

    [Test] // Plateau : 6 PM stockés = 1 dé bonus
    public void SixStoredPm_BecomeOneBonusDie()
    {
        var bank = new DiceBank(B);
        bank.AddUnusedPm(4); bank.AddUnusedPm(3);
        Assert.That(bank.BonusDice, Is.EqualTo(1));
        Assert.That(bank.StoredPm, Is.EqualTo(1));
    }

    [Test] // Review Focus 1 : réserve pleine → surplus perdu
    public void FullReserve_DiscardsSurplus()
    {
        var bank = new DiceBank(B, storedPm: 0, bonusDice: 2);
        bank.AddUnusedPm(5);
        Assert.That(bank.BonusDice, Is.EqualTo(2));
        Assert.That(bank.StoredPm, Is.EqualTo(0));
    }

    [Test]
    public void ReachingSecondDie_DropsLeftover()
    {
        var bank = new DiceBank(B, storedPm: 4, bonusDice: 1);
        bank.AddUnusedPm(5); // 9 → 1 dé (total 2) + 3 perdus
        Assert.That(bank.BonusDice, Is.EqualTo(2));
        Assert.That(bank.StoredPm, Is.EqualTo(0));
    }

    [Test]
    public void ConsumeBonusDie_FailsWhenEmpty()
    {
        var bank = new DiceBank(B);
        Assert.That(bank.TryConsumeBonusDie(), Is.False);
        bank.AddUnusedPm(6);
        Assert.That(bank.TryConsumeBonusDie(), Is.True);
        Assert.That(bank.BonusDice, Is.EqualTo(0));
    }
}
```
- [ ] **Étape 2 : Vérifier l'échec** — `dotnet test --filter DiceBankTests` → type absent.
- [ ] **Étape 3 : Implémentation**
```csharp
using System;
using N29.HeroesDawn.Definitions;
namespace N29.HeroesDawn.Domain.Board
{
    /// <summary>GDD §5 — PM non utilisés → jauge ; 6 PM = 1 dé bonus ; réserve max 2, surplus perdu.</summary>
    public sealed class DiceBank
    {
        readonly BalanceConfig _b;
        public int StoredPm { get; private set; }
        public int BonusDice { get; private set; }

        public DiceBank(BalanceConfig b) : this(b, 0, 0) { }
        public DiceBank(BalanceConfig b, int storedPm, int bonusDice) { _b = b; StoredPm = storedPm; BonusDice = bonusDice; }

        public void AddUnusedPm(int pm)
        {
            if (pm < 0) throw new ArgumentOutOfRangeException(nameof(pm));
            if (BonusDice >= _b.MaxBonusDice) { StoredPm = 0; return; }
            StoredPm += pm;
            while (StoredPm >= _b.PmPerBonusDie && BonusDice < _b.MaxBonusDice)
            {
                StoredPm -= _b.PmPerBonusDie;
                BonusDice++;
            }
            if (BonusDice >= _b.MaxBonusDice) StoredPm = 0;
        }

        public bool TryConsumeBonusDie()
        {
            if (BonusDice == 0) return false;
            BonusDice--;
            return true;
        }
    }
}
```
- [ ] **Étape 4 : Vérifier** — `dotnet test --filter DiceBankTests` → 4 verts.
- [ ] **Étape 5 : Commit** — `git commit -am "feat(board): réserve de dés bonus"`

---

### Tâche 4 : Plateau — graphe et brouillard

**Fichiers :** Créer `Domain/Board/BoardGraph.cs`, `Domain/Board/FogOfWar.cs` · Test `Assets/Tests/Domain/Board/FogTests.cs`, `Assets/Tests/Domain/Board/TestZones.cs`

**Interfaces :**
- Consomme : `ZoneDef`, `NodeDef`, `LinkDef`.
- Produit :
  - `BoardGraph(ZoneDef)` : `NodeDef Node(CaseId)`, `IReadOnlyList<CaseId> Neighbors(CaseId)` (triés), `IReadOnlyList<CaseId> AllIds` (triés), `IReadOnlyList<CaseId> WithinRadius(CaseId, int radius)` (triés).
  - `FogOfWar(BoardGraph, int radius)` : `bool IsRevealed(CaseId)`, `IReadOnlyList<CaseId> RevealAround(CaseId)` (renvoie les nouvelles cases, triées), `bool WouldReveal(CaseId)`, `void Close()`, `IReadOnlyCollection<CaseId> Revealed`, `void Restore(IEnumerable<CaseId>)`.
  - `TestZones.Line(params string[] ids)` / `TestZones.Build(CaseType[] types, (string,string)[] links)` pour les tests.

- [ ] **Étape 1 : Aide de test et tests en échec**
```csharp
// Assets/Tests/Domain/Board/TestZones.cs
using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

public static class TestZones
{
    /// <summary>Zone linéaire : chaque id peut être suffixé du type (« c:combat », « r:resource »…). Le premier est l'entrée.</summary>
    public static ZoneDef Line(params string[] specs)
    {
        var nodes = new List<NodeDef>(); var links = new List<LinkDef>();
        for (int i = 0; i < specs.Length; i++)
        {
            var (id, type) = Parse(specs[i], i == 0);
            nodes.Add(new NodeDef(new CaseId(id), type, false, CombatRank.Easy, i, 0));
            if (i > 0) links.Add(new LinkDef(nodes[i - 1].Id, nodes[i].Id));
        }
        return new ZoneDef(new ZoneId("test"), "t", nodes[0].Id, nodes, links, new ObjectiveDef[0],
            new[] { new EncounterDef(CombatRank.Easy, new[] { new EnemyId("thug") }), new EncounterDef(CombatRank.Normal, new[] { new EnemyId("thug") }) });
    }

    public static ZoneDef Graph(string[] specs, (string a, string b)[] edges)
    {
        var nodes = new List<NodeDef>(); var links = new List<LinkDef>();
        for (int i = 0; i < specs.Length; i++) { var (id, t) = Parse(specs[i], i == 0); nodes.Add(new NodeDef(new CaseId(id), t, false, CombatRank.Easy, i, 0)); }
        foreach (var (a, b) in edges) links.Add(new LinkDef(new CaseId(a), new CaseId(b)));
        return new ZoneDef(new ZoneId("test"), "t", nodes[0].Id, nodes, links, new ObjectiveDef[0],
            new[] { new EncounterDef(CombatRank.Easy, new[] { new EnemyId("thug") }), new EncounterDef(CombatRank.Normal, new[] { new EnemyId("thug") }) });
    }

    static (string, CaseType) Parse(string spec, bool first)
    {
        if (!spec.Contains(":")) return (spec, first ? CaseType.Entry : CaseType.Empty);
        var p = spec.Split(':');
        var t = p[1] switch
        {
            "combat" => CaseType.Combat, "resource" => CaseType.Resource, "rest" => CaseType.Rest,
            "narrative" => CaseType.Narrative, "minigame" => CaseType.MiniGame, _ => CaseType.Empty
        };
        return (p[0], t);
    }
}
```
```csharp
// Assets/Tests/Domain/Board/FogTests.cs
using System.Linq;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Domain.Board;
using NUnit.Framework;

public class FogTests
{
    [Test] // GDD §5 — rayon de visibilité 2
    public void RevealAround_RevealsRadiusTwo()
    {
        var g = new BoardGraph(TestZones.Line("a", "b", "c", "d", "e"));
        var fog = new FogOfWar(g, 2);
        var news = fog.RevealAround(new CaseId("a"));
        Assert.That(news.Select(x => x.Value), Is.EqualTo(new[] { "a", "b", "c" }));
        Assert.That(fog.IsRevealed(new CaseId("d")), Is.False);
    }

    [Test]
    public void WouldReveal_TrueOnlyIfUnseenWithinRadius()
    {
        var g = new BoardGraph(TestZones.Line("a", "b", "c", "d", "e"));
        var fog = new FogOfWar(g, 2);
        fog.RevealAround(new CaseId("a"));
        Assert.That(fog.WouldReveal(new CaseId("b")), Is.True);  // d est à 2 de b
        Assert.That(fog.WouldReveal(new CaseId("a")), Is.False);
    }

    [Test] // GDD §5 — le brouillard se referme en quittant la zone
    public void Close_HidesEverything()
    {
        var g = new BoardGraph(TestZones.Line("a", "b", "c"));
        var fog = new FogOfWar(g, 2);
        fog.RevealAround(new CaseId("a"));
        fog.Close();
        Assert.That(fog.Revealed.Count, Is.EqualTo(0));
    }
}
```
- [ ] **Étape 2 : Vérifier l'échec.**
- [ ] **Étape 3 : Implémentation**
```csharp
// Domain/Board/BoardGraph.cs
using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
namespace N29.HeroesDawn.Domain.Board
{
    public sealed class BoardGraph
    {
        readonly Dictionary<CaseId, NodeDef> _nodes = new Dictionary<CaseId, NodeDef>();
        readonly Dictionary<CaseId, List<CaseId>> _adj = new Dictionary<CaseId, List<CaseId>>();
        public ZoneDef Zone { get; }
        public IReadOnlyList<CaseId> AllIds { get; }

        public BoardGraph(ZoneDef zone)
        {
            Zone = zone;
            var ids = new List<CaseId>();
            foreach (var n in zone.Nodes) { _nodes.Add(n.Id, n); _adj.Add(n.Id, new List<CaseId>()); ids.Add(n.Id); }
            foreach (var l in zone.Links) { _adj[l.A].Add(l.B); _adj[l.B].Add(l.A); }
            foreach (var list in _adj.Values) list.Sort();
            ids.Sort();
            AllIds = ids;
        }

        public NodeDef Node(CaseId id) => _nodes[id];
        public IReadOnlyList<CaseId> Neighbors(CaseId id) => _adj[id];

        public IReadOnlyList<CaseId> WithinRadius(CaseId center, int radius)
        {
            var seen = new HashSet<CaseId> { center };
            var frontier = new List<CaseId> { center };
            for (int d = 0; d < radius; d++)
            {
                var next = new List<CaseId>();
                foreach (var c in frontier)
                    foreach (var n in _adj[c])
                        if (seen.Add(n)) next.Add(n);
                frontier = next;
            }
            var result = new List<CaseId>(seen);
            result.Sort();
            return result;
        }
    }
}
```
```csharp
// Domain/Board/FogOfWar.cs
using System.Collections.Generic;
using N29.HeroesDawn.Core;
namespace N29.HeroesDawn.Domain.Board
{
    public sealed class FogOfWar
    {
        readonly BoardGraph _g;
        readonly int _radius;
        readonly HashSet<CaseId> _revealed = new HashSet<CaseId>();
        public IReadOnlyCollection<CaseId> Revealed => _revealed;

        public FogOfWar(BoardGraph g, int radius) { _g = g; _radius = radius; }

        public bool IsRevealed(CaseId id) => _revealed.Contains(id);

        public bool WouldReveal(CaseId at)
        {
            foreach (var id in _g.WithinRadius(at, _radius)) if (!_revealed.Contains(id)) return true;
            return false;
        }

        public IReadOnlyList<CaseId> RevealAround(CaseId at)
        {
            var news = new List<CaseId>();
            foreach (var id in _g.WithinRadius(at, _radius)) if (_revealed.Add(id)) news.Add(id);
            return news; // déjà trié (WithinRadius trie)
        }

        public void Close() => _revealed.Clear();
        public void Restore(IEnumerable<CaseId> ids) { _revealed.Clear(); foreach (var id in ids) _revealed.Add(id); }
    }
}
```
- [ ] **Étape 4 : Vérifier** — 3 verts.
- [ ] **Étape 5 : Commit** — `git commit -am "feat(board): graphe et brouillard de guerre"`

---

### Tâche 5 : Plateau — destinations et déplacement

**Fichiers :** Créer `Domain/Board/MovementRules.cs`, `Domain/Board/MovePlan.cs` · Test `Assets/Tests/Domain/Board/MovementRulesTests.cs`

**Interfaces :**
- Consomme : `BoardGraph`, `FogOfWar`, `Func<CaseId, bool> isBlocking` (case Combat non résolue).
- Produit :
  - `sealed class MovePlan { CaseId Target; IReadOnlyList<CaseId> Path; /* sans la case de départ, se termine par Target */ int Cost => Path.Count; }`
  - `static class MovementRules { IReadOnlyDictionary<CaseId, MovePlan> FindDestinations(BoardGraph g, FogOfWar fog, CaseId from, int pm, Func<CaseId,bool> isBlocking); }`
  - Règles : destinations atteignables en 1..pm pas ; pas de retour immédiat ; on ne **traverse** pas une case bloquante (combat non résolu) ni une case qui **ouvre du brouillard** — elles peuvent être des destinations finales ; chemin retenu = le plus court, puis ordre lexicographique des ids ; on ne se déplace que sur des cases **révélées**.

- [ ] **Étape 1 : Tests en échec**
```csharp
using System.Linq;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Domain.Board;
using NUnit.Framework;

public class MovementRulesTests
{
    static FogOfWar AllRevealed(BoardGraph g) { var f = new FogOfWar(g, 99); f.RevealAround(g.AllIds[0]); return f; }

    [Test]
    public void Line_ReachesOneToPmSteps()
    {
        var g = new BoardGraph(TestZones.Line("a", "b", "c", "d"));
        var d = MovementRules.FindDestinations(g, AllRevealed(g), new CaseId("a"), 2, _ => false);
        Assert.That(d.Keys.Select(k => k.Value).OrderBy(x => x), Is.EqualTo(new[] { "b", "c" }));
        Assert.That(d[new CaseId("c")].Path.Select(p => p.Value), Is.EqualTo(new[] { "b", "c" }));
    }

    [Test] // Review Focus 2 : une case au-delà d'un combat n'est pas proposée
    public void CombatCase_StopsMovement()
    {
        var g = new BoardGraph(TestZones.Line("a", "b:resource", "c:combat", "d:resource"));
        var d = MovementRules.FindDestinations(g, AllRevealed(g), new CaseId("a"), 4, id => g.Node(id).Type == N29.HeroesDawn.Definitions.CaseType.Combat);
        Assert.That(d.ContainsKey(new CaseId("c")), Is.True);
        Assert.That(d.ContainsKey(new CaseId("d")), Is.False);
    }

    [Test] // Review Focus 2 : une case qui ouvre du brouillard arrête le déplacement
    public void FogOpeningCase_StopsMovement()
    {
        var g = new BoardGraph(TestZones.Line("a", "b", "c", "d", "e", "f"));
        var fog = new FogOfWar(g, 2);
        fog.RevealAround(new CaseId("a")); // a, b, c révélées ; se tenir sur b révélerait d
        var d = MovementRules.FindDestinations(g, fog, new CaseId("a"), 5, _ => false);
        // b ouvre du brouillard : le déplacement s'arrête sur b (c n'est pas traversable depuis b)
        Assert.That(d.Keys.Select(k => k.Value), Is.EqualTo(new[] { "b" }));
    }

    [Test] // Review Focus 3 : pas d'aller-retour immédiat
    public void NoImmediateBacktrack()
    {
        var g = new BoardGraph(TestZones.Line("a", "b", "c"));
        var d = MovementRules.FindDestinations(g, AllRevealed(g), new CaseId("b"), 2, _ => false);
        // b→a→b interdit, b→c→b interdit : seules a et c sont atteignables
        Assert.That(d.Keys.Select(k => k.Value).OrderBy(x => x), Is.EqualTo(new[] { "a", "c" }));
    }

    [Test]
    public void Loop_ChoosesShortestPath_ThenLexicographic()
    {
        var g = new BoardGraph(TestZones.Graph(new[] { "a", "b", "c", "d" },
            new[] { ("a", "b"), ("a", "c"), ("b", "d"), ("c", "d") }));
        var d = MovementRules.FindDestinations(g, AllRevealed(g), new CaseId("a"), 2, _ => false);
        Assert.That(d[new CaseId("d")].Path.Select(p => p.Value), Is.EqualTo(new[] { "b", "d" }));
    }
}
```
- [ ] **Étape 2 : Vérifier l'échec.**
- [ ] **Étape 3 : Implémentation**
```csharp
// Domain/Board/MovePlan.cs
using System.Collections.Generic;
using N29.HeroesDawn.Core;
namespace N29.HeroesDawn.Domain.Board
{
    public sealed class MovePlan
    {
        public CaseId Target { get; }
        public IReadOnlyList<CaseId> Path { get; }
        public int Cost => Path.Count;
        public MovePlan(CaseId target, IReadOnlyList<CaseId> path) { Target = target; Path = path; }
    }
}
```
```csharp
// Domain/Board/MovementRules.cs
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Core;
namespace N29.HeroesDawn.Domain.Board
{
    /// <summary>GDD §5 — destinations en 1..PM pas, arrêts forcés, pas de retour immédiat.</summary>
    public static class MovementRules
    {
        struct Walk { public CaseId At; public CaseId Prev; public bool HasPrev; public List<CaseId> Path; }

        public static IReadOnlyDictionary<CaseId, MovePlan> FindDestinations(
            BoardGraph g, FogOfWar fog, CaseId from, int pm, Func<CaseId, bool> isBlocking)
        {
            var result = new Dictionary<CaseId, MovePlan>();
            var frontier = new List<Walk> { new Walk { At = from, HasPrev = false, Path = new List<CaseId>() } };

            for (int step = 1; step <= pm; step++)
            {
                var next = new List<Walk>();
                foreach (var w in frontier)
                {
                    foreach (var n in g.Neighbors(w.At)) // voisins triés → ordre lexicographique
                    {
                        if (w.HasPrev && n == w.Prev) continue;        // pas d'aller-retour immédiat
                        if (!fog.IsRevealed(n)) continue;              // uniquement des cases visibles
                        if (n == from) continue;                       // la case de départ n'est pas une destination
                        var path = new List<CaseId>(w.Path) { n };
                        if (!result.ContainsKey(n)) result.Add(n, new MovePlan(n, path)); // 1er trouvé = plus court
                        bool stops = isBlocking(n) || fog.WouldReveal(n);
                        if (!stops) next.Add(new Walk { At = n, Prev = w.At, HasPrev = true, Path = path });
                    }
                }
                frontier = next;
            }
            return result;
        }
    }
}
```
- [ ] **Étape 4 : Vérifier** — 5 verts.
- [ ] **Étape 5 : Commit** — `git commit -am "feat(board): destinations, arrêts forcés, chemins"`

---

### Tâche 6 : Plateau — la partie de zone (ZoneRun)

**Fichiers :** Créer `Domain/Board/ZoneRunPhase.cs`, `CaseOutcome.cs`, `BoardEvents.cs`, `ZoneRun.cs`, `BoardAction.cs` · Test `Assets/Tests/Domain/Board/ZoneRunTests.cs`

**Interfaces :**
- Consomme : tâches 3 à 5, `ContentCatalog`, `IRandom`.
- Produit :
  - `enum ZoneRunPhase { AwaitingRoll, ChoosingDestination, InCombat, InCaseInteraction, Completed }`
  - `sealed class CaseOutcome { CaseId Case; CaseType Type; int Credits; int Pr; EncounterDef? Encounter; bool Stop; }`
  - `abstract class BoardEvent` et dérivés : `DiceRolledEvent(int Value, bool Bonus)`, `PawnMovedEvent(IReadOnlyList<CaseId> Path)`, `FogRevealedEvent(IReadOnlyList<CaseId> Cases)`, `ResourceCollectedEvent(CaseId, int Credits, int Pr)`, `CaseTriggeredEvent(CaseOutcome)`, `ObjectiveCompletedEvent(string ObjectiveId)`, `ZoneValidatedEvent()`, `PmBankedEvent(int Pm)`.
  - `enum BoardActionType { Roll, BonusDie, Move, EndMove, CombatResult, CaseResolved }` et `readonly struct BoardAction(BoardActionType Type, string Arg, int Value)`.
  - `sealed class ZoneRun` :
    - `ZoneRun(ContentCatalog c, ZoneDef zone, ulong seed)` — révèle autour de l'entrée.
    - propriétés : `CaseId Position`, `int PmInHand`, `ZoneRunPhase Phase`, `DiceBank Dice`, `FogOfWar Fog`, `BoardGraph Graph`, `IReadOnlyCollection<string> CompletedObjectives`, `IReadOnlyList<BoardAction> Log`, `IReadOnlyDictionary<CaseId, MovePlan> Destinations`, `CaseOutcome? PendingCase`.
    - `IReadOnlyList<BoardEvent> Roll()` ; `IReadOnlyList<BoardEvent> UseBonusDie()` ; `IReadOnlyList<BoardEvent> MoveTo(CaseId target)` ; `IReadOnlyList<BoardEvent> EndMove()` ; `IReadOnlyList<BoardEvent> ResolveCombat(bool victory)` ; `IReadOnlyList<BoardEvent> ResolveCase(bool success)` ; `bool IsBlocking(CaseId)`.
  - Règles appliquées : ressource ramassée **en passant** (une fois) ; arrêt sur Combat → phase `InCombat`, PM restants → réserve ; arrêt sur brouillard → phase `ChoosingDestination` avec PM restants ; Mini-jeu/Événement/Repos → `InCaseInteraction` seulement si c'est la case finale ; objectif `ReachCase` rempli en arrivant ; `DefeatCombat` rempli sur victoire à cette case ; 3 objectifs → `ZoneValidatedEvent` et phase `Completed` ; défaite → retour à la case précédente du chemin.

- [ ] **Étape 1 : Tests en échec**
```csharp
using System.Linq;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;
using NUnit.Framework;

public class ZoneRunTests
{
    static ContentCatalog Content => PrototypeContent.Build();

    [Test]
    public void Roll_ProducesValueInRange_AndDestinations()
    {
        var run = new ZoneRun(Content, Content.Zone(new ZoneId("zone_ny_1_1")), seed: 1);
        var ev = run.Roll();
        var rolled = ev.OfType<DiceRolledEvent>().Single();
        Assert.That(rolled.Value, Is.InRange(1, 6));
        Assert.That(run.Phase, Is.EqualTo(ZoneRunPhase.ChoosingDestination));
        Assert.That(run.Destinations.Count, Is.GreaterThan(0));
    }

    [Test]
    public void SameSeed_SameRolls()
    {
        var z = Content.Zone(new ZoneId("zone_ny_1_1"));
        var a = new ZoneRun(Content, z, 7); var b = new ZoneRun(Content, z, 7);
        Assert.That(a.Roll().OfType<DiceRolledEvent>().Single().Value, Is.EqualTo(b.Roll().OfType<DiceRolledEvent>().Single().Value));
    }

    [Test] // ressource ramassée en passant, une seule fois
    public void PassingOverResource_CollectsOnce()
    {
        var c = Content;
        var zone = TestZones.Line("a", "b:resource", "c", "d");
        var run = ZoneRun.ForTests(c, zone, seed: 3, pmInHand: 2, revealAll: true);
        var ev = run.MoveTo(new CaseId("c"));
        Assert.That(ev.OfType<ResourceCollectedEvent>().Count(), Is.EqualTo(1));
        run = ZoneRun.ForTests(c, zone, 3, pmInHand: 0, revealAll: true, position: "c", collected: new[] { "b" });
        run.SetPmForTests(1);
        Assert.That(run.MoveTo(new CaseId("b")).OfType<ResourceCollectedEvent>().Count(), Is.EqualTo(0));
    }

    [Test] // arrêt sur combat : PM restants vers la réserve
    public void StoppingOnCombat_BanksRemainingPm()
    {
        var run = ZoneRun.ForTests(Content, TestZones.Line("a", "b:combat", "c"), 3, pmInHand: 5, revealAll: true);
        run.MoveTo(new CaseId("b"));
        Assert.That(run.Phase, Is.EqualTo(ZoneRunPhase.InCombat));
        Assert.That(run.PmInHand, Is.EqualTo(0));
        Assert.That(run.Dice.StoredPm, Is.EqualTo(4));
    }

    [Test] // défaite : recul d'une case
    public void CombatDefeat_StepsBack()
    {
        var run = ZoneRun.ForTests(Content, TestZones.Line("a", "b", "c:combat"), 3, pmInHand: 2, revealAll: true);
        run.MoveTo(new CaseId("c"));
        run.ResolveCombat(victory: false);
        Assert.That(run.Position, Is.EqualTo(new CaseId("b")));
        Assert.That(run.Phase, Is.EqualTo(ZoneRunPhase.AwaitingRoll));
    }

    [Test] // victoire : la case n'est plus bloquante
    public void CombatVictory_UnblocksCase()
    {
        var run = ZoneRun.ForTests(Content, TestZones.Line("a", "b:combat", "c"), 3, pmInHand: 1, revealAll: true);
        run.MoveTo(new CaseId("b"));
        run.ResolveCombat(victory: true);
        Assert.That(run.IsBlocking(new CaseId("b")), Is.False);
    }
}
```
- [ ] **Étape 2 : Vérifier l'échec.**
- [ ] **Étape 3 : Implémentation**
```csharp
// Domain/Board/ZoneRunPhase.cs
namespace N29.HeroesDawn.Domain.Board
{
    public enum ZoneRunPhase { AwaitingRoll, ChoosingDestination, InCombat, InCaseInteraction, Completed }
}
```
```csharp
// Domain/Board/CaseOutcome.cs
#nullable enable
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
namespace N29.HeroesDawn.Domain.Board
{
    public sealed class CaseOutcome
    {
        public CaseId Case { get; } public CaseType Type { get; } public EncounterDef? Encounter { get; }
        public CaseOutcome(CaseId c, CaseType t, EncounterDef? e) { Case = c; Type = t; Encounter = e; }
    }
}
```
```csharp
// Domain/Board/BoardEvents.cs
using System.Collections.Generic;
using N29.HeroesDawn.Core;
namespace N29.HeroesDawn.Domain.Board
{
    public abstract class BoardEvent { }
    public sealed class DiceRolledEvent : BoardEvent { public int Value { get; } public bool Bonus { get; } public DiceRolledEvent(int v, bool bonus) { Value = v; Bonus = bonus; } }
    public sealed class PawnMovedEvent : BoardEvent { public IReadOnlyList<CaseId> Path { get; } public PawnMovedEvent(IReadOnlyList<CaseId> p) { Path = p; } }
    public sealed class FogRevealedEvent : BoardEvent { public IReadOnlyList<CaseId> Cases { get; } public FogRevealedEvent(IReadOnlyList<CaseId> c) { Cases = c; } }
    public sealed class ResourceCollectedEvent : BoardEvent { public CaseId Case { get; } public int Credits { get; } public int Pr { get; } public ResourceCollectedEvent(CaseId c, int cr, int pr) { Case = c; Credits = cr; Pr = pr; } }
    public sealed class CaseTriggeredEvent : BoardEvent { public CaseOutcome Outcome { get; } public CaseTriggeredEvent(CaseOutcome o) { Outcome = o; } }
    public sealed class ObjectiveCompletedEvent : BoardEvent { public string ObjectiveId { get; } public ObjectiveCompletedEvent(string id) { ObjectiveId = id; } }
    public sealed class ZoneValidatedEvent : BoardEvent { }
    public sealed class PmBankedEvent : BoardEvent { public int Pm { get; } public PmBankedEvent(int pm) { Pm = pm; } }
    public sealed class SteppedBackEvent : BoardEvent { public CaseId To { get; } public SteppedBackEvent(CaseId to) { To = to; } }
}
```
```csharp
// Domain/Board/BoardAction.cs
namespace N29.HeroesDawn.Domain.Board
{
    public enum BoardActionType { Roll, BonusDie, Move, EndMove, CombatResult, CaseResolved }
    public readonly struct BoardAction
    {
        public BoardActionType Type { get; } public string Arg { get; } public int Value { get; }
        public BoardAction(BoardActionType type, string arg, int value) { Type = type; Arg = arg; Value = value; }
    }
}
```
```csharp
// Domain/Board/ZoneRun.cs
#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
namespace N29.HeroesDawn.Domain.Board
{
    public sealed class ZoneRun
    {
        readonly ContentCatalog _c;
        readonly IRandom _diceRng, _caseRng;
        readonly HashSet<CaseId> _collected = new HashSet<CaseId>();
        readonly HashSet<CaseId> _defeated = new HashSet<CaseId>();
        readonly HashSet<string> _objectives = new HashSet<string>();
        readonly List<BoardAction> _log = new List<BoardAction>();
        CaseId _previous;
        IReadOnlyDictionary<CaseId, MovePlan> _destinations = new Dictionary<CaseId, MovePlan>();

        public BoardGraph Graph { get; }
        public FogOfWar Fog { get; }
        public DiceBank Dice { get; }
        public CaseId Position { get; private set; }
        public int PmInHand { get; private set; }
        public ZoneRunPhase Phase { get; private set; }
        public CaseOutcome? PendingCase { get; private set; }
        public IReadOnlyDictionary<CaseId, MovePlan> Destinations => _destinations;
        public IReadOnlyCollection<string> CompletedObjectives => _objectives;
        public IReadOnlyList<BoardAction> Log => _log;

        public ZoneRun(ContentCatalog c, ZoneDef zone, ulong seed)
        {
            _c = c;
            var root = new Pcg32(seed);
            _diceRng = root.Fork(1);
            _caseRng = root.Fork(2);
            Graph = new BoardGraph(zone);
            Fog = new FogOfWar(Graph, c.Balance.FogRadius);
            Dice = new DiceBank(c.Balance);
            Position = zone.Entry;
            _previous = zone.Entry;
            Fog.RevealAround(Position);
            Phase = ZoneRunPhase.AwaitingRoll;
        }

        public bool IsBlocking(CaseId id) => Graph.Node(id).Type == CaseType.Combat && !_defeated.Contains(id);

        public IReadOnlyList<BoardEvent> Roll()
        {
            Require(ZoneRunPhase.AwaitingRoll);
            int v = _diceRng.Range(1, _c.Balance.DieFaces + 1);
            _log.Add(new BoardAction(BoardActionType.Roll, "", v));
            PmInHand += v;
            RefreshDestinations();
            Phase = ZoneRunPhase.ChoosingDestination;
            return new BoardEvent[] { new DiceRolledEvent(v, false) };
        }

        public IReadOnlyList<BoardEvent> UseBonusDie()
        {
            if (Phase != ZoneRunPhase.AwaitingRoll && Phase != ZoneRunPhase.ChoosingDestination)
                throw new InvalidOperationException($"Dé bonus impossible en phase {Phase}");
            if (!Dice.TryConsumeBonusDie()) throw new InvalidOperationException("Aucun dé bonus");
            int v = _diceRng.Range(1, _c.Balance.DieFaces + 1);
            _log.Add(new BoardAction(BoardActionType.BonusDie, "", v));
            PmInHand += v;
            RefreshDestinations();
            Phase = ZoneRunPhase.ChoosingDestination;
            return new BoardEvent[] { new DiceRolledEvent(v, true) };
        }

        public IReadOnlyList<BoardEvent> MoveTo(CaseId target)
        {
            Require(ZoneRunPhase.ChoosingDestination);
            if (!_destinations.TryGetValue(target, out var plan)) throw new InvalidOperationException($"Destination invalide : {target}");
            _log.Add(new BoardAction(BoardActionType.Move, target.Value, 0));
            var events = new List<BoardEvent> { new PawnMovedEvent(plan.Path) };

            CaseId before = Position;
            foreach (var step in plan.Path)
            {
                _previous = before; before = step;
                var node = Graph.Node(step);
                if (node.Type == CaseType.Resource && _collected.Add(step))
                {
                    int cr = _caseRng.Range(_c.Balance.ResourceCreditsMin, _c.Balance.ResourceCreditsMax + 1);
                    int pr = _caseRng.Range(5, 16);
                    events.Add(new ResourceCollectedEvent(step, cr, pr));
                }
            }
            Position = target;
            PmInHand -= plan.Cost;

            var revealed = Fog.RevealAround(Position);
            if (revealed.Count > 0) events.Add(new FogRevealedEvent(revealed));
            CompleteObjectives(ObjectiveType.ReachCase, Position, events);

            var type = Graph.Node(Position).Type;
            if (type == CaseType.Combat && IsBlocking(Position))
            {
                BankRemaining(events);
                PendingCase = new CaseOutcome(Position, type, PickEncounter(Graph.Node(Position).Rank));
                Phase = ZoneRunPhase.InCombat;
                events.Add(new CaseTriggeredEvent(PendingCase));
            }
            else if (type == CaseType.MiniGame || type == CaseType.Narrative || type == CaseType.Rest)
            {
                BankRemaining(events);
                PendingCase = new CaseOutcome(Position, type, null);
                Phase = ZoneRunPhase.InCaseInteraction;
                events.Add(new CaseTriggeredEvent(PendingCase));
            }
            else if (PmInHand > 0 && revealed.Count > 0)
            {
                RefreshDestinations();                      // arrêt de brouillard : le joueur choisit la suite
                Phase = _destinations.Count > 0 ? ZoneRunPhase.ChoosingDestination : ZoneRunPhase.AwaitingRoll;
                if (Phase == ZoneRunPhase.AwaitingRoll) BankRemaining(events);
            }
            else
            {
                BankRemaining(events);
                Phase = ZoneRunPhase.AwaitingRoll;
            }
            CheckValidated(events);
            return events;
        }

        public IReadOnlyList<BoardEvent> EndMove()
        {
            Require(ZoneRunPhase.ChoosingDestination);
            _log.Add(new BoardAction(BoardActionType.EndMove, "", 0));
            var events = new List<BoardEvent>();
            BankRemaining(events);
            Phase = ZoneRunPhase.AwaitingRoll;
            return events;
        }

        public IReadOnlyList<BoardEvent> ResolveCombat(bool victory)
        {
            Require(ZoneRunPhase.InCombat);
            _log.Add(new BoardAction(BoardActionType.CombatResult, Position.Value, victory ? 1 : 0));
            var events = new List<BoardEvent>();
            if (victory)
            {
                _defeated.Add(Position);
                CompleteObjectives(ObjectiveType.DefeatCombat, Position, events);
            }
            else
            {
                Position = _previous;
                events.Add(new SteppedBackEvent(Position));
            }
            PendingCase = null;
            Phase = ZoneRunPhase.AwaitingRoll;
            CheckValidated(events);
            return events;
        }

        public IReadOnlyList<BoardEvent> ResolveCase(bool success)
        {
            Require(ZoneRunPhase.InCaseInteraction);
            _log.Add(new BoardAction(BoardActionType.CaseResolved, Position.Value, success ? 1 : 0));
            var events = new List<BoardEvent>();
            var type = Graph.Node(Position).Type;
            if (success && type == CaseType.MiniGame) CompleteObjectives(ObjectiveType.CompleteMiniGame, Position, events);
            if (success && type == CaseType.Narrative) CompleteObjectives(ObjectiveType.ResolveNarrative, Position, events);
            PendingCase = null;
            Phase = ZoneRunPhase.AwaitingRoll;
            CheckValidated(events);
            return events;
        }

        void BankRemaining(List<BoardEvent> events)
        {
            if (PmInHand <= 0) { PmInHand = 0; return; }
            Dice.AddUnusedPm(PmInHand);
            events.Add(new PmBankedEvent(PmInHand));
            PmInHand = 0;
            _destinations = new Dictionary<CaseId, MovePlan>();
        }

        void RefreshDestinations() => _destinations = MovementRules.FindDestinations(Graph, Fog, Position, PmInHand, IsBlocking);

        EncounterDef PickEncounter(CombatRank rank)
        {
            var list = new List<EncounterDef>();
            foreach (var e in Graph.Zone.Encounters) if (e.Rank == rank) list.Add(e);
            if (list.Count == 0) foreach (var e in Graph.Zone.Encounters) list.Add(e);
            return list[_caseRng.Range(0, list.Count)];
        }

        void CompleteObjectives(ObjectiveType type, CaseId at, List<BoardEvent> events)
        {
            foreach (var o in Graph.Zone.Objectives)
                if (o.Type == type && o.Target == at && _objectives.Add(o.Id)) events.Add(new ObjectiveCompletedEvent(o.Id));
        }

        void CheckValidated(List<BoardEvent> events)
        {
            if (Phase == ZoneRunPhase.Completed || Graph.Zone.Objectives.Count == 0) return;
            if (_objectives.Count == Graph.Zone.Objectives.Count) { Phase = ZoneRunPhase.Completed; events.Add(new ZoneValidatedEvent()); }
        }

        void Require(ZoneRunPhase p) { if (Phase != p) throw new InvalidOperationException($"Phase attendue {p}, actuelle {Phase}"); }

        // --- Aides de test (internes au domaine, utilisées par les tests uniquement) ---
        public static ZoneRun ForTests(ContentCatalog c, ZoneDef zone, ulong seed, int pmInHand, bool revealAll,
                                       string? position = null, string[]? collected = null)
        {
            var run = new ZoneRun(c, zone, seed);
            if (revealAll) run.Fog.Restore(run.Graph.AllIds);
            if (position != null) { run.Position = new CaseId(position); run._previous = run.Position; }
            if (collected != null) foreach (var id in collected) run._collected.Add(new CaseId(id));
            run.SetPmForTests(pmInHand);
            return run;
        }
        public void SetPmForTests(int pm)
        {
            PmInHand = pm;
            if (pm > 0) { RefreshDestinations(); Phase = ZoneRunPhase.ChoosingDestination; }
        }
    }
}
```
- [ ] **Étape 4 : Vérifier** — 6 verts. Ajouter aussi un test de validation : sur `zone_ny_1_1`, en appelant `ForTests(..., position: "n23")`, `ResolveCombat(true)` ne valide pas la zone tant que `obj_generator` et `obj_crane` ne sont pas remplis.
- [ ] **Étape 5 : Commit** — `git commit -am "feat(board): partie de zone (ZoneRun) avec journal"`

---

### Tâche 7 : Combat — unités, stats et jauges ATB

**Fichiers :** Créer `Domain/Combat/Side.cs`, `CombatUnit.cs`, `StatusInstance.cs`, `CombatSetup.cs`, `CombatEvents.cs`, `CombatSimulation.cs` (première version : ticks et jauges) · Test `Assets/Tests/Domain/Combat/AtbTests.cs`, `Assets/Tests/Domain/Combat/CombatFixtures.cs`

**Interfaces :**
- Produit :
  - `enum Side { Ally, Enemy }` ; `sealed class StatusInstance { StatusType Type; int TurnsLeft; int Value; }`.
  - `sealed class CombatUnit` : `int Id`, `Side Side`, `string DefId`, `Family Family`, `Role Role`, `int MaxHp`, `int Hp`, `int Atk`, `int Def`, `int Spd`, `long Gauge` (milli-points, plein à `AtbGaugePoints × 1000`), `int[] Cooldowns` (A1, A2, A3), `List<StatusInstance> Statuses`, `IReadOnlyList<SkillDef> Skills`, `bool IsAlive`, `bool IsReady`, `int ActionsSinceCombo`.
  - `sealed class CombatSetup` : `ulong Seed`, `CombatRank Rank`, `IReadOnlyList<AllyEntry> Allies` (`MemberId, int Level, int FatiguePercent`), `IReadOnlyList<EnemyEntry> Enemies` (`EnemyId, int Level`), `bool Auto`.
  - Événements `CombatEvent` (`int Tick`) : `GaugeFullEvent(unitId)`, `SkillUsedEvent(actor, SkillId, targets[])`, `DamageEvent(target, amount, crit)`, `HealEvent`, `StatusAppliedEvent`, `StatusExpiredEvent`, `ShieldAbsorbedEvent`, `UnitDefeatedEvent`, `ComboAvailableEvent(a, b, comboId)`, `ComboUsedEvent`, `CombatEndedEvent(bool victory)`.
  - `sealed class CombatSimulation` : `CombatSimulation(ContentCatalog, CombatSetup)` ; `int Tick` ; `IReadOnlyList<CombatUnit> Units` ; `bool IsOver` ; `bool? Victory` ; `IReadOnlyList<CombatEvent> Step(int ticks)` ; `bool Auto { get; set; }`.
  - Stats d'une unité : base × (1 + 4 %)^(niveau−1) en entiers (boucle de multiplication `× 1040 / 1000`), × rang ennemi (80 % / 100 % / 130 %, boss PV ×3 déjà dans la définition), malus de fatigue linéaire au-delà de 60 % jusqu'à −30 % à 100 %.
  - Remplissage par tick : `Spd × AtbFactorPerSecond × TickMs` milli-points (Hâte +30 %, Gel −50 %).

- [ ] **Étape 1 : Fixtures et tests en échec**
```csharp
// Assets/Tests/Domain/Combat/CombatFixtures.cs
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Combat;

public static class CombatFixtures
{
    public static readonly ContentCatalog Content = PrototypeContent.Build();
    public static CombatSetup Setup(ulong seed = 1, bool auto = true, params string[] enemies)
    {
        var allies = new[]
        {
            new AllyEntry(new MemberId("ancre"), 1, 0), new AllyEntry(new MemberId("givre"), 1, 0),
            new AllyEntry(new MemberId("echo"), 1, 0), new AllyEntry(new MemberId("seve"), 1, 0),
        };
        var ens = new System.Collections.Generic.List<EnemyEntry>();
        foreach (var e in (enemies.Length == 0 ? new[] { "thug", "thug" } : enemies)) ens.Add(new EnemyEntry(new EnemyId(e), 1));
        return new CombatSetup(seed, CombatRank.Normal, allies, ens, auto);
    }
}
```
```csharp
// Assets/Tests/Domain/Combat/AtbTests.cs
using System.Linq;
using N29.HeroesDawn.Domain.Combat;
using NUnit.Framework;

public class AtbTests
{
    [Test] // 02_Combat — VIT 100 → un tour toutes les 2,5 s = 50 ticks
    public void Speed100_IsReadyAfter50Ticks()
    {
        var sim = new CombatSimulation(CombatFixtures.Content, CombatFixtures.Setup(auto: false));
        var u = sim.Units.First(x => x.Side == Side.Ally);
        u.Gauge = 0; u.Spd = 100;
        sim.StepSingleUnitForTests(u, 49);
        Assert.That(u.IsReady, Is.False);
        sim.StepSingleUnitForTests(u, 1);
        Assert.That(u.IsReady, Is.True);
    }

    [Test] // mode manuel : une unité alliée prête attend, les autres continuent
    public void ManualMode_ReadyAllyWaits_OthersKeepFilling()
    {
        var sim = new CombatSimulation(CombatFixtures.Content, CombatFixtures.Setup(auto: false));
        sim.Step(200);
        var ready = sim.Units.Where(x => x.Side == Side.Ally && x.IsReady).ToList();
        Assert.That(ready.Count, Is.GreaterThan(0));
        Assert.That(sim.IsOver, Is.False);
    }

    [Test]
    public void LevelAndFatigue_ScaleStats()
    {
        var content = CombatFixtures.Content;
        int lvl1 = UnitFactory.ScaleStat(1000, level: 1, content.Balance);
        int lvl2 = UnitFactory.ScaleStat(1000, level: 2, content.Balance);
        Assert.That(lvl2, Is.EqualTo(1040));
        Assert.That(UnitFactory.FatigueMultiplierPermille(60, content.Balance), Is.EqualTo(1000));
        Assert.That(UnitFactory.FatigueMultiplierPermille(100, content.Balance), Is.EqualTo(700));
        Assert.That(lvl1, Is.EqualTo(1000));
    }
}
```
- [ ] **Étape 2 : Vérifier l'échec.**
- [ ] **Étape 3 : Implémentation**
```csharp
// Domain/Combat/Side.cs
namespace N29.HeroesDawn.Domain.Combat { public enum Side { Ally, Enemy } }
```
```csharp
// Domain/Combat/StatusInstance.cs
using N29.HeroesDawn.Definitions;
namespace N29.HeroesDawn.Domain.Combat
{
    public sealed class StatusInstance
    {
        public StatusType Type { get; }
        public int TurnsLeft { get; set; }
        /// <summary>Bouclier : points restants ; autres : inutilisé.</summary>
        public int Value { get; set; }
        public StatusInstance(StatusType type, int turns, int value = 0) { Type = type; TurnsLeft = turns; Value = value; }
    }
}
```
```csharp
// Domain/Combat/CombatUnit.cs
#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Definitions;
namespace N29.HeroesDawn.Domain.Combat
{
    public sealed class CombatUnit
    {
        public int Id { get; }
        public Side Side { get; }
        public string DefId { get; }
        public Family Family { get; }
        public Role Role { get; }
        public int MaxHp { get; }
        public int Hp { get; set; }
        public int Atk { get; set; }
        public int Def { get; set; }
        public int Spd { get; set; }
        public long Gauge { get; set; }
        public long GaugeMax { get; }
        public int[] Cooldowns { get; } = new int[3];
        public List<StatusInstance> Statuses { get; } = new List<StatusInstance>();
        public IReadOnlyList<SkillDef> Skills { get; }
        public int ActionsSinceCombo { get; set; } = 99;
        public bool IsAlive => Hp > 0;
        public bool IsReady => IsAlive && Gauge >= GaugeMax;

        public CombatUnit(int id, Side side, string defId, Family family, Role role, int hp, int atk, int def, int spd, long gaugeMax, IReadOnlyList<SkillDef> skills)
        { Id = id; Side = side; DefId = defId; Family = family; Role = role; MaxHp = hp; Hp = hp; Atk = atk; Def = def; Spd = spd; GaugeMax = gaugeMax; Skills = skills; }

        public StatusInstance? Find(StatusType t)
        {
            foreach (var s in Statuses) if (s.Type == t) return s;
            return null;
        }
        public bool Has(StatusType t) => Find(t) != null;
    }
}
```
```csharp
// Domain/Combat/CombatSetup.cs
using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
namespace N29.HeroesDawn.Domain.Combat
{
    public sealed class AllyEntry { public MemberId Member { get; } public int Level { get; } public int FatiguePercent { get; } public AllyEntry(MemberId m, int level, int fatigue) { Member = m; Level = level; FatiguePercent = fatigue; } }
    public sealed class EnemyEntry { public EnemyId Enemy { get; } public int Level { get; } public EnemyEntry(EnemyId e, int level) { Enemy = e; Level = level; } }
    public sealed class CombatSetup
    {
        public ulong Seed { get; } public CombatRank Rank { get; } public IReadOnlyList<AllyEntry> Allies { get; }
        public IReadOnlyList<EnemyEntry> Enemies { get; } public bool Auto { get; }
        public CombatSetup(ulong seed, CombatRank rank, IReadOnlyList<AllyEntry> allies, IReadOnlyList<EnemyEntry> enemies, bool auto)
        { Seed = seed; Rank = rank; Allies = allies; Enemies = enemies; Auto = auto; }
    }
}
```
```csharp
// Domain/Combat/UnitFactory.cs
using N29.HeroesDawn.Definitions;
namespace N29.HeroesDawn.Domain.Combat
{
    public static class UnitFactory
    {
        public static int ScaleStat(int baseValue, int level, BalanceConfig b)
        {
            long v = baseValue;
            for (int i = 1; i < level; i++) v = v * (1000 + b.StatGrowthPermillePerLevel) / 1000;
            return (int)v;
        }

        /// <summary>03_Metahumains — malus linéaire au-delà du seuil, jusqu'à −30 % à 100 %.</summary>
        public static int FatigueMultiplierPermille(int fatigue, BalanceConfig b)
        {
            if (fatigue <= b.FatigueMalusThreshold) return 1000;
            int over = fatigue - b.FatigueMalusThreshold, span = 100 - b.FatigueMalusThreshold;
            return 1000 - b.FatigueMalusMaxPercent * 10 * over / span;
        }

        public static CombatUnit Ally(int id, MemberDef d, AllyEntry e, BalanceConfig b)
        {
            int f = FatigueMultiplierPermille(e.FatiguePercent, b);
            int S(int v) => ScaleStat(v, e.Level, b) * f / 1000;
            return new CombatUnit(id, Side.Ally, d.Id.Value, d.Family, d.Role, S(d.BaseStats.Hp), S(d.BaseStats.Atk), S(d.BaseStats.Def),
                                  d.BaseStats.Spd, (long)b.AtbGaugePoints * 1000, d.Skills);
        }

        public static CombatUnit Enemy(int id, EnemyDef d, EnemyEntry e, CombatRank rank, BalanceConfig b)
        {
            int pct = rank == CombatRank.Easy ? b.RankEasyStatPercent : rank == CombatRank.Elite || rank == CombatRank.Boss ? b.RankElitePercent : 100;
            int S(int v) => ScaleStat(v, e.Level, b) * pct / 100;
            return new CombatUnit(id, Side.Enemy, d.Id.Value, d.Family, d.Role, S(d.BaseStats.Hp), S(d.BaseStats.Atk), S(d.BaseStats.Def),
                                  d.BaseStats.Spd, (long)b.AtbGaugePoints * 1000, d.Skills);
        }
    }
}
```
```csharp
// Domain/Combat/CombatEvents.cs
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
namespace N29.HeroesDawn.Domain.Combat
{
    public abstract class CombatEvent { public int Tick { get; } protected CombatEvent(int tick) { Tick = tick; } }
    public sealed class GaugeFullEvent : CombatEvent { public int Unit { get; } public GaugeFullEvent(int t, int u) : base(t) { Unit = u; } }
    public sealed class SkillUsedEvent : CombatEvent { public int Actor { get; } public SkillId Skill { get; } public int[] Targets { get; } public SkillUsedEvent(int t, int a, SkillId s, int[] tg) : base(t) { Actor = a; Skill = s; Targets = tg; } }
    public sealed class DamageEvent : CombatEvent { public int Target { get; } public int Amount { get; } public bool Crit { get; } public DamageEvent(int t, int tg, int amount, bool crit) : base(t) { Target = tg; Amount = amount; Crit = crit; } }
    public sealed class HealEvent : CombatEvent { public int Target { get; } public int Amount { get; } public HealEvent(int t, int tg, int a) : base(t) { Target = tg; Amount = a; } }
    public sealed class ShieldAbsorbedEvent : CombatEvent { public int Target { get; } public int Amount { get; } public ShieldAbsorbedEvent(int t, int tg, int a) : base(t) { Target = tg; Amount = a; } }
    public sealed class StatusAppliedEvent : CombatEvent { public int Target { get; } public StatusType Status { get; } public int Turns { get; } public StatusAppliedEvent(int t, int tg, StatusType s, int turns) : base(t) { Target = tg; Status = s; Turns = turns; } }
    public sealed class StatusExpiredEvent : CombatEvent { public int Target { get; } public StatusType Status { get; } public StatusExpiredEvent(int t, int tg, StatusType s) : base(t) { Target = tg; Status = s; } }
    public sealed class UnitDefeatedEvent : CombatEvent { public int Unit { get; } public UnitDefeatedEvent(int t, int u) : base(t) { Unit = u; } }
    public sealed class ComboAvailableEvent : CombatEvent { public int A { get; } public int B { get; } public string ComboId { get; } public ComboAvailableEvent(int t, int a, int b, string c) : base(t) { A = a; B = b; ComboId = c; } }
    public sealed class ComboUsedEvent : CombatEvent { public int A { get; } public int B { get; } public string ComboId { get; } public ComboUsedEvent(int t, int a, int b, string c) : base(t) { A = a; B = b; ComboId = c; } }
    public sealed class CombatEndedEvent : CombatEvent { public bool Victory { get; } public CombatEndedEvent(int t, bool v) : base(t) { Victory = v; } }
}
```
```csharp
// Domain/Combat/CombatSimulation.cs — version tâche 7 (jauges) ; complétée en tâches 8 et 9
#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
namespace N29.HeroesDawn.Domain.Combat
{
    public sealed partial class CombatSimulation
    {
        readonly ContentCatalog _c;
        readonly BalanceConfig _b;
        readonly IRandom _rng;
        readonly List<CombatUnit> _units = new List<CombatUnit>();
        readonly List<CombatEvent> _pending = new List<CombatEvent>();
        public CombatSetup Setup { get; }
        public int Tick { get; private set; }
        public IReadOnlyList<CombatUnit> Units => _units;
        public bool IsOver => Victory.HasValue;
        public bool? Victory { get; private set; }
        public bool Auto { get; set; }

        public CombatSimulation(ContentCatalog c, CombatSetup setup)
        {
            _c = c; _b = c.Balance; Setup = setup; Auto = setup.Auto;
            _rng = new Pcg32(setup.Seed).Fork(3);
            int id = 0;
            foreach (var a in setup.Allies) _units.Add(UnitFactory.Ally(id++, c.Member(a.Member), a, _b));
            foreach (var e in setup.Enemies) _units.Add(UnitFactory.Enemy(id++, c.Enemy(e.Enemy), e, setup.Rank, _b));
            foreach (var u in _units) u.Gauge = u.GaugeMax * _rng.Range(0, 21) / 100; // départ 0-20 %
        }

        long FillPerTick(CombatUnit u)
        {
            long fill = (long)u.Spd * _b.AtbFactorPerSecond * _b.TickMs; // milli-points par tick
            if (u.Has(StatusType.Haste)) fill = fill * 130 / 100;
            if (u.Has(StatusType.Freeze)) fill = fill * 50 / 100;
            if (u.Has(StatusType.Bind) && u.Side == Side.Enemy && u.Find(StatusType.Bind)!.Value == 1) fill = 0; // « Cristal » / Piège
            return fill;
        }

        public IReadOnlyList<CombatEvent> Step(int ticks)
        {
            var output = new List<CombatEvent>();
            for (int i = 0; i < ticks && !IsOver; i++) StepOne(output);
            return output;
        }

        void StepOne(List<CombatEvent> output)
        {
            Tick++;
            foreach (var u in _units) // ordre stable par Id
            {
                if (!u.IsAlive || u.IsReady) continue;
                u.Gauge += FillPerTick(u);
                if (u.Gauge >= u.GaugeMax) { u.Gauge = u.GaugeMax; output.Add(new GaugeFullEvent(Tick, u.Id)); }
            }
            ResolveReadyUnits(output); // tâche 8 : IA ennemie, auto, commandes en attente
            CheckEnd(output);          // tâche 9
        }

        // Tâche 7 : bouchons remplacés en tâches 8 et 9.
        partial void ResolveReadyUnitsImpl(List<CombatEvent> output);
        partial void CheckEndImpl(List<CombatEvent> output);
        void ResolveReadyUnits(List<CombatEvent> o) => ResolveReadyUnitsImpl(o);
        void CheckEnd(List<CombatEvent> o) => CheckEndImpl(o);

        // Aide de test
        public void StepSingleUnitForTests(CombatUnit u, int ticks)
        {
            for (int i = 0; i < ticks; i++) { u.Gauge += FillPerTick(u); if (u.Gauge > u.GaugeMax) u.Gauge = u.GaugeMax; }
        }
    }
}
```
- [ ] **Étape 4 : Vérifier** — 3 verts (VIT 100 × 4 × 50 = 20 000 milli-points/tick ; 1 000 000 / 20 000 = 50 ticks).
- [ ] **Étape 5 : Commit** — `git commit -am "feat(combat): unités, stats, jauges ATB"`

---

### Tâche 8 : Combat — compétences, dégâts, statuts, commandes

**Fichiers :** Créer `Domain/Combat/DamageFormula.cs`, `Domain/Combat/CombatCommand.cs`, `Domain/Combat/CombatSimulation.Actions.cs` · Test `Assets/Tests/Domain/Combat/DamageTests.cs`, `SkillTests.cs`

**Interfaces :**
- Consomme : tâche 7.
- Produit :
  - `static class DamageFormula { int Compute(int atk, int pct, int def, int defK, bool crit, bool vulnerable, int variancePercentRoll, BalanceConfig b); }` — `variancePercentRoll` ∈ [−5, +5].
  - `enum CommandType { Skill, Combo, SetAuto, Retreat }` ; `sealed class CombatCommand { CommandType Type; int Actor; int Partner; int SlotIndex; int Target; bool Flag; }` avec fabriques `UseSkill(actor, slotIndex, target)`, `UseCombo(a, b, target)`, `SetAuto(bool)`, `Retreat()`.
  - `CombatSimulation.Submit(CombatCommand)` : mise en file, appliquée au prochain tick ; `IReadOnlyList<CombatCommand> InputLog` avec le tick d'application (`CombatInput { int Tick; CombatCommand Command; }`).
  - Règles : A1 sans recharge ; recharges décrémentées au tour de l'unité ; statuts décrémentés à la fin du tour de leur porteur ; Brûlure 5 % PV max au début du tour ; Régénération 8 % ; Choc : saute le tour (jauge remise à 0) ; Provocation : les attaques mono-cible ennemies visent le porteur ; Bouclier absorbe avant les PV ; Vulnérabilité +25 % ; Marque : prochain coup reçu critique puis retirée ; une même altération rafraîchit sa durée.

- [ ] **Étape 1 : Tests en échec**
```csharp
// Assets/Tests/Domain/Combat/DamageTests.cs
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Combat;
using NUnit.Framework;

public class DamageTests
{
    static readonly BalanceConfig B = new BalanceConfig();

    [Test] // 02_Combat — ATQ 130, 100 %, DEF 70, K 300 : réduction 189 ‰ → 105
    public void BaseFormula()
        => Assert.That(DamageFormula.Compute(130, 100, 70, 300, false, false, 0, B), Is.EqualTo(105));

    [Test]
    public void Crit_And_Vulnerable_Stack()
        => Assert.That(DamageFormula.Compute(130, 100, 70, 300, true, true, 0, B), Is.EqualTo(105 * 150 / 100 * 125 / 100));

    [Test]
    public void Variance_IsApplied_AndMinimumIsOne()
    {
        Assert.That(DamageFormula.Compute(130, 100, 70, 300, false, false, 5, B), Is.EqualTo(110));
        Assert.That(DamageFormula.Compute(1, 1, 5000, 300, false, false, -5, B), Is.EqualTo(1));
    }
}
```
```csharp
// Assets/Tests/Domain/Combat/SkillTests.cs
using System.Linq;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Combat;
using NUnit.Framework;

public class SkillTests
{
    static CombatSimulation ManualSim() => new CombatSimulation(CombatFixtures.Content, CombatFixtures.Setup(auto: false));

    [Test]
    public void UsingA1_DealsDamage_AndResetsGauge()
    {
        var sim = ManualSim();
        var givre = sim.Units.First(u => u.DefId == "givre");
        var target = sim.Units.First(u => u.Side == Side.Enemy);
        givre.Gauge = givre.GaugeMax;
        foreach (var e in sim.Units.Where(u => u.Side == Side.Enemy)) e.Gauge = 0;
        int hp = target.Hp;
        sim.Submit(CombatCommand.UseSkill(givre.Id, 0, target.Id));
        var ev = sim.Step(1);
        Assert.That(ev.OfType<DamageEvent>().Any(d => d.Target == target.Id), Is.True);
        Assert.That(target.Hp, Is.LessThan(hp));
        Assert.That(givre.Gauge, Is.LessThan(givre.GaugeMax));
    }

    [Test]
    public void SkillOnCooldown_IsRejected()
    {
        var sim = ManualSim();
        var givre = sim.Units.First(u => u.DefId == "givre");
        givre.Gauge = givre.GaugeMax; givre.Cooldowns[1] = 2;
        Assert.That(sim.CanUse(givre.Id, 1), Is.False);
        Assert.That(sim.CanUse(givre.Id, 0), Is.True);
    }

    [Test]
    public void Taunt_RedirectsSingleTargetEnemyAttacks()
    {
        var sim = ManualSim();
        var ancre = sim.Units.First(u => u.DefId == "ancre");
        ancre.Statuses.Add(new StatusInstance(StatusType.Taunt, 2));
        var enemy = sim.Units.First(u => u.Side == Side.Enemy);
        Assert.That(sim.ResolveSingleTarget(enemy, Side.Ally, preferred: sim.Units.First(u => u.DefId == "seve").Id), Is.EqualTo(ancre.Id));
    }

    [Test]
    public void Shield_AbsorbsBeforeHp()
    {
        var sim = ManualSim();
        var seve = sim.Units.First(u => u.DefId == "seve");
        seve.Statuses.Add(new StatusInstance(StatusType.Shield, 2, 100));
        int hp = seve.Hp;
        sim.ApplyDamageForTests(seve, 60);
        Assert.That(seve.Hp, Is.EqualTo(hp));
        Assert.That(seve.Find(StatusType.Shield)!.Value, Is.EqualTo(40));
    }
}
```
- [ ] **Étape 2 : Vérifier l'échec.**
- [ ] **Étape 3 : Implémentation**
```csharp
// Domain/Combat/DamageFormula.cs
using System;
using N29.HeroesDawn.Definitions;
namespace N29.HeroesDawn.Domain.Combat
{
    /// <summary>02_Combat — Dégâts = ATQ × % × (1 − DEF/(DEF+K)) × critique × vulnérabilité, ±5 %.</summary>
    public static class DamageFormula
    {
        public static int Compute(int atk, int pct, int def, int defK, bool crit, bool vulnerable, int varianceRoll, BalanceConfig b)
        {
            long raw = (long)atk * pct / 100;
            long reductionPermille = (long)def * 1000 / (def + defK);
            long dmg = raw * (1000 - reductionPermille) / 1000;
            if (crit) dmg = dmg * b.CritMultiplierPercent / 100;
            if (vulnerable) dmg = dmg * (100 + b.VulnerableBonusPercent) / 100;
            dmg = dmg * (100 + varianceRoll) / 100;
            return (int)Math.Max(1, dmg);
        }
    }
}
```
```csharp
// Domain/Combat/CombatCommand.cs
namespace N29.HeroesDawn.Domain.Combat
{
    public enum CommandType { Skill, Combo, SetAuto, Retreat }
    public sealed class CombatCommand
    {
        public CommandType Type { get; } public int Actor { get; } public int Partner { get; }
        public int SlotIndex { get; } public int Target { get; } public bool Flag { get; }
        CombatCommand(CommandType t, int actor, int partner, int slot, int target, bool flag)
        { Type = t; Actor = actor; Partner = partner; SlotIndex = slot; Target = target; Flag = flag; }
        public static CombatCommand UseSkill(int actor, int slotIndex, int target) => new CombatCommand(CommandType.Skill, actor, -1, slotIndex, target, false);
        public static CombatCommand UseCombo(int a, int b, int target) => new CombatCommand(CommandType.Combo, a, b, -1, target, false);
        public static CombatCommand SetAuto(bool on) => new CombatCommand(CommandType.SetAuto, -1, -1, -1, -1, on);
        public static CombatCommand Retreat() => new CombatCommand(CommandType.Retreat, -1, -1, -1, -1, false);
    }
    public sealed class CombatInput { public int Tick { get; } public CombatCommand Command { get; } public CombatInput(int t, CombatCommand c) { Tick = t; Command = c; } }
}
```
```csharp
// Domain/Combat/CombatSimulation.Actions.cs
#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Definitions;
namespace N29.HeroesDawn.Domain.Combat
{
    public sealed partial class CombatSimulation
    {
        readonly Queue<CombatCommand> _queue = new Queue<CombatCommand>();
        readonly List<CombatInput> _inputLog = new List<CombatInput>();
        public IReadOnlyList<CombatInput> InputLog => _inputLog;

        public void Submit(CombatCommand cmd) => _queue.Enqueue(cmd);

        public CombatUnit Unit(int id) => _units[id];

        public bool CanUse(int actorId, int slotIndex)
        {
            var u = _units[actorId];
            if (!u.IsReady || slotIndex < 0 || slotIndex >= ActiveSkillCount(u)) return false;
            if (u.Has(StatusType.Bind) && slotIndex > 0) return false;
            return u.Cooldowns[slotIndex] == 0;
        }

        static int ActiveSkillCount(CombatUnit u)
        {
            int n = 0; foreach (var s in u.Skills) if (s.Slot != SkillSlot.Passive) n++; return n;
        }

        partial void ResolveReadyUnitsImpl(List<CombatEvent> output)
        {
            // 1. Commandes du joueur (appliquées au tick courant, journalisées)
            while (_queue.Count > 0 && !IsOver)
            {
                var cmd = _queue.Dequeue();
                if (ApplyCommand(cmd, output)) _inputLog.Add(new CombatInput(Tick, cmd));
            }
            // 2. Unités prêtes gérées par l'IA (ennemis toujours, alliés si Auto)
            foreach (var u in _units)
            {
                if (IsOver) return;
                if (!u.IsReady) continue;
                if (u.Side == Side.Enemy || Auto) AiAct(u, output); // AiAct : tâche 9
            }
        }

        bool ApplyCommand(CombatCommand cmd, List<CombatEvent> output)
        {
            switch (cmd.Type)
            {
                case CommandType.SetAuto: Auto = cmd.Flag; return true;
                case CommandType.Retreat: Victory = false; output.Add(new CombatEndedEvent(Tick, false)); return true;
                case CommandType.Skill:
                    if (!CanUse(cmd.Actor, cmd.SlotIndex)) return false;
                    PerformSkill(_units[cmd.Actor], _units[cmd.Actor].Skills[cmd.SlotIndex], cmd.SlotIndex, cmd.Target, output);
                    return true;
                case CommandType.Combo: return TryPerformCombo(cmd.Actor, cmd.Partner, cmd.Target, output); // tâche 9
            }
            return false;
        }

        internal void PerformSkill(CombatUnit actor, SkillDef skill, int slotIndex, int preferredTarget, List<CombatEvent> output)
        {
            StartOfTurn(actor, output);
            if (!actor.IsAlive) return;
            if (actor.Has(StatusType.Shock))
            {
                RemoveStatus(actor, StatusType.Shock, output);
                EndOfTurn(actor, output);
                return;
            }
            var targets = ResolveTargets(actor, skill.Target, preferredTarget);
            output.Add(new SkillUsedEvent(Tick, actor.Id, skill.Id, targets.ToArray()));
            ApplyEffects(actor, skill, targets, output);
            if (slotIndex >= 0 && slotIndex < 3) actor.Cooldowns[slotIndex] = skill.Cooldown + 1; // décrémenté en fin de tour
            actor.ActionsSinceCombo++;
            EndOfTurn(actor, output);
        }

        void ApplyEffects(CombatUnit actor, SkillDef skill, List<int> targets, List<CombatEvent> output)
        {
            foreach (var tid in targets)
            {
                var t = _units[tid];
                if (!t.IsAlive) continue;
                foreach (var fx in skill.Effects)
                {
                    if (!_rng.Chance(fx.ChancePercent)) continue;
                    switch (fx.Type)
                    {
                        case EffectType.Damage:
                            bool marked = t.Has(StatusType.Mark);
                            bool crit = marked || _rng.Chance(_b.CritBasePercent);
                            int dmg = DamageFormula.Compute(actor.Atk, fx.Value, t.Def, _b.DefK, crit, t.Has(StatusType.Vulnerable),
                                                            _rng.Range(-_b.VariancePercent, _b.VariancePercent + 1), _b);
                            if (marked) RemoveStatus(t, StatusType.Mark, output);
                            DealDamage(t, dmg, crit, output);
                            break;
                        case EffectType.Heal:
                            int heal = t.MaxHp * fx.Value / 100;
                            int applied = System.Math.Min(heal, t.MaxHp - t.Hp);
                            t.Hp += applied; output.Add(new HealEvent(Tick, t.Id, applied));
                            break;
                        case EffectType.Shield:
                            ApplyStatus(t, StatusType.Shield, fx.Duration, t.MaxHp * fx.Value / 100, output);
                            break;
                        case EffectType.ApplyStatus:
                            ApplyStatus(t, fx.Status, fx.Duration, fx.Status == StatusType.Bind ? 1 : 0, output);
                            break;
                        case EffectType.GaugeDelta:
                            t.Gauge = System.Math.Max(0, System.Math.Min(t.GaugeMax, t.Gauge + (long)fx.Value * 1000));
                            break;
                    }
                }
            }
        }

        void DealDamage(CombatUnit t, int dmg, bool crit, List<CombatEvent> output)
        {
            var shield = t.Find(StatusType.Shield);
            if (shield != null)
            {
                int absorbed = System.Math.Min(shield.Value, dmg);
                shield.Value -= absorbed; dmg -= absorbed;
                output.Add(new ShieldAbsorbedEvent(Tick, t.Id, absorbed));
                if (shield.Value <= 0) RemoveStatus(t, StatusType.Shield, output);
            }
            if (dmg <= 0) return;
            t.Hp = System.Math.Max(0, t.Hp - dmg);
            output.Add(new DamageEvent(Tick, t.Id, dmg, crit));
            if (t.Hp == 0) { t.Gauge = 0; t.Statuses.Clear(); output.Add(new UnitDefeatedEvent(Tick, t.Id)); }
        }

        void ApplyStatus(CombatUnit t, StatusType s, int turns, int value, List<CombatEvent> output)
        {
            var existing = t.Find(s);
            if (existing != null) { existing.TurnsLeft = turns; existing.Value = System.Math.Max(existing.Value, value); }
            else t.Statuses.Add(new StatusInstance(s, turns, value));
            output.Add(new StatusAppliedEvent(Tick, t.Id, s, turns));
        }

        void RemoveStatus(CombatUnit t, StatusType s, List<CombatEvent> output)
        {
            var st = t.Find(s);
            if (st == null) return;
            t.Statuses.Remove(st);
            output.Add(new StatusExpiredEvent(Tick, t.Id, s));
        }

        void StartOfTurn(CombatUnit u, List<CombatEvent> output)
        {
            if (u.Has(StatusType.Burn)) DealDamage(u, System.Math.Max(1, u.MaxHp * 5 / 100), false, output);
            if (u.IsAlive && u.Has(StatusType.Regen))
            {
                int h = System.Math.Min(u.MaxHp * 8 / 100, u.MaxHp - u.Hp);
                u.Hp += h; output.Add(new HealEvent(Tick, u.Id, h));
            }
        }

        void EndOfTurn(CombatUnit u, List<CombatEvent> output)
        {
            u.Gauge = 0;
            for (int i = 0; i < 3; i++) if (u.Cooldowns[i] > 0) u.Cooldowns[i]--;
            for (int i = u.Statuses.Count - 1; i >= 0; i--)
            {
                var s = u.Statuses[i];
                if (s.Type == StatusType.Mark) continue; // retirée au coup reçu
                s.TurnsLeft--;
                if (s.TurnsLeft <= 0) { u.Statuses.RemoveAt(i); output.Add(new StatusExpiredEvent(Tick, u.Id, s.Type)); }
            }
        }

        List<int> ResolveTargets(CombatUnit actor, TargetRule rule, int preferred)
        {
            var list = new List<int>();
            Side foe = actor.Side == Side.Ally ? Side.Enemy : Side.Ally;
            switch (rule)
            {
                case TargetRule.Self: list.Add(actor.Id); break;
                case TargetRule.SingleEnemy: { int t = ResolveSingleTarget(actor, foe, preferred); if (t >= 0) list.Add(t); break; }
                case TargetRule.SingleAlly:
                    if (preferred >= 0 && _units[preferred].Side == actor.Side && _units[preferred].IsAlive) list.Add(preferred);
                    else list.Add(LowestHpRatio(actor.Side));
                    break;
                case TargetRule.AllEnemies: foreach (var u in _units) if (u.Side == foe && u.IsAlive) list.Add(u.Id); break;
                case TargetRule.AllAllies: foreach (var u in _units) if (u.Side == actor.Side && u.IsAlive) list.Add(u.Id); break;
            }
            return list;
        }

        public int ResolveSingleTarget(CombatUnit actor, Side foe, int preferred)
        {
            foreach (var u in _units) if (u.Side == foe && u.IsAlive && u.Has(StatusType.Taunt)) return u.Id;
            if (preferred >= 0 && preferred < _units.Count && _units[preferred].Side == foe && _units[preferred].IsAlive) return preferred;
            foreach (var u in _units) if (u.Side == foe && u.IsAlive) return u.Id;
            return -1;
        }

        int LowestHpRatio(Side side)
        {
            int best = -1; long bestRatio = long.MaxValue;
            foreach (var u in _units)
            {
                if (u.Side != side || !u.IsAlive) continue;
                long r = (long)u.Hp * 1000 / u.MaxHp;
                if (r < bestRatio) { bestRatio = r; best = u.Id; }
            }
            return best;
        }

        public void ApplyDamageForTests(CombatUnit t, int dmg) => DealDamage(t, dmg, false, new List<CombatEvent>());
    }
}
```
- [ ] **Étape 4 : Vérifier** — 7 verts (les tests de la tâche 7 restent verts : le `partial` de `CheckEndImpl` et `AiAct`, `TryPerformCombo` sont fournis en tâche 9 ; d'ici là, créer `CombatSimulation.Stubs.cs` avec `void AiAct(CombatUnit u, List<CombatEvent> o) { PerformSkill(u, u.Skills[0], 0, -1, o); }` et `bool TryPerformCombo(int a, int b, int t, List<CombatEvent> o) => false;` — supprimé en tâche 9).
- [ ] **Étape 5 : Commit** — `git commit -am "feat(combat): compétences, dégâts, statuts, commandes"`

---

### Tâche 9 : Combat — IA, combos, fin de combat, rejeu

**Fichiers :** Créer `Domain/Combat/CombatSimulation.Ai.cs`, `CombatSimulation.Combo.cs`, `CombatSimulation.End.cs`, `CombatResult.cs`, `CombatReplay.cs` · Supprimer `CombatSimulation.Stubs.cs` · Test `Assets/Tests/Domain/Combat/ComboTests.cs`, `ReplayTests.cs`, `EndTests.cs`

**Interfaces :**
- Produit :
  - `IReadOnlyList<(int A, int B, ComboDef Combo)> AvailableCombos()` : paires d'alliés prêts, `ActionsSinceCombo ≥ 3` pour les deux, combo trouvé par familles ; paires ordonnées (A < B) ; un seul `ComboAvailableEvent` par paire tant qu'elle reste disponible.
  - `TryPerformCombo(a, b, target, output)` : les deux consomment leur tour ; effet = `ComboDef.Effect` lancé par l'unité de plus forte ATQ ; `ActionsSinceCombo = 0` pour les deux.
  - IA alliée (auto) : combo disponible → soin si un allié < 40 % PV → A3 → A2 → A1 sur l'ennemi de plus faible PV.
  - IA ennemie : Facile = compétence disponible au hasard + cible au hasard ; Normal = A2/A3 dès que prêtes, cible la plus faible 50 % du temps ; Élite/Boss = priorités de l'IA alliée.
  - `CombatResult { bool Victory; int Ticks; IReadOnlyList<(int UnitId, int Hp)> AllyHp; ulong StateHash; }` ; `CombatSimulation.Result()`, `CombatSimulation.StateHash()`.
  - `static class CombatReplay { CombatResult Run(ContentCatalog c, CombatSetup setup, IReadOnlyList<CombatInput> inputs, int maxTicks = 20_000); }` — rejoue en soumettant chaque commande **au tick exact**.

- [ ] **Étape 1 : Tests en échec**
```csharp
// Assets/Tests/Domain/Combat/ComboTests.cs
using System.Linq;
using N29.HeroesDawn.Domain.Combat;
using NUnit.Framework;

public class ComboTests
{
    [Test] // Review Focus 4
    public void TwoReadyAllies_ExposeTheirFamilyCombo_Once()
    {
        var sim = new CombatSimulation(CombatFixtures.Content, CombatFixtures.Setup(auto: false));
        foreach (var u in sim.Units) u.Gauge = 0;
        var givre = sim.Units.First(u => u.DefId == "givre");   // Élémentaire
        var echo = sim.Units.First(u => u.DefId == "echo");     // Perception
        givre.Gauge = givre.GaugeMax; echo.Gauge = echo.GaugeMax;
        var combos = sim.AvailableCombos();
        Assert.That(combos.Count, Is.EqualTo(1));
        Assert.That(combos[0].Combo.Id, Is.EqualTo("weak_point"));
        var ev1 = sim.Step(1); var ev2 = sim.Step(1);
        Assert.That(ev1.Concat(ev2).OfType<ComboAvailableEvent>().Count(), Is.EqualTo(1));
    }

    [Test]
    public void UsingCombo_ConsumesBothTurns()
    {
        var sim = new CombatSimulation(CombatFixtures.Content, CombatFixtures.Setup(auto: false));
        foreach (var u in sim.Units) u.Gauge = 0;
        var givre = sim.Units.First(u => u.DefId == "givre"); var echo = sim.Units.First(u => u.DefId == "echo");
        givre.Gauge = givre.GaugeMax; echo.Gauge = echo.GaugeMax;
        var enemy = sim.Units.First(u => u.Side == Side.Enemy);
        sim.Submit(CombatCommand.UseCombo(givre.Id, echo.Id, enemy.Id));
        var ev = sim.Step(1);
        Assert.That(ev.OfType<ComboUsedEvent>().Count(), Is.EqualTo(1));
        Assert.That(givre.IsReady || echo.IsReady, Is.False);
    }
}
```
```csharp
// Assets/Tests/Domain/Combat/EndTests.cs
using System.Linq;
using N29.HeroesDawn.Domain.Combat;
using NUnit.Framework;

public class EndTests
{
    [Test]
    public void AutoCombat_EndsWithinThreeMinutes()
    {
        var sim = new CombatSimulation(CombatFixtures.Content, CombatFixtures.Setup(seed: 5, auto: true));
        sim.Step(3600); // 180 s
        Assert.That(sim.IsOver, Is.True);
    }

    [Test]
    public void Retreat_IsDefeat()
    {
        var sim = new CombatSimulation(CombatFixtures.Content, CombatFixtures.Setup(auto: false));
        sim.Submit(CombatCommand.Retreat());
        var ev = sim.Step(1);
        Assert.That(sim.Victory, Is.False);
        Assert.That(ev.OfType<CombatEndedEvent>().Single().Victory, Is.False);
    }
}
```
```csharp
// Assets/Tests/Domain/Combat/ReplayTests.cs
using System.Linq;
using N29.HeroesDawn.Domain.Combat;
using NUnit.Framework;

public class ReplayTests
{
    [Test] // Review Focus 5 : joueur manuel qui agit tard, puis rejeu identique
    public void ManualSession_ReplaysToSameHash()
    {
        var setup = CombatFixtures.Setup(seed: 11, auto: false);
        var live = new CombatSimulation(CombatFixtures.Content, setup);
        int guard = 0;
        while (!live.IsOver && guard++ < 20000)
        {
            live.Step(7); // le joueur « réfléchit » 7 ticks entre chaque décision
            var ready = live.Units.FirstOrDefault(u => u.Side == Side.Ally && u.IsReady);
            if (ready != null)
            {
                var target = live.Units.First(u => u.Side == Side.Enemy && u.IsAlive);
                live.Submit(CombatCommand.UseSkill(ready.Id, 0, target.Id));
            }
        }
        var liveResult = live.Result();
        var replay = CombatReplay.Run(CombatFixtures.Content, setup, live.InputLog);
        Assert.That(replay.StateHash, Is.EqualTo(liveResult.StateHash));
        Assert.That(replay.Victory, Is.EqualTo(liveResult.Victory));
    }
}
```
- [ ] **Étape 2 : Vérifier l'échec.**
- [ ] **Étape 3 : Implémentation**
```csharp
// Domain/Combat/CombatSimulation.Ai.cs
using System.Collections.Generic;
using N29.HeroesDawn.Definitions;
namespace N29.HeroesDawn.Domain.Combat
{
    public sealed partial class CombatSimulation
    {
        void AiAct(CombatUnit u, List<CombatEvent> output)
        {
            bool smart = u.Side == Side.Ally || Setup.Rank == CombatRank.Elite || Setup.Rank == CombatRank.Boss;
            if (u.Side == Side.Ally)
            {
                foreach (var (a, b, _) in AvailableCombos())
                    if (a == u.Id || b == u.Id) { TryPerformCombo(a, b, WeakestFoe(u.Side), output); return; }
            }
            int slot = 0;
            if (smart)
            {
                int healSlot = FindSlot(u, EffectType.Heal);
                if (healSlot >= 0 && LowestHpPermille(u.Side) < 400) slot = healSlot;
                else if (CanUseSlot(u, 2)) slot = 2;
                else if (CanUseSlot(u, 1)) slot = 1;
                PerformSkill(u, u.Skills[slot], slot, WeakestFoe(u.Side), output);
                return;
            }
            if (Setup.Rank == CombatRank.Normal)
            {
                slot = CanUseSlot(u, 2) ? 2 : CanUseSlot(u, 1) ? 1 : 0;
                int target = _rng.Chance(50) ? WeakestFoe(u.Side) : RandomFoe(u.Side);
                PerformSkill(u, u.Skills[slot], slot, target, output);
                return;
            }
            // Facile : compétence disponible au hasard, cible au hasard
            var usable = new List<int>();
            for (int i = 0; i < ActiveSkillCount(u); i++) if (CanUseSlot(u, i)) usable.Add(i);
            slot = usable[_rng.Range(0, usable.Count)];
            PerformSkill(u, u.Skills[slot], slot, RandomFoe(u.Side), output);
        }

        bool CanUseSlot(CombatUnit u, int slot) => slot < ActiveSkillCount(u) && u.Cooldowns[slot] == 0 && !(u.Has(StatusType.Bind) && slot > 0);

        static int FindSlot(CombatUnit u, EffectType t)
        {
            for (int i = 0; i < u.Skills.Count; i++) foreach (var fx in u.Skills[i].Effects) if (fx.Type == t) return i;
            return -1;
        }

        int WeakestFoe(Side self)
        {
            Side foe = self == Side.Ally ? Side.Enemy : Side.Ally;
            int best = -1; int bestHp = int.MaxValue;
            foreach (var x in _units) if (x.Side == foe && x.IsAlive && x.Hp < bestHp) { bestHp = x.Hp; best = x.Id; }
            return best;
        }

        int RandomFoe(Side self)
        {
            Side foe = self == Side.Ally ? Side.Enemy : Side.Ally;
            var alive = new List<int>();
            foreach (var x in _units) if (x.Side == foe && x.IsAlive) alive.Add(x.Id);
            return alive.Count == 0 ? -1 : alive[_rng.Range(0, alive.Count)];
        }

        long LowestHpPermille(Side side)
        {
            long best = 1000;
            foreach (var x in _units) if (x.Side == side && x.IsAlive) best = System.Math.Min(best, (long)x.Hp * 1000 / x.MaxHp);
            return best;
        }
    }
}
```
```csharp
// Domain/Combat/CombatSimulation.Combo.cs
#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Definitions;
namespace N29.HeroesDawn.Domain.Combat
{
    public sealed partial class CombatSimulation
    {
        readonly HashSet<long> _announcedCombos = new HashSet<long>();

        public IReadOnlyList<(int A, int B, ComboDef Combo)> AvailableCombos()
        {
            var list = new List<(int, int, ComboDef)>();
            for (int i = 0; i < _units.Count; i++)
            {
                var a = _units[i];
                if (a.Side != Side.Ally || !a.IsReady || a.ActionsSinceCombo < _b.ComboCooldownActions) continue;
                for (int j = i + 1; j < _units.Count; j++)
                {
                    var b = _units[j];
                    if (b.Side != Side.Ally || !b.IsReady || b.ActionsSinceCombo < _b.ComboCooldownActions) continue;
                    var combo = _c.FindCombo(a.Family, b.Family);
                    if (combo != null) list.Add((a.Id, b.Id, combo));
                }
            }
            return list;
        }

        void AnnounceCombos(List<CombatEvent> output)
        {
            var current = new HashSet<long>();
            foreach (var (a, b, combo) in AvailableCombos())
            {
                long key = ((long)a << 32) | (uint)b;
                current.Add(key);
                if (_announcedCombos.Add(key)) output.Add(new ComboAvailableEvent(Tick, a, b, combo.Id));
            }
            _announcedCombos.IntersectWith(current);
        }

        bool TryPerformCombo(int a, int b, int target, List<CombatEvent> output)
        {
            ComboDef? combo = null;
            foreach (var (x, y, c) in AvailableCombos()) if ((x == a && y == b) || (x == b && y == a)) combo = c;
            if (combo == null) return false;
            var ua = _units[a]; var ub = _units[b];
            var caster = ua.Atk >= ub.Atk ? ua : ub;
            var other = caster == ua ? ub : ua;
            output.Add(new ComboUsedEvent(Tick, a, b, combo.Id));
            PerformSkill(caster, combo.Effect, -1, target, output);
            EndOfTurn(other, output);
            ua.ActionsSinceCombo = 0; ub.ActionsSinceCombo = 0;
            return true;
        }
    }
}
```
```csharp
// Domain/Combat/CombatSimulation.End.cs
using System.Collections.Generic;
using N29.HeroesDawn.Core;
namespace N29.HeroesDawn.Domain.Combat
{
    public sealed partial class CombatSimulation
    {
        partial void CheckEndImpl(List<CombatEvent> output)
        {
            if (IsOver) return;
            AnnounceCombos(output);
            bool alliesAlive = false, enemiesAlive = false;
            foreach (var u in _units) { if (!u.IsAlive) continue; if (u.Side == Side.Ally) alliesAlive = true; else enemiesAlive = true; }
            if (!enemiesAlive) { Victory = true; output.Add(new CombatEndedEvent(Tick, true)); }
            else if (!alliesAlive) { Victory = false; output.Add(new CombatEndedEvent(Tick, false)); }
        }

        public ulong StateHash()
        {
            var h = new Fnv1a64();
            h.Add(Tick);
            foreach (var u in _units)
            {
                h.Add(u.Id); h.Add(u.Hp); h.Add(u.Gauge);
                foreach (var s in u.Statuses) { h.Add((int)s.Type); h.Add(s.TurnsLeft); h.Add(s.Value); }
            }
            h.Add(Victory == true ? 1 : Victory == false ? 2 : 0);
            return h.Value;
        }

        public CombatResult Result()
        {
            var hp = new List<(int, int)>();
            foreach (var u in _units) if (u.Side == Side.Ally) hp.Add((u.Id, u.Hp));
            return new CombatResult(Victory == true, Tick, hp, StateHash());
        }
    }
}
```
```csharp
// Domain/Combat/CombatResult.cs
using System.Collections.Generic;
namespace N29.HeroesDawn.Domain.Combat
{
    public sealed class CombatResult
    {
        public bool Victory { get; } public int Ticks { get; } public IReadOnlyList<(int UnitId, int Hp)> AllyHp { get; } public ulong StateHash { get; }
        public CombatResult(bool v, int ticks, IReadOnlyList<(int, int)> hp, ulong hash) { Victory = v; Ticks = ticks; AllyHp = hp; StateHash = hash; }
    }
}
```
```csharp
// Domain/Combat/CombatReplay.cs
using System.Collections.Generic;
using N29.HeroesDawn.Definitions;
namespace N29.HeroesDawn.Domain.Combat
{
    public static class CombatReplay
    {
        public static CombatResult Run(ContentCatalog c, CombatSetup setup, IReadOnlyList<CombatInput> inputs, int maxTicks = 20_000)
        {
            var sim = new CombatSimulation(c, setup);
            int next = 0;
            while (!sim.IsOver && sim.Tick < maxTicks)
            {
                // Une commande journalisée au tick T a été appliquée pendant l'exécution du tick T :
                // on la soumet juste avant d'exécuter ce tick.
                while (next < inputs.Count && inputs[next].Tick == sim.Tick + 1) sim.Submit(inputs[next++].Command);
                sim.Step(1);
            }
            return sim.Result();
        }
    }
}
```
- [ ] **Étape 4 : Supprimer `CombatSimulation.Stubs.cs`, vérifier** — tous les tests du combat verts (`dotnet test --filter Combat`). Si `ManualSession_ReplaysToSameHash` échoue, vérifier que `InputLog` enregistre bien `Tick` **après** l'incrément (c'est le tick d'application).
- [ ] **Étape 5 : Commit** — `git commit -am "feat(combat): IA, combos, fin de combat, rejeu déterministe"`

---

### Tâche 10 : Application — session, ports, messages, use cases du plateau

**Fichiers :**
- Créer : `Assets/_Project/Application/Ports/IClock.cs`, `ISeedProvider.cs` ; `Session/GameSession.cs`, `Session/TeamMember.cs`, `Session/Wallet.cs` ; `Features/Board/Messages/BoardMessages.cs` ; `Features/Board/UseCases/OpenZoneUseCase.cs`, `RollDiceUseCase.cs`, `UseBonusDieUseCase.cs`, `MoveToUseCase.cs`, `EndMoveUseCase.cs`, `ResolveCaseUseCase.cs` ; `Features/Board/BoardEventPublisher.cs`
- Créer (Infrastructure) : `Assets/_Project/Infrastructure/Local/LocalSeedProvider.cs`, `SystemClock.cs`
- Test : `Assets/Tests/Domain/Application/BoardUseCaseTests.cs`

**Interfaces :**
- Consomme : `ZoneRun`, `BoardEvent`s, `ContentCatalog`.
- Produit :
  - `ISeedProvider { ulong NextSeed(string purpose); }` (local : horodatage + compteur ; M2 : Cloud Code).
  - `sealed class Wallet { int Credits; int Pr; void Add(int credits, int pr); }`.
  - `sealed class TeamMember { MemberId Id; int Level; int Fatigue; }`.
  - `sealed class GameSession { ContentCatalog Content; Wallet Wallet; List<TeamMember> Team; ZoneRun? Run; }` — prototype : équipe de départ Ancre, Givre, Écho, Sève (niv. 5).
  - Messages (`readonly struct`) : `ZoneOpened(ZoneId)`, `DiceRolled(int Value, bool Bonus)`, `DestinationsChanged(IReadOnlyList<CaseId> Targets, int PmInHand)`, `PawnMoved(IReadOnlyList<CaseId> Path)`, `FogRevealed(IReadOnlyList<CaseId> Cases)`, `ResourceCollected(CaseId Case, int Credits, int Pr)`, `CaseTriggered(CaseId Case, CaseType Type)`, `ObjectiveCompleted(string ObjectiveId)`, `ZoneValidated(ZoneId)`, `DiceBankChanged(int StoredPm, int BonusDice)`, `SteppedBack(CaseId To)`, `CombatRequested(CaseId Case, EncounterDef Encounter, CombatRank Rank)`, `WalletChanged(int Credits, int Pr)`.
  - `BoardEventPublisher.Publish(IReadOnlyList<BoardEvent>)` traduit les événements du domaine en messages et publie aussi `DestinationsChanged` et `DiceBankChanged` après chaque action.
  - Use cases (interfaces + implémentations, `Result` synchrones au M1) : `IOpenZoneUseCase.Execute(ZoneId)`, `IRollDiceUseCase.Execute()`, `IUseBonusDieUseCase.Execute()`, `IMoveToUseCase.Execute(CaseId)`, `IEndMoveUseCase.Execute()`, `IResolveCaseUseCase.Execute(bool success)`.

- [ ] **Étape 1 : Test en échec**
```csharp
using System.Collections.Generic;
using System.Linq;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Board;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using NUnit.Framework;

public class BoardUseCaseTests
{
    [Test]
    public void OpenThenRoll_PublishesDiceAndDestinations()
    {
        var session = new GameSession(PrototypeContent.Build());
        var pubs = new BoardPublishers();
        var events = new BoardEventPublisher(session, pubs);
        new OpenZoneUseCase(session, new FixedSeed(3), events).Execute(new ZoneId("zone_ny_1_1"));
        var r = new RollDiceUseCase(session, events).Execute();
        Assert.That(r.IsSuccess, Is.True);
        Assert.That(((RecordingPublisher<DiceRolled>)pubs.DiceRolled).Sent.Count, Is.EqualTo(1));
        Assert.That(((RecordingPublisher<DestinationsChanged>)pubs.DestinationsChanged).Sent.Last().Targets.Count, Is.GreaterThan(0));
    }

    [Test]
    public void Roll_WhenNotAllowed_ReturnsFailure_NotException()
    {
        var session = new GameSession(PrototypeContent.Build());
        var events = new BoardEventPublisher(session, new BoardPublishers());
        new OpenZoneUseCase(session, new FixedSeed(3), events).Execute(new ZoneId("zone_ny_1_1"));
        var roll = new RollDiceUseCase(session, events);
        roll.Execute();
        Assert.That(roll.Execute().IsFailure, Is.True);
    }

    sealed class FixedSeed : N29.HeroesDawn.Application.Ports.ISeedProvider { readonly ulong _s; public FixedSeed(ulong s) { _s = s; } public ulong NextSeed(string p) => _s; }
}
```
`BoardPublishers` regroupe les publieurs : en test, son constructeur sans argument crée des `RecordingPublisher<T>` ; en production, VContainer injecte les `IPublisher<T>` de MessagePipe.

- [ ] **Étape 2 : Vérifier l'échec** (Test Runner Unity : l'assembly de tests référence `Application` et `MessagePipe`).
- [ ] **Étape 3 : Implémentation** (extraits complets des fichiers clés)
```csharp
// Application/Ports/ISeedProvider.cs
namespace N29.HeroesDawn.Application.Ports { public interface ISeedProvider { ulong NextSeed(string purpose); } }
```
```csharp
// Application/Session/Wallet.cs, TeamMember.cs, GameSession.cs
using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;
namespace N29.HeroesDawn.Application.Session
{
    public sealed class Wallet { public int Credits { get; private set; } public int Pr { get; private set; } public void Add(int credits, int pr) { Credits += credits; Pr += pr; } }
    public sealed class TeamMember { public MemberId Id { get; } public int Level { get; set; } public int Fatigue { get; set; } public TeamMember(MemberId id, int level) { Id = id; Level = level; } }
    public sealed class GameSession
    {
        public ContentCatalog Content { get; }
        public Wallet Wallet { get; } = new Wallet();
        public List<TeamMember> Team { get; } = new List<TeamMember>();
        public ZoneRun? Run { get; set; }
        public GameSession(ContentCatalog content)
        {
            Content = content;
            foreach (var id in new[] { "ancre", "givre", "echo", "seve" }) Team.Add(new TeamMember(new MemberId(id), 5));
        }
    }
}
```
```csharp
// Application/Features/Board/Messages/BoardMessages.cs (un fichier par message dans le code final)
using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
namespace N29.HeroesDawn.Application.Features.Board
{
    public readonly struct ZoneOpened { public readonly ZoneId Zone; public ZoneOpened(ZoneId z) { Zone = z; } }
    public readonly struct DiceRolled { public readonly int Value; public readonly bool Bonus; public DiceRolled(int v, bool b) { Value = v; Bonus = b; } }
    public readonly struct DestinationsChanged { public readonly IReadOnlyList<CaseId> Targets; public readonly int PmInHand; public DestinationsChanged(IReadOnlyList<CaseId> t, int pm) { Targets = t; PmInHand = pm; } }
    public readonly struct PawnMoved { public readonly IReadOnlyList<CaseId> Path; public PawnMoved(IReadOnlyList<CaseId> p) { Path = p; } }
    public readonly struct FogRevealed { public readonly IReadOnlyList<CaseId> Cases; public FogRevealed(IReadOnlyList<CaseId> c) { Cases = c; } }
    public readonly struct ResourceCollected { public readonly CaseId Case; public readonly int Credits; public readonly int Pr; public ResourceCollected(CaseId c, int cr, int pr) { Case = c; Credits = cr; Pr = pr; } }
    public readonly struct CaseTriggered { public readonly CaseId Case; public readonly CaseType Type; public CaseTriggered(CaseId c, CaseType t) { Case = c; Type = t; } }
    public readonly struct CombatRequested { public readonly CaseId Case; public readonly EncounterDef Encounter; public readonly CombatRank Rank; public CombatRequested(CaseId c, EncounterDef e, CombatRank r) { Case = c; Encounter = e; Rank = r; } }
    public readonly struct ObjectiveCompleted { public readonly string ObjectiveId; public ObjectiveCompleted(string id) { ObjectiveId = id; } }
    public readonly struct ZoneValidated { public readonly ZoneId Zone; public ZoneValidated(ZoneId z) { Zone = z; } }
    public readonly struct DiceBankChanged { public readonly int StoredPm; public readonly int BonusDice; public DiceBankChanged(int s, int b) { StoredPm = s; BonusDice = b; } }
    public readonly struct SteppedBack { public readonly CaseId To; public SteppedBack(CaseId to) { To = to; } }
    public readonly struct WalletChanged { public readonly int Credits; public readonly int Pr; public WalletChanged(int c, int p) { Credits = c; Pr = p; } }
}
```
```csharp
// Application/Features/Board/BoardPublishers.cs
using MessagePipe;
namespace N29.HeroesDawn.Application.Features.Board
{
    public sealed class BoardPublishers
    {
        public IPublisher<ZoneOpened> ZoneOpened { get; } public IPublisher<DiceRolled> DiceRolled { get; }
        public IPublisher<DestinationsChanged> DestinationsChanged { get; } public IPublisher<PawnMoved> PawnMoved { get; }
        public IPublisher<FogRevealed> FogRevealed { get; } public IPublisher<ResourceCollected> ResourceCollected { get; }
        public IPublisher<CaseTriggered> CaseTriggered { get; } public IPublisher<CombatRequested> CombatRequested { get; }
        public IPublisher<ObjectiveCompleted> ObjectiveCompleted { get; } public IPublisher<ZoneValidated> ZoneValidated { get; }
        public IPublisher<DiceBankChanged> DiceBankChanged { get; } public IPublisher<SteppedBack> SteppedBack { get; }
        public IPublisher<WalletChanged> WalletChanged { get; }

        [VContainer.Inject]
        public BoardPublishers(IPublisher<ZoneOpened> a, IPublisher<DiceRolled> b, IPublisher<DestinationsChanged> c, IPublisher<PawnMoved> d,
            IPublisher<FogRevealed> e, IPublisher<ResourceCollected> f, IPublisher<CaseTriggered> g, IPublisher<CombatRequested> h,
            IPublisher<ObjectiveCompleted> i, IPublisher<ZoneValidated> j, IPublisher<DiceBankChanged> k, IPublisher<SteppedBack> l, IPublisher<WalletChanged> m)
        { ZoneOpened = a; DiceRolled = b; DestinationsChanged = c; PawnMoved = d; FogRevealed = e; ResourceCollected = f; CaseTriggered = g;
          CombatRequested = h; ObjectiveCompleted = i; ZoneValidated = j; DiceBankChanged = k; SteppedBack = l; WalletChanged = m; }

        /// <summary>Constructeur de test : publieurs factices.</summary>
        public BoardPublishers() : this(new RecordingPublisher<ZoneOpened>(), new RecordingPublisher<DiceRolled>(), new RecordingPublisher<DestinationsChanged>(), new RecordingPublisher<PawnMoved>(),
            new RecordingPublisher<FogRevealed>(), new RecordingPublisher<ResourceCollected>(), new RecordingPublisher<CaseTriggered>(), new RecordingPublisher<CombatRequested>(),
            new RecordingPublisher<ObjectiveCompleted>(), new RecordingPublisher<ZoneValidated>(), new RecordingPublisher<DiceBankChanged>(), new RecordingPublisher<SteppedBack>(), new RecordingPublisher<WalletChanged>()) { }

    }

    /// <summary>Publieur qui enregistre les messages (tests, outils hors Unity).</summary>
    public sealed class RecordingPublisher<T> : IPublisher<T>
    {
        public readonly System.Collections.Generic.List<T> Sent = new System.Collections.Generic.List<T>();
        public void Publish(T message) => Sent.Add(message);
    }
}
```

```csharp
// Application/Features/Board/BoardEventPublisher.cs
using System.Collections.Generic;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Domain.Board;
namespace N29.HeroesDawn.Application.Features.Board
{
    public sealed class BoardEventPublisher
    {
        readonly GameSession _s; readonly BoardPublishers _p;
        public BoardEventPublisher(GameSession s, BoardPublishers p) { _s = s; _p = p; }

        public void Publish(IReadOnlyList<BoardEvent> events)
        {
            var run = _s.Run!;
            foreach (var e in events)
            {
                switch (e)
                {
                    case DiceRolledEvent d: _p.DiceRolled.Publish(new DiceRolled(d.Value, d.Bonus)); break;
                    case PawnMovedEvent m: _p.PawnMoved.Publish(new PawnMoved(m.Path)); break;
                    case FogRevealedEvent f: _p.FogRevealed.Publish(new FogRevealed(f.Cases)); break;
                    case ResourceCollectedEvent r:
                        _s.Wallet.Add(r.Credits, r.Pr);
                        _p.ResourceCollected.Publish(new ResourceCollected(r.Case, r.Credits, r.Pr));
                        _p.WalletChanged.Publish(new WalletChanged(_s.Wallet.Credits, _s.Wallet.Pr));
                        break;
                    case CaseTriggeredEvent t:
                        _p.CaseTriggered.Publish(new CaseTriggered(t.Outcome.Case, t.Outcome.Type));
                        if (t.Outcome.Encounter != null)
                            _p.CombatRequested.Publish(new CombatRequested(t.Outcome.Case, t.Outcome.Encounter, run.Graph.Node(t.Outcome.Case).Rank));
                        break;
                    case ObjectiveCompletedEvent o: _p.ObjectiveCompleted.Publish(new ObjectiveCompleted(o.ObjectiveId)); break;
                    case ZoneValidatedEvent _: _p.ZoneValidated.Publish(new ZoneValidated(run.Graph.Zone.Id)); break;
                    case SteppedBackEvent b: _p.SteppedBack.Publish(new SteppedBack(b.To)); break;
                }
            }
            var targets = new List<CaseId>(run.Destinations.Keys); targets.Sort();
            _p.DestinationsChanged.Publish(new DestinationsChanged(targets, run.PmInHand));
            _p.DiceBankChanged.Publish(new DiceBankChanged(run.Dice.StoredPm, run.Dice.BonusDice));
        }
    }
}
```
```csharp
// Application/Features/Board/UseCases/*.cs — même gabarit pour chaque use case
using System;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Domain.Board;
namespace N29.HeroesDawn.Application.Features.Board
{
    public interface IOpenZoneUseCase { Result Execute(ZoneId zone); }
    public sealed class OpenZoneUseCase : IOpenZoneUseCase
    {
        readonly GameSession _s; readonly ISeedProvider _seeds; readonly BoardEventPublisher _events;
        public OpenZoneUseCase(GameSession s, ISeedProvider seeds, BoardEventPublisher events) { _s = s; _seeds = seeds; _events = events; }
        public Result Execute(ZoneId zone)
        {
            _s.Run = new ZoneRun(_s.Content, _s.Content.Zone(zone), _seeds.NextSeed("zone"));
            _events.Publish(new BoardEvent[] { new FogRevealedEvent(new System.Collections.Generic.List<CaseId>(_s.Run.Fog.Revealed)) });
            return Result.Ok();
        }
    }

    static class Guarded
    {
        public static readonly DomainError NotAllowed = new DomainError("board.not_allowed", "errors.board.not_allowed");
        public static Result Run(GameSession s, BoardEventPublisher events, Func<ZoneRun, System.Collections.Generic.IReadOnlyList<BoardEvent>> action)
        {
            if (s.Run == null) return Result.Fail(NotAllowed);
            try { events.Publish(action(s.Run)); return Result.Ok(); }
            catch (InvalidOperationException) { return Result.Fail(NotAllowed); } // action refusée par les règles (phase, destination)
        }
    }

    public interface IRollDiceUseCase { Result Execute(); }
    public sealed class RollDiceUseCase : IRollDiceUseCase
    {
        readonly GameSession _s; readonly BoardEventPublisher _e;
        public RollDiceUseCase(GameSession s, BoardEventPublisher e) { _s = s; _e = e; }
        public Result Execute() => Guarded.Run(_s, _e, r => r.Roll());
    }
    public interface IUseBonusDieUseCase { Result Execute(); }
    public sealed class UseBonusDieUseCase : IUseBonusDieUseCase
    {
        readonly GameSession _s; readonly BoardEventPublisher _e;
        public UseBonusDieUseCase(GameSession s, BoardEventPublisher e) { _s = s; _e = e; }
        public Result Execute() => Guarded.Run(_s, _e, r => r.UseBonusDie());
    }
    public interface IMoveToUseCase { Result Execute(CaseId target); }
    public sealed class MoveToUseCase : IMoveToUseCase
    {
        readonly GameSession _s; readonly BoardEventPublisher _e;
        public MoveToUseCase(GameSession s, BoardEventPublisher e) { _s = s; _e = e; }
        public Result Execute(CaseId target) => Guarded.Run(_s, _e, r => r.MoveTo(target));
    }
    public interface IEndMoveUseCase { Result Execute(); }
    public sealed class EndMoveUseCase : IEndMoveUseCase
    {
        readonly GameSession _s; readonly BoardEventPublisher _e;
        public EndMoveUseCase(GameSession s, BoardEventPublisher e) { _s = s; _e = e; }
        public Result Execute() => Guarded.Run(_s, _e, r => r.EndMove());
    }
    public interface IResolveCaseUseCase { Result Execute(bool success); }
    public sealed class ResolveCaseUseCase : IResolveCaseUseCase
    {
        readonly GameSession _s; readonly BoardEventPublisher _e;
        public ResolveCaseUseCase(GameSession s, BoardEventPublisher e) { _s = s; _e = e; }
        public Result Execute(bool success) => Guarded.Run(_s, _e, r =>
        {
            if (success && r.PendingCase?.Type == N29.HeroesDawn.Definitions.CaseType.Rest)
                foreach (var m in _s.Team) m.Fatigue = Math.Max(0, m.Fatigue - _s.Content.Balance.RestFatigueReduction);
            return r.ResolveCase(success);
        });
    }
}
```
```csharp
// Infrastructure/Local/LocalSeedProvider.cs
using N29.HeroesDawn.Application.Ports;
namespace N29.HeroesDawn.Infrastructure.Local
{
    public sealed class LocalSeedProvider : ISeedProvider
    {
        ulong _counter;
        public ulong NextSeed(string purpose) => (ulong)System.DateTime.UtcNow.Ticks ^ (++_counter * 0x9E3779B97F4A7C15UL);
    }
}
```
- [ ] **Étape 4 : Vérifier** — Test Runner EditMode : 2 verts.
- [ ] **Étape 5 : Commit** — `git commit -am "feat(app): session, messages et use cases du plateau"`

---

### Tâche 11 : Application — CombatRunner et résolution du combat

**Fichiers :** Créer `Assets/_Project/Application/Features/Combat/CombatRunner.cs`, `Messages/CombatMessages.cs`, `UseCases/StartCombatUseCase.cs`, `ResolveCombatUseCase.cs`, `CombatCommandsUseCase.cs` · Test `Assets/Tests/Domain/Application/CombatRunnerTests.cs`

**Interfaces :**
- Consomme : `CombatSimulation`, `CombatReplay`, `GameSession`, `ZoneRun.ResolveCombat`, `CombatRequested`.
- Produit :
  - Messages : `CombatStarted(CombatSimulation Sim)`, `CombatEventBatch(IReadOnlyList<CombatEvent> Events, int Tick)`, `CombatFinished(bool Victory, int Credits, IReadOnlyList<MemberFatigue> Fatigue)` avec `MemberFatigue(MemberId, int Before, int After)`.
  - `sealed class CombatRunner` : `bool IsRunning`, `CombatSimulation? Sim`, `int SpeedMultiplier` (1 ou 2), `void Start(CombatSetup)`, `void Advance(int deltaMs)` (accumule, exécute les ticks entiers × vitesse, publie `CombatEventBatch` s'il y a des événements, et `CombatFinished` via `ResolveCombatUseCase` à la fin), `float GaugeInterpolation` (0-1 entre deux ticks, pour l'affichage) — **seul `float` autorisé, côté affichage, jamais relu par la simulation**.
  - `IStartCombatUseCase.Execute(CombatRequested)` : construit `CombatSetup` (graine `ISeedProvider`, alliés = équipe de la session et leur fatigue, ennemis = rencontre au niveau moyen de l'équipe + écart du rang) et démarre le runner.
  - `ICombatCommandsUseCase` : `UseSkill(int actor, int slot, int target)`, `UseCombo(int a, int b, int target)`, `SetAuto(bool)`, `Retreat()`, `SetSpeed(int)` → `Submit` sur la simulation.
  - `IResolveCombatUseCase.Execute(CombatResult)` : **vérifie** le résultat par `CombatReplay` (préfiguration du serveur ; un écart = log d'erreur), applique la fatigue (+12 victoire, +27 défaite), crédite (Facile 200 / Normal 300 / Élite 500), appelle `ZoneRun.ResolveCombat` et publie les messages du plateau.

- [ ] **Étape 1 : Test en échec**
```csharp
using System.Linq;
using N29.HeroesDawn.Application.Features.Board;
using N29.HeroesDawn.Application.Features.Combat;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using NUnit.Framework;

public class CombatRunnerTests
{
    [Test]
    public void AutoCombat_Finishes_AppliesFatigue_AndCredits()
    {
        var session = new GameSession(PrototypeContent.Build());
        var h = CombatTestHarness.Create(session, seed: 21);
        h.OpenZoneAndForceCombat("n03");
        h.Commands.SetAuto(true);
        for (int i = 0; i < 400 && h.Runner.IsRunning; i++) h.Runner.Advance(500);
        Assert.That(h.Runner.IsRunning, Is.False);
        Assert.That(session.Team.All(m => m.Fatigue >= 12), Is.True);
        Assert.That(h.Finished.Count, Is.EqualTo(1));
    }
}
```
```csharp
// Assets/Tests/Domain/Application/CombatTestHarness.cs
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Application.Features.Board;
using N29.HeroesDawn.Application.Features.Combat;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Domain.Board;

public sealed class CombatTestHarness
{
    public GameSession Session { get; private set; } = null!;
    public BoardPublishers BoardPubs { get; private set; } = null!;
    public CombatRunner Runner { get; private set; } = null!;
    public ICombatCommandsUseCase Commands { get; private set; } = null!;
    public List<CombatFinished> Finished => _finished.Sent;
    readonly RecordingPublisher<CombatFinished> _finished = new RecordingPublisher<CombatFinished>();
    StartCombatUseCase _start = null!;
    ulong _seed;

    sealed class FixedSeed : ISeedProvider { readonly ulong _s; public FixedSeed(ulong s) { _s = s; } public ulong NextSeed(string p) => _s; }
    sealed class SilentLogger : ILogger { public void Info(string m) { } public void Error(string m) => NUnit.Framework.Assert.Fail(m); }

    public static CombatTestHarness Create(GameSession session, ulong seed)
    {
        var h = new CombatTestHarness { Session = session, BoardPubs = new BoardPublishers(), _seed = seed };
        var boardEvents = new BoardEventPublisher(session, h.BoardPubs);
        var resolve = new ResolveCombatUseCase(session, boardEvents, h._finished, new RecordingPublisher<WalletChanged>(), new SilentLogger());
        h.Runner = new CombatRunner(new RecordingPublisher<CombatEventBatch>(), new RecordingPublisher<CombatStarted>(), resolve);
        h.Commands = new CombatCommandsUseCase(h.Runner);
        h._start = new StartCombatUseCase(session, new FixedSeed(seed), h.Runner);
        new OpenZoneUseCase(session, new FixedSeed(seed), boardEvents).Execute(new ZoneId("zone_ny_1_1"));
        return h;
    }

    /// <summary>Place le pion sur un voisin de la case de combat, avec 1 PM, puis entre dans la case.</summary>
    public void OpenZoneAndForceCombat(string combatCase)
    {
        var zone = Session.Run!.Graph.Zone;
        var target = new CaseId(combatCase);
        var neighbor = Session.Run.Graph.Neighbors(target)[0];
        Session.Run = ZoneRun.ForTests(Session.Content, zone, _seed, pmInHand: 0, revealAll: true, position: neighbor.Value);
        Session.Run.SetPmForTests(1);
        var boardEvents = new BoardEventPublisher(Session, BoardPubs);
        new MoveToUseCase(Session, boardEvents).Execute(target);
        var request = ((RecordingPublisher<CombatRequested>)BoardPubs.CombatRequested).Sent.Last();
        _start.Execute(request);
    }
}
```

- [ ] **Étape 2 : Vérifier l'échec.**
- [ ] **Étape 3 : Implémentation**
```csharp
// Application/Features/Combat/CombatRunner.cs
#nullable enable
using System.Collections.Generic;
using MessagePipe;
using N29.HeroesDawn.Domain.Combat;
namespace N29.HeroesDawn.Application.Features.Combat
{
    public sealed class CombatRunner
    {
        readonly IPublisher<CombatEventBatch> _batch;
        readonly IPublisher<CombatStarted> _started;
        readonly IResolveCombatUseCase _resolve;
        int _accumulatedMs;
        public CombatSimulation? Sim { get; private set; }
        public bool IsRunning => Sim != null && !Sim.IsOver;
        public int SpeedMultiplier { get; set; } = 1;
        /// <summary>0-1 entre deux ticks, pour l'affichage uniquement (jamais relu par la simulation).</summary>
        public float GaugeInterpolation => Sim == null ? 0f : (float)_accumulatedMs / _tickMs;
        int _tickMs = 50;

        public CombatRunner(IPublisher<CombatEventBatch> batch, IPublisher<CombatStarted> started, IResolveCombatUseCase resolve)
        { _batch = batch; _started = started; _resolve = resolve; }

        public void Start(CombatSimulation sim, int tickMs)
        {
            Sim = sim; _tickMs = tickMs; _accumulatedMs = 0;
            _started.Publish(new CombatStarted(sim));
        }

        public void Advance(int deltaMs)
        {
            if (Sim == null || Sim.IsOver) return;
            _accumulatedMs += deltaMs * SpeedMultiplier;
            int ticks = _accumulatedMs / _tickMs;
            if (ticks == 0) return;
            _accumulatedMs -= ticks * _tickMs;
            var events = Sim.Step(ticks);
            if (events.Count > 0) _batch.Publish(new CombatEventBatch(events, Sim.Tick));
            if (Sim.IsOver) { var sim = Sim; _resolve.Execute(sim); Sim = null; }
        }
    }
}
```
```csharp
// Application/Features/Combat/Messages/CombatMessages.cs
using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Domain.Combat;
namespace N29.HeroesDawn.Application.Features.Combat
{
    public readonly struct CombatStarted { public readonly CombatSimulation Sim; public CombatStarted(CombatSimulation s) { Sim = s; } }
    public readonly struct CombatEventBatch { public readonly IReadOnlyList<CombatEvent> Events; public readonly int Tick; public CombatEventBatch(IReadOnlyList<CombatEvent> e, int t) { Events = e; Tick = t; } }
    public readonly struct MemberFatigue { public readonly MemberId Member; public readonly int Before; public readonly int After; public MemberFatigue(MemberId m, int b, int a) { Member = m; Before = b; After = a; } }
    public readonly struct CombatFinished
    {
        public readonly bool Victory; public readonly int Credits; public readonly IReadOnlyList<MemberFatigue> Fatigue;
        public CombatFinished(bool v, int c, IReadOnlyList<MemberFatigue> f) { Victory = v; Credits = c; Fatigue = f; }
    }
}
```
```csharp
// Application/Features/Combat/UseCases/StartCombatUseCase.cs
using System.Collections.Generic;
using N29.HeroesDawn.Application.Features.Board;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Combat;
namespace N29.HeroesDawn.Application.Features.Combat
{
    public interface IStartCombatUseCase { Result Execute(CombatRequested request); }
    public sealed class StartCombatUseCase : IStartCombatUseCase
    {
        readonly GameSession _s; readonly ISeedProvider _seeds; readonly CombatRunner _runner;
        public StartCombatUseCase(GameSession s, ISeedProvider seeds, CombatRunner runner) { _s = s; _seeds = seeds; _runner = runner; }
        public Result Execute(CombatRequested req)
        {
            var b = _s.Content.Balance;
            int avg = 0; foreach (var m in _s.Team) avg += m.Level; avg /= _s.Team.Count;
            int offset = req.Rank == CombatRank.Easy ? b.RankEasyLevelOffset : req.Rank == CombatRank.Normal ? 0 : b.RankEliteLevelOffset;
            int level = System.Math.Max(1, avg + offset);
            var allies = new List<AllyEntry>();
            foreach (var m in _s.Team) if (m.Fatigue < 100) allies.Add(new AllyEntry(m.Id, m.Level, m.Fatigue));
            var enemies = new List<EnemyEntry>();
            foreach (var e in req.Encounter.Enemies) enemies.Add(new EnemyEntry(e, level));
            var setup = new CombatSetup(_seeds.NextSeed("combat"), req.Rank, allies, enemies, auto: false);
            _runner.Start(new CombatSimulation(_s.Content, setup), b.TickMs);
            return Result.Ok();
        }
    }
}
```
```csharp
// Application/Features/Combat/UseCases/ResolveCombatUseCase.cs
using System.Collections.Generic;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Board;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Combat;
namespace N29.HeroesDawn.Application.Features.Combat
{
    public interface IResolveCombatUseCase { void Execute(CombatSimulation finished); }
    public sealed class ResolveCombatUseCase : IResolveCombatUseCase
    {
        readonly GameSession _s; readonly BoardEventPublisher _board; readonly IPublisher<CombatFinished> _finished;
        readonly IPublisher<WalletChanged> _wallet; readonly ILogger _log;
        public ResolveCombatUseCase(GameSession s, BoardEventPublisher board, IPublisher<CombatFinished> finished, IPublisher<WalletChanged> wallet, ILogger log)
        { _s = s; _board = board; _finished = finished; _wallet = wallet; _log = log; }

        public void Execute(CombatSimulation sim)
        {
            var result = sim.Result();
            // Préfiguration du serveur : rejeu et comparaison d'empreinte.
            var replay = CombatReplay.Run(_s.Content, sim.Setup, sim.InputLog);
            if (replay.StateHash != result.StateHash)
                _log.Error($"[Combat] Désynchronisation du rejeu : {replay.StateHash:X} ≠ {result.StateHash:X}");

            var b = _s.Content.Balance;
            int gain = result.Victory ? b.FatiguePerWin : b.FatiguePerWin + b.FatiguePerLossExtra;
            var fatigue = new List<MemberFatigue>();
            foreach (var m in _s.Team)
            {
                int before = m.Fatigue; m.Fatigue = System.Math.Min(100, m.Fatigue + gain);
                fatigue.Add(new MemberFatigue(m.Id, before, m.Fatigue));
            }
            int credits = !result.Victory ? 0 : sim.Setup.Rank switch { CombatRank.Easy => 200, CombatRank.Normal => 300, _ => 500 };
            if (credits > 0) { _s.Wallet.Add(credits, 0); _wallet.Publish(new WalletChanged(_s.Wallet.Credits, _s.Wallet.Pr)); }
            _finished.Publish(new CombatFinished(result.Victory, credits, fatigue));
            _board.Publish(_s.Run!.ResolveCombat(result.Victory));
        }
    }
}
```
```csharp
// Application/Ports/ILogger.cs
namespace N29.HeroesDawn.Application.Ports { public interface ILogger { void Info(string message); void Error(string message); } }
```
```csharp
// Infrastructure/Local/UnityLogger.cs
using N29.HeroesDawn.Application.Ports;
namespace N29.HeroesDawn.Infrastructure.Local
{
    public sealed class UnityLogger : ILogger
    {
        public void Info(string message) => UnityEngine.Debug.Log(message);
        public void Error(string message) => UnityEngine.Debug.LogError(message);
    }
}
```

```csharp
// Application/Features/Combat/UseCases/CombatCommandsUseCase.cs
using N29.HeroesDawn.Domain.Combat;
namespace N29.HeroesDawn.Application.Features.Combat
{
    public interface ICombatCommandsUseCase { void UseSkill(int actor, int slot, int target); void UseCombo(int a, int b, int target); void SetAuto(bool on); void Retreat(); void SetSpeed(int multiplier); }
    public sealed class CombatCommandsUseCase : ICombatCommandsUseCase
    {
        readonly CombatRunner _r;
        public CombatCommandsUseCase(CombatRunner r) { _r = r; }
        public void UseSkill(int actor, int slot, int target) => _r.Sim?.Submit(CombatCommand.UseSkill(actor, slot, target));
        public void UseCombo(int a, int b, int target) => _r.Sim?.Submit(CombatCommand.UseCombo(a, b, target));
        public void SetAuto(bool on) => _r.Sim?.Submit(CombatCommand.SetAuto(on));
        public void Retreat() => _r.Sim?.Submit(CombatCommand.Retreat());
        public void SetSpeed(int multiplier) => _r.SpeedMultiplier = multiplier == 2 ? 2 : 1;
    }
}
```
- [ ] **Étape 4 : Vérifier** — test vert ; toute la suite `dotnet test` + EditMode verte.
- [ ] **Étape 5 : Commit** — `git commit -am "feat(app): combat runner, commandes, résolution avec vérification par rejeu"`
