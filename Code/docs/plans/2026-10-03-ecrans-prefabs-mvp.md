# Sous-projet 3 — Écrans MVP en prefabs générés · Plan d'implémentation

> **Pour les agents :** SOUS-COMPÉTENCE REQUISE : `superpowers:subagent-driven-development` (recommandé) ou `superpowers:executing-plans` pour exécuter ce plan tâche par tâche. Les étapes utilisent des cases à cocher (`- [ ]`).

**Objectif :** les écrans Connexion, QG, Plateau et Combat (popups et modales compris) deviennent des prefabs générés une fois par un outil éditeur puis éditables dans Unity. Chaque écran a une vue `MonoBehaviour` derrière une interface `I…View` et un présentateur C# pur. Tout le texte passe en TextMeshPro, et la navigation « Retour » (Échap / retour Android) est ajoutée.

**Architecture :**
- Les recettes éditeur (`Editor/Prefabs/Recipes`) reprennent la construction actuelle des écrans avec les helpers `Ui.*` (passés en TMP) et enregistrent des prefabs imbriqués.
- Un `ScreenCatalog` (ScriptableObject) relie scopes et prefabs. Chaque scope de scène instancie sa vue (`RegisterComponentInNewPrefab`) et enregistre son présentateur (`IStartable`/`ITickable`/`IDisposable`).
- `GameFlowMachine`, scènes, use cases et bus sont inchangés.

**Stack :** Unity 6000.6.3f1, uGUI 2.6 (TextMeshPro inclus), Input System 1.20, VContainer 1.19.0, MessagePipe 1.8.2, UniTask 2.5.11, Unity Test Framework.

**Spec :** `Code/docs/specs/2026-10-03-ecrans-prefabs-mvp-design.md` (fait autorité).

## Contraintes globales
- **Comportement :** jouabilité, règles, montants et textes identiques à aujourd'hui, plus la navigation Retour (spec §7). Seule exception visuelle : les textes de 18–19 px passent à 20 px (règle des maquettes §3).
- **Vues :** une vue n'expose que `Show…/Set…(…Data)` et des événements d'intention. Elle ne référence ni `ISessionReadModel`, ni un use case, ni `UiContext`. Elle garde animations, tweens et raccourcis clavier, qu'elle relaie en intentions.
- **Présentateurs :** C# pur, dans `Presentation/<Écran>/`. Ils ne touchent jamais un `GameObject`, `Transform` ou composant. La couleur passe par l'énumération sémantique `Tone`, que la vue traduit via `Theme`.
- **Localisation :** aucun texte en dur ; les nouvelles clés vont dans `Fr` de `Presentation/Common/Loc.cs`. Les textes statiques des prefabs portent une clé via `LocalizedText`.
- **Navigation :** seule `GameFlowMachine` charge ou masque des scènes. Les présentateurs publient `NavigateRequested` / `CombatResultsClosed`.
- **Assemblies :** l'asmdef Editor ne référence **jamais** `N29.HeroesDawn.Application` (il masquerait `UnityEngine.Application`). `Core`/`Definitions`/`Domain.*` restent sans framework (garde-fou existant).
- **Descriptions de test :** chaque test porte `[Description("Vérifie que … — parce que …")]` (garde-fou `TestDescriptionsTests`).
- **Style :** `#nullable enable` dans tout nouveau fichier du jeu ; un type public par fichier (sauf les `…Data` d'un même écran, regroupés dans `<Écran>Data.cs`).
- **Rapports :** après chaque série de tests, une synthèse en chat : totaux, ce que la série prouve par fonctionnalité, échecs et causes, chemin du rapport `Code/docs/test-reports/latest-*.html`. Les `latest-*.html` sont commités avec la tâche.
- **Docs utilisateur :** ne jamais toucher ni commiter `00_GDD/*`, `03_Art_UI_UX/*` ni `Code/_deploy/`.

## Commandes (bash, Unity **fermé**)
```bash
UNITY="/c/Program Files/Unity/Hub/Editor/6000.6.3f1/Editor/Unity.exe"
PROJ="C:/Users/dnath/Documents/Heroes Dawn/Code/Heroes Dawn Unity"
"$UNITY" -batchmode -nographics -projectPath "$PROJ" -runTests -testPlatform EditMode -testResults "$TMP/e.xml" -logFile "$TMP/e.log"
"$UNITY" -batchmode -projectPath "$PROJ" -runTests -testPlatform PlayMode -testResults "$TMP/p.xml" -logFile "$TMP/p.log"   # sans -nographics
"$UNITY" -batchmode -nographics -projectPath "$PROJ" -executeMethod <Classe>.<Méthode> -quit -logFile "$TMP/m.log"
grep -o '<test-run [^>]*' "$TMP/e.xml" | grep -oE '(total|passed|failed)="[0-9]+"'
```
- Un test : `-testFilter "<Namespace>.<Classe>.<Test>"`.
- Générer : `N29.HeroesDawn.EditorTools.Prefabs.PrefabGenerator.GenerateAllBatch` (ajouter `-overwritePrefabs` pour écraser), puis `N29.HeroesDawn.EditorTools.Scenes.SceneGenerator.GenerateAll`.
- Point de départ : **EditMode 101/101, PlayMode 14/14**.

## Points d'attention (Review Focus)
1. **Modale pendant une modale, Retour pendant la file :** deux popups demandés coup sur coup s'affichent l'un après l'autre, jamais superposés ; Retour ne ferme que la modale du dessus, et seulement si elle est annulable. Tests à la tâche 2 (`ModalServiceTests`).
2. **Plateau masqué pendant le combat :** le présentateur du plateau garde ses abonnements (`ReturnedToBoard`, `ZoneValidated`), mais son gestionnaire Retour est retiré. Échap en combat ne doit jamais ouvrir « Retour au QG ? ». Tests à la tâche 6 (EditMode) et 8 (PlayMode).
3. **Régénération après retouche manuelle :** un prefab existant n'est pas écrasé sans demande ; une retouche d'un widget imbriqué se propage aux écrans. Tests à la tâche 3.
4. **Présentateur détruit pendant un appel asynchrone** (scène déchargée pendant un use case) : aucune exception, aucun appel à la vue après `Dispose`. Tests à la tâche 2 (`PresenterBase`) et 5 (QG).
5. **Raccourcis clavier derrière une modale** (Espace pendant la confirmation « Retour au QG ? ») : l'intention est ignorée. Tests aux tâches 6 et 7.

---

## Carte des fichiers
```
Assets/_Project/
├─ Resources/UI/            Rounded.png RoundedSmall.png Circle.png Ring.png VerticalGradient.png (tâche 1)
├─ Art/UI/                  HD_Logo.png HD_Title.png HD_BG_Landscape.jpg HD_BG_Portrait.jpg (tâche 1)
├─ Art/Fonts/               Bangers SDF.asset  ComicNeue-Bold SDF.asset  ComicNeue-Regular SDF.asset  + matériaux « Inked » (tâche 1)
├─ Settings/                ScreenCatalog.asset (tâche 3)
├─ Prefabs/
│  ├─ Widgets/              Portrait  Wallet  Toast  TeamCard  RecruitCard  SquadSlot  ObjectiveLine  BoardNode  BoardLink
│  │                        UnitCard  SkillButton  FloatingText  ChoiceButton  FatigueRow
│  ├─ Common/               ModalLayer  Toasts
│  ├─ Popups/               ConfirmPopup  RestPopup  NarrativePopup  MiniGamePopup  ZoneCompletePopup  CombatResultsPopup
│  └─ Screens/              Login  Hub  Board  Combat
├─ Presentation/
│  ├─ Mvp/                  IView.cs ViewBase.cs PresenterBase.cs Tone.cs ToneColors.cs BusyGate.cs IFrameClock.cs UnityFrameClock.cs
│  │                        ILocalization.cs LocLocalization.cs IToastService.cs IModalService.cs IPopupView.cs IPopupHost.cs ModalService.cs
│  │                        IBackHandler.cs IBackInput.cs InputSystemBackInput.cs BackButtonRouter.cs ScreenCatalog.cs LocalizedText.cs
│  ├─ Common/               Ui.cs (TMP) Sprites.cs (assets) Theme.cs Loc.cs Tween.cs PortraitView.cs WalletView.cs WalletBinder.cs
│  │                        ToastsView.cs ModalLayer.cs ConfirmPopupView.cs
│  ├─ Login/                ILoginView.cs LoginView.cs LoginPresenter.cs
│  ├─ Hub/                  IHubView.cs HubView.cs HubData.cs HubPresenter.cs TeamCardView.cs RecruitCardView.cs SquadSlotView.cs
│  ├─ Board/                IBoardView.cs BoardView.cs IBoardMapView.cs BoardMapView.cs BoardNodeView.cs BoardData.cs BoardPresenter.cs
│  │                        CasePopupPresenter.cs ObjectiveLineView.cs
│  ├─ Popups/               IRestPopupView.cs RestPopupView.cs INarrativePopupView.cs NarrativePopupView.cs IMiniGamePopupView.cs
│  │                        MiniGamePopupView.cs TimingMiniGame.cs IZoneCompletePopupView.cs ZoneCompletePopupView.cs PopupData.cs
│  └─ Combat/               ICombatView.cs CombatView.cs CombatData.cs CombatPresenter.cs UnitCardView.cs SkillButtonView.cs
│                           FloatingTextView.cs ICombatResultsPopupView.cs CombatResultsPopupView.cs
├─ Composition/             RootLifetimeScope.cs SceneScreenScope.cs Hub/Board/CombatLifetimeScope.cs (modifiés)
└─ Editor/
   ├─ Assets/               UiAssetBaker.cs FontAssetGenerator.cs
   └─ Prefabs/              IPrefabRecipe.cs PrefabContext.cs PrefabGenerator.cs PrefabGeneratorWindow.cs GenerationReport.cs
                            Recipes/<Nom>Recipe.cs (une par prefab)
Assets/Tests/
├─ Presentation/            N29.HeroesDawn.Tests.Presentation.asmdef (EditMode) + Fakes/ + <Écran>PresenterTests.cs
├─ Editor/                  PrefabGeneratorTests.cs PrefabStructureTests.cs ScreenCatalogTests.cs UiAssetsTests.cs
└─ PlayMode/                PlayModeHarness.cs (modifié) BootFlowTests.cs SceneFlowTests.cs BackNavigationTests.cs FullRunTests.cs
```
Supprimés en tâche 8 : `Presentation/UiContext.cs`, `UiServices.cs`, `Screens/UiScreen.cs`, `Screens/LoginScreen.cs`, `Hub/HubScreen.cs`, `Board/BoardScreen.cs`, `Combat/CombatScreen.cs`, `Combat/UnitCard.cs`, `Popups/CasePopups.cs`, `Common/Widgets.cs`, `Common/Modal.cs` (ancienne API `Open/Confirm`), `Common/Toasts.cs`.

---

### Tâche 1 : Assets d'interface et TextMeshPro

**Fichiers :**
- Créer : `Editor/Assets/UiAssetBaker.cs`, `Editor/Assets/FontAssetGenerator.cs`, `Presentation/Mvp/LocalizedText.cs`
- Modifier : `Presentation/Common/Sprites.cs`, `Presentation/Common/Ui.cs`, `Presentation/Common/Theme.cs` (si besoin des matériaux), tous les écrans (`Text` → `TextMeshProUGUI`, tailles 18–19 → 20), `Presentation/N29.HeroesDawn.Presentation.asmdef` (+ `Unity.TextMeshPro`)
- Test : `Tests/Editor/UiAssetsTests.cs`, `Tests/PlayMode/ScreenCaptureTests.cs`

**Interfaces :**
- Produit :
  - `UiAssetBaker.BakeAll()` (menu *Heroes Dawn ▸ Setup ▸ Générer les assets d'interface*, batchmode). Écrit les 5 sprites procéduraux en PNG sous `Resources/UI/` (9-slice : bordure 18 px pour `Rounded`, 7 px pour `RoundedSmall`, FullRect, sans mipmap, Clamp) et les 4 illustrations `.bytes` en `Art/UI/*` (Sprite, `npotScale = None`, `maxTextureSize = 4096`, sans redimensionnement).
  - `FontAssetGenerator.GenerateAll()` (même menu, appelé après) : importe les ressources essentielles TMP si absentes (`TMP_PackageResourceImporter`), crée un Font Asset **dynamique** par `.ttf` de `Resources/Fonts`, avec un matériau « Inked » (contour noir `_OutlineWidth` 0.2), sous `Art/Fonts/`, et y range les Font Assets dans `Resources/Fonts/TMP/` pour le chargement à l'exécution.
  - `Sprites.Rounded/RoundedSmall/Circle/Ring/VerticalGradient` chargent `Resources/UI/<Nom>` (repli procédural si absent). `Sprites.Logo/Title/BackgroundLandscape/BackgroundPortrait` restent des `Sprite` : en éditeur, ils chargent l'asset `Art/UI` (`AssetDatabase`) ; à l'exécution, le comportement actuel est conservé jusqu'à la tâche 8, où les prefabs portent la référence directe.
  - `Ui.Txt(...)` renvoie `TextMeshProUGUI` (même signature, `TextAnchor` converti en `TextAlignmentOptions`) ; `Inked(this TMP_Text t, float width)` choisit le matériau Inked ; `UiButton.Label` et `UiBar.Label` deviennent `TMP_Text`.
  - `LocalizedText : MonoBehaviour` (`[SerializeField] string key`, `public string Key`) : sur `OnEnable`, `GetComponent<TMP_Text>().text = Loc.T(key)`. Helpers `Ui.LocTxt(parent, key, size, color, anchor, font, name)` et `Ui.LocBtn(parent, key, …)` qui posent la clé.

- [ ] **Étape 1 : Tests en échec** (`UiAssetsTests`)
  - `Bake_WritesSlicedRoundedSprite` : après `BakeAll()`, `Resources.Load<Sprite>("UI/Rounded").border == new Vector4(18,18,18,18)`.
  - `Bake_KeepsArtResolution` : la largeur de `Art/UI/HD_BG_Landscape.jpg` (texture importée) est égale à celle de l'image décodée du `.bytes`.
  - `Fonts_AreDynamicTmpAssets` : les 3 `TMP_FontAsset` existent, `atlasPopulationMode == Dynamic`, chacun a un matériau `Inked` avec `_OutlineWidth > 0`.
  - `UiTxt_ReturnsTmpWithFont` : `Ui.Txt(parent, "x", 30, Color.white, TextAnchor.MiddleCenter, FontKind.Title)` est un `TextMeshProUGUI` dont `font.name` contient `Bangers`.
  - `LocalizedText_ResolvesKeyOnEnable` : un objet avec TMP + `LocalizedText{key="login.play"}` affiche `Loc.T("login.play")` une fois activé.
- [ ] **Étape 2 :** lancer `-testFilter N29.HeroesDawn.Tests.Editor.UiAssetsTests`. Attendu : échec de compilation ou de test (types absents).
- [ ] **Étape 3 :** implémenter le baker, le générateur de polices, `Sprites`, `Ui` en TMP et `LocalizedText`. Puis migrer chaque écran (`Text` → `TMP_Text`, `.alignment` en `TextAlignmentOptions`, `<size=24>` est déjà du rich text TMP), et passer les tailles 18 et 19 à 20 (`HubScreen` l.67/231/234/257, `BoardScreen` l.102, `CombatScreen` l.105/531, `UnitCard` l.69/75).
- [ ] **Étape 4 :** exécuter `UiAssetBaker.BakeAll` puis `FontAssetGenerator.GenerateAll` en batchmode, puis la suite EditMode. Attendu : 106/106.
- [ ] **Étape 5 : Captures** (`ScreenCaptureTests.CaptureEveryScreen`, PlayMode, `[Explicit]`) : parcourt Connexion → QG → Plateau → Combat avec le harnais et écrit `Code/docs/test-reports/captures/<écran>.png` (ignoré par git). Lancer avec `-testFilter`, vérifier les 4 images (textes lisibles, pas de débordement) et les envoyer à l'utilisateur.
- [ ] **Étape 6 :** suite PlayMode. Attendu : 14/14 (le test explicite n'est pas compté).
- [ ] **Étape 7 : Commit** — `feat(ui): sprites et illustrations en assets, TextMeshPro (Bangers / Comic Neue)`, avec les assets générés, les `.meta`, `.gitignore` (`captures/`) et les rapports.

### Tâche 2 : Socle MVP (exécution)

**Fichiers :**
- Créer : `Presentation/Mvp/*` (sauf `ScreenCatalog`, `LocalizedText`), `Presentation/Common/WalletBinder.cs`, `Tests/Presentation/N29.HeroesDawn.Tests.Presentation.asmdef` (Editor ; références : Tests.Domain, Presentation, Application, Core, Definitions, Domain.*, Content, UniTask, MessagePipe, VContainer, TestRunner), `Tests/Presentation/Fakes/FakeModalHost.cs`, `FakeBackInput.cs`, `FakeFrameClock.cs`, `FakeToasts.cs`, `FakeWalletView.cs`
- Modifier : `Presentation/N29.HeroesDawn.Presentation.asmdef` (+ `VContainer`)
- Test : `Tests/Presentation/BusyGateTests.cs`, `ModalServiceTests.cs`, `BackButtonRouterTests.cs`, `PresenterBaseTests.cs`, `WalletBinderTests.cs`

**Interfaces (produit) :**
```csharp
public interface IView { event Action? Shown; event Action? Hidden; bool IsVisible { get; } }
public abstract class ViewBase : MonoBehaviour, IView      // OnEnable → IsVisible=true, Shown ; OnDisable → Hidden
public enum Tone { Default, Muted, Dark, Primary, Success, Warning, Danger, Info, Perception, Credits, Pr, Shield }
public static class ToneColors { public static Color Of(Tone t); }   // traduction par Theme
public sealed class BusyGate { bool IsBusy; bool TryEnter(); void Exit(); }
public interface IFrameClock { float UnscaledDeltaTime { get; } }       // UnityFrameClock : Time.unscaledDeltaTime
public interface ILocalization { string T(string key); string F(string key, params object[] a); bool Has(string key);
                                 string Or(string key, string fallback); string Error(DomainError? e); string MemberName(MemberDef d);
                                 string EnemyName(EnemyDef d); string SkillName(SkillDef s); string ComboName(ComboDef c);
                                 string StatusName(StatusType s); string FamilyName(Family f); string RoleName(Role r);
                                 string RarityName(Rarity r); string RankName(CombatRank r); string DescribeSkill(SkillDef s); }
                                 // LocLocalization : délègue à Loc / SkillText
public interface IToastService { void Show(string text, ToastKind kind); }  // texte déjà localisé
public interface IPopupView { bool Cancelable { get; } event Action? CancelRequested; }
public interface IPopupHost { TView Spawn<TView>() where TView : class, IPopupView; void Despawn(IPopupView v); }
public interface IModalService {
    bool AnyOpen { get; }
    UniTask<TView> OpenAsync<TView>(CancellationToken ct) where TView : class, IPopupView;  // attend son tour (FIFO)
    void Close(IPopupView view);                       // ouvre le suivant de la file
    bool TryCancelTop();                               // Retour : CancelRequested sur la modale du dessus si annulable
    UniTask<bool> ConfirmAsync(ConfirmData data, CancellationToken ct);  // via IConfirmPopupView ; Retour = false
}
public readonly struct ConfirmData { string Title; string Body; string Ok; string Cancel; Tone OkTone; }
public interface IConfirmPopupView : IPopupView { UniTask<bool> AskAsync(ConfirmData d, CancellationToken ct); }
public sealed class ModalService : IModalService      // logique pure sur IPopupHost
public interface IBackHandler { bool HandleBack(); }  // true = consommé
public interface IBackInput { bool BackPressedThisFrame { get; } }   // InputSystemBackInput : Keyboard.escapeKey (le retour Android arrive en Échap)
public sealed class BackButtonRouter : ITickable { void Push(IBackHandler h); void Remove(IBackHandler h); bool Dispatch(); }
                                       // Tick : si BackPressedThisFrame → Dispatch : TryCancelTop, sinon le dernier poussé
public abstract class PresenterBase<TView> : IStartable, IDisposable where TView : IView
    // protected TView View; protected CancellationToken Lifetime; protected bool Disposed;
    // Start : abonne Shown/Hidden, appelle OnStart() puis OnShown() si View.IsVisible
    // Dispose : annule Lifetime, désabonne, appelle OnDispose ; ensuite aucune méthode OnShown/OnHidden n'est appelée
    // protected virtual IBackHandler? BackHandler => null ; poussé au routeur sur Shown, retiré sur Hidden et Dispose
public interface IWalletView { void Show(WalletData d); void SetSaving(bool saving); }
public readonly struct WalletData { int Credits; int Pr; int Gems; }
public sealed class WalletBinder : IDisposable { WalletBinder(IWalletView v, ISessionReadModel s, ISubscriber<WalletChanged> w, ISubscriber<SaveStateChanged> save); }
```

- [ ] **Étape 1 : Tests en échec**
  - `BusyGateTests` : `TryEnter_TwiceWithoutExit_SecondFails` ; `Exit_AllowsReentry`.
  - `ModalServiceTests` (avec `FakeModalHost` qui crée des faux popups) :
    - `Open_WhileOneOpen_WaitsUntilClosed` : la 2ᵉ tâche `OpenAsync` n'est pas terminée tant que la 1ʳᵉ modale n'est pas fermée, puis l'est ;
    - `TryCancelTop_NonCancelable_ReturnsFalseAndKeepsModal` ;
    - `TryCancelTop_Cancelable_RaisesCancelRequestedOnTopOnly` ;
    - `OpenAsync_CancelledWhileQueued_NeverSpawns` ;
    - `ConfirmAsync_Back_ReturnsFalse`.
  - `BackButtonRouterTests` :
    - `Dispatch_ModalOpen_CancelsModalFirst` ;
    - `Dispatch_NoModal_CallsLastPushedHandler` ;
    - `Dispatch_AfterRemove_SkipsRemovedHandler` ;
    - `Tick_WithoutPress_DoesNothing`.
  - `PresenterBaseTests` (présentateur de test sur `FakeView`) :
    - `Start_WhenViewVisible_CallsOnShown` ;
    - `Hidden_RemovesBackHandler` ;
    - `Dispose_CancelsLifetimeToken` ;
    - `Dispose_ThenShown_DoesNotCallOnShown`.
  - `WalletBinderTests` : `WalletChanged_PushesNewValues` ; `Dispose_StopsUpdates`.
- [ ] **Étape 2 :** lancer `-testFilter N29.HeroesDawn.Tests.Presentation`. Attendu : échec (types absents).
- [ ] **Étape 3 :** implémenter les types ci-dessus.
- [ ] **Étape 4 :** suite EditMode. Attendu : 106 + 17 = 123/123.
- [ ] **Étape 5 : Commit** — `feat(mvp): socle vues/présentateurs, modales en file, routeur Retour`.

### Tâche 3 : Générateur de prefabs, catalogue et widgets communs

**Fichiers :**
- Créer : `Editor/Prefabs/IPrefabRecipe.cs`, `PrefabContext.cs`, `PrefabGenerator.cs`, `PrefabGeneratorWindow.cs`, `GenerationReport.cs`, `Recipes/PortraitRecipe.cs`, `WalletRecipe.cs`, `ToastRecipe.cs`, `ToastsRecipe.cs`, `ModalLayerRecipe.cs`, `ConfirmPopupRecipe.cs`, `Presentation/Mvp/ScreenCatalog.cs`, `Presentation/Common/PortraitView.cs`, `WalletView.cs` (remplace celui de `Widgets.cs`), `ToastsView.cs`, `ModalLayer.cs` (réécrit), `ConfirmPopupView.cs`
- Modifier : `Editor/Scenes/SceneGenerator.cs` (catalogue sur les scopes), `Composition/RootLifetimeScope.cs`, `Composition/SceneScreenScope.cs`, `Presentation/UiRoot.cs`, `Presentation/UiContext.cs` (passe par `IToastService`)
- Test : `Tests/Editor/PrefabGeneratorTests.cs`, `PrefabStructureTests.cs`, `ScreenCatalogTests.cs`

**Interfaces (produit) :**
```csharp
public interface IPrefabRecipe { string RelativePath { get; }   // ex. "Widgets/Portrait.prefab"
                                 int Order { get; }              // 0 widgets, 10 common/popups, 20 screens
                                 GameObject Build(PrefabContext ctx); }
public sealed class PrefabContext { string Root; GameObject Nest(string relativePath, Transform parent);  // PrefabUtility.InstantiatePrefab
                                    void Wire(Component c, string field, UnityEngine.Object value); }   // SerializedObject
public enum OverwritePolicy { Never, All, Selected }
public static class PrefabGenerator {
    public const string DefaultRoot = "Assets/_Project/Prefabs";
    public static IReadOnlyList<IPrefabRecipe> Recipes { get; }          // par réflexion, triées (Order, RelativePath)
    public static GenerationReport Generate(string root, OverwritePolicy p, ISet<string>? selected = null);
    public static void GenerateAllBatch();   // -overwritePrefabs → All, sinon Never ; remplit le catalogue ; exit 1 si échec
}
public sealed class GenerationReport { List<string> Created, Overwritten, Skipped; List<(string path, string error)> Failed; }
[CreateAssetMenu] public sealed class ScreenCatalog : ScriptableObject {
    GameObject Login, Hub, Board, Combat, ModalLayer, Toasts; GameObject[] Popups;
    public GameObject PopupFor<TView>();     // le prefab dont la racine porte un composant TView ; exception explicite sinon
    public GameObject Screen(string scene);  // "Hub"/"Board"/"Combat" ; exception « Prefab d'écran manquant dans ScreenCatalog : X »
}
```
- `PortraitView` : `Show(PortraitData{ string Initial; Family Family; bool Boss })`.
- `ToastsView : IToastService`, qui instancie `Widgets/Toast`.
- `ModalLayer : IPopupHost`, qui instancie depuis le catalogue. Il garde provisoirement l'ancienne API `Open/Confirm/AnyOpen` (construction par code) pour les écrans non convertis ; elle est retirée en tâche 8.
- `UiRoot` : construit toujours ses canvas, mais instancie `ModalLayer` et `Toasts` depuis le catalogue.
- `RootLifetimeScope` : `[SerializeField] ScreenCatalog catalog`, enregistre `ScreenCatalog`, `IModalService`, `IToastService`, `ILocalization`, `IFrameClock`, `IBackInput`, `BackButtonRouter` (`RegisterEntryPoint`, `AsSelf`).
- `SceneScreenScope` : `[SerializeField] ScreenCatalog catalog`. Méthode abstraite `void ConfigureScreen(IContainerBuilder b, GameObject prefab, Transform root)` ; l'ancienne `CreateScreen` reste pour les scopes non convertis (supprimée en tâche 8).

- [ ] **Étape 1 : Tests en échec**
  - `PrefabGeneratorTests` (racine temporaire `Assets/_GenTest_<guid>`, supprimée en `TearDown`) :
    - `Generate_EmptyRoot_CreatesEveryRecipe` ;
    - `Generate_Never_DoesNotOverwriteEditedPrefab` (modifie la couleur d'un `Image` de `Widgets/Portrait`, régénère avec `Never`, la couleur est conservée) ;
    - `Generate_All_OverwritesPrefab` ;
    - `Generate_Twice_SameHierarchy` (noms et chemins identiques) ;
    - `NestedWidgetEdit_PropagatesToScreens` (modifie `Widgets/Portrait`, l'instance imbriquée dans `Common/…` ou `Popups/ConfirmPopup` le reflète).
  - `PrefabStructureTests` (sur `DefaultRoot`) :
    - `EveryViewSerializedReferenceIsWired` (tout champ `[SerializeField]` de type `UnityEngine.Object` d'un `MonoBehaviour` du namespace `N29.HeroesDawn.Presentation` est non nul) ;
    - `NoLegacyTextComponent` ;
    - `NoTmpTextUnder20px` ;
    - `EveryLocalizedTextKeyExists`.
  - `ScreenCatalogTests` :
    - `CatalogAsset_ReferencesExistingPrefabs` ;
    - `PopupFor_UnknownView_ThrowsExplicitMessage` ;
    - `GeneratedScenes_ScopesReferenceCatalog` (Boot, Hub, Board, Combat).
- [ ] **Étape 2 :** lancer ces trois classes. Attendu : échec.
- [ ] **Étape 3 :** implémenter le générateur, la fenêtre de menu (liste des prefabs existants, décochés par défaut, bouton « Générer »), le catalogue, les recettes et vues communes, et le câblage Root/UiRoot. Les recettes ne contiennent que des clés (`Ui.LocTxt`/`Ui.LocBtn`).
- [ ] **Étape 4 :** batchmode `PrefabGenerator.GenerateAllBatch`, puis `SceneGenerator.GenerateAll`, puis la suite EditMode (attendu : 123 + 12 = 135/135) et la suite PlayMode (attendu : 14/14, les toasts et confirmations passent par les prefabs).
- [ ] **Étape 5 : Commit** — `feat(prefabs): générateur de prefabs, ScreenCatalog, widgets et modales communs`.

### Tâche 4 : Connexion (pilote)

**Fichiers :**
- Créer : `Presentation/Login/ILoginView.cs`, `LoginView.cs`, `LoginPresenter.cs`, `Editor/Prefabs/Recipes/LoginRecipe.cs`, `Tests/Presentation/Fakes/FakeLoginView.cs`
- Modifier : `UiRoot.cs` (`SetLoginVisible` agit sur `LoginView`), `RootLifetimeScope.cs` (`RegisterComponentInNewPrefab(catalog.Login).UnderTransform(uiRoot.ScreensRoot).As<ILoginView>()`, `RegisterEntryPoint<LoginPresenter>().AsSelf()`), `Tests/PlayMode/PlayModeHarness.cs`, `BootFlowTests.cs`, `SceneFlowTests.cs`
- Test : `Tests/Presentation/LoginPresenterTests.cs`

**Interfaces :**
- `ILoginView : IView { event Action? PlayClicked; void SetBusy(bool busy, string status); void PlayIntro(); }`.
- `LoginView` expose `public Button PlayButton` (lecture seule) pour les tests PlayMode.
- `LoginPresenter(ILoginView, ISignInAsGuestUseCase, ISessionReadModel, IPublisher<NavigateRequested>, IToastService, ILocalization)` reprend `LoginScreen.PlayAsync` : `BusyGate`, toasts de bienvenue et de récupération, `NavigateRequested(Hub)`, message `login.fail_network` en cas d'exception.

- [ ] **Étape 1 : Tests en échec** (`LoginPresenterTests`, `TestApp` + `FakeLoginView`)
  - `Play_TwiceQuickly_SignsInOnce` (`FakeAuth` retardé : un seul appel au gateway) ;
  - `Play_Success_NavigatesToHubAndWelcomes` (publie `NavigateRequested(Hub)`, toast `login.welcome_new`) ;
  - `Play_Failure_ShowsErrorAndReleasesGate` (`ThrowOnSignIn` → `SetBusy(false, Loc.T("login.fail_network"))`, un second Jouer relance) ;
  - `Shown_ResetsBusyAndPlaysIntro`.
- [ ] **Étape 2 :** lancer les tests. Attendu : échec.
- [ ] **Étape 3 :** implémenter la vue, la recette (reprise ligne à ligne de `LoginScreen.Build`) et le présentateur. Retirer la création de `LoginScreen` dans `UiRoot`. Le harnais PlayMode remplace `Visible<LoginScreen>()` par `Visible<LoginView>()`, et `.Play()` par `PlayButton.onClick.Invoke()`.
- [ ] **Étape 4 :** générer les prefabs (`Never` : seul `Screens/Login` est créé), mettre à jour le catalogue, puis lancer EditMode (attendu : 139/139) et PlayMode (attendu : 14/14).
- [ ] **Étape 5 : Commit** — `feat(login): écran de connexion en prefab + LoginPresenter` (supprime `Screens/LoginScreen.cs`).

### Tâche 5 : QG

**Fichiers :**
- Créer : `Presentation/Hub/IHubView.cs`, `HubView.cs`, `HubData.cs`, `HubPresenter.cs`, `TeamCardView.cs`, `RecruitCardView.cs`, `SquadSlotView.cs`, les recettes `TeamCardRecipe`, `RecruitCardRecipe`, `SquadSlotRecipe` et `HubRecipe`, `Tests/Presentation/Fakes/FakeHubView.cs`
- Modifier : `Composition/HubLifetimeScope.cs` (`ConfigureScreen` : vue + `RegisterEntryPoint<HubPresenter>().AsSelf()` + `WalletBinder`), les tests PlayMode concernés
- Test : `Tests/Presentation/HubPresenterTests.cs`

**Interfaces :**
```csharp
public enum HubTab { Team, Recruit }
public interface IHubView : IView {
    event Action<HubTab>? TabClicked; event Action? LogoutClicked, ResetClicked, HealClicked, DeployClicked;
    event Action<MemberId>? SquadToggleClicked, TrainClicked; event Action<MemberId>? RecruitClicked;
    IWalletView Wallet { get; }
    void Show(HubData d); void ScrollToTop();
}
// HubData : PlayerLine, Tab, Hint, Team (TeamCardData[]), Recruits (RecruitCardData[]), EmptyRecruitText,
//           ZoneObjectives (string[]), ZoneStatus, ZoneDone, SquadTitle, Squad (SquadSlotData[]),
//           HealLabel, HealEnabled, DeployEnabled, DeployHint
// TeamCardData : Id, Name, RarityText, RarityTone, RoleLine, LevelText, Stats, Skills, Portrait (PortraitData),
//                Fatigue01, FatigueText, FatigueTone, InSquad, SquadLabel, TrainLabel, TrainEnabled, Exhausted
// RecruitCardData : Id, Name, RarityText, RarityTone, RoleLine, LevelText, Stats, Skills, Portrait, RecruitLabel, RecruitEnabled
// SquadSlotData : Name, Portrait, Fatigue01, FatigueTone
```
- `HubPresenter(IHubView, ISessionReadModel, ZoneId firstZone, IToggleSquadUseCase, ITrainMemberUseCase, IRecruitMemberUseCase, IHealRosterUseCase, IStartZoneUseCase, ISignOutUseCase, IResetProgressUseCase, ISubscriber<RosterChanged>, ISubscriber<WalletChanged>, IPublisher<NavigateRequested>, IModalService, IToastService, ILocalization)`.
- Il reprend `HubScreen` à l'identique : `Act` → `BusyGate` ; le déploiement garde la garde levée après succès, jusqu'au prochain `Shown` ; abonnements Roster/Wallet sur `Shown`, libérés sur `Hidden`.
- Il expose `public int RefreshCount` pour les tests PlayMode.
- `HubView` expose `DeployButton` pour les tests PlayMode. Les cartes sont des instances des prefabs widgets (`[SerializeField] TeamCardView teamCardPrefab`…) recréées par `Show`.

- [ ] **Étape 1 : Tests en échec** (`HubPresenterTests`)
  - `Shown_ShowsRosterOnTeamTab` (autant de `TeamCardData` que de membres) ;
  - `RecruitTab_ListsAvailableRecruits` ;
  - `Recruit_Success_ToastsAndRefreshes` ;
  - `Train_NotEnoughCredits_CardDisabled` (`TrainEnabled == false`) ;
  - `Heal_NoFatigue_Disabled` ;
  - `Deploy_TwiceQuickly_StartsZoneOnceAndNavigatesOnce` ;
  - `Deploy_Failure_ShowsErrorAndAllowsRetry` ;
  - `Hidden_ThenWalletChanged_NoRefresh` ;
  - `Dispose_DuringRecruit_NoViewCallAfterwards` (Review Focus 4 : `FakeHubView.CallsAfter(dispose) == 0`) ;
  - `Reset_AsksConfirmationThenResets` (faux `IModalService` qui répond `true`).
- [ ] **Étape 2 :** lancer les tests. Attendu : échec.
- [ ] **Étape 3 :** implémenter les widgets, la vue, les recettes (reprise de `HubScreen.Build`, `CardBase`, `TeamCard`, `RecruitCard`, `RefreshOps`) et le présentateur. Les tests PlayMode utilisent `DeployButton.onClick.Invoke()` et `HubPresenter.RefreshCount`, résolus via `HubLifetimeScope.Container`.
- [ ] **Étape 4 :** générer (`Never`), puis lancer EditMode (attendu : 149/149) et PlayMode (attendu : 14/14).
- [ ] **Étape 5 : Commit** — `feat(hub): QG en prefabs + HubPresenter` (supprime `Hub/HubScreen.cs`).

### Tâche 6 : Popups de case (vues et présentateur)

**Fichiers :**
- Créer : `Presentation/Popups/*` (vues, interfaces, `PopupData.cs`, `TimingMiniGame.cs` déplacé et passé en TMP), `Presentation/Board/CasePopupPresenter.cs`, les recettes `ChoiceButtonRecipe`, `RestPopupRecipe`, `NarrativePopupRecipe`, `MiniGamePopupRecipe` et `ZoneCompletePopupRecipe`, `Tests/Presentation/Fakes/FakePopups.cs`
- Test : `Tests/Presentation/CasePopupPresenterTests.cs`

**Interfaces :**
```csharp
public interface IRestPopupView : IPopupView { UniTask<bool> ChooseAsync(RestPopupData d, CancellationToken ct); }        // true = se reposer
public interface INarrativePopupView : IPopupView { UniTask<NarrativeChoice> ChooseAsync(NarrativePopupData d, CancellationToken ct); }
public interface IMiniGamePopupView : IPopupView { UniTask<bool> PlayAsync(MiniGamePopupData d, CancellationToken ct); }   // réussite
public interface IZoneCompletePopupView : IPopupView { UniTask ShowAsync(ZoneCompleteData d, CancellationToken ct); }      // « Vers le QG »
// PopupData.cs : RestPopupData{Title,Body,Skip,Take} ; NarrativePopupData{Title,Body,Choices: (NarrativeChoice, string, Tone)[]} ;
//                MiniGamePopupData{Title,Body,Rules,Stop} ; ZoneCompleteData{Title,Rewards,ToHq}
public sealed class CasePopupPresenter {
    CasePopupPresenter(IModalService, IResolveRestUseCase, IResolveNarrativeUseCase, IResolveMiniGameUseCase, ISessionReadModel, IToastService, ILocalization);
    UniTask<IReadOnlyList<BoardEvent>> RunAsync(CaseOutcome outcome, CancellationToken ct);  // ouvre, attend, résout, ferme
    UniTask ShowZoneCompleteAsync(ZoneRewards? rewards, CancellationToken ct);
}
```
- Les popups de case ne sont **pas** annulables : un choix est requis, comme aujourd'hui. Seule `ConfirmPopup` l'est.
- En cas d'échec du use case : toast d'erreur et liste vide renvoyée (comportement actuel de `CasePopups.Finish`).

- [ ] **Étape 1 : Tests en échec** (`CasePopupPresenterTests`)
  - `Rest_Take_ResolvesRestTrueAndReturnsEvents` ;
  - `Narrative_Research_ResolvesResearch` ;
  - `MiniGame_Success_ToastsRewardAndResolvesTrue` ;
  - `MiniGame_Failure_ToastsWarningAndResolvesFalse` ;
  - `UseCaseFailure_ReturnsEmptyAndToastsError` ;
  - `Popup_IsClosedAfterResolution` ;
  - `CasePopups_AreNotCancelable` ;
  - `ZoneComplete_FirstClear_ShowsCreditsAndPr`.
- [ ] **Étape 2 :** lancer les tests. Attendu : échec.
- [ ] **Étape 3 :** implémenter les vues, les recettes (reprise de `CasePopups`) et le présentateur. Ne pas encore le brancher : l'ancien plateau garde `CasePopups` jusqu'à la tâche 7.
- [ ] **Étape 4 :** générer (`Never`), puis lancer EditMode (attendu : 157/157) et PlayMode (attendu : 14/14).
- [ ] **Étape 5 : Commit** — `feat(popups): popups de case en prefabs + CasePopupPresenter`.

### Tâche 7 : Plateau

**Fichiers :**
- Créer : `Presentation/Board/IBoardView.cs`, `BoardView.cs`, `IBoardMapView.cs`, `BoardMapView.cs` (réécrit sur données), `BoardNodeView.cs`, `ObjectiveLineView.cs`, `BoardData.cs`, `BoardPresenter.cs`, les recettes `BoardNodeRecipe`, `BoardLinkRecipe`, `ObjectiveLineRecipe` et `BoardRecipe`, `Tests/Presentation/Fakes/FakeBoardView.cs`
- Modifier : `Composition/BoardLifetimeScope.cs` (vue, `BoardPresenter` en `AsSelf`, `CasePopupPresenter`, `WalletBinder`), les tests PlayMode concernés
- Test : `Tests/Presentation/BoardPresenterTests.cs`

**Interfaces :**
```csharp
public interface IBoardView : IView {
    event Action? RollClicked, BonusClicked, EndMoveClicked, BackClicked;   // Espace / B / Entrée relayés en intentions
    IWalletView Wallet { get; } IBoardMapView Map { get; }
    void SetZoneTitle(string t); void ShowObjectives(IReadOnlyList<ObjectiveLineData> o); void ShowSquad(IReadOnlyList<SquadSlotData> s);
    void ShowHud(BoardHudData d); UniTask RollDieAsync(int value, int faces, CancellationToken ct); void ResetDie();
}
public interface IBoardMapView {
    event Action<CaseId>? NodeTapped;
    void Build(BoardMapData d); void Refresh(IReadOnlyList<BoardNodeState> nodes, IReadOnlyList<BoardLinkState> links, CaseId pawnAt, bool movePawn);
    UniTask MovePawnAsync(IReadOnlyList<CaseId> path, CancellationToken ct);
}
// BoardMapData : Nodes (Id, X, Y, IsObjective)[], Links (A, B)[], Pawn (PortraitData), PawnAt
// BoardNodeState : Id, Revealed, Glyph, Sub, SubTone, Fill (Tone|CaseType+Rank+Spent), ObjectiveDone, DestinationCost (-1 = non)
// BoardLinkState : A, B, Visible, Dim
// BoardHudData : Hint, PmText, Reserve01, ReserveLabel, RollEnabled, BonusLabel, BonusEnabled, EndEnabled
// ObjectiveLineData : Text, Done
```
- `BoardPresenter : PresenterBase<IBoardView>`, avec `IBackHandler` → `RequestBackToHq()`. Dépendances : use cases de zone, `IBeginCombatUseCase`, `ILeaveZoneUseCase`, `ISessionReadModel`, `IGameFlow`, `CasePopupPresenter`, `ISubscriber<ZoneValidated>`, `ISubscriber<ReturnedToBoard>`, `ISubscriber<RosterChanged>`, `IPublisher<NavigateRequested>`, `IModalService`, `IToastService`, `ILocalization`.
- Il reprend `BoardScreen` : `RunBoard` → `BusyGate` ; ordre des événements (déplacement animé, toasts, case déclenchée) ; reprise d'une case ouverte ou d'un combat en attente sur `Shown`.
- Les abonnements `ZoneValidated`/`ReturnedToBoard` sont pris dans `OnStart` et rendus dans `OnDispose`, jamais sur `Hidden`.
- Une intention reçue alors que `IModalService.AnyOpen` est vrai est ignorée.
- Il expose `ReportsApplied` et `LastZoneRewards`. `BoardMapView` expose `NodeButton(CaseId)` et `BoardView` expose `BackButton` pour les tests PlayMode.
- La couleur de case reste calculée par `Theme.CaseColor(type, rank)` dans la vue, à partir de `CaseType`/`CombatRank`/`Spent` transmis dans `BoardNodeState`.

- [ ] **Étape 1 : Tests en échec** (`BoardPresenterTests`, zone 1.1 démarrée sur `TestApp`)
  - `Shown_NoRun_NavigatesToHub` ;
  - `Roll_RollsOnceAndAnimatesDie` ;
  - `Roll_WhileBusy_Ignored` ;
  - `Roll_WhileModalOpen_Ignored` (Review Focus 5) ;
  - `TapNode_NotDestination_Ignored` ;
  - `TapNode_Destination_MovesAndRefreshes` ;
  - `CombatCase_BeginsCombatAndNavigatesToCombat` ;
  - `RestCase_OpensRestPopupThroughCasePresenter` ;
  - `Back_AsksConfirmation_ThenLeavesZoneAndNavigatesHub` ;
  - `Back_WhenHidden_NotHandled` (Review Focus 2 : après `Hidden`, `BackButtonRouter.Dispatch()` ne demande pas de confirmation) ;
  - `ReturnedToBoard_WhileHidden_StillApplied` (`ReportsApplied == 1`) ;
  - `ZoneValidated_ShowsZoneCompleteThenLeaves`.
- [ ] **Étape 2 :** lancer les tests. Attendu : échec.
- [ ] **Étape 3 :** implémenter les widgets, la carte, la vue, les recettes (reprise de `BoardScreen.Build` et `BoardMapView.Build`) et le présentateur. Les tests PlayMode utilisent `NodeButton(new CaseId("n03")).onClick.Invoke()` et `BoardPresenter.ReportsApplied` / `LastZoneRewards`.
- [ ] **Étape 4 :** générer (`Never`), puis lancer EditMode (attendu : 169/169) et PlayMode (attendu : 14/14).
- [ ] **Étape 5 : Commit** — `feat(board): plateau en prefabs + BoardPresenter` (supprime `Board/BoardScreen.cs`, l'ancienne `BoardMapView`, `Popups/CasePopups.cs`).

### Tâche 8 : Combat

**Fichiers :**
- Créer : `Presentation/Combat/ICombatView.cs`, `CombatView.cs`, `CombatData.cs`, `CombatPresenter.cs`, `UnitCardView.cs`, `SkillButtonView.cs`, `FloatingTextView.cs`, `ICombatResultsPopupView.cs`, `CombatResultsPopupView.cs`, les recettes `UnitCardRecipe`, `SkillButtonRecipe`, `FloatingTextRecipe`, `FatigueRowRecipe`, `CombatResultsPopupRecipe` et `CombatRecipe`, `Tests/Presentation/Fakes/FakeCombatView.cs`
- Modifier : `Composition/CombatLifetimeScope.cs`, les tests PlayMode concernés
- Test : `Tests/Presentation/CombatPresenterTests.cs`

**Interfaces :**
```csharp
public interface ICombatView : IView {
    event Action? RetreatClicked, AutoClicked, SpeedClicked, DebugWinClicked, ComboClicked, CycleClicked;  // A, C, Tab relayés
    event Action<int>? SkillClicked;   // 1-3
    event Action<int>? UnitTapped;
    void BuildUnits(IReadOnlyList<UnitCardData> units); void ShowUnits(IReadOnlyList<UnitStateData> states);
    void ShowSkills(SkillPanelData d); void ShowCombo(bool visible, string label); void ShowTopBar(CombatTopBarData d);
    void SetHint(string t); void ShowBanner(string text, Tone tone); void ShowLog(IReadOnlyList<string> lines);
    void Float(int unitId, string text, Tone tone, int size); void Hit(int unitId); void Act(int unitId);
}
// UnitCardData : Id, Ally, Name, NameTone, Portrait ; UnitStateData : Id, Hp01, HpText, Gauge01, Ready, Statuses, Alive, Highlight, Selected, Targetable
// SkillPanelData : Visible, Slots (Label, Description, Tone, Pending, Enabled)[] ; CombatTopBarData : Title, AutoOn, SpeedText, RetreatEnabled, DebugVisible
public interface ICombatResultsPopupView : IPopupView {   // Cancelable = true : Retour = Continuer
    UniTask ShowAsync(CombatResultsData d, CancellationToken ct);   // se termine sur « Continuer »
    void SetVerification(string text, Tone tone);
}
// CombatResultsData : Title, TitleTone, Reward, Fatigue (FatigueRowData: Name, Before01, After01, Label, Delta)[], ToHq (string, vide si non)
```
- `CombatPresenter : PresenterBase<ICombatView>, ITickable`, avec `IBackHandler`. Dépendances : `IActiveCombatReadModel`, `ISessionReadModel`, `ISetAutoCombatUseCase`, `IFinishCombatUseCase`, `IVerifyCombatUseCase`, `IForceCombatWinUseCase`, `IPublisher<CombatResultsClosed>`, `IPublisher<NavigateRequested>`, `IModalService`, `IToastService`, `ILocalization`, `IFrameClock`.
- `Tick()` reprend `CombatScreen.Update` (accumulateur × vitesse, 12 pas maximum par image, pause si `AnyOpen`) avec `IFrameClock.UnscaledDeltaTime`.
- La sélection, le ciblage, le combo, l'auto, la vitesse, la retraite (confirmation), la fin, les résultats, la vérification et « Continuer » (`Combat.Clear()`, toast `combat.to_hq`, `CombatResultsClosed`) sont repris à l'identique.
- `HandleBack()` : si un ciblage est en cours, il l'annule et renvoie `true` ; sinon il renvoie `false` (les résultats passent par la modale annulable).
- Les touches 1-3/C/A/Tab sont lues par la vue ; Échap est retiré de la vue (routeur).
- `CombatResultsPopupView` expose `ContinueButton` pour les tests PlayMode, qui remplacent `ContinueForTests()` par l'attente du popup puis `ContinueButton.onClick.Invoke()`.

- [ ] **Étape 1 : Tests en échec** (`CombatPresenterTests`, combat démarré sur `TestApp` ; `FakeFrameClock` à 100 ms)
  - `Shown_BuildsOneCardPerUnit` ;
  - `Tick_AdvancesSimulationBySpeed` ;
  - `ReadyAlly_IsAutoSelectedOldestFirst` ;
  - `SingleTargetSkill_WithSeveralEnemies_WaitsForTarget` ;
  - `Back_DuringTargeting_CancelsTargeting` ;
  - `Back_NoTargeting_NotHandled` ;
  - `Combo_SubmitsBothActors` ;
  - `Auto_TogglesThroughUseCase` ;
  - `Speed_Cycles1To3` ;
  - `Retreat_AsksConfirmation` ;
  - `ModalOpen_PausesTicks` ;
  - `Ended_ShowsResultsThenContinuePublishesClosed` ;
  - `ResultsBack_ActsAsContinue`.
- [ ] **Étape 2 :** lancer les tests. Attendu : échec.
- [ ] **Étape 3 :** implémenter les widgets, la vue, la popup de résultats, les recettes (reprise de `CombatScreen.Build`, `UnitCard.Create`, `ShowResults`) et le présentateur.
- [ ] **Étape 4 :** générer (`Never`), puis lancer EditMode (attendu : 182/182) et PlayMode (attendu : 14/14).
- [ ] **Étape 5 : Commit** — `feat(combat): combat en prefabs + CombatPresenter` (supprime `Combat/CombatScreen.cs`, `Combat/UnitCard.cs`).

### Tâche 9 : Nettoyage, tests de bout en bout et docs

**Fichiers :**
- Supprimer : `UiContext.cs`, `UiServices.cs`, `Screens/UiScreen.cs`, `Common/Widgets.cs`, `Common/Toasts.cs` (ancien), l'API `Open/Confirm` de `ModalLayer`, `SceneScreenScope.CreateScreen`, `Sprites.Art` à l'exécution (les prefabs portent les références)
- Créer : `Tests/PlayMode/BackNavigationTests.cs`, `Tests/PlayMode/FullRunTests.cs`, `Tests/Editor/NoCodeBuiltScreensTests.cs`
- Modifier : `04_Tech_Unity/07_Presentation_Flux.md`, `09_Tests_Outils_CI.md`, `05_Production/Decisions.md`, `Code/README.md`, `Code/SETUP.md` (menus Générer les assets d'interface / les prefabs)

- [ ] **Étape 1 : Tests en échec**
  - `BackNavigationTests` :
    - `EscapeOnBoard_OpensConfirm_SecondEscapeCloses` (via `BackButtonRouter.Dispatch()` résolu dans Root) ;
    - `EscapeInCombat_DoesNotOpenHqConfirm` (Review Focus 2).
  - `FullRunTests.LoginRecruitDeployCombatResultsBoardHq` : clics réels sur les boutons des prefabs, sans aucune méthode de test.
  - `NoCodeBuiltScreensTests.RuntimeAssembliesDontCallUiFactory` : aucune référence à `Ui.Txt/Btn/Img/Panel/Rect` dans les types d'exécution de `N29.HeroesDawn.Presentation` hors `Common/Ui.cs` (analyse IL par `MethodBody.GetILAsByteArray` + `Module.ResolveMethod`).
- [ ] **Étape 2 :** lancer les tests. Attendu : échec (`NoCodeBuiltScreensTests` tant que l'ancien code est présent).
- [ ] **Étape 3 :** supprimer l'ancien code, mettre à jour les docs :
  - doc 07 : « État réel (sous-projet 3) », sans `IGameState`, avec le routeur Retour et le catalogue ;
  - doc 09 : nouvelles suites ;
  - `Decisions.md` : `2026-10-03 | Sous-projet 3 : prefabs générés puis éditables (recettes éditeur, ScreenCatalog), présentateurs C# purs, TextMeshPro, Retour par BackButtonRouter | Mise en page des maquettes : retouche des prefabs`.
- [ ] **Étape 4 :** régénérer (`Never`), relancer le générateur de scènes, puis les suites complètes. Attendu : EditMode 183/183, PlayMode 17/17. Vérifier aussi qu'une seconde génération `Never` ne signale aucun écrasement (critère de fin 6).
- [ ] **Étape 5 : Commit** — `refactor(ui): retire l'interface construite par code, tests de bout en bout et docs`.

---

## Couverture de la spec
| Spec | Tâche |
|---|---|
| §3 découpage, règles vue/présentateur | 4–8 |
| §4 générateur, recettes, prefabs imbriqués, pas d'écrasement, `ScreenCatalog` | 3 (+ recettes 4–8) |
| §5 TMP, polices, matériau encré, 20 px | 1, test en 3 |
| §6 présentateurs, `BusyGate`, cycle de vie | 2, 4–8 |
| §7 modales en file, routeur Retour, tableau des cas | 2, 6–9 |
| §8 erreurs (prefab manquant, recette en échec, use case en échec) | 3, 4–8 |
| §9 tests | toutes |
| §10 ordre | 1 → 9 (le plateau est scindé : popups en 6, plateau en 7) |
| Critère 6 (retouche conservée) | 3 (test), 9 (vérification) |

Écarts à la spec, à consigner comme décisions (`Ruling:`) à l'exécution :
- Sprites procéduraux et illustrations `.bytes` transformés en assets (tâche 1) : sans cela, un prefab ne peut pas les référencer.
- Les totaux attendus sont indicatifs : seul compte « 0 échec et autant de tests que prévu par la tâche ».
