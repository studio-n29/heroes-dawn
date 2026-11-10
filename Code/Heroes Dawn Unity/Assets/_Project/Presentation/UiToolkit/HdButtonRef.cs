#nullable enable
using UnityEngine;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.UiToolkit
{
    /// <summary>Appel d'un bouton comme « onClick.Invoke() » : envoie la validation de la manette (A) au bouton, comme un vrai appui.</summary>
    public sealed class HdClick
    {
        readonly Button _button;
        public HdClick(Button button) => _button = button;

        public void Invoke()
        {
            if (!_button.enabledInHierarchy) return;
            using (var e = NavigationSubmitEvent.GetPooled()) { e.target = _button; _button.SendEvent(e); }
        }
    }

    /// <summary>
    /// Accès d'une vue à l'un de ses boutons UI Toolkit, avec la forme des anciens boutons uGUI
    /// (<c>onClick.Invoke()</c>, <c>interactable</c>, <c>gameObject</c>) pour que les tests de parcours ne changent pas.
    /// </summary>
    public sealed class HdButtonRef
    {
        readonly Button _button;
        readonly Component _owner;

        public HdButtonRef(Button button, Component owner)
        {
            _button = button;
            _owner = owner;
            onClick = new HdClick(button);
        }

        public Button Element => _button;
        public HdClick onClick { get; }
        public bool interactable { get => _button.enabledSelf; set => _button.SetEnabled(value); }

        /// <summary>Objet de la vue (le bouton est un élément, pas un objet : l'activité de la vue fait foi).</summary>
        public GameObject gameObject => _owner.gameObject;

        /// <summary>Le bouton est affiché et actif (visible, non masqué par un parent).</summary>
        public bool Visible => _button.resolvedStyle.display != DisplayStyle.None && _button.resolvedStyle.visibility == Visibility.Visible;

        public string Text { get => _button.text; set => _button.text = value; }
    }
}
