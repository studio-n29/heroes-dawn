using System;
using System.Linq;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    /// <summary>Migration des sauvegardes 0.15.0 vers positions, rangs et trois cartes (bloc A de la 1.0.0).</summary>
    public sealed class SaveMigrationTests
    {
        static ProfileData Profile015()
        {
            var profile = new ProfileData();
            var m = new MemberSave { id = "verre", level = 7, xp = 120, fatigue = 30, inSquad = true };
            m.items.Add("kevlar_vest");
            m.cards.AddRange(new[] { "card_ember", "card_regen_field" });   // deux emplacements, comme avant la 1.0.0
            profile.team.Add(m);
            return profile;
        }

        static GameSession Load(ProfileData p)
        {
            var s = new GameSession(TestContent.Build());
            s.ApplyProfile(p);
            return s;
        }

        [Test, Description("Vérifie qu'une sauvegarde 0.15.0 à deux cartes (dont une d'une autre famille) se charge avec les deux cartes conservées et le 3e emplacement vide — parce que la migration ne doit perdre aucune carte équipée")]
        public void Une_sauvegarde_0_15_0_se_charge_avec_les_cartes_conservees()
        {
            var verre = Load(Profile015()).Find(new MemberId("verre"))!;
            Assert.That(verre.Cards.Length, Is.EqualTo(3));
            Assert.That(verre.Cards.Select(c => c?.Value), Is.EqualTo(new[] { "card_ember", "card_regen_field", null }));
        }

        [Test, Description("Vérifie que charger, sauvegarder puis recharger donne le même résultat — parce que la migration doit être idempotente")]
        public void Le_chargement_est_idempotent()
        {
            var first = Load(Profile015());
            var json = UnityEngine.JsonUtility.ToJson(first.ToProfile(DateTime.UtcNow));
            var second = Load(UnityEngine.JsonUtility.FromJson<ProfileData>(json));
            var a = first.Find(new MemberId("verre"))!;
            var b = second.Find(new MemberId("verre"))!;
            Assert.That(b.Cards.Select(c => c?.Value), Is.EqualTo(a.Cards.Select(c => c?.Value)));
            Assert.That(b.Items, Is.EqualTo(a.Items));
        }

        [Test, Description("Vérifie que le niveau, l'EXP, la fatigue et l'escouade des agents obtenus sont inchangés après la migration — parce que la progression du joueur ne doit pas bouger")]
        public void Les_agents_obtenus_gardent_leur_progression()
        {
            var verre = Load(Profile015()).Find(new MemberId("verre"))!;
            Assert.That((verre.Level, verre.Xp, verre.Fatigue, verre.InSquad), Is.EqualTo((7, 120, 30, true)));
        }

        [Test, Description("Vérifie que la position et le rang viennent du contenu et non de la sauvegarde (qui ne stocke que l'identifiant) — parce que la renommée du rôle en position et la table des rangs s'appliquent d'elles-mêmes")]
        public void La_position_et_le_rang_viennent_du_contenu()
        {
            var verre = Load(Profile015()).Find(new MemberId("verre"))!;
            Assert.That(verre.Def.Position, Is.EqualTo(N29.HeroesDawn.Definitions.Position.Controller));
            Assert.That(verre.Def.Rank, Is.InRange(1, 5));
        }
    }
}
