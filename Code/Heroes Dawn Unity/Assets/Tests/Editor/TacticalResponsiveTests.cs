using N29.HeroesDawn.Presentation;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Tests.Editor
{
    public sealed class TacticalResponsiveTests
    {
        [TestCase(1920, 1080, 0)]
        [TestCase(2160, 972, 90)]
        [TestCase(2048, 1536, 0)]
        [TestCase(2560, 1080, 120)]
        [TestCase(1280, 720, 40)]
        [Description("Le viewport sûr garde toute la composition paysage malgré les encoches, le ratio et la résolution")]
        public void SafeViewport_RetainsCompleteLandscapeComposition(int width, int height, int inset)
        {
            var safe = new Rect(inset, 20, width - 2 * inset, height - 40);
            var reference = N29.HeroesDawn.Presentation.SafeArea.ReferenceFor(safe, width, height);
            float scale = Mathf.Min(width / reference.x, height / reference.y);
            Assert.That(safe.width / scale, Is.GreaterThanOrEqualTo(1919.9f));
            Assert.That(safe.height / scale, Is.GreaterThanOrEqualTo(1079.9f));
        }

        [TestCase("Boutique")]
        [TestCase("Grimoire")]
        [TestCase("Inventory")]
        [TestCase("Loadout")]
        [TestCase("Agents")]
        [Description("Toutes les cartes et actions restent accessibles, la grille tient dans son viewport et le détail défile")]
        public void Collection_GridFitsViewport_AndAllActionsRemainAvailable(string page)
        {
            var parent = new GameObject("SafeViewport", typeof(RectTransform));
            try
            {
                ((RectTransform)parent.transform).sizeDelta = new Vector2(1744, 824);
                var asset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Pages/" + page + ".prefab");
                var go = Object.Instantiate(asset, parent.transform);
                ((RectTransform)go.transform).Stretch();
                var view = go.GetComponent<CardGridPageView>();
                view.ShowHeader(new ListHeaderData { Title = "Collection", Line = "Description", Progress = -1,
                    A = RowButtonData.Of("Action principale"), B = RowButtonData.Of("Action secondaire") });
                view.ShowFilters(new[] { new FilterGroupData { Labels = new[] { "Toutes", "Matière", "Élémentaire", "Physique / Bio", "Perception" }, Selected = 0 } });
                var data = new InfoRowData[15];
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = InfoRowData.Basic("Carte " + i, "Texte de description long qui doit rester consultable.");
                    data[i].A = RowButtonData.Of("Équiper"); data[i].B = RowButtonData.Of("Retirer"); data[i].C = RowButtonData.Of("Détail");
                }
                view.ShowRows(data);
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)go.transform);
                go.GetComponent<TacticalPageLayout>().Reflow();
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)go.transform);
                var grid = go.GetComponentInChildren<GridLayoutGroup>();
                float used = grid.constraintCount * grid.cellSize.x + (grid.constraintCount - 1) * grid.spacing.x + grid.padding.horizontal;
                Assert.That(used, Is.LessThanOrEqualTo(((RectTransform)grid.transform.parent).rect.width + .1f));
                Assert.That(view.Tiles.Count, Is.EqualTo(15));
                Assert.That(grid.cellSize.x, Is.GreaterThan(200));
                foreach (var button in go.GetComponentsInChildren<UiButton>())
                    Assert.That(button.Rect.rect.width, Is.GreaterThan(80), button.name);
                var details = go.transform.Find("Detail/Fill/TextArea").GetComponentInChildren<ScrollRect>();
                Assert.That(details.viewport.rect.height, Is.GreaterThan(100));
                Assert.That(details.vertical, Is.True);
            }
            finally { Object.DestroyImmediate(parent); }
        }
    }
}

