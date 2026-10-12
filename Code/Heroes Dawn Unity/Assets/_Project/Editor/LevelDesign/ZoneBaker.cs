#nullable enable
using System.IO;
using System.Linq;
using N29.HeroesDawn.Authoring;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Definitions;
using UnityEditor;
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.LevelDesign
{
    /// <summary>Fichiers de l'outil : génération des fonds de carte (PNG importé en sprite) et import des zones du prototype.</summary>
    public static class ZoneBaker
    {
        public const string Folder = "Assets/_Project/Resources/Zones";

        public static string BackgroundPath(ZoneAsset z) => Folder + "/" + z.zoneId + "_bg.png";

        /// <summary>Génère le fond (biome, météo, zones peintes, graine), l'écrit en PNG dans Resources/Zones et renseigne la zone.</summary>
        public static void BakeBackground(ZoneAsset z)
        {
            var bg = LevelEditorModel.BuildBackground(z);
            var tex = MapRasterizer.Render(bg);
            try
            {
                Directory.CreateDirectory(Folder);
                string path = BackgroundPath(z);
                File.WriteAllBytes(path, tex.EncodeToPNG());
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.alphaIsTransparency = false;
                importer.maxTextureSize = 4096;
                importer.textureCompression = TextureImporterCompression.Compressed;
                importer.SaveAndReimport();
            }
            finally { Object.DestroyImmediate(tex); }
            var b = LevelEditorModel.GridBounds(z);
            z.backgroundKey = z.zoneId + "_bg";
            z.backgroundMinX = b.MinX; z.backgroundMinY = b.MinY; z.backgroundMaxX = b.MaxX; z.backgroundMaxY = b.MaxY;
            EditorUtility.SetDirty(z);
        }

        public static void RemoveBackground(ZoneAsset z)
        {
            string path = BackgroundPath(z);
            if (File.Exists(path)) AssetDatabase.DeleteAsset(path);
            z.backgroundKey = "";
            EditorUtility.SetDirty(z);
        }

        public static ZoneAsset NewZone(string zoneId)
        {
            Directory.CreateDirectory(Folder);
            string path = AssetDatabase.GenerateUniqueAssetPath(Folder + "/" + zoneId + ".asset");
            var z = ScriptableObject.CreateInstance<ZoneAsset>();
            z.zoneId = zoneId;
            AssetDatabase.CreateAsset(z, path);
            AssetDatabase.SaveAssets();
            return z;
        }

        public static CampaignAsset NewCampaign()
        {
            Directory.CreateDirectory(Folder);
            string path = AssetDatabase.GenerateUniqueAssetPath(Folder + "/Campaign.asset");
            var c = ScriptableObject.CreateInstance<CampaignAsset>();
            AssetDatabase.CreateAsset(c, path);
            AssetDatabase.SaveAssets();
            return c;
        }

        /// <summary>
        /// Importe les zones du prototype (code) en assets modifiables, avec leur fond généré, et la campagne qui les ordonne.
        /// Les assets existants du même identifiant sont mis à jour. Une fois importée, la zone du dossier remplace celle du code.
        /// </summary>
        public static CampaignAsset ImportPrototype(bool bake = true)
        {
            var content = PrototypeContent.Build();
            var campaign = AssetDatabase.LoadAssetAtPath<CampaignAsset>(Folder + "/Campaign.asset") ?? NewCampaign();
            campaign.entries.Clear();
            foreach (var def in content.Zones)
            {
                string path = Folder + "/" + def.Id.Value + ".asset";
                var asset = AssetDatabase.LoadAssetAtPath<ZoneAsset>(path) ?? NewZone(def.Id.Value);
                ZoneAssetConverter.FillFrom(asset, def, key => N29.HeroesDawn.Presentation.Common.Loc.Has(key) ? N29.HeroesDawn.Presentation.Common.Loc.T(key) : "");
                if (bake) BakeBackground(asset);
                EditorUtility.SetDirty(asset);
                campaign.entries.Add(new CampaignEntry { zone = asset, act = def.Act, codeOverride = "", requires = def.Requires.Select(r => r.Value).ToList() });
            }
            EditorUtility.SetDirty(campaign);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return campaign;
        }

        /// <summary>Entrée en ligne de commande : <c>-executeMethod N29.HeroesDawn.EditorTools.LevelDesign.ZoneBaker.ImportPrototypeBatch -quit</c>.</summary>
        public static void ImportPrototypeBatch() => ImportPrototype();
    }
}
