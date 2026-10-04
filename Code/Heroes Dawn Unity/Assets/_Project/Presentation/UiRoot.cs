#nullable enable
using N29.HeroesDawn.Application.Flow;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Login;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Presentation.Tutorial;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace N29.HeroesDawn.Presentation
{
    /// <summary>
    /// Interface commune de la scène Boot : EventSystem (Input System), écran de connexion et couche modales + toasts
    /// au-dessus de toutes les scènes. Les canvas sont dans la scène générée ; les écrans et couches viennent du catalogue.
    /// </summary>
    public sealed class UiRoot : MonoBehaviour, IScreenVisibility
    {
        [SerializeField] RectTransform? screensRoot;
        [SerializeField] RectTransform? overlayRoot;

        public LoginView Login { get; private set; } = null!;
        public ModalLayer Modals { get; private set; } = null!;
        public ToastsView Toasts { get; private set; } = null!;
        public TutorialOverlayView? Tutorial { get; private set; }
        bool _layersBuilt;

        /// <summary>Instancie une fois la couche des modales, les toasts et la connexion depuis le catalogue (au premier besoin).</summary>
        public void EnsureLayers(ScreenCatalog catalog)
        {
            if (_layersBuilt) return;
            if (screensRoot == null || overlayRoot == null)
                throw new System.InvalidOperationException("Canvas de Boot manquants sur UiRoot : relancer Heroes Dawn ▸ Setup ▸ Générer les scènes.");
            _layersBuilt = true;
            EnsureEventSystem();

            // Le tutoriel passe sous les modales (une confirmation reste utilisable) mais au-dessus des écrans.
            if (catalog.TutorialOverlay != null)
            {
                // Hors de la zone sûre : le voile couvre tout l écran, encoche comprise.
                Tutorial = Instantiate(catalog.TutorialOverlay, overlayRoot.parent).GetComponent<TutorialOverlayView>();
                ((RectTransform)Tutorial.transform).Stretch();
                Tutorial.transform.SetAsFirstSibling();
            }
            Modals = Instantiate(Required(catalog.ModalLayer, "ModalLayer"), overlayRoot).GetComponent<ModalLayer>();
            ((RectTransform)Modals.transform).Stretch();
            Modals.Init(catalog);
            Toasts = Instantiate(Required(catalog.Toasts, "Toasts"), overlayRoot).GetComponent<ToastsView>();
            ((RectTransform)Toasts.transform).Stretch();

            // Écran de connexion (masqué jusqu'à ce que la machine d'états l'affiche).
            Login = Instantiate(catalog.Screen("Login"), screensRoot).GetComponent<LoginView>();
            ((RectTransform)Login.transform).Stretch();
            Login.gameObject.SetActive(false);
        }

        static GameObject Required(GameObject? prefab, string name)
            => prefab != null ? prefab : throw new System.InvalidOperationException("Prefab manquant dans ScreenCatalog : " + name);

        public void SetLoginVisible(bool visible)
        {
            if (visible) Login.transform.SetAsLastSibling();
            Login.gameObject.SetActive(visible);
        }

        static void EnsureEventSystem()
        {
            var existing = Object.FindAnyObjectByType<EventSystem>();
            if (existing != null)
            {
                // Remplace un éventuel StandaloneInputModule (ancien Input Manager) par le module Input System.
                var legacy = existing.GetComponent<StandaloneInputModule>();
                if (legacy != null)
                {
                    Object.DestroyImmediate(legacy);
                    existing.gameObject.AddComponent<InputSystemUIInputModule>();
                }
                return;
            }
            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            Object.DontDestroyOnLoad(es);
        }
    }
}
