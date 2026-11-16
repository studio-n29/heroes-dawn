#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using N29.HeroesDawn.Authoring;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Definitions;
using UnityEditor;
using UnityEngine;

namespace N29.HeroesDawn.Editor
{
    /// <summary>
    /// Écrit Resources/Content/cards.json à partir des cartes du code (PrototypeCollection.LegacyCards) : outil d'amorçage, pas de mise à jour.
    /// Il REFUSE d'écrire si le fichier existe et contient des cartes absentes du code (la vague mise en jeu par <see cref="ImportCatalogMenu"/>) :
    /// il les effacerait. En batch : <c>-executeMethod N29.HeroesDawn.Editor.ExportCardsMenu.ExportBatch</c> (code de sortie 1 en cas de refus).
    /// </summary>
    public static class ExportCardsMenu
    {
        const string AssetPath = "Assets/_Project/Resources/Content/cards.json";

        [MenuItem("Heroes Dawn/Contenu/Exporter les cartes vers cards.json")]
        public static void Export()
        {
            var refusal = Check();
            if (refusal != null)
            {
                Debug.LogError(refusal);
                EditorUtility.DisplayDialog("Exporter les cartes", refusal, "OK");
                return;
            }
            Write();
        }

        /// <summary>Point d'entrée de <c>-executeMethod</c> : mêmes effets que le menu, sans boîte de dialogue ; code de sortie 1 si l'écriture est refusée.</summary>
        public static void ExportBatch()
        {
            var refusal = Check();
            if (refusal != null)
            {
                Debug.LogError(refusal);
                EditorApplication.Exit(1);
                return;
            }
            Write();
        }

        static string? Check()
            => RefusalReason(File.Exists(AssetPath) ? File.ReadAllText(AssetPath, Encoding.UTF8) : null, PrototypeCollection.LegacyCards());

        static void Write()
        {
            var json = CardDataLibrary.ToJson(PrototypeCollection.LegacyCards());
            Directory.CreateDirectory(Path.GetDirectoryName(AssetPath)!);
            File.WriteAllText(AssetPath, json, new UTF8Encoding(false));
            AssetDatabase.ImportAsset(AssetPath);
            Debug.Log($"cards.json écrit : {AssetPath} ({Encoding.UTF8.GetByteCount(json)} octets)");
        }

        /// <summary>
        /// Décision pure : null si l'export peut écrire, sinon le message de refus. Fichier absent ou vide : on écrit. Fichier illisible, ou contenant des cartes
        /// absentes du code : on refuse (l'export les effacerait).
        /// </summary>
        public static string? RefusalReason(string? existingJson, IReadOnlyList<SkillCardDef> codeCards)
        {
            if (string.IsNullOrWhiteSpace(existingJson)) return null;
            IReadOnlyList<SkillCardDef> existing;
            try { existing = CardDataLibrary.FromJson(existingJson!); }
            catch (Exception e) { return "Export refusé : cards.json existe mais est illisible (" + e.Message + "). Le corriger ou le supprimer d'abord."; }
            var known = new HashSet<string>(codeCards.Select(c => c.Id.Value));
            var extra = existing.Select(c => c.Id.Value).Where(id => !known.Contains(id)).ToList();
            if (extra.Count == 0) return null;
            return $"Export refusé : cards.json contient {extra.Count} carte(s) absente(s) du code ({string.Join(", ", extra.Take(5))}{(extra.Count > 5 ? ", …" : "")}) qu'il effacerait. "
                 + "Pour ajouter des cartes, utiliser Heroes Dawn ▸ Contenu ▸ Mettre en jeu les cartes de mise-en-jeu.txt.";
        }
    }
}
