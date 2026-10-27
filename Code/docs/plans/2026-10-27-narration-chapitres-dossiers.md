# Narration : cinématique, chapitres, dossier d'agent (lot G) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Mettre la narration du PDF dans la cinématique (sans voix), ajouter les 8 chapitres (fenêtre d'ouverture et page Chapitres) et afficher le dossier de lore complet sur la fiche de chaque agent.

**Architecture:** Chronométrage de légendes pur et testable (`CinematicTiming`) branché sur `CinematicPlayer`. Chapitres : table de domaine (`Chapters`), statuts calculés (`ChapterProgress`), chapitres vus dans `MetaSave` (`ChapterBook`), fenêtre `ChapterPopup` ouverte au déploiement, page liste `Chapters`. Lore : générateur d'éditeur (`LoreTextGenerator`) qui écrit `LocLore.cs` depuis le Markdown ; panneau « Dossier » sur la fiche d'agent.

**Tech Stack:** Unity 6000.6.3f1, C#, NUnit, VContainer, MessagePipe, UniTask, JsonUtility.

**Spec:** `Code/docs/specs/2026-10-27-narration-chapitres-dossiers-design.md`

## Global Constraints

- Code dans `Code/Heroes Dawn Unity` ; domaine dans `Packages/com.n29.heroesdawn.domain/Definitions`, application dans `Assets/_Project/Application`, présentation dans `Assets/_Project/Presentation`, éditeur dans `Assets/_Project/Editor`, tests dans `Assets/Tests`. `#nullable enable`.
- Textes joueur en français dans `Loc` (nouveaux fichiers `LocChapters.cs`, `LocLore.cs` sur le modèle de `LocCrafting.cs`, branchés par `ExtendChapters` / `ExtendLore` dans `Loc.cs`). Commentaires et descriptions de tests en français (`Description("Vérifie que … — parce que …")`).
- Écart acté à l'exécution : la durée totale de la cinématique n'est plus limitée à une minute (le texte du PDF est long) : le test borne la durée à 60-170 s et la cinématique reste passable (bouton existant).
- Écart acté : `CinematicShot.Voice` signifie « le plan a une légende » (S46 en a une maintenant) ; une voix n'est jouée que si `Resources/Cinematic/Voice/vo_<plan>.wav` existe.
- Prefabs : après un changement de recette, `Unity.exe -batchmode -nographics -projectPath … -executeMethod N29.HeroesDawn.EditorTools.Prefabs.PrefabGenerator.GenerateAllBatch -quit -overwrite <chemins relatifs séparés par des virgules>` ; les prefabs et `Settings/ScreenCatalog.asset` modifiés sont commités avec la tâche. Une cible tactile fait au moins 96 px de haut (test `HubLayoutTests`).
- Fichiers Markdown et C# du dépôt : éditer avec un script Node qui normalise les fins de ligne (certains fichiers sont en CRLF).
- Tests : `ut.sh` de `.superpowers/sdd/2026-10-27-narration-chapitres-dossiers/` (`ut.sh EditMode '<filtre>'`), éditeur fermé.

## Review Focus

- Aucune voix manquante ne bloque la cinématique (plan sans `vo_*.wav` → légende au temps de lecture) → tests Task 1.
- La fenêtre de chapitre ne s'affiche qu'une fois, même après rechargement du profil ; Retour sur la fenêtre n'empêche pas le déploiement → tests Task 3.
- Ancien profil sans `chaptersSeen` → test Task 2.
- Les 32 agents jouables ont un dossier complet, `LocLore.cs` est à jour avec le Markdown, aucun champ vide ou tronqué → tests Task 5.
- Le bouton DOSSIER ouvre et ferme le panneau sans casser la fiche ni le soin / l'entraînement → tests Task 6.

---

### Task 1: Cinématique : nouvelle narration, légendes au temps de lecture

**Files:**
- Create: `Assets/_Project/Presentation/Login/Cinematic/CinematicTiming.cs`
- Modify: `CinematicPlayer.cs` (`RunShot`, `UpdateText`), `CinematicScript.cs` (`Kept`, `S46`), `Presentation/Common/LocCinematic.cs`
- Delete: `Assets/_Project/Resources/Cinematic/Voice/vo_*.wav` et `.meta`, le plan `S20` de `CinematicScript.Build()`
- Modify: `Code/docs`→ `02_Lore_Narration/05_Cinematique_Lancement.md` (section « Narration du PDF »)
- Test: `Assets/Tests/Presentation/CinematicScriptTests.cs` (mis à jour), `CinematicTimingTests.cs`

**Interfaces:**
- Produces (namespace `N29.HeroesDawn.Presentation.Login`) : `static class CinematicTiming { const float VoiceStart = 0.4f; static int Words(string text); static float ShotSeconds(float minSeconds, float voiceLength, string subtitle); static float SubtitleAlpha(float t, float voiceLength, float shotSeconds); }`
  - `ShotSeconds` : avec voix (`voiceLength > 0`) = `max(minSeconds, VoiceStart + voiceLength + 0.4)` ; sans voix et légende non vide = `max(minSeconds, 1.2 + 0.35 × Words(subtitle))` ; sans légende = `minSeconds`.
  - `SubtitleAlpha` : `local = t − VoiceStart` ; 0 si `local < 0` ; visible jusqu'à `end = voiceLength > 0 ? voiceLength + 0.4 : shotSeconds − VoiceStart − 0.2` ; `clamp01(local/0.3) × clamp01((end − local)/0.4)`.

- [ ] **Step 1: Write the failing tests** :
  - `CinematicTimingTests` : `Words("a b  c")` = 3, `Words("")` = 0 ; `ShotSeconds(3, 0, "")` = 3 ; `ShotSeconds(3, 0, 20 mots)` = 8.2 (1,2 + 7) ; `ShotSeconds(3, 0, "un deux")` = 3 (minimum) ; `ShotSeconds(3, 5, "texte")` = 5,8 ; `SubtitleAlpha(0.2, 0, 8)` = 0 ; `SubtitleAlpha(2, 0, 8)` = 1 ; `SubtitleAlpha(7.7, 0, 8)` = 0 (disparu avant la fin) ; avec voix de 4 s : `SubtitleAlpha(2, 4, 5)` = 1 et `SubtitleAlpha(4.8, 4, 5)` ≈ 0.
  - `CinematicScriptTests` : `Shots_AreTheCondensedCut` → `{ S01, S02, S10, S11, S15, S24, S33, S37, S41, S44, S46 }` ; `EveryVoiceAndSfx_Exists` ne cherche plus de `.wav` de voix mais exige une légende non vide `Loc.T("cinematic.vo." + id)` pour tout plan `Voice` ; `SilentShots_AreTheDocumentedOnes` → aucun plan muet, `Loc.CinematicVoiceIds` = tous les plans ; nouveau `NoVoiceFilesRemain` : `Resources.Load<AudioClip>("Cinematic/Voice/vo_" + id)` est nul pour chaque plan (aucun `.wav` de voix dans le dépôt) ; `TotalDuration_FollowsTheReadingTime` : somme des `CinematicTiming.ShotSeconds(s.Seconds, 0, légende)` ∈ [60, 170] ; `EraTitles_AreOnTheDocumentedShots` inchangé ; `NoCaptionIsEmptyOrTooLong` : chaque légende fait de 3 à 45 mots.
- [ ] **Step 2: Run** `N29.HeroesDawn.Tests.Presentation.CinematicTimingTests;N29.HeroesDawn.Tests.Presentation.CinematicScriptTests` — Expected: FAIL.
- [ ] **Step 3: Implement** : `CinematicTiming` ; `CinematicPlayer.RunShot` : `slot.Duration = CinematicTiming.ShotSeconds(shot.Seconds, voiceLen, subtitle)` ; `UpdateText` : alpha = `CinematicTiming.SubtitleAlpha(t, voiceLen, slot.Duration)` sans attendre `voiceStarted` ; `CinematicScript` : `Kept` sans S20, S46 avec `Voice = true`, plan `S20` supprimé de `Build()` ; `Loc.CinematicVoiceIds` = les 11 plans ; `git rm` des 11 `vo_*.wav` + `.meta` (S20 compris) ; légendes de `LocCinematic.cs` (table de la spec §1, textes légèrement raccourcis pour tenir en 45 mots au plus par plan, fidèles au PDF) :
  - S01 « Chaque aube naît de la nuit. » · S02 « Pendant la Guerre froide, l'Amérique a utilisé les recherches d'un généticien pour créer une arme aussi efficace que la bombe atomique, sans la fin du monde. » · S10 « Après plusieurs essais ratés, le monde a découvert l'Apex Unit : des humains génétiquement modifiés, aux pouvoirs extraordinaires, qui ont mis fin à la Guerre froide en quelques mois. » · S11 « Une longue fausse paix a suivi, où l'Apex Unit était l'étendard d'une justice manipulatrice, fondée sur le rapport de force. » · S15 « Jusqu'en 2026, où un jeune homme a réveillé ses pouvoirs enfouis : le premier méta-humain naturel de l'histoire. » · S24 « Avec son éveil, le monde a été entraîné dans une bataille contre une créature biblique, alors au rang de légende : Mephisto. » · S33 « À Paris, Mephisto s'est opposé à l'Apex Unit et a ouvert une brèche entre les mondes, qui allait tout anéantir… » · S37 « … jusqu'à ce que le jeune homme apparaisse, déploie ses ailes et se sacrifie dans ce qu'on appellera La Supernova. » · S41 « Ses radiations, disparues dans la brèche avec Mephisto, ont altéré l'ADN d'une grande partie de l'humanité : une vague de méta-humains sans précédent est née. » · S44 « Aujourd'hui, cinq ans plus tard, les méta-humains cherchent leur place, tandis que l'Apex Unit a laissé la place à The Dawn. » · S46 « Le monde est partagé entre acceptation, contrôle et anéantissement de ces nouveaux humains. »
  Doc : `05_Cinematique_Lancement.md` reçoit la section « Narration du PDF (version courte) » avec ces 11 lignes et le nom de fichier de voix attendu `vo_<plan>.wav`.
- [ ] **Step 4: Run** les mêmes filtres + `N29.HeroesDawn.Tests.Presentation` — Expected: PASS.
- [ ] **Step 5: Commit** — `feat(narration): cinématique avec la narration du PDF, légendes au temps de lecture`.

### Task 2: Chapitres : données, statuts, sauvegarde

**Files:**
- Create: `Packages/com.n29.heroesdawn.domain/Definitions/Chapters.cs`, `Assets/_Project/Application/Features/World/ChapterProgress.cs`, `Assets/_Project/Application/Features/World/ChapterBook.cs`
- Modify: `Assets/_Project/Application/Ports/MetaSave.cs` (`chaptersSeen`), `RootLifetimeScope.cs`, `Assets/Tests/Domain/Application/TestApp.cs` (`Chapters`)
- Test: `Assets/Tests/Domain/ChaptersTests.cs`, `Assets/Tests/Domain/Application/ChapterBookTests.cs`

**Interfaces:**
- Produces :
  - `sealed class ChapterDef { string Id; string City; int Order; IReadOnlyList<string> Boroughs; string TitleKey => "chapter." + Id + ".title"; string IntroKey => "chapter." + Id + ".intro"; }` et `static class Chapters { IReadOnlyList<ChapterDef> All; ChapterDef? Find(string id); ChapterDef? OfZone(ZoneDef zone); IReadOnlyList<ChapterDef> OfCity(string city); }` (8 chapitres de la spec §2, `OfZone` = par `ZoneDef.Borough`, nul pour une zone de tutoriel ou sans chapitre).
  - `enum ChapterStatus { Done, Current, Locked, Soon }` ; `static class ChapterProgress { ChapterStatus StatusOf(ISessionReadModel session, ChapterDef chapter); IReadOnlyList<ChapterDef> InDisplayOrder(ISessionReadModel session); }` (affichage : chapitres de la ville de l'acte en cours, puis des autres villes dans l'ordre de `session.Route.ActCities`).
  - `MetaSave.chaptersSeen` (`List<string>`) ; `sealed class ChapterBook(MetaContext)` : `bool Seen(string id)`, `void MarkSeen(string id)` (idempotent, `Touch(MetaArea.Profile)`), `int DoneCount()`.

- [ ] **Step 1: Write the failing tests** :
  - `ChaptersTests` : 8 chapitres, identifiants uniques `ny_brooklyn, ny_queens, ny_harlem, ny_manhattan, mos_sergueiv, mos_kapotnya, mos_sadovoye, mos_place_rouge`, `Order` 1 à 4 par ville, `OfCity("new_york")` = 4 dans l'ordre ; `OfZone` d'une zone `borough == "brooklyn"` = `ny_brooklyn`, d'une zone de tutoriel = null.
  - `ChapterBookTests` (`TestApp` avec le contenu de test) : statuts : `ny_queens`, `mos_*` = `Soon` (aucune zone) ; `ny_brooklyn` = `Current` au départ pour Agent D ; après validation de toutes les zones de Brooklyn (`Session.CompletedZones`) = `Done` ; avec 1 zone sur 4 validée = `Current` ; `InDisplayOrder` commence par les 4 chapitres de New York pour la route D ; `Seen` / `MarkSeen` : idempotent, enregistré dans `Meta.chaptersSeen` ; aller-retour de sauvegarde (`ToProfile` / `ApplyProfile`) conserve les chapitres vus ; `OldSave_WithoutChaptersSeen_Loads` (profil créé sans le champ) : liste vide, aucune erreur ; `DoneCount` suit les chapitres `Done`.
- [ ] **Step 2: Run** `ChaptersTests;ChapterBookTests` — Expected: FAIL.
- [ ] **Step 3: Implement** : table dans `Chapters.cs` ; `StatusOf` : `Soon` si aucune zone (`ZoneProgress.InBorough`/`session.Content.Zones` filtrées par quartier, hors tutoriel) ; `Done` si toutes `ZoneStatus.Done` ; sinon `Current` pour le premier chapitre non terminé et non `Soon` de la ville de `StoryProgress.Current(session).City`, sinon `Locked` ; `ChapterBook` ; enregistrer `ChapterBook` en singleton (racine et `TestApp`).
- [ ] **Step 4: Run** — Expected: PASS.
- [ ] **Step 5: Commit** — `feat(narration): chapitres, statuts et chapitres vus`.

### Task 3: Fenêtre d'ouverture de chapitre et déploiement

**Files:**
- Create: `Presentation/Common/LocChapters.cs`, `Presentation/Hub/Pages/Chapters/ChapterPopupView.cs`, `ChapterIntroFlow.cs`, `Editor/Prefabs/Recipes/ChapterPopupRecipe.cs` (`Popups/ChapterPopup.prefab`)
- Modify: `Presentation/Common/Loc.cs` (branchement `ExtendChapters`), `Presentation/Hub/Pages/CityMap/CityMapPagePresenter.cs`, `HubPageFactory.cs` (`b.Register<ChapterIntroFlow>(Lifetime.Scoped)`), `Presentation/Common/AgentArt.cs` (`Chapter(id)` : `Illustrations/Chapters/<id>`, cache y compris absent)
- Test: `Assets/Tests/Presentation/ChapterIntroTests.cs` ; mise à jour de l'appel du présentateur dans `CityMapPresenterTests`

**Interfaces:**
- Consumes: Task 2 (`Chapters`, `ChapterBook`), `IModalService`, modèle `FriendAddPopupView` / `FriendAddPopupRecipe`.
- Produces :
  - `readonly struct ChapterPopupData { string Heading; string Text; Sprite? Art; string Continue; }`, `interface IChapterPopupView : IPopupView { UniTask ShowAsync(ChapterPopupData data, CancellationToken ct); }`, `ChapterPopupView : MonoBehaviour, IChapterPopupView` (Retour = Continuer).
  - `sealed class ChapterIntroFlow(IModalService, ILocalization)` : `UniTask ShowAsync(ChapterDef chapter, CancellationToken ct)` (titre « CHAPITRE {Order} · {titre en majuscules} », texte `chapter.<id>.intro`, image `AgentArt.Chapter`).
  - `CityMapPagePresenter` : paramètres optionnels en dernier `ChapterBook? chapters = null, ChapterIntroFlow? intro = null`.
  - Clés : `chapter.<id>.title` et `chapter.<id>.intro` pour les 8 chapitres (textes de la spec §2), `chapter.heading` = « CHAPITRE {0} · {1} », `chapter.continue` = « CONTINUER ».

- [ ] **Step 1: Write the failing tests** (`ChapterIntroTests`, `FakeModalHost` avec un faux popup `IChapterPopupView` qui se termine tout de suite ou sur demande) :
  - `Deploy_FirstTime_ShowsTheChapterIntroThenLaunches` : zone de Brooklyn, chapitre non vu : une fenêtre est ouverte avec `Heading` « CHAPITRE 1 · BROOKLYN » et le texte de `chapter.ny_brooklyn.intro` ; le déploiement n'a lieu qu'après la fermeture ; le chapitre est vu.
  - `Deploy_SecondTime_LaunchesDirectly` : chapitre déjà vu : aucune fenêtre.
  - `Deploy_IntroClosedByBack_StillLaunches` : le popup se ferme par `RequestCancel` : le déploiement a lieu et le chapitre est vu.
  - `Deploy_ZoneWithoutChapter_NoIntro` : zone de tutoriel : aucune fenêtre.
  - `Deploy_DisposedDuringIntro_DoesNotLaunch` : `Dispose` du présentateur pendant la fenêtre : aucun lancement.
  - `EveryChapter_HasATitleAndAnIntro` : les 16 clés sont traduites (`Loc.T(key) != key`), chaque texte fait de 15 à 60 mots ; `Heading` d'un chapitre de Moscou « CHAPITRE 4 · LA PLACE ROUGE ».
- [ ] **Step 2: Run** `ChapterIntroTests;CityMapPresenterTests` — Expected: FAIL.
- [ ] **Step 3: Implement** : `Deploy` du présentateur : après le contrôle de verrou, si `chapters`, `intro` et un chapitre non vu existent → `DeployWithIntroAsync` (`UniTaskVoid`) : `await intro.ShowAsync`, puis `if (Disposed) return;`, `chapters.MarkSeen`, `_mission.Launch` ; une annulation de la tâche (`OperationCanceledException`) ne lance rien ; recette de la fenêtre sur le modèle de `FriendAddPopupRecipe` (fenêtre 1400 × 760 : image large en haut (hauteur 300, `AspectRatioFitter` enveloppe, masquée si pas d'image), titre, texte (28), bouton Continuer).
- [ ] **Step 4: Run** `ChapterIntroTests;CityMapPresenterTests;N29.HeroesDawn.Tests.Presentation` — Expected: PASS.
- [ ] **Step 5:** régénérer `Popups/ChapterPopup.prefab`.
- [ ] **Step 6: Commit** — `feat(narration): fenêtre d'ouverture de chapitre au premier déploiement`.

### Task 4: Page Chapitres et bouton sur la carte

**Files:**
- Create: `Presentation/Hub/Pages/Chapters/ChaptersPage.cs`, `ChaptersPageView.cs`
- Modify: `HubPage.cs` (`Chapters`), `HubPageFactory.cs`, `ICityMapPageView.cs` (`event Action ChaptersClicked`), `CityMapPageView.cs`, `CityMapPagePresenter.cs` (paramètre optionnel `HubNavigator? nav = null`), `CityMapRecipes.cs` (bouton « Chapitres » 96 px sous le détail, le panneau de détail raccourci d'autant), `ListPageRecipes.cs` (`ChaptersPageRecipe` → `Pages/Chapters.prefab`), `LocChapters.cs`, `Tests/Presentation/CityMapPresenterTests.cs` (`FakeMapView`)
- Test: `Assets/Tests/Presentation/ChaptersPageTests.cs`

**Interfaces:**
- Consumes: Tasks 2 et 3.
- Produces : `interface IChaptersPageView : IListPageView {}`, `ChaptersPageView : ListPageView`, `ChaptersPagePresenter(IChaptersPageView, ISessionReadModel, ChapterBook, ChapterIntroFlow, ILocalization, IToastService, ISubscriber<MetaChanged>, ISubscriber<WalletChanged>, ISubscriber<RosterChanged>, IClock)` : en-tête (titre « Chapitres », ligne « n / 8 chapitres terminés », barre) ; une rangée par `ChapterProgress.InDisplayOrder` : étiquette = numéro, couleur selon le statut, titre = titre du chapitre, sous-titre = statut (clés `chapter.status.Done|Current|Locked|Soon`) + début du texte d'ouverture (120 caractères puis « … »), bouton A « RELIRE » (grisé pour `Soon` avec la raison `chapter.soon_reason`) qui appelle `ChapterIntroFlow.ShowAsync` sans toucher aux chapitres vus ; rangée `Current` mise en avant (`Highlight`), `Locked`/`Soon` estompées.

- [ ] **Step 1: Write the failing tests** (`ChaptersPageTests`, `FakeListView`, `TestApp`) :
  - `Rows_FollowTheDisplayOrder_WithTheirStatus` : 8 rangées ; les 4 premières sont celles de New York ; Brooklyn `Current` (mise en avant), les 7 autres estompées avec leur statut dans le sous-titre ; en-tête « 0 / 8 ».
  - `Done_ChapterShowsInTheHeaderCount` : toutes les zones de Brooklyn validées → « 1 / 8 » et statut `Done`.
  - `Reread_OpensTheIntro_WithoutMarkingItSeen` : clic sur Relire de Brooklyn → le faux popup s'ouvre avec le bon texte ; `Meta.chaptersSeen` reste vide.
  - `Reread_IsDisabledForSoonChapters` : bouton A grisé avec la raison pour `ny_queens`.
  - `CityMap_ChaptersButton_OpensTheChaptersPage` : `FakeMapView.ClickChapters()` → `HubNavigator.Current.Page == HubPage.Chapters`.
- [ ] **Step 2: Run** `ChaptersPageTests;CityMapPresenterTests` — Expected: FAIL.
- [ ] **Step 3: Implement** la page, la fabrique (`HubPage.Chapters => Make<ChaptersPageView, IChaptersPageView, ChaptersPagePresenter>(location)`), le bouton de la carte (recette : le panneau de détail passe de `offsetMin y = 108` à `214`, le bouton « Chapitres » est posé à `y = 108`, hauteur 96) et les textes (`page.chapters` = « Chapitres », `chapters.header`, `chapters.line` = « {0} / {1} chapitres terminés », `chapter.relire` = « RELIRE », `chapters.button` = « CHAPITRES »).
- [ ] **Step 4: Run** `ChaptersPageTests;CityMapPresenterTests;N29.HeroesDawn.Tests.Presentation;N29.HeroesDawn.Tests.Editor` — Expected: PASS.
- [ ] **Step 5:** régénérer `Pages/Chapters.prefab` et `Pages/CityMap.prefab`.
- [ ] **Step 6: Commit** — `feat(narration): page Chapitres et bouton sur la carte`.

### Task 5: Textes de lore générés depuis le Markdown

**Files:**
- Create: `Assets/_Project/Editor/Design/LoreTextGenerator.cs`, `Assets/_Project/Presentation/Common/LocLore.cs` (généré, commité)
- Modify: `Presentation/Common/Loc.cs` (branchement `ExtendLore`)
- Test: `Assets/Tests/Editor/LoreTextGeneratorTests.cs`

**Interfaces:**
- Produces (namespace `N29.HeroesDawn.EditorTools.Design`) : `static class LoreTextGenerator { static string AgentIdOf(string name); static IReadOnlyDictionary<string, Dictionary<string, string>> Parse(string markdown, ISet<string> agentIds); static string Render(IReadOnlyDictionary<string, Dictionary<string, string>> lore); [MenuItem("Heroes Dawn/Setup/Générer les textes de lore")] static void Run(); }`. Champs : `age`, `sex`, `origin`, `supernova`, `story`, `look`, `power` (clés Markdown `Âge`, `Sexe`, `Origine`, `Lien à la Supernova`, `Origin Story`, `Visuel`, `Pouvoir`). `Render` produit le code de `Loc.ExtendLore` avec une clé `lore.<idAgent>.<champ>` par texte (guillemets, barres inverses et sauts de ligne échappés ; ordre : agents dans l'ordre du Markdown, champs dans l'ordre ci-dessus).

- [ ] **Step 1: Write the failing tests** (`LoreTextGeneratorTests`) :
  - `AgentIdOf_Normalizes` : `"Brooks, Darius"` → `darius_brooks` ; `"Hallström, Astrid"` → `astrid_hallstrom` ; `"Lefèvre, Clément"` → `clement_lefevre` ; `"Park, Ji-Hoon"` → `ji_hoon_park` ; `"Okafor, Samuel « Sam »"` → `sam_okafor` ; `"Agent D"` → `agent_d`.
  - `Parse_ReadsTheFieldsOfOneAgent` : un extrait de Markdown de test (deux agents, un inconnu) ne retient que l'agent présent dans `agentIds` avec ses champs.
  - `Render_EscapesAndOrders` : un texte avec guillemets et `\` donne un code C# valide (compiler n'est pas nécessaire : comparer à la chaîne attendue).
  - `LocLore_IsUpToDateWithTheMarkdown` : lit `02_Lore_Narration/04_Agents_The_Dawn.md` (chemin depuis `Application.dataPath` : `../../../02_Lore_Narration/04_Agents_The_Dawn.md`) et les identifiants de `Resources/Content/agents.json` (lecture des champs `"id"` par expression régulière), puis `Render(Parse(...))` doit être égal au contenu de `Assets/_Project/Presentation/Common/LocLore.cs` (fins de ligne normalisées).
  - `EveryPlayableAgent_HasItsDossier` : pour chacun des 32 agents de `agents.json`, les clés `age`, `sex`, `origin`, `look`, `power` existent dans `Loc` (`Loc.Has`) ; pour les 30 agents numérotés (tous sauf `agent_d` et `agent_m`), `supernova` et `story` aussi ; aucun texte vide.
- [ ] **Step 2: Run** `LoreTextGeneratorTests` — Expected: FAIL.
- [ ] **Step 3: Implement** le générateur (parcours des sections `## `, champs `- **<Champ>** : <texte>`, le nom vient du champ `Nom`) puis l'exécuter (`-executeMethod N29.HeroesDawn.EditorTools.Design.LoreTextGenerator.Run -quit`) pour produire `LocLore.cs` ; brancher `ExtendLore` dans `Loc.cs`.
- [ ] **Step 4: Run** `LoreTextGeneratorTests` — Expected: PASS.
- [ ] **Step 5: Commit** — `feat(lore): textes de lore des agents générés depuis le Markdown`.

### Task 6: Dossier sur la fiche d'agent

**Files:**
- Modify: `Presentation/Hub/Pages/MemberSheet/{IMemberSheetView.cs,MemberSheetData.cs,MemberSheetView.cs,MemberSheetPresenter.cs}`, `Editor/Prefabs/Recipes/TeamRecipes.cs` (fiche d'agent : bouton « DOSSIER » ≥ 96 px et panneau défilant), `Presentation/Common/LocLore.cs` ne change pas ; textes de sections dans `LocChapters.cs` (ou `LocProfile.cs`)
- Create: `Presentation/Hub/Pages/MemberSheet/DossierText.cs`
- Test: `Assets/Tests/Presentation/DossierTests.cs`, `MemberSheetPresenterTests.cs` (fake vue)

**Interfaces:**
- Consumes: Task 5 (clés `lore.<id>.<champ>`), `ILocalization.Has`.
- Produces : `static class DossierText { IReadOnlyList<DossierSection> Sections(ILocalization loc, string agentId); }` avec `readonly struct DossierSection { string Title; string Text; }` : `Identité` (« {âge} · {sexe} · {origine} »), `Lien à la Supernova`, `Origin Story`, `Pouvoir`, `Apparence`, dans cet ordre, sections sans texte omises ; clés de titre `dossier.identity|supernova|story|power|look`, `dossier.button` = « DOSSIER », `dossier.close` = « FERMER ».
  - `IMemberSheetView` : `event Action DossierClicked; event Action DossierClosed;` ; `MemberSheetData` : `bool ShowDossierButton; string DossierButtonLabel;` ; vue : `void ShowDossier(DossierData data)` / `void HideDossier()` avec `struct DossierData { string Title; DossierSection[] Sections; string Close; }`.
  - `MemberSheetPresenter` : `DossierClicked` → `View.ShowDossier(...)` ; `DossierClosed` → `View.HideDossier()` ; changer de fiche (Précédent / Suivant) ferme le panneau ; le bouton n'est affiché que si `DossierText.Sections` n'est pas vide.

- [ ] **Step 1: Write the failing tests** :
  - `DossierTests` : `Sections(loc, "darius_brooks")` = 5 sections dans l'ordre, la première contient « 41 ans » et « Détroit » ; `Sections(loc, "agent_d")` = 3 sections (Identité, Pouvoir, Apparence : pas de Supernova ni d'Origin Story) ; un identifiant inconnu = liste vide ; aucune section n'a de texte vide.
  - Présentateur (fiche d'agent, `FakeMemberSheetView` + `ClickDossier()` / `CloseDossier()`) : `Dossier_Button_ShownOnlyWithLore` (agent du jeu : `ShowDossierButton` vrai ; identifiant sans lore : faux) ; `Dossier_Opens_ThenCloses` : `ClickDossier` → `View.LastDossier` non nul avec les 5 sections de l'agent ; `CloseDossier` → nul ; `Dossier_ClosesWhenTheSheetChanges` : après `ClickNext` le panneau est fermé ; `Dossier_DoesNotBreakHeal` : ouvrir le dossier puis cliquer Soigner fonctionne comme avant.
- [ ] **Step 2: Run** `DossierTests;MemberSheetPresenterTests` — Expected: FAIL.
- [ ] **Step 3: Implement** ; recette : bouton « DOSSIER » (96 px) à côté de « Équipement », panneau (`Dossier`) plein sur la colonne de droite avec un défilement vertical, un titre, un texte riche (une section = titre doré + texte) et un bouton Fermer 96 px ; la vue construit le texte avec balises TMP (`<color>`, `<b>`) à partir des sections.
- [ ] **Step 4: Run** `DossierTests;MemberSheetPresenterTests;N29.HeroesDawn.Tests.Presentation;N29.HeroesDawn.Tests.Editor` — Expected: PASS.
- [ ] **Step 5:** régénérer `Pages/MemberSheet.prefab`.
- [ ] **Step 6: Commit** — `feat(lore): dossier complet de l'agent sur sa fiche`.

### Task 7: Documentation, captures et suites complètes

**Files:**
- Modify: `Code/docs/specs/2026-10-27-narration-chapitres-dossiers-design.md` (écarts actés : durée de la cinématique, `Voice` = légende), `03_Art_UI_UX/01_UX_Ecrans.md` (page Chapitres, fenêtre d'ouverture, dossier), `05_Production/Decisions.md` (3 lignes datées 2026-10-27), `Tests/PlayMode/ScreenCaptureTests.cs` (captures de la fenêtre de chapitre, de la page Chapitres et de la fiche avec son dossier)

- [ ] **Step 1: Update docs.**
- [ ] **Step 2: Add PlayMode captures** (`CaptureHubPages` ou test explicite dédié) : fenêtre de chapitre au déploiement, page Chapitres, fiche d'agent avec dossier ouvert.
- [ ] **Step 3: Run** `ut.sh EditMode` puis `ut.sh PlayMode` complets, puis le test de captures — Expected: 0 échec ; regarder les captures (légende de cinématique lisible, texte du dossier lisible et défilant).
- [ ] **Step 4: Commit** — `docs: narration, chapitres et dossiers du lot G` (avec les rapports `Code/docs/test-reports/latest-*.html`, sans les fichiers de l'utilisateur).
