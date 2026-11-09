#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.EditorTools.Prefabs;
using N29.HeroesDawn.Presentation.Common;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.EditorTools.Assets
{
    /// <summary>Migre les couleurs sérialisées des prefabs sans recette, sans remplacer leurs dispositions ni leurs liaisons.</summary>
    public static class AubeTactiquePreservedPrefabs
    {
        public static int Apply()
        {
            var generated = new HashSet<string>();
            foreach (var recipe in PrefabGenerator.Recipes) generated.Add(PrefabGenerator.DefaultRoot + "/" + recipe.RelativePath);
            int count = 0;
            foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { PrefabGenerator.DefaultRoot }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (generated.Contains(path)) continue;
                var root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    foreach (var graphic in root.GetComponentsInChildren<Graphic>(true)) graphic.color = Map(graphic.color);
                    foreach (var text in root.GetComponentsInChildren<TMP_Text>(true))
                    {
                        string old = text.font != null ? text.font.name : "";
                        text.font = old.Contains("ExtraBold") || old.Contains("Bangers") ? Ui.TitleFont : old.Contains("Regular") ? Ui.RegularFont : Ui.BodyFont;
                        text.fontSharedMaterial = text.font.material;
                    }
                    foreach (var button in root.GetComponentsInChildren<UiButton>(true))
                    {
                        var serialized = new SerializedObject(button);
                        Color bg = Map(serialized.FindProperty("_bg").colorValue);
                        Color fg = Map(serialized.FindProperty("_fg").colorValue);
                        button.SetColors(bg, fg);
                        button.Fill.material = null;
                    }
                    foreach (var image in root.GetComponentsInChildren<Image>(true))
                    {
                        if (image.name == "Fill" && image.transform.parent.TryGetComponent<Image>(out var frame) && image.sprite == Sprites.Panel)
                        {
                            image.rectTransform.Stretch(1.4f, 1.4f, 1.4f, 1.4f);
                            frame.color = Theme.FrostEdge;
                        }
                        if (image.name == "Face" && image.GetComponentInParent<UiButton>() != null) image.rectTransform.Stretch();
                        if (image.name == "Hi")
                        {
                            image.color = Theme.WithAlpha(Theme.Accent, 0.22f);
                            image.rectTransform.offsetMin = new Vector2(12, -2);
                            image.rectTransform.offsetMax = new Vector2(-12, -1);
                        }
                    }
                    foreach (var shadow in root.GetComponentsInChildren<Shadow>(true))
                    {
                        shadow.effectColor = Theme.WithAlpha(Theme.Outline, 0.28f);
                        shadow.effectDistance = new Vector2(0, -3);
                    }
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                    count++;
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            return count;
        }

        static Color Map(Color value)
        {
            uint rgb = ((uint)Mathf.RoundToInt(value.r*255) << 16) | ((uint)Mathf.RoundToInt(value.g*255) << 8) | (uint)Mathf.RoundToInt(value.b*255);
            Color mapped = rgb switch
            {
                0x17123A => Theme.Outline, 0x4B3FA0 => Theme.Surface, 0x5D58B8 => Theme.Surface2,
                0x3A2F7A => Theme.Surface3, 0x241C57 => Theme.SkyBottom, 0x5B49C4 => Theme.SkyTop,
                0x2D2566 => Theme.Background, 0x5B8CFF => Theme.Secondary, 0xCBC5F5 => Theme.TextMuted,
                0xFFCF33 => Theme.Primary, 0xFFE27A => Theme.AccentSoft, 0xE8A317 => Theme.AccentDark,
                0xE8453C => Theme.Danger, 0x3AD6A0 => Theme.Success, 0x4DA3FF => Theme.Info,
                _ => value,
            };
            mapped.a = value.a;
            return mapped;
        }
    }
}
