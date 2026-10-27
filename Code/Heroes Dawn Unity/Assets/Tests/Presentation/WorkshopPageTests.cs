using System.Linq;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Tests.Domain.Application;
using N29.HeroesDawn.Tests.Presentation.Fakes;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    /// <summary>Atelier de fabrication sur la page du labo (lot E de la 0.14.0).</summary>
    public sealed class WorkshopPageTests
    {
        // 4 recherches, un titre d'atelier, 4 compétences, puis les recettes connues.
        const int FirstRecipeRow = 4 + 1 + 4;

        TestApp _app = null!;
        FakeListView _view = null!;
        FakeToasts _toasts = null!;
        readonly LocLocalization _loc = new LocLocalization();

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.SignInDirect("p1");
            _view = new FakeListView();
            _toasts = new FakeToasts();
        }

        LabPagePresenter Lab()
        {
            var p = new LabPagePresenter(_view, _app.Lab, _app.Session, _loc, _toasts, _app.Subscriber<MetaChanged>(),
                                         _app.Subscriber<WalletChanged>(), _app.Subscriber<RosterChanged>(), _app.Time, _app.Workshop);
            _view.Show();
            ((VContainer.Unity.IStartable)p).Start();
            return p;
        }

        [Test, Description("Vérifie que la page du labo montre, après les 4 recherches, un titre d'atelier, 4 compétences et les 4 recettes de départ — parce que la fabrication vit sur la page du labo")]
        public void LabPage_ShowsTheWorkshopRowsAfterTheTechs()
        {
            var p = Lab();
            Assert.That(_view.Rows.Length, Is.EqualTo(FirstRecipeRow + 4));
            Assert.That(_view.Rows[4].Title, Is.EqualTo(_loc.T("lab.workshop")));
            Assert.That(_view.Rows[FirstRecipeRow].Title, Is.EqualTo(_loc.T("craft.recipe.scrap_blade")));
            Assert.That(_view.Rows[5].Title, Does.Contain(CollectionText.ItemTypeName(_loc, ItemType.Weapon)));
            Assert.That(_view.Rows[5].Title, Does.Contain("1/10"));
            p.Dispose();
        }

        [Test, Description("Vérifie que le bouton Fabriquer est grisé avec la raison « il manque des scraps » sans scraps, puis actif avec assez de scraps — parce que le joueur doit savoir pourquoi il ne peut pas fabriquer")]
        public void RecipeButton_DisabledWithoutScraps_EnabledWithThem()
        {
            var without = Lab();
            Assert.That(_view.Rows[FirstRecipeRow].A.Enabled, Is.False);
            Assert.That(_view.Rows[FirstRecipeRow].A.Reason, Is.EqualTo(_loc.T("errors.not_enough_scraps")));
            without.Dispose();
            CraftingOps.AddScraps(_app.Session, ScrapKind.Weapon, 6);
            var with = Lab();
            Assert.That(_view.Rows[FirstRecipeRow].A.Enabled, Is.True);
            Assert.That(_view.Rows[FirstRecipeRow].A.Label, Is.EqualTo(_loc.T("lab.craft")));
            with.Dispose();
        }

        [Test, Description("Vérifie qu'un clic sur Fabriquer dépense les scraps, ajoute l'objet et affiche un message avec son nom — parce que le joueur doit voir ce qu'il a obtenu")]
        public void ClickingTheRecipe_CraftsAndToasts()
        {
            CraftingOps.AddScraps(_app.Session, ScrapKind.Weapon, 6);
            var p = Lab();
            _view.ClickRow(FirstRecipeRow);
            Assert.That(CraftingOps.Scraps(_app.Session, ScrapKind.Weapon), Is.EqualTo(0));
            var item = _app.Session.Content.Collection.Crafted.Single();
            string name = CollectionText.ItemName(_loc, item);
            Assert.That(_toasts.Shown, Has.Some.Matches<(string Text, ToastKind Kind)>(t => t.Kind == ToastKind.Reward && t.Text.Contains(name)));
            Assert.That(_view.Rows[FirstRecipeRow].A.Enabled, Is.False, "la liste se rafraîchit : plus de scraps");
            p.Dispose();
        }

        [Test, Description("Vérifie qu'un clic sans scraps affiche l'erreur et ne crée aucun objet — parce qu'un bouton contourné ne doit rien débloquer")]
        public void ClickingWithoutScraps_ShowsTheError()
        {
            var p = Lab();
            _view.ClickRow(FirstRecipeRow);
            Assert.That(_toasts.Shown, Has.Some.Matches<(string Text, ToastKind Kind)>(t => t.Kind == ToastKind.Error));
            Assert.That(_app.Session.Meta.crafted, Is.Empty);
            p.Dispose();
        }

        [Test, Description("Vérifie que le texte de fin de combat liste les scraps obtenus (par type) et la recette apprise — parce que le joueur doit savoir ce que le combat lui a laissé pour le labo")]
        public void CraftDropLines_MentionScrapsAndNewRecipe()
        {
            var scraps = new[] { 0, 3, 0, 0, 2 };
            string text = CollectionText.CraftDropLines(_loc, new CraftDrops(scraps, "pulse_rifle"));
            Assert.That(text, Does.Contain("3 " + _loc.T("scrap.Armor")));
            Assert.That(text, Does.Contain("2 " + _loc.T("scrap.Core")));
            Assert.That(text, Does.Not.Contain(_loc.T("scrap.Weapon")));
            Assert.That(text, Does.Contain(_loc.F("combat.craft_recipe", _loc.T("craft.recipe.pulse_rifle"))));
            Assert.That(CollectionText.CraftDropLines(_loc, new CraftDrops(scraps, null)), Does.Not.Contain(_loc.T("craft.recipe.pulse_rifle")));
        }
    }
}
