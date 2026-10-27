# Lot I : compte Supabase et sauvegarde cloud, plan d'implantation

> **Pour l'exécutant :** utiliser superpowers:executing-plans (exécution native). Les étapes se cochent avec `- [ ]`.

**Objectif :** un joueur peut créer un compte (e-mail, Google, Apple), le lier à sa progression d'invité et retrouver sa progression sur un autre appareil ; sauvegarde cloud du profil sur le projet Supabase N29 HQ.

**Architecture :** deux ports dans `Application/Ports` (`IAccountGateway` étendu, `ICloudProfileGateway` nouveau) avec des simulateurs en mémoire et des adaptateurs Supabase dans `Infrastructure/Supabase` (couche HTTP injectable). Un service d'application (`AccountLink`) orchestre connexion, rattachement et conflits ; un service de synchronisation (`CloudSync`) envoie le profil en différé. Les écrans (fenêtre Compte, choix de progression, bandeau du profil, bouton du titre) sont des prefabs générés.

**Technologies :** Unity 6000.6.3f1, C# nullable, NUnit, VContainer, UniTask, `JsonUtility`, `UnityWebRequest`, uGUI/TMP. Dossiers relatifs à `Code/Heroes Dawn Unity/`.

**Spec :** `Code/docs/specs/2026-10-28-compte-supabase-design.md`.

## Contraintes globales

- Textes visibles, tests et docs en français ; commentaires de code en français ; commits terminés par `Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>` ; ni push ni merge ; ne jamais ajouter `03_Art_UI_UX/Prompts_a_coller_C2.md` à un commit.
- Fins de ligne : beaucoup de fichiers sont en CRLF. Modifier par l'outil Edit ou par un script Node (fichier dans le scratchpad, jamais `node -e` avec apostrophes) qui normalise les fins de ligne. Les commandes bash longues avec apostrophes échouent : écrire les fichiers avec l'outil Write.
- Tests Unity en lot, éditeur fermé : `bash <espace>/ut.sh EditMode|PlayMode "<filtre>"` (plusieurs filtres séparés par `;`). Prefabs régénérés par `Unity.exe -batchmode -nographics -quit -projectPath … -executeMethod N29.HeroesDawn.EditorTools.Prefabs.PrefabGenerator.GenerateAllBatch -overwrite Popups/X.prefab`.
- **Aucun écran construit par code** : les runtime n'appellent pas `Ui.Txt/Btn/Img/Panel/Rect` (test `NoCodeBuiltScreensTests`). Les écrans viennent de recettes de prefab (`IPrefabRecipe`), modèle : `FriendAddRecipes.cs` (champ de saisie) et `ChapterPopupRecipe.cs`. Cibles tactiles ≥ 96 px.
- **Secrets** : la clé `service_role` n'entre jamais dans le dépôt ni le client ; seule la clé `anon` (publique) est dans `Resources/Supabase/supabase.json`. Jetons jamais dans les journaux ni dans le profil.
- **Aucune écriture sur Supabase sans accord** : l'application de la migration SQL (tâche 4) et le test réel (tâche 9) se font après que l'utilisateur a vu le SQL / le plan du test et dit oui (outil de question). Les tables `hq_vault` et `hq_vault_history` ne sont jamais modifiées ; toutes les tables du jeu ont le préfixe `hd_`.
- Le client ne contient aucun appel réseau dans les tests : tout passe par `ISupabaseHttp` (faux en test).
- Les erreurs sont des `DomainError(code, "errors.account_*")` traduites dans `LocAccount`.

## Revue ciblée (Review Focus)

1. Aucune perte de progression : avant d'écraser le profil local ou le profil cloud, l'autre version est copiée dans une sauvegarde locale de secours ; une écriture conditionnelle refusée ne perd rien.
2. Le jeu reste jouable hors ligne et pendant un échec réseau : une synchronisation qui échoue ne bloque ni la sauvegarde locale ni l'écran, et reprend plus tard.
3. Les jetons ne fuient pas : ni dans les journaux, ni dans le profil envoyé au cloud, ni après la déconnexion.
4. Sécurité par ligne : un joueur ne lit ni n'écrit la ligne d'un autre ; la limite de taille est appliquée ; aucune politique `delete`.
5. Entrées hostiles : retour OAuth avec un `state` ou un code invalide, réponses du serveur mal formées ou vides, e-mail avec espaces ou majuscules, mot de passe trop court, double appui sur Connexion.
6. Un profil cloud d'une version de schéma plus récente n'est jamais appliqué ; un profil cloud plus ancien est migré comme un profil local.

---

### Task 1: ports, simulateurs et textes d'erreur

**Fichiers :**
- Modifier : `Assets/_Project/Application/Ports/Ports.cs` (`AuthProvider`, `PlayerAccount`, `IAccountGateway`)
- Créer : `Assets/_Project/Application/Ports/ICloudProfileGateway.cs`
- Modifier : `Assets/_Project/Infrastructure/Simulated/LocalAccountGateway.cs`
- Créer : `Assets/_Project/Infrastructure/Simulated/FakeCloudProfileGateway.cs`
- Créer : `Assets/_Project/Presentation/Common/LocAccount.cs` (+ hook `ExtendAccount` dans `Loc.cs`)
- Tester : `Assets/Tests/Domain/Application/AccountPortsTests.cs`

**Interfaces :**
- Produit : `public enum AuthProvider { Email, Google, Apple }` ; `PlayerAccount(string playerId, string displayName, string email = "", AuthProvider provider = AuthProvider.Email)` avec `Email` et `Provider` (constructeur existant à deux arguments conservé par les valeurs par défaut) ; `IAccountGateway` gagne `UniTask<Result<PlayerAccount>> SignInWithProviderAsync(AuthProvider provider, CancellationToken ct)`, `UniTask<Result<PlayerAccount?>> RestoreSessionAsync(CancellationToken ct)`, `UniTask<Result> SignOutAsync(CancellationToken ct)`, `UniTask<Result> ResetPasswordAsync(string email, CancellationToken ct)`.
- Produit : `public sealed class CloudProfile { public ProfileData Profile; public string UpdatedAt; }` ; `public enum CloudWriteStatus { Ok, Conflict }` ; `public sealed class CloudWriteResult { CloudWriteStatus Status; string UpdatedAt; }` ; `public interface ICloudProfileGateway { UniTask<Result<CloudProfile?>> GetAsync(CancellationToken ct); UniTask<Result<CloudWriteResult>> UpsertAsync(ProfileData profile, string? expectedUpdatedAt, CancellationToken ct); }` (`expectedUpdatedAt` nul = première écriture ; `UpdatedAt` est la valeur renvoyée par le serveur, conservée telle quelle en texte).

- [ ] **Étape 1 : tests rouges `AccountPortsTests`.**
  - `LocalAccountGateway_SignInWithProvider_CreatesAndReusesAnAccount` : Google deux fois rend le même identifiant ; Apple un autre ; `Provider` correct.
  - `LocalAccountGateway_RestoreSession_ReturnsTheSignedInAccount_ThenNullAfterSignOut`.
  - `LocalAccountGateway_ResetPassword_UnknownEmail_StillSucceeds` (ne révèle pas l'existence d'un compte) ; adresse invalide : erreur `errors.account_email`.
  - `FakeCloudProfileGateway_FirstWrite_ThenConditionalWrites` : écriture sans attente puis avec la bonne valeur → `Ok` et nouveau `UpdatedAt` ; avec une mauvaise valeur → `Conflict` sans écraser ; `GetAsync` rend le dernier profil (copie, pas la référence).
  - `FakeCloudProfileGateway_SimulatedFailure_ReturnsErrorAndKeepsData`.
  - `AccountErrors_AllHaveAFrenchText` : chaque clé de la liste ci-dessous existe dans `Loc`.
- [ ] **Étape 2 : rouge** (`ut.sh EditMode "AccountPortsTests"`).
- [ ] **Étape 3 : implémenter** les types, étendre `LocalAccountGateway` (session mémoire, fournisseurs simulés avec latence réglable), écrire `FakeCloudProfileGateway` (profil copié par `JsonUtility`, `UpdatedAt` = compteur croissant sous forme de texte, option `FailNext`), et les textes d'erreur dans `LocAccount.cs` : `errors.account_email`, `errors.account_password`, `errors.account_exists`, `errors.account_credentials` (déjà présents : ne pas les redéfinir, vérifier avec `grep`), puis `errors.account_network` « Impossible de joindre le serveur. Vérifie ta connexion. », `errors.account_unconfirmed` « Confirme ton adresse e-mail avant de te connecter. », `errors.account_weak_password` « Mot de passe trop faible. », `errors.account_rate_limit` « Trop d'essais : réessaie dans un instant. », `errors.account_oauth` « La connexion a échoué ou a été annulée. », `errors.account_session` « Ta session a expiré : reconnecte-toi. », `errors.account_cloud` « La sauvegarde en ligne est indisponible pour le moment. », `errors.account_schema` « Ta sauvegarde en ligne vient d'une version plus récente du jeu : mets le jeu à jour. ».
- [ ] **Étape 4 : verts + `ut.sh EditMode ""` complet.**
- [ ] **Étape 5 : commit** `feat(compte): ports de compte et de sauvegarde cloud, simulateurs, textes d'erreur`.

---

### Task 2: couche HTTP, configuration et client Supabase Auth (e-mail et session)

**Fichiers :**
- Créer dans `Assets/_Project/Infrastructure/Supabase/` : `SupabaseConfig.cs`, `ISupabaseHttp.cs`, `UnityWebRequestHttp.cs`, `ISupabaseTokenStore.cs` (+ `PlayerPrefsTokenStore`, `MemoryTokenStore`), `SupabaseAccountGateway.cs`, `SupabaseErrors.cs`, `AssemblyInfo` si besoin.
- Tester : `Assets/Tests/Domain/Application/SupabaseAccountGatewayTests.cs` avec `FakeSupabaseHttp` (dans `Assets/Tests/Domain/Application/`).

**Interfaces :**
- Produit : `public sealed class SupabaseConfig { string Url; string AnonKey; string RedirectUrl; static SupabaseConfig? Load(); static SupabaseConfig Parse(string json); bool IsValid }` (`Load` lit `Resources/Supabase/supabase.json`, nul s'il est absent ou invalide).
- Produit : `public sealed class HttpResult { int Status; string Body; bool NetworkError; }` ; `public interface ISupabaseHttp { UniTask<HttpResult> SendAsync(string method, string url, IReadOnlyDictionary<string,string> headers, string? body, CancellationToken ct); }`.
- Produit : `public interface ISupabaseTokenStore { SupabaseSession? Load(); void Save(SupabaseSession s); void Clear(); }` ; `SupabaseSession { string AccessToken; string RefreshToken; long ExpiresAtUnix; string UserId; string Email; string DisplayName; AuthProvider Provider; }`.
- Produit : `SupabaseAccountGateway(SupabaseConfig config, ISupabaseHttp http, ISupabaseTokenStore tokens, IClock clock)` implémentant `IAccountGateway` ; méthode `internal`/publique `UniTask<Result<string>> AccessTokenAsync(CancellationToken ct)` qui rafraîchit le jeton s'il expire dans moins de 60 s ; utilisée par le client cloud.
- Les en-têtes : `apikey: <anon>` toujours, `Authorization: Bearer <jeton>` pour les appels connectés, `Content-Type: application/json`.

- [ ] **Étape 1 : tests rouges** (faux HTTP qui enregistre les requêtes et rend des réponses scriptées) :
  - `SignUp_PostsToSignup_WithMetadataName_AndStoresTheSession` (corps contient e-mail normalisé en minuscules sans espaces, `data.display_name`)  ; réponse sans session (confirmation d'e-mail requise) → l'inscription réussit avec `PlayerAccount.NeedsConfirmation` vrai et aucune session enregistrée.
  - `SignIn_PostsPasswordGrant_Succeeds_AndSavesTokens` ; mauvais mot de passe (400 `invalid_credentials`) → `errors.account_credentials` ; e-mail non confirmé (`email_not_confirmed`) → `errors.account_unconfirmed` ; 429 → `errors.account_rate_limit` ; erreur réseau → `errors.account_network` ; réponse non JSON → `errors.account_network` sans exception.
  - `Validation_RejectsBadEmailOrShortPassword_WithoutAnyRequest` (aucune requête envoyée).
  - `RestoreSession_ValidToken_ReturnsAccount_WithoutNetwork` ; `RestoreSession_ExpiredToken_RefreshesAndRotatesTheRefreshToken` ; `RestoreSession_RefreshRejected_ClearsTheStoreAndReturnsNull`.
  - `AccessToken_RefreshesWhenLessThanAMinuteLeft`.
  - `SignOut_PostsLogout_AndClearsTheStore_EvenIfTheRequestFails`.
  - `ResetPassword_PostsRecover_AndNeverRevealsWhetherTheAccountExists`.
  - `Tokens_NeverAppearInLogs` : un `ILogger` espion ne reçoit aucune ligne contenant le jeton d'accès ni de rafraîchissement.
  - `ConcurrentSignIn_SecondCallWhileFirstRuns_IsRejectedOrCoalesced` (un seul envoi réseau).
  - `Config_Parse_ValidAndInvalid` (URL sans `https`, clé vide → `IsValid` faux).
- [ ] **Étape 2 : rouge. Étape 3 : implémenter.** `SignUpAsync` renvoie `Result<PlayerAccount>` ; Ajouter `public bool NeedsConfirmation { get; }` à `PlayerAccount` (faux par défaut, argument optionnel du constructeur). JSON : DTO `[Serializable]` lus par `JsonUtility`. Mapper l'erreur Supabase (`error_code`, `msg`) vers les clés de la tâche 1 dans `SupabaseErrors.Map(HttpResult)`.
- [ ] **Étape 4 : verts. Étape 5 : commit** `feat(compte): client Supabase Auth (e-mail, session, jetons)`.

---

### Task 3: Google et Apple (OAuth avec PKCE)

**Fichiers :** `Infrastructure/Supabase/{Pkce.cs, IBrowserOpener.cs, IAuthRedirectInbox.cs, OAuthFlow.cs}` ; modifier `SupabaseAccountGateway.cs` ; tests `SupabaseOAuthTests.cs`.

**Interfaces :**
- Produit : `static class Pkce { static (string Verifier, string Challenge) Create(IRandom?/byte[] entropy); }` (verifier 43 à 128 caractères base64url ; challenge = base64url(SHA256(verifier)), méthode `S256`).
- Produit : `public interface IBrowserOpener { void Open(string url); }` ; `public interface IAuthRedirectInbox { UniTask<string?> WaitForRedirectAsync(string expectedPrefix, CancellationToken ct); }` (`UnityRedirectInbox` : écoute `Application.deepLinkActivated` et le lien de lancement ; rend l'URL reçue ou nul à l'annulation).
- `SupabaseAccountGateway.SignInWithProviderAsync` : construit `{url}/auth/v1/authorize?provider=google|apple&redirect_to=<RedirectUrl>&code_challenge=<c>&code_challenge_method=S256`, ouvre le navigateur, attend le retour (délai maximal 5 minutes), lit `code` (et rejette si `error` ou si `state` fourni ne correspond pas), échange par `POST /auth/v1/token?grant_type=pkce` `{auth_code, code_verifier}`, enregistre la session.

- [ ] **Étape 1 : tests rouges :** `Pkce_Challenge_IsTheS256OfTheVerifier` (vecteur connu de la RFC 7636 : verifier `dBjftJeZ4CVP-mB92K27uhbUJU1p1r_wW1gFWFOEjXk` → challenge `E9Melhoa2OwvFrEMTJguCHaoeK1t8URWbuGJSstw-cM`) ; `Authorize_Url_HasProviderRedirectAndChallenge` (Google et Apple) ; `Redirect_WithCode_ExchangesAndStoresTheSession` ; `Redirect_WithError_ReturnsErrorsAccountOauth` ; `Redirect_WithWrongState_IsRejected` ; `Redirect_Cancelled_ReturnsErrorsAccountOauth_AndStoresNothing` ; `Redirect_Timeout_ReturnsErrorsAccountOauth` ; `Exchange_Failure_ReturnsErrorsAccountOauth` ; `SecondProviderSignIn_WhileOneIsPending_IsRejected`.
- [ ] **Étape 2 : rouge. Étape 3 : implémenter** (`OAuthFlow` isolé, testable avec `IBrowserOpener` et `IAuthRedirectInbox` factices). Enregistrer le schéma de lien `heroesdawn://auth` dans les réglages du projet (`ProjectSettings` : Android intent filter et iOS URL scheme) **uniquement si** le réglage existe déjà dans le dépôt de façon éditable ; sinon le noter dans le guide de recette.
- [ ] **Étape 4 : verts. Étape 5 : commit** `feat(compte): connexion Google et Apple par OAuth PKCE`.

---

### Task 4: sauvegarde cloud (client PostgREST) et migration SQL

**Fichiers :** `Infrastructure/Supabase/SupabaseCloudProfileGateway.cs` ; `Assets/_Project/Backend/Supabase/001_hd_profiles.sql` ; tests `SupabaseCloudProfileGatewayTests.cs`, `HdProfilesMigrationTests.cs` (EditMode, lecture du fichier SQL).

**Interfaces :** `SupabaseCloudProfileGateway(SupabaseConfig config, ISupabaseHttp http, SupabaseAccountGateway accounts)` implémentant `ICloudProfileGateway`.
- `GetAsync` : `GET /rest/v1/hd_profiles?select=profile,schema_version,updated_at&limit=1` avec le jeton du joueur ; la réponse est un tableau JSON : l'envelopper (`{"rows":` + corps + `}`) pour `JsonUtility`.
- `UpsertAsync(profile, null, …)` : `POST /rest/v1/hd_profiles` (`Prefer: return=representation`) avec `{"user_id": "<uid>", "profile": <json>, "schema_version": n}` ; `409` (ligne déjà là) → `Conflict`.
- `UpsertAsync(profile, expected, …)` : `PATCH /rest/v1/hd_profiles?user_id=eq.<uid>&updated_at=eq.<expected>` (`Prefer: return=representation`) ; tableau vide → `Conflict` ; sinon `Ok` avec le nouvel `updated_at`.
- Le jeton vient de `accounts.AccessTokenAsync` ; sans session → `errors.account_session`. Réseau / 5xx → `errors.account_cloud`. Profil > 512 Ko avant envoi → `errors.account_cloud` sans requête.

**SQL (`001_hd_profiles.sql`) :**
```sql
create table if not exists public.hd_profiles (
  user_id uuid primary key references auth.users (id) on delete cascade,
  profile jsonb not null,
  schema_version int not null,
  updated_at timestamptz not null default now(),
  constraint hd_profiles_size check (pg_column_size(profile) <= 524288)
);
alter table public.hd_profiles enable row level security;
create policy hd_profiles_select on public.hd_profiles for select to authenticated using (auth.uid() = user_id);
create policy hd_profiles_insert on public.hd_profiles for insert to authenticated with check (auth.uid() = user_id);
create policy hd_profiles_update on public.hd_profiles for update to authenticated using (auth.uid() = user_id) with check (auth.uid() = user_id);
create or replace function public.hd_profiles_touch() returns trigger language plpgsql set search_path = '' as $$
begin new.updated_at := now(); return new; end $$;
create trigger hd_profiles_touch before insert or update on public.hd_profiles for each row execute function public.hd_profiles_touch();
```

- [ ] **Étape 1 : tests rouges :** requêtes construites (méthode, URL, en-têtes `apikey` + `Authorization`, corps contenant l'identifiant et le profil) pour GET / POST / PATCH ; `Upsert_Patch_UsesTheExpectedUpdatedAtFilter` ; `Upsert_EmptyArray_IsAConflict_AndNothingIsLost` ; `Upsert_409_OnFirstWrite_IsAConflict` ; `Get_EmptyRows_ReturnsNull` ; `Get_ParsesProfileAndUpdatedAt` ; `Get_MalformedBody_ReturnsErrorsAccountCloud` ; `NoSession_ReturnsErrorsAccountSession` ; `OversizedProfile_IsRejectedWithoutARequest` ; `Network5xx_ReturnsErrorsAccountCloud` ; `Migration_File_HasRlsPoliciesSizeLimitAndNoDeletePolicy` (lit le SQL : contient `enable row level security`, 3 politiques sur `auth.uid() = user_id`, `524288`, aucune occurrence de `for delete`, aucune référence à `hq_vault`).
- [ ] **Étape 2 : rouge. Étape 3 : implémenter.**
- [ ] **Étape 4 : verts.**
- [ ] **Étape 5 : appliquer la migration sur N29 HQ — après accord.** Montrer le SQL à l'utilisateur et demander son accord (outil de question) ; si oui, `apply_migration` (nom `hd_profiles`) sur le projet `ulwbghlrglwccozbdlvr`, puis `list_tables` (RLS actif, 0 ligne), `get_advisors` type sécurité (aucune alerte sur `hd_profiles`) et un essai d'isolation par `execute_sql` en simulant deux utilisateurs (`set local role authenticated; select set_config('request.jwt.claim.sub', '<uuid>', true)`) dans une transaction annulée. Si l'utilisateur refuse, noter « migration non appliquée » au journal et continuer.
- [ ] **Étape 6 : commit** `feat(compte): client de sauvegarde cloud et migration hd_profiles`.

---

### Task 5: service de compte, rattachement, conflits et synchronisation

**Fichiers :** `Application/Features/Account/{AccountLink.cs, CloudSync.cs, CloudSyncState.cs, ProfileSummary.cs, ProgressBackup.cs, AccountEvents.cs}` ; ports `ICloudSyncStateStore` et `IProgressBackupStore` (dans `Application/Ports`), implémentations `Infrastructure/Platform/{PlayerPrefsSyncStateStore.cs, FileProgressBackupStore.cs}` et mémoire pour les tests ; modifier `GameSession` si nécessaire pour appliquer un profil (`ApplyProfile` existe) ; tests `AccountLinkTests.cs`, `CloudSyncTests.cs`.

**Interfaces :**
- `ProfileSummary { int Level (niveau d'agent le plus haut), int Credits, int ZonesCleared, DateTime LastSeenUtc; bool IsFresh }` : `static ProfileSummary Of(ProfileData)` ; `IsFresh` vrai si aucune zone terminée et tutoriel non terminé (`meta.tutorial.done == false && completedZones.Count == 0`).
- `CloudSyncState { string UserId; string LastUpdatedAt; DateTime LastSyncUtc; }` ; `ICloudSyncStateStore { CloudSyncState? Load(); void Save(CloudSyncState s); void Clear(); }`.
- `IProgressBackupStore { void Save(string reason, ProfileData profile, DateTime utcNow); }` (un fichier par sauvegarde de secours, 5 conservés par raison).
- `public enum LinkStatus { Uploaded, Pulled, NeedsChoice, InSync }` ; `public sealed class LinkOutcome { LinkStatus Status; ProfileSummary? Local; ProfileSummary? Cloud; }`.
- `AccountLink` (singleton) : `AccountState State` (`SignedOut`, `Guest`, `Connected`), `PlayerAccount? Cloud` ; `UniTask<Result<LinkOutcome>> SignUpAsync(email, password, name, ct)` (si `NeedsConfirmation` : `Result.Ok` avec `LinkOutcome` nul → l'écran affiche « E-mail de confirmation envoyé »), `SignInAsync`, `SignInWithProviderAsync`, `ResetPasswordAsync`, `RestoreAsync(ct)` (au lancement), `ResolveAsync(ChoiceKind choice, ct)` (`KeepDevice` / `KeepAccount`), `SignOutAsync(ct)`, `SyncNowAsync(ct)`.
- Règles de rattachement (après authentification réussie) : lecture du cloud (`GetAsync`) ; **cloud absent** → envoi du profil local (`expected` nul) → `Uploaded` ; **cloud présent et local frais** → application du cloud → `Pulled` ; **cloud présent, local avec progression** : si `state.LastUpdatedAt == cloud.UpdatedAt` pour ce `UserId` (déjà synchronisés) → envoi du local → `Uploaded` ; sinon → `NeedsChoice` (rien n'est écrit). `KeepAccount` : sauvegarde de secours du local (`reason = "local"`), application du cloud, état mis à jour. `KeepDevice` : sauvegarde de secours du cloud (`reason = "cloud"`), envoi du local avec `expected = cloud.UpdatedAt`, état mis à jour. Un cloud dont `schemaVersion` dépasse celui du jeu → `errors.account_schema`, rien n'est appliqué ni écrit.
- `CloudSync` : s'abonne à la sauvegarde locale (le `SaveScheduler` appelle `CloudSync.NotifyLocalSaved()` ; nouveau crochet optionnel, sans dépendance circulaire) ; envoi différé : au plus un envoi toutes les 20 s (délai injectable), un seul envoi à la fois, les modifications survenues pendant un envoi partent au suivant ; `FlushAsync(ct)` pour la fermeture et l'arrière-plan ; sur `Conflict` : relecture du cloud puis `NeedsChoice` publié (événement `ProgressConflictDetected`) ; sur échec : réessais à 5 s, 20 s, 60 s puis à la prochaine sauvegarde ; sans compte connecté : ne fait rien.
- `SignOutAsync` : `IAccountGateway.SignOutAsync`, état de synchronisation effacé, `GameSession` conservé (invité).

- [ ] **Étape 1 : tests rouges** (faux cloud, faux compte, magasins en mémoire, délai immédiat) :
  - rattachement : `Link_CloudEmpty_UploadsLocal`, `Link_CloudPresent_LocalFresh_PullsCloud`, `Link_CloudPresent_LocalProgress_FirstTime_NeedsChoice_AndWritesNothing`, `Link_AlreadyInSync_UploadsLocal`, `Link_CloudSchemaNewer_IsRefused_AndNothingChanges`, `Link_CloudOlderSchema_IsAppliedLikeALocalProfile`.
  - choix : `KeepAccount_BacksUpLocal_AppliesCloud_UpdatesState`, `KeepDevice_BacksUpCloud_UploadsWithExpected_UpdatesState`, `KeepDevice_ConflictAgain_ReturnsNeedsChoiceAgain_NoLoss`.
  - synchronisation : `LocalSaves_AreBatched_AtMostOneUploadPer20s`, `OnlyOneUploadAtATime_ChangesDuringUploadGoInTheNext`, `Flush_SendsImmediately`, `Conflict_PublishesAChoice_AndStopsUploading`, `Failure_RetriesAt5_20_60Seconds_ThenWaitsForTheNextSave`, `NoAccount_NeverCallsTheCloud`, `OfflineFailure_NeverBlocksLocalSaves`.
  - session : `Restore_ValidSession_LinksAndSyncs`, `Restore_NoSession_StaysGuest`, `SignOut_ClearsStateAndKeepsTheLocalProfile`, `DoubleSignIn_IsCoalesced`.
  - `ProfileSummary_IsFresh_Rules`.
- [ ] **Étape 2 : rouge. Étape 3 : implémenter** (aucune référence à Unity côté `Application/Features/Account`, hors `JsonUtility` pour copier un profil si déjà utilisé dans le projet).
- [ ] **Étape 4 : verts. Étape 5 : commit** `feat(compte): service de compte, rattachement, conflits et synchronisation`.

---

### Task 6: écrans (fenêtre Compte, choix de progression, bouton du titre, bandeau du profil)

**Fichiers :**
- Créer `Presentation/Account/{IAccountPopupView.cs, AccountPopupView.cs, AccountPopupPresenter.cs, IProgressChoicePopupView.cs, ProgressChoicePopupView.cs, AccountFlow.cs}`
- Créer `Editor/Prefabs/Recipes/AccountRecipes.cs` (`Popups/AccountPopup.prefab`, `Popups/ProgressChoicePopup.prefab`)
- Modifier `Presentation/Login/{ILoginView.cs, LoginView.cs, LoginPresenter.cs}` (bouton « Se connecter »), la recette du titre, `Presentation/Hub/Pages/Profile/ProfilePage.cs` (rangée « Compte »), `Presentation/Common/LocAccount.cs`
- Tester : `Tests/Presentation/AccountPopupPresenterTests.cs`, `AccountFlowTests.cs`, `ProfileAccountRowTests.cs`, `Tests/PlayMode/AccountScreensPlayTests.cs`, fakes (`FakeAccountPopup`, `FakeProgressChoicePopup`).

**Interfaces :**
- `IAccountPopupView : IPopupView` avec événements `ModeChanged(AccountMode)` (`SignIn`, `SignUp`), `SubmitClicked(AccountForm)`, `ProviderClicked(AuthProvider)`, `ForgotClicked(string email)`, `CloseClicked` ; méthodes `Show(AccountPopupData)`, `SetBusy(bool, string)`, `ShowError(string)`, `ShowInfo(string)`. `AccountForm { string Email, Password, DisplayName }`. `AccountPopupData` : textes localisés et mode.
- `AccountPopupPresenter` : valide avant d'appeler `AccountLink` (e-mail avec « @ », mot de passe ≥ 8, pseudo 3 à 16 caractères à la création) avec les messages `account.error.*` ; garde `BusyGate` contre le double appui ; ferme la fenêtre en cas de succès (`NeedsChoice` ouvre le choix) ; l'annulation en cours de requête n'écrit rien.
- `IProgressChoicePopupView : IPopupView` : `UniTask<ChoiceKind?> AskAsync(ProgressChoiceData data, CancellationToken ct)` (deux cartes : cet appareil / le compte, avec niveau, crédits, zones, date ; Retour = nul, rien n'est écrit).
- `AccountFlow` (présentation) : `UniTask RunAsync(CancellationToken)` ouvre la fenêtre Compte, enchaîne sur le choix si besoin et affiche les toasts de résultat (« Compte lié », « Progression récupérée », « Progression envoyée »). Utilisé par le titre et le profil.
- `ILoginView` gagne `event Action? AccountClicked` et `void SetAccountLabel(string)` (« Se connecter » ou l'e-mail du compte restauré) ; `LoginPresenter` accepte un `IAccountFlow?` optionnel.
- Profil : une rangée en tête de liste. Invité : titre « Compte invité », sous-titre « Lie ton compte pour sauvegarder ta progression en ligne », bouton « Lier mon compte ». Connecté : e-mail et fournisseur, sous-titre « Dernière synchronisation : <date> », boutons « Synchroniser » et « Se déconnecter » (confirmation par le popup de confirmation existant).
- Textes dans `LocAccount.cs` : onglets, libellés des champs, boutons (« Continuer avec Google », « Continuer avec Apple », « Mot de passe oublié », « Se connecter », « Créer un compte », « Annuler »), messages (`account.confirm_sent` « E-mail de confirmation envoyé : clique sur le lien puis reviens te connecter. », `account.reset_sent` « Si un compte existe pour cette adresse, un e-mail vient d'être envoyé. »), erreurs de validation, choix de progression (« Garder cet appareil », « Garder le compte », « Niveau {0} · {1} Crédits · {2} zones · {3} »), toasts, profil.

- [ ] **Étape 1 : tests rouges :** présentateur (validation sans appel, un seul envoi sur double appui, succès ferme, échec affiche l'erreur et rend les boutons, onglets, mot de passe oublié, fournisseurs), `AccountFlow` (Uploaded / Pulled / NeedsChoice → choix → Garder le compte / Garder cet appareil / Retour = rien n'est écrit), titre (`AccountClicked` ouvre le flux ; libellé), profil (rangée invité / connecté, boutons, déconnexion confirmée ou annulée), `AccountScreens_HaveNoCodeBuiltUi` couvert par le test existant.
- [ ] **Étape 2 : rouge. Étape 3 : implémenter** vues, présentateurs, flux, recettes (`PopupKit.Window`, champs sur le modèle de `FriendAddRecipes`, champ mot de passe avec `contentType = Password`), bouton du titre dans la recette de l'écran titre, textes.
- [ ] **Étape 4 : régénérer** `Popups/AccountPopup.prefab`, `Popups/ProgressChoicePopup.prefab` et l'écran titre (`-overwrite` des chemins concernés, voir le prefab du titre dans `Prefabs/Screens/`) ; vérifier `ScreenCatalogTests` et `HubLayoutTests`.
- [ ] **Étape 5 : PlayMode `AccountScreensPlayTests`** (simulateur) : la fenêtre Compte s'ouvre depuis le titre, validation et erreurs visibles, inscription puis choix, Retour ferme sans rien écrire ; le profil montre le bandeau selon l'état. Commande `ut.sh PlayMode "AccountScreensPlayTests"`.
- [ ] **Étape 6 : commit** `feat(compte): fenêtre Compte, choix de progression, bouton du titre et bandeau du profil`.

---

### Task 7: câblage, configuration et restauration au lancement

**Fichiers :** `Composition/RootLifetimeScope.cs` ; `Assets/_Project/Resources/Supabase/supabase.json` ; `Application/Session/SaveScheduler.cs` (crochet) ; point d'entrée de l'application (arrière-plan / fermeture : `Presentation` ou `Composition`, composant `AccountLifecycle : MonoBehaviour`) ; `Assets/_Project/Backend/README.md` ; tests `AccountWiringTests.cs`.

- [ ] **Étape 1 : tests rouges :** `Wiring_WithoutConfig_UsesTheSimulators` ; `Wiring_WithConfig_UsesSupabaseGateways` (la sélection est une fonction pure testable : `AccountBackend.Choose(SupabaseConfig?) → Simulated | Supabase`) ; `SaveScheduler_NotifiesTheCloudSync_AfterALocalSave` ; `Lifecycle_OnPause_FlushesTheSync` ; `Restore_RunsAfterTheGuestSignIn_NotBefore` ; `Config_File_IsValid_AndHasNoServiceRoleKey` (le JSON n'a que `url`, `anonKey`, `redirectUrl` ; l'`anonKey` décodée a le rôle `anon`).
- [ ] **Étape 2 : rouge. Étape 3 : implémenter.** Lire l'URL et la clé `anon` publiques du projet par l'outil Supabase (`get_project_url`, `get_publishable_keys`) et les écrire dans `supabase.json` (URL `https://ulwbghlrglwccozbdlvr.supabase.co`, redirection `heroesdawn://auth`). Enregistrer dans `RootLifetimeScope` : `SupabaseConfig` facultatif, passerelles Supabase ou simulateurs, `AccountLink`, `CloudSync`, magasins. `LoginPresenter` / `SignInAsGuestUseCase` : après la connexion invité, `AccountLink.RestoreAsync` (sans bloquer l'écran ; un échec réseau est silencieux). `AccountLifecycle` appelle `FlushAsync` sur `OnApplicationPause(true)` et `OnApplicationQuit`.
- [ ] **Étape 4 : suite EditMode complète verte. Étape 5 : commit** `feat(compte): câblage Supabase, configuration et restauration de session`.

---

### Task 8: documents, captures et suites complètes

**Fichiers :** `Assets/_Project/Backend/README.md` (section « Comptes joueurs » réécrite : fonctionnement, fichier de configuration, migration, **liste exacte des réglages Supabase à faire** : activer Google (Client ID et secret) et Apple (Services ID, clé privée, équipe) dans Authentication ▸ Providers, ajouter `heroesdawn://auth` aux Redirect URLs, décider de la confirmation d'e-mail, URL du site), `Code/docs/recette-compte.md` (guide de recette pour l'utilisateur : inscription, confirmation, Google, Apple, deux appareils, conflit, déconnexion, hors ligne), `03_Art_UI_UX/01_UX_Ecrans.md` (« Lot I »), `05_Production/Decisions.md` (3 lignes), spec (écarts actés), `Tests/PlayMode/ScreenCaptureTests.cs` (`CaptureAccount`, explicite).

- [ ] **Étape 1 :** `CaptureAccount` : fenêtre Compte (connexion, création, erreur, chargement), choix de progression, bandeau du profil (invité, connecté) ; lire 3 captures au hasard (Read) et corriger la mise en page si besoin.
- [ ] **Étape 2 :** écrire les documents.
- [ ] **Étape 3 :** `ut.sh EditMode ""` puis `ut.sh PlayMode ""` ; noter les totaux au journal.
- [ ] **Étape 4 : commit** `docs: compte Supabase du lot I`.

---

### Task 9: vérification réelle sur N29 HQ (avec accord)

- [ ] **Étape 1 :** exposer à l'utilisateur ce que fera le test réel (créer un utilisateur jetable `hd-test-<horodatage>@example.invalid` dans le projet, écrire une ligne `hd_profiles`, lire, tenter de lire la ligne d'un autre utilisateur, tout supprimer ensuite) et demander son accord. Refus ou impossibilité (confirmation d'e-mail obligatoire, limites de débit) : noter « test réel non fait » au journal.
- [ ] **Étape 2 :** si accord : créer l'utilisateur par l'API d'administration (clé `service_role` lue **uniquement** dans l'outil, jamais écrite dans un fichier ni un journal), obtenir un jeton, vérifier insertion, lecture, mise à jour conditionnelle, conflit sur mauvais `updated_at`, refus de lecture croisée, contrainte de taille ; supprimer l'utilisateur de test (la ligne part en cascade) ; contrôler `hd_profiles` vide.
- [ ] **Étape 3 :** consigner le résultat au journal.

---

### Relecture finale (hors tâches numérotées)

Après la tâche 9 : revue indépendante par un agent opus en lecture seule (spec, plan, journal, `git log` du lot, Review Focus), triage, correction des constats Critiques et Importants avec test rouge puis vert, mineurs différés au journal et à la mémoire, suppression de l'espace de travail, mise à jour de la note mémoire `version-0-14-0-lots.md`.
