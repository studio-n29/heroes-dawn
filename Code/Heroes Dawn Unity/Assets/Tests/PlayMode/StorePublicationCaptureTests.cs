using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Tests.PlayMode
{
    // Capture-only adapter: reproduce the release HUD, where DebugVisible is false.
    // It does not change production views, game state, textures, or combat rules.
    public sealed class StorePublicationCaptureTests
    {
        [UnityTest, Explicit("Store artwork capture"), Timeout(180000), Description("Capture un combat de la version publiée pour les visuels des stores (test explicite, à lancer à la main) — parce que les fiches Google Play et App Store ont besoin d'images du vrai jeu")]
        public IEnumerator CaptureReleaseCombat()
        {
            var go = new GameObject("StoreCaptureReleaseHud");
            Object.DontDestroyOnLoad(go);
            go.AddComponent<StoreCaptureReleaseHud>();
            yield return new ScreenCaptureTests().CaptureEveryScreen();
            Object.Destroy(go);
        }

        [UnityTearDown]
        public IEnumerator TearDown() => PlayModeHarness.TearDown();
    }

    public sealed class StoreCaptureReleaseHud : MonoBehaviour
    {
        void LateUpdate()
        {
            foreach (var doc in Object.FindObjectsByType<UIDocument>(FindObjectsSortMode.None))
            {
                if (doc == null || doc.rootVisualElement == null) continue;
                var debug = doc.rootVisualElement.Q<Button>("debugWin");
                if (debug != null) debug.style.display = DisplayStyle.None;
            }
        }
    }
}
