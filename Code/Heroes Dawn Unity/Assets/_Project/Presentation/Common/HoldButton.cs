#nullable enable
using UnityEngine;
using UnityEngine.EventSystems;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Bouton maintenu : « maintenu » tant que le doigt est dessus, « appuyé » une fois par appui (lu puis consommé).</summary>
    public sealed class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        bool _pressed;

        public bool Held { get; private set; }

        /// <summary>Vrai une seule fois par appui.</summary>
        public bool ConsumePress()
        {
            bool p = _pressed;
            _pressed = false;
            return p;
        }

        public void OnPointerDown(PointerEventData e) { Held = true; _pressed = true; }
        public void OnPointerUp(PointerEventData e) => Held = false;
        public void OnPointerExit(PointerEventData e) => Held = false;
        void OnDisable() { Held = false; _pressed = false; }
    }
}
