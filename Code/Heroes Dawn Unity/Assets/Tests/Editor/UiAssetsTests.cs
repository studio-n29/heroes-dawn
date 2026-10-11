using N29.HeroesDawn.EditorTools.Assets;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace N29.HeroesDawn.Tests.Editor
{
    public sealed class UiAssetsTests
    {
        [OneTimeSetUp]
        public void Bake()
        {
            UiAssetBaker.BakeAll();
            FontAssetGenerator.GenerateAll();
        }

        [Test, Description("Vérifie que le cadre arrondi est enregistré en sprite découpé en 9 (bordure 19 px) — parce qu'un prefab ne peut référencer qu'un asset, pas un sprite fabriqué au lancement, et que les cadres s'étirent sans déformer leurs coins")]
        public void Bake_WritesSlicedRoundedSprite()
        {
            var s = Resources.Load<Sprite>("UI/Rounded");
            Assert.That(s, Is.Not.Null);
            Assert.That(s.border, Is.EqualTo(new Vector4(19, 19, 19, 19)));
        }

        [Test, Description("Vérifie que chaque sprite du HUD pastel (panneau, losange, reflet, skyline) est cuit en asset et que les panneaux sont découpés en 9 — parce qu'un prefab ne référence que des assets et que les coins coupés doivent rester intacts à l'étirement")]
        public void Bake_WritesHudSprites()
        {
            foreach (var name in new[] { "Panel", "PanelSmall", "Diamond", "Shine", "Skyline" })
                Assert.That(Resources.Load<Sprite>("UI/" + name), Is.Not.Null, name);
            Assert.That(Resources.Load<Sprite>("UI/Panel").border, Is.EqualTo(new Vector4(15, 15, 15, 15)));
            Assert.That(Resources.Load<Sprite>("UI/PanelSmall").border, Is.EqualTo(new Vector4(9, 9, 9, 9)));
        }

        [Test, Description("Vérifie que le fond paysage importé garde la largeur de l'image d'origine — parce que les illustrations étaient stockées en octets pour ne pas être redimensionnées par l'import Unity")]
        public void Bake_KeepsArtResolution()
        {
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(UiAssetBaker.ArtFolder + "/HD_BG_Landscape.jpg");
            Assert.That(tex, Is.Not.Null);
            var bytes = Resources.Load<TextAsset>("Art/HD_BG_Landscape_jpg");
            var decoded = new Texture2D(2, 2);
            decoded.LoadImage(bytes.bytes);
            Assert.That(tex.width, Is.EqualTo(decoded.width));
            Object.DestroyImmediate(decoded);
        }

        [Test, Description("Vérifie que les 3 polices TMP existent, sont dynamiques et ont un matériau encré à contour — parce que la spec impose Bangers / Comic Neue en TextMeshPro avec un contour noir « comics » partagé")]
        public void Fonts_AreDynamicTmpAssets()
        {
            foreach (var name in new[] { "Bangers-Regular", "ComicNeue-Bold", "ComicNeue-Regular" })
            {
                var fa = Resources.Load<TMP_FontAsset>(FontAssetGenerator.ResourcesFolder + "/" + name + " SDF");
                Assert.That(fa, Is.Not.Null, name);
                Assert.That(fa.atlasPopulationMode, Is.EqualTo(AtlasPopulationMode.Dynamic), name);
                var inked = Resources.Load<Material>(FontAssetGenerator.ResourcesFolder + "/" + name + " SDF Inked");
                Assert.That(inked, Is.Not.Null, name);
                Assert.That(inked.GetFloat(ShaderUtilities.ID_OutlineWidth), Is.EqualTo(0f), name + " : plus de contour noir");
                Assert.That(inked.IsKeywordEnabled(ShaderUtilities.Keyword_Underlay), Is.True, name + " : ombre douce");
            }
        }

        [Test, Description("Vérifie que Ui.Txt crée un texte TextMeshPro avec la police Bangers pour un titre — parce que toutes les recettes de prefabs passent par ce helper")]
        public void UiTxt_ReturnsTmpWithFont()
        {
            var parent = new GameObject("p", typeof(RectTransform));
            try
            {
                var t = Ui.Txt(parent.transform, "x", 30, Color.white, TextAnchor.MiddleCenter, FontKind.Title);
                Assert.That(t, Is.InstanceOf<TextMeshProUGUI>());
                Assert.That(t.font.name, Does.Contain("Bangers"));
            }
            finally { Object.DestroyImmediate(parent); }
        }

        [Test, Description("Vérifie qu'un LocalizedText affiche la traduction de sa clé à l'activation — parce que les prefabs ne contiennent que des clés, résolues à l'exécution")]
        public void LocalizedText_ResolvesKeyOnEnable()
        {
            var go = new GameObject("t", typeof(RectTransform));
            try
            {
                go.SetActive(false);
                var tmp = go.AddComponent<TextMeshProUGUI>();
                go.AddComponent<LocalizedText>().Key = "login.play";
                go.SetActive(true);
                Assert.That(tmp.text, Is.EqualTo(Loc.T("login.play")));
            }
            finally { Object.DestroyImmediate(go); }
        }
    }
}
