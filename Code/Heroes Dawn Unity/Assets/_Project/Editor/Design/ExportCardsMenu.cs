#nullable enable
using System.IO;
using System.Text;
using N29.HeroesDawn.Authoring;
using N29.HeroesDawn.Content.Prototype;
using UnityEditor;
using UnityEngine;

namespace N29.HeroesDawn.Editor
{
    /// <summary>Écrit Resources/Content/cards.json à partir des cartes du code (PrototypeCollection.LegacyCards). À lancer une fois pour amorcer le fichier de données.</summary>
    public static class ExportCardsMenu
    {
        const string AssetPath = "Assets/_Project/Resources/Content/cards.json";

        [MenuItem("Heroes Dawn/Contenu/Exporter les cartes vers cards.json")]
        public static void Export()
        {
            var json = CardDataLibrary.ToJson(PrototypeCollection.LegacyCards());
            Directory.CreateDirectory(Path.GetDirectoryName(AssetPath)!);
            File.WriteAllText(AssetPath, json, new UTF8Encoding(false));
            AssetDatabase.ImportAsset(AssetPath);
            Debug.Log($"cards.json écrit : {AssetPath} ({Encoding.UTF8.GetByteCount(json)} octets)");
        }
    }
}
