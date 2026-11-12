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

        [Test, Description("Vérifie que les identifiants WIP sont uniques, que les fonctions livrées en sont retirées et que les trois restantes sont présentes — parce que le catalogue est la liste unique des WIP (audit de la phase 1, 2026-11-12)")]
        public void Ids_AreUnique_AndOnlyUnfinishedFeaturesRemain()
        {
            var ids = WipCatalog.All.Select(f => f.Id).ToList();
            Assert.That(ids.Distinct().Count(), Is.EqualTo(ids.Count));
            Assert.That(ids, Is.EquivalentTo(new[] { "secondary_objectives", "settings_notifications", "settings_controls" }));
            foreach (var retired in new[] { "pass", "shop", "operations", "lab", "archives", "daily_missions", "gems", "member_art", "settings_audio", "settings_graphics", "settings_accessibility" })
                Assert.Throws<ArgumentException>(() => WipCatalog.Get(retired), retired + " est livrée : plus de WIP");
        }

        [Test, Description("Vérifie qu'un identifiant WIP inconnu est signalé — parce qu'un WipMark mal saisi dans un prefab doit se voir")]
        public void Get_UnknownId_Throws()
            => Assert.Throws<ArgumentException>(() => WipCatalog.Get("inconnu"));

        [Test, Description("Vérifie que le message de toucher reprend le titre de la fonction — parce que le joueur doit savoir ce qui arrive bientôt")]
        public void WipNotice_UsesFeatureTitle()
        {
            var msg = WipNotice.Message(new LocLocalization(), "secondary_objectives");
            Assert.That(msg, Does.Contain(Loc.T(WipCatalog.Get("secondary_objectives").TitleKey)));
        }
    }
}
