#nullable enable
using System.Linq;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Definitions;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>Rang AMIF des agents (1 à 5) — bloc A de la 1.0.0.</summary>
    public sealed class AgentRankTests
    {
        static readonly System.Collections.Generic.List<MemberDef> Members =
            PrototypeAgents.Members((p, r) => PrototypeContent.BaseStats(p, r)).ToList();

        [Test, Description("Vérifie que le multiplicateur de stats vaut 100, 115, 130, 145 et 165 % du rang 1 au rang 5 — parce que ce sont les valeurs validées de la spec du bloc A")]
        public void StatPercent_donne_100_115_130_145_165()
            => Assert.That(Enumerable.Range(1, 5).Select(AgentRanks.StatPercent), Is.EqualTo(new[] { 100, 115, 130, 145, 165 }));

        [Test, Description("Vérifie qu'un rang hors bornes est ramené dans 1..5 — parce qu'un contenu JSON ancien ou erroné ne doit jamais faire sortir de la table")]
        public void Clamp_ramene_0_a_1_et_9_a_5()
        {
            Assert.That(AgentRanks.Clamp(0), Is.EqualTo(1));
            Assert.That(AgentRanks.Clamp(9), Is.EqualTo(5));
            Assert.That(AgentRanks.Clamp(3), Is.EqualTo(3));
        }

        [Test, Description("Vérifie que les 32 agents présents ont un rang et la répartition 3 / 5 / 6 / 8 / 10 (rangs 5 à 1) — parce que les 10 cryptides (rangs 4, 3 et 2) ne sont pas encore dans le jeu et complètent 3 / 6 / 10 / 13 / 10")]
        public void Les_32_agents_ont_un_rang_et_la_repartition_attendue()
        {
            Assert.That(Members.Count, Is.EqualTo(32));
            Assert.That(Members.All(m => m.Rank >= 1 && m.Rank <= 5), Is.True);
            int Count(int rank) => Members.Count(m => m.Rank == rank);
            Assert.That(new[] { Count(5), Count(4), Count(3), Count(2), Count(1) }, Is.EqualTo(new[] { 3, 5, 6, 8, 10 }));
        }

        [Test, Description("Vérifie que trois agents témoins ont le rang de la table de la spec (Victor Sorel 5, Odile Ferrand 4, Anna Petrov 1) — parce que la table validée remplace l'ancienne puissance")]
        public void Chaque_agent_a_le_rang_de_la_table_de_la_spec()
        {
            int Of(string id) => Members.Single(m => m.Id.Value == id).Rank;
            Assert.That(Of("victor_sorel"), Is.EqualTo(5));
            Assert.That(Of("odile_ferrand"), Is.EqualTo(4));
            Assert.That(Of("anna_petrov"), Is.EqualTo(1));
            Assert.That(Of("eero_nieminen"), Is.EqualTo(4), "Eero Nieminen passe de 5 à 4");
            Assert.That(Of("agent_d"), Is.EqualTo(5), "Agent D passe de 4 à 5");
        }

        [Test, Description("Vérifie qu'un agent absent de la table de rangs est au rang 1 — parce que c'est la valeur par défaut des contenus anciens (agents de test, JSON sans champ rank)")]
        public void Un_agent_sans_rang_dans_le_contenu_est_au_rang_1()
        {
            var entry = new PrototypeAgents.Entry("inconnu", "Inconnu", Family.Matter, Position.Attacker);
            Assert.That(PrototypeAgents.RankOf(entry), Is.EqualTo(1));
        }

        [Test, Description("Vérifie que le coût de recrutement monte avec le rang (150, 220, 450, 800, 1200) — parce que les rangs élevés sont plus rares et plus forts")]
        public void Le_cout_de_recrutement_suit_le_rang()
            => Assert.That(Enumerable.Range(1, 5).Select(AgentRanks.RecruitCost), Is.EqualTo(new[] { 150, 220, 450, 800, 1200 }));

        [Test, Description("Vérifie que les stats de base d'un agent suivent le multiplicateur de son rang — parce que le rang remplace l'ancien multiplicateur de rareté")]
        public void Les_stats_de_base_suivent_le_rang()
        {
            var r1 = PrototypeContent.BaseStats(Position.Attacker, 1);
            var r5 = PrototypeContent.BaseStats(Position.Attacker, 5);
            Assert.That(r1.Hp, Is.EqualTo(1000));
            Assert.That(r5.Hp, Is.EqualTo(1650));
            Assert.That(r5.Atk, Is.EqualTo(214));
        }
    }
}
