#nullable enable
using System;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Mvp
{
    /// <summary>
    /// Prefabs des écrans, couches communes et popups, rempli par le générateur de prefabs
    /// (Settings/ScreenCatalog.asset) et branché sur les scopes par le générateur de scènes.
    /// Les scènes ne contiennent pas les écrans : retoucher un prefab ne touche jamais une scène.
    /// </summary>
    [CreateAssetMenu(menuName = "Heroes Dawn/Screen Catalog", fileName = "ScreenCatalog")]
    public sealed class ScreenCatalog : ScriptableObject
    {
        [SerializeField, OptionalRef] GameObject? login;
        [SerializeField, OptionalRef] GameObject? hub;
        [SerializeField, OptionalRef] GameObject? board;
        [SerializeField, OptionalRef] GameObject? combat;
        [SerializeField, OptionalRef] GameObject? modalLayer;
        [SerializeField, OptionalRef] GameObject? toasts;
        [SerializeField, OptionalRef] GameObject? tutorialOverlay;
        [SerializeField] GameObject[] popups = Array.Empty<GameObject>();
        [SerializeField] GameObject[] pages = Array.Empty<GameObject>();

        public GameObject? ModalLayer => modalLayer;
        public GameObject? Toasts => toasts;
        public GameObject? TutorialOverlay => tutorialOverlay;

        /// <summary>Prefab de l'écran d'une scène : "Login", "Hub", "Board" ou "Combat".</summary>
        public GameObject Screen(string scene)
        {
            var p = scene switch { "Login" => login, "Hub" => hub, "Board" => board, "Combat" => combat, _ => null };
            if (p == null) throw new InvalidOperationException("Prefab d'écran manquant dans ScreenCatalog : " + scene);
            return p;
        }

        public bool HasScreen(string scene) => scene switch { "Login" => login, "Hub" => hub, "Board" => board, "Combat" => combat, _ => null } != null;

        /// <summary>Prefab de popup dont la racine porte un composant <typeparamref name="TView"/>.</summary>
        public GameObject PopupFor<TView>() where TView : class
        {
            foreach (var p in popups)
                if (p != null && p.GetComponent<TView>() != null) return p;
            throw new InvalidOperationException($"Popup manquante dans ScreenCatalog : {typeof(TView).Name}");
        }

        /// <summary>Prefab de page du hub dont la racine porte un composant <typeparamref name="TView"/>.</summary>
        public GameObject PageFor<TView>() where TView : class
        {
            foreach (var p in pages)
                if (p != null && p.GetComponent<TView>() != null) return p;
            throw new InvalidOperationException($"Page manquante dans ScreenCatalog : {typeof(TView).Name}");
        }

#if UNITY_EDITOR
        /// <summary>Remplissage par le générateur de prefabs (éditeur).</summary>
        public void Fill(GameObject? login, GameObject? hub, GameObject? board, GameObject? combat,
                         GameObject? modalLayer, GameObject? toasts, GameObject[] popups, GameObject[] pages, GameObject? tutorialOverlay = null)
        {
            this.login = login; this.hub = hub; this.board = board; this.combat = combat;
            this.modalLayer = modalLayer; this.toasts = toasts; this.popups = popups; this.pages = pages;
            this.tutorialOverlay = tutorialOverlay;
        }
#endif
    }
}
