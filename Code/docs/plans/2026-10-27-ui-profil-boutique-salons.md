# UI : Profil, boutique en cartes, salons sur la fiche d'agent (lot F) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Ajouter la page Profil (tag, statistiques, amis, accès au registre), passer la boutique en cartes avec icônes, et ramener l'entraînement et les soins sur la fiche d'agent.

**Architecture:** Règles pures (tag, validation des amis) et services dans la couche Application (`PlayerTag`, `LocalFriendsPort`, `PlayerProfile`, `HealMemberUseCase`), sauvegardés dans `MetaSave`. Présentation MVP existante : nouvelle page `ProfilePage` (liste), `BoutiquePageView` basée sur `CardGridPageView`, fiche d'agent étendue. Les prefabs sont régénérés depuis leurs recettes ; les pages Salon, Infirmerie et Lounge sont supprimées.

**Tech Stack:** Unity 6000.6.3f1, C#, NUnit, VContainer, MessagePipe, UniTask, JsonUtility, sharp (Node, hors dépôt) pour rasteriser les SVG.

**Spec:** `Code/docs/specs/2026-10-27-ui-profil-boutique-salons-design.md`

## Global Constraints

- Code dans `Code/Heroes Dawn Unity` ; application dans `Assets/_Project/Application`, présentation dans `Assets/_Project/Presentation`, recettes de prefabs dans `Assets/_Project/Editor/Prefabs/Recipes`, tests dans `Assets/Tests`. `#nullable enable`.
- Textes joueur en français dans `Loc` (nouveau fichier `LocProfile.cs` sur le modèle de `LocCrafting.cs`, branché par `ExtendProfile` dans `Loc.cs`). Commentaires et descriptions de tests en français (`Description("Vérifie que … — parce que …")`).
- Écarts actés à l'exécution par rapport à la spec (à reporter dans la spec en Task 10) : le bouton « Registre » du Profil ouvre `HubPage.Agents` (le registre des agents) et un second bouton « Archives » ouvre `HubPage.Archives` ; le QG n'a plus qu'une carte « Équipe et soins » qui ouvre `HubPage.Team` (déjà triable par fatigue), sans filtres nouveaux.
- La boutique garde ses règles d'achat, ses onglets et ses données.
- Une ancienne sauvegarde (sans `stats` ni `friends`) se charge sans erreur.
- Prefabs : après un changement de recette, `Unity.exe -batchmode -nographics -projectPath … -executeMethod N29.HeroesDawn.EditorTools.Prefabs.PrefabGenerator.GenerateAllBatch -quit -overwrite <chemins relatifs séparés par des virgules>` (ex. `Pages/Boutique.prefab,Popups/FriendAddPopup.prefab`) ; les prefabs et `Settings/ScreenCatalog.asset` modifiés sont commités avec la tâche.
- Tests : `ut.sh` de `.superpowers/sdd/2026-10-27-ui-profil-boutique-salons/` (`ut.sh EditMode '<filtre>'`), éditeur fermé.

## Review Focus

- Pseudo vide ou avec `#` : le tag reste valide (`Invité#NNNN`) → test Task 1.
- Ami : espaces, casse (doublon), `#` multiples, nom trop long, 50 maximum, propre tag → tests Task 1 et 2.
- Soin d'un agent : sans fatigue ou sans Crédits, rien n'est dépensé ; la fatigue ne descend pas sous 0 → tests Task 4.
- Plus aucune référence à `HubPage.Salon`, `HubPage.Infirmary`, `LoungePage` (code, prefabs, tests) → test Task 8.
- Achats de la boutique inchangés (boosters, objets, gemmes, échanges, cadeau) → tests de présentateur conservés Task 9.
- Badge des Archives visible sur l'onglet Profil → test Task 5.

---

### Task 1: Tag personnel et validation des amis

**Files:**
- Create: `Assets/_Project/Application/Features/Meta/PlayerTag.cs`
- Test: `Assets/Tests/Domain/Application/PlayerTagTests.cs`

**Interfaces:**
- Produces (namespace `N29.HeroesDawn.Application.Features.Meta`) : `static class PlayerTag { const int MaxNameLength = 20; static string Of(string playerId, string pseudo); static bool TryParse(string tag, out string name, out int code); static string Format(string name, int code); }`

- [ ] **Step 1: Write the failing tests** (`PlayerTagTests`) :
  - `Of_IsStableAndPinned` : `Of("p1", "Dorian") == "Dorian#1274"` ; `Of("player-2", "Dorian") == "Dorian#7023"` ; deux appels identiques donnent le même tag.
  - `Of_CleansThePseudo` : `Of("p1", "  Ma#ra  ")` = `"Mara#1274"` ; `Of("p1", "")`, `Of("p1", "   ")` et `Of("p1", "#")` = `"Invité#1274"` ; un pseudo de 30 caractères est tronqué à 20.
  - `Of_AlwaysParses` : pour 200 identifiants (`"id" + i`) et les pseudos `""`, `"a"`, `"Ñandú"`, `TryParse(Of(...))` renvoie vrai avec un code de 0 à 9999.
  - `TryParse_AcceptsOnlyNameHashFourDigits` : vrai pour `"Mara#0042"` (nom `Mara`, code 42) et `"  Mara#0042 "` ; faux pour `"Mara"`, `"Mara#42"`, `"Mara#00420"`, `"#0042"`, `"Mara#00a2"`, `"Ma#ra#0042"`, `""`, un nom de 21 caractères.
  - `Format_PadsTheCode` : `Format("Mara", 7) == "Mara#0007"`.
- [ ] **Step 2: Run** `ut.sh EditMode N29.HeroesDawn.Tests.Domain.Application.PlayerTagTests` — Expected: FAIL (compilation).
- [ ] **Step 3: Implement** `PlayerTag` : code = FNV-1a 32 bits de `playerId` (octets des caractères UTF-16, `hash ^= c; hash *= 16777619`, départ 2166136261, `uint`) modulo 10000 ; nettoyage du pseudo : `Trim()`, suppression des `#`, `Trim()` de nouveau, troncature à 20, vide → `"Invité"`.
- [ ] **Step 4: Run** — Expected: PASS.
- [ ] **Step 5: Commit** — `feat(profil): tag personnel et analyse des tags d'amis`.

### Task 2: Sauvegarde, port d'amis et statistiques

**Files:**
- Modify: `Assets/_Project/Application/Ports/MetaSave.cs`, `Assets/_Project/Application/Features/Meta/MetaTracker.cs`, `Assets/_Project/Composition/RootLifetimeScope.cs`, `Assets/Tests/Domain/Application/TestApp.cs`
- Create: `Assets/_Project/Application/Ports/IFriendsPort.cs`, `Assets/_Project/Application/Features/Meta/LocalFriendsPort.cs`
- Test: `Assets/Tests/Domain/Application/FriendsAndStatsTests.cs`

**Interfaces:**
- Consumes: Task 1 (`PlayerTag.TryParse`, `PlayerTag.Format`, `PlayerTag.Of`).
- Produces :
  - `MetaSave` : `List<CounterSave> stats` (identifiants `combatsWon`, `combatsLost`, `creditsEarned`), `List<string> friends`.
  - `readonly struct FriendEntry { string Tag; string Name; }` et `interface IFriendsPort { IReadOnlyList<FriendEntry> Friends(); Result Add(string tag); Result Remove(string tag); }` (namespace `N29.HeroesDawn.Application.Ports`).
  - `static class Stats { const string CombatsWon, CombatsLost, CreditsEarned; static int Get(MetaSave m, string id); static void Add(MetaSave m, string id, int n); }` (namespace `…Features.Meta`).
  - `FriendErrors { Invalid, Self, Duplicate, Full }` (`DomainError` avec codes `friends.invalid`, `friends.self`, `friends.duplicate`, `friends.full` et clés de texte `errors.friends_invalid` etc.), `LocalFriendsPort(MetaContext)`, `const int MaxFriends = 50`.
  - `TestApp.Friends` (`LocalFriendsPort`).

- [ ] **Step 1: Write the failing tests** (`FriendsAndStatsTests`, `TestApp` + `SignInDirect("p1")`) :
  - `Add_StoresATrimmedNormalizedTag` : `Add("  Mara#7 ")` refusé (`friends.invalid`) ; `Add(" Mara#0007 ")` accepté, `Friends()` = un ami `Tag "Mara#0007"`, `Name "Mara"`.
  - `Add_RefusesSelfDuplicatesAndOverflow` : son propre tag (`PlayerTag.Of("p1", pseudo de la session)`) → `friends.self` ; `Add("mara#0007")` après `Mara#0007` → `friends.duplicate` (insensible à la casse) ; 50 amis acceptés puis le 51e → `friends.full`.
  - `Remove_IsCaseInsensitive_AndIdempotent` : retirer `MARA#0007` retire l'ami ; retirer un ami absent réussit sans rien changer.
  - `Friends_SurviveSaveAndReload_OldSaveHasNone` : aller-retour `ToProfile` / `ApplyProfile` en JSON conserve la liste et l'ordre ; un `MetaSave` neuf n'a aucun ami ni compteur.
  - `Tracker_CountsCombatsAndCredits` : une victoire de combat (message `CombatFinished` avec rapport gagné) fait passer `combatsWon` à 1 et `combatsLost` reste 0 ; une défaite l'inverse ; un gain de Crédits (+300 via le portefeuille, publié `WalletChanged`) ajoute 300 à `creditsEarned` (y compris une récompense versée par `MetaContext.Grant`), une dépense n'ajoute rien. S'inspirer de `MetaServiceTests` pour publier `CombatFinished` / `WalletChanged` ; le tracker n'est armé qu'après `GameStateChanged` hors Login/Boot (voir `MetaTracker.OnState`).
- [ ] **Step 2: Run** `FriendsAndStatsTests` — Expected: FAIL.
- [ ] **Step 3: Implement** : `MetaSave.stats` et `friends` en fin de classe (listes vides) ; `LocalFriendsPort.Add` : `PlayerTag.TryParse` puis `Format`, refus du propre tag (comparaison avec `PlayerTag.Of(Session.Account?.PlayerId ?? "local", Meta.onboarding.pseudo)`), doublon, plein ; `Touch(MetaArea.Settings)` n'existe pas pour les amis : ajouter `MetaArea.Profile` à l'enum `MetaArea` et l'utiliser. `MetaTracker.OnCombat` : compteur gagné ou perdu **avant** le `return` de défaite ; `OnWallet` : toute hausse de Crédits (même `Granting`) s'ajoute à `creditsEarned` quand le tracker est armé. Enregistrer `LocalFriendsPort` (`.As<IFriendsPort>()`, singleton) dans `RootLifetimeScope` et `TestApp`.
- [ ] **Step 4: Run** `FriendsAndStatsTests;MetaServiceTests` — Expected: PASS.
- [ ] **Step 5: Commit** — `feat(profil): amis (port simulé local) et compteurs de statistiques`.

### Task 3: Statistiques du profil

**Files:**
- Create: `Assets/_Project/Application/Features/Meta/PlayerProfile.cs`
- Modify: `RootLifetimeScope.cs`, `TestApp.cs`
- Test: `Assets/Tests/Domain/Application/PlayerProfileTests.cs`

**Interfaces:**
- Consumes: Task 1, Task 2 (`Stats`), `TeamLinks`, `PassTrack`/`PassProgress.Level`.
- Produces : `readonly struct ProfileStats { int CombatsWon, CombatsLost, ZonesCleared, Agents, TopAgentLevel, BestLinkWins, CraftedItems, CreditsEarned; }` et `sealed class PlayerProfile(MetaContext, PassProgress)` avec `string Pseudo`, `string Tag`, `string HeroId`, `int PassLevel`, `ProfileStats Stats()`.

- [ ] **Step 1: Write the failing tests** (`PlayerProfileTests`) :
  - `Tag_UsesPseudoAndPlayerId` : session `p1`, pseudo `Dorian` → `Dorian#1274` ; pseudo vide → `Invité#1274` ; `Pseudo` renvoie `Invité` si vide.
  - `Stats_ReadTheSaveAndTheSession` : avec 2 victoires, 1 défaite (compteurs écrits via `Stats.Add`), 3 zones dans `Session.CompletedZones`, `Roster` du `TestApp`, un lien de 7 victoires (`TeamLinks.RecordVictory` ou écriture directe de `Meta.links`) et 2 objets fabriqués (`Meta.crafted`), les champs de `ProfileStats` correspondent (`TopAgentLevel` = niveau max du `Roster`, `BestLinkWins` = 7, `Agents` = `Roster.Count`).
  - `Stats_OfAnEmptySave_AreZero` : une session neuve (sans compteurs) donne 0 partout sauf `Agents` et `TopAgentLevel`.
- [ ] **Step 2: Run** — Expected: FAIL.
- [ ] **Step 3: Implement** `PlayerProfile` ; `HeroId` = `Session.Route.HeroId`.
- [ ] **Step 4: Run** — Expected: PASS.
- [ ] **Step 5: Commit** — `feat(profil): service de statistiques du profil`.

### Task 4: Soigner un agent

**Files:**
- Create: `Assets/_Project/Application/Features/Hub/IHealMemberUseCase.cs`, `HealMemberUseCase.cs`
- Modify: `Assets/_Project/Application/Session/GameSession.cs` (`MetaRules.HealMemberCost = 50`), `RootLifetimeScope.cs`, `TestApp.cs` (`Hub` cas d'usage : suivre le modèle des autres)
- Test: `Assets/Tests/Domain/Application/HealMemberTests.cs`

**Interfaces:**
- Consumes: `MetaRules.InfirmaryReduction` (30), `HubErrors.NothingToHeal`, `HubErrors.NotEnoughCredits`, `HubErrors.UnknownMember`.
- Produces : `interface IHealMemberUseCase { UniTask<Result> ExecuteAsync(MemberId member, CancellationToken ct = default); }` ; `MetaRules.HealMemberCost = 50`.

- [ ] **Step 1: Write the failing tests** (`HealMemberTests`) :
  - `Heal_SpendsCostAndReducesFatigue` : agent à 50 de fatigue, 200 Crédits : après le soin, fatigue 20 et Crédits 150 ; les autres agents ne changent pas.
  - `Heal_FatigueNeverBelowZero` : fatigue 10 → 0.
  - `Heal_RefusedWithoutFatigue_SpendsNothing` : fatigue 0 → `HubErrors.NothingToHeal` ; Crédits inchangés.
  - `Heal_RefusedWithoutCredits_ChangesNothing` : 49 Crédits, fatigue 50 → `HubErrors.NotEnoughCredits` ; fatigue inchangée.
  - `Heal_UnknownMember` : identifiant absent de l'effectif → `HubErrors.UnknownMember`.
  - `Heal_PublishesWalletAndRoster` : `Received<WalletChanged>()` et `Received<RosterChanged>()` augmentent de 1.
- [ ] **Step 2: Run** — Expected: FAIL.
- [ ] **Step 3: Implement** sur le modèle de `HealRosterUseCase` (`GameSession.Find(MemberId)`).
- [ ] **Step 4: Run** `HealMemberTests;N29.HeroesDawn.Tests.Domain.Application` — Expected: PASS.
- [ ] **Step 5: Commit** — `feat(hub): soin d'un agent`.

### Task 5: Onglet Profil et page Profil

**Files:**
- Modify: `Presentation/Hub/Navigation/HubPage.cs` (ajout `Profile`), `HubNavigator.cs` (`Tabs` : `Archives` remplacé par `Profile`), `Hub/Shell/HubShellPresenter.cs` (`NavKey` : `Profile => "nav.profile"` ; `BadgeOf` : l'onglet `Profile` reprend le badge des Archives ; `Archives` n'est plus un onglet), `HubPageFactory.cs`, `Presentation/Common/Loc.cs` (branchement `ExtendProfile`)
- Create: `Presentation/Hub/Pages/Profile/ProfilePage.cs`, `ProfilePageView.cs`, `Presentation/Common/LocProfile.cs`, `Editor/Prefabs/Recipes/ProfileRecipes.cs`
- Test: `Assets/Tests/Presentation/ProfilePagePresenterTests.cs` ; mise à jour de `HubNavigatorTests.cs`, `HubShellPresenterTests.cs`, `PlayMode/HubNavigationFlowTests.cs` pour la 6e tab `Profile`

**Interfaces:**
- Consumes: Tasks 2 et 3 (`PlayerProfile`, `IFriendsPort`, `FriendEntry`), `ListPagePresenter<TView>`.
- Produces :
  - `interface IProfilePageView : IListPageView {}` ; `ProfilePageView : ListPageView, IProfilePageView` ; `ProfilePagePresenter(IProfilePageView, PlayerProfile, IFriendsPort, HubNavigator, ILocalization, IToastService, ISubscriber<MetaChanged>, ISubscriber<WalletChanged>, ISubscriber<RosterChanged>, IClock)`.
  - Rangées : en-tête (titre = tag, ligne = « pseudo · héros · niveau de pass », bouton A « Copier mon tag », bouton B « Ajouter un ami ») ; 8 rangées de statistiques (titre = libellé, étiquette = valeur) ; un titre de section « Amis » (rangée estompée) ; une rangée par ami (titre = nom, sous-titre = tag + « en attente de connexion en ligne », bouton A « Retirer ») ; deux rangées de navigation « Registre » (→ `HubPage.Agents`) et « Archives » (→ `HubPage.Archives`) avec bouton A « Ouvrir ».
  - Ordre des rangées : navigation d'abord (2), puis statistiques (8), puis section Amis (1) puis amis. L'index de chaque action est mémorisé à chaque rendu (liste d'`Action`, comme `BoutiquePagePresenter`).
  - `ProfilePagePresenter.CopyTag` : copie via `GUIUtility.systemCopyBuffer`, toast « Tag copié » ; l'ajout d'ami est branché en Task 6 (le bouton B appelle `AddFriendRequested`, événement vide ici).
  - Clés : `profile.title`, `profile.line`, `profile.copy`, `profile.copied`, `profile.add`, `profile.registry`, `profile.archives`, `profile.open`, `profile.stat.<combatsWon|combatsLost|zones|agents|topLevel|bestLink|crafted|credits>`, `profile.friends`, `profile.friend_pending`, `profile.friend_remove`, `profile.friend_removed`, `profile.friends_empty`, `errors.friends_invalid`, `errors.friends_self`, `errors.friends_duplicate`, `errors.friends_full`.

- [ ] **Step 1: Write the failing tests** (`ProfilePagePresenterTests`, `FakeListView` de `MetaPagesPresenterTests`) :
  - `Header_ShowsTheTagAndStats` : titre de l'en-tête = `Dorian#1274` (pseudo configuré), les huit statistiques présentes avec leurs valeurs (étiquette).
  - `NavigationRows_OpenRegistryAndArchives` : clic sur la rangée Registre → `HubNavigator.Current.Page == HubPage.Agents` ; Archives → `HubPage.Archives` (la pile contient alors l'onglet puis la page).
  - `Friends_AreListedWithARemoveButton` : deux amis ajoutés par `_app.Friends.Add` : deux rangées après la section Amis ; clic sur Retirer retire l'ami et rafraîchit.
  - `EmptyFriends_ShowsAHint` : sans ami, une rangée estompée `profile.friends_empty`.
  - `Header_CopyTag` : clic sur le bouton A de l'en-tête → toast `profile.copied` (la copie réelle n'est pas vérifiée).
  - `NavigatorTabs_AreTheSix_WithProfileLast` : `HubNavigator.Tabs` = `Qg, Team, Recruit, Pass, Shop, Profile` ; `IsTab(HubPage.Archives)` faux ; ouvrir `Archives` empile la page.
  - `ShellBadge_OfTheProfileTab_IsTheArchivesBadge` : avec une fiche d'archives non lue, la tab `Profile` porte le badge (`HubShellPresenterTests` : suivre le test existant sur le badge des Archives).
- [ ] **Step 2: Run** `N29.HeroesDawn.Tests.Presentation.ProfilePagePresenterTests;N29.HeroesDawn.Tests.Presentation.HubNavigatorTests;N29.HeroesDawn.Tests.Presentation.HubShellPresenterTests` — Expected: FAIL.
- [ ] **Step 3: Implement** la page, la fabrique (`HubPage.Profile => Make<ProfilePageView, IProfilePageView, ProfilePagePresenter>(location)`), la recette `ProfilePageRecipe` (`Pages/Profile.prefab`, `ListPageKit.Build<ProfilePageView>(ctx, "ProfilePage", 0, false, false)`), les textes (`LocProfile.cs`). Les tests existants qui citent `HubPage.Archives` comme onglet sont mis à jour.
- [ ] **Step 4: Run** les mêmes filtres + `N29.HeroesDawn.Tests.Presentation` — Expected: PASS.
- [ ] **Step 5:** régénérer `Pages/Profile.prefab` (commande de prefabs des Global Constraints) ; vérifier que `ScreenCatalog.asset` contient la page.
- [ ] **Step 6: Commit** — `feat(profil): page Profil et onglet Profil`.

### Task 6: Ajouter un ami (saisie du tag)

**Files:**
- Create: `Presentation/Hub/Profile/FriendAddPopupView.cs` (dans `Presentation/Hub/Pages/Profile/`), `Editor/Prefabs/Recipes/FriendAddRecipes.cs` (`Popups/FriendAddPopup.prefab`)
- Modify: `ProfilePage.cs` (bouton B), `LocProfile.cs`
- Test: `Assets/Tests/Presentation/FriendAddTests.cs`

**Interfaces:**
- Consumes: Task 5, `IModalService.OpenAsync<TView>`, modèle `PlaytestFormPopupView` / `PlaytestFormPopupRecipe` (champ `TMP_InputField`, `PopupKit.Window`).
- Produces : `interface IFriendAddPopupView : IPopupView { UniTask<string?> AskAsync(string title, string hint, string ok, string cancel, CancellationToken ct); }` (renvoie le texte saisi, ou null si annulé) ; `FriendAddPopupView : MonoBehaviour, IFriendAddPopupView` ; le présentateur `ProfilePagePresenter` reçoit `IModalService` en dernier paramètre.

- [ ] **Step 1: Write the failing tests** (`FriendAddTests`, avec un `FakeFriendAddPopup` fourni par le test, enregistré dans le `FakeModalHost` comme les autres popups de tests) :
  - `AddFriend_WithAValidTag_AddsAndToasts` : le popup renvoie `"Mara#0007"` → l'ami est ajouté, toast de succès, la liste se rafraîchit.
  - `AddFriend_Cancelled_ChangesNothing` : le popup renvoie null → aucun ami, aucun toast d'erreur.
  - `AddFriend_InvalidOrDuplicate_ShowsTheErrorToast` : `"Mara"` → toast d'erreur `errors.friends_invalid` ; ajout d'un doublon → `errors.friends_duplicate`.
- [ ] **Step 2: Run** `FriendAddTests` — Expected: FAIL.
- [ ] **Step 3: Implement** : vue sur le modèle du formulaire de playtest (fenêtre de petite taille, champ de texte, boutons Ajouter / Annuler, retour = Annuler) ; recette `FriendAddPopupRecipe` ; le bouton B de l'en-tête ouvre le popup puis appelle `IFriendsPort.Add`. Si `FakeModalHost` ne sait pas fabriquer ce popup, suivre la manière dont `PlaytestFormFlow` est testé.
- [ ] **Step 4: Run** `FriendAddTests;ProfilePagePresenterTests` — Expected: PASS.
- [ ] **Step 5:** régénérer `Popups/FriendAddPopup.prefab`.
- [ ] **Step 6: Commit** — `feat(profil): ajout d'un ami par son tag`.

### Task 7: Soin sur la fiche d'agent et sur la page Équipe

**Files:**
- Modify: `Presentation/Hub/Pages/MemberSheet/{IMemberSheetView.cs,MemberSheetData.cs,MemberSheetView.cs,MemberSheetPresenter.cs}`, `Editor/Prefabs/Recipes/TeamRecipes.cs` (fiche d'agent et page Équipe), `Presentation/Hub/Pages/Team/TeamPagePresenter.cs` et sa vue, `LocProfile.cs` (ou `Loc.cs` : `sheet.heal`, `hub.healed_member`, `team.heal_all`)
- Test: `Assets/Tests/Presentation/MemberSheetPresenterTests.cs`, `TeamPagePresenterTests.cs`

**Interfaces:**
- Consumes: Task 4 (`IHealMemberUseCase`), `IHealRosterUseCase`, `MetaRules.HealMemberCost`.
- Produces :
  - `IMemberSheetView` : `event Action HealClicked` (remplace `RoomClicked`, supprimé) ; `MemberSheetData` : `ShowHeal`, `HealLabel`, `HealEnabled`, `HealReason` (remplacent `ShowRooms`) ; `MemberSheetView` : champ `heal` (le bouton `infirmary` devient `heal`, le bouton `salon` est supprimé).
  - `MemberSheetPresenter` reçoit `IHealMemberUseCase` ; libellé « Soigner · 50 CR » ; grisé avec raison `qg.infirmary.none` (« rien à soigner ») ou `hub.train.missing` (Crédits).
  - Page Équipe : bouton « Soigner toute l'équipe · 150 CR » (`IHealRosterUseCase`), grisé sans fatigue ou sans Crédits.

- [ ] **Step 1: Write the failing tests** :
  - `MemberSheet_Heal_ButtonStates` : sans fatigue → `HealEnabled` faux, raison de `qg.infirmary.none` ; avec fatigue et 49 Crédits → faux, raison « il manque 1 CR » ; avec fatigue et 200 Crédits → vrai, libellé contenant `50`.
  - `MemberSheet_Heal_SpendsAndRefreshes` : `HealClicked` → Crédits −50, fatigue −30, la fiche se rafraîchit (nouveau `HealEnabled`), toast `hub.healed_member`.
  - `MemberSheet_HasNoRoomButtons` : l'événement `RoomClicked` n'existe plus ; `MemberSheetData` n'a plus `ShowRooms` (le test compile avec la nouvelle forme).
  - `Team_HealAll_Button` : état grisé / actif selon fatigue et Crédits ; clic → toutes les fatigues baissent de 30, Crédits −150.
- [ ] **Step 2: Run** `MemberSheetPresenterTests;TeamPagePresenterTests` — Expected: FAIL.
- [ ] **Step 3: Implement** (modifier les recettes `TeamRecipes.cs`, régénérer les prefabs de la fiche d'agent et de la page Équipe : relever leurs `RelativePath` dans le fichier).
- [ ] **Step 4: Run** `N29.HeroesDawn.Tests.Presentation` — Expected: PASS.
- [ ] **Step 5: Commit** — `feat(hub): soin sur la fiche d'agent et soin d'équipe sur la page Équipe`.

### Task 8: Suppression des salles et carte du QG

**Files:**
- Delete: `Presentation/Hub/Pages/Infirmary/`, `Presentation/Hub/Pages/Salon/` (dont `LoungePage`, `SalonPage`, `MemberRowView`), `Editor/Prefabs/Recipes/RoomRecipes.cs`, `LoungePageRecipe` dans `CollectionPageRecipes.cs`, `Prefabs/Pages/{Infirmary,Lounge,Salon}.prefab` (+ `.meta`), `Tests/Presentation/{RoomPagePresenterTests.cs,Fakes/FakeRoomPages.cs}` ; les tests des pages supprimées ailleurs
- Modify: `HubPage.cs` (retrait de `Salon` et `Infirmary`), `HubPageFactory.cs`, `QgPagePresenter.cs`, `HubShellPresenter.cs` (astuce `infirmary`), `Presentation/Common/Loc*.cs` (textes orphelins), `HubNavigatorTests.cs`, `HubShellPresenterTests.cs`, `QgPagePresenterTests.cs`, `MemberSheetPresenterTests.cs`, `PlayMode/HubNavigationFlowTests.cs`, `PlayMode/ScreenCaptureTests.cs`
- Test: `Assets/Tests/Presentation/QgPagePresenterTests.cs`, `NoRoomPagesTests.cs`

**Interfaces:**
- Consumes: Task 7 (plus de `RoomClicked`).
- Produces : le QG a une carte `RoomCardData { Page = HubPage.Team, Title = "Équipe et soins" (clé page.team_care), Info = lounge.line-like avec le nombre d'agents à entraîner et de fatigués, Badge = fatigués }` à la place de la carte Salon ; l'astuce du tutoriel `infirmary` (ouverture de la page Infirmerie) devient une astuce sur la page Équipe (`HubPage.Team` et fatigue) avec le texte « Soigne un agent depuis sa fiche ».

- [ ] **Step 1: Write the failing tests** :
  - `Qg_CareCard_OpensTheTeam` : la carte de soins ouvre `HubPage.Team`, son badge = nombre d'agents fatigués ; aucune carte n'ouvre `Salon` ni `Infirmary`.
  - `NoRoomPages_Anywhere` (`NoRoomPagesTests`) : `Enum.GetNames(typeof(HubPage))` ne contient ni `Salon` ni `Infirmary` ; un test de source lit les `.cs` de `Assets/_Project` et `Assets/Tests` et vérifie qu'aucun ne contient `HubPage.Salon`, `HubPage.Infirmary`, `LoungePage`, `SalonPage`, `InfirmaryPage` (en excluant ce fichier de test lui-même).
  - `Tutorial_InfirmaryHint_PointsToTheTeamPage` : à l'ouverture de la page Équipe avec un agent fatigué, l'astuce `infirmary` est proposée ; elle n'est plus liée à une page Infirmerie.
- [ ] **Step 2: Run** `QgPagePresenterTests;NoRoomPagesTests;HubShellPresenterTests` — Expected: FAIL.
- [ ] **Step 3: Implement** les suppressions et le renvoi ; supprimer les textes devenus orphelins uniquement s'ils ne sont utilisés nulle part (`grep` des clés `lounge.`, `infirmary.`, `sheet.salon`, `sheet.infirmary`, `page.salon`, `page.infirmary`). Régénérer `ScreenCatalog.asset` (commande de prefabs avec un chemin quelconque déjà existant) pour retirer les pages supprimées.
- [ ] **Step 4: Run** EditMode complet — Expected: PASS.
- [ ] **Step 5: Commit** — `refactor(hub): suppression des pages Salon et Infirmerie, carte Équipe et soins au QG`.

### Task 9: Boutique en cartes

**Files:**
- Modify: `Presentation/Hub/Pages/Boutique/BoutiquePageView.cs` (`CardGridPageView` + onglets), `BoutiquePage.cs` (art des offres), `Presentation/Common/AgentArt.cs` (`Shop(string id)`), `Editor/Prefabs/Recipes/CollectionPageRecipes.cs` (`CardGridKit.Build` : paramètre `shopTabs`), `ListPageRecipes.cs` (`BoutiquePageRecipe`), `Presentation/Common/LocProfile.cs` ou `Loc*.cs` (descriptions de cartes)
- Create: `03_Art_UI_UX/Illustrations/Shop/*.svg` (8 icônes : `booster`, `gems_small`, `gems_medium`, `gems_large`, `exchange_credits`, `exchange_pr`, `exchange_rest`, `gift`), `Assets/_Project/Resources/Illustrations/Shop/*.png` (rasterisés), `Tests/Presentation/ShopCardsTests.cs`

**Interfaces:**
- Consumes: `CardGridPageView` (tuiles, détail à droite, onglets via `ShopTabsView`), `IBoutiquePageView` (`ShopTabClicked`, `ShowShopTab(int)`).
- Produces : `BoutiquePageView : CardGridPageView, IBoutiquePageView` avec `ShopTabClicked` et `ShowShopTab`; `AgentArt.Shop(string id) : Sprite?` (charge `Illustrations/Shop/<id>` ; cache par chemin) ; chaque rangée de la boutique renseigne `InfoRowData.Art` (objets : `AgentArt.Item(CollectionText.ArtId(it, defs))` ; autres offres : `AgentArt.Shop(<id>)`) et un sous-titre de description courte ; l'en-tête n'affiche plus le texte « simulé ».
- Icônes : SVG 512×512, formes à facettes cohérentes avec la direction artistique (fond transparent, couleurs du thème : Primary, Perception, Credits) ; rasterisées en PNG 512×512 par un script Node (`sharp`) conservé dans `03_Art_UI_UX/Illustrations/Shop/README.md` (commande).

- [ ] **Step 1: Write the failing tests** (`ShopCardsTests`, `BoutiquePagePresenter` avec `FakeListView`) :
  - `Rows_HaveADescription_AndAnArtId` : pour chaque onglet (boosters, objets, gemmes, cadeau), chaque rangée active a un sous-titre non vide ; l'art d'un objet vient de son illustration, celui des autres offres du chemin `Illustrations/Shop/<id>` (le test charge les sprites via `AgentArt.Shop` et accepte null en PlayMode seulement : en EditMode, il vérifie `AgentArt.ShopPath(id)` qui renvoie le chemin attendu, fonction pure ajoutée à `AgentArt`).
  - `Header_NoLongerShowsTheSimulatedNote` : `Header.Line` ne contient pas `Loc.T("shop.simulated")`.
  - `Purchases_Unchanged` : les tests existants de la boutique (`MetaPagesPresenterTests.Boutique_*`) restent verts sans modification de leur contenu.
  - `Svg_AllEightIconsExist` : les huit fichiers `.svg` et `.png` existent aux emplacements prévus (lecture du système de fichiers dans un test EditMode, `Application.dataPath`).
- [ ] **Step 2: Run** `ShopCardsTests;MetaPagesPresenterTests` — Expected: FAIL.
- [ ] **Step 3: Implement** : écrire les 8 SVG ; script Node de rasterisation (hors dépôt, dans le scratchpad) vers `Resources/Illustrations/Shop/` ; vue et recette (`CardGridKit.Build<BoutiquePageView>(ctx, "BoutiquePage", shopTabs: true)` : nicher `Widgets/ShopTabs.prefab` sous l'en-tête et décaler la grille et la fiche de `ShopTabsRecipe.Height + 10`, comme `ListPageKit`) ; présentateur : `Art` et descriptions (clés `shop.desc.booster`, `shop.desc.gems`, `shop.desc.exchange_*`, `shop.desc.gift`).
- [ ] **Step 4: Run** `ShopCardsTests;MetaPagesPresenterTests;N29.HeroesDawn.Tests.Presentation` — Expected: PASS.
- [ ] **Step 5:** régénérer `Pages/Boutique.prefab` ; importer les PNG (Unity génère les `.meta`) ; vérifier que le type d'import est Sprite (copier les réglages d'une illustration d'`Illustrations/Items`).
- [ ] **Step 6: Commit** — `feat(boutique): offres en cartes avec icônes et descriptions`.

### Task 10: Documentation, captures et suites complètes

**Files:**
- Modify: `Code/docs/specs/2026-10-27-ui-profil-boutique-salons-design.md` (écarts actés : Registre = Agents + bouton Archives ; carte QG « Équipe et soins »), `01_Game_Design/` (fiches UX : `03_Art_UI_UX/01_UX_Ecrans.md` : écrans Profil, boutique en cartes, fiche d'agent avec soin), `05_Production/Decisions.md` (3 lignes datées 2026-10-27), `Tests/PlayMode/ScreenCaptureTests.cs` (captures Profil, boutique, fiche d'agent)

- [ ] **Step 1: Update docs** (spec, UX, décisions).
- [ ] **Step 2: Add PlayMode captures** : `CaptureTheProfile`, `CaptureTheShop`, `CaptureTheMemberSheet` (suivre `CaptureTheAffinityPopup` dans `CollectionPlayTests`), sorties dans `Code/docs/test-reports/captures/`.
- [ ] **Step 3: Run** `ut.sh EditMode` puis `ut.sh PlayMode` complets — Expected: 0 échec (si `Potion_PlayedWithTheRealScreen_Succeeds` expire, le relancer seul) ; regarder les captures (grille de la boutique lisible, fiche d'agent avec Soigner, Profil).
- [ ] **Step 4: Commit** — `docs: UI du lot F` (avec les rapports `Code/docs/test-reports/latest-*.html`, sans les fichiers de l'utilisateur).
