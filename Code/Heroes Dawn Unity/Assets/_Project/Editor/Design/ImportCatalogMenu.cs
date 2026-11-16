#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using N29.HeroesDawn.Authoring;
using N29.HeroesDawn.Content.Prototype;
using UnityEditor;
using UnityEngine;

namespace N29.HeroesDawn.Editor
{
    /// <summary>
    /// Met en jeu des cartes des fiches du catalogue : lit <c>Code/docs/catalogue/mise-en-jeu.txt</c> (un identifiant par ligne, « # » = commentaire),
    /// retrouve chaque carte dans les fiches <c>cartes-*.md</c>, la convertit (<see cref="CatalogImporter"/>) et la fusionne dans
    /// <c>Resources/Content/cards.json</c> : les cartes absentes de la liste restent octet pour octet identiques et à leur place, celles de la liste sont
    /// ajoutées à la fin (ou remplacées sur place si elles existent déjà). Réutilisable pour chaque lot de cartes ; refuse de fusionner dans un fichier absent, vide ou sans les cartes du code.
    /// En batch : <c>-executeMethod N29.HeroesDawn.Editor.ImportCatalogMenu.ImportBatch</c> (code de sortie 1 en cas d'échec).
    /// </summary>
    public static class ImportCatalogMenu
    {
        const string CardsAssetPath = "Assets/_Project/Resources/Content/cards.json";
        const string ListFileName = "mise-en-jeu.txt";

        static string CatalogueFolder => Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "..", "..", "docs", "catalogue"));

        [MenuItem("Heroes Dawn/Contenu/Mettre en jeu les cartes de mise-en-jeu.txt")]
        public static void Import()
        {
            try { Apply(); }
            catch (Exception e)
            {
                Debug.LogError("Mise en jeu des cartes impossible : " + e.Message);
                EditorUtility.DisplayDialog("Mise en jeu des cartes", e.Message, "OK");
            }
        }

        /// <summary>Point d'entrée de <c>-executeMethod</c> : mêmes effets que le menu, sans boîte de dialogue, sortie en erreur si l'import échoue.</summary>
        public static void ImportBatch()
        {
            try { Apply(); }
            catch (Exception e)
            {
                Debug.LogError("Mise en jeu des cartes impossible : " + e.Message);
                EditorApplication.Exit(1);
            }
        }

        static void Apply()
        {
            var folder = CatalogueFolder;
            var ids = ParseIds(File.ReadAllText(Path.Combine(folder, ListFileName), Encoding.UTF8));
            var rows = Directory.GetFiles(folder, "cartes-*.md").OrderBy(f => f, StringComparer.Ordinal)
                .SelectMany(f => CatalogImporter.ReadTable(File.ReadAllText(f, Encoding.UTF8))).ToList();
            var existing = File.Exists(CardsAssetPath) ? File.ReadAllText(CardsAssetPath, Encoding.UTF8) : "";
            var merged = Merge(existing, rows, ids);
            File.WriteAllText(CardsAssetPath, merged, new UTF8Encoding(false));
            AssetDatabase.ImportAsset(CardsAssetPath);
            Debug.Log($"cards.json écrit : {ids.Count} carte(s) mise(s) en jeu, {CardDataLibrary.FromJson(merged).Count} au total ({Encoding.UTF8.GetByteCount(merged)} octets).");
        }

        /// <summary>Identifiants de la liste : un par ligne, lignes vides et commentaires (« # » jusqu'à la fin de ligne) ignorés ; un identifiant répété est refusé.</summary>
        public static IReadOnlyList<string> ParseIds(string text)
        {
            var ids = new List<string>();
            foreach (var raw in text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n'))
            {
                var hash = raw.IndexOf('#');
                var id = (hash < 0 ? raw : raw.Substring(0, hash)).Trim();
                if (id.Length == 0) continue;
                if (ids.Contains(id)) throw new InvalidOperationException($"{id} : identifiant répété dans la liste des cartes à mettre en jeu.");
                ids.Add(id);
            }
            return ids;
        }

        /// <summary>
        /// Fusion pure (sans fichier) : <paramref name="ids"/> sont retrouvés dans <paramref name="rows"/>, convertis, puis ajoutés à la fin de
        /// <paramref name="existingJson"/> (dans l'ordre de la liste) ou, s'ils y sont déjà, remplacés à leur place. Les autres cartes ne sont pas touchées.
        /// Échoue (<see cref="InvalidOperationException"/>) si le fichier existant est absent, vide ou sans toutes les cartes du code, pour un identifiant inconnu ou de phase P2/P3, une fiche hors grammaire, ou un résultat
        /// qui ne passe pas <see cref="CardDataLibrary.Validate"/>.
        /// </summary>
        public static string Merge(string existingJson, IReadOnlyList<CatalogRow> rows, IReadOnlyList<string> ids)
        {
            var file = string.IsNullOrWhiteSpace(existingJson) ? new CardsFile() : JsonUtility.FromJson<CardsFile>(existingJson);
            // Un fichier absent, vide ou incomplet donnerait un catalogue réduit aux cartes importées : le jeu perdrait les cartes du code (l'amorçage d'un fichier neuf relève de ExportCardsMenu).
            var missing = PrototypeCollection.LegacyCards().Select(c => c.Id.Value).Where(id => !file.cards.Any(r => r.id == id)).ToList();
            if (missing.Count > 0)
                throw new InvalidOperationException($"cards.json est absent, vide ou incomplet : il manque {missing.Count} carte(s) du code ({string.Join(", ", missing.Take(3))}…). Le restaurer (git checkout) avant de mettre des cartes en jeu.");
            var records = new List<CardRecord>();
            foreach (var id in ids)
            {
                var matches = rows.Where(r => r.Id == id).ToList();
                if (matches.Count == 0) throw new InvalidOperationException($"{id} : identifiant inconnu (aucune fiche cartes-*.md ne le porte).");
                if (matches.Count > 1) throw new InvalidOperationException($"{id} : identifiant présent {matches.Count} fois dans les fiches.");
                var row = matches[0];
                if (row.Phase != "P1") throw new InvalidOperationException($"{id} : la carte est de phase {row.Phase}, seule la phase P1 se met en jeu.");
                try { records.Add(CatalogImporter.ToRecord(row)); }
                catch (CatalogImportException e) { throw new InvalidOperationException("fiche hors grammaire : " + e.Message, e); }
            }
            foreach (var record in records)
            {
                int at = file.cards.FindIndex(c => c.id == record.id);
                if (at >= 0) file.cards[at] = record; else file.cards.Add(record);
            }
            var json = JsonUtility.ToJson(file, true);
            var errors = CardDataLibrary.Validate(CardDataLibrary.FromJson(json));
            if (errors.Count > 0) throw new InvalidOperationException("le catalogue obtenu est invalide (CardDataLibrary.Validate) : " + string.Join(" ; ", errors));
            return json;
        }
    }
}
