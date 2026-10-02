using System.Collections.Generic;
using System.IO;
using N29.HeroesDawn.Content.Prototype;
using UnityEditor;
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.Balance
{
    /// <summary>Menu Heroes Dawn ▸ Équilibrage ▸ Importer : tableur → balance.json (client) + balance.rc (Remote Config).</summary>
    public static class BalanceImportMenu
    {
        public const string ClientJsonPath = "Assets/_Project/Resources/Balance/balance.json";
        public const string RemoteConfigPath = "Assets/_Project/Backend/RemoteConfig/balance.rc";

        public static string DefaultSheetPath =>
            Path.GetFullPath(Path.Combine(Application.dataPath, "../../../01_Game_Design/05_Equilibrage.xlsx"));

        [MenuItem("Heroes Dawn/Équilibrage/Importer 05_Equilibrage.xlsx")]
        public static void ImportDefault()
        {
            var path = DefaultSheetPath;
            if (!File.Exists(path))
            {
                Debug.LogError("[Heroes Dawn] Tableur introuvable : " + path);
                return;
            }

            var warnings = new List<string>();
            string json;
            try
            {
                using var s = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                json = BalanceJson.Serialize(BalanceSheetMapper.Map(XlsxReader.ReadSheet(s, "Paramètres"), warnings));
            }
            catch (System.Exception e) when (e is IOException || e is System.ArgumentException || e is InvalidDataException)
            {
                Debug.LogError($"[Heroes Dawn] Import impossible ({path}) : {e.Message}");
                return;
            }

            Write(ClientJsonPath, json + "\n");
            Write(RemoteConfigPath, RemoteConfigFile.Build(json));
            AssetDatabase.Refresh();
            foreach (var w in warnings) Debug.LogWarning("[Heroes Dawn] " + w);
            Debug.Log($"[Heroes Dawn] Équilibrage importé : {BalanceSheetMapper.MappedCount - warnings.Count} paramètres, {warnings.Count} avertissements");
        }

        static void Write(string assetPath, string content)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(assetPath)!);
            File.WriteAllText(assetPath, content);
        }
    }
}
