#nullable enable
using System;
using System.IO;
using System.Linq;
using N29.HeroesDawn.Authoring;
using N29.HeroesDawn.Editor;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Editor
{
    /// <summary>Outil de mise en jeu des cartes (Heroes Dawn ▸ Contenu) : la fusion dans cards.json est une fonction pure sur des chaînes.</summary>
    public sealed class ImportCatalogMenuTests
    {
        const string Header = "| Id | Nom | Name | ★ | Famille | Nature | Archétype | Rôle | Cible | Coût | Phase | Base | Niv.3 | Niv.6 | Niv.10 | Idée |";
        const string Separator = "|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|";

        static string Line(string id, string phase, string baseCell)
            => $"| {id} | Essai | Test | 1 | Matière | Offensive | Brûlure | Amorce | 1E | 40 | {phase} | {baseCell} | St Vulnerable 1t | Steal 15 | Exec 30 | Une idée. |";

        static System.Collections.Generic.IReadOnlyList<CatalogRow> Rows(params string[] lines)
            => CatalogImporter.ReadTable("# Titre\n\n" + Header + "\n" + Separator + "\n" + string.Join("\n", lines) + "\n");

        static readonly string[] OneId = { "card_new_a" };

        static string CatalogueFolder => Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "..", "..", "docs", "catalogue"));

        static string ExistingJson() => CardDataLibrary.ToJson(N29.HeroesDawn.Content.Prototype.PrototypeCollection.LegacyCards());

        [Test, Description("Vérifie que la liste d'identifiants ignore lignes vides et commentaires « # », rogne les espaces et garde l'ordre — parce que mise-en-jeu.txt est édité à la main")]
        public void ParseIds_SkipsCommentsAndBlankLines()
        {
            var ids = ImportCatalogMenu.ParseIds("# vague 1\n\ncard_b  \n  card_a # fin de ligne\r\n\n#card_c\ncard_d\n");
            Assert.That(ids, Is.EqualTo(new[] { "card_b", "card_a", "card_d" }));
        }

        [Test, Description("Vérifie qu'un identifiant répété dans la liste est refusé — parce qu'une carte mise en jeu deux fois cacherait une faute de frappe")]
        public void ParseIds_RejectsADuplicate()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => ImportCatalogMenu.ParseIds("card_a\ncard_b\ncard_a"));
            Assert.That(ex!.Message, Does.Contain("card_a"));
        }

        [Test, Description("Vérifie que Merge ajoute les nouvelles cartes à la fin et laisse les 48 cartes existantes octet pour octet identiques, dans le même ordre — parce que le garde-fou du combat repose sur ces cartes")]
        public void Merge_AppendsNewCards_AndKeepsTheExistingBytes()
        {
            var existing = ExistingJson();
            var merged = ImportCatalogMenu.Merge(existing, Rows(Line("card_new_a", "P1", "Dmg 105, St Burn 2t"), Line("card_new_b", "P1", "Dmg 90")), new[] { "card_new_b", "card_new_a" });
            var prefix = existing.Substring(0, existing.LastIndexOf("\n    ]", StringComparison.Ordinal));
            Assert.That(merged.StartsWith(prefix + ",", StringComparison.Ordinal), Is.True, "les cartes existantes ne sont plus identiques");
            var cards = CardDataLibrary.FromJson(merged);
            Assert.That(cards.Count, Is.EqualTo(50));
            Assert.That(cards.Skip(48).Select(c => c.Id.Value), Is.EqualTo(new[] { "card_new_b", "card_new_a" }), "ordre de la liste");
            Assert.That(CardDataLibrary.Validate(cards), Is.Empty);
        }

        [Test, Description("Vérifie qu'une carte déjà présente est remplacée à sa place, sans toucher aux autres ni à l'ordre — parce que relancer l'outil après une correction de fiche ne doit pas déplacer les cartes")]
        public void Merge_ReplacesAnExistingCardInPlace()
        {
            var existing = ExistingJson();
            var first = CardDataLibrary.FromJson(existing)[0].Id.Value;
            var merged = ImportCatalogMenu.Merge(existing, Rows(Line(first, "P1", "Dmg 77")), new[] { first });
            var before = CardDataLibrary.FromJson(existing);
            var after = CardDataLibrary.FromJson(merged);
            Assert.That(after.Count, Is.EqualTo(before.Count));
            Assert.That(after.Select(c => c.Id.Value), Is.EqualTo(before.Select(c => c.Id.Value)));
            Assert.That(after[0].Skill.Effects[0].Value, Is.EqualTo(77));
            Assert.That(CardDataLibrary.ToJson(after.Skip(1).ToList()), Is.EqualTo(CardDataLibrary.ToJson(before.Skip(1).ToList())));
        }

        [Test, Description("Vérifie que Merge rejoué avec les mêmes fiches redonne exactement le même texte — parce que l'outil doit pouvoir être relancé sans effet de bord")]
        public void Merge_IsIdempotent()
        {
            var rows = Rows(Line("card_new_a", "P1", "Dmg 105, St Burn 2t"));
            var once = ImportCatalogMenu.Merge(ExistingJson(), rows, OneId);
            Assert.That(ImportCatalogMenu.Merge(once, rows, OneId), Is.EqualTo(once));
        }

        [Test, Description("Vérifie qu'un identifiant absent des fiches échoue en nommant l'identifiant — parce qu'une faute de frappe ne doit jamais passer en silence")]
        public void Merge_UnknownId_FailsWithTheId()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => ImportCatalogMenu.Merge(ExistingJson(), Rows(Line("card_new_a", "P1", "Dmg 100")), new[] { "card_inconnue" }));
            Assert.That(ex!.Message, Does.Contain("card_inconnue"));
        }

        [Test, Description("Vérifie qu'une carte de phase P2 ou P3 est refusée avec un message qui parle de sa phase — parce que seules les cartes de la phase P1 sont convertibles")]
        public void Merge_LaterPhaseCard_FailsWithThePhase()
        {
            foreach (var phase in new[] { "P2", "P3" })
            {
                var ex = Assert.Throws<InvalidOperationException>(() => ImportCatalogMenu.Merge(ExistingJson(), Rows(Line("card_new_a", phase, "Dmg 100")), OneId));
                Assert.That(ex!.Message, Does.Contain("card_new_a").And.Contain(phase));
            }
        }

        [Test, Description("Vérifie qu'une fiche hors grammaire échoue en gardant l'identifiant de la carte dans le message — parce que l'auteur de la fiche doit savoir laquelle corriger")]
        public void Merge_BadSheet_FailsWithTheId()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => ImportCatalogMenu.Merge(ExistingJson(), Rows(Line("card_new_a", "P1", "Boum 12")), OneId));
            Assert.That(ex!.Message, Does.Contain("card_new_a"));
        }

        [Test, Description("Vérifie qu'un résultat qui ne passe pas CardDataLibrary.Validate (identifiant en double dans le fichier existant) fait échouer Merge — parce qu'on n'écrit jamais un catalogue invalide")]
        public void Merge_InvalidResult_Fails()
        {
            var cards = CardDataLibrary.FromJson(ExistingJson()).ToList();
            var twice = CardDataLibrary.ToJson(cards.Concat(new[] { cards[0] }).ToList());
            var ex = Assert.Throws<InvalidOperationException>(() => ImportCatalogMenu.Merge(twice, Rows(Line("card_new_a", "P1", "Dmg 100")), OneId));
            Assert.That(ex!.Message, Does.Contain("Validate").Or.Contain("invalide"));
        }

        [Test, Description("Vérifie que Merge refuse un fichier existant vide, absent ou privé des cartes du code, et que le message le dit — parce qu'on obtiendrait sinon un catalogue réduit aux seules cartes importées")]
        public void Merge_EmptyOrIncompleteExisting_IsRefused()
        {
            var rows = Rows(Line("card_new_a", "P1", "Dmg 100"));
            foreach (var existing in new[] { "", "  ", "{\"cards\":[]}" })
            {
                var ex = Assert.Throws<InvalidOperationException>(() => ImportCatalogMenu.Merge(existing, rows, OneId));
                Assert.That(ex!.Message, Does.Contain("incomplet").And.Contain("48"), "« " + existing + " »");
            }
            var partial = CardDataLibrary.ToJson(CardDataLibrary.FromJson(ExistingJson()).Take(40).ToList());
            var ex2 = Assert.Throws<InvalidOperationException>(() => ImportCatalogMenu.Merge(partial, rows, OneId));
            Assert.That(ex2!.Message, Does.Contain("8"), "il manque 8 cartes du code");
        }

        [Test, Description("Vérifie que mise-en-jeu.txt liste les 12 cartes de la vague 1, que toutes se convertissent depuis les fiches et que cards.json les contient déjà à l'identique — parce que le fichier de données doit être le résultat exact de l'outil")]
        public void ShippedList_IsAlreadyAppliedToCardsJson()
        {
            var ids = ImportCatalogMenu.ParseIds(File.ReadAllText(Path.Combine(CatalogueFolder, "mise-en-jeu.txt")));
            Assert.That(ids.Count, Is.EqualTo(12));
            var rows = Directory.GetFiles(CatalogueFolder, "cartes-*.md").SelectMany(f => CatalogImporter.ReadTable(File.ReadAllText(f))).ToList();
            var asset = UnityEngine.Resources.Load<UnityEngine.TextAsset>("Content/cards");
            Assert.That(asset, Is.Not.Null);
            Assert.That(ImportCatalogMenu.Merge(asset!.text, rows, ids), Is.EqualTo(asset.text), "cards.json n'est pas à jour : relancer le menu Mettre en jeu les cartes de mise-en-jeu.txt");
        }
    }
}
