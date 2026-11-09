using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation
{
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
            // Keep a complete landscape composition inside the notch-safe viewport,
            // including tablets (4:3), wide phones and window resizing.
            var scaler = GetComponentInParent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
                scaler.referenceResolution = ReferenceFor(_last, Screen.width, Screen.height);
            }
            var min = _last.position;
            var max = _last.position + _last.size;
            min.x /= Screen.width; min.y /= Screen.height;
            max.x /= Screen.width; max.y /= Screen.height;
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        public static Vector2 ReferenceFor(Rect safe, int width, int height) =>
            new Vector2(1920f * width / Mathf.Max(1, safe.width), 1080f * height / Mathf.Max(1, safe.height));
    }
}
