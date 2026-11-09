using System.Collections.Generic;
using N29.HeroesDawn.Presentation.Common;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Tests.Editor
{
    public sealed class AubeTactiqueTests
    {
        [TestCase(1920, 1080)]
        [TestCase(2160, 972)]
        [Description("Titre, avatar, état de sauvegarde, monnaies et réglages gardent chacun leur espace en paysage 16:9 et 20:9")]
        public void HubHeader_DoesNotOverlap(int width, int height)
        {
            var parent = new GameObject("Viewport", typeof(RectTransform));
            try
            {
                ((RectTransform)parent.transform).sizeDelta = new Vector2(width, height);
                var asset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Screens/Hub.prefab");
                var go = (GameObject)PrefabUtility.InstantiatePrefab(asset, parent.transform);
                ((RectTransform)go.transform).Stretch();
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)go.transform);
                var top = go.transform.Find("TopBar");
                var names = new[] { "Title", "Avatar", "Wallet", "Settings" };
                for (int i = 0; i < names.Length - 1; i++)
                {
                    var left = new Vector3[4]; var right = new Vector3[4];
                    ((RectTransform)top.Find(names[i])).GetWorldCorners(left);
                    ((RectTransform)top.Find(names[i+1])).GetWorldCorners(right);
                    Assert.That(left[2].x, Is.LessThanOrEqualTo(right[0].x), names[i] + " / " + names[i+1]);
                }
            }
            finally { Object.DestroyImmediate(parent); }
        }

        [TestCase("Pages/Settings.prefab")]
        [TestCase("Widgets/SettingRow.prefab")]
        [Description("Les prefabs conservés sans recette doivent recevoir la même DA que les écrans reconstruits")]
        public void PreservedSettings_HaveNoLegacyPaletteOrOutlinedText(string relative)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/" + relative);
            Assert.That(prefab, Is.Not.Null);
            var legacy = new HashSet<string> { "4B3FA0", "5D58B8", "3A2F7A", "5B8CFF", "FFCF33" };
            foreach (var graphic in prefab.GetComponentsInChildren<Graphic>(true))
                Assert.That(legacy.Contains(ColorUtility.ToHtmlStringRGB(graphic.color)), Is.False, relative + " / " + graphic.name);
            foreach (var text in prefab.GetComponentsInChildren<TMP_Text>(true))
                Assert.That(text.fontSharedMaterial.GetFloat(ShaderUtilities.ID_OutlineWidth), Is.EqualTo(0).Within(0.001f), text.name);
        }

        [Test, Description("Les illustrations de production restent des sprites chargeables dans le jeu")]
        public void ProductionArt_IsAvailable()
        {
            foreach (var key in new[] { "shop-booster", "hq-new-york", "hq-moscow", "combat-red-hook", "room-workshop", "room-operations", "room-grimoire", "room-inventory", "room-register" })
                Assert.That(TacticalArt.Load(key), Is.Not.Null, key);
            foreach (var kind in MiniGameArt.Kinds)
                Assert.That(TacticalArt.Load("mini-" + kind), Is.Not.Null, kind);
        }
    }
}

