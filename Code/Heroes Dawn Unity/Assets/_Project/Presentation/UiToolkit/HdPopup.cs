#nullable enable
using N29.HeroesDawn.Presentation.Mvp;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.UiToolkit
{
    /// <summary>
    /// Base des fenêtres modales UI Toolkit : Retour (B, Échap) la ferme quand elle est annulable, la fenêtre apparaît en fondu,
    /// le focus de la manette part du bouton par défaut de la vue. Une croix de fermeture (planche 16.2) est ajoutée en haut à droite
    /// de la fenêtre des modales annulables ; elle déclenche la même annulation que Retour.
    /// </summary>
    public abstract class HdPopup : HdView, IPopupView
    {
        /// <summary>Nom de la croix de fermeture dans l'arbre (les vues concrètes ne l'utilisent pas).</summary>
        public const string CloseCrossName = "hdClose";
        const long CrossRefreshMs = 250;

        UnityEngine.UIElements.Button? _closeCross;

        public virtual bool Cancelable => true;
        public abstract void RequestCancel();

        /// <summary>
        /// La croix s'affiche-t-elle ? Oui pour une fenêtre annulable, sauf les fenêtres sans annulation voulue : résultats de combat,
        /// ouverture de booster, mini-jeux 2D (dont la fermeture est l'abandon). Une vue peut redéfinir cette règle.
        /// </summary>
        protected virtual bool ShowCloseButton => Cancelable
            && !(this is N29.HeroesDawn.Presentation.Combat.ICombatResultsPopupView)
            && !(this is N29.HeroesDawn.Presentation.Hub.IBoosterOpeningPopupView)
            && !(this is N29.HeroesDawn.Presentation.Activities.IMiniGame2DPopupView);

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
            if (window != null) AddCloseCross(window);
            DefaultFocus.schedule.Execute(() => DefaultFocus.Focus()).StartingIn(40);
        }

        /// <summary>
        /// Pose la croix en dernier enfant de la fenêtre (absolue, coin haut droit : ni l'ordre de focus initial, qui part de
        /// <see cref="DefaultFocus"/>, ni le piège de focus, qui reste dans la fenêtre, n'en changent). Elle ne se montre que si
        /// <see cref="ShowCloseButton"/> ; l'état est revu à intervalle lent, car <see cref="Cancelable"/> peut changer pendant la fenêtre.
        /// </summary>
        void AddCloseCross(VisualElement window)
        {
            if (window.Q<UnityEngine.UIElements.Button>(CloseCrossName) != null) return;
            var cross = new UnityEngine.UIElements.Button { name = CloseCrossName };
            cross.AddToClassList("hd-modal__close");
            // Même action que Retour (B, Échap) ; les boutons Fermer et Annuler des fenêtres restent en place.
            cross.clicked += () => { if (Cancelable) RequestCancel(); };
            window.Add(cross);
            _closeCross = cross;
            RefreshCross();
            cross.schedule.Execute(RefreshCross).Every(CrossRefreshMs);
        }

        void RefreshCross()
        {
            if (_closeCross == null) return;
            _closeCross.style.display = ShowCloseButton ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
