#nullable enable
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace N29.HeroesDawn.Presentation.UiToolkit
{
    /// <summary>Famille d'entrée en cours : décide du focus visible, du survol, des tailles de cibles et des invites de touches.</summary>
    public enum HdInputMode { KeyboardMouse, Touch, Gamepad }

    /// <summary>
    /// Dernier périphérique utilisé (clavier ou souris, tactile, manette). Les vues basculent leurs classes USS
    /// (<c>.input--kbm</c>, <c>.input--touch</c>, <c>.input--gamepad</c>) quand il change.
    /// </summary>
    public static class HdInput
    {
        static bool _installed;

        public static HdInputMode Mode { get; private set; } = DefaultMode();
        public static event Action<HdInputMode>? Changed;

        public static HdInputMode DefaultMode()
        {
            if (UnityEngine.Application.isConsolePlatform) return HdInputMode.Gamepad;
            if (UnityEngine.Application.isMobilePlatform) return HdInputMode.Touch;
            return HdInputMode.KeyboardMouse;
        }

        public static string ClassOf(HdInputMode mode)
            => mode == HdInputMode.Touch ? "input--touch" : mode == HdInputMode.Gamepad ? "input--gamepad" : "input--kbm";

        /// <summary>Force le mode (tests, réglages, bascule explicite).</summary>
        public static void Set(HdInputMode mode)
        {
            if (mode == Mode) return;
            Mode = mode;
            Changed?.Invoke(mode);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Install()
        {
            if (_installed) return;
            _installed = true;
            Mode = DefaultMode();
            InputSystem.onEvent += OnEvent;
        }

        static void OnEvent(InputEventPtr e, InputDevice device)
        {
            // Seuls les événements d'état comptent (pas les bruits de configuration).
            if (!e.IsA<StateEvent>() && !e.IsA<DeltaStateEvent>()) return;
            if (device is Gamepad) Set(HdInputMode.Gamepad);
            else if (device is Touchscreen) Set(HdInputMode.Touch);
            else if (device is Keyboard) Set(HdInputMode.KeyboardMouse);
            else if (device is Mouse mouse && mouse.delta.ReadValue().sqrMagnitude > 0.01f) Set(HdInputMode.KeyboardMouse);
        }
    }
}
