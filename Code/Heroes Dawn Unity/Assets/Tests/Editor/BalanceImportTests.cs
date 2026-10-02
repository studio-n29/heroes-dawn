using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.EditorTools.Balance;
using NUnit.Framework;
using UnityEngine;

namespace N29.HeroesDawn.Tests.Editor
{
    public sealed class BalanceImportTests
    {
        static string Xlsx => Path.GetFullPath(Path.Combine(Application.dataPath, "../../../01_Game_Design/05_Equilibrage.xlsx"));

        [Test]
        public void Import_RealSheet_MatchesGddValues()
        {
            using var s = new FileStream(Xlsx, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var w = new List<string>();
            var b = BalanceSheetMapper.Map(XlsxReader.ReadSheet(s, "Paramètres"), w);
            Assert.That(w, Is.Empty);
            Assert.That(b.AtbGaugePoints, Is.EqualTo(1000));
            Assert.That(b.AtbFactorPerSecond, Is.EqualTo(4));
            Assert.That(b.DefK, Is.EqualTo(300));
            Assert.That(b.CritBasePercent, Is.EqualTo(15));
            Assert.That(b.CritMultiplierPercent, Is.EqualTo(150));
            Assert.That(b.FatiguePerWin, Is.EqualTo(12));
            Assert.That(b.FatiguePerLossExtra, Is.EqualTo(15));
            Assert.That(b.FatigueMalusThreshold, Is.EqualTo(60));
            Assert.That(b.RankEasyStatPercent, Is.EqualTo(80));
            Assert.That(b.RankEliteLevelOffset, Is.EqualTo(3));
        }

        [Test] // Review Focus 1 : tableur ouvert dans Excel
        public void Import_WhileFileLockedForWriting_Succeeds()
        {
            using var excel = new FileStream(Xlsx, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            using var s = new FileStream(Xlsx, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            Assert.That(XlsxReader.ReadSheet(s, "Paramètres").Count, Is.GreaterThan(50));
        }

        [Test] // Review Focus 2 : libellé absent
        public void Map_MissingLabel_KeepsDefaultAndWarns()
        {
            var w = new List<string>();
            var b = BalanceSheetMapper.Map(new[] { new[] { "", "Constante de défense (K)", "450" } }, w);
            Assert.That(b.DefK, Is.EqualTo(450));
            Assert.That(b.CritBasePercent, Is.EqualTo(15));
            Assert.That(w, Has.Some.Contains("Chance de critique de base"));
        }

        [Test] // Review Focus 3 : arrondi et culture française
        public void Map_PercentRounding_And_FrenchCulture()
        {
            var prev = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
            try
            {
                var b = BalanceSheetMapper.Map(new[]
                {
                    new[] { "", "Fatigue par combat (victoire)", "0.29" },
                    new[] { "", "Chance de critique de base", "0.15" },
                }, new List<string>());
                Assert.That(b.FatiguePerWin, Is.EqualTo(29));
                Assert.That(b.CritBasePercent, Is.EqualTo(15));
            }
            finally { CultureInfo.CurrentCulture = prev; }
        }

        [Test]
        public void Map_NonNumericValue_WarnsAndKeepsDefault()
        {
            var w = new List<string>();
            var b = BalanceSheetMapper.Map(new[] { new[] { "", "Constante de défense (K)", "#REF!" } }, w);
            Assert.That(b.DefK, Is.EqualTo(300));
            Assert.That(w, Has.Some.Contains("Valeur illisible"));
        }

        [Test]
        public void ReadSheet_UnknownSheet_Throws()
        {
            using var s = File.OpenRead(Xlsx);
            Assert.Throws<ArgumentException>(() => XlsxReader.ReadSheet(s, "Inexistant"));
        }

        [Test]
        public void RemoteConfigFile_WrapsBalanceAsJsonEntry()
        {
            var rc = RemoteConfigFile.Build(BalanceJson.Serialize(new BalanceConfig()));
            StringAssert.Contains("\"types\"", rc);
            StringAssert.Contains("\"balance\": \"JSON\"", rc);
            StringAssert.Contains("\"DefK\": 300", rc);
        }
    }
}
