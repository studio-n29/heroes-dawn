using System.Linq;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Definitions;
using NUnit.Framework;
using UnityEngine;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    /// <summary>Migration d'une sauvegarde 0.15.0 vers l'Atelier : scraps, Noyaux, compétences, recherches du labo, anciens objets fabriqués (bloc B de la 1.0.0).</summary>
    public sealed class MetaMigrationTests
    {
        static MetaSave Old()
        {
            var m = new MetaSave();
            m.SetCounter(m.scraps, "Weapon", 7);
            m.SetCounter(m.scraps, "Armor", 3);
            m.SetCounter(m.scraps, "Core", 5);
            m.SetCounter(m.craftXp, "Armor", 120);
            m.SetCounter(m.labLevels, "training", 2);
            m.SetCounter(m.labLevels, "loot", 1);
            m.labActive = "recovery";
            m.labEndUtcTicks = 123456;
            m.crafted.Add(new CraftedItemSave { id = "crafted_1", recipeId = "patch_vest", quality = 0, hp = 9 });
            m.crafted.Add(new CraftedItemSave { id = "crafted_2", recipeId = "aegis_plate", quality = 4, hp = 20, def = 18 });
            return m;
        }

        static int Refund(MetaSave m) { MetaMigration.ToComponents(m, null, out int refunded); return refunded; }

        [Test, Description("Vérifie que 7 scraps d'arme et 3 d'armure deviennent des Composants Commun du même type et que la liste des scraps est vidée — parce que le joueur ne doit rien perdre")]
        public void Les_scraps_deviennent_des_composants_Commun()
        {
            var m = Old();
            Refund(m);
            Assert.That(m.GetCounter(m.components, "Weapon.Common"), Is.EqualTo(7));
            Assert.That(m.GetCounter(m.components, "Armor.Common"), Is.EqualTo(3));
            Assert.That(m.scraps, Is.Empty);
        }

        [Test, Description("Vérifie que 5 Noyaux deviennent 2 Métal, 1 Énergie, 1 Biologique et 1 Cristal — parce que les Noyaux se répartissent en rotation entre les catalyseurs")]
        public void Les_Noyaux_deviennent_des_catalyseurs_en_rotation()
        {
            var m = Old();
            Refund(m);
            Assert.That(new[] { "Metal", "Energy", "Bio", "Crystal" }.Select(id => m.GetCounter(m.catalysts, id)), Is.EqualTo(new[] { 2, 1, 1, 1 }));
        }

        [Test, Description("Vérifie que training niveau 2 et loot niveau 1 rendent (500 + 1000) + 500 = 2 000 Crédits, que les niveaux sont vidés et que la recherche en cours est annulée sans remboursement — parce que les Crédits dépensés dans le labo supprimé doivent revenir")]
        public void Les_recherches_sont_remboursees()
        {
            var m = Old();
            Assert.That(Refund(m), Is.EqualTo(2000));
            Assert.That(m.labLevels, Is.Empty);
            Assert.That((m.labActive, m.labEndUtcTicks), Is.EqualTo(("", 0L)));
        }

        [Test, Description("Vérifie qu'un objet fabriqué sans étoiles prend l'ancienne qualité + 1 (0 → 1, 4 → 5) avec les mêmes statistiques — parce que les objets existants ne doivent pas changer de valeur")]
        public void Les_anciens_objets_fabriques_prennent_qualite_plus_1()
        {
            var m = Old();
            Refund(m);
            Assert.That(m.crafted.Select(c => c.stars), Is.EqualTo(new[] { 1, 5 }));
            Assert.That((m.crafted[1].hp, m.crafted[1].def), Is.EqualTo((20, 18)));
        }

        [Test, Description("Vérifie que deux passes de migration donnent le même résultat qu'une seule (Composants, catalyseurs, Crédits rendus) — parce qu'un chargement répété ne doit ni rembourser ni convertir deux fois")]
        public void La_migration_est_idempotente()
        {
            var m = Old();
            Refund(m);
            string once = JsonUtility.ToJson(m);
            Assert.That(Refund(m), Is.EqualTo(0));
            Assert.That(JsonUtility.ToJson(m), Is.EqualTo(once));
        }

        [Test, Description("Vérifie que l'expérience de création est supprimée sans aucun remboursement — parce que la compétence de création n'existe plus")]
        public void Les_niveaux_de_competence_sont_supprimes()
        {
            var m = new MetaSave();
            m.SetCounter(m.craftXp, "Armor", 500);
            Assert.That(Refund(m), Is.EqualTo(0));
            Assert.That(m.craftXp, Is.Empty);
            Assert.That(m.components, Is.Empty);
        }

        [Test, Description("Vérifie qu'une sauvegarde vide reste vide — parce que la migration ne doit rien inventer")]
        public void Une_sauvegarde_vide_ne_change_rien()
        {
            var m = new MetaSave();
            string before = JsonUtility.ToJson(m);
            Assert.That(Refund(m), Is.EqualTo(0));
            Assert.That(JsonUtility.ToJson(m), Is.EqualTo(before));
        }

        [Test, Description("Vérifie qu'au chargement d'un profil 0.15.0 les Crédits sont remboursés une fois, le profil resauvegardé puis rechargé ne rembourse plus, et l'ancien objet garde ses statistiques avec ses étoiles — parce que la migration s'exécute à chaque chargement")]
        public void Le_chargement_rembourse_une_seule_fois()
        {
            var profile = new ProfileData { credits = 100 };
            profile.meta = Old();
            profile.team.Add(new MemberSave { id = "verre", level = 3, inSquad = true });
            var first = new GameSession(TestContent.Build());
            first.ApplyProfile(profile);
            Assert.That(first.Wallet.Credits, Is.EqualTo(2100));
            Assert.That(CraftingOps.Components(first, ComponentKind.Weapon, ComponentGrade.Common), Is.EqualTo(7));
            Assert.That(CraftingOps.Catalysts(first, CatalystKind.Metal), Is.EqualTo(2));
            var item = first.Content.Collection.Item(new ItemId("crafted_2"));
            Assert.That((item.HpPercent, item.DefPercent), Is.EqualTo((20, 18)));
            Assert.That(CraftedItems.TryParseNameKey(item.NameKey, out _, out int stars) && stars == 5, Is.True);

            var json = JsonUtility.ToJson(first.ToProfile(System.DateTime.UtcNow));
            var second = new GameSession(TestContent.Build());
            second.ApplyProfile(JsonUtility.FromJson<ProfileData>(json));
            Assert.That(second.Wallet.Credits, Is.EqualTo(2100), "aucun second remboursement");
            Assert.That(CraftingOps.Components(second, ComponentKind.Weapon, ComponentGrade.Common), Is.EqualTo(7));
        }
    }
}
