using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.EditorTools.Prefabs;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Tests.Editor
{
    /// <summary>Garde-fous de mise en page du hub (coque et pages), mesurés après calcul de la mise en page.</summary>
    public sealed class HubLayoutTests
    {
        const float MinTarget = 96f;

        static IEnumerable<(string Path, Vector2 Area)> HubPrefabs()
        {
            yield return ($"{PrefabGenerator.DefaultRoot}/Screens/HubShell.prefab", new Vector2(1920, 1080));
            foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { PrefabGenerator.DefaultRoot + "/Pages" }))
                yield return (AssetDatabase.GUIDToAssetPath(guid), new Vector2(1744, 860));
        }

        [Test, Description("Vérifie que chaque bouton de la coque du hub et de ses pages mesure au moins 96 × 96 px — parce que les cibles tactiles doivent rester faciles à toucher au doigt (maquettes §1)")]
        public void HubShell_AndPages_TouchTargetsAtLeast96()
        {
            Assert.That(AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabGenerator.DefaultRoot}/Screens/HubShell.prefab"), Is.Not.Null, "coque non générée");
            var small = new List<string>();
            foreach (var (path, area) in HubPrefabs())
            {
                var canvasGo = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
                try
                {
                    var root = (RectTransform)canvasGo.transform;
                    root.sizeDelta = area;
                    var go = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path), root);
                    var rt = (RectTransform)go.transform;
                    if (rt.anchorMin != rt.anchorMax) { rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero; }
                    LayoutRebuilder.ForceRebuildLayoutImmediate(root);
                    Canvas.ForceUpdateCanvases();
                    foreach (var b in go.GetComponentsInChildren<Button>(false))
                    {
                        var r = ((RectTransform)b.transform).rect;
                        if (r.width < MinTarget || r.height < MinTarget)
                            small.Add($"{path} : {b.name} ({r.width:0}×{r.height:0})");
                    }
                }
                finally { Object.DestroyImmediate(canvasGo); }
            }
            Assert.That(small, Is.Empty, string.Join("\n", small));
        }
    }
}
