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
    /// <summary>Atelier de fabrication sur la page du labo (bloc B de la 1.0.0).</summary>
    public sealed class WorkshopPageTests
    {
        // Les recettes connues ouvrent la liste, puis le titre de l'Atelier.
        const int FirstRecipeRow = 0;

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
            var p = new LabPagePresenter(_view, _app.Session, _loc, _toasts, _app.Subscriber<MetaChanged>(),
                                         _app.Subscriber<WalletChanged>(), _app.Subscriber<RosterChanged>(), _app.Time, _app.Workshop);
            _view.Show();
            ((VContainer.Unity.IStartable)p).Start();
            return p;
        }

        [Test, Description("Vérifie que la page montre d'abord les quatre recettes de départ puis un titre d'Atelier, sans plus aucune recherche ni compétence — parce que le labo de recherche est supprimé")]
        public void LabPage_ShowsRecipesThenTheWorkshopTitle()
        {
            var p = Lab();
            Assert.That(_view.Rows.Length, Is.EqualTo(5));
            Assert.That(_view.Rows[FirstRecipeRow].Title, Is.EqualTo(_loc.T("craft.recipe.scrap_blade")));
            Assert.That(_view.Rows[4].Title, Is.EqualTo(_loc.T("lab.workshop")));
            p.Dispose();
        }

        [Test, Description("Vérifie que le bouton Fabriquer est grisé avec la raison « il manque des Composants » sans Composants, puis actif avec assez de Composants — parce que le joueur doit savoir pourquoi il ne peut pas fabriquer")]
        public void RecipeButton_DisabledWithoutComponents_EnabledWithThem()
        {
            var without = Lab();
            Assert.That(_view.Rows[FirstRecipeRow].A.Enabled, Is.False);
            Assert.That(_view.Rows[FirstRecipeRow].A.Reason, Is.EqualTo(_loc.T("errors.not_enough_components")));
            without.Dispose();
            CraftingOps.AddComponents(_app.Session, ComponentKind.Weapon, ComponentGrade.Common, 6);
            var with = Lab();
            Assert.That(_view.Rows[FirstRecipeRow].A.Enabled, Is.True);
            Assert.That(_view.Rows[FirstRecipeRow].A.Label, Is.EqualTo(_loc.T("lab.craft")));
            with.Dispose();
        }

        [Test, Description("Vérifie qu'un clic sur Fabriquer dépense les Composants, ajoute l'objet et affiche un message avec son nom — parce que le joueur doit voir ce qu'il a obtenu")]
        public void ClickingTheRecipe_CraftsAndToasts()
        {
            CraftingOps.AddComponents(_app.Session, ComponentKind.Weapon, ComponentGrade.Common, 6);
            var p = Lab();
            _view.ClickRow(FirstRecipeRow);
            Assert.That(CraftingOps.Components(_app.Session, ComponentKind.Weapon, ComponentGrade.Common), Is.EqualTo(0));
            var item = _app.Session.Content.Collection.Crafted.Single();
            string name = CollectionText.ItemName(_loc, item);
            Assert.That(_toasts.Shown, Has.Some.Matches<(string Text, ToastKind Kind)>(t => t.Kind == ToastKind.Reward && t.Text.Contains(name)));
            Assert.That(_view.Rows[FirstRecipeRow].A.Enabled, Is.False, "la liste se rafraîchit : plus de Composants");
            p.Dispose();
        }

        [Test, Description("Vérifie qu'un clic sans Composants affiche l'erreur et ne crée aucun objet — parce qu'un bouton contourné ne doit rien débloquer")]
        public void ClickingWithoutComponents_ShowsTheError()
        {
            var p = Lab();
            _view.ClickRow(FirstRecipeRow);
            Assert.That(_toasts.Shown, Has.Some.Matches<(string Text, ToastKind Kind)>(t => t.Kind == ToastKind.Error));
            Assert.That(_app.Session.Meta.crafted, Is.Empty);
            p.Dispose();
        }

        [Test, Description("Vérifie que le texte de fin de combat liste les Composants obtenus (par type et qualité), le catalyseur et la recette apprise — parce que le joueur doit savoir ce que le combat lui a laissé pour l'Atelier")]
        public void CraftDropLines_MentionComponentsCatalystAndNewRecipe()
        {
            var comps = new int[ComponentKinds.Count * ComponentKinds.GradeCount];
            comps[ComponentKinds.Index(ComponentKind.Armor, ComponentGrade.Common)] = 3;
            comps[ComponentKinds.Index(ComponentKind.Armor, ComponentGrade.Refined)] = 2;
            string text = CollectionText.CraftDropLines(_loc, new CraftDrops(comps, CatalystKind.Bio, "pulse_rifle"));
            Assert.That(text, Does.Contain("3 " + _loc.T("component.Armor.Common")));
            Assert.That(text, Does.Contain("2 " + _loc.T("component.Armor.Refined")));
            Assert.That(text, Does.Not.Contain(_loc.T("component.Weapon.Common")));
            Assert.That(text, Does.Contain(_loc.F("combat.craft_catalyst", _loc.T("catalyst.Bio"))));
            Assert.That(text, Does.Contain(_loc.F("combat.craft_recipe", _loc.T("craft.recipe.pulse_rifle"))));
            Assert.That(CollectionText.CraftDropLines(_loc, new CraftDrops(comps, null, null)), Does.Not.Contain(_loc.T("craft.recipe.pulse_rifle")));
        }
    }
}
