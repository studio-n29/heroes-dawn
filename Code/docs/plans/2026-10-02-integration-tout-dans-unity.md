# Phase d'intégration — Tout dans Unity · Plan d'implémentation

> **Pour les agents :** SOUS-COMPÉTENCE REQUISE : `superpowers:subagent-driven-development` (recommandé) ou `superpowers:executing-plans` pour exécuter ce plan tâche par tâche. Les étapes utilisent des cases à cocher (`- [ ]`).

**Objectif :** que tout le projet (jeu, tests, outils d'équilibrage, configuration backend) vive et s'exécute **dans Unity**, sans projet .NET ni script hors de `Heroes Dawn Unity/`.

**Architecture :** les deux outils console (`SharedTests`, `BalanceSimulator`) et l'outil prévu `BalanceImport` deviennent des tests du Test Runner et des fenêtres du menu **Heroes Dawn**. Le module Cloud Code C# (`Code/Server/`) est abandonné. À la place, UGS est piloté depuis l'éditeur avec la fenêtre *Deployment* : scripts Cloud Code **JS**, monnaies Economy et clés Remote Config sont des fichiers rangés dans `Assets/_Project/Backend/`. Le client calcule les règles (domaine C# partagé). Le serveur fait des **contrôles légers** (plafonds, cohérence, idempotence) et ne rejoue pas les combats.

**Stack :** Unity 6000.6.3f1, Unity Test Framework (NUnit), `System.IO.Compression` + `System.Xml` (lecture xlsx, sans dépendance), packages UGS (`com.unity.services.*`, `com.unity.remote-config`, `com.unity.services.deployment`).

**Specs :** décisions du 2026-10-02 (ci-dessous) ; `04_Tech_Unity/06_Backend_Donnees.md` et `09_Tests_Outils_CI.md` (à amender, tâche 5) ; `01_Game_Design/05_Equilibrage.xlsx`, onglet « Paramètres ».

## Décisions prises (2026-10-02)
| Décision | Remplace |
|---|---|
| Tout se passe dans Unity : aucun script ni projet .NET hors du projet Unity | `Tools/`, `Server/`, `dotnet test` |
| Backend UGS piloté depuis l'éditeur (Deployment) ; logique serveur en scripts Cloud Code JS ; contrôles légers, pas de rejeu de combat côté serveur | Module Cloud Code C# partageant le domaine, checkpoints rejoués |
| Simulateur et import du tableur = outils éditeur (menu Heroes Dawn) | Consoles .NET `BalanceSimulator` / `BalanceImport` |

## Contraintes globales
- Code partagé (`Packages/com.n29.heroesdawn.domain`) et `Content` : **aucun** `using UnityEngine`, C# 9, `#nullable enable`, entiers uniquement (pas de `float`/`double`).
- Outils éditeur : assembly `N29.HeroesDawn.Editor` (namespace `N29.HeroesDawn.EditorTools`), menus sous `Heroes Dawn/…`.
- Lecture du tableur : culture **invariante** ; pourcentages arrondis avec `Math.Round(v * 100, MidpointRounding.AwayFromZero)`.
- Fichiers générés dans `Assets/` : `Assets/_Project/Resources/Balance/balance.json` (client) et `Assets/_Project/Backend/RemoteConfig/balance.rc` (serveur). Rapports du simulateur : `Heroes Dawn Unity/BalanceReports/` (hors `Assets/`, ignoré par Git).
- Textes de l'UI en jeu : clés de localisation. Les outils éditeur peuvent afficher du français en dur.
- Ne pas modifier les règles du domaine : `RulesVersion` inchangée.

## Commandes (bash, Unity **fermé** sur ce projet)
```bash
UNITY="/c/Program Files/Unity/Hub/Editor/6000.6.3f1/Editor/Unity.exe"
PROJ="C:/Users/dnath/Documents/Heroes Dawn/Code/Heroes Dawn Unity"
"$UNITY" -batchmode -nographics -projectPath "$PROJ" -runTests -testPlatform EditMode -testResults "$TMP/editmode.xml" -logFile "$TMP/unity.log"
grep -o '<test-run [^>]*' "$TMP/editmode.xml" | grep -oE '(total|passed|failed)="[0-9]+"'
```
- Un seul test : ajouter `-testFilter "<Namespace>.<Classe>.<Test>"`.
- Exécuter un menu : `-executeMethod N29.HeroesDawn.EditorTools.<Classe>.<Méthode> -quit` (méthode `public static`).
- Erreurs de compilation : `grep -E "error CS" "$TMP/unity.log"`.
- Point de départ mesuré le 2026-10-02 : **15 tests EditMode, 15 réussis**, compilation sans erreur.

## Points d'attention (Review Focus)
1. **Tableur ouvert dans Excel** pendant l'import → l'import réussit (lecture avec `FileShare.ReadWrite`) ; sinon message clair, pas d'exception brute → test tâche 2.
2. **Libellé renommé ou supprimé dans « Paramètres »** → la valeur par défaut est gardée et un avertissement nomme le paramètre manquant ; l'import ne plante pas → test tâche 2.
3. **Arrondi des pourcentages** (`0.29 × 100 = 28.999…`) et **culture française** de l'éditeur (virgule décimale) → 29, et `0.15` lu 15 sous `fr-FR` → tests tâche 2.
4. **`balance.json` absent ou corrompu au lancement** → le jeu démarre avec les valeurs par défaut et journalise un avertissement → test tâche 1.
5. **Combat qui ne finit jamais** (deux tanks, plafond de ticks atteint) → compté à part en « temps écoulé », pas comme une défaite silencieuse → test tâche 3.

---

## Carte des fichiers
```
Heroes Dawn/                                 ← racine du nouveau dépôt Git (tâche 0)
├─ .gitignore  .gitattributes                (créés)
└─ Code/
   ├─ Tools/  Server/  _deploy/              (supprimés, tâche 5)
   └─ Heroes Dawn Unity/
      ├─ Assets/_Project/
      │  ├─ Content/Prototype/BalanceJson.cs              (tâche 1)
      │  ├─ Content/Prototype/PrototypeContent.cs         (modifié, tâche 1)
      │  ├─ Composition/GameBootstrap.cs                  (modifié, tâche 1)
      │  ├─ Resources/Balance/balance.json                (généré, tâche 2)
      │  ├─ Editor/Balance/XlsxReader.cs                  (tâche 2)
      │  ├─ Editor/Balance/BalanceSheetMapper.cs          (tâche 2)
      │  ├─ Editor/Balance/BalanceImportMenu.cs           (tâche 2)
      │  ├─ Editor/Balance/RemoteConfigFile.cs            (tâche 2)
      │  ├─ Editor/Balance/BalanceSimulation.cs           (tâche 3)
      │  ├─ Editor/Balance/BalanceSimulatorWindow.cs      (tâche 3)
      │  ├─ Editor/PackageInstaller.cs                    (modifié, tâche 4)
      │  └─ Backend/                                      (tâche 4)
      │     ├─ README.md
      │     ├─ CloudCode/GetServerTime.js
      │     ├─ Economy/  (CREDITS, PR, GEMS)
      │     └─ RemoteConfig/balance.rc                    (généré, tâche 2)
      └─ Assets/Tests/
         ├─ Domain/BalanceJsonTests.cs                    (tâche 1)
         └─ Editor/N29.HeroesDawn.Tests.Editor.asmdef     (tâche 2)
            ├─ BalanceImportTests.cs                      (tâche 2)
            ├─ BalanceSimulationTests.cs                  (tâche 3)
            └─ BackendLayoutTests.cs                      (tâche 4)
```

---

### Tâche 0 : Dépôt Git propre

Le dépôt Git actif est aujourd'hui `C:/Users/dnath` (tout le dossier utilisateur, aucun commit). Un commit depuis `Heroes Dawn/` toucherait ce dépôt-là. On crée un dépôt dédié.

**Fichiers :**
- Créer : `Heroes Dawn/.gitignore`, `Heroes Dawn/.gitattributes`

- [ ] **Étape 1 : Créer `.gitignore` à la racine**
Contenu : `.idea/`, `**/.idea/`, `Code/Heroes Dawn Unity/BalanceReports/`, `~$*` (fichiers verrous Office). Les règles Unity restent dans `Code/.gitignore`. Il est relatif à `Code/` et reste valable.

- [ ] **Étape 2 : Créer `.gitattributes` à la racine**
Mettre en LFS : `*.png *.jpg *.pdf *.docx *.xlsx *.ttf *.bytes filter=lfs diff=lfs merge=lfs -text`. `Code/.gitattributes` reste en place.

- [ ] **Étape 3 : Initialiser et vérifier**
```bash
cd "/c/Users/dnath/Documents/Heroes Dawn" && git init -b main && git lfs install --local
git rev-parse --show-toplevel   # attendu : C:/Users/dnath/Documents/Heroes Dawn
git status --short | grep -E "Library/|Temp/|Logs/" | wc -l   # attendu : 0
```

- [ ] **Étape 4 : Commit de référence**
```bash
git add . && git commit -m "chore: état de référence avant l'intégration tout-Unity"
```

---

### Tâche 1 : Équilibrage chargé depuis un JSON (client)

**Fichiers :**
- Créer : `Assets/_Project/Content/Prototype/BalanceJson.cs`
- Modifier : `Assets/_Project/Content/Prototype/PrototypeContent.cs:49,118` (surcharge `Build(BalanceConfig)`)
- Modifier : `Assets/_Project/Composition/GameBootstrap.cs` (chargement `Resources/Balance/balance`)
- Test : `Assets/Tests/Domain/BalanceJsonTests.cs`

**Interfaces :**
- Produit : `public static class BalanceJson` (namespace `N29.HeroesDawn.Content.Prototype`)
  - `string Serialize(BalanceConfig config)`. Objet JSON plat `{ "NomPropriété": entier, … }`, une clé par propriété `int` publique, **ordre alphabétique**, indentation 2 espaces.
  - `Result<BalanceConfig> Deserialize(string json)`. Part de `new BalanceConfig()` et écrase les clés présentes. Clé inconnue : ignorée. JSON illisible : `Result.Fail(new DomainError("balance.invalid_json", "errors.balance.invalid_json"))`.
- Produit : `PrototypeContent.Build(BalanceConfig balance)`. `Build()` appelle `Build(new BalanceConfig())`.

Le parseur est écrit à la main (objet plat d'entiers, réflexion sur les propriétés `int`). Pas de Newtonsoft : il n'est pas installé et `Content` est `noEngineReferences`.

- [ ] **Étape 1 : Écrire les tests en échec**
```csharp
[Test] public void RoundTrip_PreservesEveryIntProperty()
{   var c = new BalanceConfig { DefK = 321, FatiguePerWin = 9, RankEasyLevelOffset = -4 };
    var back = BalanceJson.Deserialize(BalanceJson.Serialize(c)).Value;
    foreach (var p in typeof(BalanceConfig).GetProperties())
        Assert.That(p.GetValue(back), Is.EqualTo(p.GetValue(c)), p.Name); }
[Test] public void Deserialize_PartialJson_KeepsDefaultsForMissingKeys()
{   var b = BalanceJson.Deserialize("{ \"DefK\": 500 }").Value;
    Assert.That(b.DefK, Is.EqualTo(500)); Assert.That(b.CritBasePercent, Is.EqualTo(15)); }
[Test] public void Deserialize_Garbage_Fails()  // Review Focus 4
{   var r = BalanceJson.Deserialize("pas du json");
    Assert.That(r.IsFailure); Assert.That(r.Error!.Code, Is.EqualTo("balance.invalid_json")); }
[Test] public void Build_UsesGivenBalance()
{   Assert.That(PrototypeContent.Build(new BalanceConfig { DefK = 999 }).Balance.DefK, Is.EqualTo(999)); }
```

- [ ] **Étape 2 : Lancer, vérifier l'échec**
Commande de la section *Commandes* avec `-testFilter "N29.HeroesDawn.Tests.Domain.BalanceJsonTests"`. Attendu : `error CS0103` / `BalanceJson` introuvable dans `unity.log`.

- [ ] **Étape 3 : Implémenter `BalanceJson` et `PrototypeContent.Build(BalanceConfig)`**

- [ ] **Étape 4 : Brancher `GameBootstrap`**
Dans `Awake`, remplacer `PrototypeContent.Build()` par : `Resources.Load<TextAsset>("Balance/balance")`. Si l'asset est absent : `new BalanceConfig()` sans avertissement. Si `Deserialize` échoue : `new BalanceConfig()` + `log.Warn("Équilibrage illisible (balance.json) : valeurs par défaut")`. Créer le logger avant le contenu.

- [ ] **Étape 5 : Lancer toute la suite EditMode**
Attendu : `total="19" passed="19" failed="0"`.

- [ ] **Étape 6 : Commit**
```bash
git add -A "Code/Heroes Dawn Unity/Assets" && git commit -m "feat: équilibrage du client chargé depuis balance.json"
```

---

### Tâche 2 : Import du tableur dans l'éditeur (remplace `BalanceImport`)

**Fichiers :**
- Créer : `Assets/_Project/Editor/Balance/XlsxReader.cs`, `BalanceSheetMapper.cs`, `RemoteConfigFile.cs`, `BalanceImportMenu.cs`
- Créer : `Assets/Tests/Editor/N29.HeroesDawn.Tests.Editor.asmdef`, `Assets/Tests/Editor/BalanceImportTests.cs`
- Générés : `Assets/_Project/Resources/Balance/balance.json`, `Assets/_Project/Backend/RemoteConfig/balance.rc`

**Interfaces :**
- Consomme : `BalanceJson.Serialize` (tâche 1).
- Produit (namespace `N29.HeroesDawn.EditorTools.Balance`) :
  - `static class XlsxReader` · `IReadOnlyList<string[]> ReadSheet(Stream xlsx, string sheetName)`. Une ligne par `<row>`, colonnes A…Z indexées 0…25, cellules vides = `""`. Les chaînes partagées (`t="s"`) et inline (`t="inlineStr"`) sont résolues. Les nombres sont rendus tels qu'écrits dans `<v>` (point décimal). Feuille inconnue : `ArgumentException("Onglet introuvable : " + nom)`.
  - `static class BalanceSheetMapper` · `BalanceConfig Map(IReadOnlyList<string[]> rows, List<string> warnings)`. Colonne B = libellé, C = valeur. Lecture avec `CultureInfo.InvariantCulture`.
  - `static class RemoteConfigFile` · `string Build(string balanceJson)`. Produit `{ "$schema": "https://ugs-config-schemas.unity3d.com/v1/remote-config.schema.json", "entries": { "balance": <objet> }, "types": { "balance": "JSON" } }`.
  - `static class BalanceImportMenu` · `public static void ImportDefault()`, menu `Heroes Dawn/Équilibrage/Importer 05_Equilibrage.xlsx`. Source : `Path.GetFullPath(Path.Combine(Application.dataPath, "../../../01_Game_Design/05_Equilibrage.xlsx"))`. Ouverture : `new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)`. Écrit les deux fichiers, `AssetDatabase.Refresh()`, journalise chaque avertissement puis `"[Heroes Dawn] Équilibrage importé : N paramètres, M avertissements"`. Fichier introuvable : `Debug.LogError` avec le chemin, sans exception.
- Asmdef `N29.HeroesDawn.Tests.Editor` : Editor seulement, `UNITY_INCLUDE_TESTS`, `nunit.framework.dll`. Références : TestRunner (Engine + Editor), Core, Definitions, Domain.Board, Domain.Combat, Content, `N29.HeroesDawn.Editor`.

**Table de correspondance** (libellé exact de l'onglet « Paramètres » → propriété ; conversion) :
| Libellé | Propriété | Conversion |
|---|---|---|
| Taille de la jauge ATB | `AtbGaugePoints` | entier |
| Remplissage par point de Vitesse | `AtbFactorPerSecond` | entier |
| Écart de niveau — rang Facile | `RankEasyLevelOffset` | entier |
| Écart de niveau — rang Élite | `RankEliteLevelOffset` | entier |
| Constante de défense (K) | `DefK` | entier |
| Chance de critique de base | `CritBasePercent` | ×100 |
| Multiplicateur de critique | `CritMultiplierPercent` | ×100 |
| Stats ennemis — rang Facile | `RankEasyStatPercent` | ×100 |
| Stats ennemis — rang Élite | `RankEliteStatPercent` | ×100 |
| Fatigue par combat (victoire) | `FatiguePerWin` | ×100 |
| Fatigue supplémentaire en défaite | `FatiguePerLossExtra` | ×100 |
| Seuil d'apparition du malus | `FatigueMalusThreshold` | ×100 |
| Malus de stats à 100 % de fatigue | `FatigueMalusMaxPercent` | ×100 |
| Coût du 1er palier, Coût Agent… | — | non mappés au M1 (pas dans `BalanceConfig`), ignorés sans avertissement |

Un libellé de la table absent du tableur donne l'avertissement `"Paramètre absent du tableur : <libellé> (défaut <valeur> conservé)"`. Une valeur non numérique donne `"Valeur illisible pour <libellé> : '<texte>'"`.

- [ ] **Étape 1 : Créer l'asmdef de tests éditeur et écrire les tests en échec**
```csharp
static string Xlsx => Path.GetFullPath(Path.Combine(Application.dataPath, "../../../01_Game_Design/05_Equilibrage.xlsx"));

[Test] public void Import_RealSheet_MatchesGddValues()
{   using var s = new FileStream(Xlsx, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
    var w = new List<string>(); var b = BalanceSheetMapper.Map(XlsxReader.ReadSheet(s, "Paramètres"), w);
    Assert.That(w, Is.Empty);
    Assert.That(b.AtbGaugePoints, Is.EqualTo(1000)); Assert.That(b.AtbFactorPerSecond, Is.EqualTo(4));
    Assert.That(b.DefK, Is.EqualTo(300)); Assert.That(b.CritBasePercent, Is.EqualTo(15));
    Assert.That(b.CritMultiplierPercent, Is.EqualTo(150)); Assert.That(b.FatiguePerWin, Is.EqualTo(12));
    Assert.That(b.FatiguePerLossExtra, Is.EqualTo(15)); Assert.That(b.FatigueMalusThreshold, Is.EqualTo(60));
    Assert.That(b.RankEasyStatPercent, Is.EqualTo(80)); Assert.That(b.RankEliteLevelOffset, Is.EqualTo(3)); }
[Test] public void Import_WhileFileLockedForWriting_Succeeds()   // Review Focus 1
{   using var excel = new FileStream(Xlsx, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
    using var s = new FileStream(Xlsx, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
    Assert.That(XlsxReader.ReadSheet(s, "Paramètres").Count, Is.GreaterThan(50)); }
[Test] public void Map_MissingLabel_KeepsDefaultAndWarns()       // Review Focus 2
{   var w = new List<string>();
    var b = BalanceSheetMapper.Map(new[] { new[] { "", "Constante de défense (K)", "450" } }, w);
    Assert.That(b.DefK, Is.EqualTo(450)); Assert.That(b.CritBasePercent, Is.EqualTo(15));
    Assert.That(w, Has.Some.Contains("Chance de critique de base")); }
[Test] public void Map_PercentRounding_And_FrenchCulture()       // Review Focus 3
{   var prev = CultureInfo.CurrentCulture; CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
    try { var b = BalanceSheetMapper.Map(new[] { new[] { "", "Fatigue par combat (victoire)", "0.29" },
                                                 new[] { "", "Chance de critique de base", "0.15" } }, new List<string>());
          Assert.That(b.FatiguePerWin, Is.EqualTo(29)); Assert.That(b.CritBasePercent, Is.EqualTo(15)); }
    finally { CultureInfo.CurrentCulture = prev; } }
[Test] public void Map_NonNumericValue_WarnsAndKeepsDefault()
{   var w = new List<string>();
    var b = BalanceSheetMapper.Map(new[] { new[] { "", "Constante de défense (K)", "#REF!" } }, w);
    Assert.That(b.DefK, Is.EqualTo(300)); Assert.That(w, Has.Some.Contains("Valeur illisible")); }
[Test] public void ReadSheet_UnknownSheet_Throws()
{   using var s = File.OpenRead(Xlsx);
    Assert.Throws<ArgumentException>(() => XlsxReader.ReadSheet(s, "Inexistant")); }
[Test] public void RemoteConfigFile_WrapsBalanceAsJsonEntry()
{   var rc = RemoteConfigFile.Build(BalanceJson.Serialize(new BalanceConfig()));
    StringAssert.Contains("\"types\"", rc); StringAssert.Contains("\"balance\": \"JSON\"", rc);
    StringAssert.Contains("\"DefK\": 300", rc); }
```

- [ ] **Étape 2 : Lancer, vérifier l'échec** (`-testFilter "N29.HeroesDawn.Tests.Editor.BalanceImportTests"`). Attendu : types introuvables (`error CS0246`).

- [ ] **Étape 3 : Implémenter `XlsxReader`**
`ZipArchive` sur le flux. `xl/workbook.xml` donne `name` → `r:id`. `xl/_rels/workbook.xml.rels` donne `r:id` → `worksheets/sheetN.xml`. Charger `xl/sharedStrings.xml` s'il existe (concaténer les `<t>` des `<r>` pour le texte riche). Colonne = lettres de l'attribut `r` de la cellule.

- [ ] **Étape 4 : Implémenter `BalanceSheetMapper`, `RemoteConfigFile`, `BalanceImportMenu`**

- [ ] **Étape 5 : Lancer toute la suite** : attendu `total="26" passed="26"`.

- [ ] **Étape 6 : Générer les fichiers par le menu, en batchmode**
```bash
"$UNITY" -batchmode -nographics -projectPath "$PROJ" -executeMethod N29.HeroesDawn.EditorTools.Balance.BalanceImportMenu.ImportDefault -quit -logFile "$TMP/import.log"
grep "Équilibrage importé" "$TMP/import.log"   # attendu : « 13 paramètres, 0 avertissements »
```
Vérifier que `Resources/Balance/balance.json` contient `"FatiguePerWin": 12`.

- [ ] **Étape 7 : Commit**
```bash
git add -A "Code/Heroes Dawn Unity/Assets" && git commit -m "feat(outils): import du tableur d'équilibrage dans l'éditeur"
```

---

### Tâche 3 : Simulateur d'équilibrage dans l'éditeur (remplace `BalanceSimulator`)

**Fichiers :**
- Créer : `Assets/_Project/Editor/Balance/BalanceSimulation.cs`, `BalanceSimulatorWindow.cs`
- Test : `Assets/Tests/Editor/BalanceSimulationTests.cs`

**Interfaces :**
- Consomme : `PrototypeContent.Build(BalanceConfig)` et `BalanceJson.Deserialize` (tâche 1) ; `CombatSimulation`, `CombatSetup`, `AllyEntry`, `EnemyEntry` (domaine).
- Produit (namespace `N29.HeroesDawn.EditorTools.Balance`) :
  - `sealed class SimulationRequest` { `IReadOnlyList<MemberId> Team` (4) ; `int TeamLevel` ; `int FatiguePercent` ; `int RunsPerEncounter` ; `ulong BaseSeed` ; `int MaxTicks = 20000` }
  - `sealed class EncounterStats` { `int EncounterIndex` ; `CombatRank Rank` ; `int Runs` ; `int Wins` ; `int Losses` ; `int Timeouts` ; `int AvgTicksOnWin` }
  - `static class BalanceSimulation`
    - `IReadOnlyList<EncounterStats> Run(ContentCatalog content, ZoneId zone, SimulationRequest req)` : pour chaque `EncounterDef` de la zone, dans l'ordre, `RunsPerEncounter` combats. Graine du combat i de la rencontre e : `req.BaseSeed + (ulong)(e * 100000 + i)`. `CombatSetup(..., auto: true)`, ennemis au niveau `TeamLevel`. Boucle `while (!sim.IsOver && sim.Tick < MaxTicks) sim.Step(1)`. Plafond atteint sans fin : `Timeouts++`.
    - `string ToCsv(IReadOnlyList<EncounterStats> stats)` : en-tête `encounter;rank;runs;wins;losses;timeouts;win_rate_pct;avg_seconds_on_win`. Séparateur `;`, culture invariante. Secondes = `AvgTicksOnWin * TickMs / 1000` avec une décimale.
- Fenêtre `Heroes Dawn/Équilibrage/Simulateur` : 4 listes déroulantes de recrues (`content.Members`), niveau, fatigue, nombre de combats (défaut 1 000), graine, bouton **Lancer** (`EditorUtility.DisplayProgressBar` par rencontre), tableau des résultats, bouton **Exporter CSV** vers `BalanceReports/simulation_<aaaaMMjj_HHmmss>.csv`. L'équilibrage vient de `Resources/Balance/balance.json` s'il existe, sinon des valeurs par défaut.

- [ ] **Étape 1 : Écrire les tests en échec**
```csharp
static readonly MemberId[] Team = { new("ancre"), new("verre"), new("givre"), new("echo") };
static SimulationRequest Req(int runs, int maxTicks = 20000) => new SimulationRequest
    { Team = Team, TeamLevel = 5, FatiguePercent = 0, RunsPerEncounter = runs, BaseSeed = 42, MaxTicks = maxTicks };

[Test] public void Run_SameSeed_SameResults()
{   var c = PrototypeContent.Build();
    var a = BalanceSimulation.Run(c, PrototypeContent.Zone11, Req(20));
    var b = BalanceSimulation.Run(c, PrototypeContent.Zone11, Req(20));
    Assert.That(BalanceSimulation.ToCsv(a), Is.EqualTo(BalanceSimulation.ToCsv(b))); }
[Test] public void Run_OneRowPerEncounter_AndCountsAddUp()
{   var c = PrototypeContent.Build();
    var s = BalanceSimulation.Run(c, PrototypeContent.Zone11, Req(10));
    Assert.That(s.Count, Is.EqualTo(c.Zone(PrototypeContent.Zone11).Encounters.Count));
    foreach (var e in s) Assert.That(e.Wins + e.Losses + e.Timeouts, Is.EqualTo(10)); }
[Test] public void Run_TickCapReached_CountsTimeouts()            // Review Focus 5
{   var s = BalanceSimulation.Run(PrototypeContent.Build(), PrototypeContent.Zone11, Req(3, maxTicks: 1));
    Assert.That(s.Sum(e => e.Timeouts), Is.EqualTo(s.Count * 3)); }
[Test] public void ToCsv_UsesSemicolonsAndInvariantCulture()
{   var csv = BalanceSimulation.ToCsv(new[] { new EncounterStats { EncounterIndex = 0, Rank = CombatRank.Normal,
              Runs = 4, Wins = 3, Losses = 1, AvgTicksOnWin = 250 } });
    StringAssert.StartsWith("encounter;rank;runs;wins;losses;timeouts;win_rate_pct;avg_seconds_on_win", csv);
    StringAssert.Contains("0;Normal;4;3;1;0;75;12.5", csv); }
```
(`EncounterStats` : propriétés `{ get; set; }`. Pas d'`init` : le `IsExternalInit` de `Core` n'est pas visible depuis l'assembly éditeur.)

- [ ] **Étape 2 : Lancer, vérifier l'échec** (`-testFilter "N29.HeroesDawn.Tests.Editor.BalanceSimulationTests"`).

- [ ] **Étape 3 : Implémenter `BalanceSimulation`**

- [ ] **Étape 4 : Implémenter `BalanceSimulatorWindow`**

- [ ] **Étape 5 : Lancer toute la suite** : attendu `total="30" passed="30"`.

- [ ] **Étape 6 : Vérification manuelle** (Unity ouvert) : menu Simulateur, équipe Ancre/Verre/Givre/Écho niv. 5, 1 000 combats → tableau rempli en moins de 30 s, CSV créé dans `BalanceReports/`.

- [ ] **Étape 7 : Commit**
```bash
git add -A "Code/Heroes Dawn Unity/Assets" && git commit -m "feat(outils): simulateur d'équilibrage dans l'éditeur"
```

---

### Tâche 4 : Backend UGS piloté depuis Unity

**Fichiers :**
- Modifier : `Assets/_Project/Editor/PackageInstaller.cs`. Ajouter `"com.unity.services.deployment"` à `BackendPackages` et renommer le menu 3 `"Heroes Dawn/Setup/3. Installer les packages backend (UGS)"`.
- Créer : `Assets/_Project/Backend/README.md`, `Backend/CloudCode/GetServerTime.js`, `Backend/Economy/` (3 monnaies)
- Test : `Assets/Tests/Editor/BackendLayoutTests.cs`

**Interfaces :**
- Consomme : `Backend/RemoteConfig/balance.rc` (tâche 2), `BalanceJson.Deserialize` (tâche 1).
- Produit : la convention du dossier `Backend/`. Toute logique serveur = un script `Backend/CloudCode/<PointDEntrée>.js`, déployé par *Window ▸ Deployment*. Les adaptateurs `CloudCode…Gateway` du client (M2) appelleront ces noms.

- [ ] **Étape 1 : Écrire les tests en échec**
```csharp
const string Root = "Assets/_Project/Backend";
[Test] public void RemoteConfig_BalanceEntry_IsValidBalanceJson()
{   var rc = File.ReadAllText(Root + "/RemoteConfig/balance.rc");
    var start = rc.IndexOf("\"balance\": {", StringComparison.Ordinal) + "\"balance\": ".Length;
    var obj = rc.Substring(start, rc.IndexOf('}', start) - start + 1);
    Assert.That(BalanceJson.Deserialize(obj).IsSuccess); }
[Test] public void CloudCode_EveryScript_ExportsParamsAndHandler()
{   var files = Directory.GetFiles(Root + "/CloudCode", "*.js");
    Assert.That(files, Is.Not.Empty);
    foreach (var f in files) { var js = File.ReadAllText(f);
        StringAssert.Contains("module.exports", js, f); StringAssert.Contains("module.exports.params", js, f); } }
[Test] public void NoDotnetProjectOutsideUnity()   // garde-fou « tout dans Unity »
{   var code = Path.GetFullPath(Path.Combine(Application.dataPath, "../.."));
    Assert.That(Directory.GetFiles(code, "*.csproj", SearchOption.AllDirectories)
        .Where(p => !p.Contains("Heroes Dawn Unity")), Is.Empty); }
```

- [ ] **Étape 2 : Lancer, vérifier l'échec**. Attendu : `CloudCode_…` et `NoDotnet…` échouent (`Tools/*.csproj` existe encore). `NoDotnet…` passera à la tâche 5.

- [ ] **Étape 3 : Installer les packages backend**
Unity ouvert : *Heroes Dawn ▸ Setup ▸ 3*. Attendre « Packages installés ». Contrôler dans `Packages/manifest.json` : `com.unity.services.authentication`, `cloudcode`, `cloudsave`, `economy`, `deployment`, `com.unity.remote-config`.

- [ ] **Étape 4 : Écrire `Backend/CloudCode/GetServerTime.js`**
Sert à valider la chaîne de déploiement. Signature Cloud Code : `module.exports = async ({ params, context, logger }) => ({ utcMs: Date.now() });` et `module.exports.params = {};`.

- [ ] **Étape 5 : Créer les monnaies Economy** par *Assets ▸ Create ▸ Services ▸ Economy Currency* dans `Backend/Economy/` : `CREDITS` (initial 0), `PR` (initial 0), `GEMS` (initial 0). Garder les extensions générées par Unity.

- [ ] **Étape 6 : Écrire `Backend/README.md`**
Contenu : rôle de chaque sous-dossier ; règle « le serveur contrôle, le client calcule » ; liste des contrôles légers prévus au M2 :
- plafond de gain par zone, tiré de `balance` ;
- `requestId` idempotent ;
- horodatage serveur pour les timers ;
- coût de recrutement débité par Economy.

Procédure de déploiement : *Project Settings ▸ Services*, lier le projet UGS et choisir l'environnement `dev`, puis *Window ▸ Deployment ▸ Deploy All*.

- [ ] **Étape 7 : Lancer `BackendLayoutTests`**. Attendu : 2 réussis, `NoDotnetProjectOutsideUnity` encore en échec (normal avant la tâche 5).

- [ ] **Étape 8 : Déploiement réel (action de l'utilisateur)**
Lier le projet UGS (compte du studio), puis *Deploy All* sur `dev`. Attendu : 3 monnaies, 1 script et 1 clé Remote Config affichés « Deployed ». Si aucun projet UGS n'existe encore, l'étape est reportée au M2 sans bloquer la suite.

- [ ] **Étape 9 : Commit**
```bash
git add -A "Code/Heroes Dawn Unity" && git commit -m "feat(backend): configuration UGS déployable depuis l'éditeur"
```

---

### Tâche 5 : Suppression du hors-Unity et mise à jour de la documentation

**Fichiers :**
- Supprimer : `Code/Tools/` (SharedTests, BalanceSimulator), `Code/Server/`, `Code/_deploy/` (ancienne archive du code, déjà présente dans le projet Unity), `Code/.gitignore` section « .NET (outils, serveur) »
- Modifier : `Code/README.md`, `Code/SETUP.md`, `04_Tech_Unity/00_Vue_Ensemble.md`, `01_Architecture.md`, `02_Couches_Assemblies_Dossiers.md`, `05_Simulation_Deterministe.md`, `06_Backend_Donnees.md`, `08_Regles_Conventions.md`, `09_Tests_Outils_CI.md`, `05_Production/Decisions.md`, `Code/docs/plans/2026-09-29-prototype-M1-partie-2-interface.md` (tâche 19)

- [ ] **Étape 1 : Supprimer les dossiers** (après avoir vérifié que `git status` est propre)
```bash
git rm -r Code/Tools Code/Server Code/_deploy
```

- [ ] **Étape 2 : Lancer toute la suite** : attendu `total="33" passed="33" failed="0"` (`NoDotnetProjectOutsideUnity` passe).

- [ ] **Étape 3 : Mettre à jour la documentation technique**, phrase par phrase :
  - `00`, `01`, `08` : « Cloud Code C# / rejoue / hors de Unity » → « scripts Cloud Code JS déployés depuis Unity ; contrôles légers ; le client calcule ». Règle 4 de `08` : « toute dépense/récompense passe par un script Cloud Code ».
  - `02` : retirer `Server/`, `Tools/` et la phrase sur le `.csproj` partagé. Ajouter `Assets/_Project/Backend/` et `Assets/Tests/Editor/`.
  - `05` : le rejeu reste un outil client (tests, lecteur de journaux). La vérification serveur des checkpoints est abandonnée.
  - `06` : tableau des points d'entrée → scripts JS. Pipeline : `05_Equilibrage.xlsx → menu Importer → balance.json + balance.rc`. Supprimer `content_v{n}.json` côté serveur.
  - `09` : supprimer « Outils hors Unity » et `dotnet test`. Les tests passent par le Test Runner ou le batchmode (commande de ce plan). CI : GameCI seul. Déploiement par la fenêtre Deployment (ou GameCI + UGS CLI plus tard).
- [ ] **Étape 4 : `Code/README.md` et `SETUP.md`** : retirer `Server/` et `Tools/`, ainsi que le prérequis .NET SDK et `dotnet test`. Ajouter la section *Commandes* de ce plan et les menus *Équilibrage*. Remplacer la section Git par « dépôt à la racine `Heroes Dawn/` ».
- [ ] **Étape 5 : Plan M1 partie 2, tâche 19** : ajouter en tête « Remplacée par `2026-10-02-integration-tout-dans-unity.md`, tâche 3 ».
- [ ] **Étape 6 : `Decisions.md`** : ajouter les 3 lignes du tableau *Décisions prises (2026-10-02)*.
- [ ] **Étape 7 : Vérifier qu'aucune référence ne subsiste**
```bash
grep -rnE "dotnet test|Tools/SharedTests|BalanceSimulator \(console|HeroesDawn\.CloudCode" --include=*.md . | grep -v "2026-09-29-prototype"   # attendu : vide
```
- [ ] **Étape 8 : Commit**
```bash
git add -A && git commit -m "chore: tout dans Unity — suppression des outils .NET et du module serveur, docs à jour"
```

---

## Définition de « terminé »
- Aucun `.csproj` hors de `Heroes Dawn Unity/` (test `NoDotnetProjectOutsideUnity`).
- Suite EditMode verte en batchmode : 33 tests.
- Menus **Heroes Dawn ▸ Équilibrage ▸ Importer** et **Simulateur** fonctionnels ; `balance.json` chargé au lancement du jeu.
- Dossier `Backend/` déployable depuis *Window ▸ Deployment* (déploiement effectif dès qu'un projet UGS est lié).
- Documentation et journal des décisions alignés.
