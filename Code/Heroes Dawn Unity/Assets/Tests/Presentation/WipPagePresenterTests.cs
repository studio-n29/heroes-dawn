#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Presentation.Wip;
using N29.HeroesDawn.Tests.Presentation.Fakes;
using NUnit.Framework;
using UnityEngine;

namespace N29.HeroesDawn.Tests.Presentation
{
    public sealed class WipPagePresenterTests
    {
        sealed class FakeWipPageView : FakeView, IWipPageView
        {
            public readonly List<WipPageData> Shown = new List<WipPageData>();
            public void Show(WipPageData d) => Shown.Add(d);
        }

        [Test, Description("Vérifie qu'une page WIP affiche son titre, le nom de la fonction et ce qui est prévu, avec le bandeau BIENTÔT — parce qu'une page à venir doit dire ce qu'elle contiendra (spec §3.4)")]
        public void ShowsTitleAndPlannedText()
        {
            var view = new FakeWipPageView();
            var p = new WipPagePresenter(view, new WipPageArgs(WipCatalog.Pass, "page.pass"), new LocLocalization());
            view.Show();
            p.Start();
            var d = view.Shown[view.Shown.Count - 1];
            Assert.That(p.Title, Is.EqualTo(Loc.T("page.pass")));
            Assert.That(d.FeatureTitle, Is.EqualTo(Loc.T(WipCatalog.Pass.TitleKey)));
            Assert.That(d.Planned, Is.EqualTo(Loc.T(WipCatalog.Pass.PlannedKey)));
            Assert.That(d.Soon, Is.EqualTo(Loc.T("wip.soon")));
        }

        [Test, Description("Vérifie qu'un bandeau BIENTÔT touché remonte son identifiant au relais de la coque — parce que la coque affiche alors « … arrive bientôt »")]
        public void WipMark_Tap_RaisesFeatureOnParentRelay()
        {
            var root = new GameObject("Shell");
            try
            {
                var relay = root.AddComponent<UiTapRelay>();
                var child = new GameObject("Pill");
                child.transform.SetParent(root.transform);
                var mark = child.AddComponent<WipMark>();
                mark.FeatureId = "gems";
                string? got = null;
                relay.WipTapped += id => got = id;
                mark.Tap();
                Assert.That(got, Is.EqualTo("gems"));
            }
            finally { Object.DestroyImmediate(root); }
        }
    }
}
