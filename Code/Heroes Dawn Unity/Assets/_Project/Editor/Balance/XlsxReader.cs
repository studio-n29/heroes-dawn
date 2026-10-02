#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml;

namespace N29.HeroesDawn.EditorTools.Balance
{
    /// <summary>Lecture minimale d'un onglet .xlsx (zip + XML), sans dépendance externe.</summary>
    public static class XlsxReader
    {
        const string Main = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        const string Rel = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        const string PkgRel = "http://schemas.openxmlformats.org/package/2006/relationships";
        const int Columns = 26;

        /// <summary>Une ligne par &lt;row&gt;, colonnes A…Z (index 0…25), cellules vides = "".</summary>
        public static IReadOnlyList<string[]> ReadSheet(Stream xlsx, string sheetName)
        {
            using var zip = new ZipArchive(xlsx, ZipArchiveMode.Read, leaveOpen: true);
            var sheetPath = FindSheetPath(zip, sheetName);
            var shared = ReadSharedStrings(zip);

            var doc = Load(zip, sheetPath);
            var ns = Ns(doc);
            var rows = new List<string[]>();
            foreach (XmlElement row in doc.SelectNodes("//m:sheetData/m:row", ns)!)
            {
                var cells = Enumerable.Repeat("", Columns).ToArray();
                foreach (XmlElement c in row.SelectNodes("m:c", ns)!)
                {
                    int col = ColumnIndex(c.GetAttribute("r"));
                    if (col < 0 || col >= Columns) continue;
                    var type = c.GetAttribute("t");
                    if (type == "inlineStr")
                        cells[col] = Text(c.SelectSingleNode("m:is", ns) as XmlElement, ns);
                    else
                    {
                        var v = c.SelectSingleNode("m:v", ns)?.InnerText ?? "";
                        cells[col] = type == "s" && int.TryParse(v, out var i) && i < shared.Count ? shared[i] : v;
                    }
                }
                rows.Add(cells);
            }
            return rows;
        }

        static string FindSheetPath(ZipArchive zip, string sheetName)
        {
            var wb = Load(zip, "xl/workbook.xml");
            var ns = Ns(wb);
            string? relId = null;
            foreach (XmlElement s in wb.SelectNodes("//m:sheets/m:sheet", ns)!)
                if (s.GetAttribute("name") == sheetName) relId = s.GetAttribute("id", Rel);
            if (relId == null) throw new ArgumentException("Onglet introuvable : " + sheetName);

            var rels = Load(zip, "xl/_rels/workbook.xml.rels");
            var rns = new XmlNamespaceManager(rels.NameTable);
            rns.AddNamespace("r", PkgRel);
            var target = (rels.SelectSingleNode($"//r:Relationship[@Id='{relId}']", rns) as XmlElement)?.GetAttribute("Target")
                         ?? throw new ArgumentException("Onglet introuvable : " + sheetName);
            return target.StartsWith("/") ? target.TrimStart('/') : "xl/" + target;
        }

        static List<string> ReadSharedStrings(ZipArchive zip)
        {
            var list = new List<string>();
            if (zip.GetEntry("xl/sharedStrings.xml") == null) return list;
            var doc = Load(zip, "xl/sharedStrings.xml");
            var ns = Ns(doc);
            foreach (XmlElement si in doc.SelectNodes("//m:si", ns)!) list.Add(Text(si, ns));
            return list;
        }

        /// <summary>Texte simple (&lt;t&gt;) ou riche (&lt;r&gt;&lt;t&gt;…).</summary>
        static string Text(XmlElement? node, XmlNamespaceManager ns)
        {
            if (node == null) return "";
            var direct = node.SelectSingleNode("m:t", ns);
            if (direct != null) return direct.InnerText;
            return string.Concat(node.SelectNodes("m:r/m:t", ns)!.Cast<XmlNode>().Select(t => t.InnerText));
        }

        static int ColumnIndex(string cellRef)
        {
            int col = 0, n = 0;
            foreach (var ch in cellRef)
            {
                if (ch < 'A' || ch > 'Z') break;
                col = col * 26 + (ch - 'A' + 1);
                n++;
            }
            return n == 0 ? -1 : col - 1;
        }

        static XmlDocument Load(ZipArchive zip, string path)
        {
            var entry = zip.GetEntry(path) ?? throw new InvalidDataException("Entrée absente du classeur : " + path);
            var doc = new XmlDocument();
            using var s = entry.Open();
            doc.Load(s);
            return doc;
        }

        static XmlNamespaceManager Ns(XmlDocument doc)
        {
            var ns = new XmlNamespaceManager(doc.NameTable);
            ns.AddNamespace("m", Main);
            return ns;
        }
    }
}
