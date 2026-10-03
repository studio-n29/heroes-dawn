using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.EditorTools.Prefabs;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Tests.Editor
{
    public sealed class PrefabGeneratorTests
    {
        string _root = null!;

        [SetUp]
        public void SetUp()
        {
            _root = "Assets/_GenTest_" + System.Guid.NewGuid().ToString("N").Substring(0, 8);
            AssetDatabase.CreateFolder("Assets", _root.Substring("Assets/".Length));
        }

        [TearDown]
        public void TearDown() => AssetDatabase.DeleteAsset(_root);

        string PathOf(string rel) => _root + "/" + rel;

        static Color FillColor(GameObject prefabRoot) => prefabRoot.GetComponentInChildren<Image>(true).color;

        void EditFirstImage(string rel, Color c)
        {
            var contents = PrefabUtility.LoadPrefabContents(PathOf(rel));
            contents.GetComponentInChildren<Image>(true).color = c;
            PrefabUtility.SaveAsPrefabAsset(contents, PathOf(rel));
            PrefabUtility.UnloadPrefabContents(contents);
        }

        static List<string> Hierarchy(string path)
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            return go.GetComponentsInChildren<Transform>(true).Select(t => AnimationUtility.CalculateTransformPath(t, go.transform)).ToList();
        }

        [Test, Description("Vérifie qu'une génération sur un dossier vide crée un prefab par recette — parce que le menu doit produire tous les écrans en une fois (spec §4)")]
        public void Generate_EmptyRoot_CreatesEveryRecipe()
        {
            var report = PrefabGenerator.Generate(_root, OverwritePolicy.Never);
            Assert.That(report.Failed, Is.Empty, string.Join("\n", report.Failed));
            foreach (var r in PrefabGenerator.Recipes)
                Assert.That(AssetDatabase.LoadAssetAtPath<GameObject>(PathOf(r.RelativePath)), Is.Not.Null, r.RelativePath);
        }

        [Test, Description("Vérifie qu'une retouche manuelle d'un prefab survit à une régénération sans écrasement — parce que les prefabs font foi une fois générés (spec §4, critère de fin 6, Review Focus 3)")]
        public void Generate_Never_DoesNotOverwriteEditedPrefab()
        {
            PrefabGenerator.Generate(_root, OverwritePolicy.Never);
            EditFirstImage("Widgets/Portrait.prefab", Color.magenta);
            var report = PrefabGenerator.Generate(_root, OverwritePolicy.Never);
            Assert.That(report.Skipped, Does.Contain("Widgets/Portrait.prefab"));
            Assert.That(FillColor(AssetDatabase.LoadAssetAtPath<GameObject>(PathOf("Widgets/Portrait.prefab"))), Is.EqualTo(Color.magenta));
        }

        [Test, Description("Vérifie qu'un écrasement demandé remet le prefab à sa recette — parce que l'écrasement doit rester possible, mais seulement sur demande explicite (spec §4)")]
        public void Generate_All_OverwritesPrefab()
        {
            PrefabGenerator.Generate(_root, OverwritePolicy.Never);
            var original = FillColor(AssetDatabase.LoadAssetAtPath<GameObject>(PathOf("Widgets/Portrait.prefab")));
            EditFirstImage("Widgets/Portrait.prefab", Color.magenta);
            var report = PrefabGenerator.Generate(_root, OverwritePolicy.All);
            Assert.That(report.Overwritten, Does.Contain("Widgets/Portrait.prefab"));
            Assert.That(FillColor(AssetDatabase.LoadAssetAtPath<GameObject>(PathOf("Widgets/Portrait.prefab"))), Is.EqualTo(original));
        }

        [Test, Description("Vérifie que deux générations produisent la même hiérarchie pour chaque prefab — parce que la génération doit être déterministe (spec §4)")]
        public void Generate_Twice_SameHierarchy()
        {
            PrefabGenerator.Generate(_root, OverwritePolicy.All);
            var first = PrefabGenerator.Recipes.ToDictionary(r => r.RelativePath, r => Hierarchy(PathOf(r.RelativePath)));
            PrefabGenerator.Generate(_root, OverwritePolicy.All);
            foreach (var r in PrefabGenerator.Recipes)
                Assert.That(Hierarchy(PathOf(r.RelativePath)), Is.EqualTo(first[r.RelativePath]), r.RelativePath);
        }

        [Test, Description("Vérifie qu'une retouche d'un widget se retrouve dans les prefabs qui l'imbriquent — parce que les widgets sont des prefabs imbriqués, retouchés une seule fois (spec §4, Review Focus 3)")]
        public void NestedWidgetEdit_PropagatesToScreens()
        {
            PrefabGenerator.Generate(_root, OverwritePolicy.Never);
            EditFirstImage("Widgets/ResourcePill.prefab", Color.magenta);
            var wallet = AssetDatabase.LoadAssetAtPath<GameObject>(PathOf("Widgets/Wallet.prefab"));
            var pills = wallet.GetComponentsInChildren<Transform>(true)
                              .Where(t => PrefabUtility.IsAnyPrefabInstanceRoot(t.gameObject)).ToList();
            Assert.That(pills.Count, Is.EqualTo(2));
            foreach (var p in pills) Assert.That(FillColor(p.gameObject), Is.EqualTo(Color.magenta));
        }

        [Test, Description("Vérifie que -overwrite liste les seuls prefabs à écraser et qu'en son absence rien n'est sélectionné — parce qu'écraser tous les prefabs renouvelle leurs identifiants internes et casse les retouches imbriquées")]
        public void BatchArgs_Overwrite_SelectsListedPrefabsOnly()
        {
            var selected = PrefabGenerator.SelectedFromArgs(new[] { "Unity.exe", "-overwrite", "Screens/Hub.prefab,Widgets/RecruitCard.prefab", "-quit" });
            Assert.That(selected, Is.EquivalentTo(new[] { "Screens/Hub.prefab", "Widgets/RecruitCard.prefab" }));
            Assert.That(PrefabGenerator.SelectedFromArgs(new[] { "Unity.exe", "-quit" }), Is.Null);
        }
    }
}
