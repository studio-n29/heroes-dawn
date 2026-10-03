#nullable enable
using System.Collections;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Presentation.Login;
using N29.HeroesDawn.Presentation.Screens;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace N29.HeroesDawn.Tests.PlayMode
{
    /// <summary>Démarre le jeu par la scène Boot avec des doubles de test, puis nettoie toutes les scènes.</summary>
    public static class PlayModeHarness
    {
        /// <summary>Charge Boot avec les doubles injectés dans le scope racine, puis attend l'écran de connexion.</summary>
        public static IEnumerator Boot(MemoryProfileGateway profiles, InstantAuthGateway auth)
        {
            using (LifetimeScope.Enqueue(b =>
                   {
                       b.RegisterInstance<IProfileGateway>(profiles);
                       b.RegisterInstance<IAuthGateway>(auth);
                   }))
            {
                SceneManager.LoadScene("Boot", LoadSceneMode.Single);
                yield return null; // la scène se charge (Awake des scopes) pendant que les doubles sont en file
            }
            for (int i = 0; i < 300 && !Visible<LoginView>(); i++) yield return null;
        }

        /// <summary>Présentateur du QG (scope de la scène Hub chargée).</summary>
        public static N29.HeroesDawn.Presentation.Hub.HubPresenter HubPresenter()
            => Object.FindAnyObjectByType<N29.HeroesDawn.Composition.HubLifetimeScope>()!.Container.Resolve<N29.HeroesDawn.Presentation.Hub.HubPresenter>();

        /// <summary>Présentateur du plateau (scope de la scène Board chargée, même masquée).</summary>
        public static N29.HeroesDawn.Presentation.Board.BoardPresenter BoardPresenter()
            => Object.FindAnyObjectByType<N29.HeroesDawn.Composition.BoardLifetimeScope>()!.Container.Resolve<N29.HeroesDawn.Presentation.Board.BoardPresenter>();

        /// <summary>Toucher réel d'une case de la carte du plateau.</summary>
        public static void TapNode(N29.HeroesDawn.Core.CaseId id)
            => Object.FindAnyObjectByType<N29.HeroesDawn.Presentation.Board.BoardMapView>()!.NodeButton(id).onClick.Invoke();

        /// <summary>« Retour QG » sur le plateau puis « Valider » dans la confirmation.</summary>
        public static IEnumerator BackToHqConfirmed()
        {
            Object.FindAnyObjectByType<N29.HeroesDawn.Presentation.Board.BoardView>()!.BackButton.onClick.Invoke();
            yield return WaitFor(() => Object.FindAnyObjectByType<N29.HeroesDawn.Presentation.Common.ConfirmPopupView>() != null);
            Object.FindAnyObjectByType<N29.HeroesDawn.Presentation.Common.ConfirmPopupView>()!.OkButton.Button.onClick.Invoke();
        }

        /// <summary>« Continuer » sur les résultats du combat (clic réel) ; false tant qu'ils ne sont pas affichés.</summary>
        public static bool ContinueResults()
        {
            var results = Object.FindAnyObjectByType<N29.HeroesDawn.Presentation.Combat.CombatResultsPopupView>();
            if (results == null) return false;
            results.ContinueButton.onClick.Invoke();
            return true;
        }

        public static bool Visible<T>() where T : Component
        {
            var s = Object.FindAnyObjectByType<T>();
            return s != null && s.gameObject.activeInHierarchy;
        }

        public static IEnumerator WaitFor(System.Func<bool> condition, int frames = 300)
        {
            for (int i = 0; i < frames && !condition(); i++) yield return null;
        }

        /// <summary>Remplace toutes les scènes par une scène vide et détruit les objets persistants.</summary>
        public static IEnumerator TearDown()
        {
            var empty = SceneManager.CreateScene("Empty-" + System.Guid.NewGuid().ToString("N"));
            SceneManager.SetActiveScene(empty);
            for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
            {
                var s = SceneManager.GetSceneAt(i);
                if (s != empty && s.isLoaded) yield return SceneManager.UnloadSceneAsync(s);
            }
            foreach (var es in Object.FindObjectsByType<UnityEngine.EventSystems.EventSystem>(FindObjectsSortMode.None))
                Object.Destroy(es.gameObject);
            yield return null;
        }
    }
}
