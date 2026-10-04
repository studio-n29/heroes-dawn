using System;
using System.Linq;
using N29.HeroesDawn.Composition;
using N29.HeroesDawn.EditorTools.Prefabs;
using N29.HeroesDawn.Presentation.Mvp;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace N29.HeroesDawn.Tests.Editor
{
    public sealed class ScreenCatalogTests
    {
        interface IUnknownPopup : IPopupView { }

        static ScreenCatalog Catalog => AssetDatabase.LoadAssetAtPath<ScreenCatalog>(PrefabGenerator.CatalogPath);

        [Test, Description("Vérifie que le catalogue référence les prefabs générés (couche des modales, toasts, confirmation, et chaque écran dont le prefab existe) — parce que les scopes instancient les écrans depuis le catalogue (spec §4)")]
        public void CatalogAsset_ReferencesExistingPrefabs()
        {
            var c = Catalog;
            Assert.That(c, Is.Not.Null);
            Assert.That(c.ModalLayer, Is.Not.Null);
            Assert.That(c.Toasts, Is.Not.Null);
            Assert.That(c.PopupFor<IConfirmPopupView>(), Is.Not.Null);
            foreach (var scene in new[] { "Login", "Hub", "Board", "Combat" })
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabGenerator.DefaultRoot}/Screens/{scene}.prefab");
                if (prefab != null) Assert.That(c.Screen(scene), Is.SameAs(prefab), scene);
            }
        }

        [Test, Description("Vérifie qu'une popup absente du catalogue lève une erreur explicite — parce qu'un prefab manquant doit être signalé clairement, pas par une référence nulle (spec §8)")]
        public void PopupFor_UnknownView_ThrowsExplicitMessage()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => Catalog.PopupFor<IUnknownPopup>());
            Assert.That(ex.Message, Does.Contain("ScreenCatalog"));
        }

        [Test, Description("Vérifie qu'une page du hub se retrouve par le type de sa vue — parce que la fabrique de pages instancie chaque page depuis le catalogue (spec §3.3)")]
        public void PageFor_ReturnsPrefabHoldingTheView()
        {
            var c = ScriptableObject.CreateInstance<ScreenCatalog>();
            var page = new GameObject("Page", typeof(RectTransform), typeof(UnityEngine.UI.Image));
            try
            {
                c.Fill(null, null, null, null, null, null, Array.Empty<GameObject>(), new[] { page });
                Assert.That(c.PageFor<UnityEngine.UI.Image>(), Is.SameAs(page));
            }
            finally { UnityEngine.Object.DestroyImmediate(page); UnityEngine.Object.DestroyImmediate(c); }
        }

        [Test, Description("Vérifie qu'une page absente du catalogue lève une erreur explicite — parce qu'un prefab manquant doit être signalé clairement")]
        public void PageFor_Missing_ThrowsExplicitMessage()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => Catalog.PageFor<IUnknownPopup>());
            Assert.That(ex.Message, Does.Contain("ScreenCatalog"));
        }

        [Test, Description("Vérifie que le scope racine et les scopes des scènes d'écran référencent le catalogue — parce que sans lui aucun écran ne peut être instancié (spec §4)")]
        public void GeneratedScenes_ScopesReferenceCatalog()
        {
            foreach (var name in new[] { "Boot", "Hub", "Board", "Combat" })
            {
                var scene = EditorSceneManager.OpenScene($"Assets/_Project/Scenes/{name}.unity", OpenSceneMode.Additive);
                try
                {
                    var scope = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<VContainer.Unity.LifetimeScope>(true)).First();
                    var prop = new SerializedObject(scope).FindProperty("catalog");
                    Assert.That(prop, Is.Not.Null, name);
                    Assert.That(prop.objectReferenceValue, Is.SameAs(Catalog), name);
                }
                finally { EditorSceneManager.CloseScene(scene, true); }
            }
        }
    }
}
