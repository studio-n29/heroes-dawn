#nullable enable
using System.IO;
using System.Linq;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.EditorTools.Balance;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Editor
{
    /// <summary>Export CSV du contenu : la lecture des effets passe toujours par les capacités (P0).</summary>
    public sealed class ContentCsvAbilityTests
    {
        [Test, Description("Vérifie que l'export CSV des compétences liste, pour chaque compétence, tous les types d'effets de ses capacités OnCast — parce que la vue Effects doit rester celle qu'exporte le tableur de design")]
        public void ContentCsvExport_StillReadsEffects()
        {
            var dir = Path.Combine(Path.GetTempPath(), "hd_csv_" + System.Guid.NewGuid().ToString("N"));
            try
            {
                ContentCsvExport.Export(dir);
                var lines = File.ReadAllLines(Path.Combine(dir, "competences.csv"));
                var catalog = PrototypeContent.Build();
                var skill = catalog.Collection.Cards.Select(c => c.Skill).First(s => s.Effects.Count >= 2);
                var line = lines.First(l => l.StartsWith(skill.Id.Value + ","));
                foreach (var e in skill.Effects) Assert.That(line, Does.Contain(e.Type.ToString()), skill.Id.Value);
                Assert.That(line, Does.Contain(skill.Effects[0].Type + (skill.Effects[0].Value != 0 ? " " + skill.Effects[0].Value : "")));
            }
            finally { if (Directory.Exists(dir)) Directory.Delete(dir, true); }
        }
    }
}
