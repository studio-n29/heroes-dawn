#nullable enable
using N29.HeroesDawn.Presentation.Board;
using N29.HeroesDawn.Presentation.Combat;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Screens;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation
{
    /// <summary>
    /// Construit toute l'interface : Canvas (référence 1920×1080, paysage), EventSystem (Input System),
    /// couches écrans / modales / messages, puis les quatre écrans du prototype.
    /// </summary>
    public sealed class UiRoot : MonoBehaviour
    {
        public UiContext Context { get; private set; } = null!;

        public static UiRoot Create(Transform parent, UiServices services)
        {
            EnsureEventSystem();

            var go = new GameObject("UI", typeof(RectTransform));
            go.layer = 5;
            go.transform.SetParent(parent, false);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            canvas.pixelPerfect = false;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();

            var root = (RectTransform)go.transform;
            var safe = Ui.Rect(root, "SafeArea").Stretch();
            safe.gameObject.AddComponent<SafeArea>();
            var screens = Ui.Rect(safe, "Screens").Stretch();
            var modals = ModalLayer.Create(safe);
            var toastLayer = Ui.Rect(safe, "ToastLayer").Stretch();
            var toasts = Toasts.Create(toastLayer);

            var ui = go.AddComponent<UiRoot>();
            var ctx = new UiContext(services, new Navigator(), modals, toasts, screens);
            ui.Context = ctx;
            services.Events.Toast += (text, kind) => ctx.Toast(text, kind);

            ctx.Login = CreateScreen<LoginScreen>(screens, "Login", ctx);
            ctx.Hub = CreateScreen<HubScreen>(screens, "Hub", ctx);
            ctx.Board = CreateScreen<BoardScreen>(screens, "Board", ctx);
            ctx.Combat = CreateScreen<CombatScreen>(screens, "Combat", ctx);

            ctx.Nav.Show(ctx.Login);
            return ui;
        }

        static T CreateScreen<T>(RectTransform parent, string name, UiContext ctx) where T : UiScreen
        {
            var rt = Ui.Rect(parent, name).Stretch();
            var screen = rt.gameObject.AddComponent<T>();
            screen.Init(ctx);
            return screen;
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
