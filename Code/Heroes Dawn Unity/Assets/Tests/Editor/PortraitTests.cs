#nullable enable
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.EditorTools.Prefabs;
using N29.HeroesDawn.Presentation.Common;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace N29.HeroesDawn.Tests.Editor
{
    public sealed class PortraitTests
    {
        GameObject? _go;

        PortraitView Spawn()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabGenerator.DefaultRoot}/Widgets/Portrait.prefab");
            Assert.That(prefab, Is.Not.Null, "prefab de portrait généré");
            _go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            return _go.GetComponent<PortraitView>();
        }

        [TearDown] public void TearDown() { if (_go != null) Object.DestroyImmediate(_go); }

        [Test, Description("Vérifie qu'un agent sans illustration affiche la silhouette à la couleur de sa famille et jamais un portrait vide — parce que les portraits illustrés arriveront plus tard")]
        public void AgentWithoutArt_ShowsTheFamilySilhouette()
        {
            var view = Spawn();
            foreach (var family in new[] { Family.Matter, Family.Elemental, Family.PhysicalBio, Family.Perception })
            {
                view.Show(new PortraitData("Anchor", family));
                Assert.That(view.Bust.gameObject.activeSelf, Is.True, family.ToString());
                Assert.That(view.Art.gameObject.activeSelf, Is.False, family.ToString());
                Assert.That(view.Bust.sprite, Is.SameAs(Sprites.Bust));
                Assert.That(view.Bust.color, Is.Not.EqualTo(view.Disc.color), "la silhouette se détache du disque");
            }
        }

        [Test, Description("Vérifie qu'une illustration fournie remplace la silhouette — parce que l'emplacement d'image est prévu pour les vrais portraits")]
        public void AgentWithArt_ShowsTheIllustration()
        {
            var view = Spawn();
            var tex = new Texture2D(4, 4);
            var art = Sprite.Create(tex, new Rect(0, 0, 4, 4), Vector2.one * 0.5f);
            try
            {
                view.Show(new PortraitData("Anchor", Family.Matter, false, art));
                Assert.That(view.Art.gameObject.activeSelf, Is.True);
                Assert.That(view.Art.sprite, Is.SameAs(art));
                Assert.That(view.Bust.gameObject.activeSelf, Is.False);
            }
            finally { Object.DestroyImmediate(art); Object.DestroyImmediate(tex); }
        }

        [Test, Description("Vérifie que la couronne de boss et l'initiale restent affichées avec la silhouette — parce que l'initiale identifie l'agent et la couronne le boss")]
        public void InitialAndBossCrown_AreKept()
        {
            var view = Spawn();
            view.Show(new PortraitData("Voyou", Family.PhysicalBio, true));
            Assert.That(_go!.GetComponentInChildren<TMPro.TMP_Text>(true).text, Is.EqualTo("V"));
            Assert.That(_go.transform.Find("Boss").gameObject.activeSelf, Is.True);
        }
    }
}
