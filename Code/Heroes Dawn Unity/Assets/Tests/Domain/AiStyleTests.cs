using System.Linq;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>Styles d'IA des ennemis (bloc C de la 1.0.0) : définitions, style par défaut selon la position, contenu du prototype.</summary>
    public sealed class AiStyleTests
    {
        static readonly ContentCatalog C = TestContent.Build();

        [Test, Description("Vérifie que le style par défaut suit la position (Attaquant et Précision à distance agressifs, Première ligne défensive, Contrôle et Soutien à leur nom) — parce qu'un ennemi sans style saisi doit se comporter selon son rôle")]
        public void Le_style_par_defaut_suit_la_position()
        {
            Assert.That(AiStyles.Default(Position.Attacker), Is.EqualTo(AiStyle.Aggressive));
            Assert.That(AiStyles.Default(Position.Ranged), Is.EqualTo(AiStyle.Aggressive));
            Assert.That(AiStyles.Default(Position.FrontLine), Is.EqualTo(AiStyle.Defensive));
            Assert.That(AiStyles.Default(Position.Controller), Is.EqualTo(AiStyle.Control));
            Assert.That(AiStyles.Default(Position.Support), Is.EqualTo(AiStyle.Support));
        }

        [Test, Description("Vérifie qu'un style saisi prime sur celui de la position — parce que le contenu doit pouvoir faire d'un capitaine de première ligne un meneur qui contrôle")]
        public void Un_style_saisi_prime_sur_la_position()
        {
            var stats = new StatBlock(100, 10, 10, 10);
            var skills = new System.Collections.Generic.List<SkillDef>();
            var libre = new EnemyDef(new EnemyId("x"), "k", Position.FrontLine, Family.Matter, stats, skills);
            var saisi = new EnemyDef(new EnemyId("y"), "k", Position.FrontLine, Family.Matter, stats, skills, false, null, AiStyle.Control);
            Assert.That(libre.Style, Is.EqualTo(AiStyle.Defensive));
            Assert.That(saisi.Style, Is.EqualTo(AiStyle.Control));
        }

        [Test, Description("Vérifie que les ennemis du contenu prototype ont le style prévu (voyou et pyromane agressifs, brute défensive, capitaine de l'AMIF contrôle, soldat équilibré) — parce que le style ne doit pas être Équilibré par oubli")]
        public void Un_ennemi_du_contenu_prototype_a_un_style()
        {
            AiStyle Of(string id) => C.Enemy(new EnemyId(id)).Style;
            Assert.That(Of("thug"), Is.EqualTo(AiStyle.Aggressive));
            Assert.That(Of("brute"), Is.EqualTo(AiStyle.Defensive));
            Assert.That(Of("street_pyro"), Is.EqualTo(AiStyle.Aggressive));
            Assert.That(Of("amif_marksman"), Is.EqualTo(AiStyle.Aggressive));
            Assert.That(Of("amif_trooper"), Is.EqualTo(AiStyle.Balanced));
            Assert.That(Of("amif_captain"), Is.EqualTo(AiStyle.Control));
        }
    }
}
