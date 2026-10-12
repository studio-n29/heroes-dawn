#nullable enable
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace N29.HeroesDawn.Presentation.Combat
{
    /// <summary>Appui maintenu sur une carte de compétence : signale le début et la fin de l'appui (la description reste affichée tant qu'on appuie).</summary>
    public sealed class SkillHoldTrigger : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public event Action<bool>? Held;
        bool _down;

        public void OnPointerDown(PointerEventData e) { _down = true; Held?.Invoke(true); }
        public void OnPointerUp(PointerEventData e) => Release();
        public void OnPointerExit(PointerEventData e) => Release();
        void OnDisable() => Release();

        void Release()
        {
            if (!_down) return;
            _down = false;
            Held?.Invoke(false);
        }
    }
}
