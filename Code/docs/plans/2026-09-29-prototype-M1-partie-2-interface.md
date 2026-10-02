# Prototype M1 — Plan d'implémentation · Partie 2 : composition, interface, outils

> **Pour les agents :** SOUS-COMPÉTENCE REQUISE : `superpowers:subagent-driven-development` (recommandé) ou `superpowers:executing-plans`. Cases à cocher (`- [ ]`).
> Prérequis : **Partie 1** terminée (tâches 0 à 11, tous les tests verts).

**Objectif :** rendre la boucle plateau → combat **jouable, lisible et agréable** sur mobile paysage et PC. L'interface est le critère de réussite du prototype : un joueur qui ne connaît pas le jeu doit comprendre quoi faire en moins de 30 secondes sur chaque écran.

**Architecture :** MVP. Vues `MonoBehaviour` passives derrière des interfaces `I…View` ; présentateurs C# injectés par VContainer (`IStartable`, `ITickable`, `IDisposable`) ; événements via MessagePipe ; navigation par `GameFlow`. Le combat est **joué** par un `CombatDirector` qui consomme les `CombatEventBatch`.

**Stack :** uGUI + TextMeshPro, PrimeTween, Input System (EnhancedTouch pour le pinch), URP 2D.

**Specs :** `03_Art_UI_UX/01_UX_Ecrans.md`, `02_Direction_Artistique_Audio.md`, `04_Tech_Unity/07_Presentation_Flux.md`.

## Contraintes globales (interface)
- Paysage, référence **1920 × 1080**, `CanvasScaler` « Scale With Screen Size », match 0,5 ; zones sûres respectées.
- **Cibles tactiles ≥ 120 px** de côté à la résolution de référence ; espacement ≥ 24 px.
- Une **couleur + une icône** par famille (jamais la couleur seule) : Matière ambre `#F2A93B`, Élémentaire cyan `#3BC7F2`, Physique/Bio rouge `#E5484D`, Perception violet `#9B6BF2`.
- Tout retour d'action en **< 100 ms** (enfoncement du bouton, son, surlignage) ; toute animation de jeu ≤ 600 ms, accélérée en vitesse ×2.
- Textes par clés (`ILocalization`) ; tailles de texte : 28 (corps), 36 (boutons), 56 (titres), 96 (dégâts critiques).
- Aucune règle de jeu dans les vues ou présentateurs : ils affichent ce que le domaine expose (`Destinations`, `CanUse`, `AvailableCombos`…).

## Points d'attention (Review Focus)
1. **Double tap / spam** sur « Lancer le dé » ou une destination pendant une animation → une seule action ; les entrées sont bloquées tant que l'animation du déplacement n'est pas finie → test PlayMode tâche 15.
2. **Écran étroit (21:9) et encoche** : HUD jamais sous l'encoche, rien coupé → vérification manuelle avec le simulateur d'appareil (tâche 13).
3. **Combat en mode manuel quand plusieurs alliés sont prêts** : le joueur doit savoir **qui** agit ; le portrait sélectionné est explicite, et changer de portrait est possible → test PlayMode tâche 17.
4. **Cible invalide** (ennemi mort entre la sélection et la validation) → la commande est redirigée par le domaine, l'UI ne plante pas → test PlayMode tâche 17.
5. **Retour au plateau après défaite** : le pion recule visiblement, la fatigue est affichée, le dé est de nouveau disponible → test PlayMode tâche 15.

---

## Carte des fichiers (Partie 2)
```
Assets/_Project/
├─ Composition/
│  ├─ RootLifetimeScope.cs        (Boot, vit tout le jeu)
│  ├─ BoardLifetimeScope.cs       (scène Board)
│  ├─ CombatLifetimeScope.cs      (scène Combat, enfant de Board)
│  ├─ GameFlow.cs                 (navigation Board ↔ Combat ↔ résultats)
│  └─ PrototypeBootstrap.cs       (ouvre la zone 1.1 au démarrage)
├─ Presentation/
│  ├─ Common/
│  │  ├─ UiTheme.cs (ScriptableObject)  FamilyVisuals.cs
│  │  ├─ ILocalization.cs  LocalLocalization.cs  Localization/fr.json
│  │  ├─ SafeAreaFitter.cs  ComicButton.cs  InputBlocker.cs
│  │  ├─ ToastView.cs  IToastService.cs  ToastService.cs
│  │  └─ ModalView.cs  IModalService.cs  ModalService.cs
│  ├─ Board/
│  │  ├─ World/ BoardWorldView.cs  NodeView.cs  LinkView.cs  PawnView.cs  BoardCameraController.cs
│  │  ├─ Hud/   IBoardHudView.cs  BoardHudView.cs  DiceView.cs  BonusGaugeView.cs  ObjectivesView.cs  WalletView.cs
│  │  ├─ Cases/ NarrativeOverlayView.cs  RestPromptView.cs  DemonstrationView.cs  ZoneCompleteView.cs
│  │  └─ Presenters/ BoardPresenter.cs  CaseInteractionPresenter.cs
│  ├─ Combat/
│  │  ├─ Arena/ CombatDirector.cs  UnitView.cs  DamageNumberView.cs  DamageNumberPool.cs  StatusBarView.cs
│  │  ├─ Hud/   ICombatHudView.cs  CombatHudView.cs  PortraitView.cs  SkillPanelView.cs  ComboButtonView.cs
│  │  ├─ Results/ CombatResultsView.cs
│  │  └─ Presenters/ CombatPresenter.cs
│  └─ Debug/ DebugPanelView.cs
├─ Prefabs/UI/  Prefabs/Board/  Prefabs/Combat/
├─ Scenes/ Boot.unity  Board.unity  Combat.unity
└─ Settings/ UiTheme.asset
Tools/BalanceSimulator/Program.cs
```

---

### Tâche 12 : Composition — scopes, scènes, navigation

**Fichiers :** Créer `Composition/RootLifetimeScope.cs`, `BoardLifetimeScope.cs`, `CombatLifetimeScope.cs`, `GameFlow.cs`, `PrototypeBootstrap.cs` ; scènes `Boot`, `Board`, `Combat` · Test `Assets/Tests/PlayMode/BootFlowTests.cs`

**Interfaces :**
- Consomme : use cases et messages de la Partie 1.
- Produit :
  - `RootLifetimeScope` enregistre : MessagePipe (`RegisterMessagePipe()` + tous les brokers de messages de la Partie 1), `ContentCatalog` (`PrototypeContent.Build()`), `GameSession`, `ISeedProvider`→`LocalSeedProvider`, `ILogger`→`UnityLogger`, `ILocalization`→`LocalLocalization`, `BoardPublishers`, `BoardEventPublisher`, tous les use cases (singletons), `CombatRunner`, `GameFlow`, `IToastService`, `IModalService`.
  - `GameFlow` : `UniTask EnterBoardAsync()`, `UniTask EnterCombatAsync(CombatRequested)`, `UniTask ExitCombatAsync()` ; s'abonne à `CombatRequested` (→ combat) et `CombatFinished` (→ attend la fermeture de l'écran de résultats puis retour plateau via `ResultsClosed`).
  - Message supplémentaire : `ResultsClosed` (struct vide) publié par l'écran de résultats.

- [ ] **Étape 1 : Test PlayMode en échec**
```csharp
// Assets/Tests/PlayMode/BootFlowTests.cs
using System.Collections;
using NUnit.Framework;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class BootFlowTests
{
    [UnityTest]
    public IEnumerator Boot_LoadsBoardScene_WithZoneOpened()
    {
        SceneManager.LoadScene("Boot");
        for (int i = 0; i < 120 && !SceneManager.GetSceneByName("Board").isLoaded; i++) yield return null;
        Assert.That(SceneManager.GetSceneByName("Board").isLoaded, Is.True);
        var hud = UnityEngine.Object.FindAnyObjectByType<N29.HeroesDawn.Presentation.Board.BoardHudView>();
        Assert.That(hud, Is.Not.Null);
    }
}
```
(Les tests PlayMode ont le droit d'utiliser `FindAnyObjectByType` ; le code de jeu non.)

- [ ] **Étape 2 : Vérifier l'échec** (scène absente).
- [ ] **Étape 3 : Implémentation**
```csharp
// Composition/RootLifetimeScope.cs
using MessagePipe;
using N29.HeroesDawn.Application.Features.Board;
using N29.HeroesDawn.Application.Features.Combat;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Infrastructure.Local;
using N29.HeroesDawn.Presentation.Common;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace N29.HeroesDawn.Composition
{
    public sealed class RootLifetimeScope : LifetimeScope
    {
        [SerializeField] UiTheme _theme = null!;
        [SerializeField] ToastView _toast = null!;
        [SerializeField] ModalView _modal = null!;

        protected override void Awake()
        {
            DontDestroyOnLoad(gameObject);
            base.Awake();
        }

        protected override void Configure(IContainerBuilder b)
        {
            var options = b.RegisterMessagePipe();
            b.RegisterBuildCallback(c => GlobalMessagePipe.SetProvider(c.AsServiceProvider()));
            // Plateau
            b.RegisterMessageBroker<ZoneOpened>(options); b.RegisterMessageBroker<DiceRolled>(options);
            b.RegisterMessageBroker<DestinationsChanged>(options); b.RegisterMessageBroker<PawnMoved>(options);
            b.RegisterMessageBroker<FogRevealed>(options); b.RegisterMessageBroker<ResourceCollected>(options);
            b.RegisterMessageBroker<CaseTriggered>(options); b.RegisterMessageBroker<CombatRequested>(options);
            b.RegisterMessageBroker<ObjectiveCompleted>(options); b.RegisterMessageBroker<ZoneValidated>(options);
            b.RegisterMessageBroker<DiceBankChanged>(options); b.RegisterMessageBroker<SteppedBack>(options);
            b.RegisterMessageBroker<WalletChanged>(options);
            // Combat
            b.RegisterMessageBroker<CombatStarted>(options); b.RegisterMessageBroker<CombatEventBatch>(options);
            b.RegisterMessageBroker<CombatFinished>(options); b.RegisterMessageBroker<ResultsClosed>(options);

            b.RegisterInstance(PrototypeContent.Build());
            b.Register<GameSession>(Lifetime.Singleton);
            b.Register<ISeedProvider, LocalSeedProvider>(Lifetime.Singleton);
            b.Register<ILogger, UnityLogger>(Lifetime.Singleton);
            b.Register<ILocalization, LocalLocalization>(Lifetime.Singleton);
            b.RegisterInstance(_theme);
            b.RegisterComponent(_toast).As<IToastService>();
            b.RegisterComponent(_modal).As<IModalService>();

            b.Register<BoardPublishers>(Lifetime.Singleton);
            b.Register<BoardEventPublisher>(Lifetime.Singleton);
            b.Register<IOpenZoneUseCase, OpenZoneUseCase>(Lifetime.Singleton);
            b.Register<IRollDiceUseCase, RollDiceUseCase>(Lifetime.Singleton);
            b.Register<IUseBonusDieUseCase, UseBonusDieUseCase>(Lifetime.Singleton);
            b.Register<IMoveToUseCase, MoveToUseCase>(Lifetime.Singleton);
            b.Register<IEndMoveUseCase, EndMoveUseCase>(Lifetime.Singleton);
            b.Register<IResolveCaseUseCase, ResolveCaseUseCase>(Lifetime.Singleton);

            b.Register<CombatRunner>(Lifetime.Singleton);
            b.Register<IStartCombatUseCase, StartCombatUseCase>(Lifetime.Singleton);
            b.Register<IResolveCombatUseCase, ResolveCombatUseCase>(Lifetime.Singleton);
            b.Register<ICombatCommandsUseCase, CombatCommandsUseCase>(Lifetime.Singleton);

            b.RegisterEntryPoint<GameFlow>().AsSelf();
            b.RegisterEntryPoint<PrototypeBootstrap>();
        }
    }
}
```
```csharp
// Application/Features/Combat/Messages/ResultsClosed.cs
namespace N29.HeroesDawn.Application.Features.Combat { public readonly struct ResultsClosed { } }
```
```csharp
// Composition/GameFlow.cs
using System;
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Board;
using N29.HeroesDawn.Application.Features.Combat;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace N29.HeroesDawn.Composition
{
    public sealed class GameFlow : IStartable, IDisposable
    {
        readonly ISubscriber<CombatRequested> _combatRequested;
        readonly ISubscriber<ResultsClosed> _resultsClosed;
        readonly IStartCombatUseCase _startCombat;
        IDisposable? _subs;
        public bool InCombat { get; private set; }

        public GameFlow(ISubscriber<CombatRequested> combatRequested, ISubscriber<ResultsClosed> resultsClosed, IStartCombatUseCase startCombat)
        { _combatRequested = combatRequested; _resultsClosed = resultsClosed; _startCombat = startCombat; }

        public void Start()
        {
            var bag = DisposableBag.CreateBuilder();
            _combatRequested.Subscribe(r => EnterCombatAsync(r).Forget()).AddTo(bag);
            _resultsClosed.Subscribe(_ => ExitCombatAsync().Forget()).AddTo(bag);
            _subs = bag.Build();
        }

        public async UniTask EnterBoardAsync()
        {
            if (!SceneManager.GetSceneByName("Board").isLoaded)
                await SceneManager.LoadSceneAsync("Board", LoadSceneMode.Additive);
        }

        async UniTask EnterCombatAsync(CombatRequested request)
        {
            if (InCombat) return;
            InCombat = true;
            await SceneManager.LoadSceneAsync("Combat", LoadSceneMode.Additive); // le scope Combat s'enregistre comme enfant de Board
            _startCombat.Execute(request);
        }

        async UniTask ExitCombatAsync()
        {
            if (!InCombat) return;
            await SceneManager.UnloadSceneAsync("Combat");
            InCombat = false;
        }

        public void Dispose() => _subs?.Dispose();
    }
}
```
```csharp
// Composition/PrototypeBootstrap.cs
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Board;
using N29.HeroesDawn.Core;
using VContainer.Unity;

namespace N29.HeroesDawn.Composition
{
    public sealed class PrototypeBootstrap : IAsyncStartable
    {
        readonly GameFlow _flow; readonly IOpenZoneUseCase _open;
        public PrototypeBootstrap(GameFlow flow, IOpenZoneUseCase open) { _flow = flow; _open = open; }
        public async UniTask StartAsync(System.Threading.CancellationToken ct)
        {
            await _flow.EnterBoardAsync();
            await UniTask.NextFrame(ct);               // laisse le BoardLifetimeScope démarrer ses présentateurs
            _open.Execute(new ZoneId("zone_ny_1_1"));
        }
    }
}
```
```csharp
// Composition/BoardLifetimeScope.cs
using N29.HeroesDawn.Presentation.Board;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace N29.HeroesDawn.Composition
{
    public sealed class BoardLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder b)
        {
            b.RegisterComponentInHierarchy<BoardWorldView>();
            b.RegisterComponentInHierarchy<BoardCameraController>();
            b.RegisterComponentInHierarchy<PawnView>();
            b.RegisterComponentInHierarchy<BoardHudView>().As<IBoardHudView>();
            b.RegisterComponentInHierarchy<NarrativeOverlayView>();
            b.RegisterComponentInHierarchy<RestPromptView>();
            b.RegisterComponentInHierarchy<DemonstrationView>();
            b.RegisterComponentInHierarchy<ZoneCompleteView>();
            b.RegisterEntryPoint<BoardPresenter>();
            b.RegisterEntryPoint<CaseInteractionPresenter>();
        }
    }
}
```
```csharp
// Composition/CombatLifetimeScope.cs
using N29.HeroesDawn.Presentation.Combat;
using VContainer;
using VContainer.Unity;

namespace N29.HeroesDawn.Composition
{
    public sealed class CombatLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder b)
        {
            b.RegisterComponentInHierarchy<CombatDirector>();
            b.RegisterComponentInHierarchy<CombatHudView>().As<ICombatHudView>();
            b.RegisterComponentInHierarchy<CombatResultsView>();
            b.RegisterEntryPoint<CombatPresenter>(); // IStartable + ITickable : fait aussi avancer le CombatRunner
        }
    }
}
```
- [ ] **Étape 4 : Scènes**
  - `Boot.unity` : GameObject `Root` avec `RootLifetimeScope` (UiTheme, et les objets `ToastView` et `ModalView` enfants d'un Canvas « Global » en ordre de tri 100) ; caméra désactivée ; `EventSystem` avec **Input System UI Input Module**.
  - `Board.unity` : `BoardLifetimeScope` (champ *Parent* = `RootLifetimeScope` via le type de parent) ; caméra orthographique « BoardCamera » ; `BoardWorld` ; Canvas HUD (ordre 10) ; Canvas overlays (ordre 20).
  - `Combat.unity` : `CombatLifetimeScope` (*Parent* = `BoardLifetimeScope`) ; caméra « CombatCamera » (profondeur supérieure, fond opaque) ; `Arena` ; Canvas HUD combat (ordre 30) ; Canvas résultats (ordre 40).
  - `File ▸ Build Profiles` : Boot (0), Board (1), Combat (2).
- [ ] **Étape 5 : Vérifier** — PlayMode `BootFlowTests` vert ; en Play depuis Boot, la zone s'affiche (après les tâches 13 à 15).
- [ ] **Étape 6 : Commit** — `git commit -am "feat(composition): scopes VContainer, scènes, GameFlow"`

---

### Tâche 13 : Fondations de l'interface

**Fichiers :** `Presentation/Common/*` (voir carte) ; `Settings/UiTheme.asset` ; `Presentation/Common/Localization/fr.json` ; prefabs `Prefabs/UI/ComicButton.prefab`, `Toast.prefab`, `Modal.prefab` · Test `Assets/Tests/Domain/Presentation/LocalLocalizationTests.cs`

**Interfaces :**
- Produit :
  - `UiTheme : ScriptableObject` — `Color FamilyColor(Family)`, `Sprite FamilyIcon(Family)`, `Color Primary, Danger, Success, Surface, SurfaceDark, TextLight, TextDark`, `TMP_FontAsset TitleFont, BodyFont`, `float PressScale = 0.92f`, `float PressDuration = 0.08f`, `Sprite CaseIcon(CaseType)`.
  - `ILocalization { string Get(string key); string Format(string key, params object[] args); }` ; `LocalLocalization` lit `Resources/Localization/fr.json` (dictionnaire plat) ; clé absente → `⟦key⟧` + log d'avertissement.
  - `SafeAreaFitter` : ajuste le `RectTransform` à `Screen.safeArea` à chaque changement d'orientation/résolution.
  - `ComicButton : Button` : enfoncement (`PressScale`, PrimeTween), relâchement élastique, désactivation grisée, événement `Clicked` (C#), anti-double-clic (`MinInterval = 0.25 s`).
  - `IToastService.Show(string text, Color accent, float seconds = 1.6f)` : toasts empilés (max 3) en haut au centre.
  - `IModalService.Confirm(string titleKey, string bodyKey, string confirmKey, string cancelKey) : UniTask<bool>` ; `Info(...) : UniTask`.
  - `InputBlocker` : `IDisposable Block()` compte les blocages ; un panneau transparent plein écran intercepte les clics tant que le compteur > 0.

- [ ] **Étape 1 : Test en échec**
```csharp
using N29.HeroesDawn.Presentation.Common;
using NUnit.Framework;

public class LocalLocalizationTests
{
    [Test]
    public void KnownKey_Returns_Text_And_Format_Works()
    {
        var loc = new LocalLocalization("{\"ui.dice.roll\":\"Lancer\",\"ui.pm\":\"{0} PM\"}");
        Assert.That(loc.Get("ui.dice.roll"), Is.EqualTo("Lancer"));
        Assert.That(loc.Format("ui.pm", 3), Is.EqualTo("3 PM"));
    }

    [Test]
    public void MissingKey_IsVisible()
        => Assert.That(new LocalLocalization("{}").Get("nope"), Is.EqualTo("⟦nope⟧"));
}
```
- [ ] **Étape 2 : Vérifier l'échec.**
- [ ] **Étape 3 : Implémentation**
```csharp
// Presentation/Common/ILocalization.cs + LocalLocalization.cs
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
namespace N29.HeroesDawn.Presentation.Common
{
    public interface ILocalization { string Get(string key); string Format(string key, params object[] args); }

    public sealed class LocalLocalization : ILocalization
    {
        readonly Dictionary<string, string> _table;
        [VContainer.Inject]
        public LocalLocalization() : this(Resources.Load<TextAsset>("Localization/fr")?.text ?? "{}") { }
        public LocalLocalization(string json) { _table = JsonConvert.DeserializeObject<Dictionary<string, string>>(json) ?? new Dictionary<string, string>(); }
        public string Get(string key)
        {
            if (_table.TryGetValue(key, out var v)) return v;
            Debug.LogWarning($"[Loc] Clé manquante : {key}");
            return $"⟦{key}⟧";
        }
        public string Format(string key, params object[] args) => string.Format(Get(key), args);
    }
}
```
Le fichier de textes est `Assets/_Project/Presentation/Common/Resources/Localization/fr.json`.

```csharp
// Presentation/Common/SafeAreaFitter.cs
using UnityEngine;
namespace N29.HeroesDawn.Presentation.Common
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        Rect _last; Vector2Int _lastSize;
        void OnEnable() => Apply();
        void Update() { if (Screen.safeArea != _last || _lastSize.x != Screen.width || _lastSize.y != Screen.height) Apply(); }
        void Apply()
        {
            _last = Screen.safeArea; _lastSize = new Vector2Int(Screen.width, Screen.height);
            var rt = (RectTransform)transform;
            var min = _last.position; var max = _last.position + _last.size;
            min.x /= Screen.width; min.y /= Screen.height; max.x /= Screen.width; max.y /= Screen.height;
            rt.anchorMin = min; rt.anchorMax = max; rt.offsetMin = rt.offsetMax = Vector2.zero;
        }
    }
}
```
```csharp
// Presentation/Common/ComicButton.cs
using System;
using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace N29.HeroesDawn.Presentation.Common
{
    public sealed class ComicButton : Button
    {
        [SerializeField] float _pressScale = 0.92f;
        [SerializeField] float _minInterval = 0.25f;
        float _lastClick = -10f;
        public event Action? Clicked;

        protected override void Awake() { base.Awake(); onClick.AddListener(OnClickInternal); }

        void OnClickInternal()
        {
            if (Time.unscaledTime - _lastClick < _minInterval) return; // anti-double tap
            _lastClick = Time.unscaledTime;
            Clicked?.Invoke();
        }

        public override void OnPointerDown(PointerEventData e)
        {
            base.OnPointerDown(e);
            if (interactable) Tween.Scale(transform, _pressScale, 0.08f, Ease.OutQuad, useUnscaledTime: true);
        }

        public override void OnPointerUp(PointerEventData e)
        {
            base.OnPointerUp(e);
            Tween.Scale(transform, 1f, 0.25f, Ease.OutBack, useUnscaledTime: true);
        }
    }
}
```
```csharp
// Presentation/Common/InputBlocker.cs
using System;
using UnityEngine;
namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Panneau transparent plein écran (raycast target) activé tant qu'au moins un blocage est actif.</summary>
    public sealed class InputBlocker : MonoBehaviour
    {
        [SerializeField] GameObject _panel = null!;
        int _count;
        public bool IsBlocked => _count > 0;
        public IDisposable Block() { _count++; _panel.SetActive(true); return new Handle(this); }
        void Release() { _count = Mathf.Max(0, _count - 1); _panel.SetActive(_count > 0); }
        sealed class Handle : IDisposable { InputBlocker? _b; public Handle(InputBlocker b) { _b = b; } public void Dispose() { _b?.Release(); _b = null; } }
    }
}
```
```csharp
// Presentation/Common/IToastService.cs + ToastView.cs
using System.Collections.Generic;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace N29.HeroesDawn.Presentation.Common
{
    public interface IToastService { void Show(string text, Color accent, float seconds = 1.6f); }

    public sealed class ToastView : MonoBehaviour, IToastService
    {
        [SerializeField] RectTransform _container = null!;
        [SerializeField] GameObject _toastPrefab = null!; // Image (fond) + Image « Accent » + TMP_Text
        readonly Queue<GameObject> _alive = new Queue<GameObject>();

        public void Show(string text, Color accent, float seconds = 1.6f)
        {
            if (_alive.Count >= 3) Destroy(_alive.Dequeue());
            var go = Instantiate(_toastPrefab, _container);
            go.GetComponentInChildren<TMP_Text>().text = text;
            go.transform.Find("Accent").GetComponent<Image>().color = accent;
            var cg = go.GetComponent<CanvasGroup>();
            cg.alpha = 0f;
            _alive.Enqueue(go);
            Sequence.Create()
                .Group(Tween.Alpha(cg, 1f, 0.15f))
                .Group(Tween.PunchScale(go.transform, Vector3.one * 0.15f, 0.3f))
                .ChainDelay(seconds)
                .Chain(Tween.Alpha(cg, 0f, 0.2f))
                .OnComplete(() => { if (go != null) Destroy(go); });
        }
    }
}
```
```csharp
// Presentation/Common/IModalService.cs + ModalView.cs
using Cysharp.Threading.Tasks;
using PrimeTween;
using TMPro;
using UnityEngine;
namespace N29.HeroesDawn.Presentation.Common
{
    public interface IModalService
    {
        UniTask<bool> Confirm(string titleKey, string bodyKey, string confirmKey, string cancelKey);
        UniTask Info(string titleKey, string bodyKey, string okKey);
    }

    public sealed class ModalView : MonoBehaviour, IModalService
    {
        [SerializeField] CanvasGroup _root = null!;
        [SerializeField] RectTransform _card = null!;
        [SerializeField] TMP_Text _title = null!, _body = null!, _confirmLabel = null!, _cancelLabel = null!;
        [SerializeField] ComicButton _confirm = null!, _cancel = null!;
        [VContainer.Inject] ILocalization _loc = null!;
        UniTaskCompletionSource<bool>? _pending;

        void Awake() { _root.gameObject.SetActive(false); _confirm.Clicked += () => Close(true); _cancel.Clicked += () => Close(false); }

        public UniTask<bool> Confirm(string titleKey, string bodyKey, string confirmKey, string cancelKey)
        {
            _pending?.TrySetResult(false);
            _pending = new UniTaskCompletionSource<bool>();
            _title.text = _loc.Get(titleKey); _body.text = _loc.Get(bodyKey);
            _confirmLabel.text = _loc.Get(confirmKey); _cancelLabel.text = _loc.Get(cancelKey);
            _cancel.gameObject.SetActive(true);
            Open();
            return _pending.Task;
        }

        public async UniTask Info(string titleKey, string bodyKey, string okKey)
        {
            await Confirm(titleKey, bodyKey, okKey, okKey);
        }

        void Open()
        {
            _root.gameObject.SetActive(true); _root.alpha = 0f; _card.localScale = Vector3.one * 0.85f;
            Tween.Alpha(_root, 1f, 0.15f); Tween.Scale(_card, 1f, 0.25f, Ease.OutBack);
        }

        void Close(bool result)
        {
            Tween.Alpha(_root, 0f, 0.12f).OnComplete(() => _root.gameObject.SetActive(false));
            var p = _pending; _pending = null; p?.TrySetResult(result);
        }
    }
}
```
```csharp
// Presentation/Common/UiTheme.cs
using N29.HeroesDawn.Definitions;
using TMPro;
using UnityEngine;
namespace N29.HeroesDawn.Presentation.Common
{
    [CreateAssetMenu(menuName = "Heroes Dawn/UI Theme")]
    public sealed class UiTheme : ScriptableObject
    {
        public Color Matter = new Color32(0xF2, 0xA9, 0x3B, 0xFF), Elemental = new Color32(0x3B, 0xC7, 0xF2, 0xFF),
                     PhysicalBio = new Color32(0xE5, 0x48, 0x4D, 0xFF), Perception = new Color32(0x9B, 0x6B, 0xF2, 0xFF);
        public Sprite MatterIcon = null!, ElementalIcon = null!, PhysicalBioIcon = null!, PerceptionIcon = null!;
        public Color Primary = new Color32(0xFF, 0xD2, 0x3F, 0xFF), Danger = new Color32(0xE5, 0x48, 0x4D, 0xFF),
                     Success = new Color32(0x4C, 0xC9, 0x6B, 0xFF), Surface = new Color32(0x1E, 0x22, 0x2B, 0xF2),
                     SurfaceDark = new Color32(0x10, 0x12, 0x18, 0xFF), TextLight = Color.white, TextDark = new Color32(0x10, 0x12, 0x18, 0xFF);
        public TMP_FontAsset TitleFont = null!, BodyFont = null!;
        public Sprite[] CaseIcons = new Sprite[7]; // indexé par CaseType

        public Color FamilyColor(Family f) => f switch { Family.Matter => Matter, Family.Elemental => Elemental, Family.PhysicalBio => PhysicalBio, _ => Perception };
        public Sprite FamilyIcon(Family f) => f switch { Family.Matter => MatterIcon, Family.Elemental => ElementalIcon, Family.PhysicalBio => PhysicalBioIcon, _ => PerceptionIcon };
        public Sprite CaseIcon(CaseType t) => CaseIcons[(int)t];
    }
}
```
- [ ] **Étape 4 : Contenu visuel greybox** — dans `Art/UI/Greybox/` : cadres « comics » (rectangle blanc, contour noir 6 px, ombre portée décalée 8 px), 4 icônes de famille (poing = Physique, cristal = Matière, éclair = Élémentaire, œil = Perception), 7 icônes de cases (drapeau entrée, point vide, poings croisés combat, manette mini-jeu, bulle événement, coffre ressource, lit repos). Polices : une police display grasse libre (ex. *Bangers*, OFL) pour les titres et une sans-serif lisible (ex. *Inter*, OFL) pour le corps ; générer les TMP Font Assets (FR/EN/ES/PT/DE : jeu de caractères Latin-1 + Latin Extended-A).
- [ ] **Étape 5 : `fr.json` initial** — toutes les clés utilisées par les tâches 13 à 18 (extrait) :
```json
{
  "ui.dice.roll": "LANCER !", "ui.dice.bonus": "Dé bonus", "ui.pm_in_hand": "{0} PM", "ui.end_move": "Terminer",
  "ui.back_hq": "Retour QG", "ui.back_hq.title": "Quitter la zone ?", "ui.back_hq.body": "Le brouillard se refermera. Tes objectifs remplis sont conservés.",
  "ui.confirm": "Confirmer", "ui.cancel": "Annuler", "ui.ok": "OK", "ui.objectives": "Objectifs",
  "ui.resource.toast": "+{0} Crédits · +{1} PR", "ui.fog.stop": "Nouveau secteur en vue !",
  "ui.rest.title": "Planque", "ui.rest.body": "Ton équipe peut souffler : -30 % de fatigue.", "ui.rest.take": "Se reposer", "ui.skip": "Passer",
  "ui.zone_complete": "ZONE SÉCURISÉE !", "ui.continue": "Continuer",
  "ui.hint.choose_case": "Choisis une case en surbrillance", "ui.back_hq.proto": "Le QG arrive au prochain jalon !",
  "errors.board.not_allowed": "Action impossible pour le moment",
  "combat.auto": "AUTO", "combat.speed": "x{0}", "combat.retreat": "Retraite", "combat.retreat.title": "Battre en retraite ?",
  "combat.retreat.body": "L'équipe recule d'une case et prend de la fatigue.", "combat.combo": "COMBO !", "combat.choose_target": "Choisis une cible",
  "combat.victory": "VICTOIRE !", "combat.defeat": "DÉFAITE…", "combat.fatigue": "Fatigue",
  "minigame.demo.title": "Démonstration", "minigame.demo.hint": "Touche quand le curseur est dans la zone !",
  "zones.ny_1_1.name": "Red Hook — les quais",
  "objectives.ny11.warehouse": "Chasser les pillards de l'entrepôt", "objectives.ny11.generator": "Relancer le générateur", "objectives.ny11.crane": "Atteindre la grue de contrôle",
  "members.ancre.name": "Ancre", "members.verre.name": "Verre", "members.givre.name": "Givre", "members.echo.name": "Écho",
  "members.arc.name": "Arc", "members.rempart.name": "Rempart", "members.seve.name": "Sève", "members.etincelle.name": "Étincelle",
  "enemies.thug.name": "Voyou néo-méta", "enemies.brute.name": "Gros bras", "enemies.street_pyro.name": "Pyro de rue", "enemies.brick.name": "Brick",
  "combos.weak_point.name": "Point faible", "combos.triage.name": "Triage", "combos.trap.name": "Piège", "combos.forged_armor.name": "Armure forgée",
  "combos.charged_projectiles.name": "Projectiles chargés", "combos.elemental_body.name": "Corps élémentaire",
  "combos.resonance_matter.name": "Résonance", "combos.resonance_elemental.name": "Résonance", "combos.resonance_physical.name": "Résonance", "combos.resonance_perception.name": "Résonance",
  "narrative.generic.title": "Coupure de courant", "narrative.generic.body": "Un quartier replonge dans le noir. Les habitants paniquent.",
  "narrative.generic.a": "Rétablir le courant", "narrative.generic.b": "Laisser faire les services"
}
```
Ajouter les noms de compétences (`skills.<id>.name`) pour les 24 compétences du prototype, et les descriptions courtes (`skills.<id>.desc`, ≤ 60 caractères) : elles s'affichent dans le panneau de compétences.
- [ ] **Étape 6 : Vérifier** — tests verts ; **simulateur d'appareil** (Window ▸ General ▸ Device Simulator) : iPhone 15 Pro (encoche), Galaxy S23 Ultra, écran 21:9 ; un panneau test avec `SafeAreaFitter` ne passe jamais sous l'encoche (Review Focus 2).
- [ ] **Étape 7 : Commit** — `git commit -am "feat(ui): thème, localisation, zone sûre, boutons, toasts, modales"`

---

### Tâche 14 : Plateau — monde (cases, liens, brouillard, pion, caméra)

**Fichiers :** `Presentation/Board/World/*` ; prefabs `Prefabs/Board/Node.prefab`, `Link.prefab`, `Pawn.prefab`

**Interfaces :**
- Consomme : `GameSession.Run` (lecture : `Graph`, `Fog`, `Position`, `Destinations`), `UiTheme`.
- Produit :
  - `BoardWorldView` : `void Build(BoardGraph graph)`, `void SetRevealed(IEnumerable<CaseId> ids, bool animate)`, `void SetDestinations(IReadOnlyCollection<CaseId> ids)`, `void MarkCollected(CaseId)`, `void MarkDefeated(CaseId)`, `void MarkObjective(CaseId, bool done)`, `Vector3 WorldPos(CaseId)`, `event Action<CaseId> NodeTapped`.
  - `NodeView` : état visuel { Caché, Visible, Destination, Visitée, Objectif } ; icône du type ; anneau pulsé quand destination ; affiche le **nombre de pas** (coût du chemin) sur les destinations.
  - `PawnView` : `UniTask MoveAlong(IReadOnlyList<Vector3> points, float secondsPerStep)` (sauts « bond » de 0,18 s par case, petit écrasement à l'atterrissage), `void Teleport(Vector3)`.
  - `BoardCameraController` : glisser pour déplacer, pincer / molette pour zoomer (taille orthographique 4 à 9), `UniTask FocusOn(Vector3, float duration)`, bornes = boîte englobante des cases révélées + marge 1,5.

- [ ] **Étape 1 : Prefabs**
  - `Node` (≈ 1 unité monde) : `SpriteRenderer` disque (couleur `Surface`), enfant `Icon` (SpriteRenderer), enfant `Ring` (SpriteRenderer anneau jaune `Primary`, désactivé), enfant `Cost` (TextMeshPro monde, taille 3), enfant `Fog` (nuage gris 60 % d'opacité, actif par défaut), `CircleCollider2D` rayon 0,6 (cible tactile ≥ 120 px au zoom par défaut).
  - `Link` : `LineRenderer` 0,12 d'épaisseur, pointillés quand l'une des extrémités est cachée.
  - `Pawn` : portrait circulaire du leader de l'équipe avec contour de sa couleur de famille, ombre.
- [ ] **Étape 2 : Implémentation**
```csharp
// Presentation/Board/World/NodeView.cs
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using PrimeTween;
using TMPro;
using UnityEngine;
namespace N29.HeroesDawn.Presentation.Board
{
    public sealed class NodeView : MonoBehaviour
    {
        [SerializeField] SpriteRenderer _disc = null!, _icon = null!, _ring = null!, _fog = null!;
        [SerializeField] TextMeshPro _cost = null!;
        Tween _pulse;
        public CaseId Id { get; private set; }
        public CaseType Type { get; private set; }

        public void Init(CaseId id, CaseType type, Sprite icon) { Id = id; Type = type; _icon.sprite = icon; SetDestination(false, 0); }

        public void SetRevealed(bool revealed, bool animate)
        {
            if (!animate) { _fog.gameObject.SetActive(!revealed); _icon.enabled = revealed; return; }
            if (!revealed) { _fog.gameObject.SetActive(true); _icon.enabled = false; return; }
            _icon.enabled = true;
            Tween.Alpha(_fog, 0f, 0.35f).OnComplete(() => { _fog.gameObject.SetActive(false); _fog.color = new Color(1, 1, 1, 0.6f); });
            Tween.PunchScale(transform, Vector3.one * 0.2f, 0.35f);
        }

        public void SetDestination(bool on, int cost)
        {
            _ring.gameObject.SetActive(on);
            _cost.gameObject.SetActive(on);
            _cost.text = cost.ToString();
            _pulse.Stop();
            if (on) _pulse = Tween.Scale(_ring.transform, 1.15f, 0.6f, Ease.InOutSine, cycles: -1, cycleMode: CycleMode.Yoyo);
        }

        public void SetVisited() => _icon.color = new Color(1, 1, 1, 0.35f);   // ressource ramassée, combat gagné
        public void SetObjective(bool done) => _disc.color = done ? new Color32(0x4C, 0xC9, 0x6B, 0xFF) : new Color32(0xFF, 0xD2, 0x3F, 0xFF);
    }
}
```
```csharp
// Presentation/Board/World/BoardWorldView.cs
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Presentation.Common;
using UnityEngine;
using UnityEngine.InputSystem;
namespace N29.HeroesDawn.Presentation.Board
{
    public sealed class BoardWorldView : MonoBehaviour
    {
        [SerializeField] NodeView _nodePrefab = null!;
        [SerializeField] LineRenderer _linkPrefab = null!;
        [SerializeField] Transform _nodesRoot = null!, _linksRoot = null!;
        [SerializeField] float _spacing = 1.6f;
        [SerializeField] Camera _camera = null!;
        [VContainer.Inject] UiTheme _theme = null!;
        [VContainer.Inject] BoardCameraController _cameraController = null!;
        readonly Dictionary<CaseId, NodeView> _nodes = new Dictionary<CaseId, NodeView>();
        readonly List<(LineRenderer line, CaseId a, CaseId b)> _links = new List<(LineRenderer, CaseId, CaseId)>();
        readonly HashSet<CaseId> _currentDestinations = new HashSet<CaseId>();
        public event Action<CaseId>? NodeTapped;
        public bool InputEnabled { get; set; } = true;

        public void Build(BoardGraph g)
        {
            foreach (Transform t in _nodesRoot) Destroy(t.gameObject);
            foreach (Transform t in _linksRoot) Destroy(t.gameObject);
            _nodes.Clear(); _links.Clear();
            foreach (var n in g.Zone.Nodes)
            {
                var v = Instantiate(_nodePrefab, new Vector3(n.X * _spacing, n.Y * _spacing, 0), Quaternion.identity, _nodesRoot);
                v.Init(n.Id, n.Type, _theme.CaseIcon(n.Type));
                v.SetRevealed(false, false);
                _nodes.Add(n.Id, v);
            }
            foreach (var l in g.Zone.Links)
            {
                var line = Instantiate(_linkPrefab, _linksRoot);
                line.positionCount = 2;
                line.SetPosition(0, WorldPos(l.A)); line.SetPosition(1, WorldPos(l.B));
                _links.Add((line, l.A, l.B));
            }
        }

        public Vector3 WorldPos(CaseId id) => _nodes[id].transform.position;

        public void SetRevealed(IEnumerable<CaseId> ids, bool animate)
        {
            foreach (var id in ids) _nodes[id].SetRevealed(true, animate);
            foreach (var (line, a, b) in _links)
            {
                bool bothVisible = !_nodes[a].transform.Find("Fog").gameObject.activeSelf && !_nodes[b].transform.Find("Fog").gameObject.activeSelf;
                line.enabled = true;
                var c = line.startColor; c.a = bothVisible ? 1f : 0.25f; line.startColor = line.endColor = c;
            }
        }

        public void SetDestinations(IReadOnlyDictionary<CaseId, MovePlan> plans)
        {
            foreach (var id in _currentDestinations) _nodes[id].SetDestination(false, 0);
            _currentDestinations.Clear();
            foreach (var kv in plans) { _nodes[kv.Key].SetDestination(true, kv.Value.Cost); _currentDestinations.Add(kv.Key); }
        }

        public void MarkVisited(CaseId id) => _nodes[id].SetVisited();
        public void MarkObjective(CaseId id, bool done) => _nodes[id].SetObjective(done);

        void Update()
        {
            if (!InputEnabled || Pointer.current == null || !Pointer.current.press.wasReleasedThisFrame) return;
            if (_cameraController.WasDrag) return;                                              // un glisser n'est pas un tap
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return; // l'UI a priorité
            var world = _camera.ScreenToWorldPoint(Pointer.current.position.ReadValue());
            var hit = Physics2D.OverlapPoint(world);
            if (hit != null && hit.TryGetComponent<NodeView>(out var node) && _currentDestinations.Contains(node.Id)) NodeTapped?.Invoke(node.Id);
        }
    }
}
```
```csharp
// Presentation/Board/World/PawnView.cs
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;
namespace N29.HeroesDawn.Presentation.Board
{
    public sealed class PawnView : MonoBehaviour
    {
        [SerializeField] Transform _visual = null!;
        public void Teleport(Vector3 p) => transform.position = p;

        public async UniTask MoveAlong(IReadOnlyList<Vector3> points, float secondsPerStep)
        {
            foreach (var p in points)
            {
                await Tween.Position(transform, p, secondsPerStep, Ease.OutQuad).ToUniTask();
                await Tween.PunchScale(_visual, new Vector3(0.15f, -0.15f, 0), 0.12f).ToUniTask(); // atterrissage
            }
        }
    }
}
```
```csharp
// Presentation/Board/World/BoardCameraController.cs
using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
namespace N29.HeroesDawn.Presentation.Board
{
    public sealed class BoardCameraController : MonoBehaviour
    {
        [SerializeField] Camera _cam = null!;
        [SerializeField] float _minSize = 4f, _maxSize = 9f, _dragThresholdPx = 20f;
        Bounds _bounds = new Bounds(Vector3.zero, Vector3.one * 100);
        Vector2 _pressPos; Vector3 _lastWorld; bool _pressed;
        public bool WasDrag { get; private set; }

        void OnEnable() => EnhancedTouchSupport.Enable();

        public void SetBounds(Bounds b) { b.Expand(3f); _bounds = b; Clamp(); }
        public UniTask FocusOn(Vector3 p, float duration) => Tween.Position(_cam.transform, new Vector3(p.x, p.y, _cam.transform.position.z), duration, Ease.OutCubic).ToUniTask();

        void Update()
        {
            if (Touch.activeTouches.Count == 2) { Pinch(); return; }
            var pointer = Pointer.current; if (pointer == null) return;
            var screen = pointer.position.ReadValue();
            if (pointer.press.wasPressedThisFrame) { _pressed = true; WasDrag = false; _pressPos = screen; _lastWorld = _cam.ScreenToWorldPoint(screen); }
            else if (_pressed && pointer.press.isPressed)
            {
                if ((screen - _pressPos).sqrMagnitude > _dragThresholdPx * _dragThresholdPx) WasDrag = true;
                if (WasDrag) { var w = _cam.ScreenToWorldPoint(screen); _cam.transform.position += _lastWorld - w; Clamp(); }
            }
            else if (pointer.press.wasReleasedThisFrame) _pressed = false;
            if (Mouse.current != null) { float scroll = Mouse.current.scroll.ReadValue().y; if (scroll != 0) Zoom(-scroll * 0.005f); }
        }

        void Pinch()
        {
            var a = Touch.activeTouches[0]; var b = Touch.activeTouches[1];
            float now = (a.screenPosition - b.screenPosition).magnitude;
            float before = ((a.screenPosition - a.delta) - (b.screenPosition - b.delta)).magnitude;
            Zoom((before - now) * 0.01f);
            WasDrag = true;
        }

        void Zoom(float delta) { _cam.orthographicSize = Mathf.Clamp(_cam.orthographicSize + delta, _minSize, _maxSize); Clamp(); }
        void Clamp()
        {
            var p = _cam.transform.position;
            p.x = Mathf.Clamp(p.x, _bounds.min.x, _bounds.max.x); p.y = Mathf.Clamp(p.y, _bounds.min.y, _bounds.max.y);
            _cam.transform.position = p;
        }
    }
}
```
- [ ] **Étape 3 : Vérifier (manuel)** — scène Board seule avec un script de test temporaire qui appelle `Build` + `SetRevealed(all)` sur `zone_ny_1_1` : 25 cases lisibles, liens nets, zoom 4-9 fluide à la souris et au pincement (Device Simulator), glisser ne déclenche pas de tap.
- [ ] **Étape 4 : Commit** — `git commit -am "feat(board-ui): rendu du plateau, brouillard, pion, caméra"`

---

### Tâche 15 : Plateau — HUD, présentateur, cases

**Fichiers :** `Presentation/Board/Hud/*`, `Cases/*`, `Presenters/*` ; prefab/UI dans `Board.unity` · Test `Assets/Tests/PlayMode/BoardLoopTests.cs`

**Interfaces :**
- Produit :
  - `IBoardHudView` :
    - `event Action RollClicked, BonusDieClicked, EndMoveClicked, BackClicked, ObjectivesClicked;`
    - `void SetRollState(bool canRoll)` — gros bouton **« LANCER ! »** en bas à droite (260 × 260 px), pulsation douce quand disponible ;
    - `UniTask PlayDice(int value, bool bonus)` — dé 2D qui roule 0,5 s puis s'arrête sur la face (faces 1-6 en sprites), gros chiffre qui rebondit ;
    - `void SetPmInHand(int pm, bool canEndMove)` — pastille « 3 PM » au-dessus du bouton + bouton « Terminer » ;
    - `void SetBonusGauge(int storedPm, int pmPerDie, int bonusDice, int maxDice)` — 6 segments qui se remplissent + 2 emplacements de dés ; bouton « Dé bonus » actif si `bonusDice > 0` ;
    - `void SetObjectives(IReadOnlyList<(string label, bool done)> items)` — panneau repliable en haut à gauche (3 lignes, case cochée animée) ;
    - `void SetWallet(int credits, int pr)` — en haut à droite, compteurs qui défilent ;
    - `void SetZoneTitle(string title)` ;
    - `void ShowHint(string text)` — bandeau d'aide contextuel en bas au centre (« Choisis une case en surbrillance »).
  - `BoardPresenter : IStartable, IDisposable` — orchestre vues du monde + HUD ; **verrouille les entrées** pendant les animations (Review Focus 1).
  - `CaseInteractionPresenter` — ouvre la bonne vue selon `CaseTriggered` (Événement, Repos, Mini-jeu) et appelle `IResolveCaseUseCase`.
  - `NarrativeOverlayView.Show(title, body, choiceA, choiceB) : UniTask<int>` ; `RestPromptView.Ask() : UniTask<bool>` ; `DemonstrationView.Play(Color familyColor, float zoneWidth) : UniTask<bool>` ; `ZoneCompleteView.Show(IReadOnlyList<string> objectives) : UniTask`.

**Disposition du HUD (1920 × 1080, zone sûre) :**
```
┌─────────────────────────────────────────────────────────────────────┐
│ [≡ Objectifs 1/3 ▾]        RED HOOK — LES QUAIS        ◆ 12 750  ✦ 256│
│ ☐ Chasser les pillards                                               │
│ ☑ Relancer le générateur                                             │
│ ☐ Atteindre la grue                                                  │
│                                                                      │
│                         (plateau, caméra libre)                      │
│                                                                      │
│ [⌂ Retour QG]            « Choisis une case en surbrillance »  ▢▢▢▢▢▢ 🎲🎲│
│                                                        [3 PM][Terminer]│
│                                                             ( LANCER ! )│
└─────────────────────────────────────────────────────────────────────┘
```

- [ ] **Étape 1 : Test PlayMode en échec**
```csharp
// Assets/Tests/PlayMode/BoardLoopTests.cs
using System.Collections;
using System.Linq;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Domain.Board;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using VContainer.Unity;

public class BoardLoopTests
{
    GameSession Session => Object.FindAnyObjectByType<N29.HeroesDawn.Composition.RootLifetimeScope>().Container.Resolve(typeof(GameSession)) as GameSession;

    IEnumerator Boot()
    {
        SceneManager.LoadScene("Boot");
        for (int i = 0; i < 180 && (Session?.Run == null); i++) yield return null;
    }

    [UnityTest] // Review Focus 1 : spam du bouton pendant l'animation
    public IEnumerator SpammingRoll_RollsOnce()
    {
        yield return Boot();
        var hud = Object.FindAnyObjectByType<N29.HeroesDawn.Presentation.Board.BoardHudView>();
        hud.SimulateRollClickForTests(); hud.SimulateRollClickForTests(); hud.SimulateRollClickForTests();
        yield return new WaitForSeconds(1f);
        Assert.That(Session.Run!.Log.Count(a => a.Type == BoardActionType.Roll), Is.EqualTo(1));
    }

    [UnityTest] // Review Focus 5 : après une défaite, le dé est de nouveau disponible
    public IEnumerator AfterDefeat_RollIsAvailableAgain()
    {
        yield return Boot();
        // Force une défaite via la simulation : combat démarré puis retraite
        var root = Object.FindAnyObjectByType<N29.HeroesDawn.Composition.RootLifetimeScope>();
        var cmds = (N29.HeroesDawn.Application.Features.Combat.ICombatCommandsUseCase)root.Container.Resolve(typeof(N29.HeroesDawn.Application.Features.Combat.ICombatCommandsUseCase));
        Session.Run = ZoneRun.ForTests(Session.Content, Session.Run!.Graph.Zone, 5, 0, true, position: "n02");
        Session.Run.SetPmForTests(1);
        var move = (N29.HeroesDawn.Application.Features.Board.IMoveToUseCase)root.Container.Resolve(typeof(N29.HeroesDawn.Application.Features.Board.IMoveToUseCase));
        move.Execute(new N29.HeroesDawn.Core.CaseId("n03"));
        for (int i = 0; i < 120 && !SceneManager.GetSceneByName("Combat").isLoaded; i++) yield return null;
        cmds.Retreat();
        yield return new WaitForSeconds(0.5f);
        Object.FindAnyObjectByType<N29.HeroesDawn.Presentation.Combat.CombatResultsView>().SimulateContinueForTests();
        for (int i = 0; i < 120 && SceneManager.GetSceneByName("Combat").isLoaded; i++) yield return null;
        yield return new WaitForSeconds(0.5f);
        Assert.That(Session.Run.Phase, Is.EqualTo(ZoneRunPhase.AwaitingRoll));
        Assert.That(Session.Run.Position.Value, Is.EqualTo("n02"));
        Assert.That(Object.FindAnyObjectByType<N29.HeroesDawn.Presentation.Board.BoardHudView>().RollInteractable, Is.True);
    }
}
```
- [ ] **Étape 2 : Vérifier l'échec.**
- [ ] **Étape 3 : Implémentation du présentateur**
```csharp
// Presentation/Board/Presenters/BoardPresenter.cs
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Board;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Presentation.Common;
using UnityEngine;
using VContainer.Unity;

namespace N29.HeroesDawn.Presentation.Board
{
    public sealed class BoardPresenter : IStartable, IDisposable
    {
        readonly GameSession _s; readonly IBoardHudView _hud; readonly BoardWorldView _world; readonly PawnView _pawn;
        readonly BoardCameraController _cam; readonly ILocalization _loc; readonly IToastService _toast; readonly IModalService _modal; readonly UiTheme _theme;
        readonly IRollDiceUseCase _roll; readonly IUseBonusDieUseCase _bonus; readonly IMoveToUseCase _move; readonly IEndMoveUseCase _end;
        readonly ISubscriber<FogRevealed> _fog; readonly ISubscriber<DestinationsChanged> _dest; readonly ISubscriber<DiceRolled> _dice;
        readonly ISubscriber<PawnMoved> _moved; readonly ISubscriber<ResourceCollected> _res; readonly ISubscriber<DiceBankChanged> _bank;
        readonly ISubscriber<ObjectiveCompleted> _obj; readonly ISubscriber<WalletChanged> _wallet; readonly ISubscriber<SteppedBack> _back;
        readonly Queue<Func<UniTask>> _animations = new Queue<Func<UniTask>>();
        bool _animating; IDisposable? _subs;

        public BoardPresenter(GameSession s, IBoardHudView hud, BoardWorldView world, PawnView pawn, BoardCameraController cam, ILocalization loc,
            IToastService toast, IModalService modal, UiTheme theme, IRollDiceUseCase roll, IUseBonusDieUseCase bonus, IMoveToUseCase move, IEndMoveUseCase end,
            ISubscriber<FogRevealed> fog, ISubscriber<DestinationsChanged> dest, ISubscriber<DiceRolled> dice, ISubscriber<PawnMoved> moved,
            ISubscriber<ResourceCollected> res, ISubscriber<DiceBankChanged> bank, ISubscriber<ObjectiveCompleted> obj, ISubscriber<WalletChanged> wallet,
            ISubscriber<SteppedBack> back)
        {
            _s = s; _hud = hud; _world = world; _pawn = pawn; _cam = cam; _loc = loc; _toast = toast; _modal = modal; _theme = theme;
            _roll = roll; _bonus = bonus; _move = move; _end = end; _fog = fog; _dest = dest; _dice = dice; _moved = moved; _res = res;
            _bank = bank; _obj = obj; _wallet = wallet; _back = back;
        }

        public void Start()
        {
            _hud.RollClicked += () => Guard(() => _roll.Execute());
            _hud.BonusDieClicked += () => Guard(() => _bonus.Execute());
            _hud.EndMoveClicked += () => Guard(() => _end.Execute());
            _hud.BackClicked += () => OnBackAsync().Forget();
            _world.NodeTapped += id => Guard(() => _move.Execute(id));

            var bag = DisposableBag.CreateBuilder();
            _fog.Subscribe(e => Enqueue(() => OnFog(e))).AddTo(bag);
            _dice.Subscribe(e => Enqueue(() => _hud.PlayDice(e.Value, e.Bonus))).AddTo(bag);
            _moved.Subscribe(e => Enqueue(() => OnMoved(e))).AddTo(bag);
            _res.Subscribe(e => Enqueue(() => { _world.MarkVisited(e.Case); _toast.Show(_loc.Format("ui.resource.toast", e.Credits, e.Pr), _theme.Primary); return UniTask.CompletedTask; })).AddTo(bag);
            _dest.Subscribe(e => Enqueue(() => { RefreshControls(); return UniTask.CompletedTask; })).AddTo(bag);
            _bank.Subscribe(e => _hud.SetBonusGauge(e.StoredPm, _s.Content.Balance.PmPerBonusDie, e.BonusDice, _s.Content.Balance.MaxBonusDice)).AddTo(bag);
            _obj.Subscribe(_ => Enqueue(() => { RefreshObjectives(); return UniTask.CompletedTask; })).AddTo(bag);
            _wallet.Subscribe(e => _hud.SetWallet(e.Credits, e.Pr)).AddTo(bag);
            _back.Subscribe(e => Enqueue(async () => { await _pawn.MoveAlong(new[] { _world.WorldPos(e.To) }, 0.25f); RefreshControls(); })).AddTo(bag);
            _subs = bag.Build();

            if (_s.Run != null) Rebuild();
        }

        /// <summary>Appelé au premier FogRevealed d'une zone ouverte (ou au démarrage si la zone existe déjà).</summary>
        void Rebuild()
        {
            var run = _s.Run!;
            _worldBuilt = true;
            _world.Build(run.Graph);
            _world.SetRevealed(run.Fog.Revealed, false);
            foreach (var o in run.Graph.Zone.Objectives) _world.MarkObjective(o.Target, run.CompletedObjectives.Contains(o.Id));
            _pawn.Teleport(_world.WorldPos(run.Position));
            _hud.SetZoneTitle(_loc.Get(run.Graph.Zone.NameKey));
            _hud.SetWallet(_s.Wallet.Credits, _s.Wallet.Pr);
            _hud.SetBonusGauge(run.Dice.StoredPm, _s.Content.Balance.PmPerBonusDie, run.Dice.BonusDice, _s.Content.Balance.MaxBonusDice);
            RefreshObjectives(); RefreshControls(); UpdateCameraBounds();
            _cam.FocusOn(_world.WorldPos(run.Position), 0f).Forget();
        }

        async UniTask OnFog(FogRevealed e)
        {
            if (!_worldBuilt) { Rebuild(); return; }
            _world.SetRevealed(e.Cases, animate: true);
            UpdateCameraBounds();
            if (_s.Run!.PmInHand > 0) _hud.ShowHint(_loc.Get("ui.fog.stop"));
            await UniTask.Delay(250);
        }
        bool _worldBuilt;

        async UniTask OnMoved(PawnMoved e)
        {
            var pts = new List<Vector3>(); foreach (var id in e.Path) pts.Add(_world.WorldPos(id));
            _world.SetDestinations(new Dictionary<CaseId, N29.HeroesDawn.Domain.Board.MovePlan>());
            await UniTask.WhenAll(_pawn.MoveAlong(pts, 0.18f), _cam.FocusOn(pts[pts.Count - 1], 0.18f * pts.Count));
        }

        void RefreshControls()
        {
            var run = _s.Run!;
            bool idle = !_animating;
            _hud.SetRollState(idle && run.Phase == N29.HeroesDawn.Domain.Board.ZoneRunPhase.AwaitingRoll);
            _hud.SetPmInHand(run.PmInHand, idle && run.Phase == N29.HeroesDawn.Domain.Board.ZoneRunPhase.ChoosingDestination);
            _world.SetDestinations(run.Phase == N29.HeroesDawn.Domain.Board.ZoneRunPhase.ChoosingDestination ? run.Destinations : new Dictionary<CaseId, N29.HeroesDawn.Domain.Board.MovePlan>());
            _world.InputEnabled = idle;
            if (run.Phase == N29.HeroesDawn.Domain.Board.ZoneRunPhase.ChoosingDestination) _hud.ShowHint(_loc.Get("ui.hint.choose_case"));
        }

        void RefreshObjectives()
        {
            var run = _s.Run!; var items = new List<(string, bool)>();
            foreach (var o in run.Graph.Zone.Objectives)
            {
                bool done = run.CompletedObjectives.Contains(o.Id);
                items.Add((_loc.Get(o.NameKey), done));
                _world.MarkObjective(o.Target, done);
            }
            _hud.SetObjectives(items);
        }

        void UpdateCameraBounds()
        {
            var b = new Bounds(_world.WorldPos(_s.Run!.Position), Vector3.zero);
            foreach (var id in _s.Run.Fog.Revealed) b.Encapsulate(_world.WorldPos(id));
            _cam.SetBounds(b);
        }

        void Guard(Func<Result> action)
        {
            if (_animating) return; // Review Focus 1
            var r = action();
            if (r.IsFailure) _toast.Show(_loc.Get(r.Error!.LocKey), _theme.Danger);
        }

        void Enqueue(Func<UniTask> anim) { _animations.Enqueue(anim); if (!_animating) RunQueue().Forget(); }

        async UniTaskVoid RunQueue()
        {
            _animating = true; RefreshLocks();
            while (_animations.Count > 0) await _animations.Dequeue()();
            _animating = false; RefreshLocks();
        }

        void RefreshLocks() { if (_s.Run != null) RefreshControls(); }

        async UniTaskVoid OnBackAsync()
        {
            bool ok = await _modal.Confirm("ui.back_hq.title", "ui.back_hq.body", "ui.confirm", "ui.cancel");
            if (ok) _toast.Show(_loc.Get("ui.back_hq.proto"), _theme.Primary); // M1 : pas de QG ; message « Disponible au M2 »
        }

        public void Dispose() => _subs?.Dispose();
    }
}
```

- [ ] **Étape 4 : Implémentation de la vue HUD**
```csharp
// Presentation/Board/Hud/IBoardHudView.cs
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
namespace N29.HeroesDawn.Presentation.Board
{
    public interface IBoardHudView
    {
        event Action RollClicked, BonusDieClicked, EndMoveClicked, BackClicked;
        void SetRollState(bool canRoll);
        UniTask PlayDice(int value, bool bonus);
        void SetPmInHand(int pm, bool canEndMove);
        void SetBonusGauge(int storedPm, int pmPerDie, int bonusDice, int maxDice);
        void SetObjectives(IReadOnlyList<(string label, bool done)> items);
        void SetWallet(int credits, int pr);
        void SetZoneTitle(string title);
        void ShowHint(string text);
    }
}
```
```csharp
// Presentation/Board/Hud/BoardHudView.cs
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Common;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace N29.HeroesDawn.Presentation.Board
{
    public sealed class BoardHudView : MonoBehaviour, IBoardHudView
    {
        [SerializeField] ComicButton _roll = null!, _bonus = null!, _endMove = null!, _back = null!;
        [SerializeField] TMP_Text _pmLabel = null!, _zoneTitle = null!, _hint = null!, _credits = null!, _pr = null!;
        [SerializeField] GameObject _pmBadge = null!;
        [SerializeField] Image[] _gaugeSegments = new Image[6];
        [SerializeField] Image[] _bonusDiceSlots = new Image[2];
        [SerializeField] Image _die = null!; [SerializeField] Sprite[] _dieFaces = new Sprite[6];
        [SerializeField] TMP_Text _dieBigNumber = null!;
        [SerializeField] ObjectivesView _objectives = null!;
        [VContainer.Inject] ILocalization _loc = null!;
        [VContainer.Inject] UiTheme _theme = null!;
        Tween _rollPulse; int _shownCredits, _shownPr;

        public event Action? RollClicked, BonusDieClicked, EndMoveClicked, BackClicked;
        public bool RollInteractable => _roll.interactable;

        void Awake()
        {
            _roll.Clicked += () => RollClicked?.Invoke();
            _bonus.Clicked += () => BonusDieClicked?.Invoke();
            _endMove.Clicked += () => EndMoveClicked?.Invoke();
            _back.Clicked += () => BackClicked?.Invoke();
            _die.gameObject.SetActive(false); _hint.text = "";
        }

        void Start() => _roll.GetComponentInChildren<TMP_Text>().text = _loc.Get("ui.dice.roll");

        public void SetRollState(bool canRoll)
        {
            _roll.interactable = canRoll;
            _rollPulse.Stop(); _roll.transform.localScale = Vector3.one;
            if (canRoll) _rollPulse = Tween.Scale(_roll.transform, 1.06f, 0.7f, Ease.InOutSine, cycles: -1, cycleMode: CycleMode.Yoyo);
        }

        public async UniTask PlayDice(int value, bool bonus)
        {
            _die.gameObject.SetActive(true); _dieBigNumber.gameObject.SetActive(false);
            _die.color = bonus ? _theme.Primary : Color.white;
            float t = 0f;
            while (t < 0.5f) { _die.sprite = _dieFaces[UnityEngine.Random.Range(0, 6)]; _die.transform.Rotate(0, 0, 40); t += 0.05f; await UniTask.Delay(50); }
            _die.transform.rotation = Quaternion.identity; _die.sprite = _dieFaces[value - 1];
            _dieBigNumber.text = value.ToString(); _dieBigNumber.gameObject.SetActive(true);
            await Tween.PunchScale(_dieBigNumber.transform, Vector3.one * 0.6f, 0.35f).ToUniTask();
            await UniTask.Delay(250);
            _die.gameObject.SetActive(false); _dieBigNumber.gameObject.SetActive(false);
        }
        // Le tirage visuel utilise UnityEngine.Random : c'est de l'affichage, la valeur réelle vient du domaine.

        public void SetPmInHand(int pm, bool canEndMove)
        {
            _pmBadge.SetActive(pm > 0); _pmLabel.text = _loc.Format("ui.pm_in_hand", pm);
            _endMove.gameObject.SetActive(canEndMove && pm > 0);
        }

        public void SetBonusGauge(int storedPm, int pmPerDie, int bonusDice, int maxDice)
        {
            for (int i = 0; i < _gaugeSegments.Length; i++) _gaugeSegments[i].color = i < storedPm ? _theme.Primary : new Color(1, 1, 1, 0.15f);
            for (int i = 0; i < _bonusDiceSlots.Length; i++) _bonusDiceSlots[i].color = i < bonusDice ? _theme.Primary : new Color(1, 1, 1, 0.15f);
            _bonus.interactable = bonusDice > 0;
        }

        public void SetObjectives(IReadOnlyList<(string label, bool done)> items) => _objectives.Set(items);
        public void SetZoneTitle(string title) => _zoneTitle.text = title.ToUpperInvariant();

        public void SetWallet(int credits, int pr)
        {
            Tween.Custom(_shownCredits, credits, 0.5f, v => _credits.text = Mathf.RoundToInt(v).ToString("N0"));
            Tween.Custom(_shownPr, pr, 0.5f, v => _pr.text = Mathf.RoundToInt(v).ToString("N0"));
            _shownCredits = credits; _shownPr = pr;
        }

        public void ShowHint(string text) { _hint.text = text; _hint.alpha = 0f; Tween.Alpha(_hint, 1f, 0.2f); }

        public void SimulateRollClickForTests() => _roll.onClick.Invoke();
    }
}
```
```csharp
// Presentation/Board/Hud/ObjectivesView.cs
using System.Collections.Generic;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace N29.HeroesDawn.Presentation.Board
{
    public sealed class ObjectivesView : MonoBehaviour
    {
        [SerializeField] TMP_Text _header = null!;
        [SerializeField] TMP_Text[] _labels = new TMP_Text[3];
        [SerializeField] Image[] _checks = new Image[3];
        readonly bool[] _was = new bool[3];

        public void Set(IReadOnlyList<(string label, bool done)> items)
        {
            int done = 0;
            for (int i = 0; i < _labels.Length; i++)
            {
                bool visible = i < items.Count; _labels[i].gameObject.SetActive(visible); _checks[i].gameObject.SetActive(visible);
                if (!visible) continue;
                _labels[i].text = items[i].label; _labels[i].fontStyle = items[i].done ? FontStyles.Strikethrough : FontStyles.Normal;
                _checks[i].enabled = items[i].done;
                if (items[i].done && !_was[i]) Tween.PunchScale(_checks[i].transform, Vector3.one * 0.6f, 0.4f);
                _was[i] = items[i].done; if (items[i].done) done++;
            }
            _header.text = $"{done}/{items.Count}";
        }
    }
}
```
- [ ] **Étape 5 : Cases** — `CaseInteractionPresenter` :
```csharp
// Presentation/Board/Presenters/CaseInteractionPresenter.cs
using System;
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Board;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Common;
using VContainer.Unity;

namespace N29.HeroesDawn.Presentation.Board
{
    public sealed class CaseInteractionPresenter : IStartable, IDisposable
    {
        readonly ISubscriber<CaseTriggered> _triggered; readonly ISubscriber<ZoneValidated> _validated;
        readonly IResolveCaseUseCase _resolve; readonly GameSession _s; readonly ILocalization _loc; readonly UiTheme _theme;
        readonly NarrativeOverlayView _narrative; readonly RestPromptView _rest; readonly DemonstrationView _demo; readonly ZoneCompleteView _complete;
        IDisposable? _subs;

        public CaseInteractionPresenter(ISubscriber<CaseTriggered> triggered, ISubscriber<ZoneValidated> validated, IResolveCaseUseCase resolve, GameSession s,
            ILocalization loc, UiTheme theme, NarrativeOverlayView narrative, RestPromptView rest, DemonstrationView demo, ZoneCompleteView complete)
        { _triggered = triggered; _validated = validated; _resolve = resolve; _s = s; _loc = loc; _theme = theme;
          _narrative = narrative; _rest = rest; _demo = demo; _complete = complete; }

        public void Start()
        {
            var bag = DisposableBag.CreateBuilder();
            _triggered.Subscribe(e => HandleAsync(e).Forget()).AddTo(bag);
            _validated.Subscribe(_ => ShowCompleteAsync().Forget()).AddTo(bag);
            _subs = bag.Build();
        }

        async UniTaskVoid HandleAsync(CaseTriggered e)
        {
            await UniTask.Delay(400); // laisse finir l'animation du pion
            switch (e.Type)
            {
                case CaseType.Narrative:
                    int choice = await _narrative.Show(_loc.Get("narrative.generic.title"), _loc.Get("narrative.generic.body"),
                                                       _loc.Get("narrative.generic.a"), _loc.Get("narrative.generic.b"));
                    _resolve.Execute(success: choice == 0);
                    break;
                case CaseType.Rest:
                    _resolve.Execute(await _rest.Ask());
                    break;
                case CaseType.MiniGame:
                    var leader = _s.Content.Member(_s.Team[0].Id);
                    _resolve.Execute(await _demo.Play(_theme.FamilyColor(leader.Family), zoneWidth: 0.22f));
                    break;
            }
        }

        async UniTaskVoid ShowCompleteAsync()
        {
            await UniTask.Delay(600);
            var names = new System.Collections.Generic.List<string>();
            foreach (var o in _s.Run!.Graph.Zone.Objectives) names.Add(_loc.Get(o.NameKey));
            await _complete.Show(names);
        }

        public void Dispose() => _subs?.Dispose();
    }
}
```
```csharp
// Presentation/Board/Cases/DemonstrationView.cs — mini-jeu « Démonstration » (curseur + zone)
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Common;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;
namespace N29.HeroesDawn.Presentation.Board
{
    public sealed class DemonstrationView : MonoBehaviour
    {
        [SerializeField] CanvasGroup _root = null!;
        [SerializeField] RectTransform _bar = null!, _zone = null!, _cursor = null!;
        [SerializeField] Image _zoneImage = null!;
        [SerializeField] Image[] _attemptDots = new Image[3];
        [SerializeField] ComicButton _tapArea = null!; // plein écran, invisible
        [SerializeField] float _cursorSpeed = 1.4f;       // allers-retours par seconde
        [VContainer.Inject] UiTheme _theme = null!;

        void Awake() => _root.gameObject.SetActive(false);

        public async UniTask<bool> Play(Color familyColor, float zoneWidth)
        {
            _root.gameObject.SetActive(true); _root.alpha = 0; await Tween.Alpha(_root, 1f, 0.2f).ToUniTask();
            _zoneImage.color = familyColor;
            int successes = 0;
            for (int attempt = 0; attempt < 3; attempt++)
            {
                float center = Random.Range(0.25f, 0.75f);                     // affichage uniquement
                _zone.anchorMin = new Vector2(center - zoneWidth / 2, 0); _zone.anchorMax = new Vector2(center + zoneWidth / 2, 1);
                var tap = new UniTaskCompletionSource();
                void OnTap() => tap.TrySetResult();
                _tapArea.Clicked += OnTap;
                float t = 0;
                while (!tap.Task.Status.IsCompleted())
                {
                    t += Time.deltaTime * _cursorSpeed;
                    float x = Mathf.PingPong(t, 1f);
                    _cursor.anchorMin = _cursor.anchorMax = new Vector2(x, 0.5f);
                    await UniTask.Yield();
                }
                _tapArea.Clicked -= OnTap;
                float pos = _cursor.anchorMin.x;
                bool hit = pos >= _zone.anchorMin.x && pos <= _zone.anchorMax.x;
                _attemptDots[attempt].color = hit ? _theme.Success : _theme.Danger;
                await Tween.PunchScale(_attemptDots[attempt].transform, Vector3.one * 0.5f, 0.3f).ToUniTask();
                if (hit) successes++;
                if (successes == 2 || attempt - successes == 1) break;
            }
            await UniTask.Delay(400);
            await Tween.Alpha(_root, 0f, 0.2f).ToUniTask(); _root.gameObject.SetActive(false);
            foreach (var d in _attemptDots) d.color = Color.white;
            return successes >= 2;
        }
    }
}
```
> Au M1, la réussite du mini-jeu d'adresse est décidée par le client (le serveur la vérifiera par plausibilité au M2, cf. `05_Simulation_Deterministe.md`).

`NarrativeOverlayView`, `RestPromptView`, `ZoneCompleteView` : même structure que `ModalView` (CanvasGroup, carte « comics » qui apparaît avec `Ease.OutBack`, boutons `ComicButton`, `UniTaskCompletionSource<T>` rendu au clic). Mise en page de l'événement : illustration à gauche (placeholder 640 × 640), titre + texte à droite, deux boutons pleine largeur en bas (choix A en `Primary`, choix B en `Surface`). `ZoneCompleteView` : bandeau « ZONE SÉCURISÉE ! » qui glisse depuis la gauche, liste des 3 objectifs cochés un par un (délai 0,25 s), bouton « Continuer » qui relance la zone (M1 : `IOpenZoneUseCase.Execute` sur la même zone).

- [ ] **Étape 6 : Vérifier** — PlayMode `BoardLoopTests` : `SpammingRoll_RollsOnce` vert (le deuxième test passera après la tâche 17). Manuel : partir de l'entrée, ramasser une ressource (toast + compteur), s'arrêter sur un brouillard (message + choix), jouer un mini-jeu, un événement, un repos ; aucun écran ne demande plus de 2 secondes pour comprendre quoi faire.
- [ ] **Étape 7 : Commit** — `git commit -am "feat(board-ui): HUD, présentateur, dé, jauge, objectifs, cases"`

---

### Tâche 16 : Combat — arène et lecteur d'événements

**Fichiers :** `Presentation/Combat/Arena/*` ; prefabs `Prefabs/Combat/Unit.prefab`, `DamageNumber.prefab`, `StatusIcon.prefab`

**Interfaces :**
- Consomme : `CombatStarted`, `CombatEventBatch`, `CombatRunner.GaugeInterpolation`.
- Produit :
  - `UnitView` : `void Bind(CombatUnit u, Color family, Sprite familyIcon, string name)`, `UniTask PlayAttack(Vector3 towards)`, `UniTask PlayHit(bool crit)`, `void SetHp(int hp, int max)`, `void SetStatuses(IReadOnlyList<StatusType>)`, `UniTask PlayDefeat()`, `void SetTargetable(bool on)`, `void SetSelected(bool on)`, `event Action<int> Tapped`.
  - `DamageNumberPool.Spawn(Vector3 worldPos, int amount, DamageKind kind)` — `enum DamageKind { Normal, Crit, Heal, Shield }` ; styles : blanc 64 pt ; critique jaune 96 pt + « ! » + secousse ; soin vert « +x » ; bouclier bleu.
  - `CombatDirector` : file d'événements, `event Action<CombatEvent> EventPlayed` (pour le HUD), vitesse ×1/×2, `bool IsPlaying`.
  - Placement : alliés à gauche en arc (x −6…−3), ennemis à droite (x 3…6), boss plus grand (×1,4).

- [ ] **Étape 1 : Implémentation**
```csharp
// Presentation/Combat/Arena/CombatDirector.cs
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Combat;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Domain.Combat;
using N29.HeroesDawn.Presentation.Common;
using PrimeTween;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Combat
{
    /// <summary>Joue les CombatEvent dans l'ordre avec leurs animations. Ne calcule aucune règle.</summary>
    public sealed class CombatDirector : MonoBehaviour
    {
        [SerializeField] UnitView _unitPrefab = null!;
        [SerializeField] Transform _allyAnchor = null!, _enemyAnchor = null!;
        [SerializeField] DamageNumberPool _numbers = null!;
        [VContainer.Inject] ISubscriber<CombatStarted> _started = null!;
        [VContainer.Inject] ISubscriber<CombatEventBatch> _batch = null!;
        [VContainer.Inject] UiTheme _theme = null!;
        [VContainer.Inject] ILocalization _loc = null!;
        [VContainer.Inject] GameSession _session = null!;
        readonly Dictionary<int, UnitView> _views = new Dictionary<int, UnitView>();
        readonly Queue<CombatEvent> _queue = new Queue<CombatEvent>();
        IDisposable? _subs; bool _playing;
        public float Speed { get; set; } = 1f;
        public event Action<CombatEvent>? EventPlayed;
        public event Action<int>? UnitTapped;
        public IReadOnlyDictionary<int, UnitView> Views => _views;
        public bool IsPlaying => _playing || _queue.Count > 0;

        void Start()
        {
            var bag = DisposableBag.CreateBuilder();
            _started.Subscribe(e => Spawn(e.Sim)).AddTo(bag);
            _batch.Subscribe(e => { foreach (var ev in e.Events) _queue.Enqueue(ev); if (!_playing) Play().Forget(); }).AddTo(bag);
            _subs = bag.Build();
        }

        void Spawn(CombatSimulation sim)
        {
            int a = 0, e = 0;
            foreach (var u in sim.Units)
            {
                bool ally = u.Side == Side.Ally;
                var anchor = ally ? _allyAnchor : _enemyAnchor;
                int i = ally ? a++ : e++;
                var pos = anchor.position + new Vector3((ally ? -1 : 1) * (i % 2) * 1.4f, 2.2f - i * 1.6f, 0);
                var v = Instantiate(_unitPrefab, pos, Quaternion.identity, anchor);
                string name = ally ? _loc.Get(_session.Content.Member(new N29.HeroesDawn.Core.MemberId(u.DefId)).NameKey)
                                   : _loc.Get(_session.Content.Enemy(new N29.HeroesDawn.Core.EnemyId(u.DefId)).NameKey);
                v.Bind(u, _theme.FamilyColor(u.Family), _theme.FamilyIcon(u.Family), name);
                if (u.DefId == "brick") v.transform.localScale *= 1.4f;
                v.Tapped += id => UnitTapped?.Invoke(id);
                _views.Add(u.Id, v);
            }
        }

        async UniTaskVoid Play()
        {
            _playing = true;
            while (_queue.Count > 0)
            {
                var ev = _queue.Dequeue();
                await PlayOne(ev);
                EventPlayed?.Invoke(ev);
            }
            _playing = false;
        }

        UniTask Wait(float seconds) => UniTask.Delay(TimeSpan.FromSeconds(seconds / Speed));

        async UniTask PlayOne(CombatEvent ev)
        {
            switch (ev)
            {
                case SkillUsedEvent s:
                    var actor = _views[s.Actor];
                    var target = s.Targets.Length > 0 ? _views[s.Targets[0]].transform.position : actor.transform.position;
                    await actor.PlayAttack(target);
                    break;
                case DamageEvent d:
                    _numbers.Spawn(_views[d.Target].transform.position + Vector3.up, d.Amount, d.Crit ? DamageKind.Crit : DamageKind.Normal);
                    _views[d.Target].SetHpFromModel();
                    await _views[d.Target].PlayHit(d.Crit);
                    break;
                case HealEvent h when h.Amount > 0:
                    _numbers.Spawn(_views[h.Target].transform.position + Vector3.up, h.Amount, DamageKind.Heal);
                    _views[h.Target].SetHpFromModel(); await Wait(0.15f);
                    break;
                case ShieldAbsorbedEvent sh:
                    _numbers.Spawn(_views[sh.Target].transform.position + Vector3.up, sh.Amount, DamageKind.Shield); await Wait(0.1f);
                    break;
                case StatusAppliedEvent st: _views[st.Target].RefreshStatusesFromModel(); break;
                case StatusExpiredEvent sx: _views[sx.Target].RefreshStatusesFromModel(); break;
                case UnitDefeatedEvent u: await _views[u.Unit].PlayDefeat(); break;
                case ComboUsedEvent c:
                    await UniTask.WhenAll(_views[c.A].PlayComboCharge(), _views[c.B].PlayComboCharge());
                    break;
            }
        }

        void OnDestroy() => _subs?.Dispose();
    }
}
```
```csharp
// Presentation/Combat/Arena/UnitView.cs
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Combat;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
namespace N29.HeroesDawn.Presentation.Combat
{
    public sealed class UnitView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] SpriteRenderer _body = null!, _familyIcon = null!, _selectRing = null!, _targetArrow = null!;
        [SerializeField] Transform _hpFill = null!, _hpGhost = null!;   // barre de vie + « fantôme » qui descend en retard
        [SerializeField] TextMeshPro _name = null!;
        [SerializeField] StatusBarView _statuses = null!;
        CombatUnit _model = null!;
        public int UnitId => _model.Id;
        public event Action<int>? Tapped;

        public void Bind(CombatUnit u, Color family, Sprite icon, string name)
        {
            _model = u; _body.color = family; _familyIcon.sprite = icon; _name.text = name;
            if (u.Side == Side.Enemy) _body.flipX = true;
            SetHpFromModel(); SetSelected(false); SetTargetable(false);
        }

        public void SetHpFromModel()
        {
            float r = Mathf.Clamp01((float)_model.Hp / _model.MaxHp);
            _hpFill.localScale = new Vector3(r, 1, 1);
            Tween.ScaleX(_hpGhost, r, 0.5f, Ease.InQuad, startDelay: 0.2f);
        }

        public void RefreshStatusesFromModel()
        {
            var list = new List<StatusType>(); foreach (var s in _model.Statuses) list.Add(s.Type);
            _statuses.Set(list);
        }

        public async UniTask PlayAttack(Vector3 towards)
        {
            var start = transform.position;
            var dir = (towards - start).normalized * 0.8f;
            await Tween.Position(transform, start + dir, 0.12f, Ease.OutQuad).ToUniTask();
            await Tween.Position(transform, start, 0.18f, Ease.OutBack).ToUniTask();
        }

        public async UniTask PlayHit(bool crit)
        {
            Tween.Color(_body, Color.white, _body.color, 0.2f);
            await Tween.ShakeLocalPosition(transform, new Vector3(crit ? 0.35f : 0.15f, 0, 0), 0.2f).ToUniTask();
        }

        public UniTask PlayComboCharge() => Tween.PunchScale(transform, Vector3.one * 0.3f, 0.35f).ToUniTask();

        public async UniTask PlayDefeat()
        {
            await Tween.Alpha(_body, 0.2f, 0.35f).ToUniTask();
            _name.alpha = 0.4f; _statuses.Set(Array.Empty<StatusType>());
        }

        public void SetSelected(bool on) => _selectRing.enabled = on;
        public void SetTargetable(bool on) { _targetArrow.enabled = on; if (on) Tween.PunchScale(_targetArrow.transform, Vector3.one * 0.2f, 0.3f); }
        public void OnPointerClick(PointerEventData e) { if (_model.IsAlive) Tapped?.Invoke(_model.Id); }
    }
}
```
> La caméra « CombatCamera » porte un `Physics2DRaycaster` pour que `IPointerClickHandler` fonctionne sur les `SpriteRenderer` (collider `BoxCollider2D` sur le prefab `Unit`, taille ≥ 1,2 × 1,6 unités).

```csharp
// Presentation/Combat/Arena/DamageNumberPool.cs
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
namespace N29.HeroesDawn.Presentation.Combat
{
    public enum DamageKind { Normal, Crit, Heal, Shield }
    public sealed class DamageNumberPool : MonoBehaviour
    {
        [SerializeField] TextMeshPro _prefab = null!;
        ObjectPool<TextMeshPro> _pool = null!;
        void Awake() => _pool = new ObjectPool<TextMeshPro>(() => Instantiate(_prefab, transform), t => t.gameObject.SetActive(true), t => t.gameObject.SetActive(false), defaultCapacity: 24);

        public void Spawn(Vector3 pos, int amount, DamageKind kind)
        {
            var t = _pool.Get();
            t.transform.position = pos + new Vector3(Random.Range(-0.3f, 0.3f), 0, 0);
            (t.text, t.fontSize, t.color) = kind switch
            {
                DamageKind.Crit => ($"{amount}!", 9.6f, new Color32(0xFF, 0xD2, 0x3F, 0xFF)),
                DamageKind.Heal => ($"+{amount}", 6.4f, new Color32(0x4C, 0xC9, 0x6B, 0xFF)),
                DamageKind.Shield => ($"({amount})", 5.6f, new Color32(0x6B, 0xB8, 0xF2, 0xFF)),
                _ => (amount.ToString(), 6.4f, Color.white)
            };
            t.alpha = 1f; t.transform.localScale = Vector3.one * (kind == DamageKind.Crit ? 1.3f : 1f);
            Sequence.Create()
                .Group(Tween.Position(t.transform, t.transform.position + Vector3.up * 1.2f, 0.7f, Ease.OutCubic))
                .Group(Tween.Scale(t.transform, Vector3.one, 0.25f, Ease.OutBack))
                .Chain(Tween.Alpha(t, 0f, 0.25f))
                .OnComplete(() => _pool.Release(t));
        }
    }
}
```
`StatusBarView` : rangée d'icônes (max 5) sous la barre de vie, une icône par `StatusType` (sprites dans `UiTheme`), durée affichée en petit chiffre.

- [ ] **Étape 2 : Vérifier (manuel)** — combat en mode auto contre 2 voyous : chaque attaque se lit (avance, recul), les chiffres ne se chevauchent pas, les critiques se remarquent, une unité vaincue devient translucide ; en ×2, tout est deux fois plus rapide.
- [ ] **Étape 3 : Commit** — `git commit -am "feat(combat-ui): arène, lecteur d'événements, chiffres de dégâts"`

---

### Tâche 17 : Combat — HUD (portraits ATB, compétences, ciblage, combo) et résultats

**Fichiers :** `Presentation/Combat/Hud/*`, `Results/CombatResultsView.cs`, `Presenters/CombatPresenter.cs` · Test `Assets/Tests/PlayMode/CombatHudTests.cs`

**Interfaces :**
- Consomme : `CombatRunner` (`Sim`, `GaugeInterpolation`), `ICombatCommandsUseCase`, `CombatDirector`, `CombatFinished`.
- Produit :
  - `CombatPresenter.Tick()` fait avancer le runner : `_runner.Advance((int)(Time.deltaTime * 1000))`.
  - `ICombatHudView` :
    - `event Action<int> PortraitTapped` (index 0-3), `event Action<int> SkillTapped` (0-2), `event Action ComboTapped, AutoToggled, SpeedToggled, RetreatTapped;`
    - `void BindPortraits(IReadOnlyList<PortraitData>)` avec `PortraitData(int unitId, string name, Color family, Sprite icon, int fatigue)` ;
    - `void SetGauge(int index, float ratio, bool ready)`, `void SetHp(int index, float ratio)`, `void SetSelected(int index)` (−1 = aucun) ;
    - `void ShowSkills(IReadOnlyList<SkillButtonData>)` avec `SkillButtonData(string name, string desc, int cooldown, bool usable)`, `void HideSkills()` ;
    - `void ShowCombo(string name, Color a, Color b)`, `void HideCombo()` ;
    - `void SetAuto(bool on)`, `void SetSpeed(int x)`, `void ShowTargetingHint(bool on)`.
  - `CombatPresenter` — machine d'états de saisie : **Attente → Portrait sélectionné → Compétence choisie (ciblage) → Commande envoyée**. Règles UX :
    1. Quand un seul allié est prêt, il est **sélectionné automatiquement** et ses compétences s'ouvrent.
    2. Quand plusieurs sont prêts, le plus ancien prêt est sélectionné ; toucher un autre portrait prêt change la sélection (Review Focus 3).
    3. Compétence à cible unique → les cibles valides clignotent (flèche) ; toucher une cible envoie la commande. Toucher ailleurs annule.
    4. Compétence de zone / sur soi → envoi immédiat.
    5. Le bouton **COMBO** n'apparaît que si `Sim.AvailableCombos()` n'est pas vide ; il montre le nom et les deux couleurs de famille ; toucher → ciblage si l'effet est mono-cible, sinon envoi.
    6. Une compétence en recharge est grisée avec le nombre de tours restants.
    7. En mode Auto, le panneau de compétences est masqué et les portraits prêts ne s'allument plus.
  - `CombatResultsView.Show(bool victory, int credits, IReadOnlyList<(string name, Color family, int before, int after)> fatigue) : UniTask` → publie `ResultsClosed` au clic sur « Continuer » ; `SimulateContinueForTests()`.

**Disposition du HUD de combat :**
```
┌─────────────────────────────────────────────────────────────────────┐
│ [Retraite]                                          [AUTO] [x1]       │
│                                                                      │
│    (alliés)                                   (ennemis)              │
│                                                                      │
│               ┌ Compétences (au-dessus du portrait sélectionné) ┐    │
│               │ [A1 Morsure]  [A2 Souffle 2]  [A3 Zéro absolu]  │    │
│               └──────────────────────────────────────────────────┘  │
│ [Portrait 1][Portrait 2][Portrait 3][Portrait 4]         ( COMBO ! )  │
│  ▓▓▓▓▓░ ATB  ▓▓░░░░      ▓▓▓▓▓▓ PRÊT  ▓░░░░░                          │
└─────────────────────────────────────────────────────────────────────┘
```
Portraits 220 × 220 px ; jauge ATB sous chaque portrait (se remplit en continu, interpolée) ; contour lumineux jaune + petit rebond quand prêt ; barre de vie fine au-dessus du portrait ; icône de famille dans un coin ; fatigue en pastille si ≥ 60 %.

- [ ] **Étape 1 : Tests PlayMode en échec**
```csharp
// Assets/Tests/PlayMode/CombatHudTests.cs
using System.Collections;
using System.Linq;
using N29.HeroesDawn.Application.Features.Combat;
using N29.HeroesDawn.Domain.Combat;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class CombatHudTests
{
    IEnumerator EnterCombat()
    {
        SceneManager.LoadScene("Boot");
        for (int i = 0; i < 180 && Object.FindAnyObjectByType<N29.HeroesDawn.Presentation.Board.BoardHudView>() == null; i++) yield return null;
        var root = Object.FindAnyObjectByType<N29.HeroesDawn.Composition.RootLifetimeScope>();
        var session = (N29.HeroesDawn.Application.Session.GameSession)root.Container.Resolve(typeof(N29.HeroesDawn.Application.Session.GameSession));
        session.Run = N29.HeroesDawn.Domain.Board.ZoneRun.ForTests(session.Content, session.Run!.Graph.Zone, 9, 0, true, position: "n02");
        session.Run.SetPmForTests(1);
        ((N29.HeroesDawn.Application.Features.Board.IMoveToUseCase)root.Container.Resolve(typeof(N29.HeroesDawn.Application.Features.Board.IMoveToUseCase)))
            .Execute(new N29.HeroesDawn.Core.CaseId("n03"));
        for (int i = 0; i < 180 && !SceneManager.GetSceneByName("Combat").isLoaded; i++) yield return null;
    }

    CombatRunner Runner => (CombatRunner)Object.FindAnyObjectByType<N29.HeroesDawn.Composition.RootLifetimeScope>().Container.Resolve(typeof(CombatRunner));

    [UnityTest] // Review Focus 3 : plusieurs alliés prêts → sélection explicite et changeable
    public IEnumerator SeveralReadyAllies_PlayerCanSwitchSelection()
    {
        yield return EnterCombat();
        var sim = Runner.Sim!;
        foreach (var u in sim.Units) u.Gauge = u.Side == Side.Ally ? u.GaugeMax : 0;
        yield return new WaitForSeconds(0.2f);
        var hud = Object.FindAnyObjectByType<N29.HeroesDawn.Presentation.Combat.CombatHudView>();
        Assert.That(hud.SelectedIndex, Is.EqualTo(0));
        hud.SimulatePortraitTapForTests(2);
        yield return null;
        Assert.That(hud.SelectedIndex, Is.EqualTo(2));
    }

    [UnityTest] // Review Focus 4 : cible morte entre sélection et validation → pas d'erreur
    public IEnumerator DeadTarget_DoesNotBreak()
    {
        yield return EnterCombat();
        var sim = Runner.Sim!;
        var ally = sim.Units.First(u => u.Side == Side.Ally);
        var enemy = sim.Units.First(u => u.Side == Side.Enemy);
        foreach (var u in sim.Units) u.Gauge = 0;
        ally.Gauge = ally.GaugeMax;
        enemy.Hp = 0;
        ((ICombatCommandsUseCase)Object.FindAnyObjectByType<N29.HeroesDawn.Composition.RootLifetimeScope>().Container.Resolve(typeof(ICombatCommandsUseCase)))
            .UseSkill(ally.Id, 0, enemy.Id);
        yield return new WaitForSeconds(0.3f);
        LogAssert.NoUnexpectedReceived();
    }
}
```
- [ ] **Étape 2 : Vérifier l'échec.**
- [ ] **Étape 3 : Implémentation du présentateur**
```csharp
// Presentation/Combat/Presenters/CombatPresenter.cs
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Combat;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Combat;
using N29.HeroesDawn.Presentation.Common;
using VContainer.Unity;

namespace N29.HeroesDawn.Presentation.Combat
{
    public sealed class CombatPresenter : IStartable, ITickable, IDisposable
    {
        enum InputState { Idle, SkillPanel, Targeting }
        readonly CombatRunner _runner; readonly ICombatCommandsUseCase _cmd; readonly ICombatHudView _hud; readonly CombatDirector _director;
        readonly CombatResultsView _results; readonly GameSession _s; readonly ILocalization _loc; readonly UiTheme _theme; readonly IModalService _modal;
        readonly ISubscriber<CombatStarted> _started; readonly ISubscriber<CombatFinished> _finished;
        readonly List<int> _allyIds = new List<int>();
        readonly List<int> _readyOrder = new List<int>();
        InputState _state; int _selected = -1; int _pendingSlot = -1; bool _pendingCombo; (int a, int b) _combo;
        bool _auto; int _speed = 1; IDisposable? _subs;

        public CombatPresenter(CombatRunner runner, ICombatCommandsUseCase cmd, ICombatHudView hud, CombatDirector director, CombatResultsView results,
            GameSession s, ILocalization loc, UiTheme theme, IModalService modal, ISubscriber<CombatStarted> started, ISubscriber<CombatFinished> finished)
        { _runner = runner; _cmd = cmd; _hud = hud; _director = director; _results = results; _s = s; _loc = loc; _theme = theme; _modal = modal;
          _started = started; _finished = finished; }

        public void Start()
        {
            _hud.PortraitTapped += OnPortrait;
            _hud.SkillTapped += OnSkill;
            _hud.ComboTapped += OnCombo;
            _hud.AutoToggled += () => { _auto = !_auto; _cmd.SetAuto(_auto); _hud.SetAuto(_auto); if (_auto) ResetInput(); };
            _hud.SpeedToggled += () => { _speed = _speed == 1 ? 2 : 1; _cmd.SetSpeed(_speed); _director.Speed = _speed; _hud.SetSpeed(_speed); };
            _hud.RetreatTapped += () => RetreatAsync().Forget();
            _director.UnitTapped += OnUnitTapped;
            var bag = DisposableBag.CreateBuilder();
            _started.Subscribe(e => Bind(e.Sim)).AddTo(bag);
            _finished.Subscribe(e => ShowResultsAsync(e).Forget()).AddTo(bag);
            _subs = bag.Build();
            if (_runner.Sim != null) Bind(_runner.Sim);
        }

        void Bind(CombatSimulation sim)
        {
            _allyIds.Clear(); var data = new List<PortraitData>();
            var fatigueById = new Dictionary<string, int>(); foreach (var m in _s.Team) fatigueById[m.Id.Value] = m.Fatigue;
            foreach (var u in sim.Units)
            {
                if (u.Side != Side.Ally) continue;
                _allyIds.Add(u.Id);
                var def = _s.Content.Member(new N29.HeroesDawn.Core.MemberId(u.DefId));
                data.Add(new PortraitData(u.Id, _loc.Get(def.NameKey), _theme.FamilyColor(u.Family), _theme.FamilyIcon(u.Family), fatigueById[u.DefId]));
            }
            _hud.BindPortraits(data); _hud.SetAuto(false); _hud.SetSpeed(1); ResetInput();
        }

        public void Tick()
        {
            _runner.Advance((int)(UnityEngine.Time.deltaTime * 1000));
            var sim = _runner.Sim; if (sim == null) return;
            float interp = _runner.GaugeInterpolation;
            for (int i = 0; i < _allyIds.Count; i++)
            {
                var u = sim.Unit(_allyIds[i]);
                float ratio = u.IsReady ? 1f : (u.Gauge + interp * u.Spd * _s.Content.Balance.AtbFactorPerSecond * _s.Content.Balance.TickMs) / (float)u.GaugeMax;
                _hud.SetGauge(i, UnityEngine.Mathf.Clamp01(ratio), u.IsReady && !_auto);
                _hud.SetHp(i, (float)u.Hp / u.MaxHp);
                if (u.IsReady && !_readyOrder.Contains(u.Id)) _readyOrder.Add(u.Id);
                if (!u.IsReady) _readyOrder.Remove(u.Id);
            }
            if (_auto) return;
            // Règles 1 et 2 : sélection automatique du plus ancien prêt si rien n'est sélectionné
            if ((_selected < 0 || !sim.Unit(_selected).IsReady) && _state != InputState.Targeting)
            {
                _selected = _readyOrder.Count > 0 ? _readyOrder[0] : -1;
                if (_selected >= 0) OpenSkills(); else { _hud.SetSelected(-1); _hud.HideSkills(); _state = InputState.Idle; }
            }
            // Règle 5 : combo
            var combos = sim.AvailableCombos();
            if (combos.Count > 0)
            {
                var c = combos[0]; _combo = (c.A, c.B);
                _hud.ShowCombo(_loc.Get(c.Combo.NameKey), _theme.FamilyColor(sim.Unit(c.A).Family), _theme.FamilyColor(sim.Unit(c.B).Family));
            }
            else _hud.HideCombo();
        }

        void OpenSkills()
        {
            var sim = _runner.Sim!; var u = sim.Unit(_selected);
            _hud.SetSelected(_allyIds.IndexOf(_selected));
            var buttons = new List<SkillButtonData>();
            for (int i = 0; i < 3; i++)
            {
                var skill = u.Skills[i];
                buttons.Add(new SkillButtonData(_loc.Get($"skills.{skill.Id.Value}.name"), _loc.Get($"skills.{skill.Id.Value}.desc"), u.Cooldowns[i], sim.CanUse(u.Id, i)));
            }
            _hud.ShowSkills(buttons); _state = InputState.SkillPanel;
        }

        void OnPortrait(int index)
        {
            var sim = _runner.Sim; if (sim == null || _auto || index >= _allyIds.Count) return;
            if (!sim.Unit(_allyIds[index]).IsReady) return;
            _selected = _allyIds[index]; ResetTargeting(); OpenSkills();
        }

        void OnSkill(int slot)
        {
            var sim = _runner.Sim; if (sim == null || _selected < 0 || !sim.CanUse(_selected, slot)) return;
            var target = sim.Unit(_selected).Skills[slot].Target;
            if (target == TargetRule.SingleEnemy || target == TargetRule.SingleAlly) { _pendingSlot = slot; _pendingCombo = false; EnterTargeting(target == TargetRule.SingleEnemy ? Side.Enemy : Side.Ally); }
            else { _cmd.UseSkill(_selected, slot, -1); ResetInput(); }
        }

        void OnCombo()
        {
            var sim = _runner.Sim; if (sim == null || _auto) return;
            var combos = sim.AvailableCombos(); if (combos.Count == 0) return;
            var c = combos[0];
            if (c.Combo.Effect.Target == TargetRule.SingleEnemy) { _pendingCombo = true; _combo = (c.A, c.B); EnterTargeting(Side.Enemy); }
            else { _cmd.UseCombo(c.A, c.B, -1); ResetInput(); }
        }

        void EnterTargeting(Side side)
        {
            _state = InputState.Targeting; _hud.ShowTargetingHint(true);
            foreach (var kv in _director.Views) kv.Value.SetTargetable(_runner.Sim!.Unit(kv.Key).Side == side && _runner.Sim.Unit(kv.Key).IsAlive);
        }

        void OnUnitTapped(int unitId)
        {
            if (_state != InputState.Targeting) return;
            if (_pendingCombo) _cmd.UseCombo(_combo.a, _combo.b, unitId); else _cmd.UseSkill(_selected, _pendingSlot, unitId);
            ResetInput();
        }

        void ResetTargeting() { foreach (var v in _director.Views.Values) v.SetTargetable(false); _hud.ShowTargetingHint(false); _pendingSlot = -1; _pendingCombo = false; }
        void ResetInput() { ResetTargeting(); _selected = -1; _state = InputState.Idle; _hud.HideSkills(); _hud.SetSelected(-1); }

        async UniTaskVoid RetreatAsync()
        {
            if (await _modal.Confirm("combat.retreat.title", "combat.retreat.body", "ui.confirm", "ui.cancel")) _cmd.Retreat();
        }

        async UniTaskVoid ShowResultsAsync(CombatFinished e)
        {
            while (_director.IsPlaying) await UniTask.Yield(); // laisse finir les animations
            var rows = new List<(string, UnityEngine.Color, int, int)>();
            foreach (var f in e.Fatigue)
            {
                var def = _s.Content.Member(f.Member);
                rows.Add((_loc.Get(def.NameKey), _theme.FamilyColor(def.Family), f.Before, f.After));
            }
            await _results.Show(e.Victory, e.Credits, rows);
        }

        public void Dispose() => _subs?.Dispose();
    }
}
```

- [ ] **Étape 4 : Vues** — `CombatHudView` implémente `ICombatHudView` avec 4 `PortraitView` (Image portrait, Image jauge `fillAmount`, Image contour « prêt », barre de vie, pastille de fatigue, icône de famille, `ComicButton`), un `SkillPanelView` (3 `ComicButton` 360 × 150 px : nom en 36 pt, description en 24 pt, voile sombre + chiffre de recharge si non utilisable ; le panneau glisse de 40 px vers le haut à l'ouverture), un `ComboButtonView` (260 × 260, dégradé des deux couleurs de famille, pulsation + éclat à l'apparition, nom du combo sous le bouton), boutons Auto / Vitesse / Retraite, bandeau « Choisis une cible ». Exposer `int SelectedIndex` et `SimulatePortraitTapForTests(int)` pour les tests.
  `CombatResultsView` : fond assombri, bandeau « VICTOIRE ! » (ou « DÉFAITE… ») qui tombe avec rebond, gain de Crédits qui défile, une ligne par membre avec la jauge de fatigue qui passe de *avant* à *après* (rouge au-delà de 60 %), bouton « Continuer » → publie `ResultsClosed`.
- [ ] **Étape 5 : Vérifier** — `CombatHudTests` et `BoardLoopTests.AfterDefeat_RollIsAvailableAgain` verts. Manuel (Device Simulator + souris) : jouer 3 combats en manuel ; vérifier les 7 règles UX ci-dessus ; un combat Facile dure 45-90 s.
- [ ] **Étape 6 : Commit** — `git commit -am "feat(combat-ui): HUD ATB, compétences, ciblage, combo, résultats"`

---

### Tâche 18 : Panneau de debug, build de playtest, journal de session

**Fichiers :** `Presentation/Debug/DebugPanelView.cs` ; `Infrastructure/Local/SessionLogger.cs` ; `05_Production/Playtest_M1.md` (grille d'observation)

**Interfaces :**
- Produit :
  - `DebugPanelView` (uniquement si `Debug.isDebugBuild` ou éditeur ; ouvert par un appui long de 2 s en haut à gauche ou `F1`) : graine de la zone, phase, PM, forcer la prochaine valeur de dé (via `ZoneRun.SetPmForTests`), victoire instantanée (fixe les PV ennemis à 0), fatigue à 0, afficher tick/jauges du combat, vitesse ×4.
  - `SessionLogger` : s'abonne aux messages (`DiceRolled`, `CaseTriggered`, `CombatFinished`, `ObjectiveCompleted`, `ZoneValidated`) et écrit un CSV horodaté dans `Application.persistentDataPath/playtests/` (durée de chaque combat, choix d'événements, nombre de lancers par zone, temps entre deux actions).
- [ ] **Étape 1 : Implémenter** les deux classes (abonnements MessagePipe comme `BoardPresenter`, écriture via `StreamWriter` avec `AutoFlush`).
- [ ] **Étape 2 : Build** — profils **Android** (ARM64, IL2CPP, .NET Standard 2.1) et **Windows** (Mono) en « Development Build ».
- [ ] **Étape 3 : Grille de playtest** (`05_Production/Playtest_M1.md`) : 5 joueurs, 20 min chacun, sans explication ; mesurer : temps pour le premier lancer, erreurs de tap, compréhension du brouillard, du dé bonus, du combo, de la fatigue ; question finale « Envie de rejouer une zone ? (1-5) ».
- [ ] **Étape 4 : Commit** — `git commit -am "feat(debug): panneau de debug, journal de session, build de playtest"`

---

### Tâche 19 : Simulateur d'équilibrage (console .NET)

> **Remplacée** par `2026-10-02-integration-tout-dans-unity.md`, tâche 3 (simulateur dans l'éditeur Unity).

**Fichiers :** `Tools/BalanceSimulator/HeroesDawn.BalanceSimulator.csproj`, `Program.cs`

**Interfaces :**
- Consomme : `CombatSimulation`, `PrototypeContent` (le `.csproj` inclut les sources partagées et `Assets/_Project/Content/Prototype/*.cs`).
- Produit : `dotnet run --project Tools/BalanceSimulator -- --runs 1000 --team ancre,givre,echo,seve --enemies thug,thug --rank Normal --level 5` → taux de victoire, durée moyenne (s), combos utilisés par combat, PV restants moyens ; `--csv out.csv`.

- [ ] **Étape 1 : Implémentation**
```csharp
// Tools/BalanceSimulator/Program.cs
using System;
using System.Linq;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Combat;

static string Arg(string[] a, string name, string def) { int i = Array.IndexOf(a, name); return i >= 0 && i + 1 < a.Length ? a[i + 1] : def; }

var content = PrototypeContent.Build();
int runs = int.Parse(Arg(args, "--runs", "1000"));
var team = Arg(args, "--team", "ancre,givre,echo,seve").Split(',');
var enemies = Arg(args, "--enemies", "thug,thug").Split(',');
var rank = Enum.Parse<CombatRank>(Arg(args, "--rank", "Normal"));
int level = int.Parse(Arg(args, "--level", "5"));

int wins = 0; long ticks = 0; int combos = 0;
for (int r = 0; r < runs; r++)
{
    var setup = new CombatSetup((ulong)(r + 1), rank,
        team.Select(t => new AllyEntry(new MemberId(t), level, 0)).ToList(),
        enemies.Select(e => new EnemyEntry(new EnemyId(e), level)).ToList(), auto: true);
    var sim = new CombatSimulation(content, setup);
    while (!sim.IsOver && sim.Tick < 20_000) combos += sim.Step(20).OfType<ComboUsedEvent>().Count();
    if (sim.Victory == true) wins++;
    ticks += sim.Tick;
}
double avgSec = ticks / (double)runs * content.Balance.TickMs / 1000.0;
Console.WriteLine($"Victoires : {wins * 100.0 / runs:F1} %  |  Durée moyenne : {avgSec:F1} s  |  Combos/combat : {combos / (double)runs:F2}");
```
- [ ] **Étape 2 : Vérifier** — la commande affiche une ligne de résultats ; cible du GDD : durée moyenne 60-90 s pour un combat Normal au niveau de l'équipe. Consigner les résultats dans `05_Production/Decisions.md` si des paramètres sont changés.
- [ ] **Étape 3 : Commit** — `git commit -am "feat(tools): simulateur d'équilibrage des combats"`

---

## Définition de « terminé » pour le M1
- [ ] Tous les tests (`dotnet test`, EditMode, PlayMode) verts.
- [ ] Une zone complète (3 objectifs) jouable de bout en bout en 15-30 min, sur Android et Windows.
- [ ] 5 playtests menés avec la grille ; synthèse dans `05_Production/Playtest_M1.md`.
- [ ] Décision de fin de jalon notée dans `Decisions.md` : la boucle plateau → combat est-elle amusante ? Les 3 recommandations ⚑ (dé 1d6/6 PM, jauges actives en manuel, plafond de niveau) sont-elles confirmées ?
