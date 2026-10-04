using System;
using System.Linq;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Presentation.Wip;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    public sealed class WipCatalogTests
    {
        [Test, Description("Vérifie que chaque fonction en WIP a un titre et un texte « ce qui est prévu » — parce qu'un élément BIENTÔT doit dire ce qui arrive (spec §3.4)")]
        public void EveryFeature_HasTitleAndPlannedText()
        {
            var missing = WipCatalog.All.SelectMany(f => new[] { f.TitleKey, f.PlannedKey }).Where(k => !Loc.Has(k)).ToList();
            Assert.That(missing, Is.Empty, string.Join(", ", missing));
            Assert.That(Loc.Has("wip.soon") && Loc.Has("wip.toast"), Is.True);
        }

        [Test, Description("Vérifie que les identifiants WIP sont uniques et couvrent les fonctions prévues par la spec — parce que le catalogue est la liste unique des WIP")]
        public void Ids_AreUnique_AndCoverSpec()
        {
            var ids = WipCatalog.All.Select(f => f.Id).ToList();
            Assert.That(ids.Distinct().Count(), Is.EqualTo(ids.Count));
            Assert.That(ids, Is.SupersetOf(new[] { "pass", "shop", "operations", "lab", "archives", "daily_missions", "secondary_objectives", "gems", "member_art",
                                                   "settings_audio", "settings_graphics", "settings_accessibility", "settings_notifications", "settings_controls" }));
        }

        [Test, Description("Vérifie qu'un identifiant WIP inconnu est signalé — parce qu'un WipMark mal saisi dans un prefab doit se voir")]
        public void Get_UnknownId_Throws()
            => Assert.Throws<ArgumentException>(() => WipCatalog.Get("inconnu"));

        [Test, Description("Vérifie que le message de toucher reprend le titre de la fonction — parce que le joueur doit savoir ce qui arrive bientôt")]
        public void WipNotice_UsesFeatureTitle()
        {
            var msg = WipNotice.Message(new LocLocalization(), "pass");
            Assert.That(msg, Does.Contain(Loc.T(WipCatalog.Get("pass").TitleKey)));
        }
    }
}
