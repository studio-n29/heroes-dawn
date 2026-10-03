using UnityEngine;

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
