# Catalogue de 36 nouvelles cartes — plan d'implémentation (lot C2 0.14.0)

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Porter le catalogue à 48 cartes (12 par famille, 4 par nature, raretés 5/4/2/1) avec leurs paliers, leurs noms et des illustrations recadrées depuis une source différente par carte.

**Architecture:** Contenu dans `PrototypeCollection` (helpers existants `C`, `Tiers`, `Dmg`…), noms dans `LocCollection`, outil d'éditeur `CardArtCropper` (logique d'attribution pure et testable + recadrage `Texture2D`) qui tient `SOURCES.md` ; aucun changement de moteur.

**Tech Stack:** Unity 6000.6.3f1, C# (nullable), NUnit, `UnityEditor` (outil d'éditeur).

**Spec:** `Code/docs/specs/2026-10-27-catalogue-36-cartes-design.md`

## Global Constraints
- Chemins relatifs à `Code/Heroes Dawn Unity/` (sauf `03_Art_UI_UX/` et les docs, relatifs à la racine du dépôt).
- La table des 36 cartes de la spec fait foi (identifiants, noms, rareté, nature, cible, recharge, base, paliers). Coût = recharge × 10 (déjà dérivé par `SkillDef`). Boucliers : durée 2 (helper `Shield`). `Gauge +100` = `new EffectDef(EffectType.GaugeDelta, 100)`. « Mark 1t » = `St(StatusType.Mark, 1)`.
- Les 12 cartes existantes ne changent pas.
- Illustrations : 640 × 360, `.jpg` qualité 85, teinte vers `Theme.FamilyColor(famille)` à 30 %, recadrage centré 16:9 ; sources dans `03_Art_UI_UX/Illustrations/` (récursif) ; les 23 images présentes le 2026-10-27 sont réservées (liste ci-dessous) ; nom de sortie `Assets/_Project/Resources/Illustrations/Cards/<id>.jpg`.
- Images réservées (section « Réservées » de `SOURCES.md`, chemins relatifs à `03_Art_UI_UX/Illustrations/`) : `Ennemis_Cryptides/` crocodile_kribelchi, cryptide_s1_kraken, cryptide_s2_mante, cryptide_s3_caprin, cryptide_s4_phalene ; `Lieux/` fete_foraine_abandonnee_homme_aureole, moscou_place_rouge_futuriste, paris_porte_des_enfers, ruines_littoral_pleine_lune ; `Personnages/` apex_adam_west_beret, apex_brie_rayner_laboratoire, apex_john_guerin_paris, armure_sombre_epee_ville_en_ruines, eda_face_aux_demons_enfers, eda_phoenix_aureole_colisee, femme_agee_poings_cathedrale, femme_cheveux_gris_cathedrale, heros_bouclier_energie_bleu, heros_cape_rouge_assis_ruines, heros_vert_epee_new_york, homme_blond_flammes_base_militaire, homme_dreadlocks_vertes_ruines, homme_gris_energie_rouge_salon (toutes en `.jpg`).
- Chaque test porte `[Test, Description("Vérifie que … — parce que …")]`. Tests en batch, éditeur fermé (`Code/SETUP.md`).
- Commits en français, préfixe conventionnel, terminés par `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.

## Review Focus
- **Cible « Soi » avec un effet d'élan ou de jauge** : `Advance` ignore le lanceur (`if (t == actor) break`) ; aucune carte Soi ne doit compter sur un élan. → test Task 1 (`SelfCards_UseNoAdvance`).
- **Carte de soutien en mode Auto** : l'IA alliée doit pouvoir lancer les nouvelles cartes alliées (Soi, ZA, 1A) sans erreur de cible. → test Task 2.
- **Source avec majuscules ou espaces dans le nom, sous-dossier nouveau** : l'outil la trouve, et `SOURCES.md` reste lisible. → test Task 3.
- **Relance de l'outil après génération** : rien n'est regénéré ni réattribué (idempotent). → test Task 3.
- **Image source plus étroite que 16:9 (portrait)** : recadrage centré sans bande noire, sortie toujours 640 × 360. → test Task 3.

---

### Task 1: Les 36 cartes et leurs noms

**Files:**
- Modify: `Assets/_Project/Content/Prototype/PrototypeCollection.cs` (36 appels `C(...)` après les 12 existants, par famille dans l'ordre de la spec ; helper `Gauge(int)` si absent)
- Modify: `Assets/_Project/Presentation/Common/LocCollection.cs` (36 clés `skills.<id>` à côté de `skills.card_ember`)
- Modify: `Assets/Tests/Editor/DesignToolsDataTests.cs` (`SpecNatures` passe à 48 entrées)
- Test: `Assets/Tests/Domain/CollectionRulesTests.cs`, `Assets/Tests/Presentation/CollectionPagesPresenterTests.cs`

**Interfaces:**
- Consumes: `C(string id, Family, CardNature, ItemRarity, int cooldown, TargetRule, CardTier[] tiers, params EffectDef[] fx)`, `Tiers(l3, l6, l10)`, `Dmg`, `Heal`, `Shield`, `St`, `Push`, `Adv`, `Steal`, `Purge`, `Bounce`, `Exec` (déjà dans `PrototypeCollection`).

- [ ] **Step 1: Write the failing tests**
  - `Catalog_Has48Cards_12PerFamily_4PerNature` (`CollectionRulesTests`) : 48 cartes, identifiants uniques ; pour chaque famille, 12 cartes et 4 par `CardNature`.
  - `Catalog_Rarities_Are5_4_2_1_PerFamily` : par famille, Common 5, Rare 4, Epic 2, Legendary 1.
  - `EveryCard_NatureEqualsItsDeduction` : `c.Nature == CardNatures.Of(c.Skill)` pour les 48.
  - `SelfCards_UseNoAdvance` : aucune carte `TargetRule.Self` n'a d'effet `Advance` ou `GaugeDelta` (base ou palier).
  - `NewCards_MatchTheSpec` : quelques points de la table : card_chain_lightning = Legendary, 1E, recharge 7, base `[Damage 160, ApplyStatus Shock 1 (40 %)]`, niveau 6 = Vulnerable 2 ; card_power_flux base = `GaugeDelta 100` sur `AllAllies` ; card_guard_break base = `[Damage 100, Purge 1, ApplyStatus Vulnerable 2]` ; card_rivet_armor cible `Self`, palier 3 = Taunt 1.
  - `EveryCard_HasAName` (`CollectionPagesPresenterTests`) : pour les 48, `_loc.Has("skills." + id)` et le nom n'est pas l'identifiant.
  - Mettre à jour `SpecNatures` dans `DesignToolsDataTests` avec les 36 nouvelles natures (colonne « Nature » de la spec).
  - Les tests C1 existants (`EveryCard_HasThreeTiers_At3_6_10`, `EnemyCards_CarryNoAllyEffects`, `Bounce_OnlyOnDamagingCards`) couvrent les 48 sans modification.
- [ ] **Step 2: Run** `-testFilter "N29.HeroesDawn.Tests.Domain.CollectionRulesTests;N29.HeroesDawn.Tests.Editor.DesignToolsDataTests;N29.HeroesDawn.Tests.Presentation.CollectionPagesPresenterTests"` — Expected: FAIL (12 cartes).
- [ ] **Step 3: Implement** les 36 cartes exactement selon les tableaux de la spec, puis les 36 noms.
- [ ] **Step 4: Run** les mêmes classes puis EditMode complet — Expected: PASS. Si un test de présentation dépend du nombre de cartes (ex. « 3 / 12 » dans le grimoire), le mettre à jour pour 48.
- [ ] **Step 5: Commit** — `feat(contenu): 36 nouvelles cartes (48 au total, 12 par famille)`.

### Task 2: Combat automatique avec les nouvelles cartes

**Files:**
- Test: `Assets/Tests/Domain/CardMechanicsTests.cs`

**Interfaces:**
- Consumes: catalogue de Task 1, `CombatReplay.Run`, `CombatSimulation` (`Auto`, `InputLog`, `StateHash`).

- [ ] **Step 1: Write the test** `AutoCombat_WithNewCards_EndsAndReplays` : pour chaque famille, une équipe de deux agents de cette famille (premiers agents du catalogue de test de la famille) équipés, au niveau 10, d'une nouvelle carte offensive et d'une nouvelle carte de soutien ou défensive de leur famille, contre 3 ennemis ; mode Auto (`CombatSetup(..., auto: true)` ou la commande Auto existante) ; boucle `Step(1)` jusqu'à `IsOver` (garde 40 000) ; le combat se termine et `CombatReplay.Run(...).StateHash == sim.StateHash()`.
- [ ] **Step 2: Run** `-testFilter N29.HeroesDawn.Tests.Domain.CardMechanicsTests` — Expected: PASS dès l'écriture (le moteur est fait) ; s'il échoue, c'est un défaut de contenu ou de moteur : `superpowers:systematic-debugging`.
- [ ] **Step 3: Commit** — `test(combat): combat automatique et rejeu avec les nouvelles cartes`.

### Task 3: Outil de recadrage et SOURCES.md

**Files:**
- Create: `Assets/_Project/Editor/Assets/CardArtCropper.cs` (namespace `N29.HeroesDawn.EditorTools.Assets`)
- Create: `Assets/_Project/Resources/Illustrations/Cards/SOURCES.md` (sections « Réservées » — les 23 chemins des Global Constraints — et « Attribuées », vide)
- Test: `Assets/Tests/Editor/CardArtCropperTests.cs`

**Interfaces:**
- Produces:
  - `public sealed class SourcesFile { public List<string> Reserved; public Dictionary<string, string> Assigned; public static SourcesFile Parse(string text); public string Write(); }` — format Markdown : `## Réservées` puis une ligne `- <chemin>` par source ; `## Attribuées` puis `- <id> → <chemin>`. Chemins relatifs à `03_Art_UI_UX/Illustrations/`, séparateur `/`.
  - `public static (Dictionary<string, string> Assignments, int Missing) Assign(IReadOnlyList<string> cardsWithoutArt, IReadOnlyList<string> sources, SourcesFile file)` — sources libres = ni réservées ni déjà attribuées, triées par chemin (ordinal, insensible à la casse) ; une par carte, dans l'ordre des cartes ; `Missing` = cartes restées sans source (si > 0, `Assignments` est vide).
  - `public static Texture2D Crop(Texture2D source, Color tint, int width = 640, int height = 360, float tintAmount = 0.3f)` — recadrage centré au rapport width/height (en largeur ou en hauteur selon la source), redimensionnement bilinéaire, `Color.Lerp(pixel, pixel * tint, tintAmount)` (alpha 1).
  - `[MenuItem("Heroes Dawn/Setup/Recadrer les illustrations de cartes")] public static void CropMissing()` et `public static void CropMissingBatch()` (sort avec code 1 si `Missing > 0`) — cartes sans image = cartes de `PrototypeCollection.Build()` sans fichier `<id>.jpg` ; sources = fichiers `.jpg/.jpeg/.png` sous `03_Art_UI_UX/Illustrations/` (racine du dépôt = `Application.dataPath/../../..`) ; écrit les images, met à jour `SOURCES.md`, `AssetDatabase.Refresh()`.

- [ ] **Step 1: Write the failing tests** (`CardArtCropperTests`) :
  - `SourcesFile_RoundTrips` : `Parse(Write())` rend les mêmes listes ; un chemin avec espaces et majuscules (`Nouveau Dossier/Héros 01.PNG`) survit.
  - `Assign_SkipsReservedAndAssigned_OnePerCard` : sources `[a, b, c, d]`, réservée `b`, attribuée `x → a` ; cartes `[y, z]` → `y → c`, `z → d`.
  - `Assign_StopsWhenSourcesAreMissing` : 3 cartes, 2 sources libres → `Missing == 1`, `Assignments` vide.
  - `Assign_IsIdempotent` : cartes sans image vides → aucune attribution, fichier inchangé.
  - `Crop_IsAlways640x360_AndTinted` : source 300 × 600 (portrait) blanche → sortie 640 × 360, pixel central = `Color.Lerp(white, tint, 0.3)` (± 0,02) ; source 1920 × 1080 → 640 × 360.
- [ ] **Step 2: Run** `-testFilter N29.HeroesDawn.Tests.Editor.CardArtCropperTests` — Expected: compilation en échec.
- [ ] **Step 3: Implement** `SourcesFile`, `Assign`, `Crop` (via `RenderTexture` + `Graphics.Blit` avec échelle/décalage, puis `ReadPixels`, puis teinte), et les deux points d'entrée ; créer `SOURCES.md` avec les 23 réservées.
- [ ] **Step 4: Run** — Expected: PASS ; EditMode complet vert.
- [ ] **Step 5: Commit** — `feat(art): outil de recadrage des illustrations de cartes et SOURCES.md`.

### Task 4: Générer les 36 illustrations (attend les nouvelles images)

**Files:**
- Create: `Assets/_Project/Resources/Illustrations/Cards/<id>.jpg` × 36 (+ `.meta` générés)
- Modify: `Assets/_Project/Resources/Illustrations/Cards/SOURCES.md`
- Test: `Assets/Tests/Editor/CardArtCropperTests.cs`

- [ ] **Step 0: Precondition** — au moins 36 images non réservées sous `03_Art_UI_UX/Illustrations/` (compter les fichiers hors liste « Réservées »). S'il en manque : arrêter la tâche, dire à l'utilisateur combien en ajouter, et passer à Task 5 ; reprendre Task 4 quand elles sont là.
- [ ] **Step 1: Write the failing test** `EveryCard_HasItsIllustration_AndSourcesAreUnique` : pour les 48 cartes, `Resources.Load<Sprite>("Illustrations/Cards/" + id)` non nul ; `SourcesFile.Parse(SOURCES.md).Assigned.Values` sans doublon et sans source réservée ; 36 attributions.
- [ ] **Step 2: Run** — Expected: FAIL (images absentes).
- [ ] **Step 3: Run the tool** — `-executeMethod N29.HeroesDawn.EditorTools.Assets.CardArtCropper.CropMissingBatch -quit` ; vérifier 36 nouveaux `.jpg` et 36 lignes « Attribuées ».
- [ ] **Step 4: Run** le test puis EditMode complet ; lancer `N29.HeroesDawn.Tests.PlayMode.CollectionPlayTests.CaptureTheCollectionScreens` et regarder `Code/docs/test-reports/captures/18-grimoire.png` (tuiles illustrées et distinctes) — Expected: PASS, rendu cohérent avec les 12 premières.
- [ ] **Step 5: Commit** — `feat(art): illustrations des 36 nouvelles cartes (recadrages du Drive)`.

### Task 5: Documentation et vérification finale

**Files:**
- Modify: `01_Game_Design/06_Kits_Competences.md` (section « Catégories des cartes » : 48 cartes, 12 par famille, 4 par nature, raretés 5/4/2/1, renvoi à la spec)
- Modify: `05_Production/Decisions.md` (1 ligne datée 2026-10-27)

- [ ] **Step 1: Write the docs.**
- [ ] **Step 2: Run** EditMode et PlayMode complets — Expected: 0 échec.
- [ ] **Step 3: Commit** — `docs: catalogue de 48 cartes` (avec les rapports `latest-*.html`).
