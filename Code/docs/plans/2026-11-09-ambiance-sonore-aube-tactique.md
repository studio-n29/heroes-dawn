# Ambiance sonore Aube tactique — Plan d'implémentation

> **Pour les agents :** exécution native (choix habituel du projet). Les étapes utilisent des cases `- [ ]`. Tests Unity lancés en batch.

**Objectif :** donner au jeu l'identité sonore Aube tactique : un thème de plateau par ville, un thème de boss, un thème de boutique, des SFX d'interface et de récompense refaits, et un mixage recalé.

**Architecture :** `MusicDirector` reçoit un contexte (`IMusicContext`) lui donnant la ville de la zone en cours, le rang du combat et la page de hub ouverte. `AudioService` et le format du catalogue ne changent pas. Les fichiers audio (Suno pour la musique, génération IA pour les SFX) sont déposés dans `Audio/Music/<MusicId>/` et `Audio/Sfx/<SfxId>/`, puis le catalogue est reconstruit.

**Stack :** Unity, C# (VContainer, MessagePipe, NUnit), Suno, outil de génération de SFX.

**Spec :** `Code/docs/specs/2026-11-09-ambiance-sonore-aube-tactique.md`

## Contraintes globales

- Ne pas renuméroter les valeurs existantes de `MusicId` et `SfxId` (catalogue sérialisé).
- Nouvelles valeurs `MusicId` : `BoardNewYork = 7`, `BoardParis = 8`, `BoardMoscow = 9`, `BoardSalem = 10`, `Shop = 11`. Nouvelles valeurs `SfxId` : `XpGain = 80`, `BoosterOpen = 81`, `RewardTile = 82`, `TentRest = 83`.
- `Board = 3` reste le repli quand la ville est inconnue.
- Commentaires et descriptions de tests en français, comme `MusicDirectorTests` (`Description("Vérifie que… — parce que…")`).
- Aucun changement dans `AudioService`.
- Musiques : instrumental, WAV ou MP3 320 normalisé vers −16 LUFS, silences de fin coupés. 15 pistes au total (spec).

## Review Focus

- Ville inconnue ou zone de tutoriel/repaire (`ZoneDef.TutorialBorough`, `LairBorough`) : `CityOfBorough` renvoie New York par défaut, la musique ne doit ni se taire ni planter.
- Aucune zone en cours (`RunZone == null`) à l'entrée sur le plateau : repli sur `Board`.
- Catalogue sans piste pour un nouveau thème (fichiers pas encore livrés) : `AudioService` coupe en douceur, le jeu ne reste pas muet pour les autres thèmes.
- Combat de boss puis retour au plateau : la reprise de position doit viser le thème de la ville, pas `Boss`.
- Quitter la boutique pendant un fondu : le thème du QG revient sans doublon de lecture.

---

### Task 1 : Identifiants et sélection pure de la musique

**Files:**
- Modify: `Code/Heroes Dawn Unity/Assets/_Project/Presentation/Audio/MusicId.cs`
- Modify: `Code/Heroes Dawn Unity/Assets/_Project/Presentation/Audio/SfxId.cs`
- Modify: `Code/Heroes Dawn Unity/Assets/_Project/Presentation/Audio/MusicDirector.cs`
- Test: `Code/Heroes Dawn Unity/Assets/Tests/Presentation/MusicDirectorTests.cs`

**Interfaces:**
- Produces: `MusicDirector.For(GameState state, string? city, bool boss, bool shop) -> MusicId` (statique, pure). `MusicDirector.ForCity(string? city) -> MusicId` (statique) : `new_york→BoardNewYork`, `paris→BoardParis`, `moscow→BoardMoscow`, `salem→BoardSalem`, autre ou null → `Board`.

- [ ] **Step 1 : tests en échec.** Dans `MusicDirectorTests`, ajouter `For_Board_PicksCityTheme` (assertions : `For(Board,"paris",false,false) == BoardParis`, idem pour les 4 villes, `For(Board,null,…) == Board`, `For(Board,"inconnue",…) == Board`), `For_Combat_Boss` (`For(Combat,"paris",true,false) == Boss`, `For(Combat,"paris",false,false) == Combat`), `For_Hub_Shop` (`For(Hub,null,false,true) == Shop`, `For(Hub,null,false,false) == Headquarters`), `For_Login_And_Boot` (`Login → MainTheme`, `Boot → None`).
- [ ] **Step 2 :** lancer ces tests en batch Unity ; attendu : erreur de compilation (`For` à 4 paramètres absent).
- [ ] **Step 3 :** ajouter les valeurs de `MusicId`/`SfxId` données dans les contraintes. Remplacer `For(GameState)` par `For(GameState, string?, bool, bool)` et ajouter `ForCity`, en utilisant les constantes de `StoryRoutes` (`NewYork`, `Paris`, `Moscow`, `Salem`).
- [ ] **Step 4 :** adapter `OnChanged` pour appeler `For(m.To, null, false, false)` provisoirement (le contexte arrive en Task 2), faire passer `EachState_PlaysItsTheme` et `CombatToBoard_Resumes` inchangés.
- [ ] **Step 5 :** relancer `MusicDirectorTests` ; attendu : tout vert.
- [ ] **Step 6 : commit** `feat(audio): identifiants des thèmes par ville, boss et boutique`

### Task 2 : Contexte musical (ville, boss) et câblage

**Files:**
- Create: `Code/Heroes Dawn Unity/Assets/_Project/Presentation/Audio/IMusicContext.cs`
- Create: `Code/Heroes Dawn Unity/Assets/_Project/Composition/SessionMusicContext.cs`
- Modify: `Code/Heroes Dawn Unity/Assets/_Project/Presentation/Audio/MusicDirector.cs`
- Modify: `Code/Heroes Dawn Unity/Assets/_Project/Composition/RootLifetimeScope.cs` (près de la ligne 194, enregistrement de `MusicDirector`)
- Test: `Code/Heroes Dawn Unity/Assets/Tests/Presentation/MusicDirectorTests.cs`

**Interfaces:**
- Produces: `interface IMusicContext { string? City { get; } bool BossFight { get; } bool ShopOpen { get; } }`. `MusicDirector(ISubscriber<GameStateChanged> changed, IAudioService audio, IMusicContext context)`.
- `SessionMusicContext : IMusicContext` lit `GameSession` : `City = session.RunZone == null ? null : StoryRoutes.CityOfBorough(session.RunZone.Borough)` ; `BossFight` vrai si le combat en attente (`session.Run?.PendingCase`) a le rang `CombatRank.Boss` (même source que `BeginCombatUseCase`, `pending.Rank`) ; `ShopOpen` renvoie faux jusqu'à la Task 3.

- [ ] **Step 1 : tests en échec.** Ajouter un `FakeMusicContext` (dans `Tests/Presentation/Fakes`) avec propriétés modifiables, et les tests `Board_PlaysCityTheme` (contexte `City="moscow"`, transition Hub→Board, attend `(BoardMoscow, false)`), `Combat_Boss_PlaysBossTheme`, `CombatToBoard_ResumesCityTheme` (Combat→Board avec `City="salem"` : `(BoardSalem, true)`), `Board_UnknownZone_FallsBackToBoard` (`City=null` : `(Board, false)`).
- [ ] **Step 2 :** lancer en batch ; attendu : échec de compilation puis de comportement.
- [ ] **Step 3 :** implémenter l'interface, le constructeur à trois paramètres et l'appel `For(m.To, _context.City, _context.BossFight, _context.ShopOpen)`. Adapter `SetUp` des tests existants pour passer un `FakeMusicContext`.
- [ ] **Step 4 :** implémenter `SessionMusicContext` et l'enregistrer en singleton `As<IMusicContext>` avant `RegisterEntryPoint<MusicDirector>()`.
- [ ] **Step 5 :** relancer la suite `Presentation` ; attendu : verte. Vérifier que le jeu compile (aucune autre référence à l'ancien `For`/constructeur : `Grep "MusicDirector"`).
- [ ] **Step 6 : commit** `feat(audio): musique selon la ville de la zone et le combat de boss`

### Task 3 : Musique de la boutique

**Files:**
- Modify: `Code/Heroes Dawn Unity/Assets/_Project/Presentation/Audio/MusicDirector.cs`
- Modify: `Code/Heroes Dawn Unity/Assets/_Project/Composition/SessionMusicContext.cs`
- Modify: le présentateur de navigation du hub (`Presentation/Hub/Shell/HubShellPresenter.cs`, à confirmer à l'étape 1)
- Test: `Code/Heroes Dawn Unity/Assets/Tests/Presentation/MusicDirectorTests.cs`

**Interfaces:**
- Consumes: `IMusicContext.ShopOpen`.
- Produces: un message `HubPageChanged` (ou équivalent déjà présent) que le directeur écoute pour relancer `PlayMusic(For(Hub, …, shop))`.

- [ ] **Step 1 :** lire `HubShellPresenter` et l'enum `HubPage` pour trouver comment la page courante est publiée (message existant, sinon en créer un dans `Application/Messages/FlowMessages.cs`). Noter la décision en tête de la tâche avant d'écrire le code.
- [ ] **Step 2 : tests en échec.** `Hub_ShopPage_PlaysShopTheme` (page Boutique → `(Shop, false)`), `Hub_LeaveShop_ReturnsToHeadquarters` (Boutique puis autre page → `(Headquarters, false)`), `Hub_ShopThenShop_NoDuplicateRequest` (deux signaux Boutique consécutifs → une seule requête).
- [ ] **Step 3 :** implémenter l'écoute dans `MusicDirector` (n'agir que si `GameState` courant est `Hub`) et faire retourner à `SessionMusicContext.ShopOpen` l'état de la page.
- [ ] **Step 4 :** tests de la suite `Presentation` ; attendu : verts.
- [ ] **Step 5 : commit** `feat(audio): musique de la boutique`

### Task 4 : Prompts Suno et génération des musiques

**Files:**
- Create: `Code/docs/audio/prompts-suno.md` (les 9 prompts du spec, un bloc par thème, avec colonne « choisi » à remplir)
- Create: `Code/Heroes Dawn Unity/Assets/_Project/Audio/Music/{BoardNewYork,BoardParis,BoardMoscow,BoardSalem,Boss,Shop}/` (dossiers)
- Replace: les pistes de `MainTheme`, `Headquarters`, `Combat`, retirer `Board/board-01.wav` et `board-02.wav` (conservés dans git)

- [ ] **Step 1 :** écrire `prompts-suno.md` à partir du spec (prompts, réglages communs, procédure 4 variantes → 1 ou 2 retenues, normalisation −16 LUFS). L'utilisateur génère dans Suno ; **je ne peux pas appeler Suno**.
- [ ] **Step 2 :** l'utilisateur dépose les fichiers retenus dans un dossier d'échange ; je vérifie nom, format, durée, silence de fin et loudness (ffmpeg `loudnorm` si disponible) avant de les ranger.
- [ ] **Step 3 :** ranger chaque piste dans son dossier `Audio/Music/<MusicId>/` avec les noms `<theme>-01`, `<theme>-02`.
- [ ] **Step 4 : commit** `feat(audio): musiques Aube tactique (15 pistes)` une fois les fichiers présents.

### Task 5 : Import et catalogue

**Files:**
- Modify (si besoin) : `Code/Heroes Dawn Unity/Assets/_Project/Editor/Audio/AudioImportRules.cs`
- Modify (si besoin) : `Code/Heroes Dawn Unity/Assets/_Project/Editor/Audio/AudioCatalogBuilder.cs`
- Regenerate: `Assets/Settings/AudioCatalog.asset`

**Interfaces:** consomme les nouveaux `MusicId`/`SfxId` (Task 1).

- [ ] **Step 1 :** lire `AudioImportRules` et `AudioCatalogBuilder` : vérifier que les dossiers sont découverts par le nom de l'enum et que les règles d'import (streaming pour la musique, compression Vorbis, mono pour les SFX) couvrent les nouveaux dossiers. Corriger uniquement si un nom est codé en dur.
- [ ] **Step 2 :** exécuter « Heroes Dawn ▸ Setup ▸ Audio ▸ Reconstruire le catalogue » en batch ; attendu : 6 nouveaux thèmes avec pistes, aucun avertissement de dossier vide.
- [ ] **Step 3 :** régler dans l'inspecteur : `Boss` volume 0.85, `fadeIn` 1.5 ; `Shop` `fadeIn` 2 ; thèmes de villes `fadeIn` 2, `trackCrossfade` 4, `resumePosition` activé (comme `Board`).
- [ ] **Step 4 :** contrôler le poids des clips (aucun WAV non compressé en build mobile) ; attendu : musiques en Vorbis streaming.
- [ ] **Step 5 : commit** `chore(audio): catalogue reconstruit avec les thèmes Aube tactique`

### Task 6 : SFX générés par IA

**Files:**
- Replace: clips de `Audio/Sfx/<SfxId>/` listés dans le spec (interface, récompenses, plateau, QG)
- Create: `Audio/Sfx/{XpGain,BoosterOpen,RewardTile,TentRest}/`
- Create: `Code/docs/audio/prompts-sfx.md`

- [ ] **Step 1 :** rédiger `prompts-sfx.md` : une ligne par son (nom, durée visée, prompt), style feutré/métal mat/relais, jamais « arcade ». Récompenses (ToastReward, ZoneComplete, Victory, Defeat) avec une touche orchestrale légère.
- [ ] **Step 2 :** générer avec l'outil de génération de SFX, 2 à 3 variantes par son ; **l'utilisateur écoute et valide** avant tout remplacement.
- [ ] **Step 3 :** exporter en OGG/WAV, couper silences, normaliser (pic −3 dB), ranger dans les dossiers.
- [ ] **Step 4 :** brancher les 4 nouveaux `SfxId` à leurs déclencheurs : `XpGain` (gain d'EXP en fin de combat), `BoosterOpen` (ouverture de booster), `RewardTile` (tuile de récompense), `TentRest` (repos). Repérer chaque appel avec `Grep "Play(SfxId"` et reproduire le motif existant ; un test de présentateur par déclencheur avec `FakeAudio`.
- [ ] **Step 5 :** reconstruire le catalogue ; régler `cooldown`, `maxInstances` et `duckMusicTo` (jingles de victoire/défaite : `duckMusicTo` 0.35).
- [ ] **Step 6 : commit** `feat(audio): effets sonores Aube tactique`

### Task 7 : Mixage, vérification et notes

**Files:**
- Modify: `Assets/Settings/AudioCatalog.asset` (réglages)
- Modify: notes de version / `Code/docs/inventaire-assets-aube-tactique.md` §2.4 (retirer « à confirmer »)

- [ ] **Step 1 :** suite complète de tests en batch (Domain + Presentation) ; attendu : tout vert.
- [ ] **Step 2 :** écoute guidée avec l'utilisateur : titre → QG → boutique → plateau de chaque ville → combat → boss → retour plateau. Vérifier l'enchaînement des pistes, la reprise de position, les fondus, la lisibilité des SFX sous la musique.
- [ ] **Step 3 :** ajuster volumes et ducking selon l'écoute.
- [ ] **Step 4 :** mise à jour de l'inventaire et des notes de version (en français).
- [ ] **Step 5 : commit** `docs(audio): ambiance sonore Aube tactique, mixage final`

## Auto-vérification

- **Couverture du spec :** charte et prompts (Task 4, 6), 15 pistes (Task 4), thèmes par ville plateau (Tasks 1–2), boss et boutique (Tasks 2–3), SFX et nouveaux `SfxId` (Task 6), catalogue (Task 5), tests et mixage (Task 7). `AudioService` inchangé : respecté.
- **Cohérence des signatures :** `For(GameState, string?, bool, bool)` défini Task 1, utilisé Tasks 2–3 ; `IMusicContext` défini Task 2, complété Task 3.
- **Limites :** Suno et la qualité musicale ne sont pas automatisables ; le motif de quatre notes se juge à l'oreille.
