#nullable enable
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Flow;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Presentation.Login;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation
{
    /// <summary>
    /// Interface commune de la scène Boot : EventSystem (Input System), écran de connexion (canvas d'ordre 10)
    /// et couche fenêtres + messages au-dessus de toutes les scènes (canvas imbriqué d'ordre 100).
    /// Les écrans du QG, du plateau et du combat vivent dans leurs propres scènes.
    /// </summary>
    public sealed class UiRoot : MonoBehaviour, IScreenVisibility
    {
        public UiContext Context { get; private set; } = null!;
        public LoginView Login { get; private set; } = null!;

        /// <summary>Zone des écrans de Boot (connexion), dans la zone sûre.</summary>
        public RectTransform ScreensRoot { get; private set; } = null!;
        public ModalLayer Modals { get; private set; } = null!;
        public ToastsView Toasts { get; private set; } = null!;
        bool _layersBuilt;

        /// <summary>
        /// Construit une fois les canvas de Boot (écrans, puis couche commune d'ordre 100) et y instancie
        /// la couche des modales et les toasts depuis le catalogue. Appelé au premier besoin (résolution des services).
        /// </summary>
        public void EnsureLayers(ScreenCatalog catalog)
        {
            if (_layersBuilt) return;
            _layersBuilt = true;
            EnsureEventSystem();

            var go = gameObject;
            go.layer = 5;
            if (go.GetComponent<RectTransform>() == null) go.AddComponent<RectTransform>();
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            canvas.pixelPerfect = false;
            AddScaler(go);
            go.AddComponent<GraphicRaycaster>();

            var root = (RectTransform)go.transform;
            var safe = Ui.Rect(root, "SafeArea").Stretch();
            safe.gameObject.AddComponent<SafeArea>();
            ScreensRoot = Ui.Rect(safe, "Screens").Stretch();

            // Couche commune au-dessus des scènes (canvas imbriqué, tri propre).
            var overlayGo = Ui.Rect(root, "Overlay").Stretch().gameObject;
            var overlay = overlayGo.AddComponent<Canvas>();
            overlay.overrideSorting = true;
            overlay.sortingOrder = 100;
            overlayGo.AddComponent<GraphicRaycaster>();
            var overlaySafe = Ui.Rect(overlayGo.transform, "SafeArea").Stretch();
            overlaySafe.gameObject.AddComponent<SafeArea>();
            Modals = Instantiate(Required(catalog.ModalLayer, "ModalLayer"), overlaySafe).GetComponent<ModalLayer>();
            ((RectTransform)Modals.transform).Stretch();
            Modals.Init(catalog);
            Toasts = Instantiate(Required(catalog.Toasts, "Toasts"), overlaySafe).GetComponent<ToastsView>();
            ((RectTransform)Toasts.transform).Stretch();

            // Écran de connexion (masqué jusqu'à ce que la machine d'états l'affiche).
            Login = Instantiate(catalog.Screen("Login"), ScreensRoot).GetComponent<LoginView>();
            ((RectTransform)Login.transform).Stretch();
            Login.gameObject.SetActive(false);
        }

        static GameObject Required(GameObject? prefab, string name)
            => prefab != null ? prefab : throw new System.InvalidOperationException("Prefab manquant dans ScreenCatalog : " + name);

        /// <summary>Contexte des écrans encore construits par code, messages du bus en toasts. Appelé une fois par le scope racine.</summary>
        public void Build(UiServices services, ScreenCatalog catalog)
        {
            EnsureLayers(catalog);
            var ctx = new UiContext(services, Modals, Toasts);
            Context = ctx;
            // Un message peut être une clé de localisation (ex. errors.navigation, publié par la machine d'états).
            services.Toasts.Subscribe(t => ctx.Toast(Loc.Has(t.Text) ? Loc.T(t.Text) : t.Text, t.Kind)).AddTo(destroyCancellationToken);
        }

        public void SetLoginVisible(bool visible)
        {
            if (visible) Login.transform.SetAsLastSibling();
            Login.gameObject.SetActive(visible);
        }

        static void AddScaler(GameObject go)
        {
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        static void EnsureEventSystem()
        {
            var existing = Object.FindAnyObjectByType<EventSystem>();
            if (existing != null)
            {
                // Remplace un éventuel StandaloneInputModule (ancien Input Manager) par le module Input System.
                var legacy = existing.GetComponent<StandaloneInputModule>();
                if (legacy != null)
                {
                    Object.DestroyImmediate(legacy);
                    existing.gameObject.AddComponent<InputSystemUIInputModule>();
                }
                return;
            }
            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            Object.DontDestroyOnLoad(es);
        }
    }

}
