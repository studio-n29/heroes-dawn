#nullable enable
using UnityEngine;
using UnityEngine.EventSystems;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Joystick virtuel (tactile et souris) : la valeur va de -1 à 1 sur chaque axe, au repos à zéro.</summary>
    public sealed class VirtualStick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] RectTransform knob = null!;
        [SerializeField] float radius = 90f;

        public Vector2 Value { get; private set; }

        public void OnPointerDown(PointerEventData e) => Move(e);
        public void OnDrag(PointerEventData e) => Move(e);

        public void OnPointerUp(PointerEventData e)
        {
            Value = Vector2.zero;
            if (knob != null) knob.anchoredPosition = Vector2.zero;
        }

        void Move(PointerEventData e)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform, e.position, e.pressEventCamera, out var local);
            var v = Vector2.ClampMagnitude(local / radius, 1f);
            Value = v.magnitude < 0.12f ? Vector2.zero : v;
            if (knob != null) knob.anchoredPosition = v * radius;
        }

        void OnDisable() => Value = Vector2.zero;
    }
}
