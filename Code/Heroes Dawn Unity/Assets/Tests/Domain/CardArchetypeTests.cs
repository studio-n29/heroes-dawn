#nullable enable
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Definitions;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>Archétypes et rôles des 48 cartes existantes : le code doit suivre la fiche du catalogue.</summary>
    public sealed class CardArchetypeTests
    {
        static readonly CollectionDef Defs = PrototypeCollection.Build();

        static readonly Dictionary<string, Archetype> ArchetypeNames = new Dictionary<string, Archetype>
        {
            ["Brûlure"] = Archetype.Burn, ["Entrave"] = Archetype.Restrain, ["Traque"] = Archetype.Hunt,
            ["Tempo"] = Archetype.Tempo, ["Rempart"] = Archetype.Bulwark, ["Renouveau"] = Archetype.Renewal,
        };

        static readonly Dictionary<string, CardRole> RoleNames = new Dictionary<string, CardRole>
        {
            ["Amorce"] = CardRole.Primer, ["Gain"] = CardRole.Payoff, ["Pont"] = CardRole.Bridge, ["Base"] = CardRole.Basic,
        };

        /// <summary>Lit la fiche Markdown (chemin relatif à ce fichier source) : Id, archétypes, rôle de chaque ligne du tableau.</summary>
        static List<(string Id, Archetype[] Archetypes, CardRole Role)> ReadSheet([CallerFilePath] string source = "")
        {
            string path = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(source)!, "..", "..", "..", "..", "docs", "catalogue", "cartes-existantes.md"));
            var rows = new List<(string, Archetype[], CardRole)>();
            foreach (string line in File.ReadAllLines(path))
            {
                if (!line.StartsWith("|")) continue;
                string[] cells = line.Split('|').Select(c => c.Trim()).ToArray();
                // cells[0] vide (avant le premier |) ; colonnes : 1 Id, 7 Archétype, 8 Rôle.
                if (cells.Length < 9 || !cells[1].StartsWith("card_")) continue;
                var archetypes = cells[7].Split('+').Select(a => ArchetypeNames[a.Trim()]).ToArray();
                rows.Add((cells[1], archetypes, RoleNames[cells[8]]));
            }
            return rows;
        }

        /// <summary>Les 48 cartes d'avant P1 (le catalogue chargé contient aussi les 12 de la première vague, qui ont leurs propres tests).</summary>
        static System.Collections.Generic.IReadOnlyList<SkillCardDef> Existing => Defs.Cards.Where(c => !FirstWaveTests.WaveCardIds.Contains(c.Id.Value)).ToList();

        [Test, Description("Vérifie que chacune des 48 cartes d avant P1 a exactement un archétype — parce que la fiche du catalogue en donne un seul à chaque carte existante")]
        public void EveryExistingCard_HasOneArchetype()
        {
            Assert.That(Existing.Count, Is.EqualTo(48));
            Assert.That(Existing.All(c => c.Archetypes.Count == 1), Is.True);
        }

        [Test, Description("Vérifie que les archétypes se répartissent en Brûlure 2, Entrave 7, Traque 6, Tempo 9, Rempart 16, Renouveau 8 — parce que c'est la répartition de la fiche du catalogue")]
        public void ArchetypeCounts_MatchTheCatalogueSheet()
        {
            int Count(Archetype a) => Existing.Count(c => c.Archetypes.Contains(a));
            Assert.That(Count(Archetype.Burn), Is.EqualTo(2));
            Assert.That(Count(Archetype.Restrain), Is.EqualTo(7));
            Assert.That(Count(Archetype.Hunt), Is.EqualTo(6));
            Assert.That(Count(Archetype.Tempo), Is.EqualTo(9));
            Assert.That(Count(Archetype.Bulwark), Is.EqualTo(16));
            Assert.That(Count(Archetype.Renewal), Is.EqualTo(8));
        }

        [Test, Description("Vérifie que l'archétype et le rôle de chacune des 48 cartes égalent ceux de la ligne de même Id dans cartes-existantes.md — parce que la fiche est la source de vérité du catalogue")]
        public void ArchetypeAndRoleOfEachExistingCard_MatchTheSheet()
        {
            var sheet = ReadSheet();
            Assert.That(sheet.Count, Is.EqualTo(48));
            foreach (var (id, archetypes, role) in sheet)
            {
                var card = Defs.Cards.Single(c => c.Id.Value == id);
                Assert.That(card.Archetypes, Is.EqualTo(archetypes), id + " archétypes");
                Assert.That(card.Role, Is.EqualTo(role), id + " rôle");
            }
        }
    }
}
