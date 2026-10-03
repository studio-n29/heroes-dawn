using UnityEngine.InputSystem;

namespace N29.HeroesDawn.Presentation.Mvp
{
    /// <summary>Échap au clavier ; le bouton retour Android arrive aussi en Échap dans l'Input System.</summary>
    public sealed class InputSystemBackInput : IBackInput
    {
        public bool BackPressedThisFrame
        {
            get
            {
                var kb = Keyboard.current;
                return kb != null && kb.escapeKey.wasPressedThisFrame;
            }
        }
    }
}
