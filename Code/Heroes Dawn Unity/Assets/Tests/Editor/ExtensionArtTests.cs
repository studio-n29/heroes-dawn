using System.Linq;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Presentation.Common;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Editor
{
    /// <summary>Le dossier d'extension Aube tactique est branché : les sprites sont importés, copiés dans Resources et reliés aux objets du jeu.</summary>
    public sealed class ExtensionArtTests
    {
        static readonly string[] MappedItems =
        {
            "steel_knuckles", "titan_gauntlets", "kevlar_vest", "adrenaline_patch", "apex_module",
            "ballistic_fabric", "recovery_gel", "energy_cell", "neural_chip",
        };

        [Test, Description("Vérifie que chaque objet relié à l'extension charge une icône détourée — parce que la correspondance objet du jeu / icône de la planche ne doit pas pointer vers un fichier absent")]
        public void MappedItems_LoadTheirExtensionIcon()
        {
            foreach (var id in MappedItems)
                Assert.That(AgentArt.Item(id), Is.Not.Null, id);
        }

        [Test, Description("Vérifie que les quatre objets créés pour l'extension existent dans le catalogue et ont un nom — parce qu'une icône sans objet du jeu ne servirait à rien")]
        public void NewItems_AreInTheCatalogue_WithNames()
        {
            var defs = PrototypeCollection.Build();
            foreach (var id in new[] { "ballistic_fabric", "recovery_gel", "energy_cell", "neural_chip" })
            {
                var item = defs.Items.FirstOrDefault(i => i.Id.Value == id);
                Assert.That(item, Is.Not.Null, id);
                Assert.That(Loc.T(item.NameKey), Is.Not.EqualTo(item.NameKey), "nom français de " + id);
            }
        }

        [Test, Description("Vérifie que les pictogrammes de cases, les icônes de compétence d'un agent de départ et un agent détouré se chargent — parce que le plateau, la fiche d'agent et le combat les affichent")]
        public void Extension_SpritesAreLoadable()
        {
            foreach (var icon in new[] { "crossed-swords", "node-alert", "node-crate", "tent", "target" })
                Assert.That(TacticalArt.Ext("Icons", icon), Is.Not.Null, icon);
            Assert.That(AgentArt.Skill("giulia_rossi_a1"), Is.SameAs(TacticalArt.Ext("Skills", "giulia_rossi_a1")));
            Assert.That(AgentArt.Cutout("giulia_rossi"), Is.Not.Null);
            Assert.That(TacticalArt.Ext("Components", "gold-toggle-on"), Is.Not.Null);
            Assert.That(TacticalArt.Ext("Components", "bolt-pill"), Is.Not.Null);
            Assert.That(TacticalArt.Ext("MiniGames", "vision-cone"), Is.Not.Null);
        }

        [Test, Description("Vérifie que les icônes dorées de navigation du kit existent — parce que la barre du bas les utilise à la place des glyphes générés")]
        public void KitGoldIcons_ExistForTheNavigation()
        {
            foreach (var name in new[] { "team", "pass", "shop", "profile" })
                Assert.That(KitArt.Get("IconsGold/" + name), Is.Not.Null, name);
        }
    }
}
