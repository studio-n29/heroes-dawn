using System.IO;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Editor
{
    public sealed class PlayerBuildTests
    {
        [Test, Description("Vérifie que link.xml protège BalanceConfig du stripping IL2CPP — parce que sans cela le jeu mobile ignorerait l'équilibrage importé")] // BalanceJson écrit les propriétés par réflexion : les setters ne doivent pas être retirés par le stripping IL2CPP
        public void LinkXml_PreservesBalanceConfig()
        {
            const string path = "Assets/_Project/link.xml";
            Assert.That(File.Exists(path), path);
            var xml = File.ReadAllText(path);
            StringAssert.Contains("<assembly fullname=\"N29.HeroesDawn.Definitions\">", xml);
            StringAssert.Contains("<type fullname=\"N29.HeroesDawn.Definitions.BalanceConfig\" preserve=\"all\"", xml);
        }
    }
}
