#nullable enable
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.Assets
{
    /// <summary>
    /// Dossier d'extension Aube tactique (agents détourés, icônes de compétence, objets, composants, mini-jeux…) :
    /// import en sprites (9-slice pour les composants extensibles) puis copie dans Resources/UI/AubeTactique/Ext pour que le jeu puisse les charger.
    /// </summary>
    public static class AubeTactiqueExtensionSync
    {
        public const string Source = "Assets/_Project/Art/UI/AubeTactiqueExtension";
        public const string Target = "Assets/_Project/Resources/UI/AubeTactique/Ext";

        /// <summary>
        /// Groupes de la source qui ne sont chargés par aucun écran du jeu (vérifié dans le code, les UXML et les USS) : exports haute définition,
        /// planches de concept, masques et calques de carte, dossiers, scènes, poses, rivaux, chapitres. Ils restent dans le dossier d'extension
        /// (source de travail de la direction artistique) mais ne sont pas copiés dans Resources, dont tout le contenu entre dans le build
        /// (2,4 Go de textures dans l'APK de mesure du 2026-11-12, dont 1,4 Go de ces groupes).
        /// Pour en utiliser un : le retirer d'ici, puis donner son budget dans <see cref="Budget"/>.
        /// </summary>
        public static readonly HashSet<string> ExcludedGroups = new HashSet<string>
        {
            "Boards", "BrandIcons", "Chapters", "ChaptersLocked", "Decorations", "Dossiers", "Enemies", "Exports", "IconsGold",
            "MapMasks", "MapOverlays", "MiniGameIntros", "Poses", "ResourceIcons", "Resources", "Rivals", "Scenes",
        };

        public static string GroupOf(string relative) { int i = relative.IndexOf('/'); return i < 0 ? relative : relative.Substring(0, i); }

        public static bool IsExcluded(string relative) => ExcludedGroups.Contains(GroupOf(relative));

        /// <summary>
        /// Budget d'une illustration dans le build : taille maximale et compression. Les pastilles (objets, compétences) s'affichent à moins de
        /// 300 px : inutile de les embarquer à 1254 px en RGBA32 (6 Mo chacune). Les petits composants 9-slice restent non compressés.
        /// </summary>
        public static (int MaxSize, bool Compressed) Budget(string relative)
        {
            switch (GroupOf(relative))
            {
                case "Items": case "Skills": return (512, true);
                case "Icons": return (256, false);
                case "Components": return (1024, false);
                case "Cards": return (512, true);
                case "MiniGames": return (1024, true);
                case "Agents": return (1024, true);
                case "Zones": return (2048, true);
                default: return (1024, true);
            }
        }

        /// <summary>Composants extensibles : bordures 9-slice (gauche, bas, droite, haut) en pixels.</summary>
        static readonly Dictionary<string, Vector4> Borders = new Dictionary<string, Vector4>
        {
            { "Components/objectives-paper", new Vector4(48, 40, 48, 48) },
            { "Components/reward-tile", new Vector4(40, 40, 40, 40) },
            { "Components/leaderboard-row", new Vector4(48, 24, 48, 24) },
            { "Components/leaderboard-player", new Vector4(48, 24, 48, 24) },
            { "Components/node-detail", new Vector4(48, 40, 48, 40) },
            { "Components/spiral-notebook", new Vector4(48, 48, 48, 48) },
        };

        public static int Run()
        {
            if (!Directory.Exists(Source)) return 0;
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            foreach (var group in ExcludedGroups) if (AssetDatabase.IsValidFolder(Target + "/" + group)) AssetDatabase.DeleteAsset(Target + "/" + group); // copies déjà faites
            int count = 0;
            foreach (var file in Directory.GetFiles(Source, "*.png", SearchOption.AllDirectories))
            {
                string path = file.Replace('\\', '/');
                string relative = path.Substring(Source.Length + 1);
                if (IsExcluded(relative)) continue;
                string dest = Target + "/" + relative;
                Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
                Configure(path, relative);
                // Copie plutôt que déplacement : le dossier d'extension reste la source de travail de la direction artistique.
                if (File.Exists(dest) && new FileInfo(dest).Length == new FileInfo(path).Length) { Configure(dest, relative); count++; continue; } // copie déjà à jour
                if (File.Exists(dest)) AssetDatabase.DeleteAsset(dest);
                if (!AssetDatabase.CopyAsset(path, dest)) continue;
                Configure(dest, relative);
                count++;
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[Heroes Dawn] Extension Aube tactique : " + count + " sprites importés et copiés dans Resources.");
            return count;
        }

        static void Configure(string path, string relative)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return;
            var wantedBorder = Borders.TryGetValue(relative.Replace(".png", ""), out var wb) ? wb : Vector4.zero;
            var budget = Budget(relative);
            var wantedCompression = budget.Compressed ? TextureImporterCompression.CompressedHQ : TextureImporterCompression.Uncompressed;
            if (importer.textureType == TextureImporterType.Sprite && importer.spriteBorder == wantedBorder && importer.spritePixelsPerUnit == 100
                && importer.maxTextureSize == budget.MaxSize && importer.textureCompression == wantedCompression) return; // déjà configuré
            string key = relative.Replace(".png", "");
            bool photo = relative.StartsWith("Skills/") || relative.StartsWith("Dossiers/") || relative.StartsWith("Cards/");
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = !photo;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = budget.MaxSize;
            importer.textureCompression = wantedCompression;
            importer.spriteBorder = Borders.TryGetValue(key, out var b) ? b : Vector4.zero;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }
    }
}
