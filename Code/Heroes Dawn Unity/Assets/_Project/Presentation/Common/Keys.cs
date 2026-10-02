using UnityEngine.InputSystem;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Raccourcis clavier PC (GDD §10 : souris + raccourcis). Sans clavier (mobile), tout renvoie faux.</summary>
    public static class Keys
    {
        public static bool Pressed(Key key)
        {
            var kb = Keyboard.current;
            return kb != null && kb[key].wasPressedThisFrame;
        }

        public static bool ConfirmPressed() => Pressed(Key.Space) || Pressed(Key.Enter) || Pressed(Key.NumpadEnter);
    }
}
