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
        [TestCase("Pages/Settings.prefab")]
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

