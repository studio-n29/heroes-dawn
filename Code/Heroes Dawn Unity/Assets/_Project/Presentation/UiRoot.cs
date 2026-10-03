#nullable enable
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Flow;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Screens;
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
        public LoginScreen Login { get; private set; } = null!;

        /// <summary>Construit l'interface commune sur ce GameObject (posé dans la scène Boot). Appelé une fois par le scope racine.</summary>
        public void Build(UiServices services)
        {
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
            var screens = Ui.Rect(safe, "Screens").Stretch();

            // Couche commune au-dessus des scènes (canvas imbriqué, tri propre).
            var overlayGo = Ui.Rect(root, "Overlay").Stretch().gameObject;
            var overlay = overlayGo.AddComponent<Canvas>();
            overlay.overrideSorting = true;
            overlay.sortingOrder = 100;
            overlayGo.AddComponent<GraphicRaycaster>();
            var overlaySafe = Ui.Rect(overlayGo.transform, "SafeArea").Stretch();
            overlaySafe.gameObject.AddComponent<SafeArea>();
            var modals = ModalLayer.Create(overlaySafe);
            var toastLayer = Ui.Rect(overlaySafe, "ToastLayer").Stretch();
            var toasts = Toasts.Create(toastLayer);

            var ctx = new UiContext(services, modals, toasts);
            Context = ctx;
            // Un message peut être une clé de localisation (ex. errors.navigation, publié par la machine d'états).
            services.Toasts.Subscribe(t => ctx.Toast(Loc.Has(t.Text) ? Loc.T(t.Text) : t.Text, t.Kind)).AddTo(destroyCancellationToken);

            Login = UiScreen.Create<LoginScreen>(screens, "Login", ctx);
        }

        public void SetLoginVisible(bool visible)
        {
            if (visible) Login.Show();
            else Login.Hide();
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

    /// <summary>Adapte la zone d'interface à Screen.safeArea (encoches mobiles).</summary>
    public sealed class SafeArea : MonoBehaviour
    {
        Rect _last;
        Vector2Int _lastSize;

        void OnEnable() => Apply();
        void Update()
        {
            if (Screen.safeArea != _last || Screen.width != _lastSize.x || Screen.height != _lastSize.y) Apply();
        }

        void Apply()
        {
            var rt = (RectTransform)transform;
            _last = Screen.safeArea;
            _lastSize = new Vector2Int(Screen.width, Screen.height);
            if (Screen.width <= 0 || Screen.height <= 0) return;
            var min = _last.position;
            var max = _last.position + _last.size;
            min.x /= Screen.width; min.y /= Screen.height;
            max.x /= Screen.width; max.y /= Screen.height;
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
