#nullable enable
using N29.HeroesDawn.Presentation.Mvp;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.UiToolkit
{
    /// <summary>
    /// Base des fenêtres modales UI Toolkit : Retour (B, Échap) la ferme quand elle est annulable, la fenêtre apparaît en fondu,
    /// le focus de la manette part du bouton par défaut de la vue.
    /// </summary>
    public abstract class HdPopup : HdView, IPopupView
    {
        public virtual bool Cancelable => true;
        public abstract void RequestCancel();

        /// <summary>Élément pris par défaut au clavier ou à la manette à l'ouverture.</summary>
        protected abstract VisualElement DefaultFocus { get; }

        protected override void OnEnable()
        {
            base.OnEnable();
            // Le modèle est le panneau modal : il capte les clics (rien ne passe au travers) et répond à Retour.
            var modal = Root.Q<VisualElement>("modal");
            if (modal != null) modal.pickingMode = PickingMode.Position;
            Root.RegisterCallback<NavigationCancelEvent>(e => { if (Cancelable) { RequestCancel(); e.StopPropagation(); } });
            var window = Root.Q<VisualElement>("window");
            window?.schedule.Execute(() => window.AddToClassList("hd-modal__window--in")).StartingIn(20);
            DefaultFocus.schedule.Execute(() => DefaultFocus.Focus()).StartingIn(40);
        }
    }
}
